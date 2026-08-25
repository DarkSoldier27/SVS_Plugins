using Manager;
using SaveData;
using SV;
using System;
using System.Collections.Generic;

namespace SVS_CustomGameBalance
{
    internal class CGBActionBooster
    {
        private static Random _rnd = new Random();

        public static void SetAction(SVThinking thinking)
        {
            if (thinking.CharaCtrl.IsPC) return;

            var actionBoost = CustomGameBalancePlugin.GetActionBooster();
            //0: Force Sex All
            //1: Boost Ask 3P
            //2: Boost Blackmail
            //3: Boost Home Invites

            //Cheat: Force Public Sex
            if (CustomGameBalancePlugin.GetCheats()[0])
            {
                thinking._charaCtrl.AI.charaData.CommandNo = 35;
                return;
            }
            //Boost: NPC Ask 3P
            if (actionBoost[0])
            {
                ThreesomeNPCAskCondition(thinking);
            }
            //Boost: NPC Blackmail
            //Boost: NPC Home Invites
        }
        public static void ThreesomeNPCAskCondition(SVThinking thinking)
        {
            if (thinking == null) return;
            if (thinking._charaCtrl == null) return;
            if (thinking._charaCtrl.ai == null) return;
            if (thinking._charaCtrl.ai._charaData == null) return;
            if (thinking._charaCtrl.ai._charaData.charasGameParam.commandNo < 0 || thinking._charaCtrl.ai._charaData.charasGameParam.commandNo > 100) return;
            if (thinking._charaCtrl.ai._charaData.charasGameParam.commandNo == 77) return;
            if (CheckIsReactionCommand(thinking._charaCtrl.ai._charaData.CommandNo)) return;
            if (thinking._charaCtrl.ai.charaData.gameParameter.individuality.answer.Contains(29))
            {
                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Character has Singleminded trait, can not ask for 3P");
                return;
            }

            if (thinking._charaCtrl.target.kind == BehaviourController.TargetInfo.TargetKind.Chara)
            {
                if (thinking._charaCtrl.ai._charaData.gameParameter.LvChastity > 2)
                {
                    if (!thinking._charaCtrl.ai._charaData.gameParameter.isVirgin) return;
                }

                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"**********************");
                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Init NPC [{thinking._charaCtrl.ai._charaData.charasGameParam.Index}] asking for 3P");
                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"**********************");

                CustomGameBalancePlugin.RestrictionType restrictionType = CustomGameBalancePlugin.GetActionRestrictionTypes()[0];
                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Restriction type is: {restrictionType.ToString()}");
                switch (restrictionType)
                {
                    case CustomGameBalancePlugin.RestrictionType.Light:
                        break;
                    case CustomGameBalancePlugin.RestrictionType.Medium:
                        if (thinking._charaCtrl.ai.charaData.charasGameParam.state.State != StateParameter.StateKind.RUT &&
                            thinking._charaCtrl.ai.charaData.charasGameParam.state.State != StateParameter.StateKind.NORMAL &&
                            thinking._charaCtrl.ai.charaData.charasGameParam.state.State != StateParameter.StateKind.UPLIFT)
                        {
                            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Chara {thinking._charaCtrl.ai.charaData.charasGameParam.Index} is not in a good mood. 3P Action canceled");
                            return;
                        }
                        break;
                    case CustomGameBalancePlugin.RestrictionType.Heavy:
                        if (thinking._charaCtrl.ai.charaData.charasGameParam.state.State != StateParameter.StateKind.RUT)
                        {
                            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Chara {thinking._charaCtrl.ai.charaData.charasGameParam.Index} is not horny. 3P Action canceled");
                            return;
                        }
                        break;
                } 

                bool askForThreeP = false;
                int rate = CustomGameBalancePlugin.GetNPCJoinAsk3PRate()[1];
                int targetID = thinking._charaCtrl.target.id;
                int commandID = thinking._charaCtrl.AI._charaData.CommandNo;

                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Original commandNo: {commandID}");

                int askRate = 0;

