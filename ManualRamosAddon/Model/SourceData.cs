namespace ManualRamosAddon
{
    public class SourceData : ViewModelBase
    {
        private int feederNumber;
        private string sourceEstimateName;
        public string SourceEstimateName { get => sourceEstimateName; set => Set(ref sourceEstimateName, value); }
        public int FeederNumber { get => feederNumber; set => Set(ref feederNumber, value); }
        public SourceData()
        {

        }

        public SourceData(string source, int num)
        {
            SourceEstimateName = source;
            FeederNumber = num;
        }
    }
}
