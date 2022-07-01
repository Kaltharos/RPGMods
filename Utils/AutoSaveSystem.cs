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

            //-- System Related
            PvPStatistics.LoadPvPStat();
            ExperienceSystem.LoadEXPData();
        }
    }
}
