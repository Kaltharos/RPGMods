using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using RPGMods.Commands;
using RPGMods.Hooks;
using RPGMods.Systems;
using RPGMods.Utils;
using System.IO;
using System.Reflection;
using Unity.Entities;
using UnityEngine;

#if WETSTONE
    using Wetstone.API;
#endif

namespace RPGMods
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]

#if WETSTONE
    [BepInDependency("xyz.molenzwiebel.wetstone")]
    [Reloadable]
    public class Plugin : BasePlugin, IRunOnInitialized
#else
    public class Plugin : BasePlugin
#endif
    {
        private Harmony harmony;

        private static ConfigEntry<string> Prefix;
        private static ConfigEntry<string> DisabledCommands;
        private static ConfigEntry<float> DelayedCommands;
        private static ConfigEntry<int> WaypointLimit;

        private static ConfigEntry<bool> EnableVIPSystem;
        private static ConfigEntry<bool> EnableVIPWhitelist;
        private static ConfigEntry<int> VIP_Permission;

        private static ConfigEntry<double> VIP_InCombat_ResYield;
        private static ConfigEntry<double> VIP_InCombat_DurabilityLoss;
        private static ConfigEntry<double> VIP_InCombat_MoveSpeed;
        private static ConfigEntry<double> VIP_InCombat_GarlicResistance;
        private static ConfigEntry<double> VIP_InCombat_SilverResistance;

        private static ConfigEntry<double> VIP_OutCombat_ResYield;
        private static ConfigEntry<double> VIP_OutCombat_DurabilityLoss;
        private static ConfigEntry<double> VIP_OutCombat_MoveSpeed;
        private static ConfigEntry<double> VIP_OutCombat_GarlicResistance;
        private static ConfigEntry<double> VIP_OutCombat_SilverResistance;

        private static ConfigEntry<bool> AnnouncePvPKills;
        private static ConfigEntry<bool> EnablePvPLadder;
        private static ConfigEntry<bool> EnablePvPToggle;
        private static ConfigEntry<bool> EnablePvPPunish;
        private static ConfigEntry<int> PunishLevelDiff;
        private static ConfigEntry<float> PunishDuration;
        private static ConfigEntry<int> PunishOffenseLimit;
        private static ConfigEntry<float> PunishOffenseCooldown;

        private static ConfigEntry<bool> BuffSiegeGolem;
        private static ConfigEntry<float> GolemPhysicalReduction;
        private static ConfigEntry<float> GolemSpellReduction;

        private static ConfigEntry<bool> HunterHuntedEnabled;
        private static ConfigEntry<int> HeatCooldown;
        private static ConfigEntry<int> BanditHeatCooldown;
        private static ConfigEntry<int> CoolDown_Interval;
        private static ConfigEntry<int> Ambush_Interval;
        private static ConfigEntry<int> Ambush_Chance;
        private static ConfigEntry<float> Ambush_Despawn_Unit_Timer;

        private static ConfigEntry<bool> EnableExperienceSystem;
        private static ConfigEntry<int> MaxLevel;
        private static ConfigEntry<float> EXPMultiplier;
        private static ConfigEntry<float> VBloodEXPMultiplier;
        private static ConfigEntry<double> EXPLostOnDeath;
        private static ConfigEntry<float> EXPFormula_1;
        private static ConfigEntry<double> EXPGroupModifier;
        private static ConfigEntry<float> EXPGroupMaxDistance;

        private static ConfigEntry<bool> EnableWeaponMaster;
        private static ConfigEntry<bool> EnableWeaponMasterDecay;
        private static ConfigEntry<float> WeaponMasterMultiplier;
        private static ConfigEntry<int> WeaponDecayInterval;
        private static ConfigEntry<int> WeaponMaxMastery;
        private static ConfigEntry<float> WeaponMastery_VBloodMultiplier;
        private static ConfigEntry<int> Offline_Weapon_MasteryDecayValue;
        private static ConfigEntry<int> MasteryCombatTick;
        private static ConfigEntry<int> MasteryMaxCombatTicks;

        public static bool isInitialized = false;

        public static ManualLogSource Logger;

        private static World _serverWorld;
        public static World Server
        {
            get
            {
                if (_serverWorld != null) return _serverWorld;

                _serverWorld = GetWorld("Server")
                    ?? throw new System.Exception("There is no Server world (yet). Did you install a server mod on the client?");
                return _serverWorld;
            }
        }

        public static bool IsServer => Application.productName == "VRisingServer";

        private static World GetWorld(string name)
        {
            foreach (var world in World.s_AllWorlds)
            {
                if (world.Name == name)
                {
                    return world;
                }
            }

            return null;
        }

        public void InitConfig()
        {
            Prefix = Config.Bind("Config", "Prefix", ".", "The prefix used for chat commands.");
            DelayedCommands = Config.Bind("Config", "Command Delay", 5f, "The number of seconds user need to wait out before sending another command.\nAdmin will always bypass this.");
            DisabledCommands = Config.Bind("Config", "Disabled Commands", "", "Enter command names to disable them, abbreviation are included automatically. Seperated by commas.\nEx.: save,godmode");
            WaypointLimit = Config.Bind("Config", "Waypoint Limit", 3, "Set a waypoint limit per user.");

            EnableVIPSystem = Config.Bind("VIP", "Enable VIP System", false, "Enable the VIP System.");
            EnableVIPWhitelist = Config.Bind("VIP", "Enable VIP Whitelist", false, "Enable the VIP user to ignore server capacity limit.");
            VIP_Permission = Config.Bind("VIP", "Minimum VIP Permission", 10, "The minimum permission level required for the user to be considered as VIP.");

            VIP_InCombat_DurabilityLoss = Config.Bind("VIP.InCombat", "Durability Loss Multiplier", 0.5, "Multiply durability loss when user is in combat. -1.0 to disable.\nDoes not affect durability loss on death.");
            VIP_InCombat_GarlicResistance = Config.Bind("VIP.InCombat", "Garlic Resistance Multiplier", -1.0, "Multiply garlic resistance when user is in combat. -1.0 to disable.");
            VIP_InCombat_SilverResistance = Config.Bind("VIP.InCombat", "Silver Resistance Multiplier", -1.0, "Multiply silver resistance when user is in combat. -1.0 to disable.");
            VIP_InCombat_MoveSpeed = Config.Bind("VIP.InCombat", "Move Speed Multiplier", -1.0, "Multiply move speed when user is in combat. -1.0 to disable.");
            VIP_InCombat_ResYield = Config.Bind("VIP.InCombat", "Resource Yield Multiplier", 2.0, "Multiply resource yield (not item drop) when user is in combat. -1.0 to disable.");

            VIP_OutCombat_DurabilityLoss = Config.Bind("VIP.OutCombat", "Durability Loss Multiplier", 0.5, "Multiply durability loss when user is out of combat. -1.0 to disable.\nDoes not affect durability loss on death.");
            VIP_OutCombat_GarlicResistance = Config.Bind("VIP.OutCombat", "Garlic Resistance Multiplier", 2.0, "Multiply garlic resistance when user is out of combat. -1.0 to disable.");
            VIP_OutCombat_SilverResistance = Config.Bind("VIP.OutCombat", "Silver Resistance Multiplier", 2.0, "Multiply silver resistance when user is out of combat. -1.0 to disable.");
            VIP_OutCombat_MoveSpeed = Config.Bind("VIP.OutCombat", "Move Speed Multiplier", 1.25, "Multiply move speed when user is out of combat. -1.0 to disable.");
            VIP_OutCombat_ResYield = Config.Bind("VIP.OutCombat", "Resource Yield Multiplier", 2.0, "Multiply resource yield (not item drop) when user is out of combat. -1.0 to disable.");

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

            if (!Directory.Exists("BepInEx/config/RPGMods")) Directory.CreateDirectory("BepInEx/config/RPGMods");
            if (!Directory.Exists("BepInEx/config/RPGMods/Saves")) Directory.CreateDirectory("BepInEx/config/RPGMods/Saves");

            if (!File.Exists("BepInEx/config/RPGMods/kits.json"))
            {
                var stream = File.Create("BepInEx/config/RPGMods/kits.json");
                stream.Dispose();
            }
        }

        public override void Load()
        {
            InitConfig();
            Logger = Log;
            harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        public override bool Unload()
        {
            AutoSaveSystem.SaveDatabase();
            Config.Clear();
            harmony.UnpatchSelf();
            
            return true;
        }

        public void OnGameInitialized()
        {
            if (isInitialized) return;

            //-- Commands Related
            AutoSaveSystem.LoadDatabase();

            //-- Apply configs
            ChatMessageSystem_Patch.CommandPrefix = Prefix.Value;
            CommandHandler.Prefix = Prefix.Value;
            CommandHandler.DisabledCommands = DisabledCommands.Value;
            CommandHandler.delay_Cooldown = DelayedCommands.Value;
            Waypoint.WaypointLimit = WaypointLimit.Value;

            PermissionSystem.isVIPSystem = EnableVIPSystem.Value;
            PermissionSystem.isVIPWhitelist = EnableVIPWhitelist.Value;
            PermissionSystem.VIP_Permission = VIP_Permission.Value;

            PermissionSystem.VIP_InCombat_ResYield = VIP_InCombat_ResYield.Value;
            PermissionSystem.VIP_InCombat_DurabilityLoss = VIP_InCombat_DurabilityLoss.Value;
            PermissionSystem.VIP_InCombat_MoveSpeed = VIP_InCombat_MoveSpeed.Value;
            PermissionSystem.VIP_InCombat_GarlicResistance = VIP_InCombat_GarlicResistance.Value;
            PermissionSystem.VIP_InCombat_SilverResistance = VIP_InCombat_SilverResistance.Value;

            PermissionSystem.VIP_OutCombat_ResYield = VIP_OutCombat_ResYield.Value;
            PermissionSystem.VIP_OutCombat_DurabilityLoss = VIP_OutCombat_DurabilityLoss.Value;
            PermissionSystem.VIP_OutCombat_MoveSpeed = VIP_OutCombat_MoveSpeed.Value;
            PermissionSystem.VIP_OutCombat_GarlicResistance = VIP_OutCombat_GarlicResistance.Value;
            PermissionSystem.VIP_OutCombat_SilverResistance = VIP_OutCombat_SilverResistance.Value;

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

            isInitialized = true;
        }

        public static void HandleChatMessage(VChatEvent ev)
        {
            CommandHandler.HandleCommands(ev);
        }
    }
}
