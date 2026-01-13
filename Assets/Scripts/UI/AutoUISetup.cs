using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class AutoUISetup : MonoBehaviour
{
    public Sprite[] comicPanels;
    public AudioClip cinematicAudio;
    public AudioClip cinematicAudio2;

    [Header("Background Music")]
    public AudioClip menuMusic;
    public AudioClip stage1Music;
    public AudioClip stage2Music;

    [Header("Main Menu")]
    public Sprite mainMenuBackground;
    
    [Header("SFX Clips")]
    public AudioClip cardPlaceSfx;
    public AudioClip collisionSfx;
    public AudioClip augmentSfx;
    public AudioClip biomePlaceSfx;

    void Start()
    {
        SetupUI();
    }

    void SetupUI()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("GameCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        SetupIntroUI(canvas);
        SetupHandUI(canvas);
        SetupPhaseText(canvas);
        SetupTimer(canvas);
        SetupStageIndicator(canvas);
        SetupInputTutorial(canvas);

        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.menuMusic = menuMusic;
            GameManager.Instance.stage1Music = stage1Music;
            GameManager.Instance.stage2Music = stage2Music;
            
            GameManager.Instance.cardPlaceSfx = cardPlaceSfx;
            GameManager.Instance.collisionSfx = collisionSfx;
            GameManager.Instance.augmentSfx = augmentSfx;
            GameManager.Instance.biomePlaceSfx = biomePlaceSfx;
            
            GameManager.Instance.UpdateMusic(); 
        }
    }

    void SetupHandUI(Canvas canvas)
    {
        if (FindAnyObjectByType<SimpleHandUI>() != null)
        {
            Debug.Log("SimpleHandUI zaten var, atlanıyor.");
            return;
        }
        
        GameObject handPanel = new GameObject("HandPanel");
        handPanel.transform.SetParent(canvas.transform);
        
        RectTransform rt = handPanel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 20);
        rt.sizeDelta = new Vector2(0, 160);
        
        Image bg = handPanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.5f);
        
        GameObject cardContainer = new GameObject("CardContainer");
        cardContainer.transform.SetParent(handPanel.transform);
        
        RectTransform containerRt = cardContainer.AddComponent<RectTransform>();
        containerRt.anchorMin = new Vector2(0.5f, 0.5f);
        containerRt.anchorMax = new Vector2(0.5f, 0.5f);
        containerRt.pivot = new Vector2(0.5f, 0.5f);
        containerRt.anchoredPosition = Vector2.zero;
        containerRt.sizeDelta = new Vector2(600, 140);
        
        SimpleHandUI handUI = handPanel.AddComponent<SimpleHandUI>();
        handUI.cardContainer = cardContainer.transform;
        
        Debug.Log("UI kurulumu tamamlandı!");
    }

    void SetupIntroUI(Canvas canvas)
    {
        if (FindAnyObjectByType<IntroManager>() != null) return;
        
        GameObject introObj = new GameObject("IntroManager");
        IntroManager intro = introObj.AddComponent<IntroManager>();
        
        GameObject menuPanel = new GameObject("MainMenuPanel");
        menuPanel.transform.SetParent(canvas.transform, false);
        RectTransform menuRt = menuPanel.AddComponent<RectTransform>();
        menuRt.anchorMin = Vector2.zero; menuRt.anchorMax = Vector2.one; menuRt.sizeDelta = Vector2.zero;
        Image menuBg = menuPanel.AddComponent<Image>();
        
        if (mainMenuBackground != null)
        {
            menuBg.sprite = mainMenuBackground;
            menuBg.color = Color.white;
        }
        else
        {
            menuBg.color = new Color(0.1f, 0.1f, 0.15f, 1f);
        }

        
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(menuPanel.transform, false);
        TMPro.TextMeshProUGUI titleText = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
        titleText.text = "XENO CARDS";
        titleText.fontSize = 120;
        titleText.fontStyle = TMPro.FontStyles.Bold | TMPro.FontStyles.Italic;
        titleText.color = new Color(0.3f, 0.8f, 1f, 1f); 
        titleText.alignment = TMPro.TextAlignmentOptions.Left;
        
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 0.5f);
        titleRt.anchorMax = new Vector2(0, 0.5f);
        titleRt.pivot = new Vector2(0, 0.5f);
        titleRt.anchoredPosition = new Vector2(100, 50); 
        titleRt.sizeDelta = new Vector2(800, 200);

        
        float buttonSpacing = 90f;
        Vector2 buttonSize = new Vector2(320, 70);
        float startX = 100f;
        float startY = -150f; 

        GameObject startBtnObj = CreateButton(menuPanel.transform, "StartButton", "START GAME", new Vector2(startX, startY));
        StyleButton(startBtnObj, buttonSize, true);
        
        Button startBtn = startBtnObj.GetComponent<Button>();
        startBtn.onClick.AddListener(() => IntroManager.Instance.StartGameClicked());
        
        GameObject settingsBtnObj = CreateButton(menuPanel.transform, "SettingsButton", "SETTINGS", new Vector2(startX, startY - buttonSpacing));
        StyleButton(settingsBtnObj, buttonSize, true);

        Button settingsBtn = settingsBtnObj.GetComponent<Button>();
        settingsBtn.onClick.AddListener(() => IntroManager.Instance.OpenSettings());
        
        GameObject quitBtnObj = CreateButton(menuPanel.transform, "QuitButton", "QUIT", new Vector2(startX, startY - buttonSpacing * 2));
        StyleButton(quitBtnObj, buttonSize, true);

        Button quitBtn = quitBtnObj.GetComponent<Button>();
        quitBtn.onClick.AddListener(() => IntroManager.Instance.QuitGame());
        
        GameObject settingsPanel = new GameObject("SettingsPanel");
        settingsPanel.transform.SetParent(canvas.transform, false);
        intro.settingsPanel = settingsPanel;
        
        RectTransform settingsRtP = settingsPanel.AddComponent<RectTransform>();
        settingsRtP.sizeDelta = new Vector2(600, 600); 
        settingsRtP.anchoredPosition = Vector2.zero;
        
        Image settingsBg = settingsPanel.AddComponent<Image>();
        settingsBg.color = new Color(0.05f, 0.05f, 0.08f, 0.98f); 
        
        
        GameObject sBorder = new GameObject("Border");
        sBorder.transform.SetParent(settingsPanel.transform, false);
        Image sbImg = sBorder.AddComponent<Image>();
        sbImg.color = new Color(0.3f, 0.8f, 1f, 0.4f); 
        RectTransform sbRt = sBorder.GetComponent<RectTransform>();
        sbRt.anchorMin = Vector2.zero; sbRt.anchorMax = Vector2.one; sbRt.sizeDelta = new Vector2(4, 4);
        
        SetupSettingsContent(settingsPanel, intro);
        
        settingsPanel.SetActive(false);

        GameObject cinematicPanel = new GameObject("CinematicPanel");
        cinematicPanel.transform.SetParent(canvas.transform, false);
        RectTransform cinRt = cinematicPanel.AddComponent<RectTransform>();
        cinRt.anchorMin = Vector2.zero; cinRt.anchorMax = Vector2.one; cinRt.sizeDelta = Vector2.zero;
        Image cinBg = cinematicPanel.AddComponent<Image>();
        cinBg.color = Color.white;
        cinematicPanel.AddComponent<Mask>();
        
        GameObject comicObj = new GameObject("ComicStrip");
        comicObj.transform.SetParent(cinematicPanel.transform, false);
        RectTransform comicRt = comicObj.AddComponent<RectTransform>();
        comicRt.anchorMin = Vector2.zero; comicRt.anchorMax = Vector2.one;
        comicRt.sizeDelta = Vector2.zero;
        comicRt.anchoredPosition = Vector2.zero;
        Image comicImg = comicObj.AddComponent<Image>();
        comicImg.color = Color.white;
        comicImg.preserveAspect = true;
        
        intro.comicPanels = comicPanels;
        intro.cinematicAudio = cinematicAudio;
        intro.cinematicAudio2 = cinematicAudio2;
        
        GameObject skipBtnObj = CreateButton(cinematicPanel.transform, "SkipButton", "SKIP >>", Vector2.zero);
        RectTransform skipRt = skipBtnObj.GetComponent<RectTransform>();
        skipRt.anchorMin = Vector2.zero;
        skipRt.anchorMax = Vector2.zero;
        skipRt.pivot = Vector2.zero;
        skipRt.anchoredPosition = new Vector2(40, 40);
        StyleButton(skipBtnObj, new Vector2(140, 45));
        
        intro.skipButton = skipBtnObj.GetComponent<Button>();
        cinematicPanel.SetActive(false);
        
        GameObject fadeObj = new GameObject("FadeOverlay");
        fadeObj.transform.SetParent(canvas.transform, false);
        RectTransform fadeRt = fadeObj.AddComponent<RectTransform>();
        fadeRt.anchorMin = Vector2.zero; fadeRt.anchorMax = Vector2.one; fadeRt.sizeDelta = Vector2.zero;
        Image fadeImg = fadeObj.AddComponent<Image>();
        fadeImg.color = Color.black;
        fadeImg.raycastTarget = false;
        CanvasGroup fadeGroup = fadeObj.AddComponent<CanvasGroup>();
        
        intro.mainMenuPanel = menuPanel;
        intro.cinematicPanel = cinematicPanel;
        intro.comicStrip = comicRt;
        intro.fadeOverlay = fadeGroup;

        SetupPauseMenu(canvas);
    }

    void SetupPauseMenu(Canvas canvas)
    {
        GameObject pausePanel = new GameObject("PausePanel");
        pausePanel.transform.SetParent(canvas.transform, false);
        
        RectTransform rt = pausePanel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
        
        Image bg = pausePanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.7f);
        
        
        GameObject content = new GameObject("Content");
        content.transform.SetParent(pausePanel.transform, false);
        RectTransform contentRt = content.AddComponent<RectTransform>();
        contentRt.sizeDelta = new Vector2(600, 700);
        Image contentBg = content.AddComponent<Image>();
        contentBg.color = new Color(0.05f, 0.05f, 0.08f, 0.98f); 
        
        
        GameObject border = new GameObject("Border");
        border.transform.SetParent(content.transform, false);
        Image bImg = border.AddComponent<Image>();
        bImg.color = new Color(0.3f, 0.8f, 1f, 0.4f); 
        RectTransform bRt = border.GetComponent<RectTransform>();
        bRt.anchorMin = Vector2.zero; bRt.anchorMax = Vector2.one; bRt.sizeDelta = new Vector2(4, 4);

        
        SetupSettingsContent(content, IntroManager.Instance, false); 
        
        
        GameObject tutorialBtnObj = CreateButton(content.transform, "TutorialButton", "HOW TO PLAY", new Vector2(0, -100));
        StyleButton(tutorialBtnObj, new Vector2(300, 60));
        
        
        GameObject menuBtnObj = CreateButton(content.transform, "MainMenuButton", "RETURN TO MAIN MENU", new Vector2(0, -190));
        StyleButton(menuBtnObj, new Vector2(400, 60));
        menuBtnObj.GetComponent<Button>().onClick.AddListener(() => GameManager.Instance.ReturnToMainMenu());
        
        
        GameObject resumeBtnObj = CreateButton(content.transform, "ResumeButton", "RESUME", new Vector2(0, -280));
        StyleButton(resumeBtnObj, new Vector2(250, 60));
        resumeBtnObj.GetComponent<Button>().onClick.AddListener(() => GameManager.Instance.TogglePause());
        
        
        foreach(var t in content.GetComponentsInChildren<TMPro.TextMeshProUGUI>()) t.color = Color.black; 

        
        GameObject tutorialPanel = new GameObject("TutorialPanel");
        tutorialPanel.transform.SetParent(pausePanel.transform, false);
        RectTransform tutRt = tutorialPanel.AddComponent<RectTransform>();
        tutRt.sizeDelta = new Vector2(550, 650);
        Image tutBg = tutorialPanel.AddComponent<Image>();
        tutBg.color = new Color(0.05f, 0.05f, 0.08f, 0.99f); 
        
        
        GameObject tutBorder = new GameObject("Border");
        tutBorder.transform.SetParent(tutorialPanel.transform, false);
        Image tbImg = tutBorder.AddComponent<Image>();
        tbImg.color = new Color(0.3f, 0.8f, 1f, 0.6f);
        RectTransform tbRt = tutBorder.GetComponent<RectTransform>();
        tbRt.anchorMin = Vector2.zero; tbRt.anchorMax = Vector2.one; tbRt.sizeDelta = new Vector2(6, 6);

        GameObject tutTextObj = new GameObject("Text");
        tutTextObj.transform.SetParent(tutorialPanel.transform, false);
        TMPro.TextMeshProUGUI tutText = tutTextObj.AddComponent<TMPro.TextMeshProUGUI>();
        tutText.text = "<b>HOW TO PLAY</b>\n\n" +
                       "1. BIOMES: Place territories first.\n\n" +
                       "2. CHARACTERS: Deploy units on biomes.\n\n" +
                       "3. SPELLS: Cast magic to buff/heal.\n\n" +
                       "4. COMBAT: Units clash automatically!\n\n" +
                       "<b>TIP: Left-Click any card to zoom.</b>";
        tutText.fontSize = 28;
        tutText.alignment = TMPro.TextAlignmentOptions.Center;
        tutText.color = Color.black; 
        RectTransform ttRt = tutTextObj.GetComponent<RectTransform>();
        ttRt.anchorMin = Vector2.zero; ttRt.anchorMax = Vector2.one; ttRt.sizeDelta = new Vector2(-60, -60);

        tutorialPanel.SetActive(false);
        tutorialBtnObj.GetComponent<Button>().onClick.AddListener(() => tutorialPanel.SetActive(true));
        
        
        GameObject closeTutBtn = CreateButton(tutorialPanel.transform, "CloseTut", "GOT IT", new Vector2(0, -220));
        StyleButton(closeTutBtn, new Vector2(200, 60));
        closeTutBtn.GetComponent<Button>().onClick.AddListener(() => tutorialPanel.SetActive(false));

        pausePanel.SetActive(false);
        if (GameManager.Instance != null) GameManager.Instance.pausePanel = pausePanel;
    }

    void StyleButton(GameObject btnObj, Vector2 size, bool leftAlign = false)
    {
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        
        if (leftAlign)
        {
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
        }
        
        Image img = btnObj.GetComponent<Image>();
        if (img == null) img = btnObj.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.08f); 
        
        
        GameObject border = new GameObject("Border");
        border.transform.SetParent(btnObj.transform, false);
        Image borderImg = border.AddComponent<Image>();
        borderImg.color = new Color(0.3f, 0.8f, 1f, 0.4f); 
        
        RectTransform borderRt = border.GetComponent<RectTransform>();
        borderRt.anchorMin = Vector2.zero;
        borderRt.anchorMax = Vector2.one;
        borderRt.sizeDelta = new Vector2(2, 2); 
        
        
        TMPro.TextMeshProUGUI tmp = btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (tmp == null)
        {
            Text oldText = btnObj.GetComponentInChildren<Text>();
            string content = oldText != null ? oldText.text : "BUTTON";
            if (oldText != null) Destroy(oldText.gameObject);
            
            GameObject txtObj = new GameObject("TextTMP");
            txtObj.transform.SetParent(btnObj.transform, false);
            tmp = txtObj.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = content;
            RectTransform txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.sizeDelta = Vector2.zero;
        }

        tmp.fontSize = 24;
        tmp.fontStyle = TMPro.FontStyles.Bold;
        tmp.alignment = leftAlign ? TMPro.TextAlignmentOptions.Left : TMPro.TextAlignmentOptions.Center;
        if (leftAlign)
        {
            tmp.margin = new Vector4(20, 0, 0, 0); 
            rt.pivot = new Vector2(0, 0.5f);
        }
        tmp.color = Color.black; 
    }

    void SetupSettingsContent(GameObject panel, IntroManager intro, bool showCloseButton = true)
    {
        
        CreateLabeledSlider(panel, "MusicSlider", "MUSIC VOLUME", new Vector2(0, 200), (val) => intro.SetMusicVolume(val), PlayerPrefs.GetFloat("MusicVolume", 1f), intro, true);
        
        
        CreateLabeledSlider(panel, "SFXSlider", "SFX VOLUME", new Vector2(0, 100), (val) => intro.SetSFXVolume(val), PlayerPrefs.GetFloat("SFXVolume", 1f), intro, false);

        if (showCloseButton)
        {
            
            GameObject closeBtnObj = CreateButton(panel.transform, "CloseButton", "CLOSE", new Vector2(0, -120));
            StyleButton(closeBtnObj, new Vector2(180, 50));
            Button closeBtn = closeBtnObj.GetComponent<Button>();
            closeBtn.onClick.AddListener(() => intro.CloseSettings());
        }
    }

    void CreateLabeledSlider(GameObject panel, string name, string labelText, Vector2 pos, UnityEngine.Events.UnityAction<float> onValueChanged, float initialValue, IntroManager intro, bool isMusic)
    {
        
        GameObject labelObj = new GameObject(name + "Label");
        labelObj.transform.SetParent(panel.transform, false);
        TMPro.TextMeshProUGUI label = labelObj.AddComponent<TMPro.TextMeshProUGUI>();
        label.text = labelText;
        label.fontSize = 24;
        label.alignment = TMPro.TextAlignmentOptions.Center;
        label.color = Color.black; 
        
        RectTransform labelRt = labelObj.GetComponent<RectTransform>();
        labelRt.anchoredPosition = pos + new Vector2(0, 50);
        labelRt.sizeDelta = new Vector2(400, 40);

        
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(panel.transform, false);
        Slider slider = sliderObj.AddComponent<Slider>();
        if (isMusic) intro.musicSlider = slider;
        else intro.sfxSlider = slider;
        
        slider.value = initialValue;
        slider.onValueChanged.AddListener(onValueChanged);
        
        RectTransform sliderRt = sliderObj.GetComponent<RectTransform>();
        sliderRt.sizeDelta = new Vector2(400, 20);
        sliderRt.anchoredPosition = pos;

        
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.2f, 0.3f, 1f); 
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.sizeDelta = Vector2.zero;

        
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform faRt = fillArea.AddComponent<RectTransform>();
        faRt.anchorMin = Vector2.zero; faRt.anchorMax = Vector2.one; faRt.sizeDelta = new Vector2(-20, 0);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = Color.white; 
        RectTransform fRt = fill.GetComponent<RectTransform>();
        fRt.sizeDelta = Vector2.zero;
        slider.fillRect = fRt;

        
        GameObject handleArea = new GameObject("Handle Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform haRt = handleArea.AddComponent<RectTransform>();
        haRt.anchorMin = Vector2.zero; haRt.anchorMax = Vector2.one; haRt.sizeDelta = new Vector2(-20, 0);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        Image hImg = handle.AddComponent<Image>();
        hImg.color = Color.white; 
        RectTransform hRt = handle.GetComponent<RectTransform>();
        hRt.sizeDelta = new Vector2(25, 35);
        slider.handleRect = hRt;
    }

    GameObject CreateButton(Transform parent, string name, string label, Vector2 pos)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 50);
        rt.anchoredPosition = pos;
        
        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.4f);
        Button btn = btnObj.AddComponent<Button>();
        
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        Text t = txtObj.AddComponent<Text>();
        t.text = label;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.sizeDelta = Vector2.zero;
        
        return btnObj;
    }

    void SetupPhaseText(Canvas canvas)
    {
        if (FindAnyObjectByType<PhaseTextUpdater>() != null) return;
        
        GameObject phaseObj = new GameObject("PhaseText");
        phaseObj.transform.SetParent(canvas.transform, false);
        
        RectTransform rt = phaseObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -20);
        rt.sizeDelta = new Vector2(400, 50);
        
        Text text = phaseObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        
        phaseObj.AddComponent<PhaseTextUpdater>();
    }

    void SetupTimer(Canvas canvas)
    {
        if (FindAnyObjectByType<GameTimer>() != null) return;
        
        GameObject timerObj = new GameObject("GameTimer");
        timerObj.transform.SetParent(canvas.transform, false);
        
        RectTransform rt = timerObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-30, -30);
        rt.sizeDelta = new Vector2(200, 60);
        
        TMPro.TextMeshProUGUI timerText = timerObj.AddComponent<TMPro.TextMeshProUGUI>();
        timerText.text = "10:00";
        timerText.fontSize = 48;
        timerText.fontStyle = TMPro.FontStyles.Bold;
        timerText.alignment = TMPro.TextAlignmentOptions.Right;
        timerText.color = Color.white;
        
        GameTimer timer = timerObj.AddComponent<GameTimer>();
        timer.timerText = timerText;
        timer.totalTime = 600f;
        
        timerObj.SetActive(false);
    }
    
    void SetupStageIndicator(Canvas canvas)
    {
        GameObject stageObj = new GameObject("StageIndicator");
        stageObj.transform.SetParent(canvas.transform, false);
        
        RectTransform rt = stageObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(30, -30);
        rt.sizeDelta = new Vector2(500, 100);
        
        TMPro.TextMeshProUGUI stageText = stageObj.AddComponent<TMPro.TextMeshProUGUI>();
        stageText.text = "STAGE 1 - BEST OF 3\n0 - 0";
        stageText.fontSize = 28;
        stageText.fontStyle = TMPro.FontStyles.Bold;
        stageText.alignment = TMPro.TextAlignmentOptions.Left;
        stageText.color = Color.cyan;
        stageText.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        
        stageObj.AddComponent<StageIndicator>();
        stageObj.SetActive(false);
    }

    void SetupInputTutorial(Canvas canvas)
    {
        GameObject tutObj = new GameObject("InputTutorial");
        tutObj.transform.SetParent(canvas.transform, false);
        
        RectTransform rt = tutObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.75f);
        rt.anchorMax = new Vector2(0, 0.75f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = new Vector2(40, 0);
        rt.sizeDelta = new Vector2(400, 150);
        
        TMPro.TextMeshProUGUI tmp = tutObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.fontSize = 28;
        tmp.alignment = TMPro.TextAlignmentOptions.Left;
        tmp.color = new Color(1f, 0.55f, 0f); 
        tmp.fontStyle = TMPro.FontStyles.Bold;
        
        tutObj.AddComponent<InputTutorial>();
        tutObj.SetActive(false); 
    }
}

