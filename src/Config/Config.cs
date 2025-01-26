using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;

namespace T3EntrySounds;

public class PluginConfig : BasePluginConfig
{
    public Settings_Config Settings { get; set; } = new Settings_Config();
    public SankySounds_Config SankySounds { get; set; } = new SankySounds_Config();
    public Permission_Config Permission { get; set; } = new Permission_Config();
}
public class SankySounds_Config
{
    public Dictionary<string, string> Sounds { get; set; } = new Dictionary<string, string>();
}
public class Settings_Config
{
    public List<string> MenuCommands { get; set; } = ["sk", "sankysounds", "sounds"];
    public List<string> SayPrefixes { get; set; } = [""];
    public List<int> VolumeOptions { get; set; } = [0, 20, 40, 60, 80, 100];
    public int SoundsCooldown { get; set; } = 10;
    public int DefaultVolume { get; set; } = 60;
    public bool ShowSoundMessage { get; set; } = true;
}
public class Permission_Config
{
    public List<string> Permissions { get; set; } = [];
}