using RPGMods.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("help, h", Usage = "help", Description = "Shows a list of commands")]
    public static class Help
    {
        public static void Initialize(Context ctx)
        {
            List<string> commands = new List<string>();
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0).ToArray();
            try
            {
                if (types.Any(x => x.GetAttributeValue((CommandAttribute cmd) => cmd.Aliases.First() == ctx.Args[0].ToLower())))
                {
                    var type = types.First(x => x.GetAttributeValue((CommandAttribute cmd) => cmd.Aliases.First() == ctx.Args[0].ToLower()));

                    List<string> aliases = type.GetAttributeValue((CommandAttribute cmd) => cmd.Aliases);
                    if (ctx.DisabledCommands.Any(x => x.ToLower() == aliases.First().ToLower())) return;
                    string usage = type.GetAttributeValue((CommandAttribute cmd) => cmd.Usage);
                    string description = type.GetAttributeValue((CommandAttribute cmd) => cmd.Description);
                    CommandHandler.Permissions.TryGetValue(aliases[0], out bool adminOnly);

                    ctx.Event.User.SendSystemMessage($"Help for <color=#00ff00ff>{ctx.Prefix}{aliases.First()}</color>");
                    ctx.Event.User.SendSystemMessage($"<color=#ffffffff>Aliases: {string.Join(", ", aliases)}</color>");
                    if (adminOnly) ctx.Event.User.SendSystemMessage($"<color=#ffffffff>Description: <color=#ff0000ff>[ADMIN]</color> {description}</color>");
                    else ctx.Event.User.SendSystemMessage($"<color=#ffffffff>Description: {description}</color>");
                    ctx.Event.User.SendSystemMessage($"<color=#ffffffff>Usage: {ctx.Prefix}{usage}</color>");
                }
                else
                {
                    ctx.Event.User.SendSystemMessage($"Specified command not found.");
                }
            }
            catch
            {
                ctx.Event.User.SendSystemMessage("List of all commands:");
                foreach (Type type in types)
                {
                    List<string> aliases = type.GetAttributeValue((CommandAttribute cmd) => cmd.Aliases);
                    if (ctx.DisabledCommands.Any(x => x.ToLower() == aliases.First().ToLower())) continue;
                    string description = type.GetAttributeValue((CommandAttribute cmd) => cmd.Description);
                    CommandHandler.Permissions.TryGetValue(aliases[0], out bool adminOnly);

                    string s = "";
                    bool send = false;
                    if (adminOnly && ctx.Event.User.IsAdmin)
                    {
                        s = $"<color=#00ff00ff>{ctx.Prefix}{string.Join(", ", aliases)}</color> - <color=#ff0000ff>[ADMIN]</color> <color=#ffffffff>{description}</color>";
                        //s = $"<color=#00ff00ff>{ctx.Prefix}{aliases.First()}/{string.Join(", ", aliases)}</color> - <color=#ff0000ff>[ADMIN]</color> <color=#ffffffff>{description}</color>";
                        send = true;
                    }
                    else if (!adminOnly)
                    {
                        s = $"<color=#00ff00ff>{ctx.Prefix}{string.Join(", ", aliases)}</color> - <color=#ffffffff>{description}</color>";
                        //s = $"<color=#00ff00ff>{ctx.Prefix}{aliases.First()}/{string.Join(", ", aliases)}</color> - <color=#ffffffff>{description}</color>";
                        send = true;
                    }
                    if (send) ctx.Event.User.SendSystemMessage(s);
                }
            }
        }

        public static void LoadPermissions()
        {
            if (!File.Exists("BepInEx/config/RPGMods/permissions.json")) File.Create("BepInEx/config/RPGMods/permissions.json");
            string json = File.ReadAllText("BepInEx/config/RPGMods/permissions.json");
            try
            {
                CommandHandler.Permissions = JsonSerializer.Deserialize<Dictionary<string, bool>>(json);
            }
            catch
            {
                CommandHandler.Permissions = new Dictionary<string, bool>();
            }
        }
    }
}
