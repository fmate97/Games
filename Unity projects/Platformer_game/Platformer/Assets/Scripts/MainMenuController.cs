using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] Animator cameraAnimator;
    [Header("MainMenu")]
    [SerializeField] GameObject playSubMenu;
    [SerializeField] GameObject exitSubMenu;
    [Header("Settings")]
    [SerializeField] GameObject videoPanel;
    [SerializeField] GameObject highlightVideo;
    [SerializeField] GameObject audioPanel;
    [SerializeField] GameObject highlightAudio;
    [SerializeField] TMP_Dropdown resolutionDropDown;
    [SerializeField] Slider mainVolumeSlider;
    [SerializeField] TMP_InputField mainValueInputText;
    [SerializeField] Toggle mainValueMuteToggle;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] TMP_InputField musicValueInputText;
    [SerializeField] Toggle musicValueMuteToggle;
    [SerializeField] Slider effectVolumeSlider;
    [SerializeField] TMP_InputField effectValueInputText;
    [SerializeField] Toggle effectValueMuteToggle;
    [SerializeField] Toggle fullscreenToggle;
    [Header("Sounds")]
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] AudioSource buttonClick;
    [SerializeField] AudioSource uISwitchClick;

    private string _showMainMenuTrigger = "showMainMenu", _showSettingsTrigger = "showSettings";
    private string _mainVolume = "MainVolume", _musicVolume = "MusicVolume", _effectVolume = "EffectVolume";
    private Resolution[] _resolutions;
    private SettingsData _settingsData;

    void Start()
    {
        InitSettingsPanel();
    }

    void InitSettingsPanel()
    {
        _settingsData = SaveSystem.LoadSettings();

        if (_settingsData.Equals(new SettingsData()))
        {
            InitResolutionDropDown();
            fullscreenToggle.isOn = Screen.fullScreen;

            float mainVolumeValue = 50f;
            float musicVolumeValue = 50f;
            float effectVolumeValue = 50f;

            mainVolumeSlider.value = mainVolumeValue - 80f;
            audioMixer.SetFloat(_mainVolume, mainVolumeSlider.value);
            mainValueInputText.text = $"{mainVolumeValue}";

            musicVolumeSlider.value = musicVolumeValue - 80;
            audioMixer.SetFloat(_musicVolume, musicVolumeSlider.value);
            musicValueInputText.text = $"{musicVolumeValue}";

            effectVolumeSlider.value = effectVolumeValue - 80;
            audioMixer.SetFloat(_effectVolume, effectVolumeSlider.value);
            effectValueInputText.text = $"{effectVolumeValue}";
        }
        else
        {
            string[] split1 = _settingsData.resolution.Split('x');
            int width = Int32.Parse(split1[0]);
            string[] split2 = split1[1].Split('@');
            int height = Int32.Parse(split2[0]);
            string refreshRateString = split2[1].Replace("hz", "");
            int refreshRate = Int32.Parse(refreshRateString);
            bool isFullscreen = _settingsData.isFullscreen;

            Screen.SetResolution(width, height, isFullscreen, refreshRate);
            InitResolutionDropDown();
            fullscreenToggle.isOn = isFullscreen;

            mainVolumeSlider.value = _settingsData.mainVolume - 80f;
            audioMixer.SetFloat(_mainVolume, mainVolumeSlider.value);
            mainValueInputText.text = $"{_settingsData.mainVolume}";
            if (_settingsData.mainVolume == 0f)
            {
                mainValueMuteToggle.isOn = true;
                mainVolumeSlider.interactable = false;
            }

            musicVolumeSlider.value = _settingsData.musicVolume - 80f;
            audioMixer.SetFloat(_musicVolume, musicVolumeSlider.value);
            musicValueInputText.text = $"{_settingsData.musicVolume}";
            if (_settingsData.musicVolume == 0f)
            {
                musicValueMuteToggle.isOn = true;
                musicVolumeSlider.interactable = false;
            }

            effectVolumeSlider.value = _settingsData.effectVolume - 80f;
            audioMixer.SetFloat(_effectVolume, effectVolumeSlider.value);
            effectValueInputText.text = $"{_settingsData.effectVolume}";
            if (_settingsData.effectVolume == 0f)
            {
                effectValueMuteToggle.isOn = true;
                effectVolumeSlider.interactable = false;
            }
        }
    }

    void InitResolutionDropDown()
    {
        _resolutions = Screen.resolutions;
        resolutionDropDown.ClearOptions();
        int currentResolutionIndex = 0;
        List<string> options = new List<string>();
        for (int i = 0; i < _resolutions.Length; i++)
        {
            options.Add($"{_resolutions[i].width} x {_resolutions[i].height} @ {_resolutions[i].refreshRate}hz");

            if (_resolutions[i].width == Screen.width && _resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropDown.AddOptions(options);
        resolutionDropDown.value = currentResolutionIndex;
        resolutionDropDown.RefreshShownValue();
    }

    void SaveSettingsPanel()
    {
        SettingsData settingsData = new SettingsData(
            Screen.fullScreen,
            resolutionDropDown.options[resolutionDropDown.value].text,
            (int)mainVolumeSlider.value + 80,
            (int)musicVolumeSlider.value + 80,
            (int)effectVolumeSlider.value + 80
            );

        SaveSystem.SaveSettings(settingsData);
    }

    void PlayButtonClickSound()
    {
        buttonClick.Play();
    }

    void PlayUISwitchClickSound()
    {
        uISwitchClick.Play();
    }

    public void PlayButtonClick()
    {
        PlayButtonClickSound();

        exitSubMenu.SetActive(false);
        playSubMenu.SetActive(true);
    }

    public void ExitButtonClick()
    {
        PlayButtonClickSound();

        exitSubMenu.SetActive(true);
        playSubMenu.SetActive(false);
    }

    public void SettingsButtonClick()
    {
        PlayButtonClickSound();

        playSubMenu.SetActive(false);
        exitSubMenu.SetActive(false);

        cameraAnimator.SetTrigger(_showSettingsTrigger);

        PlayUISwitchClickSound();
    }

    public void ContinueButtonClick()
    {
        PlayButtonClickSound();

        Debug.LogError("TODO!");
    }

    public void NewGameButtonClick()
    {
        PlayButtonClickSound();

        SceneManager.LoadScene("Level_00");
    }

    public void LoadGameButtonClick()
    {
        PlayButtonClickSound();

        Debug.LogError("TODO!");
    }

    public void ExitYesButtonClick()
    {
        PlayButtonClickSound();

        Application.Quit();
    }

    public void ExitNoButtonClick()
    {
        PlayButtonClickSound();

        exitSubMenu.SetActive(false);
    }

    public void ReturnButtonClick()
    {
        PlayButtonClickSound();

        SaveSettingsPanel();

        cameraAnimator.SetTrigger(_showMainMenuTrigger);

        PlayUISwitchClickSound();

        audioPanel.SetActive(false);
        highlightAudio.SetActive(false);

        videoPanel.SetActive(false);
        highlightVideo.SetActive(false);
    }

    public void VideoButtonClick()
    {
        PlayButtonClickSound();

        videoPanel.SetActive(true);
        highlightVideo.SetActive(true);

        audioPanel.SetActive(false);
        highlightAudio.SetActive(false);
    }

    public void AudioButtonClick()
    {
        PlayButtonClickSound();

        audioPanel.SetActive(true);
        highlightAudio.SetActive(true);

        videoPanel.SetActive(false);
        highlightVideo.SetActive(false);
    }

    public void SetMainVolume(float volume)
    {
        audioMixer.SetFloat(_mainVolume, volume);
        mainValueInputText.text = $"{volume + 80}";
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat(_musicVolume, volume);
        musicValueInputText.text = $"{volume + 80}";
    }

    public void SetEffectVolume(float volume)
    {
        audioMixer.SetFloat(_effectVolume, volume);
        effectValueInputText.text = $"{volume + 80}";
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = _resolutions[resolutionIndex];

        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen, resolution.refreshRate);
    }

    public void MainVolumeToggle(bool isOn)
    {
        if (isOn)
        {
            audioMixer.SetFloat(_mainVolume, -80f);
            mainVolumeSlider.value = -80f;
            mainValueInputText.text = "0";

            mainVolumeSlider.interactable = false;
        }
        else
        {
            mainVolumeSlider.interactable = true;
        }
    }

    public void MusicVolumeToggle(bool isOn)
    {
        if (isOn)
        {
            audioMixer.SetFloat(_musicVolume, -80f);
            musicVolumeSlider.value = -80f;
            musicValueInputText.text = "0";

            musicVolumeSlider.interactable = false;
        }
        else
        {
            musicVolumeSlider.interactable = true;
        }
    }

    public void EffectVolumeToggle(bool isOn)
    {
        if (isOn)
        {
            audioMixer.SetFloat(_effectVolume, -80f);
            effectVolumeSlider.value = -80f;
            effectValueInputText.text = "0";

            effectVolumeSlider.interactable = false;
        }
        else
        {
            effectVolumeSlider.interactable = true;
        }
    }
}
