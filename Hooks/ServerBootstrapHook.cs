using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using Stunlock.Network;
using System;

namespace RPGMods.Hooks
{
    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
    public class OnUserConnected_Patch
    {
        public static void Postfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
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
                //ServerChatUtils.SendSystemMessageToAllClients(em, $"Vampire \"{userComponent.CharacterName.ToString()}\" has awaken!");
            }
            //Output.SendLore(userEntity, "<color=#ffffffff>Welcome to our world! Please type the .help command to check out our custom commands and features!</color>");
        }
    }

    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserDisconnected))]
    public static class OnUserDisconnected_Patch
    {
        private static void Prefix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId, ConnectionStatusChangeReason connectionStatusReason, string extraData)
        {
            if (connectionStatusReason != ConnectionStatusChangeReason.IncorrectPassword)
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
                        //Plugin.Logger.LogWarning($"Player {userData.CharacterName} Logged Out. [{Database.player_decaymastery_logout[userData.PlatformId]}]");
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnDestroy))]
    public static class OnDestroy_Patch
    {
        private static void Prefix()
        {
            AutoSaveSystem.SaveDatabase();
        }
    }
}