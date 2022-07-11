using System;
using System.Collections.Generic;
using System.Text;
using RPGMods.Utils;
using ProjectM;
using Unity.Entities;
using Wetstone.API;
using ProjectM.Network;
using System.IO;
using System.Text.Json;

namespace RPGMods.Systems
{
    public static class BanSystem
    {
        public static EntityManager em = VWorld.Server.EntityManager;
        public static bool IsUserBanned(ulong steamID, out BanData banData)
        {
            var isExist = Database.user_banlist.TryGetValue(steamID, out banData);
            if (isExist)
            {
                var CurrentTime = DateTime.Now;
                if (CurrentTime <= banData.BanUntil)
                {
                    return true;
                }
                else
                {
                    Database.user_banlist.Remove(steamID);
                    return false;
                }
            }
            return false;
        }

        public static bool BanUser(Entity userEntity, Entity targetUserEntity, int duration, string reason, out BanData banData)
        {
            banData = new BanData();
            var targetUserData = em.GetComponentData<User>(targetUserEntity);
            if (targetUserData.IsAdmin) return false;
            if (PermissionSystem.GetUserPermission(targetUserData.PlatformId) >= 100) return false;

            DateTime banUntil;
            if (duration == 0)
            {
                banUntil = DateTime.MaxValue;
            }
            else
            {
                bool isExist = Database.user_banlist.TryGetValue(targetUserData.PlatformId, out var prevBanData);
                if (isExist) banUntil = prevBanData.BanUntil.AddDays(duration);
                else banUntil = DateTime.Now.AddDays(duration);
            }

            var userData = em.GetComponentData<User>(userEntity);
            banData.BanUntil = banUntil;
            banData.Reason = reason;
            banData.BannedBy = userData.CharacterName.ToString();
            banData.SteamID = userData.PlatformId;

            Database.user_banlist[targetUserData.PlatformId] = banData;
            return true;
        }

        public static bool UnbanUser(Entity userEntity)
        {
            var userData = em.GetComponentData<User>(userEntity);
            bool isExist = Database.user_banlist.TryGetValue(userData.PlatformId, out _);
            if (isExist) Database.user_banlist.Remove(userData.PlatformId);
            else return false;
            return true;
        }

        public static void SaveBanList()
        {
            File.WriteAllText("BepInEx/config/RPGMods/user_banlist.json", JsonSerializer.Serialize(Database.user_banlist, Database.Pretty_JSON_options));
        }

        public static void LoadBanList()
        {
            if (!File.Exists("BepInEx/config/RPGMods/user_banlist.json"))
            {
                FileStream stream = File.Create("BepInEx/config/RPGMods/user_banlist.json");
                stream.Dispose();
            }
            string json = File.ReadAllText("BepInEx/config/RPGMods/user_banlist.json");
            try
            {
                Database.user_banlist = JsonSerializer.Deserialize<Dictionary<ulong, BanData>>(json);
                Plugin.Logger.LogWarning("Banlist DB Populated");
            }
            catch
            {
                Database.user_banlist = new Dictionary<ulong, BanData>();

            }
        }
    }
}
