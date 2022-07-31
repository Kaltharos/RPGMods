using ProjectM;
using RPGMods.Utils;
using System.Text.RegularExpressions;

namespace RPGMods.Commands
{
    [Command("save", Usage = "save [<name>]", Description = "Force the server to save the game as well as write RPGMods DB to a json file.")]
    public static class Save
    {
        public static void Initialize(Context ctx)
        {
            var args = ctx.Args;
            string name = "Manual Save";
            if (args.Length >= 1)
            {
                name = string.Join(' ', ctx.Args);
                if (name.Length > 50)
                {
                    Output.CustomErrorMessage(ctx, "Name is too long!");
                    return;
                }
                if (Regex.IsMatch(name, @"[^a-zA-Z0-9\x20]"))
                {
                    Output.CustomErrorMessage(ctx, "Name can only contain alphanumeric & space!");
                    return;
                }
            }

            Output.SendSystemMessage(ctx, $"Saving data....");
            //AutoSaveSystem.SaveDatabase();
            Plugin.Server.GetExistingSystem<TriggerPersistenceSaveSystem>().TriggerSave(SaveReason.ManualSave, name);
            Output.SendSystemMessage(ctx, $"Data save complete.");
        }
    }
}
