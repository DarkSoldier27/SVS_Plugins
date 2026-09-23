using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Character;
using CharacterCreation;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SaveData;
using SV;
using SV.CoordeSelectScene;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SVS_MoreOutfits
{
    [BepInPlugin(GUID, DisplayName, Version)]
    public class MoreOutfitsPlugin : BasePlugin
    {
        public const string DisplayName = Constants.Name;
        public const string GUID = "SVS_MoreOutfits";
        public const string Version = Constants.Version;

        internal static new ManualLogSource Log;
        //private static Harmony patchedHooks;
        private static ConfigEntry<int> maxOutfits { get; set; }
        
        private static ConfigEntry<bool> _disable_MoreOutfits;
        private static ConfigEntry<bool> changePCOutfit;

        private static ConfigEntry<bool> weekendOutfit;
        private static ConfigEntry<bool> nightOutfit;
        private static ConfigEntry<bool> lewdOutfit;
        private static ConfigEntry<bool> costumeOutfit;
        private static ConfigEntry<bool> sportsOutfit;
        private static ConfigEntry<bool> bathOutfit;
        private static ConfigEntry<bool> campingOutfit;
        private static ConfigEntry<bool> homeOutfit;

        private static ConfigEntry<GameDay> costumeDay;
        private static ConfigEntry<GamePeriod> costumePeriod;


        //internal static ConfigEntry<KeyCode> toggleKey_Test { get; set; }

        private static readonly List<string> outfitsList =
        [
            "Weekend",//0
            "Night",//1
            "Lewd",//2
            "Costume",//3
            "Sports",//4
            "Bath",//5
            "Camping",//6
            "Home",//7
            "Swimsuit 2",//8
            "Outfit 14",
            "Outfit 15",
            "Outfit 16",
            "Outfit 17"
        ];

        public override void Load()
        {
            // Plugin startup logic
            Log = base.Log;

            maxOutfits = Config.Bind("Outfits", "Set Outfit Amount", 8, new ConfigDescription("DO NOT CHANGE THIS! unless you know what you are doing\nSet the Max amount of outfits for all characters.", new AcceptableValueRange<int>(3, 17), new ConfigurationManagerAttributes { IsAdvanced = true, Order = 20 }));

            _disable_MoreOutfits = Config.Bind("Outfits", "Disable More Outfits", false, new ConfigDescription("Disables this mod and characters will only use the default 3 outfits.\nDisabling individual outfits is recommended, otherwise it may cause incompatibility issues with other mods that uses Custom Outfits.", null, new ConfigurationManagerAttributes { Order = 19 }));

            weekendOutfit = Config.Bind("Outfits", "Use Weekend Outfit", true, new ConfigDescription("Characters will use their weekend Outfits durin Saturday and Sunday", null, new ConfigurationManagerAttributes { Order = 17 }));
            nightOutfit = Config.Bind("Outfits", "Use Night Outfit", true, new ConfigDescription("Characters will use their Night Outfits, they will change during the evening", null, new ConfigurationManagerAttributes { Order = 16 }));
            lewdOutfit = Config.Bind("Outfits", "Use Lewd Outfit", true, new ConfigDescription("Characters will use their Lewd Outfits when get a horny fortune ", null, new ConfigurationManagerAttributes { Order = 15 }));
            costumeOutfit = Config.Bind("Outfits", "Use Costume", false, new ConfigDescription("Characters will use a costume on the day and period specify. Note: This does not override the job or swimsuit outfits, those outfits take priority over this one", null, new ConfigurationManagerAttributes { Order = 14 }));
            sportsOutfit = Config.Bind("Outfits", "Use Sports Outfits", false, new ConfigDescription("Characters will use their sport outfits", null, new ConfigurationManagerAttributes { Order = 13 }));
            bathOutfit = Config.Bind("Outfits", "Use Bath Outfits", false, new ConfigDescription("Characters will use their Bath outfits", null, new ConfigurationManagerAttributes { Browsable = false,Order = 12 }));
            campingOutfit = Config.Bind("Outfits", "Use Camping Outfits", false, new ConfigDescription("Characters will use their Camping outfits", null, new ConfigurationManagerAttributes { Browsable = false, Order = 11 }));
            homeOutfit = Config.Bind("Outfits", "Use Home Outfits", false, new ConfigDescription("Characters will use their Home outfits", null, new ConfigurationManagerAttributes {Browsable = false, Order = 10 }));

            costumeDay = Config.Bind("Outfits Settings", "Set Costume Day", GameDay.None,
            new ConfigDescription("Set the day when the costume will be use", null, new ConfigurationManagerAttributes { Order = 9 }));
            costumePeriod = Config.Bind("Outfits Settings", "Set Costume Period", GamePeriod.Morning | GamePeriod.Midday | GamePeriod.Evening | GamePeriod.Night,
            new ConfigDescription("Set the Period when the costume will be use", null, new ConfigurationManagerAttributes { Order = 8 }));

            //patchedHooks =
            Harmony.CreateAndPatchAll(typeof(Hooks));
        }
        public enum GameDay
        {
            None = -1,
            Monday = 0,
            Tuesday = 1,
            Wednesday = 2,
            Thursday = 3,
            Friday = 4,
            Saturday = 5,
            Sunday = 6,
            Weekend = 7,
            All = 8,
        }

        [Flags]
        public enum GamePeriod
        {
            None = 0,
            Morning = 1 << 0,
            Midday = 1 << 1,
            Evening = 1 << 2,
            Night = 1 << 3,
        }

        public static int GetMaxOutfits()
        {
            return maxOutfits.Value;
        }
        public static List<string> GetCustomOufitList()
        {
            return outfitsList;
        }
        public static bool GetDisableCustomOutfits()
        {
            return _disable_MoreOutfits.Value;
        }
        /// <summary>
        /// Returns outfit is on. [0] Weekend, [1] Night, [2] Lewd, [3] Costume, [4] Sport, [5] Bath, [6] Camping, [7] Home.
        /// </summary>
        /// <returns></returns>
        public static bool[] GetUseOutfit()
        {
            return [
                weekendOutfit.Value,
                nightOutfit.Value,
                lewdOutfit.Value,
                costumeOutfit.Value,
                sportsOutfit.Value,
                bathOutfit.Value,
                campingOutfit.Value,
                homeOutfit.Value
                ];
        }

        public static ValueTuple<int, bool[]> GetCostumeDayAndPeriod()
        {
            var dayAndPeriod = new ValueTuple<int, bool[]>(-1, [false, false, false, false]);
            switch (costumeDay.Value)
            {
                case GameDay.None:
                    dayAndPeriod.Item1 = -1;
                    break;
                case GameDay.Monday:
                    dayAndPeriod.Item1 = 0;
                    break;
                case GameDay.Tuesday:
                    dayAndPeriod.Item1 = 1;
                    break;
                case GameDay.Wednesday:
                    dayAndPeriod.Item1 = 2;
                    break;
                case GameDay.Thursday:
                    dayAndPeriod.Item1 = 3;
                    break;
                case GameDay.Friday:
                    dayAndPeriod.Item1 = 4;
                    break;
                case GameDay.Saturday:
                    dayAndPeriod.Item1 = 5;
                    break;
                case GameDay.Sunday:
                    dayAndPeriod.Item1 = 6;
                    break;
                case GameDay.Weekend:
                    dayAndPeriod.Item1 = 7;
                    break;
                case GameDay.All:
                    dayAndPeriod.Item1 = 8;
                    break;
            }

            if ((costumePeriod.Value & GamePeriod.Morning) != 0) dayAndPeriod.Item2[0] = true;
            if ((costumePeriod.Value & GamePeriod.Midday) != 0) dayAndPeriod.Item2[1] = true;
            if ((costumePeriod.Value & GamePeriod.Evening) != 0) dayAndPeriod.Item2[2] = true;
            if ((costumePeriod.Value & GamePeriod.Night) != 0) dayAndPeriod.Item2[3] = true;

            return dayAndPeriod;
        }
        internal static class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HumanCustom), nameof(HumanCustom.Start))]
            public static void CreateOutfitsSlotsMaker(HumanCustom __instance)
            {
                if (maxOutfits.Value > 3) MoreOutfitsUI.CreateNewOutfitIcons(outfitsList, maxOutfits.Value);
            }

            [HarmonyPrefix] //On Postfix, toggles don't work
            [HarmonyPatch(typeof(CoordeSelect), nameof(CoordeSelect.Initialize))]
            public static void CreateOutfitsSlotsSimulation(CoordeSelect __instance)
            {
                if (maxOutfits.Value > 3) MoreOutfitsUI.CreateNewOutfitIcons(outfitsList, maxOutfits.Value);
            }

            [HarmonyPostfix] 
            [HarmonyPatch(typeof(CoordeSelect), nameof(CoordeSelect.SetOpenCloseEvent))]
            public static void DisplayOutfitsSlotsSimulation(CoordeSelect __instance, bool open)
            {
                if (open) MoreOutfitsUI.DisplayCharaCoordinates(__instance);
            }

            [HarmonyPrefix] //Load Character with new outfits
            [HarmonyPatch(typeof(HumanData), nameof(HumanData.Copy))]
            public static void IncreaseCharaOutfits(HumanData dst, HumanData src)
            {
                MoreOutfits.IncreaseCharaCoordinateSlot(dst, src);
            }

            [HarmonyPriority(800)]
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HumanData), nameof(HumanData.SetCoordinateBytes))]
            public static void SetCharaCoordinate(HumanData __instance, Il2CppStructArray<byte> data)
            {
                MoreOutfits.SetCharaCoordinates(__instance, data, false);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HumanData), nameof(HumanData.SetCoordinateBytes))]
            public static void CheckCoordBytePost(HumanData __instance, Il2CppStructArray<byte> data)
            {
                MoreOutfits.SetCharaCoordinates(__instance, data, true);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CoordeSelect), nameof(CoordeSelect.SetCoordeTitle))]
            public static void CustomCoordTittle(CoordeSelect __instance, int type)
            {
                if (type > 2)
                {
                    __instance._txtCoordeTitle._tmpText.text = outfitsList[type - 3] + __instance._txtCoordeTitle._tmpText.text;
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CoordinateTypeChange), nameof(CoordinateTypeChange.ChangeType))]
            public static void DisplayOutfitName(CoordinateTypeChange __instance, int type)
            {
                MoreOutfitsUI.DisplayOutfits(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CustomDropdown), nameof(CustomDropdown.SetOptionCoordinateTypeNames))]
            public static void NewDropdown(CustomDropdown __instance)
            {
                MoreOutfitsUI.AddOutfitsToDropdown(__instance);
            }

            [HarmonyPriority(800)]
            [HarmonyPostfix]
            [HarmonyPatch(typeof(ThinkingManager), nameof(ThinkingManager.GetChangeOfClothesNum))]
            public static int ChangeClothesPerPeriodPost(int __result, Actor _actor, bool _isStart, int _timezone)
            {
                return MoreOutfits.ChangeOutfits(_actor, _isStart, _timezone, __result);                
            }
        }
    }
}
