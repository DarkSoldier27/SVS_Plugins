using ADV;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SV;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using TMPro;
using UnityEngine;
using static Character.HumanDataGameParameter_SV;

namespace SVS_MoreActions
{
    internal class MoreActions
    {
        private static GameObject commandZeroObj;
        private static GameObject commandTwoObj;
        private static Dictionary<int, MoreActionsParam.ActionParam> actionParamList = new();
        private static List<string> commandNameList = new List<string>() 
        {
            "Let's talk",
            "talk",
            "Make a statement",
            "Consult",
            "That person",
            "Let's be together",
            "Ask",
            "Skinship",
            "Ask to move",
            "weakness",
            "other"
        };
        private static List<GameObject> adultsBtns = new List<GameObject>();
        public static void AddActionIntoCommandMenu(CommandUI commandMenu)
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
                    if (actParam.CategoryID < commandNameList.Count)
                    {
                        CreateNewActionOnMenu(commandMenu, actParam);
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
                SetLewdActions(commandMenu);
                commandMenu._commandNos = nos;
            }
        }
        private static void CreateNewActionOnMenu(CommandUI commandMenu, MoreActionsParam.ActionParam actParam)
        {
            if (commandMenu == null) return;
            string categoryName = commandNameList[actParam.CategoryID];
            if (commandZeroObj == null)
            {
                var commandZeroRectTrans = commandMenu._commandList;
                var categoryZero = commandZeroRectTrans.Find(commandNameList[0]);
                if (categoryZero != null)
                {
                    var commandZero = categoryZero.Find("GameObject/btnCommand000");
                    var commandTwo = categoryZero.Find("GameObject/btnCommand002");
                    commandZeroObj = commandZero.gameObject;
                    commandTwoObj = commandTwo.gameObject;
                }
            }
            var commandsRectTrans = commandMenu._commandList;
            var category = commandsRectTrans.Find(categoryName);
            if (category == null)
            {
                CreateNewCategory(commandsRectTrans.gameObject, categoryName);
            }

            var selectionList = category.Find("GameObject");
            if (selectionList != null)
            {
                CreateNewActionButton(commandMenu, selectionList, actParam.ActionName, actParam.ActionID, actParam.IsLewdAction);
                AdjustActionSelection(selectionList, actParam.CategoryID);
            }

        }
        private static void SetLewdActions(CommandUI commandMenu)
        {
            if (adultsBtns.Count > 0)
            {
                List<GameObject> gameObjList = new List<GameObject>();
                for (int i = 0; i < commandMenu._adultButtons.Length; i++)
                {
                    gameObjList.Add(commandMenu._adultButtons[i]);
                }
                foreach (GameObject gameObj in adultsBtns)
                {
                    gameObjList.Add(gameObj);
                }
                Il2CppReferenceArray<GameObject> adults = new Il2CppReferenceArray<GameObject>(gameObjList.Count);
                for (int i = 0; i < adults.Length; i++)
                {
                    adults[i] = gameObjList[i];
                }
                commandMenu._adultButtons = adults;
            }
        }
        public static void CreateNewCategory(GameObject categoryListGameObj,string categoryName)
        {
            //MoreActionsPlugin.Log.LogInfo("Creating new category");
        }
        
