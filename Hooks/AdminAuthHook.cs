using ProjectM;
using HarmonyLib;
using RPGMods.Systems;

namespace RPGMods.Hooks
{
    [HarmonyPatch(typeof(AdminAuthSystem), nameof(AdminAuthSystem.IsAdmin))]
    public static class IsAdmin_Patch
    {
        public static void Postfix(ulong platformId, ref bool __result)
        {
            if (PermissionSystem.isVIPSystem)
            {
                if (PermissionSystem.GetUserPermission(platformId) >= PermissionSystem.min_PermissionBypass_Login)
                {
                    __result = true;
                }
            }
        }
    }
}
