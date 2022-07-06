using RPGMods.Commands;
using RPGMods.Systems;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPGMods.Utils
{
    public static class AutoSaveSystem
    {
        //-- AutoSave is now directly hooked into the Server game save activity.
        public static void SaveDatabase()
        {
            //CommandHandler.SavePermissions(); //-- Nothing new to save.
            SunImmunity.SaveImmunity();
            Waypoint.SaveWaypoints();
            NoCooldown.SaveCooldown();
            GodMode.SaveGodMode();
            Speed.SaveSpeed();
            AutoRespawn.SaveAutoRespawn();
            //Kit.SaveKits();   //-- Nothing to save here for now.

            ExperienceSystem.SaveEXPData();
            PvPSystem.SavePvPStat();
            WeaponMasterSystem.SaveWeaponMastery();

            Plugin.Logger.LogWarning("All database saved to JSON file.");
        }

        public static void LoadDatabase()
        {
            //-- Commands Related
            CommandHandler.LoadPermissions();
            SunImmunity.LoadSunImmunity();
            Waypoint.LoadWaypoints();
            NoCooldown.LoadNoCooldown();
            GodMode.LoadGodMode();
            Speed.LoadSpeed();
            AutoRespawn.LoadAutoRespawn();
            Kit.LoadKits();

            //-- System Related
            PvPSystem.LoadPvPStat();
            ExperienceSystem.LoadEXPData();
            WeaponMasterSystem.LoadWeaponMastery();
            Plugin.Logger.LogWarning("All database is now loaded.");
        }
    }
}
