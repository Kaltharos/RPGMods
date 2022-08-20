using RPGMods.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Unity.Entities;

namespace RPGMods.Commands
{
    [Command("nocooldown, nocd", Usage = "nocooldown", Description = "Toggles instant cooldown for all abilities.")]
    public static class NoCooldown
    {
        public static void Initialize(Context ctx)
        {
            Entity PlayerCharacter = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isNoCD = Database.nocooldownlist.TryGetValue(SteamID, out bool isNoCooldown_);
            if (isNoCD) isNoCD = false;
            else isNoCD = true;
            UpdateCooldownList(ctx, isNoCD);
            string p = isNoCD ? "Activated" : "Deactivated";
            Output.SendSystemMessage(ctx, $"No Cooldown is now <color=#ffff00ff>{p}</color>");
            Helper.ApplyBuff(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, Database.Buff.Buff_VBlood_Perk_Moose);
        }

        public static bool UpdateCooldownList(Context ctx, bool isNoCooldown)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            bool isExist = Database.nocooldownlist.TryGetValue(SteamID, out bool isNoCooldown_);
            if (isExist || !isNoCooldown) RemoveCooldown(ctx);
            else Database.nocooldownlist.Add(SteamID, isNoCooldown);
            return true;
        }

        public static void SaveCooldown()
        {
            File.WriteAllText("BepInEx/config/RPGMods/Saves/nocooldown.json", JsonSerializer.Serialize(Database.nocooldownlist, Database.JSON_options));
        }

        public static bool RemoveCooldown(Context ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            if (Database.nocooldownlist.TryGetValue(SteamID, out bool isNoCooldown_))
            {
                Database.nocooldownlist.Remove(SteamID);
                return true;
            }
            return false;
        }

        public static void LoadNoCooldown()
        {
            if (!File.Exists("BepInEx/config/RPGMods/Saves/nocooldown.json"))
            {
                var stream = File.Create("BepInEx/config/RPGMods/Saves/nocooldown.json");
                stream.Dispose();
            }
            string json = File.ReadAllText("BepInEx/config/RPGMods/Saves/nocooldown.json");
            try
            {
                Database.nocooldownlist = JsonSerializer.Deserialize<Dictionary<ulong, bool>>(json);
                Plugin.Logger.LogWarning("NoCooldown DB Populated.");
            }
            catch
            {
                Database.nocooldownlist = new Dictionary<ulong, bool>();
                Plugin.Logger.LogWarning("NoCooldown DB Created.");
            }
        }
    }
}