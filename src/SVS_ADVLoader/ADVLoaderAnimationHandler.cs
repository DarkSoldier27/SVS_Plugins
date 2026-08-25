using ADV;
using ADV.Serialization;
using BepInEx;
using SV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SVS_ADVLoader
{
    internal class ADVLoaderAnimationHandler
    {
        public static void LoadList()
        {
            string listFolder = Path.Combine(Paths.GameRootPath, ADVLoaderParam.GetADVFolders("anim"));
            if (Directory.Exists(listFolder))
            {
                string[] animListFiles = Directory.GetFiles(listFolder, "*.csv");
                if (animListFiles.Length > 0)
                {
                    foreach (string animFile in animListFiles)
                    {
                        ADVLoaderParam.LoadAnimationsFromList(animFile);
                    }
                    ADVLoaderPlugin.Log.LogInfo("Animation list found");
                }
                else
                {
                    ADVLoaderPlugin.Log.LogInfo("No animation list found");
                }
            }
        }
        public static void SetAnimationIfMissing(AnimationController animation, string motionID)
        {
            if (animation is null) return;
            if (int.TryParse(motionID, out int id))
            {
                if (!animation._params._dictionary.ContainsKey(id))
                {
                    var animLoader = ADVLoaderParam.GetAnimationList();
                    if (animLoader.ContainsKey(id))
                    {   
                        animation._params._dictionary.Add(id,
                        new AnimationController.StateParameter(new TableData.Info()
                        {
                            _dic = new ILLGames.Collections.Generic.Optimized.IntKeyDictionary<ILLGames.Serialization.TableData<TableData.Info>.InfoBase.RevisedList<string>>()
                        })
                        {
                            ID = id,
                            Name = animLoader[id].Name,
                            AssetBundle = animLoader[id].AssetBundle,
                            Asset = animLoader[id].Asset,
                            Manifest = animLoader[id].Manifest,
                            State = animLoader[id].State,
                            StateHash = Animator.StringToHash(animLoader[id].State),
                            SpecifiedLayers = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<int>(animLoader[id].SpecifiedLayers.Count),
                            BlendMode = animLoader[id].BlendMode,
                            Blend = animLoader[id].Blend,
                            IsLoopType = animLoader[id].IsLoopType,
                            _hasExitTimeMinMax = animLoader[id]._hasExitTimeMinMax,
                            _exitTimeMin = animLoader[id]._exitTimeMin,
                            _exitTimeMax = animLoader[id]._exitTimeMax,
                            IsOutCrossFade = animLoader[id].IsOutCrossFade,
                            PlaysSyncItems = animLoader[id].PlaysSyncItems,
                            IgnoreReset = animLoader[id].IgnoreReset,
                            IsNeckOffset = animLoader[id].IsNeckOffset,
                            UseCycleOffset = animLoader[id].UseCycleOffset,
                            UseRandomSpeed = animLoader[id].UseRandomSpeed
                        });
                    }
                    if (ADVLoaderPlugin.GetShowLog()) ADVLoaderPlugin.Log.LogInfo("Loading new animation for ADV");
                }
            }
        }
    }
}
