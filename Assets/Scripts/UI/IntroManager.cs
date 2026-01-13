using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class IntroManager : MonoBehaviour
{
    public static IntroManager Instance;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject cinematicPanel;
    public CanvasGroup fadeOverlay;

    [Header("Cinematic")]
    public RectTransform comicStrip;
    public Sprite[] comicPanels;
    public Button skipButton;
    public AudioClip cinematicAudio;
    public AudioClip cinematicAudio2;

    [Header("Settings Controls")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle fullscreenToggle;

    private bool isCinematicPlaying = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ShowMainMenu();
        if (fadeOverlay != null)
        {
            Image fImg = fadeOverlay.GetComponent<Image>();
            if (fImg != null) fImg.color = Color.black;
            fadeOverlay.alpha = 1;
            StartCoroutine(Fade(0, 1f));
        }
        
        if (skipButton != null)
            skipButton.onClick.AddListener(SkipCinematic);

        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }
    }

    public void SetMusicVolume(float value)
    {
        if (GameManager.Instance != null && GameManager.Instance.bgmSource != null)
            GameManager.Instance.bgmSource.volume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        if (GameManager.Instance != null && GameManager.Instance.sfxSource != null)
            GameManager.Instance.sfxSource.volume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void SetFullscreen(bool isFull)
    {
        Screen.fullScreen = isFull;
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        cinematicPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void StartGameClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.FadeOutMusic(1.5f);
            
        StartCoroutine(TransitionToCinematic());
    }

    IEnumerator TransitionToCinematic()
    {
        yield return StartCoroutine(Fade(1, 0.8f));
        mainMenuPanel.SetActive(false);
        cinematicPanel.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(Fade(0, 0.8f));
        yield return new WaitForSeconds(0.5f);
        
        StartCoroutine(PlayCinematic());
    }

    IEnumerator PlayCinematic()
    {
        isCinematicPlaying = true;
        
        if (comicStrip == null || comicPanels == null || comicPanels.Length == 0) 
        {
            StartGameplay();
            yield break;
        }

        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        
        Image comicImage = comicStrip.GetComponent<Image>();
        CanvasGroup comicGroup = comicStrip.GetComponent<CanvasGroup>();
        if (comicGroup == null)
            comicGroup = comicStrip.gameObject.AddComponent<CanvasGroup>();

        float[] panelHoldTimes = new float[] { 5.5f, 22f, 13f, 5.5f, 5f, 5f };

        if (cinematicAudio != null)
        {
            audioSource.clip = cinematicAudio;
            audioSource.Play();
        }

        for (int i = 0; i < comicPanels.Length && isCinematicPlaying; i++)
        {
            if (i == 3 && cinematicAudio2 != null)
            {
                audioSource.Stop();
                audioSource.clip = cinematicAudio2;
                audioSource.Play();
            }
            
            comicImage.sprite = comicPanels[i];
            
            comicGroup.alpha = 0f;
            float fadeTime = 0.5f;
            float fadeElapsed = 0f;
            while (fadeElapsed < fadeTime && isCinematicPlaying)
            {
                fadeElapsed += Time.deltaTime;
                comicGroup.alpha = Mathf.Lerp(0f, 1f, fadeElapsed / fadeTime);
                yield return null;
            }
            comicGroup.alpha = 1f;

            float holdTime = (i < panelHoldTimes.Length) ? panelHoldTimes[i] : 6f;
            float holdElapsed = 0f;
            while (holdElapsed < holdTime && isCinematicPlaying)
            {
                holdElapsed += Time.deltaTime;
                yield return null;
            }
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            Destroy(audioSource);
        }

        if (isCinematicPlaying)
            StartGameplay();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SkipCinematic()
    {
        if (!isCinematicPlaying) return;
        isCinematicPlaying = false;
        StartGameplay();
    }

    void StartGameplay()
    {
        StartCoroutine(TransitionToGame());
    }

    IEnumerator TransitionToGame()
    {
        yield return StartCoroutine(Fade(1, 0.5f));
        cinematicPanel.SetActive(false);
        
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
            
        yield return StartCoroutine(Fade(0, 0.5f));
    }

    IEnumerator Fade(float targetAlpha, float duration)
    {
        if (fadeOverlay == null) yield break;
        float startAlpha = fadeOverlay.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeOverlay.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }
        fadeOverlay.alpha = targetAlpha;
    }
}
