using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    private static string _settingsFilePath = $"{Application.persistentDataPath}/SettingsData.dat";

    public static void SaveSettings(SettingsData settings)
    {
        FileStream file;
        if (File.Exists(_settingsFilePath)) file = File.OpenWrite(_settingsFilePath);
        else file = File.Create(_settingsFilePath);

        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(file, settings);

        file.Close();
    }

    public static SettingsData LoadSettings()
    {
        FileStream file;
        if (File.Exists(_settingsFilePath)) file = File.OpenRead(_settingsFilePath);
        else return new SettingsData();

        BinaryFormatter formatter = new BinaryFormatter();
        SettingsData data = formatter.Deserialize(file) as SettingsData;

        file.Close();

        return data;
    }
}
