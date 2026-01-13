using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum GamePhase
{
    Dealing,
    PlacingBiomes,
    PlacingCharacters,
    CastingSpells,
    Combat,
    RoundEnd,
    StageComplete,
    GameOver
}

public enum GameStage
{
    Stage1,
    Stage2
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Game State")]
    public GamePhase currentPhase = GamePhase.Dealing;
    public GameStage currentStage = GameStage.Stage1;
    public int currentRound = 1;
    public int maxRounds = 3;
    public int playerScore = 0;
    public int enemyScore = 0;
    
    [Header("Events")]
    public UnityEvent<GamePhase> onPhaseChanged;
    public UnityEvent onGameOver;
    public UnityEvent<int> onRoundStart;
    public UnityEvent<GameStage> onStageChanged;

    [Header("Audio")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioClip menuMusic;
    public AudioClip stage1Music;
    public AudioClip stage2Music;

    [Header("SFX Clips")]
    public AudioClip cardPlaceSfx;
    public AudioClip collisionSfx;
    public AudioClip augmentSfx;
    public AudioClip biomePlaceSfx;
    
    private int biomesPlaced = 0;
    private int charactersPlaced = 0;
    private bool isOvertime = false;

    [Header("Pause Settings")]
    public GameObject pausePanel;
    private bool isPaused = false;

    void Awake()
    {
        Instance = this;
        
        if (onPhaseChanged == null)
            onPhaseChanged = new UnityEvent<GamePhase>();
        if (onGameOver == null)
            onGameOver = new UnityEvent();
        if (onRoundStart == null)
            onRoundStart = new UnityEvent<int>();
        if (onStageChanged == null)
            onStageChanged = new UnityEvent<GameStage>();
    }

    void Start()
    {
        if (CardZoomManager.Instance == null)
        {
            GameObject zoomMgr = new GameObject("CardZoomManager");
            zoomMgr.AddComponent<CardZoomManager>();
            Debug.Log("⚠️ GameManager automatically created CardZoomManager!");
        }

        UpdateMusic();
    }

    void Update()
    {
        
        if (currentPhase != GamePhase.GameOver && !isPaused)
        {
            if (Cursor.visible != true) Cursor.visible = true;
            if (Cursor.lockState != CursorLockMode.None) Cursor.lockState = CursorLockMode.None;
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            
            
            if (IntroManager.Instance != null && 
                !IntroManager.Instance.mainMenuPanel.activeSelf && 
                !IntroManager.Instance.cinematicPanel.activeSelf)
            {
                TogglePause();
            }
        }
    }

    public void TogglePause()
    {
        if (pausePanel == null) return;
        
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        pausePanel.SetActive(isPaused);
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }



    public void StartGame()
    {
        LaserBeam laser = FindAnyObjectByType<LaserBeam>();
        if (laser != null)
            laser.FireLaser();



        PhaseTextUpdater phaseText = FindAnyObjectByType<PhaseTextUpdater>(FindObjectsInactive.Include);
        if (phaseText != null)
            phaseText.ShowText();

        StageIndicator stageIndicator = FindAnyObjectByType<StageIndicator>(FindObjectsInactive.Include);
        if (stageIndicator != null)
            stageIndicator.Show();

        InputTutorial inputTut = FindAnyObjectByType<InputTutorial>(FindObjectsInactive.Include);
        if (inputTut != null)
            inputTut.gameObject.SetActive(true);

        currentStage = GameStage.Stage1;
        currentRound = 1;
        maxRounds = 3;
        playerScore = 0;
        enemyScore = 0;
        onStageChanged?.Invoke(currentStage);
        
        
        if (bgmSource != null) bgmSource.volume = 1f;
        UpdateMusic();
        
        StartNewRound();
    }

    public void UpdateMusic()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.GetComponent<AudioSource>();
            if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        }

        AudioClip targetClip = null;
        if (currentPhase == GamePhase.GameOver) targetClip = null; 
        else if (currentStage == GameStage.Stage1) targetClip = stage1Music;
        else if (currentStage == GameStage.Stage2) targetClip = stage2Music;

        
        if (targetClip == null && currentPhase != GamePhase.GameOver) targetClip = menuMusic;

