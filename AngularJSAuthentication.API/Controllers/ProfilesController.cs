using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Profiles")]
    public class ProfilesController : ApiController
    {
        
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Authorize]
        [Route("")]
        //public IEnumerable<People> Get()
        public People Get()
        {
            logger.Info("Get Company: ");
            int compid = 0, userid = 0;
            int Warehouse_id = 0;
            string email = "";
            string Email = "";
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;

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
                            Warehouse_id = int.Parse(claim.Value);
                        }
                        if (claim.Type == "email")
                        {
                            email = claim.Value;
                        }
                        if (claim.Type == "Email")
                        {
                            Email = claim.Value;
                        }
                    }

                    if (Warehouse_id > 0)
                    {
                        return context.GetPeoplebyIdWid(compid, Warehouse_id, Email);
                    }
                    else
                    {
                        return context.GetPeoplebyId(compid, Email);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting Company " + ex.Message);
                    return null;
                }
            }
        }


        [ResponseType(typeof(People))]
        [Route("")]
        [AcceptVerbs("POST")]
        public People add(People item)
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0;
                // Access claims
                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                }

                item.CompanyId = compid;
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }

                context.AddPeople(item);

                return item;
            }
        }

        [ResponseType(typeof(People))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public People Put(People item)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    logger.Info("Get Company: ");
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

                    item.CompanyId = compid;
                    return context.PutPeople(item);
                }
                catch
                {
                    return null;
                }
            }

        }



        [Route("UpdatePeopleProfile")]
        [AcceptVerbs("PUT")]
        public ResponseResult UpdatePeopleProfile(People item)
        {
            ResponseResult responseResult = new ResponseResult { status = false };
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    string UserName = "";

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "username"))
                        UserName = identity.Claims.FirstOrDefault(x => x.Type == "username").Value;
                    var people = context.Peoples.FirstOrDefault(x => x.PeopleID == item.PeopleID);
                    if (people != null)
                    {
                        if (people.Email != item.Email)
                        {
                            string query = "select count(email) from AspNetUsers where Email='" + item.Email + "'";
                            int count = context.Database.SqlQuery<int>(query).FirstOrDefault();
                            if (count == 0)
                            {
                                query = "Update AspNetUsers set email='" + item.Email + "' where Email='" + people.Email + "'";
                                int result = context.Database.ExecuteSqlCommand(query);
                                if (result == 0)
                                {
                                    responseResult = new ResponseResult { status = false, Message = "Email address not updated." };
                                    return responseResult;
                                }
                            }
                            else
                            {
                                responseResult = new ResponseResult { status = false, Message = "Email address already exist for other people." };
                                return responseResult;
                            }
                        }

                        people.PeopleFirstName = item.PeopleFirstName;
                        people.PeopleLastName = item.PeopleLastName;
                        people.DisplayName = people.PeopleFirstName + " " + people.PeopleLastName;
                        if (!string.IsNullOrEmpty(item.ImageUrl))
                            people.ImageUrl = item.ImageUrl;
                        people.Email = item.Email;
                        people.UpdatedDate = DateTime.Now;
                        people.UpdateBy = UserName;
                        if (context.Commit() > 0)
                            responseResult = new ResponseResult { status = true, Message = "Profile Updated Successfully." };
                        else
                            responseResult = new ResponseResult { status = false, Message = "Some error occurred during Update Profile." };

                    }
                    else
                        responseResult = new ResponseResult { status = false, Message = "People Not found." };
                }
                catch
                {
                    return responseResult;
                }
            }

            return responseResult;

        }
    }
}



