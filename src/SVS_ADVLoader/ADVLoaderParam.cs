using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Linq;

namespace SVS_ADVLoader
{
    internal class ADVLoaderParam
    {
        private static readonly Dictionary<string, string> _ADVFolderList = new Dictionary<string, string>()
        {
            ["chara"] = "abdata/mods/ADVLoader/Scenario/Chara/",
            ["common"] = "abdata/mods/ADVLoader/Scenario/Common/",
            ["other"] = "abdata/mods/ADVLoader/Scenario/Other/",
        };
        private static Dictionary<string, bool> _ADVFlagList = new Dictionary<string, bool>();
        public class ADVListParam
        {
            public string ListName { get; set; } = "Default";
            public string ADVFolder { get; set; } = "abdata\\mods\\ADVLoader\\Scenario";

            //public List<ScenarioParam> Scenario { get; set; }
        }
        public class ScenarioParam
        {
            public int Version { get; set; } = 0;
            public bool Multi {  get; set; } = false;
            public string Command { get; set; } = "";
            public string[] Args { get; set; }
        }

        public static List<ADVListParam> DeserializeADVParam(string filePath)
        {
            var sr = new StreamReader(File.OpenRead(filePath));
            var json = sr.ReadToEnd();
            sr.Close();
            try
            {
                return JsonSerializer.Deserialize<List<ADVListParam>>(json);
            }
            catch (Exception ex)
            {
                ADVLoaderPlugin.Log.LogInfo($"Failed to load json File: {ex}");
                return null;
            }
        }
        public static List<ScenarioParam> DeserializeScenario(string filePath)
        {
            var sr = new StreamReader(File.OpenRead(filePath));
            var json = sr.ReadToEnd();
            sr.Close();
            try
            {
                return JsonSerializer.Deserialize<List<ScenarioParam>>(json);
            }
            catch (Exception ex)
            {
                ADVLoaderPlugin.Log.LogInfo($"Failed to load json File: {ex}");
                return null;
            }
        }

        public static void AddADVList(string key, string value)
        {
            if (!_ADVFolderList.ContainsKey(key)) _ADVFolderList.Add(key, value);
        }

        public static string GetADVFolders(string folderType)
        {
            if (_ADVFolderList.ContainsKey(folderType)) return _ADVFolderList[folderType];
            return "";
        }

        public static void SetFlag(string flagName, bool flagOn)
        {
            ADVLoaderPlugin.Log.LogInfo($"{_ADVFlagList.Count}");
            if (_ADVFlagList.ContainsKey(flagName)) _ADVFlagList[flagName] = flagOn;
            else _ADVFlagList.Add(flagName, flagOn);
        }
        public static bool GetADVFlag(string flagName)
        {
            if(_ADVFlagList.ContainsKey(flagName)) return _ADVFlagList[flagName];
            return false;
        }
        public static void ResetADVFlags()
        {
            if (_ADVFlagList.Count > 0)
            {
                foreach (var key in _ADVFlagList.Keys.ToList())
                {
                    _ADVFlagList[key] = false;
                }
            }
        }
    }
}
