using AngularJSAuthentication.DataContracts.ServiceRequestParam;
using System.Threading.Tasks;

namespace AngularJSAuthentication.API.Helpers
{
    public class LogHelper
    {
        public static async Task<bool> TraceLog(TraceLog log)
        {
            MongoDbHelper<TraceLog> mongoDbHelper = new MongoDbHelper<TraceLog>();
            return await mongoDbHelper.InsertLog(log);

        }

        public static async Task<bool> ErrorLog(ErrorLog log)
        {
            MongoDbHelper<ErrorLog> mongoDbHelper = new MongoDbHelper<ErrorLog>();
            return await mongoDbHelper.InsertLog(log);
        }
    }
}