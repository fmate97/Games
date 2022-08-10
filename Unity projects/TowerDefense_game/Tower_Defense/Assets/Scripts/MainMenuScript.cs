using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class MainMenuScript : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject settingsPanel;
    [Header("Volume")]
    [SerializeField] AudioSource clickSound;
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] TextMeshProUGUI musicVolumePlaceholder;
    [SerializeField] Slider effectVolumeSlider;
    [SerializeField] TextMeshProUGUI effectVolumePlaceholder;
    [Header("Resolution")]
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] Toggle fullscreenToggle;
    [Header("Continue")]
    [SerializeField] Button continueButton;

    private int _currentResolutionIndex = -1;
    private string _continuePrefKey = "nextLevel", _continuePrefValue = "";
    private string _musicPrefKey = "musicVolume", _effectPrefKey = "effectVolume";
    private string _resolutionWidthPrefKey = "resolutionWidth", _resolutionHeightPrefKey = "resolutionHeight";
    private string _resolutionRefreshPrefKey = "resolutionRefresh", _resolutionFullscreenPrefKey = "fullscreen";
    private string _musicParam = "MusicParam", _effectParam = "EffectParam";
    private Resolution[] resolutions;

    private void Start()
    {
        Time.timeScale = 1;

        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);

        resolutions = Screen.resolutions;

        InitResolution();
        InitVolumes();

        if (PlayerPrefs.HasKey(_continuePrefKey))
        {
            _continuePrefValue = PlayerPrefs.GetString(_continuePrefKey);
            continueButton.interactable = true;
        }
        else
        {
            continueButton.interactable = false;
        }
    }

    private void InitResolution()
    {
        Resolution currentResolution = new Resolution();
        List<string> options = new List<string>();

        if (PlayerPrefs.HasKey(_resolutionWidthPrefKey)
            && PlayerPrefs.HasKey(_resolutionHeightPrefKey)
            && PlayerPrefs.HasKey(_resolutionRefreshPrefKey))
        {
            currentResolution.width = PlayerPrefs.GetInt(_resolutionWidthPrefKey);
            currentResolution.height = PlayerPrefs.GetInt(_resolutionHeightPrefKey);
            currentResolution.refreshRate = PlayerPrefs.GetInt(_resolutionRefreshPrefKey);
        }
        else
        {
            currentResolution = Screen.currentResolution;
        }


        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add($"{resolutions[i].width}x{resolutions[i].height}@{resolutions[i].refreshRate}hz");

            if (resolutions[i].width == currentResolution.width
                && resolutions[i].height == currentResolution.height
                && resolutions[i].refreshRate == currentResolution.refreshRate)
            {
                _currentResolutionIndex = i;
            }
        }

        if (PlayerPrefs.HasKey(_resolutionFullscreenPrefKey))
        {
            bool fullscreen = PlayerPrefs.GetString(_resolutionFullscreenPrefKey) == true.ToString();
            Screen.fullScreen = fullscreen;
            fullscreenToggle.isOn = fullscreen;
        }
        else
        {
            fullscreenToggle.isOn = Screen.fullScreen;
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = _currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void InitVolumes()
    {
        int musicVolume = -1;
        if (PlayerPrefs.HasKey(_musicPrefKey))
        {
            musicVolume = PlayerPrefs.GetInt(_musicPrefKey);
        }
        else
        {
            musicVolume = 50;
        }
        musicVolume -= 80;
        audioMixer.SetFloat(_musicParam, musicVolume);
        musicVolumeSlider.value = musicVolume;
        musicVolumePlaceholder.text = $"{musicVolume + 80}";

        int effectVolume = -1;
        if (PlayerPrefs.HasKey(_effectPrefKey))
        {
            effectVolume = PlayerPrefs.GetInt(_effectPrefKey);
        }
        else
        {
            effectVolume = 50;
        }
        effectVolume -= 80;
        audioMixer.SetFloat(_effectParam, effectVolume);
        effectVolumeSlider.value = effectVolume;
        effectVolumePlaceholder.text = $"{effectVolume + 80}";
    }

    public void ContinueButtonClick()
    {
        clickSound.Play();
        SceneManager.LoadScene(_continuePrefValue);
    }

    public void PlayButtonClick()
    {
        clickSound.Play();
        SceneManager.LoadScene("Level_01");
    }

    public void SettingsButtonClick()
    {
        clickSound.Play();
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat(_musicParam, volume);
        musicVolumePlaceholder.text = $"{Convert.ToInt32(musicVolumeSlider.value) + 80}";
    }

    public void SetEffectVolume(float volume)
    {
        audioMixer.SetFloat(_effectParam, volume);
        effectVolumePlaceholder.text = $"{Convert.ToInt32(effectVolumeSlider.value) + 80}";
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        _currentResolutionIndex = resolutionIndex;
        Resolution resolution = resolutions[_currentResolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen, resolution.refreshRate);
    }

    public void MenuButtonClick()
    {
        clickSound.Play();

        SavePlayerSettings();

        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    private void SavePlayerSettings()
    {
        PlayerPrefs.SetInt(_musicPrefKey, Convert.ToInt32(musicVolumeSlider.value) + 80);
        PlayerPrefs.SetInt(_effectPrefKey, Convert.ToInt32(effectVolumeSlider.value) + 80);

        PlayerPrefs.SetString(_resolutionFullscreenPrefKey, Screen.fullScreen.ToString());

        Resolution currentResolution = resolutions[_currentResolutionIndex];
        PlayerPrefs.SetInt(_resolutionWidthPrefKey, currentResolution.width);
        PlayerPrefs.SetInt(_resolutionHeightPrefKey, currentResolution.height);
        PlayerPrefs.SetInt(_resolutionRefreshPrefKey, currentResolution.refreshRate);

        PlayerPrefs.Save();
    }

    public void ExitButtonClick()
    {
        clickSound.Play();
        Application.Quit();
    }
}
