using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AichiFantasy.Editor
{
    public static class AichiFantasySceneBuilder
    {
        const string Root = "Assets/Resources/AichiFantasy";
        const string BackgroundRoot = Root + "/Backgrounds";
        const string PortraitRoot = Root + "/Portraits";
        const string SfxRoot = Root + "/Sfx";
        const string ScenePath = "Assets/AichiFantasyMain.unity";

        [MenuItem("Aichi Fantasy/Build Main Scene")]
        public static void BuildMainScene()
        {
            Directory.CreateDirectory(BackgroundRoot);
            Directory.CreateDirectory(PortraitRoot);
            Directory.CreateDirectory(SfxRoot);
            GenerateBackgrounds();
            GeneratePortraitCutouts();
            GenerateSfx();
            AssetDatabase.Refresh();
            ConfigureImportedAssets();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Main Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.01f, 0.008f, 0.014f);
            camera.orthographic = true;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.tag = "MainCamera";

            var light = new GameObject("Low Amber Light");
            var lightComponent = light.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            lightComponent.color = new Color(1f, 0.82f, 0.55f);
            lightComponent.intensity = 0.22f;
            light.transform.rotation = Quaternion.Euler(38f, -30f, 0f);

            var gameObject = new GameObject("AichiFantasyGame");
            gameObject.AddComponent<AichiFantasyGame>();

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.025f, 0.02f, 0.035f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.015f, 0.012f, 0.02f);
            RenderSettings.fogDensity = 0.025f;

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            Debug.Log("Aichi Fantasy TRPG scene built: " + ScenePath);
        }

        static void ConfigureImportedAssets()
        {
            ConfigureTextures(BackgroundRoot, 2048);
            ConfigureTextures(PortraitRoot, 1024);
            ConfigureAudio(SfxRoot);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void ConfigureTextures(string folder, int maxSize)
        {
            if (!Directory.Exists(folder))
                return;

            foreach (var file in Directory.GetFiles(folder, "*.png"))
            {
                string assetPath = file.Replace("\\", "/");
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                    continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.isReadable = folder == PortraitRoot;
                importer.alphaIsTransparency = true;
                importer.maxTextureSize = maxSize;
                importer.filterMode = FilterMode.Trilinear;
                importer.anisoLevel = 4;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        public static void RunUiSmokeTest()
        {
            BuildMainScene();
            EditorSceneManager.OpenScene(ScenePath);
            var game = UnityEngine.Object.FindObjectOfType<AichiFantasyGame>();
            if (game == null)
                throw new InvalidOperationException("AichiFantasyGame was not found in the smoke test scene.");

            var type = typeof(AichiFantasyGame);
            Invoke(type, game, "Awake");
            CheckChoices(type, game, "title");

            Invoke(type, game, "ShowCharacterSelect");
            CheckChoices(type, game, "character-select");
            CheckPortrait(type, game, "character-select");

            var characters = type.GetField("characters", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game) as System.Collections.IDictionary;
            foreach (System.Collections.DictionaryEntry entry in characters)
            {
                Invoke(type, game, "ShowCharacterConfirm", entry.Value);
                CheckChoices(type, game, "character-confirm-" + entry.Key);
                CheckPortrait(type, game, "character-confirm-" + entry.Key);
            }

            Invoke(type, game, "ShowTitle");
            CheckChoices(type, game, "title-after-return");
            Debug.Log("Aichi Fantasy UI smoke test passed.");
        }

        public static void RunStageExpansionSmokeTest()
        {
            BuildMainScene();
            EditorSceneManager.OpenScene(ScenePath);
            var game = UnityEngine.Object.FindObjectOfType<AichiFantasyGame>();
            if (game == null)
                throw new InvalidOperationException("AichiFantasyGame was not found in the stage smoke test scene.");

            var type = typeof(AichiFantasyGame);
            Invoke(type, game, "Awake");

            var scenes = type.GetField("scenes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game) as System.Collections.IDictionary;
            var enemies = type.GetField("enemies", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game) as System.Collections.IDictionary;
            if (scenes == null || scenes.Count < 60)
                throw new InvalidOperationException("Stage expansion did not register enough scenes.");
            if (enemies == null || enemies.Count < 40)
                throw new InvalidOperationException("Stage expansion did not register enough enemies.");
            if (!scenes.Contains("stage5_event_9") || !scenes.Contains("stage5_boss_gate"))
                throw new InvalidOperationException("Final stage scenes are missing.");
            if (!enemies.Contains("stage_boss_5") || !enemies.Contains("stage_enemy_15") || !enemies.Contains("stage1_enemy_1") || !enemies.Contains("stage5_enemy_10") || !enemies.Contains("impossible_one"))
                throw new InvalidOperationException("Stage enemies or bosses are missing.");

            var characters = type.GetField("characters", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game) as System.Collections.IDictionary;
            object traveler = characters["traveler"];
            Invoke(type, game, "StartRun", traveler);
            CheckChoices(type, game, "start-gear-select");

            Invoke(type, game, "ShowScene", "nagoya_after_battle", false);
            CheckChoices(type, game, "stage-select");
            Invoke(type, game, "ShowScene", "stage1_hub", false);
            CheckChoices(type, game, "stage1-hub");
            Invoke(type, game, "ShowScene", "stage1_event_0", false);
            CheckChoices(type, game, "stage1-event-0");
            Invoke(type, game, "ShowScene", "stage1_boss_gate", false);
            CheckChoices(type, game, "stage1-boss-gate");
            Invoke(type, game, "StartBattle", "impossible_one");
            CheckBattle(type, game, "impossible-boss");

            Debug.Log("Aichi Fantasy stage expansion smoke test passed.");
        }

        public static void RunCompletionRouteSmokeTest()
        {
            BuildMainScene();
            EditorSceneManager.OpenScene(ScenePath);
            var game = UnityEngine.Object.FindObjectOfType<AichiFantasyGame>();
            if (game == null)
                throw new InvalidOperationException("AichiFantasyGame was not found in the completion route smoke test scene.");

            var type = typeof(AichiFantasyGame);
            Invoke(type, game, "Awake");

            var scenes = type.GetField("scenes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game) as System.Collections.IDictionary;
            string[] requiredScenes =
            {
                "freedom_owari", "freedom_mikawa", "freedom_chita",
                "owari_deep_route", "mikawa_deep_route", "chita_deep_route",
                "airport_bridge", "airport_gate",
                "stage1_hub", "stage1_boss_gate", "stage5_boss_gate"
            };
            foreach (string sceneId in requiredScenes)
            {
                if (scenes == null || !scenes.Contains(sceneId))
                    throw new InvalidOperationException("Missing completion route scene: " + sceneId);
            }

            var characters = type.GetField("characters", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game) as System.Collections.IDictionary;
            object traveler = characters["traveler"];
            Invoke(type, game, "StartRun", traveler);

            object run = type.GetField("run", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game);
            object stats = run.GetType().GetField("stats", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(run);
            SetInt(run, "owari", 3);
            SetInt(run, "mikawa", 3);
            SetInt(run, "npcAirport", 6);
            SetInt(run, "shachiGaze", 3);
            SetInt(run, "dangerWarnings", 3);
            SetInt(stats, "localKnowledge", 8);
            SetInt(stats, "misoResistance", 6);
            SetInt(stats, "machineAptitude", 6);
            SetInt(stats, "mythosKnowledge", 6);
            SetInt(stats, "money", 2000);

            Invoke(type, game, "ShowStatusModal");
            string status = ReadUiText(type, game, "statusModalText");
            RequireContains("status-modal", status, "地元");
            RequireContains("status-modal", status, "尾張");
            RequireContains("status-modal", status, "鯱");
            RequireContains("status-modal", status, "空港");
            Invoke(type, game, "HideStatusModal");

            Invoke(type, game, "ShowScene", "freedom_owari", false);
            CheckChoices(type, game, "freedom-owari");
            RequireChoiceText(type, game, "owari-deep-choice", "尾張深層");

            Invoke(type, game, "ShowScene", "freedom_mikawa", false);
            CheckChoices(type, game, "freedom-mikawa");
            RequireChoiceText(type, game, "mikawa-deep-choice", "三河深層");

            Invoke(type, game, "ShowScene", "freedom_chita", false);
            CheckChoices(type, game, "freedom-chita");
            RequireChoiceText(type, game, "chita-deep-choice", "海底深層");

            Invoke(type, game, "ShowScene", "owari_deep_route", false);
            CheckChoices(type, game, "owari-deep-route");
            Invoke(type, game, "ShowScene", "mikawa_deep_route", false);
            CheckChoices(type, game, "mikawa-deep-route");
            Invoke(type, game, "ShowScene", "chita_deep_route", false);
            CheckChoices(type, game, "chita-deep-route");

            Invoke(type, game, "ShowScene", "stage1_hub", false);
            CheckChoices(type, game, "stage1-hub-completion");
            RequireChoiceText(type, game, "stage-boss-shortcut", "ボス地点");
            Invoke(type, game, "ShowScene", "stage1_boss_gate", false);
            CheckChoices(type, game, "stage1-boss-gate-completion");

            Invoke(type, game, "ShowScene", "airport_bridge", false);
            CheckChoices(type, game, "airport-bridge");
            Invoke(type, game, "ShowScene", "airport_gate", false);
            CheckChoices(type, game, "airport-gate");
            RequireChoiceText(type, game, "route-boss-choice", "ルート別ボス");

            Debug.Log("Aichi Fantasy completion route smoke test passed.");
        }

        public static void RunRandomEventFlavorSmokeTest()
        {
            BuildMainScene();
            EditorSceneManager.OpenScene(ScenePath);
            var game = UnityEngine.Object.FindObjectOfType<AichiFantasyGame>();
            if (game == null)
                throw new InvalidOperationException("AichiFantasyGame was not found in the random event flavor smoke test scene.");

            var type = typeof(AichiFantasyGame);
            Invoke(type, game, "Awake");

            var characters = type.GetField("characters", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game) as System.Collections.IDictionary;
            object traveler = characters["traveler"];
            Invoke(type, game, "StartRun", traveler);

            var events = InvokeResult(type, game, "ExpandedRandomEvents") as System.Collections.IEnumerable;
            if (events == null)
                throw new InvalidOperationException("ExpandedRandomEvents did not return an event list.");

            int count = 0;
            foreach (var randomEvent in events)
            {
                count++;
                string id = ReadString(randomEvent, "id");
                string intro = ReadString(randomEvent, "text");
                string success = ReadString(randomEvent, "successText");
                string fail = ReadString(randomEvent, "failText");
                if (string.IsNullOrWhiteSpace(id))
                    throw new InvalidOperationException("Random event has an empty id.");
                if (string.IsNullOrWhiteSpace(intro))
                    throw new InvalidOperationException(id + ": missing pre-action flavor text.");
                if (string.IsNullOrWhiteSpace(success))
                    throw new InvalidOperationException(id + ": missing success flavor text.");
                if (string.IsNullOrWhiteSpace(fail))
                    throw new InvalidOperationException(id + ": missing failure flavor text.");

                string successResult = InvokeResult(type, game, "BuildRandomEventResultText", randomEvent, true, "検証") as string;
                string failResult = InvokeResult(type, game, "BuildRandomEventResultText", randomEvent, false, "検証") as string;
                CheckFlavorSections(id + "/success", successResult);
                CheckFlavorSections(id + "/failure", failResult);
            }

            if (count < 50)
                throw new InvalidOperationException("Random event flavor smoke test found too few events: " + count);

            Debug.Log("Aichi Fantasy random event flavor smoke test passed. events=" + count);
        }

        public static void MeasureGateArrivalTime()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            BuildMainScene();
            EditorSceneManager.OpenScene(ScenePath);
            var game = UnityEngine.Object.FindObjectOfType<AichiFantasyGame>();
            if (game == null)
                throw new InvalidOperationException("AichiFantasyGame was not found in the gate timing scene.");

            var type = typeof(AichiFantasyGame);
            Invoke(type, game, "Awake");

            var characters = type.GetField("characters", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game) as System.Collections.IDictionary;
            object traveler = characters["traveler"];

            double seconds = 0;
            int scenesVisited = 0;
            int stageEvents = 0;
            int bossBattles = 0;

            Invoke(type, game, "StartRun", traveler);
            seconds += 35; // character confirm, starting gear, contract choice
            Invoke(type, game, "ShowRunContract");
            seconds += 15;
            Invoke(type, game, "ShowDice");
            seconds += 18; // dice animation and result read
            Invoke(type, game, "ShowScene", "nagoya_after_battle", false);
            scenesVisited++;
            seconds += 30;

            for (int stage = 1; stage <= 5; stage++)
            {
                Invoke(type, game, "ShowScene", "stage" + stage + "_hub", false);
                scenesVisited++;
                seconds += 26;

                for (int i = 0; i < 10; i++)
                {
                    Invoke(type, game, "ShowScene", "stage" + stage + "_event_" + i, false);
                    scenesVisited++;
                    stageEvents++;
                    seconds += 24; // read event, compare choices, click
                    if (i == 2 || i == 5 || i == 8)
                        seconds += 5; // occasional dice/check result read
                }

                Invoke(type, game, "ShowScene", "stage" + stage + "_boss_gate", false);
                scenesVisited++;
                seconds += 22;

                Invoke(type, game, "StartBattle", "stage_boss_" + stage);
                CheckBattle(type, game, "stage-boss-" + stage);
                bossBattles++;
                seconds += stage < 5 ? 95 : 125; // tactical battle, reward/drop handling
                Invoke(type, game, "WinBattle");
                seconds += 16; // loot decision
            }

            Invoke(type, game, "ShowScene", "airport_gate", false);
            scenesVisited++;
            seconds += 20;
            Invoke(type, game, "ShowEnding", "return");
            seconds += 35; // final choice, ending text, route summary
            stopwatch.Stop();

            string report = string.Format(
                "Gate arrival timing: estimated_clear_time={0:0.0}min ({1:0}s), automated_runtime={2:0.00}s, scenes={3}, stage_events={4}, bosses={5}",
                seconds / 60.0,
                seconds,
                stopwatch.Elapsed.TotalSeconds,
                scenesVisited,
                stageEvents,
                bossBattles);
            Debug.Log(report);
        }

        static void Invoke(Type type, object target, string method, params object[] args)
        {
            type.GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
            Canvas.ForceUpdateCanvases();
        }

        static object InvokeResult(Type type, object target, string method, params object[] args)
        {
            var methodInfo = type.GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic);
            if (methodInfo == null)
                throw new InvalidOperationException("Missing method: " + method);
            var result = methodInfo.Invoke(target, args);
            Canvas.ForceUpdateCanvases();
            return result;
        }

        static string ReadString(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return field != null ? field.GetValue(target) as string : null;
        }

        static void SetInt(object target, string fieldName, int value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
                throw new InvalidOperationException("Missing int field: " + fieldName);
            field.SetValue(target, value);
        }

        static string ReadUiText(Type type, object target, string fieldName)
        {
            var text = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target) as Text;
            return text != null ? text.text : "";
        }

        static void RequireContains(string label, string text, string expected)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf(expected, StringComparison.Ordinal) < 0)
                throw new InvalidOperationException(label + ": missing text: " + expected);
        }

        static void RequireChoiceText(Type type, object target, string label, string expected)
        {
            var choiceRoot = type.GetField("choiceRoot", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target) as RectTransform;
            if (choiceRoot == null)
                throw new InvalidOperationException(label + ": choice root is missing.");
            var texts = choiceRoot.GetComponentsInChildren<Text>(true);
            foreach (var text in texts)
            {
                if (text.gameObject.activeInHierarchy && !string.IsNullOrEmpty(text.text) && text.text.IndexOf(expected, StringComparison.Ordinal) >= 0)
                    return;
            }
            throw new InvalidOperationException(label + ": missing choice text: " + expected);
        }

        static void CheckFlavorSections(string label, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException(label + ": empty random event result text.");
            string[] required = { "異変:", "判定:", "結果:", "変化:" };
            foreach (string marker in required)
            {
                int index = text.IndexOf(marker, StringComparison.Ordinal);
                if (index < 0)
                    throw new InvalidOperationException(label + ": missing flavor section: " + marker);
                int next = text.Length;
                foreach (string other in required)
                {
                    int otherIndex = text.IndexOf(other, index + marker.Length, StringComparison.Ordinal);
                    if (otherIndex >= 0)
                        next = Math.Min(next, otherIndex);
                }
                string section = text.Substring(index + marker.Length, next - index - marker.Length).Trim();
                if (string.IsNullOrWhiteSpace(section))
                    throw new InvalidOperationException(label + ": empty flavor section: " + marker);
            }
        }

        static void CheckBattle(Type type, object target, string label)
        {
            var battleRoot = type.GetField("battleRoot", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target) as RectTransform;
            var activeEnemy = type.GetField("activeEnemy", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
            if (battleRoot == null || !battleRoot.gameObject.activeInHierarchy)
                throw new InvalidOperationException(label + ": battle root is not active.");
            if (activeEnemy == null)
                throw new InvalidOperationException(label + ": active enemy is missing.");
        }

        static void CheckChoices(Type type, object target, string label)
        {
            var choiceRoot = type.GetField("choiceRoot", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target) as RectTransform;
            if (choiceRoot == null || !choiceRoot.gameObject.activeInHierarchy)
                throw new InvalidOperationException(label + ": choice root is not active.");
            var texts = choiceRoot.GetComponentsInChildren<Text>(true);
            int visible = 0;
            foreach (var text in texts)
            {
                if (!text.gameObject.activeInHierarchy)
                    continue;
                if (string.IsNullOrWhiteSpace(text.text))
                    throw new InvalidOperationException(label + ": empty command text found.");
                visible++;
            }
            if (visible == 0)
                throw new InvalidOperationException(label + ": no visible command text found.");
        }

        static void CheckPortrait(Type type, object target, string label)
        {
            var image = type.GetField("portraitImage", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target) as Image;
            if (image == null || image.sprite == null)
                return;
            float width = image.sprite.rect.width;
            float height = image.sprite.rect.height;
            if (width < 80f || height < 120f)
                throw new InvalidOperationException(label + ": portrait sprite is unexpectedly small: " + width + "x" + height);
        }

        static void ConfigureAudio(string folder)
        {
            if (!Directory.Exists(folder))
                return;

            foreach (var file in Directory.GetFiles(folder, "*.wav"))
            {
                string assetPath = file.Replace("\\", "/");
                var importer = AssetImporter.GetAtPath(assetPath) as AudioImporter;
                if (importer == null)
                    continue;
                importer.forceToMono = true;
                importer.loadInBackground = false;
                var settings = importer.defaultSampleSettings;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.PCM;
                settings.quality = 1f;
                settings.preloadAudioData = true;
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
            }
        }

        static void GenerateBackgrounds()
        {
            MakeBackground("title", new Color(0.018f, 0.012f, 0.025f), new Color(0.55f, 0.42f, 0.18f), (tex) =>
            {
                DrawMoon(tex, 900, 150, 78, new Color(0.95f, 0.78f, 0.34f, 0.65f));
                DrawCastle(tex, 520, 430, 1.2f, new Color(0.06f, 0.055f, 0.075f));
                DrawShachi(tex, 780, 210, 1.1f, new Color(0.92f, 0.68f, 0.24f, 0.85f));
                DrawMist(tex, new Color(0.12f, 0.1f, 0.16f, 0.22f));
            });

            MakeBackground("characters", new Color(0.02f, 0.018f, 0.028f), new Color(0.22f, 0.15f, 0.28f), (tex) =>
            {
                for (int i = 0; i < 5; i++)
                    DrawFigure(tex, 170 + i * 185, 420, 0.95f, new Color(0.05f + i * 0.018f, 0.045f, 0.065f + i * 0.02f));
                DrawMist(tex, new Color(0.18f, 0.16f, 0.2f, 0.18f));
            });

            MakeBackground("dice", new Color(0.018f, 0.014f, 0.025f), new Color(0.34f, 0.24f, 0.12f), (tex) =>
            {
                DrawDie(tex, 420, 330, 130, 1);
                DrawDie(tex, 600, 310, 130, 4);
                DrawDie(tex, 780, 335, 130, 6);
                DrawMist(tex, new Color(0.13f, 0.1f, 0.06f, 0.25f));
            });

            MakeBackground("station", new Color(0.012f, 0.016f, 0.024f), new Color(0.08f, 0.18f, 0.25f), (tex) =>
            {
                DrawSubway(tex, new Color(0.035f, 0.04f, 0.055f), new Color(0.45f, 0.56f, 0.6f, 0.5f));
                DrawSign(tex, 560, 170, "EXIT -> <- EXIT");
                DrawMist(tex, new Color(0.05f, 0.12f, 0.18f, 0.2f));
            });

            MakeBackground("kishimen", new Color(0.025f, 0.018f, 0.014f), new Color(0.38f, 0.17f, 0.08f), (tex) =>
            {
                DrawNoodles(tex, 600, 400);
                DrawSteam(tex, new Color(0.7f, 0.55f, 0.42f, 0.18f));
            });

            MakeBackground("osu", new Color(0.018f, 0.012f, 0.02f), new Color(0.42f, 0.1f, 0.16f), (tex) =>
            {
                DrawMarket(tex);
                DrawLanterns(tex);
                DrawMist(tex, new Color(0.2f, 0.08f, 0.1f, 0.18f));
            });

            MakeBackground("castle", new Color(0.015f, 0.018f, 0.026f), new Color(0.09f, 0.14f, 0.2f), (tex) =>
            {
                DrawCastle(tex, 520, 470, 1.45f, new Color(0.045f, 0.05f, 0.065f));
                DrawShachi(tex, 520, 180, 0.9f, new Color(0.9f, 0.67f, 0.23f, 0.9f));
                DrawShachi(tex, 700, 180, -0.9f, new Color(0.9f, 0.67f, 0.23f, 0.9f));
                DrawRain(tex);
            });

            MakeBackground("okazaki", new Color(0.024f, 0.014f, 0.01f), new Color(0.38f, 0.16f, 0.07f), (tex) =>
            {
                DrawBarrels(tex);
                DrawGlyphs(tex, new Color(0.7f, 0.36f, 0.12f, 0.28f));
                DrawMist(tex, new Color(0.24f, 0.1f, 0.04f, 0.22f));
            });

            MakeBackground("miso", new Color(0.025f, 0.012f, 0.008f), new Color(0.45f, 0.12f, 0.04f), (tex) =>
            {
                DrawHugeBarrel(tex);
                DrawGlyphs(tex, new Color(0.95f, 0.42f, 0.14f, 0.36f));
            });

            MakeBackground("toyota", new Color(0.012f, 0.015f, 0.018f), new Color(0.1f, 0.22f, 0.2f), (tex) =>
            {
                DrawFactory(tex);
                DrawConveyor(tex);
                DrawMist(tex, new Color(0.08f, 0.18f, 0.16f, 0.18f));
            });

            MakeBackground("tokoname", new Color(0.018f, 0.014f, 0.018f), new Color(0.33f, 0.18f, 0.1f), (tex) =>
            {
                DrawCeramicHill(tex);
                DrawCatEyes(tex);
                DrawMist(tex, new Color(0.15f, 0.09f, 0.06f, 0.18f));
            });

            MakeBackground("airport", new Color(0.01f, 0.014f, 0.022f), new Color(0.06f, 0.18f, 0.3f), (tex) =>
            {
                DrawAirport(tex);
                DrawSeaThing(tex);
                DrawMist(tex, new Color(0.08f, 0.15f, 0.22f, 0.2f));
            });

            MakeBackground("piyorin", new Color(0.018f, 0.016f, 0.02f), new Color(0.45f, 0.33f, 0.06f), (tex) =>
            {
                DrawSubway(tex, new Color(0.035f, 0.035f, 0.045f), new Color(0.5f, 0.44f, 0.22f, 0.45f));
                for (int i = 0; i < 13; i++)
                    DrawPiyo(tex, 160 + (i % 7) * 140, 480 - (i / 7) * 120, 0.9f + (i % 3) * 0.16f);
            });

            MakeBackground("ending", new Color(0.012f, 0.01f, 0.018f), new Color(0.12f, 0.05f, 0.08f), (tex) =>
            {
                DrawDoor(tex);
                DrawMist(tex, new Color(0.18f, 0.12f, 0.16f, 0.22f));
            });

            MakeBackground("atsuta", new Color(0.016f, 0.012f, 0.014f), new Color(0.32f, 0.06f, 0.05f), (tex) =>
            {
                DrawRect(tex, 0, 470, tex.width, 150, new Color(0.025f, 0.018f, 0.015f, 0.86f));
                DrawLine(tex, 250, 470, 640, 210, new Color(0.18f, 0.08f, 0.05f, 0.7f), 16);
                DrawLine(tex, 1030, 470, 640, 210, new Color(0.18f, 0.08f, 0.05f, 0.7f), 16);
                DrawGlyphs(tex, new Color(0.9f, 0.18f, 0.12f, 0.32f));
                DrawMist(tex, new Color(0.22f, 0.08f, 0.06f, 0.2f));
            });

            MakeBackground("tsuruma", new Color(0.014f, 0.012f, 0.018f), new Color(0.17f, 0.14f, 0.26f), (tex) =>
            {
                for (int i = 0; i < 9; i++)
                    DrawRect(tex, 80 + i * 130, 180, 70, 360, new Color(0.045f, 0.032f, 0.05f, 0.88f));
                DrawGlyphs(tex, new Color(0.45f, 0.38f, 0.8f, 0.22f));
                DrawMist(tex, new Color(0.12f, 0.1f, 0.2f, 0.18f));
            });

            MakeBackground("sakae", new Color(0.012f, 0.01f, 0.018f), new Color(0.42f, 0.04f, 0.24f), (tex) =>
            {
                DrawMarket(tex);
                for (int i = 0; i < 10; i++)
                    DrawRect(tex, 80 + i * 120, 160 + (i % 3) * 45, 72, 18, new Color(0.9f, 0.12f, 0.45f, 0.34f));
                DrawMist(tex, new Color(0.18f, 0.04f, 0.15f, 0.2f));
            });

            MakeBackground("seto", new Color(0.022f, 0.016f, 0.012f), new Color(0.45f, 0.26f, 0.12f), (tex) =>
            {
                DrawCeramicHill(tex);
                DrawHugeBarrel(tex);
                DrawSteam(tex, new Color(0.8f, 0.68f, 0.5f, 0.14f));
            });

            MakeBackground("inuyama", new Color(0.014f, 0.012f, 0.018f), new Color(0.24f, 0.1f, 0.18f), (tex) =>
            {
                DrawCastle(tex, 610, 470, 1.1f, new Color(0.045f, 0.04f, 0.055f));
                for (int i = 0; i < 7; i++)
                    DrawCircle(tex, 230 + i * 140, 250 + (i % 2) * 50, 32, new Color(0.75f, 0.62f, 0.45f, 0.32f));
                DrawMist(tex, new Color(0.15f, 0.1f, 0.14f, 0.2f));
            });

            MakeBackground("toyohashi", new Color(0.012f, 0.014f, 0.022f), new Color(0.08f, 0.22f, 0.3f), (tex) =>
            {
                DrawSubway(tex, new Color(0.025f, 0.04f, 0.052f), new Color(0.36f, 0.62f, 0.72f, 0.34f));
                DrawLine(tex, 0, 580, tex.width, 510, new Color(0.3f, 0.34f, 0.36f, 0.55f), 5);
                DrawMist(tex, new Color(0.06f, 0.12f, 0.18f, 0.2f));
            });

            MakeBackground("gamagori", new Color(0.008f, 0.016f, 0.024f), new Color(0.04f, 0.22f, 0.3f), (tex) =>
            {
                DrawSeaThing(tex);
                DrawLine(tex, 130, 480, 1150, 480, new Color(0.18f, 0.36f, 0.45f, 0.36f), 10);
                DrawGlyphs(tex, new Color(0.22f, 0.65f, 0.82f, 0.22f));
            });

            MakeBackground("korankei", new Color(0.018f, 0.012f, 0.008f), new Color(0.55f, 0.06f, 0.03f), (tex) =>
            {
                for (int i = 0; i < 36; i++)
                    DrawCircle(tex, (i * 97) % tex.width, 150 + (i * 53) % 350, 24 + i % 12, new Color(0.72f, 0.08f, 0.03f, 0.32f));
                DrawMist(tex, new Color(0.28f, 0.04f, 0.02f, 0.22f));
            });

            MakeBackground("handa", new Color(0.02f, 0.012f, 0.008f), new Color(0.3f, 0.12f, 0.04f), (tex) =>
            {
                DrawBarrels(tex);
                DrawSteam(tex, new Color(0.7f, 0.45f, 0.28f, 0.14f));
                DrawMist(tex, new Color(0.18f, 0.08f, 0.04f, 0.2f));
            });

            MakeBackground("arimatsu", new Color(0.008f, 0.012f, 0.025f), new Color(0.05f, 0.18f, 0.4f), (tex) =>
            {
                for (int i = 0; i < 16; i++)
                    DrawLine(tex, 0, 130 + i * 35, tex.width, 90 + i * 38, new Color(0.45f, 0.62f, 0.86f, 0.16f), 4);
                DrawMist(tex, new Color(0.06f, 0.1f, 0.2f, 0.18f));
            });

            MakeBackground("nagakute", new Color(0.014f, 0.012f, 0.014f), new Color(0.28f, 0.05f, 0.04f), (tex) =>
            {
                for (int i = 0; i < 9; i++)
                    DrawLine(tex, 160 + i * 110, 520, 140 + i * 110, 250, new Color(0.42f, 0.08f, 0.06f, 0.55f), 5);
                DrawMist(tex, new Color(0.16f, 0.08f, 0.07f, 0.24f));
            });

            MakeBackground("kariya", new Color(0.012f, 0.014f, 0.016f), new Color(0.08f, 0.22f, 0.18f), (tex) =>
            {
                DrawFactory(tex);
                DrawConveyor(tex);
                DrawGlyphs(tex, new Color(0.3f, 0.85f, 0.62f, 0.18f));
            });

            MakeBackground("nishio", new Color(0.01f, 0.018f, 0.012f), new Color(0.1f, 0.32f, 0.12f), (tex) =>
            {
                DrawCircle(tex, 640, 390, 180, new Color(0.08f, 0.22f, 0.09f, 0.7f));
                DrawCircle(tex, 640, 390, 55, new Color(0.01f, 0.012f, 0.01f, 0.9f));
                DrawSteam(tex, new Color(0.45f, 0.68f, 0.42f, 0.14f));
            });

            MakeBackground("ichinomiya", new Color(0.012f, 0.012f, 0.018f), new Color(0.24f, 0.22f, 0.36f), (tex) =>
            {
                for (int i = 0; i < 18; i++)
                    DrawLine(tex, 80 + i * 65, 90, 110 + i * 45, 610, new Color(0.62f, 0.62f, 0.85f, 0.14f), 3);
                DrawMist(tex, new Color(0.12f, 0.12f, 0.18f, 0.18f));
            });

            MakeBackground("chiryu", new Color(0.018f, 0.012f, 0.012f), new Color(0.34f, 0.06f, 0.08f), (tex) =>
            {
                DrawRect(tex, 0, 470, tex.width, 130, new Color(0.04f, 0.02f, 0.025f, 0.86f));
                for (int i = 0; i < 6; i++)
                    DrawFigure(tex, 240 + i * 150, 450, 0.7f, new Color(0.08f, 0.04f, 0.05f, 0.8f));
                DrawLanterns(tex);
            });

            MakeBackground("laguna", new Color(0.008f, 0.014f, 0.024f), new Color(0.05f, 0.2f, 0.38f), (tex) =>
            {
                DrawSeaThing(tex);
                DrawRect(tex, 280, 360, 720, 110, new Color(0.05f, 0.04f, 0.075f, 0.74f));
                DrawLightLeaks(tex, new Color(0.3f, 0.55f, 0.9f, 0.25f));
            });

            MakeBackground("tsushima", new Color(0.01f, 0.012f, 0.02f), new Color(0.2f, 0.08f, 0.22f), (tex) =>
            {
                DrawLine(tex, 0, 560, tex.width, 500, new Color(0.08f, 0.12f, 0.24f, 0.55f), 40);
                for (int i = 0; i < 16; i++)
                    DrawCircle(tex, 80 + i * 76, 430 + (i % 3) * 30, 18, new Color(0.95f, 0.5f, 0.18f, 0.35f));
                DrawMist(tex, new Color(0.12f, 0.08f, 0.16f, 0.22f));
            });
        }

        static void MakeBackground(string name, Color baseColor, Color glowColor, Action<Texture2D> draw)
        {
            string path = Path.Combine(BackgroundRoot, name + ".png");
            if (File.Exists(path))
                return;

            const int width = 1280;
            const int height = 720;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float v = y / (float)height;
                    float cx = (x - width * 0.5f) / width;
                    float cy = (y - height * 0.5f) / height;
                    float radial = Mathf.Clamp01(1f - Mathf.Sqrt(cx * cx * 2.8f + cy * cy * 3.6f));
                    var c = Color.Lerp(baseColor, glowColor, radial * 0.55f + v * 0.08f);
                    float grain = Mathf.PerlinNoise(x * 0.035f, y * 0.035f) * 0.045f;
                    c.r += grain;
                    c.g += grain * 0.75f;
                    c.b += grain * 0.5f;
                    tex.SetPixel(x, y, c);
                }
            }

            draw(tex);
            DrawLightLeaks(tex, glowColor);
            DrawScratches(tex);
            DrawVignette(tex);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        static void GeneratePortraitCutouts()
        {
            string[] people =
            {
                "atsuta_miko", "seto_potter", "toyohashi_conductor", "gamagori_diver",
                "arimatsu_weaver", "inuyama_mask", "tsuruma_librarian", "centrair_agent",
                "subway_child", "sakae_broker", "korankei_pilgrim", "handa_brewer",
                "battlefield_monk", "tea_medium", "chiryu_puppeteer", "laguna_actor",
                "locker_keeper", "lantern_medium"
            };

            string[] monsters =
            {
                "index_hound", "kiln_crawler", "last_train", "deep_one_clerk",
                "battlefield_spear", "puppet_thing", "stage_polyps", "baggage_mouth",
                "quality_golem", "dream_eater", "sword_shadow", "lantern_dead",
                "shadow_retainer", "locker_womb", "well_tentacle", "tea_eye",
                "window_god", "shachi_avatar", "miso_voice", "impossible_one",
                "instability_0", "instability_1", "instability_2",
                "instability_3", "instability_4", "instability_5"
            };

            for (int i = 0; i < people.Length; i++)
                MakePortrait(people[i], false, i);
            for (int i = 0; i < monsters.Length; i++)
                MakePortrait(monsters[i], true, i);
            int stageSeed = monsters.Length + 20;
            for (int stage = 1; stage <= 5; stage++)
            {
                for (int enemy = 1; enemy <= 10; enemy++)
                    MakePortrait("stage" + stage + "_enemy_" + enemy, true, stageSeed++);
            }
        }

        static void MakePortrait(string name, bool monster, int seed, bool overwrite = false)
        {
            string path = Path.Combine(PortraitRoot, name + ".png");
            if (File.Exists(path) && !overwrite)
                return;

            const int size = 768;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, Color.clear);

            var main = Color.HSVToRGB((seed * 0.071f) % 1f, monster ? 0.52f : 0.32f, monster ? 0.82f : 0.72f);
            main.a = 0.96f;
            var shade = new Color(main.r * 0.18f, main.g * 0.16f, main.b * 0.2f, 0.98f);
            var gold = new Color(0.92f, 0.68f, 0.26f, 0.82f);

            if (monster)
                DrawDetailedMonsterCutout(tex, name, seed, shade, main, gold);
            else
                DrawHumanCutout(tex, seed, shade, main, gold);

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        static void DrawHumanCutout(Texture2D tex, int seed, Color shade, Color accent, Color gold)
        {
            int cx = 384 + (seed % 3 - 1) * 18;
            DrawCircle(tex, cx, 170, 58, shade);
            DrawRect(tex, cx - 62, 220, 124, 245, shade);
            DrawLine(tex, cx - 50, 270, cx - 135, 470, shade, 18);
            DrawLine(tex, cx + 50, 270, cx + 135, 470, shade, 18);
            DrawLine(tex, cx - 34, 455, cx - 92, 650, shade, 24);
            DrawLine(tex, cx + 34, 455, cx + 92, 650, shade, 24);
            DrawCircle(tex, cx - 22, 154, 7, gold);
            DrawCircle(tex, cx + 22, 154, 7, gold);
            DrawRect(tex, cx - 70, 240, 140, 36, accent);
            if (seed % 2 == 0)
                DrawLine(tex, cx - 105, 112, cx + 105, 112, gold, 10);
            if (seed % 3 == 0)
                DrawLine(tex, cx + 88, 230, cx + 145, 132, gold, 8);
            if (seed % 5 == 0)
                DrawCircle(tex, cx, 235, 28, gold);
        }

        static void DrawMonsterCutout(Texture2D tex, int seed, Color shade, Color accent, Color gold)
        {
            int cx = 384;
            DrawCircle(tex, cx, 310, 118 + seed % 35, shade);
            DrawCircle(tex, cx - 70, 230, 54, shade);
            DrawCircle(tex, cx + 70, 230, 54, shade);
            for (int i = 0; i < 5 + seed % 4; i++)
            {
                int dir = i % 2 == 0 ? -1 : 1;
                int y = 330 + i * 42;
                DrawLine(tex, cx + dir * 45, y, cx + dir * (180 + i * 12), y + 70, shade, 14);
            }
            DrawCircle(tex, cx, 288, 34, gold);
            DrawCircle(tex, cx, 288, 14, Color.black);
            DrawLine(tex, cx - 72, 420, cx + 72, 420, accent, 12);
            if (seed % 2 == 0)
                DrawLine(tex, cx - 140, 160, cx + 140, 560, gold, 5);
            if (seed % 3 == 0)
                DrawCircle(tex, cx + 95, 350, 26, accent);
        }

        static void DrawDetailedMonsterCutout(Texture2D tex, string name, int seed, Color shade, Color accent, Color gold)
        {
            int stage = EnemyImageStage(name);
            int variant = EnemyImageVariant(name, seed);
            float menace = Mathf.Clamp01((stage - 1) / 5f);
            var core = Color.HSVToRGB(Mathf.Repeat(0.58f + stage * 0.075f + seed * 0.019f, 1f), 0.62f + menace * 0.28f, 0.56f - menace * 0.18f);
            core.a = 0.96f;
            var dark = new Color(core.r * (0.12f + menace * 0.05f), core.g * 0.1f, core.b * 0.14f, 0.98f);
            var wound = Color.Lerp(new Color(0.85f, 0.65f, 0.25f, 0.85f), new Color(0.92f, 0.05f, 0.12f, 0.92f), menace);
            var coldGlow = Color.Lerp(new Color(0.62f, 0.92f, 0.78f, 0.58f), new Color(0.48f, 0.08f, 0.88f, 0.72f), menace);

            int cx = 384 + ((seed % 5) - 2) * 10;
            int cy = 330 + Mathf.RoundToInt(menace * 18f);
            int body = 120 + stage * 14 + variant * 3;
            DrawCircle(tex, cx, cy, body, dark);
            DrawCircle(tex, cx - 62, cy - 82, 48 + stage * 4, dark);
            DrawCircle(tex, cx + 62, cy - 82, 48 + stage * 4, dark);
            DrawCircle(tex, cx, cy + 18, body - 34, core);

            int limbs = 5 + stage + variant % 4;
            for (int i = 0; i < limbs; i++)
            {
                int dir = i % 2 == 0 ? -1 : 1;
                int baseY = cy - 30 + i * (24 + stage);
                int endX = cx + dir * (145 + stage * 34 + i * 9);
                int endY = baseY + 60 + (i % 3) * 34;
                DrawLine(tex, cx + dir * (34 + i * 4), baseY, endX, endY, dark, 11 + stage);
                DrawCircle(tex, endX, endY, 10 + stage * 2, dark);
                if (stage >= 4)
                    DrawLine(tex, endX, endY, endX + dir * (30 + i * 4), endY + 38, wound, 3 + stage / 2);
            }

            int eyes = 1 + (variant + stage) % 4 + stage / 2;
            for (int i = 0; i < eyes; i++)
            {
                float angle = (i / (float)Mathf.Max(1, eyes)) * Mathf.PI * 2f + seed * 0.21f;
                int ex = cx + Mathf.RoundToInt(Mathf.Cos(angle) * (26 + i * 9));
                int ey = cy - 46 + Mathf.RoundToInt(Mathf.Sin(angle) * (18 + i * 5));
                DrawCircle(tex, ex, ey, 14 + stage, wound);
                DrawCircle(tex, ex, ey, 5 + stage / 2, Color.black);
            }

            DrawLine(tex, cx - 72 - stage * 4, cy + 76, cx + 72 + stage * 4, cy + 76 + variant % 2 * 18, wound, 7 + stage);
            for (int i = 0; i < 6 + stage; i++)
            {
                int tx = cx - 58 + i * 22;
                DrawLine(tex, tx, cy + 72, tx + ((i % 2 == 0) ? -9 : 9), cy + 105 + stage * 4, new Color(0.93f, 0.85f, 0.62f, 0.78f), 3 + stage / 2);
            }

            if (stage == 1)
                DrawStationEnemyDetails(tex, cx, cy, stage, dark, coldGlow, wound, variant);
            else if (stage == 2)
                DrawCastleEnemyDetails(tex, cx, cy, stage, dark, coldGlow, wound, variant);
            else if (stage == 3)
                DrawMisoEnemyDetails(tex, cx, cy, stage, dark, coldGlow, wound, variant);
            else if (stage == 4)
                DrawSeaEnemyDetails(tex, cx, cy, stage, dark, coldGlow, wound, variant);
            else
                DrawAirportEnemyDetails(tex, cx, cy, stage, dark, coldGlow, wound, variant);

            for (int i = 0; i < 16 + stage * 6; i++)
            {
                int sx = 80 + (seed * 37 + i * 61) % 610;
                int sy = 96 + (seed * 53 + i * 89) % 555;
                int len = 24 + (i % 5) * (8 + stage);
                DrawLine(tex, sx, sy, sx + (i % 2 == 0 ? len : -len / 2), sy + len, new Color(wound.r, wound.g, wound.b, 0.05f + menace * 0.035f), 1 + stage / 3);
            }

            if (stage >= 5)
            {
                DrawCircle(tex, cx, cy, 205, new Color(0.05f, 0f, 0.08f, 0.18f));
                DrawLine(tex, cx - 235, cy - 190, cx + 225, cy + 185, coldGlow, 4);
                DrawLine(tex, cx + 230, cy - 170, cx - 220, cy + 205, coldGlow, 4);
            }
        }

        static int EnemyImageStage(string name)
        {
            if (name == "impossible_one")
                return 6;
            if (name.StartsWith("instability_") && int.TryParse(name.Substring("instability_".Length), out int instabilityStage))
                return Mathf.Clamp(instabilityStage + 1, 1, 6);
            if (name.StartsWith("stage_boss_") && int.TryParse(name.Substring("stage_boss_".Length), out int bossStage))
                return Mathf.Clamp(bossStage, 1, 5);
            if (name.StartsWith("stage"))
            {
                int marker = name.IndexOf("_enemy_", StringComparison.Ordinal);
                if (marker > 5 && int.TryParse(name.Substring(5, marker - 5), out int stage))
                    return Mathf.Clamp(stage, 1, 5);
            }
            if (name.Contains("airport") || name.Contains("window") || name.Contains("baggage"))
                return 5;
            if (name.Contains("deep") || name.Contains("polyps"))
                return 4;
            if (name.Contains("miso") || name.Contains("quality") || name.Contains("shadow"))
                return 3;
            if (name.Contains("shachi") || name.Contains("well") || name.Contains("sword") || name.Contains("battlefield"))
                return 2;
            return 1;
        }

        static int EnemyImageVariant(string name, int seed)
        {
            int marker = name.IndexOf("_enemy_", StringComparison.Ordinal);
            if (marker >= 0 && int.TryParse(name.Substring(marker + "_enemy_".Length), out int variant))
                return variant;
            return Mathf.Abs(seed % 10) + 1;
        }

        static void DrawStationEnemyDetails(Texture2D tex, int cx, int cy, int stage, Color dark, Color glow, Color wound, int variant)
        {
            DrawRect(tex, cx - 145, cy - 190, 290, 44, new Color(0.04f, 0.09f, 0.08f, 0.72f));
            for (int i = 0; i < 6; i++)
                DrawRect(tex, cx - 116 + i * 42, cy - 178, 24, 18, glow);
            DrawLine(tex, cx - 180, cy + 145, cx + 180, cy + 145, glow, 5);
            DrawLine(tex, cx - 135, cy + 175, cx + 135, cy + 175, new Color(0.7f, 0.84f, 0.76f, 0.42f), 4);
            if (variant % 2 == 0)
                DrawLine(tex, cx - 42, cy - 198, cx + 36, cy - 250, wound, 6);
        }

        static void DrawCastleEnemyDetails(Texture2D tex, int cx, int cy, int stage, Color dark, Color glow, Color wound, int variant)
        {
            DrawLine(tex, cx - 52, cy - 160, cx - 132, cy - 250, wound, 8);
            DrawLine(tex, cx + 52, cy - 160, cx + 132, cy - 250, wound, 8);
            DrawShachi(tex, cx - 118, cy - 190, -0.9f, glow);
            DrawShachi(tex, cx + 118, cy - 190, 0.9f, glow);
            DrawRect(tex, cx - 165, cy + 118, 330, 30, new Color(0.13f, 0.1f, 0.06f, 0.72f));
            if (variant % 3 == 0)
                DrawCircle(tex, cx, cy - 205, 28, new Color(0.9f, 0.55f, 0.18f, 0.62f));
        }

        static void DrawMisoEnemyDetails(Texture2D tex, int cx, int cy, int stage, Color dark, Color glow, Color wound, int variant)
        {
            for (int i = 0; i < 9; i++)
            {
                int bx = cx - 120 + i * 30;
                int by = cy - 150 + (i % 4) * 38;
                DrawCircle(tex, bx, by, 12 + i % 3 * 5, new Color(0.72f, 0.34f, 0.12f, 0.48f));
            }
            DrawLine(tex, cx - 175, cy + 122, cx + 178, cy + 110, new Color(0.58f, 0.22f, 0.1f, 0.74f), 18);
            DrawLine(tex, cx - 210, cy - 25, cx - 265, cy + 105, glow, 7);
            DrawLine(tex, cx + 210, cy - 25, cx + 265, cy + 105, glow, 7);
            if (variant % 2 == 1)
                DrawRect(tex, cx - 36, cy - 228, 72, 46, new Color(0.35f, 0.13f, 0.05f, 0.72f));
        }

        static void DrawSeaEnemyDetails(Texture2D tex, int cx, int cy, int stage, Color dark, Color glow, Color wound, int variant)
        {
            for (int i = 0; i < 7; i++)
            {
                int dir = i % 2 == 0 ? -1 : 1;
                int sx = cx + dir * (38 + i * 13);
                DrawLine(tex, sx, cy + 40, sx + dir * (95 + i * 12), cy + 185 + (i % 3) * 28, glow, 9);
                DrawCircle(tex, sx + dir * (100 + i * 12), cy + 190 + (i % 3) * 28, 15, wound);
            }
            DrawCircle(tex, cx - 88, cy - 138, 24, new Color(0.45f, 0.78f, 0.88f, 0.42f));
            DrawCircle(tex, cx + 94, cy - 152, 18, new Color(0.45f, 0.78f, 0.88f, 0.38f));
            DrawLine(tex, cx - 170, cy + 210, cx + 170, cy + 210, new Color(0.08f, 0.3f, 0.42f, 0.62f), 10);
        }

        static void DrawAirportEnemyDetails(Texture2D tex, int cx, int cy, int stage, Color dark, Color glow, Color wound, int variant)
        {
            for (int i = 0; i < 5; i++)
            {
                int x = cx - 150 + i * 75;
                DrawRect(tex, x - 22, cy - 220 + (i % 2) * 18, 44, 78, new Color(0.92f, 0.86f, 0.64f, 0.34f));
                DrawLine(tex, x - 12, cy - 196 + (i % 2) * 18, x + 12, cy - 154 + (i % 2) * 18, wound, 3);
            }
            DrawLine(tex, cx - 225, cy + 165, cx + 225, cy + 165, glow, 7);
            DrawLine(tex, cx - 185, cy + 210, cx + 185, cy + 210, new Color(0.65f, 0.62f, 0.92f, 0.42f), 5);
            DrawCircle(tex, cx - 128, cy - 104, 20, wound);
            DrawCircle(tex, cx + 128, cy - 104, 20, wound);
            if (stage >= 6)
            {
                DrawCircle(tex, cx, cy, 245, new Color(0.02f, 0f, 0.05f, 0.28f));
                for (int i = 0; i < 12; i++)
                {
                    float angle = i / 12f * Mathf.PI * 2f;
                    int x = cx + Mathf.RoundToInt(Mathf.Cos(angle) * 235f);
                    int y = cy + Mathf.RoundToInt(Mathf.Sin(angle) * 235f);
                    DrawLine(tex, cx, cy, x, y, new Color(0.42f, 0.02f, 0.72f, 0.22f), 5);
                }
            }
        }

        static void GenerateSfx()
        {
            WriteWav(Path.Combine(SfxRoot, "click.wav"), 0.09f, 880f, 0.22f, WaveKind.Sine, 0.08f);
            WriteWav(Path.Combine(SfxRoot, "hit.wav"), 0.16f, 150f, 0.7f, WaveKind.Noise, 0.24f);
            WriteWav(Path.Combine(SfxRoot, "hurt.wav"), 0.22f, 95f, 0.75f, WaveKind.Saw, 0.2f);
            WriteWav(Path.Combine(SfxRoot, "doom.wav"), 0.9f, 55f, 0.82f, WaveKind.SineDrop, 0.28f);
            WriteWav(Path.Combine(SfxRoot, "reward.wav"), 0.38f, 660f, 0.45f, WaveKind.Chime, 0.18f);
            WriteWav(Path.Combine(SfxRoot, "whisper.wav"), 0.72f, 180f, 0.38f, WaveKind.Whisper, 0.5f);
            WriteWav(Path.Combine(SfxRoot, "page.wav"), 0.24f, 320f, 0.5f, WaveKind.Page, 0.16f);
            WriteWav(Path.Combine(SfxRoot, "ambient.wav"), 6.0f, 48f, 0.32f, WaveKind.Ambient, 6.0f);
        }

        enum WaveKind
        {
            Sine,
            Saw,
            Noise,
            SineDrop,
            Chime,
            Whisper,
            Page,
            Ambient
        }

        static void WriteWav(string path, float duration, float frequency, float volume, WaveKind kind, float decay)
        {
            const int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            var random = new System.Random(1234 + path.GetHashCode());
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(stream))
            {
                int byteRate = sampleRate * 2;
                int dataLength = samples * 2;
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + dataLength);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(sampleRate);
                writer.Write(byteRate);
                writer.Write((short)2);
                writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                writer.Write(dataLength);

                for (int i = 0; i < samples; i++)
                {
                    float t = i / (float)sampleRate;
                    float env = Mathf.Exp(-t / Mathf.Max(0.001f, decay));
                    float f = kind == WaveKind.SineDrop ? Mathf.Lerp(frequency, frequency * 0.35f, t / duration) : frequency;
                    float value;
                    switch (kind)
                    {
                        case WaveKind.Noise:
                            value = (float)(random.NextDouble() * 2.0 - 1.0);
                            break;
                        case WaveKind.Saw:
                            value = 2f * (t * f - Mathf.Floor(0.5f + t * f));
                            break;
                        case WaveKind.Chime:
                            value = Mathf.Sin(Mathf.PI * 2f * f * t) * 0.7f + Mathf.Sin(Mathf.PI * 2f * f * 1.5f * t) * 0.3f;
                            break;
                        case WaveKind.Whisper:
                            value = (float)(random.NextDouble() * 2.0 - 1.0) * Mathf.Sin(Mathf.PI * 2f * (f * 0.08f) * t);
                            value += Mathf.Sin(Mathf.PI * 2f * f * t) * 0.08f;
                            break;
                        case WaveKind.Page:
                            value = (float)(random.NextDouble() * 2.0 - 1.0) * Mathf.Clamp01(1f - t / duration);
                            value += Mathf.Sin(Mathf.PI * 2f * (f + 240f * t) * t) * 0.18f;
                            break;
                        case WaveKind.Ambient:
                            value = Mathf.Sin(Mathf.PI * 2f * f * t) * 0.38f;
                            value += Mathf.Sin(Mathf.PI * 2f * (f * 1.51f) * t) * 0.22f;
                            value += Mathf.Sin(Mathf.PI * 2f * (f * 0.49f) * t) * 0.18f;
                            value += (float)(random.NextDouble() * 2.0 - 1.0) * 0.035f;
                            env = 0.65f + Mathf.Sin(Mathf.PI * 2f * 0.08f * t) * 0.18f;
                            break;
                        default:
                            value = Mathf.Sin(Mathf.PI * 2f * f * t);
                            break;
                    }

                    short sample = (short)Mathf.Clamp(value * env * volume * short.MaxValue, short.MinValue, short.MaxValue);
                    writer.Write(sample);
                }
            }
        }

        static void DrawRect(Texture2D tex, int x, int y, int w, int h, Color color)
        {
            for (int yy = Mathf.Max(0, y); yy < Mathf.Min(tex.height, y + h); yy++)
            {
                for (int xx = Mathf.Max(0, x); xx < Mathf.Min(tex.width, x + w); xx++)
                    Blend(tex, xx, yy, color);
            }
        }

        static void DrawCircle(Texture2D tex, int cx, int cy, int r, Color color)
        {
            int r2 = r * r;
            for (int y = cy - r; y <= cy + r; y++)
            {
                for (int x = cx - r; x <= cx + r; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;
                    if (dx * dx + dy * dy <= r2)
                        Blend(tex, x, y, color);
                }
            }
        }

        static void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color color, int thickness = 1)
        {
            int dx = Math.Abs(x1 - x0);
            int sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0);
            int sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            while (true)
            {
                DrawCircle(tex, x0, y0, thickness, color);
                if (x0 == x1 && y0 == y1)
                    break;
                int e2 = 2 * err;
                if (e2 >= dy)
                {
                    err += dy;
                    x0 += sx;
                }
                if (e2 <= dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        static void Blend(Texture2D tex, int x, int y, Color color)
        {
            if (x < 0 || x >= tex.width || y < 0 || y >= tex.height)
                return;
            var baseColor = tex.GetPixel(x, y);
            tex.SetPixel(x, y, Color.Lerp(baseColor, color, color.a));
        }

        static void DrawVignette(Texture2D tex)
        {
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    float nx = (x - tex.width * 0.5f) / (tex.width * 0.5f);
                    float ny = (y - tex.height * 0.5f) / (tex.height * 0.5f);
                    float d = Mathf.Clamp01(Mathf.Sqrt(nx * nx + ny * ny));
                    Blend(tex, x, y, new Color(0f, 0f, 0f, Mathf.SmoothStep(0.15f, 0.82f, d) * 0.55f));
                }
            }
        }

        static void DrawMist(Texture2D tex, Color color)
        {
            for (int y = 0; y < tex.height; y += 2)
            {
                for (int x = 0; x < tex.width; x += 2)
                {
                    float n = Mathf.PerlinNoise(x * 0.008f, y * 0.014f);
                    if (n > 0.52f)
                    Blend(tex, x, y, new Color(color.r, color.g, color.b, color.a * (n - 0.5f) * 2.2f));
                }
            }
        }

        static void DrawLightLeaks(Texture2D tex, Color glow)
        {
            for (int i = 0; i < 4; i++)
            {
                int cx = 130 + i * 310;
                int cy = 120 + (i % 2) * 430;
                int radius = 180 + i * 35;
                for (int y = cy - radius; y <= cy + radius; y += 2)
                {
                    for (int x = cx - radius; x <= cx + radius; x += 2)
                    {
                        float dx = (x - cx) / (float)radius;
                        float dy = (y - cy) / (float)radius;
                        float d = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                        if (d > 0f)
                            Blend(tex, x, y, new Color(glow.r, glow.g, glow.b, d * d * 0.075f));
                    }
                }
            }
        }

        static void DrawScratches(Texture2D tex)
        {
            for (int i = 0; i < 70; i++)
            {
                int x = (i * 97) % tex.width;
                int y = (i * 211) % tex.height;
                int len = 35 + (i % 9) * 18;
                var color = i % 3 == 0
                    ? new Color(0.95f, 0.82f, 0.55f, 0.035f)
                    : new Color(0f, 0f, 0f, 0.08f);
                DrawLine(tex, x, y, x + 12 - (i % 5) * 5, y + len, color, 1);
            }
        }

        static void DrawMoon(Texture2D tex, int cx, int cy, int r, Color color)
        {
            DrawCircle(tex, cx, cy, r, color);
            DrawCircle(tex, cx - 28, cy + 8, r - 8, new Color(0.02f, 0.018f, 0.03f, 0.55f));
        }

        static void DrawCastle(Texture2D tex, int x, int y, float scale, Color color)
        {
            DrawRect(tex, x - Mathf.RoundToInt(190 * scale), y, Mathf.RoundToInt(380 * scale), Mathf.RoundToInt(80 * scale), color);
            DrawRect(tex, x - Mathf.RoundToInt(135 * scale), y - Mathf.RoundToInt(80 * scale), Mathf.RoundToInt(270 * scale), Mathf.RoundToInt(85 * scale), color);
            DrawRect(tex, x - Mathf.RoundToInt(85 * scale), y - Mathf.RoundToInt(150 * scale), Mathf.RoundToInt(170 * scale), Mathf.RoundToInt(80 * scale), color);
            DrawLine(tex, x - Mathf.RoundToInt(115 * scale), y - Mathf.RoundToInt(80 * scale), x, y - Mathf.RoundToInt(140 * scale), color, Mathf.RoundToInt(8 * scale));
            DrawLine(tex, x + Mathf.RoundToInt(115 * scale), y - Mathf.RoundToInt(80 * scale), x, y - Mathf.RoundToInt(140 * scale), color, Mathf.RoundToInt(8 * scale));
            DrawLine(tex, x - Mathf.RoundToInt(190 * scale), y, x - Mathf.RoundToInt(120 * scale), y - Mathf.RoundToInt(70 * scale), color, Mathf.RoundToInt(10 * scale));
            DrawLine(tex, x + Mathf.RoundToInt(190 * scale), y, x + Mathf.RoundToInt(120 * scale), y - Mathf.RoundToInt(70 * scale), color, Mathf.RoundToInt(10 * scale));
        }

        static void DrawShachi(Texture2D tex, int x, int y, float scale, Color color)
        {
            int dir = scale < 0 ? -1 : 1;
            float s = Mathf.Abs(scale);
            DrawCircle(tex, x, y, Mathf.RoundToInt(24 * s), color);
            DrawLine(tex, x, y, x + dir * Mathf.RoundToInt(80 * s), y - Mathf.RoundToInt(12 * s), color, Mathf.RoundToInt(12 * s));
            DrawLine(tex, x + dir * Mathf.RoundToInt(65 * s), y - Mathf.RoundToInt(10 * s), x + dir * Mathf.RoundToInt(95 * s), y - Mathf.RoundToInt(55 * s), color, Mathf.RoundToInt(8 * s));
            DrawLine(tex, x - dir * Mathf.RoundToInt(8 * s), y - Mathf.RoundToInt(18 * s), x - dir * Mathf.RoundToInt(30 * s), y - Mathf.RoundToInt(45 * s), color, Mathf.RoundToInt(6 * s));
        }

        static void DrawFigure(Texture2D tex, int x, int y, float scale, Color color)
        {
            int r = Mathf.RoundToInt(28 * scale);
            DrawCircle(tex, x, y - Mathf.RoundToInt(115 * scale), r, color);
            DrawRect(tex, x - Mathf.RoundToInt(32 * scale), y - Mathf.RoundToInt(90 * scale), Mathf.RoundToInt(64 * scale), Mathf.RoundToInt(120 * scale), color);
            DrawLine(tex, x - Mathf.RoundToInt(28 * scale), y - Mathf.RoundToInt(10 * scale), x - Mathf.RoundToInt(70 * scale), y + Mathf.RoundToInt(60 * scale), color, Mathf.RoundToInt(7 * scale));
            DrawLine(tex, x + Mathf.RoundToInt(28 * scale), y - Mathf.RoundToInt(10 * scale), x + Mathf.RoundToInt(70 * scale), y + Mathf.RoundToInt(60 * scale), color, Mathf.RoundToInt(7 * scale));
        }

        static void DrawDie(Texture2D tex, int x, int y, int size, int value)
        {
            DrawRect(tex, x - size / 2, y - size / 2, size, size, new Color(0.86f, 0.78f, 0.62f, 0.75f));
            DrawRect(tex, x - size / 2 + 8, y - size / 2 + 8, size - 16, size - 16, new Color(0.08f, 0.05f, 0.06f, 0.35f));
            int d = size / 4;
            if (value == 1 || value == 3 || value == 5) DrawCircle(tex, x, y, 9, Color.black);
            if (value >= 2)
            {
                DrawCircle(tex, x - d, y - d, 8, Color.black);
                DrawCircle(tex, x + d, y + d, 8, Color.black);
            }
            if (value >= 4)
            {
                DrawCircle(tex, x + d, y - d, 8, Color.black);
                DrawCircle(tex, x - d, y + d, 8, Color.black);
            }
            if (value == 6)
            {
                DrawCircle(tex, x - d, y, 8, Color.black);
                DrawCircle(tex, x + d, y, 8, Color.black);
            }
        }

        static void DrawSubway(Texture2D tex, Color wall, Color light)
        {
            DrawRect(tex, 0, 430, tex.width, 290, new Color(0.01f, 0.011f, 0.014f, 0.8f));
            for (int i = 0; i < 9; i++)
            {
                int x = i * 170 - 60;
                DrawLine(tex, x, 120, 610, 430, wall, 6);
                DrawLine(tex, tex.width - x, 120, 670, 430, wall, 6);
            }
            for (int i = 0; i < 7; i++)
                DrawRect(tex, 210 + i * 130, 130, 74, 12, light);
        }

        static void DrawSign(Texture2D tex, int x, int y, string unused)
        {
            DrawRect(tex, x - 210, y - 35, 420, 70, new Color(0.02f, 0.05f, 0.04f, 0.82f));
            for (int i = 0; i < 8; i++)
                DrawRect(tex, x - 170 + i * 42, y - 8, 25, 16, new Color(0.7f, 0.92f, 0.75f, 0.5f));
        }

        static void DrawNoodles(Texture2D tex, int cx, int cy)
        {
            DrawCircle(tex, cx, cy + 70, 150, new Color(0.2f, 0.08f, 0.035f, 0.85f));
            DrawCircle(tex, cx, cy + 40, 126, new Color(0.68f, 0.54f, 0.34f, 0.7f));
            for (int i = 0; i < 13; i++)
                DrawLine(tex, cx - 140 + i * 24, cy - 100, cx - 80 + i * 18, cy + 120, new Color(0.9f, 0.76f, 0.48f, 0.45f), 4);
        }

        static void DrawSteam(Texture2D tex, Color color)
        {
            for (int i = 0; i < 8; i++)
                DrawLine(tex, 360 + i * 75, 460, 420 + i * 60, 160, color, 12);
        }

        static void DrawMarket(Texture2D tex)
        {
            for (int i = 0; i < 8; i++)
            {
                int x = 80 + i * 155;
                int h = 220 + (i % 3) * 55;
                DrawRect(tex, x, 420 - h, 100, h, new Color(0.04f, 0.03f, 0.045f, 0.9f));
                DrawRect(tex, x + 10, 420 - h + 24, 80, 12, new Color(0.45f, 0.13f, 0.18f, 0.55f));
            }
        }

        static void DrawLanterns(Texture2D tex)
        {
            for (int i = 0; i < 11; i++)
                DrawCircle(tex, 90 + i * 115, 185 + (i % 2) * 30, 18, new Color(0.85f, 0.25f, 0.16f, 0.55f));
        }

        static void DrawRain(Texture2D tex)
        {
            for (int i = 0; i < 240; i++)
            {
                int x = (i * 83) % tex.width;
                int y = (i * 191) % tex.height;
                DrawLine(tex, x, y, x - 15, y + 46, new Color(0.45f, 0.6f, 0.75f, 0.18f), 1);
            }
        }

        static void DrawBarrels(Texture2D tex)
        {
            for (int i = 0; i < 6; i++)
            {
                int x = 150 + i * 180;
                DrawCircle(tex, x, 440, 82, new Color(0.2f, 0.09f, 0.035f, 0.88f));
                DrawRect(tex, x - 80, 350, 160, 180, new Color(0.16f, 0.07f, 0.03f, 0.86f));
                DrawLine(tex, x - 76, 395, x + 76, 395, new Color(0.45f, 0.25f, 0.1f, 0.45f), 4);
            }
        }

        static void DrawHugeBarrel(Texture2D tex)
        {
            DrawCircle(tex, 640, 360, 210, new Color(0.13f, 0.045f, 0.02f, 0.9f));
            DrawCircle(tex, 640, 360, 155, new Color(0.32f, 0.12f, 0.04f, 0.5f));
            DrawLine(tex, 460, 280, 820, 280, new Color(0.74f, 0.38f, 0.13f, 0.35f), 8);
            DrawLine(tex, 460, 440, 820, 440, new Color(0.74f, 0.38f, 0.13f, 0.35f), 8);
        }

        static void DrawGlyphs(Texture2D tex, Color color)
        {
            for (int i = 0; i < 10; i++)
            {
                int x = 150 + i * 105;
                DrawCircle(tex, x, 170 + (i % 3) * 70, 24, color);
                DrawLine(tex, x - 22, 170 + (i % 3) * 70, x + 22, 170 + (i % 3) * 70, color, 3);
            }
        }

        static void DrawFactory(Texture2D tex)
        {
            DrawRect(tex, 0, 430, tex.width, 180, new Color(0.03f, 0.04f, 0.045f, 0.85f));
            for (int i = 0; i < 7; i++)
            {
                DrawRect(tex, 70 + i * 170, 250 - (i % 2) * 50, 75, 190 + (i % 2) * 50, new Color(0.035f, 0.045f, 0.05f, 0.85f));
                DrawCircle(tex, 107 + i * 170, 220 - (i % 2) * 50, 28, new Color(0.05f, 0.14f, 0.12f, 0.35f));
            }
        }

        static void DrawConveyor(Texture2D tex)
        {
            DrawRect(tex, 0, 520, tex.width, 60, new Color(0.08f, 0.09f, 0.09f, 0.85f));
            for (int i = 0; i < 13; i++)
                DrawCircle(tex, 70 + i * 100, 550, 22, new Color(0.16f, 0.18f, 0.17f, 0.75f));
        }

        static void DrawCeramicHill(Texture2D tex)
        {
            for (int i = 0; i < 12; i++)
                DrawCircle(tex, 90 + i * 105, 470 + (i % 3) * 18, 62, new Color(0.32f, 0.17f, 0.08f, 0.65f));
            DrawLine(tex, 170, 520, 1120, 280, new Color(0.18f, 0.1f, 0.06f, 0.8f), 24);
        }

        static void DrawCatEyes(Texture2D tex)
        {
            for (int i = 0; i < 5; i++)
            {
                int x = 340 + i * 120;
                DrawCircle(tex, x, 250, 10, new Color(0.86f, 0.68f, 0.32f, 0.65f));
                DrawCircle(tex, x + 38, 250, 10, new Color(0.86f, 0.68f, 0.32f, 0.65f));
            }
        }

        static void DrawAirport(Texture2D tex)
        {
            DrawRect(tex, 80, 415, 1120, 58, new Color(0.05f, 0.06f, 0.075f, 0.85f));
            DrawRect(tex, 240, 300, 820, 100, new Color(0.04f, 0.055f, 0.07f, 0.82f));
            for (int i = 0; i < 9; i++)
                DrawRect(tex, 280 + i * 80, 330, 45, 24, new Color(0.2f, 0.38f, 0.55f, 0.38f));
            DrawLine(tex, 140, 570, 1160, 570, new Color(0.42f, 0.48f, 0.5f, 0.35f), 6);
        }

        static void DrawSeaThing(Texture2D tex)
        {
            DrawCircle(tex, 640, 650, 210, new Color(0.02f, 0.035f, 0.055f, 0.8f));
            DrawLine(tex, 500, 560, 780, 560, new Color(0.08f, 0.2f, 0.25f, 0.5f), 10);
        }

        static void DrawPiyo(Texture2D tex, int x, int y, float scale)
        {
            int r = Mathf.RoundToInt(36 * scale);
            DrawCircle(tex, x, y, r, new Color(0.94f, 0.72f, 0.18f, 0.82f));
            DrawCircle(tex, x - Mathf.RoundToInt(12 * scale), y - Mathf.RoundToInt(8 * scale), Mathf.RoundToInt(4 * scale), Color.black);
            DrawCircle(tex, x + Mathf.RoundToInt(12 * scale), y - Mathf.RoundToInt(8 * scale), Mathf.RoundToInt(4 * scale), Color.black);
            DrawLine(tex, x - Mathf.RoundToInt(8 * scale), y + Mathf.RoundToInt(10 * scale), x + Mathf.RoundToInt(8 * scale), y + Mathf.RoundToInt(10 * scale), new Color(0.5f, 0.22f, 0.08f, 0.7f), Mathf.RoundToInt(3 * scale));
        }

        static void DrawDoor(Texture2D tex)
        {
            DrawRect(tex, 520, 170, 240, 400, new Color(0.035f, 0.03f, 0.05f, 0.9f));
            DrawRect(tex, 548, 205, 184, 330, new Color(0.08f, 0.055f, 0.075f, 0.75f));
            DrawCircle(tex, 715, 370, 10, new Color(0.9f, 0.68f, 0.28f, 0.65f));
            DrawLine(tex, 640, 170, 640, 570, new Color(0.75f, 0.56f, 0.25f, 0.18f), 4);
        }
    }
}
