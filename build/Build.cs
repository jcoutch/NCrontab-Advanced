using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.OctoVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

class Build : NukeBuild
{
    const string CiBranchNameEnvVariable = "OCTOVERSION_CurrentBranch";

    [Parameter(
        "Whether to auto-detect the branch name - this is okay for a local build, but should not be used under CI.")]
    readonly bool AutoDetectBranch = IsLocalBuild;

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;

    [Parameter("Test filter expression", Name = "where")] readonly string TestFilter = string.Empty;

    [OctoVersion(BranchParameter = nameof(BranchName),
        AutoDetectBranchParameter = nameof(AutoDetectBranch),
        Framework = "net6.0")]
    public OctoVersionInfo OctoVersionInfo;

    [Parameter(
        "Branch name for OctoVersion to use to calculate the version number. Can be set via the environment variable " +
        CiBranchNameEnvVariable + ".",
        Name = CiBranchNameEnvVariable)]
    string BranchName { get; set; }

    static AbsolutePath SourceDirectory => RootDirectory / "source";
    static AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    static AbsolutePath TestResultsDirectory => RootDirectory / "TestResults";

    Target Clean => _ => _
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            DeleteDirectory(TestResultsDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetVersion(OctoVersionInfo.FullSemVer)
                .SetAssemblyVersion(OctoVersionInfo.MajorMinorPatch)
                .SetInformationalVersion(OctoVersionInfo.InformationalVersion)
                .EnableNoRestore());
        });

    Target UnitTests => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest( _ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetLoggers("trx")
                .SetFilter(TestFilter)
                .SetVerbosity(DotNetVerbosity.Normal)
                .EnableNoBuild()
                .EnableNoRestore()
                .SetResultsDirectory(TestResultsDirectory));
        });

    Target Pack => _ => _
        .DependsOn(UnitTests)
        .Executes(() =>
        {
            DotNetPack(_ => _
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .SetOutputDirectory(ArtifactsDirectory)
                .EnableNoBuild()
                .EnableNoRestore()
                .EnableIncludeSymbols()
                .AddProperty("Version", OctoVersionInfo.FullSemVer));
        });

    Target Default => _ => _
        .DependsOn(Pack);

    public static int Main() => Execute<Build>(x => x.Default);

}
