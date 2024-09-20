using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using Nest;
using NLog;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AngularJSAuthentication.API.Helper
{
    public class AdvancePOtoCreditConvertHelper
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public async Task<int> ConvertAdvancePOtoCredit()
        {
            logger.Info("start AdvancePOtoCreditConvert");
            int alert = 0;
            try
            {
                using (var context = new AuthContext())
                {
                    
                    var Affectedrows = await context.Database.ExecuteSqlCommandAsync("ConvertAdvancePOtoCreditbyJob");
                    if (Affectedrows > 0)
                    {
                        alert = Affectedrows;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in AdvancePOtoCreditConvert " + ex.Message);
            }
            logger.Info("End AdvancePOtoCreditConvert");
            return alert;
        }
    }
}