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
        const string ResourceRoot = "Assets/Resources/AichiFantasy";
        const string BackgroundRoot = ResourceRoot + "/Backgrounds";
        const string PortraitRoot = ResourceRoot + "/Portraits";

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
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.dataCaching = false;
            PlayerSettings.WebGL.initialMemorySize = 128;
            PlayerSettings.WebGL.maximumMemorySize = 512;
            PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;

            ConfigureWebGlTextures();

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

            PatchIndexHtml(buildPath, GetArg("-webglCacheBust", DateTime.UtcNow.ToString("yyyyMMddHHmmss")));

            Debug.Log("Aichi Fantasy WebGL player exported: " + buildPath);
        }

        static void PatchIndexHtml(string buildPath, string cacheBust)
        {
            string indexPath = Path.Combine(buildPath, "index.html");
            if (!File.Exists(indexPath))
                return;

            string html = File.ReadAllText(indexPath);
            html = html.Replace(
                "var loaderUrl = buildUrl + \"/WebGL.loader.js\";",
                "var cacheBust = \"" + cacheBust + "\";\n      var loaderUrl = buildUrl + \"/WebGL.loader.js?v=\" + cacheBust;");
            html = html.Replace(
                "dataUrl: buildUrl + \"/WebGL.data\",",
                "dataUrl: buildUrl + \"/WebGL.data?v=\" + cacheBust,");
            html = html.Replace(
                "frameworkUrl: buildUrl + \"/WebGL.framework.js\",",
                "frameworkUrl: buildUrl + \"/WebGL.framework.js?v=\" + cacheBust,");
            html = html.Replace(
                "codeUrl: buildUrl + \"/WebGL.wasm\",",
                "codeUrl: buildUrl + \"/WebGL.wasm?v=\" + cacheBust,");
            html = html.Replace(
                "        // config.devicePixelRatio = 1;",
                "        config.devicePixelRatio = 1;");
            File.WriteAllText(indexPath, html);
        }

        static void ConfigureWebGlTextures()
        {
            ConfigureWebGlTexturesInFolder(BackgroundRoot, 512);
            ConfigureWebGlTexturesInFolder(PortraitRoot, 512);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void ConfigureWebGlTexturesInFolder(string folder, int maxSize)
        {
            if (!Directory.Exists(folder))
                return;

            foreach (string file in Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories))
            {
                string assetPath = file.Replace("\\", "/");
                if (AssetImporter.GetAtPath(assetPath) is not TextureImporter importer)
                    continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.isReadable = false;
                importer.alphaIsTransparency = true;
                importer.textureCompression = TextureImporterCompression.Compressed;

                var settings = importer.GetPlatformTextureSettings("WebGL");
                settings.name = "WebGL";
                settings.overridden = true;
                settings.maxTextureSize = maxSize;
                settings.format = TextureImporterFormat.Automatic;
                settings.textureCompression = TextureImporterCompression.Compressed;
                importer.SetPlatformTextureSettings(settings);
                importer.SaveAndReimport();
            }
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
