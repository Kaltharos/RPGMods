using ProjectM;
using RPGMods.Utils;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("Save", Usage = "save", Description = "Force the server to save the game as well as write RPGMods DB to a json file.")]
    public static class Save
    {
        public static void Initialize(Context ctx)
        {
            ctx.Event.User.SendSystemMessage($"Saving data....");
            //AutoSaveSystem.SaveDatabase();
            VWorld.Server.GetExistingSystem<TriggerPersistenceSaveSystem>().TriggerSave(SaveReason.ManualSave, "Force Game Save");
            ctx.Event.User.SendSystemMessage($"Data save complete.");
        }
    }
}
