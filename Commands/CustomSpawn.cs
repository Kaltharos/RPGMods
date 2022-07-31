using ProjectM;
using RPGMods.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Transforms;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("customspawn, cspn", "customspawn <Prefab Name> [<BloodType> <BloodQuality> <BloodConsumeable(\"true/false\")>]", "Spawns a modified NPC at your current position.")]
    public static class CustomSpawnNPC
    {
        public static void Initialize(Context ctx)
        {
            if (ctx.Args.Length != 0)
            {
                string name;
                int count = 1;

                PrefabGUID type = new PrefabGUID((int)Helper.BloodType.Frailed);
                float quality = 0;
                bool bloodconsume = true;

                if (ctx.Args.Length >= 4)
                {
                    if (ctx.Args[3].ToLower().Equals("false")) bloodconsume = false;
                    else bloodconsume = true;
                }

                if (ctx.Args.Length >= 3)
                {
                    quality = float.Parse(ctx.Args[2]);
                    if (float.Parse(ctx.Args[2]) < 0) quality = 0;
                    if (float.Parse(ctx.Args[2]) > 100) quality = 100;
                }

                if (ctx.Args.Length >= 2) {
                    type = new PrefabGUID((int)Helper.GetBloodTypeFromName(ctx.Args[1]));
                }

                if (ctx.Args.Length >= 1)
                {
                    name = ctx.Args[0];
                    var pos = ctx.EntityManager.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;
                    if (!Helper.SpawnNPCIdentify(out var npc_id, name, pos, 1, 2, 1800))
                    {
                        Output.CustomErrorMessage(ctx, $"Could not find specified unit: {name}");
                        return;
                    }

                    var Options = new SpawnOptions(true, type, quality, bloodconsume, false, default, true);
                    var NPCData = Cache.spawnNPC_Listen[npc_id];
                    NPCData.Options = Options;
                    if (NPCData.EntityIndex != 0) NPCData.Process = true;

                    Cache.spawnNPC_Listen[npc_id] = NPCData;

                    ctx.Event.User.SendSystemMessage($"Spawning CustomNPC \"{name}\" at <{pos.x}, {pos.z}>");
                }
            }
            else
            {
                Output.MissingArguments(ctx);
            }
        }
    }
}