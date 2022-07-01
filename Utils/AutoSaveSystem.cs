using RPGMods.Commands;
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
            CommandHandler.SavePermissions();
            SunImmunity.SaveImmunity();
            Waypoint.SaveWaypoints();
            NoCooldown.SaveCooldown();
            GodMode.SaveGodMode();
            Speed.SaveSpeed();
            AutoRespawn.SaveAutoRespawn();
            //Kit.SaveKits();   //-- Nothing to save here for now.

            ExperienceSystem.SaveEXPData();
            PvPStatistics.SavePvPStat();
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
            PvPStatistics.LoadPvPStat();
            ExperienceSystem.LoadEXPData();
        }
    }
}
