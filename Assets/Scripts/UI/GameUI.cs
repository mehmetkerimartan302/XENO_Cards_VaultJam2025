using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("Phase Display")]
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI instructionText;
    
    [Header("Score Display")]
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI enemyScoreText;
    
    [Header("Buttons")]
    public Button skipSpellButton;
    public Button restartButton;
    
    [Header("Game Over")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onPhaseChanged.AddListener(OnPhaseChanged);
        }
        
        if (skipSpellButton != null)
            skipSpellButton.onClick.AddListener(OnSkipSpell);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestart);
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    void OnPhaseChanged(GamePhase phase)
    {
        UpdatePhaseUI(phase);
        
        if (skipSpellButton != null)
            skipSpellButton.gameObject.SetActive(phase == GamePhase.CastingSpells);
        
        if (phase == GamePhase.GameOver)
        {
            ShowGameOver();
        }
    }

    void UpdatePhaseUI(GamePhase phase)
    {
        if (phaseText == null) return;
        
        switch (phase)
        {
            case GamePhase.Dealing:
                phaseText.text = "Kartlar Dağıtılıyor...";
                if (instructionText) instructionText.text = "";
                break;
            case GamePhase.PlacingBiomes:
                phaseText.text = "BIOME YERLEŞTİRME";
                if (instructionText) instructionText.text = "Biome kartlarını tahtanıza sürükleyin";
                break;
            case GamePhase.PlacingCharacters:
                phaseText.text = "KARAKTER YERLEŞTİRME";
                if (instructionText) instructionText.text = "Karakterleri biome'ların üzerine yerleştirin";
                break;
            case GamePhase.CastingSpells:
                phaseText.text = "SPELL KULLANIMI";
                if (instructionText) instructionText.text = "Spell kartını kullanın veya geçin";
                break;
            case GamePhase.Combat:
                phaseText.text = "SAVAŞ!";
                if (instructionText) instructionText.text = "Karakterler savaşıyor...";
                break;
            case GamePhase.RoundEnd:
                phaseText.text = "ROUND BİTTİ";
                if (instructionText) instructionText.text = "Yeni tur hazırlanıyor...";
                break;
            case GamePhase.GameOver:
                phaseText.text = "OYUN BİTTİ";
                if (instructionText) instructionText.text = "";
                break;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        
        if (playerScoreText != null)
            playerScoreText.text = $"Oyuncu: {GameManager.Instance.playerScore}";
        
        if (enemyScoreText != null)
            enemyScoreText.text = $"Düşman: {GameManager.Instance.enemyScore}";
    }

    void OnSkipSpell()
    {
        GameManager.Instance?.SkipSpell();
    }

    void OnRestart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverText != null)
            {
                bool playerWon = true;
                foreach (var cell in BoardManager.Instance.GetEnemyCells())
                {
                    if (cell.HasCharacter())
                    {
                        playerWon = false;
                        break;
                    }
                }
                
                gameOverText.text = playerWon ? "KAZANDINIZ!" : "KAYBETTİNİZ!";
                gameOverText.color = playerWon ? Color.green : Color.red;
            }
        }
    }
}

