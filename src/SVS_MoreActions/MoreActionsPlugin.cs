using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using SemanticVersioning;
using SV;

namespace SVS_MoreActions
{
    [BepInPlugin(GUID, DisplayName, Version)]
    [BepInDependency("DS27.SVS.ADVLoader", BepInDependency.DependencyFlags.HardDependency)]
    public class MoreActionsPlugin : BasePlugin
    {
        public const string DisplayName = "SVS_MoreActions";
        public const string GUID = "DS27.SVS.MoreActions";
        public const string Version = Constants.Version;
        
        internal static new ManualLogSource Log;
        private static Harmony patchedHooks;

        private static ConfigEntry<bool> _option_showLog;

        public override void Load()
        {
            // Plugin startup logic
            Log = base.Log;

            _option_showLog = Config.Bind("Options", "Show Log", false, new ConfigDescription("Will add information of this mod operations to the LogOutput file.\nOnly enable this if you have issues", null, new ConfigurationManagerAttributes { Order = 9 }));


            if (!GetADVLoaderPlugin(true)) return;
            patchedHooks = Harmony.CreateAndPatchAll(typeof(Hooks));
        }

        public static bool GetADVLoaderPlugin(bool showWarning)
        {
            if (IL2CPPChainloader.Instance.Plugins.TryGetValue("DS27.SVS.ADVLoader", out var advLoader))
            {
                var version = advLoader.Metadata.Version;
                var vNew = new Version("1.1.4");
                int result = version.CompareTo(vNew);
                if (result < 0 && showWarning)
                {
                    Log.Log(LogLevel.Message, "MoreAction dependency: Old SVS_ADVLoader detected, please update to v1.1.4 or latest");
                    return false;
                }
                return true;
            }
            else
            {
                Log.Log(LogLevel.Message, "MoreAction missing dependency: SVS_ADVLoader plugin not found");
            }
            return false;
        }
        public static bool GetShowLog()
        {
            return _option_showLog.Value;
        }
        internal static class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(CommandUI), nameof(CommandUI.Start))]
            private static void StartPost(CommandUI __instance)
            {
                MoreActions.AddActionIntoCommandMenu(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CommandUI), nameof(CommandUI.ButtonAction))]
            private static void InitAction(CommandUI __instance, int _commandNo)
            {
                MoreActions.JudgeAction(_commandNo);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(NormalConditionFavourable), nameof(NormalConditionFavourable.Conditions))]
            private static int TestNormCondFavorJudge(int __result, NormalConditionFavourable __instance)
            {
                return MoreActions.SetActionFavorMood(__instance, __result);
            }
        }
    }
}
