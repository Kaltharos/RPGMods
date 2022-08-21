using ProjectM;
using ProjectM.Network;
using RPGMods.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Cache = RPGMods.Utils.Cache;

namespace RPGMods.Systems
{
    public static class ProximityLoop
    {
        public static float maxDistance = 15.0f;

        private static EntityManager em = Plugin.Server.EntityManager;
        private static HashSet<Entity> SkipList = new();
        private static Dictionary<Entity, ulong> HostileList = new();
        private static HashSet<Entity> HostileOutRange = new();

        private static bool LoopInProgress = false;

        public static void UpdateCache()
        {
            var EntityArray = em.CreateEntityQuery(ComponentType.ReadOnly<PlayerCharacter>()).ToEntityArray(Allocator.Temp);
            foreach (var entity in EntityArray)
            {
                Cache.PlayerLocations[entity] = em.GetComponentData<LocalToWorld>(entity);
            }
        }

        public static void HostileProximityGlow()
        {
            if (LoopInProgress) return;
            LoopInProgress = true;

            SkipList.Clear();
            HostileList.Clear();
            HostileOutRange.Clear();

            foreach (var entity in Cache.HostilityState)
            {
                if (!entity.Value.IsHostile) continue;
                if (SkipList.Contains(entity.Key)) continue;

                if (ClosePlayers(entity.Key, out var TBSkip))
                {
                    SkipList.Add(entity.Key);
                    HostileList[entity.Key] = entity.Value.SteamID;

                    foreach (var close_entity in TBSkip)
                    {
                        SkipList.Add(close_entity);
                        if (Cache.HostilityState[close_entity].IsHostile) HostileList[close_entity] = Cache.HostilityState[close_entity].SteamID;
                    }
                }
                else
                {
                    SkipList.Add(entity.Key);
                    HostileOutRange.Add(entity.Key);
                }
            }

            foreach (var entity in HostileList)
            {
                bool hasHostileBuff = Helper.HasBuff(entity.Key, PvPSystem.HostileBuff);
                bool isRatForm = Helper.HasBuff(entity.Key, Database.Buff.RatForm);
                if (hasHostileBuff)
                {
                    if (isRatForm) Helper.RemoveBuff(entity.Key, PvPSystem.HostileBuff);
                }
                else
                {
                    if (!isRatForm) Helper.ApplyBuff(Cache.SteamPlayerCache[entity.Value].UserEntity, entity.Key, PvPSystem.HostileBuff);
                }
            }

            foreach(var entity in HostileOutRange)
            {
                Helper.RemoveBuff(entity, PvPSystem.HostileBuff);
            }

            LoopInProgress = false;
        }

        private static bool ClosePlayers(Entity characterEntity, out List<Entity> ClosePlayers)
        {
            ClosePlayers = new();

            if (Cache.PlayerLocations.TryGetValue(characterEntity, out var charPosition))
            {
                foreach (var item in Cache.HostilityState)
                {
                    if (item.Key.Equals(characterEntity)) continue;
                    if (SkipList.Contains(item.Key)) continue;

                    Cache.SteamPlayerCache.TryGetValue(item.Value.SteamID, out var playerData);
                    if (playerData.IsOnline == false)
                    {
                        SkipList.Add(item.Key);
                        continue;
                    }

                    if (Cache.PlayerLocations.TryGetValue(item.Key, out var targetPosition))
                    {
                        var distance = math.distance(charPosition.Position.xz, targetPosition.Position.xz);

                        if (distance < maxDistance)
                        {
                            ClosePlayers.Add(item.Key);
                        }
                    }
                    else
                    {
                        SkipList.Add(item.Key);
                        continue;
                    }
                }
                if (ClosePlayers.Count > 0) return true;
            }
            else
            {
                SkipList.Add(characterEntity);
                return false;
            }
            
            return false;
        }
    }
}
