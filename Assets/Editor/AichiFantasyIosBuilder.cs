using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AichiFantasy.Editor
{
    public static class AichiFantasyIosBuilder
    {
        const string ScenePath = "Assets/AichiFantasyMain.unity";

        public static void BuildIosXcodeProject()
        {
            string buildPath = GetArg("-iosBuildPath", "build/iOS/AichiFantasyTRPG");
            string bundleId = GetArg("-bundleId", "com.kogit.aichifantasytrpg");
            string buildNumber = GetArg("-buildNumber", "1");
            string version = GetArg("-bundleVersion", "1.0");

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS))
                throw new InvalidOperationException("Could not switch Unity build target to iOS.");

            AichiFantasySceneBuilder.BuildMainScene();

            PlayerSettings.companyName = "kogit";
            PlayerSettings.productName = "AichiFantasyTRPG";
            PlayerSettings.bundleVersion = version;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, bundleId);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.iOS, ApiCompatibilityLevel.NET_Standard);
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.iOS.buildNumber = buildNumber;
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.iOS.appleEnableAutomaticSigning = false;

            AichiFantasyBuildAssetOptimizer.ConfigureForIos();

            if (Directory.Exists(buildPath))
                Directory.Delete(buildPath, true);
            Directory.CreateDirectory(buildPath);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = buildPath,
                target = BuildTarget.iOS,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            if (summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new Exception("iOS Xcode export failed: " + summary.result + " / " + summary.totalErrors + " errors");

            Debug.Log("Aichi Fantasy iOS Xcode project exported: " + buildPath);
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
