using GenricEcommers.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/NewsApi")]
    public class NewsController : ApiController
    {
       
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //[Authorize]
        [Route("")]
        public IEnumerable<News> Get()
        {
            using (var context = new AuthContext())
            {
                logger.Info("start News: ");
                List<News> List = new List<News>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 1, userid = 0;
                    int Warehouse_id = 0;
                    string email = "";
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

                    }
                    logger.Info("End Get Company: ");
                    if (Warehouse_id > 0)
                    {
                        List = context.AllNewsWid(compid, Warehouse_id).ToList();
                        return List;
                    }
                    else
                    {
                        List = context.AllNews(compid).ToList();

                        return List;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in News " + ex.Message);
                    logger.Info("End  News: ");
                    return null;
                }
            }
        }


        [Route("")]
        public News Get(int id)
        {
            using (var context = new AuthContext())
            {
                logger.Info("start single News: ");
                News News = new News();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

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
                    logger.Info("in News");

                    News = context.GetNewsId(id, compid);
                    logger.Info("End Get News by item id: ");// + News.NewsName);
                    return News;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Get News by item id " + ex.Message);
                    logger.Info("End  single News: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(News))]
        [Route("")]
        [AcceptVerbs("POST")]
        public News add(News news)
        {
            using (var context = new AuthContext())
            {
                logger.Info("Add News: ");
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

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
                    logger.Info("in News");
                    if (news == null)
                    {
                        throw new ArgumentNullException("News");
                    }
                    news.CompanyId = compid;
                    context.AddNews(news);
                    logger.Info("End  Add News: ");
                    return news;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add News " + ex.Message);

                    return null;
                }
            }

        }

        //[ResponseType(typeof(News))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public News Put(News News)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

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
                    News.CompanyId = compid;
                    return context.PutNews(News);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Put News " + ex.Message);
                    return null;
                }
            }
        }


        ////[ResponseType(typeof(News))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public string Remove(int id)
        {
            using (var context = new AuthContext())
            {
                logger.Info("DELETE Remove: ");
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

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
                    if (context.DeleteNews(id, compid))
                    {
                        return "success";
                    }
                    else
                    {
                        return "error";
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Remove News " + ex.Message);
                    return "error";
                }
            }
        }

    }
}
