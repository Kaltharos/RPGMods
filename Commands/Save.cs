using RPGMods.Utils;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("Save", Usage = "save", Description = "Save all the current database memory to a json file.")]
    public static class Save
    {
        public static void Initialize(Context ctx)
        {
            ctx.Event.User.SendSystemMessage($"Saving data....");
            AutoSaveSystem.SaveDatabase();
            ctx.Event.User.SendSystemMessage($"Data save complete.");
        }
    }
}
