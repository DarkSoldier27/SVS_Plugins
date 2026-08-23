using ADV;
using ADV.Commands.Game.LowChara;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Configuration;
using HarmonyLib;
using SV;
using System.Collections.Generic;
using UnityEngine;

namespace SVS_ADVLoader
{
    [BepInProcess("SamabakeScramble")]
    [BepInPlugin(GUID, PluginName, PluginVersion)]
    public class ADVLoaderPlugin : BasePlugin
    {
        public const string PluginName = "SVS_ADVLoader";
        public const string GUID = "DS27.SVS.ADVLoader";
        public const string PluginVersion = "1.0.0";

        internal static new ManualLogSource Log;
        private static Harmony patchedHooks;

        private static ConfigEntry<bool> _option_showLog;
        private static ConfigEntry<bool> _option_displayCurrentADV;
        private static ConfigEntry<bool> _option_sideloadADV;

        //Extraction Settings
        private static ConfigEntry<FileFormat> _extract_FileFormat;
        private static ConfigEntry<ExtractionType> _extract_curretADVType;

        private static ConfigEntry<string> _assetADV;
        private static ConfigEntry<string> _bundleADV;
        
        private static ConfigEntry<bool> _extract_ADV;
        public override void Load()
        {
            Log = base.Log;

            _option_sideloadADV = Config.Bind("ADV options", "Load Custom ADV", true, new ConfigDescription("Enables sideloading ADV files (dialogues in the game)", null, new ConfigurationManagerAttributes { Order = 10 }));
            _option_showLog = Config.Bind("ADV options", "Show Log", false, new ConfigDescription("Will add information of this mod operations to the LogOutput file.\nOnly enable this if you have issues", null, new ConfigurationManagerAttributes { Order = 9 }));
            _option_displayCurrentADV = Config.Bind("ADV options", "Display current ADV", false, new ConfigDescription("Will display the ADV asset name when it gets loaded and if it is custom or original ADV", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 8 }));

            _extract_ADV = Config.Bind("Extracting Tool", "Enable Extraction of ADV", false, new ConfigDescription("Can extract ADVs", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 4 }));
            _extract_curretADVType = Config.Bind("Extracting Tool", "Extract current ADV type", ExtractionType.CharacterADV, new ConfigDescription("ADV that will be extracted from current ADV.\nNone: no ADV will be extracted.\nCharacter ADV: Only the current ADV of the character will be extracted.\nAll Loaded ADVs: Extracts all loaded ADVs for the current Scene", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 }));
            //_extract_FileFormat = Config.Bind("Extracting Tool", "Extrated file format", FileFormat.json, new ConfigDescription("Format of the extracted ADV", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 }));
            _assetADV = Config.Bind("Extracting Tool", "ADV Asset Name", "", new ConfigDescription("Name of the asset to extract", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 }));
            _bundleADV = Config.Bind("Extracting Tool", "ADV Bundle Path", "adv/scenario/", new ConfigDescription("Path of the ADV file inside the abdata folder.", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 0 }));

            ADVLoader.CreateADVLoaderDirectory();
            patchedHooks = Harmony.CreateAndPatchAll(typeof(Hooks));
        }
        public enum ExtractionType
        {
            None = 0,
            CharacterADV = 1,
            AllLoadedADVs = 2,
        }
        public enum FileFormat
        {
            json = 0,
            csv = 1,
        }
        public static bool GetSideloadADV()
        {
            return _option_sideloadADV.Value;
        }
        public static bool GetShowLog()
        {
            return _option_showLog.Value;
        }
        public static bool GetDisplayCurrentADV()
        {
            return _option_displayCurrentADV.Value;
        }
        public static ExtractionType GetExtractionType()
        {
            return _extract_curretADVType.Value;
        }
        /// <summary>
        /// Returns Asset name and Bundle path and Asset. [0] Asset, [1] Bundle
        /// </summary>
        /// <returns></returns>
        public static string[] GetAssetAndFilePath()
        {
            return [_assetADV.Value, _bundleADV.Value];
        }
        public static FileFormat GetFileFormat()
        {
            return _extract_FileFormat.Value;
        }
        internal static class Hooks
        {
            [HarmonyPriority(800)]
            [HarmonyPostfix]
            [HarmonyPatch(typeof(SimulationScene), nameof(SimulationScene.Update))]
            public static void SetupADV(SimulationScene __instance)
            {
                if (_extract_ADV.Value) ADVExtractor.GetExtractingKeyDown(__instance);
            }

            [HarmonyPriority(300)]
            [HarmonyPrefix]
            [HarmonyPatch(typeof(TextScenario), nameof(TextScenario.LoadFile))]
            public static void PreLoadFile(TextScenario __instance, string bundle, string asset)
            {
                ADVLoader.PreADVLoadInit(__instance,bundle, asset);
            }

            [HarmonyPriority(300)]
            [HarmonyPrefix]
            [HarmonyPatch(typeof(OpenData), nameof(OpenData.Load), typeof(string), typeof(string))]
            public static bool ADVSideloader(OpenData __instance, string bundle, string asset)
            {
                return ADVLoader.SideLoadADV(__instance, bundle, asset, false, out Il2CppSystem.Collections.Generic.List<ScenarioCommand> lowPolyScenarios);
            }

            [HarmonyPriority(300)]
            [HarmonyPrefix]
            [HarmonyPatch(typeof(LowCharaADV), nameof(LowCharaADV.Load), typeof(string), typeof(string))]
            public static bool LowPolyADVSideloader(ref Il2CppSystem.Collections.Generic.List<ScenarioCommand> __result, string bundle, string asset)
            {
                if (!ADVLoader.SideLoadADV(null, bundle, asset, true, out Il2CppSystem.Collections.Generic.List<ScenarioCommand> lowPolyScenarios))
                {
                    __result = lowPolyScenarios;
                    return false;
                }
                return true;
            }
        }
    }
}
