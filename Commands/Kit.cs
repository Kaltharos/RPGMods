using ProjectM;
using RPGMods.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RPGMods.Commands
{
    [Command("kit", "kit <Name>", "Gives you a previously specified set of items.")]
    public static class Kit
    {
        private static List<ItemKit> kits;

        public static void Initialize(Context ctx)
        {
            if (ctx.Args.Length < 1)
            {
                Output.SendSystemMessage(ctx, $"Kit name missing.");
                return;
            }

            string name = string.Join(' ', ctx.Args);

            try
            {
                ItemKit kit = kits.First(x => x.Name.ToLower() == name.ToLower());
                foreach (var guid in kit.PrefabGUIDs)
                {
                    Helper.AddItemToInventory(ctx, new PrefabGUID(guid.Key), guid.Value);
                }
                Output.SendSystemMessage(ctx, $"You got the kit: <color=#ffff00>{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name)}</color>");
            }
            catch
            {
                Output.SendSystemMessage(ctx, $"Kit doesn't exist.");
                return;
            }
        }

        public static void LoadKits()
        {
            if (!File.Exists("BepInEx/config/RPGMods/kits.json"))
            {
                var stream = File.Create("BepInEx/config/RPGMods/kits.json");
                stream.Dispose();
            }
            string json = File.ReadAllText("BepInEx/config/RPGMods/kits.json");
            try
            {
                kits = JsonSerializer.Deserialize<List<ItemKit>>(json);
                Plugin.Logger.LogWarning("Kits DB Populated.");
            }
            catch
            {
                kits = new List<ItemKit>();
                Plugin.Logger.LogWarning("Kits DB Created.");
            }
        }

        public static void SaveKits()
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                IncludeFields = true
            };
            File.WriteAllText("BepInEx/config/RPGMods/kits.json", JsonSerializer.Serialize(kits, options));
        }
    }
}
