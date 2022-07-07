using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using ProjectM.Scripting;
using RPGMods.Commands;
using RPGMods.Systems;
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
        
        private ConfigEntry<bool> AnnouncePvPKills;
        private ConfigEntry<bool> EnablePvPLadder;
        private ConfigEntry<bool> EnablePvPToggle;
        private ConfigEntry<bool> EnablePvPPunish;
        private ConfigEntry<int> PunishLevelDiff;
        private ConfigEntry<float> PunishDuration;
        private ConfigEntry<int> PunishOffenseLimit;
        private ConfigEntry<float> PunishOffenseCooldown;

        private ConfigEntry<bool> BuffSiegeGolem;
        private ConfigEntry<float> GolemPhysicalReduction;
        private ConfigEntry<float> GolemSpellReduction;

        private ConfigEntry<bool> HunterHuntedEnabled;
        private ConfigEntry<int> HeatCooldown;
        private ConfigEntry<int> BanditHeatCooldown;
        private ConfigEntry<int> CoolDown_Interval;
        private ConfigEntry<int> Ambush_Interval;
        private ConfigEntry<int> Ambush_Chance;
        private ConfigEntry<float> Ambush_Despawn_Unit_Timer;

        private ConfigEntry<bool> EnableExperienceSystem;
        private ConfigEntry<int> MaxLevel;
        private ConfigEntry<float> EXPMultiplier;
        private ConfigEntry<float> VBloodEXPMultiplier;
        private ConfigEntry<double> EXPLostOnDeath;
        private ConfigEntry<float> EXPFormula_1;
        private ConfigEntry<double> EXPGroupModifier;
        private ConfigEntry<float> EXPGroupMaxDistance;

        private ConfigEntry<bool> EnableWeaponMaster;
        private ConfigEntry<bool> EnableWeaponMasterDecay;
        private ConfigEntry<float> WeaponMasterMultiplier;
        private ConfigEntry<int> WeaponDecayInterval;
        private ConfigEntry<int> WeaponMaxMastery;
        private ConfigEntry<float> WeaponMastery_VBloodMultiplier;
        private ConfigEntry<int> Offline_Weapon_MasteryDecayValue;
        private ConfigEntry<int> MasteryCombatTick;
        private ConfigEntry<int> MasteryMaxCombatTicks;

        public static ManualLogSource Logger;

        public void InitConfig()
        {
            Prefix = Config.Bind("Config", "Prefix", ".", "The prefix used for chat commands.");
            DelayedCommands = Config.Bind("Config", "Command Delay", 5f, "The number of seconds user need to wait out before sending another command.\nAdmin will always bypass this.");
            DisabledCommands = Config.Bind("Config", "Disabled Commands", "", "Enter command names to disable them. Seperated by commas.\nEx.: save,godmode,god");
            WaypointLimit = Config.Bind("Config", "Waypoint Limit", 3, "Set a waypoint limit per user.");

            AnnouncePvPKills = Config.Bind("PvP", "Announce PvP Kills", true, "Do I really need to explain this...?");
            EnablePvPLadder = Config.Bind("PvP", "Enable PvP Ladder", true, "Enables the PvP Ladder in the PvP command.");
            EnablePvPToggle = Config.Bind("PvP", "Enable PvP Toggle", true, "Enable/disable the pvp toggle feature in the pvp command.");
            EnablePvPPunish = Config.Bind("PvP", "Enable PvP Punishment", true, "Enables the punishment system for killing lower level player.");
            PunishLevelDiff = Config.Bind("PvP", "Punish Level Difference", -10, "Only punish the killer if the victim level is this much lower.");
            PunishOffenseLimit = Config.Bind("PvP", "Offense Limit", 3, "Killer must make this many offense before the punishment debuff is applied.");
            PunishOffenseCooldown = Config.Bind("PvP", "Offense Cooldown", 300f, "Reset the offense counter after this many seconds has passed since last offense.");
            PunishDuration = Config.Bind("PvP", "Debuff Duration", 1800f, "Apply the punishment debuff for this amount of time.");

            BuffSiegeGolem = Config.Bind("Siege", "Buff Siege Golem", false, "Enabling this will reduce all incoming physical and spell damage according to config.");
            GolemPhysicalReduction = Config.Bind("Siege", "Physical Damage Reduction", 0.5f, "Reduce incoming damage by this much. Ex.: 0.25 -> 25%");
            GolemSpellReduction = Config.Bind("Siege", "Spell Damage Reduction", 0.5f, "Reduce incoming spell damage by this much. Ex.: 0.75 -> 75%");

            HunterHuntedEnabled = Config.Bind("HunterHunted", "Enable", true, "Enable/disable the HunterHunted system.");
            HeatCooldown = Config.Bind("HunterHunted", "Heat Cooldown", 35, "Set the reduction value for player heat for every cooldown interval.");
            BanditHeatCooldown = Config.Bind("HunterHunted", "Bandit Heat Cooldown", 35, "Set the reduction value for player heat from the bandits faction for every cooldown interval.");
            CoolDown_Interval = Config.Bind("HunterHunted", "Cooldown Interval", 60, "Set every how many seconds should the cooldown interval trigger.");
            Ambush_Interval = Config.Bind("HunterHunted", "Ambush Interval", 300, "Set how many seconds player can be ambushed again since last ambush.");
            Ambush_Chance = Config.Bind("HunterHunted", "Ambush Chance", 50, "Set the percentage that an ambush may occur for every cooldown interval.");
            Ambush_Despawn_Unit_Timer = Config.Bind("HunterHunted", "Ambush Despawn Timer", 300f, "Despawn the ambush squad after this many second if they are still alive. Ex.: -1 -> Never Despawn");


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

            EnableWeaponMaster = Config.Bind("Mastery", "Enable Weapon Mastery", true, "Enable/disable the weapon mastery system.");
            EnableWeaponMasterDecay = Config.Bind("Mastery", "Enable Mastery Decay", true, "Enable/disable the decay of weapon mastery when the user is offline.");
            WeaponMaxMastery = Config.Bind("Mastery", "Max Mastery Value", 100000, "Configure the maximum mastery the user can atain. (100000 is 100%)");
            MasteryCombatTick = Config.Bind("Mastery", "Mastery Value/Combat Ticks", 5, "Configure the amount of mastery gained per combat ticks. (5 -> 0.005%)");
            MasteryMaxCombatTicks = Config.Bind("Mastery", "Max Combat Ticks", 12, "Mastery will no longer increase after this many ticks is reached in combat. (1 tick = 5 seconds)");
            WeaponMasterMultiplier = Config.Bind("Mastery", "Mastery Multiplier", 1f, "Multiply the gained mastery value by this amount.");
            WeaponMastery_VBloodMultiplier = Config.Bind("Mastery", "VBlood Mastery Multiplier", 15f, "Multiply Mastery gained from VBlood kill.");
            WeaponDecayInterval = Config.Bind("Mastery", "Decay Interval", 60, "Every amount of seconds the user is offline by the configured value will translate as 1 decay tick.");
            Offline_Weapon_MasteryDecayValue = Config.Bind("Mastery", "Decay Value", 1, "Mastery will decay by this amount for every decay tick.(1 -> 0.001%)");

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
            HunterHunted.ambush_despawn_timer = Ambush_Despawn_Unit_Timer.Value;

            PvPSystem.isLadderEnabled = EnablePvPLadder.Value;
            PvPSystem.isPvPToggleEnabled = EnablePvPToggle.Value;
            PvPSystem.announce_kills = AnnouncePvPKills.Value;
            PvPSystem.isPunishEnabled = EnablePvPPunish.Value;
            PvPSystem.PunishLevelDiff = PunishLevelDiff.Value;
            PvPSystem.PunishDuration = PunishDuration.Value;
            PvPSystem.OffenseLimit = PunishOffenseLimit.Value;
            PvPSystem.Offense_Cooldown = PunishOffenseCooldown.Value;

            SiegeSystem.isSiegeBuff = BuffSiegeGolem.Value;
            SiegeSystem.GolemPDef.Value = GolemPhysicalReduction.Value;
            SiegeSystem.GolemSDef.Value = GolemSpellReduction.Value;

            ExperienceSystem.isEXPActive = EnableExperienceSystem.Value;
            ExperienceSystem.MaxLevel = MaxLevel.Value;
            ExperienceSystem.EXPMultiplier = EXPMultiplier.Value;
            ExperienceSystem.VBloodMultiplier = VBloodEXPMultiplier.Value;
            ExperienceSystem.EXPLostOnDeath = EXPLostOnDeath.Value;
            ExperienceSystem.EXPConstant = EXPFormula_1.Value;
            ExperienceSystem.GroupModifier = EXPGroupModifier.Value;
            ExperienceSystem.GroupMaxDistance = EXPGroupMaxDistance.Value;

            WeaponMasterSystem.isMasteryEnabled = EnableWeaponMaster.Value;
            WeaponMasterSystem.isDecaySystemEnabled = EnableWeaponMasterDecay.Value;
            WeaponMasterSystem.Offline_DecayValue = Offline_Weapon_MasteryDecayValue.Value;
            WeaponMasterSystem.DecayInterval = WeaponDecayInterval.Value;
            WeaponMasterSystem.VBloodMultiplier = WeaponMastery_VBloodMultiplier.Value;
            WeaponMasterSystem.MasteryMultiplier = WeaponMasterMultiplier.Value;
            WeaponMasterSystem.MaxMastery = WeaponMaxMastery.Value;
            WeaponMasterSystem.MasteryCombatTick = MasteryCombatTick.Value;
            WeaponMasterSystem.MaxCombatTick = MasteryMaxCombatTicks.Value;
        }

        private void HandleChatMessage(VChatEvent ev)
        {
            cmd.HandleCommands(ev, Log, Config);
        }
    }
}
