using RPGMods.Utils;

namespace RPGMods.Commands
{
    [Command("shutdown, quit, exit", Usage = "shutdown", Description = "Trigger the exit signal & shutdown the server.")]
    public static class Shutdown
    {
        public static void Initialize(Context ctx)
        {
            UnityEngine.Application.Quit();
        }
    }
}
