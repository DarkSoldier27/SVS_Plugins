using ADV;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Manager;
using SaveData;
using SV;
using System;
using System.Collections.Generic;

namespace SVS_MoreActions
{
    public class MoreActions
    {       
        private static Dictionary<int, MoreActionsParam.ActionParam> actionParamList = new();
        public static Dictionary<int,ValueTuple<float,int>> ActionJudgeAnswers = new();
        public static void AddActionFromList(CommandUI commandMenu)
        {
            var actionList = MoreActionsParam.DeserializeAction();
            if (actionList != null || actionList.Count > 0)
            {
                int actionCount = 53 + actionList.Count;

                Il2CppStructArray<SimulationDefine.CommandNo> nos = new Il2CppStructArray<SimulationDefine.CommandNo>(actionCount);
                for (int i = 0; i < commandMenu._commandNos.Count; i++)
                {
                    nos[i] = commandMenu._commandNos[i];
                }
                for (int i = 53; i < nos.Count; i++)
                {
                    nos[i] = SimulationDefine.CommandNo.Continue;
                }
                int index = 53;
                foreach (var actParam in actionList)
                {
                    if (actParam.CategoryID < MoreActionUI.commandNameList.Count)
                    {
                        MoreActionUI.CreateNewActionOnMenu(commandMenu, actParam);
                        AddActionCost(actParam.ActionID, actParam.ActionTimeCost);
                        AddActionJudge(actParam.CategoryID, actParam.ActionID, actParam.BaseJudgeRates, actParam.JudgeRates);
                        AddFavorPoints(actParam);
                        AddMoodPoints(actParam);
                        if (!actionParamList.ContainsKey(actParam.ActionID)) actionParamList.Add(actParam.ActionID, actParam);
                        if (Enum.IsDefined(typeof(SimulationDefine.CommandNo), actParam.ActionID))
                        {
                            if (index < nos.Length) nos[index] = (SimulationDefine.CommandNo)actParam.ActionID;
                        }
                        if (MoreActionsPlugin.GetShowLog()) MoreActionsPlugin.Log.LogInfo($"Action created: {actParam.ActionID} - {actParam.ActionName}");
                    }
                    index++;
                }
                MoreActionUI.SetLewdActions(commandMenu);
                commandMenu._commandNos = nos;
            }
        }         
        private static Il2CppSystem.Collections.Generic.List<float> SetJudgeBaseRates(Il2CppSystem.Collections.Generic.List<float> defaultBaseRates, List<float> judgeBaseRates)
        {
            Il2CppSystem.Collections.Generic.List<float> baseRates = new();
            foreach (var baseValue in defaultBaseRates)
            {
                baseRates.Add(baseValue);
            }
            for (int i = 0; i < judgeBaseRates.Count; i++)
            {
                if (i >= baseRates.Count) break;
                baseRates[i] = judgeBaseRates[i];
            }
            return baseRates;
        }
        private static Il2CppSystem.Collections.Generic.List<float> SetJudgeRates(Il2CppSystem.Collections.Generic.List<float> defaultRates, List<float> judgeRates)
        {
            Il2CppSystem.Collections.Generic.List<float> rates = new();
            foreach (var ratesValue in defaultRates)
            {
                rates.Add(ratesValue);
            }
            for (int i = 0; i < judgeRates.Count; i++)
            {
                if (i >= rates.Count) break;
                rates[i] = judgeRates[i];
            }
            return rates;
        }
        public static void AddActionJudge(int categoryID, int actionID, List<float> judgeBaseRates, List<float> judgeRates)
        {
            if (YesNoJudgeManager.Instance is null) return;
            Il2CppSystem.Collections.Generic.List<float> baseRates = new();
            Il2CppSystem.Collections.Generic.List<float> rates = new();
            switch (categoryID)
            {
                case 0://Talk
                    if (YesNoJudgeManager.Instance.talkAnswer.table.ContainsKey(actionID)) return;                
                    YesNoJudgeManager.Instance.talkAnswer.table.Add(actionID, new AnswerBaseDataParam()
                    {
                        ID = actionID,
                        BaseRates = SetJudgeBaseRates(YesNoJudgeManager.Instance.talkAnswer.table[0].BaseRates, judgeBaseRates),
                        Rates = SetJudgeRates(YesNoJudgeManager.Instance.talkAnswer.table[0].Rates, judgeRates)
                    });
                    break;
                case 1://Tell
                    if (YesNoJudgeManager.Instance.tellAnswer.table.ContainsKey(actionID)) return;
                    YesNoJudgeManager.Instance.tellAnswer.table.Add(actionID, new AnswerBaseDataParam()
                    {
                        ID = actionID,
                        BaseRates = SetJudgeBaseRates(YesNoJudgeManager.Instance.tellAnswer.table[3].BaseRates, judgeBaseRates),
                        Rates = SetJudgeRates(YesNoJudgeManager.Instance.tellAnswer.table[3].Rates, judgeRates)
                    });
                    break;
                case 2://Caution
                    if (YesNoJudgeManager.Instance.cautionAnswer.table.ContainsKey(actionID)) return;
                    YesNoJudgeManager.Instance.cautionAnswer.table.Add(actionID, new AnswerBaseDataParam()
                    {
                        ID = actionID,
                        BaseRates = SetJudgeBaseRates(YesNoJudgeManager.Instance.cautionAnswer.table[10].BaseRates, judgeBaseRates),
                        Rates = SetJudgeRates(YesNoJudgeManager.Instance.cautionAnswer.table[10].Rates, judgeRates)
                    });
                    break;
                case 3://Consultation
                    if (YesNoJudgeManager.Instance.consultationAnswer.table.ContainsKey(actionID)) return;
                    YesNoJudgeManager.Instance.consultationAnswer.table.Add(actionID, new AnswerBaseDataParam()
                    {
                        ID = actionID,
                        BaseRates = SetJudgeBaseRates(YesNoJudgeManager.Instance.consultationAnswer.table[87].BaseRates, judgeBaseRates),
                        Rates = SetJudgeRates(YesNoJudgeManager.Instance.consultationAnswer.table[87].Rates, judgeRates)
                    });
                    break;
                case 4://ThatAnswer
                    if (YesNoJudgeManager.Instance.thatAnswer.table.ContainsKey(actionID)) return;
                    YesNoJudgeManager.Instance.thatAnswer.table.Add(actionID, new AnswerBaseDataParam()
                    {
                        ID = actionID,
                        BaseRates = SetJudgeBaseRates(YesNoJudgeManager.Instance.thatAnswer.table[17].BaseRates, judgeBaseRates),
                        Rates = SetJudgeRates(YesNoJudgeManager.Instance.thatAnswer.table[17].Rates, judgeRates)
                    });
                    break;
                case 5://WithAction
                    if (YesNoJudgeManager.Instance.withAnswer.table.ContainsKey(actionID)) return;
                    YesNoJudgeManager.Instance.withAnswer.table.Add(actionID, new AnswerBaseDataParam()
                    {
                        ID = actionID,
                        BaseRates = SetJudgeBaseRates(YesNoJudgeManager.Instance.withAnswer.table[22].BaseRates, judgeBaseRates),
                        Rates = SetJudgeRates(YesNoJudgeManager.Instance.withAnswer.table[22].Rates, judgeRates)
                    });
                    break;
                case 6://Request
                    if (YesNoJudgeManager.Instance.requestAnswer.table.ContainsKey(actionID)) return;
                    YesNoJudgeManager.Instance.requestAnswer.table.Add(actionID, new AnswerBaseDataParam()
                    {
                        ID = actionID,
                        BaseRates = SetJudgeBaseRates(YesNoJudgeManager.Instance.requestAnswer.table[27].BaseRates, judgeBaseRates),
                        Rates = SetJudgeRates(YesNoJudgeManager.Instance.requestAnswer.table[27].Rates, judgeRates)
                    });
                    break;
                case 7://Skinship
                    if (YesNoJudgeManager.Instance.skinshipAnswer.table.ContainsKey(actionID)) return;
                    YesNoJudgeManager.Instance.skinshipAnswer.table.Add(actionID, new AnswerBaseDataParam()
                    {
                        ID = actionID,
                        BaseRates = SetJudgeBaseRates(YesNoJudgeManager.Instance.skinshipAnswer.table[31].BaseRates, judgeBaseRates),
                        Rates = SetJudgeRates(YesNoJudgeManager.Instance.skinshipAnswer.table[31].Rates, judgeRates)
                    });
                    break;
                case 8://MoveAnswer
                    if (YesNoJudgeManager.Instance.moveAnswer.table.ContainsKey(actionID)) return;                    
                    YesNoJudgeManager.Instance.moveAnswer.table.Add(actionID, new AnswerBaseDataParam()
                    {
                        ID = actionID,
                        BaseRates = SetJudgeBaseRates(YesNoJudgeManager.Instance.moveAnswer.table[38].BaseRates, judgeBaseRates),
                        Rates = SetJudgeRates(YesNoJudgeManager.Instance.moveAnswer.table[38].Rates, judgeRates)
                    });
                    break;
                case 9://Pressure
                    if (YesNoJudgeManager.Instance.pressureAnswer.table.ContainsKey(actionID)) return;
                    YesNoJudgeManager.Instance.pressureAnswer.table.Add(actionID, new AnswerBaseDataParam()
                    {
                        ID = actionID,
                        BaseRates = SetJudgeBaseRates(YesNoJudgeManager.Instance.pressureAnswer.table[80].BaseRates, judgeBaseRates),
                        Rates = SetJudgeRates(YesNoJudgeManager.Instance.pressureAnswer.table[80].Rates, judgeRates)
                    });
                    break;
                case 10:
                    if (YesNoJudgeManager.Instance.talkAnswer.table.ContainsKey(actionID)) return;
                    YesNoJudgeManager.Instance.talkAnswer.table.Add(actionID, new AnswerBaseDataParam()
                    {
                        ID = actionID,
                        BaseRates = SetJudgeBaseRates(YesNoJudgeManager.Instance.talkAnswer.table[0].BaseRates, judgeBaseRates),
                        Rates = SetJudgeRates(YesNoJudgeManager.Instance.talkAnswer.table[0].Rates, judgeRates)
                    });
                    break;
                case 11://EveryOne
                    if (YesNoJudgeManager.Instance.everyoneAnswer.table.ContainsKey(actionID)) return;
                    YesNoJudgeManager.Instance.everyoneAnswer.table.Add(actionID, new AnswerBaseDataParam()
                    {
                        ID = actionID,
                        BaseRates = SetJudgeBaseRates(YesNoJudgeManager.Instance.everyoneAnswer.table[40].BaseRates, judgeBaseRates),
                        Rates = SetJudgeRates(YesNoJudgeManager.Instance.everyoneAnswer.table[40].Rates, judgeRates)
                    });
                    break;
                default:
                    break;
            }
        }
        public static void AddActionCost(int actionID, int cost)
        {
            if (!GlobalListLoad.Instance.costTable.ContainsKey(actionID))
            {
                GlobalListLoad.Instance.costTable.Add(actionID, new CommandCostDataParam() { ID = actionID, Cost = cost });
            } 
        }
        public static void AddFavorPoints(MoreActionsParam.ActionParam actParam)
        {
            if (FavourableImpressionManager.Instance is null) return;
            if (actParam.FavorMoodParam != null)
            {
                foreach (var favorability in actParam.FavorMoodParam.Values)
                {
                    if (!FavourableImpressionManager.Instance.normalTable.ContainsKey(favorability.ID))
                    {
                        Il2CppSystem.Collections.Generic.List<FavourableImpressionDataFavourable> dataFavourables = new();                      
                        foreach (var favor in favorability.Favors.Values)
                        {
                            Il2CppSystem.Collections.Generic.List<int> favors = new();
                            foreach (var fav in favor)
                            {
                                favors.Add(fav);
                            }
                            dataFavourables.Add(new FavourableImpressionDataFavourable()
                            {
                                Counts = favors
                            });
                        }
                        FavourableImpressionManager.Instance.normalTable.Add(favorability.ID, new FavourableImpressionDataParam()
                        {
                            ID = favorability.ID,
                            Favourables = dataFavourables
                        });
                    }                  
                }               
            }
        }
        public static void AddMoodPoints(MoreActionsParam.ActionParam actParam)
        {
            if (ConditionManager.Instance is null) return;
            if (actParam.FavorMoodParam != null)
            {
                foreach (var favorMood in actParam.FavorMoodParam.Values)
                {
                    if (favorMood.Moods is null) continue;
                    if (!ConditionManager.Instance.normalTable.ContainsKey(favorMood.ID))
                    {
                        Il2CppSystem.Collections.Generic.List<StateDataState> dataFavourables = new();
                        foreach (var moodAnswers in favorMood.Moods.Values)
                        {
                            Il2CppSystem.Collections.Generic.List<int> favors = new();
                            foreach (var states in moodAnswers)
                            {
                                favors.Add(states);
                            }
                            dataFavourables.Add(new StateDataState()
                            {
                                Counts = favors
                            });
                        }
                        ConditionManager.Instance.normalTable.Add(favorMood.ID, new StateDataParam()
                        {
                            ID = favorMood.ID,
                            States = dataFavourables
                        });
                    }
                }
            }
        }
        public static void JudgeAction(int commandID)
        {
            var advScene = ADVManager.Instance.gameObject.GetComponentInChildren<ADVScene>();
            if (advScene != null)
            {
                if (advScene._scenario != null)
                {
                    if (ActionJudgeAnswers.ContainsKey(commandID))
                    {
                        if (MoreActionsPlugin.GetShowLog()) MoreActionsPlugin.Log.LogInfo($"Judge System: CommandID:{commandID} - Rate: {ActionJudgeAnswers[commandID].Item1} - Answer type: {ActionJudgeAnswers[commandID].Item2}");
                        advScene._scenario.CurrentHeroine.charasGameParam.judgeRate = (int)ActionJudgeAnswers[commandID].Item1;
                        advScene._scenario.CurrentHeroine.charasGameParam.judgeAnswer = ActionJudgeAnswers[commandID].Item2;
                    }
                    else 
                    {
                        advScene._scenario.CurrentHeroine.charasGameParam.judgeRate = 100;
                        advScene._scenario.CurrentHeroine.charasGameParam.judgeAnswer = 0;
                    }                  
                }
            }
        }
        public static int SetActionFavorMood(NormalConditionFavourable normCondFav, int valueID)
        {
            if (MoreActionsPlugin.GetShowLog()) MoreActionsPlugin.Log.LogInfo($"Set Favor and mood: CommandID:{normCondFav.commandNo} - Answer type: {normCondFav.answer}");
            if (!Enum.IsDefined(typeof(SimulationDefine.CommandNo), normCondFav.commandNo))
            {
                if (actionParamList.Count > 0)
                {
                    if (actionParamList.ContainsKey(normCondFav.commandNo))
                    {
                        if (actionParamList[normCondFav.commandNo].FavorMoodParam.ContainsKey(normCondFav.answer))
                        {
                            if (actionParamList[normCondFav.commandNo].FavorMoodParam != null)
                            {
                                if (actionParamList[normCondFav.commandNo].FavorMoodParam.ContainsKey(normCondFav.answer)) return actionParamList[normCondFav.commandNo].FavorMoodParam[normCondFav.answer].ID;
                            } 
                        }
                    }
                }
            }
            return valueID;
        }
        public static void OpenActionMenu(CommandUI commandMenu, Actor chara)
        {
            if (actionParamList.Count == 0) return;
            foreach (var actParam in actionParamList)
            {
                if (actParam.Value.AdvanceOptions != null) ActionButtonDisplay(commandMenu, chara, actParam.Value);
                if (actParam.Value.BaseJudgeRates.Count == 7) GetActionJudgeAnswer(actParam.Value.ActionID);
            }
        }
        public static void GetActionJudgeAnswer(int actionID)
        {
            YesNoJudgeManager.Instance.EachJudgment(actionID, 0);
            float answerRate = YesNoJudgeManager.Instance.ansInfo.rate;
            int answerType = YesNoJudgeManager.Instance.ansInfo.ans;
            if (answerRate == 0 & answerType == 0) answerRate = 100;
            ValueTuple<float, int> rateAnswer;
            rateAnswer.Item1 = answerRate;
            rateAnswer.Item2 = answerType;
            if (!ActionJudgeAnswers.ContainsKey(actionID)) ActionJudgeAnswers.Add(actionID, rateAnswer);
            else if (ActionJudgeAnswers.ContainsKey(actionID)) ActionJudgeAnswers[actionID] = rateAnswer;
        }
        public static void ActionButtonDisplay(CommandUI commandMenu, Actor targetChara, MoreActionsParam.ActionParam actParam)
        {
            if (actParam.AdvanceOptions != null)
            {
                if (actParam.AdvanceOptions.IsButtonHidden)
                {
                    if (ActionButtonConditions(actParam, targetChara))
                    {
                        ShowButton(commandMenu, actParam, true);
                    }
                    else
                    {
                        ShowButton(commandMenu, actParam, false);
                    }
                }
            }

        }
        private static void ShowButton(CommandUI commandMenu, MoreActionsParam.ActionParam actParam, bool IsShow)
        {
            string categoryName = MoreActionUI.commandNameList[actParam.CategoryID];
            var commandsRectTrans = commandMenu._commandList;
            var category = commandsRectTrans.Find(categoryName);
            var selectionList = category.Find("GameObject");
            if (selectionList != null)
            {
                var actionObj = selectionList.Find(actParam.ActionName);
                if (actionObj != null) actionObj.gameObject.SetActive(IsShow);
            }
        }
        private static bool ActionButtonConditions(MoreActionsParam.ActionParam actParam, Actor targetChara)
        {
            var charaPC = GameChara.Player;
            int trueCount = 0;
            if (MoreActionsPlugin.GetShowLog()) MoreActionsPlugin.Log.LogInfo($"Action {actParam.ActionID}_{actParam.ActionName} Conditions {actParam.AdvanceOptions.ShowActionConditions.Count} - Meet All: {actParam.AdvanceOptions.NeedAllConditions}");
            foreach (var condition in actParam.AdvanceOptions.ShowActionConditions)
            {
                if (MoreActionsPlugin.GetShowLog()) MoreActionsPlugin.Log.LogInfo($"Checking Condition: {condition.Key}");
                string keyValue = condition.Key.ToLower();
                switch (keyValue)
                {
                    case "trait":
                        if (condition.Value.Count > 1)
                        {
                            int trait = int.TryParse(condition.Value[1], out int value) ? value : -2;
                            if (condition.Value[0].ToLower() == "pc" && charaPC.gameParameter.individuality.answer.Contains(trait)) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && targetChara.gameParameter.individuality.answer.Contains(trait)) trueCount++;
                        }
                        break;
                    case "htrait":
                        if (condition.Value.Count > 1)
                        {
                            int trait = int.TryParse(condition.Value[1], out int value) ? value : -2;
                            if (condition.Value[0].ToLower() == "pc" && charaPC.gameParameter.preferenceH.answer.Contains(trait)) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && targetChara.gameParameter.preferenceH.answer.Contains(trait)) trueCount++;
                        }
                        break;
                    case "sex":
                        if (condition.Value.Count > 1)
                        {
                            int sex = int.TryParse(condition.Value[1], out int value) ? value : -1;
                            if (condition.Value[0].ToLower() == "pc" && charaPC.parameter.sex == sex) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && targetChara.parameter.sex == sex) trueCount++;
                        }
                        break;
                    case "time":
                        if (condition.Value.Count > 1)
                        {
                            int weekDay = -1;
                            int period = -1;
                            int gameWeekDay = Game.saveData.Week;
                            if (int.TryParse(condition.Value[0], out int day))
                            {
                                weekDay = day;
                            }
                            else
                            {
                                switch (condition.Value[0].ToLower())
                                {
                                    case "monday": 
                                        weekDay = 0;
                                        break;
                                    case "tuesday":
                                        weekDay = 1;
                                        break;
                                    case "wesnesday":
                                        weekDay = 2;
                                        break;
                                    case "thursday":
                                        weekDay = 3;
                                        break;
                                    case "friday":
                                        weekDay = 4;
                                        break;
                                    case "saturday":
                                        weekDay = 5;
                                        break;
                                    case "sunday":
                                        weekDay = 6;
                                        break;
                                }
                            }
                            if (int.TryParse(condition.Value[1], out int timePeriod))
                            {
                                period = timePeriod;
                            }
                            else
                            {
                                switch (condition.Value[1].ToLower())
                                {
                                    case "morning":
                                        period = 0;
                                        break;
                                    case "midday":
                                        period = 1;
                                        break;
                                    case "evening":
                                        period = 2;
                                        break;
                                    case "night":
                                        period = 3;
                                        break;
                                }
                            }
                            if (gameWeekDay == weekDay && period == GetTimeOfDay()) trueCount++;
                        }
                        break;
                    case "love":
                        if (condition.Value.Count > 2)
                        {
                            int checkValue = int.TryParse(condition.Value[2], out int value) ? value : 0;
                            if (condition.Value[0].ToLower() == "pc" && GetFavorability(charaPC, condition.Value[1], targetChara.charasGameParam.Index, 0, checkValue)) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && GetFavorability(targetChara, condition.Value[1], charaPC.charasGameParam.Index, 0, checkValue)) trueCount++;
                        }
                        break;
                    case "friend":
                        if (condition.Value.Count > 2)
                        {
                            int checkValue = int.TryParse(condition.Value[2], out int value) ? value : 0;
                            if (condition.Value[0].ToLower() == "pc" && GetFavorability(charaPC, condition.Value[1], targetChara.charasGameParam.Index, 1, checkValue)) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && GetFavorability(targetChara, condition.Value[1], charaPC.charasGameParam.Index, 1, checkValue)) trueCount++;
                        }
                        break;
                    case "indifferent":
                    case "distant":
                        if (condition.Value.Count > 2)
                        {
                            int checkValue = int.TryParse(condition.Value[2], out int value) ? value : 0;
                            if (condition.Value[0].ToLower() == "pc" && GetFavorability(charaPC, condition.Value[1], targetChara.charasGameParam.Index, 2, checkValue)) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && GetFavorability(targetChara, condition.Value[1], charaPC.charasGameParam.Index, 2, checkValue)) trueCount++;
                        }
                        break;
                    case "hate":
                        if (condition.Value.Count > 2)
                        {
                            int checkValue = int.TryParse(condition.Value[2], out int value) ? value : 0;
                            if (condition.Value[0].ToLower() == "pc" && GetFavorability(charaPC, condition.Value[1], targetChara.charasGameParam.Index, 3, checkValue)) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && GetFavorability(targetChara, condition.Value[1], charaPC.charasGameParam.Index, 3, checkValue)) trueCount++;
                        }
                        break;
                    case "virtue":
                    case "chastity":
                        if (condition.Value.Count > 1)
                        {
                            int virtue = int.TryParse(condition.Value[1], out int value) ? value : -1;
                            if (condition.Value[0].ToLower() == "pc" && charaPC.gameParameter.LvChastity == virtue) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && targetChara.gameParameter.LvChastity == virtue) trueCount++;
                        }
                        break;
                    case "job":
                        if (condition.Value.Count > 1)
                        {
                            int jobID = int.TryParse(condition.Value[1], out int value) ? value : -1;
                            if (condition.Value[0].ToLower() == "pc" && charaPC.Job == jobID) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && targetChara.Job == jobID) trueCount++;
                        }
                        break;
                    case "personality":
                        if (condition.Value.Count > 1)
                        {
                            int perID = int.TryParse(condition.Value[1], out int value) ? value : -1;
                            if (condition.Value[0].ToLower() == "pc" && charaPC.personality == perID) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && targetChara.personality == perID) trueCount++;
                        }
                        break;
                    case "sexualtarget":
                        if (condition.Value.Count > 1)
                        {
                            int sexTarget = int.TryParse(condition.Value[1], out int value) ? value : -1;
                            if (condition.Value[0].ToLower() == "pc" && charaPC.gameParameter.sexualTarget == sexTarget) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && targetChara.gameParameter.sexualTarget == sexTarget) trueCount++;
                        }
                        break;
                    case "sociability":
                        if (condition.Value.Count > 1)
                        {
                            int charaSocial = int.TryParse(condition.Value[1], out int value) ? value : -1;
                            if (condition.Value[0].ToLower() == "pc" && charaPC.gameParameter.LvSociability == charaSocial) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && targetChara.gameParameter.sexualTarget == charaSocial) trueCount++;
                        }
                        break;
                    case "fight":
                    case "insult":
                        if (condition.Value.Count > 1)
                        {
                            int fightStyle = int.TryParse(condition.Value[1], out int value) ? value : -1;
                            if (condition.Value[0].ToLower() == "pc" && charaPC.gameParameter.LvDefeat == fightStyle) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && targetChara.gameParameter.LvDefeat == fightStyle) trueCount++;
                        }
                        break;
                    case "virgin":
                        if (condition.Value.Count > 1)
                        {
                            bool virgin = bool.Parse(condition.Value[1]);
                            if (condition.Value[0].ToLower() == "pc" && charaPC.gameParameter.isVirgin == virgin) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && targetChara.gameParameter.isVirgin == virgin) trueCount++;
                        }
                        break;
                    case "analvirgin":
                        if (condition.Value.Count > 1)
                        {
                            bool virgin = bool.Parse(condition.Value[1]);
                            if (condition.Value[0].ToLower() == "pc" && charaPC.gameParameter.isAnalVirgin == virgin) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && targetChara.gameParameter.isAnalVirgin == virgin) trueCount++;
                        }
                        break;
                    case "malevirgin":
                        if (condition.Value.Count > 1)
                        {
                            bool virgin = bool.Parse(condition.Value[1]);
                            if (condition.Value[0].ToLower() == "pc" && charaPC.gameParameter.isMaleVirgin == virgin) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && targetChara.gameParameter.isMaleVirgin == virgin) trueCount++;
                        }
                        break;
                    case "maleanalvirgin":
                        if (condition.Value.Count > 1)
                        {
                            bool virgin = bool.Parse(condition.Value[1]);
                            if (condition.Value[0].ToLower() == "pc" && charaPC.gameParameter.isMaleAnalVirgin == virgin) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && targetChara.gameParameter.isMaleAnalVirgin == virgin) trueCount++;
                        }
                        break;
                    case "statconversation":
                    case "statspeech":
                        if (condition.Value.Count > 2)
                        {
                            int checkValue = int.TryParse(condition.Value[1], out int value) ? value : -1;
                            if (condition.Value[0].ToLower() == "pc" && CompareValues(condition.Value[1], charaPC.charasGameParam.baseParameter.Conversation, checkValue)) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && CompareValues(condition.Value[1], targetChara.charasGameParam.baseParameter.Conversation, checkValue)) trueCount++;
                        }
                        break;
                    case "statstamina":
                    case "statstrength":
                        if (condition.Value.Count > 2)
                        {
                            int checkValue = int.TryParse(condition.Value[1], out int value) ? value : -1;
                            if (condition.Value[0].ToLower() == "pc" && CompareValues(condition.Value[1], charaPC.charasGameParam.baseParameter.Conversation, checkValue)) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && CompareValues(condition.Value[1], targetChara.charasGameParam.baseParameter.Conversation, checkValue)) trueCount++;
                        }
                        break;
                    case "statstudy":
                        if (condition.Value.Count > 2)
                        {
                            int checkValue = int.TryParse(condition.Value[1], out int value) ? value : -1;
                            if (condition.Value[0].ToLower() == "pc" && CompareValues(condition.Value[1], charaPC.charasGameParam.baseParameter.Study, checkValue)) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && CompareValues(condition.Value[1], targetChara.charasGameParam.baseParameter.Study, checkValue)) trueCount++;
                        }
                        break;
                    case "statliving":
                        if (condition.Value.Count > 2)
                        {
                            int checkValue = int.TryParse(condition.Value[1], out int value) ? value : -1;
                            if (condition.Value[0].ToLower() == "pc" && CompareValues(condition.Value[1], charaPC.charasGameParam.baseParameter.Living, checkValue)) trueCount++;
                            if (condition.Value[0].ToLower() == "npc" && CompareValues(condition.Value[1], targetChara.charasGameParam.baseParameter.Living, checkValue)) trueCount++;
                        }
                        break;
                    case "islover":
                        if (charaPC.charasGameParam.memory.IsLoversContainsID(targetChara.charasGameParam.Index)) trueCount++;
                        break;
                }
                if (!actParam.AdvanceOptions.NeedAllConditions && trueCount > 0) return true;
            }
            if (trueCount == actParam.AdvanceOptions.ShowActionConditions.Count) return true;
            return false;
        }

        private static bool GetFavorability(Actor chara, string compareOperator, int charaID, int type, int checkValue)
        {
            if (type < 0 || type > 3) return false;
            if (chara.charasGameParam.sensitivity.tableFavorabiliry.ContainsKey(charaID))
            {
                int favorValue = chara.charasGameParam.sensitivity.tableFavorabiliry[charaID].longSensitivityCounts[type];
                return CompareValues(compareOperator, favorValue, checkValue);
            }
            return false;
        }
        private static bool CompareValues(string textOperator, float valueOne, float valuetwo)
        {
            switch (textOperator)
            {
                case "<":
                    if (valueOne < valuetwo) return true;
                    break;
                case "<=":
                    if (valueOne <= valuetwo) return true;
                    break;
                case "=":
                    if (valueOne == valuetwo) return true;
                    break;
                case "!=":
                    if (valueOne != valuetwo) return true;
                    break;
                case ">=":
                    if (valueOne >= valuetwo) return true;
                    break;
                case ">":
                    if (valueOne > valuetwo) return true;
                    break;
            }
            return false;
        }
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
    }
}
