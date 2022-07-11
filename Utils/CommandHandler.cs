using BepInEx.Configuration;
using BepInEx.Logging;
using RPGMods.Systems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Unity.Entities;
using UnityEngine;
using Wetstone.API;
using Wetstone.Hooks;

namespace RPGMods.Utils
{
    public class CommandHandler
    {
        public string Prefix { get; set; }
        public string DisabledCommands { get; set; }
        public static Dictionary<string, bool> Permissions { get; set; }

        public static float delay_Cooldown = 5;

        public CommandHandler(string prefix, string disabledCommands)
        {
            Prefix = prefix;
            DisabledCommands = disabledCommands;
        }

        public void HandleCommands(VChatEvent ev, ManualLogSource Log, ConfigFile config)
        {
            if (!ev.Message.StartsWith(Prefix)) return;
            if (!VWorld.IsServer) return;

            string[] args = { };
            if (ev.Message.Contains(' '))
                args = ev.Message.Split(' ').Skip(1).ToArray();

            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0).ToArray();
            float getCurrentTime = Time.realtimeSinceStartup;
            foreach (Type type in types)
            {
                ev.Cancel();

                string command = ev.Message.Split(' ')[0].Remove(0, 1).ToLower();
                if (!NameExists(type, command, out var primary)) continue;
                if (DisabledCommands.Split(',').Any(x => x.ToLower() == primary)) continue;

                if (!ev.User.IsAdmin)
                {
                    var userSteamID = ev.User.PlatformId;
                    if (!Database.command_permission.TryGetValue(primary, out var commandPermission)) commandPermission = 100;
                    if (PermissionSystem.GetUserPermission(userSteamID) < commandPermission)
                    {
                        ev.User.SendSystemMessage($"<color=#ff0000ff>You do not have the required permissions to use that.</color>");
                        return;
                    }
                }

                Cache.command_Cooldown.TryGetValue(ev.User.PlatformId, out float last_Command);
                if (getCurrentTime < last_Command && !ev.User.IsAdmin)
                {
                    int wait = (int)Math.Ceiling(last_Command - getCurrentTime);
                    ev.User.SendSystemMessage($"<color=#ff0000ff>Please wait for {wait} second(s) before sending another command.</color>");
                    return;
                }
                Cache.command_Cooldown[ev.User.PlatformId] = getCurrentTime + delay_Cooldown;
                var cmd = type.GetMethod("Initialize");
                cmd.Invoke(null, new[] { new Context(Prefix, ev, Log, config, args, DisabledCommands) });
                return;
            }
            Output.InvalidCommand(ev);
        }

        private bool NameExists(Type type, string command, out string primary)
        {
            primary = "invalid";
            List<string> aliases = type.GetAttributeValue((CommandAttribute cmd) => cmd.Aliases);
            if (aliases.Any(x => x.ToLower() == command.ToLower()))
            {
                primary = aliases.First().ToLower();
                return true;
            }
            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CommandAttribute : Attribute
    {
        public List<string> Aliases;

        public string Name { get; set; }
        public string Usage { get; set; }
        public string Description { get; set; }
        public int ReqPermission { get; set; }

        public CommandAttribute(string name, string usage = "", string description = "None", int reqPermission = 100)
        {
            Name = name;
            Usage = usage;
            Description = description;
            ReqPermission = reqPermission;

            Aliases = new List<string>();
            Aliases.AddRange(Name.ToLower().Split(", "));
        }
    }

    public class Context
    {
        public string Prefix { get; set; }
        public VChatEvent Event { get; set; }
        public ManualLogSource Log { get; set; }
        public string[] Args { get; set; }
        public ConfigFile Config { get; set; }
        public EntityManager EntityManager { get; set; }

        public string[] DisabledCommands;

        public Context(string prefix, VChatEvent ev, ManualLogSource log, ConfigFile config, string[] args, string disabledCommands)
        {
            Prefix = prefix;
            Event = ev;
            Log = log;
            Args = args;
            Config = config;

            EntityManager = VWorld.Server.EntityManager;
            DisabledCommands = disabledCommands.Split(',');
        }
    }

    public static class AttributeExtensions
    {
        public static TValue GetAttributeValue<TAttribute, TValue>(this Type type, Func<TAttribute, TValue> valueSelector) where TAttribute : Attribute
        {
            var att = type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
            if (att != null)
            {
                return valueSelector(att);
            }
            return default;
        }
    }
}
