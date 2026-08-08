using Manager;
using SaveData;
using SV;
using SVS_Detour;
using System;
using System.Collections.Generic;

namespace SVS_CustomGameBalance
{
    internal class CGBNightEvent
    {
        private static Random _rnd = new Random();
        private static Dictionary<int, int[]> _nightVisitCandidates = new();

        public static void GetNightEventCharacters()
        {
            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"**************************");
            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Night Event Initialization");
            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"**************************");

            _nightVisitCandidates.Clear();

            Actor charaPC = GameChara.Player;
            if (charaPC is null ) return;

            int charaID = charaPC.charasGameParam.Index;

            foreach (var charaNPC in Game.Charas)
            {
                if (charaNPC.Value.charaBase is null) continue;
                if (charaNPC.Value.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(charaID))
                {
                    int lovePoints = charaNPC.Value.charasGameParam.sensitivity.tableFavorabiliry[charaID].longSensitivityCounts[0];
                    int friendPoints = charaNPC.Value.charasGameParam.sensitivity.tableFavorabiliry[charaID].longSensitivityCounts[1];
                    int distantPoints = charaNPC.Value.charasGameParam.sensitivity.tableFavorabiliry[charaID].longSensitivityCounts[2];
                    int hatePoints = charaNPC.Value.charasGameParam.sensitivity.tableFavorabiliry[charaID].longSensitivityCounts[3];
                    
                    if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Checking Game Initial restriction for chara: {charaNPC.Value.charasGameParam.Index}");

                    if (lovePoints > 10 || friendPoints > 10)//Vanilla game check
                    {
                        if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Passed!");

                        int sexRate = 0;
                        int playRate = 0;

                        bool sexVisit = false;
                        //bool playVisit = false;

                        int virtueLv = charaNPC.Value.gameParameter.LvChastity;
                        if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Chara Virtue: {virtueLv}");
                        switch (virtueLv)
                        {
                            case 0://Lowest
                                if (SexVisitCondition_Orientation(charaNPC.Value,charaPC))
                                {
                                    if (SexVisitCondition_Trait(charaNPC.Value, charaPC))
                                    {
                                        sexVisit = true;
                                    }
                                }
                                break;
                            case 1://Low
                                if (SexVisitCondition_Orientation(charaNPC.Value, charaPC))
                                {
                                    if (SexVisitCondition_Trait(charaNPC.Value, charaPC))
                                    {
                                        sexVisit = true;
                                    }
                                }
                                break;
                            case 2://Normal
                                if (SexVisitCondition_Orientation(charaNPC.Value, charaPC))
                                {
                                    if (SexVisitCondition_Trait(charaNPC.Value, charaPC))
                                    {
                                        sexVisit = true;
                                    }
                                }
                                break;
                            case 3://High
                                if (SexVisitCondition_Orientation(charaNPC.Value, charaPC))
                                {
                                    if (SexVisitCondition_Trait(charaNPC.Value, charaPC))
                                    {
                                        if (lovePoints > 20 && !charaNPC.Value.gameParameter.isVirgin) sexVisit = true;
                                        else if (lovePoints > 15 && !charaNPC.Value.gameParameter.isVirgin)
                                        {
                                            if (charaNPC.Value.charasGameParam.memory.pairTable.ContainsKey(charaID))
                                            {
                                                if (charaNPC.Value.charasGameParam.memory.pairTable[charaID].TotalH > 0) sexVisit = true;
                                            }
                                        }
                                    }
                                }
                                break;
                            case 4://Highest
                                if (SexVisitCondition_Orientation(charaNPC.Value, charaPC))
                                {
                                    if (SexVisitCondition_Trait(charaNPC.Value, charaPC))
                                    {
                                        if (lovePoints > 20 && charaNPC.Value.gameParameter.isVirgin)
                                        {
                                            if (charaNPC.Value.charasGameParam.memory.pairTable.ContainsKey(charaID))
                                            {
                                                if (charaNPC.Value.charasGameParam.memory.pairTable[charaID].TotalH > 0) sexVisit = true;
                                            }                                           
                                        } 
                                    }
                                }
                                break;
                        }

                        if (!_nightVisitCandidates.ContainsKey(charaNPC.Key)) _nightVisitCandidates.Add(charaNPC.Key, [0, 0]);

                        if (friendPoints > 10)
                        {
                            playRate = FriendlyVisitRate(charaNPC.Value, charaPC);
                            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Play rate: {playRate}");                            
                            if (_nightVisitCandidates.ContainsKey(charaNPC.Key)) _nightVisitCandidates[charaNPC.Key][0] = playRate;
                        }

                        if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Can visit for H?: {sexVisit}");
                        if (sexVisit)
                        {
                            sexRate = SexVisitRate(charaNPC.Value, charaPC);
                            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Sex rate: {sexRate}");
                            if (_nightVisitCandidates.ContainsKey(charaNPC.Key)) _nightVisitCandidates[charaNPC.Key][1] = sexRate;
                        }
                    }
                    else if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Failed!");
                }
            }
            NightEventChance();
        }

        public static void NightEventChance()
        {
            var nightVisitChance = CustomGameBalancePlugin.GetNightChance();

            int visitTypeTotalWeight = 0;
            int charaVisitTotalWeight = 0;
            int visitType = 0;

            if (_nightVisitCandidates.Count > 0)
            {
                foreach (var candidate in _nightVisitCandidates)
                {
                    charaVisitTotalWeight += candidate.Value[0] + candidate.Value[1];
                }
            }

            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Candidates for night visit: {_nightVisitCandidates.Count} - Total weight: {charaVisitTotalWeight}");

            int sumOfWeights = 0;
            int charaVisitChance = _rnd.Next(0, charaVisitTotalWeight);           
            foreach (var candidate in _nightVisitCandidates)
            {
                sumOfWeights += candidate.Value[0] + candidate.Value[1];
                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Checking candidate ID:{candidate.Key} | chances: {sumOfWeights}/{charaVisitChance}");
                if (sumOfWeights > charaVisitChance)
                {
                    visitTypeTotalWeight = candidate.Value[0] + candidate.Value[1];
                    var visitTypeChance = _rnd.Next(0, visitTypeTotalWeight);
                    if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Checking Visit Type: Play visit {candidate.Value[0]} - Sex visit {candidate.Value[1]}");
                    if (candidate.Value[0] > visitTypeChance) visitType = 1;
                    else visitType = 2;

                    int triggerNightEvent = _rnd.Next(0, 100);
                    switch (visitType)
                    {
                        case 1:
                            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Night Event Play chance: {nightVisitChance[0]}/{triggerNightEvent}");
                            if (nightVisitChance[0] < triggerNightEvent)
                            {
                                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Night Event Play Failed!");
                                NightEvent.SetNightEventCharacter(-1, 0, 0);
                                return;
                            }
                            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Night Play Event Success! - character visiting: {candidate.Key}");
                            NightEvent.SetNightEventCharacter(candidate.Key, visitType, 5);
                            return;
                        case 2:
                            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Night Event Sex chance: {nightVisitChance[1]}/{triggerNightEvent}");
                            if (nightVisitChance[1] < triggerNightEvent)
                            {
                                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Night Event Sex Failed!");
                                NightEvent.SetNightEventCharacter(-1, 0, 0);
                                return;
                            }
                            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Night Sex Event Success! - character visiting: {candidate.Key}");
                            NightEvent.SetNightEventCharacter(candidate.Key, visitType, 5);
                            return;
                    }
                    break;
                }
            }           
            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"No suitable character for Night Event. Event won't trigger");
        }
        private static bool SexVisitCondition_Orientation(Actor visitor, Actor hostChara)
        {
            int sexualTarget = visitor.gameParameter.sexualTarget;
            switch (sexualTarget)
            {
                case 0://Hetero
                    if ((visitor.parameter.sex == 0 && hostChara.parameter.sex == 0) ||
                        (visitor.parameter.sex == 1 && hostChara.parameter.sex == 1)) { return false; }
                    break;
                case 4://Homo
                    if ((visitor.parameter.sex == 0 && hostChara.parameter.sex == 1) ||
                        (visitor.parameter.sex == 1 && hostChara.parameter.sex == 0)) { return false; }
                    break;
            }
            return true;
        }

        private static bool SexVisitCondition_Trait(Actor visitorChara, Actor hostChara)
        {
            if (visitorChara.gameParameter.individuality.answer.Contains(29))
            {
                if (visitorChara.charasGameParam.memory.lovers.Count > 0)
                {
                    foreach (var lover in visitorChara.charasGameParam.memory.lovers)
                    {
                        if (lover.id == hostChara.charasGameParam.Index) return true;
                    }
                    return false;
                }
                else return false;
            }

            return true;
        }
        private static int SexVisitRate(Actor visitorChara, Actor hostChara)
        {
            int sexVisitRate = 0;

            int charaID = hostChara.charasGameParam.Index;

            int lovePoints = visitorChara.charasGameParam.sensitivity.tableFavorabiliry[charaID].longSensitivityCounts[0];
            int friendPoints = visitorChara.charasGameParam.sensitivity.tableFavorabiliry[charaID].longSensitivityCounts[1];
            int distantPoints = visitorChara.charasGameParam.sensitivity.tableFavorabiliry[charaID].longSensitivityCounts[2];
            int hatePoints = visitorChara.charasGameParam.sensitivity.tableFavorabiliry[charaID].longSensitivityCounts[3];

            //Virtue rate
            int chastity = visitorChara.gameParameter.LvChastity;
            switch (chastity)
            {
                case 0:
                    sexVisitRate += (lovePoints + friendPoints) - hatePoints;
                    break;
                case 1:
                    sexVisitRate += (lovePoints + (friendPoints / 2)) - (distantPoints + hatePoints);
                    break;
                case 2:
                    sexVisitRate += lovePoints - (distantPoints + hatePoints);
                    break;
                case 3:
                    sexVisitRate += lovePoints - (distantPoints + hatePoints);
                    break;
                case 4:
                    sexVisitRate += lovePoints - (distantPoints + hatePoints);
                    break;
            }
            //Socialbility rate
            sexVisitRate += visitorChara.charasGameParam.baseParameter.ConversationLV;

            //Trait rates
            switch (hostChara.parameter.sex)
            {
                case 0:
                    if (visitorChara.gameParameter.individuality.answer.Contains(2)) sexVisitRate -= 10;
                    break;
                case 1:
                    if (visitorChara.gameParameter.individuality.answer.Contains(3)) sexVisitRate -= 10;
                    break;
            }

            //Shy
            if (visitorChara.gameParameter.individuality.answer.Contains(9)) sexVisitRate -= 10;
            //Pervert
            if (visitorChara.gameParameter.individuality.answer.Contains(12)) sexVisitRate += 10;
            //Romantic
            if (visitorChara.gameParameter.individuality.answer.Contains(27)) sexVisitRate += 5;
            //Evil
            if (visitorChara.gameParameter.individuality.answer.Contains(36)) sexVisitRate += 15;
            //Carnivorous
            if (visitorChara.gameParameter.individuality.answer.Contains(19) && sexVisitRate > 0) sexVisitRate = (int)(sexVisitRate * 1.5f);
            //Herbivorous
            if (visitorChara.gameParameter.individuality.answer.Contains(20) && sexVisitRate > 0) sexVisitRate = (int)(sexVisitRate * 0.5f);

            if (hostChara.charasGameParam.memory.lovers.Count > 0)
            {
                foreach (var lover in hostChara.charasGameParam.memory.lovers)
                {
                    if (lover.id == hostChara.charasGameParam.Index) sexVisitRate += 15; 
                }
            }

            return sexVisitRate;
        }
        private static int FriendlyVisitRate(Actor visitorChara, Actor hostChara)
        {
            int friendVisitRate = 0;

            int charaID = hostChara.charasGameParam.Index;

            int lovePoints = visitorChara.charasGameParam.sensitivity.tableFavorabiliry[charaID].longSensitivityCounts[0];
            int friendPoints = visitorChara.charasGameParam.sensitivity.tableFavorabiliry[charaID].longSensitivityCounts[1];
            int distantPoints = visitorChara.charasGameParam.sensitivity.tableFavorabiliry[charaID].longSensitivityCounts[2];
            int hatePoints = visitorChara.charasGameParam.sensitivity.tableFavorabiliry[charaID].longSensitivityCounts[3];

            friendVisitRate = friendPoints - (distantPoints + hatePoints);

            friendVisitRate += visitorChara.charasGameParam.baseParameter.ConversationLV;

            //Trait rates
            switch (hostChara.parameter.sex)
            {
                case 0:
                    //Bad with Boys
                    if (visitorChara.gameParameter.individuality.answer.Contains(2)) friendVisitRate -= 10;
                    break;
                case 1:
                    //Bad with Girls
                    if (visitorChara.gameParameter.individuality.answer.Contains(3)) friendVisitRate -= 10;
                    break;
            }

            //Affable
            if (visitorChara.gameParameter.individuality.answer.Contains(1)) friendVisitRate += 10;
            //Trendy
            if (visitorChara.gameParameter.individuality.answer.Contains(6)) friendVisitRate += 10;
            //Shy
            if (visitorChara.gameParameter.individuality.answer.Contains(9)) friendVisitRate -= 10;
            //Chatty
            if (visitorChara.gameParameter.individuality.answer.Contains(24)) friendVisitRate += 10;

            return friendVisitRate;
        }
    }
}
