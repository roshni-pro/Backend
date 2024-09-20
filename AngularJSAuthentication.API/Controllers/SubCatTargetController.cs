using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.Model;
using LinqKit;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/SubCatTarget")]
    public class SubCatTargetController : BaseAuthController
    {
        [Route("GetSSCBySubCatId")]
        [HttpGet]
        public HttpResponseMessage GetSSCBySubCatId(int subCatID)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var list1 = (from ssc in context.SubsubCategorys
                                 join sc in context.BrandCategoryMappingDb on ssc.SubsubCategoryid equals sc.SubsubCategoryId
                                 where sc.SubCategoryMappingId == subCatID
                                 select new
                                 {
                                     SubsubcategoryName = ssc.SubsubcategoryName,
                                     SubsubCategoryid = ssc.SubsubCategoryid
                                 }).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, list1);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("RewardItem")]
        [HttpGet]
        public HttpResponseMessage RewardItem()
        {
            List<int> warehouses_Ids = new List<int>();
            using (AuthContext context = new AuthContext())
            {
                var RewardItem = context.RewardItemsDb.Where(x => x.IsDeleted == false && x.IsActive == true).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, RewardItem);

            }
        }

        [Route("GetItemCitybasedSubcattarget")]
        [HttpGet]
        public HttpResponseMessage GetItemCitybased(int CityId, int SubCatId)
        {
            List<CityBaseItemSubCatTargetDc> cityBaseItemSubCatTargetDcs = new List<CityBaseItemSubCatTargetDc>();
            if (CityId > 0 && SubCatId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    List<SqlParameter> paramList = new List<SqlParameter>();
                    paramList.Add(new SqlParameter("@CityId", CityId));
                    paramList.Add(new SqlParameter("@SubCatId", SubCatId));
                    cityBaseItemSubCatTargetDcs = context.Database.SqlQuery<CityBaseItemSubCatTargetDc>("GetItemOnSubCatTargetDashboard @CityId, @SubCatId", paramList.ToArray()).ToList();
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, cityBaseItemSubCatTargetDcs);
        }

        [Route("PostSubCatTargetData")]
        [HttpPost]
        public TargetReturnResponse PostSubCatTargetData(SubCatTarget subCatTarget)
        {
            TargetReturnResponse targetReturnResponse = new TargetReturnResponse { status = true, message = "" };
            if (subCatTarget != null)
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                using (AuthContext context = new AuthContext())
                {

                    var brandids = string.Join(",", subCatTarget.subCatTargetBrands.Select(x => x.BrandIds).ToList()).Split(',').Select(x => Convert.ToInt32(x)).ToList();

                    DataTable cityDt = new DataTable();
                    cityDt.Columns.Add("IntValue");
                    foreach (var item in brandids)
                    {
                        DataRow dr = cityDt.NewRow();
                        dr[0] = item;
                        cityDt.Rows.Add(dr);
                    }

                    var param = new SqlParameter("brandIds", cityDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    var paramstartDate = new SqlParameter("startDate", subCatTarget.StartDate);
                    var paramendDate = new SqlParameter("endDate", subCatTarget.EndDate);
                    var paramsubCatId = new SqlParameter("subCategoryId", subCatTarget.SubCatId);
                    var paramcityid = new SqlParameter("cityId", subCatTarget.CityId);
                    var count = context.Database.SqlQuery<int>("exec [CheckTargetExists] @cityId,@subCategoryId,@startDate,@endDate,@brandIds", paramcityid, paramsubCatId, paramstartDate, paramendDate, param).FirstOrDefault();


                    if (count > 0)
                    {
                        targetReturnResponse.status = false;
                        targetReturnResponse.message = "Same City, SubCategory and Brand data already exists. Please check and correct.";

                    }

                    if (targetReturnResponse.status)
                    {
                        subCatTarget.CreatedBy = userid;
                        subCatTarget.CreatedDate = DateTime.Now;
                        subCatTarget.IsActive = true;
                        subCatTarget.IsDeleted = false;
                        subCatTarget.subCatTargetBrands.ToList().ForEach(x =>
                        {
                            x.IsActive = true;
                            x.CreatedBy = userid;
                            x.CreatedDate = DateTime.Now;
                        });

                        context.subCatTargets.Add(subCatTarget);
                        targetReturnResponse.status = context.Commit() > 0;
                        if (targetReturnResponse.status)
                            targetReturnResponse.message = "Target save successfully.";
                        else
                            targetReturnResponse.message = "During save some error occurred. please try after some time.";
                    }
                }

            }
            return targetReturnResponse;
        }


        [Route("CheckIfExist")]
        [HttpPost]
        public bool CheckIfExist(SubCatTargetDC CheckRecordobj)
        {
            using (AuthContext context = new AuthContext())
            {
                bool Exist = context.subCatTargets.Any(x => x.CityId == CheckRecordobj.CityId && x.SubCatId == CheckRecordobj.SubCategoryId && x.StartDate >= CheckRecordobj.StartDate && x.EndDate <= CheckRecordobj.EndDate && x.IsDeleted == false);
                return Exist;
            }
        }


        [Route("GetAllSubcategoryTarget")]
        [HttpPost]
        public CompanyTargetListDC GetAllSubcategoryTarget(TargetRequest targetRequest)
        {
            CompanyTargetListDC companyTargetListDC = new CompanyTargetListDC { totalItem = 0, CompanyTargetDCs = new List<CompanyTargetDC>() };
            using (AuthContext context = new AuthContext())
            {
                var Predicate = PredicateBuilder.New<SubCatTarget>(x => x.CityId == targetRequest.CityId && !x.IsDeleted.Value);
                if (targetRequest.SubCategoryId.HasValue && targetRequest.SubCategoryId > 0)
                    Predicate.And(x => x.SubCatId == targetRequest.SubCategoryId.Value);

                if (targetRequest.StartDate.HasValue && targetRequest.EndDate.HasValue)
                {
                    targetRequest.StartDate = targetRequest.StartDate.Value.Date;
                    targetRequest.EndDate = targetRequest.EndDate.Value.Date.AddDays(1).AddMilliseconds(-1);
                    Predicate.And(x => x.StartDate >= targetRequest.StartDate.Value && x.EndDate <= targetRequest.EndDate.Value);
                }
                if (targetRequest.Status.HasValue)
                    Predicate.And(x => x.IsActive == targetRequest.Status.Value);
                if (targetRequest.BrandId.Any())
                {
                    foreach (var item in targetRequest.BrandId)
                    {
                        string brandid = item.ToString();
                        Predicate.And(x => x.subCatTargetBrands.Any(y => y.BrandIds.Contains(brandid)));
                    }
                }
                var data = context.subCatTargets.Where(Predicate).Select(x => new CompanyTargetDC
                {
                    CityId = x.CityId,
                    EndDate = x.EndDate,
                    Id = x.Id,
                    StartDate = x.StartDate,
                    SubCategoryId = x.SubCatId,
                    IsCustomerSpacific = x.IsCustomerSpacific,
                    IsCustomerUploaded = x.SubCatTargetSpacificCusts.Any(y => y.IsActive),
                    brandList = x.subCatTargetBrands.Select(y => new brandList
                    {
                        brandids = y.BrandIds,
                        SubCatTargetBrandId = y.Id,
                        IsActive = y.IsActive
                    }).ToList()
                }).OrderByDescending(x => x.Id).Skip(targetRequest.skip).Take(targetRequest.take).ToList();

                if (data != null && data.Any())
                {
                    var subcategoryids = data.Select(x => x.SubCategoryId).Distinct().ToList();
                    var cityids = data.Select(x => x.CityId).Distinct().ToList();
                    var strbrandids = data.SelectMany(x => x.brandList.Select(y => y.brandids)).ToList();
                    var brandids = strbrandids.SelectMany(x => x.Split(',').Select(y => Convert.ToInt32(y))).Distinct().ToList();
                    var subcats = context.SubCategorys.Where(x => subcategoryids.Contains(x.SubCategoryId)).Select(x => new { x.SubcategoryName, x.SubCategoryId }).ToList();
                    var brands = context.SubsubCategorys.Where(x => brandids.Contains(x.SubsubCategoryid)).Select(x => new { x.SubsubCategoryid, x.SubsubcategoryName }).ToList();
                    var cityname = context.Cities.Where(x => cityids.Contains(x.Cityid)).Select(x => new { x.Cityid, x.CityName }).ToList();
                    foreach (var item in data)
                    {

                        if (subcats.Any(x => x.SubCategoryId == item.SubCategoryId))
                        {
                            item.SubCategoryName = subcats.FirstOrDefault(x => x.SubCategoryId == item.SubCategoryId).SubcategoryName;
                        }

                        if (cityname.Any(x => x.Cityid == item.CityId))
                        {
                            item.CityName = cityname.FirstOrDefault(x => x.Cityid == item.CityId).CityName;
                        }
                        foreach (var brand in item.brandList)
                        {
                            brandids = brand.brandids.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                            brand.BrandNames = string.Join(",", brands.Where(x => brandids.Contains(x.SubsubCategoryid)).Select(x => x.SubsubcategoryName).ToList());
                        }
                    }
                }
                companyTargetListDC.CompanyTargetDCs = data;
                companyTargetListDC.totalItem = context.subCatTargets.Count(Predicate);
            }
            return companyTargetListDC;

        }

        [Route("UploadSubCategoryCustomerTarget")]
        [HttpPost]
        public IHttpActionResult UploadSubCategoryCustomerTarget(int Id)
        {
            var msg = "";
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            System.Web.HttpPostedFile httpPostedFile = HttpContext.Current.Request.Files["file"];
            if (httpPostedFile != null)
            {
                var FileUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles"), httpPostedFile.FileName);
                // Validate the uploaded image(optional)
                byte[] buffer = new byte[httpPostedFile.ContentLength];

                using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
                {
                    br.Read(buffer, 0, buffer.Length);
                }
                XSSFWorkbook hssfwb;
                //   XSSFWorkbook workbook1;
                using (MemoryStream memStream = new MemoryStream())
                {
                    BinaryFormatter binForm = new BinaryFormatter();
                    memStream.Write(buffer, 0, buffer.Length);
                    memStream.Seek(0, SeekOrigin.Begin);
                    hssfwb = new XSSFWorkbook(memStream);
                }

                httpPostedFile.SaveAs(FileUrl);
                string sSheetName = hssfwb.GetSheetName(0);
                ISheet sheet = hssfwb.GetSheet(sSheetName);
                List<string> CustomerSkcodes = new List<string>();
                IRow rowData;
                ICell cellData = null;
                for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                {
                    rowData = sheet.GetRow(iRowIdx);
                    cellData = rowData.GetCell(0);
                    CustomerSkcodes.Add(cellData.ToString());
                }

                if (CustomerSkcodes != null && CustomerSkcodes.Any())
                {
                    using (AuthContext context = new AuthContext())
                    {
                        var subCatTargets = context.subCatTargets.FirstOrDefault(x => x.Id == Id && x.IsActive);
                        if (subCatTargets != null)
                        {
                            var customerids = context.Customers.Where(x => CustomerSkcodes.Contains(x.Skcode) && x.Cityid == subCatTargets.CityId).Select(x => x.CustomerId).ToList();
                            if (customerids != null && customerids.Any())
                            {
                                context.SubCatTargetSpacificCusts.AddRange(customerids.Select(x => new SubCatTargetSpacificCust
                                {
                                    CreatedBy = userid,
                                    CreatedDate = DateTime.Now,
                                    CustomerId = x,
                                    IsActive = true,
                                    IsDeleted = false,
                                    SubCatTargetId = Id
                                }));
                                if (context.Commit() > 0)
                                {
                                    msg = "Successfully uploaded for " + customerids.Count() + " target city customer";
                                    return Created<string>(msg, msg);
                                    //return Request.CreateResponse(HttpStatusCode.OK, msg);
                                }
                                else { 
                                msg = "Error during upload customer target.";
                                //return Request.CreateResponse(HttpStatusCode.OK, "Error during upload customer target.");
                                return Created<string>(msg, msg);
                                }
                            }
                            else
                            {
                                msg = "Uploaded customer not belowing to target city.";
                                //return Request.CreateResponse(HttpStatusCode.OK, "Uploaded customer not belowing to target city.");
                                return Created<string>(msg, msg);

                            }
                        }
                        else
                        {
                            msg = "Selected company target not active.";
                            //return Request.CreateResponse(HttpStatusCode.OK, "Selected company target not active.");
                            return Created<string>(msg, msg);

                        }
                    }
                }
                else
                {
                    msg = "Customer not found in uploaded file";
                    //return Request.CreateResponse(HttpStatusCode.OK, "Customer not found in uploaded file");
                    return Created<string>(msg, msg);

                }
            }
            else
            {
                msg = "Uploaded file not found";
                //return Request.CreateResponse(HttpStatusCode.OK, "Uploaded file not found");
                return Created<string>(msg, msg);

            }

        }

        [Route("GetBrandTarget")]
        [HttpGet]
        public List<BrandTargetDc> GetBrandTarget(long subCatTargetBrandId)
        {
            List<BrandTargetDc> brandTargetDcs = new List<BrandTargetDc>();
            using (AuthContext context = new AuthContext())
            {
                var subCatTargets = context.SubCatTargetDetails.Where(x => x.SubCatTargetBrandId == subCatTargetBrandId)
                      .Include(x => x.SubCatTargetLevelBrands)
                      .Include(x => x.SubCatTargetLevelFreeItems)
                      .Include(x => x.SubCatTargetLevelItems).ToList();
                if (subCatTargets != null && subCatTargets.Any())
                {
                    var brandids = subCatTargets.SelectMany(x => x.SubCatTargetLevelBrands.Select(y => y.BrandId)).ToList();
                    var brands = brandids.Any() ? context.SubsubCategorys.Where(x => brandids.Contains(x.SubsubCategoryid)).Select(x => new { x.SubsubCategoryid, x.SubsubcategoryName }).ToList() : null;
                    var itemskus = subCatTargets.SelectMany(x => x.SubCatTargetLevelItems.Select(y => y.SellingSku)).ToList();
                    var Freeitemskus = subCatTargets.Where(x => x.Type == "FreeItem").SelectMany(x => x.SubCatTargetLevelFreeItems.Select(y => y.SellingSku)).Distinct().ToList();
                    var FreeItemFromItemMasters = Freeitemskus.Any() ? context.itemMasters.Where(x => Freeitemskus.Contains(x.SellingSku)).Select(x => new { x.itemname, x.SellingSku }).Distinct().ToList() : null;
                    var items = itemskus.Any() ? context.itemMasters.Where(x => itemskus.Contains(x.Number)).Select(x => new { x.itemname, x.Number }).Distinct().ToList() : null;
                    var ritemids = subCatTargets.Where(x => x.Type == "DreamItem").SelectMany(x => x.SubCatTargetLevelFreeItems.Select(y => y.RewardItemId)).ToList();
                    var freeItem = ritemids.Any() ? context.RewardItemsDb.Where(x => x.IsActive && ritemids.Contains(x.rItemId)).Select(x => new { x.rName, x.rItemId }).ToList() : null;
                    brandTargetDcs = subCatTargets.Select(x => new BrandTargetDc
                    {
                        MinTargetValue = x.MinTargetValue,
                        NoOfLineItem = x.NoOfLineItem,
                        SlabLowerLimit = x.SlabLowerLimit,
                        SlabUpperLimit = x.SlabUpperLimit,
                        TargetbyValue = x.TargetbyValue,
                        Type = x.Type,
                        ValueBy = x.ValueBy,
                        WalletValue = x.WalletValue,
                        RequiredTargetBrandDcs = x.SubCatTargetLevelBrands.Select(y => new RequiredTargetBrandDc
                        {
                            BrandId = y.BrandId,
                            BrandName = brands != null && brands.Any(z => z.SubsubCategoryid == y.BrandId) ? brands.FirstOrDefault(z => z.SubsubCategoryid == y.BrandId).SubsubcategoryName : "",
                            Value = y.Value,
                            ValueType = y.ValueType
                        }).ToList(),
                        RequiredTargetItemDcs = x.SubCatTargetLevelItems.Select(y => new RequiredTargetItemDc
                        {
                            SellingSku = y.SellingSku,
                            ItemName = items != null && items.Any(z => z.Number == y.SellingSku) ? items.FirstOrDefault(z => z.Number == y.SellingSku).itemname : "",
                            Value = y.Value,
                            ValueType = y.ValueType
                        }).ToList(),
                        TargetFreeItemDcs = x.SubCatTargetLevelFreeItems.Select(y => new TargetFreeItemDc
                        {
                            Qty = y.Qty,
                            RewardItemId = y.RewardItemId,
                            SellingSku = y.SellingSku,
                            ItemName = x.Type == "FreeItem" ?
                                       (FreeItemFromItemMasters != null && FreeItemFromItemMasters.Any(z => z.SellingSku == y.SellingSku) ? FreeItemFromItemMasters.FirstOrDefault(z => z.SellingSku == y.SellingSku).itemname : "")
                                       : x.Type == "DreamItem" ? (
                                       freeItem != null && freeItem.Any(z => z.rItemId == y.RewardItemId) ? freeItem.FirstOrDefault(z => z.rItemId == y.RewardItemId).rName : ""
                                       ) : "",

                        }).ToList()
                    }).ToList();
                }
            }
            return brandTargetDcs;
        }


        [Route("GetBrandCustomerTarget")]
        [HttpGet]
        public async Task<List<SubCategoryTargetCustomerDc>> GetBrandCustomerTarget(long subCatTargetBrandId)
        {
            var subCategoryTargetCustomerDcs = new List<SubCategoryTargetCustomerDc>();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetBrandCustomerTarget]";
                cmd.Parameters.Add(new SqlParameter("@BrandTargetId", subCatTargetBrandId));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                subCategoryTargetCustomerDcs = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<SubCategoryTargetCustomerDc>(reader).ToList();
            }
            return subCategoryTargetCustomerDcs;
        }

        [Route("InactiveBrandCustomerTarget")]
        [HttpPost]
        public bool BrandCustomerActiveInactive(ActiveInactiveDc ActiveInactiveDcs)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var context = new AuthContext())
            {
                var updatedata = context.SubCatTargetBrands.Where(x => x.Id == ActiveInactiveDcs.SubCatTargetBrandId).FirstOrDefault();
                if (ActiveInactiveDcs.InActive == true)
                {
                    updatedata.IsActive = true;
                    updatedata.ModifiedBy = userid;
                    updatedata.ModifiedDate = DateTime.Now;
                    context.Entry(updatedata).State = EntityState.Modified;
                    context.Commit();
                }
                else
                {
                    updatedata.IsActive = false;
                    updatedata.ModifiedBy = userid;
                    updatedata.ModifiedDate = DateTime.Now;
                    context.Entry(updatedata).State = EntityState.Modified;
                    context.Commit();
                }
            }
            return true;
        }
    }


    public class TargetReturnResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
    }
    public class ActiveInactiveDc
    {
        public bool InActive { get; set; }
        public long SubCatTargetBrandId { get; set; }
    }

    public class CityBaseItemSubCatTargetDc
    {
        public int ItemMultiMRPId { get; set; }
        public int MinOrderQty { get; set; }
        public string itemname { get; set; }
        public string SellingSku { get; set; }
    }

    public class TargetRequest
    {
        public int CityId { get; set; }
        public int? SubCategoryId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<int> BrandId { get; set; }
        public bool? Status { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
    }

    public class CompanyTargetDC
    {
        public long Id { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCustomerSpacific { get; set; }
        public bool IsCustomerUploaded { get; set; }
        public List<brandList> brandList { get; set; }
    }
    public class brandList
    {
        public string BrandNames { get; set; }
        public string brandids { get; set; }
        public long SubCatTargetBrandId { get; set; }
        public bool IsActive { get; set; }
    }

    public class BrandTargetDc
    {
        public int SlabLowerLimit { get; set; }
        public int SlabUpperLimit { get; set; }
        public string ValueBy { get; set; }
        public decimal TargetbyValue { get; set; }
        public decimal MinTargetValue { get; set; }
        public int NoOfLineItem { get; set; }
        public string Type { get; set; } //WalletPoint/FreeItem/DreamItem
        public int WalletValue { get; set; }

        public List<RequiredTargetBrandDc> RequiredTargetBrandDcs { get; set; }

        public List<RequiredTargetItemDc> RequiredTargetItemDcs { get; set; }

        public List<TargetFreeItemDc> TargetFreeItemDcs { get; set; }
    }

    public class RequiredTargetBrandDc
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public string ValueType { get; set; }
        public int Value { get; set; }
    }

    public class RequiredTargetItemDc
    {
        public string SellingSku { get; set; }
        public string ItemName { get; set; }
        public string ValueType { get; set; }
        public int Value { get; set; }
    }

    public class TargetFreeItemDc
    {
        public string SellingSku { get; set; }
        public int? RewardItemId { get; set; } //Dream Item 
        public int Qty { get; set; }
        public string ItemName { get; set; }

    }


    public class CompanyTargetListDC
    {
        public int totalItem { get; set; }
        public List<CompanyTargetDC> CompanyTargetDCs { get; set; }
    }
    public class SubCatTargetDC
    {
        public int CityId { get; set; }
        public int SubCategoryId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<SubCatTargetDetailDC> SubCatTargetDetails { get; set; }
    }
    public class SubCatTargetDetailDC
    {
        public string Level { get; set; }
        public int SlabLowerLimit { get; set; }
        public int SlabUpperLimit { get; set; }
        public int TargetbyValue { get; set; }
        public int NoofBrand { get; set; }
        public int NoOfLineItem { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public List<SubCatTargetBrandDC> subCatTargetBrandLists { get; set; }
        public List<SubCatTargetItemDC> subCatTargetItemLists { get; set; }

    }
    public class SubCatTargetBrandDC
    {
        public int SubsubCategoryid { get; set; }
    }
    public class SubCatTargetItemDC
    {
        public int rItemId { get; set; }
        public string SellingSku { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string itemname { get; set; }
        public int MinOrderQty { get; set; }
        public int qty { get; set; }


    }
}
