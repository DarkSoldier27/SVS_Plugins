using ADV;
using BepInEx;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Collections.Generic;
using System.IO;
using System;
using BepInEx.Logging;


namespace SVS_ADVLoader
{
    internal class ADVLoader
    {
        private static readonly Dictionary<string, string> customADVList = new Dictionary<string, string>();
        private static int charaID = -1;
        public static void PreADVLoadInit(TextScenario scenario, string bundle, string asset)
        {
            if (scenario is null) return;
            if (ADVLoaderPlugin.GetDisplayCurrentADV()) ADVLoaderPlugin.Log.Log(LogLevel.Message, $"Current ADV: {asset}");
            if (scenario.CurrentHeroine != null) charaID = scenario.CurrentHeroine.personality;
            ADVLoaderParam.ResetADVFlags();
        }
        public static bool SideLoadADV(OpenData openData, string bundle, string asset)
        {
            if (openData is null) return true;
            if (ADVExtractor.IsExtraction()) return true;
            if (!ADVLoaderPlugin.GetSideloadADV()) return true;
            if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Executing ADVLoader");           
            
            string filePath = "";
            string[] advDirectories = [];
            int type = GetAssetType(asset, bundle);

            //Get the type of the ADV (Character, Common or Other)
            if (type > -1 && type < 30)
            {
                filePath = Path.Combine(Paths.GameRootPath, ADVLoaderParam.GetADVFolders("chara"));
                if (!Directory.Exists(filePath)) return true;
                advDirectories = Directory.GetDirectories(filePath, $"{charaID}_*", SearchOption.TopDirectoryOnly);
                if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"ADV Type: Chara");
            }
            else if (type > 30)
            {
                filePath = Path.Combine(Paths.GameRootPath, ADVLoaderParam.GetADVFolders("common"));
                if (!Directory.Exists(filePath)) return true;
                advDirectories = Directory.GetDirectories(filePath, "*", SearchOption.TopDirectoryOnly);
                if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"ADV Type: Common");
            }
            else
            {
                if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"ADV Type: Other");
            }

            if (advDirectories.Length == 0)
            {
                ADVLoaderPlugin.Log.LogInfo($"Could not find custom ADV folders. ADV will load normally");
                return true;
            } 
            if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Found custom ADV folders");

            string[] advFiles = Directory.GetFiles(advDirectories[0], $"{asset}*.json", SearchOption.AllDirectories);
            if (advFiles.Length == 0)
            {
                ADVLoaderPlugin.Log.LogInfo($"Could not find custom ADV file. ADV will load normally");
                return true;
            } 
            if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Found ADV files for {asset}");

            Dictionary<string,List<ADVLoaderParam.ScenarioParam>> scenarioList = new Dictionary<string,List<ADVLoaderParam.ScenarioParam>>();
            if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Deserializing Scenarios");
            foreach (var adv in advFiles)
            {
                var scenarios = ADVLoaderParam.DeserializeScenario(adv);
                if (scenarios != null)
                {
                    if (scenarios.Count > 0)
                    {                     
                        string fileName = Path.GetFileName(adv);
                        if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"File Name: {fileName}");

                        if (fileName == $"{asset}.json")
                        {
                            fileName = fileName.Replace(".json", "");
                            if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"File Name without extension: {fileName}");
                            if (!scenarioList.ContainsKey(fileName)) scenarioList.Add(fileName, scenarios);
                        }
                        else 
                        {
                            fileName = fileName.Replace($"{asset}", "");
                            fileName = fileName.Replace(".json", "");
                            fileName = fileName.Replace("_", "");
                            if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"File Name Tag: {fileName}");
                            if (!scenarioList.ContainsKey(fileName)) scenarioList.Add(fileName, scenarios);
                        }                           
                    } 
                } 
            }

            if (scenarioList.Count == 0)
            {
                ADVLoaderPlugin.Log.LogInfo($"Failed to deserialize files");
                return true;
            } 
            if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Deserialization DONE");

            string advAsset = "";
            foreach (var scenario in scenarioList)
            {
                if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Searching flag {scenario.Key}");

                if (scenario.Key.ToLower() == "test")
                {
                    if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Found default flag: {advAsset}");
                    advAsset = scenario.Key;
                }
                else if (ADVLoaderParam.GetADVFlag(scenario.Key))
                {
                    if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Found active flag: {scenario.Key}");
                    advAsset = scenario.Key;
                    break;
                }
                 
            }
            if (advAsset == "") advAsset = asset;
            
            if (!scenarioList.ContainsKey(advAsset)) return true;
            ScenarioCommand[] scenarioCommands = new ScenarioCommand[scenarioList[advAsset].Count];
            
            int sceneCount = 0;
            int index = 0;
            foreach (var sceneCommand in scenarioList[advAsset])
            {
                if (Enum.TryParse(sceneCommand.Command, out Command advCommand))
                {
                    if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Valid Command: {advCommand.ToString()}");
                    scenarioCommands[index] = new ScenarioCommand()
                    { _version = sceneCommand.Version, _multi = sceneCommand.Multi, _command = advCommand };
                    if (sceneCommand.Args != null)
                    {
                        scenarioCommands[index]._args = new Il2CppStringArray(sceneCommand.Args.Length);
                        for (int i = 0; i < scenarioCommands[index].Args.Length; i++)
                        {
                            scenarioCommands[index]._args[i] = sceneCommand.Args[i];
                        }
                    }
                    scenarioCommands[index].Hash = scenarioCommands[index].GetHashCode();
                    sceneCount++;
                }
                else
                {
                    if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Invalid command for Scenario {index}, setting a default scenario");
                    scenarioCommands[index] = new ScenarioCommand()
                    { _version = 0, _multi = false, _command = Command.None};
                }
                index++;
                if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Loading Scenario Command {sceneCount}: {sceneCommand.Version},{sceneCommand.Multi},{sceneCommand.Command},{sceneCommand.Args.Length}");
            }
            if (sceneCount > 0)
            {
                openData._data = new ScenarioData() { _list = new Il2CppReferenceArray<ScenarioCommand>(scenarioCommands) };
                if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Custom ADV Loaded");
                return false;
            }
            return true;
        }      
        public static int GetAssetType(string asset, string bundle)
        {
            string[] names = asset.Split('_');
            switch (names[0])
            {
                case "a":
                    if (bundle.Contains("common")) return 100;
                    return 0;
                case "abduction":
                    return 0;
                case "all":
                    return 1;
                case "ask":
                    return 2;
                case "consult":
                    return 3;
                case "daily":
                    return 4;
                case "erotic":
                    return 5;
                case "h":
                    return 6;
                case "intro":
                    return 7;
                case "kiss":
                    return 8;
                case "love":
                    return 9;
                case "move":
                    return 10;
                case "n":
                    if (bundle.Contains("common"))
                    {
                        if (asset.Contains("n_H")) return 102;
                        return 101;
                    } 
                    return 11;
                case "no":
                    return 12;
                case "o":
                    return 13;
                case "p":
                    return 103;
                case "pc":
                    return 200;
                case "person":
                    return 14;
                case "r":
                    return 15;
                case "skinship":
                    return 16;
                case "special":
                    return 17;
                case "t":
                    return 18;
                case "talk":
                    return 19;
                case "talk2":
                    return 20;
                case "touch":
                    return 21;
                case "u":
                    return 22;
                case "w":
                    return 23;
                case "yn":
                    return 24;
                case "i":
                    if (asset.Contains("i_a")) return 124;
                    if (asset.Contains("i_p")) return 124;
                    return 25;
                case "e":
                    return 26;
                default:
                    switch (asset)
                    {
                        case "s_83": return 37;
                        case "s_84": return 38;
                        case "s_85": return 39;
                        case "s_86": return 40;
                        case "a_25_1": return 41;
                        case "p_25_1": return 42;
                        case "a_43_1": return 43;
                        case "p_43_1": return 44;
                        case "timechange": return 45;
                        default:
                            if (asset.Contains("abductionreport")) return 400;
                            if (asset.Contains("board")) return 500;
                            if (asset.Contains("dayend")) return 50;
                            if (asset.Contains("evaluation")) return 600;
                            if (asset.Contains("morning")) return 60;
                            if (asset.Contains("nightcrawling")) return 61;
                            if (asset.Contains("nightlife")) return 62;
                            return -1;
                    }
            }
        }

        public static void CreateADVLoaderDirectory()
        {
            string[] folders = [Path.Combine(Paths.GameRootPath, "abdata/mods/ADVLoader/Scenario/Chara/"), Path.Combine(Paths.GameRootPath, "abdata/mods/ADVLoader/Scenario/Common/")];
            if (!Directory.Exists(folders[0])) Directory.CreateDirectory(folders[0]);
            if (!Directory.Exists(folders[1])) Directory.CreateDirectory(folders[1]);
        }
    }
}
