using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/demand")]
    public class DemandController : ApiController
    {


        [Route("")]
        public IList<DemandDetails> Get()
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    var demand = (from a in context.dbDemandDetails where a.CompanyId == compid select a).ToList();
                    return demand;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        [ResponseType(typeof(DemandDetails))]
        [Route("")]
        [AcceptVerbs("POST")]
        public DemandMaster Post(List<DemandDetails> demand)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int warehouseid = 0;
                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "Warehouseid")
                        {
                            warehouseid = int.Parse(claim.Value);
                        }
                    }

                    DemandMaster dm = new DemandMaster();
                    dm.demand = demand;
                    dm.CompanyId = compid;
                    dm.WarehouseId = warehouseid;
                    context.Adddemand(dm);
                    return null;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
    }
}
