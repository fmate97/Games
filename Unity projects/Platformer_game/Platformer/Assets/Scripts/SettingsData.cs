using System;

[System.Serializable]
public class SettingsData
{
    public bool isFullscreen;
    public string resolution;
    public int mainVolume;
    public int musicVolume;
    public int effectVolume;

    public SettingsData()
    {
        isFullscreen = false;
        resolution = "";
        mainVolume = 0;
        musicVolume = 0;
        effectVolume = 0;
    }

    public SettingsData(bool isFullscreen, string resolution, int mainVolume, int musicVolume, int effectVolume)
    {
        this.isFullscreen = isFullscreen;
        this.resolution = resolution;
        this.mainVolume = mainVolume;
        this.musicVolume = musicVolume;
        this.effectVolume = effectVolume;
    }

    public override bool Equals(object obj)
    {
        SettingsData other = obj as SettingsData;

        if (isFullscreen == other.isFullscreen
            && resolution == other.resolution
            && mainVolume == other.mainVolume
            && musicVolume == other.musicVolume
            && effectVolume == other.effectVolume)
        {
            return true;
        }
        else return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(isFullscreen, resolution, mainVolume, musicVolume, effectVolume);
    }
}
