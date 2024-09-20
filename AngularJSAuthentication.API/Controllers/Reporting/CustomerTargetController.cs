using AngularJSAuthentication.DataContracts.Transaction.CustomerTarget;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.Reporting
{
    [RoutePrefix("api/CustomerTarget")]
    public class CustomerTargetController : ApiController
    {
        [AllowAnonymous]
        [Route("DispatchData/month/{month}/year/{year}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetDispatchDataAsync(int month, int year)
        {

            try
            {
                using (var authContext = new AuthContext())
                {
                    var monthparam = new SqlParameter("month", month);
                    var yeararparam = new SqlParameter("year", year);
                    var ordHistories = authContext.Database.SqlQuery<CustomerTargetDispatchDataViewModel>("exec GetCustomerTargetData @month, @year", monthparam, yeararparam).ToList();
                    return Ok(ordHistories);
                }
            }
            catch (Exception ex)
            {
                return Ok();
            }

            //try
            //{
            //    var customerTargetManager = new CustomerTargetManager();
            //    var list = customerTargetManager.GetDispatchData(month, year);
            //    return Ok(list);
            //}catch(Exception ex)
            //{
            //    return Ok();
            //}

        }
    }
}
