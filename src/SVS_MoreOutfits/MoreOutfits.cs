using BepInEx.Logging;
using Character;
using CharacterCreation;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Manager;
using SaveData;
using SV;
using SV.SaveLoadScene;
using SV.Title;

namespace SVS_MoreOutfits
{
    internal static class MoreOutfits
    {
        private static readonly Il2CppSystem.Collections.Generic.List<ThinkingManager.ChangeClothInfos> changeClothInfosList = new();
        public static int GetTimeOfDay()
        {
            if (SimulationManager.Instance is null) return -1;
            switch (SimulationManager.Instance.Mode)
            {
                case SimulationManager.SimulationMode.RoomMorning_SimMorning: return 0;
                case SimulationManager.SimulationMode.SimMorning: return 0;
                case SimulationManager.SimulationMode.SimMorning_SimNoon: return 1;
                case SimulationManager.SimulationMode.SimNoon: return 1;
                case SimulationManager.SimulationMode.SimNoon_SimEvening: return 2;
                case SimulationManager.SimulationMode.SimEvening: return 2;
                case SimulationManager.SimulationMode.SimEvening_SimNight: return 3;
                case SimulationManager.SimulationMode.SimNight: return 3;
            }
            return -1;
        }
        public static int GetInUseFortuneID()
        {
            if (Game.saveData.dataCount.isCircularNoticeUse) return Game.saveData.dataCount.circularNotice;
            return -1;
        }
        public static void IncreaseCharaCoordinateSlot(HumanData dst, HumanData src)
        {
            var maxOutfits = MoreOutfitsPlugin.GetMaxOutfits();
            if (dst.Coordinates.Count != maxOutfits)
            {
                Il2CppReferenceArray<HumanDataCoordinate> moreCoordinate = new Il2CppReferenceArray<HumanDataCoordinate>(maxOutfits);
                for (int i = 0; i < maxOutfits; i++)
                {
                    if (i >= dst.Coordinates.Count) moreCoordinate[i] = new(dst.Coordinates[0]);
                    else moreCoordinate[i] = dst.Coordinates[i];
                }
                dst.Coordinates = moreCoordinate;
            }
        }

        public static void SetCharaCoordinates(HumanData human, Il2CppStructArray<byte> data, bool isLoaded)
        {
            if (human is null) return;
            var maxCustomOutfits = MoreOutfitsPlugin.GetMaxOutfits();
            var charaCardCoords = HumanData.CoordinateByteData.Deserialize(data);
            
            int maxOutfits = maxCustomOutfits;
            if (HumanCustom.Instance == null && charaCardCoords.Count <= maxCustomOutfits) maxOutfits = charaCardCoords.Count;
            if (isLoaded)
            {
                //Set default clothes if character did not have extra outfits.
                for (int i = 0; i < human.Coordinates.Count; i++)
                {
                    if (i >= charaCardCoords.Count) human.Coordinates[i] = new(human.Coordinates[0]);
                }
                //Shows a warning if characters don't have extra outfits when loading a save file.
                if (SaveLoad.Instance != null)
                {
                    if (SaveLoad.Instance.IsTransitionUI) return;
                    if (HumanCustom.Instance != null) return;

                    if (charaCardCoords.Count < MoreOutfitsPlugin.GetMaxOutfits() && SaveLoad.Instance.isActiveAndEnabled)
                    {
                        MoreOutfitsPlugin.Log.Log(LogLevel.Message, $"Custom outfits have not been added to one or more characters\nCasual outfits will be use instead");
                        MoreOutfitsPlugin.Log.LogInfo($"Current Coordinates Amount: {human.Coordinates.Count} - Chara Card Coordinates Amount: {charaCardCoords.Count}");
                    }
                }
            }
            else
            {
                //Sets new coordinate array
                if (human.Coordinates.Count != maxOutfits)
                {
                    Il2CppReferenceArray<HumanDataCoordinate> moreCoordinate = new Il2CppReferenceArray<HumanDataCoordinate>(maxOutfits);
                    for (int i = 0; i < maxOutfits; i++)
                    {
                        if (i >= human.Coordinates.Count) moreCoordinate[i] = new(human.Coordinates[0]);
                        else if (i >= charaCardCoords.Count) moreCoordinate[i] = new(human.Coordinates[0]);
                        else moreCoordinate[i] = human.Coordinates[i];
                    }
                    human.Coordinates = moreCoordinate;
                }
            }
        }
        public static int ChangeOutfits(Actor chara, bool isStart, int timezone, int outfit)
        {
            if (MoreOutfitsPlugin.GetDisableCustomOutfits())
            {
                if (chara.charFile.Coordinates.Count <= outfit) return 0;
                return outfit;
            } 
            int newOutfit = outfit;
            int currentOutfit = chara.charFile.Status.coordinateType;

            if (MoreOutfitsPlugin.GetUseOutfit()[0]) newOutfit = MoreOutfitsConditions.OutfitWeekend(chara, newOutfit);
            if (MoreOutfitsPlugin.GetUseOutfit()[2]) newOutfit = MoreOutfitsConditions.OutfitLewd(newOutfit);
            if (MoreOutfitsPlugin.GetUseOutfit()[1]) newOutfit = MoreOutfitsConditions.OutfitNight(isStart, timezone, newOutfit);
            if (MoreOutfitsPlugin.GetUseOutfit()[3]) newOutfit = MoreOutfitsConditions.OutfitCostume(timezone, newOutfit);
            if (!MoreOutfitsPlugin.GetUseOutfit()[4] && (outfit == 7 || newOutfit == 7)) newOutfit = 1;
            if (!MoreOutfitsPlugin.GetUseOutfit()[5] && (outfit == 8 || newOutfit == 8)) newOutfit = 0;

            if (isStart && timezone == 0)
            {
                if (chara.Job > 3) chara.charaBase.IsFirstClothChange = false;
                if (chara.charFile.Coordinates.Count <= newOutfit) return 0;
                else return newOutfit;
            }
            else if (isStart && currentOutfit != newOutfit)
            {
                return currentOutfit;
            }

            if (chara.charFile.Coordinates.Count <= newOutfit)
            {
                if (chara.charFile.Coordinates.Count <= outfit) return 0;
                else return outfit;
            }
            else if (newOutfit > -1 && newOutfit != outfit) return newOutfit;
            return outfit;
        }
    }
}
