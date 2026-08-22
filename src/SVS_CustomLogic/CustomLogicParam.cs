namespace SVS_CustomLogic
{
    internal class CustomLogicParam
    {
        public class TraitParam
        {
            public string TraitName { get; set; }
            public int TraitID { get; set; }
            public string Description { get; set; }
        }

        public class LewdTraitParam
        {
            public string LewdTraitName { get; set; }
            public int LewdTraitID { get; set; }
            public string Description { get; set; }
        }

        public class JobParam
        {
            public string JobName { get; set; }
            public int JobID { get; set; }
        }

        public class FortuneParam
        {
            public string FortuneID { get; set; }
            public string FortuneName { get; set; }

            public class FortuneScenarioParam
            {
                public string Command { get;set; }
                public string[] Args { get; set; }
            }
        }
    }
}
