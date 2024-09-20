namespace AngularJSAuthentication.DataContracts.Masters
{
    public class ItemMinDc
    {
        public string ItemNumber { get; set; }
        public string ItemName{ get; set; }
        public int ItemMultiMrpId { get; set; }
    }

    public class ItemIncentiveClassification
    {
        public int CityId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public string Classification { get; set; }
        public string BackgroundRgbColor { get; set; }

    }

    public class ItemIncentiveClassificationMarginDc
    {
        public double? MinMarginPercent { get; set; }
        public string ItemNumber { get; set; }
        public string Classification { get; set; }
        public int ItemMultiMrpId { get; set; }

    }
    public class ItemIncentiveClassificationMarginMultiMrpIDDc
    {
        public double? MinMarginPercent { get; set; }
        public int ItemMultiMrpId { get; set; }
        public string Classification { get; set; }

    }

}
