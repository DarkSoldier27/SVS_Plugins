using ADV;
using ADV.Commands.Game.LowChara;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using Manager;
using SaveData;
using SV;
using SV.Chara;
using SV.MyRoomScene;
using System;
using UnityEngine;

namespace SVS_CustomGameBalance
{
    internal static class CustomGameBalance
    {
        private static readonly System.Random _rnd = new();
        private static readonly Dictionary<int, GlobalListLoad.MoveAnimationPitch> npcSpeedDic = new();

        public static class OffScreenInteractions
        {
            private static AI _npc;
            private static AI _npc2;
            private static AI _npc3;
            private static int tempMapID_1;
            private static int tempMapID_2;
            private static int tempMapID_3;

            public static void PreNPCSkip(LowPolyText lowPolyText)
            {
                if (CustomGameBalancePlugin.GetNoSkipNPCOffScreenInteractions())
                {
                    lowPolyText.LowPolyChara.TryGet(0, out Actor _actor);
                    lowPolyText.LowPolyChara.TryGet(1, out Actor _actor2);
                    lowPolyText.LowPolyChara.TryGet(2, out Actor _actor3);

                    if (_actor != null)
                    {
                        _npc = GameChara.FindCharaAI(_actor._chaCtrl_k__BackingField);
                        if (_npc != null)
                        {
                            if (MapManager.Instance._mapID == _npc.BehaviourCtrl.nowMapID)
                            {
                                tempMapID_1 = -1;
                                tempMapID_2 = -1;
                                tempMapID_3 = -1;
                                return;
                            }

                            tempMapID_1 = _npc.BehaviourCtrl.nowMapID;
                            _npc.BehaviourCtrl.nowMapID = MapManager.Instance._mapID;
                        }
                    }
                    else
                    {
                        _npc = null;
                        tempMapID_1 = -1;
                    }

                    if (_actor2 != null)
                    {
                        _npc2 = GameChara.FindCharaAI(_actor2._chaCtrl_k__BackingField);
                        if (_npc2 != null)
                        {
                            tempMapID_2 = _npc2.BehaviourCtrl.nowMapID;
                            _npc2.BehaviourCtrl.nowMapID = MapManager.Instance._mapID;
                        }
                    }
                    else
                    {
                        _npc2 = null;
                        tempMapID_2 = -1;
                    }

                    if (_actor3 != null)
                    {
                        _npc3 = GameChara.FindCharaAI(_actor3._chaCtrl_k__BackingField);
                        if (_npc3 != null)
                        {
                            tempMapID_3 = _npc3.BehaviourCtrl.nowMapID;
                            _npc3.BehaviourCtrl.nowMapID = MapManager.Instance._mapID;
                        }
                    }
                    else
                    {
                        _npc3 = null;
                        tempMapID_3 = -1;
                    }
                }
            }
            public static bool DisplayFukidashi()
            {
                if (CustomGameBalancePlugin.GetNoSkipNPCOffScreenInteractions())
                {
                    if (tempMapID_1 > -1) return false;
                }
                return true;
            }
            public static void PostNPCSkip(LowPolyText lowPolyText)
            {
                if (CustomGameBalancePlugin.GetNoSkipNPCOffScreenInteractions())
                {
                    if (tempMapID_1 > -1 && _npc != null) _npc.BehaviourCtrl.nowMapID = tempMapID_1;
                    if (tempMapID_2 > -1 && _npc2 != null) _npc2.BehaviourCtrl.nowMapID = tempMapID_2;
                    if (tempMapID_3 > -1 && _npc3 != null) _npc3.BehaviourCtrl.nowMapID = tempMapID_3;
                }
            }
        }

