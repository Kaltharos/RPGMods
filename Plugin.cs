using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using RPGMods.Commands;
using RPGMods.Utils;
using System.IO;
using System.Reflection;
using Wetstone.API;
using Wetstone.Hooks;

namespace RPGMods
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("xyz.molenzwiebel.wetstone")]
    [Reloadable]
    public class Plugin : BasePlugin, IRunOnInitialized
    {
        private Harmony harmony;

        private CommandHandler cmd;
        private ConfigEntry<string> Prefix;
        private ConfigEntry<string> DisabledCommands;
        private ConfigEntry<float> DelayedCommands;
        private ConfigEntry<int> WaypointLimit;
        private ConfigEntry<int> HeatCooldown;
        private ConfigEntry<int> BanditHeatCooldown;
        private ConfigEntry<int> CoolDown_Interval;
        private ConfigEntry<int> Ambush_Interval;
        private ConfigEntry<int> Ambush_Chance;
        private ConfigEntry<bool> AnnouncePvPKills;
        private ConfigEntry<bool> EnablePvPLadder;
        private ConfigEntry<bool> HunterHuntedEnabled;
        private ConfigEntry<bool> EnableExperienceSystem;
        private ConfigEntry<int> MaxLevel;
        private ConfigEntry<float> EXPMultiplier;
        private ConfigEntry<float> VBloodEXPMultiplier;
        private ConfigEntry<double> EXPLostOnDeath;
        private ConfigEntry<float> EXPFormula_1;
        private ConfigEntry<double> EXPGroupModifier;
        private ConfigEntry<float> EXPGroupMaxDistance;

        public static ManualLogSource Logger;

        public void InitConfig()
        {
            Prefix = Config.Bind("Config", "Prefix", ".", "The prefix used for chat commands.");
            DelayedCommands = Config.Bind("Config", "Command Delay", 5f, "The number of seconds user need to wait out before sending another command.\nAdmin will always bypass this.");
            DisabledCommands = Config.Bind("Config", "Disabled Commands", "", "Enter command names to disable them. Seperated by commas.\nEx.: save,godmode,god");
            WaypointLimit = Config.Bind("Config", "Waypoint Limit", 3, "Set a waypoint limit per user.");

            AnnouncePvPKills = Config.Bind("PvP", "Announce PvP Kills", true, "Do I really need to explain this...?");
            EnablePvPLadder = Config.Bind("PvP", "Enable PvP Ladder", true, "Enables the PvP Ladder in the PvP command.");

            HunterHuntedEnabled = Config.Bind("HunterHunted", "Enable", true, "Enable/disable the HunterHunted system.");
            HeatCooldown = Config.Bind("HunterHunted", "Heat Cooldown", 35, "Set the reduction value for player heat for every cooldown interval.");
            BanditHeatCooldown = Config.Bind("HunterHunted", "Bandit Heat Cooldown", 35, "Set the reduction value for player heat from the bandits faction for every cooldown interval.");
            CoolDown_Interval = Config.Bind("HunterHunted", "Cooldown Interval", 60, "Set every how many seconds should the cooldown interval trigger.");
            Ambush_Interval = Config.Bind("HunterHunted", "Ambush Interval", 300, "Set how many seconds player can be ambushed again since last ambush.");
            Ambush_Chance = Config.Bind("HunterHunted", "Ambush Chance", 50, "Set the percentage that an ambush may occur for every cooldown interval.");

            EnableExperienceSystem = Config.Bind("Experience", "Enable", true, "Enable/disable the the Experience System.");
            MaxLevel = Config.Bind("Experience", "Max Level", 80, "Configure the experience system max level.");
            EXPMultiplier = Config.Bind("Experience", "Multiplier", 1.0f, "Multiply the EXP gained by player.\nEx.: 0.7f -> Will reduce the EXP gained by 30%\nFormula: UnitKilledLevel * EXPMultiplier");
            VBloodEXPMultiplier = Config.Bind("Experience", "VBlood Multiplier", 15f, "Multiply EXP gained from VBlood kill.\nFormula: EXPGained * VBloodMultiplier * EXPMultiplier");
            EXPLostOnDeath = Config.Bind("Experience", "EXP Lost / Death", 0.10, "Percentage of experience the player lost for every death by NPC, no EXP is lost for PvP.\nFormula: TotalPlayerEXP - (EXPNeeded * EXPLost)");
            EXPFormula_1 = Config.Bind("Experience", "Constant", 0.2f, "Increase or decrease the required EXP to level up.\nFormula: (level/constant)^2\n" +
                "EXP Table & Formula: https://bit.ly/3npqdJw");
            EXPGroupModifier = Config.Bind("Experience", "Group Modifier", 0.75, "Set the modifier for EXP gained for each ally(player) in vicinity.\n" +
                "Example if you have 2 ally nearby, EXPGained = ((EXPGained * Modifier)*Modifier)");
            EXPGroupMaxDistance = Config.Bind("Experience", "Ally Max Distance", 50f, "Set the maximum distance an ally(player) has to be from the player for them to share EXP with the player");

            if (!Directory.Exists("BepInEx/config/RPGMods/Saves")) Directory.CreateDirectory("BepInEx/config/RPGMods/Saves");

            if (!File.Exists("BepInEx/config/RPGMods/kits.json"))
            {
                if (!Directory.Exists("BepInEx/config/RPGMods")) Directory.CreateDirectory("BepInEx/config/RPGMods");
                var stream = File.Create("BepInEx/config/RPGMods/kits.json");
                stream.Dispose();
            }

            if (!File.Exists("BepInEx/config/RPGMods/permissions.json"))
            {
                if (!Directory.Exists("BepInEx/config/RPGMods")) Directory.CreateDirectory("BepInEx/config/RPGMods");
                var stream = File.Create("BepInEx/config/RPGMods/permissions.json");
                stream.Dispose();
            }
        }

        public override void Load()
        {
            InitConfig();
            Logger = Log;
            cmd = new CommandHandler(Prefix.Value, DisabledCommands.Value);
            Chat.OnChatMessage += HandleChatMessage;
            harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        public override bool Unload()
        {
            AutoSaveSystem.SaveDatabase();
            Config.Clear();
            Chat.OnChatMessage -= HandleChatMessage;
            harmony.UnpatchSelf();
            return true;
        }

        public void OnGameInitialized()
        {
            //-- Commands Related
            AutoSaveSystem.LoadDatabase();

            //-- Apply configs
            CommandHandler.delay_Cooldown = DelayedCommands.Value;

            HunterHunted.isActive = HunterHuntedEnabled.Value;
            HunterHunted.heat_cooldown = HeatCooldown.Value;
            HunterHunted.bandit_heat_cooldown = BanditHeatCooldown.Value;
            HunterHunted.cooldown_timer = CoolDown_Interval.Value;
            HunterHunted.ambush_interval = Ambush_Interval.Value;
            HunterHunted.ambush_chance = Ambush_Chance.Value;

            PvP.isLadderEnabled = EnablePvPLadder.Value;
            PvPStatistics.announce_kills = AnnouncePvPKills.Value;

            ExperienceSystem.isEXPActive = EnableExperienceSystem.Value;
            ExperienceSystem.MaxLevel = MaxLevel.Value;
            ExperienceSystem.EXPMultiplier = EXPMultiplier.Value;
            ExperienceSystem.VBloodMultiplier = VBloodEXPMultiplier.Value;
            ExperienceSystem.EXPLostOnDeath = EXPLostOnDeath.Value;
            ExperienceSystem.EXPConstant = EXPFormula_1.Value;
            ExperienceSystem.GroupModifier = EXPGroupModifier.Value;
            ExperienceSystem.GroupMaxDistance = EXPGroupMaxDistance.Value;
        }

        private void HandleChatMessage(VChatEvent ev)
        {
            cmd.HandleCommands(ev, Log, Config);
        }
    }
}
