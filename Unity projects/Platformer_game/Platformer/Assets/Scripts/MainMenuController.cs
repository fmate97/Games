using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;

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

    void Start()
    {
        _resolutions = Screen.resolutions;
        resolutionDropDown.ClearOptions();
        int currentResolutionIndex = 0;
        List<string> options = new List<string>();
        for(int i = 0; i < _resolutions.Length; i++)
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

        SceneManager.LoadScene("MainLevel");
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

        cameraAnimator.SetTrigger(_showMainMenuTrigger);

        PlayUISwitchClickSound();
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
