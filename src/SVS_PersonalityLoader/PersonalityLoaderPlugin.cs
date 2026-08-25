using ADV;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using SV.Config;

namespace PersonalityLoader
{
    [BepInPlugin(GUID, DisplayName, Version)]
    [BepInDependency("DS27.SVS.ADVLoader", BepInDependency.DependencyFlags.SoftDependency)]
    public class PersonalityLoaderPlugin : BasePlugin
    {
        public const string DisplayName = Constants.Name;
        public const string GUID = "SVS_PersonalityLoader";
        public const string Version = Constants.Version;

        internal static new ManualLogSource Log;
        private static Harmony patchedHooks;

        public override void Load()
        {
            Log = base.Log;
            GetADVLoaderPlugin(true);
            patchedHooks = Harmony.CreateAndPatchAll(typeof(Hooks));
        }
        public static bool GetADVLoaderPlugin(bool showWarning)
        {
            if (IL2CPPChainloader.Instance.Plugins.TryGetValue("DS27.SVS.ADVLoader", out var advloader)) return true;
            else
            {
                if (showWarning) Log.Log(LogLevel.Message, "SVS_PersonalityLoader missing dependency: SVS_ADVLoader. Loading ADVs from Hard mods");
                return false;
            }
        }
        internal static class Hooks
        {
            //Game Voice Setting
            [HarmonyPostfix]
            [HarmonyPatch(typeof(VoiceSetting), nameof(VoiceSetting.Init))]
            public static void CreateCustomPersonalityVoiceSetting(VoiceSetting __instance)
            {
                PersonalityLoader.CreatePersoanlityVoiceSetting(__instance);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(AnimationController), nameof(AnimationController.Initialize), typeof(byte), typeof(int))]
            public static void SetAnimCtrlIfMissing(AnimationController __instance, byte sex, ref int personality)
            {
                personality = PersonalityLoader.SetAnimationCtrlIfMissing(sex, personality);
            }

        }
    }
}
