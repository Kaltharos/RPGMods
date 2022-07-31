using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using Stunlock.Network;
using System;

namespace RPGMods.Hooks
{
    [HarmonyPatch(typeof(GameBootstrap), nameof(GameBootstrap.Start))]
    public static class GameBootstrap_Patch
    {
        public static void Postfix()
        {
            Plugin Plugin = new Plugin();
            Plugin.OnGameInitialized();
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
            bool process = true;
            switch (connectionStatusReason)
            {
                case ConnectionStatusChangeReason.IncorrectPassword:
                    process = false;
                    break;
                case ConnectionStatusChangeReason.Unknown:
                    process = false;
                    break;
                case ConnectionStatusChangeReason.NoFreeSlots:
                    process = false;
                    break;
                case ConnectionStatusChangeReason.Banned:
                    process = false;
                    break;
                default:
                    process = true;
                    break;
            }
            if (process)
            {
                try
                {
                    var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
                    var serverClient = __instance._ApprovedUsersLookup[userIndex];
                    var userData = __instance.EntityManager.GetComponentData<User>(serverClient.UserEntity);
                    bool isNewVampire = userData.CharacterName.IsEmpty;

                    if (!isNewVampire)
                    {
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
}