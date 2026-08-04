using BepInEx.Logging;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using Manager;
using SaveData;
using SV;
using SV.Chara;
using SV.MyRoomScene;
using System;

namespace SVS_CustomGameBalance
{
    internal static class CustomGameBalance
    {
        private static readonly Random _rnd = new();

        private static readonly Dictionary<int, AnswerBaseDataParam> nightTable = new();
        private static readonly System.Collections.Generic.List<AI> charas = new();
        private static AI selectedAI = new();

        private static bool _isPCDisable;
        //private static bool _onPeriodEnd = false;

        private static int _Conversation;
        private static int _Living;
        private static int _Stamina;
        private static int _Study;
        //private static int _JobPoint = 0;
        private static int _tempValue;
        private static int nightType = -1;
        private static int selectedCharaID = -1;
        private static float _aditiveMod = 1f;
        public static void SetAutoPC()
        {
            if (GameChara.PlayerAI != null)
            {
                AI player = GameChara.PlayerAI;
                player.BehaviourCtrl.isAuto = !player.BehaviourCtrl.isAuto;
                if (player.BehaviourCtrl.isAuto)
                {
                    player.BehaviourCtrl.isThinking = true;
                    SimulationManager.Instance?.UISimCtrl.SetVisibleButton(false);
                    if (MapManager.Instance != null && MapManager.Instance.objMapMoveUICanvas.active) MapManager.Instance.objMapMoveUICanvas.active = false;
                    SV.Sound.Play(SystemSE.ok);
                    CustomGameBalancePlugin.Log.Log(LogLevel.Message, $"PC Control: Auto");
                }
                else
                {
                    player.BehaviourCtrl.isThinking = false;
                    SimulationManager.Instance?.UISimCtrl.SetVisibleButton(true);
                    if (MapManager.Instance != null && !MapManager.Instance.objMapMoveUICanvas.active) MapManager.Instance.objMapMoveUICanvas.active = true;
                    SV.Sound.Play(SystemSE.ok);
                    CustomGameBalancePlugin.Log.Log(LogLevel.Message, $"PC Control: Manual");
                }
            }
            else
            {
                if (MyRoom._instance != null && MyRoom._instance.IsOpen()) CustomGameBalancePlugin.Log.Log(LogLevel.Message, $"Leave your room to set Auto PC");
            }
        }
               
