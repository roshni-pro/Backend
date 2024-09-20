namespace AngularJSAuthentication.DataContracts.Masters
{
    public class PeopleMinDc
    {
        public int PeopleId { get; set; }
        public string PeopleName { get; set; }
    }

    public class BuyerMinDc
    {
        public int PeopleId { get; set; }
        public string DisplayName { get; set; }
    }
    public class PeopleCityDc
    {
        public int PeopleId { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public string CityPlaceId { get; set; }
        public double CityLatitude { get; set; }
        public double CityLongitude { get; set; }

    }
}
