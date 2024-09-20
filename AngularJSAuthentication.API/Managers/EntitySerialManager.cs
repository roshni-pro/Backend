using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.API.Managers
{
    public class EntitySerialManager
    {
        /// <summary>
        /// Get Entity Serial Number based on Entity Name and StateId
        /// </summary>
        /// <param name="stateId"></param>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public async Task<string> GetCurrentEntityNumberAsync(int stateId, string entityName)
        {
            var number = string.Empty;
            using (var authContext = new AuthContext())
            {
                var query = new StringBuilder("exec spGetCurrentNumber '").Append(entityName).Append("', ").Append(stateId);
                number = await authContext.Database.SqlQuery<string>(query.ToString()).FirstOrDefaultAsync();
            }
            return number;
        }

        /// <summary>
        /// Get Entity Serial Number based on Entity Name and StateId
        /// </summary>
        /// <param name="stateId"></param>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public string GetCurrentEntityNumber(int stateId, string entityName)
        {
            var number = string.Empty;
            using (var authContext = new AuthContext())
            {
                var query = new StringBuilder("exec spGetCurrentNumber '").Append(entityName).Append("', ").Append(stateId);
                var nextNumber = authContext.Database.SqlQuery<NextNumber>(query.ToString()).FirstOrDefault();
                number = nextNumber.CurrentNumber;
            }
            return number;
        }
    }


    internal class NextNumber
    {
        public string CurrentNumber { get; set; }
    }

}