        public static void CreateNewActionButton(CommandUI commandMenu,Transform selectionListObj, string actionName, int actionID, bool isLewd)
        {
            if (selectionListObj.Find(actionName) != null) return;
            if (commandZeroObj == null || commandTwoObj == null)
            {
                MoreActionsPlugin.Log.LogInfo("Could not find default action gameObject");
                return;
            }

            int childObjCount = selectionListObj.childCount > 1 ? selectionListObj.childCount - 1 : 0;
            float xLocalPos = 50 - (childObjCount * 10);
            float yLocalPos = 23 - (childObjCount * 40);

            GameObject actionObj;
            if (isLewd) actionObj = UnityEngine.Object.Instantiate(commandTwoObj);
            else actionObj = UnityEngine.Object.Instantiate(commandZeroObj);

            actionObj.name = actionName;
            var actionButtomCommand = actionObj.GetComponent<CommandButton>();
            if (actionButtomCommand != null)
            {
                actionButtomCommand.Button.onClick.m_PersistentCalls.m_Calls[0].arguments.intArgument = actionID;
            }

            actionObj.transform.SetParent(selectionListObj);
            var btnRect = actionObj.GetComponent<RectTransform>();
            btnRect.localPosition = new Vector3(xLocalPos, yLocalPos, 0);
            btnRect.localScale = new Vector3(1, 1, 1);//Reset Local scale
            var btnText = actionObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = actionName;
            }
            if (isLewd)
            {
                adultsBtns.Add(actionObj);
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
            if (!Enum.IsDefined(typeof(SimulationDefine.CommandNo), commandID))
            {
                YesNoJudgeManager.Instance.EachJudgment(commandID, 0);
                int answerRate = (int)YesNoJudgeManager.Instance.ansInfo.rate;
                int answerType = YesNoJudgeManager.Instance.ansInfo.ans;
                var advScene = ADVManager.Instance.gameObject.GetComponentInChildren<ADVScene>();
                if (advScene != null)
                {
                    if (advScene._scenario != null)
                    {
                        if (MoreActionsPlugin.GetShowLog()) MoreActionsPlugin.Log.LogInfo($"Judge System: CommandID:{commandID} - Rate: {answerRate} - Answer type: {answerType}");
                        advScene._scenario.CurrentHeroine.charasGameParam.judgeRate = answerRate;
                        advScene._scenario.CurrentHeroine.charasGameParam.judgeAnswer = answerType;
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
        private static void AdjustActionSelection(Transform transObj, int categoryID)
        {
            float localX = 0;
            float localY = 0;
            var rectTrans = transObj.gameObject.GetComponent<RectTransform>();
            if (rectTrans != null)
            {
                switch (categoryID)
                {
                    case 0:
                        break;
                    case 1:
                        if (transObj.childCount > 23)
                        {
                            localX = 6 * (transObj.childCount - 23);
                            localY = 40 * (transObj.childCount - 23);
                            rectTrans.localPosition = new Vector3(localX, localY);
                        }
                        break;
                    case 2:
                        if (transObj.childCount > 21)
                        {
                            localX = 6 * (transObj.childCount - 21);
                            localY = 40 * (transObj.childCount - 21);
                            rectTrans.localPosition = new Vector3(localX, localY);
                        }
                        break;
                    case 3:
                        if (transObj.childCount > 19)
                        {
                            localX = 6 * (transObj.childCount - 19);
                            localY = 40 * (transObj.childCount - 19);
                            rectTrans.localPosition = new Vector3(localX, localY);
                        }
                        break;
                    case 4:
                        if (transObj.childCount > 17)
                        {
                            localX = 6 * (transObj.childCount - 17);
                            localY = 40 * (transObj.childCount - 17);
                            rectTrans.localPosition = new Vector3(localX, localY);
                        }
                        break;
                    case 5:
                        if (transObj.childCount > 15)
                        {
                            localX = 6 * (transObj.childCount - 15);
                            localY = 40 * (transObj.childCount - 15);
                            rectTrans.localPosition = new Vector3(localX, localY);
                        }
                        break;
                    case 6:
                        if (transObj.childCount > 13)
                        {
                            localX = 6 * (transObj.childCount - 13);
                            localY = 40 * (transObj.childCount - 13);
                            rectTrans.localPosition = new Vector3(localX, localY);
                        }
                        break;
                    case 7:
                        if (transObj.childCount > 11)
                        {
                            localX = 6 * (transObj.childCount - 11);
                            localY = 40 * (transObj.childCount - 11);
                            rectTrans.localPosition = new Vector3(localX, localY);
                        }
                        break;
                    case 8:
                        if (transObj.childCount > 9)
                        {
                            localX = 6 * (transObj.childCount - 9);
                            localY = 40 * (transObj.childCount - 9);
                            rectTrans.localPosition = new Vector3(localX, localY);
                        }
                        break;
                    case 9:
                        if (transObj.childCount > 6)
                        {
                            localX = 6 * (transObj.childCount - 6);
                            localY = 40 * (transObj.childCount - 6);
                            rectTrans.localPosition = new Vector3(localX, localY);
                        }
                        break;
                    case 10:
                        if (transObj.childCount > 6)
                        {
                            localX = 6 * (transObj.childCount - 6);
                            localY = 40 * (transObj.childCount - 6);
                            rectTrans.localPosition = new Vector3(localX, localY);
                        }
                        break;
                }
            }
        }
    }
}