                if (thinking._charaCtrl.ai._charaData.charasGameParam.state.State == StateParameter.StateKind.RUT) askRate += 10;
                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Ask Rate from Mood: {askRate}");

                if (Game.Charas.TryGetValue(targetID, out Actor targetActor))
                {
                    if (thinking._charaCtrl.ai._charaData.gameParameter.individuality.answer.Contains(2) && targetActor.parameter.sex == 0) askRate -= 10;
                    if (thinking._charaCtrl.ai._charaData.gameParameter.individuality.answer.Contains(3) && targetActor.parameter.sex == 1) askRate -= 10;
                }

                if (thinking._charaCtrl.ai._charaData.gameParameter.individuality.answer.Contains(9)) askRate -= 5;
                if (thinking._charaCtrl.ai._charaData.gameParameter.individuality.answer.Contains(10)) askRate -= 5;
                if (thinking._charaCtrl.ai._charaData.gameParameter.individuality.answer.Contains(12)) askRate += 10;
                if (thinking._charaCtrl.ai._charaData.gameParameter.individuality.answer.Contains(13)) askRate -= 5;
                if (thinking._charaCtrl.ai._charaData.gameParameter.individuality.answer.Contains(14)) askRate -= 5;
                if (thinking._charaCtrl.ai._charaData.gameParameter.individuality.answer.Contains(19)) askRate += 10;
                if (thinking._charaCtrl.ai._charaData.gameParameter.individuality.answer.Contains(20)) askRate -= 5;
                if (thinking._charaCtrl.ai._charaData.gameParameter.individuality.answer.Contains(34)) askRate += 5;

                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Ask Rate from traits: {askRate}");

                switch (thinking._charaCtrl.ai._charaData.gameParameter.LvChastity)
                {
                    case 0: askRate += 10; break;
                    case 1: askRate += 5; break;
                    case 3: askRate -= 5; break;
                    case 4: askRate -= 10; break;
                }
                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Ask Rate from Virtue LV: {askRate}");

                int chance = _rnd.Next(0, 100);
                if (rate == 0)
                {
                    switch (commandID)
                    {
                        case 35:
                            askRate += 35;
                            if (chance < askRate) askForThreeP = true;
                            break;
                        case 37:
                            askRate += 15;
                            if (chance < askRate) askForThreeP = true;
                            break;
                    }
                }
                else
                {
                    if (rate < 100) rate += askRate;
                    if (rate > chance) askForThreeP = true;
                }

                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Is NPC asking for 3P: {askForThreeP}");
                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Rate: {rate}/{chance}");

                if (askForThreeP)
                {
                    var sexualTarget = thinking._charaCtrl.ai._charaData.gameParameter.SexualTarget;
                    var sex = thinking._charaCtrl.ai._charaData.parameter.sex;
                    var virtue = thinking._charaCtrl.ai._charaData.gameParameter.LvChastity;
                    var charaSensitivity = thinking._charaCtrl.ai._charaData.charasGameParam.sensitivity;
                    var charaActor = thinking._charaCtrl.ai._charaData;

                    if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"CharaID: {charaActor.charasGameParam.Index}");
                    if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"TargetID: {targetID}");

