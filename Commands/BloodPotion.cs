using ProjectM;
using RPGMods.Utils;
using Unity.Entities;

namespace RPGMods.Commands
{
    [Command("bloodpotion, bp", Usage = "bloodpotion <Type> [<Quality>]", Description = "Creates a Potion with specified Blood Type, Quality and Value")]
    public static class BloodPotion
    {
        public static void Initialize(Context ctx)
        {
            if (ctx.Args.Length != 0)
            {
                try
                {
                    Helper.BloodType type = Helper.BloodType.Warrior;
                    float quality = 100;

                    if (ctx.Args.Length >= 1) type = Helper.GetBloodTypeFromName(ctx.Args[0]);
                    if (ctx.Args.Length >= 2)
                    {
                        quality = float.Parse(ctx.Args[1]);
                        if (float.Parse(ctx.Args[1]) < 0) quality = 0;
                        if (float.Parse(ctx.Args[1]) > 100) quality = 100;
                    }

                    Entity entity = Helper.AddItemToInventory(ctx, new PrefabGUID(828432508), 1);
                    var blood = ctx.EntityManager.GetComponentData<StoredBlood>(entity);
                    blood.BloodQuality = quality;
                    blood.BloodType = new PrefabGUID((int)type);
                    ctx.EntityManager.SetComponentData(entity, blood);

                    Output.SendSystemMessage(ctx.Event, $"Got Blood Potion Type <color=#ffff00ff>{type}</color> with <color=#ffff00ff>{quality}</color>% quality");
                }
                catch
                {
                    Output.InvalidArguments(ctx);
                }

            }
            else
            {
                Output.MissingArguments(ctx);
            }
        }
    }
}
