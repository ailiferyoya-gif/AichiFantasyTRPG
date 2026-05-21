using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AichiFantasy.Editor
{
    public static class AichiFantasyBackupUtility
    {
        const string LastAutoBackupKey = "AichiFantasy.LastAutoBackupUtc";
        const double AutoBackupIntervalMinutes = 30.0;

        static readonly string[] BackupTargets =
        {
            "Assets/Scripts/AichiFantasyGame.cs",
            "Assets/Editor/AichiFantasySceneBuilder.cs",
            "Assets/AichiFantasyMain.unity"
        };

        [InitializeOnLoadMethod]
        static void AutoBackupOnEditorStart()
        {
            string lastRaw = EditorPrefs.GetString(LastAutoBackupKey, "");
            if (DateTime.TryParse(lastRaw, null, System.Globalization.DateTimeStyles.RoundtripKind, out var lastUtc))
            {
                if ((DateTime.UtcNow - lastUtc).TotalMinutes < AutoBackupIntervalMinutes)
                    return;
            }

            CreateSnapshot("auto");
            EditorPrefs.SetString(LastAutoBackupKey, DateTime.UtcNow.ToString("o"));
        }

        [MenuItem("Aichi Fantasy/Backup/Create Snapshot")]
        public static void CreateManualSnapshot()
        {
            string path = CreateSnapshot("manual");
            EditorUtility.RevealInFinder(path);
        }

        public static string CreateSnapshot(string reason)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string safeReason = MakeSafeName(string.IsNullOrEmpty(reason) ? "snapshot" : reason);
            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupRoot = Path.Combine(projectRoot, "Backups", stamp + "_" + safeReason);
            Directory.CreateDirectory(backupRoot);

            foreach (string relativePath in BackupTargets)
            {
                string source = Path.Combine(projectRoot, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (!File.Exists(source))
                    continue;

                string destination = Path.Combine(backupRoot, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                File.Copy(source, destination, true);
            }

            File.WriteAllText(
                Path.Combine(backupRoot, "manifest.txt"),
                "Aichi Fantasy TRPG backup" + Environment.NewLine +
                "Created: " + DateTime.Now.ToString("o") + Environment.NewLine +
                "Reason: " + safeReason + Environment.NewLine +
                "Files:" + Environment.NewLine +
                string.Join(Environment.NewLine, BackupTargets),
                new System.Text.UTF8Encoding(false));

            Debug.Log("Aichi Fantasy backup created: " + backupRoot);
            return backupRoot;
        }

        static string MakeSafeName(string value)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                value = value.Replace(c, '_');
            return value.Trim();
        }
    }
}
