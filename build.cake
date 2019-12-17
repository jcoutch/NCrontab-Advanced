//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#tool "nuget:?package=GitVersion.CommandLine&version=4.0.0"
#addin "Cake.FileHelpers"
#addin "Cake.ExtendedNuGet"

using Path = System.IO.Path;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var solutionFile = "./NCrontab.Advanced.sln";
var assemblyInfoFile = "./NCrontab.Advanced/Properties/AssemblyInfo.cs";

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////
var artifactsDir = "./build/target/";
var tempDir = "./build/temp";

GitVersion gitVersionInfo;
string nugetVersion;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
Setup(context =>
{
    gitVersionInfo = GitVersion(new GitVersionSettings {
        OutputType = GitVersionOutput.Json
    });


    nugetVersion = gitVersionInfo.MajorMinorPatch + "-" + gitVersionInfo.PreReleaseLabel + gitVersionInfo.CommitsSinceVersionSourcePadded;

    if(BuildSystem.IsRunningOnTeamCity)
        BuildSystem.TeamCity.SetBuildNumber(nugetVersion);

    Information("Building NCrontab.Advanced v{0}", nugetVersion);
    Information("Informational Version {0}", gitVersionInfo.InformationalVersion);
});

Teardown(context =>
{
    Information("Finished running tasks.");
});

//////////////////////////////////////////////////////////////////////
//  PRIVATE TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
	CleanDirectory(tempDir);
    CleanDirectories("./**/bin");
    CleanDirectories("./**/obj");
    CleanDirectories("./Package");
    CleanDirectories("./**/TestResults");
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() => {
        NuGetRestore(solutionFile);
    });

Task("SetVersion")
   .Does(() => {
       ReplaceRegexInFiles(assemblyInfoFile, 
                           "(?<=AssemblyVersion\\(\")(.+?)(?=\"\\))", 
                           gitVersionInfo.MajorMinorPatch);
       ReplaceRegexInFiles(assemblyInfoFile, 
                           "(?<=AssemblyFileVersion\\(\")(.+?)(?=\"\\))", 
                           gitVersionInfo.MajorMinorPatch);
   });

Task("Build")
    .IsDependentOn("SetVersion")
    .IsDependentOn("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        MSBuild(solutionFile, settings => settings
			.SetConfiguration(configuration)
            .WithProperty("Version", nugetVersion)
			.WithProperty("PackageVersion", nugetVersion)
			.WithProperty("FileVersion", nugetVersion)
			.WithTarget("Build"));
    });

Task("UnitTests")
    .IsDependentOn("Build")
    .Does(() =>
    {
        //NCrontab.Advanced.Tests/bin/Release/NCrontab.Advanced.Tests.dll
        MSTest("./NCrontab.Advanced.Tests/**/NCrontab.Advanced.Tests.dll");
    });

Task("Pack")
    .IsDependentOn("UnitTests")
    .Does(() =>
    {
        DotNetCoreMSBuildSettings msBuildSettings = new DotNetCoreMSBuildSettings()
            .WithProperty("Version", nugetVersion)
            .WithProperty("AssemblyVersion", nugetVersion)
            .WithProperty("FileVersion", nugetVersion);

        DotNetCorePack("NCrontab.Advanced/NCrontab.Advanced.csproj", new DotNetCorePackSettings {
            Configuration = configuration,
            OutputDirectory = "./Package",
            NoBuild = true,
            NoRestore = true,
            IncludeSymbols = false,
            MSBuildSettings = msBuildSettings
        });
    });
    

Task("Default")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////
RunTarget(target);