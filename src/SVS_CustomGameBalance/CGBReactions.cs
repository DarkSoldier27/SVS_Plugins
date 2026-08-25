using ADV;
using SaveData;
using SV.Chara;

namespace SVS_CustomGameBalance
{
    internal class CGBReactions
    {
        private static readonly System.Random _rnd = new();
        public static int SetReaction(AI charaAI, AI charaAI1, AI charaAI2, int reactionCategory, int reactionNo)
        {
            if (charaAI is null || charaAI1 is null || charaAI2 is null) return reactionNo;
            //Always join 3P no matter what.
            if (CustomGameBalancePlugin.GetCheats()[2])
            {
                if (reactionCategory == 1)
                {
                    if (charaAI.chaCtrl.sex == 0 && charaAI1.chaCtrl.sex == 0 && charaAI2.chaCtrl.sex == 0) return reactionNo;
                    if (charaAI.chaCtrl.sex == 1 && charaAI1.chaCtrl.sex == 1 && charaAI2.chaCtrl.sex == 1)
                    {
                        if (charaAI.chaCtrl.fileParam.isFutanari || charaAI1.chaCtrl.fileParam.isFutanari || charaAI2.chaCtrl.fileParam.isFutanari) return 4;
                        return reactionNo;
                    }
                    return 4;
                }
            }
            return ReactionChance(charaAI, charaAI1, charaAI2, reactionCategory, reactionNo);
        }
        public static int ReactionChance(AI charaAI, AI charaAI1, AI charaAI2, int reactCategory, int reactionNo)
        {
            if (!CustomGameBalancePlugin.GetReactionManagerOptions()[0]) return reactionNo;
            if (charaAI is null) return reactionNo;

            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Reaction Manager, reaction type {reactionNo} - category {reactCategory}");

            bool[] affectsChara = [CustomGameBalancePlugin.GetReactionManagerOptions()[1], CustomGameBalancePlugin.GetReactionManagerOptions()[2]];
            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Reduce interruptions for: PC {affectsChara[0]}, NPC {affectsChara[1]}");
            if (!affectsChara[0])
            {
                if (charaAI1 != null)
                {
                    if (charaAI1.charaData.IsPC) return reactionNo;
                }
                if (charaAI2 != null)
                {
                    if (charaAI2.charaData.IsPC) return reactionNo;
                }
            }
            if (!affectsChara[1])
            {
                if (charaAI1 != null && charaAI2 != null)
                {
                    if (!charaAI1.charaData.IsPC && !charaAI2.charaData.IsPC) return reactionNo;
                }
                else if (charaAI1 != null && charaAI2 is null)
                {
                    if (!charaAI1.charaData.IsPC) return reactionNo;
                }
                else if (charaAI1 is null && charaAI2 != null)
                {
                    if (!charaAI2.charaData.IsPC) return reactionNo;
                }
            }

            if (reactCategory == 1 && CanJoinThreesome(charaAI.charaData, charaAI1.charaData, charaAI2.charaData, false)) return 4;

            return ReduceReactionRate(charaAI.charaData, reactCategory, reactionNo);
        }
        public static int ReduceReactionRate(Actor chara, int reactionCategory, int reactionNo)
        {
            int maxChance = 100;
            int[] reduceRate = CustomGameBalancePlugin.GetReactionReduceRate();

            if (chara.gameParameter.individuality.answer.Contains(10)) maxChance += 33;
            if (chara.gameParameter.individuality.answer.Contains(36)) maxChance += 13;
            if (chara.gameParameter.individuality.answer.Contains(38)) maxChance -= 66;
            if (maxChance < 0) maxChance = 0;

            int prob = _rnd.Next(0, maxChance);
            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Reducing chance: {reduceRate[0]}/{prob} - For reaction type: {reactionNo} - Category: {reactionCategory}");
            if (reduceRate[0] > prob) return -1;

            switch (reactionCategory)
            {
                case 0://Unknown.
                    break;
                case 1://Reacting to public sex.
                    switch (reactionNo)
                    {
                        case 2:
                            prob = _rnd.Next(0, maxChance);
                            if (reduceRate[3] > prob) return -1;
                            break;
                        case 4:
                            break;
                    }                   
                    break;
                case 2://Reacting to masturbation.
                    break;
                case 5://Reacting to fights.
                    break;
                case 6://Reacting to skinship.
                    switch (reactionNo)
                    {
                        case 0:
                            prob = _rnd.Next(0, maxChance);
                            if (reduceRate[2] > prob) return -1;
                            break;
                        case 1:
                            prob = _rnd.Next(0, maxChance);
                            if (reduceRate[2] > prob) return -1;
                            break;
                    }
                    break;
                case 7://Reacting to characters interacting with each other.
                    switch (reactionNo)
                    {
                        case 0:
                            prob = _rnd.Next(0, maxChance);
                            if (reduceRate[1] > prob) return -1;
                            break;
                        case 1:
                            prob = _rnd.Next(0, maxChance);
                            if (reduceRate[1] > prob) return -1;
                            break;
                    }
                    break;
                case 8://Unknown.
                    break;
                case 9://Unknown.
                    break;
                case 10://Racting to 3P.
                    break;
            }

            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Reaction not changed! - Reaction Type: {reactionNo} - Category: {reactionCategory}");
            return reactionNo;
        }
        public static bool CanJoinThreesome(Actor chara, Actor actor1, Actor actor2, bool isADV)
        {
            if (chara is null || actor1 is null || actor2 is null) return false;
            if (chara.gameParameter.individuality.answer.Contains(29) || actor1.gameParameter.individuality.answer.Contains(29) || actor2.gameParameter.individuality.answer.Contains(29))
            {
                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Character can not join 3P. 1 or more characters have the Singleminded trait, 3P not possible");
                return false;
            }
            int rate = CustomGameBalancePlugin.GetNPCJoinAsk3PRate()[0];
            if (chara.parameter.sex == 0 && actor1.parameter.sex == 0 && actor2.parameter.sex == 0) return false;
            if (chara.parameter.sex == 1 && actor1.parameter.sex == 1 && actor2.parameter.sex == 1)
            {
                if (!chara.parameter.isFutanari && !actor1.parameter.isFutanari && !actor2.parameter.isFutanari) return false;
            }

            if (!chara.IsPC && !isADV)
            {
                switch (chara.gameParameter.SexualTarget)
                {
                    case 0://In case of gay mod is made
                        if (chara.parameter.sex == 0 && (actor1.parameter.sex == 0 && actor2.parameter.sex == 0)) return false;
                        if (chara.parameter.sex == 1 && (actor1.parameter.sex == 1 && actor2.parameter.sex == 1)) return false;
                        break;
                    case 4:
                        if (chara.parameter.sex == 0 && (actor1.parameter.sex == 1 || actor2.parameter.sex == 1)) return false;
                        if (chara.parameter.sex == 1 && (actor1.parameter.sex == 0 || actor2.parameter.sex == 0)) return false;
                        break;
                }
            }

            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Can character ID [{chara.charasGameParam.Index}] join 3P?");
            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Is ADV 3P: {isADV}");

            bool[] charasAgree = [false, false, false];

            if (isADV && chara.IsPC) charasAgree[0] = true;
            else charasAgree[0] = ThreesomeJoinConditions_Virtue(chara, actor1, actor2);

            if (isADV && actor1.IsPC) charasAgree[1] = true;
            else charasAgree[1] = ThreesomeJoinConditions_Virtue(actor1, chara, actor2);

            if (isADV && actor2.IsPC) charasAgree[2] = true;
            else charasAgree[2] = ThreesomeJoinConditions_Virtue(actor2, chara, actor1);

            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"CharaID {chara.charasGameParam.Index} Accepts 3P: {charasAgree[0]}");
            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"CharaID {actor1.charasGameParam.Index} Accepts 3P: {charasAgree[1]}");
            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"CharaID {actor2.charasGameParam.Index} Accepts 3P: {charasAgree[2]}");

            if (charasAgree[0] && charasAgree[1] && charasAgree[2])
            {
                if (isADV) return true;
                int chance3P = _rnd.Next(0, 100);
                rate += ThreesomeJoinRateByTraits(chara, actor1, actor2);
                if (chance3P < rate) return true;
            }
            return false;
        }
        private static bool ThreesomeJoinConditions_Virtue(Actor chara, Actor partner1, Actor partner2)
        {
            int virtue = chara.gameParameter.LvChastity;
            int partner1ID = partner1.charasGameParam.Index;
            int partner2ID = partner2.charasGameParam.Index;

            bool isLover1 = false;
            bool isLover2 = false;

            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"CharaID: {chara.charasGameParam.Index} - Virtue LV: {virtue}");
            //Conditions base on Virtue/Chastity Lv
            switch (virtue)
            {
                case 0:
                    return true;
                case 1:
                    if (chara.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(partner1ID) && chara.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(partner2ID))
                    {
                        if (chara.charasGameParam.sensitivity.tableFavorabiliry[partner1ID].mostRanks.Count > 0 && chara.charasGameParam.sensitivity.tableFavorabiliry[partner2ID].mostRanks.Count > 0)
                        {
                            if (chara.charasGameParam.sensitivity.tableFavorabiliry[partner1ID].mostRanks[0] < 2 && chara.charasGameParam.sensitivity.tableFavorabiliry[partner2ID].mostRanks[0] < 2) return true;
                        }
                    }
                    return false;
                case 2:
                    if (chara.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(partner1ID) && chara.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(partner2ID))
                    {
                        if (chara.charasGameParam.sensitivity.tableFavorabiliry[partner1ID].longSensitivityCounts[0] > 10 && chara.charasGameParam.sensitivity.tableFavorabiliry[partner2ID].longSensitivityCounts[0] > 10) return true;
                    }
                    return false;
                case 3:
                    if (chara.charasGameParam.memory.pairTable.ContainsKey(partner1ID) && chara.charasGameParam.memory.pairTable.ContainsKey(partner2ID))
                    {
                        //Check if they had sex before
                        if (chara.charasGameParam.memory.pairTable[partner1ID].TotalH > 0 && chara.charasGameParam.memory.pairTable[partner2ID].TotalH > 0)
                        {
                            if (chara.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(partner1ID) && chara.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(partner2ID))
                            {
                                if (chara.charasGameParam.sensitivity.tableFavorabiliry[partner1ID].longSensitivityCounts[0] > 20 && chara.charasGameParam.sensitivity.tableFavorabiliry[partner2ID].longSensitivityCounts[0] > 20) return true;
                            }
                        }
                    }
                    return false;
                case 4:
                    if (chara.charasGameParam.memory.pairTable.ContainsKey(partner1ID) && chara.charasGameParam.memory.pairTable.ContainsKey(partner2ID))
                    {
                        //Check if they had sex before
                        if (chara.charasGameParam.memory.pairTable[partner1ID].TotalH > 0 && chara.charasGameParam.memory.pairTable[partner2ID].TotalH > 0)
                        {
                            if (chara.charasGameParam.memory.lovers.Count > 0)
                            {
                                foreach (var lover in chara.charasGameParam.memory.lovers)
                                {
                                    if (lover.id == partner1ID) isLover1 = true;
                                    if (lover.id == partner2ID) isLover2 = true;
                                }
                            }
                            if (isLover1 && isLover2)
                            {
                                if (chara.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(partner1ID) && chara.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(partner2ID))
                                {
                                    if (chara.charasGameParam.sensitivity.tableFavorabiliry[partner1ID].longSensitivityCounts[0] > 20 && chara.charasGameParam.sensitivity.tableFavorabiliry[partner2ID].longSensitivityCounts[0] > 20) return true;
                                }
                            }
                            else if (chara.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(partner1ID) && chara.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(partner2ID))
                            {
                                if (chara.charasGameParam.sensitivity.tableFavorabiliry[partner1ID].longSensitivityCounts[0] > 25 && chara.charasGameParam.sensitivity.tableFavorabiliry[partner2ID].longSensitivityCounts[0] > 25) return true;
                            }
                        }
                    }
                    return false;
            }
            return false;
        }
        private static int ThreesomeJoinRateByTraits(Actor chara, Actor partner1, Actor partner2)
        {
            int addRate = 0;
            if (chara.gameParameter.individuality.answer.Contains(2))
            {
                if (partner1.parameter.sex == 0) addRate -= 5;
                if (partner2.parameter.sex == 0) addRate -= 5;
            }
            if (chara.gameParameter.individuality.answer.Contains(3))
            {
                if (partner1.parameter.sex == 1) addRate -= 5;
                if (partner2.parameter.sex == 1) addRate -= 5;
            }
            if (chara.gameParameter.individuality.answer.Contains(9)) addRate -= 5;
            if (chara.gameParameter.individuality.answer.Contains(12)) addRate += 10;
            if (chara.gameParameter.individuality.answer.Contains(13)) addRate -= 5;
            if (chara.gameParameter.individuality.answer.Contains(14)) addRate -= 5;
            if (chara.gameParameter.individuality.answer.Contains(19)) addRate += 10;
            if (chara.gameParameter.individuality.answer.Contains(20)) addRate -= 10;
            if (chara.gameParameter.individuality.answer.Contains(34)) addRate += 5;

            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Join 3P rate by traits: {addRate}");

            return addRate;
        }
        public static void SetADVThreesomeOption(ADVManager advManager, Actor actor, Actor actor1, Actor actor2)
        {
            if (CustomGameBalancePlugin.GetCheats()[1])
            {
                if (actor.chaCtrl.sex == 0 && actor1.chaCtrl.sex == 0 && actor2.chaCtrl.sex == 0) return;
                if (actor.chaCtrl.sex == 1 && actor1.chaCtrl.sex == 1 && actor2.chaCtrl.sex == 1)
                {
                    if (actor.chaCtrl.fileParam.isFutanari || actor1.chaCtrl.fileParam.isFutanari || actor2.chaCtrl.fileParam.isFutanari) advManager._packData.Is3P = true;
                    return;
                }
                advManager._packData.Is3P = true;
            }
            else if (CustomGameBalancePlugin.GetPCJoin3P()) advManager._packData.Is3P = CanJoinThreesome(actor, actor1, actor2, true);
        }
    }
}
