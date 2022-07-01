using System;
using System.Collections.Generic;
using System.Text;
using Wetstone.API;
using Wetstone.Hooks;

namespace RPGMods.Utils
{
    public static class CommandOutput
    {
        public static void CustomErrorMessage(Context ctx, string message)
        {
            ctx.Event.User.SendSystemMessage($"<color=#ff0000ff>{message}</color>");
        }

        public static void InvalidCommand(VChatEvent ev)
        {
            ev.User.SendSystemMessage($"<color=#ff0000ff>Invalid command.</color>");
        }

        public static void InvalidArguments(Context ctx)
        {
            ctx.Event.User.SendSystemMessage($"<color=#ff0000ff>Invalid command parameters. Check {ctx.Prefix}help for more information.</color>");
        }

        public static void MissingArguments(Context ctx)
        {
            ctx.Event.User.SendSystemMessage($"<color=#ff0000ff>Missing command parameters. Check {ctx.Prefix}help for more information.</color>");
        }
    }
}
