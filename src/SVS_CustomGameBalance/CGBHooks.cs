using ADV;
using ADV.Commands.Game.LowChara;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SaveData;
using SV;
using SV.Chara;
using SV.Talk;
using SV.Title;

namespace SVS_CustomGameBalance
{
    internal class CGBHooks
    {
        internal static class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(TitleScene), nameof(TitleScene.Start))]
            public static void PostTittleStart(TitleScene __instance)
            {
                CustomGameBalance.InitCGB();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SimulationScene), nameof(SimulationScene.Start))]
            public static void SimSceneStart(SimulationScene __instance)
            {
                CGBCharaPCController.CreateSwitchPCCharacterButton(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SimulationScene), nameof(SimulationScene.Update))]
            public static void SimSceneUpdate(SimulationScene __instance)
            {
                CustomGameBalance.SimUpdate();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(PCPassiveTalkTask), nameof(PCPassiveTalkTask.UIActiveAndBaseEnd))]
            public static void SetPCInteractible()
            {
                if (GameChara.Player.charasGameParam.isWithAction) GameChara.Player.charasGameParam.isWithAction = false;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(MemoryParameter), nameof(MemoryParameter.EndOfDay))]
            public static void EndOfDayGameBalance(MemoryParameter __instance, Actor _self)
            {
                CustomGameBalance.CharaStatsReductionPerDay(_self);
                CustomGameBalance.CheatingStatusDebuff(_self, -1,true);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(BehaviourController), nameof(BehaviourController.RunAndWalk))]
            public static void SetNewBaseSpeed(BehaviourController __instance, bool _isPC, int _actionNo)
            {
                CustomGameBalance.SetCharaWalkAndRunSpeed(_isPC);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(WitnessChara), nameof(WitnessChara.IsEntrySkinship))]
            public static bool SkinshipCheck(bool __result, AI _self, AI _target)
            {
                return CustomGameBalance.GetIsSkinshipAction(_self, _target, __result);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ADVManager), nameof(ADVManager.Set3PFlag))]
            public static void ShowJoin3POptionADV(ADVManager __instance, Actor actor, Actor actor1, Actor actor2)
            {
                CGBReactions.SetADVThreesomeOption(__instance, actor, actor1, actor2);
            }

            [HarmonyPriority(800)]
            [HarmonyPostfix]
            [HarmonyPatch(typeof(SVThinking), nameof(SVThinking.OnUpdate))]
            public static void ActionOverride(SVThinking __instance)
            {
                CGBActionBooster.SetAction(__instance);
            }

            [HarmonyPriority(300)]
            [HarmonyPostfix]
            [HarmonyPatch(typeof(ReactionManager), nameof(ReactionManager.Confirmation))]
            public static int SetNewReaction(int __result, AI _ai, AI _ai1, AI _ai2, int no)
            {
                return CGBReactions.SetReaction(_ai,_ai1,_ai2, no, __result);
            }

            [HarmonyPriority(300)]
            [HarmonyPostfix]
            [HarmonyPatch(typeof(ThinkingManager), nameof(ThinkingManager.InterpersonalCommandSelectionTarget))]
            public static int SetTarget(int __result, Actor _actor, int _commandID)
            {
                if (__result > -1) return CustomGameBalance.NewCommandTarget(_actor, _commandID, __result);
                return __result;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CommandUI), nameof(CommandUI.ToStringProb))]
            public static string ShowProbability(string __result, int rate)
            {
                if (CustomGameBalancePlugin.GetProbDisplay() == CustomGameBalancePlugin.DisplayPercentage.Hide) return "";
                else if (CustomGameBalancePlugin.GetProbDisplay() == CustomGameBalancePlugin.DisplayPercentage.Real) return rate + "%";
                return __result;
            }

            private static bool[] fixes = [false, false];
            [HarmonyPriority(300)]
            [HarmonyPostfix]
            [HarmonyPatch(typeof(BaseAnswer), nameof(BaseAnswer.Judge))]
            public static void ActionAnswerRate(bool __result, YesNoJudgeManager.AnswerInfo _ansInfo, YesNoJudgeManager.YesNoInfo _ynInfo, int _commandID, int _questionCount, Il2CppStructArray<bool> _calcs)
            {
                if (__result)
                {
                    CustomGameBalance.NewAnswerRate(_ansInfo, _ynInfo, _commandID, _questionCount);
                    if (_ansInfo.ans == 0 && _ynInfo.passive != null) CustomGameBalance.CheatingStatusDebuff(_ynInfo.passive, _ynInfo.active.charasGameParam.Index, false);
                } 
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(LowPolyText), nameof(LowPolyText.Do))]
            public static void RemoveNPCSkipPre(LowPolyText __instance)
            {
                CustomGameBalance.OffScreenInteractions.PreNPCSkip(__instance);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(FukidashiUICanvas), nameof(FukidashiUICanvas.Emit))]
            public static bool DisplayFukidashi()
            {              
                return CustomGameBalance.OffScreenInteractions.DisplayFukidashi();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(LowPolyText), nameof(LowPolyText.Do))]
            public static void RemoveNPCSkipPost(LowPolyText __instance)
            {
                CustomGameBalance.OffScreenInteractions.PostNPCSkip(__instance);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(LowPolyHMotionPlay), nameof(LowPolyHMotionPlay.Do))]
            public static void NPCSexDuration(LowPolyHMotionPlay __instance)
            {
                CustomGameBalance.LowPolySexDuration(__instance);              
            }

            /*[HarmonyPrefix]
            [HarmonyPatch(typeof(YesNoJudgeManager), nameof(YesNoJudgeManager.CommonCondition19__22))]
            public static bool RemoveActionLimit()
            {
                if (_removeActionLimit.Value) return false;
                return true;
            }*/

            [HarmonyPriority(300)]
            [HarmonyPostfix]
            [HarmonyPatch(typeof(SuccessJudgeManager), nameof(SuccessJudgeManager.Judge))]
            public static int OverrideSuccess(int __result, int _commandNo)
            {
                return CustomGameBalance.NewSuccessValue(__result);
            }

            [HarmonyPriority(300)]
            [HarmonyPrefix]
            [HarmonyPatch(typeof(SimulationButtonAction), nameof(SimulationButtonAction.ShiftFromSimNightToRoomMorning))]
            public static void NightCharactersPool(SimulationButtonAction __instance)
            {               
                CGBNightEvent.SetNightEventCharacter();
            }
        }
    }
}
