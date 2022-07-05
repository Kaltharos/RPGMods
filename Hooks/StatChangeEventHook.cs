//using HarmonyLib;
//using ProjectM;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using Unity.Entities;

//[HarmonyPatch(typeof(StatChangeSystem),nameof(StatChangeSystem.ApplyHealthChangeToEntity))]
//public class StatChangeSystem_Patch
//{
//    public static void Prefix(StatChangeSystem __instance, Entity statChangeEntity, StatChangeEvent statChange, EntityCommandBufferSafe commandBuffer, double currentTime)
//    {
//        // statChangeEntity got statChange.Change damage
//    }
//}