        if (bgmSource.clip != targetClip)
        {
            bgmSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1f); 
            bgmSource.clip = targetClip;
            if (targetClip != null) bgmSource.Play();
            else bgmSource.Stop();
        }
        else if (targetClip != null && !bgmSource.isPlaying)
        {
            bgmSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            bgmSource.Play();
        }
    }

    public void FadeOutMusic(float duration)
    {
        StartCoroutine(AnimateFadeOut(duration));
    }

    private IEnumerator AnimateFadeOut(float duration)
    {
        if (bgmSource == null || !bgmSource.isPlaying) yield break;
        float startVolume = bgmSource.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        bgmSource.volume = 0f;
        bgmSource.Stop();
        bgmSource.volume = startVolume; 
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        
        sfxSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        sfxSource.PlayOneShot(clip, sfxSource.volume);
    }

    void StartNewRound()
    {
        biomesPlaced = 0;
        charactersPlaced = 0;
        
        Debug.Log($"=== STAGE {(currentStage == GameStage.Stage1 ? 1 : 2)} - ROUND {currentRound} ===");
        onRoundStart?.Invoke(currentRound);
        
        DeckManager.Instance?.DealCards();
        
        SimpleHandUI handUI = FindAnyObjectByType<SimpleHandUI>();
        if (handUI != null)
            handUI.RefreshHand();

        SetPhase(GamePhase.Dealing);
        EnemyAI.Instance?.PlaceEnemyBiomes();
    }

    public void OnEnemyBiomesPlaced()
    {
        SetPhase(GamePhase.PlacingBiomes);
    }

    public void OnPlayerBiomesComplete()
    {
        EnemyAI.Instance?.PlaceEnemyCharacters();
    }

    public void OnEnemyCharactersPlaced()
    {
        StartCoroutine(ConsolidateEnemyStats());
        SetPhase(GamePhase.PlacingCharacters);
    }
    
    System.Collections.IEnumerator ConsolidateEnemyStats()
    {
        yield return new WaitForSeconds(0.3f);
        
        var enemyCells = BoardManager.Instance.GetEnemyCells();
        foreach (var cell in enemyCells)
        {
            if (!cell.HasCharacter()) continue;
            
            foreach (Transform child in cell.transform)
            {
                if (child.name.StartsWith("StatsCanvas_"))
                {
                    var display = child.GetComponent<CardStatsDisplay>();
                    if (display != null)
                        StartCoroutine(display.ConsolidateStats());
                }
            }
        }
    }

    public void TransitionToNextRound()
    {
        StartCoroutine(RoundTransitionAnimation());
    }

    System.Collections.IEnumerator RoundTransitionAnimation()
    {
        yield return StartCoroutine(AnimateCardsExit());
        ClearBoard();
        yield return new WaitForSeconds(0.5f);
        StartNewRound();
    }

    System.Collections.IEnumerator AnimateCardsExit()
    {
        if (BoardManager.Instance == null) yield break;
        
        float duration = 0.5f;
        
        var playerCards = new System.Collections.Generic.List<Transform>();
        var enemyCards = new System.Collections.Generic.List<Transform>();
        
        foreach (var cell in BoardManager.Instance.GetPlayerCells())
        {
            foreach (Transform child in cell.transform)
            {
                if (child.name.StartsWith("Card_"))
                    playerCards.Add(child);
            }
        }
        
        foreach (var cell in BoardManager.Instance.GetEnemyCells())
        {
            foreach (Transform child in cell.transform)
            {
                if (child.name.StartsWith("Card_"))
                    enemyCards.Add(child);
            }
        }
        
        var playerStarts = new System.Collections.Generic.List<Vector3>();
        var enemyStarts = new System.Collections.Generic.List<Vector3>();
        var playerStartRots = new System.Collections.Generic.List<Quaternion>();
        var enemyStartRots = new System.Collections.Generic.List<Quaternion>();
        Quaternion flatRot = Quaternion.Euler(90, 0, 0);
        
        foreach (var card in playerCards)
        {
            playerStarts.Add(card.position);
            playerStartRots.Add(card.rotation);
            foreach(Transform child in card)
                if (child.name.StartsWith("StatsCanvas_")) child.gameObject.SetActive(false);
        }
        foreach (var card in enemyCards)
        {
            enemyStarts.Add(card.position);
            enemyStartRots.Add(card.rotation);
            foreach(Transform child in card)
                if (child.name.StartsWith("StatsCanvas_")) child.gameObject.SetActive(false);
        }
        
        float playerTargetX = -3.8f;
        float enemyTargetX = 3.8f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeT = t * t;
            
            for (int i = 0; i < playerCards.Count; i++)
            {
                if (playerCards[i] != null)
                {
                    Vector3 start = playerStarts[i];
                    Vector3 target = new Vector3(playerTargetX, start.y, start.z);
                    playerCards[i].position = Vector3.Lerp(start, target, easeT);
                    playerCards[i].rotation = Quaternion.Slerp(playerStartRots[i], flatRot, easeT);
                }
            }
            
            for (int i = 0; i < enemyCards.Count; i++)
            {
                if (enemyCards[i] != null)
                {
                    Vector3 start = enemyStarts[i];
                    Vector3 target = new Vector3(enemyTargetX, start.y, start.z);
                    enemyCards[i].position = Vector3.Lerp(start, target, easeT);
                    enemyCards[i].rotation = Quaternion.Slerp(enemyStartRots[i], flatRot, easeT);
                }
            }
            
            yield return null;
        }

        foreach(var c in playerCards) if(c != null) c.gameObject.SetActive(false);
        foreach(var c in enemyCards) if(c != null) c.gameObject.SetActive(false);
    }

    void ClearBoard()
    {
        if (BoardManager.Instance == null) return;
        
        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                BoardCell cell = BoardManager.Instance.GetCell(row, col);
                if (cell != null)
                {
                    for (int i = cell.transform.childCount - 1; i >= 0; i--)
                    {
                        Transform child = cell.transform.GetChild(i);
                        if (child.name.StartsWith("Card") || child.name.StartsWith("StatsCanvas"))
                            Destroy(child.gameObject);
                    }
                    cell.Clear();
                }
            }
        }
    }

    public void SetPhase(GamePhase phase)
    {
        currentPhase = phase;
        onPhaseChanged?.Invoke(phase);
        
        if (phase == GamePhase.PlacingBiomes)
            BoardManager.Instance?.HighlightPlayerCells(true);
        else
            BoardManager.Instance?.ClearHighlights();
    }

    public void OnBiomePlaced()
    {
        biomesPlaced++;
        if (biomesPlaced >= 3)
            OnPlayerBiomesComplete();
    }

    public void OnCharacterPlaced()
    {
        charactersPlaced++;
        if (charactersPlaced >= 3)
        {
            EnemyAI.Instance?.CastEnemySpells();
            SetPhase(GamePhase.CastingSpells);
        }
    }
    
    public void OnCardPickedUp(CardType cardType)
    {
        if (cardType == CardType.Biome)
            biomesPlaced = Mathf.Max(0, biomesPlaced - 1);
        else if (cardType == CardType.Character)
            charactersPlaced = Mathf.Max(0, charactersPlaced - 1);
    }

    public void OnSpellCast()
    {
        SetPhase(GamePhase.Combat);
        CombatManager.Instance?.ResolveCombat();
    }

    public void SkipSpell()
    {
        SetPhase(GamePhase.Combat);
        CombatManager.Instance?.ResolveCombat();
    }

    public void OnCombatEnd()
    {
        SetPhase(GamePhase.RoundEnd);
        
        StartCoroutine(ImmediatePrepareCardsForExit());
        
        bool roundWasDraw = isOvertime;
        if (isOvertime)
            isOvertime = false;
        
        
        if (!roundWasDraw)
        {
            currentRound++;
        }
        else
        {
            Debug.Log("Round DRAW! Replaying...");
            Invoke(nameof(TransitionToNextRound), 1.2f);
            return;
        }

        int requiredWins = (maxRounds / 2) + 1;
        if (playerScore >= requiredWins || enemyScore >= requiredWins)
        {
            CheckStageEnd();
        }
        else
        {
            Invoke(nameof(TransitionToNextRound), 1.2f);
        }
    }

    System.Collections.IEnumerator ImmediatePrepareCardsForExit()
    {
        if (BoardManager.Instance == null) yield break;
        
        var allCards = new System.Collections.Generic.List<Transform>();
        var startPositions = new System.Collections.Generic.List<Vector3>();
        var cells = new System.Collections.Generic.List<BoardCell>();
        cells.AddRange(BoardManager.Instance.GetPlayerCells());
        cells.AddRange(BoardManager.Instance.GetEnemyCells());

        foreach (var cell in cells)
        {
            foreach (Transform child in cell.transform)
            {
                if (child.name.StartsWith("Card_"))
                {
                    allCards.Add(child);
                    startPositions.Add(child.position);
                }
            }
        }

        Quaternion flatRot = Quaternion.Euler(90, 0, 0);
        float duration = 0.4f;
        float elapsed = 0f;

        foreach (var card in allCards)
        {
            foreach (Transform child in card)
                if (child.name.StartsWith("StatsCanvas_")) child.gameObject.SetActive(false);
            
            var visual = card.GetComponent<Card3DVisual>();
            if (visual != null) visual.isExiting = true;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeT = t * (2 - t);

            for (int i = 0; i < allCards.Count; i++)
            {
                if (allCards[i] != null)
                {
                    allCards[i].rotation = Quaternion.Slerp(allCards[i].rotation, flatRot, easeT);
                    allCards[i].position = Vector3.Lerp(startPositions[i], startPositions[i] + Vector3.down * 0.4f, easeT);
                }
            }
            yield return null;
        }
    }

    void CheckStageEnd()
    {
        int requiredWins = (maxRounds / 2) + 1;
        bool playerPassed = playerScore >= requiredWins;
        
        if (currentStage == GameStage.Stage1)
        {
            if (playerPassed)
            {
                StartStage2();
            }
            else
            {
                StartCoroutine(GameOverSequence());
            }
        }
        else
        {
            StartCoroutine(GameOverSequence());
        }
    }

    void StartStage2()
    {
        currentStage = GameStage.Stage2;
        currentRound = 1;
        maxRounds = 5;
        playerScore = 0;
        enemyScore = 0;
        
        DeckManager.Instance?.AddCavalryToDeck();
        
        Debug.Log("=== STAGE 2 BAŞLADI! Süvari kartı desteye eklendi! ===");
        onStageChanged?.Invoke(currentStage);
        UpdateMusic();
        
        StartCoroutine(Stage2TransitionAnimation());
    }

    System.Collections.IEnumerator Stage2TransitionAnimation()
    {
        yield return StartCoroutine(AnimateCardsExit());
        ClearBoard();
        yield return new WaitForSeconds(1f);
        StartNewRound();
    }
    
    public void TriggerOvertime()
    {
        
        isOvertime = true;
        
    }

    System.Collections.IEnumerator GameOverSequence()
    {
        yield return StartCoroutine(AnimateCardsExit());
        ClearBoard();
        EndGame();
    }

    void CalculateRoundScore()
    {
        int playerAlive = 0;
        int enemyAlive = 0;
        int playerTotalHealth = 0;
        int enemyTotalHealth = 0;
        
        foreach (var cell in BoardManager.Instance.GetPlayerCells())
        {
            if (cell.HasCharacter())
            {
                playerAlive++;
                playerTotalHealth += cell.currentHealth;
            }
        }
        
        foreach (var cell in BoardManager.Instance.GetEnemyCells())
        {
            if (cell.HasCharacter())
            {
                enemyAlive++;
                enemyTotalHealth += cell.currentHealth;
            }
        }
        
        playerScore += playerAlive * 10 + playerTotalHealth;
        enemyScore += enemyAlive * 10 + enemyTotalHealth;
    }

    void EndGame()
    {
        SetPhase(GamePhase.GameOver);
        
        GameTimer timer = FindAnyObjectByType<GameTimer>();
        if (timer != null)
            timer.StopTimer();
        
        bool playerWon = playerScore > enemyScore;
        string stageText = currentStage == GameStage.Stage1 ? "Stage 1" : "Stage 2";
        Debug.Log($"=== OYUN BİTTİ! {stageText} - {(playerWon ? "KAZANDINIZ!" : "KAYBETTİNİZ!")} ===");
        
        LaserBeam laser = FindAnyObjectByType<LaserBeam>();
        
        if (playerWon && currentStage == GameStage.Stage2)
        {
            if (laser != null)
                laser.DisappearLaser();
            
            StartCoroutine(ShowVictoryScreen());
        }
        else if (!playerWon)
        {
            if (laser != null)
                laser.ImpactImmediately();
            
            StartCoroutine(ShowDefeatScreen());
        }
        
        onGameOver?.Invoke();
    }
    
    public void OnTimerExpired()
    {
        SetPhase(GamePhase.GameOver);
        
        LaserBeam laser = FindAnyObjectByType<LaserBeam>();
        if (laser != null)
            laser.ImpactImmediately();
        
        StartCoroutine(CameraShake(1.5f, 0.3f));
        StartCoroutine(ShowDefeatScreen());
        onGameOver?.Invoke();
    }

    public IEnumerator CameraShake(float duration, float magnitude)
    {
        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            Camera.main.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.localPosition = originalPos;
    }
    
    System.Collections.IEnumerator ShowVictoryScreen()
    {
        yield return new WaitForSeconds(2f);
        
        GameObject victoryUI = new GameObject("VictoryScreen");
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
            victoryUI.transform.SetParent(canvas.transform, false);
        
        UnityEngine.UI.Image bg = victoryUI.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        
        GameObject textObj = new GameObject("VictoryText");
        textObj.transform.SetParent(victoryUI.transform, false);
        TMPro.TextMeshProUGUI tmp = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = "YOU SAVED THE EARTH!\nThe laser has been stopped!";
        tmp.fontSize = 72;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.green;
        RectTransform textRt = tmp.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.5f, 0.6f);
        textRt.anchorMax = new Vector2(0.5f, 0.6f);
        textRt.sizeDelta = new Vector2(1200, 200);
        
        yield return new WaitForSeconds(4f);
        
        
        float fadeTime = 1f;
        float elapsedF = 0f;
        CanvasGroup victoryGroup = victoryUI.GetComponent<CanvasGroup>();
        if (victoryGroup == null) victoryGroup = victoryUI.AddComponent<CanvasGroup>();
        
        while (elapsedF < fadeTime)
        {
            elapsedF += Time.deltaTime;
            victoryGroup.alpha = 1f - (elapsedF / fadeTime);
            yield return null;
        }
        
        victoryUI.SetActive(false);
        StartCoroutine(ShowCredits(canvas.gameObject));
    }
    
    System.Collections.IEnumerator ShowDefeatScreen()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(CameraShake(2.0f, 0.4f));
        yield return new WaitForSeconds(2f);
        
        GameObject defeatUI = new GameObject("DefeatScreen");
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
            defeatUI.transform.SetParent(canvas.transform, false);
        
        UnityEngine.UI.Image bg = defeatUI.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0.2f, 0, 0, 0.9f);
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        
        GameObject textObj = new GameObject("DefeatText");
        textObj.transform.SetParent(defeatUI.transform, false);
        TMPro.TextMeshProUGUI tmp = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = "EARTH DESTROYED\nThe laser reached its target...";
        tmp.fontSize = 72;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.white; 
        tmp.fontStyle = TMPro.FontStyles.Bold;
        RectTransform textRt = tmp.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.5f, 0.5f);
        textRt.anchorMax = new Vector2(0.5f, 0.5f);
        textRt.sizeDelta = new Vector2(1200, 200);

        yield return new WaitForSeconds(4f);

        
        float fadeTime = 1f;
        float elapsedF = 0f;
        CanvasGroup defeatGroup = defeatUI.GetComponent<CanvasGroup>();
        if (defeatGroup == null) defeatGroup = defeatUI.AddComponent<CanvasGroup>();
        
        while (elapsedF < fadeTime)
        {
            elapsedF += Time.deltaTime;
            defeatGroup.alpha = 1f - (elapsedF / fadeTime);
            yield return null;
        }
        
        defeatUI.SetActive(false);
        StartCoroutine(ShowCredits(canvas.gameObject));
    }
    
    System.Collections.IEnumerator ShowCredits(GameObject parent)
    {
        GameObject creditsPanel = new GameObject("CreditsPanel");
        creditsPanel.transform.SetParent(parent.transform, false);
        RectTransform cpRt = creditsPanel.AddComponent<RectTransform>();
        cpRt.anchorMin = Vector2.zero; cpRt.anchorMax = Vector2.one; cpRt.sizeDelta = Vector2.zero;
        
        
        Image bg = creditsPanel.AddComponent<Image>();
        bg.color = Color.black;
        CanvasGroup cg = creditsPanel.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        
        GameObject creditsObj = new GameObject("CreditsText");
        creditsObj.transform.SetParent(creditsPanel.transform, false);
        
        TMPro.TextMeshProUGUI credits = creditsObj.AddComponent<TMPro.TextMeshProUGUI>();
        credits.text = "CREDITS\n\n" +
                       "DEVELOPERS\nMehmet Kerim Artan\nBerke Yaşar\n\n" +
                       "DESIGNERS\nDuygu İrem El (2D Artist)\nErkan Berke Aksoy (3D Artist)\n\n" +
                       "Thank you for playing!";
        credits.fontSize = 36;
        credits.alignment = TMPro.TextAlignmentOptions.Center;
        credits.color = Color.white;
        
        RectTransform rt = credits.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(1000, 800);
        rt.anchoredPosition = new Vector2(0, -800);
        
        
        float fTime = 1.5f;
        float fElapsed = 0f;
        while (fElapsed < fTime)
        {
            fElapsed += Time.deltaTime;
            cg.alpha = fElapsed / fTime;
            yield return null;
        }
        cg.alpha = 1f;
        
        float scrollDuration = 12f;
        float elapsedScroll = 0f;
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = new Vector2(0, 1000);
        
        while (elapsedScroll < scrollDuration)
        {
            elapsedScroll += Time.deltaTime;
            float t = elapsedScroll / scrollDuration;
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        ReturnToMainMenu();
    }
}

