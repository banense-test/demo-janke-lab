using EmployeePortal.Poc.Application;
using EmployeePortal.Poc.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EmployeePortal.Poc.Infrastructure;

/// <summary>
/// SQLite-based implementation of ILocalStore for offline clocking persistence.
/// Creates tables via EnsureCreated() — no migration history needed for buffer store.
/// Traces to: COMP-I3 (Local Store), INT-003 (ILocalStore), RISK-T01.
/// </summary>
public sealed class SqliteLocalStore : ILocalStore, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly object _dbLock = new();
    private bool _disposed;

    public SqliteLocalStore(string connectionString = "Data Source=:memory:")
    {
        _connection = new SqliteConnection(connectionString);
        _connection.Open();
        EnsureSchema();
    }

    private void EnsureSchema()
    {
        lock (_dbLock)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = """
                CREATE TABLE IF NOT EXISTS clockings_local (
                    local_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    id TEXT NOT NULL,
                    employee_id TEXT NOT NULL,
                    type TEXT NOT NULL,
                    timestamp TEXT NOT NULL,
                    source TEXT NOT NULL DEFAULT 'OFFLINE'
                );

                CREATE TABLE IF NOT EXISTS sync_records_local (
                    id TEXT PRIMARY KEY,
                    local_id INTEGER NOT NULL,
                    clocking_id TEXT NOT NULL,
                    status TEXT NOT NULL DEFAULT 'PENDING',
                    queued_at TEXT NOT NULL,
                    synced_at TEXT
                );
                """;
            cmd.ExecuteNonQuery();
        }
    }

    public Task<int> SaveAsync(Clocking clocking)
    {
        lock (_dbLock)
        {
            // Insert clocking
            using var clockingCmd = _connection.CreateCommand();
            clockingCmd.CommandText = """
                INSERT INTO clockings_local (id, employee_id, type, timestamp, source)
                VALUES (@id, @emp, @type, @ts, @src);
                SELECT last_insert_rowid();
                """;
            clockingCmd.Parameters.AddWithValue("@id", clocking.Id.ToString());
            clockingCmd.Parameters.AddWithValue("@emp", clocking.EmployeeId.ToString());
            clockingCmd.Parameters.AddWithValue("@type", clocking.Type.ToString());
            clockingCmd.Parameters.AddWithValue("@ts", clocking.Timestamp.ToString("O"));
            clockingCmd.Parameters.AddWithValue("@src", clocking.Source);

            var localId = Convert.ToInt32(clockingCmd.ExecuteScalar());

            // Insert sync record
            var syncRecord = new SyncRecord(localId, clocking.Id);
            using var syncCmd = _connection.CreateCommand();
            syncCmd.CommandText = """
                INSERT INTO sync_records_local (id, local_id, clocking_id, status, queued_at)
                VALUES (@id, @localId, @clockingId, 'PENDING', @queuedAt);
                """;
            syncCmd.Parameters.AddWithValue("@id", syncRecord.Id.ToString());
            syncCmd.Parameters.AddWithValue("@localId", localId);
            syncCmd.Parameters.AddWithValue("@clockingId", clocking.Id.ToString());
            syncCmd.Parameters.AddWithValue("@queuedAt", syncRecord.QueuedAt.ToString("O"));
            syncCmd.ExecuteNonQuery();

            return Task.FromResult(localId);
        }
    }

    public Task<IReadOnlyList<(Clocking Clocking, SyncRecord Record)>> GetPendingAsync()
    {
        lock (_dbLock)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = """
                SELECT c.local_id, c.id, c.employee_id, c.type, c.timestamp, c.source,
                       s.id, s.status, s.queued_at, s.synced_at
                FROM clockings_local c
                INNER JOIN sync_records_local s ON s.local_id = c.local_id
                WHERE s.status = 'PENDING'
                ORDER BY c.local_id;
                """;

            var results = new List<(Clocking, SyncRecord)>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var localId = reader.GetInt32(0);
                var clockingId = Guid.Parse(reader.GetString(1));
                var employeeId = Guid.Parse(reader.GetString(2));
                var type = Enum.Parse<ClockingType>(reader.GetString(3));
                var timestamp = DateTime.Parse(reader.GetString(4), null, System.Globalization.DateTimeStyles.RoundtripKind);
                var source = reader.GetString(5);

                var clocking = new Clocking(employeeId, type, timestamp, source);
                // Use reflection to set Id since it's init-only
                typeof(Clocking).GetProperty("Id")!.SetValue(clocking, clockingId);

                var recordId = Guid.Parse(reader.GetString(6));
                var status = Enum.Parse<SyncStatus>(reader.GetString(7));
                var queuedAt = DateTime.Parse(reader.GetString(8), null, System.Globalization.DateTimeStyles.RoundtripKind);

                var record = new SyncRecord(localId, clockingId);
                typeof(SyncRecord).GetProperty("Id")!.SetValue(record, recordId);
                typeof(SyncRecord).GetProperty("QueuedAt")!.SetValue(record, queuedAt);
                record.Status = status;

                results.Add((clocking, record));
            }

            return Task.FromResult<IReadOnlyList<(Clocking, SyncRecord)>>(results);
        }
    }

    public Task UpdateSyncStatusAsync(int localId, SyncStatus status)
    {
        lock (_dbLock)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = """
                UPDATE sync_records_local
                SET status = @status, synced_at = @syncedAt
                WHERE local_id = @localId;
                """;
            cmd.Parameters.AddWithValue("@status", status.ToString());
            cmd.Parameters.AddWithValue("@syncedAt", status == SyncStatus.PENDING ? (object)DBNull.Value : DateTime.UtcNow.ToString("O"));
            cmd.Parameters.AddWithValue("@localId", localId);
            cmd.ExecuteNonQuery();
            return Task.CompletedTask;
        }
    }

    public Task<int> GetPendingCountAsync()
    {
        lock (_dbLock)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM sync_records_local WHERE status = 'PENDING';";
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return Task.FromResult(count);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _connection.Dispose();
            _disposed = true;
        }
    }
}
