using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace SVS_CustomGameBalance
{
    [BepInPlugin(GUID, DisplayName, Version)]
    [BepInDependency("SVS.Detour", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("DS27.SVS.ADVLoader", BepInDependency.DependencyFlags.SoftDependency)]

    public class CustomGameBalancePlugin : BasePlugin
    {
        public const string DisplayName = Constants.Name;
        public const string GUID = "DS27.SVS.CustomGameBalance";
        public const string Version = Constants.Version;

        internal static new ManualLogSource Log;
        private static Harmony patchedHooks;

        internal static ConfigEntry<KeyCode> toggleKey_AutoPC { get; set; }
        internal static ConfigEntry<KeyCode> toggleKey_SwitchPCCharacter { get; set; }
        internal static ConfigEntry<KeyCode> toggleKey_AutoCharaVoice { get; set; }

        //private static ConfigEntry<AllowHugging> _cheaterStatus_DisableHugIsSkinship;
        private static ConfigEntry<CharaType> _reaction_CharaType;
        private static ConfigEntry<CharaType> _charaType_CharaAnswerRate;
        private static ConfigEntry<HighVirtueBBQ> _bbqAnswerType;
        private static ConfigEntry<DisplayPercentage> _actionPorcentageDisplay;

        private static ConfigEntry<bool> _byEndPeriod;
        
        private static ConfigEntry<bool> _enableActionLowestRate;
        private static ConfigEntry<bool> _applyGameFixes;
        private static ConfigEntry<bool> _applyAreYouDatingFix;
        private static ConfigEntry<bool> _makePCNonInteractable;
        private static ConfigEntry<bool> _removeActionLimit;
        //private static ConfigEntry<bool> _escapeVoiceFix;
        private static ConfigEntry<bool> _fortuneSuccessFix;
        //private static ConfigEntry<bool> forceNightEvent;
        private static ConfigEntry<bool> _forceBreakUp;
        //private static ConfigEntry<bool> hugLovePointsSwitch;
        private static ConfigEntry<bool> _enablePC_3PJoin;
        private static ConfigEntry<bool> _enableNPC_3PJoin;

        private static ConfigEntry<bool> _showLog;
        private static ConfigEntry<bool> _enable_NormalVirtueBalance;
        private static ConfigEntry<bool> _actManager_disableKidnapping;

        private static ConfigEntry<int> _pointAditive;
        
        private static ConfigEntry<int> _NPC_3PAskRate;
        private static ConfigEntry<int> _NPC_3PJoinRate;
        
        //private static ConfigEntry<int> _actionLimit;
        
        private static ConfigEntry<float> _lowestActionRate;
        //private static ConfigEntry<float> _NPCSexSpeed;

        //Action Manager Parameter
        private static ConfigEntry<RestrictionType> _actManager_Ask3PRestriction;
        private static ConfigEntry<bool> _enableNPC_3PAsk;

        //Stats Reduction Parameters
        private static ConfigEntry<float> _statsReduc_reduceRate;

        private static ConfigEntry<bool> _statsReduc_enable;
        private static ConfigEntry<bool> _statsReduc_PCReduction;

        //Simulation Parameters
        private static ConfigEntry<NPCSex> _sim_NPCLowPolySex;

        private static ConfigEntry<int> _sim_NPCSexTimerMin;
        private static ConfigEntry<int> _sim_NPCSexTimerMax;

        private static ConfigEntry<float> _pcRunSpeed;
        private static ConfigEntry<float> _pcWalkSpeed;
        private static ConfigEntry<float> _npcRunSpeed;
        private static ConfigEntry<float> _npcWalkSpeed;

        private static ConfigEntry<bool> _sim_DoNotSkipNPC;
        private static ConfigEntry<bool> _cheaterStatus_DisableHugIsSkinship;

        //Reaction Manager Parameters
        private static ConfigEntry<int> _reactionChanceOverall;
        private static ConfigEntry<int> _reactionChanceH;
        //private static ConfigEntry<int> _reactionChanceMasturbation;
        //private static ConfigEntry<int> _reactionChanceFight;
        private static ConfigEntry<int> _reactionChanceSkinship;
        private static ConfigEntry<int> _reactionChanceNormal;
        //private static ConfigEntry<int> _losingReactionChance;
        //private static ConfigEntry<int> _h2ReactionChance;
        //private static ConfigEntry<int> _h3ReactionChance;

        private static ConfigEntry<bool> _reaction_ReactionManagerEnable;

        //CheaterManager Parameters
        private static ConfigEntry<bool> _cheaterStatus_Enable;
        private static ConfigEntry<bool> _cheaterStatus_IgnorePC;
        private static ConfigEntry<int> _cheaterStatus_hatePoints;

        //Cheats Parameters
        private static ConfigEntry<bool> _cheat_forceThreesome;
        private static ConfigEntry<bool> _cheat_forceThreesomeNPC;
        private static ConfigEntry<bool> _cheat_forceHAction;

        //Night Event Parameters
        private static ConfigEntry<bool> _nightEvent_active;
        private static ConfigEntry<int> _nightChance;
        private static ConfigEntry<int> _nightChanceH;

        public override void Load()
        {
            //Plugin startup logic
            Log = base.Log;
            //CGB Settings
            _showLog = Config.Bind("CGB Settings", "Show Log", true, new ConfigDescription("For debugging", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 20 }));

            //Stats Reduction
            _statsReduc_enable = Config.Bind("Stats Reduction", "Enable Stats Reduction for Characters", true, new ConfigDescription("The characters will lose some points on their Stamina, Study, LifeStyle and Talk stats at the end of the day, the amount of point they lose will depend on what was set on the abilities tab in the Character Maker", null, new ConfigurationManagerAttributes { Order = 3 }));
            _statsReduc_PCReduction = Config.Bind("Stats Reduction", "Disable PC stats reduction", true, new ConfigDescription("Disable the stats getting reduced for the Playable Character. By default this option is Enable, which means the Playable Character won't get his/her stats reduce at the end of the day", null, new ConfigurationManagerAttributes { Order = 2 }));
            _statsReduc_reduceRate = Config.Bind("Stats Reduction", "Set Rate Point Reduction", 1.0f, new ConfigDescription("Set the rate of points that will be reduce per stat.\nAt 1 normal amount.\nAt 2 double the amount.", new AcceptableValueRange<float>(0.1f, 2.0f), new ConfigurationManagerAttributes { Order = 1 }));

            //Interruption Manager / Reaction Manager
            _reaction_ReactionManagerEnable = Config.Bind("Interruption Manager", "Enable Interruption Manager", true, new ConfigDescription("If enabled, it will use the sliders below to determined the probability of interruptions from NPCs", null, new ConfigurationManagerAttributes { Order = 15 }));
            _reaction_CharaType = Config.Bind("Interruption Manager", "Reduce Interruptions for", CharaType.PC | CharaType.NPC,
            new ConfigDescription("Set if the reduced interruption rate affect PC, NPC or both", null, new ConfigurationManagerAttributes { Order = 14 }));

            _reactionChanceOverall = Config.Bind("Interruption Manager", "Reduce All Interruptions Rate by %", 0, new ConfigDescription("Reduce the chances for all types of Interruptions by the chosen amount.", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = 13 }));
            _reactionChanceNormal = Config.Bind("Interruption Manager", "Reduce Normal Interruptions by %", 0, new ConfigDescription("Reduce the chances for Normal Interruptions by the chosen amount.", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = 12 }));
            _reactionChanceSkinship = Config.Bind("Interruption Manager", "Reduce Contact Interruptions by %", 0, new ConfigDescription("Reduce the chances for Contact (Hug, Kiss, Touch) Interruptions by the chosen amount.", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = 11 }));
            _reactionChanceH = Config.Bind("Interruption Manager", "Reduce public sex Interruptions by %", 0, new ConfigDescription("Reduce the chances of public sex Interruptions by the chosen amount. Doesn't affect NPCs asking for 3P", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = 10 }));
            //_masturbationReactionChance = Config.Bind("Interruption Manager", "Reduce public sex Interruptions by", 0, new ConfigDescription("Reduce the chance for all types of Interruptions", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = 9 }));
            //_fightReactionChance = Config.Bind("Interruption Manager", "Reduce public sex Interruptions by", 0, new ConfigDescription("Reduce the chance for all types of Interruptions", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = 8 }));
            //_losingReactionChance = Config.Bind("Interruption Manager", "Reduce public sex Interruptions by", 0, new ConfigDescription("Reduce the chance for all types of Interruptions", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = 7 }));
            //_h2ReactionChance = Config.Bind("Interruption Manager", "Reduce public sex Interruptions by", 0, new ConfigDescription("Reduce the chance for all types of Interruptions", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = 6 }));
            //_h3ReactionChance = Config.Bind("Interruption Manager", "Reduce public sex Interruptions by", 0, new ConfigDescription("Reduce the chance for all types of Interruptions", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = 5 }));
            _enablePC_3PJoin = Config.Bind("Interruption Manager", "PC 3P Interruption", true, new ConfigDescription("If enabled, PC will be able to join 3P more easily", null, new ConfigurationManagerAttributes { Order = 4 }));
            _enableNPC_3PJoin = Config.Bind("Interruption Manager", "NPC 3P Interruption", true, new ConfigDescription("If enabled, NPCs will be able to join 3P more frequently. The chance of success will be affected by Trait, Sexual Orientation and Virtue level", null, new ConfigurationManagerAttributes { Order = 3 }));
            _NPC_3PJoinRate = Config.Bind("Interruption Manager", "NPC 3P Interruption Rate", 0, new ConfigDescription("Rate of how often NPCs will join 3P when they see public sex. \nVirtue and trait will affect this \nAt 100 they will always join for 3P if conditions are met", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = 2 }));

            //Night Event
            if (GetDetourPlugin(true)) _nightEvent_active = Config.Bind("Night Event", "Increase Night Event Rate", true, new ConfigDescription("If enabled, the rate of the Night event will be increase. Note: Male characters can not visit you if you play as a female or male character (Illgames did not add events for them)", null, new ConfigurationManagerAttributes { Order = 10 }));
            if (GetDetourPlugin(false)) _nightChance = Config.Bind("Night Event", "Normal Night Event Rate", 50, new ConfigDescription("Increase the rate of Normal Night Events.", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = 9 }));
            if (GetDetourPlugin(false)) _nightChanceH = Config.Bind("Night Event", "Sex Night Event Rate", 50, new ConfigDescription("Increase the rate of Sex Night Events.", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = 8 }));

            //Action Manager
            _actionPorcentageDisplay = Config.Bind("Action Manager", "Display Probability", DisplayPercentage.Normal,
            new ConfigDescription("Normal: Display percentage values as normal | Real: Display the real percentage value | Hide: Hide the percentage value", null, new ConfigurationManagerAttributes { Order = 9 }));
            _removeActionLimit = Config.Bind("Action Manager", "Remove Action Limit", true, new ConfigDescription("If enabled, the 3 action limit per period will be removed", null, new ConfigurationManagerAttributes { Order = 8 }));
            _forceBreakUp = Config.Bind("Action Manager", "Easy Break Ups", false, new ConfigDescription("If enabled, the break up action is always at 100 percent for both PC and NPCs (Only for lovers of course)", null, new ConfigurationManagerAttributes { Order = 7 }));
            _enableNPC_3PAsk = Config.Bind("Action Manager", "NPC ask for 3P", true, new ConfigDescription("If enabled, NPCs will be able to ask for 3P more frequently (The rate will be affected by Traits and Virtue level)", null, new ConfigurationManagerAttributes { Order = 6 }));
            _actManager_Ask3PRestriction = Config.Bind("Action Manager", "Ask 3P Mood Restrition Type", RestrictionType.Medium,
            new ConfigDescription("Type of restriction for when a NPC ask for 3P. All restriction types respect the Virtue and Trait rules.\nLights: Can ask for 3P in any mood.\nMedium: Can only ask for 3P when the asking character is in a good mood.\nHeavy: Can ask 3P only when the character is horny", null, new ConfigurationManagerAttributes { Order = 5 }));
            _NPC_3PAskRate = Config.Bind("Action Manager", "NPC ask for 3P Rate", 0, new ConfigDescription("Rate of how often the NPCs will ask for 3P. \nAt 0, NPCs will have a small chance of asking for 3P whenever they try to do public or private H.\nValues between 1 and 99, will be the chance of a NPC asking for 3P whenever they try to do an action.\nAt 100 they will always try to ask for 3P.", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { Order = 4 }));
            _actManager_disableKidnapping = Config.Bind("Action Manager", "Disable Abductions", false, new ConfigDescription("If enabled, character will evil trait will not Kidnap characters", null, new ConfigurationManagerAttributes { Order = 3 }));

            //Simulation
            toggleKey_AutoPC = Config.Bind("Simulation", "Toggle Auto PC", KeyCode.Y, new ConfigDescription("Hotkey to Toggle Auto PC | Auto PC means that your character will act like a NPC. It will take a little bit of time before the character starts to move", null, new ConfigurationManagerAttributes { Order = 13 }));
            toggleKey_SwitchPCCharacter = Config.Bind("Simulation", "Switch PC Character", KeyCode.U, new ConfigDescription("Hotkey to Switch PC Character | You can Change your character (PC) to any other character present on the map (You can only switch with NPCs that have the blue circle)", null, new ConfigurationManagerAttributes { Order = 12 }));
            toggleKey_AutoCharaVoice = Config.Bind("Simulation", "Auto PC Voice", KeyCode.O, new ConfigDescription("Hotkey to play a voice line during Auto PC. It does not do anything else", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 11 }));

            _sim_DoNotSkipNPC = Config.Bind("Simulation", "NPCs offscreen actions", true, new ConfigDescription("NPCs will no longer skip their actions if they are on a different map", null, new ConfigurationManagerAttributes { Order = 9 }));
            _pcWalkSpeed = Config.Bind("Simulation", "Set PC Walk Speed", 1f, new ConfigDescription("Set the base walking speed of the Playable Character", new AcceptableValueRange<float>(1f, 10f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 8 }));
            _pcRunSpeed = Config.Bind("Simulation", "Set PC Run Speed", 1f, new ConfigDescription("Set the base running speed of the Playable Character", new AcceptableValueRange<float>(1f, 10f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 7 }));
            _npcWalkSpeed = Config.Bind("Simulation", "Set NPC Walk Speed", 1f, new ConfigDescription("Set the base walking speed for NPCs", new AcceptableValueRange<float>(1f, 5f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 6 }));
            _npcRunSpeed = Config.Bind("Simulation", "Set NPC Run Speed", 1f, new ConfigDescription("Set the base running speed for NPCs", new AcceptableValueRange<float>(1f, 5f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 5 }));

            _sim_NPCLowPolySex = Config.Bind("Simulation", "Set NPCs Sex Animation", NPCSex.None,
            new ConfigDescription("The low Poly sex animations for NPCs will use new values for they duration and or speed", null, new ConfigurationManagerAttributes { Order = 4 }));
            _sim_NPCSexTimerMin = Config.Bind("Simulation", "NPCs Sex Duration Min", 10, new ConfigDescription("Set the minimum amount of time a NPC will have sex in seconds! | Note that this is per position and the NPCs will do 3 during H, so the total time is 3 times this value", new AcceptableValueRange<int>(10, 590), new ConfigurationManagerAttributes { Order = 3 }));
            _sim_NPCSexTimerMax = Config.Bind("Simulation", "NPCs Sex Duration Max", 20, new ConfigDescription("Set the maximum amount of time a NPC will have sex in seconds! | Note that this is per position and the NPCs will do 3 during H, so the total time is 3 times this value", new AcceptableValueRange<int>(20, 600), new ConfigurationManagerAttributes { Order = 2 }));
            //_NPCSexSpeed = Config.Bind("Simulation", "Set NPC Sex Speed", 1f, new ConfigDescription("Set the base speed for the NPCs sex animation", new AcceptableValueRange<float>(0.1f, 10f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 0 }));

            //Cheater Status Manager
            _cheaterStatus_Enable = Config.Bind("Cheater Status Manager", "Enable Cheater Status Manager", true, new ConfigDescription("The characters will gain hate toward their cheater partners and the one they cheated with on each interaction and at the end of the day", null, new ConfigurationManagerAttributes { Order = 2 }));
            _cheaterStatus_IgnorePC = Config.Bind("Cheater Status Manager", "Ignore PC cheating", false, new ConfigDescription("If enabled, the playable character will not get affected by this mod", null, new ConfigurationManagerAttributes { Order = 1 }));
            _cheaterStatus_hatePoints = Config.Bind("Cheater Status Manager", "Set base point", 30, new ConfigDescription("Set the amount of points the character will gain from cheaters (every 30 points is equal to 1 Hate Point)", new AcceptableValueRange<int>(0, 300), new ConfigurationManagerAttributes { Order = 0 }));
            _cheaterStatus_DisableHugIsSkinship = Config.Bind("Cheater Status Manager", "Friendly Hugs", false, new ConfigDescription("The hug action will not longer trigger the cheating status and it will give friend points instead of love.", null, new ConfigurationManagerAttributes { Order = 10 }));

            //Game Fixes
            _applyGameFixes = Config.Bind("Game Fixes", "Apply Gameplay Fixes", true, new ConfigDescription("If enabled, it will apply all gameplay fixes that are enable", null, new ConfigurationManagerAttributes { Order = 10 }));
            _bbqAnswerType = Config.Bind("Game Fixes", "BBQ Date Fixes", HighVirtueBBQ.Normal,
            new ConfigDescription("Fixes the issue of characters with High and Highest virtue inviting other to a BBQ when the requirements are not met. Possible fixes are | Accept: NPC will accept BBQ with a low chance | Do Not Ask: NPC with High and Highest virtue will no longer ask for BBQ outside of the expected requierements (High Love)", null, new ConfigurationManagerAttributes { Order = 9 }));
            _applyAreYouDatingFix = Config.Bind("Game Fixes", "NPCs Are You Dating Fix", true, new ConfigDescription("NPCs can use the action Are You dating Anyone? to get into a relationship, this action is bugged since it can bypass Traits, Favorability and Virtue level, making High and Highest Virtue character to date anyone. Enabling this option will fix this problem", null, new ConfigurationManagerAttributes { Order = 8 }));
            _makePCNonInteractable = Config.Bind("Game Fixes", "PC Non Interactable while following", true, new ConfigDescription("PC will become non interactable while following a NPC, it will become interactable when they reach their destination", null, new ConfigurationManagerAttributes { Order = 7 }));
            //_escapeVoiceFix = Config.Bind("Game Fixes", "Escape Voice Fix", true, new ConfigDescription("The escape voice line will play when a NPC starts to run away", null, new ConfigurationManagerAttributes { Order = 6 }));

            //Other Options
            _enableActionLowestRate = Config.Bind("Other Options", "Override Minimum Success Rate", false, new ConfigDescription("If enabled, it will override the minimum success rate", null, new ConfigurationManagerAttributes { Order = 10 }));
            _charaType_CharaAnswerRate = Config.Bind("Other Options", "Set minimum success rate for", CharaType.PC | CharaType.NPC,
            new ConfigDescription("Set the minimum success rate for PC, NPC or both", null, new ConfigurationManagerAttributes { Order = 9 }));
            _lowestActionRate = Config.Bind("Other Options", "Set Minimum Success Rate value", 0f, new ConfigDescription("Set the minimum success rate for an action (it will override action with 0 success rate)", new AcceptableValueRange<float>(0f, 100f), new ConfigurationManagerAttributes { ShowRangeAsPercent = true, Order = 8 }));
            _enable_NormalVirtueBalance = Config.Bind("Other Options", "Normal Virtue Balances", false, new ConfigDescription("If enabled, some balance will be applied to normal Virtue characters. \nNormal virtue will only confess if they have 11 points in love.", null, new ConfigurationManagerAttributes { Order = 6 }));

            //Fortune
            _fortuneSuccessFix = Config.Bind("Fortune Manager", "Buff fortune 7", true, new ConfigDescription("In vanilla, this fortune barely has an impact in the game. Enabling this option will buff this fortune. Now alongside of improving the 3 options result, now it increases the probability of an interaction by 10 percent", null, new ConfigurationManagerAttributes { Order = 10 }));

            //Cheats
            _cheat_forceThreesome = Config.Bind("Cheats", "Force 3P Option", false, new ConfigDescription("The 3P option will always be available when interrupting Public H", null, new ConfigurationManagerAttributes { Order = 10 }));
            _cheat_forceThreesomeNPC = Config.Bind("Cheats", "Force NPC to join 3P", false, new ConfigDescription("NPC will ask to join 3P any time they see Public H", null, new ConfigurationManagerAttributes { Order = 9 }));
            _cheat_forceHAction = Config.Bind("Cheats", "Force Only H Action", false, new ConfigDescription("Force NPC to only ask for Sex (Results may vary) WARNING: this affect all NPCs regardless of virtue, traits, sex, etc", null, new ConfigurationManagerAttributes { Order = 8 }));
            //forceNightEvent = Config.Bind("Cheats", "Force H Night Event", false, new ConfigDescription("If enabled, the lewd night event will be trigger", null, new ConfigurationManagerAttributes { Order = 7 }));

            GetADVLoaderPlugin(true);
            patchedHooks = Harmony.CreateAndPatchAll(typeof(CGBHooks.Hooks));
        }
        public static bool GetDetourPlugin(bool showWarning)
        {
            if (IL2CPPChainloader.Instance.Plugins.TryGetValue("SVS.Detour", out var detour)) return true;
            else
            {
                if (showWarning) Log.Log(LogLevel.Message, "SVS_CustomGameBalance missing dependency: SVS_Detour. Night Event options disabled");
                return false;
            }
        }
        public static bool GetADVLoaderPlugin(bool showWarning)
        {
            if (IL2CPPChainloader.Instance.Plugins.TryGetValue("DS27.SVS.ADVLoader", out var advloader)) return true;
            else
            {
                if (showWarning) Log.Log(LogLevel.Message, "SVS_CustomGameBalance missing dependency: SVS_ADVLoader. Some features may not work as intended");
                return false;
            }
        }
        public enum AllowHugging
        {
            None = 0,
            AllCharacters = 1,
            ExcludeSomeTraits = 2,
            LowestVirtueOnly = 3,
        }

        [Flags]
        public enum CharaType
        {
            None = 0,
            PC = 1 << 0,
            NPC = 1 << 1,
        }

        [Flags]
        public enum NPCSex
        {
            None = 0,
            Duration = 1 << 0,
            //Speed = 1 << 1,
        }
        public enum HighVirtueBBQ
        {
            Normal = 0,
            Accept = 1,
            DoNotAsk = 2,
        }
        public enum DisplayPercentage
        {
            Normal = 0,
            Real = 1,
            Hide = 2,
        }
        public enum RestrictionType
        {
            Light = 0,
            Medium = 1,
            Heavy = 2,
        }
        /// <summary>
        /// Returns Actions boost options. [0] Ask 3P, [1] Blackmail, [2] Invites Home.
        /// </summary>
        /// <returns></returns>
        public static bool[] GetActionBooster()
        {
            return [_enableNPC_3PAsk.Value];
        }
        /// <summary>
        /// Returns Restriction types. [0] NPC ask 3P
        /// </summary>
        /// <returns></returns>
        public static RestrictionType[] GetActionRestrictionTypes()
        {
            return [_actManager_Ask3PRestriction.Value];
        }
        /// <summary>
        /// Returns lowest rate options. [0] Lowest rate on/off, [1] Affects PC, [2] Affects NPC. 
        /// </summary>
        /// <returns></returns>
        public static bool[] GetActionLowestRateEnable()
        {
            bool[] lowesRate = [
                _enableActionLowestRate.Value,
                (_charaType_CharaAnswerRate.Value & CharaType.PC) != 0,
                (_charaType_CharaAnswerRate.Value & CharaType.NPC) != 0
                ];
            return lowesRate;
        }    
        /// <summary>
        /// Returns the new movement speeds. [0] PC Walk, [1] PC Run, [2] NPC Walk, [3] NPC Run.
        /// </summary>
        /// <returns></returns>
        public static float[] GetCharaWalkRunSpeed()
        {
            return [_pcWalkSpeed.Value, _pcRunSpeed.Value, _npcWalkSpeed.Value, _npcRunSpeed.Value];
        }
        /// <summary>
        /// Returns Cheater Status Manager enabled settings. [0] on/Off, [1] Ignore PC. [2] Friendly hugs.
        /// </summary>
        /// <returns></returns>
        public static bool[] GetCheaterManagerOptions()
        {
            return [_cheaterStatus_Enable.Value, _cheaterStatus_IgnorePC.Value, _cheaterStatus_DisableHugIsSkinship.Value];
        }
        public static int GetCheaterManagerValues()
        {
            return _cheaterStatus_hatePoints.Value;
        }
        /// <summary>
        /// Returns Cheat options. [0] Force H, [1] Force ADV 3P, [2] Force NPC 3P.
        /// </summary>
        /// <returns></returns>
        public static bool[] GetCheats()
        {
            return [_cheat_forceHAction.Value, _cheat_forceThreesome.Value, _cheat_forceThreesomeNPC.Value];
        }
        public static bool[] GetForceActions()
        {
            bool[] forceActions = [_forceBreakUp.Value];

            return forceActions;
        }
        public static bool GetFortuneFix()
        {
            return _fortuneSuccessFix.Value;
        }
        /// <summary>
        /// Returns game fixes options. [0] Apply Fixes, [1] Accept BBQ, [2] Are you dating fix.
        /// </summary>
        /// <returns></returns>
        public static bool[] GetGameFixes()
        {
            bool[] fixes = [_applyGameFixes.Value,
                            _bbqAnswerType.Value == HighVirtueBBQ.Accept,
                            _applyAreYouDatingFix.Value];
            return fixes;
        }
        public static HighVirtueBBQ GetHighVirtueBBQ()
        {
            return _bbqAnswerType.Value;
        }
        public static bool GetKidnapOption()
        {
            return _actManager_disableKidnapping.Value;
        }
        public static NPCSex GetNPCSex()
        {
            return _sim_NPCLowPolySex.Value;
        }
        /// <summary>
        /// Returns min and max values as string. [0] Min, [1] Max.
        /// </summary>
        /// <returns></returns>
        public static string[] GetNPCSexDuration()
        {
            if (_sim_NPCSexTimerMin.Value > _sim_NPCSexTimerMax.Value) return [_sim_NPCSexTimerMax.Value.ToString(), _sim_NPCSexTimerMin.Value.ToString()];
            return [_sim_NPCSexTimerMin.Value.ToString(), _sim_NPCSexTimerMax.Value.ToString()];
        }
        public static float GetNewRate()
        {
            return _lowestActionRate.Value;
        }
        public static bool GetIsNightEventActive()
        {
            return _nightEvent_active.Value;
        }
        public static int[] GetNightChance()
        {
            return [_nightChance.Value, _nightChanceH.Value];
        }
        public static bool GetNormalVirtueBalance()
        {
            return _enable_NormalVirtueBalance.Value;
        }
        public static bool GetNoSkipNPCOffScreenInteractions()
        {
            return _sim_DoNotSkipNPC.Value;
        }
        /// <summary>
        /// Returns NPC rates for 3P. [0] NPC join rate, [1] NPC ask rate.
        /// </summary>
        /// <returns></returns>
        public static int[] GetNPCJoinAsk3PRate()
        {
            return [_NPC_3PJoinRate.Value, _NPC_3PAskRate.Value];
        }
        public static bool GetPCJoin3P()
        {
            return _enablePC_3PJoin.Value;
        }
        public static bool GetPCFollowProtection()
        {
            return _makePCNonInteractable.Value;
        }
        public static DisplayPercentage GetProbDisplay()
        {
            return _actionPorcentageDisplay.Value;
        }
        /// <summary>
        /// Returns enabled options. [0] Reaction Manager On/Off, [1] Affects PC, [2] Affects NPC.
        /// </summary>
        /// <returns></returns>
        public static bool[] GetReactionManagerOptions()
        {
            bool affectsPC = false;
            bool affectsNPC = false;
            if ((_reaction_CharaType.Value & CharaType.PC) != 0) affectsPC = true;
            if ((_reaction_CharaType.Value & CharaType.NPC) != 0) affectsNPC = true;
            return [_reaction_ReactionManagerEnable.Value, affectsPC, affectsNPC];
        }
        /// <summary>
        /// Returns the reaction reduce rates. [0] All, [1] Normal, [2] Skinship/Contact, [3] Sex.
        /// </summary>
        /// <returns></returns>
        public static int[] GetReactionReduceRate()
        {
            return [_reactionChanceOverall.Value, _reactionChanceNormal.Value, _reactionChanceSkinship.Value, _reactionChanceH.Value];
        }
        public static bool GetShowLog()
        {
            return _showLog.Value;
        }
        /// <summary>
        /// Returns enabled stats reduction settings. [0] On/Off, [1] Ignore PC
        /// </summary>
        /// <returns></returns>
        public static bool[] GetStatsReductionSettings()
        {
            return [_statsReduc_enable.Value, _statsReduc_PCReduction.Value];
        }
        public static float GetStatsReductionRate()
        {
            return _statsReduc_reduceRate.Value;
        }
        /// <summary>
        /// Return hotkeys. [0] switch PC, [1] Auto PC, [2] Auto PC Voice
        /// </summary>
        /// <returns></returns>
        public static KeyCode[] GetToggleKeys()
        {
            return [toggleKey_SwitchPCCharacter.Value, toggleKey_AutoPC.Value, toggleKey_AutoCharaVoice.Value];
        }        
    }
}
