using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AichiFantasy
{
    public sealed class AichiFantasyGame : MonoBehaviour
    {
        const string SaveKey = "AichiFantasyTRPG.Progress";
        enum Mode
        {
            Title,
            CharacterSelect,
            Dice,
            Scene,
            Battle,
            Ending,
            Unlocks
        }
        [Serializable]
        sealed class Progress
        {
            public int memoryFragments;
            public int insuranceTickets;
            public int maxInstabilityUnlocked;
            public int piyorinVictories;
            public int misoVictories;
            public int gateVictories;
            public List<string> unlockedCharacters = new List<string> { "traveler", "worker", "local", "occult" };
            public List<string> endings = new List<string>();
            public List<string> defeated = new List<string>();
            public List<string> deaths = new List<string>();
            public List<string> seenMonsters = new List<string>();
            public List<string> monsterWeaknesses = new List<string>();
            public List<string> bossesDefeated = new List<string>();
            public List<string> brokenGear = new List<string>();
            public List<string> regretLog = new List<string>();
            public List<string> milestoneClaims = new List<string>();
            public List<string> warehouseGear = new List<string>();
            public List<string> awakenedGear = new List<string>();
            public List<string> rememberedChoices = new List<string>();
        }
        sealed class CharacterDef
        {
            public string id;
            public string name;
            public string subtitle;
            public string description;
            public int unlockCost;
            public Stats stats;
            public string weapon;
            public string armor;
            public float rewardRate = 1f;
            public string ability;
        }
        sealed class Stats
        {
            public int maxHp;
            public int hp;
            public int maxMp;
            public int mp;
            public int attack;
            public int defense;
            public int speed;
            public int luck;
            public int maxSanity;
            public int sanity;
            public int hunger;
            public int money;
            public int localKnowledge;
            public int misoResistance;
            public int machineAptitude;
            public int mythosKnowledge;
            public int mythosCorruption;
            public Stats Clone()
            {
                return (Stats)MemberwiseClone();
            }
      }
        sealed class Gear
        {
            public string id;
            public string name;
            public string kind;
            public string slot;
            public string rarity;
            public string setTag;
            public int attack;
            public int defense;
            public int speed;
            public int luck;
            public int score;
            public string note;
            public Gear Clone()
            {
                return (Gear)MemberwiseClone();
            }
        }
        sealed class Choice
        {
            public string label;
            public string next;
            public string battle;
            public string ending;
            public Action<RunState> effect;
            public Func<RunState, bool> condition;
            public string disabledReason;
        }
        sealed class ChoiceCommand
        {
            public string label;
            public Action action;
            public bool interactable;
        }

      sealed class SceneDef
        {
            public string id;
            public string title;
            public string area;
            public string image;
            public string portrait;
            public string text;
            public List<Choice> choices = new List<Choice>();
        }
        sealed class EnemyDef
        {
            public string id;
            public string name;
            public string image;
            public int maxHp;
            public int hp;
            public int attack;
            public int defense;
            public int speed;
            public int sanityDamage;
            public int reward;
            public string intro;
            public string victoryText;
            public string defeatEnding;
            public string weakness;
            public string portrait;
        }
        sealed class RandomEventDef
        {
            public string id;
            public string title;
            public string area;
            public string image;
            public string portrait;
            public string text;
            public string successLabel;
            public string failLabel;
            public int difficulty;
            public string successText;
           public string failText;
            public string failBattle;
            public Action<RunState> success;
            public Action<RunState> fail;
        }
        sealed class RunState
        {
            public CharacterDef character;
            public Stats stats;
            public Gear weapon;
            public Gear armor;
            public Gear accessory;
            public Gear pendingGear;
            public string sceneId;
            public string pendingSceneAfterRandom;
            public string battleReturnScene;
            public string lastChoiceLabel;
            public string pendingOutcomeText;
            public string lastRollSummary;
            public string routeGoal;
            public string contract;
            public int instability;
            public int dangerWarnings = 1;
            public int steps;
            public int randomCooldown;
            public int owari;
            public int mikawa;
            public int npcCafe;
            public int npcOccult;
            public int npcAirport;
            public int shachiGaze;
            public int ogura;
            public int restsUsed;
            public int sanityCollapseTurns = -1;
            public string sanityCollapseReturnScene;
            public HashSet<string> flags = new HashSet<string>();
            public List<string> recentLog = new List<string>();
        }
      Mode mode;
        Progress progress;
        RunState run;
        EnemyDef activeEnemy;
        float attackWindow;
        float attackGauge;
        float enemyAttackTimer;
        float guardWindow;
        float guardGauge;
        int battleRound;
        string endingTitle;
        string endingBody;
        int lastReward;
        int selectedInstability;
        int characterPage;
        int choicePage;
        int warehouseGearPage;
        CharacterDef pendingCharacter;
        Canvas canvas;
        RawImage background;
        Image vignette;
        Image madnessOverlay;
        Image portraitImage;
        RectTransform portraitPanel;
        Slider hpSlider;
        Text hpText;
        Text coinText;
        Text titleText;
        Text areaText;
        Text bodyText;
        Text statsText;
        Text inventoryText;
        Text footerText;
        RectTransform choiceRoot;
        Button[] choiceButtons;
        Text[] choiceButtonLabels;
        readonly List<ChoiceCommand> choiceCommands = new List<ChoiceCommand>();
        RectTransform battleRoot;
        Text battleText;
        Slider enemyHpSlider;
       Slider attackSlider;
        Slider guardSlider;
        Button attackButton;
        Button guardButton;
        AudioSource audioSource;
        AudioSource ambientSource;
        AudioClip clickSfx;
        AudioClip hitSfx;
        AudioClip hurtSfx;
        AudioClip doomSfx;
        AudioClip rewardSfx;
        AudioClip whisperSfx;
        AudioClip pageSfx;
        AudioClip ambientSfx;
        AudioClip walkSfx;
        AudioClip eventSfx;
        readonly Dictionary<string, CharacterDef> characters = new Dictionary<string, CharacterDef>();
        readonly Dictionary<string, Gear> gears = new Dictionary<string, Gear>();
        readonly Dictionary<string, SceneDef> scenes = new Dictionary<string, SceneDef>();
        readonly Dictionary<string, EnemyDef> enemies = new Dictionary<string, EnemyDef>();
        readonly string[] contracts = { "無契約", "血の契約: 攻撃+2 / 最大HP-3", "灯の契約: LUK+2 / SAN-2", "倉庫契約: 記憶定着+1 / 報酬-10%" };
        readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
        readonly Dictionary<string, Sprite> portraitCache = new Dictionary<string, Sprite>();
        readonly System.Random rng = new System.Random();
        bool diceRolling;
        void Awake()
        {
            BuildData();
            LoadProgress();
            BuildUi();
            LoadAudio();
            ShowTitle();
        }
        void Update()
        {
            UpdateMadnessVisuals();
            if (mode != Mode.Battle || activeEnemy == null)
               return;
            if (attackWindow > 0f)
            {
                float speed = 0.18f + Mathf.Max(0, run.stats.speed + run.weapon.speed + run.accessory.speed) * 0.018f;
                attackGauge = Mathf.Min(1f, attackGauge + Time.deltaTime * speed);
                attackSlider.value = attackGauge;
                attackButton.interactable = true;
            }
            if (guardWindow > 0f)
            {
                float speed = 0.09f + Mathf.Max(0, run.stats.luck + run.accessory.luck + run.stats.mythosKnowledge) * 0.008f;
                guardGauge = Mathf.Min(1f, guardGauge + Time.deltaTime * speed);
                guardSlider.value = guardGauge;
                guardButton.interactable = guardGauge >= 1f;
            }
            if (enemyAttackTimer > 0f)
            {
                enemyAttackTimer -= Time.deltaTime;
                if (enemyAttackTimer <= 0f)
                    ResolveEnemyAttack();
            }
        }
        void BuildUi()
        {
            var existingEventSystem = FindObjectOfType<EventSystem>();
            if (existingEventSystem == null)
            {
                var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                if (Application.isPlaying)
                    DontDestroyOnLoad(eventSystemObject);
            }
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 1.0f;
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.playOnAwake = false;
            ambientSource.loop = true;
           ambientSource.volume = 0.34f;
            canvas = NewObject<Canvas>("Canvas", transform);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            canvas.gameObject.AddComponent<GraphicRaycaster>();
            var root = canvas.transform;
            background = NewRawImage("Background", root, Color.black);
            Stretch(background.rectTransform, 0, 0, 0, 0);
            var veil = NewImage("BlueBlackVeil", root, new Color(0.02f, 0.015f, 0.03f, 0.32f));
            Stretch(veil.rectTransform, 0, 0, 0, 0);
            vignette = NewImage("Vignette", root, new Color(0f, 0f, 0f, 0.42f));
            Stretch(vignette.rectTransform, 0, 0, 0, 0);
            madnessOverlay = NewImage("MadnessOverlay", root, new Color(0.35f, 0.02f, 0.04f, 0f));
            Stretch(madnessOverlay.rectTransform, 0, 0, 0, 0);
            var topBar = NewPanel("TopBar", root, new Color(0.015f, 0.012f, 0.018f, 0.78f));
            Anchor(topBar, 0, 0.89f, 1, 0.992f, 8, 0, -8, -4);
            AddBorder(topBar, new Color(0.75f, 0.52f, 0.22f, 0.25f));
            hpSlider = NewSlider("TopHP", topBar, new Color(0.72f, 0.05f, 0.08f));
            Anchor(hpSlider.GetComponent<RectTransform>(), 0.055f, 0.46f, 0.33f, 0.76f, 0, 0, 0, 0);
            hpText = NewText("HPText", topBar, 20, FontStyle.Bold, new Color(0.98f, 0.92f, 0.86f));
            Anchor(hpText.rectTransform, 0.11f, 0.45f, 0.32f, 0.86f, 0, 0, 0, 0);
            hpText.alignment = TextAnchor.MiddleCenter;
            coinText = NewText("CoinText", topBar, 20, FontStyle.Bold, new Color(0.95f, 0.78f, 0.32f));
            Anchor(coinText.rectTransform, 0.055f, 0.08f, 0.32f, 0.42f, 0, 0, 0, 0);
            titleText = NewText("Title", topBar, 24, FontStyle.Bold, new Color(0.95f, 0.72f, 0.28f));
            Anchor(titleText.rectTransform, 0.335f, 0.08f, 0.665f, 0.92f, 0, 0, 0, 0);
            titleText.alignment = TextAnchor.MiddleCenter;
            areaText = NewText("Area", topBar, 16, FontStyle.Normal, new Color(0.7f, 0.82f, 0.92f));
           Anchor(areaText.rectTransform, 0.68f, 0.08f, 0.985f, 0.92f, 4, 0, -14, 0);
            areaText.alignment = TextAnchor.MiddleRight;
            var sidePanel = NewPanel("StatusPanel", root, new Color(0.025f, 0.02f, 0.03f, 0.78f));
            Anchor(sidePanel, 0.745f, 0.045f, 0.985f, 0.36f, 0, 0, 0, 0);
            AddBorder(sidePanel, new Color(0.75f, 0.52f, 0.22f, 0.22f));
            statsText = NewText("Stats", sidePanel, 16, FontStyle.Normal, new Color(0.88f, 0.9f, 0.86f));
            Anchor(statsText.rectTransform, 0, 0, 1, 1, 16, 8, -16, -14);
            var leftPanel = NewPanel("InventoryPanel", root, new Color(0.025f, 0.02f, 0.03f, 0.78f));
            Anchor(leftPanel, 0.015f, 0.045f, 0.255f, 0.36f, 0, 0, 0, 0);
            AddBorder(leftPanel, new Color(0.75f, 0.52f, 0.22f, 0.22f));
            inventoryText = NewText("Inventory", leftPanel, 16, FontStyle.Normal, new Color(0.78f, 0.82f, 0.76f));
            Anchor(inventoryText.rectTransform, 0, 0, 1, 1, 16, 10, -16, -8);
            var storyPanel = NewPanel("StoryPanel", root, new Color(0.018f, 0.014f, 0.02f, 0.84f));
            Anchor(storyPanel, 0.27f, 0.245f, 0.73f, 0.425f, 0, 0, 0, 0);
            AddBorder(storyPanel, new Color(0.72f, 0.5f, 0.22f, 0.28f));
            portraitPanel = NewPanel("PortraitPanel", root, new Color(0.01f, 0.008f, 0.012f, 0.08f));
            Anchor(portraitPanel, 0.22f, 0.43f, 0.78f, 0.902f, 0, 0, 0, 0);
            portraitPanel.GetComponent<Image>().raycastTarget = false;
            portraitImage = NewImage("PortraitImage", portraitPanel, Color.clear);
            Anchor(portraitImage.rectTransform, 0, 0, 1, 1, 0, 0, 0, 0);
            portraitImage.preserveAspect = true;
            portraitImage.raycastTarget = false;
            bodyText = NewText("Body", storyPanel, 18, FontStyle.Normal, new Color(0.93f, 0.9f, 0.82f));
            Anchor(bodyText.rectTransform, 0, 0, 1, 1, 16, 12, -16, -12);
            bodyText.alignment = TextAnchor.UpperLeft;
            bodyText.lineSpacing = 1.18f;
            choiceRoot = NewObject<RectTransform>("Choices", root);
            Anchor(choiceRoot, 0.245f, 0.045f, 0.755f, 0.225f, 0, 0, 0, 0);
            var choiceLayout = choiceRoot.gameObject.AddComponent<GridLayoutGroup>();
            choiceLayout.cellSize = new Vector2(300f, 58f);
            choiceLayout.spacing = new Vector2(12f, 12f);
            choiceLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            choiceLayout.constraintCount = 2;
            choiceLayout.childAlignment = TextAnchor.MiddleCenter;
            CreateChoiceSlots();
          battleRoot = NewObject<RectTransform>("Battle", root);
            Anchor(battleRoot, 0.27f, 0.055f, 0.73f, 0.19f, 0, 0, 0, 0);
            var battleBg = battleRoot.gameObject.AddComponent<Image>();
            battleBg.color = new Color(0.05f, 0.025f, 0.035f, 0.9f);
            AddBorder(battleRoot, new Color(0.78f, 0.22f, 0.16f, 0.32f));
            battleText = NewText("BattleText", battleRoot, 14, FontStyle.Bold, new Color(1f, 0.87f, 0.62f));
            Anchor(battleText.rectTransform, 0.02f, 0.08f, 0.34f, 0.92f, 0, 0, 0, 0);
            enemyHpSlider = NewSlider("EnemyHP", battleRoot, new Color(0.65f, 0.08f, 0.11f));
            Anchor(enemyHpSlider.GetComponent<RectTransform>(), 0.36f, 0.63f, 0.58f, 0.86f, 0, 0, 0, 0);
            attackSlider = NewSlider("AttackGauge", battleRoot, new Color(0.94f, 0.65f, 0.18f));
            Anchor(attackSlider.GetComponent<RectTransform>(), 0.36f, 0.34f, 0.58f, 0.55f, 0, 0, 0, 0);
            guardSlider = NewSlider("SkillGauge", battleRoot, new Color(0.32f, 0.66f, 0.95f));
            Anchor(guardSlider.GetComponent<RectTransform>(), 0.36f, 0.1f, 0.58f, 0.27f, 0, 0, 0, 0);
            attackButton = NewButton("AttackButton", battleRoot, "攻撃", new Color(0.45f, 0.09f, 0.09f), 22);
            Anchor(attackButton.GetComponent<RectTransform>(), 0.61f, 0.1f, 0.78f, 0.86f, 0, 0, 0, 0);
            attackButton.onClick.AddListener(OnAttackTap);
            guardButton = NewButton("SkillButton", battleRoot, "固有スキル", new Color(0.1f, 0.22f, 0.36f), 22);
            Anchor(guardButton.GetComponent<RectTransform>(), 0.80f, 0.1f, 0.98f, 0.86f, 0, 0, 0, 0);
            guardButton.onClick.AddListener(OnGuardTap);
            footerText = NewText("Footer", root, 12, FontStyle.Normal, new Color(0.7f, 0.68f, 0.62f));
            Anchor(footerText.rectTransform, 0.045f, 0.006f, 0.955f, 0.04f, 0, 0, 0, 0);
            footerText.alignment = TextAnchor.MiddleCenter;
        }
        void LoadAudio()
        {
            clickSfx = Resources.Load<AudioClip>("AichiFantasy/Sfx/click");
            hitSfx = Resources.Load<AudioClip>("AichiFantasy/Sfx/hit");
            hurtSfx = Resources.Load<AudioClip>("AichiFantasy/Sfx/hurt");
            doomSfx = Resources.Load<AudioClip>("AichiFantasy/Sfx/doom");
            rewardSfx = Resources.Load<AudioClip>("AichiFantasy/Sfx/reward");
            whisperSfx = Resources.Load<AudioClip>("AichiFantasy/Sfx/whisper");
            pageSfx = Resources.Load<AudioClip>("AichiFantasy/Sfx/page");
            ambientSfx = Resources.Load<AudioClip>("AichiFantasy/Sfx/ambient");
           walkSfx = Resources.Load<AudioClip>("AichiFantasy/Sfx/walk");
            eventSfx = Resources.Load<AudioClip>("AichiFantasy/Sfx/event_ominous");
            if (ambientSfx != null && ambientSource != null)
            {
                ambientSource.clip = ambientSfx;
                ambientSource.Play();
            }
        }
        void ShowTitle()
        {
            mode = Mode.Title;
            activeEnemy = null;
            SetBackground("title");
            SetPortrait(null);
            battleRoot.gameObject.SetActive(false);
            choiceRoot.gameObject.SetActive(true);
            titleText.text = "愛知ファンタジーTRPG";
            areaText.text = "異界愛知へようこそ";
            bodyText.text =
                "金鯱が泳ぐ夜空の下、名駅の地下街は内側へ折れ曲がっている。\n\n" +
                "あなたは何度でも迷い込み、何度でも死に、記憶片を拾い集める。\n" +
                "双鯱、味噌樽、工場心臓。知るほど正気は削られるが、知らなければ帰れない。";
            if (hpSlider != null) hpSlider.value = 0;
            if (hpText != null) hpText.text = "";
            if (coinText != null) coinText.text = "● " + progress.memoryFragments;
            statsText.text = "周回通貨: " + progress.memoryFragments + " 記憶片\n到達エンド: " + progress.endings.Count +
                             "\n死因登録: " + progress.deaths.Count + "\n怪異遭遇: " + progress.seenMonsters.Count;
            inventoryText.text = "本番向け要素:\n死因図鑑\n怪異図鑑\n正気度UI変化\nランダムイベント\n\n同梱SEはゲーム用に生成した仮素材です。";
            footerText.text = "暗いご当地ファンタジー / クトゥルフ風探索 / 連打バトル";
            ClearChoices();
            AddChoiceButton("はじめる", ShowInstabilitySelect);
            AddChoiceButton("拠点/倉庫", ShowBase);
            AddChoiceButton("図鑑と解放", ShowUnlocks);
            AddChoiceButton("進行を初期化", ResetProgress);
        }
        void ShowInstabilitySelect()
        {
            mode = Mode.CharacterSelect;
            SetBackground("title");
          SetPortrait("event_shachi_avatar");
            battleRoot.gameObject.SetActive(false);
            choiceRoot.gameObject.SetActive(true);
            titleText.text = "境界不安定度";
            areaText.text = "周回難度を選択";
            bodyText.text = "異界愛知との境界を、どれだけ薄くして進むかを選ぶ。\n\n不安定度が高いほど敵とイベントは危険になるが、記憶片、レア装備、真相イベントの気配も濃くなる。低い不安定度にはいつでも戻れる。";
            statsText.text = "解放済み: " + progress.maxInstabilityUnlocked + "\n保険札: " + progress.insuranceTickets;
            inventoryText.text = "0 通常\n1 微細な歪み\n2 漏れ出す異界\n3 神話の夜\n4 双鯱の注視\n5 境界崩壊";
            ClearChoices();
            int max = Mathf.Clamp(progress.maxInstabilityUnlocked, 0, 5);
            for (int i = 0; i <= max; i++)
            {
                int level = i;
                AddChoiceButton(InstabilityName(level) + "\n報酬x" + InstabilityReward(level).ToString("0.0"), () =>
                {
                    selectedInstability = level;
                    ShowCharacterSelect();
                });
            }
            AddChoiceButton("戻る", ShowTitle);
        }
        void ShowCharacterSelect()
        {
            mode = Mode.CharacterSelect;
            SetBackground("characters");
            SetPortrait("traveler");
            battleRoot.gameObject.SetActive(false);
            choiceRoot.gameObject.SetActive(true);
            titleText.text = "\u30ad\u30e3\u30e9\u30af\u30bf\u30fc\u9078\u629e";
            areaText.text = "\u30af\u30ea\u30c3\u30af\u3067\u8a73\u7d30\u78ba\u8a8d";
            bodyText.text = "\u4f7f\u3046\u30ad\u30e3\u30e9\u30af\u30bf\u30fc\u3092\u9078\u3093\u3067\u304f\u3060\u3055\u3044\u3002\u30af\u30ea\u30c3\u30af\u5f8c\u306b\u78ba\u8a8d\u753b\u9762\u3092\u8868\u793a\u3057\u307e\u3059\u3002";
            statsText.text = "\u8a18\u61b6\u7247 " + progress.memoryFragments + "\n\u4fdd\u967a\u672d " + progress.insuranceTickets;
            inventoryText.text = "\u5883\u754c\u4e0d\u5b89\u5b9a\u5ea6:\n" + InstabilitySummary(selectedInstability);
            ClearChoices();
            var list = new List<CharacterDef>(characters.Values);
            int pageSize = 5;
            int maxPage = Mathf.Max(0, (list.Count - 1) / pageSize);
            characterPage = Mathf.Clamp(characterPage, 0, maxPage);
            bodyText.text += "\n\n\u30da\u30fc\u30b8 " + (characterPage + 1) + "/" + (maxPage + 1) + "\u3002\u672a\u89e3\u653e\u30ad\u30e3\u30e9\u306f\u5fc5\u8981\u306a\u8a18\u61b6\u7247\u3092\u8868\u793a\u3057\u307e\u3059\u3002";
            int start = characterPage * pageSize;
          int end = Mathf.Min(list.Count, start + pageSize);
            for (int i = start; i < end; i++)
            {
                var character = list[i];
                bool unlocked = progress.unlockedCharacters.Contains(character.id);
                var s = character.stats;
                string compactStats = "HP" + s.maxHp + " 攻" + s.attack + " 防" + s.defense + " 速" + s.speed + " 運" + s.luck;
                string label = unlocked ? character.name + "\n" + compactStats : character.name + "\n\u672a\u89e3\u653e " + character.unlockCost + " \u8a18\u61b6\u7247";
                AddChoiceButton(label, () => ShowCharacterConfirm(character));
            }
            if (characterPage > 0)
                AddChoiceButton("\u524d\u30da\u30fc\u30b8", () => { characterPage--; ShowCharacterSelect(); });
            if (characterPage < maxPage)
                AddChoiceButton("\u6b21\u30da\u30fc\u30b8", () => { characterPage++; ShowCharacterSelect(); });
            AddChoiceButton("\u96e3\u5ea6\u3078\u623b\u308b", ShowInstabilitySelect);
        }
        void ShowCharacterConfirm(CharacterDef character)
        {
            pendingCharacter = character;
            bool unlocked = progress.unlockedCharacters.Contains(character.id);
            SetPortrait(CharacterPortraitId(character.id));
            titleText.text = character.name;
            areaText.text = unlocked ? "\u9078\u629e\u78ba\u8a8d" : "\u89e3\u653e\u78ba\u8a8d";
            var s = character.stats;
            string unlockLine = unlocked ? "\u89e3\u653e\u6e08\u307f" : "\u672a\u89e3\u653e: \u5fc5\u8981 " + character.unlockCost + " \u8a18\u61b6\u7247 / \u6240\u6301 " + progress.memoryFragments;
            bodyText.text = unlockLine + "\n" + character.subtitle + "\n" + character.description + "\n\nHP " + s.maxHp + " / MP " + s.maxMp +
                "\n\u653b\u6483 " + s.attack + " \u9632\u5fa1 " + s.defense + " \u901f\u3055 " + s.speed + " LUK " + s.luck +
                "\nSAN " + s.maxSanity + " / \u5831\u916c\u500d\u7387 x" + character.rewardRate.ToString("0.00") +
                "\n\n持ち込みを選ばない場合、武器・防具・装飾はすべて装備なしで開始します。";
            statsText.text = character.name + "\nHP " + s.hp + "/" + s.maxHp + "  MP " + s.mp + "/" + s.maxMp +
                "\n攻撃 " + s.attack + "  防御 " + s.defense +
                "\n速さ " + s.speed + "  LUK " + s.luck +
                "\nSAN " + s.sanity + "/" + s.maxSanity +
                "\n所持金 " + s.money +
                "\n地元 " + s.localKnowledge + " 味噌 " + s.misoResistance +
                "\n機械 " + s.machineAptitude + " 神話 " + s.mythosKnowledge;
            ClearChoices();
            if (unlocked)
                AddChoiceButton("\u3053\u306e\u30ad\u30e3\u30e9\u3067\u958b\u59cb", () => StartRun(pendingCharacter));
            else
                AddChoiceButton("\u8a18\u61b6\u7247\u3067\u89e3\u653e", () => TryUnlockCharacter(pendingCharacter));
                            AddChoiceButton("\u4e00\u89a7\u3078\u623b\u308b", ShowCharacterSelect);
        }
        string CharacterPortraitId(string id)
        {
            switch (id)
            {
                case "occult": return "occult_researcher";
                default: return id;
            }
        }
        void TryUnlockCharacter(CharacterDef character)
        {
            if (progress.memoryFragments < character.unlockCost)
            {
                Play(clickSfx);
                footerText.text = "記憶片が足りません。";
                return;
            }

            progress.memoryFragments -= character.unlockCost;
            progress.unlockedCharacters.Add(character.id);
            SaveProgress();
            Play(rewardSfx);
            ShowCharacterSelect();
        }
        void ShowBase()
        {
            mode = Mode.Unlocks;
            activeEnemy = null;
            SetBackground("characters");
            SetPortrait("event_occult_researcher");
            battleRoot.gameObject.SetActive(false);
            choiceRoot.gameObject.SetActive(true);
            titleText.text = "拠点/倉庫";
            areaText.text = "異界愛知対策室";
            bodyText.text = "持ち帰った装備、NPC依頼、覚醒進捗を確認する。\n\n倉庫装備: " + progress.warehouseGear.Count + "\n覚醒装備: " + progress.awakenedGear.Count + "\n記憶した選択肢: " + progress.rememberedChoices.Count;
            statsText.text = "記憶片 " + progress.memoryFragments + "\n保険札 " + progress.insuranceTickets;
            inventoryText.text = WarehouseSummary();
            ClearChoices();
            AddChoiceButton("倉庫装備を覚醒\n記憶片15", AwakenWarehouseGear);
            AddChoiceButton("NPC依頼を見る", ShowNpcRequests);
            AddChoiceButton("タイトルへ", ShowTitle);
        }
        string WarehouseSummary()
        {
            if (progress.warehouseGear.Count == 0)
                return "倉庫は空です。\n死亡時の記憶定着や戦利品送付で装備が入ります。";
            string text = "倉庫:\n";
            for (int i = Mathf.Max(0, progress.warehouseGear.Count - 5); i < progress.warehouseGear.Count; i++)
            {
                var gear = DeserializeGear(progress.warehouseGear[i]);
                text += "・" + (gear != null ? gear.name + " [" + gear.rarity + "]" : progress.warehouseGear[i]) + "\n";
            }
            return text;
        }
        void AwakenWarehouseGear()
        {
           if (progress.memoryFragments < 15 || progress.warehouseGear.Count == 0)
            {
                footerText.text = "覚醒には記憶片15と倉庫装備が必要です。";
                return;
            }
            int index = progress.warehouseGear.Count - 1;
            var gear = DeserializeGear(progress.warehouseGear[index]);
            if (gear == null)
                return;
            progress.memoryFragments -= 15;
            gear.attack += 1;
            gear.defense += 1;
            gear.luck += 1;
            gear.note += " 覚醒済み。";
            ApplyGearRarity(gear);
            progress.warehouseGear[index] = SerializeGear(gear);
            progress.awakenedGear.Add(gear.name);
            SaveProgress();
            Play(rewardSfx);
            ShowBase();
        }
        void ShowNpcRequests()
        {
            titleText.text = "NPC依頼";
            bodyText.text = "喫茶店員: 小倉の印を3つ集める\nオカルト研究者: 神話理解を5以上にする\n空港職員: 検査官を突破する\n\n依頼は周回中の行動で関係値が上がり、日報と報酬に反映されます。";
            inventoryText.text = "関係値は周回ごとに変動します。今後は永続NPC好感度へ拡張可能。";
            ClearChoices();
            AddChoiceButton("拠点へ戻る", ShowBase);
        }
        void StartRun(CharacterDef character)
        {
            run = new RunState();
            run.character = character;
            run.stats = character.stats.Clone();
            run.weapon = EmptyGear("武器");
            run.armor = EmptyGear("防具");
            run.accessory = EmptyGear("装飾品");
            run.sceneId = "nagoya_start";
            run.instability = selectedInstability;
            if (character.id != "traveler")
               run.dangerWarnings = 0;
            LogRun("周回開始: 装備確認へ");
            ShowStartingGearSelect();
        }
        void ShowStartingGearSelect()
        {
            mode = Mode.Scene;
            SetBackground("characters");
            SetPortrait(CharacterPortraitId(run.character.id));
            battleRoot.gameObject.SetActive(false);
            choiceRoot.gameObject.SetActive(true);
            titleText.text = "開始装備の確認";
            areaText.text = "倉庫から取り出す";
            bodyText.text = "倉庫の装備を取り出してから出発できます。取り出した装備は倉庫から外れ、現在の周回装備になります。\n\n" +
                "何も選ばなければ、武器・防具・装飾品なしで開始します。\n\n倉庫装備: " + progress.warehouseGear.Count;
            UpdateSideText();
            ClearChoices();
            if (progress.warehouseGear.Count > 0)
            {
                int pageSize = 2;
                int maxPage = Mathf.Max(0, (progress.warehouseGear.Count - 1) / pageSize);
                warehouseGearPage = Mathf.Clamp(warehouseGearPage, 0, maxPage);
                int start = warehouseGearPage * pageSize;
                int end = Mathf.Min(progress.warehouseGear.Count, start + pageSize);
                for (int i = start; i < end; i++)
                {
                    int index = i;
                    var gear = DeserializeGear(progress.warehouseGear[index]);
                    string label = gear != null ? "取り出す\n" + GearShortName(gear) : "取り出す\n不明な装備";
                    AddChoiceButton(label, () => TakeWarehouseGear(index), gear != null);
                }
                if (warehouseGearPage < maxPage)
                    AddChoiceButton("次の倉庫装備", () => { warehouseGearPage++; ShowStartingGearSelect(); });
                else if (warehouseGearPage > 0)
                    AddChoiceButton("前の倉庫装備", () => { warehouseGearPage--; ShowStartingGearSelect(); });
            }
            AddChoiceButton("この装備で契約へ", ShowRunContract);
            AddChoiceButton("キャラ選択へ戻る", ShowCharacterSelect);
      }
        void TakeWarehouseGear(int index)
        {
            if (index < 0 || index >= progress.warehouseGear.Count)
            {
                ShowStartingGearSelect();
                return;
            }
            var gear = DeserializeGear(progress.warehouseGear[index]);
            if (gear == null)
            {
                progress.warehouseGear.RemoveAt(index);
                SaveProgress();
                ShowStartingGearSelect();
                return;
            }
            progress.warehouseGear.RemoveAt(index);
            var current = CurrentGearForSlot(gear.slot);
            if (current != null && !IsEmptyGear(current))
                StoreGear(current);
            EquipGear(gear);
            SaveProgress();
            LogRun("倉庫から装備: " + GearShortName(gear) + (current != null && !IsEmptyGear(current) ? " / 前装備は倉庫へ" : ""));
            footerText.text = GearShortName(gear) + " を装備しました。";
            if (warehouseGearPage > 0 && warehouseGearPage * 2 >= progress.warehouseGear.Count)
                warehouseGearPage--;
            ShowStartingGearSelect();
        }
        void ShowRunContract()
        {
            mode = Mode.Scene;
            SetBackground("characters");
            SetPortrait(CharacterPortraitId(run.character.id));
            titleText.text = "周回契約";
            areaText.text = run.character.name;
            bodyText.text = "異界愛知へ入る前に、今回の契約を選ぶ。\n\n目標カードは廃止しました。今回の周回は、探索・イベント・戦闘を重ねて空港境界の突破を目指します。";
            UpdateSideText();
            ClearChoices();
           for (int i = 0; i < contracts.Length; i++)
            {
                int index = i;
                AddChoiceButton(contracts[index], () =>
                {
                    run.contract = contracts[index];
                    ApplyContract(run.contract);
                    ShowDice();
                });
            }
        }
        void ApplyContract(string contract)
        {
            if (contract.StartsWith("血"))
            {
                run.stats.attack += 2;
                run.stats.maxHp = Math.Max(1, run.stats.maxHp - 3);
                run.stats.hp = Math.Min(run.stats.hp, run.stats.maxHp);
            }
            else if (contract.StartsWith("灯"))
            {
                run.stats.luck += 2;
                run.stats.sanity = Math.Max(1, run.stats.sanity - 2);
            }
        }
        void ShowDice()
        {
            mode = Mode.Dice;
            SetBackground("dice");
            SetPortrait(run.character.id == "occult" ? "occult_researcher" : null);
            titleText.text = "運命の三つの目";
            areaText.text = run.character.name;
            bodyText.text = "異界に落ちる瞬間、あなたの運命は三つの六面体に刻まれる。\n\n" +
                            "出目が悪いほど危険だが、死亡時の記憶片は少し増える。";
            UpdateSideText();
            ClearChoices();
            AddChoiceButton("さいころを振る", RollDice);
            AddChoiceButton("キャラ選択へ戻る", ShowCharacterSelect);
        }

       void RollDice()
        {
            if (diceRolling)
                return;
            StartCoroutine(RollDiceRoutine());
        }
        IEnumerator RollDiceRoutine()
        {
            diceRolling = true;
            ClearChoices();
            Play(pageSfx);
            for (int i = 0; i < 18; i++)
            {
                int ra = rng.Next(1, 7);
                int rb = rng.Next(1, 7);
                int rc = rng.Next(1, 7);
                bodyText.text = "さいころが転がっている...\n\n出目: " + ra + " / " + rb + " / " + rc;
                yield return new WaitForSeconds(0.035f + i * 0.006f);
            }
            int a = rng.Next(1, 7);
            int b = rng.Next(1, 7);
            int c = rng.Next(1, 7);
            int total = a + b + c;
            string result;
            if (total <= 5)
            {
                run.stats.maxHp -= 2;
                run.stats.hp = Mathf.Min(run.stats.hp, run.stats.maxHp);
                run.stats.sanity -= 1;
                run.flags.Add("bad_dice");
                result = "不吉な目覚め。HP -2、正気度 -1。だが死の記憶は濃くなる。";
            }
            else if (total <= 8)
            {
               run.stats.hp -= 1;
                run.stats.hunger += 1;
                result = "疲れた目覚め。HP -1、空腹度 +1。";
            }
            else if (total <= 12)
            {
                result = "普通の目覚め。異界はまだ、あなたを特別扱いしていない。";
            }
            else if (total <= 15)
            {
                run.stats.money += 300;
                run.flags.Add("lucky_item");
                result = "幸運な目覚め。所持金 +300。地下街の風が少しだけ味方した。";
            }
            else
            {
                run.stats.attack += 1;
                run.stats.mythosCorruption += 1;
                run.shachiGaze += 1;
                result = "異界に選ばれた目覚め。攻撃力 +1、神話汚染 +1。夜空の鯱がこちらを見た。";
            }
            Play(rewardSfx);
            bodyText.text = "出目: " + a + " / " + b + " / " + c + "  合計 " + total + "\n\n" + result;
            UpdateSideText();
            ClearChoices();
            AddChoiceButton("名駅の底で目覚める", () => ShowScene("nagoya_start", false));
            diceRolling = false;
        }
        void ShowScene(string sceneId, bool allowRandom = true)
        {
            if (TryShowSanityCollapse(sceneId))
                return;
            if (allowRandom && TryShowRandomEvent(sceneId))
                return;
            mode = Mode.Scene;
            if (run != null && run.steps > 0)
                Play(walkSfx, 0.65f);
            activeEnemy = null;
            run.sceneId = sceneId;
            if (sceneId == run.pendingSceneAfterRandom)
                run.pendingSceneAfterRandom = null;
           run.steps++;
            if (run.randomCooldown > 0)
                run.randomCooldown--;
            var scene = scenes[sceneId];
            battleRoot.gameObject.SetActive(false);
            choiceRoot.gameObject.SetActive(true);
            SetBackground(scene.image);
            SetPortrait(scene.portrait);
            titleText.text = scene.title;
            areaText.text = scene.area;
            bodyText.text = scene.text;
            if (!string.IsNullOrEmpty(run.pendingOutcomeText))
            {
                bodyText.text += "\n\n直前の結果\n" + run.pendingOutcomeText;
                run.pendingOutcomeText = null;
            }
            if (SceneHasLuckChoice(scene))
                bodyText.text += "\n\nLUK判定: 6面ダイス3個 + LUK補正(LUK/2、最大+6)で判定。LUKが高いほど3個目が6になりやすい。成功で報酬、失敗でHP/SANを失う。";
            UpdateSideText();
            ClearChoices();
            foreach (var choice in scene.choices)
            {
                bool enabled = choice.condition == null || choice.condition(run);
                string label = BuildChoiceLabel(choice, enabled);
                AddChoiceButton(label, () =>
                {
                    if (!enabled)
                    {
                        footerText.text = choice.disabledReason;
                        return;
                    }
                    run.lastChoiceLabel = choice.label;
                    if (!progress.rememberedChoices.Contains(choice.label))
                        progress.rememberedChoices.Add(choice.label);
                    if (choice.label.Contains("きしめん") || choice.label.Contains("小倉") || choice.label.Contains("食"))
                        run.npcCafe++;
                    if (choice.label.Contains("神話") || choice.label.Contains("予言") || choice.label.Contains("攻略"))
                        run.npcOccult++;
                    if (choice.label.Contains("空港") || choice.label.Contains("搭乗") || choice.label.Contains("検査"))
                        run.npcAirport++;
                    if (choice.effect != null && choice.label.Contains("LUK判定"))
                    {
                        StartCoroutine(ResolveChoiceWithDiceAnimation(choice));
                        return;
                    }
                    var before = run.stats.Clone();
                   run.lastRollSummary = null;
                    choice.effect?.Invoke(run);
                    string delta = BuildStatDelta(before, run.stats);
                    string outcome = choice.label.Replace("\n", " / ") + "\n" + delta;
                    if (!string.IsNullOrEmpty(run.lastRollSummary))
                        outcome += "\n" + run.lastRollSummary;
                    LogRun(choice.label + ": " + delta + (!string.IsNullOrEmpty(run.lastRollSummary) ? " / " + run.lastRollSummary : ""));
                    if (!string.IsNullOrEmpty(choice.battle))
                        StartBattle(choice.battle);
                    else if (!string.IsNullOrEmpty(choice.ending))
                        ShowEnding(choice.ending);
                    else if (choice.effect != null)
                        ShowChoiceOutcome(choice.next, outcome);
                    else
                        ShowScene(choice.next);
                }, enabled);
            }
            if (sceneId == "airport_gate")
            {
                AddChoiceButton("ルート別ボスに挑む", () =>
                {
                    string boss = run.mikawa > run.owari ? "shadow_retainer" : run.shachiGaze >= 2 ? "window_god" : "sword_shadow";
                    run.battleReturnScene = "airport_gate";
                    StartBattle(boss);
                });
                AddChoiceButton("キャラ専用エンドへ進む", () => ShowEnding("personal_" + run.character.id));
                AddChoiceButton("空港周辺へ戻る", () => ShowScene("airport_bridge", false));
            }
        }
        IEnumerator ResolveChoiceWithDiceAnimation(Choice choice)
        {
            ClearChoices();
            int luckBonus = Mathf.Clamp(run.stats.luck / 2, 0, 6);
            string label = choice.label.Replace("\n", " / ");
            for (int i = 0; i < 8; i++)
            {
                int a = rng.Next(1, 7);
                int b = rng.Next(1, 7);
                int c = rng.Next(1, 7);
                bodyText.text = "サイコロが転がっている...\n\n" + label + "\n[" + a + "] [" + b + "] [" + c + "]  LUK補正 +" + luckBonus;
                Play(clickSfx, 0.18f);
                yield return new WaitForSeconds(0.08f + i * 0.018f);
            }

            var before = run.stats.Clone();
            run.lastRollSummary = null;
            choice.effect?.Invoke(run);
            string delta = BuildStatDelta(before, run.stats);
            string outcome = label + "\n" + delta;
            if (!string.IsNullOrEmpty(run.lastRollSummary))
                outcome += "\n" + run.lastRollSummary;
            LogRun(label + ": " + delta + (!string.IsNullOrEmpty(run.lastRollSummary) ? " / " + run.lastRollSummary : ""));

            if (!string.IsNullOrEmpty(choice.battle))
                StartBattle(choice.battle);
            else if (!string.IsNullOrEmpty(choice.ending))
                ShowEnding(choice.ending);
            else
                ShowChoiceOutcome(choice.next, outcome);
        }
        void ShowChoiceOutcome(string nextSceneId, string outcome)
        {
            mode = Mode.Scene;
            battleRoot.gameObject.SetActive(false);
            choiceRoot.gameObject.SetActive(true);
            titleText.text = "行動結果";
            areaText.text = "イベント結果";
            bodyText.text = "今回の行動で起きたこと\n\n" + OutcomeFlavor(outcome) + "\n\n" + outcome;
            footerText.text = "結果を確認してから次へ進みます。";
            UpdateSideText();
            ClearChoices();
            AddChoiceButton("次に進む", () => ShowScene(nextSceneId, false));
       }
        string OutcomeFlavor(string outcome)
        {
            if (string.IsNullOrEmpty(outcome))
                return "足音だけが、少し遅れてついてきた。";
            if (outcome.Contains("失敗") || outcome.Contains("足りない") || outcome.Contains("HP-") || outcome.Contains("SAN-"))
                return "選択の余韻が、冷たい痛みとして残った。それでも道はまだ閉じていない。";
            if (outcome.Contains("成功") || outcome.Contains("上回った") || outcome.Contains("所持金+") || outcome.Contains("LUK+"))
                return "迷いの中で、ひとつだけ正しい手触りがあった。";
            if (outcome.Contains("所持金-"))
                return "硬貨の音が現実側へ落ち、代わりに少しだけ安全が残った。";
            return "小さな判断が、次の景色の色を変えた。";
        }
        bool TryShowRandomEvent(string targetSceneId)
        {
            if (run == null || run.steps <= 0 || run.randomCooldown > 0)
                return false;
            if (!string.IsNullOrEmpty(run.pendingSceneAfterRandom))
                return false;
            if (targetSceneId == "airport_gate" || targetSceneId == "nagoya_start")
                return false;
            float chance = 0.48f + run.instability * 0.055f + Mathf.Clamp01(run.stats.mythosCorruption * 0.045f) + Mathf.Clamp01(progress.endings.Count * 0.02f);
            if (rng.NextDouble() > chance)
            {
                if (run.steps >= 3 && rng.NextDouble() < 0.22f)
                {
                    string ambush = SelectAmbushEnemy(targetSceneId);
                    if (!string.IsNullOrEmpty(ambush))
                    {
                        run.pendingSceneAfterRandom = targetSceneId;
                        run.battleReturnScene = targetSceneId;
                        run.randomCooldown = 2;
                        Play(eventSfx, 0.72f);
                        LogRun("割り込み戦闘: " + EnemyName(ambush));
                        StartBattle(ambush);
                        return true;
                    }
                }
                return false;
            }
            run.pendingSceneAfterRandom = targetSceneId;
            run.randomCooldown = 2;
            LogRun("ランダムイベント発生");
            ShowRandomEvent(targetSceneId);
            return true;
        }
        string SelectAmbushEnemy(string sceneId)
        {
            if (!scenes.TryGetValue(sceneId, out var scene))
                return "";
            string image = scene.image;
            if (image == "castle") return rng.NextDouble() < 0.5 ? "battlefield_spear" : "well_tentacle";
            if (image == "okazaki" || image == "miso") return rng.NextDouble() < 0.5 ? "miso_voice" : "shadow_retainer";
            if (image == "airport") return rng.NextDouble() < 0.5 ? "gate_guard" : "baggage_mouth";
            if (image == "toyota") return "quality_golem";
            if (image == "tokoname") return "kiln_crawler";
            if (image == "osu") return rng.NextDouble() < 0.5 ? "index_hound" : "dream_eater";
            if (image == "station" || image == "kishimen") return rng.NextDouble() < 0.5 ? "last_train" : "piyorin";
            return rng.NextDouble() < 0.5 ? "index_hound" : "dream_eater";
        }
        string BuildChoiceLabel(Choice choice, bool enabled)
        {
            string label = enabled ? choice.label : choice.label + "\n" + choice.disabledReason;
            if (enabled && !string.IsNullOrEmpty(choice.ending) && progress.deaths.Contains(choice.ending))
                label += "\n前回の死因に近い";
            if (enabled && !string.IsNullOrEmpty(choice.battle) && progress.seenMonsters.Contains(choice.battle) && !progress.monsterWeaknesses.Contains(choice.battle))
                label += "\n怪異図鑑: 未解析";
            if (enabled && !string.IsNullOrEmpty(choice.battle) && progress.monsterWeaknesses.Contains(choice.battle))
                label += "\n怪異図鑑: 弱点記録済み";
            if (run != null && run.stats.sanity <= Mathf.CeilToInt(run.stats.maxSanity * 0.28f))
                label = DistortLabel(label);
            return label;
        }
       bool SceneHasLuckChoice(SceneDef scene)
        {
            if (scene == null || scene.choices == null)
                return false;
            foreach (var choice in scene.choices)
            {
                if (!string.IsNullOrEmpty(choice.label) && choice.label.Contains("LUK"))
                    return true;
            }
            return false;
        }
        string DistortLabel(string label)
        {
            if (string.IsNullOrEmpty(label))
                return label;
            char[] chars = label.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '\n' || chars[i] == ' ')
                    continue;
                if (rng.NextDouble() < 0.07)
                    chars[i] = rng.NextDouble() < 0.5 ? '・' : '■';
            }
            return new string(chars);
        }
        void ShowRandomEvent(string targetSceneId)
        {
            mode = Mode.Scene;
            Play(eventSfx != null ? eventSfx : whisperSfx, 0.78f);
            battleRoot.gameObject.SetActive(false);
            choiceRoot.gameObject.SetActive(true);
            ClearChoices();
            string area = scenes.ContainsKey(targetSceneId) ? scenes[targetSceneId].area : "異界愛知";
            int roll = rng.Next(0, 100);
            if (rng.NextDouble() < 0.72)
            {
                ShowExpandedRandomEvent(targetSceneId);
                return;
            }
            if (area.Contains("岡崎") && roll < 35)
            {
                SetBackground("miso");
                SetPortrait("event_miso_voice");
                titleText.text = "ランダムイベント: 樽鳴り";
                areaText.text = "岡崎 / 神話";
                bodyText.text = "通りすがりの味噌樽が、内側から三度だけ鳴った。\n\n一度目はあなたの名前。二度目はまだ見ていない死因。三度目は、出口の方角だった。";
                AddChoiceButton("耳をふさいで進む", () =>
                {
                    run.stats.sanity = Math.Max(0, run.stats.sanity - 1);
                    ShowScene(run.pendingSceneAfterRandom, false);
                });
                AddChoiceButton("三度目だけ聞き取る", () =>
                {
                    run.stats.mythosKnowledge += 1;
                    run.stats.sanity = Math.Max(0, run.stats.sanity - 2);
                    progress.memoryFragments += 2;
                    SaveProgress();
                    ShowScene(run.pendingSceneAfterRandom, false);
                });
            }
            else if (area.Contains("名古屋") && roll < 35)
            {
                SetBackground("kishimen");
                SetPortrait("event_cafe_server");
                titleText.text = "ランダムイベント: 深夜モーニング";
                areaText.text = "名古屋 / 喫茶";
                bodyText.text = "存在しないはずの喫茶店が、まだ開いている。\n\n席には湯気の立つ皿が置かれていた。あなたのために、ずっと前から用意されていたように。";
                AddChoiceButton("食べる", () =>
                {
                    run.stats.hp = Math.Min(run.stats.maxHp, run.stats.hp + 5);
                    run.stats.hunger = Math.Max(0, run.stats.hunger - 2);
                    run.ogura += 1;
                    ShowScene(run.pendingSceneAfterRandom, false);
                });
                AddChoiceButton("断る", () =>
                {
                    run.stats.localKnowledge += 1;
                    run.stats.sanity = Math.Max(0, run.stats.sanity - 1);
                    ShowScene(run.pendingSceneAfterRandom, false);
                });
            }
            else if (area.Contains("常滑") && roll < 45)
            {
                SetBackground("tokoname");
                SetPortrait("event_shachi_avatar");
                titleText.text = "ランダムイベント: 招き猫の瞬き";
                areaText.text = "常滑 / 境界";
                bodyText.text = "巨大な招き猫が、ゆっくりと瞬きした。\n\nその一瞬だけ、海上空港の灯りが別の世界の星座に見える。";
                AddChoiceButton("礼をして通る", () =>
                {
                    run.stats.localKnowledge += 1;
                    progress.memoryFragments += 1;
                    SaveProgress();
                    ShowScene(run.pendingSceneAfterRandom, false);
                });
                AddChoiceButton("視線に逆らう", () =>
                {
                    run.battleReturnScene = run.pendingSceneAfterRandom;
                    StartBattle("gate_guard");
                });
            }
            else if (roll < 25)
            {
                SetBackground("piyorin");
                SetPortrait("piyorin_swarm");
                titleText.text = "ランダムイベント: 黄色い行列";
                areaText.text = "突発戦闘";
                bodyText.text = "曲がり角の向こうから、柔らかい足音が増えてくる。\n\nかわいい。かわいいが、今回は列を作っている。";
                AddChoiceButton("突破する", () =>
                {
                    run.battleReturnScene = run.pendingSceneAfterRandom;
                    StartBattle("piyorin");
                });
                AddChoiceButton("脇道へ逃げる", () =>
                {
                    run.stats.speed += 1;
                    run.stats.hp = Math.Max(1, run.stats.hp - 2);
                    ShowScene(run.pendingSceneAfterRandom, false);
                });
            }
            else if (roll < 45)
            {
                SetBackground("osu");
                SetPortrait("event_occult_researcher");
                titleText.text = "ランダムイベント: 怪しい自販機";
                areaText.text = "小報酬";
                bodyText.text = "路地の奥に、見たことのない自販機がある。\n\n商品名はすべて文字化けしているが、硬貨投入口だけは妙に現実的だった。";
                AddChoiceButton("300円入れる", () =>
                {
                    var before = run.stats.Clone();
                    string note;
                    if (run.stats.money >= 300)
                    {
                        run.stats.money -= 300;
                        int gain = rng.Next(0, 3);
                        if (gain == 0) { run.stats.attack += 1; note = "缶の底から、錆びた勇気が喉へ落ちた。"; }
                        else if (gain == 1) { run.stats.defense += 1; note = "冷たい炭酸が、骨の内側を少し硬くした。"; }
                        else { run.stats.sanity = Math.Min(run.stats.maxSanity, run.stats.sanity + 3); note = "甘すぎる液体が、現実の輪郭を少し戻した。"; }
                    }
                    else
                    {
                        note = "返却口から、入れていないはずの硬貨が一枚だけ鳴った。所持金が足りない。";
                    }
                    string delta = BuildStatDelta(before, run.stats);
                    LogRun("怪しい自販機: " + delta);
                    ShowChoiceOutcome(run.pendingSceneAfterRandom, "300円入れる\n" + note + "\n結果: " + delta);
                });
                AddChoiceButton("叩いてみる", () =>
                {
                    var before = run.stats.Clone();
                    run.stats.hp = Math.Max(1, run.stats.hp - 1);
                    progress.memoryFragments += 2;
                    SaveProgress();
                    string delta = BuildStatDelta(before, run.stats) + " / 記憶片+2";
                    LogRun("怪しい自販機: " + delta);
                    ShowChoiceOutcome(run.pendingSceneAfterRandom, "叩いてみる\n自販機の奥で、未来の領収書が破れる音がした。\n結果: " + delta);
                });
            }
            else if (roll < 63)
            {
                SetBackground("castle");
                SetPortrait("event_shachi_avatar");
                titleText.text = "ランダムイベント: 鯱の影";
                areaText.text = "神話汚染";
                bodyText.text = "夜空を巨大な影が横切った。\n\n雨が下から上へ降り、あなたの影だけが一歩遅れてついてくる。";
                AddChoiceButton("影を見ない", () =>
                {
                    run.stats.sanity = Math.Min(run.stats.maxSanity, run.stats.sanity + 1);
                    ShowScene(run.pendingSceneAfterRandom, false);
                });
                AddChoiceButton("見返す", () =>
                {
                    run.shachiGaze += 1;
                    run.stats.mythosCorruption += 1;
                    run.stats.attack += 1;
                    ShowScene(run.pendingSceneAfterRandom, false);
                });
            }
            else if (roll < 82)
            {
                SetBackground("toyota");
                SetPortrait("event_factory_inspector");
                titleText.text = "ランダムイベント: 工程表の囁き";
                areaText.text = "機械";
                bodyText.text = "壁に貼られた工程表が、あなたの今日の死に方まで管理しようとしている。\n\nしかし余白には、抜け道も小さく印刷されていた。";
                AddChoiceButton("抜け道を読む", () =>
                {
                    run.stats.machineAptitude += 1;
                    run.stats.sanity = Math.Max(0, run.stats.sanity - 1);
                    ShowScene(run.pendingSceneAfterRandom, false);
                });
                AddChoiceButton("工程表を破る", () =>
                {
                    run.stats.attack += 1;
                    run.stats.hp = Math.Max(1, run.stats.hp - 2);
                    ShowScene(run.pendingSceneAfterRandom, false);
                });
            }
            else
            {
                SetBackground("ending");
                SetPortrait("event_occult_researcher");
                titleText.text = "ランダムイベント: 前回のあなた";
                areaText.text = "周回記憶";
                bodyText.text = "路肩に、あなたと同じ顔の人物が座っている。\n\nその人は何も言わず、掌の中の記憶片を差し出した。";
                AddChoiceButton("受け取る", () =>
                {
                    var before = run.stats.Clone();
                    progress.memoryFragments += 4;
                    run.stats.mythosKnowledge += 1;
                    SaveProgress();
                    string delta = BuildStatDelta(before, run.stats) + " / 記憶片+4";
                    LogRun("前回のあなた: " + delta);
                    ShowChoiceOutcome(run.pendingSceneAfterRandom, "受け取る\n掌の記憶片は温かかった。あなたが忘れた痛みだけが、まだ生きている。\n結果: " + delta);
                });
                AddChoiceButton("埋葬する", () =>
                {
                    var before = run.stats.Clone();
                    run.stats.sanity = Math.Min(run.stats.maxSanity, run.stats.sanity + 3);
                    progress.memoryFragments += 1;
                    SaveProgress();
                    string delta = BuildStatDelta(before, run.stats) + " / 記憶片+1";
                    LogRun("前回のあなた: " + delta);
                    ShowChoiceOutcome(run.pendingSceneAfterRandom, "埋葬する\n土をかけるたびに、同じ顔の誰かが少しだけ眠れる顔になった。\n結果: " + delta);
                });
            }
            UpdateSideText();
            footerText.text = "ランダムイベント発生。周回ごとに割り込み内容が変化します。";
        }
        void ShowExpandedRandomEvent(string targetSceneId)
        {
            var events = ExpandedRandomEvents();
            var e = events[rng.Next(events.Count)];
            SetBackground(e.image);
            SetPortrait(e.portrait);
            titleText.text = e.title;
            areaText.text = e.area;
            int preview = PreviewEventRoll(e);
            string checkName = EventCheckName(e);
            bodyText.text = e.text + "\n\n判定目標: " + e.difficulty + " / 予兆: " + DiceForecast(preview, e.difficulty) +
                "\n" + checkName + ": 6面ダイス3個 + " + EventCheckBonusLabel(e);
            ClearChoices();
            AddChoiceButton(e.successLabel + "\n" + checkName, () =>
            {
                bool animateEventDice = true;
                if (animateEventDice)
                {
                    StartCoroutine(ResolveExpandedRandomEventWithDice(e));
                    return;
                }
                int roll = RollLuckDiceAgainst(e.difficulty);
                bool success = roll >= e.difficulty;
                if (success)
                {
                    var before = run.stats.Clone();
                    e.success?.Invoke(run);
                    string delta = BuildStatDelta(before, run.stats);
                    LogRun(e.title + ": 成功 " + delta);
                    bodyText.text = e.successText + "\n\n" + run.lastRollSummary + "\n結果: " + delta;
                    Play(rewardSfx, 0.65f);
                   UpdateSideText();
                    ClearChoices();
                    AddChoiceButton("次に進む", () => ShowScene(run.pendingSceneAfterRandom, false));
                }
                else
                {
                    var before = run.stats.Clone();
                    e.fail?.Invoke(run);
                    string delta = BuildStatDelta(before, run.stats);
                    LogRun(e.title + ": 失敗 " + delta);
                    bodyText.text = e.failText + "\n\n" + run.lastRollSummary + "\n結果: " + delta;
                    Play(doomSfx, 0.45f);
                    UpdateSideText();
                    if (!string.IsNullOrEmpty(e.failBattle))
                    {
                        run.battleReturnScene = run.pendingSceneAfterRandom;
                        StartBattle(e.failBattle);
                    }
                    else if (run.stats.hp <= 0)
                    {
                        ShowEnding("event_death");
                    }
                    else if (run.stats.sanity <= 0)
                    {
                        QueueSanityCollapse(run.pendingSceneAfterRandom);
                    }
                    else
                    {
                        ClearChoices();
                        AddChoiceButton("次に進む", () => ShowScene(run.pendingSceneAfterRandom, false));
                    }
                }
            });
            AddChoiceButton(e.failLabel + "\n安全策", () =>
            {
                if (run.stats.money >= 180)
                {
                    run.stats.money -= 180;
                    LogRun(e.title + ": 安全策 所持金-180");
                    bodyText.text = "金で安全な抜け道を買った。\n\n結果: 所持金-180";
                }
               else
                {
                    var before = run.stats.Clone();
                    run.stats.hp = Math.Max(1, run.stats.hp - 3);
                    run.stats.sanity = Math.Max(0, run.stats.sanity - 1);
                    string delta = BuildStatDelta(before, run.stats);
                    LogRun(e.title + ": 安全策 " + delta);
                    bodyText.text = "払える金が足りず、身体で代償を払った。\n\n結果: " + delta;
                }
                UpdateSideText();
                ClearChoices();
                AddChoiceButton("次に進む", () => ShowScene(run.pendingSceneAfterRandom, false));
            });
            footerText.text = checkName + "は3D6+" + EventCheckBonusLabel(e) + "。安全策は所持金180、足りない時はHPで支払います。";
        }
        IEnumerator ResolveExpandedRandomEventWithDice(RandomEventDef e)
        {
            ClearChoices();
            int a = rng.Next(1, 7);
            int b = rng.Next(1, 7);
            int c = rng.Next(1, 7);
            int checkBonus = EventCheckBonus(e);
            if (UsesLuckCheck(e) && rng.NextDouble() < Mathf.Clamp01(run.stats.luck * 0.055f))
                c = 6;
            for (int i = 0; i < 8; i++)
            {
                int ra = rng.Next(1, 7);
                int rb = rng.Next(1, 7);
                int rc = rng.Next(1, 7);
                bodyText.text = e.text + "\n\nサイコロが転がっている...\n[" + ra + "] [" + rb + "] [" + rc + "]  " + EventCheckBonusLabel(e);
                Play(clickSfx, 0.2f);
                yield return new WaitForSeconds(0.08f + i * 0.018f);
            }
            int roll = a + b + c + checkBonus;
            bool success = roll >= e.difficulty;
            run.lastRollSummary = EventRollDetail(e, a, b, c, checkBonus, roll, e.difficulty);
            if (success)
            {
                var before = run.stats.Clone();
                e.success?.Invoke(run);
                string delta = BuildStatDelta(before, run.stats);
               LogRun(e.title + ": 成功 " + delta);
                bodyText.text = e.successText + "\n\n" + run.lastRollSummary + "\n結果: " + delta;
                Play(rewardSfx, 0.65f);
                UpdateSideText();
                ClearChoices();
                AddChoiceButton("次に進む", () => ShowScene(run.pendingSceneAfterRandom, false));
            }
            else
            {
                var before = run.stats.Clone();
                e.fail?.Invoke(run);
                string delta = BuildStatDelta(before, run.stats);
                LogRun(e.title + ": 失敗 " + delta);
                bodyText.text = e.failText + "\n\n" + run.lastRollSummary + "\n結果: " + delta;
                Play(doomSfx, 0.45f);
                UpdateSideText();
                if (!string.IsNullOrEmpty(e.failBattle))
                {
                    run.battleReturnScene = run.pendingSceneAfterRandom;
                    StartBattle(e.failBattle);
                }
                else if (run.stats.hp <= 0)
                {
                    ShowEnding("event_death");
                }
                else if (run.stats.sanity <= 0)
                {
                    QueueSanityCollapse(run.pendingSceneAfterRandom);
                }
                else
                {
                    ClearChoices();
                    AddChoiceButton("次に進む", () => ShowScene(run.pendingSceneAfterRandom, false));
                }
            }
        }
        int RollLuckDice()
        {
            int a = rng.Next(1, 7);
            int b = rng.Next(1, 7);
          int c = rng.Next(1, 7);
            int luckBonus = Mathf.Clamp(run.stats.luck / 2, 0, 6);
            if (rng.NextDouble() < Mathf.Clamp01(run.stats.luck * 0.055f))
                c = 6;
            int total = a + b + c + luckBonus;
            if (run != null)
                run.lastRollSummary = "LUK判定: " + a + "/" + b + "/" + c + " + LUK" + luckBonus + " = " + total;
            return total;
        }
        int RollLuckDiceAgainst(int target)
        {
            int total = RollLuckDice();
            if (run != null)
                run.lastRollSummary += " / 目標 " + target + "\n" + LuckMarginText(total, target);
            return total;
        }
        string LuckRollDetail(int a, int b, int c, int luckBonus, int total, int target)
        {
            return "LUK判定: " + a + "/" + b + "/" + c + " + LUK" + luckBonus + " = " + total + " / 目標 " + target + "\n" + LuckMarginText(total, target);
        }
        string LuckMarginText(int total, int target)
        {
            int margin = total - target;
            if (margin >= 0)
                return "成功: 目標を " + margin + " 上回った";
            return "失敗: あと " + (-margin) + " 足りない";
        }
        bool UsesLuckCheck(RandomEventDef e)
        {
            return e == null || (!e.id.StartsWith("san_") && !e.id.StartsWith("mythos_"));
        }
        string EventCheckName(RandomEventDef e)
        {
            if (e != null && e.id.StartsWith("san_"))
                return "SAN判定";
            if (e != null && e.id.StartsWith("mythos_"))
                return "神話判定";
            return "LUK判定";
        }
        int EventCheckBonus(RandomEventDef e)
        {
            if (e != null && e.id.StartsWith("san_"))
                return Mathf.Clamp(run.stats.sanity / 3, 0, 8);
            if (e != null && e.id.StartsWith("mythos_"))
                return Mathf.Clamp(run.stats.mythosKnowledge, 0, 10);
            return Mathf.Clamp(run.stats.luck / 2, 0, 6);
        }
        string EventCheckBonusLabel(RandomEventDef e)
        {
            if (e != null && e.id.StartsWith("san_"))
                return "SAN補正 +" + EventCheckBonus(e) + "(SAN/3、最大+8)";
            if (e != null && e.id.StartsWith("mythos_"))
                return "神話理解補正 +" + EventCheckBonus(e) + "(最大+10)";
            return "LUK補正 +" + EventCheckBonus(e) + "(LUK/2、最大+6)";
        }
        int PreviewEventRoll(RandomEventDef e)
        {
            return 10 + EventCheckBonus(e);
        }
        string EventRollDetail(RandomEventDef e, int a, int b, int c, int bonus, int total, int target)
        {
            return EventCheckName(e) + ": " + a + "/" + b + "/" + c + " + " + bonus + " = " + total + " / 目標 " + target + "\n" + LuckMarginText(total, target);
        }
        int PreviewLuckRoll(int difficulty)
        {
            return 10 + Mathf.Clamp(run.stats.luck / 2, 0, 6) - Mathf.Max(0, difficulty - 14) / 3;
        }
        string DiceForecast(int preview, int difficulty)
        {
            if (preview >= difficulty + 3) return "かなり良い";
            if (preview >= difficulty) return "五分以上";
            if (preview + 3 >= difficulty) return "危険";
            return "非常に危険";
        }
        List<RandomEventDef> ExpandedRandomEvents()
        {
            var list = new List<RandomEventDef>();
           AddEvent(list, "mirror_platform", "鏡張りのホーム", "名駅地下 / 鏡界", "station", "event_subway_child", "柱すべてに半拍遅れて動くあなたが映る。子供の声が『本物を選んで』と笑った。", "目を閉じて足音を数える", "壁沿いに逃げる", 13, r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); r.stats.localKnowledge += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); }, "鏡の列車を見送った。地図の読み方が少しわかる。", "鏡の中のあなたが一人だけ残った。SANが削られる。", "");
            AddEvent(list, "atsuta_oath", "熱田の封じ縄", "熱田 / 旧神封印", "atsuta", "event_atsuta_miko", "焦げた注連縄が宙に浮き、外なるものの名を一拍だけ縛っている。", "縄の結び目を読み替える", "手を合わせて退く", 14, r => { r.stats.mythosKnowledge += 1; progress.memoryFragments += 2; SaveProgress(); }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); r.stats.mythosCorruption += 1; }, "封印の綻びから記憶片がこぼれた。", "結び目が指に食い込み、神話汚染が増す。", "");
            AddEvent(list, "library_index", "鶴舞地下書庫", "鶴舞 / 禁書目録", "tsuruma", "event_tsuruma_librarian", "司書は顔のない索引カードを差し出す。分類番号はあなたの死因だった。", "カードを逆順に読む", "本を閉じる", 15, r => { r.stats.mythosKnowledge += 2; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 4); }, "禁書の目次だけを盗み読んだ。", "索引があなたの名前を正しく発音した。", "index_hound");
            AddEvent(list, "sakae_contract", "栄の黒い契約屋", "栄 / 契約", "sakae", "event_sakae_broker", "ネオンの影に、交換条件だけが立っている。『一時間の記憶と、今すぐの力を』", "条件を値切る", "契約しない", 12, r => { r.stats.attack += 1; r.stats.money += 250; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); progress.memoryFragments = Math.Max(0, progress.memoryFragments - 1); SaveProgress(); }, "契約の余白を突いた。力と金が残る。", "署名欄に過去の筆跡が増えた。", "");
            AddEvent(list, "seto_kiln", "瀬戸の白い窯", "瀬戸 / 焼成異界", "seto", "event_seto_potter", "窯の中から、まだ焼かれていない未来の骨が鳴る。", "窯の温度を読む", "灰を払って離れる", 13, r => { r.stats.defense += 1; r.stats.machineAptitude += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 4); }, "陶片が防具のように肌へ馴染んだ。", "白い熱が肺に入り、HPを失う。", "kiln_crawler");
            AddEvent(list, "inuyama_mask", "犬山の能面市", "犬山 / 面", "inuyama", "event_inuyama_mask", "古い面が軒先からこちらを見ている。笑っている面だけ、内側が濡れている。", "笑っていない面を選ぶ", "目を伏せて通る", 14, r => { r.stats.luck += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, r => { r.stats.mythosCorruption += 1; }, "面はあなたを通行人として認めた。LUK+1。", "面の裏にある顔が、一瞬あなたになった。", "");
            AddEvent(list, "toyohashi_signal", "豊橋の終電信号", "豊橋 / 終電", "toyohashi", "event_toyohashi_conductor", "無人の車掌が、到着しないはずの終電を待っている。", "発車ベルの拍を外す", "改札の外へ戻る", 13, r => { r.stats.speed += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 2); r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "ベルの隙間を抜け、速さを得た。", "終電の風が身体を少し持っていった。", "last_train");
            AddEvent(list, "gamagori_tide", "蒲郡の逆潮", "蒲郡 / 海底星図", "gamagori", "event_gamagori_diver", "海が空へ落ち、底に星図が露出する。潜水服の人物が手招きした。", "星図を三角測量する", "砂を握って戻る", 16, r => { r.stats.mythosKnowledge += 1; progress.memoryFragments += 3; SaveProgress(); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); r.shachiGaze += 1; }, "海底星図から記憶片を拾った。", "逆潮があなたの影だけを連れていった。", "deep_one_clerk");
            AddEvent(list, "korankei_red", "香嵐渓の赤すぎる葉", "香嵐渓 / 紅葉迷宮", "korankei", "event_korankei_pilgrim", "葉が落ちるたびに、誰かの後悔が一つ増える。赤が濃すぎる。", "赤くない葉を探す", "急いで抜ける", 12, r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); }, r => { progress.regretLog.Add("香嵐渓の赤い葉"); SaveProgress(); r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "まだ緑の葉が、正気を繋ぎ止めた。", "後悔が一枚、日報に増えた。", "");
            AddEvent(list, "handa_brew", "半田の黒酢槽", "半田 / 発酵", "handa", "event_handa_brewer", "発酵槽の表面に、未来の新聞見出しが泡で浮かぶ。", "泡の順番を読む", "蓋を閉める", 13, r => { r.stats.misoResistance += 1; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 2); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "発酵のリズムを掴んだ。味噌耐性+1。", "泡があなたの死亡記事になった。", "");
            AddEvent(list, "arimatsu_thread", "有松の縛り糸", "有松 / 絞り染め", "arimatsu", "event_arimatsu_weaver", "紺の布に白く抜かれた模様が、探索ルートの分岐そのものに見える。", "結び目を一つほどく", "布を買って退く", 15, r => { r.dangerWarnings += 1; r.stats.luck += 1; }, r => { r.stats.money = Math.Max(0, r.stats.money - 180); r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, "糸が未来の危険を一つ教えた。", "模様の一部が皮膚に移った。", "");
            AddEvent(list, "nagakute_battlefield", "長久手の無音陣", "長久手 / 古戦場", "nagakute", "event_battlefield_monk", "旗だけが風に鳴り、人の声が一切しない。陣形は巨大な魔法円だった。", "陣の欠けを踏む", "旗を避けて進む", 14, r => { r.stats.attack += 1; r.owari += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); }, "陣の力を逆流させた。攻撃+1。", "無音の槍が腹をかすめた。", "battlefield_spear");
            AddEvent(list, "kariya_gear", "刈谷の歯車神棚", "刈谷 / 機械祭具", "kariya", "event_factory_inspector", "神棚に納められた歯車が、手を合わせる速度に合わせて回る。", "正しい回転数で拝む", "電源を切る", 13, r => { r.stats.machineAptitude += 2; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 2); r.stats.machineAptitude += 1; }, "機械の祝詞を覚えた。", "火花を浴びたが、仕組みは少しわかった。", "");
            AddEvent(list, "nishio_tea", "西尾の暗い茶室", "西尾 / 抹茶夢", "nishio", "event_tea_medium", "茶碗の底に、巨大な眼が沈んでいる。見なければ飲める。", "眼を見ずに飲む", "茶室を出る", 12, r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 3); r.stats.mythosCorruption += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "苦味が正気を戻し、別の何かを残した。", "襖の向こうから茶筅の音が追ってくる。", "");
            AddEvent(list, "ichinomiya_thread", "一宮の繊維迷路", "一宮 / 織物", "ichinomiya", "event_arimatsu_weaver", "道路そのものが織機になり、歩幅を一本ずつ編み込んでいく。", "縦糸だけを踏む", "横道へ逃げる", 13, r => { r.stats.speed += 1; r.stats.luck += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 2); }, "足取りが軽くなった。速さ+1 / LUK+1。", "足首に糸が残った。", "");
            AddEvent(list, "chiryu_puppet", "知立の人形芝居", "知立 / 人形", "chiryu", "event_chiryu_puppeteer", "人形遣いの手は見えない。だが糸は、あなたの肩にも伸びている。", "自分の糸を切る", "拍手して終える", 15, r => { r.stats.attack += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); }, "糸が切れ、身体が少し自由になった。", "拍手の音があなたの手からではなかった。", "puppet_thing");
            AddEvent(list, "laguna_stage", "ラグーナの無人ショー", "蒲郡 / 水上劇場", "laguna", "event_laguna_actor", "誰もいない客席に拍手が満ち、舞台上の怪物が出番を待つ。", "台本を即興で変える", "幕が下りるまで待つ", 14, r => { progress.memoryFragments += 2; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); SaveProgress(); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "物語の役を奪い、記憶片を得た。", "拍手が一つ、頭の中に残った。", "stage_polyps");
            AddEvent(list, "meieki_coinlocker", "名駅コインロッカー胎内", "名駅 / ロッカー", "station", "event_locker_keeper", "ロッカーの扉が内側から叩かれる。番号は前回死んだ順番で並んでいる。", "正しい番号を飛ばす", "鍵を捨てる", 14, r => { progress.memoryFragments += 3; SaveProgress(); }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); }, "開けなかった扉から、記憶片だけが落ちた。", "扉の内側が、少しだけあなたを覚えた。", "locker_womb");
            AddEvent(list, "osu_radio", "大須の異界ラジオ", "大須 / 電波", "osu", "event_occult_researcher", "古いラジオが、まだ起きていないボス戦の実況を流している。", "周波数を半目盛ずらす", "電池を抜く", 12, r => { r.stats.mythosKnowledge += 1; r.dangerWarnings += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "未来の敵行動を少し聞いた。", "実況者があなたの呼吸を実況した。", "");
            AddEvent(list, "castle_well", "名古屋城の底なし井戸", "名古屋城 / 井戸", "castle", "event_shachi_avatar", "井戸の底から海鳴りがする。ここは城の中なのに、潮の匂いが強い。", "桶を途中で止める", "井戸を覗く", 16, r => { r.shachiGaze += 1; progress.memoryFragments += 2; SaveProgress(); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 4); }, "桶の中に金の鱗が一枚あった。", "底から巨大な瞳がこちらを見た。", "well_tentacle");
            AddEvent(list, "okazaki_stamp", "岡崎の家康印章", "岡崎 / 印章", "okazaki", "event_miso_voice", "封蝋に押された印が、徳ではなく『渡るな』と読める。", "印を逆さに押す", "封蝋を削る", 13, r => { r.mikawa += 1; r.stats.defense += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "印が守り札として固まった。", "封蝋の下から別の王印が出た。", "");
            AddEvent(list, "airport_baggage", "空港の帰らない荷物", "空港 / 手荷物", "airport", "event_gate_inspector", "ターンテーブルを回る荷物は、すべてあなたの持ち物だった。まだ手に入れていない物もある。", "自分のものだけ拾う", "触らずに離れる", 15, r => { r.stats.money += 350; r.stats.luck += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 2); progress.brokenGear.Add("空港で噛まれた荷物"); SaveProgress(); }, "未来の所持金と運を少し前借りした。", "荷物の口が開き、装備記録に傷が残る。", "baggage_mouth");
            AddEvent(list, "miso_oracle", "味噌蔵の三度目の声", "岡崎 / 神託", "miso", "event_miso_voice", "一度目は警告。二度目は嘘。三度目だけが、あなたの声で話す。", "三度目だけ聞く", "耳を塞ぐ", 17, r => { r.stats.mythosKnowledge += 2; r.stats.misoResistance += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 5); }, "声の意味を掴んだ。強いが危うい知識だ。", "三度目の声が、今後も内側に残る。", "miso_voice");
            AddEvent(list, "toyota_quality", "豊田の品質検査室", "豊田 / 検査", "toyota", "event_factory_inspector", "検査票にはHP、SAN、攻撃、逃走率が赤字で並ぶ。不良品欄にあなたの名前がある。", "検査項目を書き換える", "不良品棚を出る", 14, r => { r.stats.machineAptitude += 1; r.stats.attack += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); }, "検査基準を味方につけた。", "赤ペンが皮膚の下を走った。", "quality_golem");
            AddEvent(list, "tokoname_cat", "常滑の百目猫", "常滑 / 猫", "tokoname", "event_shachi_avatar", "焼き物の猫が百の目で、あなたのサイコロを見つめている。", "一つだけ目を閉じさせる", "礼だけして通る", 12, r => { r.stats.luck += 2; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, r => { r.stats.luck = Math.Max(0, r.stats.luck - 1); }, "目が一つ閉じ、出目が軽くなった。", "猫は出目を一つ持っていった。", "");
            AddEvent(list, "shinkansen_dream", "新幹線ホームの夢喰い", "名駅 / 速度夢", "station", "event_subway_child", "通過列車の窓すべてに、眠っている探索者たちが映る。", "起きている顔を探す", "ベンチの下へ伏せる", 15, r => { r.stats.speed += 1; progress.memoryFragments += 1; SaveProgress(); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); }, "眠りを避けて速度を得た。", "夢の一部を食われた。", "dream_eater");
            AddEvent(list, "atsuta_sword", "熱田の抜けない剣", "熱田 / 草薙影", "atsuta", "event_atsuta_miko", "剣は台座に刺さっていない。世界そのものに刺さっている。", "柄に触れて離す", "抜こうとする", 18, r => { r.stats.attack += 2; r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 5); r.stats.mythosCorruption += 1; }, "刃の影だけを借りた。攻撃+2。", "世界が少し裂け、あなたも裂けた。", "sword_shadow");
            AddEvent(list, "tsushima_lantern", "津島の流れない灯籠", "津島 / 川祭", "tsushima", "event_tea_medium", "川面に灯籠が止まっている。火は水中で燃え、名前を読むと誰かが消える。", "名前を読まずに数える", "一つ拾う", 13, r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); progress.memoryFragments += 1; SaveProgress(); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); }, "数だけを覚え、火を持ち帰らなかった。", "灯籠の火が記憶を焦がした。", "lantern_dead");
            AddEvent(list, "mikawa_shadow", "三河の影武者", "三河 / 影", "okazaki", "event_battlefield_monk", "あなたより一歩強い影武者が、同じ装備で立っている。", "影より遅く構える", "先に斬る", 16, r => { r.stats.defense += 1; r.stats.attack += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 4); }, "影の拍子を奪った。攻防+1。", "影のほうが少し早かった。", "shadow_retainer");
            AddEvent(list, "centrair_window", "セントレアの窓外神", "空港 / 窓", "airport", "event_gate_inspector", "搭乗待合の窓外に、滑走路より大きなものが浮いている。職員は誰も見ていない。", "見ていないふりをする", "写真を撮る", 17, r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); r.dangerWarnings += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 4); r.stats.mythosKnowledge += 1; }, "見ない技術を覚えた。", "写真にはあなたの背後しか写っていない。", "window_god");
            AddEvent(list, "sakae_luck_parlor", "栄の出目パーラー", "栄 / 運試し", "sakae", "event_sakae_broker", "古い遊技台が、硬貨ではなく未来のHPを飲み込んで光る。店員は『運があるなら増やせます』とだけ言う。", "500円を賭ける", "台から離れる", 16, r => { int stake = Math.Min(500, r.stats.money); r.stats.money -= stake; r.stats.money += 900; r.stats.luck += 1; }, r => { r.stats.money = Math.Max(0, r.stats.money - 500); r.stats.hp = Math.Max(1, r.stats.hp - 3); }, "台は七を揃えた。所持金とLUKが増えた。", "台はHPを先に徴収した。金も少し消えた。", "");
            AddEvent(list, "meieki_night_clinic", "名駅地下の夜間診療所", "名駅 / 診療所", "station", "event_locker_keeper", "診療所の窓口には料金表だけがある。HP回復、正気補修、幸運注射。すべて現金前払いだ。", "治療費を値切る", "保険なしで逃げる", 14, r => { int fee = r.stats.money >= 420 ? 420 : r.stats.money; r.stats.money -= fee; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 7); r.stats.luck += fee >= 420 ? 1 : 0; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 4); r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, "治療は雑だが効いた。HPが戻り、満額ならLUKも上がる。", "請求書が傷口から出てきた。", "");
            AddEvent(list, "osu_lucky_auction", "大須の幸運オークション", "大須 / 路地市", "osu", "event_occult_researcher", "競り台に並ぶのは装備ではなく、明日の偶然だ。値札は所持金、落札条件はLUK。", "競り勝つ", "冷やかして去る", 15, r => { r.stats.money = Math.Max(0, r.stats.money - 300); r.stats.luck += 2; progress.memoryFragments += 1; SaveProgress(); }, r => { r.stats.money = Math.Max(0, r.stats.money - 180); r.stats.luck = Math.Max(0, r.stats.luck - 1); }, "偶然を一束買った。LUK+2、記憶片+1。", "冷やかし代を取られ、運まで少し削られた。", "");
            AddEvent(list, "handa_blood_coupon", "半田の赤い回数券", "半田 / 交通", "handa", "event_handa_brewer", "回数券は赤く湿っている。使えば近道になるが、改札は残りHPを数えている。", "HPで改札を通る", "普通運賃を払う", 13, r => { r.stats.hp = Math.Max(1, r.stats.hp - 4); r.stats.money += 450; r.dangerWarnings += 1; }, r => { r.stats.money = Math.Max(0, r.stats.money - 260); r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, "血の改札を抜けた。HPは減ったが金と危険察知を得た。", "普通運賃なのに、領収書が悲鳴を上げた。", "");
            AddEvent(list, "tokoname_luck_kiln", "常滑の運焼き窯", "常滑 / 窯", "tokoname", "event_shachi_avatar", "窯の中でサイコロが焼かれている。高温ほどよい出目になるが、近づくほどHPが削れる。", "高温で焼く", "低温で済ませる", 17, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); r.stats.luck += 3; }, r => { r.stats.luck += 1; r.stats.money = Math.Max(0, r.stats.money - 160); }, "指先を焦がしたが、出目は軽くなった。LUK+3。", "低温の出目は安いが、窯代は取られた。", "");
            AddEvent(list, "owari_thread_shrine", "尾張の緯糸神社", "一宮 / 織物神域", "owari_shrine", "event_arimatsu_weaver", "夜の参道に張られた糸が、鳥居から鳥居へ星図のように伸びている。結び目の一つ一つが、別の周回で死んだ探索者の名前だった。", "死者の結び目をほどく", "糸を避けて参道を抜ける", 15, r => { r.stats.localKnowledge += 2; r.dangerWarnings += 1; progress.memoryFragments += 1; SaveProgress(); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); r.stats.speed = Math.Max(0, r.stats.speed - 1); }, "ほどけた糸が、尾張の道筋を一つ教えた。地元知識+2、危険察知+1。", "糸は足首に残り、歩幅を一つ奪った。", "");
            AddEvent(list, "okumikawa_horaiji_steps", "鳳来寺山の逆さ石段", "奥三河 / 山岳神域", "okumikawa_horaiji", "event_korankei_pilgrim", "鳳来寺山へ続く石段が、登るほど下へ沈んでいく。杉の隙間には空ではなく、星のない黒い水面が見える。", "段数を数え直す", "息を止めて駆け上がる", 16, r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); r.stats.defense += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 4); r.stats.mythosKnowledge += 1; }, "正しい段数だけが足元に残った。SAN+2、防御+1。", "山は少しだけあなたの肺を覚えた。HPが削れ、神話理解が増す。", "shadow_retainer");
            AddEvent(list, "toyokawa_inari_red_void", "豊川稲荷の赤い曲がり廊", "豊川 / 稲荷異界", "toyokawa_inari", "event_inuyama_mask", "赤い幟の廊下で、狐像の影だけがこちらを向く。賽銭箱の底から、硬貨ではなく小さな星が落ちる音がした。", "狐像と同じ向きで礼をする", "賽銭箱を覗く", 14, r => { r.stats.luck += 2; r.stats.money = Math.Max(0, r.stats.money - 120); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); r.stats.mythosCorruption += 1; }, "礼の角度が合い、運だけが通行を許された。LUK+2。", "箱の底は空ではなく、こちらを覗き返す穴だった。", "");
            AddEvent(list, "chita_black_tide", "知多の黒い潮だまり", "知多半島 / 海蝕蔵", "chita_coast", "event_handa_brewer", "海沿いの古い蔵の前で、潮だまりが夜空を映さず、別の海底を映している。酢の匂いと潮の匂いが混ざり、呼吸の順番がわからなくなる。", "潮だまりに塩を撒く", "蔵の戸を閉める", 15, r => { r.stats.misoResistance += 1; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); r.shachiGaze += 1; }, "黒い水面が曇り、発酵した塩気が身を守った。味噌耐性+1。", "戸の向こうで海鳴りが近づき、鯱の視線が増した。", "deep_one_clerk");
            AddEvent(list, "komaki_airfield_orbit", "小牧の円軌道滑走路", "小牧 / 夜間飛行場", "komaki_airfield", "event_gate_inspector", "誘導灯が滑走路ではなく円を描いている。空自の格納庫の影から、離陸していない機体のエンジン音だけが戻ってくる。", "誘導灯の欠けを読む", "走って円の外へ出る", 16, r => { r.stats.speed += 1; r.dangerWarnings += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); r.stats.hp = Math.Max(1, r.stats.hp - 2); }, "円の切れ目を見つけた。速度+1、危険察知+1。", "滑走路は一周ぶん長くなり、膝と正気を削った。", "");
            AddEvent(list, "toyota_line_nonhuman", "豊田の人でない組立線", "豊田 / 自動化工廠", "toyota", "event_factory_inspector", "無人の組立線が、車ではなく探索者の選択肢を組み立てている。不良品箱には『人間らしさ』と印字された部品が積まれていた。", "検査規格を逆用する", "非常停止を押す", 16, r => { r.stats.machineAptitude += 2; r.stats.attack += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); r.stats.machineAptitude += 1; }, "規格外の力をこちらの装備に組み込んだ。機械適性+2、攻撃+1。", "停止ボタンは効いたが、腕の中に工程表が残った。", "quality_golem");
            AddEvent(list, "seto_moon_ceramic", "瀬戸の月白い陶片", "瀬戸 / 陶祖窯跡", "seto", "event_seto_potter", "窯跡に散った陶片が、月の満ち欠けと違う形で光る。裏返すと、釉薬の下に小さな海の化石が閉じ込められていた。", "欠けた月だけ拾う", "全部砕いて進む", 15, r => { r.stats.defense += 1; r.stats.mythosKnowledge += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, "陶片は薄い盾になり、古い海の知識も残した。", "砕いた音が窯の底へ落ち、破片が肌に刺さった。", "kiln_crawler");
            AddEvent(list, "toyohashi_black_tram", "豊橋の黒い路面電車", "豊橋 / 市電終点", "toyohashi", "event_toyohashi_conductor", "終点に着いたはずの路面電車が、線路のない暗がりへまだ進もうとしている。運転席には運転士ではなく、濡れた時刻表が座っていた。", "時刻表を一分遅らせる", "飛び降りる", 14, r => { r.stats.speed += 1; progress.memoryFragments += 1; SaveProgress(); }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 2); r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "一分の遅れが、こちらの速度になった。", "線路のない揺れが身体に残った。", "last_train");
            AddEvent(list, "tsushima_river_king", "津島天王川の沈む山車", "津島 / 川祭深部", "tsushima", "event_tea_medium", "川面に浮くはずの山車が、水中をゆっくり進んでいる。提灯の火は消えず、火袋の中で小さな深海が揺れていた。", "火袋の数だけ息を止める", "山車を岸へ引く", 15, r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); progress.memoryFragments += 1; SaveProgress(); }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); r.stats.mythosCorruption += 1; }, "息を止めた数だけ、川はあなたを見逃した。", "綱の向こうに川底の祭りが絡みついた。", "lantern_dead");
            AddEvent(list, "nishio_green_eye", "西尾抹茶の緑眼", "西尾 / 茶畑夢層", "nishio", "event_tea_medium", "茶畑の畝が巨大な指紋のように湾曲し、茶碗の底には泡ではなく緑の眼が沈む。見ないで飲めば効く。見れば、向こうも覚える。", "眼を見ずに点てる", "泡の形を読む", 14, r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 3); }, r => { r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 3); }, "苦味だけが正気へ戻った。SAN+3。", "泡は星図だった。読めたが、向こうにも読まれた。", "tea_eye");
            AddEvent(list, "san_atsuta_quiet_prayer", "熱田の正気祓い", "熱田 / SAN専用", "atsuta", "event_atsuta_miko", "熱田の社の奥で、鈴の音だけがあなたの正気度を数えている。ここでは知識よりも、まだ人間として残っている沈黙が試される。", "SANを整えて祓いを受ける", "無理に鈴を鳴らす", 12, r => { int gain = r.stats.sanity <= r.stats.maxSanity / 2 ? 5 : 2; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + gain); r.stats.mythosCorruption = Math.Max(0, r.stats.mythosCorruption - 1); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 4); r.stats.mythosCorruption += 1; }, "鈴は一度だけ正しく鳴った。SANが戻り、神話汚染が少し薄れる。", "鈴の内側から別の音が返り、正気が削れた。", "");
            AddEvent(list, "san_sakae_neon_overdose", "栄ネオンの正気過量", "栄 / SAN専用", "sakae", "event_sakae_broker", "栄の路地で、ネオンが心拍と同じ速度で点滅している。浴びれば恐怖は薄れるが、薄れすぎた恐怖は危険を危険と呼ばなくなる。", "SANを支払って恐怖を麻痺させる", "目を閉じて通る", 13, r => { r.stats.sanity = Math.Max(1, r.stats.sanity - 3); r.stats.attack += 2; r.dangerWarnings += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "恐怖が一枚剥がれた。SAN-3、攻撃+2、危険察知+1。", "目を閉じても、まぶたの裏に看板が残った。", "");
            AddEvent(list, "mythos_tsuruma_forbidden_catalog", "鶴舞の神話目録", "鶴舞 / 神話専用", "tsuruma", "event_tsuruma_librarian", "地下書庫の禁書目録は、読めない本ではなく読んではいけない本を先に開く。神話理解が高いほど、ページはあなたを読者ではなく共著者として扱う。", "神話理解で索引を逆引きする", "普通の分類で探す", 16, r => { r.stats.mythosKnowledge += 2; r.dangerWarnings += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 3); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "索引が一つ噛み合った。神話理解+2、危険察知+1、SAN-3。", "普通の分類番号のほうが、かえってあなたの死因に近かった。", "index_hound");
            AddEvent(list, "mythos_castle_sealed_name", "名古屋城の封名井戸", "名古屋城 / 神話専用", "castle", "event_shachi_avatar", "井戸の底に、呼んではいけない名が沈んでいる。神話理解が足りれば封じ直せる。足りなければ、名のほうがこちらを覚える。", "神話理解で名を封じ直す", "耳を塞いで離れる", 17, r => { r.stats.mythosKnowledge += 1; r.stats.defense += 1; progress.memoryFragments += 2; SaveProgress(); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 4); r.stats.mythosCorruption += 1; }, "名は水底へ戻った。神話理解+1、防御+1、記憶片+2。", "井戸はあなたの名を一拍だけ先に呼んだ。", "well_tentacle");
            return list;
        }
     void AddEvent(List<RandomEventDef> list, string id, string title, string area, string image, string portrait, string text, string successLabel, string failLabel, int difficulty, Action<RunState> success, Action<RunState> fail, string successText, string failText, string failBattle)
        {
            list.Add(new RandomEventDef
            {
                id = id,
                title = title,
                area = area,
                image = image,
                portrait = portrait,
                text = text,
                successLabel = successLabel,
                failLabel = failLabel,
                difficulty = difficulty,
                success = success,
                fail = fail,
                successText = successText,
                failText = failText,
                failBattle = failBattle
            });
        }
        void StartBattle(string enemyId)
        {
            enemyId = ResolveMythicBossOverride(enemyId);
            if ((enemyId == "miso_voice" || enemyId == "gate_guard") && run != null && !run.flags.Contains("prep_" + enemyId))
            {
                ShowBossPrep(enemyId);
                return;
            }
          mode = Mode.Battle;
            var template = enemies[enemyId];
            activeEnemy = new EnemyDef
            {
                id = template.id,
                name = template.name,
                image = template.image,
                maxHp = template.maxHp,
                hp = template.maxHp,
                attack = template.attack,
                defense = template.defense,
                speed = template.speed,
                sanityDamage = template.sanityDamage,
                reward = template.reward,
                intro = template.intro,
                victoryText = template.victoryText,
                defeatEnding = template.defeatEnding,
                weakness = template.weakness,
                portrait = template.portrait
            };
            ApplyInstabilityToEnemy(activeEnemy);
            ApplyBossWorldChange(activeEnemy);
            if (run.flags.Contains("weaken_" + enemyId))
            {
                activeEnemy.maxHp = Mathf.CeilToInt(activeEnemy.maxHp * 0.8f);
                activeEnemy.hp = activeEnemy.maxHp;
            }
            RegisterMonsterSeen(activeEnemy.id);
            battleRound = 1;
            SetBackground(activeEnemy.image);
            SetPortrait(activeEnemy.portrait);
            choiceRoot.gameObject.SetActive(false);
            battleRoot.gameObject.SetActive(true);
            titleText.text = activeEnemy.name;
            areaText.text = "連打バトル";
            bodyText.text = activeEnemy.intro + "\n\n" + MonsterHint(activeEnemy);
            enemyHpSlider.maxValue = activeEnemy.maxHp;
          enemyHpSlider.value = activeEnemy.hp;
            attackGauge = 0f;
            guardGauge = 0f;
            ScheduleEnemyAttack();
            BeginAttackPhase();
            UpdateSideText();
            AddRetreatOption();
        }
        string ResolveMythicBossOverride(string enemyId)
        {
            if (run == null || enemyId == "impossible_one")
                return enemyId;
            bool boss = enemyId == "miso_voice" || enemyId == "gate_guard" || enemyId.StartsWith("stage_boss_");
            if (!boss)
                return enemyId;
            int mythicPressure = run.stats.mythosKnowledge + run.stats.mythosCorruption * 2;
            if (mythicPressure >= 12 && enemies.ContainsKey("impossible_one"))
            {
                LogRun("神話圧が限界を超えた。ボスの輪郭が、この世のものではない何かへ置き換わった。");
                return "impossible_one";
            }
            return enemyId;
        }
        void ShowBossPrep(string enemyId)
        {
            mode = Mode.Scene;
            run.flags.Add("prep_" + enemyId);
            battleRoot.gameObject.SetActive(false);
            choiceRoot.gameObject.SetActive(true);
            SetBackground(enemyId == "miso_voice" ? "miso" : "airport");
            SetPortrait(enemyId == "miso_voice" ? "miso_voice" : "gate_inspector");
            titleText.text = "ボス前準備";
            areaText.text = enemyId == "miso_voice" ? "岡崎 / 樽の前" : "空港 / 搭乗口";
            bodyText.text = "決定的な怪異が、すぐ先であなたを待っている。\n\n準備を一つだけ選べる。ここでの準備は所持金を使わないが、ステージ末端の休憩準備には所持金が必要になる。";
            ClearChoices();
            AddChoiceButton("深呼吸する\nHP+5 / SAN+3", () =>
            {
               run.stats.hp = Math.Min(run.stats.maxHp, run.stats.hp + 5);
                run.stats.sanity = Math.Min(run.stats.maxSanity, run.stats.sanity + 3);
                StartBattle(enemyId);
            });
            AddChoiceButton("装備を捧げる\n敵HP-20%", () =>
            {
                run.flags.Add("weaken_" + enemyId);
                StartBattle(enemyId);
            });
            AddChoiceButton("SANを削る\n攻撃+4 / SAN-5", () =>
            {
                run.stats.attack += 4;
                run.stats.sanity = Math.Max(1, run.stats.sanity - 5);
                StartBattle(enemyId);
            });
            UpdateSideText();
        }
        void AddRetreatOption()
        {
            choiceRoot.gameObject.SetActive(true);
            ClearChoices();
            AddChoiceButton("撤退する", TryRetreat);
        }
        void TryRetreat()
        {
            if (activeEnemy == null || run == null)
                return;
            int cost = 120 + run.instability * 80;
            float chance = Mathf.Clamp01(0.62f + run.stats.speed * 0.025f - activeEnemy.speed * 0.035f);
            bool success = rng.NextDouble() < chance;
            run.lastChoiceLabel = "撤退する";
            if (success)
            {
                run.stats.money = Math.Max(0, run.stats.money - cost);
                bodyText.text = "あなたは暗い通路へ身を滑らせた。\n\n撤退成功。所持金 -" + cost;
                string next = !string.IsNullOrEmpty(run.battleReturnScene) ? run.battleReturnScene : run.sceneId;
                enemyAttackTimer = 0f;
                activeEnemy = null;
                battleRoot.gameObject.SetActive(false);
                choiceRoot.gameObject.SetActive(true);
              ClearChoices();
                AddChoiceButton("次に進む", () => ShowScene(next, false));
            }
            else
            {
                int damage = Mathf.Max(2, activeEnemy.attack / 2);
                run.stats.hp = Mathf.Max(0, run.stats.hp - damage);
                bodyText.text = "逃げ道が一拍遅れて閉じた。\n\n撤退失敗。HP -" + damage;
                if (run.stats.hp <= 0)
                    ShowEnding(activeEnemy.defeatEnding);
                else
                    BeginAttackPhase();
            }
            UpdateSideText();
        }
        void BeginAttackPhase()
        {
            attackWindow = 1f;
            guardWindow = 1f;
            attackButton.interactable = true;
            guardButton.interactable = guardGauge >= 1f;
            attackSlider.value = attackGauge;
            guardSlider.value = guardGauge;
            battleText.text = "Round " + battleRound + "\n攻撃可能";
            footerText.text = "攻撃はいつでも可能。攻撃ゲージが高いほど威力上昇。敵はランダムな間隔で攻撃します。";
        }
        void OnAttackTap()
        {
            if (mode != Mode.Battle || activeEnemy == null || run == null)
                return;
            ResolvePlayerAttack();
            Play(clickSfx, 0.35f);
        }
        void ResolvePlayerAttack()
        {
          attackButton.interactable = false;
            attackWindow = 0f;
            float gaugePower = Mathf.Clamp01(attackGauge);
            float tier = 0.72f + gaugePower * 0.78f + Mathf.Min(0.22f, (run.stats.speed + run.weapon.speed + run.accessory.speed) * 0.02f);
            int baseDamage = run.stats.attack + run.weapon.attack + run.accessory.attack + Mathf.RoundToInt(2 + gaugePower * 8f) - activeEnemy.defense;
            int damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * tier));
            if (activeEnemy.id == "impossible_one" && run.stats.mythosKnowledge >= 10)
                damage += run.stats.mythosKnowledge * 4 + run.stats.luck * 2;
            bool crit = rng.NextDouble() < 0.08 + (run.stats.luck + run.accessory.luck) * 0.012f;
            if (crit)
                damage = Mathf.RoundToInt(damage * 1.65f);
            activeEnemy.hp = Mathf.Max(0, activeEnemy.hp - damage);
            enemyHpSlider.value = activeEnemy.hp;
            attackGauge = 0f;
            attackSlider.value = 0f;
            Play(hitSfx);
            bodyText.text = activeEnemy.name + "へ " + damage + " ダメージ。" + (crit ? "\n会心。" : "");
            if (activeEnemy.hp <= 0)
            {
                WinBattle();
                return;
            }
            attackWindow = 1f;
            attackButton.interactable = true;
        }
        void BeginGuardPhase()
        {
            guardWindow = 2.4f + run.stats.speed * 0.03f;
            guardGauge = UnityEngine.Random.Range(0.05f, 0.45f);
            attackButton.interactable = false;
            guardButton.interactable = true;
            battleText.text = "敵の反撃\n防御ゲージを止める";
        }
        void OnGuardTap()
        {
           if (mode != Mode.Battle || guardGauge < 1f)
                return;
            ResolveCharacterSkill();
            Play(clickSfx, 0.35f);
        }
        void ResolveEnemyAttack()
        {
            if (mode != Mode.Battle || activeEnemy == null || run == null)
                return;
            enemyAttackTimer = 0f;
            if (activeEnemy.id == "impossible_one")
            {
                run.stats.hp = 0;
                run.stats.sanity = 0;
                Play(doomSfx);
                bodyText.text = activeEnemy.name + "の一撃で、HPとSANが同時に0になった。";
                UpdateSideText();
                ShowEnding("impossible_death");
                return;
            }
            int raw = activeEnemy.attack + Mathf.Max(0, activeEnemy.speed - run.stats.speed);
            int blocked = Mathf.RoundToInt((run.stats.defense + run.armor.defense + run.accessory.defense) * 0.55f);
            int damage = Mathf.Max(1, raw - blocked);
            run.stats.hp = Mathf.Max(0, run.stats.hp - damage);
            int sanityLoss = Mathf.Max(0, activeEnemy.sanityDamage - Mathf.RoundToInt((run.stats.luck + run.accessory.luck) * 0.15f));
            run.stats.sanity = Mathf.Max(0, run.stats.sanity - sanityLoss);
            Play(hurtSfx);
            bodyText.text = activeEnemy.name + "の攻撃。HP -" + damage + (sanityLoss > 0 ? " / SAN -" + sanityLoss : "");
            UpdateSideText();
            if (run.stats.hp <= 0)
            {
                ShowEnding(activeEnemy.defeatEnding);
                return;
            }
          if (run.stats.sanity <= 0)
            {
                QueueSanityCollapse(!string.IsNullOrEmpty(run.battleReturnScene) ? run.battleReturnScene : run.sceneId);
                return;
            }
            battleRound++;
            battleText.text = "Round " + battleRound + "\n敵の次行動を警戒";
            ScheduleEnemyAttack();
        }
        void ScheduleEnemyAttack()
        {
            if (activeEnemy == null || run == null)
            {
                enemyAttackTimer = 0f;
                return;
            }
            float baseDelay = UnityEngine.Random.Range(2.2f, 5.2f);
            float speedPressure = Mathf.Clamp(activeEnemy.speed - run.stats.speed, -4f, 8f) * 0.18f;
            enemyAttackTimer = Mathf.Clamp(baseDelay - speedPressure + UnityEngine.Random.Range(-0.45f, 0.85f), 1.4f, 6.4f);
        }
        void ResolveCharacterSkill()
        {
            guardButton.interactable = false;
            guardGauge = 0f;
            guardSlider.value = 0f;
            string message;
            switch (run.character.id)
            {
                case "worker":
                    run.stats.hp = Math.Min(run.stats.maxHp, run.stats.hp + 5);
                    run.stats.sanity = Math.Min(run.stats.maxSanity, run.stats.sanity + 1);
                    message = "出張会社員の危機対応。HP+5 / SAN+1";
                    break;
                case "local":
                  activeEnemy.hp = Mathf.Max(0, activeEnemy.hp - Mathf.Max(3, run.stats.localKnowledge + run.stats.luck / 2));
                    message = "地元出身者の抜け道看破。敵HPを削った。";
                    break;
                case "occult":
                    activeEnemy.defense = Mathf.Max(0, activeEnemy.defense - 2);
                    run.stats.mythosKnowledge += 1;
                    run.stats.sanity = Math.Max(0, run.stats.sanity - 1);
                    message = "オカルト研究者の解析。敵防御-2 / 神話理解+1 / SAN-1";
                    break;
                case "samurai":
                    activeEnemy.hp = Mathf.Max(0, activeEnemy.hp - Mathf.Max(6, run.stats.attack + run.weapon.attack));
                    message = "三河武士の裂帛。追加攻撃が入った。";
                    break;
                case "mechanic":
                    run.stats.hp = Math.Min(run.stats.maxHp, run.stats.hp + 4);
                    run.stats.defense += 1;
                    message = "工場の整備士の応急補修。HP+4 / 防御+1";
                    break;
                case "shachi_seen":
                    activeEnemy.hp = Mathf.Max(0, activeEnemy.hp - 10);
                    run.stats.sanity = Math.Max(0, run.stats.sanity - 2);
                    message = "金鯱の視線を返した。敵HP-10 / SAN-2";
                    break;
                case "atsuta_miko":
                    activeEnemy.attack = Mathf.Max(1, activeEnemy.attack - 2);
                    run.stats.sanity = Math.Max(0, run.stats.sanity - 1);
                    message = "熱田の封じ縄。敵攻撃-2 / SAN-1";
                    break;
                case "seto_potter":
                    run.stats.defense += 2;
                    run.stats.hp = Math.Min(run.stats.maxHp, run.stats.hp + 3);
                    message = "瀬戸の陶片装甲。防御+2 / HP+3";
                    break;
                case "toyohashi_conductor":
                    attackGauge = 1f;
                    attackSlider.value = 1f;
                    attackButton.interactable = true;
                    run.stats.speed += 1;
                    message = "豊橋の終電ベル。攻撃ゲージ即時100 / 速さ+1";
                    break;
                case "gamagori_diver":
                   activeEnemy.hp = Mathf.Max(0, activeEnemy.hp - 8);
                    run.stats.mythosKnowledge += 1;
                    message = "蒲郡の海底星図。敵HP-8 / 神話理解+1";
                    break;
                case "arimatsu_weaver":
                    run.stats.luck += 2;
                    run.dangerWarnings += 1;
                    message = "有松の分岐糸。LUK+2 / 危険察知+1";
                    break;
                case "inuyama_mask":
                    activeEnemy.defense = Mathf.Max(0, activeEnemy.defense - 1);
                    activeEnemy.attack = Mathf.Max(1, activeEnemy.attack - 1);
                    run.stats.sanity = Math.Min(run.stats.maxSanity, run.stats.sanity + 1);
                    message = "犬山の変相面。敵攻防-1 / SAN+1";
                    break;
                case "tsuruma_librarian":
                    activeEnemy.defense = 0;
                    run.stats.mythosKnowledge += 2;
                    run.stats.mythosCorruption += 1;
                    message = "鶴舞の禁書索引。敵防御0 / 神話理解+2 / 汚染+1";
                    break;
                case "centrair_agent":
                    activeEnemy.hp = Mathf.Max(0, activeEnemy.hp - 12);
                    run.stats.hp = Math.Min(run.stats.maxHp, run.stats.hp + 6);
                    message = "境界職員の搭乗拒否。敵HP-12 / HP+6";
                    break;
                default:
                    run.stats.hp = Math.Min(run.stats.maxHp, run.stats.hp + 3);
                    run.dangerWarnings += 1;
                    message = "旅行者の直感。HP+3 / 危険察知+1";
                    break;
            }
            enemyHpSlider.value = activeEnemy.hp;
            bodyText.text = message;
            UpdateSideText();
           if (activeEnemy.hp <= 0)
                WinBattle();
            else if (run.stats.sanity <= 0)
                QueueSanityCollapse(!string.IsNullOrEmpty(run.battleReturnScene) ? run.battleReturnScene : run.sceneId);
        }
        bool TryShowSanityCollapse(string nextSceneId)
        {
            if (run == null || run.stats.sanity > 0 || run.sanityCollapseTurns < 0)
                return false;
            run.sanityCollapseReturnScene = string.IsNullOrEmpty(nextSceneId) ? run.sceneId : nextSceneId;
            run.sanityCollapseTurns--;
            if (run.sanityCollapseTurns > 0)
                return false;
            ShowSanityCollapseEvent(run.sanityCollapseReturnScene);
            return true;
        }
        void QueueSanityCollapse(string returnScene)
        {
            if (run == null)
                return;
            run.stats.sanity = 0;
            run.sanityCollapseReturnScene = string.IsNullOrEmpty(returnScene) ? run.sceneId : returnScene;
            if (run.sanityCollapseTurns < 0)
                run.sanityCollapseTurns = 3;
            ShowSanityWarning(run.sanityCollapseReturnScene);
        }
        void ShowSanityWarning(string returnScene)
        {
            mode = Mode.Scene;
            activeEnemy = null;
            enemyAttackTimer = 0f;
            attackWindow = 0f;
            guardWindow = 0f;
            battleRoot.gameObject.SetActive(false);
            choiceRoot.gameObject.SetActive(true);
            SetBackground("tsuruma");
            SetPortrait("event_occult_researcher");
            titleText.text = "SAN崩壊の予兆";
            areaText.text = "正気の底 / 予兆";
            bodyText.text =
                "視界の端で、あなたの名前だけが一拍遅れて動いた。\n\n" +
                "まだ戦闘ではない。けれどSANが底を打ったため、数ターン後に専用イベントへ入る。\n" +
                "それまでに正気を取り戻せなければ、愛知の街並みは少しずつ別の地図へ置き換わっていく。";
            footerText.text = "SAN 0: 予兆発生。あと " + Mathf.Max(1, run.sanityCollapseTurns) + " 回の進行後、SAN専用イベントへ移行します。";
            UpdateSideText();
            ClearChoices();
            AddChoiceButton("震えを抑えて進む", () => ShowScene(returnScene, false));
        }
        void ShowSanityCollapseEvent(string returnScene)
        {
            mode = Mode.Scene;
            activeEnemy = null;
            enemyAttackTimer = 0f;
            attackWindow = 0f;
            guardWindow = 0f;
            run.sanityCollapseTurns = -1;
            battleRoot.gameObject.SetActive(false);
            choiceRoot.gameObject.SetActive(true);
            SetBackground("atsuta");
            SetPortrait("event_atsuta_miko");
            titleText.text = "SAN専用イベント: 帰れない影";
            areaText.text = "熱田 / 正気祓い";
            bodyText.text =
                "熱田の鳥居の影が、あなたの歩幅に合わせて一本ずつ増えていく。\n\n" +
                "影はボスではない。あなた自身が現実に戻るための、最後の結び目だ。";
            footerText.text = "SAN専用イベント。選択で正気を回復するか、神話理解で傷口を縫い止めます。";
            UpdateSideText();
            ClearChoices();
            AddChoiceButton("正気の欠片を拾う\nHP-4 / SAN+4", () =>
            {
                var before = run.stats.Clone();
                run.stats.hp = Math.Max(1, run.stats.hp - 4);
                run.stats.sanity = Math.Min(run.stats.maxSanity, run.stats.sanity + 4);
                string delta = BuildStatDelta(before, run.stats);
                LogRun("SAN専用イベント: 正気の欠片 " + delta);
                ShowChoiceOutcome(returnScene, "正気の欠片を拾う\n" + delta);
            });
            AddChoiceButton("神話で縫い止める\n神話+1 / SAN+2", () =>
            {
                var before = run.stats.Clone();
                run.stats.mythosKnowledge += 1;
                run.stats.mythosCorruption += 1;
                run.stats.sanity = Math.Min(run.stats.maxSanity, run.stats.sanity + 2);
                string delta = BuildStatDelta(before, run.stats);
                LogRun("SAN専用イベント: 神話で縫い止める " + delta);
                ShowChoiceOutcome(returnScene, "神話で縫い止める\n" + delta);
            });
            AddChoiceButton("見ないふりで歩く\n危険察知+1", () =>
            {
                var before = run.stats.Clone();
                run.dangerWarnings += 1;
                run.stats.mythosCorruption += 2;
                run.stats.sanity = 1;
                string delta = BuildStatDelta(before, run.stats);
                LogRun("SAN専用イベント: 見ないふり " + delta);
                ShowChoiceOutcome(returnScene, "見ないふりで歩く\n" + delta + "\n危険察知+1");
            });
        }
        void TriggerImpossibleBattle()
        {
            if (run == null || !enemies.ContainsKey("impossible_one"))
            {
                ShowEnding("madness");
                return;
            }
            if (run.flags.Contains("impossible_battle_done"))
            {
                ShowEnding("madness");
                return;
            }
            run.flags.Add("impossible_battle_done");
            run.battleReturnScene = null;
            LogRun("SANが0になり、現実の外側から「この世のものとは思えないもの」が現れた。");
            StartBattle("impossible_one");
        }
        void ApplyInstabilityToEnemy(EnemyDef enemy)
        {
            if (run == null || run.instability <= 0)
                return;
            float hpRate = 1f + run.instability * 0.16f;
            float atkRate = 1f + run.instability * 0.11f;
            enemy.maxHp = Mathf.CeilToInt(enemy.maxHp * hpRate);
            enemy.hp = enemy.maxHp;
            enemy.attack = Mathf.CeilToInt(enemy.attack * atkRate);
            enemy.reward = Mathf.CeilToInt(enemy.reward * InstabilityReward(run.instability));
            enemy.sanityDamage += run.instability >= 3 ? 1 : 0;
        }
        void ApplyBossWorldChange(EnemyDef enemy)
        {
            if (progress.bossesDefeated.Contains(enemy.id))
            {
               enemy.maxHp = Mathf.CeilToInt(enemy.maxHp * 0.92f);
                enemy.hp = enemy.maxHp;
                enemy.reward += 2;
                enemy.intro += "\n\n図鑑に残った過去の勝利が、敵の癖をわずかに浮かび上がらせる。";
            }
        }
        void WinBattle()
        {
            int reward = activeEnemy.reward;
            if (run.flags.Contains("bad_dice"))
                reward = Mathf.CeilToInt(reward * 1.2f);
            reward = Mathf.CeilToInt(reward * run.character.rewardRate);
            progress.memoryFragments += reward;
            if (!progress.defeated.Contains(activeEnemy.id))
                progress.defeated.Add(activeEnemy.id);
            RegisterVictoryCounter(activeEnemy.id);
            if ((activeEnemy.id == "miso_voice" || activeEnemy.id == "gate_guard") && !progress.bossesDefeated.Contains(activeEnemy.id))
                progress.bossesDefeated.Add(activeEnemy.id);
            if (!progress.monsterWeaknesses.Contains(activeEnemy.id))
                progress.monsterWeaknesses.Add(activeEnemy.id);
            CheckMilestones();
            SaveProgress();
            Play(rewardSfx);
            if (activeEnemy.id == "impossible_one")
            {
                LogRun("ありえない敵を倒した。世界はあなたを搭乗者ではなく観測者として記録した。");
                ShowEnding("impossible_true");
                return;
            }
            string next = !string.IsNullOrEmpty(run.battleReturnScene) ? run.battleReturnScene :
                          activeEnemy.id == "piyorin" ? "nagoya_after_battle" :
                          activeEnemy.id == "miso_voice" ? "okazaki_after_battle" :
                          activeEnemy.id == "gate_guard" ? "airport_gate" : "nagoya_after_battle";
            run.battleReturnScene = null;
            run.pendingGear = GenerateRandomGear(activeEnemy.reward + run.instability * 2);
            LogRun("戦闘勝利: " + activeEnemy.name + " 記憶片+" + reward);
            bodyText.text = activeEnemy.victoryText + "\n\n記憶片 +" + reward;
            battleRoot.gameObject.SetActive(false);
          choiceRoot.gameObject.SetActive(true);
            ClearChoices();
            enemyAttackTimer = 0f;
            activeEnemy = null;
            AddChoiceButton("戦利品を見る", () => ShowGearOffer(next));
            UpdateSideText();
            AddChoiceButton("次に進む", () => ShowScene(next));
        }
        Gear GenerateRandomGear(int power)
        {
            var templates = new List<Gear>();
            foreach (var g in gears.Values)
            {
                if (!IsEmptyGear(g))
                    templates.Add(g);
            }
            var gear = templates[rng.Next(templates.Count)].Clone();
            int tier = SelectDropRarityTier(power);
            int variance = Mathf.Clamp(power / 8 + rng.Next(0, 4), 1, 8);
            gear.attack = Mathf.Max(0, gear.attack + rng.Next(0, 2));
            gear.defense = Mathf.Max(0, gear.defense + rng.Next(0, 2));
            gear.speed += rng.Next(0, 2);
            gear.luck += rng.Next(0, 2);
            int stat = rng.Next(0, 4);
            if (stat == 0) gear.attack += variance;
            else if (stat == 1) gear.defense += variance;
            else if (stat == 2) gear.speed += Mathf.Max(1, variance / 2);
            else gear.luck += Mathf.Max(1, variance / 2);
            ApplyDropRarity(gear, tier, power);
            return gear;
        }
        Gear EmptyGear(string slot)
        {
            return new Gear
            {
              id = "empty_" + slot,
                name = slot == "武器" ? "素手" : slot == "防具" ? "防具なし" : "装飾なし",
                kind = "なし",
                slot = slot,
                rarity = "-",
                attack = 0,
                defense = 0,
                speed = 0,
                luck = 0,
                score = 0,
                note = "持ち込みなし。現地で拾った装備だけが頼りになる。"
            };
        }
        bool IsEmptyGear(Gear gear)
        {
                       return gear == null || (!string.IsNullOrEmpty(gear.id) && gear.id.StartsWith("empty_"));
        }
        Gear RollGear(Gear template)
        {
            var gear = template.Clone();
            gear.attack = Mathf.Max(0, gear.attack + rng.Next(-1, 3));
            gear.defense = Mathf.Max(0, gear.defense + rng.Next(-1, 3));
            gear.speed += rng.Next(-1, 2);
            gear.luck += rng.Next(0, 3);
            ApplyGearRarity(gear);
            return gear;
        }
        void ApplyGearRarity(Gear gear)
        {
            gear.score = gear.attack * 3 + gear.defense * 3 + gear.speed * 2 + gear.luck * 2;
            if (gear.score >= 24) { gear.rarity = "橙"; if (!gear.name.StartsWith("伝説の")) gear.name = "伝説の" + gear.name; }
            else if (gear.score >= 17) { gear.rarity = "紫"; if (!gear.name.StartsWith("異質な")) gear.name = "異質な" + gear.name; }
            else if (gear.score >= 11) { gear.rarity = "緑"; if (!gear.name.StartsWith("貴重な")) gear.name = "貴重な" + gear.name; }
            else gear.rarity = "白";
        }
        int SelectDropRarityTier(int power)
        {
            float pressure = Mathf.Clamp01(power / 34f);
            float orange = 0.012f + pressure * 0.045f + run.instability * 0.003f;
            float purple = 0.055f + pressure * 0.115f + run.instability * 0.008f;
            float green = 0.245f + pressure * 0.13f;
            double roll = rng.NextDouble();
            if (roll < orange) return 3;
            if (roll < orange + purple) return 2;
            if (roll < orange + purple + green) return 1;
            return 0;
        }
        void ApplyDropRarity(Gear gear, int tier, int power)
        {
            string baseName = StripRarityPrefix(gear.name);
            float multiplier = tier == 3 ? 3.1f : tier == 2 ? 2.15f : tier == 1 ? 1.45f : 1f;
            int flat = tier == 3 ? 6 : tier == 2 ? 3 : tier == 1 ? 1 : 0;
            int powerBonus = Mathf.Clamp(power / 12, 0, tier == 3 ? 5 : tier == 2 ? 3 : 2);
           gear.attack = Mathf.Max(0, Mathf.RoundToInt(gear.attack * multiplier));
            gear.defense = Mathf.Max(0, Mathf.RoundToInt(gear.defense * multiplier));
            gear.speed = Mathf.RoundToInt(gear.speed * (1f + (multiplier - 1f) * 0.55f));
            gear.luck = Mathf.Max(0, Mathf.RoundToInt(gear.luck * (1f + (multiplier - 1f) * 0.65f)));
            if (gear.slot == "武器") gear.attack += flat + powerBonus;
            else if (gear.slot == "防具") gear.defense += flat + powerBonus;
            else gear.luck += Mathf.Max(1, flat / 2 + powerBonus);
            gear.score = gear.attack * 3 + gear.defense * 3 + gear.speed * 2 + gear.luck * 2;
            if (tier == 3) { gear.rarity = "橙"; gear.name = "伝説の" + baseName; }
            else if (tier == 2) { gear.rarity = "紫"; gear.name = "異質な" + baseName; }
            else if (tier == 1) { gear.rarity = "緑"; gear.name = "貴重な" + baseName; }
            else { gear.rarity = "白"; gear.name = baseName; }
            gear.note = gear.note + "\n希少度補正: " + DropRarityNote(tier);
        }
        string DropRarityNote(int tier)
        {
            if (tier == 3) return "極低確率。性能が大きく跳ね上がる。";
            if (tier == 2) return "低確率。基礎性能が大きく上がる。";
            if (tier == 1) return "やや希少。平均より扱いやすい。";
            return "通常品。安定している。";
        }
        string StripRarityPrefix(string name)
        {
            if (name.StartsWith("伝説の")) return name.Substring(3);
            if (name.StartsWith("異質な")) return name.Substring(3);
            if (name.StartsWith("貴重な")) return name.Substring(3);
            return name;
        }
        void ShowGearOffer(string next)
        {
            if (run.pendingGear == null)
            {
                ShowScene(next);
                return;
           }
            var current = CurrentGearForSlot(run.pendingGear.slot);
            titleText.text = "装備比較";
            areaText.text = run.pendingGear.rarity + " / " + run.pendingGear.slot;
            bodyText.text = GearComparison(run.pendingGear, current);
            ClearChoices();
            AddChoiceButton("装備する", () => { LogRun("装備変更: " + GearShortName(run.pendingGear)); EquipGear(run.pendingGear); run.pendingGear = null; UpdateSideText(); ShowScene(next); });
            AddChoiceButton("倉庫へ送る\n喪失リスク", () => TryStorePendingGear(next));
            AddChoiceButton("捨てる", () => { run.pendingGear = null; ShowScene(next); });
            footerText.text = "希少度: 白は出やすい / 緑はやや希少 / 紫は低確率 / 橙は極低確率。希少なほど性能が大きく上がります。";
        }
        Gear CurrentGearForSlot(string slot)
        {
            if (slot == "防具") return run.armor;
            if (slot == "装飾品") return run.accessory;
            return run.weapon;
        }
        void EquipGear(Gear gear)
        {
            if (gear.slot == "防具") run.armor = gear;
            else if (gear.slot == "装飾品") run.accessory = gear;
            else run.weapon = gear;
        }
        string GearSummary(Gear gear)
        {
            if (gear == null || IsEmptyGear(gear)) return "装備なし";
            return gear.name + " [" + gear.rarity + "]\n攻+" + gear.attack + " 防+" + gear.defense + " 速+" + gear.speed + " LUK+" + gear.luck + "\n" + gear.note;
        }
        string GearComparison(Gear next, Gear current)
        {
            string delta = GearDeltaLine(next, current);
            return "現在の" + next.slot + "との差: " + delta +
                   "\n\nドロップ: " + GearOneLine(next) +
                   "\n現在: " + GearOneLine(current) +
                   "\n\n詳細\n" + GearSummary(next) +
                   "\n\n倉庫へ送る場合、異界転送に失敗して装備が壊れることがあります。高レアほど転送が不安定です。";
        }
      string GearOneLine(Gear gear)
        {
            if (gear == null || IsEmptyGear(gear)) return "装備なし";
            return gear.name + " [" + gear.rarity + "] 攻" + Signed(gear.attack) + " 防" + Signed(gear.defense) + " 速" + Signed(gear.speed) + " LUK" + Signed(gear.luck);
        }
        string GearDeltaLine(Gear next, Gear current)
        {
            int attackDelta = next.attack - (current != null ? current.attack : 0);
            int defenseDelta = next.defense - (current != null ? current.defense : 0);
            int speedDelta = next.speed - (current != null ? current.speed : 0);
            int luckDelta = next.luck - (current != null ? current.luck : 0);
            int scoreDelta = next.score - (current != null ? current.score : 0);
            return "攻撃 " + Signed(attackDelta) + " / 防御 " + Signed(defenseDelta) +
                   " / 速さ " + Signed(speedDelta) + " / LUK " + Signed(luckDelta) +
                   " / 総合 " + Signed(scoreDelta);
        }
        string Signed(int value)
        {
            return value >= 0 ? "+" + value : value.ToString();
        }
        void TryStorePendingGear(string next)
        {
            var gear = run.pendingGear;
            run.pendingGear = null;
            if (gear == null)
            {
                ShowScene(next);
                return;
            }
            float chance = 0.84f + run.stats.luck * 0.012f - run.instability * 0.045f - RarityTransferPenalty(gear);
            chance = Mathf.Clamp(chance, 0.42f, 0.92f);
            bool success = rng.NextDouble() < chance;
            ClearChoices();
            if (success)
            {
                StoreGear(gear);
                LogRun("倉庫転送成功: " + GearShortName(gear));
               bodyText.text = gear.name + " を倉庫へ転送した。\n\n成功率 " + Mathf.RoundToInt(chance * 100f) + "%。今回は境界が装備を飲み込まなかった。";
                Play(rewardSfx);
            }
            else
            {
                string broken = "転送失敗: " + gear.name;
                progress.brokenGear.Add(broken);
                SaveProgress();
                LogRun("倉庫転送失敗: " + GearShortName(gear));
                bodyText.text = gear.name + " の倉庫転送に失敗した。\n\n成功率 " + Mathf.RoundToInt(chance * 100f) + "%。装備は境界で砕け、破損記録だけが残った。";
                Play(doomSfx);
            }
            UpdateSideText();
            AddChoiceButton("次に進む", () => ShowScene(next));
        }
        float RarityTransferPenalty(Gear gear)
        {
            if (gear.rarity == "橙") return 0.18f;
            if (gear.rarity == "紫") return 0.11f;
            if (gear.rarity == "緑") return 0.05f;
            return 0f;
        }
        void StoreGear(Gear gear)
        {
            if (gear == null || IsEmptyGear(gear)) return;
            progress.warehouseGear.Add(SerializeGear(gear));
            SaveProgress();
        }
        string SerializeGear(Gear gear)
        {
            return gear.id + "|" + gear.name + "|" + gear.slot + "|" + gear.rarity + "|" + gear.attack + "|" + gear.defense + "|" + gear.speed + "|" + gear.luck + "|" + gear.note.Replace("|", "/");
        }
        Gear DeserializeGear(string data)
        {
          var p = data.Split('|');
            if (p.Length < 9 || !gears.ContainsKey(p[0])) return null;
            var gear = gears[p[0]].Clone();
            gear.name = p[1]; gear.slot = p[2]; gear.rarity = p[3];
            int.TryParse(p[4], out gear.attack);
            int.TryParse(p[5], out gear.defense);
            int.TryParse(p[6], out gear.speed);
            int.TryParse(p[7], out gear.luck);
            gear.note = p[8];
            ApplyGearRarity(gear);
            return gear;
        }
        void RegisterMonsterSeen(string monsterId)
        {
            if (!progress.seenMonsters.Contains(monsterId))
            {
                progress.seenMonsters.Add(monsterId);
                SaveProgress();
                Play(pageSfx);
                footerText.text = "怪異図鑑に新しい項目が追加されました。";
            }
        }
        string MonsterHint(EnemyDef enemy)
        {
            if (progress.monsterWeaknesses.Contains(enemy.id))
                return "図鑑解析済み: 弱点 " + enemy.weakness + "\n対策: " + MonsterStrategy(enemy.id);
            if (progress.seenMonsters.Contains(enemy.id))
                return "図鑑記録: まだ弱点は曖昧だ。倒せば解析できる。\n予兆: " + MonsterForeshadow(enemy.id);
            return "図鑑未登録: 正体不明。";
        }
        string MonsterStrategy(string enemyId)
        {
            switch (enemyId)
            {
              case "piyorin": return "打撃武器と高い正気度。低HPなら撤退も有効。";
                case "miso_voice": return "味噌耐性とSAN回復。樽の声を聞きすぎない。";
                case "gate_guard": return "神話理解と鯱の注視。ボス前準備で敵を弱体化。";
                default: return "図鑑情報を増やすほど対策が明確になる。";
            }
        }
        string MonsterForeshadow(string enemyId)
        {
            switch (enemyId)
            {
                case "piyorin": return "床に黄色い羽毛が落ちる。";
                case "miso_voice": return "味噌の匂いが濃くなる。";
                case "gate_guard": return "搭乗案内があなたの名前を呼ぶ。";
                default: return "空気の温度が一段下がる。";
            }
        }
        string DeathHint(string endingId)
        {
            if (endingId == "return" || endingId == "true_shachi" || endingId == "impossible_true")
                return "";
            return "\n\n死因図鑑に「" + EndingName(endingId) + "」が登録された。次回以降、近い選択肢には警告が出る。";
        }
        string EndingName(string endingId)
        {
            if (endingId == "impossible_death")
                return "現実外捕食";
            if (endingId == "impossible_true")
                return "裏ED: 観測者の搭乗";
            switch (endingId)
            {
                case "return": return "帰還";
                case "true_shachi": return "双鯱調停";
                case "miso_sink": return "味噌沈降";
                case "machine_part": return "効率化";
                case "madness": return "神話開眼";
                case "piyorin_bad": return "黄色い圧";
                case "airport_lost": return "荷物検査";
               default: return "永住";
            }
        }
        void RegisterVictoryCounter(string enemyId)
        {
            if (enemyId == "piyorin")
                progress.piyorinVictories++;
            else if (enemyId == "miso_voice")
                progress.misoVictories++;
            else if (enemyId == "gate_guard")
                progress.gateVictories++;
        }
        void CheckMilestones()
        {
            ClaimMilestone("piyorin_1", progress.piyorinVictories >= 1, "黄色い群体を初撃破。記憶片+5。", 5, 0);
            ClaimMilestone("piyorin_3", progress.piyorinVictories >= 3, "黄色い群体撃破3回。保険札+1。", 0, 1);
            ClaimMilestone("miso_1", progress.misoVictories >= 1, "味噌樽の声を鎮めた。記憶片+8。", 8, 0);
            ClaimMilestone("gate_1", progress.gateVictories >= 1, "搭乗検査官を突破。保険札+1。", 0, 1);
        }
        void ClaimMilestone(string id, bool condition, string message, int memory, int insurance)
        {
            if (!condition || progress.milestoneClaims.Contains(id))
                return;
            progress.milestoneClaims.Add(id);
            progress.memoryFragments += memory;
            progress.insuranceTickets += insurance;
            footerText.text = "実績: " + message;
            Play(rewardSfx);
        }
        void ShowEnding(string endingId)
        {
            mode = Mode.Ending;
            enemyAttackTimer = 0f;
            activeEnemy = null;
            battleRoot.gameObject.SetActive(false);
            choiceRoot.gameObject.SetActive(true);
            Play(doomSfx);
          int reward = 4;
            if (endingId == "impossible_death")
            {
                endingTitle = "現実外捕食";
                endingBody = "SANが0になった瞬間、搭乗ゲートよりも古い何かがあなたを見つけた。\n\n戦いはあった。だが、それは攻撃を受けるまでの数秒を戦闘と呼ぶなら、の話だった。";
                reward = 18;
            }
            else if (endingId == "impossible_true")
            {
                endingTitle = "裏ED: 観測者の搭乗";
                endingBody = "この世のものとは思えないものが崩れ、空港の床に存在しない星図が残った。\n\nあなたは搭乗者ではない。次の周回を、外側から観測する者として記録された。";
                reward = 90;
            }
            else switch (endingId)
            {
                case "return":
                    endingTitle = "帰還";
                    endingBody = "搭乗口の先に、見慣れた朝があった。\n\nだがポケットには、使った覚えのない喫茶店の回数券が一枚だけ残っている。";
                    reward = 22;
                    break;
                case "true_shachi":
                    endingTitle = "双鯱調停";
                    endingBody = "二つの金鯱は、互いの尾を噛むように夜空で輪を描いた。\n\n異界愛知と外界愛知の境界は、しばらく保たれる。あなたが次に眠るまでは。";
                    reward = 44;
                    break;
                case "miso_sink":
                    endingTitle = "味噌沈降";
                    endingBody = "あなたの記憶は樽の底へ沈んだ。\n\n熟成には時間がかかる。次に目覚めるあなたは、少しだけ塩辛い。";
                    reward = 10;
                    break;
                case "machine_part":
                    endingTitle = "効率化";
                    endingBody = "不要な感情が削られ、あなたは工場心臓の小さな部品になった。\n\n回転は滑らかだ。悲しいほどに。";
                    reward = 12;
                    break;
                case "madness":
                    endingTitle = "神話開眼";
                    endingBody = "見てはいけない構造が見えた。\n\n出口とは、内側へ進むための言葉だった。";
                    reward = 9;
                    break;
                case "piyorin_bad":
                   endingTitle = "黄色い圧";
                    endingBody = "柔らかな群れが、あなたの輪郭をやさしく押し潰した。\n\n最後に聞こえたのは、甘い羽音だった。";
                    reward = 7;
                    break;
                case "airport_lost":
                    endingTitle = "荷物検査";
                    endingBody = "検査官はあなたの記憶を丁寧に畳み、透明な箱へしまった。\n\n身軽になったあなたは、どこへも飛べなくなった。";
                    reward = 14;
                    break;
                default:
                    endingTitle = "永住";
                    endingBody = "モーニングは毎朝出てくる。\n\n断らなければ、ここはとても優しい場所だった。";
                    reward = 8;
                    break;
            }
            lastReward = Mathf.CeilToInt(reward * (run != null ? run.character.rewardRate : 1f));
            if (run != null && run.flags.Contains("bad_dice"))
                lastReward = Mathf.CeilToInt(lastReward * 1.2f);
            progress.memoryFragments += lastReward;
            if (!progress.endings.Contains(endingId))
                progress.endings.Add(endingId);
            if (endingId != "return" && endingId != "true_shachi" && endingId != "impossible_true" && !progress.deaths.Contains(endingId))
            {
                progress.deaths.Add(endingId);
                Play(pageSfx, 0.9f);
            }
            string lossReport = HandleDeathConsequences(endingId);
            UnlockInstabilityOnClear(endingId);
            SaveProgress();
            SetBackground(endingId == "true_shachi" ? "castle" : "ending");
            SetPortrait(endingId == "true_shachi" ? "shachi_avatar" : (endingId == "impossible_death" || endingId == "impossible_true") ? "impossible_one" : null);
            titleText.text = "END: " + endingTitle;
            areaText.text = "記憶片 +" + lastReward;
            bodyText.text = endingBody + DeathHint(endingId) + lossReport + BuildNewspaper(endingId);
            UpdateSideText();
            ClearChoices();
            if (run != null && endingId != "return" && endingId != "true_shachi" && endingId != "impossible_true")
                AddChoiceButton("記憶定着\n装備を1つ倉庫へ", ShowMemoryAnchor);
            AddChoiceButton("次の周回へ", ShowCharacterSelect);
            AddChoiceButton("タイトルへ", ShowTitle);
      }
        string HandleDeathConsequences(string endingId)
        {
            if (run == null || endingId == "return" || endingId == "true_shachi")
                return "";
            if (!string.IsNullOrEmpty(run.lastChoiceLabel))
            {
                string regret = EndingName(endingId) + ": " + run.lastChoiceLabel;
                if (!progress.regretLog.Contains(regret))
                    progress.regretLog.Add(regret);
            }
            if (progress.insuranceTickets > 0)
            {
                progress.insuranceTickets--;
                return "\n\n保険札が燃え、装備の輪郭を一つだけ現実へ繋ぎ止めた。";
            }
            if (rng.NextDouble() < 0.35)
            {
                string broken = "壊れた " + (rng.NextDouble() < 0.5 ? run.weapon.name : run.armor.name);
                progress.brokenGear.Add(broken);
                return "\n\n" + broken + " が倉庫に戻った。修理すれば、いつか使えるかもしれない。";
            }
            return "\n\n装備は異界に残された。記憶定着できたものはない。";
        }
        void ShowMemoryAnchor()
        {
            titleText.text = "記憶定着";
            areaText.text = "死亡時保護";
            bodyText.text = "異界に沈む直前、装備をひとつだけ現実へ繋ぎ止められる。\n\n" +
                "武器\n" + GearSummary(run.weapon) + "\n\n防具\n" + GearSummary(run.armor) + "\n\n装飾\n" + GearSummary(run.accessory);
            ClearChoices();
           AddChoiceButton("武器を定着", () => { StoreGear(run.weapon); ShowCharacterSelect(); }, !IsEmptyGear(run.weapon));
            AddChoiceButton("防具を定着", () => { StoreGear(run.armor); ShowCharacterSelect(); }, !IsEmptyGear(run.armor));
            AddChoiceButton("装飾を定着", () => { StoreGear(run.accessory); ShowCharacterSelect(); }, !IsEmptyGear(run.accessory));
            AddChoiceButton("定着せず戻る", ShowCharacterSelect);
        }
        void UnlockInstabilityOnClear(string endingId)
        {
            if (run == null)
                return;
            bool cleared = endingId == "return" || endingId == "true_shachi";
            if (cleared && run.instability >= progress.maxInstabilityUnlocked && progress.maxInstabilityUnlocked < 5)
                progress.maxInstabilityUnlocked = run.instability + 1;
        }
        string BuildNewspaper(string endingId)
        {
            if (run == null)
                return "";
            string headline = endingId == "return" || endingId == "true_shachi"
                ? run.character.name + "、" + InstabilityName(run.instability) + "から生還"
                : run.character.name + "、" + EndingName(endingId) + "により消息不明";
            string route = run.mikawa > run.owari ? "岡崎方面で樽鳴り増加" : "名古屋方面で鯱の影を観測";
            string boss = progress.bossesDefeated.Count > 0 ? "撃破済みボス: " + string.Join(", ", progress.bossesDefeated) : "ボス討伐記録なし";
            return "\n\n異界愛知日報\n" + headline + "\n" + route + "\n" + boss + "\n獲得記憶片: " + lastReward;
        }
        void ShowUnlocks()
        {
            mode = Mode.Unlocks;
            SetBackground("characters");
            SetPortrait("event_occult_researcher");
            titleText.text = "図鑑と解放";
            areaText.text = "周回記録";
            var text = "記憶片: " + progress.memoryFragments + " / 保険札: " + progress.insuranceTickets + "\n";
            text += "解放済み不安定度: " + progress.maxInstabilityUnlocked + "\n\n解放済みキャラクター:\n";
            foreach (var id in progress.unlockedCharacters)
                text += "・" + characters[id].name + "\n";
            text += "\n到達エンド:\n";
            if (progress.endings.Count == 0)
                text += "・未到達\n";
            foreach (var ending in progress.endings)
               text += "・" + EndingName(ending) + "\n";
            text += "\n死因図鑑:\n";
            if (progress.deaths.Count == 0)
                text += "・未登録\n";
            foreach (var death in progress.deaths)
                text += "・" + EndingName(death) + " / 次回から危険選択肢に警告\n";
            text += "\n怪異図鑑:\n";
            if (progress.seenMonsters.Count == 0)
                text += "・未遭遇\n";
            foreach (var monster in progress.seenMonsters)
            {
                string status = progress.monsterWeaknesses.Contains(monster) ? "弱点解析済み" : "遭遇のみ";
                text += "・" + EnemyName(monster) + " / " + status + "\n  対策: " + MonsterStrategy(monster) + "\n";
            }
            text += "\n後悔ログ:\n";
            if (progress.regretLog.Count == 0)
                text += "・未記録\n";
            for (int i = Mathf.Max(0, progress.regretLog.Count - 5); i < progress.regretLog.Count; i++)
                text += "・" + progress.regretLog[i] + "\n";
            bodyText.text = text;
            statsText.text = "";
            inventoryText.text = "破損装備:\n" + BrokenGearSummary() + "\n\n実績:\n黄色撃破 " + progress.piyorinVictories + "\n味噌撃破 " + progress.misoVictories + "\n検査官撃破 " + progress.gateVictories;
            battleRoot.gameObject.SetActive(false);
            choiceRoot.gameObject.SetActive(true);
            ClearChoices();
            AddChoiceButton("キャラ選択へ", ShowCharacterSelect);
            AddChoiceButton("保険札を買う\n記憶片10", BuyInsurance);
            AddChoiceButton("戻る", ShowTitle);
        }
        void BuyInsurance()
        {
            if (progress.memoryFragments < 10)
            {
                footerText.text = "記憶片が足りません。";
                Play(clickSfx);
                return;
            }
            progress.memoryFragments -= 10;
            progress.insuranceTickets++;
            SaveProgress();
            Play(rewardSfx);
         ShowUnlocks();
        }
        string BrokenGearSummary()
        {
            if (progress.brokenGear.Count == 0)
                return "なし";
            string text = "";
            for (int i = Mathf.Max(0, progress.brokenGear.Count - 4); i < progress.brokenGear.Count; i++)
                text += "・" + progress.brokenGear[i] + "\n";
            return text.TrimEnd();
        }
        string EnemyName(string enemyId)
        {
            return enemies.ContainsKey(enemyId) ? enemies[enemyId].name : enemyId;
        }
        string InstabilityName(int level)
        {
            switch (level)
            {
                case 1: return "微細な歪み";
                case 2: return "漏れ出す異界";
                case 3: return "神話の夜";
                case 4: return "双鯱の注視";
                case 5: return "境界崩壊";
                default: return "通常";
            }
        }
        float InstabilityReward(int level)
        {
            return 1f + Mathf.Clamp(level, 0, 5) * 0.22f;
        }
        string InstabilitySummary(int level)
        {
            float enemy = 1f + level * 0.16f;
            float attack = 1f + level * 0.11f;
         return "敵HP x" + enemy.ToString("0.00") + "\n敵攻撃 x" + attack.ToString("0.00") + "\n報酬 x" + InstabilityReward(level).ToString("0.00") + "\nイベント発生率 +" + (level * 4.5f).ToString("0") + "%";
        }
        void ResetProgress()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            LoadProgress();
            ShowTitle();
        }
        void UpdateSideText()
        {
            if (run == null)
                return;
            var s = run.stats;
            if (hpSlider != null)
            {
                hpSlider.maxValue = Mathf.Max(1, s.maxHp);
                hpSlider.value = Mathf.Clamp(s.hp, 0, s.maxHp);
            }
            if (hpText != null)
                hpText.text = s.hp + "/" + s.maxHp;
            if (coinText != null)
                coinText.text = "所持金 " + s.money;
            statsText.text =
                run.character.name + "\n" +
                "MP " + s.mp + "/" + s.maxMp + "\n" +
                "攻撃 " + (s.attack + run.weapon.attack + run.accessory.attack) + "\n" +
                "防御 " + (s.defense + run.armor.defense + run.accessory.defense) + "\n" +
                "速さ " + (s.speed + run.armor.speed + run.weapon.speed + run.accessory.speed) + "\n" +
                "LUK " + (s.luck + run.accessory.luck) + "\n" +
                "正気度 " + s.sanity + "/" + s.maxSanity + "\n" +
                "空腹 " + s.hunger + "\n" +
                "神話汚染 " + s.mythosCorruption;
         inventoryText.text =
                "武器: " + GearSummary(run.weapon) + "\n\n" +
                "防具: " + GearSummary(run.armor) + "\n\n" +
                "装飾: " + GearSummary(run.accessory) + "\n\n" +
                "所持金 " + s.money + "\n" +
                InstabilityName(run.instability) + " / 保険札 " + progress.insuranceTickets + "\n" +
                "死因 " + progress.deaths.Count + " / 怪異 " + progress.seenMonsters.Count + "\n\n" +
                SanityFlavor();
            inventoryText.text =
                "武器: " + GearSideSummary(run.weapon) + "\n" +
                "防具: " + GearSideSummary(run.armor) + "\n" +
                "装飾: " + GearSideSummary(run.accessory) + "\n\n" +
                "所持金 " + s.money + "\n" +
                InstabilityName(run.instability) + " / 保険札 " + progress.insuranceTickets + "\n" +
                "死因 " + progress.deaths.Count + " / 怪異 " + progress.seenMonsters.Count + "\n\n" +
                "最近の出来事\n" + RecentLogText();
        }
        void UpdateMadnessVisuals()
        {
            if (madnessOverlay == null || background == null || bodyText == null)
                return;
            float madness = 0f;
            if (run != null && (mode == Mode.Scene || mode == Mode.Battle || mode == Mode.Ending))
                madness = Mathf.Clamp01(1f - Mathf.Clamp01(run.stats.sanity / (float)Mathf.Max(1, run.stats.maxSanity)) + run.instability * 0.045f);
            float pulse = Mathf.Sin(Time.time * (5f + madness * 8f)) * 0.5f + 0.5f;
            madnessOverlay.color = new Color(0.45f, 0.015f, 0.035f, Mathf.Clamp01((madness - 0.35f) * 0.48f + pulse * madness * 0.06f));
            vignette.color = new Color(0f, 0f, 0f, 0.42f + madness * 0.28f);
            var rect = background.rectTransform;
            float wobble = madness > 0.45f ? Mathf.Sin(Time.time * 6.7f) * madness * 8f : 0f;
            rect.anchoredPosition = new Vector2(wobble, Mathf.Cos(Time.time * 5.3f) * madness * 4f);
           rect.localScale = Vector3.one * (1f + madness * 0.035f);
            bodyText.color = Color.Lerp(new Color(0.93f, 0.9f, 0.82f), new Color(1f, 0.62f, 0.58f), Mathf.Clamp01((madness - 0.45f) * 1.6f));
            titleText.color = Color.Lerp(new Color(0.95f, 0.86f, 0.62f), new Color(1f, 0.42f, 0.32f), Mathf.Clamp01((madness - 0.55f) * 1.8f));
            if (ambientSource != null)
                ambientSource.volume = 0.24f + madness * 0.24f;
        }
        void LogRun(string message)
        {
            if (run == null || string.IsNullOrEmpty(message))
                return;
            if (run.recentLog == null)
                run.recentLog = new List<string>();
            run.recentLog.Add(message);
            while (run.recentLog.Count > 6)
                run.recentLog.RemoveAt(0);
            if (footerText != null)
                footerText.text = message;
            UpdateSideText();
        }
        string RecentLogText()
        {
            if (run == null || run.recentLog == null || run.recentLog.Count == 0)
                return "まだ記録なし";
            string text = "";
            int start = Mathf.Max(0, run.recentLog.Count - 3);
            for (int i = start; i < run.recentLog.Count; i++)
                text += "・" + run.recentLog[i] + "\n";
            return text.TrimEnd();
        }
        string BuildStatDelta(Stats before, Stats after)
        {
            var changes = new List<string>();
            AddDelta(changes, "HP", after.hp - before.hp);
            AddDelta(changes, "SAN", after.sanity - before.sanity);
            AddDelta(changes, "所持金", after.money - before.money);
            AddDelta(changes, "攻撃", after.attack - before.attack);
            AddDelta(changes, "防御", after.defense - before.defense);
            AddDelta(changes, "速さ", after.speed - before.speed);
           AddDelta(changes, "LUK", after.luck - before.luck);
            AddDelta(changes, "神話", after.mythosKnowledge - before.mythosKnowledge);
            AddDelta(changes, "汚染", after.mythosCorruption - before.mythosCorruption);
            if (changes.Count == 0)
                return "大きな変化なし";
            return string.Join(" / ", changes);
        }
        void AddDelta(List<string> changes, string label, int delta)
        {
            if (delta != 0)
                changes.Add(label + Signed(delta));
        }
        string GearShortName(Gear gear)
        {
            if (gear == null || IsEmptyGear(gear))
                return "装備なし";
            return gear.name + " [" + gear.slot + "]";
        }
        string GearSideSummary(Gear gear)
        {
            if (gear == null || IsEmptyGear(gear))
                return "なし";
            return gear.name + " " + GearStatLine(gear);
        }
        string GearStatLine(Gear gear)
        {
            return "攻" + Signed(gear.attack) + " 防" + Signed(gear.defense) + " 速" + Signed(gear.speed) + " LUK" + Signed(gear.luck);
        }
        string SanityFlavor()
        {
            if (run == null)
                return "";
            float ratio = run.stats.sanity / (float)Mathf.Max(1, run.stats.maxSanity);
            if (ratio <= 0.18f)
              return "正気警告:\n文字があなたを読んでいる。";
            if (ratio <= 0.34f)
                return "正気警告:\n選択肢が少しずつ別の意味を持ち始めた。";
            if (ratio <= 0.55f)
                return "正気:\nまだ帰りたい理由を覚えている。";
            return "正気:\n今のところ、現実の輪郭は保たれている。";
        }
        void BuildData()
        {
            gears["umbrella"] = new Gear { id = "umbrella", name = "折りたたみ傘", kind = "打撃", attack = 2, defense = 0, speed = 0, note = "水難の気配に少し強い。" };
            gears["pcbag"] = new Gear { id = "pcbag", name = "社用PCバッグ", kind = "打撃", attack = 1, defense = 1, speed = -1, note = "重いが身を守れる。" };
            gears["ticket"] = new Gear { id = "ticket", name = "喫茶店の回数券", kind = "儀式具", attack = 1, defense = 0, speed = 1, note = "モーニング契約を少し誤魔化す。" };
            gears["recorder"] = new Gear { id = "recorder", name = "古い録音機", kind = "儀式具", attack = 1, defense = 0, speed = 0, note = "怪異の声を記録する。" };
            gears["wood_sword"] = new Gear { id = "wood_sword", name = "木刀", kind = "打撃", attack = 4, defense = 0, speed = 1, note = "連打ゲージがよく伸びる気がする。" };
            gears["wrench"] = new Gear { id = "wrench", name = "モンキーレンチ", kind = "機械具", attack = 3, defense = 1, speed = -1, note = "機械系に強い。" };
            gears["shachi_dagger"] = new Gear { id = "shachi_dagger", name = "しゃちほこ鱗の短剣", kind = "儀式具", attack = 5, defense = 0, speed = 2, note = "神話存在に届く。見返される。" };
            gears["jacket"] = new Gear { id = "jacket", name = "旅行ジャケット", kind = "衣服", attack = 0, defense = 1, speed = 0, note = "動きやすい普通の服。" };
            gears["suit"] = new Gear { id = "suit", name = "くたびれたスーツ", kind = "衣服", attack = 0, defense = 1, speed = 0, note = "社会性が少し残っている。" };
            gears["local_clothes"] = new Gear { id = "local_clothes", name = "地元の上着", kind = "衣服", attack = 0, defense = 1, speed = 1, note = "道に迷いにくい。" };
            gears["coat"] = new Gear { id = "coat", name = "研究者のコート", kind = "祭具", attack = 0, defense = 0, speed = 0, note = "正気を保つポケットが多い。" };
            gears["charm"] = new Gear { id = "charm", name = "家紋入りのお守り", kind = "祭具", attack = 0, defense = 2, speed = 0, note = "三河の気配を帯びる。" };
            gears["safety"] = new Gear { id = "safety", name = "安全靴", kind = "作業具", attack = 0, defense = 3, speed = -1, note = "工場ラインに踏みとどまる。" };
            gears["raincoat"] = new Gear { id = "raincoat", name = "金鯱の雨合羽", kind = "祭具", attack = 0, defense = 3, speed = 1, note = "水難に強いが、鯱に見られる。" };
            gears["morning_badge"] = new Gear { id = "morning_badge", name = "モーニング徽章", kind = "装飾", slot = "装飾品", attack = 0, defense = 0, speed = 1, luck = 2, note = "朝の契約で出目が少し軽くなる。" };
            gears["black_ticket"] = new Gear { id = "black_ticket", name = "黒い片道切符", kind = "装飾", slot = "装飾品", attack = 1, defense = 0, speed = 2, luck = 0, note = "終電と夢に少し強い。" };
            gears["sealed_coin"] = new Gear { id = "sealed_coin", name = "封蝋の古銭", kind = "装飾", slot = "装飾品", attack = 0, defense = 1, speed = 0, luck = 3, note = "失敗の気配をわずかに曲げる。" };
            gears["charm"].slot = "装飾品";
            foreach (var gear in gears.Values)
            {
              if (string.IsNullOrEmpty(gear.slot))
                    gear.slot = gear.attack > 0 ? "武器" : "防具";
                if (string.IsNullOrEmpty(gear.setTag))
                    gear.setTag = gear.kind;
                ApplyGearRarity(gear);
            }
            AddCharacter("traveler", "旅行者", "標準型", "危険選択肢を一度だけ察知する。", 0, 20, 5, 4, 3, 4, 5, 20, 3, 1200, 2, 2, 2, 0, 0, "umbrella", "jacket", 1f);
            AddCharacter("worker", "出張会社員", "生存型", "名駅・工場系イベントに強い。", 0, 23, 5, 3, 4, 3, 3, 22, 4, 1600, 2, 2, 5, 0, 0, "pcbag", "suit", 1f);
            AddCharacter("local", "地元出身者", "探索型", "名古屋めしの副作用を軽減する。", 0, 20, 5, 4, 2, 5, 7, 20, 2, 1000, 6, 5, 2, 0, 0, "ticket", "local_clothes", 1f);
            AddCharacter("occult", "オカルト研究者", "神話型", "隠し選択肢が見えるが汚染されやすい。", 0, 16, 8, 2, 1, 4, 4, 26, 3, 900, 3, 2, 1, 5, 1, "recorder", "coat", 1.15f);
            AddCharacter("samurai", "三河武士の末裔", "解放: 戦闘型", "連打バトルで押し切る強力な人物。", 35, 28, 5, 7, 4, 4, 3, 19, 3, 900, 3, 5, 2, 0, 0, "wood_sword", "charm", 1f);
            AddCharacter("mechanic", "工場の整備士", "解放: 防御型", "機械系の怪異に強い。", 55, 30, 6, 5, 7, 3, 2, 20, 4, 1100, 2, 3, 8, 0, 0, "wrench", "safety", 1f);
            AddCharacter("shachi_seen", "金鯱に見られた者", "解放: 神話型", "水難と迷宮化を一度だけ無効化する。", 90, 26, 8, 7, 7, 6, 4, 14, 3, 800, 4, 4, 4, 5, 3, "shachi_dagger", "raincoat", 1.25f);
            AddCharacter("atsuta_miko", "熱田の巫覡", "解放: 封印型", "旧神の封じ縄を読み、SANを代償に危険を縛る。", 130, 27, 9, 6, 6, 5, 5, 24, 3, 1000, 5, 4, 3, 6, 2, "charm", "coat", 1.28f);
            AddCharacter("seto_potter", "瀬戸の窯守", "解放: 防具型", "陶片の守りを重ね、長期探索に強い。", 180, 32, 7, 6, 9, 3, 4, 22, 4, 1200, 4, 5, 4, 3, 1, "wrench", "raincoat", 1.32f);
            AddCharacter("toyohashi_conductor", "豊橋の終電車掌", "解放: 速度型", "終電の隙間を走り、攻撃ゲージが溜まりやすい。", 240, 29, 8, 7, 5, 8, 5, 21, 3, 1300, 5, 3, 5, 4, 2, "ticket", "local_clothes", 1.36f);
            AddCharacter("gamagori_diver", "蒲郡の潜水者", "解放: 深海型", "水底の星図を読み、神話理解と報酬が伸びる。", 320, 34, 10, 8, 6, 4, 4, 20, 4, 1400, 4, 4, 5, 7, 3, "recorder", "raincoat", 1.42f);
            AddCharacter("arimatsu_weaver", "有松の絞り師", "解放: 幸運型", "分岐の糸目を見つけ、LUK判定に強い。", 420, 30, 9, 7, 5, 6, 10, 22, 3, 1500, 7, 4, 4, 5, 2, "ticket", "charm", 1.45f);
            AddCharacter("inuyama_mask", "犬山の面打ち", "解放: 変相型", "面を替えて敵の狙いを逸らす。", 540, 35, 8, 8, 7, 5, 6, 18, 4, 1500, 5, 5, 5, 5, 3, "wood_sword", "coat", 1.5f);
            AddCharacter("tsuruma_librarian", "鶴舞の禁書司書", "解放: 禁書型", "禁書目録を読めるが、汚染の進みも速い。", 700, 28, 12, 8, 5, 5, 5, 28, 3, 1100, 5, 4, 3, 10, 5, "recorder", "coat", 1.62f);
            AddCharacter("centrair_agent", "セントレア境界職員", "解放: 終盤型", "空港境界を扱い、撤退とボス戦準備に強い。", 900, 38, 12, 10, 8, 7, 7, 24, 4, 1800, 6, 5, 7, 8, 4, "shachi_dagger", "raincoat", 1.75f);
            AddScenes();
            AddStageExpansion();
            AddEnemies();
        }
        void AddCharacter(string id, string name, string subtitle, string description, int unlockCost, int hp, int mp, int attack, int defense, int speed, int luck, int sanity, int hunger, int money, int local, int miso, int machine, int myth, int corruption, string weapon, string armor, float reward)
        {
            characters[id] = new CharacterDef
            {
                id = id,
                name = name,
                subtitle = subtitle,
                description = description,
                unlockCost = unlockCost,
                stats = new Stats
                {
                    maxHp = hp,
                    hp = hp,
                    maxMp = mp,
                    mp = mp,
                    attack = attack,
                    defense = defense,
                    speed = speed,
                    luck = luck,
                    maxSanity = sanity,
                    sanity = sanity,
                    hunger = hunger,
                    money = money,
                    localKnowledge = local,
                    misoResistance = miso,
                    machineAptitude = machine,
                    mythosKnowledge = myth,
                    mythosCorruption = corruption
                },
                weapon = weapon,
                armor = armor,
                rewardRate = reward,
                ability = description
            };
        }
        void AddStageExpansion()
        {
            scenes["nagoya_after_battle"] = new SceneDef
            {
                id = "nagoya_after_battle",
                title = "異界愛知ステージ選択",
                area = "名駅地下 / 周回本線",
                image = "station",
                portrait = "event_subway_child",
                text = "地下街の床に、五つの路線図が浮かぶ。\n\nこの周回はステージを越えて空港へ向かう。各ステージの最後には、土地の怪異を束ねるボスが待っている。",
                choices =
                {
                   new Choice { label = "STAGE 1\n名駅地下回廊へ", next = "stage1_hub" },
                    new Choice { label = "準備してから進む", next = "station_rest" },
                    new Choice { label = "旧ルートへ寄り道", next = "castle_gate" }
                }
            };
            string[] titles = { "名駅地下回廊", "尾張異聞街道", "三河発酵迷宮", "知多海底線", "空港境界区" };
            string[] areas = { "名古屋", "尾張", "三河", "知多・蒲郡", "セントレア" };
            string[] images = { "station", "castle", "miso", "gamagori", "airport" };
            string[] portraits = { "event_subway_child", "event_shachi_avatar", "event_miso_voice", "event_gamagori_diver", "event_gate_inspector" };
            string[] bossIds = { "stage_boss_1", "stage_boss_2", "stage_boss_3", "stage_boss_4", "stage_boss_5" };
            for (int stage = 0; stage < 5; stage++)
            {
                int stageNo = stage + 1;
                string hub = "stage" + stageNo + "_hub";
                string first = "stage" + stageNo + "_event_0";
                scenes[hub] = new SceneDef
                {
                    id = hub,
                    title = "STAGE " + stageNo + ": " + titles[stage],
                    area = areas[stage] + " / 開始地点",
                    image = images[stage],
                    portrait = portraits[stage],
                    text = "ここから十の出来事を越える。途中の選択でHP、所持金、LUK、SAN、神話理解が削られ、また強くなる。\n\n最後にはステージボスが待つ。",
                    choices =
                    {
                        new Choice { label = "探索を始める", next = first },
                        new Choice { label = "息を整える\nHP+3/SAN+1", next = first, effect = r => { r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 3); r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); } },
                        new Choice { label = "不吉な近道\n神話+1/SAN-2", next = first, effect = r => { r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 2); } }
                    }
                };
                for (int index = 0; index < 10; index++)
                    AddStageEventScene(stageNo, index, titles[stage], areas[stage], images[stage], portraits[stage], bossIds[stage]);
            }
        }
       void AddStageEventScene(int stageNo, int index, string stageTitle, string area, string image, string portrait, string bossId)
        {
            string id = "stage" + stageNo + "_event_" + index;
            string next = index < 9 ? "stage" + stageNo + "_event_" + (index + 1) : "stage" + stageNo + "_boss_gate";
            int eventNo = index + 1;
            string motif = StageEventMotif(stageNo, index);
            string enemyId = "stage" + stageNo + "_enemy_" + eventNo;
            scenes[id] = new SceneDef
            {
                id = id,
               title = "STAGE " + stageNo + "-" + eventNo + ": " + StageEventTitle(stageNo, index),
                area = area + " / " + stageTitle,
                image = image,
                portrait = portrait,
                text = motif + "\n\n土地の癖を読めば、怪異は少しだけ遅れる。SANが低いほど神話的な選択は強く、危険になる。",
                choices =
                {
                    new Choice { label = "慎重に処理する\nSAN+1/所持金-80", next = next, effect = r => { if (r.stats.money >= 80) { r.stats.money -= 80; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); } else { r.stats.hp = Math.Max(1, r.stats.hp - 2); r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); } StageReward(r, stageNo, index, false); } },
                    new Choice { label = "踏み込む\nLUK判定", next = next, effect = r => { int target = 12 + stageNo + index / 3; int roll = RollLuckDiceAgainst(target); if (roll >= target) { r.stats.luck += 1; r.stats.money += 80 + stageNo * 30; StageReward(r, stageNo, index, true); } else { r.stats.hp = Math.Max(1, r.stats.hp - (1 + stageNo)); r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } } },
                    new Choice { label = "神話的に解く\n神話/SAN", next = next, condition = r => r.stats.mythosKnowledge >= stageNo - 1, disabledReason = "神話理解が足りない", effect = r => { r.stats.mythosKnowledge += 1; r.stats.mythosCorruption += index % 3 == 0 ? 1 : 0; r.stats.sanity = Math.Max(0, r.stats.sanity - (2 + stageNo / 2)); StageReward(r, stageNo, index, true); } },
                    new Choice { label = "怪異を迎撃する", battle = enemyId, effect = r => { r.battleReturnScene = next; } }
                }
            };
            if (index == 9)
            {
                string afterBoss = stageNo < 5 ? "stage" + (stageNo + 1) + "_hub" : "airport_gate";
                scenes[next] = new SceneDef
                {
                    id = next,
                    title = "STAGE " + stageNo + ": ボス前",
                    area = area + " / 境界核",
                    image = image,
                    portrait = portrait,
                    text = "十の出来事が一つの影へ集まり、ステージボスの形を取った。\n\n最後の準備には所持金180が必要。足りなければ、準備せずにボスへ挑むしかない。",
                    choices =
                    {
                        new Choice { label = "ボスに挑む", battle = bossId, effect = r => { r.battleReturnScene = afterBoss; } },
                        new Choice { label = "最後の準備\n所持金-180/HP+4/SAN+2", next = next, condition = r => r.stats.money >= 180, disabledReason = "所持金180が必要", effect = r => { r.stats.money -= 180; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 4); r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); } }
                    }
                };
            }
        }
        string StageEventMotif(int stageNo, int index)
        {
            string[,] motifs =
            {
                {
                    "名駅地下の柱番号が一つずつ欠け、消えた番号だけが足元のタイルに浮かぶ。",
                    "きしめん屋の券売機が、まだ注文していない一杯の食券を吐き出す。",
                    "大名古屋ビルヂングのガラスに、あなたより半歩遅い通行人が三人映る。",
                    "大須の閉じたシャッターに、値札の形をした神話文字が滲む。",
                    "地下街の案内図で、現在地だけが名古屋港の水面へ沈んでいく。",
                    "ナナちゃん人形の足元にいる案内人が、あなたの本名と次の改札を知っている。",
                    "久屋大通のベンチだけが、人の心臓の温度で温かい。",
                    "熱田へ続く道端の小さな祠に、サイコロと古いmanacaが供えられている。",
                    "東山線の風の奥で、ステージボスの呼吸だけが駅名標を揺らす。",
                    "名駅へ戻る通路の直前で、ユニモールの出口が一つだけ地図から消える。"
                },
                {
                    "名古屋城の堀に映った金鯱が、空ではなく水底へ向かって泳いでいる。",
                    "本丸御殿の襖絵から、濡れた魚の鱗だけが床へ落ちる。",
                    "尾張徳川の古い印章が、あなたの影にだけ朱を押す。",
                    "能楽堂の静かな拍子に合わせて、堀の水面が一拍遅れて膨らむ。",
                    "清洲越しの古い道筋が、地図上で城の井戸へ吸い込まれる。",
                    "城下の土産物屋に立つ店員が、買う前から釣銭を濡らしている。",
                    "堀端の休憩椅子に座ると、石垣の中から水難者の鼓動が返る。",
                    "金鯱横丁の小さな祭壇に、尾張の小判と濡れたサイコロが並ぶ。",
                    "天守のない空白から、鯱ではないものの呼吸が水音で聞こえる。",
                    "正門へ戻る道が一つ消え、代わりに海の匂いがする暗渠が開く。"
                },
                {
                    "岡崎の味噌蔵で、桶の木目が家康公の印ではなくあなたの死亡時刻を刻む。",
                    "八丁味噌の重石が一つ浮き、下からまだ発酵していない声が漏れる。",
                    "矢作川の風に乗って、赤味噌の香りだけが逆流してくる。",
                    "三河武士の古い旗印に、味噌樽の輪郭をした神話文字が滲む。",
                    "蔵の温度計が、現在地ではなく未来のSANを測っている。",
                    "岡崎公園の案内人が、あなたの戦績を味噌の熟成年数で呼ぶ。",
                    "休憩所の木椅子から、桶を叩く低い音が心臓のように返る。",
                    "小さな社に、赤味噌を塗ったサイコロと家康印の紙片が供えられている。",
                    "蔵の奥で、ステージボスの声だけが三度発酵して聞こえる。",
                    "八丁蔵通りの出口が一つ消え、代わりに黒い桶の内側へ続く戸が開く。"
                },
                {
                    "蒲郡の海面に、ラグーナの観覧車が逆さまに沈んだまま回っている。",
                    "竹島橋の欄干から、濡れた領収書と貝殻が同じ枚数だけ落ちる。",
                    "知多の海沿い道路で、同じ漁火の影が三つ並んで追ってくる。",
                    "常滑焼の招き猫の背に、潮でふやけた神話文字が浮かぶ。",
                    "三河湾の地図で、現在地だけが海底の星図へ沈む。",
                    "蒲郡の土産物屋に立つ案内人が、あなたの名前を潮位表で読んでいる。",
                    "海辺の休憩所の椅子だけが、深海生物の体温でぬるい。",
                    "西尾の茶箱を流用した小祭壇に、抹茶色のサイコロと貝殻が供えられている。",
                    "湾の向こうで、ステージボスの呼吸だけが波の高さを変えている。",
                    "堤防の出口が一つ消え、代わりに濡れた観光案内板が海へ続く。"
                },
                {
                    "セントレアの出発案内に、存在しない便名とあなたの最終搭乗時刻が表示される。",
                    "動く歩道の終端で、未来の搭乗券と噛まれた手荷物タグが吐き出される。",
                    "スカイデッキのガラスに、同じ飛行機影が三つ重なって静止している。",
                    "免税店の閉じたシャッターに、航空路線図の形をした神話文字が滲む。",
                    "空港の現在地マップで、ターミナルだけが地球の外側へ沈んでいく。",
                    "保安検査場の案内人が、あなたの名前を搭乗拒否リストから読み上げる。",
                    "搭乗口の椅子だけが、まだ離陸していない機体の心音で温かい。",
                    "展望風呂へ向かう通路の小祭壇に、黒いサイコロと割れた翼のピンが供えられている。",
                    "滑走路の向こうで、ステージボスの呼吸だけが誘導灯を消していく。",
                    "到着口の直前で戻り道が一つ消え、代わりに窓の外側へ続く通路が開く。"
                }
            };
            return motifs[stageNo - 1, index];
        }
        string StageEventTitle(int stageNo, int index)
        {
            string[,] names =
            {
                { "名駅柱番号", "きしめん食券", "ビル鏡の三重影", "大須値札文字", "沈む地下街地図", "ナナちゃん下の案内人", "久屋の鼓動椅子", "熱田のmanaca供物", "東山線の呼吸", "消えるユニモール" },
                { "堀の金鯱影", "本丸御殿の鱗", "尾張徳川印", "能楽堂の水拍子", "清洲越しの井戸道", "濡れた城下釣銭", "石垣の鼓動椅子", "金鯱横丁の供物", "天守空白の呼吸", "暗渠へ戻る門" },
                { "岡崎味噌蔵時計", "八丁味噌の浮き重石", "矢作川の逆香", "三河旗印文字", "未来SAN温度計", "熟成年数の案内人", "桶鳴りの休憩椅子", "家康印の供物", "三度発酵する声", "黒桶の出口" },
                { "逆さ観覧車", "竹島橋の濡れ札", "知多漁火の三重影", "常滑招き猫文字", "三河湾の海底地図", "潮位表の案内人", "海辺のぬるい椅子", "西尾茶箱の供物", "湾を変える呼吸", "濡れた観光案内板" },
                { "存在しない便名", "噛まれた手荷物タグ", "スカイデッキ三重影", "免税店路線文字", "外側へ沈む地図", "搭乗拒否の案内人", "搭乗口の心音椅子", "展望風呂前の供物", "誘導灯を消す呼吸", "窓外へ続く到着口" }
            };
            return names[stageNo - 1, index];
        }
        void StageReward(RunState r, int stageNo, int index, bool bold)
        {
            int type = (stageNo + index) % 5;
            if (type == 0) r.stats.attack += bold ? 1 : 0;
            else if (type == 1) r.stats.defense += bold ? 1 : 0;
            else if (type == 2) r.stats.speed += bold ? 1 : 0;
            else if (type == 3) r.stats.localKnowledge += 1;
            else r.npcAirport += stageNo >= 4 ? 1 : 0;
            if (bold && r.stats.sanity <= r.stats.maxSanity / 2)
                r.stats.mythosKnowledge += 1;
        }
        void AddScenes()
        {
            scenes["nagoya_start"] = new SceneDef
            {
                id = "nagoya_start",
                title = "名駅の底",
                area = "名古屋中心部",
                image = "station",
                text = "あなたは名駅の地下で目を覚ました。\n\n白すぎる照明がどこまでも続いている。案内板には「出口」と書かれているが、矢印はすべて内側を向いている。\n\n遠くで、きしめんをすする音がした。それは食事の音ではない。何かが、長いものをゆっくりと引きずっている音だった。",
                choices =
                {
                    new Choice { label = "案内板を見る", next = "station_sign", effect = r => { r.stats.sanity -= 1; } },
                    new Choice { label = "きしめん屋へ入る", next = "kishimen_oracle", condition = r => r.stats.money >= 300, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 300; r.stats.hunger = Math.Max(0, r.stats.hunger - 1); } },
                    new Choice { label = "黄色い足音に備える", battle = "piyorin" }
                }
            };
            scenes["station_sign"] = new SceneDef
            {
                id = "station_sign",
                title = "内側を向く出口",
                area = "名駅地下街",
                image = "station",
                text = "案内板の矢印は、何度見てもあなたの胸を指している。\n\n小さな文字で「出口とは、帰りたい気持ちの別名です」と書かれていた。読んだ瞬間、地下街の床が一段だけ沈む。",
                choices =
                {

                  new Choice { label = "大須方面へ歩く", next = "osu", effect = r => { r.stats.localKnowledge += 1; } },
                    new Choice { label = "黄色い群れの音を追う", battle = "piyorin" },
                    new Choice { label = "地下の終電に乗る", ending = "machine_part", condition = r => r.stats.machineAptitude < 4, disabledReason = "機械適性が高い者だけ危険を察知できる" },
                    new Choice { label = "終電の行き先を読む", next = "toyota_line", condition = r => r.stats.machineAptitude >= 4, disabledReason = "機械適性が足りない" }
                }
            };
            scenes["kishimen_oracle"] = new SceneDef
            {
                id = "kishimen_oracle",
                title = "きしめん屋の予言",
                area = "名駅地下街",
                image = "kishimen",
                portrait = "event_kishimen_owner",
                text = "湯気の向こうに、顔の見えない店主がいる。\n\n「城へ行くなら鯱を見るな。岡崎へ行くなら樽の声を聞くな。空港へ行くなら、荷物を忘れろ」\n\n麺は長すぎて、どんぶりの底へ届いていない。",
                choices =
                {
                    new Choice { label = "予言を手帳に写す", next = "nagoya_after_battle", effect = r => { r.stats.mythosKnowledge += 1; } },
                    new Choice { label = "完食する", next = "nagoya_after_battle", effect = r => { r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 3); r.stats.mythosCorruption += 1; } },
                    new Choice { label = "店主の顔を見る", ending = "madness" }
                }
            };
            scenes["nagoya_after_battle"] = new SceneDef
            {
                              id = "nagoya_after_battle",
                title = "地上への階段",
                area = "名古屋中心部",
                image = "station",
                text = "黄色い気配が遠ざかると、地下街の奥に階段が現れた。\n\n上からは、雨音と、巨大な魚が空気を切るような音が聞こえる。",
                choices =
                {
                    new Choice { label = "名古屋城へ向かう", next = "castle_gate", effect = r => { r.owari += 1; } },
                    new Choice { label = "岡崎へ向かう", next = "okazaki_storehouse", effect = r => { r.mikawa += 1; } },
                    new Choice { label = "鶴舞の書庫へ寄る", next = "tsuruma_archive" },
                    new Choice { label = "有松で準備する", next = "arimatsu_dye" },
                    new Choice { label = "地下休憩所で整える", next = "station_rest" },
                    new Choice { label = "常滑行きの切符を買う", next = "tokoname", condition = r => r.stats.money >= 700, disabledReason = "所持金700が必要", effect = r => { r.stats.money -= 700; } }
                }
            };
            scenes["osu"] = new SceneDef
            {
                id = "osu",
                title = "大須混沌市",
                area = "大須",
                image = "osu",
                portrait = "occult_researcher",
                text = "古着、電脳、寺社、屋台の匂いが一つの湿った夢になっている。\n\n中古ゲーム店の棚に、あなたの死因だけが書かれた攻略本が並んでいた。",
                choices =
                {
                    new Choice { label = "攻略本を読む", next = "nagoya_after_battle", effect = r => { r.stats.mythosKnowledge += 1; r.stats.sanity -= 2; } },
                    new Choice { label = "小倉トーストを買う", next = "nagoya_after_battle", condition = r => r.stats.money >= 250, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 250; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 4); r.ogura += 1; } },
                    new Choice { label = "路地市で運を買う", next = "osu_luck_market", condition = r => r.stats.money >= 200, disabledReason = "所持金が足りない" },
                    new Choice { label = "仁王門通りをまっすぐ進む", next = "castle_gate" }
                }
            };
            scenes["toyota_line"] = new SceneDef
            {
                id = "toyota_line",
                title = "無人の終電",
                area = "リニア未成線",
                image = "toyota",
                portrait = "factory_inspector",
                text = "車内広告はすべて「あなたの無駄を削ります」と書かれている。\n\nつり革が規則正しく揺れ、あなたの呼吸まで工程表に組み込もうとする。",
                choices =
               {
                    new Choice { label = "工場心臓のリズムを読む", next = "nagoya_after_battle", effect = r => { r.stats.machineAptitude += 1; r.stats.sanity -= 1; } },
                    new Choice { label = "終点まで乗る", ending = "machine_part" }
                }
            };
            scenes["station_rest"] = new SceneDef
            {
                id = "station_rest",
                title = "地下街の仮眠ベンチ",
                area = "名駅地下 / 休憩所",
                image = "station",
                portrait = "event_locker_keeper",
                text = "シャッターの下りた地下街に、まだ温かいベンチが一つだけある。\n\n休める。買える。準備できる。ただし、長く留まるほど地下街はあなたの名前を覚える。",
                choices =
                {
                    new Choice { label = "軽く仮眠する\n無料/2回まで", next = "nagoya_after_battle", condition = r => r.restsUsed < 2, disabledReason = "もう眠ると戻れなくなりそうだ", effect = r => { r.restsUsed += 1; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 5); r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); } },
                    new Choice { label = "補給食を買う\n所持金-220/HP+7", next = "nagoya_after_battle", condition = r => r.stats.money >= 220, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 220; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 7); r.stats.hunger = Math.Max(0, r.stats.hunger - 1); } },
                    new Choice { label = "ルートを確認する\n所持金-160/LUK+1", next = "nagoya_after_battle", condition = r => r.stats.money >= 160, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 160; r.stats.luck += 1; r.dangerWarnings += 1; } },
                    new Choice { label = "すぐ出発する", next = "nagoya_after_battle" }
                }
            };
            scenes["tsuruma_archive"] = new SceneDef
            {
                id = "tsuruma_archive",
                title = "鶴舞の地下書架",
                area = "鶴舞",
                image = "tsuruma",
                portrait = "event_tsuruma_librarian",
                text = "公園の地下に、閉館しない書架がある。\n\n本は背表紙をこちらに向けず、すべてページの断面を見せて並んでいた。",
                choices =
                {
                    new Choice { label = "禁書目録を読む\n神話+2/SAN-2", next = "nagoya_after_battle", effect = r => { r.stats.mythosKnowledge += 2; r.stats.sanity = Math.Max(0, r.stats.sanity - 2); } },
                    new Choice { label = "古地図を写す\n空港知識+1", next = "sakae_neon_crossing", effect = r => { r.stats.localKnowledge += 1; r.npcAirport += 1; } },
                    new Choice { label = "索引犬に追われる", battle = "index_hound" },
                    new Choice { label = "閲覧席で休む", next = "tsuruma_rest" }
                }
            };
          scenes["tsuruma_rest"] = new SceneDef
            {
                id = "tsuruma_rest",
                title = "鶴舞の閲覧席",
                area = "鶴舞 / 休憩所",
                image = "tsuruma",
                portrait = "event_tsuruma_librarian",
                text = "閲覧席には湯気の立つ紙コップと、誰かが残した栞がある。\n\n栞には『ここまで読んだなら、少し休め』と書かれていた。",
                choices =
                {
                    new Choice { label = "紙コップを飲む\nHP+4/SAN+1", next = "tsuruma_archive", effect = r => { r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 4); r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); } },
                    new Choice { label = "栞を買う\n所持金-240/神話+1", next = "sakae_neon_crossing", condition = r => r.stats.money >= 240, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 240; r.stats.mythosKnowledge += 1; } },
                    new Choice { label = "書架へ戻る", next = "tsuruma_archive" }
                }
            };
            scenes["sakae_neon_crossing"] = new SceneDef
            {
                id = "sakae_neon_crossing",
                title = "栄の逆さ交差点",
                area = "栄",
                image = "sakae",
                portrait = "event_sakae_broker",
                text = "ネオンは地面から空へ落ち、交差点の中央で交通標識が祈っている。\n\nここでは信号を守るほど遠回りになる。",
                choices =
                {
                    new Choice { label = "青で止まる\nLUK+1", next = "nagoya_after_battle", effect = r => { r.stats.luck += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } },
                    new Choice { label = "黒いタクシーに乗る\n所持金-420", next = "handa_canal", condition = r => r.stats.money >= 420, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 420; r.npcAirport += 1; } },
                    new Choice { label = "ネオン裏の休憩室", next = "sakae_rest" },
                    new Choice { label = "大須方面へ抜ける", next = "osu" }
                }
            };
            scenes["sakae_rest"] = new SceneDef
            {
                id = "sakae_rest",
                title = "ネオン裏の休憩室",
                area = "栄 / 休憩所",
                image = "sakae",
                portrait = "event_cafe_server",
                text = "小さな休憩室には自販機と古いソファがある。\n\n自販機のボタンは『水』『食事』『明日の運』の三つだけだ。",
                choices =
                {
                   new Choice { label = "水を買う\n所持金-80/SAN+1", next = "sakae_neon_crossing", condition = r => r.stats.money >= 80, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 80; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); } },
                    new Choice { label = "食事を買う\n所持金-260/HP+8", next = "sakae_neon_crossing", condition = r => r.stats.money >= 260, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 260; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 8); r.stats.hunger = Math.Max(0, r.stats.hunger - 2); } },
                    new Choice { label = "明日の運を買う\n所持金-300/LUK+2", next = "sakae_neon_crossing", condition = r => r.stats.money >= 300, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 300; r.stats.luck += 2; } },
                    new Choice { label = "出発する", next = "sakae_neon_crossing" }
                }
            };
            scenes["osu_luck_market"] = new SceneDef
            {
                id = "osu_luck_market",
                title = "路地裏の幸運市",
                area = "大須",
                image = "osu",
                portrait = "event_sakae_broker",
                text = "屋台の奥で、明日の偶然が小瓶に詰められて売られている。\n\n安い瓶ほどよく光る。高い瓶ほど、中で何かがこちらを見返す。",
                choices =
                {
                    new Choice { label = "幸運瓶を買う\n所持金-300/LUK+2", next = "nagoya_after_battle", condition = r => r.stats.money >= 300, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 300; r.stats.luck += 2; } },
                    new Choice { label = "値切って買う\nLUK判定", next = "nagoya_after_battle", effect = r => { if (RollLuckDiceAgainst(15) >= 15) { r.stats.money = Math.Max(0, r.stats.money - 120); r.stats.luck += 2; } else { r.stats.money = Math.Max(0, r.stats.money - 120); r.stats.luck = Math.Max(0, r.stats.luck - 1); r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } } },
                    new Choice { label = "瓶を割って逃げる", battle = "index_hound" }
                }
            };
            scenes["castle_gate"] = new SceneDef
            {
                id = "castle_gate",
                title = "金鯱の寝所",
                area = "名古屋城",
                image = "castle",
                portrait = "shachi_avatar",
                text = "城は雨に濡れている。天守の上で、二つの金鯱が眠ったふりをしていた。\n\n片方の尾が、あなたの選択肢をなぞる。",
                choices =
                {
                    new Choice { label = "鯱に帰還を願う", next = "tokoname", effect = r => { r.shachiGaze += 2; r.stats.mythosCorruption += 1; } },
                    new Choice { label = "鱗を拾う", next = "castle_scale", effect = r => { r.shachiGaze += 1; r.stats.attack += 1; } },
                    new Choice { label = "犬山方面へ迂回する", next = "inuyama_mask_market", effect = r => { r.stats.localKnowledge += 1; } },
                    new Choice { label = "正面から見上げる", ending = "madness", condition = r => r.stats.sanity < 10, disabledReason = "正気度が高いうちは目を逸らせる" }
                }
            };
            scenes["castle_scale"] = new SceneDef
            {
                id = "castle_scale",
                title = "濡れた鱗",
                area = "名古屋城",
                image = "castle",
                portrait = "shachi_avatar",
                text = "鱗は金色ではなく、深い水の色をしていた。\n\n握ると、どこか遠くで搭乗案内が鳴る。",
                choices =
                {
                    new Choice { label = "岡崎で封蝋を探す", next = "okazaki_storehouse" },
                    new Choice { label = "瀬戸で鱗を焼き直す", next = "seto_kiln_path" },
                    new Choice { label = "空港へ急ぐ", next = "tokoname", condition = r => r.npcAirport >= 1 || r.stats.localKnowledge >= 2, disabledReason = "空港への近道をまだ掴めていない" }
                }
            };
            scenes["okazaki_storehouse"] = new SceneDef
            {
                id = "okazaki_storehouse",
                title = "八丁の眠れる樽",
                area = "岡崎",
                image = "okazaki",
                portrait = "miso_voice",
                text = "味噌蔵の奥は地下神殿になっていた。\n\n樽の中から、あなたがまだ死んでいない周回の声がする。聞けば出口がわかる。聞きすぎれば、戻ってこられない。",
                choices =
                {
                    new Choice { label = "樽の声と戦う", battle = "miso_voice" },
                    new Choice { label = "封蝋を額に押す", next = "okazaki_after_battle", condition = r => r.stats.misoResistance >= 4, disabledReason = "味噌耐性が足りない", effect = r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); r.stats.mythosKnowledge += 1; } },
                    new Choice { label = "樽に耳をつけ続ける", ending = "miso_sink" }
                }
            };
            scenes["okazaki_after_battle"] = new SceneDef
            {
                id = "okazaki_after_battle",
                title = "熟成された道",
                area = "岡崎",
                image = "okazaki",
                portrait = "miso_voice",
                text = "樽の声は静かになった。\n\n床に浮かんだ赤茶色の地図は、常滑と海上空港を指している。地図の端では、金鯱が小さく呼吸していた。",
                choices =
                {
                    new Choice { label = "半田の運河へ向かう", next = "handa_canal" },
                    new Choice { label = "蒲郡の逆潮を読む", next = "gamagori_tide_map" },
                    new Choice { label = "常滑へ向かう", next = "tokoname" },
                    new Choice { label = "もう一度だけ樽を見る", ending = "miso_sink" }
                }
            };
            scenes["arimatsu_dye"] = new SceneDef
            {
                id = "arimatsu_dye",
                title = "有松の分岐染め",
                area = "有松",
                image = "arimatsu",
                portrait = "event_arimatsu_weaver",
                text = "絞り染めの布には、まだ選んでいない道が白く残っている。\n\n職人は『ほどくほど遠回りになる。けれど帰り道は増える』と言った。",
                choices =
                {
                    new Choice { label = "分岐糸をほどく\nLUK+1", next = "nagoya_after_battle", effect = r => { r.stats.luck += 1; r.dangerWarnings += 1; } },
                    new Choice { label = "染め布を買う\n所持金-260/防御+1", next = "tokoname", condition = r => r.stats.money >= 260, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 260; r.stats.defense += 1; r.stats.localKnowledge += 1; } },
                    new Choice { label = "絞り茶屋で休む", next = "arimatsu_rest" },
                    new Choice { label = "知立の人形芝居へ寄る", next = "chiryu_puppet_stage" }
                }
            };
            scenes["arimatsu_rest"] = new SceneDef
            {
                id = "arimatsu_rest",
                title = "有松の絞り茶屋",
                area = "有松 / 休憩所",
                image = "arimatsu",
                portrait = "event_arimatsu_weaver",
                text = "古い茶屋の暖簾は、入るたびに違う模様へ絞られている。\n\nここでは休むだけでなく、道そのものを縫い直せる。",
                choices =
                {
                    new Choice { label = "抹茶を飲む\n所持金-180/HP+5", next = "arimatsu_dye", condition = r => r.stats.money >= 180, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 180; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 5); } },
                    new Choice { label = "分岐札を買う\n所持金-260/危険察知+2", next = "tokoname", condition = r => r.stats.money >= 260, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 260; r.dangerWarnings += 2; } },
                    new Choice { label = "針箱を借りる\n防御+1/SAN-1", next = "chiryu_puppet_stage", effect = r => { r.stats.defense += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } },
                    new Choice { label = "茶屋を出る", next = "arimatsu_dye" }
               }
            };
            scenes["inuyama_mask_market"] = new SceneDef
            {
                id = "inuyama_mask_market",
                title = "犬山の面売り",
                area = "犬山",
                image = "inuyama",
                portrait = "event_inuyama_mask",
                text = "面売りの屋台では、買った面ではなく、買わなかった面が後をついてくる。\n\n城の方角から、金色の雨音が遠く聞こえた。",
                choices =
                {
                    new Choice { label = "笑っていない面を買う\n所持金-220/LUK+1", next = "castle_scale", condition = r => r.stats.money >= 220, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 220; r.stats.luck += 1; } },
                    new Choice { label = "面を見返す\nSAN-2/神話+1", next = "seto_kiln_path", effect = r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); r.stats.mythosKnowledge += 1; } },
                    new Choice { label = "犬山橋を渡る", next = "arimatsu_dye" }
                }
            };
            scenes["seto_kiln_path"] = new SceneDef
            {
                id = "seto_kiln_path",
                title = "瀬戸の白い窯道",
                area = "瀬戸",
                image = "seto",
                portrait = "event_seto_potter",
                text = "白い窯がずらりと並び、どの窯もあなたの装備を焼き直したがっている。\n\n火に近づけば強くなる。近づきすぎれば、先に身体が焼ける。",
                choices =
                {
                    new Choice { label = "窯熱で鍛える\nHP-3/攻撃+1", next = "tokoname", effect = r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); r.stats.attack += 1; } },
                    new Choice { label = "陶片の盾を拾う\n防御+1", next = "handa_canal", effect = r => { r.stats.defense += 1; r.stats.localKnowledge += 1; } },
                    new Choice { label = "窯の底を覗く", battle = "kiln_crawler" }
                }
            };
            scenes["chiryu_puppet_stage"] = new SceneDef
            {
                id = "chiryu_puppet_stage",
                title = "知立の糸なし芝居",
                area = "知立",
                image = "chiryu",
                portrait = "event_chiryu_puppeteer",
               text = "舞台の人形には糸がない。糸がないのに、あなたの指だけが勝手に動く。\n\n拍手をすれば終わる。拍手をしなければ、役が回ってくる。",
                choices =
                {
                    new Choice { label = "役を奪う\n攻撃+1/SAN-1", next = "okazaki_storehouse", effect = r => { r.stats.attack += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } },
                    new Choice { label = "人形の糸を切る\nLUK判定", next = "arimatsu_dye", effect = r => { if (RollLuckDiceAgainst(14) >= 14) r.stats.luck += 2; else r.stats.hp = Math.Max(1, r.stats.hp - 2); } },
                    new Choice { label = "客席から逃げる", battle = "puppet_thing" }
                }
            };
            scenes["handa_canal"] = new SceneDef
            {
                id = "handa_canal",
                title = "半田の黒い運河",
                area = "半田",
                image = "handa",
                portrait = "event_handa_brewer",
                text = "運河の水面に、まだ払っていない代金が浮かんでいる。\n\n酒蔵の戸口から、発酵した時間の匂いが流れてきた。",
                choices =
                {
                    new Choice { label = "黒酢を飲む\nHP+4/所持金-180", next = "tokoname", condition = r => r.stats.money >= 180, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 180; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 4); r.stats.misoResistance += 1; } },
                    new Choice { label = "運河沿いを急ぐ\nHP-2/探索短縮", next = "tokoname", effect = r => { r.stats.hp = Math.Max(1, r.stats.hp - 2); r.npcAirport += 1; } },
                    new Choice { label = "蔵の休憩所へ入る", next = "handa_rest" },
                    new Choice { label = "蒲郡へ流れを追う", next = "gamagori_tide_map" }
                }
            };
            scenes["handa_rest"] = new SceneDef
            {
                id = "handa_rest",
                title = "半田の蔵休み",
                area = "半田 / 休憩所",
                image = "handa",
                portrait = "event_handa_brewer",
                text = "酒蔵の奥に、旅人用の板間がある。\n\n発酵の音が一定のリズムで鳴り、傷の痛みを少しだけ別の時間へ移してくれる。",
                choices =
                {
                    new Choice { label = "甘酒を飲む\n所持金-160/HP+6", next = "handa_canal", condition = r => r.stats.money >= 160, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 160; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 6); } },
                    new Choice { label = "酢の湿布を貼る\nHP+3/防御+1", next = "tokoname", effect = r => { r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 3); r.stats.defense += 1; } },
                    new Choice { label = "発酵地図を読む\n神話+1/SAN-1", next = "gamagori_tide_map", effect = r => { r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } },
                    new Choice { label = "運河へ戻る", next = "handa_canal" }
                }
            };
           scenes["gamagori_tide_map"] = new SceneDef
            {
                id = "gamagori_tide_map",
                title = "蒲郡の逆潮星図",
                area = "蒲郡",
                image = "gamagori",
                portrait = "event_gamagori_diver",
                text = "海は空へ落ち、干上がった底に星図が残っている。\n\n空港へ向かう橋は、この星図の端で何度も折り畳まれていた。",
                choices =
                {
                    new Choice { label = "星図を読む\n神話+1/SAN-1", next = "airport_bridge", effect = r => { r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); r.npcAirport += 1; } },
                    new Choice { label = "潜水服を借りる\n所持金-350/HP+3", next = "tokoname", condition = r => r.stats.money >= 350, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 350; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 3); } },
                    new Choice { label = "海辺の売店で整える", next = "gamagori_rest" },
                    new Choice { label = "海底改札へ降りる", battle = "deep_one_clerk" }
                }
            };
            scenes["gamagori_rest"] = new SceneDef
            {
                id = "gamagori_rest",
                title = "蒲郡の海辺売店",
                area = "蒲郡 / 休憩所",
                image = "gamagori",
                portrait = "event_gamagori_diver",
                text = "売店には濡れていないものが一つもない。\n\n店主は潜水服のまま、温かいものと危険なものを同じ棚に並べている。",
                choices =
                {
                    new Choice { label = "温かい缶を買う\n所持金-120/SAN+2", next = "gamagori_tide_map", condition = r => r.stats.money >= 120, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 120; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); } },
                    new Choice { label = "防水札を買う\n所持金-300/空港知識+2", next = "airport_bridge", condition = r => r.stats.money >= 300, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 300; r.npcAirport += 2; } },
                    new Choice { label = "潮風で仮眠\nHP+4/SAN-1", next = "gamagori_tide_map", effect = r => { r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 4); r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } },
                    new Choice { label = "海へ戻る", next = "gamagori_tide_map" }
                }
            };
            scenes["tokoname"] = new SceneDef
            {
                id = "tokoname",
                title = "土管坂の螺旋",
                area = "常滑",
                image = "tokoname",
               portrait = "shachi_avatar",
                text = "土管坂は地中へ続く螺旋階段になっていた。\n\n招き猫の目が、あなたではなく、あなたの背後の誰かを招いている。海風の向こうに空港の灯りが見える。",
                choices =
                {
                    new Choice { label = "招き猫に礼をする", next = "airport_bridge", effect = r => { r.stats.localKnowledge += 1; } },
                    new Choice { label = "土管の中を進む", next = "airport_bridge", effect = r => { r.stats.hp -= 2; r.stats.mythosKnowledge += 1; } },
                    new Choice { label = "視線を無視する", battle = "gate_guard" }
                }
            };
            scenes["airport_bridge"] = new SceneDef
            {
                id = "airport_bridge",
                title = "海上への橋",
                area = "中部国際空港",
                image = "airport",
                portrait = "gate_inspector",
                text = "橋は満潮のたびに少しずつ短くなる。\n\n空港はすぐそこに見える。しかし滑走路の下で、巨大なものが寝返りを打った。ここから先は、帰るための施設ではなく、帰る資格を検査する迷宮だ。",
                choices =
                {
                    new Choice { label = "保安検査場へ進む", next = "airport_security" },
                    new Choice { label = "連絡通路を調べる", next = "airport_security", effect = r => { r.stats.localKnowledge += 1; r.stats.mythosKnowledge += 1; r.npcAirport += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } },
                    new Choice { label = "検査官を突破する", battle = "gate_guard" },
                    new Choice { label = "海の下を覗く", ending = "madness", condition = r => r.stats.mythosKnowledge >= 3 || r.shachiGaze >= 2, disabledReason = "まだ海の下を読む準備がない" }
                }
            };
            scenes["airport_security"] = new SceneDef
            {
                id = "airport_security",
                title = "保安検査の列",
                area = "中部国際空港",
                image = "airport",
                portrait = "event_gate_inspector",
                text = "保安検査の列は動いているのに、誰も前へ進んでいない。\n\nトレーには財布、スマホ、靴、そして覚えていない記憶を置くよう案内が出ている。",
                choices =
                {
                    new Choice { label = "所持金を申告する\n所持金-220/空港知識+1", next = "airport_baggage_maze", condition = r => r.stats.money >= 220, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 220; r.npcAirport += 1; } },
                    new Choice { label = "検査機の死角を抜ける\nLUK判定", next = "airport_baggage_maze", effect = r => { if (RollLuckDiceAgainst(15) >= 15) { r.npcAirport += 2; r.stats.luck += 1; } else { r.stats.hp = Math.Max(1, r.stats.hp - 3); r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } } },
                    new Choice { label = "制限区域ラウンジへ", next = "airport_lounge" },
                    new Choice { label = "検査官に目を合わせる", battle = "gate_guard" }
                }
           };
            scenes["airport_lounge"] = new SceneDef
            {
                id = "airport_lounge",
                title = "制限区域ラウンジ",
                area = "中部国際空港 / 休憩所",
                image = "airport",
                portrait = "event_gate_inspector",
                text = "ラウンジには誰もいない。椅子だけが、搭乗時刻を待つように整列している。\n\nカウンターには『最後の準備』と書かれたメニューが置かれていた。",
                choices =
                {
                    new Choice { label = "温かい食事\n所持金-300/HP+8", next = "airport_baggage_maze", condition = r => r.stats.money >= 300, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 300; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 8); } },
                    new Choice { label = "搭乗保険\n所持金-400/LUK+2", next = "airport_baggage_maze", condition = r => r.stats.money >= 400, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 400; r.stats.luck += 2; r.dangerWarnings += 1; } },
                    new Choice { label = "案内端末を読む\n空港知識+2/SAN-1", next = "airport_baggage_maze", effect = r => { r.npcAirport += 2; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } },
                    new Choice { label = "保安検査へ戻る", next = "airport_security" }
                }
            };
            scenes["airport_baggage_maze"] = new SceneDef
            {
                id = "airport_baggage_maze",
                title = "手荷物迷路",
                area = "中部国際空港",
                image = "airport",
                portrait = "baggage_mouth",
                text = "ターンテーブルが幾重にも絡まり、荷物があなたの名前を呼びながら回っている。\n\n受け取るべきものと、絶対に受け取ってはいけないものが同じ色をしていた。",
                choices =
                {
                    new Choice { label = "自分の荷物だけ拾う\nLUK判定", next = "airport_under_runway", effect = r => { if (RollLuckDiceAgainst(14) >= 14) { r.stats.money += 260; r.stats.luck += 1; } else { r.stats.hp = Math.Max(1, r.stats.hp - 2); progress.brokenGear.Add("噛まれた手荷物タグ"); SaveProgress(); } } },
                    new Choice { label = "荷物を捨てて走る\nHP-2", next = "airport_under_runway", effect = r => { r.stats.hp = Math.Max(1, r.stats.hp - 2); r.npcAirport += 1; } },
                    new Choice { label = "鳴いている鞄を開ける", battle = "baggage_mouth" }
                }
            };
            scenes["airport_under_runway"] = new SceneDef
            {
                id = "airport_under_runway",
                title = "滑走路の下",
                area = "中部国際空港",
                image = "airport",
                portrait = "window_god",
              text = "滑走路の下には、離陸しなかった飛行機の影だけが吊るされている。\n\n影の間を抜ければ搭乗ゲートだ。だが窓の外にいる小さな神が、こちらの歩数を数えている。",
                choices =
                {
                    new Choice { label = "影の隙間を走る\n速さ/LUK", next = "airport_gate", effect = r => { if (r.stats.speed + r.stats.luck >= 14) r.npcAirport += 1; else r.stats.sanity = Math.Max(0, r.stats.sanity - 2); } },
                    new Choice { label = "窓外神を見ない\n神話+1/SAN-1", next = "airport_gate", effect = r => { r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } },
                    new Choice { label = "影を一つ撃ち落とす", battle = "window_god" }
                }
            };
            scenes["airport_gate"] = new SceneDef
            {
                id = "airport_gate",
                title = "外界への搭乗",
                area = "中部国際空港",
                image = "airport",
                portrait = "gate_inspector",
                text = "搭乗券には行き先ではなく、代償が印字されていた。\n\nあなたが持ち帰れるものは一つだけだ。記憶か、正気か、異界の鍵か。",
                choices =
                {
                    new Choice { label = "記憶を捨てて帰る", ending = "return", condition = r => r.stats.sanity >= 5, disabledReason = "正気度が足りない" },
                    new Choice { label = "双鯱を調停する", ending = "true_shachi", condition = r => r.shachiGaze >= 2 && r.stats.mythosKnowledge >= 2, disabledReason = "鯱の注視と神話理解が足りない" },
                    new Choice { label = "荷物検査を受ける", ending = "airport_lost" }
                }
            };
        }
        void AddEnemies()
        {
            enemies["piyorin"] = new EnemyDef
            {
                id = "piyorin",
                name = "ぴよりん群体",
                image = "piyorin",
                maxHp = 18,
                attack = 5,
                defense = 1,
                speed = 5,
                sanityDamage = 1,
                reward = 6,
                weakness = "打撃 / 冷静",
                portrait = "piyorin_swarm",
                intro = "黄色い柔らかな群れが、地下街の角を曲がってくる。\nかわいい。かわいいが、多すぎる。",
                victoryText = "群れは小さな羽音を残して散った。床に黄色い夢の欠片が落ちている。",
                defeatEnding = "piyorin_bad"
            };
            enemies["miso_voice"] = new EnemyDef
            {
                id = "miso_voice",
                name = "味噌樽の中の声",
                image = "miso",
                maxHp = 24,
                attack = 6,
                defense = 2,
                speed = 2,
                sanityDamage = 3,
                reward = 10,
                weakness = "味噌耐性 / 儀式具",
                portrait = "miso_voice",
                intro = "樽の中から、あなたと同じ声がする。\nそれは先に死んだあなたであり、まだ生まれていないあなたでもあった。",
                victoryText = "声は底へ沈み、代わりに空港への道が浮かんだ。",
                defeatEnding = "miso_sink"
            };
            enemies["gate_guard"] = new EnemyDef
            {
                id = "gate_guard",
                name = "空港の搭乗検査官",
                image = "airport",
                maxHp = 30,
                attack = 7,
                defense = 3,
                speed = 4,
                sanityDamage = 2,
                reward = 14,
                weakness = "神話理解 / 鯱の注視",
                portrait = "gate_inspector",
                intro = "検査官は顔のない笑みで、あなたの搭乗券を裏返した。\nそこには、あなたが忘れた死因が細かく印字されている。",
                victoryText = "検査官は無言で道を開けた。搭乗ゲートの先に、朝の匂いがする。",
                defeatEnding = "airport_lost"
            };
            AddExpandedEnemies();
            AddStageEnemies();
            AddStageSpecificEnemies();
        }
        void AddStageSpecificEnemies()
        {
            string[] images = { "station", "castle", "miso", "gamagori", "airport" };
            string[] areas = { "名駅地下", "尾張城下", "三河発酵路", "知多海底道", "常滑空港外縁" };
            string[,] names =
            {
                { "地下改札の欠番", "曲がる階段標識", "空席だけの通勤者", "コインロッカーの舌", "終電前の影", "地下水の駅員", "切符を噛むもの", "蛍光灯下の無顔者", "ホーム下の掌", "路線図の小さな影" },
                { "金鯱瓦の落胤", "城堀の首なし鯉", "有松染めの黒布", "犬山面の笑い声", "古井戸の指", "熱田剣影", "長久手の無音槍", "尾張帳簿の牙", "瓦礫の侍従", "楼門の逆さ僧" },
                { "発酵樽の耳", "味噌蔵の泡人", "岡崎影武者", "西尾茶臼の目隠し", "豊橋終電の残響", "工場検査の腕", "赤錆びの品質票", "三河湾の塩骨", "からくり糸の影人形", "蔵底の寝言布" },
                { "潮騒劇場の粘る影", "蒲郡潜水服の客", "海底路線の濁り群れ", "常滑焼の白いもの", "水圧の案内板", "船底の提灯亡者", "湾岸窓の小神", "波間の手荷物口", "深い改札係", "沈んだ観覧車の影" },
                { "黒い搭乗券の群れ", "保安検査の死角", "手荷物口の捕食者", "滑走路下の胎音", "空港窓の小神", "ゲート番号の空洞", "免税店の値札口", "時刻表の剥製", "搭乗橋の長い腕", "最後列の空席" }
            };
            string[,] weaknesses =
            {
                { "地元知識 / 標識を信じない", "速さ / 階段を数えない", "SAN維持 / 視線を返さない", "所持金管理 / 鍵を見せない", "LUK / 影を踏まない", "神話理解 / 水音を読む", "防御 / 切符を捨てる", "攻撃 / 光源を断つ", "速さ / ホーム端を離れる", "地元知識 / 路線を逆に辿る" },
                { "攻撃 / 鱗を割る", "SAN維持 / 水面を見ない", "LUK / 柄をほどく", "神話理解 / 面を外す", "防御 / 井戸を塞ぐ", "神話理解 / 刃筋を避ける", "防御 / 拍を外す", "所持金管理 / 署名しない", "攻撃 / 主君名を問う", "SAN維持 / 祈りを聞かない" },
                { "SAN維持 / 発酵音を遮る", "攻撃 / 泡を潰す", "防御 / 後手を取る", "神話理解 / 目を合わせない", "速さ / 発車ベル", "機械適性 / 検査票", "地元知識 / 工程表", "防御 / 塩を払う", "LUK / 糸を切る", "SAN維持 / 寝言に返事しない" },
                { "即興 / 音を立てない", "神話理解 / 水難耐性", "速さ / 群れを割る", "防御 / 冷却", "地元知識 / 潮目", "SAN維持 / 火を見ない", "神話理解 / 見ない勇気", "速さ / 噛ませない", "SAN維持 / 空港知識", "攻撃 / 軸を止める" },
                { "速さ / 読まない", "LUK / 死角を抜ける", "防御 / 名札を守る", "SAN維持 / 呼吸を合わせない", "神話理解 / 見ない勇気", "地元知識 / ゲート変更", "所持金管理 / 払わない", "SAN維持 / 時刻を忘れる", "攻撃 / 関節を断つ", "LUK / 席を譲らない" }
            };
            string[] defeatEndings = { "madness", "event_death", "miso_sink", "airport_lost", "airport_lost" };

            for (int stage = 1; stage <= 5; stage++)
            {
                for (int i = 1; i <= 10; i++)
                {
                    int s = stage - 1;
                    int n = i - 1;
                    string id = "stage" + stage + "_enemy_" + i;
                    int hp = 22 + stage * 5 + i * 2;
                    int attack = 6 + stage + i / 3;
                    int defense = 1 + stage / 2 + i % 3;
                    int speed = 2 + (stage + i * 2) % 7;
                    int sanityDamage = 1 + stage / 2 + i % 4;
                    int reward = 8 + stage * 4 + i;
                    string intro = names[s, n] + "が" + areas[s] + "の通路を塞ぐ。次の一歩を、こちらの記憶ごと奪おうとしている。";
                    string victory = names[s, n] + "はほどけ、" + areas[s] + "の道が一拍だけ現実に戻った。";
                    AddEnemy(id, names[s, n], images[s], hp, attack, defense, speed, sanityDamage, reward, weaknesses[s, n], id, intro, victory, defeatEndings[s]);
                }
            }
        }
        void AddStageEnemies()
        {
            AddEnemy("stage_enemy_1", "地下標識の首なし案内", "station", 24, 7, 2, 4, 2, 9, "地元知識 / 標識を信じない", "event_subway_child", "案内板の矢印が、首のない駅員の腕になって伸びる。", "矢印は折れ、現在地だけが地図に戻った。", "madness");
            AddEnemy("stage_enemy_2", "領収書蛭", "osu", 22, 6, 1, 6, 1, 8, "所持金管理 / 火で払う", "locker_keeper", "紙の蛭が財布へ潜り、まだ使っていない金額を吸う。", "蛭は印字ミスになって剥がれた。", "event_death");
            AddEnemy("stage_enemy_3", "三重影の通行人", "station", 26, 8, 2, 5, 3, 10, "LUK / 影を踏まない", "shadow_retainer", "同じ歩幅の影が三つ、あなたより先に交差点を渡る。", "影は信号の赤に閉じ込められた。", "madness");
            AddEnemy("stage_enemy_4", "落書き神官", "castle", 28, 8, 3, 3, 4, 11, "神話理解 / 読み飛ばす", "event_atsuta_miko", "シャッターの文字が祭服をまとい、読めと命じる。", "文字は塗料へ戻り、意味だけが残った。", "madness");
            AddEnemy("stage_enemy_5", "水没地図魚", "gamagori", 25, 7, 2, 7, 2, 10, "速さ / 海図", "deep_one_clerk", "地図から魚が跳ね、道順を水へ変えていく。", "魚は紙の染みになった。", "airport_lost");
            AddEnemy("stage_enemy_6", "名前を知る案内人", "sakae", 30, 8, 3, 4, 3, 12, "SAN維持 / 偽名", "event_sakae_broker", "案内人はあなたの本名、死因、次の選択を順に呼ぶ。", "案内人は名札だけを残して消えた。", "madness");
            AddEnemy("stage_enemy_7", "休憩椅子の心臓", "handa", 32, 9, 4, 1, 2, 11, "防御 / 座らない", "event_handa_brewer", "椅子の座面が鼓動し、休むほど深く沈めようとする。", "鼓動は止まり、短い休息だけが残った。", "event_death");
            AddEnemy("stage_enemy_8", "供物骰子の巫女", "atsuta", 27, 7, 2, 6, 4, 12, "LUK / 出目を捨てる", "event_atsuta_miko", "巫女はサイコロを三つ差し出し、外れ目を血で塗る。", "骰子は普通の骨に戻った。", "madness");
            AddEnemy("stage_enemy_9", "遠い呼吸の壁", "tokoname", 34, 10, 5, 1, 3, 13, "攻撃 / 呼吸を合わせる", "window_god", "壁全体がゆっくり呼吸し、通路を肺の中へ変える。", "壁は息を吐き切り、道が開いた。", "event_death");
            AddEnemy("stage_enemy_10", "戻り道を食うもの", "inuyama", 36, 10, 3, 5, 4, 14, "神話理解 / 振り返らない", "puppet_thing", "背後の道が咀嚼音を立てて短くなる。", "咀嚼音は遠のき、分岐が一つ戻った。", "madness");
            AddEnemy("stage_enemy_11", "SAN検札係", "airport", 38, 11, 4, 4, 5, 15, "SAN残量 / 空港知識", "event_gate_inspector", "検札係は切符ではなく正気度を読み取る。", "検札鋏は閉じ、あなたの正気は改札を抜けた。", "airport_lost");
            AddEnemy("stage_enemy_12", "神話汚染サンプル", "toyota", 40, 12, 5, 2, 5, 16, "機械適性 / 汚染隔離", "quality_golem", "試験管の中身が人型になり、検査室を歩き出す。", "サンプルはラベルへ戻った。", "machine_part");
            AddEnemy("stage_enemy_13", "黒い搭乗券の群れ", "airport", 34, 10, 2, 8, 3, 15, "速さ / 読まない", "last_train", "搭乗券が鳥のように舞い、名前の欄へ嘴を突き立てる。", "券面は白紙になり、風に散った。", "airport_lost");
            AddEnemy("stage_enemy_14", "古戦場の無音鼓", "nagakute", 42, 13, 5, 3, 2, 16, "防御 / 拍を外す", "battlefield_spear", "音のない太鼓が腹の奥だけを震わせる。", "鼓は破れ、無音の圧力が抜けた。", "event_death");
            AddEnemy("stage_enemy_15", "帰宅願望の抜け殻", "station", 44, 12, 4, 4, 6, 18, "SAN / 帰る理由", "dream_eater", "帰りたい気持ちだけが人型に剥がれ、こちらへ歩いてくる。", "抜け殻はあなたの胸へ戻り、少しだけ重くなった。", "madness");
            AddEnemy("stage_boss_1", "名駅地下の路線図母体", "station", 48, 12, 5, 4, 4, 20, "地元知識 / 迷わない意志", "locker_womb", "路線図が胎内のように広がり、すべての出口をへその緒で結ぶ。", "路線図母体は折り畳まれ、次の土地への線だけが残った。", "madness");
            AddEnemy("stage_boss_2", "尾張金鯱の影王", "castle", 58, 15, 6, 5, 4, 24, "鯱の注視 / 攻撃", "shachi_avatar", "城の影から、金鯱ではない鯱が王冠のように浮かぶ。", "影王は瓦へ戻り、空が少し低くなった。", "event_death");
            AddEnemy("stage_boss_3", "三河発酵する声塊", "miso", 64, 16, 7, 3, 6, 28, "味噌耐性 / SAN管理", "miso_voice", "声が発酵し、泡立つ肉の塊として樽から溢れる。", "声塊は沈み、空港へ続く臭いだけが残った。", "miso_sink");
            AddEnemy("stage_boss_4", "海底星図の深き監査官", "gamagori", 70, 17, 7, 5, 6, 32, "神話理解 / 水難耐性", "deep_one_clerk", "星図を背負った監査官が、海底の印鑑を押しに来る。", "監査印は割れ、空港の滑走路が星図に現れた。", "airport_lost");
            AddEnemy("stage_boss_5", "搭乗門外の小神群", "airport", 82, 19, 8, 6, 8, 40, "見ない勇気 / 神話理解", "window_god", "搭乗門の外側に、小さな神々が鈴なりになって待っている。", "小神群は搭乗時刻を失い、最後のゲートが開いた。", "airport_lost");
            AddEnemy("impossible_one", "この世のものとは思えないもの", "ending", 240, 999, 14, 32, 999, 70, "神話理解10以上 / LUK / 最初の数秒", "impossible_one", "SANが底を打つ。床、空、選択肢の文字までもが一度だけ裏返り、名付けられない輪郭が現れる。\n\nこれは通常のボスではない。攻撃を許せば、ほぼ確実に終わる。", "それは倒されたのではない。あなたという観測によって、存在できなくなった。", "impossible_death");
        }
        void AddExpandedEnemies()
        {
           AddEnemy("index_hound", "索引猟犬", "tsuruma", 20, 6, 2, 6, 3, 8, "視線を外す / 禁書理解", "index_hound", "紙片の群れが犬の形に折り畳まれ、あなたの名前だけを追跡してくる。", "猟犬はページ番号へ戻り、禁書の余白だけが残った。", "madness");
            AddEnemy("kiln_crawler", "窯這いの白いもの", "seto", 22, 7, 4, 2, 2, 9, "冷却 / 陶片防御", "kiln_crawler", "焼かれる前の土と骨が、窯の口から這い出す。", "白いものは砕け、熱だけが防具に残った。", "event_death");
            AddEnemy("last_train", "終電に乗った影", "toyohashi", 24, 7, 2, 7, 2, 9, "速さ / 発車ベル", "last_train", "誰もいない車内から、あなたの影だけが降りてくる。", "影は閉まる扉に挟まれ、夜の線路へ戻った。", "madness");
            AddEnemy("deep_one_clerk", "深きものの改札係", "gamagori", 28, 8, 3, 3, 3, 11, "神話理解 / 水難耐性", "deep_one_clerk", "濡れた制服の係員が、鱗のある手で切符を求める。", "改札鋏が錆び、潮の匂いが薄れた。", "airport_lost");
            AddEnemy("battlefield_spear", "長久手の無音槍", "nagakute", 26, 8, 3, 4, 1, 10, "防御 / 陣形読み", "battlefield_spear", "音のしない槍衾が、古戦場の霧から組み上がる。", "槍は旗の影へ戻り、攻め筋だけが残った。", "event_death");
            AddEnemy("tea_eye", "茶碗底の単眼", "nishio", 18, 5, 1, 1, 4, 7, "見ない / SAN維持", "tea_eye", "茶碗の底の眼が、湯気越しに瞬きをする。", "眼は泡になって沈み、苦味だけが残った。", "madness");
            AddEnemy("puppet_thing", "糸を持つもの", "chiryu", 27, 8, 2, 5, 3, 10, "LUK / 糸切り", "puppet_thing", "人形ではない何かが、客席の上から糸を垂らす。", "糸は切れ、舞台の床に黒く滲んだ。", "madness");
            AddEnemy("stage_polyps", "水上劇場のポリプ", "laguna", 30, 9, 3, 3, 4, 12, "即興 / 音を立てない", "stage_polyps", "見えない笛に合わせて、半透明の筒状生物が踊る。", "拍手が止み、水面に輪だけが残った。", "event_death");
            AddEnemy("locker_womb", "ロッカー胎内", "station", 25, 7, 4, 1, 2, 9, "番号記憶 / 打撃", "locker_womb", "ロッカーの内側が肉のように脈打ち、鍵穴が呼吸している。", "扉は閉じ、鍵だけが現実に残った。", "event_death");
            AddEnemy("well_tentacle", "井戸底の触腕", "castle", 34, 10, 3, 2, 4, 13, "鯱の視線 / 神話理解", "well_tentacle", "井戸の底から城より古い水の腕が伸びる。", "触腕は金鯱の影に裂かれて沈んだ。", "madness");
            AddEnemy("baggage_mouth", "荷物口の捕食者", "airport", 26, 8, 3, 5, 2, 10, "速さ / 空港知識", "baggage_mouth", "スーツケースのファスナーが歯列になり、名札を噛み砕く。", "荷物はターンテーブルの下へ落ちた。", "airport_lost");
            AddEnemy("quality_golem", "品質検査ゴーレム", "toyota", 32, 9, 5, 2, 1, 12, "機械適性 / 工程表", "quality_golem", "赤ペンと検査票でできた巨体が、不良判定を押しに来る。", "検査票は合格印で埋まり、機械音が静まった。", "machine_part");
            AddEnemy("dream_eater", "通過列車の夢喰い", "station", 24, 7, 2, 8, 3, 10, "速さ / 起床", "dream_eater", "窓に映る眠った顔が、こちらの夢を噛んでいる。", "列車は夢だけを乗せて通過した。", "madness");
            AddEnemy("sword_shadow", "草薙影", "atsuta", 38, 11, 4, 6, 4, 15, "封印 / SAN管理", "sword_shadow", "抜けない剣の影だけが、世界から少し遅れて斬りかかる。", "影は台座へ戻り、刃の輪郭が薄れた。", "event_death");
            AddEnemy("lantern_dead", "流れない灯籠死人", "tsushima", 23, 6, 2, 2, 4, 9, "数える / 火を見ない", "lantern_dead", "灯籠の火の中から、濡れた死人がこちらへ歩いてくる。", "火は川面で眠り、名札だけが燃え尽きた。", "madness");
            AddEnemy("shadow_retainer", "三河影武者", "okazaki", 36, 10, 4, 5, 2, 13, "防御 / 後手", "shadow_retainer", "あなたより一歩強い影武者が、同じ癖で構える。", "影はあなたより一歩遅れて消えた。", "event_death");
            AddEnemy("window_god", "窓外の小さな神", "airport", 42, 12, 5, 1, 5, 18, "見ない / 神話理解", "window_god", "窓の外にいるものは小さい。遠近法が壊れているだけだ。", "窓外神は搭乗時刻を過ぎ、雲の裏へ去った。", "airport_lost");
        }
        void AddEnemy(string id, string name, string image, int hp, int attack, int defense, int speed, int sanityDamage, int reward, string weakness, string portrait, string intro, string victoryText, string defeatEnding)
        {
            enemies[id] = new EnemyDef
            {
                id = id,
                name = name,
                image = image,
                maxHp = hp,
                attack = attack,
                defense = defense,
                speed = speed,
                sanityDamage = sanityDamage,
                reward = reward,
                weakness = weakness,
                portrait = portrait,
                intro = intro,
                victoryText = victoryText,
                defeatEnding = defeatEnding
          };
        }
        void LoadProgress()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                progress = JsonUtility.FromJson<Progress>(PlayerPrefs.GetString(SaveKey));
                if (progress == null)
                    progress = new Progress();
            }
            else
            {
                progress = new Progress();
            }
            NormalizeProgress();
        }
        void SaveProgress()
        {
            NormalizeProgress();
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(progress));
            PlayerPrefs.Save();
        }
        void NormalizeProgress()
        {
            if (progress == null)
                progress = new Progress();
            if (progress.unlockedCharacters == null)
                progress.unlockedCharacters = new List<string> { "traveler", "worker", "local", "occult" };
            if (progress.endings == null)
                progress.endings = new List<string>();
            if (progress.defeated == null)
                progress.defeated = new List<string>();
            if (progress.deaths == null)
                progress.deaths = new List<string>();
            if (progress.seenMonsters == null)
               progress.seenMonsters = new List<string>();
            if (progress.monsterWeaknesses == null)
                progress.monsterWeaknesses = new List<string>();
            if (progress.bossesDefeated == null)
                progress.bossesDefeated = new List<string>();
            if (progress.brokenGear == null)
                progress.brokenGear = new List<string>();
            if (progress.regretLog == null)
                progress.regretLog = new List<string>();
            if (progress.milestoneClaims == null)
                progress.milestoneClaims = new List<string>();
            if (progress.warehouseGear == null)
                progress.warehouseGear = new List<string>();
            if (progress.awakenedGear == null)
                progress.awakenedGear = new List<string>();
            if (progress.rememberedChoices == null)
                progress.rememberedChoices = new List<string>();
            progress.maxInstabilityUnlocked = Mathf.Clamp(progress.maxInstabilityUnlocked, 0, 5);
        }
        void ClearChoices()
        {
            choiceCommands.Clear();
            choicePage = 0;
            ClearChoiceVisuals();
        }
        void ClearChoiceVisuals()
        {
            EnsureChoiceSlots();
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                var button = choiceButtons[i];
                if (button == null)
                    continue;
                button.onClick.RemoveAllListeners();
                button.interactable = false;
                button.gameObject.SetActive(false);
                var text = choiceButtonLabels[i];
                if (text != null)
              {
                    text.text = "";
                    text.enabled = true;
                    text.gameObject.SetActive(true);
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(choiceRoot);
            Canvas.ForceUpdateCanvases();
        }
        void AddChoiceButton(string label, Action action, bool interactable = true)
        {
            choiceCommands.Add(new ChoiceCommand { label = label, action = action, interactable = interactable });
            RenderChoiceCommands();
        }
        void RenderChoiceCommands()
        {
            ClearChoiceVisuals();
            int total = choiceCommands.Count;
            if (total == 0)
                return;
            int pageSize = total > 4 ? 3 : 4;
            int pageCount = Mathf.Max(1, Mathf.CeilToInt(total / (float)pageSize));
            choicePage = Mathf.Clamp(choicePage, 0, pageCount - 1);
            int start = choicePage * pageSize;
            int end = Mathf.Min(total, start + pageSize);
            for (int i = start; i < end; i++)
            {
                var command = choiceCommands[i];
                CreateChoiceButton(command.label, command.action, command.interactable);
            }
            if (total > 4)
            {
                string label = choicePage < pageCount - 1
                    ? "次の選択肢\n" + (choicePage + 2) + "/" + pageCount
                    : "前の選択肢\n" + choicePage + "/" + pageCount;
                CreateChoiceButton(label, () =>
                {
                   choicePage = choicePage < pageCount - 1 ? choicePage + 1 : choicePage - 1;
                    RenderChoiceCommands();
                }, true);
            }
        }
        void CreateChoiceButton(string label, Action action, bool interactable = true)
        {
            EnsureChoiceSlots();
            int slot = -1;
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (choiceButtons[i] != null && !choiceButtons[i].gameObject.activeSelf)
                {
                    slot = i;
                    break;
                }
            }
            if (slot < 0)
                return;
            var button = choiceButtons[slot];
            var text = choiceButtonLabels[slot];
            button.gameObject.SetActive(true);
            button.onClick.RemoveAllListeners();
            button.interactable = interactable;
            var colors = button.colors;
            colors.normalColor = interactable ? new Color(0.12f, 0.095f, 0.13f) : new Color(0.055f, 0.05f, 0.06f);
            colors.highlightedColor = new Color(0.22f, 0.16f, 0.19f);
            colors.pressedColor = new Color(0.44f, 0.22f, 0.16f);
            colors.disabledColor = new Color(0.055f, 0.05f, 0.06f);
            button.colors = colors;
            if (interactable)
            {
               button.onClick.AddListener(() =>
                {
                    Play(clickSfx);
                    StartCoroutine(RunChoiceAction(action));
                });
            }
            if (text != null)
            {
                text.text = label;
                text.enabled = true;
                text.gameObject.SetActive(true);
                text.verticalOverflow = VerticalWrapMode.Truncate;
                text.resizeTextForBestFit = false;
                text.fontSize = label.Contains("\n") || label.Length > 13 ? 14 : 16;
                text.resizeTextMinSize = 13;
                text.resizeTextMaxSize = 16;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(button.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(choiceRoot);
            Canvas.ForceUpdateCanvases();
        }
        void CreateChoiceSlots()
        {
            choiceButtons = new Button[4];
            choiceButtonLabels = new Text[4];
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                var button = NewButton("Choice" + i, choiceRoot, "", new Color(0.12f, 0.095f, 0.13f), 16);
                var layout = button.gameObject.AddComponent<LayoutElement>();
                layout.minWidth = 300f;
                layout.minHeight = 58f;
                layout.preferredWidth = 300f;
                layout.preferredHeight = 58f;
                var text = button.GetComponentInChildren<Text>(true);
                if (text != null)
                {
                    text.text = "";
                    text.enabled = true;
                    text.gameObject.SetActive(true);
                   text.verticalOverflow = VerticalWrapMode.Truncate;
                    text.resizeTextForBestFit = false;
                    text.resizeTextMinSize = 13;
                    text.resizeTextMaxSize = 16;
                    text.raycastTarget = false;
                }
                button.onClick.RemoveAllListeners();
                button.interactable = false;
                button.gameObject.SetActive(false);
                choiceButtons[i] = button;
                choiceButtonLabels[i] = text;
            }
        }
        void EnsureChoiceSlots()
        {
            bool needsCreate = choiceButtons == null || choiceButtons.Length != 4 || choiceButtonLabels == null || choiceButtonLabels.Length != 4;
            if (!needsCreate)
            {
                for (int i = 0; i < choiceButtons.Length; i++)
                {
                    if (choiceButtons[i] == null)
                    {
                        needsCreate = true;
                        break;
                    }
                }
            }
            if (needsCreate)
                CreateChoiceSlots();
        }
        IEnumerator RunChoiceAction(Action action)
        {
            yield return null;
            action?.Invoke();
        }
        void SetBackground(string id)
        {
            if (string.IsNullOrEmpty(id))
             id = "title";
            var texture = Resources.Load<Texture2D>("AichiFantasy/Backgrounds/" + id);
            if (texture == null)
            {
                var sprite = Resources.Load<Sprite>("AichiFantasy/Backgrounds/" + id);
                if (sprite != null)
                    texture = sprite.texture;
            }
            background.texture = texture != null ? texture : Texture2D.blackTexture;
            background.color = Color.white;
        }
        void SetPortrait(string id)
        {
            if (portraitPanel == null || portraitImage == null)
                return;
            if (string.IsNullOrEmpty(id))
            {
                portraitPanel.gameObject.SetActive(false);
                portraitImage.sprite = null;
                return;
            }
            if (!portraitCache.TryGetValue(id, out var sprite))
            {
                var texture = Resources.Load<Texture2D>("AichiFantasy/Portraits/" + id);
                if (texture == null)
                {
                    portraitPanel.gameObject.SetActive(false);
                    return;
                }
                sprite = CreateTrimmedPortraitSprite(texture);
                portraitCache[id] = sprite;
            }
            ApplyPortraitLayout(id);
            portraitPanel.gameObject.SetActive(true);
            portraitImage.sprite = sprite;
            portraitImage.color = Color.white;
        }
      void ApplyPortraitLayout(string id)
        {
            if (portraitPanel == null)
                return;
            portraitPanel.SetSiblingIndex(4);
            if (id == "impossible_one")
            {
                Anchor(portraitPanel, 0.00f, 0.015f, 1.00f, 0.965f, 0, 0, 0, 0);
                return;
            }
            if (IsBossPortrait(id))
            {
                Anchor(portraitPanel, 0.11f, 0.285f, 0.89f, 0.93f, 0, 0, 0, 0);
                return;
            }
            Anchor(portraitPanel, 0.22f, 0.43f, 0.78f, 0.902f, 0, 0, 0, 0);
        }
        bool IsBossPortrait(string id)
        {
            return id == "locker_womb" ||
                   id == "shachi_avatar" ||
                   id == "miso_voice" ||
                   id == "deep_one_clerk" ||
                   id == "window_god" ||
                   id == "gate_inspector" ||
                   id == "impossible_one";
        }
        Sprite CreateTrimmedPortraitSprite(Texture2D texture)
        {
            try
            {
                var pixels = texture.GetPixels32();
                int minX = texture.width;
                int minY = texture.height;
                int maxX = -1;
                int maxY = -1;
              for (int y = 0; y < texture.height; y++)
                {
                    int row = y * texture.width;
                    for (int x = 0; x < texture.width; x++)
                    {
                        if (pixels[row + x].a <= 18)
                            continue;
                        if (x < minX) minX = x;
                        if (y < minY) minY = y;
                        if (x > maxX) maxX = x;
                        if (y > maxY) maxY = y;
                    }
                }
                if (maxX >= minX && maxY >= minY)
                {
                    int width = maxX - minX + 1;
                    int height = maxY - minY + 1;
                    int pad = Mathf.RoundToInt(Mathf.Max(width, height) * 0.08f);
                    minX = Mathf.Max(0, minX - pad);
                    minY = Mathf.Max(0, minY - pad);
                    maxX = Mathf.Min(texture.width - 1, maxX + pad);
                    maxY = Mathf.Min(texture.height - 1, maxY + pad);
                    return Sprite.Create(texture, new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1), new Vector2(0.5f, 0.5f), 100f);
                }
            }
            catch (UnityException)
            {
                // Some imported textures may not be readable yet; use the full sprite as a safe fallback.
            }
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        }
        void Play(AudioClip clip, float volume = 1f)
        {
            if (clip != null && audioSource != null)
                audioSource.PlayOneShot(clip, volume);
     }
        static T NewObject<T>(string name, Transform parent) where T : Component
        {
            var go = new GameObject(name, typeof(T));
            go.transform.SetParent(parent, false);
            return go.GetComponent<T>();
        }
        static T NewObject<T>(string name, Component parent) where T : Component
        {
            return NewObject<T>(name, parent.transform);
        }
        Font UiFont()
        {
            string[] candidates = { "Yu Mincho", "Yu Mincho Demibold", "Yu Gothic", "Yu Gothic UI", "Meiryo", "MS Gothic", "Arial" };
            return Font.CreateDynamicFontFromOSFont(candidates, 18);
        }
        Text NewText(string name, Transform parent, int size, FontStyle style, Color color)
        {
            var text = NewObject<Text>(name, parent);
            text.font = UiFont();
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(14, Mathf.RoundToInt(size * 0.78f));
            text.resizeTextMaxSize = size;
            var shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.78f);
            shadow.effectDistance = new Vector2(1.6f, -1.6f);
            return text;
        }
              Image NewImage(string name, Transform parent, Color color)
        {
            var image = NewObject<Image>(name, parent);
            image.color = color;
            return image;
        }
        RawImage NewRawImage(string name, Transform parent, Color color)
        {
            var image = NewObject<RawImage>(name, parent);
            image.color = color;
            return image;
        }
        RectTransform NewPanel(string name, Transform parent, Color color)
        {
            var image = NewImage(name, parent, color);
            return image.rectTransform;
        }
        Button NewButton(string name, Transform parent, string label, Color color, int fontSize)
        {
            var button = NewObject<Button>(name, parent);
            var image = button.gameObject.AddComponent<Image>();
            image.color = color;
            button.targetGraphic = image;
            AddBorder(button.GetComponent<RectTransform>(), new Color(0.82f, 0.58f, 0.22f, 0.26f));
            var text = NewText("Label", button.transform, fontSize, FontStyle.Bold, new Color(0.94f, 0.88f, 0.72f));
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextMinSize = 14;
            text.raycastTarget = false;
            Anchor(text.rectTransform, 0, 0, 1, 1, 10, 8, -10, -8);
            text.text = label;
            return button;
        }
        Slider NewSlider(string name, Transform parent, Color fillColor)
        {
            var root = NewObject<RectTransform>(name, parent);
            var bg = root.gameObject.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.075f, 0.085f, 0.95f);
           var fillArea = NewObject<RectTransform>("Fill Area", root);
            Anchor(fillArea, 0, 0, 1, 1, 4, 4, -4, -4);
            var fill = NewImage("Fill", fillArea, fillColor);
            Stretch(fill.rectTransform, 0, 0, 0, 0);
            var slider = root.gameObject.AddComponent<Slider>();
            slider.transition = Selectable.Transition.None;
            slider.fillRect = fill.rectTransform;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 0;
            return slider;
        }
        void AddBorder(RectTransform parent, Color color)
        {
            const float thickness = 2f;
            var top = NewImage("BorderTop", parent, color);
            Anchor(top.rectTransform, 0, 1, 1, 1, 0, -thickness, 0, 0);
            var bottom = NewImage("BorderBottom", parent, color);
            Anchor(bottom.rectTransform, 0, 0, 1, 0, 0, 0, 0, thickness);
            var left = NewImage("BorderLeft", parent, color);
            Anchor(left.rectTransform, 0, 0, 0, 1, 0, 0, thickness, 0);
            var right = NewImage("BorderRight", parent, color);
            Anchor(right.rectTransform, 1, 0, 1, 1, -thickness, 0, 0, 0);
        }
        static void Stretch(RectTransform rect, float left, float top, float right, float bottom)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }
        static void Anchor(RectTransform rect, float minX, float minY, float maxX, float maxY, float left, float bottom, float right, float top)
        {
            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(right, top);
        }
    }
}