        public static void InitCGB()
        {
            //NPC movement speed, get original speed table.
            if (npcSpeedDic.Count == 0)
            {
                foreach (var npc in GlobalListLoad.Instance.moveAnimSpeedNPCTable)
                {
                    var move = new GlobalListLoad.MoveAnimationPitch();
                    move.runRate = npc.value.runRate;
                    move.speed_run = npc.Value.speed_run;
                    move.speed_walk = npc.Value.speed_walk;
                    npcSpeedDic.Add(npc.Key, move);
                }
            }
        }
        public static void SimUpdate()
        {
            if (Input.GetKeyDown(CustomGameBalancePlugin.GetToggleKeys()[1]))
            {
                CGBCharaPCController.SetAutoPC();
                return;
            }

            if (Input.GetKeyDown(CustomGameBalancePlugin.GetToggleKeys()[2]))
            {
                if (GameChara.PlayerAI != null)
                {
                    if (GameChara.PlayerAI.BehaviourCtrl.isAuto) LowpolyActionVoiceManager.Instance.LowpolyVoicePlay(33, GameChara.PlayerAI);
                }
                return;
            }

            if (Input.GetKeyDown(CustomGameBalancePlugin.GetToggleKeys()[0]))
            {
                CGBCharaPCController.SwitchPCCharacter();
                return;
            }

            if (CustomGameBalancePlugin.GetPCFollowProtection())
            {
                if (GameChara.Player != null)
                {
                    if (GameChara.Player.charasGameParam.isChase && !GameChara.Player.charasGameParam.isWithAction)
                    {
                        var isNotFree = ADVManager._instance.IsHScene || ADVManager._instance.IsADV
                                    || (MyRoom._instance != null && MyRoom._instance.IsOpen());
                        if (!isNotFree) GameChara.Player.charasGameParam.isWithAction = true;
                    }
                }
            }
        }
        public static void SetCharaWalkAndRunSpeed(bool isPC)
        {
            if (isPC)
            {
                var walkSpeed = GlobalListLoad.Instance.moveAnimSpeedPCTable[0].speed_walk;
                var runSpeed = GlobalListLoad.Instance.moveAnimSpeedPCTable[0].speed_run;

                var pcWalkSpeed = CustomGameBalancePlugin.GetCharaWalkRunSpeed()[0];
                var pcRunSpeed = CustomGameBalancePlugin.GetCharaWalkRunSpeed()[1];

                if (walkSpeed != pcWalkSpeed) GlobalListLoad.Instance.moveAnimSpeedPCTable[0].speed_walk = (float)Math.Round(pcWalkSpeed, 2);
                if (runSpeed != pcRunSpeed) GlobalListLoad.Instance.moveAnimSpeedPCTable[0].speed_run = (float)Math.Round(pcRunSpeed, 2);
            }
            else
            {
                var npcWalkSpeed = CustomGameBalancePlugin.GetCharaWalkRunSpeed()[2];
                var npcRunSpeed = CustomGameBalancePlugin.GetCharaWalkRunSpeed()[3];

                foreach (var npcspeed in npcSpeedDic)
                {
                    GlobalListLoad.Instance.moveAnimSpeedNPCTable[npcspeed.Key].speed_walk = (float)Math.Round(npcspeed.Value.speed_walk * npcWalkSpeed, 2);
                    GlobalListLoad.Instance.moveAnimSpeedNPCTable[npcspeed.Key].speed_run = (float)Math.Round(npcspeed.Value.speed_run * npcRunSpeed, 2);
                }
            }
        }
        /// <summary>
        /// It will reduce the stats of the character each day by a specific amount
        /// <param name="chara"></param>
        /// </summary>
        public static void CharaStatsReductionPerDay(Actor chara)
        {
            if (chara is null) return;
            if (!CustomGameBalancePlugin.GetStatsReductionSettings()[0]) return;
            if (!CustomGameBalancePlugin.GetStatsReductionSettings()[1] && chara.IsPC) return;

            //Base rate
            int baseRate = (int)Math.Round(10f / CustomGameBalancePlugin.GetStatsReductionRate());

            //Get the stats from the character and proficiency from the character stats level
            int stamina = chara.charasGameParam.baseParameter.Stamina;
            int staminaProf = chara.gameParameter.lvPhysical * 2;

            int conversation = chara.charasGameParam.baseParameter.Conversation;
            int conversatioProf = chara.gameParameter.LvTalk * 2;

            int study = chara.charasGameParam.baseParameter.Study;
            int studyProf = chara.gameParameter.LvStudy * 2;

            int living = chara.charasGameParam.baseParameter.Living;
            int livingProf = chara.gameParameter.lvLiving * 2;

            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Chara [{chara.charasGameParam.Index}] Stats: Sta {stamina}, Con {conversation}, Stu {study}, Liv {living}");

            //Apply proficiency base on traits
            if (chara.gameParameter.individuality.answer.Contains(23)) staminaProf += 2;
            if (chara.gameParameter.individuality.answer.Contains(24)) conversatioProf += 2;
            if (chara.gameParameter.individuality.answer.Contains(26)) studyProf += 2;
            if (chara.gameParameter.individuality.answer.Contains(25)) livingProf += 2;

            //The math of how many points the character will lose each day.
            int reduceSta = (stamina / (baseRate + staminaProf));
            int reduceCon = conversation / (baseRate + conversatioProf);
            int reduceStu = study / (baseRate + studyProf);
            int reduceLiv = living / (baseRate + livingProf);

            //Basic less than 0 check.
            if (reduceSta < 0) reduceSta = 0;
            if (reduceCon < 0) reduceSta = 0;
            if (reduceStu < 0) reduceSta = 0;
            if (reduceLiv < 0) reduceSta = 0;

            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Stats Reduce by: Sta -{reduceSta}, Con -{reduceCon}, Stu -{reduceStu}, Liv -{reduceLiv}");

            //Apply the reduction of stats.
            chara.charasGameParam.baseParameter.Stamina -= reduceSta;
            chara.charasGameParam.baseParameter.Conversation -= reduceCon;
            chara.charasGameParam.baseParameter.Study -= reduceStu;
            chara.charasGameParam.baseParameter.Living -= reduceLiv;

            //Less than 0 check.
            if (chara.charasGameParam.baseParameter.Stamina < 0) chara.charasGameParam.baseParameter.Stamina = 0;
            if (chara.charasGameParam.baseParameter.Conversation < 0) chara.charasGameParam.baseParameter.Conversation = 0;
            if (chara.charasGameParam.baseParameter.Study < 0) chara.charasGameParam.baseParameter.Study = 0;
            if (chara.charasGameParam.baseParameter.Living < 0) chara.charasGameParam.baseParameter.Living = 0;

            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"New Chara [{chara.charasGameParam.Index}] " +
                $"Stats: Sta {chara.charasGameParam.baseParameter.Stamina}, " +
                $"Con {chara.charasGameParam.baseParameter.Conversation}, " +
                $"Stu {chara.charasGameParam.baseParameter.Study}, " +
                $"Liv {chara.charasGameParam.baseParameter.Living}");
        }
        public static void CheatingStatusDebuff(Actor chara, int targetID, bool isEndOfDay)
        {
            if (!CustomGameBalancePlugin.GetCheaterManagerOptions()[0]) return;
            if (chara is null) return;

            List<int> charaCheaterIDs = new();
            List<int> charaStealerIDs = new();

            //Check for cheaters.
            if (chara.charasGameParam.memory.logInfoTables.ContainsKey(74))
            {
                if (chara.charasGameParam.memory.logInfoTables[74].Count > 0)
                {
                    foreach (var cheaterCharas in chara.charasGameParam.memory.logInfoTables[74])
                    {
                        foreach (var cheater in cheaterCharas.charas)
                        {
                            if (CustomGameBalancePlugin.GetCheaterManagerOptions()[1])
                            {
                                if (GameChara.Player != null)
                                {
                                    if (GameChara.Player.charasGameParam.Index == cheater && GameChara.Player.IsPC)
                                    {
                                        if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"CSM ignore PC is ON! - Chara PC won't get cheater points");
                                        continue;
                                    }
                                }                        
                            }
                            charaCheaterIDs.Add(cheater);
                        }                    
                    }
                }
            }
            //Check for stealers
            if (chara.charasGameParam.memory.logInfoTables.ContainsKey(75))
            {
                if (chara.charasGameParam.memory.logInfoTables[75].Count > 0)
                {
                    foreach (var cheaterCharas in chara.charasGameParam.memory.logInfoTables[75])
                    {
                        foreach (var stealer in cheaterCharas.charas)
                        {
                            if (CustomGameBalancePlugin.GetCheaterManagerOptions()[1])
                            {
                                if (GameChara.Player != null)
                                {
                                    if (GameChara.Player.charasGameParam.Index == stealer)
                                    {
                                        if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"CSM ignore PC is ON! - Chara PC won't get stealer points");
                                        continue;
                                    }
                                }
                            }
                            charaCheaterIDs.Add(stealer);
                        }
                    }
                }
            }

            if (isEndOfDay)
            {
                if (charaCheaterIDs.Count > 0)
                {
                    if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Cheater Status Manager adding hate toward cheaters");

                    foreach (var cheaterID in charaCheaterIDs)
                    {
                        SetCheaterFavors(chara, cheaterID, 0);
                    }                  
                }
                if (charaStealerIDs.Count > 0)
                {
                    if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Cheater Status Manager adding hate toward stealers");

                    foreach (var stealerID in charaStealerIDs)
                    {
                        SetCheaterFavors(chara, stealerID, 1);
                    }
                }
                return;
            }
            else if (targetID > -1)
            {
                if (charaCheaterIDs.Contains(targetID)) SetCheaterFavors(chara, targetID, 0);
                if (charaStealerIDs.Contains(targetID)) SetCheaterFavors(chara, targetID, 1);
            }
        }
        private static void SetCheaterFavors(Actor chara, int targetID, int cheaterType)
        {
            Il2CppStructArray<int> favors = new(4);
            for (int i = 0; i < favors.Length; i++)
            {
                favors[i] = 0;//Initialize values at 0 just in case.
            }

            //Get the base amount of hate points.
            int baseHate = CustomGameBalancePlugin.GetCheaterManagerValues();

            //Jealous
            if (chara.gameParameter.individuality.answer.Contains(10) && cheaterType == 0) baseHate += 10;
            if (chara.gameParameter.individuality.answer.Contains(10) && cheaterType == 1) baseHate += 30;
            //Melancholic 
            if (chara.gameParameter.individuality.answer.Contains(11)) baseHate += 5;
            //Serious
            if (chara.gameParameter.individuality.answer.Contains(13)) baseHate += 15;
            //Hot-Headed
            if (chara.gameParameter.individuality.answer.Contains(18)) baseHate += 5;
            //Singleminded
            if (chara.gameParameter.individuality.answer.Contains(27)) baseHate += 60;
            //Romantic
            if (chara.gameParameter.individuality.answer.Contains(29)) baseHate += 5;

            //Evil
            if (chara.gameParameter.individuality.answer.Contains(36)) baseHate *= 2;

            //Obedient
            if (chara.gameParameter.individuality.answer.Contains(7)) baseHate -= 15;
            // Indecisive
            if (chara.gameParameter.individuality.answer.Contains(30)) baseHate -= 5;

            // Masochist
            if (chara.gameParameter.individuality.answer.Contains(35))
            {
                int charaSex = chara.parameter.sex;
                int charaSexualTarget = chara.gameParameter.sexualTarget;

                int baseLove = 0;
                if (baseHate > 0) baseLove = baseHate / 2;
                if (Game.Charas.ContainsKey(targetID))
                {
                    Actor targetChara = Game.Charas[targetID];
                    switch (charaSexualTarget)
                    {
                        case 0://Hetero
                            if (charaSex != targetChara.parameter.sex) baseLove += 20;
                            break;
                        case 1:
                            if (charaSex != targetChara.parameter.sex) baseLove += 15;
                            if (charaSex == targetChara.parameter.sex) baseLove += 10;
                            break;
                        case 2://Bi
                            if (charaSex != targetChara.parameter.sex) baseLove += 15;
                            if (charaSex == targetChara.parameter.sex) baseLove += 15;
                            break;
                        case 3:
                            if (charaSex != targetChara.parameter.sex) baseLove += 10;
                            if (charaSex == targetChara.parameter.sex) baseLove += 15;
                            break;
                        case 4://Homo
                            if (charaSex == targetChara.parameter.sex) baseLove += 20;
                            break;
                    }
                }
                if (baseLove > 0) favors[0] = baseLove;
            }

            if (baseHate < 0) baseHate = 0;
            favors[3] = baseHate;
            if (favors[0] > 0 || favors[3] > 0) chara.charasGameParam.sensitivity.AddFavor(chara.charasGameParam.memory, targetID, favors);
        }

        public static void LowPolySexDuration(LowPolyHMotionPlay lowPolyHMotionPlay)
        {
            if ((CustomGameBalancePlugin.GetNPCSex() & CustomGameBalancePlugin.NPCSex.Duration) != 0)
            {
                if (lowPolyHMotionPlay.Args.Count > 2)
                {
                    lowPolyHMotionPlay.Args[1] = CustomGameBalancePlugin.GetNPCSexDuration()[0];
                    lowPolyHMotionPlay.Args[2] = CustomGameBalancePlugin.GetNPCSexDuration()[1];
                }
            }
        }
        public static void NewAnswerRate(YesNoJudgeManager.AnswerInfo _oldAnswerInfo, YesNoJudgeManager.YesNoInfo yesNoInfo, int _commandID, int _questionCount)
        {
            if (yesNoInfo.active is null || yesNoInfo.passive is null) return;
            var isGameFixes = CustomGameBalancePlugin.GetGameFixes();
            var isNewLowestRate = CustomGameBalancePlugin.GetActionLowestRateEnable();
            //var isForceActions = CustomGameBalancePlugin.GetForceActions();

            if (isGameFixes[0]) CGBFixes.FixOnAnswerRate(_oldAnswerInfo, yesNoInfo, _commandID, _questionCount);
            if (isNewLowestRate[0])
            {
                if (isNewLowestRate[1] && isNewLowestRate[2])
                {
                    var _newLowestRate = CustomGameBalancePlugin.GetNewRate();
                    if (_oldAnswerInfo.rate < _newLowestRate) _oldAnswerInfo.rate = _newLowestRate;
                    if (_oldAnswerInfo.ans == 1 && _oldAnswerInfo.rate > 0)
                    {
                        var chance = _rnd.Next(1, 100);
                        if (chance <= _newLowestRate) _oldAnswerInfo.ans = 0;
                    }
                }
                if (yesNoInfo.aParam.isPC && isNewLowestRate[1])
                {
                    var _newLowestRate = CustomGameBalancePlugin.GetNewRate();
                    if (_oldAnswerInfo.rate < _newLowestRate) _oldAnswerInfo.rate = _newLowestRate;
                    if (_oldAnswerInfo.ans == 1 && _oldAnswerInfo.rate > 0)
                    {
                        var chance = _rnd.Next(1, 100);
                        if (chance <= _newLowestRate) _oldAnswerInfo.ans = 0;
                        else _oldAnswerInfo.ans = 1;
                    }
                }
                else
                {
                    if (isNewLowestRate[2])
                    {
                        var _newLowestRate = CustomGameBalancePlugin.GetNewRate();
                        if (_oldAnswerInfo.rate < _newLowestRate) _oldAnswerInfo.rate = _newLowestRate;
                        if (_oldAnswerInfo.ans == 1 && _oldAnswerInfo.rate > 0)
                        {
                            var chance = _rnd.Next(1, 100);
                            if (chance <= _newLowestRate) _oldAnswerInfo.ans = 0;
                        }
                    }
                }
            }          
        }   
        public static int NewSuccessValue(int _successNo)
        {
            if (CustomGameBalancePlugin.GetFortuneFix())
            {
                if (Game.saveData.dataCount.circularNotice == 7 && Game.saveData.dataCount.isCircularNoticeUse)
                {
                    int chance = _rnd.Next(1, 100);
                    switch (_successNo)
                    {
                        case 1:
                            if (chance > 50) return 0;
                            break;
                        case 2:
                            if (chance > 90) return 0;
                            else if (chance > 50) return 1;
                            break;
                        case 3:
                            if (chance > 90) return 1;
                            else if (chance > 50) return 2;
                            break;
                        case 4:
                            if (chance > 90) return 2;
                            else if (chance > 50) return 3;
                            break;
                    }
                }
            }
            return _successNo;
        }
        public static bool GetIsSkinshipAction(AI charaAI, AI targetAI,bool isSkinship)
        {
            if (!CustomGameBalancePlugin.GetCheaterManagerOptions()[2]) return isSkinship;
            if (charaAI is null || targetAI is null) return isSkinship;
            if (charaAI.charaData is null || targetAI.charaData is null) return isSkinship;
            
            if (targetAI.charaData.CommandNo == 32 || targetAI.charaData.CommandNo == 54)
            {
                return false;
            }
            return isSkinship;
        }
        public static int NewCommandTarget(Actor actor, int command, int targetCharaID)
        {
            if (command < 0) return command;

            switch (command)
            {
                case 0://DailyLifeTalk
                    break;

                case 1://LoveTalk
                    break;

                case 2://
                    break;   
                    
                case 3://
                    break;
                    
                case 4://
                    break;
                    
                case 5://
                    break;
                    
                case 6://
                    break;
                    
                case 7://
                    break;
                    
                case 8://
                    break;
                    
                case 9://
                    break;
                    
                case 10://
                    break;
                    
                case 11://
                    break;
                    
                case 12://
                    break;
                    
                case 13://
                    break;
                    
                case 14://
                    break;
                    
                case 15://
                    break;
                    
                case 16://
                    break;
                    
                case 17://
                    break;
                    
                case 18://
                    break;
                    
                case 19://
                    break;
                    
                case 20://
                    break;
                    
                case 21://
                    break;
                    
                case 22://
                    break;
                    
                case 23://
                    break;
                    
                case 24://
                    break;
                    
                case 25://
                    break;
                    
                case 26://
                    break;
                    
                case 27://
                    break;
                    
                case 28://AppointmentForAParty
                    if (!actor.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(targetCharaID)) return targetCharaID;
                    
                    var bbqFix = CustomGameBalancePlugin.GetHighVirtueBBQ();
                    if (bbqFix == CustomGameBalancePlugin.HighVirtueBBQ.DoNotAsk)
                    {
                        if (actor.gameParameter.LvChastity > 2)
                        {
                            if (actor.charasGameParam.sensitivity.tableFavorabiliry[targetCharaID].ranks[0] == SensitivityParameter.Rank.HIGH || actor.charasGameParam.sensitivity.tableFavorabiliry[targetCharaID].ranks[0] == SensitivityParameter.Rank.MAX) return targetCharaID;
                            return -1;
                        }
                    }                     
                    break;
                    
                case 29://OfferToTakePartInARelationship
                    if (CustomGameBalancePlugin.GetNormalVirtueBalance())
                    {
                        if (actor.gameParameter.LvChastity == 2)
                        {
                            if (actor.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(targetCharaID))
                            {
                                if (actor.charasGameParam.sensitivity.tableFavorabiliry[targetCharaID].longSensitivityCounts[0] < 11) return -1;
                            }
                        }
                    }
                    break;
                    
                case 30://BrokeItOff
                    break;
                    
                case 31://Caressing
                    break;
                    
                case 32://HugSomeoneClose
                    break;

                case 33://Kiss
                    if (CustomGameBalancePlugin.GetNormalVirtueBalance())
                    {
                        if (actor.gameParameter.LvChastity == 2)
                        {
                            if (actor.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(targetCharaID))
                            {
                                if (actor.charasGameParam.sensitivity.tableFavorabiliry[targetCharaID].longSensitivityCounts[0] < 11) return -1;
                            }
                        }
                    }
                    break;

                case 34://Touch
                    if (CustomGameBalancePlugin.GetNormalVirtueBalance())
                    {
                        if (actor.gameParameter.LvChastity == 2)
                        {
                            if (actor.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(targetCharaID))
                            {
                                if (actor.charasGameParam.sensitivity.tableFavorabiliry[targetCharaID].longSensitivityCounts[0] < 11) return -1;
                            }
                        }
                    }
                    break;

                case 35://H
                    if (CustomGameBalancePlugin.GetNormalVirtueBalance())
                    {
                        if (actor.gameParameter.LvChastity == 2)
                        {
                            if (actor.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(targetCharaID))
                            {
                                if (actor.charasGameParam.sensitivity.tableFavorabiliry[targetCharaID].longSensitivityCounts[0] < 11) return -1;
                            }
                        }
                    }
                    break;

                case 36://
                    break;

                case 37://TakingOnesLover
                    if (CustomGameBalancePlugin.GetNormalVirtueBalance())
                    {
                        if (actor.gameParameter.LvChastity == 2)
                        {
                            if (actor.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(targetCharaID))
                            {
                                if (actor.charasGameParam.sensitivity.tableFavorabiliry[targetCharaID].longSensitivityCounts[0] < 11) return -1;
                            }
                        }
                    }
                    break;

                case 38://
                    break;

                case 39://
                    break;

                case 40://
                    break;

                case 41://
                    break;

                case 42://
                    break;

                case 43://
                    break;

                case 44://
                    break;

                case 45://
                    break;

                case 46://
                    break;

                case 47://
                    break;

                case 48://
                    break;

                case 49://
                    break;

                case 50://
                    break;

                case 51://
                    break;

                case 52://LetsHave3P [Interruption]
                    break;

                case 53://
                    break;

                case 54://
                    break;

                case 55://
                    break;

                case 56://
                    break;

                case 57://
                    break;

                case 58://
                    break;

                case 59://DatingAnyone
                    if (!actor.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(targetCharaID)) return targetCharaID;
                    var isGameFixes = CustomGameBalancePlugin.GetGameFixes();
                    if (!isGameFixes[0] || !isGameFixes[2]) break;
                    if (actor.gameParameter.LvChastity > 2)
                    {
                        if (actor.charasGameParam.sensitivity.tableFavorabiliry[targetCharaID].ranks[0] == SensitivityParameter.Rank.HIGH || actor.charasGameParam.sensitivity.tableFavorabiliry[targetCharaID].ranks[0] == SensitivityParameter.Rank.MAX) return targetCharaID;
                        return -1;
                    }
                    else if (actor.gameParameter.LvChastity == 2 && CustomGameBalancePlugin.GetNormalVirtueBalance())
                    {
                        if (actor.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(targetCharaID))
                        {
                            if (actor.charasGameParam.sensitivity.tableFavorabiliry[targetCharaID].longSensitivityCounts[0] < 11) return -1;
                        }
                    }
                    break;

                case 60://TradePossessions
                    break;

                case 61://BadRumorPursuit
                    break;

                case 62://AbductTheSubject
                    if (CustomGameBalancePlugin.GetKidnapOption()) return -1;                  
                    break;

                case 63://AbductionOfTheTargetSubject
                    if (CustomGameBalancePlugin.GetKidnapOption()) return -1;
                    break;

                case 64://GoodMorningKiss
                    break;

                case 65://SurpriseKiss
                    break;

                case 66://BreakingUpWithYou
                    break;

                case 67://
                    break;

                case 68://
                    break;

                case 69://DontFollowMe
                    break;

                case 70://
                    break;

                case 71://
                    break;

                case 72://ThatGuyWasConfessing
                    break;

                case 73://TheGuyWasHavingSex
                    break;

                case 74://FollowedByThatPerson
                    break;

                case 75://YoureVeryPopular
                    break;

                case 76://DarknessDailyH
                    break;

                case 77://WantA3P
                    break;

                case 78://TakeAdvantageOfWeakness
                    break;

                case 79://KeepHimAway
                    break;

                case 80://LetMeSeeThat
                    break;

                case 81://LetMeHold
                    break;

                case 82://EliminateWeaknesses
                    break;

                case 83://
                    break;

                case 84://
                    break;

                case 85://SoloTraining
                    break;

                case 86://
                    break;

                case 87://
                    break;

                case 88://
                    break;

                case 89://
                    break;

                case 90://
                    break;

                case 91://
                    break;

                case 92://Consult_Love
                    break;

                case 93://Consult_H
                    break;

                case 94://LetsAllHomeParty
                    break;

                case 95://ImportantItemExchange
                    break;

                case 96://ComeToMyRoomWithYouHVer
                    break;

                case 97://LetsAllHaveSex_Combination
                    break;
            }
            return targetCharaID;
        }     
    }
}
