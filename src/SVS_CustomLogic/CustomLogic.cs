using ADV;
using Character;
using Manager;
using SaveData;
using SV;
using SV.Chara;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SVS_CustomLogic
{
    internal class CustomLogic
    {
        private static Random _rnd = new Random();
        private static readonly Dictionary<int, CustomLogicParam.TraitParam> traitDic = [];

        private static bool traitLoaded = false;
        /// <summary>
        /// Calculates the type of answer base on the rate values. Returns 0 for Yes and 1 for No. Rate value equal or over 100 is always yes.
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static int CalcAnswer(float rate)
        {
            if (rate >= 100) return 0;
            int answerChance = _rnd.Next(0, 100);
            if (rate > answerChance) return 0;
            return 1;
        }
        /// <summary>
        /// Returns the map ID of the character. Returns -2 if the character is unloaded and -1 if they have not arrive yet.
        /// </summary>
        /// <param name="chara"></param>
        /// <returns></returns>
        public static int GetCharacterCurrentMap(Actor chara)
        {
            if (chara != null)
            {
                if (chara.charaBase != null)
                {
                    if (chara.charaBase.BehaviourCtrl != null) return chara.charaBase.BehaviourCtrl.nowMapID;
                }
            }
            return -2;
        }

        public static void AddTrait()
        {
            if (traitLoaded) return;
            traitDic.Clear();

            //Template
            traitDic.Add(999, new CustomLogicParam.TraitParam()
            {
                TraitID = 999,
                TraitName = "TestTrait",
                Description = "A test for a custom Trait"
            });
            //Sibling
            traitDic.Add(40, new CustomLogicParam.TraitParam()
            {
                TraitID = 40,
                TraitName = "Sibling",
                Description = "Is a sibling of a character with the same last name"
            });
            //Parent
            traitDic.Add(41, new CustomLogicParam.TraitParam()
            {
                TraitID = 41,
                TraitName = "Parent",
                Description = "Is the parent of a character with the same last name"
            });
            //Children
            traitDic.Add(42, new CustomLogicParam.TraitParam()
            {
                TraitID = 42,
                TraitName = "Son or Daughter",
                Description = "Is the child of a parent character with the same last name"
            });
            //Loyal
            traitDic.Add(43, new CustomLogicParam.TraitParam()
            {
                TraitID = 43,
                TraitName = "Loyal",
                Description = "If dating someone, no physical contact accepted with anyone else. Will also avoid other suitors trying to steal them away."
            });
            //Stalker
            traitDic.Add(44, new CustomLogicParam.TraitParam()
            {
                TraitID = 44,
                TraitName = "Stalker",
                Description = "Sometimes they will follow a character they are interested in, or stay near them"
            });
            //Lazy
            traitDic.Add(45, new CustomLogicParam.TraitParam()
            {
                TraitID = 45,
                TraitName = "Lazy",
                Description = "Doesn't like doing activities and less likely to help with them."
            });

            var sortedTraitDic = traitDic.OrderBy(trait => trait.Key);
            //Adds the custom trait to the game.
            if (Game.IndividualityInfoTable != null)
            {
                foreach (var customTrait in sortedTraitDic)
                {
                    if (!Game.IndividualityInfoTable.ContainsKey(customTrait.Key)) Game.IndividualityInfoTable.Add(customTrait.Key, new IndividualityInfoParam()
                    { ID = customTrait.Value.TraitID, Name = customTrait.Value.TraitName, Information = customTrait.Value.Description });
                }
                CustomLogicPlugin.Log.LogInfo($"Loaded {traitDic.Count} custom traits");
            }
            traitLoaded = true;
        }
        public static void CustomLogicAnswerRate(YesNoJudgeManager.AnswerInfo answerInfo, YesNoJudgeManager.YesNoInfo yesNoInfo, int commandID, int questionCount)
        {
            if (commandID < 0) return;
            if (answerInfo.ans < 0) return;

            //Vanilla rate after vanilla traits
            float answerRate = answerInfo.rate;

            if (traitDic.Count > 0)
            {
                foreach (var trait in traitDic)
                {
                    //Check if passive or active has a custom trait
                    if (yesNoInfo.passive.gameParameter.individuality.answer.Contains(trait.Key) || yesNoInfo.active.gameParameter.individuality.answer.Contains(trait.Key))
                    {
                        //Add a case with your custom trait ID.
                        //You trait should only edit the answer rate for compatibilty reasons, if you specify the answer type do a return instead of break.
                        switch (trait.Key)
                        {
                            case 40://Sibling
                                if (CustomLogicPlugin.GetShowLog()) CustomLogicPlugin.Log.LogInfo($"Applying Sinbling trait answer rate logic");
                                if (answerInfo.ans != 2) answerRate += CustomTraitConditions.TraitSibling.SetAnswer(answerInfo, yesNoInfo, commandID, questionCount, answerRate);
                                break;
                            case 41://Parent
                                if (CustomLogicPlugin.GetShowLog()) CustomLogicPlugin.Log.LogInfo($"Applying Parent trait answer rate logic");
                                if (answerInfo.ans != 2) answerRate += CustomTraitConditions.TraitSibling.SetAnswer(answerInfo, yesNoInfo, commandID, questionCount, answerRate);
                                break;
                            case 42://Son or daughter
                                if (CustomLogicPlugin.GetShowLog()) CustomLogicPlugin.Log.LogInfo($"Applying Son or daughter trait answer rate logic");
                                if (answerInfo.ans != 2) answerRate += CustomTraitConditions.TraitSibling.SetAnswer(answerInfo, yesNoInfo, commandID, questionCount, answerRate);
                                break;
                            case 45://Lazy
                                if (CustomLogicPlugin.GetShowLog()) CustomLogicPlugin.Log.LogInfo($"Applying Lazy trait answer rate logic");
                                if (answerInfo.ans != 2) answerRate += CustomTraitConditions.TraitLazy.SetAnswer(answerInfo, yesNoInfo, commandID, questionCount, answerRate);
                                break;
                            case 999://TestTrait
                                //if (CustomTraitsPlugin.GetShowLog()) CustomTraitsPlugin.Log.LogInfo($"");
                                if (CustomLogicPlugin.GetShowLog()) CustomLogicPlugin.Log.LogInfo($"Test Trait answer rate called");
                                if (answerInfo.ans != 2) answerRate += CustomTraitConditions.TraitTemplate.SetAnswer(answerInfo, yesNoInfo, commandID, questionCount, answerRate);
                                break;
                        }
                    }
                }
                //If the answer rate has been chancge, calculates the chance of success.
                if (answerRate != answerInfo.rate)
                {
                    answerInfo.ans = CalcAnswer(answerRate);
                    if (answerRate < 0) answerInfo.rate = 0;
                    else answerInfo.rate = answerRate;
                }
            }
        }
        public static int CustomLogicReaction(AI charaAI, AI charaAI_2, AI charaAI_3, int no, int reactionNo)
        {
            if (charaAI is null || charaAI_2 is null || charaAI_3 is null) return reactionNo;
            if (traitDic.Count > 0)
            {
                foreach (var trait in traitDic)
                {
                    if (charaAI.charaData.gameParameter.individuality.answer.Contains(trait.Key))
                    {
                        switch (trait.Key)
                        {
                            case 45://Lazy
                                if (CustomLogicPlugin.GetShowLog()) CustomLogicPlugin.Log.LogInfo($"Applying Lazy trait reaction logic");
                                CustomTraitConditions.TraitLazy.SetReaction(charaAI, charaAI_2, charaAI_3, no, reactionNo);
                                break;
                            case 999:
                                if (CustomLogicPlugin.GetShowLog()) CustomLogicPlugin.Log.LogInfo($"Applying TestTrait trait reaction logic");
                                CustomTraitConditions.TraitTemplate.SetReaction(charaAI, charaAI_2, charaAI_3, no, reactionNo);
                                break;
                        }
                    }
                }
            }

            return reactionNo;
        }
        public static void CustomLogicAction(SVThinking thinking)
        {
            if (thinking.CharaCtrl is null) return;
            if (thinking.CharaCtrl.AI is null) return;
            if (thinking.CharaCtrl.AI.charaData is null) return;
            if (traitDic.Count > 0)
            {
                Actor chara = thinking.CharaCtrl.AI.charaData;
                foreach (var trait in traitDic)
                {
                    if (chara.gameParameter.individuality.answer.Contains(trait.Key))
                    {
                        switch (trait.Key)
                        {
                            case 44://Stalker
                                if (CustomLogicPlugin.GetShowLog()) CustomLogicPlugin.Log.LogInfo($"Applying Stalker trait action logic");
                                CustomTraitConditions.TraitStalker.SetAction(thinking);
                                break;
                            case 45://Lazy
                                if (CustomLogicPlugin.GetShowLog()) CustomLogicPlugin.Log.LogInfo($"Applying Lazy trait action logic");
                                CustomTraitConditions.TraitLazy.SetAction(thinking);
                                break;
                            case 999://TestTrait
                                if (CustomLogicPlugin.GetShowLog()) CustomLogicPlugin.Log.LogInfo($"Applying Template trait action logic");
                                CustomTraitConditions.TraitTemplate.SetAction(thinking);
                                break;
                        }
                    }
                }
            }
        }

        public static void CustomLogicFavorabiltyGain(FavourableImpressionManager favourable, bool _isActive, bool _isOneWay, HumanData _myCharaData, CharactersGameParameter _myGameParam, HumanData _targetCharaData, CharactersGameParameter _targetGameParam)
        {
            if (traitDic.Count > 0)
            {
                foreach (var trait in traitDic)
                {
                    if (_myCharaData.GameParameter.individuality.answer.Contains(trait.Key) || _targetCharaData.GameParameter.individuality.answer.Contains(trait.Key))
                    {
                        switch (trait.Key)
                        {
                            case 40://Sibling
                                if (CustomLogicPlugin.GetShowLog()) CustomLogicPlugin.Log.LogInfo($"Applying Sibling trait Favorability Called");
                                CustomTraitConditions.TraitSibling.SetFavorabiltyGain(favourable, _myCharaData, _myGameParam, _targetCharaData, _targetGameParam);
                                break;
                            case 41://Parent
                                if (CustomLogicPlugin.GetShowLog()) CustomLogicPlugin.Log.LogInfo($"Applying Parent trait favorability Logic");
                                CustomTraitConditions.TraitParent.SetFavorabiltyGain(favourable, _myCharaData, _myGameParam, _targetCharaData, _targetGameParam);
                                break;
                            case 42://SonDaughter
                                if (CustomLogicPlugin.GetShowLog()) CustomLogicPlugin.Log.LogInfo($"Applying SonDaughter trait favorability logic");
                                CustomTraitConditions.TraitSonDaughter.SetFavorabiltyGain(favourable, _myCharaData, _myGameParam, _targetCharaData, _targetGameParam);
                                break;
                            case 999://TestTrait
                                if (CustomLogicPlugin.GetShowLog()) CustomLogicPlugin.Log.LogInfo($"Applying Test trait favorability logic");
                                CustomTraitConditions.TraitTemplate.SetFavorabiltyGain(favourable, _isActive, _isOneWay, _myCharaData, _myGameParam, _targetCharaData, _targetGameParam);
                                break;
                        }
                    }
                }
            }
        }
        public static void CustomLogicADV(TextScenario scenario, string assetName)
        {
            if (!CustomLogicPlugin.GetADVLoaderPlugin(false)) return;
            if (scenario is null) return;
            if (scenario.CurrentHeroine is null) return;
            if (GameChara.Player is null) return;

            Actor chara = GameChara.Player;
            Actor charaADV = scenario.CurrentHeroine;

            if (traitDic.Count > 0)
            {
                foreach (var trait in traitDic)
                {
                    if (chara.gameParameter.individuality.answer.Contains(trait.Key) || charaADV.gameParameter.individuality.answer.Contains(trait.Key))
                    {
                        switch (trait.Key)
                        {
                            case 40://Sibling
                                if (CustomLogicPlugin.GetShowLog()) CustomLogicPlugin.Log.LogInfo($"Applying Sibling trait Favorability Called");
                                CustomTraitConditions.TraitSibling.SetTraitADV(scenario, chara, charaADV);
                                break;
                            case 41://Parent
                                if (CustomLogicPlugin.GetShowLog()) CustomLogicPlugin.Log.LogInfo($"Applying Parent trait favorability Logic");                 
                                break;
                            case 42://SonDaughter
                                if (CustomLogicPlugin.GetShowLog()) CustomLogicPlugin.Log.LogInfo($"Applying SonDaughter trait favorability logic");
                                break;
                            case 999://TestTrait
                                if (CustomLogicPlugin.GetShowLog()) CustomLogicPlugin.Log.LogInfo($"Applying Test trait favorability logic");
                                CustomTraitConditions.TraitTemplate.SetTraitADV(scenario, chara, charaADV);
                                break;                         
                        }
                    }
                }
            }
        }
        public static void Test()
        {
          
        }
    }
}
