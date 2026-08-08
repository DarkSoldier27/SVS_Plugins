using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

namespace SVS_Detour
{
    [BepInProcess("SamabakeScramble")]
    [BepInPlugin(GUID, PluginName, PluginVersion)]
    public class DetourPlugin : BasePlugin
    {
        public const string PluginName = "SVS_Detour";
        public const string GUID = "SVS.Detour";
        public const string PluginVersion = "0.1.0";

        internal static new ManualLogSource Log;

        private static ConfigEntry<bool> _showLog;

        public override void Load()
        {
            //Logging
            Log = base.Log;
            _showLog = Config.Bind("options", "Show Log", false, new ConfigDescription("Show log", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 20 }));

            NightEventJudgeHook.Install();
            Log.LogInfo("Patched NightEventManager");
        }

        public static bool GetShowLog()
        {
            return _showLog.Value;
        }
    }
}

