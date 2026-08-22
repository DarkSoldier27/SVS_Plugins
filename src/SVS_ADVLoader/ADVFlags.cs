namespace SVS_ADVLoader
{
    public class ADVFlags
    {
        public static void SetADVFlag(string flagName, bool flagOn)
        {
            ADVLoaderParam.SetFlag(flagName, flagOn);
        }
    }
}
