using ProjectM;
using RPGMods.Utils;
using Unity.Transforms;

namespace RPGMods.Commands
{
    [Command("customspawn, cspn", "customspawn <Prefab Name> [<BloodType> <BloodQuality> <BloodConsumeable(\"true/false\")> <Duration>]", "Spawns a modified NPC at your current position.")]
    public static class CustomSpawnNPC
    {
        public static void Initialize(Context ctx)
        {
            if (ctx.Args.Length != 0)
            {
                string name;

                PrefabGUID type = new PrefabGUID((int)Helper.BloodType.Frailed);
                float quality = 0;
                bool bloodconsume = true;
                float duration = 1800;

                if (ctx.Args.Length >= 5)
                {
                    if (!float.TryParse(ctx.Args[4], out duration))
                    {
                        Output.InvalidArguments(ctx);
                        return;
                    }
                }

                if (ctx.Args.Length >= 4)
                {
                    if (ctx.Args[3].ToLower().Equals("false")) bloodconsume = false;
                    else bloodconsume = true;
                }

                if (ctx.Args.Length >= 3)
                {
                    if (float.TryParse(ctx.Args[2], out quality))
                    {
                        if (quality < 0) quality = 0;
                        if (quality > 100) quality = 100;
                    }
                    else
                    {
                        Output.InvalidArguments(ctx);
                        return;
                    }
                }

                if (ctx.Args.Length >= 2)
                {
                    type = new PrefabGUID((int)Helper.GetBloodTypeFromName(ctx.Args[1]));
                }

                if (ctx.Args.Length >= 1)
                {
                    name = ctx.Args[0];
                    var pos = ctx.EntityManager.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;
                    if (!Helper.SpawnNPCIdentify(out var npc_id, name, pos, 1, 2, duration))
                    {
                        Output.CustomErrorMessage(ctx, $"Could not find specified unit: {name}");
                        return;
                    }

                    var Options = new SpawnOptions(true, type, quality, bloodconsume, false, default, true);
                    var NPCData = Cache.spawnNPC_Listen[npc_id];
                    NPCData.Options = Options;
                    if (NPCData.EntityIndex != 0) NPCData.Process = true;

                    Cache.spawnNPC_Listen[npc_id] = NPCData;

                    Output.SendSystemMessage(ctx, $"Spawning CustomNPC {name} at your position with LifeTime of {duration}s");
                }
            }
            else
            {
                Output.MissingArguments(ctx);
            }
        }
    }
}