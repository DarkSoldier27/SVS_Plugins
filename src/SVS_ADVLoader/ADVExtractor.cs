using ADV;
using BepInEx;
using BepInEx.Logging;
using Il2CppSystem.Collections.Generic;
using SV;
using System;
using System.IO;
using UnityEngine;

namespace SVS_ADVLoader
{
    internal class ADVExtractor
    {
        private static bool isExtraction = false;

        public static bool IsExtraction()
        {
            return isExtraction;
        }
        public static void GetExtractingKeyDown(SimulationScene simUpdate)
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.E))
            {
                ExtractADV(null, true);
            }
            else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.E))
            {
                var advScene = simUpdate.transform.Find("Manager/ADVManager/ADVScene(Clone)");
                if (advScene != null)
                {
                    var adv = advScene.GetComponent<ADVScene>();
                    if (adv != null)
                    {
                        ExtractADV(adv._scenario, false);
                    }
                }
            }
        }

        private static void ExtractADV(TextScenario scenario, bool fromPath)
        {
            isExtraction = true;
            string extractedText = "";
            string assetName = "";
            if (fromPath)
            {
                string[] files = ADVLoaderPlugin.GetAssetAndFilePath();
                string checkPath = Path.Combine(Paths.GameRootPath, "abdata/" + files[1]);
                if (File.Exists(checkPath) && files[0] != "")
                {
                    var sceneList = GetScenarioListFromOpenData(files[1], files[0]);
                    if (sceneList != null)
                    {
                        if (sceneList.Count > 0)
                        {
                            extractedText = GetJsonText(sceneList, files[0]);
                            assetName = files[0];
                        } 
                    }
                }
            }
            else
            {
                if (scenario is null) return;
                if (scenario.CommandPacks.Count > 0)
                {
                    if (ADVLoaderPlugin.GetExtractionType() == ADVLoaderPlugin.ExtractionType.CharacterADV)
                    {
                        var sceneList = GetScenarioListFromOpenData(scenario.LoadBundleName, scenario.LoadAssetName);
                        if (sceneList != null)
                        {
                            if (sceneList.Count > 0)
                            {
                                extractedText = GetJsonText(sceneList, scenario.LoadAssetName);
                                assetName = scenario.LoadAssetName;
                            } 
                        }
                    }
                    else if (ADVLoaderPlugin.GetExtractionType() == ADVLoaderPlugin.ExtractionType.AllLoadedADVs)
                    {
                        extractedText = GetJsonText(scenario.CommandPacks, scenario.LoadAssetName);
                        assetName = scenario.LoadAssetName;
                    } 
                }
            }
            if (assetName != "" && extractedText != "") WriteToFile(extractedText, assetName);
            isExtraction = false;
        }
        private static List<ScenarioCommand> GetScenarioListFromOpenData(string bundle, string asset)
        {
            OpenData openData = new OpenData();
            openData.Bundle = bundle;
            openData.Asset = asset;
            try
            {
                openData.Load();
            }
            catch
            {
                ADVLoaderPlugin.Log.Log(LogLevel.Message, $"Error! Asset does not exist in bundle");
            }

            if (openData._data is not null)
            {
                List<ScenarioCommand> commands = new List<ScenarioCommand>();
                foreach (var sceneCommand in openData._data._list)
                {
                    commands.Add(sceneCommand);
                }
                return commands;
            }
            return null;
        }
        private static string GetJsonText(List<ScenarioCommand> scenarios, string assetName)
        {
            if (scenarios is null) return "";
            if (scenarios.Count > 0)
            {
                ADVLoaderPlugin.Log.LogInfo($"Extracting ADV in json format.");
                string extractedText = "[";
                int endIndex = scenarios.Count - 1;
                int index = 0;
                foreach (var scenarioData in scenarios)
                {
                    ADVLoaderPlugin.Log.LogInfo($"Scenario: {index}");

                    extractedText += "\n\t{\n";
                    extractedText += "\t\t\"Version\": " + scenarioData._version + ",\n";
                    extractedText += "\t\t\"Multi\": " + scenarioData._multi.ToString().ToLower() + ",\n";
                    extractedText += "\t\t\"Command\": \"" + scenarioData._command.ToString() + "\",\n";

                    if (scenarioData._args != null)
                    {
                        extractedText += "\t\t\"Args\": " + "[";
                        if (scenarioData._args.Count > 0)
                        {
                            int argIndex = scenarioData._args.Count - 1;
                            //string rawText = $@"";
                            for (int i = 0; i < scenarioData._args.Count; i++)
                            {
                                if (scenarioData._args[i].Contains("\n"))
                                {
                                    string tempText = scenarioData._args[i].Replace("\n", "\\n");
                                    if (argIndex == i) extractedText += $"\"{tempText}\"";
                                    else extractedText += $"\"{tempText}\",";
                                }
                                else if (argIndex == i) extractedText += $"\"{scenarioData._args[i]}\"";
                                else extractedText += $"\"{scenarioData._args[i]}\",";
                            }
                        }
                        extractedText += "]";
                    }
                    if (endIndex == index) extractedText += "\n\t}";
                    else extractedText += "\n\t},";
                    index++;
                }
                extractedText += "\n]";
                return extractedText;
            }
            return "";
        } 
        private static void WriteToFile(string extractedText, string name)
        {
            string folderPath = Path.Combine(Paths.GameRootPath, "UserData\\extractedADV");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string fileName = name + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
            string extractedFile = Path.Combine(Paths.GameRootPath, "UserData\\extractedADV\\" + fileName);
            string fileAssetNameOnly = Path.Combine(Paths.GameRootPath, "UserData\\extractedADV\\" + name + ".json");
            File.WriteAllText(extractedFile, extractedText);
            File.WriteAllText(fileAssetNameOnly, extractedText);
            ADVLoaderPlugin.Log.Log(LogLevel.Message, $"Done Extracting ADV: {name}. File can be found in UserData/extractedADV");
        }
    }
}
