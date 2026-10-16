using Cysharp.Threading.Tasks;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SV;
using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace SVS_MoreActions
{
    internal class MoreActionUI
    {
        private static GameObject commandZeroObj;
        private static GameObject commandTwoObj;
        private static List<GameObject> adultsBtns = new List<GameObject>();//List of pink buttons
        public static List<string> commandNameList = new List<string>()//List of categories. The names here are the GameObject names. May change it into a Dictionary<int,string> in the future
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
        public static void CreateNewCategory(CommandUI commandMenu)
        {
            //MoreActionsPlugin.Log.LogInfo("Creating new category");
        }
        //Create action button
        public static void CreateNewActionOnMenu(CommandUI commandMenu, MoreActionsParam.ActionParam actParam)
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
            if (category != null)
            {
                var selectionList = category.Find("GameObject");
                if (selectionList != null)
                {
                    CreateNewActionButton(commandMenu, selectionList, actParam);
                    AdjustActionSelection(selectionList, actParam.CategoryID);
                }
            }
            else MoreActionsPlugin.Log.LogInfo($"Action category does not exist");
        }
        private static void CreateNewActionButton(CommandUI commandMenu, Transform selectionListObj, MoreActionsParam.ActionParam actParam)
        {
            if (selectionListObj.Find(actParam.ActionName) != null)
            {
                MoreActionsPlugin.Log.LogInfo($"Action with name {actParam.ActionName} has already been added");
                return;
            }
            if (commandZeroObj == null || commandTwoObj == null)
            {
                MoreActionsPlugin.Log.LogInfo("Could not find default action gameObject");
                return;
            }

            int childObjCount = selectionListObj.childCount > 1 ? selectionListObj.childCount - 1 : 0;
            float xLocalPos = 50 - (childObjCount * 10);
            float yLocalPos = 23 - (childObjCount * 40);

            //Create new GameObject base on an existing GameObject with its parameters.
            GameObject actionObj;
            if (actParam.IsLewdAction) actionObj = UnityEngine.Object.Instantiate(commandTwoObj);
            else actionObj = UnityEngine.Object.Instantiate(commandZeroObj);

            actionObj.name = actParam.ActionName;
            var actionButtomCommand = actionObj.GetComponent<CommandButton>();
            if (actionButtomCommand != null)
            {
                actionButtomCommand.Button.onClick.m_PersistentCalls.m_Calls[0].arguments.intArgument = actParam.ActionID;
            }

            //Set Button Entry Event
            var inEvent = actionObj.GetComponent<ObservablePointerEnterTrigger>();
            if (inEvent != null)
            {
                inEvent.OnPointerEnterAsObservable()
                        .SubscribeWithState(commandMenu._txtSuccessRate, (Action<UnityEngine.EventSystems.PointerEventData, TMPro.TMP_Text>)((_, txt) => DisplayActionRate(commandMenu, actParam.ActionID, true)))
                        .AddTo(commandMenu);
            }

            //Set Button Exit Event
            var exitEvent = actionObj.GetComponent<ObservablePointerExitTrigger>();
            if (exitEvent != null)
            {
                exitEvent.OnPointerExitAsObservable()
                            .SubscribeWithState(commandMenu._txtSuccessRate, (Action<UnityEngine.EventSystems.PointerEventData, TMPro.TMP_Text>)((_, txt) => DisplayActionRate(commandMenu, actParam.ActionID, false)))
                            .AddTo(commandMenu);
            }

            //Set Action GameObject Parent
            actionObj.transform.SetParent(selectionListObj);

            //Set RectTransform Position and Scale, the game may do it automatically though
            var btnRect = actionObj.GetComponent<RectTransform>();
            btnRect.localPosition = new Vector3(xLocalPos, yLocalPos, 0);
            btnRect.localScale = new Vector3(1, 1, 1);//Reset Local scale after setting parent

            //Set Button Text
            var btnText = actionObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = actParam.ActionName;
            }

            //Add button to the Adult Button List
            if (actParam.IsLewdAction)
            {
                adultsBtns.Add(actionObj);
            }

            //Check if the action starts hidden
            if (actParam.AdvanceOptions != null)
            {
                if (actParam.AdvanceOptions.IsButtonHidden)
                {
                    actionObj.SetActive(false);
                }
            }
        }
        
        public static void SetLewdActions(CommandUI commandMenu)
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
        public static void DisplayActionRate(CommandUI commandMenu, int actionID, bool fadeIn)
        {
            if (fadeIn)
            {
                if (MoreActions.ActionJudgeAnswers.ContainsKey(actionID))
                {
                    commandMenu._tokenOnPointerEnter.Cancel();
                    commandMenu._txtSuccessRate.text = commandMenu.ToStringProb((int)MoreActions.ActionJudgeAnswers[actionID].Item1);
                    commandMenu._successRateFadeCanvas.StartAsync((FadeCanvas.Fade)1, 0.3f, true, false).Forget();
                }
            }
            else
            {
                commandMenu._successRateFadeCanvas.StartAsync((FadeCanvas.Fade)2, 0.3f, true, false).Forget();
            }
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
