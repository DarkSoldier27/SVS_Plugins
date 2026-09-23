using Manager;
using SaveData;

namespace SVS_MoreOutfits
{
    internal class MoreOutfitsConditions
    {
        public static int OutfitWeekend(Actor chara, int oldOutfit)
        {
            int weekDay = Game.saveData.Week;
            if (weekDay > 4 && (oldOutfit == 0 || (chara.Job == 0 && oldOutfit == 1)))
            {
                return 3;
            }
            return oldOutfit;
        }

        public static int OutfitNight(bool isStart, int timezone, int oldOutfit)
        {
            switch (timezone)
            {
                case 3:
                    if (!isStart) return 4;
                    break;
                case -1:
                    if (MoreOutfits.GetTimeOfDay() == 3) return 4;
                    break;
            }
            return oldOutfit;
        }
        public static int OutfitLewd(int oldOutfit)
        {
            if (MoreOutfits.GetInUseFortuneID() == 1 && (oldOutfit == 0 || oldOutfit == 3))
            {
                return 5;
            }
            return oldOutfit;
        }

        public static int OutfitCostume(int timezone, int oldOutfit)
        {
            var dayAndPeriod = MoreOutfitsPlugin.GetCostumeDayAndPeriod();
            int costumeID = 6;
            int weekDay = Game.saveData.Week;
            if (weekDay == dayAndPeriod.Item1)
            {
                switch (timezone)
                {
                    case 0:
                        if (dayAndPeriod.Item2[0]) return costumeID;
                        break;
                    case 1:
                        if (dayAndPeriod.Item2[1]) return costumeID;
                        break;
                    case 2:
                        if (dayAndPeriod.Item2[2]) return costumeID;
                        break;
                    case 3:
                        if (dayAndPeriod.Item2[3]) return costumeID;
                        break;
                    case -1:
                        if (dayAndPeriod.Item2[0] && MoreOutfits.GetTimeOfDay() == 0) return costumeID;
                        if (dayAndPeriod.Item2[1] && MoreOutfits.GetTimeOfDay() == 1) return costumeID;
                        if (dayAndPeriod.Item2[2] && MoreOutfits.GetTimeOfDay() == 2) return costumeID;
                        if (dayAndPeriod.Item2[3] && MoreOutfits.GetTimeOfDay() == 3) return costumeID;
                        break;
                }
            }
            else if (dayAndPeriod.Item1 == 7 && weekDay > 4)
            {
                switch (timezone)
                {
                    case 0:
                        if (dayAndPeriod.Item2[0]) return costumeID;
                        break;
                    case 1:
                        if (dayAndPeriod.Item2[1]) return costumeID;
                        break;
                    case 2:
                        if (dayAndPeriod.Item2[2]) return costumeID;
                        break;
                    case 3:
                        if (dayAndPeriod.Item2[3]) return costumeID;
                        break;
                    case -1:
                        if (dayAndPeriod.Item2[0] && MoreOutfits.GetTimeOfDay() == 0) return costumeID;
                        if (dayAndPeriod.Item2[1] && MoreOutfits.GetTimeOfDay() == 1) return costumeID;
                        if (dayAndPeriod.Item2[2] && MoreOutfits.GetTimeOfDay() == 2) return costumeID;
                        if (dayAndPeriod.Item2[3] && MoreOutfits.GetTimeOfDay() == 3) return costumeID;
                        break;
                }
            }
            else if (dayAndPeriod.Item1 == 8)
            {
                switch (timezone)
                {
                    case 0:
                        if (dayAndPeriod.Item2[0]) return costumeID;
                        break;
                    case 1:
                        if (dayAndPeriod.Item2[1]) return costumeID;
                        break;
                    case 2:
                        if (dayAndPeriod.Item2[2]) return costumeID;
                        break;
                    case 3:
                        if (dayAndPeriod.Item2[3]) return costumeID;
                        break;
                    case -1:
                        if (dayAndPeriod.Item2[0] && MoreOutfits.GetTimeOfDay() == 0) return costumeID;
                        if (dayAndPeriod.Item2[1] && MoreOutfits.GetTimeOfDay() == 1) return costumeID;
                        if (dayAndPeriod.Item2[2] && MoreOutfits.GetTimeOfDay() == 2) return costumeID;
                        if (dayAndPeriod.Item2[3] && MoreOutfits.GetTimeOfDay() == 3) return costumeID;
                        break;
                }
            }              
            return oldOutfit;
        }
    }
}