                    if (Game.Charas.TryGetValue(targetID, out var targetChara))
                    {
                        switch (sexualTarget)
                        {
                            case 0://Hetero
                                int oppositeSex = -1;
                                if (sex == 1) oppositeSex = 0;
                                else oppositeSex = 1;
                                SelectThreesomePartner(charaActor, targetChara, oppositeSex, virtue);
                                break;
                            case 4://Homo
                                if (sex == 0) return;
                                else if (sex == 1 && thinking._charaCtrl.ai._charaData.parameter.isFutanari)
                                {
                                    SelectThreesomePartner(charaActor, targetChara, sex, virtue);
                                }
                                break;
                            default:
                                SelectThreesomePartner(charaActor, targetChara, -1, virtue);
                                break;
                        }
                    }
                }
                else if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Conditions not met, charaID {thinking._charaCtrl.ai._charaData.charasGameParam.Index} won't ask for 3P");
            }
        }
        private static void SelectThreesomePartner(Actor chara, Actor targetActor, int sexType, int virtueLV)
        {
            if (chara.parameter.sex == 0 && targetActor.parameter.sex == 0 && sexType == 0) return;

            Dictionary<int, int> charaWeights = new Dictionary<int, int>();
            var targetID = targetActor.charasGameParam.Index;
            int summedWeights = 0;
            bool isFuta = false;

            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Selecting 3P Partner: sex type {sexType}");
            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Chara 1: {chara.charasGameParam.Index} sex: {chara.parameter.sex} virtue: {virtueLV}");
            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Chara 2: {targetID} sex: {targetActor.parameter.sex}");

            if (sexType == -1)
            {
                if (chara.parameter.sex == 0 && targetActor.parameter.sex == 0) sexType = 1;
                if (chara.parameter.sex == 1 && targetActor.parameter.sex == 1)
                {
                    if (!targetActor.parameter.isFutanari && !chara.parameter.isFutanari) isFuta = true;
                    //sexType = 0;
                }
            }

            //Check for Lovers
            if (chara.charasGameParam.memory.lovers.Count > 0)
            {
                foreach (var lover in chara.charasGameParam.memory.lovers)
                {
                    if (!charaWeights.ContainsKey(lover.id) && lover.id != targetID)
                    {
                        if (Game.Charas.TryGetValue(lover.id, out Actor ThirdCharacter))
                        {
                            if (ThirdCharacter.charaBase != null)
                            {
                                if (chara.parameter.sex == 1 && targetActor.parameter.sex == 1 && ThirdCharacter.parameter.sex == 1) continue;
                                if (chara.parameter.sex == 0 && targetActor.parameter.sex == 0 && ThirdCharacter.parameter.sex == 0) continue;
                                if (!ThirdCharacter.IsPC)
                                {
                                    if (!ThirdCharacter.gameParameter.individuality.answer.Contains(29)) charaWeights.Add(lover.id, 30);
                                }
                            }
                        }
                    }
                }
            }

            //Check if they have the same lover
            if (targetActor.charasGameParam.memory.lovers.Count > 0)
            {
                foreach (var lover in targetActor.charasGameParam.memory.lovers)
                {
                    if (charaWeights.ContainsKey(lover.id))
                    {
                        if (Game.Charas.TryGetValue(lover.id, out Actor ThirdCharacter))
                        {
                            if (ThirdCharacter.charaBase != null)
                            {
                                if (chara.parameter.sex == 1 && targetActor.parameter.sex == 1 && ThirdCharacter.parameter.sex == 1) continue;
                                if (chara.parameter.sex == 0 && targetActor.parameter.sex == 0 && ThirdCharacter.parameter.sex == 0) continue;
                                if (!ThirdCharacter.IsPC)
                                {
                                    if (!ThirdCharacter.gameParameter.individuality.answer.Contains(29)) charaWeights[lover.id] += 20;
                                }
                            }
                        }
                    }
                }
            }

            //Check favorability points
            if (chara.charasGameParam.sensitivity.tableFavorabiliry.Count > 0)
            {
                foreach (var charaFavor in chara.charasGameParam.sensitivity.tableFavorabiliry)
                {
                    if (Game.Charas.TryGetValue(charaFavor.Key, out Actor thatCharacter))
                    {
                        if (thatCharacter.charasGameParam.Index != targetID && !thatCharacter.IsPC && thatCharacter.charaBase != null)
                        {
                            if (MapManager.Instance.MapListTable.ContainsKey(thatCharacter.charaBase.BehaviourCtrl.nowMapID))
                            {
                                if (MapManager.Instance.MapListTable[thatCharacter.charaBase.BehaviourCtrl.nowMapID].Kind == 1) continue;
                            }
                            if (thatCharacter.gameParameter.individuality.answer.Contains(29)) continue;
                            if (thatCharacter.parameter.sex == sexType && sexType != -1)
                            {
                                switch (virtueLV)
                                {
                                    case 2://Normal Virtue/Chastity
                                        if (charaWeights.ContainsKey(charaFavor.Key))
                                        {
                                            if (charaFavor.Value.longSensitivityCounts[0] > 10) charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[0];
                                            if (charaFavor.Value.longSensitivityCounts[1] > 10) charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[1];
                                            charaWeights[charaFavor.Key] -= charaFavor.Value.longSensitivityCounts[3];
                                        }
                                        else
                                        {
                                            charaWeights.Add(charaFavor.Key, 0);
                                            if (charaFavor.Value.longSensitivityCounts[0] > 10) charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[0];
                                            if (charaFavor.Value.longSensitivityCounts[1] > 10) charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[1];
                                            charaWeights[charaFavor.Key] -= charaFavor.Value.longSensitivityCounts[3];
                                        }
                                        break;
                                    case 3://High Virtue/Chastity
                                        if (charaWeights.ContainsKey(charaFavor.Key))
                                        {
                                            if (charaFavor.Value.longSensitivityCounts[0] > 20) charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[0];
                                            charaWeights[charaFavor.Key] -= charaFavor.Value.longSensitivityCounts[3];
                                        }
                                        else
                                        {
                                            charaWeights.Add(charaFavor.Key, 0);
                                            if (charaFavor.Value.longSensitivityCounts[0] > 20) charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[0];
                                            charaWeights[charaFavor.Key] -= charaFavor.Value.longSensitivityCounts[3];
                                        }
                                        break;
                                    case 4://Highest Virtue/Chastity
                                        if (chara.charasGameParam.memory.pairTable.ContainsKey(charaFavor.Key))
                                        {
                                            if (chara.charasGameParam.memory.pairTable[charaFavor.Key].TotalH > 0)
                                            {
                                                if (charaWeights.ContainsKey(charaFavor.Key))
                                                {
                                                    if (charaFavor.Value.longSensitivityCounts[0] > 25) charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[0];
                                                    charaWeights[charaFavor.Key] -= charaFavor.Value.longSensitivityCounts[3];
                                                }
                                                else
                                                {
                                                    charaWeights.Add(charaFavor.Key, 0);
                                                    if (charaFavor.Value.longSensitivityCounts[0] > 25) charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[0];
                                                    charaWeights[charaFavor.Key] -= charaFavor.Value.longSensitivityCounts[3];
                                                }
                                            }
                                        }
                                        break;
                                    default://Low and Lowest Virtue/Chastity
                                        if (charaWeights.ContainsKey(charaFavor.Key))
                                        {
                                            charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[0];
                                            charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[1];
                                            charaWeights[charaFavor.Key] -= charaFavor.Value.longSensitivityCounts[3];
                                        }
                                        else
                                        {
                                            charaWeights.Add(charaFavor.Key, 0);
                                            charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[0];
                                            charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[1];
                                            charaWeights[charaFavor.Key] -= charaFavor.Value.longSensitivityCounts[3];
                                        }
                                        break;
                                }
                            }
                            else if (sexType == -1)
                            {
                                if (isFuta && (thatCharacter.parameter.sex == 1 && !thatCharacter.parameter.isFutanari)) continue;
                                switch (virtueLV)
                                {
                                    case 2://Normal Virtue/Chastity
                                        if (charaWeights.ContainsKey(charaFavor.Key))
                                        {
                                            if (charaFavor.Value.longSensitivityCounts[0] > 10) charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[0];
                                            if (charaFavor.Value.longSensitivityCounts[1] > 10) charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[1];
                                            charaWeights[charaFavor.Key] -= charaFavor.Value.longSensitivityCounts[3];
                                        }
                                        else
                                        {
                                            charaWeights.Add(charaFavor.Key, 0);
                                            if (charaFavor.Value.longSensitivityCounts[0] > 10) charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[0];
                                            if (charaFavor.Value.longSensitivityCounts[1] > 10) charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[1];
                                            charaWeights[charaFavor.Key] -= charaFavor.Value.longSensitivityCounts[3];
                                        }
                                        break;
                                    case 3://High Virtue/Chastity
                                        if (charaWeights.ContainsKey(charaFavor.Key))
                                        {
                                            if (charaFavor.Value.longSensitivityCounts[0] > 20) charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[0];
                                            charaWeights[charaFavor.Key] -= charaFavor.Value.longSensitivityCounts[3];
                                        }
                                        else
                                        {
                                            charaWeights.Add(charaFavor.Key, 0);
                                            if (charaFavor.Value.longSensitivityCounts[0] > 20) charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[0];
                                            charaWeights[charaFavor.Key] -= charaFavor.Value.longSensitivityCounts[3];
                                        }
                                        break;
                                    case 4://Highest Virtue/Chastity
                                        if (chara.charasGameParam.memory.pairTable.ContainsKey(charaFavor.Key))
                                        {
                                            if (chara.charasGameParam.memory.pairTable[charaFavor.Key].TotalH > 0)
                                            {
                                                if (charaWeights.ContainsKey(charaFavor.Key))
                                                {
                                                    if (charaFavor.Value.longSensitivityCounts[0] > 25) charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[0];
                                                    charaWeights[charaFavor.Key] -= charaFavor.Value.longSensitivityCounts[3];
                                                }
                                                else
                                                {
                                                    charaWeights.Add(charaFavor.Key, 0);
                                                    if (charaFavor.Value.longSensitivityCounts[0] > 25) charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[0];
                                                    charaWeights[charaFavor.Key] -= charaFavor.Value.longSensitivityCounts[3];
                                                }
                                            }
                                        }
                                        break;
                                    default://Low and Lowest Virtue/Chastity
                                        if (charaWeights.ContainsKey(charaFavor.Key))
                                        {
                                            charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[0];
                                            charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[1];
                                            charaWeights[charaFavor.Key] -= charaFavor.Value.longSensitivityCounts[3];
                                        }
                                        else
                                        {
                                            charaWeights.Add(charaFavor.Key, 0);
                                            charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[0];
                                            charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[1];
                                            charaWeights[charaFavor.Key] -= charaFavor.Value.longSensitivityCounts[3];
                                        }
                                        break;
                                }
                            }
                            if (charaWeights.ContainsKey(charaFavor.Key))
                            {
                                if (chara.gameParameter.individuality.answer.Contains(2) && thatCharacter.parameter.sex == 0) charaWeights[charaFavor.Key] -= 20;
                                if (chara.gameParameter.individuality.answer.Contains(3) && thatCharacter.parameter.sex == 1) charaWeights[charaFavor.Key] -= 20;
                            }
                        }
                    }
                }
            }

            if (charaWeights.Count > 0)
            {
                foreach (var weights in charaWeights)
                {
                    if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Candidate ID: {weights.Key} - Weight: {weights.Value}");
                    if (weights.Value > 0) summedWeights += weights.Value;
                }
            }

            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Calculating... sums of weights: {summedWeights}");
            if (summedWeights > 0)
            {
                int pickChance = _rnd.Next(0, summedWeights);
                int weight = 0;
                foreach (var weights in charaWeights)
                {
                    if (weights.Value <= 0) continue;                  
                    weight += weights.Value;
                    if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Chances: {weight}/{pickChance}");
                    if (weight > pickChance)
                    {
                        if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"3P Partner selected: {weights.Key}");
                        chara.charasGameParam.commandNo = 77;
                        chara.charasGameParam.thatPersonCharaArrayIndex = weights.Key;
                        if (Game.Charas.TryGetValue(weights.Key, out Actor thatPersonActor))
                        {
                            chara.charasGameParam.thatPersonName = thatPersonActor.Name;
                        }
                        return;
                    }
                }
            }
            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"No partner for 3P found");
        }

        public static void BlackmailRate(SVThinking thinking)
        {
            
        }
        public static void HomeInvitesRate(SVThinking thinking)
        {

        }

        private static bool CheckIsReactionCommand(int commandNo)
        {
            switch (commandNo)
            {
                case 47: return true;
                case 48: return true;
                case 49: return true;
                case 50: return true;
                case 51: return true;
                case 52: return true;
            }
            return false;
        }
    }
}
