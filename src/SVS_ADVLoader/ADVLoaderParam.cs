using ADV;
using ADV.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using UnityEngine;

namespace SVS_ADVLoader
{
    internal class ADVLoaderParam
    {
        private static readonly Dictionary<int, AnimationController.StateParameter> _Animations = new Dictionary<int, AnimationController.StateParameter>();
        private static readonly Dictionary<string, string> _ADVFolderList = new Dictionary<string, string>()
        {
            ["chara"] = "abdata/mods/ADVLoader/Scenario/Chara/",
            ["common"] = "abdata/mods/ADVLoader/Scenario/Common/",
            ["other"] = "abdata/mods/ADVLoader/Scenario/Other/",
            ["anim"] = "abdata/mods/ADVLoader/List/"
        };
        private static Dictionary<string, bool> _ADVFlagList = new Dictionary<string, bool>();
        public class ScenarioParam
        {
            public int Version { get; set; } = 0;
            public bool Multi {  get; set; } = false;
            public string Command { get; set; } = "";
            public string[] Args { get; set; }
        }
        public static List<ScenarioParam> DeserializeScenario(string filePath)
        {
            var sr = new StreamReader(File.OpenRead(filePath));
            var json = sr.ReadToEnd();
            sr.Close();
            try
            {
                var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip};
                return JsonSerializer.Deserialize<List<ScenarioParam>>(json, options);
            }
            catch (Exception ex)
            {
                ADVLoaderPlugin.Log.LogInfo($"Failed to load json File: {ex}");
                return null;
            }
        }
        public static void LoadAnimationsFromList(string filePath)
        {
            if (filePath == "") return;
            var animList = File.ReadAllLines(filePath);
            foreach (var anim in animList)
            {
                var animParam = anim.Split(',');
                if (int.TryParse(animParam[0], out int animID))
                {
                    if (animParam.Length < 20) continue;
                    if (!_Animations.ContainsKey(animID))
                    {
                        _Animations.Add(animID,
                            new AnimationController.StateParameter(new TableData.Info()
                            {
                                _dic = new ILLGames.Collections.Generic.Optimized.IntKeyDictionary<ILLGames.Serialization.TableData<TableData.Info>.InfoBase.RevisedList<string>>()
                            })
                            {
                                ID = animID,
                                Name = animParam[1],
                                AssetBundle = animParam[2],
                                Asset = animParam[3],
                                Manifest = animParam[4],
                                State = animParam[5],
                                StateHash = Animator.StringToHash(animParam[5]),
                                SpecifiedLayers = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<int>(int.TryParse(animParam[6], out int layerVal) ? layerVal : 0),
                                BlendMode = int.TryParse(animParam[7], out int blendModeVal) ? blendModeVal : 1,
                                Blend = float.TryParse(animParam[8], out float blendVal) ? blendVal : 0.3f,
                                IsLoopType = bool.TryParse(animParam[9], out bool isLoopT) ? isLoopT : false,
                                _hasExitTimeMinMax = bool.TryParse(animParam[10], out bool hasExitTime) ? hasExitTime : false,
                                _exitTimeMin = float.TryParse(animParam[11], out float exitMin) ? exitMin : 0.95f,
                                _exitTimeMax = float.TryParse(animParam[12], out float exitMax) ? exitMax : 0,
                                IsOutCrossFade = bool.TryParse(animParam[13], out bool IsOutCross) ? IsOutCross : true,
                                PlaysSyncItems = bool.TryParse(animParam[14], out bool PlaysSyncI) ? PlaysSyncI : false,
                                IgnoreReset = bool.TryParse(animParam[15], out bool IgnoreRes) ? IgnoreRes : false,
                                IsNeckOffset = bool.TryParse(animParam[16], out bool sNeckOff) ? sNeckOff : false,
                                UseCycleOffset = bool.TryParse(animParam[17], out bool UseCycleOff) ? UseCycleOff : false,
                                UseRandomSpeed = bool.TryParse(animParam[18], out bool UseRandomSp) ? UseRandomSp : false,
                            });
                    }
                }
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
        public static Dictionary<int, AnimationController.StateParameter> GetAnimationList()
        {
            return _Animations;
        }
    }
}
