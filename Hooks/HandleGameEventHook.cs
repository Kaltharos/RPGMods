using HarmonyLib;
using ProjectM.Gameplay.Systems;
using RPGMods.Utils;
using ProjectM;
using Unity.Entities;
using System;
using RPGMods.Systems;

namespace RPGMods.Hooks
{
    [HarmonyPatch(typeof(HandleGameplayEventsSystem), nameof(HandleGameplayEventsSystem.OnUpdate))]
    public class HandleGameplayEventsSystem_Patch
    {
        private static void Postfix(HandleGameplayEventsSystem __instance)
        {
            //-- Player Location Caching
            if (ExperienceSystem.isEXPActive || (PvPSystem.isHonorSystemEnabled && PvPSystem.isEnableHostileGlow && PvPSystem.isUseProximityGlow)) ProximityLoop.UpdateCache();
            //-- HonorSystem Hostile Glow
            if (PvPSystem.isHonorSystemEnabled && PvPSystem.isEnableHostileGlow && PvPSystem.isUseProximityGlow) ProximityLoop.HostileProximityGlow();

            //-- Spawn Custom NPC Task
            if (Cache.spawnNPC_Listen.Count > 0)
            {
                foreach (var item in Cache.spawnNPC_Listen)
                {
                    if (item.Value.Process == false) continue;

                    var entity = item.Value.getEntity();
                    var Option = item.Value.Options;

                    if (Option.ModifyBlood)
                    {
                        var BloodSource = __instance.EntityManager.GetComponentData<BloodConsumeSource>(entity);
                        BloodSource.UnitBloodType = Option.BloodType;
                        BloodSource.BloodQuality = Option.BloodQuality;
                        BloodSource.CanBeConsumed = Option.BloodConsumeable;
                        __instance.EntityManager.SetComponentData(entity, BloodSource);
                    }

                    if (Option.ModifyStats)
                    {
                        __instance.EntityManager.SetComponentData(entity, Option.UnitStats);
                    }

                    if (item.Value.Duration < 0)
                    {
                        __instance.EntityManager.SetComponentData(entity, new LifeTime()
                        {
                            Duration = 0,
                            EndAction = LifeTimeEndAction.None
                        });
                    }
                    else
                    {
                        __instance.EntityManager.SetComponentData(entity, new LifeTime()
                        {
                            Duration = item.Value.Duration,
                            EndAction = LifeTimeEndAction.Destroy
                        });
                    }

                    Cache.spawnNPC_Listen.Remove(item.Key);
                }
            }
        }
    }
}