namespace AngularJSAuthentication.API.DataContract
{
    public class KisanDanGallaryDC
    {
        public long Id { get; set; }
        public string LogoUrl { get; set; }
        public string Description { get; set; }
        public bool IsGallaryImage { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

    }
}