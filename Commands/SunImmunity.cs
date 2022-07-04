using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Wetstone.API;
using ProjectM;
using RPGMods.Utils;
using ProjectM.Network;
using Unity.Entities;
using System;

namespace RPGMods.Commands
{
    [Command("sunimmunity, sun", Usage = "sunimmunity", Description = "Toggles sun immunity.")]
    public static class SunImmunity
    {
        public static void Initialize(Context ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isSunImmune = Database.sunimmunity.TryGetValue(SteamID, out bool isSunImmune_);
            if (isSunImmune) isSunImmune = false;
            else isSunImmune = true;
            UpdateImmunity(ctx, isSunImmune);
            string s = isSunImmune ? "Activated" : "Deactivated";
            ctx.Event.User.SendSystemMessage($"Sun Immunity <color=#ffff00ff>{s}</color>");
            Helper.ApplyBuff(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, Database.buff.Buff_VBlood_Perk_Moose);
        }

        public static bool UpdateImmunity(Context ctx, bool isSunImmune)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isExist = Database.sunimmunity.TryGetValue(SteamID, out bool isSunImmune_);
            if (isExist || !isSunImmune) RemoveImmunity(ctx);
            else Database.sunimmunity.Add(SteamID, isSunImmune);
            return true;
        }

        public static void SaveImmunity()
        {
            File.WriteAllText("BepInEx/config/RPGMods/Saves/sunimmunity.json", JsonSerializer.Serialize(Database.sunimmunity, Database.JSON_options));
        }

        public static bool RemoveImmunity(Context ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            if (Database.sunimmunity.TryGetValue(SteamID, out bool isSunImmune))
            {
                Database.sunimmunity.Remove(SteamID);
                return true;
            }
            return false;
        }

        public static void LoadSunImmunity()
        {
            if (!File.Exists("BepInEx/config/RPGMods/Saves/sunimmunity.json"))
            {
                var stream = File.Create("BepInEx/config/RPGMods/Saves/sunimmunity.json");
                stream.Dispose();
            }

            string json = File.ReadAllText("BepInEx/config/RPGMods/Saves/sunimmunity.json");
            try
            {
                Database.sunimmunity = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                Plugin.Logger.LogWarning("SunImmunity DB Populated.");
            }
            catch
            {
                Database.sunimmunity = new Dictionary<ulong, bool>();
                Plugin.Logger.LogWarning("SunImmunity DB Created.");
            }
        }
    }
}
