using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using RPGMods.Systems;
using RPGMods.Utils;
using Stunlock.Network;
using System;

namespace RPGMods.Hooks
{
    [HarmonyPatch(typeof(SettingsManager), nameof(SettingsManager.VerifyServerGameSettings))]
    public class ServerGameSetting_Patch
    {
        private static bool isInitialized = false;
        public static void Postfix()
        {
            if (isInitialized == false)
            {
                Plugin.Initialize();
                isInitialized = true;
            }
        }
    }

    [HarmonyPatch(typeof(GameBootstrap), nameof(GameBootstrap.Start))]
    public static class GameBootstrap_Patch
    {
        public static void Postfix()
        {
            Plugin.Initialize();
        }
    }

    [HarmonyPatch(typeof(GameBootstrap), nameof(GameBootstrap.OnApplicationQuit))]
    public static class GameBootstrapQuit_Patch
    {
        public static void Prefix()
        {
            AutoSaveSystem.SaveDatabase();
        }
    }

    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
    public static class OnUserConnected_Patch
    {
        public static void Postfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
        {
            try
            {
                var em = __instance.EntityManager;
                var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
                var serverClient = __instance._ApprovedUsersLookup[userIndex];
                var userEntity = serverClient.UserEntity;
                var userData = __instance.EntityManager.GetComponentData<User>(userEntity);
                bool isNewVampire = userData.CharacterName.IsEmpty;

                if (!isNewVampire)
                {
                    if (PvPSystem.isHonorSystemEnabled)
                    {
                        if (PvPSystem.isHonorTitleEnabled) Helper.RenamePlayer(userEntity, userData.LocalCharacter._Entity, userData.CharacterName);

                        Database.PvPStats.TryGetValue(userData.PlatformId, out var pvpStats);
                        Database.SiegeState.TryGetValue(userData.PlatformId, out var siegeState);

                        if (pvpStats.Reputation <= -1000)
                        {
                            PvPSystem.HostileON(userData.PlatformId, userData.LocalCharacter._Entity, userEntity);
                        }
                        else
                        {
                            if (!siegeState.IsSiegeOn)
                            {
                                PvPSystem.HostileOFF(userData.PlatformId, userData.LocalCharacter._Entity);
                            }
                        }
                    }
                    else
                    {
                        var playerName = userData.CharacterName.ToString();
                        Helper.UpdatePlayerCache(userEntity, playerName, playerName);
                    }
                    if (WeaponMasterSystem.isDecaySystemEnabled && WeaponMasterSystem.isMasteryEnabled)
                    {
                        WeaponMasterSystem.DecayMastery(userEntity);
                    }
                }
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserDisconnected))]
    public static class OnUserDisconnected_Patch
    {
        private static void Prefix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId, ConnectionStatusChangeReason connectionStatusReason, string extraData)
        {
            try
            {
                var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
                var serverClient = __instance._ApprovedUsersLookup[userIndex];
                var userData = __instance.EntityManager.GetComponentData<User>(serverClient.UserEntity);
                bool isNewVampire = userData.CharacterName.IsEmpty;

                if (!isNewVampire)
                {
                    var playerName = userData.CharacterName.ToString();
                    Helper.UpdatePlayerCache(serverClient.UserEntity, playerName, playerName, true);
                    if (WeaponMasterSystem.isDecaySystemEnabled)
                    {
                        Database.player_decaymastery_logout[userData.PlatformId] = DateTime.Now;
                    }
                }
            }
            catch { };
        }
    }
}