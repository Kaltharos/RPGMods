using HarmonyLib;
using ProjectM;
using RPGMods.Utils;
using ProjectM.Network;
using ProjectM.CastleBuilding;
using RPGMods.Systems;
using Unity.Entities;
using System;

namespace RPGMods.Hooks
{
    public delegate void OnUpdateEventHandler(World world);
    [HarmonyPatch(typeof(StatChangeSystem), nameof(StatChangeSystem.OnUpdate))]
    public class SCSHook
    {
        public static event OnUpdateEventHandler OnUpdate;
        private static void Postfix(StatChangeSystem __instance)
        {
            try
            {
                OnUpdate?.Invoke(__instance.World);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(StatChangeSystem), nameof(StatChangeSystem.ApplyHealthChangeToEntity))]
    public class StatChangeSystem_Patch
    {
        private static void Prefix(StatChangeSystem __instance, ref StatChangeEvent statChange)
        {
            if (PvPSystem.isHonorSystemEnabled && statChange.Change < 0)
            {
                if (PvPSystem.isHonorBenefitEnabled == false) return;
                //-- PvP Honor System
                if (Cache.HostilityState.TryGetValue(statChange.Entity, out var receiverState)) {
                    if (VampireDownedServerEventSystem.TryFindRootOwner(statChange.Source, 1, __instance.EntityManager, out var Source))
                    {
                        if (statChange.Entity.Equals(Source)) return;

                        if (__instance.EntityManager.HasComponent<PlayerCharacter>(Source))
                        {
                            //-- Get Data
                            Database.PvPStats.TryGetValue(receiverState.SteamID, out var receiverPvPStats);
                            Cache.HostilityState.TryGetValue(Source, out var aggressorState);
                            Database.PvPStats.TryGetValue(aggressorState.SteamID, out var aggressorPvPStats);

                            //Database.SiegeState.TryGetValue(receiverState.SteamID, out var siegeState);
                            //if (!receiverState.IsHostile && siegeState.IsSiegeOn)
                            //{
                            //    receiverState.IsHostile = siegeState.IsSiegeOn;
                            //    Helper.ApplyBuff(Cache.SteamPlayerCache[receiverState.SteamID].UserEntity, statChange.Entity, PvPSystem.HostileBuff);
                            //}

                            //-- Calculate Damage
                            if (receiverPvPStats.Reputation <= -3000)
                            {
                                if (aggressorPvPStats.Reputation > 0)
                                {
                                    statChange.Change *= 0.8f;
                                    return;
                                }
                            }
                            else
                            {
                                if (aggressorPvPStats.Reputation <= -10000 && receiverPvPStats.Reputation > 0)
                                {
                                    statChange.Change *= 1.2f;
                                    return;
                                }

                                if (receiverState.IsHostile || receiverPvPStats.Reputation <= -1000) return;

                                if (aggressorPvPStats.Reputation > -1000 && aggressorState.IsHostile == false)
                                {
                                    statChange.Change = 0;
                                    return;
                                }
                            }
                        }
                    }
                }
                //-- Castle Siege
                else if (__instance.EntityManager.HasComponent<CastleHeartConnection>(statChange.Entity))
                {
                    var HeartEntity = __instance.EntityManager.GetComponentData<CastleHeartConnection>(statChange.Entity).CastleHeartEntity._Entity;
                    if (__instance.EntityManager.HasComponent<Pylonstation>(HeartEntity))
                    {
                        var CastleHeart = __instance.EntityManager.GetComponentData<Pylonstation>(HeartEntity);

                        if (CastleHeart.State == PylonstationState.Decaying) return;

                        var Owner = __instance.EntityManager.GetComponentData<UserOwner>(HeartEntity).Owner._Entity;
                        var OwnerData = __instance.EntityManager.GetComponentData<User>(Owner);

                        Database.PvPStats.TryGetValue(OwnerData.PlatformId, out var pvpStats);

                        if (PvPSystem.Interlocked.isSiegeOn)
                        {
                            if (pvpStats.Reputation >= 10000)
                            {
                                statChange.Change = 0;
                                return;
                            }

                            if (pvpStats.Reputation >= 5000)
                            {
                                statChange.Change = statChange.Change * 0.5f;
                                return;
                            }
                        }
                        else
                        {
                            if (VampireDownedServerEventSystem.TryFindRootOwner(statChange.Source, 1, __instance.EntityManager, out var Source))
                            {
                                if (__instance.EntityManager.HasComponent<PlayerCharacter>(Source))
                                {
                                    Cache.HostilityState.TryGetValue(Source, out var aggressorState);
                                    Database.SiegeState.TryGetValue(aggressorState.SteamID, out var aggresorSiege);

                                    if (!aggresorSiege.IsSiegeOn)
                                    {
                                        statChange.Change = 0;
                                        return;
                                    }
                                }
                            }

                            if (pvpStats.Reputation > -20000)
                            {
                                Database.SiegeState.TryGetValue(OwnerData.PlatformId, out var siegeState);
                                if (siegeState.IsSiegeOn == false || pvpStats.Reputation >= 10000)
                                {
                                    statChange.Change = 0;
                                    return;
                                }
                                else
                                {
                                    if (pvpStats.Reputation >= 5000)
                                    {
                                        statChange.Change = statChange.Change * 0.5f;
                                        return;
                                    }
                                }
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
