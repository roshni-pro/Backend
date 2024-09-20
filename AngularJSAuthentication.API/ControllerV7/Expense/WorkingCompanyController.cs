using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.APIParams;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.Expense
{
    [RoutePrefix("api/WorkingCompany")]
    public class WorkingCompanyController : BaseApiController
    {
        [Route("GetAll")]
        [HttpGet]
        public async Task<List<DropDown>> GetWorkingLocation()
        {
            using (var authContext = new AuthContext())
            {
                List<DropDown> list = await authContext.WorkingCompanyDB
                    .Where(x => x.IsActive == true && x.IsDeleted != true)
                     .Select(x => new DropDown
                     {
                         ID = (int)x.Id,
                         Label = x.Name
                     })
                    .ToListAsync();
                return list;
            }
        }
    }
}
