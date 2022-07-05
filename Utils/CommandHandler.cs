using BepInEx.Configuration;
using BepInEx.Logging;
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
                if (DisabledCommands.Split(',').Any(x => x.ToLower() == command)) continue;
                if (!NameExists(type, command)) continue;
                
                Permissions.TryGetValue(command, out bool isAdminOnly);
                if (IsNotAdmin(type, ev, isAdminOnly))
                {
                    ev.User.SendSystemMessage($"<color=#ff0000ff>You do not have the required permissions to use that.</color>");
                    return;
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

                Log.LogInfo($"[CommandHandler] {ev.User.CharacterName} used command: {command.ToLower()}");
                return;
            }
            Output.InvalidCommand(ev);
        }

        private bool NameExists(Type type, string command)
        {
            List<string> aliases = type.GetAttributeValue((CommandAttribute cmd) => cmd.Aliases);
            if (aliases.Any(x => x.ToLower() == command.ToLower()))
            {
                if (!Permissions.ContainsKey(aliases[0])) Permissions.Add(aliases[0], false);
                return true;
            }
            return false;
        }

        private bool IsNotAdmin(Type type, VChatEvent ev, bool isAdminOnly)
        {
            return isAdminOnly && !ev.User.IsAdmin;
        }

        public static void LoadPermissions()
        {
            if (!File.Exists("BepInEx/config/RPGMods/permissions.json")) File.Create("BepInEx/config/RPGMods/permissions.json");
            string json = File.ReadAllText("BepInEx/config/RPGMods/permissions.json");
            try
            {
                Permissions = JsonSerializer.Deserialize<Dictionary<string, bool>>(json);
                Plugin.Logger.LogWarning("Permissions DB Populated");
            }
            catch
            {
                Permissions = new Dictionary<string, bool>();
                Plugin.Logger.LogWarning("Permission DB Created.");
            }
        }

        public static void SavePermissions()
        {
            File.WriteAllText("BepInEx/config/RPGMods/permissions.json", JsonSerializer.Serialize(Permissions, Database.Pretty_JSON_options));
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CommandAttribute : Attribute
    {
        public List<string> Aliases;

        public string Name { get; set; }
        public string Usage { get; set; }
        public string Description { get; set; }
        public bool AdminOnly { get; set; }

        public CommandAttribute(string name, string usage = "", string description = "None", bool adminOnly = false)
        {
            Name = name;
            Usage = usage;
            Description = description;
            AdminOnly = adminOnly;

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
