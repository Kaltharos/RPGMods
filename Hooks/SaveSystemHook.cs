using HarmonyLib;
using ProjectM;
using RPGMods.Utils;

namespace RPGMods.Hooks
{
    [HarmonyPatch(typeof(TriggerPersistenceSaveSystem), nameof(TriggerPersistenceSaveSystem.TriggerSave))]
    public class TriggerPersistenceSaveSystem_Patch
    {
        public static void Prefix()
        {
            AutoSaveSystem.SaveDatabase();
        }
    }
}
