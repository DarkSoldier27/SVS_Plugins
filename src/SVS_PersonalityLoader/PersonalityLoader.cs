using Manager;
using SV.Config;
using System.Collections.Generic;

namespace PersonalityLoader
{
    internal static class PersonalityLoader
    {
        public static Dictionary<int, int> personalitiesIDsDic = new();
        public static void CreatePersoanlityVoiceSetting(VoiceSetting voiceSetting)
        {
            if (Game.expIDCharaDic == null) return;

            if (Game.expIDCharaDic.Count != 0)
            {
                List<int> _persoKeys = new List<int>();
                foreach (var ID in Game.expIDCharaDic)
                {
                    if (ID.Key > 99)
                    {
                        if (!_persoKeys.Contains(ID.Key)) _persoKeys.Add(ID.Key);
                    }
                }

                var _personalities = Voice.InfoTable;
                if (_personalities == null) return;

                var _num = voiceSetting._table.Count + 3;
                foreach (var _perso in _persoKeys)
                {
                    if (_perso > 99)
                    {
                        if (_personalities.ContainsKey(_perso))
                        {
                            voiceSetting.Create(_num, _perso, _personalities[_perso].Personality);
                            PersonalityLoaderPlugin.Log.LogInfo($"Created Voice Setting for: {_personalities[_perso].Personality} ID:{_perso}");
                            _num++;
                        }
                    }
                }
            }
        }
        public static int SetAnimationCtrlIfMissing(byte sex, int personality)
        {
            if (sex == 0 && (personality < 100 && personality > 103)) return 100;
            if (sex == 1 && personality > 15) return 0;
            return personality;
        }
    }
}
