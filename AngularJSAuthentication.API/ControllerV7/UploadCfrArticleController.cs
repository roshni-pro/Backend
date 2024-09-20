using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AngularJSAuthentication.Model;
using NLog;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Web;
using System.Web.Script.Serialization;
using System.Text;
using System.Data.SqlClient;
using System.Data.Entity;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/UploadCfrArticle")]
    public class UploadCfrArticleController : ApiController
    {

        [HttpPost]
        public string UploadCfrArticle(List<uploaditemlistDTO> uploadCollection)
        {
            string msg;

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            bool IsUploaded = false;
            if (uploadCollection.Count > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    bool status = false;
                    StringBuilder stringBuilder = new StringBuilder();
                    List<uploaditemlistDTO> uploadItem = null;
                    List<string> remainingitemNumbers = new List<string>();
                    List<string> itemmaster = new List<string>();
                    var subcatid = uploadCollection.FirstOrDefault().SubCatId;
                    var itemnumbers = uploadCollection.Select(x => x.ItemNumber).Distinct().ToList();
                    if (uploadCollection.Any(x => x.SubCatId > 0))
                    {
                        itemmaster = context.ItemMasterCentralDB.Where(x => x.SubCategoryId == subcatid && itemnumbers.Contains(x.Number)).Select(x => x.Number).Distinct().ToList();
                    }
                    else
                    {
                        itemmaster = context.ItemMasterCentralDB.Where(x => itemnumbers.Contains(x.Number)).Select(x => x.Number).Distinct().ToList();
                    }
                    remainingitemNumbers = itemnumbers.Except(itemmaster).ToList();
                    if (remainingitemNumbers != null && remainingitemNumbers.Any())
                    {
                        status = false;
                        msg = string.Join(" ,", remainingitemNumbers) + " " + "this item number is not belong to selected SubCategory";
                        return msg;
                    }
                    else
                    {
                        status = true;
                    }
                    if(status == true)
                    {
                        IsUploaded = UploadCFr(uploadCollection, userid, context);
                    }                
                }
            }
            if (IsUploaded)
            {
                msg = "Your Excel data is uploaded succesfully.";
                return msg;
            }
            else
            {
                msg = "Something went wrong";
                return msg;
            }
        }

        internal bool UploadCFr(List<uploaditemlistDTO> Uploadcollection, int userid, AuthContext context)
        {
            int PeopleId = Convert.ToInt32(userid);
            var User = context.Peoples.FirstOrDefault(x => x.PeopleID == PeopleId);
            List<CfrArticle> AddCfrArticleList = new List<CfrArticle>();          
                if (User.PeopleID > 0 && Uploadcollection != null && Uploadcollection.Any())
            {
                string Query = "";
                //string Query = "truncate table CfrArticles;";
                if (Uploadcollection.Any(x => x.SubCatId > 0))
                {
                    var subcatid = Uploadcollection.FirstOrDefault().SubCatId;
                    var cityid = Uploadcollection.FirstOrDefault().CityId;
                    Query = "delete from CfrArticles where SubCatId =" + subcatid + " " + "and CityId =" + cityid;
                }
                else
                {
                    var cityid = Uploadcollection.FirstOrDefault().CityId;
                    Query = "delete from CfrArticles where SubCatId = 0" + " " + "and CityId =" + cityid;
                }
                var _result = context.Database.ExecuteSqlCommand(Query);

                foreach (var o in Uploadcollection)
                {
                    CfrArticle articleobj = new CfrArticle();
                    if (o.ItemNumber != null)
                    {
                        articleobj.ItemNumber = o.ItemNumber;
                        //articleobj.WarehouseId = o.WarehouseId;
                        articleobj.SubCatId = o.SubCatId;
                        articleobj.CreatedDate = DateTime.Now;
                        articleobj.ModifiedDate = DateTime.Now;
                        articleobj.ModifiedBy = User.PeopleID;
                        articleobj.CreatedBy = User.PeopleID;
                        articleobj.IsActive = true;
                        articleobj.IsDeleted = false;
                        articleobj.CityId = o.CityId;
                        AddCfrArticleList.Add(articleobj);
                    }
                }
                context.CfrArticles.AddRange(AddCfrArticleList);
            }
            if (context.Commit() > 0) { return true; } else { return false; }
        }


        [HttpGet]
        [Route("GetCfrArticleData")]
        public List<GetCfrArticleDataDTO> GetCfrArticleData( int cityid,int Subcategoryid)
        {
            List<GetCfrArticleDataDTO>  data = new List<GetCfrArticleDataDTO>();
            using (var context = new AuthContext())
            {
                var cityids = new SqlParameter("@cityid", cityid);
                var Subcategoryids = new SqlParameter("@Subcategoryid", Subcategoryid);
                data = context.Database.SqlQuery<GetCfrArticleDataDTO>("exec GetCfrArticleData @cityid,@Subcategoryid", cityids,Subcategoryids).ToList();
            }
                return data;
        }

        [HttpGet]
        [Route("GetCfrAddItemList")]
        public List<GetCfrAddItemListDTO> GetCfrAddItemList(int cityid) {
            List<GetCfrAddItemListDTO> data = new List<GetCfrAddItemListDTO>();
            using (var context = new AuthContext())
            {
                var cityids = new SqlParameter("@cityid", cityid);
                data = context.Database.SqlQuery<GetCfrAddItemListDTO>("exec GetCfrAddItemList" +
                    " @cityid", cityids).ToList();
            }
            return data;
        }

        [HttpGet]
        [Route("RemoveCfrArticleData")]
        public string RemoveCfrArticleData(int Id)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

              
                using (AuthContext context = new AuthContext())
                {
                    var res = "";
                    if (Id > 0)
                    {
                        var ExistData = context.CfrArticles.FirstOrDefault(x => x.Id == Id);
                        if (ExistData != null)
                        {
                            ExistData.IsActive = false;
                            ExistData.IsDeleted = true;
                            ExistData.ModifiedBy = userid;
                            ExistData.ModifiedDate = DateTime.Now;
                            context.Entry(ExistData).State = EntityState.Modified;
                        }

                        if (context.Commit() > 0)
                        {
                            res = "Removed succesfully!";
                        }
                        else
                        {
                            res = "Try again later!";
                        }
                    }
                    return res;
                }
            }
            catch (Exception ex)
            {
                return ex.ToString(); 
            };
        }

        [HttpPost]
        [Route("AddCfrArticleData")]
        public string AddCfrArticleData(List<uploaditemlistDTONew> AddArticle) {
            var res = "";
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (AuthContext db = new AuthContext())
            {
                foreach (var r in AddArticle)

                {
                    var checkArticle = false;
                    checkArticle = db.CfrArticles.Any(x => x.ItemNumber== r.ItemNumber && x.CityId== r.CityId && x.IsActive==true && x.IsDeleted==false);
                    if (!checkArticle)
                    {
                        CfrArticle cfrArticle = new CfrArticle();
                        cfrArticle.ItemNumber = r.ItemNumber;
                        cfrArticle.CityId = r.CityId;
                        cfrArticle.SubCatId = r.SubCategoryId;
                        cfrArticle.CreatedDate = DateTime.Now;
                        cfrArticle.IsActive = true;
                        cfrArticle.IsDeleted = false;
                        cfrArticle.CreatedBy = userid;
                        db.CfrArticles.Add(cfrArticle);
                      
                    }
                    else
                    {
                        res = "Already exist!";
                    }
                }
                if (db.Commit() > 0 && res=="")
                {
                    res = "Successfully added!";
                }
                else
                {
                    res = res == ""?"Error!":res;
                }
            }

            return res;
        }

    }
}

    public class uploaditemlistDTO
    {
        public string ItemNumber { get; set; }
        //public int WarehouseId { get; set; }
        public int CityId { get; set; }
        public int SubCatId { get; set; } //0 for Sk , 0< then for Brand
    }

public class uploaditemlistDTONew
{
    public string ItemNumber { get; set; }
    //public int WarehouseId { get; set; }
    public int CityId { get; set; }
    public int SubCategoryId { get; set; } //0 for Sk , 0< then for Brand
}

public class GetCfrArticleDataDTO
    {
        public long articleId { get; set; }
        public string SubcategoryName { get; set; }
        public string itemname { get; set; }
        public string ItemNumber { get; set; }
        public string CityName { get; set; }
        public DateTime? CreatedDate{ get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class GetCfrAddItemListDTO
    {

    public int SubCategoryId { get; set; }
    public string SubcategoryName { get; set; }
    public string itemname { get; set; }
    public string ItemNumber { get; set; }
    public string CityName { get; set; }
    public int Cityid{ get; set; }
}




