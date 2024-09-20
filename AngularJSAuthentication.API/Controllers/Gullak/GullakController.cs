using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.BusinessLayer.Managers.Transactions;
using AngularJSAuthentication.DataContracts.KPPApp;
using AngularJSAuthentication.DataContracts.Transaction;
using AngularJSAuthentication.Model.Gullak;
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
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Gullak")]
    public class GullakController : BaseAuthController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("GetGullak")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<GullakPageData> GetGullak(GetGullak getGullak)
        {
            var manager = new GullakManager();
            return await manager.GetGullak(getGullak);
        }

        //[Route("GetGullakTransaction")]
        //[HttpPost]
        //public async Task<GullakTransactionPageData> GetGullakTransaction(GetGullakTransaction getGullakTransaction)
        //{
        //    var manager = new GullakManager();
        //    return await manager.GetGullakTransaction(getGullakTransaction);
        //}

        [Route("GetGullakTransaction")]
        [HttpPost]
        public async Task<GullakTransactionPageData> GetGullakTransaction(GetGullakTransaction getGullakTransaction)
        {
            using (var context = new AuthContext())
            {
                GullakTransactionPageData result = new GullakTransactionPageData();

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[dbo].[GullakTransactionGetList]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@Skip", getGullakTransaction.Skip));
                cmd.Parameters.Add(new SqlParameter("@Take", getGullakTransaction.Take));
                cmd.Parameters.Add(new SqlParameter("@GullakId", getGullakTransaction.GullakId));
                cmd.Parameters.Add(new SqlParameter("@CustomerId", getGullakTransaction.CustomerId));

                var reader = cmd.ExecuteReader();
                var data = ((IObjectContextAdapter)context).ObjectContext.Translate<GullakTransactionDc>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    result.total_count = Convert.ToInt32(reader["TotalRecords"]);
                }
                result.GullakTransactionDc = data;
                return result;
            }
        }



        [Route("GetExportGullakTransaction")]
        [HttpPost]
        public async Task<GullakTransactionPageData> GetExportGullakTransaction(GetGullakTransaction getGullakTransaction)
        {
            var manager = new GullakManager();
            return await manager.GetExportGullakTransaction(getGullakTransaction);
        }
        [Route("PaymentResponse")]
        [AcceptVerbs("POST")]
        public async Task<bool> PaymentResponse(AddGullakPayment addGullakPayment)
        {
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                List<int> InvalidOrderId = new List<int>();
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var Inpayment = db.GullakDB.Where(x => x.Id == addGullakPayment.id && x.CustomerId == addGullakPayment.CustomerId && x.IsActive == true).FirstOrDefault();
                if (Inpayment != null)
                {
                    GullakInPayment gullakInPayment = new GullakInPayment();
                    gullakInPayment.GullakId = Inpayment.Id;
                    gullakInPayment.amount = addGullakPayment.Amount;
                    gullakInPayment.Comment = addGullakPayment.comment;
                    gullakInPayment.PaymentFrom = addGullakPayment.PaymentFrom;
                    gullakInPayment.GatewayTransId = addGullakPayment.GatewayTransId;
                    gullakInPayment.GatewayRequest = addGullakPayment.GatewayRequest;
                    gullakInPayment.GullakImage = addGullakPayment.GullakImage;
                    gullakInPayment.IsActive = true;
                    gullakInPayment.IsDeleted = false;
                    gullakInPayment.CreatedBy = userid;
                    gullakInPayment.CreatedDate = indianTime;
                    gullakInPayment.CustomerId = Inpayment.CustomerId;
                    gullakInPayment.status = "fromBackend";

                    db.GullakInPaymentDB.Add(gullakInPayment);

                    GullakTransaction gullakTransaction = new GullakTransaction();

                    gullakTransaction.Amount = addGullakPayment.Amount;
                    gullakTransaction.CreatedBy = userid;
                    gullakTransaction.CreatedDate = indianTime;
                    gullakTransaction.CustomerId = addGullakPayment.CustomerId;
                    gullakTransaction.GullakId = Inpayment.Id;
                    gullakTransaction.IsActive = true;
                    gullakTransaction.IsDeleted = false;
                    gullakTransaction.Comment = !string.IsNullOrEmpty(addGullakPayment.comment) ? addGullakPayment.comment : "Advance Payment";
                    gullakTransaction.ObjectType = addGullakPayment.PaymentFrom;
                    gullakTransaction.ObjectId = addGullakPayment.GatewayTransId + "---" + addGullakPayment.GatewayRequest;

                    db.GullakTransactionDB.Add(gullakTransaction);

                    //add to main amount 
                    Inpayment.TotalAmount = Inpayment.TotalAmount + addGullakPayment.Amount;
                    Inpayment.ModifiedDate = DateTime.Now;
                    Inpayment.ModifiedBy = userid;
                    db.Entry(Inpayment).State = EntityState.Modified;
                }
                if (db.Commit() > 0) { return true; } else { return false; }

            }
        }
        public async Task<double> GetGullakCashBack(int customerId, double amount)
        {
            double cashBackAmount = 0;
            using (var context = new AuthContext())
            {
                string query = "Exec GetGullakCashBack " + customerId + "," + amount;
                cashBackAmount = context.Database.SqlQuery<double>(query).FirstOrDefault();
            }
            return cashBackAmount;
        }

        #region image upload 
        /// <summary>
        ///  image upload 
        /// </summary>
        /// <returns></returns>
        [Route("uploadgullak")]
        [HttpPost]
        public IHttpActionResult uploadgullak()
        {
            string ImageUrl = "";
            bool status = false;
            string resultMessage = "";
            var SaveUrl = "";
            using (AuthContext context = new AuthContext())
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        var fileuploadPath = HttpContext.Current.Server.MapPath("~/GullakImages/");

                        if (!Directory.Exists(fileuploadPath))
                        {
                            Directory.CreateDirectory(fileuploadPath);
                        }
                        string extension = Path.GetExtension(httpPostedFile.FileName);

                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                        ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/GullakImages/"), fileName);
                        httpPostedFile.SaveAs(ImageUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/GullakImages", ImageUrl);

                        ImageUrl = "/GullakImages/" + fileName;
                        SaveUrl = "/GullakImages/" + fileName;
                    }
                }
                else
                {
                    ImageUrl = null;

                }

                ImageUrl = SaveUrl;
                return Created<string>(ImageUrl, ImageUrl);

                // return ImageUrl;
            }
        }
        #endregion

        [Route("update")]
        [HttpPut]
        public IHttpActionResult update(GullakCashBack gkcash)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (AuthContext context = new AuthContext())
            {

                GullakCashBack gk = context.GullakCashBackDB.Where(c => c.WarehouseId.Equals(gkcash.WarehouseId) && c.IsDeleted == false).FirstOrDefault();
                //var countryname = context.Countries.Where(x => x.CountryId == Zon.CountryId).Select(y => y.CountryName).FirstOrDefault();
                //People people = context.Peoples.Where(c => c.PeopleID.Equals(Zon.CountryId) && c.Deleted == false).SingleOrDefault();

                var gks = context.GullakCashBackDB.Where(c => c.WarehouseId == gkcash.WarehouseId).FirstOrDefault();
                // if (gks == null)

                try
                {


                    gk.WarehouseId = gkcash.WarehouseId;
                    gk.StartDate = gkcash.StartDate;
                    gk.EndDate = gkcash.EndDate;
                    gk.MaximumCashBackAmount = gkcash.MaximumCashBackAmount;
                    gk.CashBackPercent = gkcash.CashBackPercent;
                    gk.AmountFrom = gkcash.AmountFrom;
                    gk.AmountTo = gkcash.AmountTo;
                    gk.ModifiedBy = userid;
                    gk.ModifiedDate = DateTime.Now;
                    gk.IsActive = true;
                    gk.IsDeleted = false;
                    gk.CreatedDate = DateTime.Now;
                    gk.CreatedBy = userid;
                    //context.zone.Attach(Zon);
                    //context.SaveChanges();
                    context.Entry(gk).State = EntityState.Modified;
                    context.Commit();
                    return Ok(gk);
                }
                catch (Exception ex)
                {
                    return null;
                }



            }


        }

        [AllowAnonymous]
        [Route("saveGullak")]
        [HttpPost]

        public GullakCashBack insert(GullakCashBack gullak)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


            using (var db = new AuthContext())
            {


                var WarehouseId = new SqlParameter
                {
                    ParameterName = "WarehouseId",
                    Value = gullak.WarehouseId,

                };
                var StartDate = new SqlParameter
                {
                    ParameterName = "StartDate",
                    Value = gullak.StartDate,

                };
                var EndDate = new SqlParameter
                {
                    ParameterName = "EndDate",
                    Value = gullak.EndDate,

                };
                var AmountFrom = new SqlParameter
                {
                    ParameterName = "AmountFrom",
                    Value = gullak.AmountFrom,

                };
                var AmountTo = new SqlParameter
                {
                    ParameterName = "AmountTo",
                    Value = gullak.AmountTo,

                };

                var AmountToop = new SqlParameter
                {
                    ParameterName = "AmountToop",
                    DbType = DbType.Double,
                    Value = 0,
                    Direction = ParameterDirection.Output
                };


                db.Database.ExecuteSqlCommand("GullakCashBackCheck @WarehouseId,@StartDate,@EndDate,@AmountFrom,@AmountToOP out ", WarehouseId,
                          StartDate, EndDate, AmountFrom, AmountToop);


                double resultamnt = (double)AmountToop.Value;


                GullakCashBack gullakcashlist = new GullakCashBack();

                if (gullak.AmountTo >= resultamnt)
                {

                    gullakcashlist.WarehouseId = gullak.WarehouseId;
                    gullakcashlist.StartDate = gullak.StartDate;
                    gullakcashlist.EndDate = gullak.EndDate;
                    gullakcashlist.MaximumCashBackAmount = gullak.MaximumCashBackAmount;
                    gullakcashlist.CashBackPercent = gullak.CashBackPercent;
                    gullakcashlist.AmountFrom = gullak.AmountFrom;
                    gullakcashlist.AmountTo = gullak.AmountTo;
                    gullakcashlist.ModifiedBy = userid;
                    gullakcashlist.ModifiedDate = DateTime.Now;
                    gullakcashlist.IsActive = true;
                    gullakcashlist.IsDeleted = false;
                    gullakcashlist.CreatedDate = DateTime.Now;
                    gullakcashlist.CreatedBy = userid;
                    gullakcashlist.IsVerify = true;
                    db.GullakCashBackDB.Add(gullakcashlist);
                    db.Commit();
                    return gullakcashlist;


                }
                else
                {
                    gullakcashlist.IsVerify = false;
                    return null;
                }


            }

        }

        [AllowAnonymous]
        [Route("addNewGullak")]
        [HttpGet]
        public responceDc AddNewGullak(string skCode)
        {
            responceDc res = new responceDc();
            using (var db = new AuthContext())
            {

                var customer = db.Customers.Where(x => x.Skcode == skCode.ToUpper().Trim()).FirstOrDefault();
                if (customer != null && skCode!=null)
                {
                    var ifExist = db.GullakDB.FirstOrDefault(x=>x.CustomerId==customer.CustomerId && x.IsActive==true && x.IsDeleted==false);
                    if(ifExist==null)
                    {
                        var Skcode = new SqlParameter("@skcode", skCode);
                        db.Database.ExecuteSqlCommand("EXEC addNewGullak @skcode", Skcode);
                        res.Status = true;
                        res.Message = "Gullak Added Successfully";
                    }
                    else
                    {
                        res.Status = false;
                        res.Message = "Skcode already Exists!!";
                    }
                }
                else
                {
                    res.Status = false;
                    res.Message = "Skcode not Found";
                }
                return res;
            }
        }

        [AllowAnonymous]
        [Route("Search")]
        [HttpGet]

        public dynamic GetInvent(int? warehouseid, DateTime? StartDate, DateTime? EndDate)
        {
            using (var db = new AuthContext())
            {

                if (warehouseid == null)
                {
                    warehouseid = 0;
                }


                string Start = StartDate?.ToString("MM/DD/YYYY");
                string End = EndDate?.ToString("MM/DD/YYYY");
                DateTime todate = EndDate.Value.AddDays(1);

                var Id = new SqlParameter("warehouseid", warehouseid ?? (object)DBNull.Value);
                var Fromdate = new SqlParameter("StartDate", StartDate ?? (object)DBNull.Value);
                var Todate = new SqlParameter("EndDate", EndDate ?? (object)DBNull.Value);


                List<GullakCashBack> data = db.GullakCashBackDB.Where(x => x.WarehouseId == warehouseid && x.StartDate >= StartDate && x.EndDate <= EndDate).ToList();


                return data;
            }

        }


        /// <summary>
        /// neha
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="IsActive"></param>
        /// <returns></returns>
        [Route("Activegullak")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<GullakCashBack> Activegullak(long Id, bool IsActive)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var context = new AuthContext())
            {
                GullakCashBack gullakcashlist = new GullakCashBack();



                GullakCashBack gullak = context.GullakCashBackDB.Where(x => x.Id == Id && x.IsDeleted == false).FirstOrDefault();
                if (gullak != null)
                {
                    gullak.IsActive = IsActive;
                    gullak.CreatedBy = userid;
                    context.Commit();

                }
                return gullak;
            }

        }

        //Get List of All Pending Status of that GullakId And CustomerId // Startpkz
        [AllowAnonymous]
        [Route("getPendingGullakDetail")]
        [HttpPost]
        public dynamic getPendingGullakDetail(GetGullakTransaction getGullakTransaction)
        {
            using (var context = new AuthContext())
            {
                var result = context.GullakInPaymentDB.Where(x => x.GullakId == getGullakTransaction.GullakId && x.CustomerId == getGullakTransaction.CustomerId && x.status == "Pending").ToList();
                return result;
            }
        }
        //End
        //Get List of All Pending Status // Startpkz
        [AllowAnonymous]
        [Route("getPendingStatus")]
        [HttpPost]
        public List<GullakInPaymentDc> getPendingStatus()
        {
            using (var context = new AuthContext())
            {
                var result = context.GullakInPaymentDB.Where(x => x.status == "Pending").OrderByDescending(x => x.CreatedDate).ToList();
                var GullakInPaymentDcs = Mapper.Map(result).ToANew<List<GullakInPaymentDc>>();
                if (GullakInPaymentDcs != null && GullakInPaymentDcs.Any())
                {
                    var customerId = GullakInPaymentDcs.Select(x => x.CustomerId).Distinct().ToList();
                    var customers = context.Customers.Where(x => customerId.Contains(x.CustomerId)).Select(x => new { x.CustomerId, x.Skcode, x.Name, x.ShopName, x.Mobile }).ToList();
                    string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                    foreach (var item in GullakInPaymentDcs)
                    {
                        if (customers.Any(x => x.CustomerId == item.CustomerId))
                        {
                            item.Name = customers.FirstOrDefault(x => x.CustomerId == item.CustomerId).Name;
                            item.ShopName = customers.FirstOrDefault(x => x.CustomerId == item.CustomerId).ShopName;
                            item.SKcode = customers.FirstOrDefault(x => x.CustomerId == item.CustomerId).Skcode;
                            item.Mobile = customers.FirstOrDefault(x => x.CustomerId == item.CustomerId).Mobile;
                        }
                        item.GullakImage = baseUrl + "/" + item.GullakImage;
                    }
                }

                return GullakInPaymentDcs;
            }
        }
        //End

        //For Single Id whose status can be changed from Pending in only GullakInPaymentDB Table //Startpkz
        [AllowAnonymous]
        [Route("ChangePendingStatus")]
        [HttpPut]
        public dynamic ChangePendingStatus(GullakPendingDc payment)
        {
            using (var db = new AuthContext())
            {
                var gullakpending = db.GullakInPaymentDB.Where(x => x.GullakId == payment.GullakId && x.CustomerId == payment.CustomerId && x.status == "Pending").FirstOrDefault();
                if (gullakpending.GullakId == payment.GullakId)
                {
                    gullakpending.GatewayTransId = payment.GatewayTransId;
                    gullakpending.IsActive = true;
                    gullakpending.status = payment.status;
                    gullakpending.ModifiedBy = payment.CustomerId;
                    gullakpending.ModifiedDate = DateTime.Now;
                    db.Entry(gullakpending).State = EntityState.Modified;
                }
                if (db.Commit() > 0)
                {
                    return gullakpending.Id;
                }
                else
                {
                    return 0;
                }
            }
        }
        //End

        //For Single Id whose status can be changed from Pending //Startpkz
        [AllowAnonymous]
        [Route("ChangePendingStatusV2")]
        [HttpPut]
        public async Task<bool> ChangePendingStatusV2(GullakPendingDc payment)
        {
            try
            {
                using (var db = new AuthContext())
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    List<int> InvalidOrderId = new List<int>();
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    var Inpayment = db.GullakDB.Where(x => x.Id == payment.GullakId && x.CustomerId == payment.CustomerId && x.IsActive == true).FirstOrDefault();
                    var gullakpending = db.GullakInPaymentDB.Where(x => x.GullakId == payment.GullakId && x.CustomerId == payment.CustomerId && x.status == "Pending").FirstOrDefault();
                    if (gullakpending.GullakId == payment.GullakId)
                    {
                        gullakpending.GatewayTransId = payment.GatewayTransId;
                        //gullakpending.IsActive = payment.IsActive;
                        gullakpending.status = payment.status;
                        gullakpending.ModifiedBy = userid;
                        gullakpending.amount = payment.Amount;
                        gullakpending.CustomerId = payment.CustomerId;
                        gullakpending.Comment = !string.IsNullOrEmpty(payment.comment) ? payment.comment : "Advance Payment";
                        gullakpending.PaymentFrom = payment.PaymentFrom;
                        gullakpending.GatewayRequest = payment.GatewayRequest;
                        gullakpending.ModifiedDate = DateTime.Now;
                        db.Entry(gullakpending).State = EntityState.Modified;
                    }
                    GullakTransaction gullakTransaction = new GullakTransaction();
                    gullakTransaction.CreatedBy = userid;
                    gullakTransaction.CreatedDate = DateTime.Now;
                    gullakTransaction.Amount = payment.Amount;
                    gullakTransaction.ModifiedBy = userid;
                    gullakTransaction.ModifiedDate = indianTime;
                    gullakTransaction.CustomerId = payment.CustomerId;
                    gullakTransaction.GullakId = payment.GullakId;
                    gullakTransaction.IsActive = true;
                    gullakTransaction.IsDeleted = false;
                    gullakTransaction.Comment = !string.IsNullOrEmpty(payment.comment) ? payment.comment : "Advance Payment";
                    gullakTransaction.ObjectType = payment.PaymentFrom;
                    gullakTransaction.ObjectId = payment.GatewayTransId + "---" + payment.GatewayRequest;

                    db.GullakTransactionDB.Add(gullakTransaction);
                    //add to main amount 
                    Inpayment.TotalAmount = Inpayment.TotalAmount + payment.Amount;
                    Inpayment.ModifiedDate = DateTime.Now;
                    Inpayment.ModifiedBy = userid;
                    db.Entry(Inpayment).State = EntityState.Modified;

                    if (db.Commit() > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }


                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //End

        //For Multiple Id whose status can be changed from Pending //Startpkz
        [AllowAnonymous]
        [Route("ChangePendingV2")]
        [HttpPut]
        public async Task<bool> ChangePendingV2(List<GullakPendingDc> payment)
        {

            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                foreach (var item in payment)
                {

                    var gullakpending = db.GullakInPaymentDB.Where(x => x.GullakId == item.GullakId && x.CustomerId == item.CustomerId && x.Id == item.id && x.status == "Pending").FirstOrDefault();
                    if (gullakpending != null)
                    {
                        gullakpending.GatewayTransId = item.GatewayTransId;
                        gullakpending.status = item.status;
                        gullakpending.amount = item.Amount;
                        gullakpending.CustomerId = item.CustomerId;
                        gullakpending.Comment = item.comment;
                        gullakpending.PaymentFrom = item.PaymentFrom;
                        gullakpending.GatewayRequest = item.GatewayRequest;
                        gullakpending.ModifiedDate = DateTime.Now;
                        gullakpending.ModifiedBy = userid;
                        db.Entry(gullakpending).State = EntityState.Modified;
                    }
                    if (item.status != "Fail" && gullakpending != null)
                    {
                        var Inpayment = db.GullakDB.Where(x => x.Id == item.GullakId && x.CustomerId == item.CustomerId && x.IsActive == true).FirstOrDefault();
                        if (Inpayment != null)
                        {
                            Inpayment.TotalAmount = Inpayment.TotalAmount + item.Amount;
                            Inpayment.ModifiedDate = DateTime.Now;
                            Inpayment.ModifiedBy = userid;

                            db.Entry(Inpayment).State = EntityState.Modified;
                        }

                        GullakTransaction gullakTransaction = new GullakTransaction();
                        gullakTransaction.CreatedBy = userid;
                        gullakTransaction.CreatedDate = DateTime.Now;
                        gullakTransaction.Amount = item.Amount;
                        gullakTransaction.ModifiedBy = userid;
                        gullakTransaction.ModifiedDate = indianTime;
                        gullakTransaction.CustomerId = item.CustomerId;
                        gullakTransaction.GullakId = item.GullakId;
                        gullakTransaction.IsActive = true;
                        gullakTransaction.IsDeleted = false;
                        gullakTransaction.Comment = !string.IsNullOrEmpty(item.comment) ? item.comment : "Advance Payment";
                        gullakTransaction.ObjectType = item.PaymentFrom;
                        gullakTransaction.ObjectId = item.GatewayTransId + "---" + item.GatewayRequest;

                        db.GullakTransactionDB.Add(gullakTransaction);
                    }

                }

                if (db.Commit() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }
        //End



        public dynamic ChangePendingStatus(GetGullakTransaction getGullakTransaction)
        {
            using (var context = new AuthContext())
            {
                var result = context.GullakInPaymentDB.Where(x => x.GullakId == getGullakTransaction.GullakId && x.CustomerId == getGullakTransaction.CustomerId && x.status == "Pending").ToList();
                return result;
            }
        }

    }



    public class GKDC
    {
        public int WarehouseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double AmountFrom { get; set; }
        public double AmountTo { get; set; }
        public double MaximumCashBackAmount { get; set; }
        public double CashBackPercent { get; set; }

    }
}