        public static void EndOfDayStatsReduction(this Actor _self, int _basePoint, int _additive, bool _pcReduction)
        {
            _Stamina = _self.charasGameParam._baseParameter_k__BackingField.Stamina;
            _Conversation = _self.charasGameParam._baseParameter_k__BackingField.Conversation;
            _Study = _self.charasGameParam._baseParameter_k__BackingField.Study;
            _Living = _self.charasGameParam._baseParameter_k__BackingField.Living;
            //_JobPoint = _self.charasGameParam._baseParameter_k__BackingField.JobPoint;

            _aditiveMod = (float)_additive / 100;

            _isPCDisable = false;
            if (_pcReduction)
            {
                if (_self.IsPC)
                {
                    _isPCDisable = true;
                }
            }

            if (!_isPCDisable)
            {
                switch (_self.gameParameter.LvPhysical)
                {
                    case 0:
                        {
                            //Log.LogInfo($"LvPhysical: Case 0 " + _aditiveMod);
                            if (_Stamina > 200)
                            {
                                _tempValue = (int)Math.Round((_basePoint * (1 + (_aditiveMod * 4))));
                                _Stamina = _Stamina - _rnd.Next(1, _tempValue);
                                if (_Stamina < 0) _Stamina = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Stamina = _Stamina;
                            }
                            else
                            {
                                _Stamina = _Stamina - _rnd.Next(1, _basePoint);
                                if (_Stamina < 0) _Stamina = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Stamina = _Stamina;
                            }
                        }
                        break;

                    case 1:
                        {
                            //Log.LogInfo($"LvPhysical: Case 1");
                            if (_Stamina > 400)
                            {
                                _tempValue = (int)Math.Round((_basePoint * (1 + (_aditiveMod * 3))));
                                _Stamina = _Stamina - _rnd.Next(1, _tempValue);
                                if (_Stamina < 0) _Stamina = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Stamina = _Stamina;
                            }
                            else
                            {
                                _Stamina = _Stamina - _rnd.Next(1, _basePoint);
                                if (_Stamina < 0) _Stamina = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Stamina = _Stamina;
                            }
                        }
                        break;

                    case 2:
                        {
                            //Log.LogInfo($"LvPhysical: Case 2");
                            if (_Stamina > 600)
                            {
                                _tempValue = (int)Math.Round((_basePoint * (1 + (_aditiveMod * 2))));
                                _Stamina = _Stamina - _rnd.Next(1, _tempValue);
                                if (_Stamina < 0) _Stamina = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Stamina = _Stamina;
                            }
                            else
                            {
                                _Stamina = _Stamina - _rnd.Next(1, _basePoint);
                                if (_Stamina < 0) _Stamina = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Stamina = _Stamina;
                            }
                        }
                        break;

                    case 3:
                        {
                            //Log.LogInfo($"LvPhysical: Case 3" + _aditiveMod);
                            if (_Stamina > 800)
                            {
                                _tempValue = (int)Math.Round((_basePoint * (1 + _aditiveMod)));
                                _Stamina = _Stamina - _rnd.Next(1, _tempValue);
                                if (_Stamina < 0) _Stamina = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Stamina = _Stamina;
                            }
                            else
                            {
                                _Stamina = _Stamina - _rnd.Next(1, _basePoint);
                                if (_Stamina < 0) _Stamina = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Stamina = _Stamina;
                            }
                        }
                        break;

                    case 4:
                        {
                            //Log.LogInfo($"LvPhysical: Case 4");
                            _Stamina = _Stamina - _rnd.Next(1, _basePoint);
                            if (_Stamina < 0) _Stamina = 0;
                            _self.charasGameParam._baseParameter_k__BackingField.Stamina = _Stamina;
                        }
                        break;
                }

                switch (_self.gameParameter.LvTalk)
                {
                    case 0:
                        {
                            //Log.LogInfo($"LvTalk: Case 0");
                            if (_Conversation > 200)
                            {
                                _tempValue = (int)Math.Round((_basePoint * (1 + (_aditiveMod * 4))));
                                _Conversation = _Conversation - _rnd.Next(1, _tempValue);
                                if (_Conversation < 0) _Conversation = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Conversation = _Conversation;
                            }
                            else
                            {
                                _Conversation = _Conversation - _rnd.Next(1, _basePoint);
                                if (_Conversation < 0) _Conversation = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Conversation = _Conversation;
                            }
                        }
                        break;

                    case 1:
                        {
                            //Log.LogInfo($"LvTalk: Case 1");
                            if (_Conversation > 400)
                            {
                                _tempValue = (int)Math.Round((_basePoint * (1 + (_aditiveMod * 3))));
                                _Conversation = _Conversation - _rnd.Next(1, _tempValue);
                                if (_Conversation < 0) _Conversation = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Conversation = _Conversation;
                            }
                            else
                            {
                                _Conversation = _Conversation - _rnd.Next(1, _basePoint);
                                if (_Conversation < 0) _Conversation = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Conversation = _Conversation;
                            }

                        }
                        break;

                    case 2:
                        {
                            //Log.LogInfo($"LvTalk: Case 2");
                            if (_Conversation > 600)
                            {
                                _tempValue = (int)Math.Round((_basePoint * (1 + (_aditiveMod * 2))));
                                _Conversation = _Conversation - _rnd.Next(1, _tempValue);
                                if (_Conversation < 0) _Conversation = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Conversation = _Conversation;
                            }
                            else
                            {
                                _Conversation = _Conversation - _rnd.Next(1, _basePoint);
                                if (_Conversation < 0) _Conversation = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Conversation = _Conversation;
                            }

                        }
                        break;

                    case 3:
                        {
                            //Log.LogInfo($"LvTalk: Case 3");
                            if (_Conversation > 800)
                            {
                                _tempValue = (int)Math.Round((_basePoint * (1 + _aditiveMod)));

                                _Conversation = _Conversation - _rnd.Next(1, _tempValue);
                                if (_Conversation < 0) _Conversation = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Conversation = _Conversation;
                            }
                            else
                            {
                                _Conversation = _Conversation - _rnd.Next(1, _basePoint);
                                if (_Conversation < 0) _Conversation = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Conversation = _Conversation;
                            }
                        }
                        break;

                    case 4:
                        {
                            //Log.LogInfo($"LvTalk: Case 4");
                            _Conversation = _Conversation - _rnd.Next(1, _basePoint);
                            if (_Conversation < 0) _Conversation = 0;
                            _self.charasGameParam._baseParameter_k__BackingField.Conversation = _Conversation;
                        }
                        break;
                }

                switch (_self.gameParameter.LvStudy)
                {
                    case 0:
                        {
                            //Log.LogInfo($"LvStudy: Case 0");
                            if (_Study > 200)
                            {
                                _tempValue = (int)Math.Round((_basePoint * (1 + (_aditiveMod * 4))));
                                _Study = _Study - _rnd.Next(1, _tempValue);
                                if (_Study < 0) _Study = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Study = _Study;
                            }
                            else
                            {
                                _Study = _Study - _rnd.Next(1, _basePoint);
                                if (_Study < 0) _Study = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Study = _Study;
                            }


                        }
                        break;

                    case 1:
                        {
                            //Log.LogInfo($"LvStudy: Case 1");
                            if (_Study > 400)
                            {
                                _tempValue = (int)Math.Round((_basePoint * (1 + (_aditiveMod * 3))));
                                _Study = _Study - _rnd.Next(1, _tempValue);
                                if (_Study < 0) _Study = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Study = _Study;
                            }
                            else
                            {
                                _Study = _Study - _rnd.Next(1, _basePoint);
                                if (_Study < 0) _Study = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Study = _Study;
                            }
                        }
                        break;

                    case 2:
                        {
                            //Log.LogInfo($"LvStudy: Case 2");
                            if (_Study > 600)
                            {
                                _tempValue = (int)Math.Round((_basePoint * (1 + (_aditiveMod * 2))));
                                _Study = _Study - _rnd.Next(1, _tempValue);
                                if (_Study < 0) _Study = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Study = _Study;
                            }
                            else
                            {
                                _Study = _Study - _rnd.Next(1, _basePoint);
                                if (_Study < 0) _Study = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Study = _Study;
                            }
                        }
                        break;

                    case 3:
                        {
                            //Log.LogInfo($"LvStudy: Case 3");
                            if (_Study > 800)
                            {
                                _tempValue = (int)Math.Round((_basePoint * (1 + _aditiveMod)));
                                _Study = _Study - _rnd.Next(1, _tempValue);
                                if (_Study < 0) _Study = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Study = _Study;
                            }
                            else
                            {
                                _Study = _Study - _rnd.Next(1, _basePoint);
                                if (_Study < 0) _Study = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Study = _Study;
                            }
                        }
                        break;

                    case 4:
                        {
                            //Log.LogInfo($"LvStudy: Case 4");
                            _Study = _Study - _rnd.Next(1, _basePoint);
                            if (_Study < 0) _Study = 0;
                            _self.charasGameParam._baseParameter_k__BackingField.Study = _Study;
                        }
                        break;
                }

                switch (_self.gameParameter.LvLiving)
                {
                    case 0:
                        {
                            //Log.LogInfo($"LvLiving: Case 0");
                            if (_Living > 200)
                            {
                                _tempValue = (int)Math.Round((_basePoint * (1 + (_aditiveMod * 4))));
                                _Living = _Living - _rnd.Next(1, _tempValue);
                                if (_Living < 0) _Living = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Living = _Living;
                            }
                            else
                            {
                                _Living = _Stamina - _rnd.Next(1, _basePoint);
                                if (_Living < 0) _Living = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Living = _Living;
                            }

                        }
                        break;

                    case 1:
                        {
                            //Log.LogInfo($"LvLiving: Case 1");
                            if (_Living > 400)
                            {
                                _tempValue = (int)Math.Round((_basePoint * (1 + (_aditiveMod * 3))));
                                _Living = _Living - _rnd.Next(1, _tempValue);
                                if (_Living < 0) _Living = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Living = _Living;
                            }
                            else
                            {
                                _Living = _Stamina - _rnd.Next(1, _basePoint);
                                if (_Living < 0) _Living = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Living = _Living;
                            }
                        }
                        break;

                    case 2:
                        {
                            //Log.LogInfo($"LvLiving: Case 2");
                            if (_Living > 600)
                            {
                                _tempValue = (int)Math.Round((_basePoint * (1 + (_aditiveMod * 2))));
                                _Living = _Living - _rnd.Next(1, _tempValue);
                                if (_Living < 0) _Living = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Living = _Living;
                            }
                            else
                            {
                                _Living = _Stamina - _rnd.Next(1, _basePoint);
                                if (_Living < 0) _Living = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Living = _Living;
                            }

                        }
                        break;

                    case 3:
                        {
                            //Log.LogInfo($"LvLiving: Case 3");
                            if (_Living > 800)
                            {
                                _tempValue = (int)Math.Round((_basePoint * (1 + _aditiveMod)));
                                _Living = _Living - _rnd.Next(1, _tempValue);
                                if (_Living < 0) _Living = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Living = _Living;
                            }
                            else
                            {
                                _Living = _Stamina - _rnd.Next(1, _basePoint);
                                if (_Living < 0) _Living = 0;
                                _self.charasGameParam._baseParameter_k__BackingField.Living = _Living;
                            }

                        }
                        break;

                    case 4:
                        {
                            //Log.LogInfo($"LvLiving: Case 4");
                            _Living = _Stamina - _rnd.Next(1, _basePoint);
                            if (_Living < 0) _Living = 0;
                            _self.charasGameParam._baseParameter_k__BackingField.Living = _Living;
                        }
                        break;
                }
            }
        }
        public static void CheaterEnhancer(this Actor _actor, int _cheaterPoints, bool _noPC, bool _onEndPeriod)
        {
            var _memory = _actor.charasGameParam.memory;
            var _sex = _actor.charFile.Parameter.sex; //1 is Female
            var _sexTarget = _actor.gameParameter.SexualTarget; //0 Hetero

            var _checkForAdultery = _actor.charasGameParam.memory.logInfoTables.ContainsKey(74);
            var _checkForThievingCat = _actor.charasGameParam.memory.logInfoTables.ContainsKey(75);

            if (_checkForAdultery)
            {
                var _count = _actor.charasGameParam.memory.logInfoTables[74].Count;

                if (_count > 0)
                {
                    foreach (var _cheater in _actor.charasGameParam.memory.logInfoTables[74])
                    {
                        foreach (var _targetID in _cheater.charas)
                        {
                            if (!Game.Charas[_targetID].IsPC)
                            {
                                //Set the array shortStocks (subPoints)
                                Il2CppStructArray<int> _favors = CheaterFavorValues(_actor, _cheaterPoints, _sex, _sexTarget, _targetID, _onEndPeriod);
                                _actor.charasGameParam.sensitivity.AddFavor(_memory, _targetID, _favors);
                            }
                            else
                            {
                                if (!_noPC)
                                {
                                    //Set the array shortStocks (subPoints)
                                    Il2CppStructArray<int> _favors = CheaterFavorValues(_actor, _cheaterPoints, _sex, _sexTarget, _targetID, _onEndPeriod);
                                    _actor.charasGameParam.sensitivity.AddFavor(_memory, _targetID, _favors);
                                }
                            }
                        }
                    }
                }
            }

            if (_checkForThievingCat)
            {
                var _count = _actor.charasGameParam.memory.logInfoTables[75].Count;

                if (_count > 0)
                {
                    foreach (var _cheater in _actor.charasGameParam.memory.logInfoTables[75])
                    {
                        foreach (var _targetID in _cheater.charas)
                        {
                            if (!Game.Charas[_targetID].IsPC)
                            {
                                //Set the array shortStocks (subPoints)
                                Il2CppStructArray<int> _favors = ThievingCatFavorValues(_actor, _cheaterPoints, _sex, _sexTarget, _targetID, _onEndPeriod);
                                _actor.charasGameParam.sensitivity.AddFavor(_memory, _targetID, _favors);
                            }
                            else
                            {
                                if (!_noPC)
                                {
                                    //Set the array shortStocks (subPoints)
                                    Il2CppStructArray<int> _favors = ThievingCatFavorValues(_actor, _cheaterPoints, _sex, _sexTarget, _targetID, _onEndPeriod);
                                    _actor.charasGameParam.sensitivity.AddFavor(_memory, _targetID, _favors);
                                }
                            }
                        }
                    }
                }
            }
        }
        public static Il2CppStructArray<int> CheaterFavorValues(this Actor _self, int _cheaterPoints, byte _sex, int _sexTarget, int _targetID, bool _onEndPeriod)
        {
            var _targetSex = Game.Charas[_targetID].charFile.Parameter.sex;

            Il2CppStructArray<int> _favors = new(4);
            for (int i = 0; i < _favors.Length; i++)
            {
                _favors[i] = 0;
            }

            _favors[3] = _cheaterPoints;

            //Additive
            //Jealous
            if (_self.gameParameter.individuality.answer.Contains(10)) _favors[3] = _favors[3] + 45;
            // Melancholic 
            if (_self.gameParameter.individuality.answer.Contains(11)) _favors[3] = _favors[3] + 10;
            // Serious
            if (_self.gameParameter.individuality.answer.Contains(13)) _favors[3] = _favors[3] + 30;
            // Hot-Headed
            if (_self.gameParameter.individuality.answer.Contains(18)) _favors[3] = _favors[3] + 20;
            // Romantic
            if (_self.gameParameter.individuality.answer.Contains(27)) _favors[3] = _favors[3] + 40;
            // Single Minded
            if (_self.gameParameter.individuality.answer.Contains(29)) _favors[3] = _favors[3] + 50;
            // Masochist
            if (_self.gameParameter.individuality.answer.Contains(35))
            {
                if (_sex != _targetSex)
                {
                    if (_sexTarget != 4) _favors[0] = _favors[0] + 30;
                }
                else
                {
                    if (_sexTarget != 0) _favors[0] = _favors[0] + 30;
                }
            }

            //Minus
            //Obedient
            if (_self.gameParameter.individuality.answer.Contains(7)) _favors[3] = _favors[3] - 15;
            // Indecisive
            if (_self.gameParameter.individuality.answer.Contains(30)) _favors[3] = _favors[3] - 5;
            // Blind
            if (_self.gameParameter.individuality.answer.Contains(38)) _favors[3] = _favors[3] - 30;

            //Multiplier
            // Evil
            if (_self.gameParameter.individuality.answer.Contains(36)) _favors[3] = (_favors[3] + 10) * 2;

            if (_favors[0] < 0) _favors[0] = 0;
            if (_favors[1] < 0) _favors[1] = 0;
            if (_favors[2] < 0) _favors[2] = 0;
            if (_favors[3] < 0) _favors[3] = 0;

            if (_onEndPeriod)
            {
                if(_favors[3] != 0) _favors[3] = _favors[3] / 4;
                if(_favors[0] != 0) _favors[0] = _favors[0] / 4;
            }

            return _favors;
        }
        public static Il2CppStructArray<int> ThievingCatFavorValues(this Actor _self, int _cheaterPoints, byte _sex, int _sexTarget, int _targetID, bool _onEndPeriod)
        {
            var _targetSex = Game.Charas[_targetID].charFile.Parameter.sex;

            Il2CppStructArray<int> _favors = new(4);
            for (int i = 0; i < _favors.Length; i++)
            {
                _favors[i] = 0;
            }

            _favors[3] = _cheaterPoints;

            // Aditive
            //Jealous
            if (_self.gameParameter.individuality.answer.Contains(10)) _favors[3] = _favors[3] + 90;
            // Melancholic 
            if (_self.gameParameter.individuality.answer.Contains(11)) _favors[3] = _favors[3] + 5;
            // Serious
            if (_self.gameParameter.individuality.answer.Contains(13)) _favors[3] = _favors[3] + 30;
            // Hot-Headed
            if (_self.gameParameter.individuality.answer.Contains(18)) _favors[3] = _favors[3] + 15;
            // Single Minded
            if (_self.gameParameter.individuality.answer.Contains(29)) _favors[3] = _favors[3] + 20;
            // Masochist
            if (_self.gameParameter.individuality.answer.Contains(35))
            {
                if (_sex != _targetSex)
                {
                    if (_sexTarget != 4) _favors[0] = _favors[0] + 10;
                }
                else
                {
                    if (_sexTarget != 0) _favors[0] = _favors[0] + 10;
                }
            }

            // Minus
            //Obedient
            if (_self.gameParameter.individuality.answer.Contains(7)) _favors[3] = _favors[3] - 5;
            // Indecisive
            if (_self.gameParameter.individuality.answer.Contains(30)) _favors[3] = _favors[3] - 5;
            // Blind
            if (_self.gameParameter.individuality.answer.Contains(38)) _favors[3] = _favors[3] - 30;
            
            //Multiplier
            // Evil
            if (_self.gameParameter.individuality.answer.Contains(36)) _favors[3] = (_favors[3] + 10) * 2;           

            if (_favors[0] < 0) _favors[0] = 0;
            if (_favors[1] < 0) _favors[1] = 0;
            if (_favors[2] < 0) _favors[2] = 0;
            if (_favors[3] < 0) _favors[3] = 0;

            if (_onEndPeriod)
            {
                if (_favors[3] != 0) _favors[3] = _favors[3] / 4;
                if (_favors[0] != 0) _favors[0] = _favors[0] / 4;
            }

            return _favors;
        }
        public static int ReactionChance(AI _ai, AI _ai1, AI _ai2, int no, int reactionNo, int[] _chances, bool[] _charaType)
        {
            if (_ai == null) return reactionNo;
            
            // ActionNo:
            // 0  : None
            // 1  : React to H
            // 2  : React Masturbation?
            // 5  : Fight
            // 6  : Skinship
            // 7  : Normal Interruption
            // 8  : Losing H Contest
            // 9  : Changing Room?
            // 10 : H again 3P?

            if (reactionNo == -1) return -1;
            int _reactionValue = reactionNo;

            if (_ai1 != null && _ai2 != null)
            {
                if (!_charaType[0])
                {
                    if (_ai1.charaData.IsPC || _ai2.charaData.IsPC) return _reactionValue;
                }

                if (!_charaType[1])
                {
                    if (!_ai1.charaData.IsPC && !_ai2.charaData.IsPC) return _reactionValue;
                }
            }

            int _rngMax = 100;
            if (_ai.chaCtrl.fileGameParam.individuality.answer.Contains(10)) _rngMax = 133;

            switch (no)
            {
                case 1:// React to H                   
                    if (reactionNo == 2 || reactionNo == 3)
                    {
                        if (_chances[0] > 0)
                        {
                            var prob = _rnd.Next(1, _rngMax);
                            if (prob <= _chances[0]) _reactionValue = -1;
                            else
                            {
                                prob = _rnd.Next(1, _rngMax);
                                if (prob <= _chances[3]) _reactionValue = -1;
                                break;
                            }
                        }

                        if (_chances[3] > 0)
                        {
                            var prob = _rnd.Next(1, _rngMax);
                            if (prob <= _chances[3]) _reactionValue = -1;
                        }
                    }
                    break;

                case 6://React to skinship
                    if (reactionNo == 0 || reactionNo == 1)
                    {
                        if (_chances[0] > 0)
                        {
                            var prob = _rnd.Next(1, _rngMax);
                            if (prob <= _chances[0]) _reactionValue = -1;
                            else
                            {
                                prob = _rnd.Next(1, _rngMax);
                                if (prob <= _chances[2]) _reactionValue = -1;
                                break;
                            }
                        }

                        if (_chances[2] > 0)
                        {
                            var prob = _rnd.Next(1, _rngMax);
                            if (prob <= _chances[2]) _reactionValue = -1;
                        }
                    }                     
                    break;

                case 7://React Normal Interactions
                    if (reactionNo == 0 || reactionNo == 1)
                    {
                        if (_chances[0] > 0)
                        {
                            var prob = _rnd.Next(1, _rngMax);
                            if (prob <= _chances[0]) _reactionValue = -1;
                            else
                            {
                                prob = _rnd.Next(1, _rngMax);
                                if (prob <= _chances[1]) _reactionValue = -1;
                                break;
                            }
                        }

                        if (_chances[1] > 0)
                        {
                            var prob = _rnd.Next(1, _rngMax);
                            if (prob <= _chances[1]) _reactionValue = -1;
                        }
                    }                 
                    break;
            }
            return _reactionValue;
        }
        public static void NewAnswerRate(YesNoJudgeManager.AnswerInfo _oldAnswerInfo, YesNoJudgeManager.YesNoInfo yesNoInfo, int _commandID, int _questionCount)
        {
            if (yesNoInfo.active == null || yesNoInfo.passive == null) return;
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
        /*public static YesNoJudgeManager.AnswerInfo NewAnswerRateOld(YesNoJudgeManager.AnswerInfo _oldAnswerInfo, YesNoJudgeManager.YesNoInfo yesNoInfo, int _commandID, int _questionCount)
        {
            var isGameFixes = CustomGameBalancePlugin.GetGameFixes();
            var isNewLowestRate = CustomGameBalancePlugin.GetActionLowestRateEnable();
            var isForceActions = CustomGameBalancePlugin.GetForceActions();

            if (yesNoInfo.active == null && yesNoInfo.passive == null) return _oldAnswerInfo;


            if (!isGameFixes[0] && !isNewLowestRate[0]) return _oldAnswerInfo;

            if (isGameFixes[0])
            {
                int askingCharaID = yesNoInfo.active.charasGameParam.Index;

                switch (_commandID)
                {
                    case 28:
                        if (!isGameFixes[1]) break;
                        if (yesNoInfo.passive.charasGameParam.memory.passivePromiseHousePartys.Count > 0 || yesNoInfo.passive.charasGameParam.memory.activePromiseHousePartys.Count > 0) return _oldAnswerInfo;

                        if (yesNoInfo.active != null && yesNoInfo.passive != null)
                        {
                            if (yesNoInfo.passive.gameParameter.LvChastity > 2)
                            {
                                if (_oldAnswerInfo.rate < 10)
                                {
                                    if (!yesNoInfo.passive.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(askingCharaID)) break;

                                    _oldAnswerInfo.rate = CalcBBQAnswerBaseRate(yesNoInfo.passive, yesNoInfo.active, askingCharaID);
                                    if (_oldAnswerInfo.rate <= 0) break;
                                    if (_oldAnswerInfo.rate >= 100)
                                    {
                                        _oldAnswerInfo.ans = 0;
                                        break;
                                    }
                                    int chance = _rnd.Next(1, 100);
                                    if (chance <= _oldAnswerInfo.rate) _oldAnswerInfo.ans = 0;
                                    else _oldAnswerInfo.ans = 1;
                                }
                            }
                        }
                        break;

                    case 59:
                        if (!isGameFixes[2]) break;
                        if (_questionCount == 1)
                        {
                            switch (yesNoInfo.passive.gameParameter.LvChastity)
                            {
                                case 3:
                                    if (yesNoInfo.passive.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(askingCharaID))
                                    {
                                        if (yesNoInfo.passive.charasGameParam.sensitivity.tableFavorabiliry[askingCharaID].ranks[0] == SensitivityParameter.Rank.HIGH || yesNoInfo.passive.charasGameParam.sensitivity.tableFavorabiliry[askingCharaID].ranks[0] == SensitivityParameter.Rank.MAX) break;
                                        else
                                        {
                                            if (yesNoInfo.passive.charasGameParam.sensitivity.tableFavorabiliry[askingCharaID].ranks[0] == SensitivityParameter.Rank.MIDDLE)
                                            {
                                                _oldAnswerInfo.rate = 10;
                                                int chance = _rnd.Next(1, 100);
                                                if (chance <= _oldAnswerInfo.rate) _oldAnswerInfo.ans = 0;
                                                else _oldAnswerInfo.ans = 1;
                                            }
                                            else
                                            {
                                                _oldAnswerInfo.rate = 0;
                                                _oldAnswerInfo.ans = 1;
                                            }
                                        }
                                    }
                                    break;

                                case 4:
                                    if (yesNoInfo.passive.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(askingCharaID))
                                    {
                                        if (yesNoInfo.passive.charasGameParam.sensitivity.tableFavorabiliry[askingCharaID].ranks[0] == SensitivityParameter.Rank.HIGH || yesNoInfo.passive.charasGameParam.sensitivity.tableFavorabiliry[askingCharaID].ranks[0] == SensitivityParameter.Rank.MAX) break;
                                        else
                                        {
                                            _oldAnswerInfo.rate = 0;
                                            _oldAnswerInfo.ans = 1;
                                        }
                                    }
                                    break;
                            }
                        }

                        break;
                    case 30:
                        if (isForceActions[0])
                        {
                            if (yesNoInfo.passive.charasGameParam.memory.lovers.Count > 0)
                            {
                                var lovers = yesNoInfo.passive.charasGameParam.memory.lovers;
                                foreach (var lover in lovers) 
                                {
                                    if (lover.id == yesNoInfo.active.charasGameParam.Index)
                                    {
                                        _oldAnswerInfo.rate = 100;
                                        _oldAnswerInfo.ans = 0;
                                    } 
                                }
                            }
                        }
                        break;
                }
            }

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
                if (yesNoInfo.aParam.isPC)
                {
                    if (isNewLowestRate[1])
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

            if (CustomGameBalancePlugin.GetFortuneFix())
            {
                if (_oldAnswerInfo.rate > 0 && Manager.Game.saveData.dataCount.circularNotice == 7 && Manager.Game.saveData.dataCount.isCircularNoticeUse)
                {
                    _oldAnswerInfo.rate *= 1.1f;
                    if (_oldAnswerInfo.ans == 1 && _oldAnswerInfo.rate > 0)
                    {
                        var chance = _rnd.Next(1, 100);
                        if (chance <= _oldAnswerInfo.rate) _oldAnswerInfo.ans = 0;
                        else _oldAnswerInfo.ans = 1;
                    }
                }
            }

            return _oldAnswerInfo;
        }*/
        /*public static int CalcBBQAnswerBaseRate(Actor _actor, Actor _TargetActor, int targetID)
        {
            int baseRate = 0;

            if ((_actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[0] == SensitivityParameter.Rank.MIDDLE && _actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[1] == SensitivityParameter.Rank.MIDDLE)
                || _actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[1] == SensitivityParameter.Rank.HIGH 
                || (_actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[0] == SensitivityParameter.Rank.MIDDLE && _actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[2] == SensitivityParameter.Rank.LOW && _actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[3] == SensitivityParameter.Rank.LOW))
            {
                switch (_actor.gameParameter.LvChastity)
                {
                    case 3:
                        if (_actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[1] == SensitivityParameter.Rank.HIGH) baseRate = 60;
                        if (_actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[0] == SensitivityParameter.Rank.MIDDLE && _actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[1] == SensitivityParameter.Rank.MIDDLE) baseRate = 50;
                        else if (_actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[0] == SensitivityParameter.Rank.MIDDLE && _actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[2] == SensitivityParameter.Rank.LOW && _actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[3] == SensitivityParameter.Rank.LOW) baseRate = 40;
                        else break;
                        if (_actor.charasGameParam.memory.lovers.Count > 0)
                        {
                            foreach (var lover in _actor.charasGameParam.memory.lovers)
                            {
                                if (lover.id == targetID)
                                {
                                    baseRate += 22;
                                    break;
                                } 
                            }
                        }
                        break;
                    case 4:
                        if (_actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[1] == SensitivityParameter.Rank.HIGH) baseRate = 40;
                        if (_actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[0] == SensitivityParameter.Rank.MIDDLE && _actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[1] == SensitivityParameter.Rank.MIDDLE) baseRate = 30;
                        else if (_actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[0] == SensitivityParameter.Rank.MIDDLE && _actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[2] == SensitivityParameter.Rank.LOW && _actor.charasGameParam.sensitivity.tableFavorabiliry[targetID].ranks[3] == SensitivityParameter.Rank.LOW) baseRate = 20;
                        else break;
                        if (_actor.charasGameParam.memory.lovers.Count > 0)
                        {
                            foreach (var lover in _actor.charasGameParam.memory.lovers)
                            {
                                if (lover.id == targetID)
                                {
                                    baseRate += 11;
                                    break;
                                }
                            }
                        }
                        break;
                }

                ///Additive and subtraction
                //Check Traits
                if (_actor.gameParameter.individuality.answer.Contains(6)) baseRate += 4;
                if (_actor.gameParameter.individuality.answer.Contains(7)) baseRate += 7;
                if (_actor.gameParameter.individuality.answer.Contains(9)) baseRate -= 10;
                if (_actor.gameParameter.individuality.answer.Contains(27)) baseRate += 5;

                //Check Mood
                if (_actor.charasGameParam.state._State_k__BackingField == StateParameter.StateKind.UPLIFT) baseRate += 4;
                if (_actor.charasGameParam.state._State_k__BackingField == StateParameter.StateKind.EARNESTNESS) baseRate += 8;
                if (_actor.charasGameParam.state._State_k__BackingField == StateParameter.StateKind.ANGER) baseRate -= 16;
                if (_actor.charasGameParam.state._State_k__BackingField == StateParameter.StateKind.DISAPPOINTMENT) baseRate -= 8;
                if (_actor.charasGameParam.state._State_k__BackingField == StateParameter.StateKind.TENSION) baseRate -= 4;

                ///Multi and Division
                if (_actor.gameParameter.individuality.answer.Contains(2))
                {
                    if (_TargetActor.parameter.sex == 0)
                    {
                        var division = baseRate / 1.1f;
                        baseRate = (int)division;
                    }
                }
                if (_actor.gameParameter.individuality.answer.Contains(3))
                {
                    if (_TargetActor.parameter.sex == 1)
                    {
                        var division = baseRate / 1.1f;
                        baseRate = (int)division;
                    }
                }
                if (_TargetActor.gameParameter.individuality.answer.Contains(4))
                {
                    if (_TargetActor.parameter.sex != _actor.parameter.sex)
                    {
                        var division = baseRate * 1.1f;
                        baseRate = (int)division;
                    }
                    else
                    {
                        var division = baseRate / 1.1f;
                        baseRate = (int)division;
                    }
                }
                if (_actor.gameParameter.individuality.answer.Contains(5))
                {
                    if (_TargetActor.parameter.sex == _actor.parameter.sex)
                    {
                        var division = baseRate * 1.1f;
                        baseRate = (int)division;
                    }                  
                }

                ///Overrides
                if (_actor.gameParameter.individuality.answer.Contains(29))
                {
                    if (_actor.charasGameParam.memory.lovers.Count > 0)
                    {
                        bool foundLover = false;
                        foreach (var lover in _actor.charasGameParam.memory.lovers)
                        {
                            if (lover.id == targetID)
                            {
                                foundLover = true;
                                break;
                            }
                        }
                        if (foundLover) baseRate += 5;
                        else baseRate = 0;
                    }
                }
            }

            if (baseRate < 0) baseRate = 0;
            return baseRate;
        }*/   

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
                            if (chance > 50) _successNo = 0;
                            break;
                        case 2:
                            if (chance > 90) _successNo = 0;
                            else if (chance > 50) _successNo = 1;
                            break;
                        case 3:
                            if (chance > 90) _successNo = 1;
                            else if (chance > 50) _successNo = 2;
                            break;
                        case 4:
                            if (chance > 90) _successNo = 2;
                            else if (chance > 50) _successNo = 3;
                            break;
                    }
                }
            }
            return _successNo;
        }
        public static void GetNightRateList()
        {
            if (nightTable.Count == 0)
            {
                var nightEventTable = NightEventManager.Instance.table;
                foreach (var table in nightEventTable)
                {
                    AnswerBaseDataParam abdp = new();
                    abdp.ID = table.Value.ID;
                    abdp.BaseRates = new();
                    foreach (var bRates in table.Value.BaseRates)
                    {
                        abdp.BaseRates.Add(bRates);
                    }
                    abdp.Rates = new();
                    foreach (var rate in table.Value.Rates)
                    {
                        abdp.Rates.Add(rate);
                    }

                    nightTable.Add(table.Key, abdp);
                }
            }
        }
        public static void SetNightCharacters()
        {
            charas.Clear();
            var tempAI = SimulationScene.Instance.tempAIs;
            var playerID = GameChara.Player.charasGameParam.Index;
            var playerSex = GameChara.Player.chaCtrl.data.Parameter.sex;
            var playerSexTarget = GameChara.Player.gameParameter.SexualTarget;

            if (tempAI == null)
            {
                CustomGameBalancePlugin.Log.LogInfo($"ERROR! tempAIs is Null!");
                return;
            }
            if (tempAI.Count == 0)
            {
                CustomGameBalancePlugin.Log.LogInfo($"ERROR! tempAIs is Empty!");
                return;
            }

            //CustomGameBalancePlugin.Log.LogInfo($"Character in tempAIs {tempAI.Count}");
            //Get Chara Pool
            foreach (var ai in tempAI)
            {
                if (ai.charaData.charasGameParam.isPC || ai.charaData.chaCtrl.data.Parameter.sex == 0) continue;
                //if (ai.charaData.charasGameParam.isPC && ai.charaData.chaCtrl.data.Parameter.sex == 1) continue;
                if (ai.charaData.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(playerID))
                {
                    var charaRanks = ai.charaData.charasGameParam.sensitivity.tableFavorabiliry[playerID].ranks;
                    var charaMood = ai.charaData.charasGameParam.state.State;
                    if (charaRanks[0] == SensitivityParameter.Rank.MIDDLE || charaRanks[0] == SensitivityParameter.Rank.HIGH ||
                        charaRanks[1] == SensitivityParameter.Rank.MIDDLE || charaRanks[1] == SensitivityParameter.Rank.HIGH)
                    {
                        if (charaMood == StateParameter.StateKind.ANGER || charaMood == StateParameter.StateKind.DISAPPOINTMENT) continue;
                        charas.Add(ai);
                        //CustomGameBalancePlugin.Log.LogInfo($"Chara ID:{ai.charaData.charasGameParam.Index} Added to character Pool:");
                    }
                }
            }

            //CustomGameBalancePlugin.Log.LogInfo($"Charas for night event {charas.Count}");

            //Select Chara from Pool and Type of visit
            if (charas.Count > 0)
            {
                int[] chance = CustomGameBalancePlugin.GetNightChance();
                if (chance[0] > 0 || chance[1] > 0)
                {
                    int[] visitType = new int[charas.Count];
                    //CustomGameBalancePlugin.Log.LogInfo($"Checks Lists (They should be the same): List1:{visitType.Length} = List2:{charas.Count}");

                    System.Collections.Generic.List<int> visitHigh = new();

                    int charaSelectedIndex;
                    for (int i = 0; i < charas.Count; i++)
                    {
                        if (i >= charas.Count) break;
                        if (charas[i].charaData.charasGameParam.sensitivity.tableFavorabiliry[playerID].mostRanks != null)
                        {
                            if (charas[i].charaData.charasGameParam.sensitivity.tableFavorabiliry[playerID].mostRanks.Count > 0)
                            {
                                visitType[i] = charas[i].charaData.charasGameParam.sensitivity.tableFavorabiliry[playerID].mostRanks[0];
                            }
                            else visitType[i] = 1;
                        }
                        else visitType[i] = 1;

                        if (charas[i].charaData.charasGameParam.sensitivity.tableFavorabiliry[playerID].ranks != null)
                        {
                            if (charas[i].charaData.charasGameParam.sensitivity.tableFavorabiliry[playerID].ranks.Count > 3)
                            {
                                if (charas[i].charaData.charasGameParam.sensitivity.tableFavorabiliry[playerID].ranks[visitType[i]] == SensitivityParameter.Rank.HIGH) visitHigh.Add(i);
                            }
                        }

                        if (charas[i].charaData.charasGameParam.memory.lovers.Count > 0)
                        {
                            foreach (var lover in charas[i].charaData.charasGameParam.memory.lovers)
                            {
                                if (lover.id == playerID)
                                {
                                    visitHigh.Add(i);
                                    break;
                                } 
                            }
                        } 
                        if (charas[i].charaData.gameParameter.LvSociability > 3) visitHigh.Add(i);

                        var traits = charas[i].charaData.gameParameter.individuality.answer;
                        if (traits.Contains(27)) visitHigh.Add(i);
                        if (traits.Contains(36)) visitHigh.Add(i);                
                    }
                    //CustomGameBalancePlugin.Log.LogInfo($"Regular Visit List Count: {visitType.Length}");
                    //CustomGameBalancePlugin.Log.LogInfo($"HighVisit Count: {visitHigh.Count}");

                    int selectedFromAllPool = 0;
                    if (visitType.Length > 1)
                    {
                        selectedFromAllPool = _rnd.Next(0, visitType.Length);
                    }

                    int selectedFromHighPool = 0;
                    if (visitHigh.Count > 1)
                    {
                        selectedFromHighPool = _rnd.Next(0, visitHigh.Count);
                    }

                    if (!visitHigh.Contains(selectedFromAllPool) && visitHigh.Count > 0)
                    {
                        int finalSelection = _rnd.Next(0, 3);
                        if (finalSelection == 0) charaSelectedIndex = selectedFromAllPool;
                        else charaSelectedIndex = visitHigh[selectedFromHighPool];
                    }
                    else charaSelectedIndex = selectedFromAllPool;                

                    int type = visitType[charaSelectedIndex];
                    bool isLover = false;
                    
                    //CustomGameBalancePlugin.Log.LogInfo($"Pre Selection of Chara ID: {charaSelectedIndex}");
                    //CustomGameBalancePlugin.Log.LogInfo($"Chara Visit Type: {type}");

                    //Check character Orientation
                    if (charas[charaSelectedIndex].charaData.gameParameter.SexualTarget == 0 || playerSexTarget == 0)//Hetero
                    {
                        if (charas[charaSelectedIndex].charaData.chaCtrl.data.Parameter.sex == playerSex)
                        {
                            type = 1;
                            selectedAI = charas[charaSelectedIndex];
                            selectedCharaID = charas[charaSelectedIndex].charaData.charasGameParam.Index;
                            nightType = type;
                            //CustomGameBalancePlugin.Log.LogInfo($"Selected Hetero:{selectedCharaID} - {selectedAI.charaData.Name}");
                            return;
                        } 
                    }

                    if (charas[charaSelectedIndex].charaData.gameParameter.SexualTarget == 4 || playerSexTarget == 4)//Homo
                    {
                        if (charas[charaSelectedIndex].charaData.chaCtrl.data.Parameter.sex != playerSex)
                        {
                            type = 1;
                            selectedAI = charas[charaSelectedIndex];
                            selectedCharaID = charas[charaSelectedIndex].charaData.charasGameParam.Index;
                            nightType = type;
                            //CustomGameBalancePlugin.Log.LogInfo($"Selected Homo:{selectedCharaID} - {selectedAI.charaData.Name}");
                            return;
                        } 
                    }

                    //Check Hard Conditions
                    switch (charas[charaSelectedIndex].charaData.gameParameter.LvChastity)
                    {
                        case 4:
                            switch (type)
                            {
                                case 0://Sex Visit
                                    if (charas[charaSelectedIndex].charaData.charasGameParam.memory.pairTable[playerID].TotalH == 0)
                                    {
                                        type = 1;
                                        break;
                                    }
                                    if (charas[charaSelectedIndex].charaData.charasGameParam.sensitivity.tableFavorabiliry[playerID].ranks[0] != SensitivityParameter.Rank.HIGH)
                                    {
                                        type = 1;
                                        break;
                                    }
                                    if (charas[charaSelectedIndex].charaData.gameParameter.individuality.answer.Contains(29))
                                    {
                                        if (charas[charaSelectedIndex].charaData.charasGameParam.memory.lovers.Count > 0)
                                        {
                                            foreach (var lover in charas[charaSelectedIndex].charaData.charasGameParam.memory.lovers)
                                            {
                                                if (lover.id == playerID)
                                                {
                                                    isLover = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (!isLover)
                                        {
                                            type = 1;
                                        }
                                    }
                                    break;
                                case 1://Friend Visit
                                    if (charas[charaSelectedIndex].charaData.charasGameParam.state.State == StateParameter.StateKind.RUT && 
                                        charas[charaSelectedIndex].charaData.charasGameParam.memory.pairTable[playerID].TotalH > 0 &&
                                        charas[charaSelectedIndex].charaData.charasGameParam.sensitivity.tableFavorabiliry[playerID].ranks[0] == SensitivityParameter.Rank.HIGH)
                                    {
                                        if (charas[charaSelectedIndex].charaData.gameParameter.individuality.answer.Contains(29))
                                        {
                                            if (charas[charaSelectedIndex].charaData.charasGameParam.memory.lovers.Count > 0)
                                            {
                                                foreach (var lover in charas[charaSelectedIndex].charaData.charasGameParam.memory.lovers)
                                                {
                                                    if (lover.id == playerID)
                                                    {
                                                        isLover = true;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (!isLover)
                                            {
                                                break;
                                            }
                                        }
                                        type = 0;
                                    }
                                    break;
                            }                        
                            break;
                        case 3:
                            switch (type)
                            {
                                case 0://Sex Visit
                                    if (charas[charaSelectedIndex].charaData.charasGameParam.memory.pairTable[playerID].TotalH == 0)
                                    {
                                        type = 1;
                                        break;
                                    }
                                    if (charas[charaSelectedIndex].charaData.charasGameParam.sensitivity.tableFavorabiliry[playerID].ranks[0] == SensitivityParameter.Rank.MIDDLE &&
                                        charas[charaSelectedIndex].charaData.charasGameParam.state.State != StateParameter.StateKind.RUT)
                                    {
                                        type = 1;
                                        break;
                                    }
                                    if (charas[charaSelectedIndex].charaData.gameParameter.individuality.answer.Contains(29))
                                    {
                                        if (charas[charaSelectedIndex].charaData.charasGameParam.memory.lovers.Count > 0)
                                        {
                                            foreach (var lover in charas[charaSelectedIndex].charaData.charasGameParam.memory.lovers)
                                            {
                                                if (lover.id == playerID)
                                                {
                                                    isLover = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (!isLover)
                                        {
                                            type = 1;
                                        }
                                    }
                                    break;
                                case 1://Friend Visit
                                    if (charas[charaSelectedIndex].charaData.charasGameParam.state.State == StateParameter.StateKind.RUT &&
                                        charas[charaSelectedIndex].charaData.charasGameParam.memory.pairTable[playerID].TotalH > 0 &&
                                        (charas[charaSelectedIndex].charaData.charasGameParam.sensitivity.tableFavorabiliry[playerID].ranks[0] == SensitivityParameter.Rank.HIGH ||
                                        charas[charaSelectedIndex].charaData.charasGameParam.sensitivity.tableFavorabiliry[playerID].ranks[0] == SensitivityParameter.Rank.MIDDLE))
                                    {
                                        if (charas[charaSelectedIndex].charaData.gameParameter.individuality.answer.Contains(29))
                                        {
                                            if (charas[charaSelectedIndex].charaData.charasGameParam.memory.lovers.Count > 0)
                                            {
                                                foreach (var lover in charas[charaSelectedIndex].charaData.charasGameParam.memory.lovers)
                                                {
                                                    if (lover.id == playerID)
                                                    {
                                                        isLover = true;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (!isLover)
                                            {
                                                break;
                                            }
                                        }
                                        type = 0;
                                    }
                                    break;
                            }
                            break;                        
                        case 2:
                            switch (type)
                            {
                                case 0://Sex Visit
                                    if (charas[charaSelectedIndex].charaData.gameParameter.individuality.answer.Contains(29))
                                    {
                                        if (charas[charaSelectedIndex].charaData.charasGameParam.memory.lovers.Count > 0)
                                        {
                                            foreach (var lover in charas[charaSelectedIndex].charaData.charasGameParam.memory.lovers)
                                            {
                                                if (lover.id == playerID)
                                                {
                                                    isLover = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (!isLover)
                                        {
                                            type = 1;
                                        }
                                    }
                                    break;
                                case 1://Friend Visit
                                    if (charas[charaSelectedIndex].charaData.charasGameParam.state.State == StateParameter.StateKind.RUT &&
                                        charas[charaSelectedIndex].charaData.charasGameParam.sensitivity.tableFavorabiliry[playerID].ranks[0] == SensitivityParameter.Rank.MIDDLE)
                                    {
                                        type = 0;
                                    }
                                    break;
                            }
                            break;                           
                        case 1:
                            switch (type)
                            {
                                case 0://Sex Visit
                                    break;
                                case 1://Friend Visit
                                    if (charas[charaSelectedIndex].charaData.charasGameParam.state.State == StateParameter.StateKind.RUT)
                                    {
                                        type = 0;
                                    }
                                    break;
                            }
                            break;                          
                        case 0:
                            switch (type)
                            {
                                case 0://Sex Visit
                                    if (charas[charaSelectedIndex].charaData.gameParameter.individuality.answer.Contains(29))
                                    {
                                        if (charas[charaSelectedIndex].charaData.charasGameParam.memory.lovers.Count > 0)
                                        {
                                            foreach (var lover in charas[charaSelectedIndex].charaData.charasGameParam.memory.lovers)
                                            {
                                                if (lover.id == playerID)
                                                {
                                                    isLover = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (!isLover)
                                        {
                                            type = 1;
                                        }
                                    }
                                    break;
                                case 1://Friend Visit
                                    if (charas[charaSelectedIndex].charaData.charasGameParam.state.State == StateParameter.StateKind.RUT)
                                    {
                                        type = 0;
                                    }
                                    break;
                            }
                            break;
                    }
                    //CustomGameBalancePlugin.Log.LogInfo($"Chara Chastity: {charas[charaSelectedIndex].charaData.gameParameter.LvChastity}");
                    //CustomGameBalancePlugin.Log.LogInfo($"Chara Visit Type Final: {type}");
                    //selectedAI = charas[charaSelectedIndex];
                    selectedCharaID = charas[charaSelectedIndex].charaData.charasGameParam.Index;
                    nightType = type;
                    //CustomGameBalancePlugin.Log.LogInfo($"Selected CharaID:{selectedCharaID}");
                    return;     
                }                 
            }          
            CustomGameBalancePlugin.Log.LogInfo($"No available characters for night visit");
            nightType = -1;
            selectedCharaID = -1;
        }

        public static void NightEventChance(NightEventManager nightEvent, Actor chara)
        {
            if (chara == null) return;
            if (nightEvent == null) return;
            if (chara.charasGameParam.Index == selectedCharaID)
            {
                //CustomGameBalancePlugin.Log.LogInfo($"Chara Found");
                var visitType = CustomGameBalancePlugin.GetNightChance();

                switch (nightType)
                {
                    case 0:
                        var chanceLewd = _rnd.Next(0, 100);
                        //CustomGameBalancePlugin.Log.LogInfo($"Mod Chance {visitType[0]}%");

                        if (chanceLewd <= visitType[0])
                        {
                            for (int idx = 0; idx < 3; idx++)
                            {
                                for (int i = 0; i < nightEvent.table[idx].BaseRates.Count; i++)
                                {
                                    nightEvent.table[idx].BaseRates[i] = idx is 2 ? 100f : 0f;
                                }
                                for (int i = 0; i < nightEvent.table[idx].Rates.Count; i++)
                                {
                                    nightEvent.table[idx].Rates[i] = idx is 2 ? 1f : 0f;
                                }
                            }
                            return;
                        }                    
                        break;
                    case 1:
                        var chanceNormal = _rnd.Next(0, 100);
                        //CustomGameBalancePlugin.Log.LogInfo($"Mod Chance {visitType[1]}%");

                        if (chanceNormal <= visitType[1])
                        {
                            for (int idx = 0; idx < 3; idx++)
                            {
                                for (int i = 0; i < nightEvent.table[idx].BaseRates.Count; i++)
                                {
                                    nightEvent.table[idx].BaseRates[i] = idx is 1 ? 100f : 0f;
                                }
                                for (int i = 0; i < nightEvent.table[idx].Rates.Count; i++)
                                {
                                    nightEvent.table[idx].Rates[i] = idx is 1 ? 1f : 0f;
                                }
                            }
                            return;
                        }                          
                        break;
                }  
            }

            for (int idx = 0; idx < 3; idx++)
            {
                for (int i = 0; i < nightEvent.table[idx].BaseRates.Count; i++)
                {
                    nightEvent.table[idx].BaseRates[i] = nightTable[idx].BaseRates[i];
                }
                for (int i = 0; i < nightEvent.table[idx].Rates.Count; i++)
                {
                    nightEvent.table[idx].Rates[i] = nightTable[idx].Rates[i];
                }
            }
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
        public static void ThreesomeNPCAskCondition(SVThinking thinking, int rate)
        {
            if (thinking == null) return;
            if (thinking._charaCtrl == null) return;
            if (thinking._charaCtrl.ai == null) return;
            if (thinking._charaCtrl.ai._charaData == null) return;
            if (thinking._charaCtrl.ai._charaData.charasGameParam.commandNo < 0 || thinking._charaCtrl.ai._charaData.charasGameParam.commandNo > 100) return;
            if (thinking._charaCtrl.ai._charaData.charasGameParam.commandNo == 77) return;
            if (thinking._charaCtrl.ai.charaData.gameParameter.individuality.answer.Contains(29))
            {
                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Character has Singleminded trait, can not ask for 3P");
                return;
            }

            if (thinking._charaCtrl.target.kind == BehaviourController.TargetInfo.TargetKind.Chara)
            {
                if (thinking._charaCtrl.ai._charaData.gameParameter.LvChastity > 2)
                {
                    if (thinking._charaCtrl.ai._charaData.gameParameter.isVirgin) return;
                }

                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"**********************");
                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Init NPC asking for 3P");
                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"**********************");

                bool askForThreeP = false;
                int targetID = thinking._charaCtrl.target.id;
                int commandID = thinking._charaCtrl.AI._charaData.CommandNo;

                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Original commandNo: {commandID}");
                
                int askRate = 0;
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
                    if (rate > chance)
                    {
                        askForThreeP = true;
                    }
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
            }
        }
        private static void SelectThreesomePartner(Actor chara, Actor targetActor, int sexType, int virtueLV)
        {
            if (chara.parameter.sex == 0 && targetActor.parameter.sex == 0 && sexType == 0) return;

            Dictionary<int, int> charaWeights = new Dictionary<int, int>();
            var targetID = targetActor.charasGameParam.Index;
            int summedWeights = 0;
            //bool isFuta = false;

            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Selecting 3P Partner: sex type {sexType}");
            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Chara 1: {chara.charasGameParam.Index} sex: {chara.parameter.sex} virtue: {virtueLV}");
            if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Chara 2: {targetID} sex: {targetActor.parameter.sex}");

            if (sexType == -1)
            {
                if (chara.parameter.sex == 0 && targetActor.parameter.sex == 0) sexType = 1;
                if (chara.parameter.sex == 1 && targetActor.parameter.sex == 1)
                {
                    //if (!targetActor.parameter.isFutanari && !_actor.parameter.isFutanari) isFuta = true;
                    sexType = 0;
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
                            if (ThirdCharacter.charaBase == null) continue;
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

            //Check if it has the same lover
            if (targetActor.charasGameParam.memory.lovers.Count > 0)
            {
                foreach (var lover in targetActor.charasGameParam.memory.lovers)
                {
                    if (charaWeights.ContainsKey(lover.id))
                    {
                        if (Game.Charas.TryGetValue(lover.id, out Actor ThirdCharacter))
                        {
                            if (ThirdCharacter.charaBase == null) continue;
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

            //Check favorability points
            if (chara.charasGameParam.sensitivity.tableFavorabiliry.Count > 0)
            {
                foreach (var charaFavor in chara.charasGameParam.sensitivity.tableFavorabiliry)
                {
                    if (Game.Charas.TryGetValue(charaFavor.Key, out Actor thatCharacter))
                    {
                        if (thatCharacter.charaBase == null) continue;
                        if (thatCharacter.charasGameParam.Index != targetID && !thatCharacter.IsPC)
                        {
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
                                                    if (charaFavor.Value.longSensitivityCounts[0] > 20) charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[0];
                                                    charaWeights[charaFavor.Key] -= charaFavor.Value.longSensitivityCounts[3];
                                                }
                                                else
                                                {
                                                    charaWeights.Add(charaFavor.Key, 0);
                                                    if (charaFavor.Value.longSensitivityCounts[0] > 20) charaWeights[charaFavor.Key] += charaFavor.Value.longSensitivityCounts[0];
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
                                //if (isFuta && (thatCharacter.parameter.sex == 1 && !thatCharacter.parameter.isFutanari)) continue;
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
        public static bool ThreesomeConfirmation(Actor chara, bool result)
        {
            if (chara.charasGameParam.commandNo == 77)
            {
                if (Game.Charas.TryGetValue(chara.charasGameParam.thatPersonCharaArrayIndex, out var thatChara))
                {
                    if (thatChara.charaBase != null)
                    {
                        if (thatChara.charaBase.BehaviourCtrl != null)
                        {
                            if (MapManager.Instance.MapListTable.ContainsKey(thatChara.charaBase.BehaviourCtrl.nowMapID))
                            {
                                if (MapManager.Instance.MapListTable[thatChara.charaBase.BehaviourCtrl.nowMapID].Kind == 1) 
                                {
                                    if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"3P Target is on a Private Room");
                                    chara.charasGameParam.commandNo = 39;
                                }
                            }
                            else chara.charasGameParam.commandNo = 39;
                        }
                        else chara.charasGameParam.commandNo = 39;
                    }
                    else chara.charasGameParam.commandNo = 39;
                }
                else chara.charasGameParam.commandNo = 39;
            }
            return result;
        }
        public static bool CanJoinThreesome(Actor chara, Actor actor1, Actor actor2, bool isADV, int rate)
        {
            if (chara == null || actor1 == null || actor2 == null) return false;
            if (chara.gameParameter.individuality.answer.Contains(29) || actor1.gameParameter.individuality.answer.Contains(29) || actor2.gameParameter.individuality.answer.Contains(29))
            {
                if (CustomGameBalancePlugin.GetShowLog()) CustomGameBalancePlugin.Log.LogInfo($"Character can not join 3P. 1 or more characters have the Singleminded trait, 3P not possible");
                return false;
            }

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

            switch (virtue)
            {
                case 0:
                    return true;
                case 1:
                    if (chara.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(partner1ID) && chara.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(partner2ID))
                    {
                        if (chara.charasGameParam.sensitivity.tableFavorabiliry[partner1ID].mostRanks[0] < 2 && chara.charasGameParam.sensitivity.tableFavorabiliry[partner2ID].mostRanks[0] < 2) return true;
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
    }
}
