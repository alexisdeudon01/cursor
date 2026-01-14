using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildLinuxMono
{
    public static void PerformBuild()
    {
        // Force Mono pour CI
        PlayerSettings.SetScriptingBackend(
            NamedBuildTarget.Standalone,
            ScriptingImplementation.Mono2x
        );

        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            target = BuildTarget.StandaloneLinux64,
            locationPathName = "build/StandaloneLinux64/LinuxBuild.x86_64",
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception("Build failed: " + report.summary.result);
    }
}
