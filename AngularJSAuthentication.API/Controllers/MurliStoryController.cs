using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/MurliStory")]
    public class MurliStoryController : ApiController
    {
        int compid = 0, userid = 0;
        public MurliStoryController()
        {
            var identity = User.Identity as ClaimsIdentity;


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
        }

        [Route("GetBypager")]
        [HttpPost]
        public MurliList<MurliStory> GetBypager(MurliPager pager)
        {
            MurliList<MurliStory> vm = new MurliList<MurliStory>();
            try
            {
                using (var context = new AuthContext())
                {
                    vm.StoryList = context.MurliStoryDB.Where(x => x.IsActive == true && x.Deleted == false && (string.IsNullOrEmpty(pager.Keyword) || x.Title.ToLower().Contains(pager.Keyword))).OrderBy(x => x.Id).Skip(pager.Skip).Take(pager.Take).ToList();
                    vm.Count = context.MurliStoryDB.Where(x => string.IsNullOrEmpty(pager.Keyword) || x.Title.ToLower().Contains(pager.Keyword)).Count();
                }

                if (vm.StoryList != null && vm.StoryList.Count > 0)
                {
                    foreach (var story in vm.StoryList)
                    {
                        if (story.MurliStoryPageList != null && story.MurliStoryPageList.Count > 0)
                        {
                            story.MurliStoryPageList = story.MurliStoryPageList.OrderBy(x => x.PageNumber).ToList();
                        }
                    }
                }

                return vm;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Route("GetById/{storyId}")]
        [HttpGet]
        public MurliStory GetById(int storyId)
        {
            MurliStory story = null;
            try
            {

                using (var context = new AuthContext())
                {
                    story = context.MurliStoryDB.Include("MurliStoryPageList").Where(x => x.Id == storyId && x.IsActive == true && x.Deleted == false).FirstOrDefault();
                }
                if (story.MurliStoryPageList != null)
                {
                    story.MurliStoryPageList = story.MurliStoryPageList.Where(x => x.IsActive == true).ToList();
                    story.MurliStoryPageList = story.MurliStoryPageList.OrderBy(x => x.PageNumber).ToList();
                }
                return story;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Route("GetPublishedStory")]
        [HttpGet]
        public MurliStory GetPublishedStory(int warehouseId, int customerId)
        {
            MurliStory story = null;
            try
            {

                using (var context = new AuthContext())
                {
                    story = context.MurliStoryDB.Include("MurliStoryPageList").Where(x => x.IsPublished == true && x.IsActive == true && x.Deleted == false && (x.ValidFrom <= DateTime.Now && x.ValidTo >= DateTime.Now)).FirstOrDefault();
                }
                if (story.MurliStoryPageList != null)
                {
                    story.MurliStoryPageList = story.MurliStoryPageList.Where(x => x.IsActive == true && x.Deleted == false && x.IsPublished == true).ToList();
                    story.MurliStoryPageList = story.MurliStoryPageList.OrderBy(x => x.PageNumber).ToList();
                }
                return story;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        [Route("Publish/murliStoryId/{murliStoryId}/IsPublish/{IsPublish}")]
        public bool Publish(int murliStoryId, bool IsPublish)
        {
            try
            {

                using (var context = new AuthContext())
                {
                    context.Database.ExecuteSqlCommand("Update MurliStories SET IsPublished = 0 Where IsActive = 1");
                    context.Database.ExecuteSqlCommand("Update MurliStories SET IsPublished = " + (IsPublish ? 1 : 0).ToString() + " Where Id=" + murliStoryId.ToString());
                    return true;
                }

            }
            catch (Exception ex)
            {
                return true;
            }
        }

        [HttpGet]
        [Route("DeletePage/{murliStoryPageId}")]
        public bool DeletePage(int murliStoryPageId)
        {
            using (var context = new AuthContext())
            {
                var page = context.MurliStoryPageDB.FirstOrDefault(x => x.Id == murliStoryPageId);
                page.IsActive = false;
                page.Deleted = true;
                context.Commit();
                return true;
            }
        }



        [HttpPost]
        [Route("SaveMurliStory")]
        public MurliStory SaveMurliStory(MurliStory story)
        {
            try
            {


                using (var context = new AuthContext())
                {

                    if (story.MurliStoryPageList != null && story.MurliStoryPageList.Count > 0)
                    {
                        List<MurliStoryPage> pageList = new List<MurliStoryPage>();
                        int number = 1;
                        foreach (var page in story.MurliStoryPageList)
                        {

                            if (page.Id > 0)
                            {
                                MurliStoryPage dbStoryPage = context.MurliStoryPageDB.FirstOrDefault(x => x.Id == page.Id);
                                //dbStory.ImagePath = story.ImagePath;
                                dbStoryPage.UpdatedDate = DateTime.Now;
                                dbStoryPage.UpdatedBy = userid;
                                dbStoryPage.IsActive = page.IsActive;
                                dbStoryPage.Deleted = page.Deleted;
                                dbStoryPage.IsPublished = page.IsPublished;
                                dbStoryPage.PageNumber = number++;
                                pageList.Add(dbStoryPage);
                            }
                            else
                            {
                                page.MurliStoryId = story.Id;
                                page.CreatedBy = userid;
                                page.CreatedDate = DateTime.Now;
                                page.IsActive = true;
                                page.Deleted = false;
                                page.PageNumber = number++;
                                if (story.Id > 0)
                                {
                                    context.MurliStoryPageDB.Add(page);
                                    context.Commit();
                                }
                                pageList.Add(page);
                            }
                        }
                        story.MurliStoryPageList = pageList;
                    }

                    if (story.Id > 0)
                    {
                        MurliStory dbStory = context.MurliStoryDB.FirstOrDefault(x => x.Id == story.Id);
                        //dbStory.ImagePath = story.ImagePath;
                        dbStory.UpdatedDate = DateTime.Now;
                        dbStory.UpdatedBy = userid;
                        dbStory.IsActive = story.IsActive;
                        dbStory.Deleted = story.Deleted;
                        dbStory.ValidFrom = story.ValidFrom;
                        dbStory.ValidTo = story.ValidTo;
                        context.Commit();
                        return dbStory;
                    }
                    else
                    {
                        story.CreatedBy = userid;
                        story.CreatedDate = DateTime.Now;
                        story.IsActive = true;
                        story.Deleted = false;
                        context.MurliStoryDB.Add(story);
                        context.Commit();
                        return story;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }


        [Route("PostUserImage")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> PostUserImage()
        {
            string message1 = "";
            Dictionary<string, object> dict = new Dictionary<string, object>();
            try
            {

                var httpRequest = HttpContext.Current.Request;

                foreach (string file in httpRequest.Files)
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);

                    var postedFile = httpRequest.Files[file];
                    if (postedFile != null && postedFile.ContentLength > 0)
                    {

                        int MaxContentLength = 1024 * 1024 * 1; //Size = 1 MB  

                        IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png", ".jpeg" };
                        var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                        var extension = ext.ToLower();
                        if (!AllowedFileExtensions.Contains(extension))
                        {

                            var message = string.Format("Please Upload image of type .jpg,.gif,.png.");

                            dict.Add("error", message);
                            return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                        }
                        else if (postedFile.ContentLength > MaxContentLength)
                        {

                            var message = string.Format("Please Upload a file upto 1 mb.");

                            dict.Add("error", message);
                            return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                        }
                        else
                        {


                            string folderPath = "~/storyImage/";
                            string flieName = postedFile.FileName.Substring(0, postedFile.FileName.LastIndexOf('.')) + "_" + DateTime.Now.ToString("MMddyyyyHHMMss") + extension;
                            var filePath = HttpContext.Current.Server.MapPath(folderPath) + flieName;


                            bool exists = System.IO.Directory.Exists(System.Web.Hosting.HostingEnvironment.MapPath(folderPath));
                            if (!exists)
                                System.IO.Directory.CreateDirectory(System.Web.Hosting.HostingEnvironment.MapPath(folderPath));
                            postedFile.SaveAs(filePath);
                            AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(flieName, folderPath, filePath);

                            message1 = "/storyImage/" + flieName;
                        }
                    }


                    return Request.CreateErrorResponse(HttpStatusCode.Created, message1); ;
                }
                var res = string.Format("Please Upload a image.");
                dict.Add("error", res);
                return Request.CreateResponse(HttpStatusCode.NotFound, dict);
            }
            catch (Exception ex)
            {
                var res = string.Format("some Message");
                dict.Add("error", res);
                return Request.CreateResponse(HttpStatusCode.NotFound, dict);
            }
        }


        [Route("PostRaw")]
        [AllowAnonymous]
        public string PostRaw(dynamic text)
        {

            return "";
        }

    }
}
