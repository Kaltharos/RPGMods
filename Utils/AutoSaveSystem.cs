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
            PermissionSystem.SaveUserPermission(); //-- Nothing new to save.
            SunImmunity.SaveImmunity();
            Waypoint.SaveWaypoints();
            NoCooldown.SaveCooldown();
            GodMode.SaveGodMode();
            Speed.SaveSpeed();
            AutoRespawn.SaveAutoRespawn();
            //Kit.SaveKits();   //-- Nothing to save here for now.

            //-- System Related
            ExperienceSystem.SaveEXPData();
            PvPSystem.SavePvPStat();
            WeaponMasterSystem.SaveWeaponMastery();
            BanSystem.SaveBanList();

            Plugin.Logger.LogWarning("All database saved to JSON file.");
        }

        public static void LoadDatabase()
        {
            //-- Commands Related
            PermissionSystem.LoadPermissions();
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
            BanSystem.LoadBanList();

            Plugin.Logger.LogWarning("All database is now loaded.");
        }
    }
}
