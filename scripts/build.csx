#r "../packages/SetStartupProjects.1.4.0/lib/net452/SetStartupProjects.dll"

#load "../packages/simple-targets-csx.6.0.0/contentFiles/csx/any/simple-targets.csx"
#load "cmd.csx"

using System;
using static SimpleTargets;
using SetStartupProjects;

var vswhere = "../packages/vswhere.2.1.4/tools/vswhere.exe";
var nuget = ".nuget/v4.3.0/NuGet.exe";
string msBuild = null;

var exerciseSolutions = Directory.EnumerateFiles("exercises", "*.sln", SearchOption.AllDirectories);

var targets = new TargetDictionary();

targets.Add("default", DependsOn("exercises"));

targets.Add(
    "restore-exercises",
    () =>
    {
        foreach (var solution in exerciseSolutions)
        {
            Cmd(nuget, $"restore {solution}");
        }
    });

targets.Add(
    "find-msbuild",
    () => msBuild = $"C:/Program Files (x86)/Microsoft Visual Studio/2019/Professional/MSBuild/Current/Bin/MSBuild.exe");

targets.Add(
    "exercises",
    DependsOn("find-msbuild", "restore-exercises"),
    () =>
    {
        foreach (var solution in exerciseSolutions)
        {
            Cmd(msBuild, $"{solution} /p:Configuration=Debug /nologo /m /v:m /nr:false");
        }
    });

targets.Add(
    "delete-vs-folders",
    () =>
    {
        foreach (var suo in Directory.EnumerateDirectories(".", ".vs", SearchOption.AllDirectories))
        {
            Directory.Delete(suo, true);
        }
    }
);

targets.Add(
    "set-startup-projects",
    DependsOn("delete-vs-folders"),
    () =>
    {
        var suoCreator = new StartProjectSuoCreator();
        foreach (var sln in Directory.EnumerateFiles(".", "*.sln", SearchOption.AllDirectories))
        {
            var startProjects = new StartProjectFinder().GetStartProjects(sln).ToList();
            if (startProjects.Any())
            {
                suoCreator.CreateForSolutionFile(sln, startProjects, VisualStudioVersions.Vs2017);
            }
        }
    }
);

Run(Args, targets);
