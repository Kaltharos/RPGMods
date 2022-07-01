using ProjectM;
using RPGMods.Utils;
using System.Globalization;
using System.Linq;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("give, g", Usage = "give <itemname> [<amount>]", Description = "Adds specified items to your inventory")]
    public static class Give
    {
        public static void Initialize(Context ctx)
        {
            string name = string.Join(' ', ctx.Args);
            int amount = 1;
            if (int.TryParse(ctx.Args.Last(), out int a))
            {
                name = string.Join(' ', ctx.Args.SkipLast(1));
                amount = a;
            }
            PrefabGUID guid = CommandHelper.GetGUIDFromName(name);
            if (guid.GuidHash == 0)
            {
                CommandOutput.CustomErrorMessage(ctx, "Could not find specified item name.");
                return;
            }

            CommandHelper.AddItemToInventory(ctx, guid, amount);
            ctx.Event.User.SendSystemMessage($"You got <color=#ffff00ff>{amount} {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name)}</color>");
        }
    }
}
