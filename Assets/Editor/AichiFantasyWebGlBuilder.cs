using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AichiFantasy.Editor
{
    public static class AichiFantasyWebGlBuilder
    {
        const string ScenePath = "Assets/AichiFantasyMain.unity";

        public static void BuildWebGlPlayer()
        {
            string buildPath = GetArg("-webglBuildPath", "build/WebGL");
            string version = GetArg("-bundleVersion", "1.0");

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                throw new InvalidOperationException("Could not switch Unity build target to WebGL.");

            AichiFantasySceneBuilder.BuildMainScene();

            PlayerSettings.companyName = "kogit";
            PlayerSettings.productName = "AichiFantasyTRPG";
            PlayerSettings.bundleVersion = version;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;

            if (Directory.Exists(buildPath))
                Directory.Delete(buildPath, true);
            Directory.CreateDirectory(buildPath);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = buildPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
                throw new Exception("WebGL build failed: " + summary.result + " / " + summary.totalErrors + " errors");

            Debug.Log("Aichi Fantasy WebGL player exported: " + buildPath);
        }

        static string GetArg(string name, string fallback)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name)
                    return args[i + 1];
            }
            return fallback;
        }
    }
}
