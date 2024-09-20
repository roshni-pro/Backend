using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using Nest;
using NLog;
using System;
using System.Configuration;
using System.Linq;

namespace AngularJSAuthentication.API.Helper
{
    public class WHLicenseExpAlert
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        public bool WHLicenseExpDateAlert()
        {
            logger.Info("start WHLicenseExpDateAlert");
            bool alert = false;
            try
            {
                using (var context = new AuthContext())
                {
                    var whlist = context.Database.SqlQuery<WHAlertDC>("GetWarehouseFSSAILicenseExpiryDays").ToList();
                    if (whlist != null)
                    {
                        foreach (var wh in whlist)
                        {
                            if (!string.IsNullOrEmpty(wh.EmailTo))
                            {
                                try
                                {
                                    bool issent = EmailHelper.SendMail(AppConstants.MasterEmail, wh.EmailTo.ToString().Trim(), "",
                                                        ConfigurationManager.AppSettings["Environment"] + " FSSAI License Expiry Date Alert in Warehouse : " + wh.WarehouseName
                                                        , "Your FSSAI License Expiry Date " + wh.FSSAILicenseExpiryDate.ToString() + " is expirying in " + wh.dayss.ToString() + " days at Warehouse : " + wh.WarehouseName 
                                                        , "");
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Error in WHLicenseExpDateAlert in email " + ex.Message);
                                }
                            }
                        }
                        alert = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in WHLicenseExpDateAlert " + ex.Message);
            }
            logger.Info("End WHLicenseExpDateAlert");
            return alert;
        }
    }
    public class WHAlertDC
    {
        public int dayss { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string EmailTo { get; set; }
        public string EmailFrom { get; set; }
        public string EmailBCC { get; set; }
        public string EmailType { get; set; }
        public string FSSAILicenseExpiryDate { get; set; }
        public string FSSAILicenseNumber { get; set; }
    }
}