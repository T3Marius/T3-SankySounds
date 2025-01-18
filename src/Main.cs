﻿using System.Linq;
using System.Text.RegularExpressions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using T3MenuSharedApi;

namespace T3EntrySounds;

public class Main : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleAuthor => "T3Marius";
    public override string ModuleName => "T3-EntrySounds";
    public override string ModuleVersion => "1.0";
    public static Main Instance { get; set; } = new Main();
    public PluginConfig Config { get; set; } = new PluginConfig();
    public DateTime LastSoundTime = DateTime.MinValue;
    public void OnConfigParsed(PluginConfig config)
    {
        Config = config;
    }
    public IT3MenuManager? MenuManager;
    public IT3MenuManager? GetMenuManager()
    {
        if (MenuManager == null)
            MenuManager = new PluginCapability<IT3MenuManager>("t3menu:manager").Get();

        return MenuManager;
    }
    public override void Load(bool hotReload)
    {
        AddCommandListener("say", Command_Say, HookMode.Pre);
        AddCommandListener("say_team", Command_Say, HookMode.Pre);
        foreach (var cmd in Config.Settings.MenuCommands)
        {
            AddCommand($"css_{cmd}", "volume menu", Command_Menu);
        }
        SoundPlayerSettings.Initialize(Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "plugins", "T3-SankySounds"));
    }
    public HookResult Command_Say(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || player.IsBot)
            return HookResult.Continue;

        if (!HasPermission(player))
        {
            player.PrintToChat(Localizer["prefix"] + Localizer["no.permission"]);
            return HookResult.Continue;
        }

        string commandArgument = info.ArgByIndex(1);
        List<string> prefixes = Config.Settings.SayPrefixes;

        if (commandArgument != null)
        {
            foreach (var prefix in prefixes)
            {
                if (commandArgument.StartsWith(prefix))
                {
                    string commandKey = commandArgument.Substring(prefix.Length).Trim();
                    if (Config.SankySounds.Sounds.Any(s => s.Key.Split(',').Select(k => k.Trim()).Contains(commandKey)))
                    {
                        double secondsSinceLastSound = (DateTime.Now - LastSoundTime).TotalSeconds;
                        double remainingCooldown = Config.Settings.SoundsCooldown - secondsSinceLastSound;

                        if (remainingCooldown > 0)
                        {
                            player.PrintToChat(Localizer["prefix"] + Localizer["cooldown", Math.Floor(remainingCooldown).ToString("0") + " seconds"]);
                            return HookResult.Continue;
                        }
                        foreach (var soundEntry in Config.SankySounds.Sounds)
                        {
                            var keys = soundEntry.Key.Split(',').Select(k => k.Trim());
                            if (keys.Contains(commandKey))
                            {
                                foreach (var p in Utilities.GetPlayers())
                                {
                                    string pSteamID = p.SteamID.ToString();
                                    var pSettings = SoundPlayerSettings.GetPlayerSettings(pSteamID);

                                    if (pSettings.Volume > 0)
                                    {
                                        string SoundPath = Regex.Replace(soundEntry.Value, @"\d+_volume", $"{pSettings.Volume}_volume");
                                        p.ExecuteClientCommand($"play {SoundPath}");
                                        LastSoundTime = DateTime.Now;
                                    }
                                }
                                return Config.Settings.ShowSoundMessage ? HookResult.Continue : HookResult.Handled;
                            }
                        }
                    }
                }
            }
        }

        return HookResult.Continue;
    }
    public void Command_Menu(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
            return;

        var manager = GetMenuManager();
        if (manager == null)
            return;

        string SteamID = player.SteamID.ToString();
        var playerSettings = SoundPlayerSettings.GetPlayerSettings(SteamID);

        var menu = manager.CreateMenu(Localizer["menu<title>"], isSubMenu: false);

        List<object> volumeOptions = Config.Settings.VolumeOptions.Cast<object>().ToList();

        menu.AddSliderOption(Localizer["option<volume>"], volumeOptions, (int)playerSettings.Volume, (p, option) =>
        {
            playerSettings.Volume = (int)option.SliderValue!;
            SoundPlayerSettings.SetPlayerSettings(SteamID, playerSettings);

            p.PrintToChat(Localizer["prefix"] + Localizer["volume<selected>", option.SliderValue + "%"]);
        });

        menu.Add(Localizer["submenu<sounds>"], (p, option) =>
        {
            var soundMenu = manager.CreateMenu(Localizer["submenu<title>"], isSubMenu: true);
            soundMenu.ParentMenu = menu;

            foreach (var key in Config.SankySounds.Sounds)
            {
                string firstKey = key.Key.Split(',').First().Trim();

                soundMenu.AddTextOption(firstKey);
            }
            manager.OpenSubMenu(p, soundMenu);
        });
        manager.OpenMainMenu(player, menu);
    }
    public bool HasPermission(CCSPlayerController player)
    {
        var permissions = Config.Permission.Permissions;

        foreach (var perm in permissions)
        {
            if (perm.StartsWith("#") && AdminManager.PlayerInGroup(player, perm.Substring(1)))
            {
                return true;
            }
            else if (perm.StartsWith("@") && AdminManager.PlayerHasPermissions(player, perm.Substring(1)))
            {
                return true;
            }
            else if (perm.Equals(player.SteamID.ToString()))
            {
                return true;
            }
        }
        return false;
    }

}