public class PhaseTextUpdater : MonoBehaviour
{
    private Text text;
    
    void Start()
    {
        text = GetComponent<Text>();
        text.text = "";
        gameObject.SetActive(false);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onPhaseChanged.AddListener(UpdateText);
        }
    }
    
    public void ShowText()
    {
        gameObject.SetActive(true);
    }

    void UpdateText(GamePhase phase)
    {
        if (text == null) return;
        
        switch (phase)
        {
            case GamePhase.PlacingBiomes:
                text.text = "🌲 PLACE BIOME CARDS";
                text.color = new Color(0.3f, 0.8f, 0.3f);
                break;
            case GamePhase.PlacingCharacters:
                text.text = "⚔️ PLACE CHARACTER CARDS";
                text.color = new Color(0.3f, 0.5f, 0.9f);
                break;
            case GamePhase.CastingSpells:
                text.text = "✨ CAST SPELLS";
                text.color = new Color(0.8f, 0.3f, 0.8f);
                break;
            case GamePhase.Combat:
                text.text = "⚔️ COMBAT!";
                text.color = Color.red;
                break;
            case GamePhase.RoundEnd:
                text.text = "🔄 ROUND END - NEW TURN...";
                text.color = Color.cyan;
                break;
            case GamePhase.GameOver:
                text.text = "🏆 GAME OVER";
                text.color = Color.yellow;
                break;
            default:
                text.text = phase.ToString();
                text.color = Color.white;
                break;
        }
    }
}

