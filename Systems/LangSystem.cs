using ProjectM;
using RPGMods.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Wetstone.API;

namespace RPGMods.Systems
{
    public class LangSystem
    {
        public static string t(string trsString, params object[] strs) {
            if (Database.langs.TryGetValue(trsString, out var translatedString))
            {
                return String.Format(translatedString, strs);
            }

            //if string dont exist, add it to json
            Database.langs.Add(trsString, trsString);
            LangSystem.Save(); 
            return String.Format(trsString, strs);


        }
        public static void Load()
        {
            if (!File.Exists("BepInEx/config/RPGMods/langs.json"))
            {
                FileStream stream = File.Create("BepInEx/config/RPGMods/langs.json");
                stream.Dispose();
            }
            string json = File.ReadAllText("BepInEx/config/RPGMods/langs.json");
            try
            {
                Database.langs = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                Plugin.Logger.LogWarning("Langs loaded");
            }
            catch
            {
                Database.langs = new Dictionary<string, string>(); 
                Plugin.Logger.LogWarning("Langs created");
            }
        }
        public static void Save() { 
           File.WriteAllText("BepInEx/config/RPGMods/langs.json", JsonSerializer.Serialize(Database.langs, Database.Pretty_JSON_options));
        }
    }
}
