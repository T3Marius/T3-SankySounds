using Newtonsoft.Json;
using static T3EntrySounds.Main;

namespace T3EntrySounds;

public static class SoundPlayerSettings
{
    private static string? settingsPath;
    private static Dictionary<string, PlayerSoundSettings> settings = new Dictionary<string, PlayerSoundSettings>();

    public static void Initialize(string basePath)
    {
        settingsPath = Path.Combine(basePath, "sound_settings.json");
        LoadSettings();
    }
    private static void LoadSettings()
    {
        try
        {
            if (!File.Exists(settingsPath))
            {
                settings = new Dictionary<string, PlayerSoundSettings>();
                SaveSettings();
            }
            else
            {
                string json = File.ReadAllText(settingsPath);
                settings = JsonConvert.DeserializeObject<Dictionary<string, PlayerSoundSettings>>(json) ?? new Dictionary<string, PlayerSoundSettings>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load sound settings: {ex.Message}");
        }
    }
    public static void SaveSettings()
    {
        try
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(settingsPath!, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
    public static PlayerSoundSettings GetPlayerSettings(string steamId)
    {
        settings.TryGetValue(steamId, out var setting);
        return setting ?? new PlayerSoundSettings { Volume = Instance.Config.Settings.DefaultVolume };
    }
    public static void SetPlayerSettings(string steamId, PlayerSoundSettings setting)
    {
        settings[steamId] = setting;
        SaveSettings();
    }
}
public class PlayerSoundSettings
{
    public float Volume { get; set; }
}