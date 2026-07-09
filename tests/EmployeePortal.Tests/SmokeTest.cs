using Xunit;

namespace EmployeePortal.Tests;

/// <summary>
/// Smoke tests validating the Employee Portal project skeleton.
/// CR #6 fix: Replaced placeholder Assert.True(true) with meaningful tests
/// that validate project structure and basic Razor Pages invariants.
/// </summary>
public class SmokeTest
{
    [Fact]
    public void ProjectSkeleton_MainProjectCompiles()
    {
        // If this test runs, the EmployeePortal project compiled successfully
        // and its dependency chain is intact — that IS the smoke test.
        var assembly = typeof(EmployeePortal.Pages.IndexModel).Assembly;
        Assert.NotNull(assembly);
        Assert.Equal("EmployeePortal", assembly.GetName().Name);
    }

    [Fact]
    public void ProjectSkeleton_IndexModelIsRazorPageModel()
    {
        // Verify the IndexModel inherits from PageModel — confirms Razor Pages wiring
        var indexModelType = typeof(EmployeePortal.Pages.IndexModel);
        Assert.NotNull(indexModelType);
        Assert.True(typeof(Microsoft.AspNetCore.Mvc.RazorPages.PageModel).IsAssignableFrom(indexModelType),
            "IndexModel should inherit from PageModel");
    }

    [Fact]
    public void ProjectSkeleton_SolutionFileExists()
    {
        // Navigate from bin/Debug/net10.0 up to repo root
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var slnFile = Path.Combine(repoRoot, "EmployeePortal.sln");
        Assert.True(File.Exists(slnFile), $"Solution file should exist at {slnFile}");
    }
}