public class StageIndicator : MonoBehaviour
{
    private TMPro.TextMeshProUGUI text;
    
    void Awake()
    {
        text = GetComponent<TMPro.TextMeshProUGUI>();
    }
    
    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onStageChanged.AddListener(OnStageChanged);
            GameManager.Instance.onRoundStart.AddListener(OnRoundStart);
        }
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
        UpdateDisplay();
    }
    
    void OnStageChanged(GameStage stage)
    {
        UpdateDisplay();
    }
    
    void OnRoundStart(int round)
    {
        UpdateDisplay();
    }
    
    void UpdateDisplay()
    {
        if (GameManager.Instance == null || text == null) return;
        
        string stageName = GameManager.Instance.currentStage == GameStage.Stage1 ? "STAGE 1" : "STAGE 2";
        string boX = GameManager.Instance.currentStage == GameStage.Stage1 ? "BEST OF 3" : "BEST OF 5";
        int pScore = GameManager.Instance.playerScore;
        int eScore = GameManager.Instance.enemyScore;
        
        text.text = $"{stageName} - {boX}\nYOU {pScore} vs {eScore} ALIEN";
    }
}

public class InputTutorial : MonoBehaviour
{
    private TMPro.TextMeshProUGUI text;
    private bool leftClicked = false;
    private bool rightClicked = false;
    private bool isDone = false;
    
    void Start()
    {
        text = GetComponent<TMPro.TextMeshProUGUI>();
        text.text = "<color=#ff8c00><b>MOUSE CONTROLS:</b></color>\n" +
                   "LEFT CLICK: Zoom / Details\n" +
                   "RIGHT CLICK: Interact / Place\n" +
                   "<size=20>(Click both to dismiss)</size>";
    }
    
    void Update()
    {
        if (isDone) return;
        
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame) leftClicked = true;
        if (Mouse.current.rightButton.wasPressedThisFrame) rightClicked = true;
        
        if (leftClicked && rightClicked)
        {
            isDone = true;
            StartCoroutine(FadeOut());
        }
    }
    
    System.Collections.IEnumerator FadeOut()
    {
        float duration = 1f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            text.alpha = 1f - (elapsed / duration);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
