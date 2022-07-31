using ProjectM;
using HarmonyLib;
using RPGMods.Utils;

namespace RPGMods.Hooks
{
    [HarmonyPatch(typeof(UnitSpawnerReactSystem), nameof(UnitSpawnerReactSystem.OnUpdate))]
    public static class UnitSpawnerReactSystem_Patch
    {
        public static bool listen = false;
        public static void Prefix(UnitSpawnerReactSystem __instance)
        {
            if (listen)
            {
                if (__instance.__OnUpdate_LambdaJob0_entityQuery != null)
                {
                    var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
                    foreach (var entity in entities)
                    {
                        //Plugin.Logger.LogWarning($"Entity I: {entity} GUID: {Helper.GetPrefabGUID(entity)} N: {Helper.GetPrefabName(Helper.GetPrefabGUID(entity))}");
                        var Data = __instance.EntityManager.GetComponentData<LifeTime>(entity);
                        if (Cache.spawnNPC_Listen.TryGetValue(Data.Duration, out var Content))
                        {
                            Content.EntityIndex = entity.Index;
                            Content.EntityVersion = entity.Version;
                            if (Content.Options.Process) Content.Process = true;

                            Cache.spawnNPC_Listen[Data.Duration] = Content;

                            //Plugin.Logger.LogWarning($"Struct D: {Cache.spawnNPC_Listen[Data.Duration].Duration} E: {Cache.spawnNPC_Listen[Data.Duration].getEntity()} Ready: {Cache.spawnNPC_Listen[Data.Duration].Process}");

                            listen = false;
                        }
                    }
                }
            }
        }
    }
}
