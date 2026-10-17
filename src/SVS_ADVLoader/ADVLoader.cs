using ADV;
using BepInEx;
using BepInEx.Logging;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SVS_ADVLoader
{
    public class ADVLoader
    {
        private static int personalityID = -1;
        public enum ScenarioTypeADV
        {
            None = 0,
            Chara = 1,
            Common = 2,
            External = 3,
        }
        public static void GetPersonalityIDFromScenario(TextScenario scenario)
        {
            if (scenario == null)
            {
                personalityID = -1;
                return;
            }
            if (scenario.CurrentHeroine != null)
            {
                personalityID = scenario.CurrentHeroine.personality;
            }
            else personalityID = -1;
        }
        public static void GetLowCharaPersonalityFromBundleName(string bundle)
        {
            if (bundle == "")
            {
                personalityID = -1;
                return;
            }
            string[] searchPersoID = bundle.Split("/");
            foreach (string splitSearch in searchPersoID)
            {
                if (splitSearch.StartsWith("c"))
                {
                    string persoID = splitSearch.Replace("c", "");
                    if (int.TryParse(persoID, out int id))
                    {
                        personalityID = id;
                        return;
                    }
                }
            }
        }
        public static bool SideLoadADV(OpenData openData, string bundle, string asset, bool isLowPolyADV, out Il2CppSystem.Collections.Generic.List<ScenarioCommand> lowPolyScenarios)
        {
            lowPolyScenarios = new();
            if (ADVManager.Instance == null) return true;
            if (openData is null && !isLowPolyADV) return true;
            if (ADVExtractor.IsExtraction()) return true;
            if (!ADVLoaderPlugin.GetSideloadADV()) return true;
            if (isLowPolyADV)
            {
                if (ADVLoaderPlugin.GetDisplayCurrentADV()) ADVLoaderPlugin.Log.Log(LogLevel.Message, $"Current NPC ADV: {asset}");
                else if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Current NPC ADV: {asset}");
            }
            else
            {
                if (ADVLoaderPlugin.GetDisplayCurrentADV()) ADVLoaderPlugin.Log.Log(LogLevel.Message, $"Current ADV: {asset}");
                else if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Current ADV: {asset}");
            }

            var advScene = ADVManager.Instance.gameObject.GetComponentInChildren<ADVScene>();
            if (isLowPolyADV) GetLowCharaPersonalityFromBundleName(bundle);
            else 
            {
                if (advScene != null)
                {
                    if (advScene._scenario != null)
                    {
                        GetPersonalityIDFromScenario(advScene._scenario);
                    }
                    else return true;
                }
                else return true;
            }

            if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Executing ADVLoader");
            if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Personality: {personalityID}");

            string filePath;
            string[] advDirectories = [];

            ScenarioTypeADV sceneType = GetAssetType(asset, bundle);
            switch (sceneType)
            {
                case ScenarioTypeADV.Chara:
                    filePath = Path.Combine(Paths.GameRootPath, ADVLoaderParam.GetADVFolders("chara"));
                    if (!Directory.Exists(filePath)) return true;
                    advDirectories = Directory.GetDirectories(filePath, $"{personalityID}_*", SearchOption.TopDirectoryOnly);
                    if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"ADV Type: Chara");
                    break;
                case ScenarioTypeADV.Common:
                    filePath = Path.Combine(Paths.GameRootPath, ADVLoaderParam.GetADVFolders("common"));
                    if (!Directory.Exists(filePath)) return true;
                    advDirectories = [filePath];
                    if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"ADV Type: Common");
                    break;
                case ScenarioTypeADV.None:
                case ScenarioTypeADV.External:
                    filePath = Path.Combine(Paths.GameRootPath, ADVLoaderParam.GetADVFolders("other"));
                    if (!Directory.Exists(filePath)) return true;
                    advDirectories = [filePath];
                    if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"ADV Type: Other");
                    break;
            }

            if (advDirectories.Length == 0)
            {
                ADVLoaderPlugin.Log.LogInfo($"Could not find custom ADV folders. ADV will load normally");
                if (!isLowPolyADV)
                {
                    if (!VerifyADVAsset(bundle, asset))
                    {
                        SetErrorADV(openData, asset, sceneType);
                        return false;
                    }                  
                }
                return true;
            }
            if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Found custom ADV folders");

            //Finding the custom ADV file.
            string[] advFiles = [];
            foreach (var advFile in advDirectories)
            {
                advFiles = Directory.GetFiles(advFile, $"{asset}*.json", SearchOption.AllDirectories);
                if (advFiles.Length > 0) break;
            }
            
            if (advFiles.Length == 0)
            {
                ADVLoaderPlugin.Log.LogInfo($"Could not find custom ADV file. ADV will load normally");
                if (!isLowPolyADV)
                {
                    if (!VerifyADVAsset(bundle, asset))
                    {
                        SetErrorADV(openData, asset, sceneType);
                        return false;
                    }
                }
                return true;
            }
            if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Found ADV files for {asset}");

            //Reading the Custom ADV file.
            Dictionary<string, List<ADVLoaderParam.ScenarioParam>> scenarioList = new Dictionary<string, List<ADVLoaderParam.ScenarioParam>>();
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
                if (!isLowPolyADV)
                {
                    if (!VerifyADVAsset(bundle, asset))
                    {
                        SetErrorADV(openData, asset, sceneType);
                        return false;
                    }
                }
                return true;
            }
            if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Deserialization DONE");

            //Findig active flags.
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
            
            //Loading the ADV
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
                    switch (scenarioCommands[index]._command)
                    {
                        case Command.LowMotionPlay:
                            break;
                        case Command.Motion:
                            if (!ADVLoaderAnimationHandler.SetAnimationIfMissing(advScene._scenario, scenarioCommands[index]._args[1]))
                            {
                                scenarioCommands[index]._args[1] = "0";
                            }
                            break;
                        case Command.Expression:
                            break;
                    }
                    sceneCount++;
                }
                else
                {
                    if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Invalid command for Scenario {index}, setting a default scenario");
                    scenarioCommands[index] = new ScenarioCommand()
                    { _version = 0, _multi = false, _command = Command.None };
                }
                index++;
                if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Loading Scenario Command {sceneCount}: {sceneCommand.Version},{sceneCommand.Multi},{sceneCommand.Command},{sceneCommand.Args.Length}");
            }
            if (sceneCount > 0)
            {
                if (isLowPolyADV)
                {
                    for (int i = 0; i < scenarioCommands.Length; i++)
                    {
                        lowPolyScenarios.Add(scenarioCommands[i]);
                    }
                    if (lowPolyScenarios.Count > 0)
                    {
                        if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Custom NPC ADV Loaded");                     
                        return false;
                    }
                }
                else
                {
                    openData._data = new ScenarioData() { _list = new Il2CppReferenceArray<ScenarioCommand>(scenarioCommands) };
                    if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo($"Custom ADV Loaded");
                    return false;
                }                
            }
            return true;
        }
        public static ScenarioTypeADV GetAssetType(string asset, string bundle)
        {
            string[] names = asset.Split('_');
            switch (names[0])
            {
                case "a":
                    if (asset == "a_25_1") return ScenarioTypeADV.Common;
                    if (asset == "a_43_1") return ScenarioTypeADV.Common;
                    if (bundle.Contains("common")) return ScenarioTypeADV.Common;
                    if (int.TryParse(names[1], out int actionID))
                    {
                        if (actionID > 1) return ScenarioTypeADV.Common;
                    }
                    return ScenarioTypeADV.Chara;
                case "abduction":
                case "all":
                case "ask":
                case "consult":
                case "daily":
                case "erotic":
                case "h":
                case "intro":
                case "kiss":
                case "love":
                case "move":
                    return ScenarioTypeADV.Chara;
                case "n":
                    if (bundle.Contains("common"))
                    {
                        if (asset.Contains("n_H")) return ScenarioTypeADV.Common;
                        return ScenarioTypeADV.Common;
                    }
                    return ScenarioTypeADV.Chara;
                case "no":
                case "npc":
                case "npcr":
                case "o":
                    return ScenarioTypeADV.Chara;
                case "p":
                case "pc":
                    return ScenarioTypeADV.Common;
                case "person":
                case "r":
                case "skinship":
                case "special":
                case "t":
                case "talk":
                case "talk2":
                case "touch":
                case "u":
                case "w":
                case "yn":
                    return ScenarioTypeADV.Chara;
                case "i":
                    if (asset.Contains("i_a")) return ScenarioTypeADV.Common;
                    if (asset.Contains("i_p")) return ScenarioTypeADV.Common;
                    return ScenarioTypeADV.Chara;
                case "e":
                case "chara":
                    return ScenarioTypeADV.Chara;
                default:
                    switch (asset)
                    {
                        case "s_83":
                        case "s_84":
                        case "s_85":
                        case "s_86":
                        case "timechange": return ScenarioTypeADV.Common;
                        default:
                            if (asset.Contains("abductionreport")) return ScenarioTypeADV.Common;
                            if (asset.Contains("board")) return ScenarioTypeADV.Common;
                            if (asset.Contains("dayend")) return ScenarioTypeADV.Common;
                            if (asset.Contains("evaluation")) return ScenarioTypeADV.Common;
                            if (asset.Contains("morning")) return ScenarioTypeADV.Common;
                            if (asset.Contains("nightcrawling")) return ScenarioTypeADV.Common;
                            if (asset.Contains("nightlife")) return ScenarioTypeADV.Common;
                            return ScenarioTypeADV.None;
                    }
            }
        }
        public static void CreateADVLoaderDirectory()
        {
            string[] folders = [Path.Combine(Paths.GameRootPath, "abdata/mods/ADVLoader/Scenario/Chara"), 
                                Path.Combine(Paths.GameRootPath, "abdata/mods/ADVLoader/Scenario/Common"),
                                Path.Combine(Paths.GameRootPath, "abdata/mods/ADVLoader/Scenario/Other"),
                                Path.Combine(Paths.GameRootPath, "abdata/mods/ADVLoader/List")];
            if (!Directory.Exists(folders[0])) Directory.CreateDirectory(folders[0]);
            if (!Directory.Exists(folders[1])) Directory.CreateDirectory(folders[1]);
            if (!Directory.Exists(folders[2])) Directory.CreateDirectory(folders[2]);
        }
        private static bool VerifyADVAsset(string bundle, string asset)
        {
            var filePath = Path.Combine(Paths.GameRootPath, "abdata/" + bundle);
            if (File.Exists(filePath))
            {
                if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo("Verifying ADV Asset");
                AssetBundle assetADV = AssetBundle.LoadFromFile(filePath);
                if (assetADV != null)
                {
                    if (assetADV.Contains(asset))
                    {
                        ADVLoaderPlugin.Log.LogInfo($"Found asset [{asset}] in bundle");
                        assetADV.Unload(true);
                        return true;
                    } 
                }
            }
            ADVLoaderPlugin.Log.LogInfo($"ADV Asset [{asset}] not found in bundle");
            return false;
        }
        private static void SetErrorADV(OpenData openData, string assetName, ScenarioTypeADV sceneType)
        {
            string auxTextError = "";
            switch (sceneType)
            {
                case ScenarioTypeADV.Chara:
                    auxTextError = $"\nFor Personality ID: {personalityID}";
                    break;
                case ScenarioTypeADV.Common:
                    auxTextError = $"\nFor ADV Type: Common";
                    break;
                case ScenarioTypeADV.None:
                case ScenarioTypeADV.External:
                    auxTextError = $"\nFor ADV Type: Other";
                    break;
            }
            ScenarioCommand[] scenarioCommands = new ScenarioCommand[2];
            Il2CppStringArray defArgs = new Il2CppStringArray(5);
            defArgs[0] = "";
            defArgs[1] = $"<color=\"red\">ERROR! <color=\"white\">Failed to load or Missing ADV file! [{assetName}]{auxTextError}";
            defArgs[2] = $"<color=\"red\">ERROR! <color=\"white\">Failed to load or Missing ADV file! [{assetName}]{auxTextError}";
            defArgs[3] = $"<color=\"red\">ERROR! <color=\"white\">Failed to load or Missing ADV file! [{assetName}]{auxTextError}";
            defArgs[4] = $"<color=\"red\">ERROR! <color=\"white\">Failed to load or Missing ADV file! [{assetName}]{auxTextError}";

            scenarioCommands[0] = new ScenarioCommand()
            { _version = 0, _multi = false, _command = Command.Text, _args = defArgs };
            scenarioCommands[1] = new ScenarioCommand()
            { _version = 0, _multi = false, _command = Command.Close, _args = new Il2CppStringArray(0) };

            openData._data = new ScenarioData() { _list = new Il2CppReferenceArray<ScenarioCommand>(scenarioCommands) };
            ADVLoaderPlugin.Log.LogInfo($"ERROR! Failed to load or Missing ADV file: {assetName}{auxTextError}");
        }
    }
}
