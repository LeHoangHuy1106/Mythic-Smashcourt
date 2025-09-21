using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class SettingPanel : BasePanel
{
    public static SettingPanel Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private Slider sliderSound;
    [SerializeField] private Slider sliderMusic;

    [SerializeField] private Toggle toggleFPS30;
    [SerializeField] private Toggle toggleFPS60;
    [SerializeField] private Toggle toggleFPS120;

    [SerializeField] private TMP_Text txtSoundValue;
    [SerializeField] private TMP_Text txtMusicValue;

    [SerializeField] private Button btnHome;
    [Header("Audio Sources")]
    [SerializeField] private AudioSource[] soundObjects;  // SFX
    [SerializeField] private AudioSource musicSource;     // ✅ 1 AudioSource chung

    [Header("Music Clips")]
    [SerializeField] private AudioClip musicClipMainScene;
    [SerializeField] private AudioClip musicClipGameplay;

    private const string SOUND_VOLUME_PREF = "sound_volume";
    private const string MUSIC_VOLUME_PREF = "music_volume";
    private const string FPS_PREF = "fps_value";

    private void Awake()
    {
 
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        sliderSound.onValueChanged.AddListener(OnSoundVolumeChanged);
        sliderMusic.onValueChanged.AddListener(OnMusicVolumeChanged);

        toggleFPS30.onValueChanged.AddListener((isOn) => { if (isOn) SetFPS(30); });
        toggleFPS60.onValueChanged.AddListener((isOn) => { if (isOn) SetFPS(60); });
        toggleFPS120.onValueChanged.AddListener((isOn) => { if (isOn) SetFPS(120); });
        btnHome?.onClick.AddListener(OnClickHome);
        LoadSettings();
    }
    private void OnClickHome()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainScene");
    }
    public override void Show()
    {
        base.Show();
        LoadSettings();
        StartCoroutine(PauseGame());
        
    }
    private IEnumerator PauseGame()
    {
         yield return new WaitForSeconds(1f);
        Time.timeScale = 0f;
    }
    public override void Hide()
    {
        base.Hide();
        Time.timeScale = 1f;
    }
    private void LoadSettings()
    {
        float soundVolume = PlayerPrefs.GetFloat(SOUND_VOLUME_PREF, 1f);
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_PREF, 1f);
        int fpsValue = PlayerPrefs.GetInt(FPS_PREF, 60);

        sliderSound.value = soundVolume;
        sliderMusic.value = musicVolume;

        ApplySoundVolume(soundVolume);
        ApplyMusicVolume(musicVolume);

        toggleFPS30.isOn = (fpsValue == 30);
        toggleFPS60.isOn = (fpsValue == 60);
        toggleFPS120.isOn = (fpsValue == 120);

        Application.targetFrameRate = fpsValue;

        UpdateVolumeTexts();
    }

    private void OnSoundVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(SOUND_VOLUME_PREF, value);
        ApplySoundVolume(value);
        UpdateVolumeTexts();
    }

    private void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_PREF, value);
        ApplyMusicVolume(value);
        UpdateVolumeTexts();
    }

    private void ApplySoundVolume(float value)
    {
        foreach (var sfx in soundObjects)
        {
            if (sfx != null) sfx.volume = value;
        }
    }

    private void ApplyMusicVolume(float value)
    {
        if (musicSource != null)
        {
            musicSource.volume = value;
        }
    }

    private void UpdateVolumeTexts()
    {
        if (txtSoundValue != null) txtSoundValue.text = $"{Mathf.RoundToInt(sliderSound.value * 100)}%";
        if (txtMusicValue != null) txtMusicValue.text = $"{Mathf.RoundToInt(sliderMusic.value * 100)}%";
    }

    private void SetFPS(int value)
    {
        SettingPanel.Instance.PlaySound(0);
        Application.targetFrameRate = value;
        PlayerPrefs.SetInt(FPS_PREF, value);
        Debug.Log($"FPS set to {value}");
    }

    // ✅ PLAY SFX gọi từ bên ngoài
    public void PlaySound(int index)
    {
        if (index >= 0 && index < soundObjects.Length && soundObjects[index] != null)
        {
            soundObjects[index].Play();
        }
    }

    // ✅ STATIC tiện lợi
    public static void PlaySFX(int index)
    {
        if (Instance != null)
        {
            Instance.PlaySound(index);
        }
    }

    // ✅ Phát nhạc MainScene
    public void PlayMainSceneMusic()
    {
        if (musicSource != null && musicClipMainScene != null)
        {
            musicSource.clip = musicClipMainScene;
            musicSource.loop = true;
            musicSource.Play();
            Debug.Log("🎵 Playing MainScene Music");
        }
    }

    // ✅ Phát nhạc Gameplay
    public void PlayGameplayMusic()
    {
        if (musicSource != null && musicClipGameplay != null)
        {
            musicSource.clip = musicClipGameplay;
            musicSource.loop = true;
            musicSource.Play();
            Debug.Log("🎵 Playing Gameplay Music");
        }
    }
}
