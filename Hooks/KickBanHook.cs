using HarmonyLib;
using ProjectM;
using RPGMods.Systems;

namespace RPGMods.Hooks
{
    [HarmonyPatch(typeof(KickBanSystem_Server), nameof(KickBanSystem_Server.IsBanned))]
    public class KickBanSystem_Server_Patch
    {
        public static void Postfix(ulong platformId, ref bool __result)
        {
            // Let's not directly override the result so we don't ruin the banlist.txt
            if(BanSystem.IsUserBanned(platformId, out _))
            {
                __result = true;
            }
        }
    }
}
