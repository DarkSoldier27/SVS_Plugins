using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SVS_MoreActions
{
    internal class MoreActionsParam
    {
        public class ActionParam
        {
            public int ActionID { get; set; }
            public int CategoryID { get; set; }
            public string ActionName { get; set; }
            public bool IsLewdAction { get; set; } = false;
            public int ActionTimeCost { get; set; } = 0;
            public List<float> BaseJudgeRates { get; set; } = new List<float>();
            public List<float> JudgeRates { get; set; } = new List<float>();
            public Dictionary<int, FavorMoodParam> FavorMoodParam { get; set; }
        }
        public class FavorMoodParam
        {
            public int ID { get; set; }
            public Dictionary<int, List<int>> Favors { get; set; }
            public Dictionary<int, List<int>> Moods { get; set; }

        }
        public class FavorParam
        {
            public int ID { get; set; }
            public Dictionary<int, List<int>> Favors { get; set; }
        }
        public class MoodParam
        {
            public int ID { get; set; }
            public Dictionary<int, List<int>> Moods { get; set; }

        }
        public static List<ActionParam> DeserializeAction()
        {
            List<ActionParam> actionList = new List<ActionParam>();
            string actionFolder = Path.Combine(Paths.GameRootPath, "abdata/mods/MoreActions");
            if (Directory.Exists(actionFolder))
            {
                string[] textFiles = Directory.GetFiles(actionFolder, "*.json", SearchOption.AllDirectories);
                if (textFiles.Length > 0)
                {
                    foreach (string textFile in textFiles)
                    {
                        var sr = new StreamReader(File.OpenRead(textFile));
                        var json = sr.ReadToEnd();
                        sr.Close();
                        try
                        {
                            var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip };
                            var loadedActions = JsonSerializer.Deserialize<List<ActionParam>>(json, options);
                            foreach (var actParam in loadedActions)
                            {
                                actionList.Add(actParam);
                            }
                        }
                        catch (Exception ex)
                        {
                            MoreActionsPlugin.Log.LogInfo($"Failed to load json File: {ex}");
                        }
                    }
                }
            }
            return actionList;
        }
    }
}
