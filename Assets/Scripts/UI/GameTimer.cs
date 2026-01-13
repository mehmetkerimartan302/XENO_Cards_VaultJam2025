using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance;
    
    [Header("Timer Settings")]
    public float totalTime = 600f; 
    
    [Header("UI")]
    public TextMeshProUGUI timerText;
    
    private float remainingTime;
    private bool isRunning = false;
    private bool hasEnded = false;
    
    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        remainingTime = totalTime;
        UpdateTimerDisplay();
    }
    
    public void StartTimer()
    {
        gameObject.SetActive(true);
        remainingTime = totalTime;
        isRunning = true;
        UpdateTimerDisplay();
    }
    
    public void StopTimer()
    {
        isRunning = false;
    }
    
    void Update()
    {
        if (!isRunning || hasEnded) return;
        
        remainingTime -= Time.deltaTime;
        
        if (remainingTime <= 0)
        {
            remainingTime = 0;
            hasEnded = true;
            isRunning = false;
            OnTimeUp();
        }
        
        UpdateTimerDisplay();
    }
    
    void UpdateTimerDisplay()
    {
        if (timerText == null) return;
        
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        
        if (remainingTime <= 180f) 
            timerText.color = new Color(1f, 0f, 0f, 1f);
        else if (remainingTime <= 300f) 
            timerText.color = new Color(1f, 0.5f, 0f); 
        else
            timerText.color = Color.white;
    }
    
    void OnTimeUp()
    {
        Debug.Log("SÜRE BİTTİ! Lazer dünyaya ulaşıyor...");
        
        LaserBeam laser = FindAnyObjectByType<LaserBeam>();
        if (laser != null)
        {
            laser.ImpactEarth();
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerExpired();
        }
    }
    
    public float GetRemainingTime()
    {
        return remainingTime;
    }
}
