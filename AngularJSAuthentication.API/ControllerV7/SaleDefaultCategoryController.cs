using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using AgileObjects.AgileMapper;
using AngularJSAuthentication.Model;

namespace AngularJSAuthentication.API.ControllerV7
{
    [RoutePrefix("api/SaleDefaultCategory")]
    public class SaleDefaultCategoryController : ApiController
    {
        [Route("category")]
        [HttpPost]
        public ResSaleDefaultCategoryDc category(SaleDefaultCategoryPostDc item)
        {
            ResSaleDefaultCategoryDc res = new ResSaleDefaultCategoryDc();
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                //SaleDefaultCategory SaleDefaultCategory = new SaleDefaultCategory();

                if (item.ItemList.Count > 0)
                {
                    foreach (var obj in item.ItemList)
                    {
                        var salescategory = context.SaleDefaultCategories.FirstOrDefault(x => x.CategoryId == obj.CategoryId && x.SubCategoryId == obj.SubCategoryId && x.SubSubCategoryId == obj.SubSubCategoryId && x.IsDeleted == false);
                        if (salescategory != null)
                        {
                            salescategory.CategoryId = obj.CategoryId;
                            salescategory.SubCategoryId = obj.SubCategoryId;
                            salescategory.SubSubCategoryId = obj.SubSubCategoryId;
                            salescategory.ModifiedDate = DateTime.Now;
                            salescategory.ModifiedBy = userid;
                            context.Entry(salescategory).State = EntityState.Modified;
                            context.Commit();
                        }
                        else
                        {
                            var SaleDefaultCategory = new SaleDefaultCategory
                            {
                                CategoryId = obj.CategoryId,
                                SubCategoryId = obj.SubCategoryId,
                                SubSubCategoryId = obj.SubSubCategoryId,
                                CreatedBy = userid,
                                CreatedDate = DateTime.Now,
                                IsActive = true,
                                IsDeleted=false
                            };
                            context.SaleDefaultCategories.Add(SaleDefaultCategory);
                        }
                    }
                    context.Commit();
                    res.Result = true;
                    res.msg = "Saved successfully";
                }
                else
                {
                    res.Result = false;
                    res.msg = "failed";
                }
            }
            return res;
        }

        [Route("BrandRemove")]
        [HttpGet]
        public ResSaleDefaultCategoryDc BrandRemove(int Id)
        {
            ResSaleDefaultCategoryDc res = new ResSaleDefaultCategoryDc();
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
               
                var Branddata = context.SaleDefaultCategories.FirstOrDefault(x => x.Id == Id && x.IsDeleted == false && x.IsActive == true);
                
                if(Branddata != null)
                {
                    Branddata.IsActive = false;
                    Branddata.IsDeleted = true;
                    Branddata.ModifiedBy = userid;
                    Branddata.ModifiedDate = DateTime.Now;

                    context.Entry(Branddata).State = EntityState.Modified;
                    context.Commit();

                    res.Result = true;
                    res.msg = "success";
                }
                else
                {
                    res.Result = false;
                    res.msg = "failed";
                }

            }
            return res;
        }

        [Route("GetSaleDefaultCategoryList")]
        [HttpGet]
        public ResSaleDefaultCategoryDc GetSaleCategoryList()
        {
            ResSaleDefaultCategoryDc res = new ResSaleDefaultCategoryDc();

            using (var context = new AuthContext())
            {
                List<SaleDefaultCategoryDc> List = new List<SaleDefaultCategoryDc>();
                var list = context.SaleDefaultCategories.Where(x => x.IsDeleted == false && x.IsActive == true).OrderByDescending(x=>x.Id).ToList();
                List = Mapper.Map(list).ToANew<List<SaleDefaultCategoryDc>>();

                var cateids = List.Select(x => x.CategoryId).Distinct().ToList();
                var subcateids = List.Select(x => x.SubCategoryId).Distinct().ToList();
                var subsubcateids = List.Select(x => x.SubSubCategoryId).Distinct().ToList();

                var catname = context.Categorys.Where(x => cateids.Contains(x.Categoryid)).ToList();
                var subcatname = context.SubCategorys.Where(x => subcateids.Contains(x.SubCategoryId)).ToList();
                var subsubcatname = context.SubsubCategorys.Where(x => subsubcateids.Contains(x.SubsubCategoryid)).ToList();

                List.ForEach(x =>
                {
                    x.CategoryName = catname.Where(y => y.Categoryid == x.CategoryId).Select(y => y.CategoryName).FirstOrDefault();
                    x.SubCategoryName = subcatname.Where(y => y.SubCategoryId == x.SubCategoryId).Select(y => y.SubcategoryName).FirstOrDefault();
                    x.SubSubCategoryName = subsubcatname.Where(y => y.SubsubCategoryid == x.SubSubCategoryId).Select(y => y.SubsubcategoryName).FirstOrDefault();
                });

                res.SaleDefaultCategoryDcs = List;
                res.Result = true;
            }
            return res;
        }
    }
}

public class SaleDefaultCategoryPostDc
{
    public List<SaleDefaultCategory> ItemList { get; set; }
}
public class ResSaleDefaultCategoryDc
{
    public int totalcount { get; set; }
    public dynamic res { get; set; }
    public List<SaleDefaultCategoryDc> SaleDefaultCategoryDcs { get; set; }
    public bool Result { get; set; }
    public string msg { get; set; }
}
public class SaleDefaultCategoryDc
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public int SubCategoryId { get; set; }
    public int SubSubCategoryId { get; set; }
    public string CategoryName { get; set; }
    public string SubCategoryName { get; set; }
    public string SubSubCategoryName { get; set; }
}
