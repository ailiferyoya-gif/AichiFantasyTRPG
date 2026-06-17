using System.IO;
using UnityEditor;
using UnityEngine;

namespace AichiFantasy.Editor
{
    public static class AichiFantasyBuildAssetOptimizer
    {
        const string ResourceRoot = "Assets/Resources/AichiFantasy";
        const string BackgroundRoot = ResourceRoot + "/Backgrounds";
        const string PortraitRoot = ResourceRoot + "/Portraits";

        public static void ConfigureForWebGl()
        {
            ConfigureTexturesInFolder(BackgroundRoot, "WebGL", 512, TextureImporterFormat.ASTC_6x6, true);
            ConfigureTexturesInFolder(PortraitRoot, "WebGL", 384, TextureImporterFormat.ASTC_6x6, true);
            Save();
        }

        public static void ConfigureForIos()
        {
            ConfigureTexturesInFolder(BackgroundRoot, "iPhone", 1024, TextureImporterFormat.ASTC_6x6, true);
            ConfigureTexturesInFolder(PortraitRoot, "iPhone", 768, TextureImporterFormat.ASTC_6x6, true);
            Save();
        }

        static void ConfigureTexturesInFolder(
            string folder,
            string buildTarget,
            int maxSize,
            TextureImporterFormat format,
            bool crunchedCompression)
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
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.compressionQuality = 40;

                var settings = importer.GetPlatformTextureSettings(buildTarget);
                settings.name = buildTarget;
                settings.overridden = true;
                settings.maxTextureSize = maxSize;
                settings.format = format;
                settings.textureCompression = TextureImporterCompression.CompressedHQ;
                settings.compressionQuality = 40;
                settings.crunchedCompression = crunchedCompression;
                importer.SetPlatformTextureSettings(settings);
                importer.SaveAndReimport();
            }
        }

        static void Save()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
