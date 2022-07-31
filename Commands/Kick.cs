using RPGMods.Utils;

namespace RPGMods.Commands
{
    [Command("kick", Usage = "kick <playername>", Description = "Kick the specified player out of the server.")]
    public static class Kick
    {
        public static void Initialize(Context ctx)
        {
            var args = ctx.Args;
            if (args.Length < 1)
            {
                Output.MissingArguments(ctx);
                return;
            }

            var name = args[0];
            if (Helper.FindPlayer(name, true, out _, out var targetUserEntity))
            {
                Helper.KickPlayer(targetUserEntity);
                Output.SendSystemMessage(ctx, $"Player \"{name}\" has been kicked from server.");
            }
            else
            {
                Output.CustomErrorMessage(ctx, "Specified player not found.");
            }
        }
    }
}
