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
            public string effect;
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
            public string freedomRegion;
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
            public int finalRushIndex;
            public int stageSeed;
            public string sanityCollapseReturnScene;
            public bool suppressSanityQueueOnce;
            public HashSet<string> flags = new HashSet<string>();
            public HashSet<string> seenRandomEvents = new HashSet<string>();
            public Dictionary<string, int> choiceUses = new Dictionary<string, int>();
            public List<string> recentLog = new List<string>();
        }
        sealed class ProgressSnapshot
        {
            public int owari;
            public int mikawa;
            public int npcCafe;
            public int npcOccult;
            public int npcAirport;
            public int shachiGaze;
            public int dangerWarnings;
            public int ogura;
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
        RectTransform topBarRoot;
        RectTransform storyPanelRoot;
        RectTransform storyContent;
        ScrollRect storyScroll;
        Button storyPrevButton;
        Button storyNextButton;
        Text storyPageText;
        RectTransform sidePanelRoot;
        RectTransform leftPanelRoot;
        Button leftPanelToggleButton;
        RectTransform statusModalRoot;
        Text statusModalText;
        Button statusModalCloseButton;
        Text diceOverlayText;
        RectTransform diceOverlayRoot;
        Text statsText;
        Text inventoryText;
        Text footerText;
        RectTransform choiceRoot;
        RectTransform choiceContent;
        Button[] choiceButtons;
        Text[] choiceButtonLabels;
        readonly List<ChoiceCommand> choiceCommands = new List<ChoiceCommand>();
        bool storyExpanded;
        string currentPortraitId;
        string fullStoryText = "";
        string renderedStoryPageText = "";
        readonly List<string> storyPages = new List<string>();
        int storyPageIndex;
        int lastScreenWidth;
        int lastScreenHeight;
        bool lastBattleActive;
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
            ApplyResponsiveLayout();
            RefreshStoryPagination();
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
            topBarRoot = NewPanel("TopBar", root, new Color(0.015f, 0.012f, 0.018f, 0.78f));
            Anchor(topBarRoot, 0, 0.855f, 1, 0.975f, 18, 0, -18, -8);
            AddBorder(topBarRoot, new Color(0.75f, 0.52f, 0.22f, 0.25f));
            hpSlider = NewSlider("TopHP", topBarRoot, new Color(0.72f, 0.05f, 0.08f));
            Anchor(hpSlider.GetComponent<RectTransform>(), 0.055f, 0.52f, 0.32f, 0.76f, 0, 0, 0, 0);
            hpText = NewText("HPText", topBarRoot, 18, FontStyle.Bold, new Color(0.98f, 0.92f, 0.86f));
            Anchor(hpText.rectTransform, 0.11f, 0.48f, 0.31f, 0.84f, 0, 0, 0, 0);
            hpText.alignment = TextAnchor.MiddleCenter;
            coinText = NewText("CoinText", topBarRoot, 18, FontStyle.Bold, new Color(0.95f, 0.78f, 0.32f));
            Anchor(coinText.rectTransform, 0.055f, 0.12f, 0.31f, 0.42f, 0, 0, 0, 0);
            titleText = NewText("Title", topBarRoot, 22, FontStyle.Bold, new Color(0.95f, 0.72f, 0.28f));
            Anchor(titleText.rectTransform, 0.335f, 0.12f, 0.665f, 0.88f, 0, 0, 0, 0);
            titleText.alignment = TextAnchor.MiddleCenter;
            areaText = NewText("Area", topBarRoot, 14, FontStyle.Normal, new Color(0.7f, 0.82f, 0.92f));
           Anchor(areaText.rectTransform, 0.68f, 0.12f, 0.985f, 0.88f, 4, 0, -12, 0);
            areaText.alignment = TextAnchor.MiddleRight;
            sidePanelRoot = NewPanel("StatusPanel", root, new Color(0.025f, 0.02f, 0.03f, 0.78f));
            Anchor(sidePanelRoot, 0.745f, 0.045f, 0.985f, 0.36f, 0, 0, 0, 0);
            AddBorder(sidePanelRoot, new Color(0.75f, 0.52f, 0.22f, 0.22f));
            statsText = NewText("Stats", sidePanelRoot, 16, FontStyle.Normal, new Color(0.88f, 0.9f, 0.86f));
            Anchor(statsText.rectTransform, 0, 0, 1, 1, 16, 8, -16, -14);
            leftPanelRoot = NewPanel("InventoryPanel", root, new Color(0.025f, 0.02f, 0.03f, 0.78f));
            Anchor(leftPanelRoot, 0.015f, 0.045f, 0.255f, 0.36f, 0, 0, 0, 0);
            AddBorder(leftPanelRoot, new Color(0.75f, 0.52f, 0.22f, 0.22f));
            inventoryText = NewText("Inventory", leftPanelRoot, 16, FontStyle.Normal, new Color(0.78f, 0.82f, 0.76f));
            Anchor(inventoryText.rectTransform, 0, 0, 1, 1, 16, 10, -16, -8);
            leftPanelToggleButton = NewButton("StatusButton", root, "ステータス", new Color(0.09f, 0.075f, 0.1f, 0.92f), 13);
            Anchor(leftPanelToggleButton.GetComponent<RectTransform>(), 0.015f, 0.785f, 0.125f, 0.84f, 0, 0, 0, 0);
            leftPanelToggleButton.onClick.AddListener(ShowStatusModal);
            storyPanelRoot = NewPanel("StoryPanel", root, new Color(0.018f, 0.014f, 0.02f, 0.84f));
            Anchor(storyPanelRoot, 0.27f, 0.245f, 0.73f, 0.425f, 0, 0, 0, 0);
            AddBorder(storyPanelRoot, new Color(0.72f, 0.5f, 0.22f, 0.28f));
            storyPanelRoot.gameObject.AddComponent<RectMask2D>();
            storyContent = NewObject<RectTransform>("StoryContent", storyPanelRoot);
            storyContent.anchorMin = new Vector2(0f, 1f);
            storyContent.anchorMax = new Vector2(1f, 1f);
            storyContent.pivot = new Vector2(0.5f, 1f);
            storyContent.offsetMin = Vector2.zero;
            storyContent.offsetMax = Vector2.zero;
            storyScroll = storyPanelRoot.gameObject.AddComponent<ScrollRect>();
            storyScroll.content = storyContent;
            storyScroll.viewport = storyPanelRoot;
            storyScroll.horizontal = false;
            storyScroll.vertical = true;
            storyScroll.movementType = ScrollRect.MovementType.Clamped;
            storyScroll.scrollSensitivity = 30f;
            portraitPanel = NewPanel("PortraitPanel", root, new Color(0.01f, 0.008f, 0.012f, 0.08f));
            Anchor(portraitPanel, 0.22f, 0.43f, 0.78f, 0.902f, 0, 0, 0, 0);
            portraitPanel.GetComponent<Image>().raycastTarget = false;
            portraitImage = NewImage("PortraitImage", portraitPanel, Color.clear);
            Anchor(portraitImage.rectTransform, 0, 0, 1, 1, 0, 0, 0, 0);
            portraitImage.preserveAspect = true;
            portraitImage.raycastTarget = false;
            bodyText = NewText("Body", storyContent, 18, FontStyle.Normal, new Color(0.93f, 0.9f, 0.82f));
            Anchor(bodyText.rectTransform, 0, 0, 1, 1, 16, 38, -16, -12);
            bodyText.alignment = TextAnchor.UpperLeft;
            bodyText.lineSpacing = 1.18f;
            bodyText.verticalOverflow = VerticalWrapMode.Overflow;
            bodyText.resizeTextForBestFit = false;
            storyPrevButton = NewButton("StoryPrev", storyPanelRoot, "前頁", new Color(0.09f, 0.075f, 0.1f), 14);
            Anchor(storyPrevButton.GetComponent<RectTransform>(), 0.02f, 0.01f, 0.18f, 0.13f, 0, 0, 0, 0);
            storyPrevButton.onClick.AddListener(() => ChangeStoryPage(-1));
            storyPageText = NewText("StoryPage", storyPanelRoot, 13, FontStyle.Normal, new Color(0.82f, 0.76f, 0.62f));
            Anchor(storyPageText.rectTransform, 0.20f, 0.01f, 0.80f, 0.13f, 0, 0, 0, 0);
            storyPageText.alignment = TextAnchor.MiddleCenter;
            storyNextButton = NewButton("StoryNext", storyPanelRoot, "次頁", new Color(0.09f, 0.075f, 0.1f), 14);
            Anchor(storyNextButton.GetComponent<RectTransform>(), 0.82f, 0.01f, 0.98f, 0.13f, 0, 0, 0, 0);
            storyNextButton.onClick.AddListener(() => ChangeStoryPage(1));
            diceOverlayRoot = NewPanel("DiceOverlay", root, new Color(0.018f, 0.014f, 0.02f, 0.92f));
            Anchor(diceOverlayRoot, 0.285f, 0.445f, 0.715f, 0.71f, 0, 0, 0, 0);
            AddBorder(diceOverlayRoot, new Color(0.72f, 0.5f, 0.22f, 0.45f));
            diceOverlayText = NewText("DiceOverlayText", diceOverlayRoot, 28, FontStyle.Bold, new Color(0.98f, 0.86f, 0.56f));
            Anchor(diceOverlayText.rectTransform, 0, 0, 1, 1, 18, 12, -18, -12);
            diceOverlayText.alignment = TextAnchor.MiddleCenter;
            diceOverlayText.lineSpacing = 1.05f;
            diceOverlayRoot.gameObject.SetActive(false);
            choiceRoot = NewObject<RectTransform>("Choices", root);
            Anchor(choiceRoot, 0.27f, 0.035f, 0.73f, 0.235f, 0, 0, 0, 0);
            var choiceBg = choiceRoot.gameObject.AddComponent<Image>();
            choiceBg.color = new Color(0.018f, 0.014f, 0.02f, 0.62f);
            choiceRoot.gameObject.AddComponent<RectMask2D>();
            AddBorder(choiceRoot, new Color(0.72f, 0.5f, 0.22f, 0.2f));
            choiceContent = NewObject<RectTransform>("ChoiceContent", choiceRoot);
            choiceContent.anchorMin = new Vector2(0f, 1f);
            choiceContent.anchorMax = new Vector2(1f, 1f);
            choiceContent.pivot = new Vector2(0.5f, 1f);
            choiceContent.offsetMin = new Vector2(10f, 0f);
            choiceContent.offsetMax = new Vector2(-10f, 0f);
            var choiceLayout = choiceContent.gameObject.AddComponent<VerticalLayoutGroup>();
            choiceLayout.spacing = 8f;
            choiceLayout.padding = new RectOffset(2, 2, 10, 10);
            choiceLayout.childAlignment = TextAnchor.UpperCenter;
            choiceLayout.childControlWidth = true;
            choiceLayout.childControlHeight = true;
            choiceLayout.childForceExpandWidth = true;
            choiceLayout.childForceExpandHeight = false;
            var choiceFitter = choiceContent.gameObject.AddComponent<ContentSizeFitter>();
            choiceFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var choiceScroll = choiceRoot.gameObject.AddComponent<ScrollRect>();
            choiceScroll.content = choiceContent;
            choiceScroll.horizontal = false;
            choiceScroll.vertical = true;
            choiceScroll.movementType = ScrollRect.MovementType.Clamped;
            choiceScroll.scrollSensitivity = 28f;
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
            statusModalRoot = NewPanel("StatusModal", root, new Color(0.018f, 0.014f, 0.02f, 0.94f));
            Anchor(statusModalRoot, 0.18f, 0.14f, 0.82f, 0.80f, 0, 0, 0, 0);
            AddBorder(statusModalRoot, new Color(0.75f, 0.52f, 0.22f, 0.45f));
            statusModalRoot.gameObject.AddComponent<RectMask2D>();
            statusModalText = NewText("StatusModalText", statusModalRoot, 18, FontStyle.Normal, new Color(0.91f, 0.89f, 0.78f));
            Anchor(statusModalText.rectTransform, 0, 0, 1, 1, 24, 24, -24, -76);
            statusModalText.alignment = TextAnchor.UpperLeft;
            statusModalText.lineSpacing = 1.16f;
            statusModalCloseButton = NewButton("StatusModalClose", statusModalRoot, "閉じる", new Color(0.12f, 0.095f, 0.13f), 16);
            Anchor(statusModalCloseButton.GetComponent<RectTransform>(), 0.37f, 0.025f, 0.63f, 0.105f, 0, 0, 0, 0);
            statusModalCloseButton.onClick.AddListener(HideStatusModal);
            statusModalRoot.gameObject.SetActive(false);
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
                "あなたは何度でも迷い込み、何度でも死に、残った道筋だけを覚えていく。\n" +
                "双鯱、味噌樽、工場心臓。知るほど正気は削られるが、知らなければ帰れない。";
            if (hpSlider != null) hpSlider.value = 0;
            if (hpText != null) hpText.text = "";
            if (coinText != null) coinText.text = "● " + progress.memoryFragments;
            statsText.text = "周回通貨: " + progress.memoryFragments + " 記憶片\n到達エンド: " + progress.endings.Count +
                             "\n死因登録: " + progress.deaths.Count + "\n怪異遭遇: " + progress.seenMonsters.Count;
            inventoryText.text = "本番向け要素:\n死因図鑑\n怪異図鑑\n正気度UI変化\n名のない気配\n\n同梱SEはゲーム用に生成した仮素材です。";
            footerText.text = "暗いご当地ファンタジー / クトゥルフ風探索 / 連打バトル";
            ClearChoices();
            AddChoiceButton("はじめる", ShowInstabilitySelect);
            AddChoiceButton("拠点/倉庫", ShowBase);
            AddChoiceButton("図鑑と解放", ShowUnlocks);
            AddChoiceButton("\u30c6\u30b9\u30c8\u7528: \u8a18\u61b6\u7247+3000", AddTestMemoryFragments);
            AddChoiceButton("進行を初期化", ResetProgress);
        }
        void AddTestMemoryFragments()
        {
            progress.memoryFragments += 3000;
            SaveProgress();
            Play(rewardSfx);
            ShowTitle();
            footerText.text = "\u30c6\u30b9\u30c8\u7528\u306b\u8a18\u61b6\u7247\u30923000\u8ffd\u52a0\u3057\u307e\u3057\u305f\u3002";
        }
        void ShowInstabilitySelect()
        {
            mode = Mode.CharacterSelect;
            SetBackground("title");
            SetPortrait(InstabilityPortraitId(selectedInstability));
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
                    ShowInstabilityConfirm(level);
                });
            }
            AddChoiceButton("戻る", ShowTitle);
        }
        void ShowInstabilityConfirm(int level)
        {
            mode = Mode.CharacterSelect;
            selectedInstability = Mathf.Clamp(level, 0, 5);
            SetBackground("title");
            SetPortrait(InstabilityPortraitId(selectedInstability));
            battleRoot.gameObject.SetActive(false);
            choiceRoot.gameObject.SetActive(true);
            titleText.text = InstabilityName(selectedInstability);
            areaText.text = "境界不安定度 確認";
            bodyText.text = InstabilityDetail(selectedInstability);
            statsText.text = InstabilitySummary(selectedInstability);
            inventoryText.text = "難易度を上げるほど、立ち絵の輪郭も人から遠ざかる。\n\nこの内容で開始する場合は確認して次へ進んでください。";
            ClearChoices();
            AddChoiceButton("この難易度で進む", ShowCharacterSelect);
            AddChoiceButton("難易度を選び直す", ShowInstabilitySelect);
            AddChoiceButton("タイトルへ戻る", ShowTitle);
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
            bodyText.text = unlockLine + "\n" + CharacterInvestmentRank(character) + "\n" + character.subtitle + "\n" + character.description + "\n\nHP " + s.maxHp + " / MP " + s.maxMp +
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
        string CharacterInvestmentRank(CharacterDef character)
        {
            if (character.unlockCost <= 0)
                return "初期キャラ: 低コストで扱いやすいが、専門性能は控えめ。";
            if (character.id == "final_observer")
                return "最終解放ランク: この世のものとは思えないものを倒すための規格外キャラ。選択時は専用ボスラッシュへ進む。";
            if (character.unlockCost >= 700)
                return "投資ランク: 極大。記憶片を大量に使う代わりに、周回性能と報酬倍率が最上位。";
            if (character.unlockCost >= 320)
                return "投資ランク: 大。高難度や特定ルートを強く押し通せる上級キャラ。";
            if (character.unlockCost >= 130)
                return "投資ランク: 中。無料キャラより明確に強く、得意分野では別物。";
            return "投資ランク: 小。初期キャラから一段上の専門性能を持つ。";
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
            run.stageSeed = rng.Next(100000, 999999);
            if (character.id != "traveler")
               run.dangerWarnings = 0;
            ApplyCharacterStartTraits();
            LogRun("周回開始: 装備確認へ");
            ShowStartingGearSelect();
        }
        void ApplyCharacterStartTraits()
        {
            if (run == null || run.character == null)
                return;
            switch (run.character.id)
            {
                case "traveler":
                    run.flags.Add("skill_fallback_route");
                    run.dangerWarnings += 1;
                    break;
                case "worker":
                    run.flags.Add("skill_expense_report");
                    run.stats.machineAptitude += 1;
                    break;
                case "local":
                    run.flags.Add("skill_local_route");
                    run.stats.localKnowledge += 1;
                    break;
                case "occult":
                    run.flags.Add("skill_forbidden_note");
                    run.stats.mythosKnowledge += 1;
                    run.stats.sanity = Math.Max(1, run.stats.sanity - 1);
                    break;
                case "mechanic":
                    run.flags.Add("skill_machine_guard");
                    run.stats.defense += 1;
                    break;
                case "samurai":
                    run.flags.Add("skill_battle_oath");
                    run.mikawa += 1;
                    run.stats.attack += 1;
                    break;
                case "shachi_seen":
                    run.flags.Add("skill_shachi_mark");
                    run.shachiGaze += 2;
                    run.dangerWarnings += 1;
                    break;
                case "atsuta_miko":
                    run.flags.Add("skill_atsuta_ward");
                    run.dangerWarnings += 1;
                    run.stats.sanity = Math.Min(run.stats.maxSanity, run.stats.sanity + 2);
                    break;
                case "seto_potter":
                    run.flags.Add("skill_ceramic_guard");
                    run.stats.hp = Math.Min(run.stats.maxHp, run.stats.hp + 4);
                    run.stats.defense += 1;
                    break;
                case "toyohashi_conductor":
                    run.flags.Add("skill_last_train");
                    run.dangerWarnings += 1;
                    run.stats.speed += 1;
                    break;
                case "gamagori_diver":
                    run.flags.Add("skill_deep_chart");
                    run.npcAirport += 1;
                    run.stats.mythosKnowledge += 1;
                    break;
                case "centrair_agent":
                    run.flags.Add("skill_border_staff");
                    run.npcAirport += 3;
                    run.dangerWarnings += 2;
                    break;
                case "arimatsu_weaver":
                    run.flags.Add("skill_thread_sight");
                    run.stats.luck += 1;
                    run.stats.localKnowledge += 1;
                    break;
                case "inuyama_mask":
                    run.flags.Add("skill_mask_shift");
                    run.dangerWarnings += 1;
                    run.stats.luck += 1;
                    break;
                case "tsuruma_librarian":
                    run.flags.Add("skill_index_debt");
                    run.stats.mythosKnowledge += 2;
                    run.stats.sanity = Math.Max(1, run.stats.sanity - 1);
                    break;
                case "final_observer":
                    run.flags.Add("skill_final_observer");
                    run.flags.Add("impossible_battle_ready");
                    run.dangerWarnings += 5;
                    run.npcAirport += 5;
                    run.shachiGaze += 5;
                    run.stats.mythosKnowledge += 5;
                    run.stats.mythosCorruption = Math.Max(0, run.stats.mythosCorruption - 4);
                    run.stats.hp = run.stats.maxHp;
                    run.stats.sanity = run.stats.maxSanity;
                    break;
            }
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
            bodyText.text = "異界愛知へ入る前に、今回の契約を選ぶ。\n\n探索、遭遇、戦闘。そのどれもが空港境界へ続く。契約の名だけが、道の歪み方を決める。";
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
            if (run.character.id == "final_observer")
            {
                run.stats.hp = run.stats.maxHp;
                run.stats.sanity = run.stats.maxSanity;
                run.stats.mythosKnowledge += 2;
                result += "\n\n境界外の観測者は通常探索を飛ばし、専用ボスラッシュへ進む。";
            }
            Play(rewardSfx);
            bodyText.text = "出目: " + a + " / " + b + " / " + c + "  合計 " + total + "\n\n" + result;
            UpdateSideText();
            ClearChoices();
            if (run.character.id == "final_observer")
                AddChoiceButton("専用ボスラッシュへ", ShowFinalBossRushIntro);
            else
                AddChoiceButton("名駅の底で目覚める", () => ShowScene("nagoya_start", false));
            diceRolling = false;
        }
        void ShowScene(string sceneId, bool allowRandom = true)
        {
            SetStoryPanelExpanded(false);
            HideDiceOverlay();
            if (run != null && run.stats.sanity <= 0 && run.sanityCollapseTurns < 0)
            {
                if (run.suppressSanityQueueOnce)
                    run.suppressSanityQueueOnce = false;
                else
                {
                    QueueSanityCollapse(sceneId);
                    return;
                }
            }
            if (TryShowSanityCollapse(sceneId))
                return;
            if (allowRandom && TryShowImpossibleNonBossEncounter(sceneId))
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
            ApplyLoopPressure();
            if (run.randomCooldown > 0)
                run.randomCooldown--;
            var scene = BuildRuntimeStageEventScene(sceneId, scenes[sceneId]);
            battleRoot.gameObject.SetActive(false);
            choiceRoot.gameObject.SetActive(true);
            SetBackground(scene.image);
            SetPortrait(scene.portrait);
            titleText.text = scene.title;
            areaText.text = scene.area;
            bodyText.text = scene.text;
            string progressHint = BuildProgressHint(sceneId);
            if (!string.IsNullOrEmpty(progressHint))
                bodyText.text += "\n\n進行目標\n" + progressHint;
            if (!string.IsNullOrEmpty(run.pendingOutcomeText))
            {
                bodyText.text += "\n\n直前の結果\n" + run.pendingOutcomeText;
                run.pendingOutcomeText = null;
            }
            if (SceneHasLuckChoice(scene))
                bodyText.text += "\n\nLUK手応え: 6面ダイス3個 + LUK補正(LUK/2、最大+6)で判定。LUKが高いほど3個目が6になりやすい。成功で報酬、失敗でHP/SANを失う。";
            if (run.steps >= 10)
                bodyText.text += "\n\n周回圧: 長く留まるほど怪異の巡回が濃くなる。安全行動の反復は危険察知と不安定度を押し上げる。";
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
                    if (IsForcedRandomEventChoice(choice))
                    {
                        RecordChoiceUse(choice);
                        run.pendingSceneAfterRandom = string.IsNullOrEmpty(choice.next) ? sceneId : choice.next;
                        run.randomCooldown = 0;
                        LogRun("名のない気配発生");
                        ShowRandomEvent(run.pendingSceneAfterRandom);
                        return;
                    }
                    if (choice.effect != null && choice.label.Contains("LUK判定"))
                    {
                        RecordChoiceUse(choice);
                        StartCoroutine(ResolveChoiceWithDiceAnimation(choice));
                        return;
                    }
                    var before = run.stats.Clone();
                    var progressBefore = CaptureProgress();
                   run.lastRollSummary = null;
                    RecordChoiceUse(choice);
                    choice.effect?.Invoke(run);
                    string delta = BuildStatDelta(before, run.stats, progressBefore);
                    string outcome = BuildChoiceOutcomeText(choice, choice.label.Replace("\n", " / "), delta);
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
        void SetStoryPanelExpanded(bool expanded)
        {
            storyExpanded = expanded;
            ApplyStoryPanelAnchor();
            ResetStoryPages();
        }
        bool IsMobileLayout()
        {
            return Screen.height > Screen.width || Screen.width < 900;
        }
        bool IsSideCommandLayout()
        {
            return Screen.width >= 900 && Screen.width >= Screen.height;
        }
        void ShowStatusModal()
        {
            RefreshStatusModalText();
            if (statusModalRoot != null)
            {
                statusModalRoot.gameObject.SetActive(true);
                statusModalRoot.SetAsLastSibling();
            }
        }
        void HideStatusModal()
        {
            if (statusModalRoot != null)
                statusModalRoot.gameObject.SetActive(false);
        }
        string FreedomRegionLabel(string region)
        {
            if (string.IsNullOrEmpty(region))
                return "\u672a\u9078\u629e";
            if (region == "owari")
                return "\u5c3e\u5f35";
            if (region == "mikawa")
                return "\u4e09\u6cb3";
            if (region == "chita")
                return "\u77e5\u591a/\u6d77";
            return region;
        }
        string RouteGoalLabel(string goal)
        {
            if (string.IsNullOrEmpty(goal))
                return "\u672a\u8a2d\u5b9a";
            if (goal == "airport_return")
                return "\u7a7a\u6e2f\u5e30\u9084";
            if (goal == "shachi_true")
                return "\u9bf1\u771f\u76f8";
            if (goal == "regional_deep")
                return "\u5730\u65b9\u6df1\u5c64";
            if (goal == "survive")
                return "\u751f\u5b58\u512a\u5148";
            return goal;
        }
        void RefreshStatusModalText()
        {
            if (statusModalText == null)
                return;
            if (run == null)
            {
                statusModalText.text =
                    "ステータス\n\n" +
                    (statsText != null ? statsText.text : "") +
                    "\n\n所持品/進行\n\n" +
                    (inventoryText != null ? inventoryText.text : "");
                return;
            }
            var s = run.stats;
            statusModalText.text =
                run.character.name + "\n\n" +
                "HP " + s.hp + "/" + s.maxHp + "  MP " + s.mp + "/" + s.maxMp + "\n" +
                "進行値: 地元 " + s.localKnowledge + "  尾張 " + run.owari + "  三河 " + run.mikawa + "  鯱 " + run.shachiGaze + "\n" +
                "空港 " + run.npcAirport + "  危険察知 " + run.dangerWarnings + "  喫茶 " + run.npcCafe + "  オカルト " + run.npcOccult + "\n" +
                "味噌耐性 " + s.misoResistance + "  機械 " + s.machineAptitude + "  方面 " + FreedomRegionLabel(run.freedomRegion) + "\n" +
                "目標 " + RouteGoalLabel(run.routeGoal) + "\n\n" +
                "攻撃 " + (s.attack + run.weapon.attack + run.accessory.attack) +
                "  防御 " + (s.defense + run.armor.defense + run.accessory.defense) + "\n" +
                "速さ " + (s.speed + run.armor.speed + run.weapon.speed + run.accessory.speed) +
                "  LUK " + (s.luck + run.accessory.luck) + "\n" +
                "正気度 " + s.sanity + "/" + s.maxSanity +
                "  神話理解 " + s.mythosKnowledge +
                "  神話汚染 " + s.mythosCorruption + "\n" +
                "空腹 " + s.hunger + "  所持金 " + s.money + "\n\n" +
                "武器: " + GearSummary(run.weapon) + "\n\n" +
                "防具: " + GearSummary(run.armor) + "\n\n" +
                "装飾: " + GearSummary(run.accessory) + "\n\n" +
                InstabilityName(run.instability) + " / 保険札 " + progress.insuranceTickets + "\n" +
                "死因 " + progress.deaths.Count + " / 怪異 " + progress.seenMonsters.Count + "\n\n" +
                SanityFlavor();
        }
        void ApplyResponsiveLayout(bool force = false)
        {
            bool battleActive = mode == Mode.Battle && battleRoot != null && battleRoot.gameObject.activeSelf;
            if (!force && lastScreenWidth == Screen.width && lastScreenHeight == Screen.height && lastBattleActive == battleActive)
                return;
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            lastBattleActive = battleActive;
            bool mobile = IsMobileLayout();
            bool sideCommands = IsSideCommandLayout();
            if (topBarRoot != null)
                Anchor(topBarRoot, 0, mobile ? 0.865f : 0.855f, 1, 0.975f, mobile ? 8 : 18, 0, mobile ? -8 : -18, -8);
            if (sidePanelRoot != null)
            {
                sidePanelRoot.gameObject.SetActive(false);
            }
            if (leftPanelRoot != null)
            {
                leftPanelRoot.gameObject.SetActive(false);
            }
            if (leftPanelToggleButton != null)
            {
                leftPanelToggleButton.gameObject.SetActive(true);
                if (mobile)
                    Anchor(leftPanelToggleButton.GetComponent<RectTransform>(), 0.03f, 0.795f, 0.22f, 0.855f, 0, 0, 0, 0);
                else
                    Anchor(leftPanelToggleButton.GetComponent<RectTransform>(), 0.015f, 0.785f, 0.125f, 0.84f, 0, 0, 0, 0);
            }
            if (statusModalRoot != null)
            {
                if (mobile)
                    Anchor(statusModalRoot, 0.045f, 0.10f, 0.955f, 0.80f, 0, 0, 0, 0);
                else
                    Anchor(statusModalRoot, 0.18f, 0.14f, 0.82f, 0.80f, 0, 0, 0, 0);
            }
            if (choiceRoot != null)
            {
                if (sideCommands)
                    Anchor(choiceRoot, 0.755f, 0.045f, 0.985f, 0.84f, 0, 0, 0, 0);
                else if (mobile)
                    Anchor(choiceRoot, 0.06f, 0.045f, 0.94f, 0.285f, 0, 0, 0, 0);
                else
                    Anchor(choiceRoot, 0.27f, 0.035f, 0.73f, 0.235f, 0, 0, 0, 0);
            }
            if (battleRoot != null)
            {
                if (sideCommands)
                    Anchor(battleRoot, 0.27f, 0.055f, 0.73f, 0.19f, 0, 0, 0, 0);
                else if (mobile)
                    Anchor(battleRoot, 0.06f, 0.055f, 0.94f, 0.205f, 0, 0, 0, 0);
                else
                    Anchor(battleRoot, 0.27f, 0.055f, 0.73f, 0.19f, 0, 0, 0, 0);
            }
            if (portraitPanel != null && portraitPanel.gameObject.activeSelf)
                ApplyPortraitLayout(currentPortraitId);
            ApplyStoryPanelAnchor();
        }
        void ApplyStoryPanelAnchor()
        {
            if (storyPanelRoot == null)
                return;
            bool mobile = IsMobileLayout();
            bool sideCommands = IsSideCommandLayout();
            bool battleActive = mode == Mode.Battle && battleRoot != null && battleRoot.gameObject.activeSelf;
            if (storyExpanded)
            {
                if (sideCommands)
                    Anchor(storyPanelRoot, 0.22f, battleActive ? 0.245f : 0.055f, 0.735f, battleActive ? 0.38f : 0.46f, 0, 0, 0, 0);
                else if (mobile)
                    Anchor(storyPanelRoot, 0.06f, 0.305f, 0.94f, 0.735f, 0, 0, 0, 0);
                else
                    Anchor(storyPanelRoot, 0.24f, 0.245f, 0.76f, 0.58f, 0, 0, 0, 0);
                if (bodyText != null)
                {
                    bodyText.fontSize = mobile ? 18 : 16;
                    bodyText.resizeTextMinSize = mobile ? 18 : 16;
                    bodyText.resizeTextMaxSize = mobile ? 18 : 16;
                    bodyText.lineSpacing = mobile ? 1.12f : 1.08f;
                }
            }
            else
            {
                if (sideCommands)
                    Anchor(storyPanelRoot, 0.24f, battleActive ? 0.245f : 0.055f, 0.735f, battleActive ? 0.36f : 0.35f, 0, 0, 0, 0);
                else if (mobile)
                    Anchor(storyPanelRoot, 0.06f, 0.305f, 0.94f, 0.64f, 0, 0, 0, 0);
                else
                    Anchor(storyPanelRoot, 0.27f, 0.245f, 0.73f, 0.425f, 0, 0, 0, 0);
                if (bodyText != null)
                {
                    bodyText.fontSize = mobile ? 19 : 18;
                    bodyText.resizeTextMinSize = mobile ? 19 : 18;
                    bodyText.resizeTextMaxSize = mobile ? 19 : 18;
                    bodyText.lineSpacing = mobile ? 1.15f : 1.18f;
                }
            }
            RefreshStoryPagination();
        }
        void RefreshStoryScroll()
        {
            if (storyContent == null || bodyText == null || storyPanelRoot == null)
                return;
            float viewportHeight = Mathf.Max(1f, storyPanelRoot.rect.height);
            float preferred = Mathf.Max(viewportHeight, bodyText.preferredHeight + 28f);
            storyContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferred);
            Anchor(bodyText.rectTransform, 0, 0, 1, 1, 16, 38, -16, -12);
        }
        void ResetStoryScrollTop()
        {
            if (storyScroll == null)
                return;
            Canvas.ForceUpdateCanvases();
            RefreshStoryScroll();
            storyScroll.verticalNormalizedPosition = 1f;
        }
        void RefreshStoryPagination()
        {
            if (bodyText == null)
                return;
            if (bodyText.text != renderedStoryPageText)
                CaptureStoryText(bodyText.text);
            UpdateStoryPageControls();
        }
        void CaptureStoryText(string text)
        {
            fullStoryText = text ?? "";
            storyPageIndex = 0;
            RebuildStoryPages();
            RenderStoryPage();
        }
        void ResetStoryPages()
        {
            RebuildStoryPages();
            storyPageIndex = Mathf.Clamp(storyPageIndex, 0, Mathf.Max(0, storyPages.Count - 1));
            RenderStoryPage();
        }
        void RebuildStoryPages()
        {
            storyPages.Clear();
            string text = fullStoryText ?? "";
            bool battleActive = mode == Mode.Battle && battleRoot != null && battleRoot.gameObject.activeSelf;
            int limit = IsMobileLayout() ? 230 : IsSideCommandLayout() ? (battleActive ? 320 : 460) : 280;
            if (storyExpanded)
                limit += IsMobileLayout() ? 80 : 70;
            if (text.Length <= limit)
            {
                storyPages.Add(text);
                return;
            }
            int index = 0;
            while (index < text.Length)
            {
                int take = Mathf.Min(limit, text.Length - index);
                int split = index + take;
                if (split < text.Length)
                {
                    int paragraph = text.LastIndexOf("\n\n", split, take);
                    int line = text.LastIndexOf('\n', split - 1, take);
                    int punctuation = LastJapaneseBreak(text, index, split);
                    int best = paragraph > index + limit / 2 ? paragraph + 2 : line > index + limit / 2 ? line + 1 : punctuation > index + limit / 2 ? punctuation + 1 : split;
                    split = best;
                }
                storyPages.Add(text.Substring(index, split - index).Trim());
                index = split;
                while (index < text.Length && char.IsWhiteSpace(text[index]))
                    index++;
            }
            if (storyPages.Count == 0)
                storyPages.Add("");
        }
        int LastJapaneseBreak(string text, int start, int end)
        {
            int last = -1;
            end = Mathf.Min(end, text.Length);
            for (int i = start; i < end; i++)
            {
                char c = text[i];
                if (c == '。' || c == '！' || c == '？' || c == '.' || c == '!' || c == '?')
                    last = i;
            }
            return last;
        }
        void RenderStoryPage()
        {
            if (bodyText == null)
                return;
            if (storyPages.Count == 0)
                storyPages.Add("");
            storyPageIndex = Mathf.Clamp(storyPageIndex, 0, storyPages.Count - 1);
            renderedStoryPageText = storyPages[storyPageIndex];
            bodyText.text = renderedStoryPageText;
            RefreshStoryScroll();
            if (storyScroll != null)
                storyScroll.verticalNormalizedPosition = 1f;
            UpdateStoryPageControls();
        }
        void ChangeStoryPage(int delta)
        {
            if (storyPages.Count <= 1)
                return;
            storyPageIndex = Mathf.Clamp(storyPageIndex + delta, 0, storyPages.Count - 1);
            RenderStoryPage();
        }
        void UpdateStoryPageControls()
        {
            bool multi = storyPages.Count > 1;
            if (storyPrevButton != null)
            {
                storyPrevButton.gameObject.SetActive(multi);
                storyPrevButton.interactable = storyPageIndex > 0;
            }
            if (storyNextButton != null)
            {
                storyNextButton.gameObject.SetActive(multi);
                storyNextButton.interactable = storyPageIndex < storyPages.Count - 1;
            }
            if (storyPageText != null)
            {
                storyPageText.gameObject.SetActive(multi);
                storyPageText.text = multi ? (storyPageIndex + 1) + " / " + storyPages.Count : "";
            }
        }
        string BuildProgressHint(string sceneId)
        {
            if (run == null || string.IsNullOrEmpty(sceneId))
                return "";
            if (sceneId.StartsWith("stage") && sceneId.EndsWith("_hub"))
                return "STAGEボスは地方深層の先ではなく、このステージ内のボスゲートにいます。10個の出来事を越えるか、「STAGEボス地点へ急ぐ」を選ぶと到達します。STAGE 5のボス後は空港ゲートへ近づきます。";
            if (sceneId.StartsWith("stage") && sceneId.Contains("_event_"))
                return "このSTAGEの出来事を進めると、10個目の後にSTAGEボスゲートへ入ります。失敗しても進行は続きますが、HP/SANが削れます。地方深層は別ルートです。";
            if (sceneId == "freedom_owari")
                return "尾張方面のゴール: 尾張手がかり3以上、または鯱注視3以上で「尾張深層へ」が開きます。空港へ進むには 空港知識2 / 地元知識4 / 危険察知2 のどれかを満たします。\n現在: 尾張" + run.owari + " / 鯱注視" + run.shachiGaze + " / 空港" + run.npcAirport + " / 地元" + run.stats.localKnowledge + " / 危険" + run.dangerWarnings;
            if (sceneId == "freedom_mikawa")
                return "三河方面のゴール: 三河手がかり3以上、または味噌耐性5以上で「三河深層へ」が開きます。空港へ進むには 空港知識2 / 地元知識4 / 危険察知2 のどれかを満たします。\n現在: 三河" + run.mikawa + " / 味噌耐性" + run.stats.misoResistance + " / 空港" + run.npcAirport + " / 地元" + run.stats.localKnowledge + " / 危険" + run.dangerWarnings;
            if (sceneId == "freedom_chita")
                return "知多・海方面のゴール: 空港知識+神話理解が6以上で「海底深層へ」が開きます。空港へ進むには 空港知識2 / 地元知識4 / 危険察知2 のどれかを満たします。\n現在: 空港" + run.npcAirport + " / 神話理解" + run.stats.mythosKnowledge + " / 地元" + run.stats.localKnowledge + " / 危険" + run.dangerWarnings;
            if (sceneId == "nagoya_after_battle" || sceneId == "freedom_base" || sceneId == "meieki_goal")
                return "大目標: 空港境界へ向かい帰還する。条件は 空港知識2以上、地元知識4以上、危険察知2以上のいずれか。\n現在: 空港" + run.npcAirport + "/2 / 地元" + run.stats.localKnowledge + "/4 / 危険" + run.dangerWarnings + "/2";
            if (sceneId == "airport_bridge")
                return "空港周辺まで到達済みです。検査・手荷物・滑走路下を越えると空港ゲートへ進みます。";
            if (sceneId == "airport_gate")
                return "帰還ENDへ進むか、条件が揃っていればルート別ボスやキャラ専用ENDへ進めます。";
            if (sceneId.EndsWith("_deep_route"))
                return "地方深層です。ここは専用ENDや空港帰還準備のための別ルートです。STAGE本線のボスは各STAGE内のボスゲートにいます。";
            return "";
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
                bodyText.text = BuildChoiceDiceText(label, a, b, c, luckBonus);
                ShowDiceOverlay("LUK判定", a, b, c, luckBonus, -1);
                titleText.text = "LUK判定中";
                areaText.text = "[" + a + "] [" + b + "] [" + c + "]";
                footerText.text = "出目合計 " + (a + b + c) + " + LUK補正 " + luckBonus + "。目標値は選択内容で確定します。";
                Play(clickSfx, 0.18f);
                yield return new WaitForSeconds(0.08f + i * 0.018f);
            }
            HideDiceOverlay();

            var before = run.stats.Clone();
            var progressBefore = CaptureProgress();
            run.lastRollSummary = null;
            choice.effect?.Invoke(run);
            string delta = BuildStatDelta(before, run.stats, progressBefore);
            string outcome = BuildChoiceOutcomeText(choice, label, delta);
            LogRun(label + ": " + delta + (!string.IsNullOrEmpty(run.lastRollSummary) ? " / " + run.lastRollSummary : ""));
            if (!string.IsNullOrEmpty(run.lastRollSummary))
            {
                ShowRollSummaryOverlay("判定結果", run.lastRollSummary);
                titleText.text = run.lastRollSummary.Contains("成功") ? "LUK判定 成功" : "LUK判定 失敗";
                areaText.text = run.lastRollSummary.Contains("成功") ? "成功" : "失敗";
                footerText.text = OneLineRollSummary(run.lastRollSummary);
                yield return new WaitForSeconds(1.1f);
                HideDiceOverlay();
            }

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
            titleText.text = "足跡";
            areaText.text = "残ったもの";
            bool hasStructuredFlavor = !string.IsNullOrEmpty(outcome) && (outcome.Contains("直前の気配") || outcome.Contains("手を伸ばした先")) && outcome.Contains("残響");
            bool compactOutcome = !string.IsNullOrEmpty(outcome) && (outcome.StartsWith("踏み跡:") || outcome.StartsWith("兆し:"));
            bodyText.text = compactOutcome ? outcome : hasStructuredFlavor ? outcome + OutcomeAfterword(nextSceneId, outcome) : "足跡\n\n" + OutcomeFlavor(outcome) + "\n\n" + outcome + OutcomeAfterword(nextSceneId, outcome);
            footerText.text = "余韻が薄れるのを待つ。";
            UpdateSideText();
            ClearChoices();
            AddChoiceButton("次に進む", () => ShowScene(nextSceneId, false));
       }
        string OutcomeFlavor(string outcome)
        {
            if (string.IsNullOrEmpty(outcome))
                return "足音だけが、少し遅れてついてきた。";
            if (outcome.Contains("鯱") || outcome.Contains("井戸") || outcome.Contains("尾張"))
                return "金色の視線が、天井の配管を伝ってこちらを追う。選んだ道は尾張のどこかに結び目を作った。";
            if (outcome.Contains("味噌") || outcome.Contains("工場") || outcome.Contains("三河") || outcome.Contains("機械"))
                return "発酵する匂いと機械油が混ざり、判断の輪郭が少し硬くなる。三河の道は、耐えた者だけを通す。";
            if (outcome.Contains("空港") || outcome.Contains("海") || outcome.Contains("蒲郡") || outcome.Contains("知多"))
                return "遠くで搭乗案内が水音に変わる。近道は確かに近いが、足元の深さも増している。";
            if (outcome.Contains("神話"))
                return "理解は鍵ではなく、鍵穴のほうだった。覗いたぶんだけ、向こうからも覗き返される。";
            if (outcome.Contains("失敗") || outcome.Contains("足りない") || outcome.Contains("HP-") || outcome.Contains("SAN-"))
                return "選択の余韻が、冷たい痛みとして残った。それでも道はまだ閉じていない。";
            if (outcome.Contains("成功") || outcome.Contains("上回った") || outcome.Contains("所持金+") || outcome.Contains("LUK+"))
                return "迷いの中で、ひとつだけ正しい手触りがあった。";
            if (outcome.Contains("所持金-"))
                return "硬貨の音が現実側へ落ち、代わりに少しだけ安全が残った。";
            return "小さな判断が、次の景色の色を変えた。";
        }
        string BuildChoiceOutcomeText(Choice choice, string label, string delta)
        {
            bool luckOutcome = !string.IsNullOrEmpty(run.lastRollSummary) || (!string.IsNullOrEmpty(label) && label.Contains("LUK判定"));
            string detail = ChoiceResultDetail(choice, delta);
            string roll = luckOutcome
                ? CompactLine(!string.IsNullOrEmpty(run.lastRollSummary) ? OneLineRollSummary(run.lastRollSummary) : "LUK判定を実行")
                : "なし";
            return "踏み跡: " + CompactLine(CleanChoiceLabel(label), 42) + "\n" +
                "手応え: " + CompactLine(roll, 56) + "\n" +
                "残響: " + CompactLine(detail, 64) + "\n" +
                "爪痕: " + CompactStatDelta(delta);
        }
        string BuildChoiceDiceText(string label, int a, int b, int c, int luckBonus)
        {
            return "LUK判定\n\n" +
                ShortenForDice(label, 34) +
                "\n\n      [" + a + "]     [" + b + "]     [" + c + "]" +
                "\n\n出目合計 " + (a + b + c) + " + LUK補正 " + luckBonus +
                "\n目標: 選択内容で確定" +
                "\n\n境界の奥で、三つの骨片が同じ傷口を探している。";
        }
        string ShortenForDice(string text, int limit)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            text = text.Replace("\n", " / ");
            if (text.Length <= limit)
                return text;
            return text.Substring(0, limit - 1) + "…";
        }
        string CompactLine(string text, int limit = 64)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "なし";
            text = text.Replace("\r", " ").Replace("\n", " ");
            while (text.Contains("  "))
                text = text.Replace("  ", " ");
            text = text.Trim();
            int firstStop = text.IndexOf('。');
            if (firstStop >= 12 && firstStop < limit)
                text = text.Substring(0, firstStop + 1);
            if (text.Length <= limit)
                return text;
            return text.Substring(0, Mathf.Max(1, limit - 1)) + "…";
        }
        string CompactStatDelta(string delta)
        {
            if (string.IsNullOrWhiteSpace(delta) || delta.Contains("大きな変化なし") || delta.Contains("変化なし"))
                return "大きな変化なし";
            return CompactLine(delta, 70);
        }
        string ChoiceBeforeDetail(Choice choice, string label)
        {
            if (choice != null && !string.IsNullOrEmpty(choice.label))
                label = choice.label.Replace("\n", " / ");
            string action = CleanChoiceLabel(label);
            string destination = ChoiceDestinationName(choice);
            return "手を伸ばした先: " + action +
                "\n向かった先: " + destination +
                "\n胸の内: " + ChoiceIntentDetail(choice, action, destination) +
                "\n直前の気配: " + ChoiceOmenDetail(choice, action);
        }
        string CleanChoiceLabel(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                return "無名の行動";
            return label.Replace("\n", " / ").Trim();
        }
        string ChoiceDestinationName(Choice choice)
        {
            if (choice == null)
                return "現在地";
            if (!string.IsNullOrEmpty(choice.battle) && enemies.TryGetValue(choice.battle, out EnemyDef enemy))
                return "戦闘: " + enemy.name;
            if (!string.IsNullOrEmpty(choice.ending))
                return "END: " + choice.ending;
            if (!string.IsNullOrEmpty(choice.next) && scenes.TryGetValue(choice.next, out SceneDef scene))
            {
                string area = string.IsNullOrEmpty(scene.area) ? "" : " / " + scene.area;
                return scene.title + area;
            }
            if (!string.IsNullOrEmpty(choice.next))
                return choice.next;
            return "現在地";
        }
        SceneDef ChoiceScene(Choice choice)
        {
            if (choice == null)
                return null;
            foreach (SceneDef scene in scenes.Values)
                if (scene != null && scene.choices != null && scene.choices.Contains(choice))
                    return scene;
            return null;
        }
        bool ChoiceLabelContains(string label, params string[] words)
        {
            if (string.IsNullOrEmpty(label))
                return false;
            foreach (string word in words)
                if (!string.IsNullOrEmpty(word) && label.Contains(word))
                    return true;
            return false;
        }
        string ChoiceIntentDetail(Choice choice, string label, string destination)
        {
            if (ChoiceLabelContains(label, "LUK", "判定"))
                return "運だけに任せるのではなく、LUK補正と目標値を見極め、異界が開けた一瞬の隙間へ踏み込もうとした。";
            if (choice != null && !string.IsNullOrEmpty(choice.battle))
                return destination + "に正面から触れ、逃げ道を塞がれる前に主導権を取り返そうとした。";
            if (choice != null && !string.IsNullOrEmpty(choice.ending))
                return "ここまで集めた手掛かりと傷を抱え、物語を決着へ運ぶ最後の一歩を選んだ。";
            if (ChoiceLabelContains(label, "拠点", "休む", "相談", "買う"))
                return "名駅の拠点で装備、傷、証言を整え、次の探索で失うものを少しでも減らそうとした。";
            if (ChoiceLabelContains(label, "作戦", "安全", "方針"))
                return "危険な場所へ行く前に、どこを避け、誰を頼り、何を切り捨てるかを決めようとした。";
            if (ChoiceLabelContains(label, "尾張", "鯱", "熱田", "城", "大須"))
                return "尾張に残る社、城、商店街の痕跡をたどり、鯱の視線が示す封印の綻びを探ろうとした。";
            if (ChoiceLabelContains(label, "三河", "味噌", "工場", "岡崎", "豊田"))
                return "三河の発酵蔵と工場の工程に紛れた異常を読み、機械と菌糸のどちらが怪異を動かすのか確かめようとした。";
            if (ChoiceLabelContains(label, "知多", "海", "蒲郡", "空港", "常滑"))
                return "潮、霧、滑走路の向こうに続く道を調べ、海底から伸びる神話の航路を見つけようとした。";
            if (ChoiceLabelContains(label, "神話", "禁書", "予言", "記録"))
                return "読んではならない記録を読み、理解した瞬間に正気を削る知識を手掛かりへ変えようとした。";
            if (!string.IsNullOrEmpty(destination) && destination != "現在地")
                return destination + "へ進み、そこに残された手掛かりか出口を直接確かめようとした。";
            return "今いる場所で選べる手段を試し、停滞した状況に次の変化を起こそうとした。";
        }
        string ChoiceOmenDetail(Choice choice, string label)
        {
            if (ChoiceLabelContains(label, "LUK", "判定"))
                return "サイコロを握る指先に冷たい汗が浮き、必要な値だけが視界の中央で白く滲んだ。";
            if (choice != null && !string.IsNullOrEmpty(choice.battle))
                return "相手の輪郭が顔のない影へ沈み、攻撃の前触れだけが胸骨の奥で鳴った。";
            if (choice != null && !string.IsNullOrEmpty(choice.ending))
                return "背後の道が一つずつ消え、戻れる場所よりも進むべき門のほうが近くなった。";
            if (ChoiceLabelContains(label, "拠点", "休む", "相談", "買う"))
                return "机上の灯りが一度だけ暗くなり、地図の余白に知らない海岸線が浮かんだ。";
            if (ChoiceLabelContains(label, "尾張", "鯱", "熱田", "城", "大須"))
                return "屋根の上で見えない鯱が尾を振り、熱田の方角から黒い鳥居の影が伸びた。";
            if (ChoiceLabelContains(label, "三河", "味噌", "工場", "岡崎", "豊田"))
                return "甘い発酵臭に鉄粉の匂いが混じり、機械の拍子が心拍より少し遅れて響いた。";
            if (ChoiceLabelContains(label, "知多", "海", "蒲郡", "空港", "常滑"))
                return "潮の音に管制塔のノイズが重なり、霧の奥で水に濡れた誘導灯が点滅した。";
            if (ChoiceLabelContains(label, "神話", "禁書", "予言", "記録"))
                return "文字の隙間から黒い星空が覗き、読まれる前の知識が先にこちらの名を呼んだ。";
            return "選択肢の先にある場所の気配が濃くなり、足元の影だけが半歩早く動いた。";
        }
        string ChoiceResultDetail(Choice choice, string delta)
        {
            string label = choice != null ? choice.label : "";
            SceneDef scene = ChoiceScene(choice);
            if (!string.IsNullOrEmpty(run.lastRollSummary))
                return ChoiceLuckResultDetail(choice, scene, label, delta);
            string specific = ChoiceSpecificResultDetail(scene, label, delta);
            if (!string.IsNullOrEmpty(specific))
                return specific;
            if (scene != null)
                return ChoiceSceneContextResultDetail(scene, label, delta);
            if (label.Contains("拠点") || label.Contains("休む") || label.Contains("相談") || label.Contains("買う"))
                return "名駅の底に戻るたび、机の上の地図は少しだけ書き換わる。準備は数値以上に、次の恐怖へ入る姿勢を整えた。";
            if (label.Contains("作戦") || label.Contains("安全") || label.Contains("方針"))
                return "行き先を決める前に、避けるものを決めた。危険そのものは消えないが、踏む順番は選べる。";
            if (label.Contains("尾張") || label.Contains("鯱") || label.Contains("熱田") || label.Contains("城"))
                return "尾張の怪異は、真正面から倒すよりも由来を読んだほうが深く動く。鯱の注視は敵意であり、同時に道標でもある。";
            if (label.Contains("三河") || label.Contains("味噌") || label.Contains("工場") || label.Contains("岡崎") || label.Contains("豊田"))
                return "三河の異変は、発酵と工程のように遅れて効いてくる。耐性や機械適性が、戦わない突破口を作り始めた。";
            if (label.Contains("知多") || label.Contains("海") || label.Contains("蒲郡") || label.Contains("空港"))
                return "知多の海は空港へ近い。近いほど、波の下にある別の滑走路も見えてしまう。";
            if (label.Contains("神話"))
                return "理解した瞬間、風景のほうがあなたを理解し返す。知識は選択肢を増やすが、眠りを浅くする。";
            if (label.Contains("LUK判定"))
                return "サイコロは手の中で冷え、最後の一転だけ音がしなかった。出目は偶然ではなく、異界が許したわずかな隙間だった。";
            return "その行動はすぐに終わったように見えたが、足元の路線図に新しい細線が一本増えていた。";
        }
        string ChoiceLuckResultDetail(Choice choice, SceneDef scene, string label, string delta)
        {
            bool success = run.lastRollSummary.Contains("成功");
            string place = scene != null ? scene.title : ChoiceDestinationName(choice);
            string result = success ? "成功" : "失敗";
            string change = string.IsNullOrWhiteSpace(delta) ? "目に見える変化なし" : DeltaNarration(delta);
            if (ChoiceLabelContains(label, "海路で危険を避ける", "海路で空港へ近づく"))
            {
                if (success)
                    return place + "で潮の切れ目を読み、深い改札の巡回線を避けて空港側へ近づいた。海の下に沈む航路がひらき、空港知識と運が増えた。";
                return place + "で潮目を読み違え、足首をつかむ冷たい水に引き戻された。空港への手掛かりは少し得たが、濡れた階段で身体を打ちHPが減った。爪痕: " + change;
            }
            if (ChoiceLabelContains(label, "値切って買う"))
            {
                if (success)
                    return place + "で瓶の中の明日の偶然を言い負かした。安い代金で運を買い取れた。";
                return place + "で値切り文句を口にした瞬間、屋台の奥のものが笑った。代金を払ったうえで瓶の中身が逆流し、LUKとSANが削られた。爪痕: " + change;
            }
            if (ChoiceLabelContains(label, "人形の糸を切る"))
            {
                if (success)
                    return place + "で操り糸の結び目だけを切り落とした。舞台の役から逃れ、運がこちらへ戻った。";
                return place + "で糸を切る手元が一拍遅れ、人形の針が皮膚の下へ潜った。傷口から体温を奪われHPが減った。爪痕: " + change;
            }
            if (ChoiceLabelContains(label, "検査機の死角を抜ける"))
            {
                if (success)
                    return place + "で検査機のまばたきに合わせて死角を抜けた。保安検査の裏側にある空港境界の情報を拾えた。";
                return place + "で死角に入ったはずの身体を、検査機が骨の影だけで見つけた。警告音が肉体と正気を同時に削り、HPとSANが減った。爪痕: " + change;
            }
            if (ChoiceLabelContains(label, "自分の荷物だけ拾う"))
            {
                if (success)
                    return place + "で本物の荷物だけを見分け、噛みつく鞄を避けて拾い上げた。所持金と運を取り戻した。";
                return place + "で名札の声に反応した荷物が指へ噛みついた。手荷物タグが壊れ、振りほどく代償としてHPが減った。爪痕: " + change;
            }
            if (success)
                return place + "で伸ばした手は怪異の条件に噛み合い、" + change + " が残った。";
            return place + "で伸ばした手は一拍だけ遅れた。怪異はこちらの隙を代償に変えた。爪痕: " + change;
        }
        string ChoiceSpecificResultDetail(SceneDef scene, string label, string delta)
        {
            string place = scene != null ? scene.title : "";
            if (ChoiceLabelContains(label, "城下を調べる"))
                return place + "の城下で雨に濡れた石垣を追うと、鯱の影が古い井戸の位置を示した。地元知識は増えたが、同時に鯱の注視も強まった。";
            if (ChoiceLabelContains(label, "大須で情報収集"))
                return place + "でサーバー音のする路地を聞き込み、禁じられた噂を拾った。神話理解は増えたが、聞いた声が耳の奥に残りSANが削れた。";
            if (ChoiceLabelContains(label, "熱田で祓う"))
                return place + "で黒い鳥居の影に息を合わせ、まとわりついた汚染を一枚はがした。祓いは成功し、SANが戻り神話汚染が薄くなった。";
            if (ChoiceLabelContains(label, "井戸を封じ", "城の井戸"))
                return place + "で井戸の縁に神話の継ぎ目を縫い直した。防御は増えたが、封じたものは底からこちらを覚えた。";
            if (ChoiceLabelContains(label, "岡崎の蔵", "蔵を読む"))
                return place + "で樽の底から響く声を読み分けた。発酵した言葉の規則を掴み、味噌耐性と三河の手掛かりが増えた。";
            if (ChoiceLabelContains(label, "豊田で検査"))
                return place + "で試験ラインの点滅に合わせて異常工程を覚えた。機械適性は増えたが、検査アームの冷たい接触でHPが減った。";
            if (ChoiceLabelContains(label, "有松", "布を買う"))
                return place + "で絞り模様の裏に隠れ道を見つけた。代金は減ったが、布の結び目が次の危険を先に知らせる。";
            if (ChoiceLabelContains(label, "蒲郡で星図", "星図を読む"))
                return place + "で逆潮の上に置かれた星図を読んだ。空と海の位置が入れ替わるのを理解し、神話理解は増えたがSANが削れた。";
            if (ChoiceLabelContains(label, "所持金を申告する"))
                return place + "で記憶までトレーへ置くふりをして、申告書の余白から空港境界の規則を読んだ。所持金は減ったが空港知識が増えた。";
            if (ChoiceLabelContains(label, "荷物を捨てて走る"))
                return place + "で呼び続ける鞄を置き去りにし、ターンテーブルの隙間を走り抜けた。噛みつく荷札でHPは減ったが、空港の奥へ進む隙を得た。";
            if (ChoiceLabelContains(label, "温かい食事"))
                return place + "で湯気の立つ食事を飲み込むと、胃の中だけが現実へ戻った。所持金は減り、HPが回復した。";
            if (ChoiceLabelContains(label, "案内端末を読む"))
                return place + "で案内端末の地下階層を読み、通常の搭乗案内ではない経路を覚えた。空港知識は増えたが、読んだ文字が正気を削った。";
            if (ChoiceLabelContains(label, "戻る", "出発する", "すぐ出発"))
                return place + "からいったん身を引いた。大きな変化は少ないが、いま見たものは次の判断に残る。";
            return "";
        }
        string ChoiceSceneContextResultDetail(SceneDef scene, string label, string delta)
        {
            string action = CleanChoiceLabel(label);
            string place = scene != null ? scene.title : "この場所";
            string sceneLead = SceneLeadText(scene);
            string change = string.IsNullOrWhiteSpace(delta) ? "目に見える変化なし" : DeltaNarration(delta);
            string cause = "その選択により、場の均衡が少しだけ動いた。";
            if (delta.Contains("HP-"))
                cause = "踏み込んだ代償として身体に傷が残り、HPが減った。";
            else if (delta.Contains("SAN-"))
                cause = "見てはいけない規則を理解してしまい、SANが削れた。";
            else if (delta.Contains("所持金-"))
                cause = "現実側の代金を支払い、代わりに異界を進むための猶予を買った。";
            else if (delta.Contains("HP+"))
                cause = "この場で得た休息や補給が、身体を現実へ少し引き戻した。";
            else if (delta.Contains("LUK+"))
                cause = "偶然の向きがわずかに変わり、次の危険で拾える隙が増えた。";
            else if (delta.Contains("神話") || action.Contains("神話") || action.Contains("禁書"))
                cause = "神話の断片を理解したことで、道は開いたが眠りは浅くなった。";
            else if (delta.Contains("防御") || delta.Contains("攻撃") || delta.Contains("機械") || delta.Contains("味噌") || delta.Contains("地元"))
                cause = "この土地の仕組みを一つ覚え、次の怪異に対する備えが増えた。";
            return place + "で「" + action + "」へ踏み込んだ。" + sceneLead + cause + " 爪痕: " + change;
        }
        string SceneLeadText(SceneDef scene)
        {
            if (scene == null || string.IsNullOrWhiteSpace(scene.text))
                return "";
            string lead = scene.text.Replace("\r", "").Split('\n')[0].Trim();
            if (string.IsNullOrEmpty(lead))
                return "";
            if (lead.Length > 46)
                lead = lead.Substring(0, 45) + "…";
            return " " + lead + " ";
        }
        string ChoiceRollAftertaste(string rollSummary)
        {
            if (string.IsNullOrEmpty(rollSummary))
                return "";
            if (rollSummary.Contains("成功"))
                return "骨片は静かに止まり、足元の影が半歩だけ道を譲った。";
            if (rollSummary.Contains("失敗"))
                return "最後の出目が止まった瞬間、どこかで鍵の閉まる音がした。";
            return "出目の意味は、次の角を曲がるまで確定しない。";
        }
        string ChoiceAfterDetail(Choice choice, string delta)
        {
            string label = choice != null ? choice.label ?? "" : "";
            if (!string.IsNullOrEmpty(delta) && (delta.Contains("変化なし") || delta.Trim() == ""))
                return "目に見える変化はない。だが、異界では何も変わらないことも一つの記録になる。";
            if (delta.Contains("HP-") || delta.Contains("SAN-"))
                return "痛みはすぐに引かない。次に同じ匂いを嗅いだ時、身体のほうが先にこの場を思い出す。";
            if (delta.Contains("LUK+") || label.Contains("LUK"))
                return "見えないところで、明日の偶然が少しだけこちらへ傾いた。";
            if (delta.Contains("所持金"))
                return "硬貨の増減よりも、財布に残った冷たさのほうが長く残った。";
            return "選択の余波は、地図の余白に細く残った。次の行き先で、それが道か罠か分かる。";
        }
        string DeltaNarration(string delta)
        {
            if (string.IsNullOrWhiteSpace(delta))
                return "目に見える変化なし";
            if (delta.Contains("変化なし"))
                return delta + "。ただし、境界はその選択を記録した。";
            return delta;
        }
        string StatDeltaNarration(string delta)
        {
            string normalized = DeltaNarration(delta);
            var lines = new List<string>();
            lines.Add("数値爪痕: " + normalized);
            if (string.IsNullOrWhiteSpace(delta) || delta.Contains("変化なし"))
            {
                lines.Add("目に見える消耗や獲得はないが、イベントの記録は残った。");
                return string.Join("\n", lines);
            }
            string[] parts = delta.Split(new[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string raw in parts)
            {
                string part = raw.Trim();
                if (part.StartsWith("HP-"))
                    lines.Add("HPが減少: 身体に傷や疲労が残った。次の戦闘で余裕が少なくなる。");
                else if (part.StartsWith("HP+"))
                    lines.Add("HPが回復: 呼吸が整い、もう少し無理が利く。");
                else if (part.StartsWith("SAN-"))
                    lines.Add("SANが減少: 見てはいけないものを理解しかけた。正気崩壊に近づく。");
                else if (part.StartsWith("SAN+"))
                    lines.Add("SANが回復: 恐怖を一つ言葉にできた。心の揺れが少し戻る。");
                else if (part.StartsWith("所持金-"))
                    lines.Add("所持金が減少: 安全、情報、通行のために現実側の資源を支払った。");
                else if (part.StartsWith("所持金+"))
                    lines.Add("所持金が増加: 使える資金が増え、買い物や安全策の余地が広がった。");
                else if (part.StartsWith("LUK+"))
                    lines.Add("LUKが上昇: 次のLUK判定で補正や良い出目の期待が少し強くなる。");
                else if (part.StartsWith("LUK-"))
                    lines.Add("LUKが低下: 偶然に頼る行動が少し危うくなる。");
                else if (part.StartsWith("神話+"))
                    lines.Add("神話理解が上昇: 異界の仕組みを読めるが、理解したぶん戻れない。");
                else if (part.StartsWith("汚染+"))
                    lines.Add("神話汚染が上昇: 外なるものの理屈が身体に残った。");
                else if (part.StartsWith("攻撃+") || part.StartsWith("防御+") || part.StartsWith("速さ+"))
                    lines.Add(part + ": 戦闘や探索で使える能力が伸びた。");
                else if (part.StartsWith("攻撃-") || part.StartsWith("防御-") || part.StartsWith("速さ-"))
                    lines.Add(part + ": 行動性能が落ち、次の突破が重くなる。");
                else if (part.Contains("記憶片+"))
                    lines.Add(part + ": 周回後にも残る手がかりを得た。");
            }
            return string.Join("\n", lines);
        }
        string OutcomeAfterword(string nextSceneId, string outcome)
        {
            string next = "";
            if (!string.IsNullOrEmpty(nextSceneId) && scenes.TryGetValue(nextSceneId, out var scene))
                next = scene.title;
            string region = !string.IsNullOrEmpty(run.freedomRegion) ? run.freedomRegion : "";
            string line = "";
            if (region == "owari" || outcome.Contains("尾張") || outcome.Contains("鯱"))
                line = "尾張の気配はまだ途切れていない。戻るまでは、この地方の因果が続く。";
            else if (region == "mikawa" || outcome.Contains("三河") || outcome.Contains("味噌"))
                line = "三河の工程は中断を嫌う。次の判断にも、発酵した結果が残る。";
            else if (region == "chita" || outcome.Contains("知多") || outcome.Contains("海"))
                line = "潮の音は遠ざからない。空港へ近づくほど、海底の選択肢も増える。";
            else
                line = "名駅の底は、あなたの判断を静かに記録している。";
            if (!string.IsNullOrEmpty(next))
                line += "\n次の行き先: " + next;
            return "\n\n余韻\n" + line;
        }
        string RunStateResonance()
        {
            if (run == null)
                return "";
            var parts = new List<string>();
            if (run.stats.localKnowledge >= 4)
                parts.Add("地元知識が高く、地方イベント判定に地の利が乗りやすい。");
            if (run.stats.localKnowledge >= 6)
                parts.Add("地元知識が深まり、近道・回避・隠しNPCの選択肢が開く。");
            if (run.shachiGaze >= 3)
                parts.Add("鯱の注視が濃い。尾張や空港周辺で道標にも圧力にもなる。");
            if (run.shachiGaze >= 4)
                parts.Add("鯱注視が臨界に近い。鯱の導きや強制遭遇が起こりやすい。");
            if (run.dangerWarnings >= 4)
                parts.Add("危険察知が蓄積し、空港へ向かう判断材料が揃いつつある。");
            if (run.stats.mythosKnowledge >= 5)
                parts.Add("神話知識が深く、専用の選択肢やENDへ踏み込める。");
            if (parts.Count == 0)
                return "";
            return string.Join("\n", parts);
        }
        bool TryShowRandomEvent(string targetSceneId)
        {
            if (run == null || run.steps <= 0 || run.randomCooldown > 0)
                return false;
            if (!string.IsNullOrEmpty(run.pendingSceneAfterRandom))
                return false;
            if (targetSceneId == "airport_gate" || targetSceneId == "nagoya_start")
                return false;
            float chance = 0.48f + run.instability * 0.055f + Mathf.Clamp01(run.stats.mythosCorruption * 0.045f) + Mathf.Clamp01(run.shachiGaze * 0.025f) + Mathf.Clamp01(progress.endings.Count * 0.02f);
            if (rng.NextDouble() > chance)
            {
                if (run.steps >= 3 && rng.NextDouble() < 0.22f + Mathf.Clamp01(run.shachiGaze * 0.025f))
                {
                    string ambush = SelectAmbushEnemy(targetSceneId);
                    if (!string.IsNullOrEmpty(ambush))
                    {
                        run.pendingSceneAfterRandom = targetSceneId;
                        run.battleReturnScene = targetSceneId;
                        run.randomCooldown = 2;
                        Play(eventSfx, 0.72f);
                        LogRun("敵の気配: " + EnemyName(ambush));
                        ShowRiskAmbushEvent(targetSceneId, ambush);
                        return true;
                    }
                }
                return false;
            }
            run.pendingSceneAfterRandom = targetSceneId;
            run.randomCooldown = 2;
            LogRun("名のない気配発生");
            ShowRandomEvent(targetSceneId);
            return true;
        }
        void ApplyLoopPressure()
        {
            if (run == null || run.steps <= 0)
                return;
            if (run.steps % 7 == 0)
            {
                run.dangerWarnings += 1;
                LogRun("長居で危険察知+1");
            }
            if (run.steps % 13 == 0)
            {
                run.instability += 1;
                run.stats.sanity = Math.Max(0, run.stats.sanity - 1);
                LogRun("周回圧: 不安定度+1/SAN-1");
            }
        }
        string SelectAmbushEnemy(string sceneId)
        {
            if (!scenes.TryGetValue(sceneId, out var scene))
                return "";
            string image = scene.image;
            if (run != null && run.shachiGaze >= 6 && rng.NextDouble() < 0.38)
                return "window_god";
            if (image == "castle") return rng.NextDouble() < 0.5 ? "battlefield_spear" : "well_tentacle";
            if (image == "okazaki" || image == "miso") return rng.NextDouble() < 0.5 ? "miso_voice" : "shadow_retainer";
            if (image == "airport") return rng.NextDouble() < 0.5 ? "gate_guard" : "baggage_mouth";
            if (image == "toyota") return "quality_golem";
            if (image == "tokoname") return "kiln_crawler";
            if (image == "osu") return rng.NextDouble() < 0.5 ? "index_hound" : "dream_eater";
            if (image == "station" || image == "kishimen") return rng.NextDouble() < 0.5 ? "last_train" : "piyorin";
            return rng.NextDouble() < 0.5 ? "index_hound" : "dream_eater";
        }
        void ShowRiskAmbushEvent(string targetSceneId, string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId) || !enemies.TryGetValue(enemyId, out var enemy))
            {
                ShowScene(targetSceneId, false);
                return;
            }
            SetBackground(enemy.image);
            SetPortrait(enemy.portrait);
            titleText.text = "敵の気配";
            areaText.text = "リスク遭遇 / 回避可能";
            int risk = AmbushAvoidTarget(enemyId, targetSceneId);
            int preview = 10 + AmbushAvoidBonus(targetSceneId);
            bodyText.text = "進路の先で、" + enemy.name + " の気配が濃くなっている。\n\nまだ襲われてはいない。ここで危険を避ければ消耗は抑えられるが、迂回したぶんだけ手がかりを取り逃がすかもしれない。\n\n回避目標: " + risk + " / 予兆: " + DiceForecast(preview, risk) + "\n回避補正: " + AmbushAvoidBonusLabel(targetSceneId);
            footerText.text = "敵はランダムに出現します。戦うか、リスクを払って避けるかを選べます。";
            UpdateSideText();
            ClearChoices();
            AddChoiceButton("危険を避ける\n地元/鯱/LUK判定", () => StartCoroutine(ResolveAmbushAvoidance(targetSceneId, enemyId, risk)));
            AddChoiceButton("踏み込んで戦う", () =>
            {
                run.battleReturnScene = targetSceneId;
                StartBattle(enemyId);
            });
            AddChoiceButton("距離を取り直す\n危険察知+1/SAN-1", () =>
            {
                var before = run.stats.Clone();
                run.dangerWarnings += 1;
                run.stats.sanity = Math.Max(0, run.stats.sanity - 1);
                string delta = BuildStatDelta(before, run.stats);
                ShowChoiceOutcome(targetSceneId, "敵の気配から距離を取る\n追跡音は消えないが、今はまだ戦わずに済んだ。\n残響: " + delta);
            });
        }
        IEnumerator ResolveAmbushAvoidance(string targetSceneId, string enemyId, int target)
        {
            ClearChoices();
            int bonus = AmbushAvoidBonus(targetSceneId);
            for (int i = 0; i < 8; i++)
            {
                int ra = rng.Next(1, 7);
                int rb = rng.Next(1, 7);
                int rc = rng.Next(1, 7);
                bodyText.text = "敵の気配を避ける道を探している...\n\n[" + ra + "] [" + rb + "] [" + rc + "]  " + AmbushAvoidBonusLabel(targetSceneId);
                Play(clickSfx, 0.18f);
                yield return new WaitForSeconds(0.08f + i * 0.018f);
            }
            int a = rng.Next(1, 7);
            int b = rng.Next(1, 7);
            int c = rng.Next(1, 7);
            int total = a + b + c + bonus;
            run.lastRollSummary = "回避手応え: " + a + "/" + b + "/" + c + " + " + bonus + " = " + total + " / 目標 " + target + "\n" + LuckMarginText(total, target);
            if (total >= target)
            {
                var before = run.stats.Clone();
                run.dangerWarnings += 1;
                if (run.stats.localKnowledge >= 6)
                    run.stats.localKnowledge += 1;
                string delta = BuildStatDelta(before, run.stats);
                ShowChoiceOutcome(targetSceneId, "敵の気配を回避する\n土地勘と鯱の圧を読み、怪異の巡回線から一歩外れた。\n残響: " + delta + "\n" + run.lastRollSummary);
            }
            else
            {
                var before = run.stats.Clone();
                run.stats.sanity = Math.Max(0, run.stats.sanity - 1);
                string delta = BuildStatDelta(before, run.stats);
                bodyText.text = "回避しようとした足音を、敵が聞き分けた。\n\n残響: " + delta + "\n" + run.lastRollSummary + "\n\n戦闘に入る。";
                UpdateSideText();
                yield return new WaitForSeconds(0.45f);
                run.battleReturnScene = targetSceneId;
                StartBattle(enemyId);
            }
        }
        int AmbushAvoidTarget(string enemyId, string targetSceneId)
        {
            int target = 13 + Mathf.Clamp(run.instability / 2, 0, 4);
            if (!string.IsNullOrEmpty(enemyId) && enemyId.Contains("stage"))
                target += 2;
            if (targetSceneId != null && targetSceneId.Contains("airport"))
                target += 1;
            return target;
        }
        int AmbushAvoidBonus(string targetSceneId)
        {
            int bonus = Mathf.Clamp(run.stats.luck / 3, 0, 4);
            bonus += Mathf.Clamp(run.stats.localKnowledge / 3, 0, 4);
            bonus += Mathf.Clamp(run.dangerWarnings / 3, 0, 3);
            if (targetSceneId != null && (targetSceneId.Contains("owari") || targetSceneId.Contains("castle") || targetSceneId.Contains("airport")))
                bonus += Mathf.Clamp(run.shachiGaze / 3, 0, 2);
            if (run.flags.Contains("shachi_truce"))
                bonus += 1;
            return Mathf.Clamp(bonus, 0, 10);
        }
        string AmbushAvoidBonusLabel(string targetSceneId)
        {
            return "LUK/地元/危険察知/鯱 +" + AmbushAvoidBonus(targetSceneId);
        }
        string BuildChoiceLabel(Choice choice, bool enabled)
        {
            string label = enabled ? choice.label : choice.label + "\n" + choice.disabledReason;
            int limit = ChoiceUseLimit(choice);
            if (limit > 0)
            {
                int used = ChoiceUseCount(choice);
                if (used >= limit)
                    label = choice.label + "\n再使用: 危険上昇";
                else if (used > 0)
                    label += "\n残り " + (limit - used) + " 回";
            }
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
        string ChoiceUseKey(Choice choice)
        {
            if (choice == null || string.IsNullOrEmpty(choice.label))
                return "";
            return choice.label.Replace("\n", "/");
        }
        int ChoiceUseCount(Choice choice)
        {
            if (run == null || run.choiceUses == null)
                return 0;
            return run.choiceUses.TryGetValue(ChoiceUseKey(choice), out int count) ? count : 0;
        }
        bool IsChoiceExhausted(Choice choice)
        {
            int limit = ChoiceUseLimit(choice);
            return limit > 0 && ChoiceUseCount(choice) >= limit;
        }
        bool IsForcedRandomEventChoice(Choice choice)
        {
            if (choice == null || string.IsNullOrEmpty(choice.label))
                return false;
            string label = choice.label;
            return label.Contains("名のない気配") || label.Contains("出来事を探す");
        }
        void RecordChoiceUse(Choice choice)
        {
            int limit = ChoiceUseLimit(choice);
            if (limit <= 0 || run == null)
                return;
            if (run.choiceUses == null)
                run.choiceUses = new Dictionary<string, int>();
            string key = ChoiceUseKey(choice);
            int before = run.choiceUses.TryGetValue(key, out int count) ? count : 0;
            run.choiceUses[key] = before + 1;
            if (before > 0)
            {
                run.dangerWarnings += 1;
                if (before % 2 == 1)
                    run.instability += 1;
                LogRun("同じ行動の反復で危険上昇");
            }
        }
        int ChoiceUseLimit(Choice choice)
        {
            if (choice == null || choice.effect == null)
                return 0;
            string label = choice.label ?? "";
            if (label.Contains("戻る") || label.Contains("方面へ") || label.Contains("ゴールを確認") || label.Contains("STAGE") || label.Contains("ENDへ"))
                return 0;
            if (label.Contains("空港へ向かう") || label.Contains("何も決めず"))
                return 0;
            if (label.Contains("短く休む"))
                return 2;
            if (label.Contains("相談") || label.Contains("見る") || label.Contains("確認"))
                return 2;
            if (label.Contains("買う") || label.Contains("護符") || label.Contains("救急") || label.Contains("保険"))
                return 2;
            if (label.Contains("名のない気配") || label.Contains("出来事を探す"))
                return 0;
            return 1;
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
            ShowExpandedRandomEvent(targetSceneId);
            return;
            string area = scenes.ContainsKey(targetSceneId) ? scenes[targetSceneId].area : "異界愛知";
            int roll = rng.Next(0, 100);
            if ((run != null && !string.IsNullOrEmpty(run.freedomRegion)) || rng.NextDouble() < 0.72)
            {
                ShowExpandedRandomEvent(targetSceneId);
                return;
            }
            if (area.Contains("岡崎") && roll < 35)
            {
                SetBackground("miso");
                SetPortrait("event_miso_voice");
                titleText.text = "樽鳴り";
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
                    AwardRareMemory(2);
                    SaveProgress();
                    ShowScene(run.pendingSceneAfterRandom, false);
                });
            }
            else if (area.Contains("名古屋") && roll < 35)
            {
                SetBackground("kishimen");
                SetPortrait("event_cafe_server");
                titleText.text = "深夜モーニング";
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
                titleText.text = "招き猫の瞬き";
                areaText.text = "常滑 / 境界";
                bodyText.text = "巨大な招き猫が、ゆっくりと瞬きした。\n\nその一瞬だけ、海上空港の灯りが別の世界の星座に見える。";
                AddChoiceButton("礼をして通る", () =>
                {
                    run.stats.localKnowledge += 1;
                    AwardRareMemory(1);
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
                titleText.text = "黄色い行列";
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
                titleText.text = "怪しい自販機";
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
                    ShowChoiceOutcome(run.pendingSceneAfterRandom, "300円入れる\n" + note + "\n残響: " + delta);
                });
                AddChoiceButton("叩いてみる", () =>
                {
                    var before = run.stats.Clone();
                    run.stats.hp = Math.Max(1, run.stats.hp - 1);
                    int memory = AwardRareMemory(2);
                    SaveProgress();
                    string delta = BuildStatDelta(before, run.stats) + (memory > 0 ? " / 記憶片+" + memory : "");
                    LogRun("怪しい自販機: " + delta);
                    ShowChoiceOutcome(run.pendingSceneAfterRandom, "叩いてみる\n自販機の奥で、未来の領収書が破れる音がした。\n残響: " + delta);
                });
            }
            else if (roll < 63)
            {
                SetBackground("castle");
                SetPortrait("event_shachi_avatar");
                titleText.text = "鯱の影";
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
                titleText.text = "工程表の囁き";
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
                titleText.text = "前回のあなた";
                areaText.text = "周回記憶";
                bodyText.text = "路肩に、あなたと同じ顔の人物が座っている。\n\nその人は何も言わず、掌の中の記憶片を差し出した。";
                AddChoiceButton("受け取る", () =>
                {
                    var before = run.stats.Clone();
                    int memory = AwardRareMemory(4);
                    run.stats.mythosKnowledge += 1;
                    SaveProgress();
                    string delta = BuildStatDelta(before, run.stats) + (memory > 0 ? " / 記憶片+" + memory : "");
                    LogRun("前回のあなた: " + delta);
                    ShowChoiceOutcome(run.pendingSceneAfterRandom, "受け取る\n掌の記憶片は温かかった。あなたが忘れた痛みだけが、まだ生きている。\n残響: " + delta);
                });
                AddChoiceButton("埋葬する", () =>
                {
                    var before = run.stats.Clone();
                    run.stats.sanity = Math.Min(run.stats.maxSanity, run.stats.sanity + 3);
                    int memory = AwardRareMemory(1);
                    SaveProgress();
                    string delta = BuildStatDelta(before, run.stats) + (memory > 0 ? " / 記憶片+" + memory : "");
                    LogRun("前回のあなた: " + delta);
                    ShowChoiceOutcome(run.pendingSceneAfterRandom, "埋葬する\n土をかけるたびに、同じ顔の誰かが少しだけ眠れる顔になった。\n残響: " + delta);
                });
            }
            UpdateSideText();
            footerText.text = "境界が割り込んだ。次に同じ形で現れるとは限らない。";
        }
        bool IsFreedomRegionEvent(RandomEventDef e, string region)
        {
            if (e == null || string.IsNullOrEmpty(region))
                return true;
            string key = (e.image ?? "") + " " + (e.area ?? "") + " " + (e.id ?? "");
            if (region == "owari")
                return key.Contains("castle") || key.Contains("osu") || key.Contains("atsuta") || key.Contains("tsuruma") || key.Contains("inuyama") || key.Contains("station") || key.Contains("owari") || key.Contains("名古屋") || key.Contains("尾張") || key.Contains("熱田") || key.Contains("大須") || key.Contains("犬山") || key.Contains("鶴舞");
            if (region == "mikawa")
                return key.Contains("miso") || key.Contains("okazaki") || key.Contains("toyota") || key.Contains("arimatsu") || key.Contains("chiryu") || key.Contains("nagakute") || key.Contains("toyokawa") || key.Contains("mikawa") || key.Contains("岡崎") || key.Contains("豊田") || key.Contains("有松") || key.Contains("知立") || key.Contains("長久手") || key.Contains("三河");
            if (region == "chita")
                return key.Contains("handa") || key.Contains("tokoname") || key.Contains("gamagori") || key.Contains("nishio") || key.Contains("chita") || key.Contains("laguna") || key.Contains("sea") || key.Contains("半田") || key.Contains("常滑") || key.Contains("蒲郡") || key.Contains("知多") || key.Contains("西尾") || key.Contains("海");
            return true;
        }
        void ShowExpandedRandomEvent(string targetSceneId)
        {
            var events = ExpandedRandomEvents();
            if (run != null && !string.IsNullOrEmpty(run.freedomRegion))
            {
                var regionalEvents = new List<RandomEventDef>();
                foreach (var candidate in events)
                {
                    if (IsFreedomRegionEvent(candidate, run.freedomRegion))
                        regionalEvents.Add(candidate);
                }
                if (regionalEvents.Count > 0)
                    events = regionalEvents;
            }
            var unseen = new List<RandomEventDef>();
            if (run != null)
            {
                if (run.seenRandomEvents == null)
                    run.seenRandomEvents = new HashSet<string>();
                foreach (var candidate in events)
                {
                    if (!run.seenRandomEvents.Contains(candidate.id))
                        unseen.Add(candidate);
                }
            }
            if (unseen.Count > 0)
                events = unseen;
            else if (run != null)
            {
                run.instability += 1;
                run.dangerWarnings += 1;
                LogRun("既知の怪異が巡回を強めた");
            }
            var e = events[rng.Next(events.Count)];
            if (run != null && run.seenRandomEvents != null)
                run.seenRandomEvents.Add(e.id);
            SetBackground(e.image);
            SetPortrait(e.portrait);
            SetStoryPanelExpanded(false);
            titleText.text = e.title;
            areaText.text = e.area;
            int preview = PreviewEventRoll(e);
            string checkName = EventCheckName(e);
            bodyText.text = BuildRandomEventIntroText(e) + "\n\n判定目標: " + e.difficulty + " / 予兆: " + DiceForecast(preview, e.difficulty) +
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
                    var progressBefore = CaptureProgress();
                    e.success?.Invoke(run);
                    string delta = BuildStatDelta(before, run.stats, progressBefore);
                    LogRun(e.title + ": 成功 " + delta);
                    SetStoryPanelExpanded(true);
                    bodyText.text = BuildRandomEventResultText(e, true, delta);
                    footerText.text = "結果、能力変化、後味は本文に表示しています。";
                    Play(rewardSfx, 0.65f);
                   UpdateSideText();
                    ClearChoices();
                    AddChoiceButton("次に進む", () => ShowScene(run.pendingSceneAfterRandom, false));
                }
                else
                {
                    var before = run.stats.Clone();
                    var progressBefore = CaptureProgress();
                    e.fail?.Invoke(run);
                    string delta = BuildStatDelta(before, run.stats, progressBefore);
                    LogRun(e.title + ": 失敗 " + delta);
                    SetStoryPanelExpanded(true);
                    bodyText.text = BuildRandomEventResultText(e, false, delta);
                    footerText.text = "結果、能力変化、後味は本文に表示しています。";
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
                    SetStoryPanelExpanded(true);
                    bodyText.text = BuildRandomEventSafeResultText(e, "金で安全な抜け道を買った。", "所持金-180");
                    footerText.text = "安全策の結果と代償は本文に表示しています。";
                }
               else
                {
                    var before = run.stats.Clone();
                    var progressBefore = CaptureProgress();
                    run.stats.hp = Math.Max(1, run.stats.hp - 3);
                    run.stats.sanity = Math.Max(0, run.stats.sanity - 1);
                    string delta = BuildStatDelta(before, run.stats, progressBefore);
                    LogRun(e.title + ": 安全策 " + delta);
                    SetStoryPanelExpanded(true);
                    bodyText.text = BuildRandomEventSafeResultText(e, "払える金が足りず、身体で代償を払った。", delta);
                    footerText.text = "安全策の結果と代償は本文に表示しています。";
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
                bodyText.text = BuildRandomEventDiceText(e, ra, rb, rc, checkBonus);
                ShowDiceOverlay(EventCheckName(e), ra, rb, rc, checkBonus, e.difficulty);
                titleText.text = EventCheckName(e) + "中";
                areaText.text = "[" + ra + "] [" + rb + "] [" + rc + "]";
                footerText.text = DiceProgressText(ra, rb, rc, checkBonus, e.difficulty);
                Play(clickSfx, 0.2f);
                yield return new WaitForSeconds(0.08f + i * 0.018f);
            }
            HideDiceOverlay();
            int roll = a + b + c + checkBonus;
            bool success = roll >= e.difficulty;
            run.lastRollSummary = EventRollDetail(e, a, b, c, checkBonus, roll, e.difficulty);
            ShowDiceOverlayResult(EventCheckName(e), a, b, c, checkBonus, e.difficulty, success);
            titleText.text = EventCheckName(e) + (success ? " 成功" : " 失敗");
            areaText.text = success ? "成功" : "失敗";
            footerText.text = DiceProgressText(a, b, c, checkBonus, e.difficulty) + " / " + (success ? "成功" : "失敗");
            yield return new WaitForSeconds(1.0f);
            HideDiceOverlay();
            if (success)
            {
                var before = run.stats.Clone();
                var progressBefore = CaptureProgress();
                e.success?.Invoke(run);
                ApplyRandomEventAftermath(e, true);
                string delta = BuildStatDelta(before, run.stats, progressBefore);
               LogRun(e.title + ": 成功 " + delta);
                SetStoryPanelExpanded(true);
                bodyText.text = BuildRandomEventResultText(e, true, delta);
                footerText.text = "結果、能力変化、後味は本文に表示しています。";
                Play(rewardSfx, 0.65f);
                UpdateSideText();
                ClearChoices();
                AddChoiceButton("次に進む", () => ShowScene(run.pendingSceneAfterRandom, false));
            }
            else
            {
                var before = run.stats.Clone();
                var progressBefore = CaptureProgress();
                e.fail?.Invoke(run);
                ApplyRandomEventAftermath(e, false);
                string delta = BuildStatDelta(before, run.stats, progressBefore);
                LogRun(e.title + ": 失敗 " + delta);
                SetStoryPanelExpanded(true);
                bodyText.text = BuildRandomEventResultText(e, false, delta);
                footerText.text = "結果、能力変化、後味は本文に表示しています。";
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
        string BuildRandomEventResultText(RandomEventDef e, bool success, string delta)
        {
            string roll = !string.IsNullOrEmpty(run.lastRollSummary)
                ? OneLineRollSummary(run.lastRollSummary)
                : EventCheckName(e) + " " + (success ? "成功" : "失敗");
            string outcome = success ? e.successText : e.failText;
            if (string.IsNullOrWhiteSpace(outcome))
                outcome = RandomEventSpecificOutcome(e, success, delta);
            return "兆し: " + CompactLine(RandomEventBeforeText(e), 58) + "\n" +
                "手応え: " + CompactLine(roll, 58) + "\n" +
                "残響: " + CompactLine(outcome, 66) + "\n" +
                "爪痕: " + CompactStatDelta(delta);
        }
        void ApplyRandomEventAftermath(RandomEventDef e, bool success)
        {
            if (run == null || e == null)
                return;
            if (success && HasGearEffect("stage_bonus"))
                run.stats.money += 40;
            if (success && run.character.id == "local" && (e.area.Contains("名駅") || e.area.Contains("尾張")))
                run.dangerWarnings += 1;
            if (success && run.character.id == "worker" && (e.area.Contains("工場") || e.area.Contains("機械") || e.area.Contains("豊田")))
                run.stats.money += 80;
            if (success && run.character.id == "occult" && (e.id.StartsWith("mythos_") || e.area.Contains("神話") || e.area.Contains("異界")))
                run.stats.mythosKnowledge += 1;
            if (!success && HasGearEffect("miso_guard") && (e.area.Contains("三河") || e.area.Contains("味噌") || e.area.Contains("発酵")))
                run.stats.sanity = Math.Min(run.stats.maxSanity, run.stats.sanity + 1);
            if (!success && HasGearEffect("memory_anchor") && rng.NextDouble() < 0.2)
                AwardRareMemory(1);
        }

        string BuildRandomEventDiceText(RandomEventDef e, int a, int b, int c, int bonus)
        {
            string checkName = EventCheckName(e);
            string title = e != null ? e.title : "名のない気配";
            int target = e != null ? e.difficulty : 0;
            return checkName + "\n\n" +
                ShortenForDice(title, 30) +
                "\n\n      [" + a + "]     [" + b + "]     [" + c + "]" +
                "\n\n" + DiceProgressText(a, b, c, bonus, target) +
                "\n\n出目が止まるまで、怪異はまだこちらの名前を知らない。";
        }

        void ShowDiceOverlay(string heading, int a, int b, int c, int bonus, int target)
        {
            if (diceOverlayRoot == null || diceOverlayText == null)
                return;
            diceOverlayRoot.gameObject.SetActive(true);
            diceOverlayText.text = heading + "\n\n[" + a + "]   [" + b + "]   [" + c + "]\n\n" + DiceProgressText(a, b, c, bonus, target);
            diceOverlayText.fontSize = 28;
            LayoutRebuilder.ForceRebuildLayoutImmediate(diceOverlayRoot);
            Canvas.ForceUpdateCanvases();
        }

        void ShowDiceOverlayResult(string heading, int a, int b, int c, int bonus, int target, bool success)
        {
            if (diceOverlayRoot == null || diceOverlayText == null)
                return;
            diceOverlayRoot.gameObject.SetActive(true);
            diceOverlayText.text = heading + " " + (success ? "成功" : "失敗") +
                "\n\n[" + a + "]   [" + b + "]   [" + c + "]" +
                "\n\n" + DiceProgressText(a, b, c, bonus, target);
            diceOverlayText.fontSize = 28;
            LayoutRebuilder.ForceRebuildLayoutImmediate(diceOverlayRoot);
            Canvas.ForceUpdateCanvases();
        }

        void ShowRollSummaryOverlay(string heading, string summary)
        {
            if (diceOverlayRoot == null || diceOverlayText == null)
                return;
            diceOverlayRoot.gameObject.SetActive(true);
            diceOverlayText.text = heading + "\n\n" + FormatRollSummaryForOverlay(summary);
            diceOverlayText.fontSize = 24;
            LayoutRebuilder.ForceRebuildLayoutImmediate(diceOverlayRoot);
            Canvas.ForceUpdateCanvases();
        }

        string FormatRollSummaryForOverlay(string summary)
        {
            if (string.IsNullOrEmpty(summary))
                return "";
            string text = summary.Replace(" / 目標 ", "\n目標 ");
            text = text.Replace("LUK手応え: ", "LUK判定\n");
            text = text.Replace("SAN手応え: ", "SAN判定\n");
            text = text.Replace("神話手応え: ", "神話判定\n");
            return text;
        }

        string OneLineRollSummary(string summary)
        {
            if (string.IsNullOrEmpty(summary))
                return "";
            return summary.Replace("\n", " / ");
        }

        string DiceProgressText(int a, int b, int c, int bonus, int target)
        {
            int dice = a + b + c;
            int total = dice + bonus;
            if (target > 0)
            {
                int margin = total - target;
                string state = margin >= 0 ? "成功圏 +" + margin : "不足 " + (-margin);
                return "出目合計 " + dice + " + 補正 " + bonus + " = " + total + "\n目標 " + target + " / " + state;
            }
            return "出目合計 " + dice + " + 補正 " + bonus + " = " + total + "\n目標: 選択内容で確定";
        }

        void HideDiceOverlay()
        {
            if (diceOverlayRoot != null)
                diceOverlayRoot.gameObject.SetActive(false);
        }

        string BuildRandomEventIntroText(RandomEventDef e)
        {
            return "床下からの呼び声\n\n" + RandomEventBeforeText(e);
        }

        string BuildRandomEventSafeResultText(RandomEventDef e, string happened, string delta)
        {
            return "兆し: " + CompactLine(RandomEventBeforeText(e), 58) + "\n" +
                "手応え: 身を低くした\n" +
                "残響: " + CompactLine(happened, 66) + "\n" +
                "爪痕: " + CompactStatDelta(delta);
        }

        string RandomEventBeforeText(RandomEventDef e)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.text))
                return "境界の空気が薄くなる。まだ何も起きていないが、選ばなければならない気配だけが先に来ている。";
            return e.text;
        }
        string RandomEventAftermath(RandomEventDef e, bool success)
        {
            if (e == null)
                return success ? "境界は一度だけ道を譲った。" : "境界は代償の形だけを残した。";
            string place = string.IsNullOrWhiteSpace(e.area) ? "境界" : e.area;
            string motif = EventMythosMotif(e);
            string trace = EventTraceObject(e);
            string action = success ? e.successLabel : e.failLabel;
            if (string.IsNullOrWhiteSpace(action))
                action = success ? "踏み込んだ一歩" : "退いた判断";

            if (success)
            {
                return e.title + "のあと、" + place + "には" + motif + "の気配だけが薄く残った。\n" +
                    action + "は怪異を倒したのではなく、名状しがたい規則の一行を一時的に書き換えたにすぎない。\n" +
                    trace + "はまだ温かく、次に同じ場所を通る時、こちらの名前を先に思い出す。";
            }
            return e.title + "のあと、" + place + "の暗がりは" + motif + "を飲み込みきれず、床の下で小さく蠢いた。\n" +
                action + "は逃げ道ではあったが、怪異にこちらの癖を教える合図にもなった。\n" +
                trace + "には見覚えのない濡れた印が残り、次の失敗を待っている。";
        }
        string RandomEventAftermathShort(RandomEventDef e, bool success)
        {
            if (e == null)
                return success ? "境界は一度だけ道を譲った。" : "境界は代償の形だけを残した。";
            string trace = EventTraceObject(e);
            if (success)
                return trace + "に、次の選択で使える小さな手掛かりが残った。";
            return trace + "に濡れた印が残り、怪異はあなたの癖を一つ覚えた。";
        }

        string EventMythosMotif(RandomEventDef e)
        {
            string key = ((e.id ?? "") + " " + (e.title ?? "") + " " + (e.area ?? "") + " " + (e.image ?? "") + " " + (e.portrait ?? "")).ToLowerInvariant();
            if (key.Contains("airport") || key.Contains("空港") || key.Contains("滑走") || key.Contains("搭乗"))
                return "海底から伸びる滑走路と、星辰のずれた搭乗案内";
            if (key.Contains("sea") || key.Contains("chita") || key.Contains("蒲郡") || key.Contains("知多") || key.Contains("海") || key.Contains("ferry"))
                return "潮の下で眠る鰓のある司祭と、逆さに沈む星図";
            if (key.Contains("miso") || key.Contains("koji") || key.Contains("岡崎") || key.Contains("発酵") || key.Contains("味噌"))
                return "発酵槽の底で増える胞子状の祈り";
            if (key.Contains("toyota") || key.Contains("factory") || key.Contains("quality") || key.Contains("工場") || key.Contains("機械"))
                return "工程表に紛れた非ユークリッドな検査手順";
            if (key.Contains("atsuta") || key.Contains("sword") || key.Contains("熱田") || key.Contains("剣"))
                return "封じ縄の隙間から漏れる古い神名";
            if (key.Contains("castle") || key.Contains("shachi") || key.Contains("名古屋城") || key.Contains("鯱"))
                return "金鯱の鱗に映る二重の月";
            if (key.Contains("osu") || key.Contains("server") || key.Contains("radio") || key.Contains("大須") || key.Contains("電波"))
                return "壊れた周波数で唱えられる召喚ログ";
            if (key.Contains("station") || key.Contains("meieki") || key.Contains("名駅") || key.Contains("地下"))
                return "地下路線図の裏に印刷された異星の循環器";
            if (key.Contains("library") || key.Contains("index") || key.Contains("書庫") || key.Contains("禁書"))
                return "索引カードに棲む猟犬の足音";
            if (key.Contains("puppet") || key.Contains("chiryu") || key.Contains("人形") || key.Contains("糸"))
                return "見えない手から垂れる赤い操り糸";
            if (key.Contains("lantern") || key.Contains("fox") || key.Contains("toyokawa") || key.Contains("灯") || key.Contains("狐"))
                return "狐火に偽装した異界の眼球";
            if (key.Contains("tea") || key.Contains("nishio") || key.Contains("茶"))
                return "茶碗の底で瞬く閉じない瞳";
            if (key.Contains("locker") || key.Contains("記憶") || key.Contains("ロッカー"))
                return "番号札の隙間に保管された前回の死因";
            if (key.Contains("battlefield") || key.Contains("nagakute") || key.Contains("古戦場") || key.Contains("槍"))
                return "無音の陣形に刻まれた戦死者の拍動";
            return "外なるものの爪痕";
        }

        string EventTraceObject(RandomEventDef e)
        {
            string key = ((e.id ?? "") + " " + (e.title ?? "") + " " + (e.area ?? "") + " " + (e.image ?? "")).ToLowerInvariant();
            if (key.Contains("locker") || key.Contains("記憶"))
                return "ロッカー番号の裏側";
            if (key.Contains("airport") || key.Contains("空港"))
                return "搭乗券の余白";
            if (key.Contains("miso") || key.Contains("発酵") || key.Contains("味噌"))
                return "樽の泡";
            if (key.Contains("tea") || key.Contains("茶"))
                return "茶碗の縁";
            if (key.Contains("puppet") || key.Contains("糸"))
                return "切れたはずの糸";
            if (key.Contains("library") || key.Contains("書庫"))
                return "禁書目録の端";
            if (key.Contains("sea") || key.Contains("海") || key.Contains("ferry"))
                return "濡れた靴底";
            if (key.Contains("castle") || key.Contains("shachi") || key.Contains("鯱"))
                return "鯱の影";
            return "選択肢の文字の縁";
        }
        string RandomEventSpecificOutcome(RandomEventDef e, bool success, string delta)
        {
            if (e == null)
                return success ? "判定は通った。" : "判定は崩れた。";
            string action = success ? e.successLabel : e.failLabel;
            string result = success ? e.successText : e.failText;
            if (string.IsNullOrWhiteSpace(action))
                action = success ? "踏み込む" : "退く";
            if (string.IsNullOrWhiteSpace(result))
                result = success ? "足元の気配が一度だけ道を譲った。" : "足元の気配は代償だけを残した。";

            string text = "踏み跡: " + action + "\n";
            if (success)
            {
                text += "残響: 怪異の継ぎ目に届いた。\n";
                text += "残った声: " + result + "\n";
                text += "拾ったもの: " + RandomEventRewardText(e, delta, result);
            }
            else
            {
                text += "残響: 怪異の条件を満たせなかった。\n";
                text += "残った声: " + result + "\n";
                text += "爪痕: " + RandomEventFailureText(e, delta);
            }
            return text;
        }
        string RandomEventRewardText(RandomEventDef e, string delta, string result)
        {
            string title = e != null ? e.title : "このイベント";
            string trace = EventTraceObject(e);
            if ((string.IsNullOrWhiteSpace(delta) || delta.Contains("変化なし")) && string.IsNullOrWhiteSpace(result))
                return title + "は数値ではなく道筋を残した。" + trace + "に刻まれた違和感が、次の選択で手掛かりになる。";
            var parts = new List<string>();
            string source = (delta ?? "") + " " + (result ?? "");
            if (source.Contains("記憶片+"))
                parts.Add("記憶片を得た");
            if (source.Contains("危険察知+"))
                parts.Add("次の危険を読む手掛かりを得た");
            if (source.Contains("空港知識") || source.Contains("空港へ"))
                parts.Add("空港へ近づく手掛かりを得た");
            if (delta.Contains("HP+"))
                parts.Add("身体が現実へ引き戻され、HPが回復した");
            if (delta.Contains("SAN+"))
                parts.Add("恐怖の輪郭を整理でき、SANが戻った");
            if (delta.Contains("LUK+"))
                parts.Add("偶然の向きがこちらへ傾き、LUKが増えた");
            if (delta.Contains("所持金+"))
                parts.Add("怪異が残した現実側の対価として所持金を得た");
            if (delta.Contains("神話") || delta.Contains("知識"))
                parts.Add("禁じられた規則を読み取り、知識を得た");
            if (delta.Contains("攻撃") || delta.Contains("防御") || delta.Contains("速さ") || delta.Contains("機械") || delta.Contains("味噌") || delta.Contains("地元"))
                parts.Add("土地や怪異への対処法を覚え、能力が伸びた");
            if (parts.Count == 0)
                parts.Add("変化は " + DeltaNarration(delta) + " として残った");
            return string.Join("。", parts) + "。";
        }
        string RandomEventFailureText(RandomEventDef e, string delta)
        {
            string title = e != null ? e.title : "このイベント";
            string motif = EventMythosMotif(e);
            if (string.IsNullOrWhiteSpace(delta) || delta.Contains("変化なし"))
                return title + "の怪異は目に見える傷を残さなかったが、" + motif + "の気配をこちらの影に結びつけた。";
            var parts = new List<string>();
            if (delta.Contains("HP-"))
                parts.Add(motif + "に触れた身体が傷つき、HPが減った");
            if (delta.Contains("SAN-"))
                parts.Add("理解してはいけない構造を見てしまい、SANが減った");
            if (delta.Contains("LUK-"))
                parts.Add("偶然の逃げ道を奪われ、LUKが減った");
            if (delta.Contains("所持金-"))
                parts.Add("逃げるための代金や落とし物として所持金を失った");
            if (delta.Contains("汚染") || delta.Contains("神話汚染"))
                parts.Add("外なるものの痕跡が残り、神話汚染が進んだ");
            if (parts.Count == 0)
                parts.Add("失敗の代償は " + DeltaNarration(delta) + " として残った");
            return title + "で対応が遅れた結果、" + string.Join("。", parts) + "。";
        }
        string RandomEventResultLead(RandomEventDef e, bool success)
        {
            string area = e != null ? e.area ?? "" : "";
            if (success)
            {
                if (area.Contains("空港") || area.Contains("海"))
                    return "水音に混じって、搭乗案内のような声が正しい方角を一度だけ告げた。";
                if (area.Contains("熱田") || area.Contains("尾張") || area.Contains("名古屋"))
                    return "古い土地の理屈が、ほんの一瞬だけこちらの手順に従った。";
                if (area.Contains("三河") || area.Contains("岡崎") || area.Contains("豊田"))
                    return "発酵する時間と機械の拍が重なり、見えない継ぎ目が露出した。";
                return "恐怖の輪郭がほどけ、通れるだけの隙間が生まれた。";
            }
            if (area.Contains("空港") || area.Contains("海"))
                return "遠い波音が近づき、足元の床が一瞬だけ甲板のように揺れた。";
            if (area.Contains("熱田") || area.Contains("尾張") || area.Contains("名古屋"))
                return "金属の鳴る音がして、見えない視線が選択の失敗を数えた。";
            if (area.Contains("三河") || area.Contains("岡崎") || area.Contains("豊田"))
                return "工程表にない赤い印が増え、失敗だけが先に発酵を始めた。";
            return "出遅れた一拍の中で、怪異はこちらの弱い場所を覚えた。";
        }
        string RandomEventRollAftertaste(bool success)
        {
            return success ? "最後の出目が止まると、周囲の暗さが一枚だけ剥がれた。" : "最後の出目が止まると、息をしていない誰かが隣で笑った。";
        }
        string RandomEventLingeringDetail(RandomEventDef e, bool success, string delta)
        {
            string area = e != null ? e.area ?? "" : "";
            string text = success
                ? "この場を離れても、成功の感触は小さな道具のように手元へ残る。"
                : "この場を離れても、失敗の形は影の端に引っかかったままだ。";
            if (!string.IsNullOrEmpty(delta) && !delta.Contains("変化なし"))
                text += " 変化は数値だけでなく、次の判断の重さにも混じっていく。";
            if (area.Contains("空港"))
                text += " 空港境界は、今の選択を搭乗記録のように保存した。";
            else if (area.Contains("名駅"))
                text += " 名駅の底では、誰かが今の出来事を路線図の余白へ書き足している。";
            return text;
        }
        string RandomEventResonance(RandomEventDef e, bool success)
        {
            if (run == null || e == null)
                return "";
            var parts = new List<string>();
            if (run.stats.localKnowledge >= 4)
                parts.Add("地元知識 " + run.stats.localKnowledge + ": 土地の由来を読めるため、地方イベント判定に補正が乗る。");
            if (run.shachiGaze >= 2)
                parts.Add("鯱注視 " + run.shachiGaze + ": 尾張・空港系の判断で道標になるが、深追いの危険も増す。");
            if (!success && run.stats.sanity <= run.stats.maxSanity / 2)
                parts.Add("SAN低下: 次の神話的な結果が重くなりやすい。");
            if (parts.Count == 0)
                return "";
            return string.Join("\n", parts);
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
                run.lastRollSummary = "LUK手応え: " + a + "/" + b + "/" + c + " + LUK" + luckBonus + " = " + total;
            return total;
        }
        int RollLuckDiceAgainst(int target)
        {
            int total = RollLuckDice();
            if (run != null && total < target && HasGearEffect("luck_reroll") && !run.flags.Contains("gear_luck_reroll_used"))
            {
                run.flags.Add("gear_luck_reroll_used");
                total = target;
                run.lastRollSummary += " / 再抽選装備で成功へ押し上げ";
                Play(rewardSfx, 0.45f);
            }
            if (run != null)
                run.lastRollSummary += " / 目標 " + target + "\n" + LuckMarginText(total, target);
            return total;
        }
        string LuckRollDetail(int a, int b, int c, int luckBonus, int total, int target)
        {
            return "LUK手応え: " + a + "/" + b + "/" + c + " + LUK" + luckBonus + " = " + total + " / 目標 " + target + "\n" + LuckMarginText(total, target);
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
            if (run == null)
                return 0;
            if (e != null && e.id.StartsWith("san_"))
                return Mathf.Clamp(run.stats.sanity / 3 + EventLocalBonus(e), 0, 10);
            if (e != null && e.id.StartsWith("mythos_"))
                return Mathf.Clamp(run.stats.mythosKnowledge + EventLocalBonus(e), 0, 12);
            return Mathf.Clamp(run.stats.luck / 2 + EventLocalBonus(e), 0, 10);
        }
        int EventLocalBonus(RandomEventDef e)
        {
            if (run == null || e == null)
                return 0;
            string key = (e.area ?? "") + " " + (e.image ?? "") + " " + (e.id ?? "");
            int bonus = 0;
            if (key.Contains("尾張") || key.Contains("名古屋") || key.Contains("熱田") || key.Contains("大須") || key.Contains("castle") || key.Contains("atsuta") || key.Contains("owari"))
                bonus += Mathf.Clamp(run.stats.localKnowledge / 3, 0, 3);
            if (key.Contains("三河") || key.Contains("岡崎") || key.Contains("豊田") || key.Contains("miso") || key.Contains("toyota") || key.Contains("mikawa"))
                bonus += Mathf.Clamp((run.stats.localKnowledge + run.stats.misoResistance) / 5, 0, 3);
            if (key.Contains("知多") || key.Contains("蒲郡") || key.Contains("常滑") || key.Contains("海") || key.Contains("gamagori") || key.Contains("chita") || key.Contains("tokoname"))
                bonus += Mathf.Clamp((run.stats.localKnowledge + run.npcAirport) / 5, 0, 3);
            if (key.Contains("鯱") || key.Contains("castle") || key.Contains("airport") || key.Contains("window") || key.Contains("shachi"))
            {
                int gaze = Mathf.Clamp(run.shachiGaze / 2, 0, 3);
                bonus += run.flags.Contains("shachi_truce") ? gaze : Mathf.Min(gaze, 1);
            }
            return bonus;
        }
        string EventCheckBonusLabel(RandomEventDef e)
        {
            if (e != null && e.id.StartsWith("san_"))
                return "SAN/土地補正 +" + EventCheckBonus(e) + " (" + EventBonusBreakdown(e) + ")";
            if (e != null && e.id.StartsWith("mythos_"))
                return "神話/土地補正 +" + EventCheckBonus(e) + " (" + EventBonusBreakdown(e) + ")";
            return "LUK/土地補正 +" + EventCheckBonus(e) + " (" + EventBonusBreakdown(e) + ")";
        }
        string EventBonusBreakdown(RandomEventDef e)
        {
            if (run == null)
                return "補正なし";
            int baseBonus = e != null && e.id.StartsWith("san_") ? Mathf.Clamp(run.stats.sanity / 3, 0, 8) :
                e != null && e.id.StartsWith("mythos_") ? Mathf.Clamp(run.stats.mythosKnowledge, 0, 10) :
                Mathf.Clamp(run.stats.luck / 2, 0, 6);
            int local = EventLocalBonus(e);
            if (local > 0)
                return "基礎+" + baseBonus + "/地元・鯱+" + local;
            return "基礎+" + baseBonus;
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
            AddEvent(list, "atsuta_oath", "熱田の封じ縄", "熱田 / 旧神封印", "atsuta", "event_atsuta_miko", "焦げた注連縄が宙に浮き、外なるものの名を一拍だけ縛っている。", "縄の結び目を読み替える", "手を合わせて退く", 14, r => { r.stats.mythosKnowledge += 1; AwardRareMemory(2); SaveProgress(); }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); r.stats.mythosCorruption += 1; }, "封印の綻びから、記憶片になりかけた光がこぼれた。", "結び目が指に食い込み、神話汚染が増す。", "");
            AddEvent(list, "library_index", "鶴舞地下書庫", "鶴舞 / 禁書目録", "tsuruma", "event_tsuruma_librarian", "司書は顔のない索引カードを差し出す。分類番号はあなたの死因だった。", "カードを逆順に読む", "本を閉じる", 15, r => { r.stats.mythosKnowledge += 2; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 4); }, "禁書の目次だけを盗み読んだ。", "索引があなたの名前を正しく発音した。", "index_hound");
            AddEvent(list, "sakae_contract", "栄の黒い契約屋", "栄 / 契約", "sakae", "event_sakae_broker", "ネオンの影に、交換条件だけが立っている。『一時間の記憶と、今すぐの力を』", "条件を値切る", "契約しない", 12, r => { r.stats.attack += 1; r.stats.money += 250; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); progress.memoryFragments = Math.Max(0, progress.memoryFragments - 1); SaveProgress(); }, "契約の余白を突いた。力と金が残る。", "署名欄に過去の筆跡が増えた。", "");
            AddEvent(list, "seto_kiln", "瀬戸の白い窯", "瀬戸 / 焼成異界", "seto", "event_seto_potter", "窯の中から、まだ焼かれていない未来の骨が鳴る。", "窯の温度を読む", "灰を払って離れる", 13, r => { r.stats.defense += 1; r.stats.machineAptitude += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 4); }, "陶片が防具のように肌へ馴染んだ。", "白い熱が肺に入り、HPを失う。", "kiln_crawler");
            AddEvent(list, "inuyama_mask", "犬山の能面市", "犬山 / 面", "inuyama", "event_inuyama_mask", "古い面が軒先からこちらを見ている。笑っている面だけ、内側が濡れている。", "笑っていない面を選ぶ", "目を伏せて通る", 14, r => { r.stats.luck += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, r => { r.stats.mythosCorruption += 1; }, "面はあなたを通行人として認めた。LUK+1。", "面の裏にある顔が、一瞬あなたになった。", "");
            AddEvent(list, "toyohashi_signal", "豊橋の終電信号", "豊橋 / 終電", "toyohashi", "event_toyohashi_conductor", "無人の車掌が、到着しないはずの終電を待っている。", "発車ベルの拍を外す", "改札の外へ戻る", 13, r => { r.stats.speed += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 2); r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "ベルの隙間を抜け、速さを得た。", "終電の風が身体を少し持っていった。", "last_train");
            AddEvent(list, "gamagori_tide", "蒲郡の逆潮", "蒲郡 / 海底星図", "gamagori", "event_gamagori_diver", "海が空へ落ち、底に星図が露出する。潜水服の人物が手招きした。", "星図を三角測量する", "砂を握って戻る", 16, r => { r.stats.mythosKnowledge += 1; AwardRareMemory(3); SaveProgress(); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); r.shachiGaze += 1; }, "海底星図から、記憶片になりかけた光を拾った。", "逆潮があなたの影だけを連れていった。", "deep_one_clerk");
            AddEvent(list, "korankei_red", "香嵐渓の赤すぎる葉", "香嵐渓 / 紅葉迷宮", "korankei", "event_korankei_pilgrim", "葉が落ちるたびに、誰かの後悔が一つ増える。赤が濃すぎる。", "赤くない葉を探す", "急いで抜ける", 12, r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); }, r => { progress.regretLog.Add("香嵐渓の赤い葉"); SaveProgress(); r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "まだ緑の葉が、正気を繋ぎ止めた。", "後悔が一枚、日報に増えた。", "");
            AddEvent(list, "handa_brew", "半田の黒酢槽", "半田 / 発酵", "handa", "event_handa_brewer", "発酵槽の表面に、未来の新聞見出しが泡で浮かぶ。", "泡の順番を読む", "蓋を閉める", 13, r => { r.stats.misoResistance += 1; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 2); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "発酵のリズムを掴んだ。味噌耐性+1。", "泡があなたの死亡記事になった。", "");
            AddEvent(list, "arimatsu_thread", "有松の縛り糸", "有松 / 絞り染め", "arimatsu", "event_arimatsu_weaver", "紺の布に白く抜かれた模様が、探索ルートの分岐そのものに見える。", "結び目を一つほどく", "布を買って退く", 15, r => { r.dangerWarnings += 1; r.stats.luck += 1; }, r => { r.stats.money = Math.Max(0, r.stats.money - 180); r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, "糸が未来の危険を一つ教えた。", "模様の一部が皮膚に移った。", "");
            AddEvent(list, "nagakute_battlefield", "長久手の無音陣", "長久手 / 古戦場", "nagakute", "event_battlefield_monk", "旗だけが風に鳴り、人の声が一切しない。陣形は巨大な魔法円だった。", "陣の欠けを踏む", "旗を避けて進む", 14, r => { r.stats.attack += 1; r.owari += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); }, "陣の力を逆流させた。攻撃+1。", "無音の槍が腹をかすめた。", "battlefield_spear");
            AddEvent(list, "kariya_gear", "刈谷の歯車神棚", "刈谷 / 機械祭具", "kariya", "event_factory_inspector", "神棚に納められた歯車が、手を合わせる速度に合わせて回る。", "正しい回転数で拝む", "電源を切る", 13, r => { r.stats.machineAptitude += 2; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 2); r.stats.machineAptitude += 1; }, "機械の祝詞を覚えた。", "火花を浴びたが、仕組みは少しわかった。", "");
            AddEvent(list, "nishio_tea", "西尾の暗い茶室", "西尾 / 抹茶夢", "nishio", "event_tea_medium", "茶碗の底に、巨大な眼が沈んでいる。見なければ飲める。", "眼を見ずに飲む", "茶室を出る", 12, r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 3); r.stats.mythosCorruption += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "苦味が正気を戻し、別の何かを残した。", "襖の向こうから茶筅の音が追ってくる。", "");
            AddEvent(list, "ichinomiya_thread", "一宮の繊維迷路", "一宮 / 織物", "ichinomiya", "event_arimatsu_weaver", "道路そのものが織機になり、歩幅を一本ずつ編み込んでいく。", "縦糸だけを踏む", "横道へ逃げる", 13, r => { r.stats.speed += 1; r.stats.luck += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 2); }, "足取りが軽くなった。速さ+1 / LUK+1。", "足首に糸が残った。", "");
            AddEvent(list, "chiryu_puppet", "知立の人形芝居", "知立 / 人形", "chiryu", "event_chiryu_puppeteer", "人形遣いの手は見えない。だが糸は、あなたの肩にも伸びている。", "自分の糸を切る", "拍手して終える", 15, r => { r.stats.attack += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); }, "糸が切れ、身体が少し自由になった。", "拍手の音があなたの手からではなかった。", "puppet_thing");
            AddEvent(list, "laguna_stage", "ラグーナの無人ショー", "蒲郡 / 水上劇場", "laguna", "event_laguna_actor", "誰もいない客席に拍手が満ち、舞台上の怪物が出番を待つ。", "台本を即興で変える", "幕が下りるまで待つ", 14, r => { AwardRareMemory(2); r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); SaveProgress(); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "物語の役を奪い、記憶片になりかけた光に触れた。", "拍手が一つ、頭の中に残った。", "stage_polyps");
            AddEvent(list, "meieki_coinlocker", "名駅コインロッカー胎内", "名駅 / ロッカー", "station", "event_locker_keeper", "ロッカーの扉が内側から叩かれる。番号は前回死んだ順番で並んでいる。", "正しい番号を飛ばす", "鍵を捨てる", 14, r => { AwardRareMemory(3); SaveProgress(); }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); }, "開けなかった扉から、記憶片になりかけた光だけが落ちた。", "扉の内側が、少しだけあなたを覚えた。", "locker_womb");
            AddEvent(list, "osu_radio", "大須の異界ラジオ", "大須 / 電波", "osu", "event_occult_researcher", "古いラジオが、まだ起きていないボス戦の実況を流している。", "周波数を半目盛ずらす", "電池を抜く", 12, r => { r.stats.mythosKnowledge += 1; r.dangerWarnings += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "未来の敵行動を少し聞いた。", "実況者があなたの呼吸を実況した。", "");
            AddEvent(list, "castle_well", "名古屋城の底なし井戸", "名古屋城 / 井戸", "castle", "event_shachi_avatar", "井戸の底から海鳴りがする。ここは城の中なのに、潮の匂いが強い。", "桶を途中で止める", "井戸を覗く", 16, r => { r.shachiGaze += 1; AwardRareMemory(2); SaveProgress(); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 4); }, "桶の中に金の鱗が一枚あった。", "底から巨大な瞳がこちらを見た。", "well_tentacle");
            AddEvent(list, "okazaki_stamp", "岡崎の家康印章", "岡崎 / 印章", "okazaki", "event_miso_voice", "封蝋に押された印が、徳ではなく『渡るな』と読める。", "印を逆さに押す", "封蝋を削る", 13, r => { r.mikawa += 1; r.stats.defense += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "印が守り札として固まった。", "封蝋の下から別の王印が出た。", "");
            AddEvent(list, "airport_baggage", "空港の帰らない荷物", "空港 / 手荷物", "airport", "event_gate_inspector", "ターンテーブルを回る荷物は、すべてあなたの持ち物だった。まだ手に入れていない物もある。", "自分のものだけ拾う", "触らずに離れる", 15, r => { r.stats.money += 350; r.stats.luck += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 2); progress.brokenGear.Add("空港で噛まれた荷物"); SaveProgress(); }, "未来の所持金と運を少し前借りした。", "荷物の口が開き、装備記録に傷が残る。", "baggage_mouth");
            AddEvent(list, "miso_oracle", "味噌蔵の三度目の声", "岡崎 / 神託", "miso", "event_miso_voice", "一度目は警告。二度目は嘘。三度目だけが、あなたの声で話す。", "三度目だけ聞く", "耳を塞ぐ", 17, r => { r.stats.mythosKnowledge += 2; r.stats.misoResistance += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 5); }, "声の意味を掴んだ。強いが危うい知識だ。", "三度目の声が、今後も内側に残る。", "miso_voice");
            AddEvent(list, "toyota_quality", "豊田の品質検査室", "豊田 / 検査", "toyota", "event_factory_inspector", "検査票にはHP、SAN、攻撃、逃走率が赤字で並ぶ。不良品欄にあなたの名前がある。", "検査項目を書き換える", "不良品棚を出る", 14, r => { r.stats.machineAptitude += 1; r.stats.attack += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); }, "検査基準を味方につけた。", "赤ペンが皮膚の下を走った。", "quality_golem");
            AddEvent(list, "tokoname_cat", "常滑の百目猫", "常滑 / 猫", "tokoname", "event_shachi_avatar", "焼き物の猫が百の目で、あなたのサイコロを見つめている。", "一つだけ目を閉じさせる", "礼だけして通る", 12, r => { r.stats.luck += 2; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, r => { r.stats.luck = Math.Max(0, r.stats.luck - 1); }, "目が一つ閉じ、出目が軽くなった。", "猫は出目を一つ持っていった。", "");
            AddEvent(list, "shinkansen_dream", "新幹線ホームの夢喰い", "名駅 / 速度夢", "station", "event_subway_child", "通過列車の窓すべてに、眠っている探索者たちが映る。", "起きている顔を探す", "ベンチの下へ伏せる", 15, r => { r.stats.speed += 1; AwardRareMemory(1); SaveProgress(); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); }, "眠りを避けて速度を得た。", "夢の一部を食われた。", "dream_eater");
            AddEvent(list, "atsuta_sword", "熱田の抜けない剣", "熱田 / 草薙影", "atsuta", "event_atsuta_miko", "剣は台座に刺さっていない。世界そのものに刺さっている。", "柄に触れて離す", "抜こうとする", 18, r => { r.stats.attack += 2; r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 5); r.stats.mythosCorruption += 1; }, "刃の影だけを借りた。攻撃+2。", "世界が少し裂け、あなたも裂けた。", "sword_shadow");
            AddEvent(list, "tsushima_lantern", "津島の流れない灯籠", "津島 / 川祭", "tsushima", "event_tea_medium", "川面に灯籠が止まっている。火は水中で燃え、名前を読むと誰かが消える。", "名前を読まずに数える", "一つ拾う", 13, r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); AwardRareMemory(1); SaveProgress(); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); }, "数だけを覚え、火を持ち帰らなかった。", "灯籠の火が記憶を焦がした。", "lantern_dead");
            AddEvent(list, "mikawa_shadow", "三河の影武者", "三河 / 影", "okazaki", "event_battlefield_monk", "あなたより一歩強い影武者が、同じ装備で立っている。", "影より遅く構える", "先に斬る", 16, r => { r.stats.defense += 1; r.stats.attack += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 4); }, "影の拍子を奪った。攻防+1。", "影のほうが少し早かった。", "shadow_retainer");
            AddEvent(list, "centrair_window", "セントレアの窓外神", "空港 / 窓", "airport", "event_gate_inspector", "搭乗待合の窓外に、滑走路より大きなものが浮いている。職員は誰も見ていない。", "見ていないふりをする", "写真を撮る", 17, r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); r.dangerWarnings += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 4); r.stats.mythosKnowledge += 1; }, "見ない技術を覚えた。", "写真にはあなたの背後しか写っていない。", "window_god");
            AddEvent(list, "sakae_luck_parlor", "栄の出目パーラー", "栄 / 運試し", "sakae", "event_sakae_broker", "古い遊技台が、硬貨ではなく未来のHPを飲み込んで光る。店員は『運があるなら増やせます』とだけ言う。", "500円を賭ける", "台から離れる", 16, r => { int stake = Math.Min(500, r.stats.money); r.stats.money -= stake; r.stats.money += 900; r.stats.luck += 1; }, r => { r.stats.money = Math.Max(0, r.stats.money - 500); r.stats.hp = Math.Max(1, r.stats.hp - 3); }, "台は七を揃えた。所持金とLUKが増えた。", "台はHPを先に徴収した。金も少し消えた。", "");
            AddEvent(list, "meieki_night_clinic", "名駅地下の夜間診療所", "名駅 / 診療所", "station", "event_locker_keeper", "診療所の窓口には料金表だけがある。HP回復、正気補修、幸運注射。すべて現金前払いだ。", "治療費を値切る", "保険なしで逃げる", 14, r => { int fee = r.stats.money >= 420 ? 420 : r.stats.money; r.stats.money -= fee; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 7); r.stats.luck += fee >= 420 ? 1 : 0; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 4); r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, "治療は雑だが効いた。HPが戻り、満額ならLUKも上がる。", "請求書が傷口から出てきた。", "");
            AddEvent(list, "osu_lucky_auction", "大須の幸運オークション", "大須 / 路地市", "osu", "event_occult_researcher", "競り台に並ぶのは装備ではなく、明日の偶然だ。値札は所持金、落札条件はLUK。", "競り勝つ", "冷やかして去る", 15, r => { r.stats.money = Math.Max(0, r.stats.money - 300); r.stats.luck += 2; AwardRareMemory(1); SaveProgress(); }, r => { r.stats.money = Math.Max(0, r.stats.money - 180); r.stats.luck = Math.Max(0, r.stats.luck - 1); }, "偶然を一束買った。LUK+2。ごく稀に記憶片の欠片が混じる。", "冷やかし代を取られ、運まで少し削られた。", "");
            AddEvent(list, "handa_blood_coupon", "半田の赤い回数券", "半田 / 交通", "handa", "event_handa_brewer", "回数券は赤く湿っている。使えば近道になるが、改札は残りHPを数えている。", "HPで改札を通る", "普通運賃を払う", 13, r => { r.stats.hp = Math.Max(1, r.stats.hp - 4); r.stats.money += 450; r.dangerWarnings += 1; }, r => { r.stats.money = Math.Max(0, r.stats.money - 260); r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, "血の改札を抜けた。HPは減ったが金と危険察知を得た。", "普通運賃なのに、領収書が悲鳴を上げた。", "");
            AddEvent(list, "tokoname_luck_kiln", "常滑の運焼き窯", "常滑 / 窯", "tokoname", "event_shachi_avatar", "窯の中でサイコロが焼かれている。高温ほどよい出目になるが、近づくほどHPが削れる。", "高温で焼く", "低温で済ませる", 17, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); r.stats.luck += 3; }, r => { r.stats.luck += 1; r.stats.money = Math.Max(0, r.stats.money - 160); }, "指先を焦がしたが、出目は軽くなった。LUK+3。", "低温の出目は安いが、窯代は取られた。", "");
            AddEvent(list, "owari_thread_shrine", "尾張の緯糸神社", "一宮 / 織物神域", "owari_shrine", "event_arimatsu_weaver", "夜の参道に張られた糸が、鳥居から鳥居へ星図のように伸びている。結び目の一つ一つが、別の周回で死んだ探索者の名前だった。", "死者の結び目をほどく", "糸を避けて参道を抜ける", 15, r => { r.stats.localKnowledge += 2; r.dangerWarnings += 1; AwardRareMemory(1); SaveProgress(); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); r.stats.speed = Math.Max(0, r.stats.speed - 1); }, "ほどけた糸が、尾張の道筋を一つ教えた。地元知識+2、危険察知+1。", "糸は足首に残り、歩幅を一つ奪った。", "");
            AddEvent(list, "okumikawa_horaiji_steps", "鳳来寺山の逆さ石段", "奥三河 / 山岳神域", "okumikawa_horaiji", "event_korankei_pilgrim", "鳳来寺山へ続く石段が、登るほど下へ沈んでいく。杉の隙間には空ではなく、星のない黒い水面が見える。", "段数を数え直す", "息を止めて駆け上がる", 16, r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); r.stats.defense += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 4); r.stats.mythosKnowledge += 1; }, "正しい段数だけが足元に残った。SAN+2、防御+1。", "山は少しだけあなたの肺を覚えた。HPが削れ、神話理解が増す。", "shadow_retainer");
            AddEvent(list, "toyokawa_inari_red_void", "豊川稲荷の赤い曲がり廊", "豊川 / 稲荷異界", "toyokawa_inari", "event_inuyama_mask", "赤い幟の廊下で、狐像の影だけがこちらを向く。賽銭箱の底から、硬貨ではなく小さな星が落ちる音がした。", "狐像と同じ向きで礼をする", "賽銭箱を覗く", 14, r => { r.stats.luck += 2; r.stats.money = Math.Max(0, r.stats.money - 120); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); r.stats.mythosCorruption += 1; }, "礼の角度が合い、運だけが通行を許された。LUK+2。", "箱の底は空ではなく、こちらを覗き返す穴だった。", "");
            AddEvent(list, "chita_black_tide", "知多の黒い潮だまり", "知多半島 / 海蝕蔵", "chita_coast", "event_handa_brewer", "海沿いの古い蔵の前で、潮だまりが夜空を映さず、別の海底を映している。酢の匂いと潮の匂いが混ざり、呼吸の順番がわからなくなる。", "潮だまりに塩を撒く", "蔵の戸を閉める", 15, r => { r.stats.misoResistance += 1; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); r.shachiGaze += 1; }, "黒い水面が曇り、発酵した塩気が身を守った。味噌耐性+1。", "戸の向こうで海鳴りが近づき、鯱の視線が増した。", "deep_one_clerk");
            AddEvent(list, "komaki_airfield_orbit", "小牧の円軌道滑走路", "小牧 / 夜間飛行場", "komaki_airfield", "event_gate_inspector", "誘導灯が滑走路ではなく円を描いている。空自の格納庫の影から、離陸していない機体のエンジン音だけが戻ってくる。", "誘導灯の欠けを読む", "走って円の外へ出る", 16, r => { r.stats.speed += 1; r.dangerWarnings += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); r.stats.hp = Math.Max(1, r.stats.hp - 2); }, "円の切れ目を見つけた。速度+1、危険察知+1。", "滑走路は一周ぶん長くなり、膝と正気を削った。", "");
            AddEvent(list, "toyota_line_nonhuman", "豊田の人でない組立線", "豊田 / 自動化工廠", "toyota", "event_factory_inspector", "無人の組立線が、車ではなく探索者の選択肢を組み立てている。不良品箱には『人間らしさ』と印字された部品が積まれていた。", "検査規格を逆用する", "非常停止を押す", 16, r => { r.stats.machineAptitude += 2; r.stats.attack += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); r.stats.machineAptitude += 1; }, "規格外の力をこちらの装備に組み込んだ。機械適性+2、攻撃+1。", "停止ボタンは効いたが、腕の中に工程表が残った。", "quality_golem");
            AddEvent(list, "seto_moon_ceramic", "瀬戸の月白い陶片", "瀬戸 / 陶祖窯跡", "seto", "event_seto_potter", "窯跡に散った陶片が、月の満ち欠けと違う形で光る。裏返すと、釉薬の下に小さな海の化石が閉じ込められていた。", "欠けた月だけ拾う", "全部砕いて進む", 15, r => { r.stats.defense += 1; r.stats.mythosKnowledge += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, "陶片は薄い盾になり、古い海の知識も残した。", "砕いた音が窯の底へ落ち、破片が肌に刺さった。", "kiln_crawler");
            AddEvent(list, "toyohashi_black_tram", "豊橋の黒い路面電車", "豊橋 / 市電終点", "toyohashi", "event_toyohashi_conductor", "終点に着いたはずの路面電車が、線路のない暗がりへまだ進もうとしている。運転席には運転士ではなく、濡れた時刻表が座っていた。", "時刻表を一分遅らせる", "飛び降りる", 14, r => { r.stats.speed += 1; AwardRareMemory(1); SaveProgress(); }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 2); r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "一分の遅れが、こちらの速度になった。", "線路のない揺れが身体に残った。", "last_train");
            AddEvent(list, "tsushima_river_king", "津島天王川の沈む山車", "津島 / 川祭深部", "tsushima", "event_tea_medium", "川面に浮くはずの山車が、水中をゆっくり進んでいる。提灯の火は消えず、火袋の中で小さな深海が揺れていた。", "火袋の数だけ息を止める", "山車を岸へ引く", 15, r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); AwardRareMemory(1); SaveProgress(); }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); r.stats.mythosCorruption += 1; }, "息を止めた数だけ、川はあなたを見逃した。", "綱の向こうに川底の祭りが絡みついた。", "lantern_dead");
            AddEvent(list, "nishio_green_eye", "西尾抹茶の緑眼", "西尾 / 茶畑夢層", "nishio", "event_tea_medium", "茶畑の畝が巨大な指紋のように湾曲し、茶碗の底には泡ではなく緑の眼が沈む。見ないで飲めば効く。見れば、向こうも覚える。", "眼を見ずに点てる", "泡の形を読む", 14, r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 3); }, r => { r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 3); }, "苦味だけが正気へ戻った。SAN+3。", "泡は星図だった。読めたが、向こうにも読まれた。", "tea_eye");
            AddEvent(list, "san_atsuta_quiet_prayer", "熱田の正気祓い", "熱田 / SAN専用", "atsuta", "event_atsuta_miko", "熱田の社の奥で、鈴の音だけがあなたの正気度を数えている。ここでは知識よりも、まだ人間として残っている沈黙が試される。", "SANを整えて祓いを受ける", "無理に鈴を鳴らす", 12, r => { int gain = r.stats.sanity <= r.stats.maxSanity / 2 ? 5 : 2; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + gain); r.stats.mythosCorruption = Math.Max(0, r.stats.mythosCorruption - 1); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 4); r.stats.mythosCorruption += 1; }, "鈴は一度だけ正しく鳴った。SANが戻り、神話汚染が少し薄れる。", "鈴の内側から別の音が返り、正気が削れた。", "");
            AddEvent(list, "san_sakae_neon_overdose", "栄ネオンの正気過量", "栄 / SAN専用", "sakae", "event_sakae_broker", "栄の路地で、ネオンが心拍と同じ速度で点滅している。浴びれば恐怖は薄れるが、薄れすぎた恐怖は危険を危険と呼ばなくなる。", "SANを支払って恐怖を麻痺させる", "目を閉じて通る", 13, r => { r.stats.sanity = Math.Max(1, r.stats.sanity - 3); r.stats.attack += 2; r.dangerWarnings += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "恐怖が一枚剥がれた。SAN-3、攻撃+2、危険察知+1。", "目を閉じても、まぶたの裏に看板が残った。", "");
            AddEvent(list, "mythos_tsuruma_forbidden_catalog", "鶴舞の神話目録", "鶴舞 / 神話専用", "tsuruma", "event_tsuruma_librarian", "地下書庫の禁書目録は、読めない本ではなく読んではいけない本を先に開く。神話理解が高いほど、ページはあなたを読者ではなく共著者として扱う。", "神話理解で索引を逆引きする", "普通の分類で探す", 16, r => { r.stats.mythosKnowledge += 2; r.dangerWarnings += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 3); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "索引が一つ噛み合った。神話理解+2、危険察知+1、SAN-3。", "普通の分類番号のほうが、かえってあなたの死因に近かった。", "index_hound");
            AddEvent(list, "mythos_castle_sealed_name", "名古屋城の封名井戸", "名古屋城 / 神話専用", "castle", "event_shachi_avatar", "井戸の底に、呼んではいけない名が沈んでいる。神話理解が足りれば封じ直せる。足りなければ、名のほうがこちらを覚える。", "神話理解で名を封じ直す", "耳を塞いで離れる", 17, r => { r.stats.mythosKnowledge += 1; r.stats.defense += 1; AwardRareMemory(2); SaveProgress(); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 4); r.stats.mythosCorruption += 1; }, "名は水底へ戻った。神話理解+1、防御+1。ごく稀に記憶片の欠片が残る。", "井戸はあなたの名を一拍だけ先に呼んだ。", "well_tentacle");
            AddEvent(list, "owari_atsuta_breathless_gate", "熱田の息をしない鳥居", "尾張 / 熱田深部", "atsuta", "event_atsuta_miko", "鳥居の下だけ風が止まり、息を吸うと古い祝詞が肺に入ってくる。吐く言葉を間違えると、名前が一文字減る。", "祝詞を途中で切る", "息を止めて抜ける", 15, r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); r.stats.localKnowledge += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); r.stats.mythosCorruption += 1; }, "祝詞は途中で途切れ、正気だけが戻った。", "息を止めたはずなのに、別の声が喉を使った。", "");
            AddEvent(list, "owari_osu_back_alley_server", "大須の裏路地サーバー", "尾張 / 大須電脳", "osu", "event_occult_researcher", "閉店した電器店の奥で、古いサーバーが城の井戸と同じ水音を立てている。画面には『参照先: あなた』とだけ表示されている。", "ログを一行だけ消す", "電源を抜く", 14, r => { r.dangerWarnings += 1; r.stats.mythosKnowledge += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 2); r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "ログは消え、次の危険だけが残った。", "電源コードは血管のように脈打っていた。", "index_hound");
            AddEvent(list, "mikawa_koji_white_room", "岡崎の白すぎる麹室", "三河 / 岡崎発酵", "miso", "event_miso_voice", "麹室の白は雪ではない。壁一面の菌糸が、あなたの判断をゆっくり発酵させている。", "温度を一度だけ下げる", "扉を閉めて待つ", 15, r => { r.stats.misoResistance += 2; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); r.stats.mythosCorruption += 1; }, "発酵は落ち着き、耐性が身体に残った。", "待つ時間が長すぎて、考え方が少し変わった。", "miso_voice");
            AddEvent(list, "mikawa_factory_afterimage", "豊田の残像検査員", "三河 / 豊田検査線", "toyota", "event_factory_inspector", "検査員は一人しかいないのに、残像だけがラインの端まで並んでいる。全員が同じミスを指摘してくる。", "最初の指摘だけ直す", "全部に謝る", 16, r => { r.stats.machineAptitude += 2; r.dangerWarnings += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); r.stats.machineAptitude += 1; }, "最初の一つだけが本物だった。機械適性が伸びる。", "謝罪は承認されず、身体のほうが修正された。", "quality_golem");
            AddEvent(list, "chita_airport_shell_phone", "常滑の貝殻電話", "知多 / 空港前夜", "tokoname", "event_gate_inspector", "坂の途中に落ちた貝殻から、空港アナウンスが聞こえる。搭乗口番号は波音に隠れている。", "波の間を読む", "耳から離す", 13, r => { r.npcAirport += 2; r.stats.localKnowledge += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "搭乗口の断片が聞こえた。空港知識が増える。", "貝殻の奥で、誰かがあなたの名前を呼んだ。", "");
            AddEvent(list, "chita_gamagori_orbital_fish", "蒲郡の軌道魚", "知多 / 蒲郡星海", "gamagori", "event_gamagori_diver", "海面から浮いた魚が、星の軌道で群れている。網で捕るより、進路を読んだほうが早い。", "軌道を読んで避ける", "一匹捕まえる", 16, r => { r.stats.speed += 1; r.stats.mythosKnowledge += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, "軌道の切れ目が見え、足が速くなる。", "魚の鱗は小さな夜空で、触れた指が冷えた。", "deep_one_clerk");
            AddEvent(list, "meieki_memory_ticket", "記憶でできた切符", "名駅 / 周回知識", "station", "event_memory_vendor", "券売機の画面に、あなたが前回選ばなかった行き先だけが表示されている。料金は現金ではなく、覚えている失敗の数だった。", "失敗を一枚だけ差し出す", "買わずに路線図を見る", 14, r => { AwardRareMemory(2); r.dangerWarnings += 1; SaveProgress(); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "切符には次の危険が薄く印字されていた。危険察知+1。ごく稀に記憶片の欠片が残る。", "路線図は読めたが、読んだ場所が少しだけ消えた。", "");
            AddEvent(list, "meieki_cartographer_blank_map", "路線図職人の白地図", "名駅 / 攻略導線", "station", "event_route_cartographer", "路線図職人は、まだ行っていない場所だけが白く抜けた地図を広げた。白い部分は、見るほどこちらへ近づいてくる。", "白地図に目的を書く", "地図を折って戻す", 15, r => { r.stats.localKnowledge += 2; r.npcAirport += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); r.dangerWarnings += 1; }, "目的を書いた瞬間、空港へ向かう線が一本濃くなった。", "折り目から、まだ見ていない敵の気配が漏れた。", "");
            AddEvent(list, "owari_shachi_hunter_shadow", "鯱狩りの影踏み", "尾張 / 鯱注視高", "castle", "shachi_hunter", "瓦屋根の影から、鯱狩りの仮面武者がこちらの影だけを踏んでくる。鯱に見られた者を、狩人も見つける。", "地元道へ影を逃がす", "振り返って名を問う", 17, r => { r.stats.localKnowledge += 1; r.shachiGaze = Math.Max(0, r.shachiGaze - 1); r.dangerWarnings += 1; }, r => { r.shachiGaze += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 3); }, "影は路地へ逃げ、鯱の注視が少し薄れた。", "仮面の奥から、あなたの影の名前が返ってきた。", "shachi_hunter");
            AddEvent(list, "owari_castle_rooftop_route", "名古屋城屋根裏の帰路", "尾張 / 地元近道", "castle", "event_shachi_avatar", "観光順路の外に、屋根裏へ続く古い階段がある。地元の古い呼び名を知らなければ、階段は壁になる。", "古い地名で階段を呼ぶ", "鯱の尾を目印にする", 15, r => { r.stats.localKnowledge += 2; r.npcAirport += 1; }, r => { r.shachiGaze += 1; r.stats.hp = Math.Max(1, r.stats.hp - 2); }, "階段は一度だけ道になり、空港方面の線が見えた。", "尾を目印にしたせいで、鯱の視線も濃くなった。", "");
            AddEvent(list, "mikawa_miso_contract", "味噌蔵の雇用契約", "三河 / 契約", "miso", "event_miso_voice", "味噌蔵の壁に貼られた雇用契約書が、あなたを一日だけ蔵人として雇おうとしている。勤務内容は『異界の発酵管理』。", "条件を読んで署名する", "朱印を押さずに出る", 15, r => { r.stats.misoResistance += 2; r.stats.money += 180; r.mikawa += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); r.stats.mythosCorruption += 1; }, "一日分の給金と、発酵に耐える身体が残った。", "契約書の空欄に、あなたの声だけが署名した。", "miso_voice");
            AddEvent(list, "mikawa_factory_escape_sim", "豊田の脱出シミュレータ", "三河 / 試験走路", "toyota", "event_factory_inspector", "試験走路のモニターが、あなたの次の三回の逃走失敗を再生している。失敗を先に見るほど、本番では速くなる。", "失敗映像を最後まで見る", "途中で停止する", 16, r => { r.dangerWarnings += 2; r.stats.speed += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); }, "失敗の記録が、次の回避ルートになった。", "停止ボタンは効いたが、映像の中のあなたはまだ走っていた。", "quality_golem");
            AddEvent(list, "chita_ferry_no_return", "知多の帰らない渡船", "知多 / 海路", "chita_coast", "event_gamagori_diver", "桟橋に、時刻表へ載っていない渡船が停まっている。船頭は空港へ行けると言うが、戻り便については笑うだけだった。", "片道で乗る", "船頭の顔を確かめる", 16, r => { r.npcAirport += 3; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); r.shachiGaze += 1; }, "渡船は確かに空港の灯りへ近づいた。戻り道は薄くなった。", "船頭の顔は、海面に映ったあなたの顔だった。", "deep_one_clerk");
            AddEvent(list, "chita_baggage_from_sea", "海から来た手荷物", "知多 / 空港前", "gamagori", "event_gate_inspector", "波打ち際に、空港タグのついたスーツケースが流れ着いている。まだ出発していないはずのあなたの名前が書かれている。", "タグだけ剥がす", "中身を開ける", 15, r => { r.npcAirport += 1; r.stats.luck += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 2); progress.brokenGear.Add("海から来た手荷物"); SaveProgress(); }, "タグの裏に、保安検査場の抜け道が書かれていた。", "中身は濡れた衣類ではなく、次の失敗の音だった。", "baggage_mouth");
            AddEvent(list, "mythos_meieki_under_name", "名駅地下の下の名前", "名駅 / 神話深度", "station", "event_subway_child", "名駅のさらに下には駅名がない。そこでは、地名ではなく人名がホーム名として使われている。", "自分の名前を読まない", "ホーム名を確かめる", 18, r => { r.stats.mythosKnowledge += 2; r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 5); r.stats.mythosCorruption += 1; }, "読まなかった名前の数だけ、神話理解が増えた。", "ホーム名はあなたの名前だった。次の列車はもう来ている。", "dream_eater");
            AddEvent(list, "san_local_breakfast_anchor", "地元喫茶の正気錨", "名古屋 / SAN回復", "kishimen", "event_cafe_server", "古い喫茶店のモーニングは、異界に対する錨のように湯気を立てている。地元の話ができれば、少しだけ現実へ戻れる。", "常連のふりで座る", "急いで食べる", 13, r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 4); r.stats.localKnowledge += 1; }, r => { r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 2); r.stats.money = Math.Max(0, r.stats.money - 180); }, "店員は何も聞かず、水だけを先に置いた。SANが戻る。", "味は現実だったが、会計だけが異界だった。", "");
            AddEvent(list, "airport_last_call_wrong_name", "違う名前の最終搭乗案内", "空港 / 終盤導線", "airport", "event_gate_inspector", "スピーカーが最終搭乗案内を流す。呼ばれた名前はあなたではない。だが、なぜか立ち上がりそうになる。", "呼ばれていないと答える", "別名で搭乗する", 17, r => { r.npcAirport += 2; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); }, r => { r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 3); }, "あなたはまだあなたの名前で立っている。搭乗口が一つ近づいた。", "別名はよく馴染んだ。馴染みすぎて、元の名が少し薄れた。", "window_god");
            AddThirtyRandomEventExpansion(list);
            return list;
        }
        void AddThirtyRandomEventExpansion(List<RandomEventDef> list)
        {
            AddEvent(list, "ai_meieki_memory_lockers_01", "記憶ロッカーの空番号", "名駅 / 記憶倉庫", "meieki_memory_lockers", "event_memory_locker_keeper", "空のロッカーだけが呼吸している。番号札には、まだ失っていない記憶の名前が薄く浮かんでいた。", "空番号を逆から読む", "鍵だけ持ち去る", 13, r => { AwardRareMemory(2); r.dangerWarnings += 1; SaveProgress(); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "空番号は次の危険の位置を示していた。危険察知+1。ごく稀に記憶片の欠片が残る。", "鍵は掌で冷え、何かを閉じ込める音だけが残った。", "locker_womb");
            AddEvent(list, "ai_meieki_memory_lockers_02", "終電前の預けもの", "名駅 / 記憶倉庫", "meieki_memory_lockers", "event_memory_locker_keeper", "古い駅員は、あなたが次に死ぬ時刻の荷札を差し出す。受け取れば道は短くなるが、時刻も近づく。", "荷札を半分だけ破る", "受け取って走る", 15, r => { r.stats.speed += 1; r.stats.localKnowledge += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); r.instability += 1; }, "破れた荷札から、まだ使える近道が落ちた。", "荷札の時刻が、今に一分だけ近づいた。", "");
            AddEvent(list, "ai_meieki_memory_lockers_03", "改札裏の忘れ物台帳", "名駅 / 周回記録", "meieki_memory_lockers", "event_route_cartographer", "台帳には、前回のあなたが拾えなかったものだけが記されている。文字は読むほど新しくなる。", "拾わなかった理由を読む", "台帳を閉じる", 14, r => { AwardRareMemory(3); r.stats.sanity = Math.Max(0, r.stats.sanity - 1); SaveProgress(); }, r => { r.dangerWarnings += 1; }, "理由は痛かったが、次の周回へ持ち越せる形になった。", "閉じた台帳の背に、知らない手形が増えた。", "");
            AddEvent(list, "ai_atsuta_black_torii_01", "黒鳥居の息継ぎ", "尾張 / 熱田", "atsuta_black_torii", "event_black_torii_miko", "黒い鳥居の下だけ、呼吸の音が一拍遅れて返る。巫女は『吸ってはいけない祝詞がある』と囁いた。", "息を吐いたまま通る", "祝詞を吸い込む", 15, r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); r.owari += 1; }, r => { r.stats.mythosCorruption += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 3); }, "鳥居はあなたを通行人として扱った。", "祝詞は肺でほどけ、しばらく自分の声ではなくなった。", "");
            AddEvent(list, "ai_atsuta_black_torii_02", "草薙影の鞘鳴り", "尾張 / 草薙影", "atsuta_black_torii", "sword_shadow", "地面に落ちた剣の影が、鞘へ戻れず震えている。触れれば力を借りられる。抜けば影もこちらを見る。", "影だけを借りる", "影を抜く", 17, r => { r.stats.attack += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 4); r.shachiGaze += 1; }, "刃の輪郭だけが腕に残った。攻撃+1。", "抜いたのは剣ではなく、こちらの影だった。", "sword_shadow");
            AddEvent(list, "ai_atsuta_black_torii_03", "注連縄の裏結び", "尾張 / 封印", "atsuta_black_torii", "event_black_torii_miko", "焦げた注連縄の裏側に、誰かが封印をほどくための結び方を書き残している。", "裏結びを逆にする", "そのまま覚える", 16, r => { r.stats.defense += 1; r.stats.mythosCorruption = Math.Max(0, r.stats.mythosCorruption - 1); }, r => { r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "結びは少し固くなり、身を守る感覚が残った。", "覚えた瞬間、ほどき方も覚えてしまった。", "");
            AddEvent(list, "ai_owari_rooftop_01", "金鯱の雨樋", "尾張 / 城屋根", "owari_rooftop_shachi", "event_shachi_avatar", "屋根の雨樋を、雨ではなく金色の小さな鱗が流れている。拾うほど鯱の視線が濃くなる。", "一枚だけ拾う", "流れをせき止める", 14, r => { r.stats.luck += 1; r.shachiGaze += 1; }, r => { r.shachiGaze += 2; r.stats.hp = Math.Max(1, r.stats.hp - 2); }, "一枚の鱗は幸運の重さだった。", "流れは止まらず、屋根のほうがこちらへ傾いた。", "");
            AddEvent(list, "ai_owari_rooftop_02", "屋根裏の観光順路", "尾張 / 城屋根", "owari_rooftop_shachi", "shachi_hunter", "観光順路の矢印が屋根裏へ曲がっている。奥では鯱狩りの足音が、こちらの影だけを追っていた。", "地元名で矢印を呼び戻す", "屋根裏へ踏み込む", 16, r => { r.stats.localKnowledge += 2; r.dangerWarnings += 1; }, r => { r.shachiGaze += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "矢印は本来の道へ戻り、近道だけを残した。", "足音はすぐ後ろに増えた。", "shachi_hunter");
            AddEvent(list, "ai_owari_rooftop_03", "鯱狩りの見逃し札", "尾張 / 鯱注視", "owari_rooftop_shachi", "shachi_hunter", "瓦の隙間に『見逃し』と書かれた札が挟まっている。札は鯱に見られた者ほど重い。", "札を半分燃やす", "胸元に隠す", 18, r => { r.shachiGaze = Math.Max(0, r.shachiGaze - 2); r.dangerWarnings += 1; }, r => { r.shachiGaze += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 3); }, "札は燃え、視線が少しだけ剥がれた。", "札の裏には、すでにあなたの名があった。", "shachi_hunter");
            AddEvent(list, "ai_osu_server_01", "大須サーバーの水音", "尾張 / 大須電脳", "osu_server_alley", "event_osu_signal_hacker", "閉店後のサーバーラックから城の井戸と同じ水音がする。ログには『参照先: あなた』とだけある。", "ログを一行だけ消す", "電源を抜く", 14, r => { r.dangerWarnings += 2; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 2); r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "次の危険だけがログに残った。危険察知+2。", "電源コードは血管のように脈打っていた。", "index_hound");
            AddEvent(list, "ai_osu_server_02", "中古端末の予知通知", "尾張 / 大須電脳", "osu_server_alley", "event_osu_signal_hacker", "中古端末に、三分後のあなたから通知が届く。通知は助言ではなく、謝罪だった。", "謝罪の前文を読む", "通知を削除する", 13, r => { r.dangerWarnings += 1; r.stats.speed += 1; }, r => { r.instability += 1; }, "前文だけで十分だった。足が少し早くなる。", "削除済み通知が、画面の裏から震え続けた。", "");
            AddEvent(list, "ai_osu_server_03", "ネオン信号の赤点滅", "尾張 / 栄大須境界", "osu_server_alley", "event_osu_signal_hacker", "ネオンの赤点滅が心拍と同期する。見続けるほど強くなるが、逃げ道の色も失われる。", "拍をずらして見る", "赤だけを追う", 15, r => { r.stats.attack += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); r.dangerWarnings += 1; }, "赤はただの光へ戻り、力だけが残った。", "信号はあなたの脈を覚えた。", "");
            AddEvent(list, "ai_mikawa_koji_01", "白すぎる麹花嫁", "三河 / 岡崎発酵", "mikawa_koji_room", "event_koji_bride", "白い麹のヴェールを被った花嫁が、あなたの名前を発酵前の音で呼ぶ。返事をすれば耐性を得る。", "古い屋号で返す", "自分の名で返す", 15, r => { r.stats.misoResistance += 2; r.mikawa += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); r.stats.mythosCorruption += 1; }, "花嫁は頷き、発酵の熱だけを分けた。", "自分の名が、白い壁で増殖した。", "miso_voice");
            AddEvent(list, "ai_mikawa_koji_02", "味噌桶の底なし階段", "三河 / 岡崎発酵", "mikawa_koji_room", "event_koji_bride", "巨大な桶の底に、降りるほど味が濃くなる階段がある。底まで行けば力になるが、戻る味を忘れる。", "三段だけ降りる", "底まで降りる", 16, r => { r.stats.defense += 1; r.stats.misoResistance += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); r.stats.misoResistance += 1; }, "三段目で引き返せた。濃さは防御になった。", "底はなかった。味だけが身体に残った。", "miso_voice");
            AddEvent(list, "ai_mikawa_koji_03", "発酵する勤務表", "三河 / 契約", "mikawa_koji_room", "event_koji_bride", "勤務表の空欄に、あなたの明日の時間が勝手に書き込まれていく。働けば金は残る。時間は残らない。", "勤務時間を値切る", "一日分働く", 13, r => { r.stats.money += 220; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, r => { r.stats.money += 300; r.instability += 1; }, "値切りは通った。時間の損失は少ない。", "給金は出たが、夕方の記憶が丸ごとない。", "");
            AddEvent(list, "ai_toyota_track_01", "試験走路の先読み", "三河 / 豊田試験路", "toyota_test_track", "event_factory_inspector", "試験走路のモニターに、三通りの事故が同時に映る。事故を選べば、その一つだけは避けられる。", "一番軽い事故を選ぶ", "全事故を記憶する", 15, r => { r.dangerWarnings += 2; r.stats.speed += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); r.dangerWarnings += 1; }, "選んだ事故は起きなかった。足が速くなる。", "記憶は増えたが、ブレーキ音も増えた。", "quality_golem");
            AddEvent(list, "ai_toyota_track_02", "赤ペン検査票の群れ", "三河 / 豊田検査", "toyota_test_track", "event_factory_inspector", "赤ペンでできた検査票が空中を泳ぐ。不良項目にはHP、SAN、帰還意思まで並んでいる。", "検査基準を書き換える", "不良印を受ける", 14, r => { r.stats.machineAptitude += 2; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); r.stats.machineAptitude += 1; }, "基準はあなた向けに少し甘くなった。", "不良印は痛いが、工程だけは理解できた。", "quality_golem");
            AddEvent(list, "ai_toyota_track_03", "無人搬送車の葬列", "三河 / 工場夜道", "toyota_test_track", "event_factory_inspector", "無人搬送車が、誰も乗せていない棺を運んでいる。棺のラベルはまだ印刷途中だ。", "ラベルを剥がす", "列の後ろを歩く", 16, r => { r.stats.luck += 1; r.dangerWarnings += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); }, "ラベルは白紙になり、今日だけは運が残った。", "列はあなたの歩幅を覚えた。", "");
            AddEvent(list, "ai_chita_ferry_01", "帰らない渡船の切符", "知多 / 霧の桟橋", "chita_fog_ferry", "event_fog_ferry_pilot", "霧の渡船は空港の灯りへ行けると言う。船頭は、戻り便の話だけを笑って聞き流した。", "片道切符を買う", "船頭の顔を確かめる", 16, r => { r.npcAirport += 3; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 3); r.shachiGaze += 1; }, "渡船は確かに空港の灯りへ近づいた。", "船頭の顔は海面に映ったあなたの顔だった。", "deep_one_clerk");
            AddEvent(list, "ai_chita_ferry_02", "貝殻電話の搭乗口", "知多 / 霧の桟橋", "chita_fog_ferry", "event_fog_ferry_pilot", "桟橋に落ちた貝殻から、まだ開いていない搭乗口の案内が聞こえる。波音が番号を隠している。", "波の間を読む", "耳から離す", 13, r => { r.npcAirport += 2; r.stats.localKnowledge += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "番号の一部が聞こえた。空港知識が増える。", "貝殻の奥で、誰かがあなたの名を呼んだ。", "");
            AddEvent(list, "ai_chita_ferry_03", "海霧の手荷物検査", "知多 / 霧の桟橋", "chita_fog_ferry", "event_under_runway_clerk", "海霧の中に保安検査台が浮いている。係員は荷物ではなく、あなたが持ち帰る記憶を検査する。", "危ない記憶を預ける", "全部持ち帰る", 15, r => { AwardRareMemory(2); r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); SaveProgress(); }, r => { r.dangerWarnings += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "預けた記憶は、ごく稀に記憶片の欠片として残る。", "全部は重すぎた。次の危険が近づく。", "");
            AddEvent(list, "ai_airport_under_runway_01", "滑走路下の逆さ雨", "空港 / 滑走路下", "airport_under_runway", "event_under_runway_clerk", "滑走路の下で、雨が天井へ落ちている。係員は『濡れた名前は搭乗できません』と告げた。", "名前を乾かす", "別名で通る", 15, r => { r.npcAirport += 2; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); }, r => { r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 3); }, "名前はまだ読める形で乾いた。", "別名は思ったより体に馴染んだ。", "window_god");
            AddEvent(list, "ai_airport_under_runway_02", "手荷物ベルトの海鳴り", "空港 / 手荷物裏", "airport_under_runway", "event_under_runway_clerk", "手荷物ベルトの奥から海鳴りがする。流れてくるスーツケースは、出発前のあなたのものばかりだった。", "タグだけ剥がす", "中身を開ける", 14, r => { r.npcAirport += 1; r.stats.luck += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 2); progress.brokenGear.Add("海鳴りの手荷物"); SaveProgress(); }, "タグの裏に抜け道が印字されていた。", "中身は濡れた失敗の音だった。", "baggage_mouth");
            AddEvent(list, "ai_airport_under_runway_03", "整備員の無言誘導", "空港 / 整備通路", "airport_under_runway", "event_under_runway_clerk", "整備員は一言も話さず、光る誘導棒だけで進路を示す。従えば早い。疑えば安全だ。", "誘導に従う", "誘導棒の影を見る", 16, r => { r.stats.speed += 1; r.npcAirport += 1; }, r => { r.dangerWarnings += 2; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); }, "通路は短くなり、空港の構造が少し見えた。", "影は別の方向を指していた。危険察知だけが残る。", "");
            AddEvent(list, "ai_toyokawa_fox_01", "狐灯籠の回り道", "三河 / 豊川稲荷", "toyokawa_fox_lanterns", "event_osu_signal_hacker", "狐の灯籠が一つずつ違う方角を照らしている。正しい狐は、こちらを見ない。", "見ない狐について行く", "一番明るい狐を選ぶ", 14, r => { r.stats.luck += 1; r.stats.localKnowledge += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); r.stats.luck = Math.Max(0, r.stats.luck - 1); }, "見ない狐は、見られない道を知っていた。", "明るすぎる狐は、こちらの影を食べた。", "");
            AddEvent(list, "ai_toyokawa_fox_02", "赤い札の借金", "三河 / 豊川稲荷", "toyokawa_fox_lanterns", "event_black_torii_miko", "赤い札には、まだ借りていない幸運の返済日が書かれている。借りれば今は助かる。返済日は近い。", "少額だけ借りる", "札を破る", 15, r => { r.stats.luck += 2; r.instability += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); }, "幸運は来た。返済日も来る。", "札は破れたが、赤い粉が指紋に残った。", "");
            AddEvent(list, "ai_toyokawa_fox_03", "狐面の空港行き", "三河 / 豊川稲荷", "toyokawa_fox_lanterns", "event_fog_ferry_pilot", "狐面をつけた案内人が、空港へ抜ける夜道を知っていると言う。尾は一本だが、影は三本ある。", "影の少ない道を選ぶ", "案内人に任せる", 17, r => { r.npcAirport += 2; r.dangerWarnings += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); r.stats.mythosKnowledge += 1; }, "影の少ない道は遠回りだが、空港へ近づいた。", "案内人の影が一本、あなたの足元へ移った。", "last_train");
            AddEvent(list, "ai_airport_under_runway_04", "搭乗橋の下の潮溜まり", "空港 / 搭乗橋下", "airport_under_runway", "event_under_runway_clerk", "搭乗橋の下に潮溜まりがあり、そこだけ空港の音が海底から聞こえる。覗けば近道、触れれば別の滑走路だ。", "水面だけを見る", "指で滑走路をなぞる", 16, r => { r.npcAirport += 2; r.dangerWarnings += 1; }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 3); r.stats.mythosKnowledge += 1; }, "水面に、今使える搭乗口だけが映った。", "指先が一瞬だけ海底の空へ届いた。", "deep_one_clerk");
            AddEvent(list, "ai_chita_ferry_04", "霧船頭の忘れ櫂", "知多 / 霧の桟橋", "chita_fog_ferry", "event_fog_ferry_pilot", "桟橋に古い櫂が一本だけ残されている。握ると、まだ乗っていない船の揺れが足元へ伝わる。", "櫂を岸へ戻す", "櫂で霧をかく", 14, r => { r.stats.localKnowledge += 1; r.npcAirport += 1; }, r => { r.stats.hp = Math.Max(1, r.stats.hp - 2); r.shachiGaze += 1; }, "櫂は岸を覚えていた。空港への海路も少し見えた。", "霧は水より重く、腕に潮の痛みが残った。", "");
            AddEvent(list, "ai_meieki_memory_lockers_04", "ロッカー内の非常口", "名駅 / 記憶倉庫", "meieki_memory_lockers", "event_memory_locker_keeper", "開けたロッカーの奥に、小さすぎる非常口がある。通れないはずなのに、向こうのあなたは手招きしている。", "非常口の表示だけ剥がす", "身体を折って入る", 17, r => { r.stats.localKnowledge += 1; AwardRareMemory(2); SaveProgress(); }, r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 4); r.instability += 1; }, "表示は地図の余白に貼り付いた。次の周回でも使えそうだ。", "身体は通った。心の形だけが少し戻りにくい。", "dream_eater");
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
            bool bossBattle = IsBossEnemy(enemyId);
            if (run != null && run.stats.sanity <= 0 && enemyId != "impossible_one" && !bossBattle)
            {
                QueueSanityCollapse(!string.IsNullOrEmpty(run.battleReturnScene) ? run.battleReturnScene : run.sceneId);
                return;
            }
            if (run != null && run.stats.sanity <= 0 && bossBattle)
                EnsureSanityCollapseQueued(!string.IsNullOrEmpty(run.battleReturnScene) ? run.battleReturnScene : run.sceneId);
            enemyId = ResolveMythicBossOverride(enemyId);
            if (IsBossEnemy(enemyId) && run != null && !run.flags.Contains("final_boss_rush") && !run.flags.Contains("prep_" + enemyId))
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
            ApplyEnemyThreatBalance(activeEnemy);
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
            bodyText.text = BossBattlePrelude(activeEnemy.id) + activeEnemy.intro + "\n\n" + MonsterHint(activeEnemy);
            enemyHpSlider.maxValue = activeEnemy.maxHp;
          enemyHpSlider.value = activeEnemy.hp;
            attackGauge = 0f;
            guardGauge = 0f;
            ApplyBattleOpeningTraits(activeEnemy);
            ScheduleEnemyAttack();
            BeginAttackPhase();
            UpdateSideText();
            AddRetreatOption();
        }
        string ResolveMythicBossOverride(string enemyId)
        {
            if (run == null || enemyId == "impossible_one")
                return enemyId;
            if (run.stats.sanity <= 0 || run.sanityCollapseTurns >= 0)
                return enemyId;
            bool boss = IsBossEnemy(enemyId);
            if (!boss)
                return enemyId;
            int mythicPressure = run.stats.mythosKnowledge + run.stats.mythosCorruption * 2;
            if (mythicPressure >= 12 && enemies.ContainsKey("impossible_one"))
            {
                LogRun("神話圧が限界を超えた。ボスの輪郭が、この世のものではない何かへ置き換わった。");
                return enemyId;
            }
            return enemyId;
        }
        bool TryShowImpossibleNonBossEncounter(string targetSceneId)
        {
            if (run == null || !enemies.ContainsKey("impossible_one"))
                return false;
            if (run.stats.sanity <= 0 || run.sanityCollapseTurns >= 0)
                return false;
            if (run.flags.Contains("impossible_nonboss_spawned"))
                return false;
            if (run.steps < 2 || targetSceneId == "nagoya_start" || targetSceneId == "airport_gate")
                return false;
            if (!string.IsNullOrEmpty(targetSceneId) && targetSceneId.IndexOf("boss", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;
            int mythicPressure = run.stats.mythosKnowledge + run.stats.mythosCorruption * 2;
            if (mythicPressure < 12)
                return false;
            run.flags.Add("impossible_nonboss_spawned");
            run.battleReturnScene = targetSceneId;
            LogRun("Impossible non-boss encounter queued.");
            StartBattle("impossible_one");
            return true;
        }
        bool IsBossEnemy(string enemyId)
        {
            return enemyId == "miso_voice" || enemyId == "gate_guard" || enemyId == "boundary_airport_director" ||
                   enemyId == "well_tentacle" || enemyId == "shadow_retainer" ||
                   enemyId == "deep_one_clerk" || enemyId == "window_god" ||
                   (!string.IsNullOrEmpty(enemyId) && enemyId.StartsWith("stage_boss_"));
        }
        void ShowBossPrep(string enemyId)
        {
            mode = Mode.Scene;
            run.flags.Add("prep_" + enemyId);
            battleRoot.gameObject.SetActive(false);
            choiceRoot.gameObject.SetActive(true);
            SetBackground(BossPrepBackground(enemyId));
            SetPortrait(BossPrepPortrait(enemyId));
            titleText.text = "ボス前準備";
            areaText.text = BossPrepArea(enemyId);
            bodyText.text = BossPrepText(enemyId) + "\n\n準備を一つだけ選べる。装備、キャラ固有性、これまでの地方経験によって選択の重みが変わる。";
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
            AddChoiceButton("退路を確保する\n速さ+1/危険察知+1", () =>
            {
                run.stats.speed += 1;
                run.dangerWarnings += 1;
                run.flags.Add("boss_escape_route_" + enemyId);
                StartBattle(enemyId);
            });
            AddChoiceButton("SANを削る\n攻撃+4 / SAN-5", () =>
            {
                run.stats.attack += 4;
                run.stats.sanity = Math.Max(1, run.stats.sanity - 5);
                run.flags.Add("boss_desperate_attack_" + enemyId);
                StartBattle(enemyId);
            });
            AddChoiceButton("地方の手掛かりを使う\n敵攻防-1", () =>
            {
                run.flags.Add("boss_local_clue_" + enemyId);
                run.stats.localKnowledge += run.character.id == "local" ? 1 : 0;
                StartBattle(enemyId);
            }, run.stats.localKnowledge + run.owari + run.mikawa + run.npcAirport >= 4 || run.character.id == "local");
            UpdateSideText();
        }
        string BossPrepBackground(string enemyId)
        {
            if (enemyId == "miso_voice" || enemyId == "stage_boss_3") return "miso";
            if (enemyId == "well_tentacle" || enemyId == "shadow_retainer" || enemyId == "stage_boss_2") return "castle";
            if (enemyId == "deep_one_clerk" || enemyId == "stage_boss_4") return "gamagori";
            if (enemyId == "window_god" || enemyId == "gate_guard" || enemyId == "boundary_airport_director" || enemyId == "stage_boss_5") return "airport";
            return "station";
        }
        string BossPrepPortrait(string enemyId)
        {
            if (enemies.ContainsKey(enemyId))
                return enemies[enemyId].portrait;
            return enemyId == "miso_voice" ? "miso_voice" : "gate_inspector";
        }
        string BossPrepArea(string enemyId)
        {
            if (enemyId == "miso_voice" || enemyId == "stage_boss_3") return "三河 / 樽の前";
            if (enemyId == "gate_guard" || enemyId == "window_god" || enemyId == "boundary_airport_director" || enemyId == "stage_boss_5") return "空港 / 搭乗口";
            if (enemyId == "stage_boss_2" || enemyId == "well_tentacle" || enemyId == "shadow_retainer") return "尾張 / 境界核";
            if (enemyId == "stage_boss_4" || enemyId == "deep_one_clerk") return "知多・蒲郡 / 境界核";
            return "名駅 / 境界核";
        }
        string BossPrepText(string enemyId)
        {
            if (enemyId.StartsWith("stage_boss_"))
                return "十の出来事がボスゲートの奥で一つに束ねられている。ここで何を削り、何を守るかが、戦闘の入り方を変える。";
            if (enemyId == "miso_voice")
                return "樽の奥で、あなたの声に似たものが発酵している。";
            if (enemyId == "boundary_airport_director")
                return "管制室の椅子に座るものは、外なる神ではない。だからこそ、ここで倒さなければ帰還便は欠航のままになる。";
            if (enemyId == "gate_guard" || enemyId == "window_god")
                return "搭乗口の外側で、まだ呼ばれていない便名があなたを待っている。";
            return "決定的な怪異が、すぐ先であなたを待っている。";
        }
        void AddRetreatOption()
        {
            // Battle uses its own compact controls; keep the scene command panel hidden so it does not overlap.
            choiceRoot.gameObject.SetActive(false);
            ClearChoices();
            footerText.text = "戦闘中: 攻撃と固有スキルで突破します。撤退は次の調整でバトル専用ボタンへ移します。";
        }
        string BossBattlePrelude(string enemyId)
        {
            string text = "";
            switch (enemyId)
            {
                case "stage_boss_1":
                    text = "名駅地下で拾った番号札が、すべて同じロッカーを指している。ここを折り畳まなければ、次の土地へ続く線路は生まれない。";
                    break;
                case "stage_boss_2":
                    text = "尾張で集めた鯱の視線が、城の影に王冠を作った。影王を退けなければ、帰路は屋根瓦の下へ縫い込まれる。";
                    break;
                case "stage_boss_3":
                    text = "三河の樽鳴りは、声ではなく命令になった。発酵する声塊を沈めなければ、あなたの呼吸まで工程に組み込まれる。";
                    break;
                case "stage_boss_4":
                    text = "海底星図の監査印が、空港へ向かう許可を拒んでいる。ここで割らなければ、橋も滑走路も海の帳簿に回収される。";
                    break;
                case "stage_boss_5":
                    text = "搭乗門の外側に溜まった小神群が、帰還という選択肢を食べている。最後のゲートを開くには、見ないまま進む勇気がいる。";
                    break;
                case "well_tentacle":
                    text = "尾張の井戸は、ランダムな怪異ではなく城下に残った古い水脈の口だった。底を塞がなければ、鯱の影まで引きずり込まれる。";
                    break;
                case "shadow_retainer":
                    text = "三河で重ねた選択が、あなたより一歩強い影武者を作った。影を越えなければ、次の道は常に先回りされる。";
                    break;
                case "deep_one_clerk":
                    text = "知多の海路を使うたび、改札鋏は濡れた音を覚えた。深きものの係員を退けなければ、空港行きの切符は海底で切られる。";
                    break;
                case "gate_guard":
                    text = "空港の搭乗検査官は、荷物ではなく記憶を検査している。通過するには、捨てる記憶と持ち帰る記憶をここで選ばなければならない。";
                    break;
                case "boundary_airport_director":
                    text = "境界空港長は、帰還便を欠航にするためだけにこの世界の規則へ縛られている。外側のものではない。倒せる門番だ。";
                    break;
                case "miso_voice":
                    text = "三度目の声は、あなたの喉を借りて帰還を拒んでいる。沈めなければ、次に話す言葉はすべて樽の中から出る。";
                    break;
                case "window_god":
                    text = "空港の窓外にいた小さな神は、見られなかった回数だけ近づいた。戦うというより、視線の契約をここで断つ。";
                    break;
            }
            return string.IsNullOrEmpty(text) ? "" : text + "\n\n";
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
            footerText.text = "攻撃はいつでも可能。ただしゲージ不足では刃が浅い。七割以上で本命、満タン付近で大きく伸びる。";
        }
        void ApplyBattleOpeningTraits(EnemyDef enemy)
        {
            if (run == null || enemy == null)
                return;
            if (HasGearEffect("opening_attack"))
                attackGauge = Mathf.Max(attackGauge, 0.25f);
            if (HasGearEffect("opening_guard"))
                guardGauge = Mathf.Max(guardGauge, 0.25f);
            if (HasGearEffect("danger_preparation"))
                run.dangerWarnings += 1;
            if (run.character.id == "toyohashi_conductor")
                attackGauge = Mathf.Max(attackGauge, 0.35f);
            if (run.character.id == "mechanic" && IsMachineOrAirportEnemy(enemy))
                guardGauge = Mathf.Max(guardGauge, 0.35f);
            if (run.character.id == "occult" && IsBossEnemy(enemy.id))
                enemy.defense = Mathf.Max(0, enemy.defense - 1);
            if (run.flags.Contains("boss_local_clue_" + enemy.id))
            {
                enemy.attack = Mathf.Max(1, enemy.attack - 1);
                enemy.defense = Mathf.Max(0, enemy.defense - 1);
            }
            attackSlider.value = attackGauge;
            guardSlider.value = guardGauge;
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
            float tier = AttackGaugeDamageRate(gaugePower) + Mathf.Min(0.16f, (run.stats.speed + run.weapon.speed + run.accessory.speed) * 0.012f);
            int baseDamage = run.stats.attack + run.weapon.attack + run.accessory.attack + Mathf.RoundToInt(1 + gaugePower * 10f) - activeEnemy.defense;
            int damage = Mathf.Max(1, Mathf.RoundToInt(Mathf.Max(1, baseDamage) * tier));
            if (HasGearEffect("airport_slayer") && IsMachineOrAirportEnemy(activeEnemy))
                damage = Mathf.CeilToInt(damage * 1.2f);
            if (HasGearEffect("mythic_edge") && IsBossEnemy(activeEnemy.id))
                damage += Mathf.Max(1, run.stats.mythosKnowledge / 2);
            if (HasGearEffect("desperate_power") && run.stats.sanity <= run.stats.maxSanity / 2)
                damage = Mathf.CeilToInt(damage * 1.25f);
            if (run.character.id == "samurai" && IsBossEnemy(activeEnemy.id))
                damage += 2 + run.stats.attack / 3;
            if (run.character.id == "gamagori_diver" && (activeEnemy.image == "gamagori" || activeEnemy.id == "deep_one_clerk" || activeEnemy.id == "stage_boss_4"))
                damage += 4;
            if (run.character.id == "final_observer")
            {
                damage += 18 + run.stats.mythosKnowledge * 2;
                if (activeEnemy.id == "impossible_one")
                    damage += 80 + run.stats.luck * 3;
            }
            if (activeEnemy.id == "impossible_one" && run.stats.mythosKnowledge >= 10)
                damage += run.stats.mythosKnowledge * 4 + run.stats.luck * 2;
            bool crit = gaugePower >= 0.65f && rng.NextDouble() < 0.06 + (run.stats.luck + run.accessory.luck) * 0.01f + gaugePower * 0.035f;
            if (crit)
                damage = Mathf.RoundToInt(damage * 1.65f);
            activeEnemy.hp = Mathf.Max(0, activeEnemy.hp - damage);
            enemyHpSlider.value = activeEnemy.hp;
            attackGauge = 0f;
            attackSlider.value = 0f;
            Play(hitSfx);
            bodyText.text = activeEnemy.name + "へ " + damage + " ダメージ。\n" + AttackGaugeFlavor(gaugePower) + (crit ? "\n会心。" : "");
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
        float AttackGaugeDamageRate(float gaugePower)
        {
            if (gaugePower < 0.35f)
                return Mathf.Lerp(0.12f, 0.28f, gaugePower / 0.35f);
            if (gaugePower < 0.70f)
                return Mathf.Lerp(0.45f, 0.86f, (gaugePower - 0.35f) / 0.35f);
            if (gaugePower < 0.95f)
                return Mathf.Lerp(1.00f, 1.28f, (gaugePower - 0.70f) / 0.25f);
            return 1.55f;
        }
        string AttackGaugeFlavor(float gaugePower)
        {
            if (gaugePower < 0.35f)
                return "踏み込みが浅い。傷はついたが、怪異の芯には届いていない。";
            if (gaugePower < 0.70f)
                return "刃は届いた。だが、まだ押し切るには足りない。";
            if (gaugePower < 0.95f)
                return "十分に溜めた一撃が、怪異の輪郭を割った。";
            return "限界まで引き絞った一撃が、現実側へ強く叩き戻した。";
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
                if (run.character != null && run.character.id == "final_observer" && run.flags.Contains("final_boss_rush"))
                {
                    int impossibleDamage = Mathf.Max(18, activeEnemy.attack - Mathf.RoundToInt((run.stats.defense + run.armor.defense + run.accessory.defense) * 0.42f));
                    run.stats.hp = Mathf.Max(0, run.stats.hp - impossibleDamage);
                    int impossibleSan = Mathf.Max(2, activeEnemy.sanityDamage - run.stats.mythosKnowledge / 8);
                    run.stats.sanity = Mathf.Max(0, run.stats.sanity - impossibleSan);
                    Play(doomSfx);
                    bodyText.text = activeEnemy.name + "が現実外から触れる。HP -" + impossibleDamage + " / SAN -" + impossibleSan;
                    UpdateSideText();
                    if (run.stats.hp <= 0 || run.stats.sanity <= 0)
                    {
                        ShowEnding("impossible_death");
                        return;
                    }
                    battleRound++;
                    battleText.text = "Round " + battleRound + "\n観測を継続する";
                    ScheduleEnemyAttack();
                    return;
                }
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
            if (run.character.id == "seto_potter")
                blocked += 2;
            if (HasGearEffect("miso_guard") && IsMisoEnemy(activeEnemy))
                blocked += 2;
            int damage = Mathf.Max(1, raw - blocked);
            run.stats.hp = Mathf.Max(0, run.stats.hp - damage);
            int sanityLoss = Mathf.Max(0, activeEnemy.sanityDamage - Mathf.RoundToInt((run.stats.luck + run.accessory.luck) * 0.15f));
            if (HasGearEffect("san_guard"))
                sanityLoss = Mathf.Max(0, sanityLoss - 1);
            if (HasGearEffect("miso_guard") && IsMisoEnemy(activeEnemy))
                sanityLoss = Mathf.Max(0, sanityLoss - 1);
            if (HasGearEffect("desperate_power") && run.stats.sanity <= run.stats.maxSanity / 2)
                sanityLoss += 1;
            if (run.character.id == "atsuta_miko" && IsBossEnemy(activeEnemy.id))
                sanityLoss = Mathf.Max(0, sanityLoss - 1);
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
                string nextAfterBattle = !string.IsNullOrEmpty(run.battleReturnScene) ? run.battleReturnScene : run.sceneId;
                if (IsBossEnemy(activeEnemy.id))
                    EnsureSanityCollapseQueued(nextAfterBattle);
                else
                {
                    QueueSanityCollapse(nextAfterBattle);
                    return;
                }
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
            float baseDelay = UnityEngine.Random.Range(2.45f, 5.15f);
            float speedPressure = Mathf.Clamp(activeEnemy.speed - run.stats.speed, -4f, 8f) * 0.18f;
            float bossPressure = IsBossEnemy(activeEnemy.id) ? 0.12f : 0f;
            enemyAttackTimer = Mathf.Clamp(baseDelay - speedPressure - bossPressure + UnityEngine.Random.Range(-0.30f, 0.75f), 1.65f, 6.2f);
        }
        bool IsMachineOrAirportEnemy(EnemyDef enemy)
        {
            if (enemy == null)
                return false;
            return enemy.id.Contains("gate") || enemy.id.Contains("window") || enemy.id.Contains("baggage") ||
                   enemy.id.Contains("stage_boss_5") || enemy.image == "airport" || enemy.image == "toyota";
        }
        bool IsMisoEnemy(EnemyDef enemy)
        {
            if (enemy == null)
                return false;
            return enemy.id.Contains("miso") || enemy.id == "stage_boss_3" || enemy.image == "miso";
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
                    if (IsMachineOrAirportEnemy(activeEnemy))
                        activeEnemy.attack = Mathf.Max(1, activeEnemy.attack - 2);
                    message = "出張会社員の危機対応。HP+5 / SAN+1" + (IsMachineOrAirportEnemy(activeEnemy) ? " / 機械系の敵攻撃-2" : "");
                    break;
                case "local":
                  activeEnemy.hp = Mathf.Max(0, activeEnemy.hp - Mathf.Max(3, run.stats.localKnowledge + run.stats.luck / 2));
                    run.dangerWarnings += 1;
                    message = "地元出身者の抜け道看破。敵HPを削った。";
                    break;
                case "occult":
                    activeEnemy.defense = Mathf.Max(0, activeEnemy.defense - (IsBossEnemy(activeEnemy.id) ? 3 : 2));
                    run.stats.mythosKnowledge += 1;
                    run.stats.sanity = Math.Max(0, run.stats.sanity - 1);
                    message = "オカルト研究者の解析。敵防御低下 / 神話理解+1 / SAN-1";
                    break;
                case "samurai":
                    activeEnemy.hp = Mathf.Max(0, activeEnemy.hp - Mathf.Max(6, run.stats.attack + run.weapon.attack));
                    message = "三河武士の裂帛。追加攻撃が入った。";
                    break;
                case "mechanic":
                    run.stats.hp = Math.Min(run.stats.maxHp, run.stats.hp + 4);
                    run.stats.defense += 1;
                    if (IsMachineOrAirportEnemy(activeEnemy))
                        guardGauge = 1f;
                    message = "工場の整備士の応急補修。HP+4 / 防御+1" + (IsMachineOrAirportEnemy(activeEnemy) ? " / 次の防御準備完了" : "");
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
                case "final_observer":
                    int observedDamage = Mathf.Max(30, run.stats.mythosKnowledge * 3 + run.stats.luck + run.stats.attack / 2);
                    if (activeEnemy.id == "impossible_one")
                        observedDamage += 70;
                    activeEnemy.hp = Mathf.Max(0, activeEnemy.hp - observedDamage);
                    activeEnemy.attack = Mathf.Max(1, activeEnemy.attack - 3);
                    activeEnemy.defense = Mathf.Max(0, activeEnemy.defense - 3);
                    run.stats.sanity = Math.Min(run.stats.maxSanity, run.stats.sanity + 8);
                    message = "境界外の観測。敵HP-" + observedDamage + " / 敵攻防-3 / SAN+8";
                    break;
                default:
                    run.stats.hp = Math.Min(run.stats.maxHp, run.stats.hp + 3);
                    run.dangerWarnings += 1;
                    message = "旅行者の直感。HP+3 / 危険察知+1";
                    break;
            }
            enemyHpSlider.value = activeEnemy.hp;
            guardSlider.value = guardGauge;
            bodyText.text = message;
            UpdateSideText();
           if (activeEnemy.hp <= 0)
                WinBattle();
            else if (run.stats.sanity <= 0)
            {
                string nextAfterBattle = !string.IsNullOrEmpty(run.battleReturnScene) ? run.battleReturnScene : run.sceneId;
                if (IsBossEnemy(activeEnemy.id))
                    EnsureSanityCollapseQueued(nextAfterBattle);
                else
                    QueueSanityCollapse(nextAfterBattle);
            }
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
            EnsureSanityCollapseQueued(returnScene);
            ShowSanityWarning(run.sanityCollapseReturnScene);
        }
        void EnsureSanityCollapseQueued(string returnScene)
        {
            if (run == null)
                return;
            run.sanityCollapseReturnScene = string.IsNullOrEmpty(returnScene) ? run.sceneId : returnScene;
            if (run.sanityCollapseTurns < 0)
                run.sanityCollapseTurns = 3;
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
                "まだ戦闘ではない。けれどSANが底を打ち、数歩先の影がこちらを待っている。\n" +
                "それまでに正気を取り戻せなければ、愛知の街並みは少しずつ別の地図へ置き換わっていく。";
            footerText.text = "SAN 0: 影が近い。あと " + Mathf.Max(1, run.sanityCollapseTurns) + " 回進むと、帰れない影が立ち上がる。";
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
            titleText.text = "帰れない影";
            areaText.text = "熱田 / 正気祓い";
            bodyText.text =
                "熱田の鳥居の影が、あなたの歩幅に合わせて一本ずつ増えていく。\n\n" +
                "影はボスではない。あなた自身が現実に戻るための、最後の結び目だ。";
            footerText.text = "影は足元から離れない。正気で払うか、神話で縫い止める。";
            UpdateSideText();
            ClearChoices();
            AddChoiceButton("正気の欠片を拾う\nHP-4 / SAN+4", () =>
            {
                var before = run.stats.Clone();
                run.stats.hp = Math.Max(1, run.stats.hp - 4);
                run.stats.sanity = Math.Min(run.stats.maxSanity, run.stats.sanity + 4);
                string delta = BuildStatDelta(before, run.stats);
                LogRun("帰れない影: 正気の欠片 " + delta);
                ShowChoiceOutcome(returnScene, "正気の欠片を拾う\n" + delta);
            });
            AddChoiceButton("神話で縫い止める\n神話+1 / SAN+2", () =>
            {
                var before = run.stats.Clone();
                run.stats.mythosKnowledge += 1;
                run.stats.mythosCorruption += 1;
                run.stats.sanity = Math.Min(run.stats.maxSanity, run.stats.sanity + 2);
                string delta = BuildStatDelta(before, run.stats);
                LogRun("帰れない影: 神話で縫い止める " + delta);
                ShowChoiceOutcome(returnScene, "神話で縫い止める\n" + delta);
            });
            AddChoiceButton("見ないふりで歩く\n危険察知+1", () =>
            {
                var before = run.stats.Clone();
                run.dangerWarnings += 1;
                run.stats.mythosCorruption += 2;
                run.stats.sanity = 1;
                string delta = BuildStatDelta(before, run.stats);
                LogRun("帰れない影: 見ないふり " + delta);
                ShowChoiceOutcome(returnScene, "見ないふりで歩く\n" + delta + "\n危険察知+1");
            });
            AddChoiceButton("SANを回復しない\n危険察知+1", () =>
            {
                var before = run.stats.Clone();
                run.dangerWarnings += 1;
                run.stats.mythosCorruption += 2;
                run.stats.sanity = 0;
                run.sanityCollapseTurns = 4;
                run.sanityCollapseReturnScene = returnScene;
                run.suppressSanityQueueOnce = true;
                string delta = BuildStatDelta(before, run.stats);
                LogRun("SAN no recovery: " + delta);
                run.flags.Add("impossible_nonboss_spawned");
                run.battleReturnScene = returnScene;
                StartBattle("impossible_one");
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
            QueueSanityCollapse(run != null ? run.sceneId : null);
        }
        readonly string[] finalBossRushEnemies =
        {
            "stage_boss_1",
            "stage_boss_2",
            "stage_boss_3",
            "stage_boss_4",
            "stage_boss_5",
            "well_tentacle",
            "shadow_retainer",
            "deep_one_clerk",
            "window_god",
            "gate_guard",
            "impossible_one"
        };
        void ShowFinalBossRushIntro()
        {
            mode = Mode.Scene;
            battleRoot.gameObject.SetActive(false);
            choiceRoot.gameObject.SetActive(true);
            SetBackground("ending");
            SetPortrait("final_observer");
            titleText.text = "最終観測: ボスラッシュ";
            areaText.text = "境界核 / 観測者専用";
            bodyText.text = "境界外の観測者は通常ルートへ入らない。\n\n各STAGEボスと深部ボスを連続で戦闘可能な輪郭へ固定し、最後に「この世のものとは思えないもの」を倒す。";
            footerText.text = "最終キャラ専用。連戦中は名のない気配、装備入手、通常移動を挟まない。";
            run.flags.Add("final_boss_rush");
            run.finalRushIndex = 0;
            run.randomCooldown = 999;
            run.stats.hp = run.stats.maxHp;
            run.stats.sanity = run.stats.maxSanity;
            UpdateSideText();
            ClearChoices();
            AddChoiceButton("連戦を開始", StartNextFinalBossRushBattle);
            AddChoiceButton("キャラ選択へ戻る", ShowCharacterSelect);
        }
        void StartNextFinalBossRushBattle()
        {
            if (run == null || run.finalRushIndex >= finalBossRushEnemies.Length)
            {
                ShowEnding("final_observer_end");
                return;
            }
            string enemyId = finalBossRushEnemies[run.finalRushIndex];
            run.battleReturnScene = "final_boss_rush";
            run.stats.hp = Math.Min(run.stats.maxHp, run.stats.hp + 18);
            run.stats.sanity = Math.Min(run.stats.maxSanity, run.stats.sanity + 10);
            LogRun("最終観測 " + (run.finalRushIndex + 1) + "/" + finalBossRushEnemies.Length + ": " + enemyId);
            StartBattle(enemyId);
        }
        void ShowFinalBossRushVictory(string defeatedEnemyId)
        {
            mode = Mode.Scene;
            battleRoot.gameObject.SetActive(false);
            choiceRoot.gameObject.SetActive(true);
            run.finalRushIndex++;
            bool finalDone = defeatedEnemyId == "impossible_one" || run.finalRushIndex >= finalBossRushEnemies.Length;
            SetBackground(finalDone ? "ending" : BossPrepBackground(finalBossRushEnemies[Mathf.Min(run.finalRushIndex, finalBossRushEnemies.Length - 1)]));
            SetPortrait(finalDone ? "final_observer" : BossPrepPortrait(finalBossRushEnemies[run.finalRushIndex]));
            titleText.text = finalDone ? "観測完了" : "観測継続";
            areaText.text = "境界核 / " + run.finalRushIndex + "/" + finalBossRushEnemies.Length;
            bodyText.text = finalDone
                ? "最後の輪郭が砕けた。この世のものとは思えないものは、もう記録の外側にいない。"
                : "撃破したボスの輪郭が観測記録へ畳まれていく。\n\n次の戦闘前にHPとSANが少し戻る。";
            run.stats.hp = Math.Min(run.stats.maxHp, run.stats.hp + 26);
            run.stats.sanity = Math.Min(run.stats.maxSanity, run.stats.sanity + 14);
            UpdateSideText();
            ClearChoices();
            if (finalDone)
                AddChoiceButton("専用EDへ", () => ShowEnding("final_observer_end"));
            else
                AddChoiceButton("次のボスへ", StartNextFinalBossRushBattle);
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
            if (run != null && run.character != null && run.character.id == "final_observer")
            {
                enemy.reward += 12;
                if (run.flags.Contains("final_boss_rush"))
                    enemy.intro += "\n\n観測者の視線が、怪異を逃げ場のない戦闘形状へ固定している。";
            }
            if (run != null && run.flags.Contains("final_boss_rush") && enemy.id == "impossible_one")
            {
                enemy.maxHp = 420;
                enemy.hp = enemy.maxHp;
                enemy.attack = 46;
                enemy.defense = 18;
                enemy.speed = 18;
                enemy.sanityDamage = 10;
                enemy.reward = 180;
                enemy.intro += "\n\n通常なら一撃で現実を断つ相手だが、観測者だけがそれを戦闘ルールの内側へ縫い留められる。";
            }
            if (run != null && run.flags.Contains("boss_local_clue_" + enemy.id))
            {
                enemy.maxHp = Mathf.CeilToInt(enemy.maxHp * 0.92f);
                enemy.hp = enemy.maxHp;
                enemy.intro += "\n\n集めた土地の手掛かりが、怪異の輪郭を少しだけ現実側へ引き戻した。";
            }
            if (run != null && run.flags.Contains("boss_escape_route_" + enemy.id))
                enemy.speed = Mathf.Max(0, enemy.speed - 1);
            if (progress.bossesDefeated.Contains(enemy.id))
            {
               enemy.maxHp = Mathf.CeilToInt(enemy.maxHp * 0.92f);
                enemy.hp = enemy.maxHp;
                enemy.reward += 2;
                enemy.intro += "\n\n図鑑に残った過去の勝利が、敵の癖をわずかに浮かび上がらせる。";
            }
        }
        void ApplyEnemyThreatBalance(EnemyDef enemy)
        {
            if (enemy == null || run == null || enemy.id == "impossible_one")
                return;
            bool boss = IsBossEnemy(enemy.id);
            if (boss)
            {
                float hpRate = enemy.id == "boundary_airport_director" ? 1.36f : 1.28f;
                float attackRate = enemy.id == "boundary_airport_director" ? 1.30f : 1.22f;
                enemy.maxHp = Mathf.CeilToInt(enemy.maxHp * hpRate);
                enemy.hp = enemy.maxHp;
                enemy.attack = Mathf.CeilToInt(enemy.attack * attackRate);
                enemy.defense += enemy.id.StartsWith("stage_boss_") ? 2 : 1;
                enemy.sanityDamage += 1;
                enemy.reward = Mathf.CeilToInt(enemy.reward * 1.18f);
                enemy.intro += "\n\n正面から削り切るには重い。ゲージを溜め、準備と弱点で輪郭を崩す必要がある。";
                return;
            }
            enemy.maxHp = Mathf.CeilToInt(enemy.maxHp * 1.14f);
            enemy.hp = enemy.maxHp;
            enemy.attack = Mathf.CeilToInt(enemy.attack * 1.18f);
            enemy.defense += enemy.maxHp >= 35 ? 1 : 0;
            enemy.sanityDamage += enemy.sanityDamage >= 4 ? 1 : 0;
            enemy.reward = Mathf.CeilToInt(enemy.reward * 1.08f);
        }
        void WinBattle()
        {
            int reward = activeEnemy.reward;
            if (run.flags.Contains("bad_dice"))
                reward = Mathf.CeilToInt(reward * 1.2f);
            int moneyReward = Mathf.CeilToInt(reward * run.character.rewardRate) * 12;
            int memoryReward = BattleMemoryReward(activeEnemy.id);
            run.stats.money += moneyReward;
            progress.memoryFragments += memoryReward;
            if (!progress.defeated.Contains(activeEnemy.id))
                progress.defeated.Add(activeEnemy.id);
            RegisterVictoryCounter(activeEnemy.id);
            if (IsBossEnemy(activeEnemy.id) && !progress.bossesDefeated.Contains(activeEnemy.id))
                progress.bossesDefeated.Add(activeEnemy.id);
            if (!progress.monsterWeaknesses.Contains(activeEnemy.id))
                progress.monsterWeaknesses.Add(activeEnemy.id);
            CheckMilestones();
            SaveProgress();
            Play(rewardSfx);
            if (activeEnemy.id == "impossible_one")
            {
                if (run.flags.Contains("final_boss_rush") && run.character.id == "final_observer")
                {
                    LogRun("最終観測完了: この世のものとは思えないものを撃破");
                    ShowFinalBossRushVictory(activeEnemy.id);
                    return;
                }
                LogRun("ありえない敵を倒した。世界はあなたを搭乗者ではなく観測者として記録した。");
                ShowEnding("impossible_true");
                return;
            }
            if (run.flags.Contains("final_boss_rush") && run.character.id == "final_observer")
            {
                ShowFinalBossRushVictory(activeEnemy.id);
                return;
            }
            string next = !string.IsNullOrEmpty(run.battleReturnScene) ? run.battleReturnScene :
                          activeEnemy.id == "piyorin" ? "nagoya_after_battle" :
                          activeEnemy.id == "miso_voice" ? "okazaki_after_battle" :
                          activeEnemy.id == "gate_guard" ? "airport_gate" : "nagoya_after_battle";
            run.battleReturnScene = null;
            run.pendingGear = GenerateRandomGear(activeEnemy.reward + run.instability * 2);
            LogRun("戦闘勝利: " + activeEnemy.name + " 所持金+" + moneyReward + (memoryReward > 0 ? " / 記憶片+" + memoryReward : ""));
            bodyText.text = activeEnemy.victoryText + "\n\n所持金 +" + moneyReward + (memoryReward > 0 ? "\n記憶片 +" + memoryReward : "\n記憶片は残らなかった。");
            battleRoot.gameObject.SetActive(false);
          choiceRoot.gameObject.SetActive(true);
            ClearChoices();
            enemyAttackTimer = 0f;
            activeEnemy = null;
            AddChoiceButton("戦利品を見る", () => ShowGearOffer(next));
            UpdateSideText();
            AddChoiceButton("次に進む", () => ShowScene(next));
        }
        int BattleMemoryReward(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId))
                return 0;
            bool firstDefeat = !progress.defeated.Contains(enemyId);
            if (enemyId == "boundary_airport_director")
                return firstDefeat ? 3 : 0;
            if (enemyId.StartsWith("stage_boss_"))
                return firstDefeat ? 1 : 0;
            if (enemyId == "impossible_one")
                return run != null && run.flags.Contains("final_boss_rush") ? 5 : 0;
            return 0;
        }
        int AwardRareMemory(int requested)
        {
            if (requested <= 0)
                return 0;
            int granted = 0;
            if (requested >= 4)
                granted = rng.NextDouble() < 0.35 ? 1 : 0;
            else if (requested >= 2)
                granted = rng.NextDouble() < 0.22 ? 1 : 0;
            else
                granted = rng.NextDouble() < 0.12 ? 1 : 0;
            if (granted > 0)
                progress.memoryFragments += granted;
            return granted;
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
            if (tier >= 1 && rng.NextDouble() < (tier == 3 ? 0.42 : tier == 2 ? 0.30 : 0.18))
                gear.speed += 1;
            if (tier >= 2 && rng.NextDouble() < (tier == 3 ? 0.34 : 0.20))
                gear.luck += 1;
            int stat = RollGearPrimaryStat(tier);
            if (stat == 0) gear.attack += variance;
            else if (stat == 1) gear.defense += variance;
            else if (stat == 2) gear.speed += Mathf.Max(1, variance / 3);
            else gear.luck += Mathf.Max(1, variance / 3);
            ApplyDropRarity(gear, tier, power);
            return gear;
        }
        int RollGearPrimaryStat(int tier)
        {
            if (tier <= 0)
                return rng.Next(0, 2);
            int roll = rng.Next(100);
            if (tier == 1)
            {
                if (roll < 43) return 0;
                if (roll < 86) return 1;
                if (roll < 97) return 2;
                return 3;
            }
            if (tier == 2)
            {
                if (roll < 37) return 0;
                if (roll < 74) return 1;
                if (roll < 91) return 2;
                return 3;
            }
            if (roll < 32) return 0;
            if (roll < 64) return 1;
            if (roll < 84) return 2;
            return 3;
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
                note = "持ち込みなし。現地で拾った装備だけが頼りになる。",
                effect = ""
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
            gear.speed += rng.NextDouble() < 0.22 ? rng.Next(0, 2) : 0;
            gear.luck += rng.NextDouble() < 0.14 ? rng.Next(0, 2) : 0;
            ApplyGearRarity(gear);
            return gear;
        }
        void ApplyGearRarity(Gear gear)
        {
            gear.score = GearScore(gear);
            if (gear.score >= 24) { gear.rarity = "橙"; if (!gear.name.StartsWith("伝説の")) gear.name = "伝説の" + gear.name; }
            else if (gear.score >= 17) { gear.rarity = "紫"; if (!gear.name.StartsWith("異質な")) gear.name = "異質な" + gear.name; }
            else if (gear.score >= 11) { gear.rarity = "緑"; if (!gear.name.StartsWith("貴重な")) gear.name = "貴重な" + gear.name; }
            else gear.rarity = "白";
            EnforceMinimumRarityLuckRule(gear);
        }
        int GearScore(Gear gear)
        {
            if (gear == null)
                return 0;
            return gear.attack * 3 + gear.defense * 3 + gear.speed * 5 + gear.luck * 6;
        }
        int SelectDropRarityTier(int power)
        {
            float pressure = Mathf.Clamp01(power / 34f);
            float orange = 0.006f + pressure * 0.028f + run.instability * 0.002f;
            float purple = 0.034f + pressure * 0.078f + run.instability * 0.006f;
            float green = 0.18f + pressure * 0.105f + run.instability * 0.004f;
            double roll = rng.NextDouble();
            if (roll < orange) return 3;
            if (roll < orange + purple) return 2;
            if (roll < orange + purple + green) return 1;
            return 0;
        }
        void ApplyDropRarity(Gear gear, int tier, int power)
        {
            string baseName = StripRarityPrefix(gear.name);
            float multiplier = tier == 3 ? 3.45f : tier == 2 ? 2.3f : tier == 1 ? 1.38f : 1f;
            int flat = tier == 3 ? 9 : tier == 2 ? 5 : tier == 1 ? 2 : 0;
            int powerBonus = tier == 0 ? 0 : Mathf.Clamp(power / 12, 0, tier == 3 ? 6 : tier == 2 ? 4 : 2);
           gear.attack = Mathf.Max(0, Mathf.RoundToInt(gear.attack * multiplier));
            gear.defense = Mathf.Max(0, Mathf.RoundToInt(gear.defense * multiplier));
            gear.speed = Mathf.RoundToInt(gear.speed * (1f + (multiplier - 1f) * 0.38f));
            gear.luck = Mathf.Max(0, Mathf.RoundToInt(gear.luck * (1f + (multiplier - 1f) * 0.42f)));
            if (gear.slot == "武器") gear.attack += flat + powerBonus;
            else if (gear.slot == "防具") gear.defense += flat + powerBonus;
            else if (tier > 0 && rng.NextDouble() < (tier == 3 ? 0.55 : tier == 2 ? 0.34 : 0.16)) gear.luck += Mathf.Max(1, flat / 3 + powerBonus / 2);
            gear.score = GearScore(gear);
            if (tier == 3) { gear.rarity = "橙"; gear.name = "伝説の" + baseName; }
            else if (tier == 2) { gear.rarity = "紫"; gear.name = "異質な" + baseName; }
            else if (tier == 1) { gear.rarity = "緑"; gear.name = "貴重な" + baseName; }
            else { gear.rarity = "白"; gear.name = baseName; }
            EnforceMinimumRarityLuckRule(gear);
            AssignDropEffect(gear, tier);
            gear.note = gear.note + "\n希少度補正: " + DropRarityNote(tier);
            if (!string.IsNullOrEmpty(gear.effect))
                gear.note += "\n特殊効果: " + GearEffectText(gear.effect);
        }
        void AssignDropEffect(Gear gear, int tier)
        {
            if (gear == null || tier <= 0)
            {
                if (gear != null)
                    gear.effect = "";
                return;
            }
            var candidates = new List<string>();
            if (gear.slot == "武器")
            {
                candidates.Add("opening_attack");
                candidates.Add("mythic_edge");
                candidates.Add("airport_slayer");
            }
            else if (gear.slot == "防具")
            {
                candidates.Add("opening_guard");
                candidates.Add("san_guard");
                candidates.Add("miso_guard");
            }
            else
            {
                candidates.Add("luck_reroll");
                candidates.Add("stage_bonus");
                candidates.Add("memory_anchor");
            }
            if (tier >= 2)
                candidates.Add("danger_preparation");
            if (tier >= 3)
                candidates.Add("desperate_power");
            gear.effect = candidates[rng.Next(candidates.Count)];
        }
        void EnforceMinimumRarityLuckRule(Gear gear)
        {
            if (gear == null || gear.rarity != "白")
                return;
            gear.speed = 0;
            gear.luck = 0;
            gear.score = gear.attack * 3 + gear.defense * 3;
        }
        string DropRarityNote(int tier)
        {
            if (tier == 3) return "極低確率。全体性能が大きく跳ね上がる。";
            if (tier == 2) return "低確率。基礎性能と補助性能が大きく上がる。";
            if (tier == 1) return "やや希少。通常品より明確に扱いやすい。";
            return "通常品。速さとLUK補正は付かない。";
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
            bodyText.text = GearAcquisitionText(run.pendingGear) + "\n\n" + GearComparison(run.pendingGear, current);
            ClearChoices();
            AddChoiceButton("装備する", () => { LogRun("装備変更: " + GearShortName(run.pendingGear)); EquipGear(run.pendingGear); run.pendingGear = null; UpdateSideText(); ShowScene(next); });
            AddChoiceButton("倉庫へ送る\n喪失リスク", () => TryStorePendingGear(next));
            AddChoiceButton("捨てる", () => { run.pendingGear = null; ShowScene(next); });
            footerText.text = "希少度: " + GearRarityScarcity(run.pendingGear.rarity) + " / " + GearRarityPowerText(run.pendingGear);
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
            string effect = string.IsNullOrEmpty(gear.effect) ? "" : "\n効果: " + GearEffectText(gear.effect);
            return gear.name + " [" + gear.rarity + "]\n攻+" + gear.attack + " 防+" + gear.defense + " 速+" + gear.speed + " LUK+" + gear.luck + effect + "\n" + gear.note;
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
        string GearAcquisitionText(Gear gear)
        {
            if (gear == null || IsEmptyGear(gear))
                return "戦利品は境界の向こうへ沈んだ。";

            return "戦利品: " + gear.name + "\n" +
                   "希少度 " + gear.rarity + " - " + GearRarityScarcity(gear.rarity) + "\n" +
                   GearRarityPowerText(gear) + "\n" +
                   GearEffectAcquisitionText(gear) + "\n" +
                   "強み: " + GearStrengthReason(gear);
        }
        string GearEffectAcquisitionText(Gear gear)
        {
            if (gear == null || string.IsNullOrEmpty(gear.effect))
                return "特殊効果: なし。純粋な数値更新で判断する装備。";
            return "特殊効果: " + GearEffectText(gear.effect);
        }
        string GearEffectText(string effect)
        {
            switch (effect)
            {
                case "opening_attack": return "先制。戦闘開始時に攻撃ゲージ+25%。";
                case "opening_guard": return "護符。戦闘開始時に防御ゲージ+25%。";
                case "luck_reroll": return "再抽選。LUK判定失敗時、周回中一度だけ成功へ押し上げる。";
                case "stage_bonus": return "土地勘。STAGEイベント成功時に所持金+70。";
                case "san_guard": return "正気護り。戦闘中のSAN減少を1軽減。";
                case "airport_slayer": return "対空港。空港/ゲート系の敵への与ダメージ+20%。";
                case "miso_guard": return "味噌耐性。味噌/発酵系のSAN被害と圧を軽減。";
                case "memory_anchor": return "記憶定着。倉庫転送と死亡時の装備定着が少し安定。";
                case "mythic_edge": return "神話刃。神話理解が高いほどボスへの与ダメージが増える。";
                case "danger_preparation": return "危険察知。ボス前準備とSTAGE突破で危険察知+1。";
                case "desperate_power": return "不吉な大出力。SAN半分以下で攻撃+25%、被SAN被害+1。";
            }
            return "未解析の効果。境界記録が乱れている。";
        }
        string GearRarityScarcity(string rarity)
        {
            if (rarity == "橙") return "極低確率。境界が薄い時だけ、現実へ引っかかる伝説級。";
            if (rarity == "紫") return "低確率。通常戦利品より明確に跳ねた異質品。";
            if (rarity == "緑") return "やや希少。白より一段強い、拾えたら嬉しい良品。";
            if (rarity == "白") return "出やすい通常品。速さとLUK補正は付かないが、攻撃/防御の更新に向く。";
            return "希少度不明。境界記録が乱れている。";
        }
        string GearRarityPowerText(Gear gear)
        {
            if (gear == null)
                return "";
            if (gear.rarity == "橙") return "性能: 攻防に加え、貴重な速さ/LUKが大きく付くことがある。倉庫転送は最も不安定。";
            if (gear.rarity == "紫") return "性能: 主能力が強く伸び、速さ/LUKが付けば当たり装備。";
            if (gear.rarity == "緑") return "性能: 白より扱いやすく、まれに速さやLUKが付く。";
            if (gear.rarity == "白") return "性能: 速さ+0/LUK+0固定。攻撃、防御の地味な更新を狙う装備。";
            return "性能: 詳細不明。";
        }
        string GearStrengthReason(Gear gear)
        {
            int best = Mathf.Max(gear.attack * 3, gear.defense * 3, gear.speed * 5, gear.luck * 6);
            var parts = new List<string>();
            if (gear.attack * 3 == best && gear.attack > 0)
                parts.Add("攻撃が伸び、短期決着に向く");
            if (gear.defense * 3 == best && gear.defense > 0)
                parts.Add("防御が伸び、事故死を減らす");
            if (gear.speed * 5 == best && gear.speed != 0)
                parts.Add("希少な速さ補正。ゲージ、先手、逃走に効く当たり能力");
            if (gear.luck * 6 == best && gear.luck > 0)
                parts.Add("希少なLUK補正。判定、会心、偶然の拾い方が強くなる大当たり能力");
            if (parts.Count == 0)
                parts.Add("大きな尖りはないが、現装備との差分で判断しやすい");
            return string.Join(" / ", parts);
        }
        string GearOneLine(Gear gear)
        {
            if (gear == null || IsEmptyGear(gear)) return "装備なし";
            string effect = string.IsNullOrEmpty(gear.effect) ? "" : " / " + ShortGearEffectName(gear.effect);
            return gear.name + " [" + gear.rarity + "] 攻" + Signed(gear.attack) + " 防" + Signed(gear.defense) + " 速" + Signed(gear.speed) + " LUK" + Signed(gear.luck) + effect;
        }
        string ShortGearEffectName(string effect)
        {
            if (effect == "opening_attack") return "先制";
            if (effect == "opening_guard") return "護符";
            if (effect == "luck_reroll") return "再抽選";
            if (effect == "stage_bonus") return "土地勘";
            if (effect == "san_guard") return "正気護り";
            if (effect == "airport_slayer") return "対空港";
            if (effect == "miso_guard") return "味噌耐性";
            if (effect == "memory_anchor") return "記憶定着";
            if (effect == "mythic_edge") return "神話刃";
            if (effect == "danger_preparation") return "危険察知";
            if (effect == "desperate_power") return "不吉";
            return "特殊";
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
        bool HasGearEffect(string effect)
        {
            if (run == null || string.IsNullOrEmpty(effect))
                return false;
            return GearHasEffect(run.weapon, effect) || GearHasEffect(run.armor, effect) || GearHasEffect(run.accessory, effect);
        }
        bool GearHasEffect(Gear gear, string effect)
        {
            return gear != null && !IsEmptyGear(gear) && gear.effect == effect;
        }
        string EquippedEffectSummary()
        {
            if (run == null)
                return "";
            var effects = new List<string>();
            AddEquippedEffect(effects, run.weapon);
            AddEquippedEffect(effects, run.armor);
            AddEquippedEffect(effects, run.accessory);
            return effects.Count == 0 ? "" : string.Join(" / ", effects);
        }
        void AddEquippedEffect(List<string> effects, Gear gear)
        {
            if (gear == null || IsEmptyGear(gear) || string.IsNullOrEmpty(gear.effect))
                return;
            effects.Add(ShortGearEffectName(gear.effect));
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
            if (HasGearEffect("memory_anchor") || GearHasEffect(gear, "memory_anchor") || (run.character != null && run.character.id == "centrair_agent"))
                chance += 0.08f;
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
            return gear.id + "|" + gear.name + "|" + gear.slot + "|" + gear.rarity + "|" + gear.attack + "|" + gear.defense + "|" + gear.speed + "|" + gear.luck + "|" + gear.note.Replace("|", "/") + "|" + (gear.effect ?? "");
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
            gear.effect = p.Length >= 10 ? p[9] : "";
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
            if (endingId == "return" || endingId == "normal_clear" || endingId == "true_shachi" || endingId == "impossible_true" || endingId == "final_observer_end")
                return "";
            return "\n\n死因図鑑に「" + EndingName(endingId) + "」が登録された。次回以降、近い選択肢には警告が出る。";
        }
        string EndingName(string endingId)
        {
            if (endingId == "final_observer_end")
                return "最終ED: 境界外の観測者";
            if (endingId == "impossible_death")
                return "現実外捕食";
            if (endingId == "impossible_true")
                return "裏ED: 観測者の搭乗";
            switch (endingId)
            {
                case "return": return "帰還";
                case "normal_clear": return "欠航管制突破";
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
            ClaimMilestone("piyorin_1", progress.piyorinVictories >= 1, "黄色い群体を初撃破。記憶片+1。", 1, 0);
            ClaimMilestone("piyorin_3", progress.piyorinVictories >= 3, "黄色い群体撃破3回。保険札+1。", 0, 1);
            ClaimMilestone("miso_1", progress.misoVictories >= 1, "味噌樽の声を鎮めた。記憶片+1。", 1, 0);
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
            else if (endingId == "final_observer_end")
            {
                endingTitle = "最終ED: 境界外の観測者";
                endingBody = "すべてのボスの輪郭が閉じ、この世のものとは思えないものは記録の内側へ落とされた。\n\nそれは倒されたというより、もう外側から狩ることを許されない形に観測された。";
                reward = 240;
            }
            else switch (endingId)
            {
                case "return":
                    endingTitle = "帰還";
                    endingBody = "搭乗口の先に、見慣れた朝があった。\n\nだがポケットには、使った覚えのない喫茶店の回数券が一枚だけ残っている。";
                    reward = 22;
                    break;
                case "normal_clear":
                    endingTitle = "欠航管制突破";
                    endingBody = "境界空港長は欠航印を押し損ね、管制室の時計が初めて現実の時刻へ戻った。\n\n空港の外側にいるものは倒されていない。けれど、あなたの便だけは今日の世界へ滑り込んだ。";
                    reward = 38;
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
                case "meieki_mythos":
                    endingTitle = "名駅最下層";
                    endingBody = "あなたは名駅の底が駅ではなく、愛知という夢を見るための器官だと理解した。\n\n次の電車は来ない。あなた自身が、地下へ続く路線図になった。";
                    reward = 16;
                    break;
                case "owari_thread_end":
                    endingTitle = "尾張縫合";
                    endingBody = "尾張の緯糸は、帰り道と異界の裂け目を同じ布として縫い合わせた。\n\nあなたは戻った。だが服の裏地には、まだ小さな路線図が縫い込まれている。";
                    reward = 24;
                    break;
                case "mikawa_process_end":
                    endingTitle = "三河工程化";
                    endingBody = "発酵と検査と組立の手順が、あなたの呼吸より正確に身体を動かした。\n\n人間としては帰れなかった。だが異界を処理する工程として、あなたは完成した。";
                    reward = 24;
                    break;
                case "chita_sea_takeoff":
                    endingTitle = "海底離陸";
                    endingBody = "知多の海底道は滑走路になり、波の下から音もなく機体が浮上した。\n\n窓の外には空ではなく海が広がる。それでも、あなたは確かに離陸した。";
                    reward = 24;
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
            int moneyReward = Mathf.CeilToInt(reward * (run != null ? run.character.rewardRate : 1f)) * 10;
            if (run != null && run.flags.Contains("bad_dice"))
                moneyReward = Mathf.CeilToInt(moneyReward * 1.2f);
            lastReward = EndingMemoryReward(endingId);
            if (run != null)
                run.stats.money += moneyReward;
            progress.memoryFragments += lastReward;
            if (!progress.endings.Contains(endingId))
                progress.endings.Add(endingId);
            if (endingId != "return" && endingId != "normal_clear" && endingId != "true_shachi" && endingId != "impossible_true" && endingId != "final_observer_end" && !progress.deaths.Contains(endingId))
            {
                progress.deaths.Add(endingId);
                Play(pageSfx, 0.9f);
            }
            string lossReport = HandleDeathConsequences(endingId);
            UnlockInstabilityOnClear(endingId);
            SaveProgress();
            SetBackground(endingId == "true_shachi" ? "castle" : "ending");
            SetPortrait(endingId == "final_observer_end" ? "final_observer" : endingId == "true_shachi" ? "shachi_avatar" : (endingId == "impossible_death" || endingId == "impossible_true") ? "impossible_one" : null);
            titleText.text = "END: " + endingTitle;
            areaText.text = "所持金 +" + moneyReward + " / 記憶片 +" + lastReward;
            bodyText.text = endingBody + EndingCoda(endingId) + RegionalEndingEcho(endingId) + DeathHint(endingId) + lossReport + BuildNewspaper(endingId);
            UpdateSideText();
            ClearChoices();
            if (run != null && endingId != "return" && endingId != "normal_clear" && endingId != "true_shachi" && endingId != "impossible_true" && endingId != "final_observer_end")
                AddChoiceButton("記憶定着\n装備を1つ倉庫へ", ShowMemoryAnchor);
            AddChoiceButton("次の周回へ", ShowCharacterSelect);
            AddChoiceButton("タイトルへ", ShowTitle);
      }
        int EndingMemoryReward(string endingId)
        {
            bool firstEnding = !progress.endings.Contains(endingId);
            if (!firstEnding)
                return 0;
            if (endingId == "return")
                return 2;
            if (endingId == "true_shachi" || endingId == "normal_clear")
                return 3;
            if (endingId == "final_observer_end")
                return 5;
            if (endingId == "impossible_true")
                return 4;
            return 0;
        }
        string EndingCoda(string endingId)
        {
            if (endingId == "final_observer_end")
                return "\n\n後日譚: 名駅、尾張、三河、知多、空港の境界に、小さな空白が残った。そこは出口ではない。観測者が立っていた場所だけが、まだ記録から消えずにいる。";
            switch (endingId)
            {
                case "return":
                    return "\n\n余韻: 改札を出る直前、スマホの地図から一秒だけ愛知県が消えた。戻ったとき、あなたの現在地だけが少し海側へずれていた。";
                case "normal_clear":
                    return "\n\n余韻: 欠航印の割れ目から、朝の空港アナウンスが漏れている。背後の深い気配は消えないが、今日はあなたの名前を読み損ねた。";
                case "true_shachi":
                    return "\n\n余韻: 夜明けの城で、二つの鯱は互いを見ないまま同じ方角を向いた。境界は閉じたが、屋根の金色だけはあなたを覚えている。";
                case "owari_thread_end":
                    return "\n\n余韻: ほどいたはずの糸が、朝になると袖口に一本だけ残っていた。引けば帰路、切れば異界。あなたはまだ選べる。";
                case "mikawa_process_end":
                    return "\n\n余韻: 工程表の最後の欄には、あなたの名前ではなく稼働音が記録された。人ではないが、異界を止める部品としては完璧だった。";
                case "chita_sea_takeoff":
                    return "\n\n余韻: 機内アナウンスは波音で、窓の外には滑走路の代わりに海底灯が並ぶ。着陸先だけが、まだ誰にも読めない。";
                case "airport_lost":
                    return "\n\n余韻: 手荷物タグだけがターンテーブルを回り続ける。名前の欄は空白で、行き先には『次の周回』と印字されていた。";
                case "miso_sink":
                    return "\n\n余韻: 樽の底で泡が一つ弾けるたび、あなたが言いそびれた言葉が発酵していく。いつか誰かの声で戻ってくる。";
                case "machine_part":
                    return "\n\n余韻: 朝礼のチャイムが鳴ると、あなたの部品番号だけが赤く点灯する。効率は上がった。帰り道は削除された。";
                case "madness":
                    return "\n\n余韻: 見てはいけないものを見たのではない。見られる側に回ったのだ。以後、選択肢の文字が時々あなたを読む。";
                case "impossible_true":
                    return "\n\n余韻: 観測記録の末尾に、あなたの筆跡ではない追記がある。『まだ勝利ではない。ただ、今回は存在し損ねた』。";
            }
            return "";
        }
        string RegionalEndingEcho(string endingId)
        {
            if (run == null)
                return "";

            string echo = "";
            if (run.owari >= run.mikawa && run.owari >= run.npcAirport && run.owari > 0)
                echo = "尾張方面の余韻: 金鯱の影、織物の糸、城下の水拍子が、帰還後もしばらく視界の端で同じ向きに揺れている。";
            else if (run.mikawa >= run.owari && run.mikawa >= run.npcAirport && run.mikawa > 0)
                echo = "三河方面の余韻: 味噌蔵の発酵音と工場ラインの拍が、心拍に混じって一拍だけ遅れて鳴り続ける。";
            else if (run.npcAirport > 0)
                echo = "空港・知多方面の余韻: 荷物タグ、潮の匂い、消えた便名が、次の搭乗案内のように耳の奥で点滅している。";
            else if (run.stats.localKnowledge > 0)
                echo = "名古屋方面の余韻: 地下街の柱番号と出口案内が、現実の道順に薄く重なって見える。";

            if (string.IsNullOrEmpty(echo))
                return "";

            if (endingId != "return" && endingId != "normal_clear" && endingId != "true_shachi" && endingId != "impossible_true")
                echo += " 死因はひとつでも、そこへ至る土地の気配は次の周回の警告として残った。";

            return "\n\n" + echo;
        }
        string HandleDeathConsequences(string endingId)
        {
            if (run == null || endingId == "return" || endingId == "normal_clear" || endingId == "true_shachi")
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
            double breakChance = HasGearEffect("memory_anchor") ? 0.18 : 0.35;
            if (rng.NextDouble() < breakChance)
            {
                string broken = "壊れた " + (rng.NextDouble() < 0.5 ? run.weapon.name : run.armor.name);
                progress.brokenGear.Add(broken);
                return "\n\n" + broken + " が倉庫に戻った。修理すれば、いつか使えるかもしれない。";
            }
            if (HasGearEffect("memory_anchor") && rng.NextDouble() < 0.35)
            {
                Gear anchored = !IsEmptyGear(run.accessory) ? run.accessory : (!IsEmptyGear(run.weapon) ? run.weapon : run.armor);
                if (!IsEmptyGear(anchored))
                {
                    StoreGear(anchored);
                    return "\n\n記憶定着の効果で " + anchored.name + " だけが倉庫に戻った。";
                }
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
            bool cleared = endingId == "return" || endingId == "normal_clear" || endingId == "true_shachi" || endingId == "final_observer_end";
            if (cleared && run.instability >= progress.maxInstabilityUnlocked && progress.maxInstabilityUnlocked < 5)
                progress.maxInstabilityUnlocked = run.instability + 1;
        }
        string BuildNewspaper(string endingId)
        {
            if (run == null)
                return "";
            string headline = endingId == "return" || endingId == "normal_clear" || endingId == "true_shachi"
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
        string InstabilityPortraitId(int level)
        {
            return "instability_" + Mathf.Clamp(level, 0, 5);
        }
        string InstabilityDetail(int level)
        {
            string summary = InstabilitySummary(level);
            switch (Mathf.Clamp(level, 0, 5))
            {
                case 1:
                    return "境界に髪の毛ほどの裂け目が入る。案内板の文字が一拍だけ別の地名に変わり、誰かの呼吸が背後で増える。\n\n" +
                        summary + "\n\n体感: 初回周回向け。怪異は増えるが、まだ人の判断で戻れる。\n確認後、キャラクター選択へ進みます。";
                case 2:
                    return "異界の水が駅の床下から漏れ、知らない改札音があなたの名前を区切る。安全な道にも、少しだけ神話の臭いが混じる。\n\n" +
                        summary + "\n\n体感: 探索報酬が伸び、失敗時の削れ方も重くなる。\n確認後、キャラクター選択へ進みます。";
                case 3:
                    return "夜が地上から剥がれ、愛知の地名が星の配置に置き換わる。見るほど理解でき、理解するほど正気が遠のく。\n\n" +
                        summary + "\n\n体感: 神話理解を伸ばしやすいが、SAN管理が重要になる。\n確認後、キャラクター選択へ進みます。";
                case 4:
                    return "二つの鯱が空と水底から同時に見ている。選ばなかった選択肢まで、あなたの影に結果を刻み始める。\n\n" +
                        summary + "\n\n体感: ボスとイベントの圧が高い。装備と進行値を意識して進む難度。\n確認後、キャラクター選択へ進みます。";
                case 5:
                    return "境界はもう門ではなく傷口になっている。人型の輪郭は保つが、顔だけが世界から許されないまま欠けている。\n\n" +
                        summary + "\n\n体感: 最難関。高報酬だが、HP/SAN/神話汚染の崩壊が速い。\n確認後、キャラクター選択へ進みます。";
                default:
                    return "境界はまだ厚い。怪異は遠くで身じろぎするだけで、駅灯りは人間のための明るさを保っている。\n\n" +
                        summary + "\n\n体感: 標準難度。物語と導線を確認しながら進める。\n確認後、キャラクター選択へ進みます。";
            }
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
            if (statsText != null)
                statsText.fontSize = 14;
            statsText.text =
                run.character.name + "\n" +
                "MP " + s.mp + "/" + s.maxMp + "\n" +
                "攻撃 " + (s.attack + run.weapon.attack + run.accessory.attack) + "\n" +
                "防御 " + (s.defense + run.armor.defense + run.accessory.defense) + "\n" +
                "速さ " + (s.speed + run.armor.speed + run.weapon.speed + run.accessory.speed) + "\n" +
                "LUK " + (s.luck + run.accessory.luck) + "\n" +
                "正気度 " + s.sanity + "/" + s.maxSanity + "\n" +
                "神話理解 " + s.mythosKnowledge + "\n" +
                "神話汚染 " + s.mythosCorruption + "\n" +
                "空腹 " + s.hunger + "\n" +
                "空港知識 " + run.npcAirport;
         inventoryText.text =
                "武器: " + GearSummary(run.weapon) + "\n\n" +
                "防具: " + GearSummary(run.armor) + "\n\n" +
                "装飾: " + GearSummary(run.accessory) + "\n\n" +
                "所持金 " + s.money + "\n" +
                "神話理解 " + s.mythosKnowledge + " / 汚染 " + s.mythosCorruption + "\n" +
                "空港 " + run.npcAirport + " / 地元 " + s.localKnowledge + " / 危険 " + run.dangerWarnings + "\n" +
                InstabilityName(run.instability) + " / 保険札 " + progress.insuranceTickets + "\n" +
                "死因 " + progress.deaths.Count + " / 怪異 " + progress.seenMonsters.Count + "\n\n" +
                SanityFlavor();
            inventoryText.text =
                "武器: " + GearSideSummary(run.weapon) + "\n" +
                "防具: " + GearSideSummary(run.armor) + "\n" +
                "装飾: " + GearSideSummary(run.accessory) + "\n\n" +
                "所持金 " + s.money + "\n" +
                InstabilityName(run.instability) + " / 保険札 " + progress.insuranceTickets + "\n" +
                (string.IsNullOrEmpty(EquippedEffectSummary()) ? "" : "特殊効果 " + EquippedEffectSummary() + "\n") +
                "死因 " + progress.deaths.Count + " / 怪異 " + progress.seenMonsters.Count + "\n\n" +
                "最近の出来事\n" + RecentLogText();
            if (statusModalRoot != null && statusModalRoot.gameObject.activeSelf)
                RefreshStatusModalText();
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
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
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
        ProgressSnapshot CaptureProgress()
        {
            if (run == null)
                return null;
            return new ProgressSnapshot
            {
                owari = run.owari,
                mikawa = run.mikawa,
                npcCafe = run.npcCafe,
                npcOccult = run.npcOccult,
                npcAirport = run.npcAirport,
                shachiGaze = run.shachiGaze,
                dangerWarnings = run.dangerWarnings,
                ogura = run.ogura
            };
        }
        string BuildStatDelta(Stats before, Stats after, ProgressSnapshot beforeProgress = null)
        {
            var changes = new List<string>();
            AddDelta(changes, "HP", after.hp - before.hp);
            AddDelta(changes, "SAN", after.sanity - before.sanity);
            AddDelta(changes, "所持金", after.money - before.money);
            AddDelta(changes, "攻撃", after.attack - before.attack);
            AddDelta(changes, "防御", after.defense - before.defense);
            AddDelta(changes, "速さ", after.speed - before.speed);
           AddDelta(changes, "LUK", after.luck - before.luck);
            AddDelta(changes, "\u5730\u5143", after.localKnowledge - before.localKnowledge);
            AddDelta(changes, "\u5473\u564c", after.misoResistance - before.misoResistance);
            AddDelta(changes, "\u6a5f\u68b0", after.machineAptitude - before.machineAptitude);
            AddDelta(changes, "神話", after.mythosKnowledge - before.mythosKnowledge);
            AddDelta(changes, "汚染", after.mythosCorruption - before.mythosCorruption);
            if (beforeProgress != null && run != null)
            {
                AddDelta(changes, "\u5c3e\u5f35", run.owari - beforeProgress.owari);
                AddDelta(changes, "\u4e09\u6cb3", run.mikawa - beforeProgress.mikawa);
                AddDelta(changes, "\u7a7a\u6e2f", run.npcAirport - beforeProgress.npcAirport);
                AddDelta(changes, "\u9bf1", run.shachiGaze - beforeProgress.shachiGaze);
                AddDelta(changes, "\u5371\u967a\u5bdf\u77e5", run.dangerWarnings - beforeProgress.dangerWarnings);
                AddDelta(changes, "\u55ab\u8336", run.npcCafe - beforeProgress.npcCafe);
                AddDelta(changes, "\u30aa\u30ab\u30eb\u30c8", run.npcOccult - beforeProgress.npcOccult);
                AddDelta(changes, "\u5c0f\u5009", run.ogura - beforeProgress.ogura);
            }
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
            AddCharacter("traveler", "旅行者", "標準型", "初期キャラ。危険察知と安定感があり、初見ルート向き。", 0, 22, 5, 4, 3, 5, 5, 22, 3, 1150, 3, 2, 2, 0, 0, "umbrella", "jacket", 1.00f);
            AddCharacter("worker", "出張会社員", "生存型", "初期キャラ。HP、防御、所持金、機械適性が高く、工場と空港で粘れる。", 0, 30, 5, 3, 7, 2, 3, 20, 4, 1800, 2, 2, 8, 2, 0, "pcbag", "suit", 0.98f);
            AddCharacter("local", "地元出身者", "探索型", "初期キャラ。地元知識、速さ、LUKに優れ、ルート事故を減らす。", 0, 21, 5, 4, 2, 7, 9, 20, 2, 1050, 9, 6, 2, 0, 0, "ticket", "local_clothes", 1.05f);
            AddCharacter("occult", "オカルト研究者", "神話型", "初期キャラ。脆いが神話理解と報酬倍率が高く、危険な選択で伸びる。", 0, 15, 9, 2, 1, 4, 4, 28, 3, 900, 3, 2, 1, 8, 2, "recorder", "coat", 1.22f);
            AddCharacter("samurai", "三河武士の末裔", "解放: 戦闘型", "低コスト解放。初期キャラより明確に戦えるが、防御準備を怠ると削られる。", 35, 44, 7, 14, 7, 6, 4, 26, 3, 1400, 3, 8, 2, 2, 0, "wood_sword", "charm", 1.28f);
            AddCharacter("mechanic", "工場の整備士", "解放: 防御型", "低コスト解放。高HP、高防御、機械適性で雑魚戦の事故を抑える。", 55, 50, 8, 10, 16, 4, 4, 28, 4, 1550, 3, 5, 14, 2, 0, "wrench", "safety", 1.34f);
            AddCharacter("shachi_seen", "金鯱に見られた者", "解放: 神話戦闘型", "中コスト手前。攻防と神話耐性が高く、水難・鯱系の戦闘を押し返す。", 90, 50, 10, 15, 14, 9, 7, 25, 3, 1500, 6, 6, 6, 9, 4, "shachi_dagger", "raincoat", 1.48f);
            AddCharacter("atsuta_miko", "熱田の巫覡", "解放: 封印型", "中コスト。高SANと封印能力でボスの攻撃を受け止めやすい。", 130, 56, 13, 12, 17, 8, 9, 40, 3, 1650, 9, 6, 5, 12, 2, "charm", "coat", 1.60f);
            AddCharacter("seto_potter", "瀬戸の窯守", "解放: 重装型", "中コスト。重い敵火力を受けて進むためのHPと防御を持つ。", 180, 68, 10, 12, 22, 5, 7, 36, 4, 1800, 6, 9, 6, 6, 1, "wrench", "raincoat", 1.72f);
            AddCharacter("toyohashi_conductor", "豊橋の終電車掌", "解放: 速度型", "中高コスト。速さと先制力でゲージを溜める余裕を作り、戦闘テンポを支配する。", 240, 58, 12, 15, 11, 19, 10, 34, 3, 1950, 9, 5, 9, 8, 2, "ticket", "local_clothes", 1.86f);
            AddCharacter("gamagori_diver", "蒲郡の潜水者", "解放: 深海型", "高コスト。高HPと神話理解で深海・知多・蒲郡ルートの高圧戦闘に耐える。", 320, 76, 14, 17, 15, 8, 8, 36, 4, 2200, 7, 7, 10, 15, 4, "recorder", "raincoat", 2.05f);
            AddCharacter("arimatsu_weaver", "有松の絞り師", "解放: 幸運型", "高コスト。LUKと地元知識が圧倒的で、判定事故と不利な遭遇を曲げる。", 420, 64, 13, 14, 12, 13, 24, 38, 3, 2350, 17, 7, 7, 10, 2, "ticket", "charm", 2.22f);
            AddCharacter("inuyama_mask", "犬山の面打ち", "解放: 変相型", "高コスト。高い総合力で敵の攻防をずらし、正面戦闘でも崩れにくい。", 540, 82, 13, 21, 18, 11, 15, 36, 4, 2500, 10, 10, 10, 12, 4, "wood_sword", "coat", 2.45f);
            AddCharacter("tsuruma_librarian", "鶴舞の禁書司書", "解放: 禁書型", "極大投資。高SANと禁書火力を持ち、神話選択とボス戦で別格に伸びる。", 700, 72, 20, 19, 13, 10, 13, 48, 3, 2300, 10, 7, 7, 24, 8, "recorder", "coat", 2.75f);
            AddCharacter("centrair_agent", "セントレア境界職員", "解放: 終盤支配型", "最大投資。全能力が高く、空港・撤退・ボス準備・周回収支で最上位。", 900, 96, 19, 25, 22, 16, 18, 50, 4, 3400, 13, 10, 18, 18, 5, "shachi_dagger", "raincoat", 3.10f);
            AddCharacter("final_observer", "境界外の観測者", "最終解放: 現実外討伐型", "この世のものとは思えないものを倒すためだけに記録された最強キャラクター。選択時は通常探索を飛ばし、専用ボスラッシュへ進む。", 3000, 180, 20, 52, 38, 26, 24, 90, 1, 5000, 18, 18, 18, 36, 10, "shachi_dagger", "raincoat", 3.50f);
            AddScenes();
            AddStageExpansion();
            AddFreedomExpansion();
            AddRegionalLoopScenes();
            AddMeiekiBaseScene();
            AddMeiekiStartScene();
            AddWideFreedomExpansion();
            AddAiWideExpansion();
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
        void AddFreedomExpansion()
        {
            scenes["nagoya_after_battle"] = new SceneDef
            {
                id = "nagoya_after_battle",
                title = "異界愛知 行き先選択",
                area = "名駅地下 / 自由行動",
                image = "station",
                portrait = "event_subway_child",
                text = "地下街の床に、愛知の地方が路線図のように浮かんでいる。\n\n今回は一直線に進まなくていい。休む、買う、相談する、危険を避ける、踏み込む。どの寄り道も、空港境界へ向かうための準備になる。",
                choices =
                {
                    new Choice { label = "尾張方面を選ぶ\n城/大須/熱田", next = "freedom_owari", effect = r => { r.owari += 1; r.stats.localKnowledge += 1; } },
                    new Choice { label = "西三河方面を選ぶ\n岡崎/豊田/有松", next = "freedom_mikawa", effect = r => { r.mikawa += 1; r.stats.misoResistance += 1; } },
                    new Choice { label = "知多・海方面を選ぶ\n半田/常滑/蒲郡", next = "freedom_chita", effect = r => { r.npcAirport += 1; } },
                    new Choice { label = "任意探索に出る\n地方イベント", next = "freedom_optional" },
                    new Choice { label = "拠点で整える\n休息/買い物/NPC", next = "freedom_base" },
                    new Choice { label = "STAGE踏破ルートへ", next = "stage1_hub" },
                    new Choice { label = "空港へ向かう", next = "airport_bridge", condition = r => r.npcAirport >= 2 || r.stats.localKnowledge >= 4 || r.dangerWarnings >= 2, disabledReason = "空港へ安全に向かう準備が足りない" }
                }
            };
            scenes["freedom_base"] = new SceneDef
            {
                id = "freedom_base",
                title = "地下街の仮拠点",
                area = "名駅地下 / 拠点",
                image = "station",
                portrait = "event_locker_keeper",
                text = "閉店したはずの喫茶店、ロッカー、古い観光案内端末が並ぶ。\n\nここでは戦わずに態勢を整えられる。ただし、時間を使うほど異界の注視も増えていく。",
                choices =
                {
                    new Choice { label = "短く休む\nHP+5/SAN+2", next = "nagoya_after_battle", effect = r => { r.restsUsed += 1; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 5); r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); if (r.restsUsed >= 2) r.shachiGaze += 1; } },
                    new Choice { label = "喫茶店員に相談\n地元+1/喫茶関係+1", next = "nagoya_after_battle", effect = r => { r.stats.localKnowledge += 1; r.npcCafe += 1; } },
                    new Choice { label = "研究者に相談\n神話+1/SAN-1", next = "nagoya_after_battle", effect = r => { r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); r.npcOccult += 1; } },
                    new Choice { label = "空港職員に相談\n空港知識+2/所持金-120", next = "nagoya_after_battle", condition = r => r.stats.money >= 120, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 120; r.npcAirport += 2; } },
                    new Choice { label = "護符を買う\n所持金-260/LUK+1", next = "nagoya_after_battle", condition = r => r.stats.money >= 260, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 260; r.stats.luck += 1; } },
                    new Choice { label = "戻る", next = "nagoya_after_battle" }
                }
            };
            scenes["freedom_owari"] = new SceneDef
            {
                id = "freedom_owari",
                title = "尾張の寄り道",
                area = "尾張 / 選択",
                image = "castle",
                portrait = "event_shachi_avatar",
                text = "城の堀、大須の電波、熱田の沈黙が同じ方角に重なっている。\n\n安全な道は浅く、危険な道は報酬が濃い。戦わずに解ける道もある。",
                choices =
                {
                    new Choice { label = "名古屋城へ\n鯱の注視", next = "castle_gate", effect = r => { r.owari += 1; } },
                    new Choice { label = "大須で情報収集\n神話+1/SAN-2", next = "osu", effect = r => { r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 2); } },
                    new Choice { label = "熱田で祓う\nSAN+3/汚染-1", next = "nagoya_after_battle", condition = r => r.stats.mythosCorruption > 0 || r.stats.sanity < r.stats.maxSanity, disabledReason = "祓うほどの乱れがまだない", effect = r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 3); r.stats.mythosCorruption = Math.Max(0, r.stats.mythosCorruption - 1); r.flags.Add("owari_purified"); } },
                    new Choice { label = "城の井戸を神話的に封じる\n神話4以上", next = "castle_scale", condition = r => r.stats.mythosKnowledge >= 4, disabledReason = "神話理解が足りない", effect = r => { r.stats.defense += 1; r.shachiGaze += 1; r.flags.Add("sealed_castle_well"); } },
                    new Choice { label = "危険を避けて戻る\nHP+2", next = "nagoya_after_battle", effect = r => { r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 2); } }
                }
            };
            scenes["freedom_mikawa"] = new SceneDef
            {
                id = "freedom_mikawa",
                title = "三河の寄り道",
                area = "西三河 / 選択",
                image = "miso",
                portrait = "event_miso_voice",
                text = "岡崎の蔵、豊田のライン、有松の糸が、別々の答えを出している。\n\n力で突破する以外に、耐性、機械適性、交渉で道を開ける。",
                choices =
                {
                    new Choice { label = "岡崎の蔵へ\n味噌耐性+1", next = "okazaki_storehouse", effect = r => { r.mikawa += 1; r.stats.misoResistance += 1; } },
                    new Choice { label = "豊田で検査を学ぶ\n機械+2/HP-1", next = "toyota_line", effect = r => { r.stats.machineAptitude += 2; r.stats.hp = Math.Max(1, r.stats.hp - 1); } },
                    new Choice { label = "有松で隠れる布を買う\n所持金-260/危険察知+1", next = "arimatsu_dye", condition = r => r.stats.money >= 260, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 260; r.dangerWarnings += 1; } },
                    new Choice { label = "味噌声塊を弱める\n味噌4以上", next = "nagoya_after_battle", condition = r => r.stats.misoResistance >= 4, disabledReason = "味噌耐性が足りない", effect = r => { r.flags.Add("weaken_miso_voice"); r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); } },
                    new Choice { label = "戦わず交渉する\n地元+味噌7以上", next = "airport_bridge", condition = r => r.stats.localKnowledge + r.stats.misoResistance >= 7, disabledReason = "交渉材料が足りない", effect = r => { r.npcAirport += 2; } },
                    new Choice { label = "戻る", next = "nagoya_after_battle" }
                }
            };
            scenes["freedom_chita"] = new SceneDef
            {
                id = "freedom_chita",
                title = "知多と海の寄り道",
                area = "知多半島 / 海路",
                image = "gamagori",
                portrait = "event_gamagori_diver",
                text = "黒酢の運河、常滑の坂、蒲郡の星図が、空港へ向かう海の下で繋がっている。\n\n海路は近い。近いぶん、見てはいけないものも多い。",
                choices =
                {
                    new Choice { label = "半田で休む\nHP+4/味噌+1", next = "handa_rest", effect = r => { r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 4); r.stats.misoResistance += 1; } },
                    new Choice { label = "常滑から空港を見る\n地元+1", next = "tokoname", effect = r => { r.stats.localKnowledge += 1; } },
                    new Choice { label = "蒲郡で星図を読む\n神話+1/SAN-1", next = "gamagori_tide_map", effect = r => { r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } },
                    new Choice { label = "海路で危険を避ける\n空港知識+1/LUK判定", next = "airport_bridge", effect = r => { int target = 14; int roll = RollLuckDiceAgainst(target); if (roll >= target) { r.npcAirport += 2; r.stats.luck += 1; } else { r.npcAirport += 1; r.stats.hp = Math.Max(1, r.stats.hp - 2); } } },
                    new Choice { label = "深い改札を避けずに進む", battle = "deep_one_clerk", effect = r => { r.battleReturnScene = "airport_bridge"; } },
                    new Choice { label = "戻る", next = "nagoya_after_battle" }
                }
            };
            scenes["freedom_optional"] = new SceneDef
            {
                id = "freedom_optional",
                title = "任意探索",
                area = "異界愛知 / 寄り道",
                image = "osu",
                portrait = "event_occult_researcher",
                text = "地図に載らない寄り道がいくつか口を開けている。\n\n踏み込めば能力やフラグが伸びる。避ければ消耗を抑えられる。",
                choices =
                {
                    new Choice { label = "鶴舞地下書庫\n神話+2/SAN-2", next = "tsuruma_archive", effect = r => { r.stats.mythosKnowledge += 2; r.stats.sanity = Math.Max(0, r.stats.sanity - 2); } },
                    new Choice { label = "犬山の面市\nLUK+1/SAN-1", next = "inuyama_mask_market", effect = r => { r.stats.luck += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } },
                    new Choice { label = "瀬戸の窯跡\n防御+1/機械+1", next = "seto_kiln_path", effect = r => { r.stats.defense += 1; r.stats.machineAptitude += 1; } },
                    new Choice { label = "知立の人形舞台\n戦闘以外で解く", next = "chiryu_puppet_stage", condition = r => r.stats.luck >= 5 || r.stats.mythosKnowledge >= 3, disabledReason = "LUKか神話理解が足りない", effect = r => { r.dangerWarnings += 1; } },
                    new Choice { label = "危険を避けて観察\n危険察知+1", next = "nagoya_after_battle", effect = r => { r.dangerWarnings += 1; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); } },
                    new Choice { label = "戻る", next = "nagoya_after_battle" }
                }
            };
        }
        void AddRegionalLoopScenes()
        {
            scenes["nagoya_after_battle"] = new SceneDef
            {
                id = "nagoya_after_battle",
                title = "異界愛知 行き先選択",
                area = "名駅地下 / 自由行動",
                image = "station",
                portrait = "event_subway_child",
                text = "地下街の床に、愛知の地方が路線図のように浮かんでいる。\n\n地方を選ぶと、その地方の行動と名のない気配に集中する。戻るを選ぶまでは、この行き先選択には戻らない。",
                choices =
                {
                    new Choice { label = "尾張方面へ\n城/大須/熱田", next = "freedom_owari", effect = r => { r.freedomRegion = "owari"; r.owari += 1; r.stats.localKnowledge += 1; } },
                    new Choice { label = "西三河方面へ\n岡崎/豊田/有松", next = "freedom_mikawa", effect = r => { r.freedomRegion = "mikawa"; r.mikawa += 1; r.stats.misoResistance += 1; } },
                    new Choice { label = "知多・海方面へ\n半田/常滑/蒲郡", next = "freedom_chita", effect = r => { r.freedomRegion = "chita"; r.npcAirport += 1; } },
                    new Choice { label = "任意探索へ\n地方外の寄り道", next = "freedom_optional", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "拠点で整える\n休息/買い物/NPC", next = "freedom_base", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "STAGE踏破ルートへ", next = "stage1_hub", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "空港へ向かう", next = "airport_bridge", condition = r => r.npcAirport >= 2 || r.stats.localKnowledge >= 4 || r.dangerWarnings >= 2, disabledReason = "空港へ安全に向かう準備が足りない", effect = r => { r.freedomRegion = ""; } }
                }
            };
            scenes["freedom_owari"] = new SceneDef
            {
                id = "freedom_owari",
                title = "尾張方面",
                area = "尾張 / 滞在中",
                image = "castle",
                portrait = "event_shachi_avatar",
                text = "城の堀、大須の電波、熱田の沈黙が同じ方角に重なっている。\n\nここにいる間は尾張系の出来事だけが起こる。別地方へ移るには戻るを選ぶ。",
                choices =
                {
                    new Choice { label = "城下を調べる\n地元+1/鯱注視+1", next = "freedom_owari", effect = r => { r.freedomRegion = "owari"; r.stats.localKnowledge += 1; r.shachiGaze += 1; } },
                    new Choice { label = "大須で情報収集\n神話+1/SAN-2", next = "freedom_owari", effect = r => { r.freedomRegion = "owari"; r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 2); r.npcOccult += 1; } },
                    new Choice { label = "熱田で祓う\nSAN+3/汚染-1", next = "freedom_owari", condition = r => r.stats.mythosCorruption > 0 || r.stats.sanity < r.stats.maxSanity, disabledReason = "祓うほどの乱れがまだない", effect = r => { r.freedomRegion = "owari"; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 3); r.stats.mythosCorruption = Math.Max(0, r.stats.mythosCorruption - 1); } },
                    new Choice { label = "井戸を封じ直す\n神話4以上/防御+1", next = "freedom_owari", condition = r => r.stats.mythosKnowledge >= 4, disabledReason = "神話理解が足りない", effect = r => { r.freedomRegion = "owari"; r.stats.defense += 1; r.flags.Add("sealed_castle_well"); } },
                    new Choice { label = "尾張の出来事を探す\n名のない気配", next = "freedom_owari" },
                    new Choice { label = "尾張の怪異に踏み込む", battle = "well_tentacle", effect = r => { r.freedomRegion = "owari"; r.battleReturnScene = "freedom_owari"; } },
                    new Choice { label = "戻る", next = "nagoya_after_battle", effect = r => { r.freedomRegion = ""; } }
                }
            };
            scenes["freedom_mikawa"] = new SceneDef
            {
                id = "freedom_mikawa",
                title = "西三河方面",
                area = "西三河 / 滞在中",
                image = "miso",
                portrait = "event_miso_voice",
                text = "岡崎の蔵、豊田のライン、有松の糸が、別々の答えを出している。\n\nここにいる間は西三河系の出来事だけが起こる。別地方へ移るには戻るを選ぶ。",
                choices =
                {
                    new Choice { label = "岡崎の蔵を読む\n味噌+1/三河+1", next = "freedom_mikawa", effect = r => { r.freedomRegion = "mikawa"; r.mikawa += 1; r.stats.misoResistance += 1; } },
                    new Choice { label = "豊田で検査を学ぶ\n機械+2/HP-1", next = "freedom_mikawa", effect = r => { r.freedomRegion = "mikawa"; r.stats.machineAptitude += 2; r.stats.hp = Math.Max(1, r.stats.hp - 1); } },
                    new Choice { label = "有松の布を買う\n所持金-260/危険察知+1", next = "freedom_mikawa", condition = r => r.stats.money >= 260, disabledReason = "所持金が足りない", effect = r => { r.freedomRegion = "mikawa"; r.stats.money -= 260; r.dangerWarnings += 1; } },
                    new Choice { label = "味噌声塊を弱める\n味噌4以上", next = "freedom_mikawa", condition = r => r.stats.misoResistance >= 4, disabledReason = "味噌耐性が足りない", effect = r => { r.freedomRegion = "mikawa"; r.flags.Add("weaken_miso_voice"); r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); } },
                    new Choice { label = "三河の出来事を探す\n名のない気配", next = "freedom_mikawa" },
                    new Choice { label = "三河の怪異に踏み込む", battle = "shadow_retainer", effect = r => { r.freedomRegion = "mikawa"; r.battleReturnScene = "freedom_mikawa"; } },
                    new Choice { label = "戻る", next = "nagoya_after_battle", effect = r => { r.freedomRegion = ""; } }
                }
            };
            scenes["freedom_chita"] = new SceneDef
            {
                id = "freedom_chita",
                title = "知多・海方面",
                area = "知多半島 / 滞在中",
                image = "gamagori",
                portrait = "event_gamagori_diver",
                text = "黒酢の運河、常滑の坂、蒲郡の星図が、空港へ向かう海の下で繋がっている。\n\nここにいる間は知多・海系の出来事だけが起こる。別地方へ移るには戻るを選ぶ。",
                choices =
                {
                    new Choice { label = "半田で休む\nHP+4/味噌+1", next = "freedom_chita", effect = r => { r.freedomRegion = "chita"; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 4); r.stats.misoResistance += 1; } },
                    new Choice { label = "常滑から空港を見る\n地元+1/空港知識+1", next = "freedom_chita", effect = r => { r.freedomRegion = "chita"; r.stats.localKnowledge += 1; r.npcAirport += 1; } },
                    new Choice { label = "蒲郡で星図を読む\n神話+1/SAN-1", next = "freedom_chita", effect = r => { r.freedomRegion = "chita"; r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } },
                    new Choice { label = "海路で空港へ近づく\nLUK判定", next = "freedom_chita", effect = r => { r.freedomRegion = "chita"; int target = 14; int roll = RollLuckDiceAgainst(target); if (roll >= target) { r.npcAirport += 2; r.stats.luck += 1; } else { r.npcAirport += 1; r.stats.hp = Math.Max(1, r.stats.hp - 2); } } },
                    new Choice { label = "知多・海の出来事を探す\n名のない気配", next = "freedom_chita" },
                    new Choice { label = "海の怪異に踏み込む", battle = "deep_one_clerk", effect = r => { r.freedomRegion = "chita"; r.battleReturnScene = "freedom_chita"; } },
                    new Choice { label = "戻る", next = "nagoya_after_battle", effect = r => { r.freedomRegion = ""; } }
                }
            };
        }
        void AddMeiekiBaseScene()
        {
            scenes["nagoya_after_battle"] = new SceneDef
            {
                id = "nagoya_after_battle",
                title = "名駅の底",
                area = "拠点 / 地方移動",
                image = "station",
                portrait = "event_subway_child",
                text = "名駅の底に仮拠点を作った。\n\nここから愛知各地へ移動できる。地方方面を選ぶと、その地方の行動と名のない気配に集中する。戻るを選ぶまでは、この拠点へは戻らない。",
                choices =
                {
                    new Choice { label = "尾張方面へ\n城/大須/熱田", next = "freedom_owari", effect = r => { r.freedomRegion = "owari"; r.owari += 1; r.stats.localKnowledge += 1; } },
                    new Choice { label = "西三河方面へ\n岡崎/豊田/有松", next = "freedom_mikawa", effect = r => { r.freedomRegion = "mikawa"; r.mikawa += 1; r.stats.misoResistance += 1; } },
                    new Choice { label = "知多・海方面へ\n半田/常滑/蒲郡", next = "freedom_chita", effect = r => { r.freedomRegion = "chita"; r.npcAirport += 1; } },
                    new Choice { label = "名古屋城へ", next = "castle_gate", effect = r => { r.freedomRegion = "owari"; r.owari += 1; } },
                    new Choice { label = "大須へ", next = "osu", effect = r => { r.freedomRegion = "owari"; r.npcOccult += 1; } },
                    new Choice { label = "岡崎へ", next = "okazaki_storehouse", effect = r => { r.freedomRegion = "mikawa"; r.mikawa += 1; } },
                    new Choice { label = "豊田へ", next = "toyota_line", effect = r => { r.freedomRegion = "mikawa"; r.stats.machineAptitude += 1; } },
                    new Choice { label = "半田へ", next = "handa_canal", effect = r => { r.freedomRegion = "chita"; r.stats.misoResistance += 1; } },
                    new Choice { label = "常滑へ", next = "tokoname", effect = r => { r.freedomRegion = "chita"; r.npcAirport += 1; } },
                    new Choice { label = "蒲郡へ", next = "gamagori_tide_map", effect = r => { r.freedomRegion = "chita"; } },
                    new Choice { label = "任意探索へ\n地方外の寄り道", next = "freedom_optional", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "拠点で整える\n休息/買い物/NPC", next = "freedom_base", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "STAGE踏破ルートへ", next = "stage1_hub", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "空港へ向かう", next = "airport_bridge", condition = r => r.npcAirport >= 2 || r.stats.localKnowledge >= 4 || r.dangerWarnings >= 2, disabledReason = "空港へ安全に向かう準備が足りない", effect = r => { r.freedomRegion = ""; } }
                }
            };
        }
        void AddMeiekiStartScene()
        {
            scenes["nagoya_start"] = new SceneDef
            {
                id = "nagoya_start",
                title = "名駅の底",
                area = "名駅地下 / 初期拠点",
                image = "station",
                portrait = "event_subway_child",
                text = "あなたは名駅の底で目を覚ました。\n\n白すぎる照明の下に、仮拠点にできそうな空間がある。ここから愛知各地へ移動できる。地方方面を選ぶと、戻るまでその地方の出来事に集中する。\n\n床の案内図のさらに下で、読めない文字が呼吸している。",
                choices =
                {
                    new Choice { label = "案内板を見る\nSAN-1", next = "station_sign", effect = r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } },
                    new Choice { label = "きしめん屋へ\n300円", next = "kishimen_oracle", condition = r => r.stats.money >= 300, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 300; r.stats.hunger = Math.Max(0, r.stats.hunger - 1); } },
                    new Choice { label = "尾張方面へ\n城/大須/熱田", next = "freedom_owari", effect = r => { r.freedomRegion = "owari"; r.owari += 1; r.stats.localKnowledge += 1; } },
                    new Choice { label = "西三河方面へ\n岡崎/豊田/有松", next = "freedom_mikawa", effect = r => { r.freedomRegion = "mikawa"; r.mikawa += 1; r.stats.misoResistance += 1; } },
                    new Choice { label = "知多・海方面へ\n半田/常滑/蒲郡", next = "freedom_chita", effect = r => { r.freedomRegion = "chita"; r.npcAirport += 1; } },
                    new Choice { label = "任意探索へ\n地方外の寄り道", next = "freedom_optional", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "拠点で整える\n休息/買い物/NPC", next = "freedom_base", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "STAGE踏破ルートへ", next = "stage1_hub", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "神話の底を覗く\n神話5以上", next = "meieki_mythos_event", condition = r => r.stats.mythosKnowledge >= 5, disabledReason = "神話知識が足りない", effect = r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); r.stats.mythosCorruption += 1; } },
                    new Choice { label = "黄色い群れの音に備える", battle = "piyorin", effect = r => { r.battleReturnScene = "nagoya_after_battle"; } }
                }
            };
            scenes["meieki_mythos_event"] = new SceneDef
            {
                id = "meieki_mythos_event",
                title = "名駅の底の神話",
                area = "名駅地下 / 認識の最下層",
                image = "station",
                portrait = "event_subway_child",
                text = "あなたは案内図の下層を読む。\n\n路線ではなく、都市そのものの神経が描かれていた。尾張も三河も知多も、すべて名駅の底へ沈むための長い迂回路だった。\n\n理解した瞬間、帰り道は目的地に変わる。",
                choices =
                {
                    new Choice { label = "ENDへ進む", ending = "meieki_mythos" }
                }
            };
        }
        void AddWideFreedomExpansion()
        {
            scenes["nagoya_start"] = new SceneDef
            {
                id = "nagoya_start",
                title = "名駅の底",
                area = "名駅地下 / 初期拠点",
                image = "station",
                portrait = "event_subway_child",
                text = "あなたは名駅の底で目を覚ました。\n\n白すぎる照明の下に、仮拠点にできそうな空間がある。ここから愛知各地へ移動できる。地方方面を選ぶと、戻るまでその地方の出来事に集中する。\n\n床の案内図のさらに下で、読めない文字が呼吸している。",
                choices =
                {
                    new Choice { label = "拠点で整える\n休息/買い物/NPC", next = "freedom_base", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "ゴールを確認する\n脱出条件", next = "meieki_goal", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "作戦会議をする\n安全な方針選択", next = "meieki_strategy", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "尾張方面へ\n城/大須/熱田", next = "freedom_owari", effect = r => { r.freedomRegion = "owari"; r.owari += 1; r.stats.localKnowledge += 1; } },
                    new Choice { label = "西三河方面へ\n岡崎/豊田/有松", next = "freedom_mikawa", effect = r => { r.freedomRegion = "mikawa"; r.mikawa += 1; r.stats.misoResistance += 1; } },
                    new Choice { label = "知多・海方面へ\n半田/常滑/蒲郡", next = "freedom_chita", effect = r => { r.freedomRegion = "chita"; r.npcAirport += 1; } },
                    new Choice { label = "任意探索へ\n地方外の寄り道", next = "freedom_optional", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "案内板を見る\nSAN-1", next = "station_sign", effect = r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } },
                    new Choice { label = "きしめん屋へ\n300円", next = "kishimen_oracle", condition = r => r.stats.money >= 300, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 300; r.stats.hunger = Math.Max(0, r.stats.hunger - 1); } },
                    new Choice { label = "STAGE踏破ルートへ", next = "stage1_hub", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "空港へ向かう", next = "airport_bridge", condition = r => r.npcAirport >= 2 || r.stats.localKnowledge >= 4 || r.dangerWarnings >= 2, disabledReason = "空港へ安全に向かう準備が足りない", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "神話の底を覗く\n神話5以上", next = "meieki_mythos_event", condition = r => r.stats.mythosKnowledge >= 5, disabledReason = "神話知識が足りない", effect = r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); r.stats.mythosCorruption += 1; } },
                    new Choice { label = "黄色い群れの音に備える", battle = "piyorin", effect = r => { r.battleReturnScene = "nagoya_after_battle"; } }
                }
            };
            scenes["nagoya_after_battle"] = new SceneDef
            {
                id = "nagoya_after_battle",
                title = "名駅の底",
                area = "拠点 / 地方移動",
                image = "station",
                portrait = "event_subway_child",
                text = "名駅の底に仮拠点を作った。\n\nここから愛知各地へ移動できる。地方方面を選ぶと、その地方の行動と名のない気配に集中する。戻るを選ぶまでは、この拠点へは戻らない。",
                choices =
                {
                    new Choice { label = "本線へ進む\nSTAGE踏破", next = "meieki_stage_routes", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "地方へ向かう\n尾張/三河/知多", next = "meieki_regional_routes", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "拠点で整える\n休息/買い物/NPC", next = "freedom_base", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "記憶と方針\n市場/作戦/特殊道", next = "meieki_route_support", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "空港へ向かう", next = "airport_bridge", condition = r => r.npcAirport >= 2 || r.stats.localKnowledge >= 4 || r.dangerWarnings >= 2, disabledReason = "空港へ安全に向かう準備が足りない", effect = r => { r.freedomRegion = ""; } }
                }
            };
            scenes["meieki_stage_routes"] = new SceneDef
            {
                id = "meieki_stage_routes",
                title = "本線の路線図",
                area = "名駅の底 / STAGE",
                image = "station",
                portrait = "event_route_cartographer",
                text = "床の路線図は五つの区間に分かれている。\n\n未踏の区間は遠く、撃破済みのボスがいる区間だけが短絡路として薄く光る。",
                choices =
                {
                    new Choice { label = "STAGE 1\n名駅地下回廊へ", next = "stage1_hub" },
                    new Choice { label = "STAGE 2へ短絡\n路線図母体撃破済み", next = "stage2_hub", condition = r => progress.bossesDefeated.Contains("stage_boss_1"), disabledReason = "名駅地下のボス記録がない" },
                    new Choice { label = "STAGE 3へ短絡\n金鯱影王撃破済み", next = "stage3_hub", condition = r => progress.bossesDefeated.Contains("stage_boss_2"), disabledReason = "尾張のボス記録がない" },
                    new Choice { label = "STAGE 4へ短絡\n声塊撃破済み", next = "stage4_hub", condition = r => progress.bossesDefeated.Contains("stage_boss_3"), disabledReason = "三河のボス記録がない" },
                    new Choice { label = "STAGE 5へ短絡\n監査官撃破済み", next = "stage5_hub", condition = r => progress.bossesDefeated.Contains("stage_boss_4"), disabledReason = "知多のボス記録がない" },
                    new Choice { label = "戻る", next = "nagoya_after_battle" }
                }
            };
            scenes["meieki_regional_routes"] = new SceneDef
            {
                id = "meieki_regional_routes",
                title = "地方への分岐",
                area = "名駅の底 / 地方",
                image = "station",
                portrait = "event_subway_child",
                text = "地方へ出ると、戻るまではその土地の気配に寄せられる。\n\n尾張は地元と鯱、三河は味噌と機械、知多は海と空港に近い。",
                choices =
                {
                    new Choice { label = "尾張方面へ\n城/大須/熱田", next = "freedom_owari", effect = r => { r.freedomRegion = "owari"; r.owari += 1; r.stats.localKnowledge += 1; } },
                    new Choice { label = "西三河方面へ\n岡崎/豊田/有松", next = "freedom_mikawa", effect = r => { r.freedomRegion = "mikawa"; r.mikawa += 1; r.stats.misoResistance += 1; } },
                    new Choice { label = "知多・海方面へ\n半田/常滑/蒲郡", next = "freedom_chita", effect = r => { r.freedomRegion = "chita"; r.npcAirport += 1; } },
                    new Choice { label = "任意探索へ\n地方外の寄り道", next = "freedom_optional", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "旧ルートへ寄り道\n名古屋城", next = "castle_gate", effect = r => { r.freedomRegion = "owari"; r.owari += 1; } },
                    new Choice { label = "戻る", next = "nagoya_after_battle", effect = r => { r.freedomRegion = ""; } }
                }
            };
            scenes["freedom_base"] = new SceneDef
            {
                id = "freedom_base",
                title = "名駅の底 拠点",
                area = "拠点 / 準備フェーズ",
                image = "station",
                portrait = "event_locker_keeper",
                text = "閉店したはずの喫茶店、ロッカー、観光案内端末、折り畳み机の作戦地図が並ぶ。\n\nここでは戦わずに態勢を整えられる。ただし同じ準備は周回内で何度も効かない。長く留まるほど異界にも居場所を覚えられる。",
                choices =
                {
                    new Choice { label = "短く休む\nHP+5/SAN+2", next = "freedom_base", effect = r => { r.restsUsed += 1; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 5); r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); if (r.restsUsed >= 2) r.shachiGaze += 1; } },
                    new Choice { label = "しっかり休む\nHP+9/SAN+4/注視+1", next = "freedom_base", effect = r => { r.restsUsed += 1; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 9); r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 4); r.shachiGaze += 1; } },
                    new Choice { label = "喫茶店員に相談\n地元+1/喫茶+1", next = "freedom_base", effect = r => { r.stats.localKnowledge += 1; r.npcCafe += 1; } },
                    new Choice { label = "研究者に相談\n神話+1/SAN-1", next = "freedom_base", effect = r => { r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); r.npcOccult += 1; } },
                    new Choice { label = "空港職員に相談\n空港+2/所持金-120", next = "freedom_base", condition = r => r.stats.money >= 120, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 120; r.npcAirport += 2; } },
                    new Choice { label = "地元の抜け道を聞く\n地元6以上/危険察知+2", next = "freedom_base", condition = r => r.stats.localKnowledge >= 6, disabledReason = "地元知識が足りない", effect = r => { r.dangerWarnings += 2; r.npcAirport += 1; r.flags.Add("local_shortcut"); } },
                    new Choice { label = "鯱の視線を読む\n鯱4以上/神話+1", next = "shachi_pressure_event", condition = r => r.shachiGaze >= 4, disabledReason = "鯱の注視が足りない", effect = r => { r.shachiGaze += 1; } },
                    new Choice { label = "護符を買う\n所持金-260/LUK+1", next = "freedom_base", condition = r => r.stats.money >= 260, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 260; r.stats.luck += 1; } },
                    new Choice { label = "救急セットを買う\n所持金-320/HP+6", next = "freedom_base", condition = r => r.stats.money >= 320, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 320; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 6); } },
                    new Choice { label = "異界メモを整理\n危険察知+1", next = "freedom_base", effect = r => { r.dangerWarnings += 1; r.stats.localKnowledge += r.stats.mythosKnowledge >= 3 ? 1 : 0; } },
                    new Choice { label = "神話を封じて眠る\n神話3以上/SAN+5", next = "freedom_base", condition = r => r.stats.mythosKnowledge >= 3, disabledReason = "神話理解が足りない", effect = r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 5); r.stats.mythosCorruption = Math.Max(0, r.stats.mythosCorruption - 1); r.stats.mythosKnowledge = Math.Max(0, r.stats.mythosKnowledge - 1); } },
                    new Choice { label = "戻る", next = "nagoya_after_battle", effect = r => { r.freedomRegion = ""; } }
                }
            };
            scenes["meieki_goal"] = new SceneDef
            {
                id = "meieki_goal",
                title = "脱出目標",
                area = "名駅の底 / ゴール確認",
                image = "station",
                portrait = "event_gate_inspector",
                text = "この周回の大きな目的は、名駅の底から愛知各地を経由して空港境界へ向かい、帰還または別種のENDへ到達すること。\n\n空港へ向かうには、空港知識、地元知識、危険察知のどれかを十分に集める。STAGE踏破でも空港ゲートへ近づける。\n\n同じ行動は周回内で枯れる。失敗で得た記憶片や図鑑情報を次の周回へ持ち越し、危険な近道を学ぶのが基本になる。\n\n地元知識は安全な近道と回避に効く。鯱注視は尾張と空港の道標になるが、溜まりすぎると強い怪異にも見つかる。",
                choices =
                {
                    new Choice { label = "空港へ向かう条件を見る\n空港2/地元4/危険2", next = "meieki_goal", effect = r => { r.dangerWarnings += 1; } },
                    new Choice { label = "地元知識の使い道を見る\n地元+1", next = "meieki_goal", effect = r => { r.stats.localKnowledge += 1; } },
                    new Choice { label = "鯱注視の意味を見る\n鯱+1", next = "meieki_goal", effect = r => { r.shachiGaze += 1; } },
                    new Choice { label = "空港へ向かう", next = "airport_bridge", condition = r => r.npcAirport >= 2 || r.stats.localKnowledge >= 4 || r.dangerWarnings >= 2, disabledReason = "空港へ安全に向かう準備が足りない", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "戻る", next = "nagoya_after_battle" }
                }
            };
            scenes["shachi_pressure_event"] = new SceneDef
            {
                id = "shachi_pressure_event",
                title = "鯱の注視",
                area = "名駅の底 / 閾値イベント",
                image = "castle",
                portrait = "event_shachi_avatar",
                text = "天井の配管が、金の鱗のように一斉に鳴った。\n\n鯱の注視はただの危険値ではない。見られているからこそ、見えない道も浮かび上がる。だが、さらに注視を集めれば、向こうもあなたを選び始める。",
                choices =
                {
                    new Choice { label = "道標として受け入れる\n空港+2/危険察知+1", next = "freedom_base", effect = r => { r.npcAirport += 2; r.dangerWarnings += 1; r.flags.Add("shachi_guidance"); } },
                    new Choice { label = "注視を祓う\nSAN+2/鯱-2", next = "freedom_base", effect = r => { r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); r.shachiGaze = Math.Max(0, r.shachiGaze - 2); } },
                    new Choice { label = "双鯱へ踏み込む\n神話2以上", ending = "true_shachi", condition = r => r.stats.mythosKnowledge >= 2, disabledReason = "神話理解が足りない" }
                }
            };
            scenes["meieki_strategy"] = new SceneDef
            {
                id = "meieki_strategy",
                title = "名駅の底 作戦会議",
                area = "拠点 / 方針",
                image = "station",
                portrait = "event_occult_researcher",
                text = "折り畳み机に、愛知の地図と地下街の避難経路図を重ねる。\n\nどの危険を避け、どの危険に踏み込むかを決められる。",
                choices =
                {
                    new Choice { label = "安全重視で進む\n危険察知+2/速度-1", next = "nagoya_after_battle", effect = r => { r.dangerWarnings += 2; r.stats.speed = Math.Max(0, r.stats.speed - 1); } },
                    new Choice { label = "神話調査を優先\n神話+2/SAN-3", next = "nagoya_after_battle", effect = r => { r.stats.mythosKnowledge += 2; r.stats.sanity = Math.Max(0, r.stats.sanity - 3); r.npcOccult += 1; } },
                    new Choice { label = "空港突破を優先\n空港+2/所持金-180", next = "nagoya_after_battle", condition = r => r.stats.money >= 180, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 180; r.npcAirport += 2; } },
                    new Choice { label = "地方を広く聞き込む\n地元+2/所持金-150", next = "nagoya_after_battle", condition = r => r.stats.money >= 150, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 150; r.stats.localKnowledge += 2; } },
                    new Choice { label = "何も決めずに戻る", next = "nagoya_after_battle" }
                }
            };
            scenes["freedom_owari"] = new SceneDef
            {
                id = "freedom_owari",
                title = "尾張方面",
                area = "尾張 / 滞在中",
                image = "castle",
                portrait = "event_shachi_avatar",
                text = "城の堀、大須の電波、熱田の沈黙が同じ方角に重なっている。\n\nここにいる間は尾張系の出来事だけが起こる。別地方へ移るには戻るを選ぶ。",
                choices =
                {
                    new Choice { label = "城下を調べる\n地元+1/鯱注視+1", next = "freedom_owari", effect = r => { r.freedomRegion = "owari"; r.stats.localKnowledge += 1; r.shachiGaze += 1; } },
                    new Choice { label = "大須で情報収集\n神話+1/SAN-2", next = "freedom_owari", effect = r => { r.freedomRegion = "owari"; r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 2); r.npcOccult += 1; } },
                    new Choice { label = "熱田で祓う\nSAN+3/汚染-1", next = "freedom_owari", condition = r => r.stats.mythosCorruption > 0 || r.stats.sanity < r.stats.maxSanity, disabledReason = "祓うほどの乱れがまだない", effect = r => { r.freedomRegion = "owari"; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 3); r.stats.mythosCorruption = Math.Max(0, r.stats.mythosCorruption - 1); } },
                    new Choice { label = "繊維の抜け道を使う\nLUK判定", next = "freedom_owari", effect = r => { r.freedomRegion = "owari"; int target = 13; int roll = RollLuckDiceAgainst(target); if (roll >= target) { r.stats.luck += 1; r.dangerWarnings += 1; r.owari += 1; r.stats.localKnowledge += 1; } else { r.stats.hp = Math.Max(1, r.stats.hp - 2); r.dangerWarnings += 1; } } },
                    new Choice { label = "井戸を封じ直す\n神話4以上/防御+1", next = "freedom_owari", condition = r => r.stats.mythosKnowledge >= 4, disabledReason = "神話理解が足りない", effect = r => { r.freedomRegion = "owari"; r.stats.defense += 1; r.flags.Add("sealed_castle_well"); } },
                    new Choice { label = "地元の近道で城を抜ける\n地元6以上", next = "freedom_owari", condition = r => r.stats.localKnowledge >= 6, disabledReason = "地元知識が足りない", effect = r => { r.freedomRegion = "owari"; r.dangerWarnings += 2; r.owari += 1; } },
                    new Choice { label = "鯱の導きに従う\n鯱4以上", next = "shachi_pressure_event", condition = r => r.shachiGaze >= 4, disabledReason = "鯱の注視が足りない", effect = r => { r.freedomRegion = "owari"; } },
                    new Choice { label = "鯱と交渉する\n地元+神話7以上", next = "freedom_owari_deal", condition = r => r.stats.localKnowledge + r.stats.mythosKnowledge >= 7, disabledReason = "交渉材料が足りない", effect = r => { r.freedomRegion = "owari"; r.shachiGaze += 1; } },
                    new Choice { label = "尾張の出来事を探す\n名のない気配", next = "freedom_owari" },
                    new Choice { label = "尾張の怪異に踏み込む", battle = "well_tentacle", effect = r => { r.freedomRegion = "owari"; r.battleReturnScene = "freedom_owari"; } },
                    new Choice { label = "尾張深層へ\n尾張3以上", next = "owari_deep_route", condition = r => r.owari >= 3 || r.shachiGaze >= 3, disabledReason = "尾張での手がかりが足りない", effect = r => { r.freedomRegion = "owari"; } },
                    new Choice { label = "戻る", next = "nagoya_after_battle", effect = r => { r.freedomRegion = ""; } }
                }
            };
            scenes["freedom_mikawa"] = new SceneDef
            {
                id = "freedom_mikawa",
                title = "西三河方面",
                area = "西三河 / 滞在中",
                image = "miso",
                portrait = "event_miso_voice",
                text = "岡崎の蔵、豊田のライン、有松の糸が、別々の答えを出している。\n\nここにいる間は西三河系の出来事だけが起こる。別地方へ移るには戻るを選ぶ。",
                choices =
                {
                    new Choice { label = "岡崎の蔵を読む\n味噌+1/三河+1", next = "freedom_mikawa", effect = r => { r.freedomRegion = "mikawa"; r.mikawa += 1; r.stats.misoResistance += 1; } },
                    new Choice { label = "豊田で検査を学ぶ\n機械+2/HP-1", next = "freedom_mikawa", effect = r => { r.freedomRegion = "mikawa"; r.stats.machineAptitude += 2; r.stats.hp = Math.Max(1, r.stats.hp - 1); } },
                    new Choice { label = "有松の布を買う\n所持金-260/危険察知+1", next = "freedom_mikawa", condition = r => r.stats.money >= 260, disabledReason = "所持金が足りない", effect = r => { r.freedomRegion = "mikawa"; r.stats.money -= 260; r.dangerWarnings += 1; } },
                    new Choice { label = "工場規格で通る\n機械5以上", next = "freedom_mikawa", condition = r => r.stats.machineAptitude >= 5, disabledReason = "機械適性が足りない", effect = r => { r.freedomRegion = "mikawa"; r.stats.defense += 1; r.npcAirport += 1; } },
                    new Choice { label = "地元の顔で通してもらう\n地元6以上", next = "freedom_mikawa", condition = r => r.stats.localKnowledge >= 6, disabledReason = "地元知識が足りない", effect = r => { r.freedomRegion = "mikawa"; r.mikawa += 1; r.dangerWarnings += 1; r.npcAirport += 1; } },
                    new Choice { label = "味噌声塊を弱める\n味噌4以上", next = "freedom_mikawa", condition = r => r.stats.misoResistance >= 4, disabledReason = "味噌耐性が足りない", effect = r => { r.freedomRegion = "mikawa"; r.flags.Add("weaken_miso_voice"); r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); } },
                    new Choice { label = "影武者と交渉する\n地元+味噌7以上", next = "freedom_mikawa_deal", condition = r => r.stats.localKnowledge + r.stats.misoResistance >= 7, disabledReason = "交渉材料が足りない", effect = r => { r.freedomRegion = "mikawa"; } },
                    new Choice { label = "三河の出来事を探す\n名のない気配", next = "freedom_mikawa" },
                    new Choice { label = "三河の怪異に踏み込む", battle = "shadow_retainer", effect = r => { r.freedomRegion = "mikawa"; r.battleReturnScene = "freedom_mikawa"; } },
                    new Choice { label = "三河深層へ\n三河3以上", next = "mikawa_deep_route", condition = r => r.mikawa >= 3 || r.stats.misoResistance >= 5, disabledReason = "三河での手がかりが足りない", effect = r => { r.freedomRegion = "mikawa"; } },
                    new Choice { label = "戻る", next = "nagoya_after_battle", effect = r => { r.freedomRegion = ""; } }
                }
            };
            scenes["freedom_chita"] = new SceneDef
            {
                id = "freedom_chita",
                title = "知多・海方面",
                area = "知多半島 / 滞在中",
                image = "gamagori",
                portrait = "event_gamagori_diver",
                text = "黒酢の運河、常滑の坂、蒲郡の星図が、空港へ向かう海の下で繋がっている。\n\nここにいる間は知多・海系の出来事だけが起こる。別地方へ移るには戻るを選ぶ。",
                choices =
                {
                    new Choice { label = "半田で休む\nHP+4/味噌+1", next = "freedom_chita", effect = r => { r.freedomRegion = "chita"; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 4); r.stats.misoResistance += 1; } },
                    new Choice { label = "常滑から空港を見る\n地元+1/空港+1", next = "freedom_chita", effect = r => { r.freedomRegion = "chita"; r.stats.localKnowledge += 1; r.npcAirport += 1; } },
                    new Choice { label = "蒲郡で星図を読む\n神話+1/SAN-1", next = "freedom_chita", effect = r => { r.freedomRegion = "chita"; r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } },
                    new Choice { label = "海路で空港へ近づく\nLUK判定", next = "freedom_chita", effect = r => { r.freedomRegion = "chita"; int target = 14; int roll = RollLuckDiceAgainst(target); if (roll >= target) { r.npcAirport += 2; r.stats.luck += 1; } else { r.npcAirport += 1; r.stats.hp = Math.Max(1, r.stats.hp - 2); } } },
                    new Choice { label = "黒潮を読んで迂回\n神話3以上", next = "freedom_chita", condition = r => r.stats.mythosKnowledge >= 3, disabledReason = "神話理解が足りない", effect = r => { r.freedomRegion = "chita"; r.dangerWarnings += 2; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } },
                    new Choice { label = "地元の渡船を使う\n地元6以上", next = "airport_bridge", condition = r => r.stats.localKnowledge >= 6, disabledReason = "地元知識が足りない", effect = r => { r.freedomRegion = ""; r.npcAirport += 2; r.dangerWarnings += 1; } },
                    new Choice { label = "空港職員へ無線相談\n空港3以上", next = "airport_bridge", condition = r => r.npcAirport >= 3, disabledReason = "空港知識が足りない", effect = r => { r.freedomRegion = ""; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); } },
                    new Choice { label = "知多・海の出来事を探す\n名のない気配", next = "freedom_chita" },
                    new Choice { label = "海の怪異に踏み込む", battle = "deep_one_clerk", effect = r => { r.freedomRegion = "chita"; r.battleReturnScene = "freedom_chita"; } },
                    new Choice { label = "海底深層へ\n空港+神話6以上", next = "chita_deep_route", condition = r => r.npcAirport + r.stats.mythosKnowledge >= 6, disabledReason = "海底へ進む手がかりが足りない", effect = r => { r.freedomRegion = "chita"; } },
                    new Choice { label = "戻る", next = "nagoya_after_battle", effect = r => { r.freedomRegion = ""; } }
                }
            };
            scenes["freedom_optional"] = new SceneDef
            {
                id = "freedom_optional",
                title = "任意探索",
                area = "異界愛知 / 寄り道",
                image = "osu",
                portrait = "event_occult_researcher",
                text = "地図に載らない寄り道がいくつか口を開けている。\n\n踏み込めば能力やフラグが伸びる。避ければ消耗を抑えられる。",
                choices =
                {
                    new Choice { label = "鶴舞地下書庫\n神話+2/SAN-2", next = "freedom_optional", effect = r => { r.stats.mythosKnowledge += 2; r.stats.sanity = Math.Max(0, r.stats.sanity - 2); } },
                    new Choice { label = "犬山の面市\nLUK+1/SAN-1", next = "freedom_optional", effect = r => { r.stats.luck += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } },
                    new Choice { label = "瀬戸の窯跡\n防御+1/機械+1", next = "freedom_optional", effect = r => { r.stats.defense += 1; r.stats.machineAptitude += 1; } },
                    new Choice { label = "知立の人形舞台\nLUK判定", next = "freedom_optional", effect = r => { int target = 15; int roll = RollLuckDiceAgainst(target); if (roll >= target) { r.stats.attack += 1; r.dangerWarnings += 1; } else { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); } } },
                    new Choice { label = "香嵐渓で正気を戻す\nSAN+3/所持金-200", next = "freedom_optional", condition = r => r.stats.money >= 200, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= 200; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 3); } },
                    new Choice { label = "栄の契約屋\n攻撃+1/汚染+1", next = "freedom_optional", effect = r => { r.stats.attack += 1; r.stats.mythosCorruption += 1; } },
                    new Choice { label = "危険を避けて観察\n危険察知+1", next = "nagoya_after_battle", effect = r => { r.dangerWarnings += 1; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); } },
                    new Choice { label = "戻る", next = "nagoya_after_battle", effect = r => { r.freedomRegion = ""; } }
                }
            };
            scenes["freedom_owari_deal"] = new SceneDef
            {
                id = "freedom_owari_deal",
                title = "鯱との交渉",
                area = "尾張 / 非戦闘解決",
                image = "castle",
                portrait = "event_shachi_avatar",
                text = "金の鯱は問いを投げる。倒すのではなく、何を守るために尾張へ来たのか。\n\n言葉を選べば、怪異は敵ではなく境界の番人になる。",
                choices =
                {
                    new Choice { label = "境界を守ると約束する\n防御+2/SAN+1", next = "freedom_owari", effect = r => { r.stats.defense += 2; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); r.flags.Add("shachi_truce"); } },
                    new Choice { label = "鯱の名を聞く\n神話+1/SAN-2", next = "freedom_owari", effect = r => { r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 2); } }
                }
            };
            scenes["freedom_mikawa_deal"] = new SceneDef
            {
                id = "freedom_mikawa_deal",
                title = "影武者との取引",
                area = "西三河 / 非戦闘解決",
                image = "okazaki",
                portrait = "event_battlefield_monk",
                text = "影武者は刀を抜かない。代わりに、味噌蔵の奥で古い誓紙を差し出す。\n\n読めれば戦わずに通れる。読めなければ影はあなたの形になる。",
                choices =
                {
                    new Choice { label = "誓紙を読み替える\n攻防+1", next = "freedom_mikawa", effect = r => { r.stats.attack += 1; r.stats.defense += 1; r.flags.Add("mikawa_shadow_pact"); } },
                    new Choice { label = "影を空港へ向かわせる\n空港+2/SAN-1", next = "freedom_mikawa", effect = r => { r.npcAirport += 2; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } }
                }
            };
            scenes["owari_deep_route"] = new SceneDef
            {
                id = "owari_deep_route",
                title = "尾張深層",
                area = "尾張 / 深層",
                image = "owari_shrine",
                portrait = "event_arimatsu_weaver",
                text = "尾張の道は城ではなく、織物の裏側へ続いていた。\n\nここでは、帰る道そのものを編み直すことができる。",
                choices =
                {
                    new Choice { label = "帰路の糸を編む\n危険察知+2", next = "freedom_owari", effect = r => { r.dangerWarnings += 2; r.stats.localKnowledge += 1; } },
                    new Choice { label = "鯱の夢を断つ\n神話5以上END", ending = "owari_thread_end", condition = r => r.stats.mythosKnowledge >= 5, disabledReason = "神話理解が足りない", effect = r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); } },
                    new Choice { label = "戻る", next = "freedom_owari" }
                }
            };
            scenes["mikawa_deep_route"] = new SceneDef
            {
                id = "mikawa_deep_route",
                title = "三河深層",
                area = "西三河 / 深層",
                image = "miso",
                portrait = "event_miso_voice",
                text = "味噌蔵の底で、発酵は時間ではなく記憶を食べていた。\n\nここでは、戦闘ではなく耐えることが答えになる。",
                choices =
                {
                    new Choice { label = "発酵の拍を合わせる\n味噌+2/SAN+1", next = "freedom_mikawa", effect = r => { r.stats.misoResistance += 2; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); } },
                    new Choice { label = "人でない工程に入る\n機械6以上END", ending = "mikawa_process_end", condition = r => r.stats.machineAptitude >= 6, disabledReason = "機械適性が足りない", effect = r => { r.stats.mythosCorruption += 1; } },
                    new Choice { label = "戻る", next = "freedom_mikawa" }
                }
            };
            scenes["chita_deep_route"] = new SceneDef
            {
                id = "chita_deep_route",
                title = "知多海底深層",
                area = "知多・海 / 深層",
                image = "chita_coast",
                portrait = "event_gamagori_diver",
                text = "潮の下に、空港の滑走路と同じ形の海底道がある。\n\nここまで来ると、帰ることと飛ぶことの区別が薄くなる。",
                choices =
                {
                    new Choice { label = "海底道を測る\n空港+2/神話+1", next = "freedom_chita", effect = r => { r.npcAirport += 2; r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } },
                    new Choice { label = "海底から離陸する\n空港5以上END", ending = "chita_sea_takeoff", condition = r => r.npcAirport >= 5, disabledReason = "空港知識が足りない", effect = r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); } },
                    new Choice { label = "戻る", next = "freedom_chita" }
                }
            };
        }
        void AddAiWideExpansion()
        {
            if (scenes.TryGetValue("nagoya_start", out var start))
            {
                start.choices.Insert(1, new Choice { label = "記憶と方針\n市場/目標", next = "meieki_route_support", effect = r => { r.freedomRegion = ""; } });
            }
            if (scenes.TryGetValue("freedom_base", out var baseHub))
            {
                baseHub.choices.Insert(3, new Choice { label = "土地鑑定人に相談\n地元+2/記憶片-2", next = "freedom_base", condition = r => progress.memoryFragments >= 2, disabledReason = "記憶片が足りない", effect = r => { progress.memoryFragments -= 2; r.stats.localKnowledge += 2; SaveProgress(); } });
                baseHub.choices.Insert(4, new Choice { label = "鯱除けの札を貼る\n鯱-2/記憶片-3", next = "freedom_base", condition = r => progress.memoryFragments >= 3 && r.shachiGaze >= 2, disabledReason = "記憶片か鯱注視が足りない", effect = r => { progress.memoryFragments -= 3; r.shachiGaze = Math.Max(0, r.shachiGaze - 2); r.dangerWarnings += 1; SaveProgress(); } });
            }
            scenes["memory_market"] = new SceneDef
            {
                id = "memory_market",
                title = "名駅記憶市場",
                area = "名駅の底 / 周回強化",
                image = "station",
                portrait = "event_memory_vendor",
                text = "閉じたシャッターの奥に、前回までの失敗だけを売る市場がある。\n\n記憶片はただの通貨ではない。死に方を覚えているほど、次の周回で危険を読む力になる。",
                choices =
                {
                    new Choice { label = "地元の古地図を買う\n記憶片-3/地元+3", next = "memory_market", condition = r => progress.memoryFragments >= 3, disabledReason = "記憶片が足りない", effect = r => { progress.memoryFragments -= 3; r.stats.localKnowledge += 3; SaveProgress(); } },
                    new Choice { label = "敵の巡回表を買う\n記憶片-4/危険察知+3", next = "memory_market", condition = r => progress.memoryFragments >= 4, disabledReason = "記憶片が足りない", effect = r => { progress.memoryFragments -= 4; r.dangerWarnings += 3; SaveProgress(); } },
                    new Choice { label = "神話注釈を買う\n記憶片-5/神話+2/SAN-1", next = "memory_market", condition = r => progress.memoryFragments >= 5, disabledReason = "記憶片が足りない", effect = r => { progress.memoryFragments -= 5; r.stats.mythosKnowledge += 2; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); SaveProgress(); } },
                    new Choice { label = "未練を保険に変える\n記憶片-6/LUK+2", next = "memory_market", condition = r => progress.memoryFragments >= 6, disabledReason = "記憶片が足りない", effect = r => { progress.memoryFragments -= 6; r.stats.luck += 2; SaveProgress(); } },
                    new Choice { label = "戻る", next = "nagoya_after_battle" }
                }
            };
            scenes["route_planning"] = new SceneDef
            {
                id = "route_planning",
                title = "今回の攻略目標",
                area = "名駅の底 / 方針選択",
                image = "station",
                portrait = "event_route_cartographer",
                text = "路線図職人は、白紙の切符に今回の目的を書けと言う。\n\n目的を決めると、道は少し狭くなる。そのぶん報酬と危険の形がはっきりする。",
                choices =
                {
                    new Choice { label = "空港帰還を狙う\n空港+2/危険+1", next = "nagoya_after_battle", effect = r => { r.routeGoal = "airport_return"; r.npcAirport += 2; r.dangerWarnings += 1; } },
                    new Choice { label = "双鯱調停を狙う\n鯱+2/神話+1", next = "nagoya_after_battle", effect = r => { r.routeGoal = "shachi_true"; r.shachiGaze += 2; r.stats.mythosKnowledge += 1; } },
                    new Choice { label = "地方深層ENDを狙う\n地元+2/神話+1", next = "nagoya_after_battle", effect = r => { r.routeGoal = "regional_deep"; r.stats.localKnowledge += 2; r.stats.mythosKnowledge += 1; } },
                    new Choice { label = "生存重視で進む\n危険察知+3/攻撃-1", next = "nagoya_after_battle", effect = r => { r.routeGoal = "survive"; r.dangerWarnings += 3; r.stats.attack = Math.Max(1, r.stats.attack - 1); } },
                    new Choice { label = "決めずに戻る", next = "nagoya_after_battle" }
                }
            };
            scenes["local_mastery_gate"] = new SceneDef
            {
                id = "local_mastery_gate",
                title = "地元だけが知る改札",
                area = "名駅の底 / 地元知識8以上",
                image = "station",
                portrait = "event_route_cartographer",
                text = "案内図に載らない改札がある。観光客には壁に見えるが、土地の名前を順番に読める者には通路に見える。\n\nここを抜ければ、戦闘を一つ避けて空港へ近づける。",
                choices =
                {
                    new Choice { label = "地名を順に読む\n空港+3/SAN+1", next = "airport_bridge", condition = r => r.stats.localKnowledge >= 8, disabledReason = "地元知識が足りない", effect = r => { r.npcAirport += 3; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); r.flags.Add("local_master_route"); } },
                    new Choice { label = "読めないまま覗く\nSAN-2", next = "nagoya_after_battle", effect = r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); } },
                    new Choice { label = "戻る", next = "nagoya_after_battle" }
                }
            };
            scenes["shachi_hunt_threshold"] = new SceneDef
            {
                id = "shachi_hunt_threshold",
                title = "鯱狩りの夜",
                area = "尾張 / 鯱注視7以上",
                image = "castle",
                portrait = "shachi_hunter",
                text = "鯱の注視が強くなりすぎた時、それを追うものも現れる。\n\n鯱狩りはあなたを獲物とは呼ばない。鯱に見られている者、と呼ぶ。",
                choices =
                {
                    new Choice { label = "地元道で撒く\n地元8以上", next = "nagoya_after_battle", condition = r => r.stats.localKnowledge >= 8, disabledReason = "地元知識が足りない", effect = r => { r.dangerWarnings += 2; r.shachiGaze = Math.Max(0, r.shachiGaze - 2); } },
                    new Choice { label = "狩人と戦う", battle = "shachi_hunter", effect = r => { r.battleReturnScene = "nagoya_after_battle"; } },
                    new Choice { label = "鯱へ助けを求める\n神話4以上", ending = "true_shachi", condition = r => r.stats.mythosKnowledge >= 4, disabledReason = "神話理解が足りない" }
                }
            };
            scenes["meieki_route_support"] = new SceneDef
            {
                id = "meieki_route_support",
                title = "記憶と方針",
                area = "名駅の底 / 裏の机",
                image = "station",
                portrait = "event_route_cartographer",
                text = "ロッカーの裏に、記憶市場の札と作戦地図が重なっている。\n\nここは道を増やす場所ではなく、次に進む道の意味を変える場所だ。",
                choices =
                {
                    new Choice { label = "記憶市場へ\n周回強化", next = "memory_market", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "攻略目標を選ぶ\n今回の方針", next = "route_planning", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "ゴールを確認する\n脱出条件", next = "meieki_goal", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "作戦会議をする\n方針を決める", next = "meieki_strategy", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "地元だけが知る改札\n地元8以上", next = "local_mastery_gate", condition = r => r.stats.localKnowledge >= 8, disabledReason = "地元知識が足りない", effect = r => { r.freedomRegion = ""; } },
                    new Choice { label = "鯱狩りの夜へ\n鯱7以上", next = "shachi_hunt_threshold", condition = r => r.shachiGaze >= 7, disabledReason = "鯱の注視が足りない", effect = r => { r.freedomRegion = "owari"; } },
                    new Choice { label = "戻る", next = "nagoya_after_battle", effect = r => { r.freedomRegion = ""; } }
                }
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
                    new Choice { label = "STAGE 2へ短絡\n路線図母体撃破済み", next = "stage2_hub", condition = r => progress.bossesDefeated.Contains("stage_boss_1"), disabledReason = "名駅地下のボス記録がない" },
                    new Choice { label = "STAGE 3へ短絡\n金鯱影王撃破済み", next = "stage3_hub", condition = r => progress.bossesDefeated.Contains("stage_boss_2"), disabledReason = "尾張のボス記録がない" },
                    new Choice { label = "STAGE 4へ短絡\n声塊撃破済み", next = "stage4_hub", condition = r => progress.bossesDefeated.Contains("stage_boss_3"), disabledReason = "三河のボス記録がない" },
                    new Choice { label = "STAGE 5へ短絡\n監査官撃破済み", next = "stage5_hub", condition = r => progress.bossesDefeated.Contains("stage_boss_4"), disabledReason = "知多のボス記録がない" },
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
                    text = StageHubIntro(stageNo) + "\n\nここから十の出来事を越える。途中の選択でHP、所持金、LUK、SAN、神話理解が削られ、また強くなる。\n\nSTAGEボスは地方深層の先ではなく、このステージ内のボスゲートにいる。十の出来事を越えるか、近道でボス地点へ急げば到達できる。",
                    choices =
                    {
                        new Choice { label = "探索を始める", next = first },
                        new Choice { label = "ステージ内ショップ\n周回強化", next = "stage" + stageNo + "_shop" },
                        new Choice { label = "息を整える\nHP+3/SAN+1", next = first, effect = r => { r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 3); r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); } },
                        new Choice { label = "不吉な近道\n神話+1/SAN-2", next = first, effect = r => { r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 2); } },
                        new Choice { label = "STAGEボス地点へ急ぐ\n危険察知+1", next = "stage" + stageNo + "_boss_gate", effect = r => { r.dangerWarnings += 1; StageReward(r, stageNo, 9, false); } }
                    }
                };
                AddStageShopScene(stageNo, "stage" + stageNo + "_shop", hub, titles[stage], areas[stage], images[stage]);
                for (int index = 0; index < 10; index++)
                    AddStageEventScene(stageNo, index, titles[stage], areas[stage], images[stage], portraits[stage], bossIds[stage]);
            }
        }
        void AddStageShopScene(int stageNo, string id, string returnScene, string stageTitle, string area, string image)
        {
            int careCost = 120 + stageNo * 45;
            int offenseCost = 180 + stageNo * 70;
            int guardCost = 170 + stageNo * 65;
            int toolCost = 210 + stageNo * 75;
            int gearCost = 280 + stageNo * 95;
            scenes[id] = new SceneDef
            {
                id = id,
                title = "STAGE " + stageNo + ": 路地の店",
                area = area + " / " + stageTitle,
                image = image,
                portrait = StageShopPortrait(stageNo),
                text = StageShopText(stageNo) + "\n\nここで買った強化はこの周回の体にだけ残る。帰れなければ、代金の重さだけが記憶に沈む。",
                choices =
                {
                    new Choice { label = "救急封筒\n所持金-" + careCost + "/最大HP+4/SAN+2", next = id, condition = r => r.stats.money >= careCost, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= careCost; r.stats.maxHp += 4; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 6); r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); r.pendingOutcomeText = "封筒の中身は湿っていたが、体は少しだけ戻った。"; } },
                    new Choice { label = "赤い工具\n所持金-" + offenseCost + "/攻撃+2", next = id, condition = r => r.stats.money >= offenseCost, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= offenseCost; r.stats.attack += 2; r.pendingOutcomeText = "工具は手に馴染まない。だが、怪異には馴染む。"; } },
                    new Choice { label = "防護札\n所持金-" + guardCost + "/防御+2", next = id, condition = r => r.stats.money >= guardCost, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= guardCost; r.stats.defense += 2; r.pendingOutcomeText = "札は服の内側に貼りつき、剥がすまで肌の一部になった。"; } },
                    new Choice { label = "曲がった方位具\n所持金-" + toolCost + "/速さ+1/LUK+1", next = id, condition = r => r.stats.money >= toolCost, disabledReason = "所持金が足りない", effect = r => { r.stats.money -= toolCost; r.stats.speed += 1; r.stats.luck += 1; r.pendingOutcomeText = "針は北を指さない。あなたがまだ死んでいない方角だけを指す。"; } },
                    new Choice { label = "土地装備を買う\n所持金-" + gearCost, next = id, condition = r => r.stats.money >= gearCost, disabledReason = "所持金が足りない", effect = r => { BuyStageShopGear(r, stageNo, gearCost); } },
                    new Choice { label = "戻る", next = returnScene }
                }
            };
        }
        string StageShopPortrait(int stageNo)
        {
            if (stageNo == 5)
                return "event_gate_inspector";
            if (stageNo == 3)
                return "event_factory_inspector";
            if (stageNo == 4)
                return "event_gamagori_diver";
            if (stageNo == 2)
                return "event_sakae_broker";
            return "event_memory_vendor";
        }
        string StageShopText(int stageNo)
        {
            switch (stageNo)
            {
                case 1: return "改札のない売店で、店主は値札を裏返したまま待っている。";
                case 2: return "堀端の屋台は水面にだけ映り、硬貨を落とすと品物が浮く。";
                case 3: return "発酵蔵の片隅で、工具と護符が同じ棚に吊られている。";
                case 4: return "濡れた土産物屋の奥で、海図より古い装備が干されている。";
                case 5: return "制限区域の自販機は、押したボタンと違うものを吐き出す。";
            }
            return "路地の店が、まだ使えるものだけを並べている。";
        }
        void BuyStageShopGear(RunState r, int stageNo, int cost)
        {
            r.stats.money -= cost;
            Gear gear = GenerateRandomGear(10 + stageNo * 7 + r.instability * 3);
            Gear current = CurrentGearForSlot(gear.slot);
            EquipGear(gear);
            r.pendingOutcomeText = "購入装備: " + GearShortName(gear) + "\n" + GearDeltaLine(gear, current) + "\n" + GearStrengthReason(gear);
            LogRun("ショップ装備購入: " + GearShortName(gear));
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
                string afterBoss = stageNo < 5 ? "stage" + (stageNo + 1) + "_hub" : "airport_bridge";
                scenes[next] = new SceneDef
                {
                    id = next,
                    title = "STAGE " + stageNo + ": ボス前",
                    area = area + " / 境界核",
                    image = image,
                    portrait = portrait,
                    text = BossGateText(stageNo) + "\n\nここがSTAGEボス地点だ。十の出来事が一つの影へ集まり、地方深層ではなく、この境界核でステージボスの形を取った。\n\n最後の準備には所持金180が必要。足りなければ、準備せずにボスへ挑むしかない。",
                    choices =
                    {
                        new Choice { label = "ボスに挑む", battle = bossId, effect = r => { r.battleReturnScene = afterBoss; } },
                        new Choice { label = "ステージ内ショップ\n最後の買い物", next = "stage" + stageNo + "_boss_shop" },
                        new Choice { label = "最後の準備\n所持金-180/HP+4/SAN+2", next = next, condition = r => r.stats.money >= 180, disabledReason = "所持金180が必要", effect = r => { r.stats.money -= 180; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 4); r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 2); } }
                    }
                };
                AddStageShopScene(stageNo, "stage" + stageNo + "_boss_shop", next, stageTitle, area, image);
            }
            AddAirportFinalApproach();
        }
        SceneDef BuildRuntimeStageEventScene(string sceneId, SceneDef baseScene)
        {
            if (run == null)
                return baseScene;
            int stageNo;
            int index;
            if (!TryParseStageEventId(sceneId, out stageNo, out index))
                return baseScene;
            if (run.stageSeed == 0)
                run.stageSeed = rng.Next(100000, 999999);
            int eventNo = index + 1;
            int variant = RuntimeStageVariant(stageNo, index, 0);
            int choiceVariant = RuntimeStageVariant(stageNo, index, 1) % 3;
            string next = index < 9 ? "stage" + stageNo + "_event_" + (index + 1) : "stage" + stageNo + "_boss_gate";
            string enemyId = "stage" + stageNo + "_enemy_" + eventNo;
            SceneDef scene = new SceneDef
            {
                id = baseScene.id,
                title = "STAGE " + stageNo + "-" + eventNo + ": " + RuntimeStageEventTitle(stageNo, index, variant),
                area = baseScene.area,
                image = RuntimeStageEventImage(stageNo, baseScene.image, variant),
                portrait = RuntimeStageEventPortrait(stageNo, baseScene.portrait, variant),
                text = RuntimeStageEventMotif(stageNo, index, variant) + "\n\n" + RuntimeStageEventOmen(stageNo, index, variant)
            };
            scene.choices.AddRange(RuntimeStageEventChoices(stageNo, index, next, enemyId, choiceVariant));
            return scene;
        }
        bool TryParseStageEventId(string sceneId, out int stageNo, out int index)
        {
            stageNo = 0;
            index = 0;
            if (string.IsNullOrEmpty(sceneId) || !sceneId.StartsWith("stage"))
                return false;
            string marker = "_event_";
            int markerIndex = sceneId.IndexOf(marker, StringComparison.Ordinal);
            if (markerIndex <= 5)
                return false;
            string stageText = sceneId.Substring(5, markerIndex - 5);
            string indexText = sceneId.Substring(markerIndex + marker.Length);
            if (!int.TryParse(stageText, out stageNo) || !int.TryParse(indexText, out index))
                return false;
            return stageNo >= 1 && stageNo <= 5 && index >= 0 && index <= 9;
        }
        int RuntimeStageVariant(int stageNo, int index, int salt)
        {
            unchecked
            {
                int value = run.stageSeed;
                value = (value * 397) ^ (stageNo * 73856093);
                value = (value * 397) ^ (index * 19349663);
                value = (value * 397) ^ (salt * 83492791);
                return (value & int.MaxValue) % 4;
            }
        }
        string RuntimeStageEventImage(int stageNo, string baseImage, int variant)
        {
            if (variant == 1 && stageNo <= 3)
                return "stage_meieki_unstable";
            if ((variant == 2 || variant == 3) && stageNo >= 4)
                return "stage_airport_service";
            return baseImage;
        }
        string RuntimeStageEventPortrait(int stageNo, string basePortrait, int variant)
        {
            if (variant == 1)
                return "stage_route_attendant";
            if ((variant == 2 || variant == 3) && stageNo >= 4)
                return "stage_quarantine_clerk";
            return basePortrait;
        }
        string RuntimeStageEventTitle(int stageNo, int index, int variant)
        {
            if (variant == 0)
                return StageEventTitle(stageNo, index);
            string[] names =
            {
                "貼り替わる案内",
                "遅延証明の口",
                "閉まらない扉",
                "黒い呼出番号",
                "逆流する標識",
                "濡れた許可印",
                "名を食う列",
                "剥がれた出口"
            };
            return names[(index + variant * 2 + stageNo) % names.Length];
        }
        string RuntimeStageEventMotif(int stageNo, int index, int variant)
        {
            if (variant == 0)
                return StageEventMotif(stageNo, index);
            string[] places =
            {
                "名駅地下の使われていない改札",
                "尾張の水音だけ残る城下",
                "三河の発酵蔵の奥",
                "知多の海風が届かない堤防下",
                "空港の荷捌き通路"
            };
            string[] signs =
            {
                "床の継ぎ目が、あなたの歩幅に合わせて一つずつ増える。",
                "古い係員の影が、顔のないまま切符鋏を鳴らしている。",
                "掲示板の文字が濡れ、まだ選んでいない選択だけを先に汚す。",
                "閉まった扉の向こうで、同じ名前を呼ぶ声が人数分より多い。",
                "置き去りの荷札に、前回死んだ場所の匂いが染みている。",
                "赤いランプが瞬くたび、所持金の硬貨だけが少し軽くなる。",
                "通路の先に立つものが、こちらを見ずに道を譲る。",
                "案内矢印が壁から剥がれ、黒い虫のように足元へ集まる。"
            };
            return places[stageNo - 1] + "で、" + signs[(index + variant * 3) % signs.Length];
        }
        string RuntimeStageEventOmen(int stageNo, int index, int variant)
        {
            string[] omens =
            {
                "遠くで、ボスゲートの鍵束が鳴った。",
                "背後の道は残っている。ただ、同じ道ではなくなった。",
                "呼吸を浅くすると、壁の中の呼吸も浅くなる。",
                "ここで得るものは小さい。失うものは、次の角で形になる。"
            };
            return omens[(stageNo + index + variant) % omens.Length];
        }
        List<Choice> RuntimeStageEventChoices(int stageNo, int index, string next, string enemyId, int choiceVariant)
        {
            if (choiceVariant == 0)
            {
                return new List<Choice>
                {
                    new Choice { label = "息を殺して通る\nSAN+1/所持金-80", next = next, effect = r => { if (r.stats.money >= 80) { r.stats.money -= 80; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); r.pendingOutcomeText = "音を立てずに通った。背後で、誰かの舌打ちだけが残った。"; } else { r.stats.hp = Math.Max(1, r.stats.hp - 2); r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); r.pendingOutcomeText = "足りない分を血で払った。通路はそれで黙った。"; } StageReward(r, stageNo, index, false); } },
                    new Choice { label = "踏み込む\nLUK判定", next = next, effect = r => { int target = 12 + stageNo + index / 3; int roll = RollLuckDiceAgainst(target); if (roll >= target) { r.stats.luck += 1; r.stats.money += 80 + stageNo * 30; r.pendingOutcomeText = "出目が沈む前に掴んだ。影の財布から硬貨がこぼれた。"; StageReward(r, stageNo, index, true); } else { r.stats.hp = Math.Max(1, r.stats.hp - (1 + stageNo)); r.stats.sanity = Math.Max(0, r.stats.sanity - 1); r.pendingOutcomeText = "一歩深かった。床下のものが足首を覚えた。"; } } },
                    new Choice { label = "名を読んで捻じる\n神話/SAN", next = next, condition = r => r.stats.mythosKnowledge >= stageNo - 1, disabledReason = "神話理解が足りない", effect = r => { r.stats.mythosKnowledge += 1; r.stats.mythosCorruption += index % 3 == 0 ? 1 : 0; r.stats.sanity = Math.Max(0, r.stats.sanity - (2 + stageNo / 2)); r.pendingOutcomeText = "読んだ名は、少しだけあなたの声になった。"; StageReward(r, stageNo, index, true); } },
                    new Choice { label = "呼ばれた影を迎撃する", battle = enemyId, effect = r => { r.battleReturnScene = next; } }
                };
            }
            if (choiceVariant == 1)
            {
                return new List<Choice>
                {
                    new Choice { label = "記録を破る\n危険察知+1/HP-1", next = next, effect = r => { r.dangerWarnings += 1; r.stats.hp = Math.Max(1, r.stats.hp - 1); r.pendingOutcomeText = "破れた紙片が逃げ道の形に散った。"; StageReward(r, stageNo, index, false); } },
                    new Choice { label = "影を追い越す\nLUK判定", next = next, effect = r => { int target = 13 + stageNo + index / 4; int roll = RollLuckDiceAgainst(target); if (roll >= target) { r.stats.speed += 1; r.stats.money += 100 + stageNo * 35; r.pendingOutcomeText = "影より先に曲がった。遅れて来たものは壁にぶつかった。"; StageReward(r, stageNo, index, true); } else { r.stats.hp = Math.Max(1, r.stats.hp - stageNo); r.dangerWarnings += 1; r.pendingOutcomeText = "影はあなたの少し前にいた。"; } } },
                    new Choice { label = "供物を読む\n神話+1/SAN-2", next = next, condition = r => r.stats.mythosKnowledge >= Math.Max(0, stageNo - 2), disabledReason = "神話理解が足りない", effect = r => { r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 2); r.pendingOutcomeText = "供物の向きが変わり、次の道だけが残った。"; StageReward(r, stageNo, index, true); } },
                    new Choice { label = "札を鳴らすものを斬る", battle = enemyId, effect = r => { r.battleReturnScene = next; } }
                };
            }
            return new List<Choice>
            {
                new Choice { label = "金で黙らせる\n所持金-120", next = next, condition = r => r.stats.money >= 120, disabledReason = "所持金120が必要", effect = r => { r.stats.money -= 120; r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 1); r.pendingOutcomeText = "硬貨の音が止むまで、通路の口は閉じていた。"; StageReward(r, stageNo, index, false); } },
                new Choice { label = "正面から割る\n攻撃/HP", next = next, effect = r => { int damage = Math.Max(1, stageNo + 2 - r.stats.attack / 3); r.stats.hp = Math.Max(1, r.stats.hp - damage); r.stats.attack += index % 4 == 0 ? 1 : 0; r.pendingOutcomeText = "割れたものの内側に、あなたの通れる幅だけ隙間ができた。"; StageReward(r, stageNo, index, true); } },
                new Choice { label = "見ないで数える\n神話/SAN", next = next, condition = r => r.stats.mythosKnowledge >= stageNo - 1, disabledReason = "神話理解が足りない", effect = r => { r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - (1 + stageNo / 2)); r.stats.luck += index % 5 == 0 ? 1 : 0; r.pendingOutcomeText = "数は合った。見てはいけないものも、ひとつだけ増えた。"; StageReward(r, stageNo, index, true); } },
                new Choice { label = "検査灯の下へ出る", battle = enemyId, effect = r => { r.battleReturnScene = next; } }
            };
        }
        void AddAirportFinalApproach()
        {
            scenes["airport_manifest_hall"] = new SceneDef
            {
                id = "airport_manifest_hall",
                title = "搭乗名簿の廊下",
                area = "中部国際空港 / 制限区域",
                image = "airport",
                portrait = "event_gate_inspector",
                text = "名簿には、まだ死んでいないあなたの名前まで印字されている。\n\n空港は出口ではなく、帰る者を選別する器官だった。ここから先は搭乗ではなく、削除の手続きに近い。",
                choices =
                {
                    new Choice { label = "名簿から自分を探す\nSAN-1/危険察知+1", next = "airport_quarantine_spine", effect = r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 1); r.dangerWarnings += 1; } },
                    new Choice { label = "別人の名前で進む\nLUK判定", next = "airport_quarantine_spine", effect = r => { int target = 17; int roll = RollLuckDiceAgainst(target); if (roll >= target) { r.stats.luck += 1; r.npcAirport += 1; } else { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); } } },
                    new Choice { label = "搭乗券の群れを焼く", battle = "stage_enemy_13", effect = r => { r.battleReturnScene = "airport_quarantine_spine"; } }
                }
            };
            scenes["airport_quarantine_spine"] = new SceneDef
            {
                id = "airport_quarantine_spine",
                title = "帰還検疫線",
                area = "中部国際空港 / 検疫奥",
                image = "airport",
                portrait = "event_under_runway_clerk",
                text = "白い検疫線の向こうで、影だけが先に体温を測られている。\n\nここを越えるたび、現実へ戻れる部分と戻れない部分が分けられていく。",
                choices =
                {
                    new Choice { label = "体温を影に預ける\nHP-2/空港+1", next = "airport_final_taxiway", effect = r => { r.stats.hp = Math.Max(1, r.stats.hp - 2); r.npcAirport += 1; } },
                    new Choice { label = "検疫印を偽造する\n機械8以上", next = "airport_final_taxiway", condition = r => r.stats.machineAptitude >= 8, disabledReason = "機械適性が足りない", effect = r => { r.stats.money += 300; r.dangerWarnings += 1; } },
                    new Choice { label = "正気で押し通る\nSAN6以上", next = "airport_final_taxiway", condition = r => r.stats.sanity >= 6, disabledReason = "SANが足りない", effect = r => { r.stats.sanity = Math.Max(0, r.stats.sanity - 2); r.stats.localKnowledge += 1; } }
                }
            };
            scenes["airport_final_taxiway"] = new SceneDef
            {
                id = "airport_final_taxiway",
                title = "第零誘導路",
                area = "中部国際空港 / 滑走路外縁",
                image = "airport",
                portrait = "window_god",
                text = "誘導灯は滑走路ではなく、空港の内臓へ向かって並んでいる。\n\n管制塔の窓がひとつだけ開き、そこから誰かが欠航の印を押している。",
                choices =
                {
                    new Choice { label = "誘導灯を数えない\n神話+1/SAN-2", next = "airport_final_boss_gate", effect = r => { r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 2); } },
                    new Choice { label = "欠航印を盗む\n地元7以上", next = "airport_final_boss_gate", condition = r => r.stats.localKnowledge >= 7, disabledReason = "地元知識が足りない", effect = r => { r.dangerWarnings += 1; r.stats.luck += 1; } },
                    new Choice { label = "小神群を散らす", battle = "stage_boss_5", effect = r => { r.battleReturnScene = "airport_final_boss_gate"; } }
                }
            };
            scenes["airport_final_boss_gate"] = new SceneDef
            {
                id = "airport_final_boss_gate",
                title = "欠航管制室",
                area = "中部国際空港 / 最終管制",
                image = "airport",
                portrait = "gate_inspector",
                text = "管制室には椅子が一つだけある。座っているものは人ではないが、倒せる程度にはこの世界の規則に縛られている。\n\nその背後で、倒してはいけないものの気配だけが空港全体を沈めている。",
                choices =
                {
                    new Choice { label = "境界空港長に挑む", battle = "boundary_airport_director", effect = r => { r.battleReturnScene = "airport_gate"; } },
                    new Choice { label = "最後の準備\n所持金500/HP+6/SAN+3", next = "airport_final_boss_gate", condition = r => r.stats.money >= 500, disabledReason = "所持金500が必要", effect = r => { r.stats.money -= 500; r.stats.hp = Math.Min(r.stats.maxHp, r.stats.hp + 6); r.stats.sanity = Math.Min(r.stats.maxSanity, r.stats.sanity + 3); } }
                }
            };
        }
        string StageHubIntro(int stageNo)
        {
            switch (stageNo)
            {
                case 1:
                    return "名駅地下の案内板は、出口ではなくあなたの記憶を指している。十の出来事はすべて、同じロッカー番号へ戻っていく。";
                case 2:
                    return "尾張の屋根瓦に、金鯱ではない影が泳いでいる。城下の水路と織物の糸が、同じ結び目を作り始めた。";
                case 3:
                    return "三河の発酵蔵では、樽の泡が勤務表のように並ぶ。味噌、影武者、工場ラインが、あなたを工程の一部として数え始める。";
                case 4:
                    return "知多と蒲郡の海は、観光地の顔をしたまま深く沈む。潮位表の数字が、海底へ降りる階段の段数に変わっている。";
                case 5:
                    return "セントレアの搭乗案内は、まだ存在しない便名を読み上げる。ゲートの外側で、小さな神々が帰還手続きを噛み砕いている。";
            }
            return "土地の境界が薄くなり、十の出来事が同じ一点へ流れ込んでいる。";
        }
        string BossGateText(int stageNo)
        {
            switch (stageNo)
            {
                case 1:
                    return "柱番号、食券、地下街の呼吸が一つの鍵束になった。名駅のボスゲートは、出口の形をしてこちらを待っている。";
                case 2:
                    return "鯱の視線、城下の水拍子、尾張の古い印が影の王冠へ集まった。ここを越えなければ、屋根の下に帰路はない。";
                case 3:
                    return "味噌蔵の泡、影武者の拍、工場の検査音が発酵して声になった。三河のボスゲートは、あなたの呼吸を工程に入れようとしている。";
                case 4:
                    return "海底星図、潮位表、観光案内板の濡れた文字が監査印へ変わった。知多海底線の門は、空港へ進む許可を試している。";
                case 5:
                    return "便名、荷札、誘導灯、到着口が外側へ向いている。空港境界区のボスゲートは、帰還という選択肢そのものを食べている。";
            }
            return "十の出来事が境界核に集まり、ボスゲートの輪郭を作った。";
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
            if (bold)
                r.dangerWarnings += index % 3 == 0 ? 1 : 0;
            else
                r.dangerWarnings += 1;
            if (stageNo == 1)
                r.stats.localKnowledge += 1;
            else if (stageNo == 2)
            {
                r.owari += 1;
                if (bold)
                    r.shachiGaze += 1;
            }
            else if (stageNo == 3)
            {
                r.mikawa += 1;
                if (bold)
                    r.stats.misoResistance += 1;
            }
            else
                r.npcAirport += 1;
            int type = (stageNo + index) % 5;
            if (type == 0) r.stats.attack += bold ? 1 : 0;
            else if (type == 1) r.stats.defense += bold ? 1 : 0;
            else if (type == 2) r.stats.speed += bold ? 1 : 0;
            else if (type == 3) r.stats.localKnowledge += 1;
            else r.npcAirport += stageNo >= 4 ? 1 : 0;
            if (bold && r.stats.sanity <= r.stats.maxSanity / 2)
                r.stats.mythosKnowledge += 1;
            ApplyStageResonanceReward(r, stageNo, bold);
        }
        void ApplyStageResonanceReward(RunState r, int stageNo, bool bold)
        {
            if (r == null)
                return;
            if (HasGearEffect("stage_bonus"))
                r.stats.money += 70;
            if (HasGearEffect("danger_preparation"))
                r.dangerWarnings += 1;
            if (r.character.id == "local")
            {
                r.stats.localKnowledge += bold ? 1 : 0;
                if (stageNo == 1 || stageNo == 2)
                    r.dangerWarnings += 1;
            }
            else if (r.character.id == "worker" && (stageNo == 3 || stageNo == 5))
            {
                r.stats.machineAptitude += 1;
                r.stats.money += 40;
            }
            else if (r.character.id == "occult" && bold)
            {
                r.stats.mythosKnowledge += 1;
                r.stats.sanity = Math.Max(1, r.stats.sanity - 1);
            }
            else if (r.character.id == "centrair_agent" && stageNo >= 4)
            {
                r.npcAirport += 1;
            }
            else if (r.character.id == "arimatsu_weaver" && bold)
            {
                r.stats.luck += 1;
            }
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
                    new Choice { label = "影の隙間を走る\n速さ/LUK", next = "airport_manifest_hall", effect = r => { if (r.stats.speed + r.stats.luck >= 14) r.npcAirport += 1; else r.stats.sanity = Math.Max(0, r.stats.sanity - 2); } },
                    new Choice { label = "窓外神を見ない\n神話+1/SAN-1", next = "airport_manifest_hall", effect = r => { r.stats.mythosKnowledge += 1; r.stats.sanity = Math.Max(0, r.stats.sanity - 1); } },
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
                text = "境界空港長の印が割れ、搭乗券にはようやく行き先が戻った。\n\nあなたが持ち帰れるものは一つだけだ。記憶か、正気か、異界の鍵か。背後の深い気配は倒されていない。ただ、今回は目を閉じている。",
                choices =
                {
                    new Choice { label = "欠航印を越えて帰る", ending = "normal_clear", condition = r => progress.defeated.Contains("boundary_airport_director"), disabledReason = "境界空港長を越えていない" },
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
            enemies["boundary_airport_director"] = new EnemyDef
            {
                id = "boundary_airport_director",
                name = "境界空港長",
                image = "airport",
                maxHp = 145,
                attack = 30,
                defense = 12,
                speed = 10,
                sanityDamage = 9,
                reward = 55,
                weakness = "空港知識 / 欠航印 / 正気維持",
                portrait = "gate_inspector",
                intro = "管制室の椅子に、空港長の制服だけが座っている。\n\n袖口から伸びる黒い搭乗券が、あなたの帰還便へ欠航印を押そうとしていた。これは倒せる怪異だ。背後の外なるものではない。",
                victoryText = "欠航印は割れ、管制室の窓に朝焼けが戻った。遠くで眠る巨大な気配は、今回だけあなたを見失った。",
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
                    int hp = 28 + stage * 7 + i * 3;
                    int attack = 8 + stage * 2 + i / 2;
                    int defense = 2 + stage / 2 + i % 3;
                    int speed = 3 + (stage + i * 2) % 7;
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
            AddEnemy("stage_boss_1", "名駅地下の路線図母体", "station", 66, 16, 7, 5, 5, 24, "地元知識 / 迷わない意志", "locker_womb", "路線図が胎内のように広がり、すべての出口をへその緒で結ぶ。", "路線図母体は折り畳まれ、次の土地への線だけが残った。", "madness");
            AddEnemy("stage_boss_2", "尾張金鯱の影王", "castle", 80, 19, 8, 6, 5, 29, "鯱の注視 / 攻撃", "shachi_avatar", "城の影から、金鯱ではない鯱が王冠のように浮かぶ。", "影王は瓦へ戻り、空が少し低くなった。", "event_death");
            AddEnemy("stage_boss_3", "三河発酵する声塊", "miso", 92, 21, 9, 4, 7, 34, "味噌耐性 / SAN管理", "miso_voice", "声が発酵し、泡立つ肉の塊として樽から溢れる。", "声塊は沈み、空港へ続く臭いだけが残った。", "miso_sink");
            AddEnemy("stage_boss_4", "海底星図の深き監査官", "gamagori", 105, 23, 10, 6, 8, 40, "神話理解 / 水難耐性", "deep_one_clerk", "星図を背負った監査官が、海底の印鑑を押しに来る。", "監査印は割れ、空港の滑走路が星図に現れた。", "airport_lost");
            AddEnemy("stage_boss_5", "搭乗門外の小神群", "airport", 122, 26, 11, 8, 10, 50, "見ない勇気 / 神話理解", "window_god", "搭乗門の外側に、小さな神々が鈴なりになって待っている。", "小神群は搭乗時刻を失い、最後のゲートが開いた。", "airport_lost");
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
            AddEnemy("shachi_hunter", "鯱狩りの仮面武者", "castle", 46, 13, 6, 6, 5, 22, "地元知識 / 鯱の注視を祓う", "shachi_hunter", "金鯱を狩るために作られた仮面武者が、あなたを鯱の影として追ってくる。", "仮面は割れ、金の鱗だけが夜風に散った。", "event_death");
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
            if (choiceContent != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(choiceContent);
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
            int visibleTotal = Mathf.Min(total, choiceButtons.Length);
            for (int i = 0; i < visibleTotal; i++)
            {
                var command = choiceCommands[i];
                CreateChoiceButton(command.label, command.action, command.interactable);
            }
            if (choiceContent != null)
                choiceContent.anchoredPosition = Vector2.zero;
            return;
        }
#if false
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
#endif
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
            bool mobile = IsMobileLayout();
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
                text.alignment = TextAnchor.MiddleLeft;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Overflow;
                text.resizeTextForBestFit = false;
                text.fontSize = mobile ? (label.Contains("\n") || label.Length > 13 ? 17 : 19) : (label.Contains("\n") || label.Length > 13 ? 15 : 17);
                text.resizeTextMinSize = mobile ? 15 : 12;
                text.resizeTextMaxSize = mobile ? 19 : 17;
            }
            var layout = button.GetComponent<LayoutElement>();
            if (layout != null)
            {
                bool sideCommands = IsSideCommandLayout();
                layout.minHeight = sideCommands ? 68f : mobile ? 66f : 54f;
                layout.preferredHeight = sideCommands ? 78f : mobile ? 76f : 62f;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(button.GetComponent<RectTransform>());
            if (choiceContent != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(choiceContent);
            LayoutRebuilder.ForceRebuildLayoutImmediate(choiceRoot);
            Canvas.ForceUpdateCanvases();
        }
        void CreateChoiceSlots()
        {
            choiceButtons = new Button[16];
            choiceButtonLabels = new Text[16];
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                var parent = choiceContent != null ? choiceContent : choiceRoot;
                var button = NewButton("Choice" + i, parent, "", new Color(0.12f, 0.095f, 0.13f), 16);
                var layout = button.gameObject.AddComponent<LayoutElement>();
                layout.minHeight = 54f;
                layout.preferredHeight = 62f;
                layout.flexibleWidth = 1f;
                var text = button.GetComponentInChildren<Text>(true);
                if (text != null)
                {
                    text.text = "";
                    text.enabled = true;
                    text.gameObject.SetActive(true);
                    text.alignment = TextAnchor.MiddleLeft;
                    text.horizontalOverflow = HorizontalWrapMode.Wrap;
                   text.verticalOverflow = VerticalWrapMode.Overflow;
                    text.resizeTextForBestFit = false;
                    text.resizeTextMinSize = 12;
                    text.resizeTextMaxSize = 17;
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
            bool needsCreate = choiceRoot == null || choiceContent == null || choiceButtons == null || choiceButtons.Length != 16 || choiceButtonLabels == null || choiceButtonLabels.Length != 16;
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
            currentPortraitId = id;
            if (string.IsNullOrEmpty(id))
            {
                portraitPanel.gameObject.SetActive(false);
                portraitImage.sprite = null;
                return;
            }
            bool enemyPortraitContext = mode == Mode.Battle && activeEnemy != null && activeEnemy.portrait == id;
            string cacheKey = id + (enemyPortraitContext ? "#enemy" : "#scene");
            if (!portraitCache.TryGetValue(cacheKey, out var sprite))
            {
                var texture = LoadPortraitTexture(id, enemyPortraitContext);
                if (texture == null)
                {
                    portraitPanel.gameObject.SetActive(false);
                    return;
                }
                sprite = CreateNormalizedPortraitSprite(id, texture, enemyPortraitContext);
                portraitCache[cacheKey] = sprite;
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
            if (IsMobileLayout())
            {
                if (IsBossPortrait(id))
                    Anchor(portraitPanel, 0.02f, 0.62f, 0.98f, 0.86f, 0, 0, 0, 0);
                else
                    Anchor(portraitPanel, 0.08f, 0.62f, 0.92f, 0.85f, 0, 0, 0, 0);
                return;
            }
            if (IsSideCommandLayout())
            {
                if (IsBossPortrait(id))
                    Anchor(portraitPanel, 0.14f, 0.405f, 0.745f, 0.865f, 0, 0, 0, 0);
                else
                    Anchor(portraitPanel, 0.22f, 0.375f, 0.735f, 0.845f, 0, 0, 0, 0);
                return;
            }
            if (IsBossPortrait(id))
            {
                Anchor(portraitPanel, 0.11f, 0.245f, 0.89f, 0.895f, 0, 0, 0, 0);
                return;
            }
            Anchor(portraitPanel, 0.22f, 0.390f, 0.78f, 0.870f, 0, 0, 0, 0);
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
        Texture2D LoadPortraitTexture(string id, bool enemyPortraitContext)
        {
            if (enemyPortraitContext)
            {
                var enemyTexture = Resources.Load<Texture2D>("AichiFantasy/Portraits/Enemies/" + id);
                if (enemyTexture != null)
                    return enemyTexture;
            }
            else if (!string.IsNullOrEmpty(id) && (id.StartsWith("event_") || id == "stage_route_attendant" || id == "stage_quarantine_clerk"))
            {
                var npcTexture = Resources.Load<Texture2D>("AichiFantasy/Portraits/NPC/" + id);
                if (npcTexture != null)
                    return npcTexture;
            }
            return Resources.Load<Texture2D>("AichiFantasy/Portraits/" + id);
        }
        Sprite CreateNormalizedPortraitSprite(string id, Texture2D texture, bool enemyPortraitContext)
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
                    return CreateFittedPortraitSprite(texture, pixels, minX, minY, width, height, enemyPortraitContext);
                }
            }
            catch (Exception)
            {
                // Some imported textures may not be readable yet; use the full sprite as a safe fallback.
            }
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        }
        Sprite CreateFittedPortraitSprite(Texture2D source, Color32[] sourcePixels, int sourceX, int sourceY, int sourceWidth, int sourceHeight, bool enemyPortraitContext)
        {
            const int canvasWidth = 768;
            const int canvasHeight = 1024;
            var fitted = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);
            var outPixels = new Color32[canvasWidth * canvasHeight];
            for (int i = 0; i < outPixels.Length; i++)
                outPixels[i] = new Color32(0, 0, 0, 0);

            float targetWidth = canvasWidth * 0.78f;
            float targetHeight = canvasHeight * 0.86f;
            float scale = Mathf.Min(targetWidth / Mathf.Max(1, sourceWidth), targetHeight / Mathf.Max(1, sourceHeight));
            int drawWidth = Mathf.Max(1, Mathf.RoundToInt(sourceWidth * scale));
            int drawHeight = Mathf.Max(1, Mathf.RoundToInt(sourceHeight * scale));
            float contentCenterX = enemyPortraitContext ? sourceWidth * 0.5f : EstimateAlphaCenterX(sourcePixels, source.width, sourceX, sourceY, sourceWidth, sourceHeight);
            int drawX = Mathf.RoundToInt(canvasWidth * 0.5f - contentCenterX * scale);
            int sidePadding = Mathf.RoundToInt(canvasWidth * 0.025f);
            drawX = Mathf.Clamp(drawX, sidePadding, canvasWidth - sidePadding - drawWidth);
            int bottomMargin = Mathf.RoundToInt(canvasHeight * 0.035f);
            int drawY = bottomMargin;
            drawY = Mathf.Clamp(drawY, 0, canvasHeight - drawHeight);

            for (int y = 0; y < drawHeight; y++)
            {
                float v = drawHeight <= 1 ? 0f : y / (float)(drawHeight - 1);
                int sy = Mathf.Clamp(sourceY + Mathf.RoundToInt(v * (sourceHeight - 1)), 0, source.height - 1);
                int sourceRow = sy * source.width;
                int outRow = (drawY + y) * canvasWidth;
                for (int x = 0; x < drawWidth; x++)
                {
                    float u = drawWidth <= 1 ? 0f : x / (float)(drawWidth - 1);
                    int sx = Mathf.Clamp(sourceX + Mathf.RoundToInt(u * (sourceWidth - 1)), 0, source.width - 1);
                    Color32 c = sourcePixels[sourceRow + sx];
                    if (c.a <= 6)
                        continue;
                    outPixels[outRow + drawX + x] = c;
                }
            }

            fitted.SetPixels32(outPixels);
            fitted.Apply(false, true);
            return Sprite.Create(fitted, new Rect(0, 0, canvasWidth, canvasHeight), new Vector2(0.5f, 0.5f), 100f);
        }
        float EstimateAlphaCenterX(Color32[] pixels, int textureWidth, int sourceX, int sourceY, int sourceWidth, int sourceHeight)
        {
            double weightedX = 0;
            double weight = 0;
            for (int y = 0; y < sourceHeight; y++)
            {
                int row = (sourceY + y) * textureWidth;
                for (int x = 0; x < sourceWidth; x++)
                {
                    byte alpha = pixels[row + sourceX + x].a;
                    if (alpha <= 18)
                        continue;
                    weight += alpha;
                    weightedX += x * alpha;
                }
            }
            if (weight <= 0)
                return sourceWidth * 0.5f;
            return (float)(weightedX / weight);
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
