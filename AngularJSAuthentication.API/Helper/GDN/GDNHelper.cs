using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.GDN;
using MongoDB.Driver.Core.Operations;
using NPOI.OpenXmlFormats.Dml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using NLog;

using static AngularJSAuthentication.API.ControllerV7.SarthiAppController;

namespace AngularJSAuthentication.API.Helper.GDN
{
    public class GDNHelper
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        public bool AddGDN(AddPOGRDetailDc AddPOGRDetailobj, AuthContext context)
        {
            bool result = false;
            try
            {


                GoodsDescripancyNoteMaster master = new GoodsDescripancyNoteMaster();
                List<GoodsDescripancyNoteDetail> details = new List<GoodsDescripancyNoteDetail>();
                if (AddPOGRDetailobj.PurchaseOrderDetailDc != null && (AddPOGRDetailobj.PurchaseOrderDetailDc.Any(x => x.IsGDNDamage == true || x.IsGDNExpiry == true || x.ShortQty > 0)))
                {
                    master.IsGDNGenerate = false;
                    master.PurchaseOrderId = AddPOGRDetailobj.PurchaseOrderDetailDc.Select(x => x.PurchaseOrderId).FirstOrDefault();
                    master.Status = "Pending";
                    //Random random = new Random();
                    master.GDNNumber = "PO-" + master.PurchaseOrderId + "-" + master.Id;
                    master.CreatedDate = DateTime.Now;
                    master.CreatedBy = AddPOGRDetailobj.UserId;
                    master.GrSerialNo = AddPOGRDetailobj.GrSerialNumber;
                    master.IsActive = true;
                    master.IsDeleted = false;
                    context.GoodsDescripancyNoteMasterDB.Add(master);

                    foreach (var item in AddPOGRDetailobj.PurchaseOrderDetailDc)
                    {
                        var gd = context.GoodsReceivedDetail.Where(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.PurchaseOrderDetailId == item.PurchaseOrderDetailId && x.GrSerialNumber == item.GrSerialNumber).FirstOrDefault();
                        if (item.IsGDNExpiry || item.IsGDNDamage || item.ShortQty > 0)
                        {
                            GoodsDescripancyNoteDetail obj = new GoodsDescripancyNoteDetail()
                            {
                                ItemMultiMRPId = item.ItemMultiMRPId,
                                DamageQty = item.IsGDNDamage ? item.DamageQty : 0,
                                ExpiryQty = item.IsGDNExpiry ? item.ExpiryQty : 0,
                                ShortQty = item.ShortQty > 0 ? item.ShortQty : 0,
                                IsActive = true,
                                IsDeleted = false,
                                CreatedBy = AddPOGRDetailobj.UserId,
                                CreatedDate = DateTime.Now,
                                GoodsDescripancyNoteMasterId = master.Id,
                                GoodsReceivedDetailId = gd.Id
                            };
                            details.Add(obj);
                        }
                    }
                    context.GoodsDescripancyNoteDetailDB.AddRange(details);
                    if (context.Commit() > 0)
                    {

                        var gdnNo = context.GoodsDescripancyNoteMasterDB.Where(z => z.Id == master.Id).FirstOrDefault();
                        gdnNo.GDNNumber = "PO-" + master.PurchaseOrderId + "-" + master.Id;
                        context.Entry(gdnNo).State = EntityState.Modified;
                        InsertConversation(master.PurchaseOrderId, master.Id, master.CreatedBy, context);
                        context.Commit();
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public bool RemoveGDN(int PurchaseOrderId, int UserId, int GrSerialNo, AuthContext context)
        {
            bool Result = false;
            try
            {
                var GDN = context.GoodsDescripancyNoteMasterDB.Where(x => x.IsActive == true && x.IsDeleted == false && x.PurchaseOrderId == PurchaseOrderId && x.GrSerialNo == GrSerialNo).FirstOrDefault();

                if (GDN != null)
                {
                    GDN.IsDeleted = true;
                    GDN.IsActive = false;
                    GDN.ModifiedBy = UserId;
                    GDN.ModifiedDate = DateTime.Now;
                    context.Entry(GDN).State = EntityState.Modified;

                    var GDNDtls = context.GoodsDescripancyNoteDetailDB.Where(z => z.GoodsDescripancyNoteMasterId == GDN.Id).ToList();

                    foreach (var item in GDNDtls)
                    {
                        item.ModifiedBy = UserId;
                        item.ModifiedDate = DateTime.Now;
                        item.IsDeleted = true;
                        item.IsActive = false;
                        context.Entry(item).State = EntityState.Modified;
                    }
                    Result = true;
                }
            }
            catch (Exception ex)
            {

            }
            return Result;
        }

        public static bool sendGDNtoSupplier(int PurchaseOrderId, string WhName, string Mob, int GrSerialNo, string email, AuthContext context)
        {
            bool flag = false;
            try
            {
                Sms s = new Sms();

                Random rand = new Random(100);
                int randomNumber = rand.Next(000000, 999999);
                var otpno = OtpGenerator.GetSixDigitNumber(randomNumber);

                var gdn = context.GoodsDescripancyNoteMasterDB.Where(x => x.PurchaseOrderId == PurchaseOrderId && x.GrSerialNo == GrSerialNo && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                if (gdn != null)
                {
                    //AES256 aes = new AES256();
                    //string redisAesKey = DateTime.Now.ToString("yyyyMMdd") + "1201";
                    //var gndi = aes.Encrypt(gdn.Id.ToString(), redisAesKey);

                    gdn.IsGDNGenerate = true;
                    gdn.OTP = Convert.ToInt32(otpno);
                    context.Entry(gdn).State = EntityState.Modified;
                    context.Commit();
                    //if (context.Commit() > 0)
                    //{
                    string gdnurl = ConfigurationManager.AppSettings["GDNUrl"];
                    var url = gdnurl + gdn.Id;
                    // var url = "http://192.168.1.101:8959/#/sellerwebview/gdnote/" + gdn.Id;
                    string GDNMsg = "ShopKirana has raised GDN against  PO #: " + PurchaseOrderId + " In Hub:" + WhName +
                        " OTP for Access GND is : " + otpno + "  and URL is : " + url;
                    try
                    {
                        if (!string.IsNullOrEmpty(email))
                        {
                            EmailHelper.SendMail(AppConstants.MasterEmail, email, "",
                                      ConfigurationManager.AppSettings["Environment"] + " ShopKirana has raised GDN "
                                      , GDNMsg
                                      , "");
                        }
                    }
                    catch (Exception ex)
                    {

                        logger.Error("Error in send Email in GDN  " + ex.Message);
                    }
                    if (!string.IsNullOrEmpty(Mob))
                    {
                        flag = s.sendOtp(Mob, GDNMsg,"");
                    }
                }
                //}

            }
            catch (Exception ex)
            {
                logger.Error("Error in GDN  " + ex.Message);
            }
            return flag;
        }
        public bool InsertConversation(long PurchaseOrderId, long GDNId, int Userid, AuthContext context)
        {
            bool i = true;
            try
            {
                List<GDNConversation> cnvs = new List<GDNConversation>();

                var po = context.DPurchaseOrderMaster.Where(x => x.PurchaseOrderId == PurchaseOrderId && x.Active == true && x.Deleted == false).FirstOrDefault();

                var gndconv = context.GDNConversations.Where(x => x.GDNId == GDNId && x.IsActive == true && x.IsDeleted == false).ToList();
                foreach (var item in gndconv)
                {
                    item.IsActive = false;
                    item.IsDeleted = true;
                    item.ModifiedDate = DateTime.Now;
                    context.Entry(item).State = EntityState.Modified;
                }
                GDNConversation gDN = new GDNConversation()
                {
                    GDNId = GDNId,
                    ApprovedBy = po.SupplierId,
                    PeopleType = "Supplier",
                    Status = "Pending",
                    CreatedBy = Userid,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false
                };
                cnvs.Add(gDN);
                GDNConversation gdnc = new GDNConversation()
                {
                    GDNId = GDNId,
                    ApprovedBy = Convert.ToInt32(po.BuyerId),
                    PeopleType = "Buyer",
                    Status = "Pending",
                    CreatedBy = Userid,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false
                };
                cnvs.Add(gdnc);
                context.GDNConversations.AddRange(cnvs);
            }
            catch (Exception a)
            {
            }
            return i;
        }


        public bool SupplierAction(GNDActionDC dc, int userid)
        {
            bool Result = false;
            try
            {
                if (dc.status == "A")
                {
                    dc.status = GDNStatus.Approved.ToString();
                }
                else
                {
                    dc.status = GDNStatus.Rejected.ToString();
                }
                using (var context = new AuthContext())
                {
                    var GDN = context.GoodsDescripancyNoteMasterDB.Where(z => z.Id == dc.GDNId && z.IsActive == true && z.IsDeleted == false && z.PurchaseOrderId == dc.PurchaseOrderId).FirstOrDefault();
                    if (GDN != null)
                    {
                        GDN.Status = dc.status;
                        GDN.ModifiedBy = userid;
                        GDN.ModifiedDate = DateTime.Now;

                        context.Entry(GDN).State = EntityState.Modified;
                    }
                    var gndConv = context.GDNConversations.Where(x => x.GDNId == dc.GDNId && x.PeopleType == "Supplier" && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if (gndConv != null)
                    {
                        gndConv.Status = dc.status;
                        gndConv.ModifiedBy = userid;
                        gndConv.ModifiedDate = DateTime.Now;
                        gndConv.Commants = dc.Comment;
                        context.Entry(gndConv).State = EntityState.Modified;
                    }
                    if (context.Commit() > 0) { Result = true; }
                }
            }
            catch (Exception ex)
            {
            }

            return Result;
        }

        public bool BuyerAction(GoodsDescripancyNoteDetail obj, int userid)
        {
            bool i = false;
            using (var context = new AuthContext())
            {
                var objdetail = context.GoodsDescripancyNoteDetailDB.FirstOrDefault(x => x.Id == obj.Id);
                if (objdetail != null)
                {
                    //objdetail.ChangedDamageQty = (obj.ChangedDamageQty == null) ? objdetail.ChangedDamageQty
                    //                            : obj.ChangedDamageQty >= 0 ? obj.DamageQty : obj.ChangedDamageQty;
                    //objdetail.ChangedExpiryQty = (obj.ChangedExpiryQty == null) ? objdetail.ChangedExpiryQty
                    //                            : obj.ChangedExpiryQty >= 0 ? obj.ExpiryQty : obj.ChangedExpiryQty;
                    //objdetail.ChangedShortQty =  (obj.ChangedShortQty == null) ? objdetail.ChangedShortQty
                    //                            : obj.ChangedShortQty >= 0 ? obj.ShortQty : obj.ChangedShortQty;

                    int dbchngdamag = objdetail.ChangedDamageQty == null ? 0 : (int)objdetail.ChangedDamageQty;
                    int dbchngexp = objdetail.ChangedExpiryQty == null ? 0 : (int)objdetail.ChangedExpiryQty;
                    int dbchngshort = objdetail.ChangedShortQty == null ? 0 : (int)objdetail.ChangedShortQty;
                    objdetail.ChangedDamageQty = obj.ChangedDamageQty >= 0 ? dbchngdamag + (objdetail.DamageQty - obj.ChangedDamageQty) : objdetail.ChangedDamageQty;
                    objdetail.ChangedExpiryQty = obj.ChangedExpiryQty >= 0 ? dbchngexp + (objdetail.ExpiryQty - obj.ChangedExpiryQty) : objdetail.ChangedExpiryQty;
                    objdetail.ChangedShortQty = obj.ChangedShortQty >= 0 ? dbchngshort + (objdetail.ShortQty - obj.ChangedShortQty) : objdetail.ChangedShortQty;
                    objdetail.DamageQty = obj.ChangedDamageQty >= 0 ? (int)obj.ChangedDamageQty : objdetail.DamageQty;
                    objdetail.ShortQty = obj.ChangedShortQty >= 0 ? (int)obj.ChangedShortQty : objdetail.ShortQty;
                    objdetail.ExpiryQty = obj.ChangedExpiryQty >= 0 ? (int)obj.ChangedExpiryQty : objdetail.ExpiryQty;
                    objdetail.ModifiedBy = userid;
                    objdetail.ModifiedDate = DateTime.Now;
                    context.Entry(objdetail).State = EntityState.Modified;

                    var gndConv = context.GDNConversations.Where(x => x.GDNId == obj.GoodsDescripancyNoteMasterId && x.PeopleType == "Buyer" && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if (gndConv != null)
                    {
                        gndConv.Status = GDNStatus.Approved.ToString();
                        gndConv.ModifiedBy = userid;
                        gndConv.ModifiedDate = DateTime.Now;
                        gndConv.Commants = "Qty updated by buyer";
                        context.Entry(gndConv).State = EntityState.Modified;
                    }
                    var gdn = context.GoodsDescripancyNoteMasterDB.Where(x => x.Id == obj.GoodsDescripancyNoteMasterId && x.IsActive == true && x.IsDeleted == false && x.Status == "Rejected").FirstOrDefault();
                    if (gdn != null)
                    {
                        gdn.Status = GDNStatus.Pending.ToString();
                        gdn.ModifiedBy = userid;
                        gdn.ModifiedDate = DateTime.Now;
                        context.Entry(gdn).State = EntityState.Modified;
                    }
                    //InsertConversation(gdn.PurchaseOrderId, gdn.Id, userid, context);
                    if (context.Commit() > 0)
                    {
                        i = true;
                    }
                }
            }
            return i;
        }

        public bool BuyerCancelGDN(long GDNid, int Userid, string comment)
        {
            bool i = false;
            try
            {
                using (var context = new AuthContext())
                {

                    var gdn = context.GoodsDescripancyNoteMasterDB.Where(x => x.Id == GDNid && x.IsActive == true && x.IsDeleted == false).Include(x => x.goodsDescripancyNoteDetail).FirstOrDefault();
                    if (gdn != null)
                    {
                        gdn.Status = GDNStatus.Cancel.ToString();
                        gdn.ModifiedBy = Userid;
                        gdn.ModifiedDate = DateTime.Now;
                        context.Entry(gdn).State = EntityState.Modified;

                        foreach (var item in gdn.goodsDescripancyNoteDetail)
                        {
                            int dbchngdamag = item.ChangedDamageQty == null ? 0 : (int)item.ChangedDamageQty;
                            int dbchngexp = item.ChangedExpiryQty == null ? 0 : (int)item.ChangedExpiryQty;
                            int dbchngshort = item.ChangedShortQty == null ? 0 : (int)item.ChangedShortQty;
                            item.ChangedDamageQty = dbchngdamag + item.DamageQty;
                            item.ChangedExpiryQty = dbchngexp + item.ExpiryQty;
                            item.ChangedShortQty = dbchngshort + item.ShortQty;
                            item.ShortQty = 0;
                            item.DamageQty = 0;
                            item.ExpiryQty = 0;
                            context.Entry(item).State = EntityState.Modified;
                        }
                    }

                    var gndConv = context.GDNConversations.Where(x => x.GDNId == GDNid && x.PeopleType == "Buyer" && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if (gndConv != null)
                    {
                        gndConv.Status = GDNStatus.Cancel.ToString();
                        gndConv.ModifiedBy = Userid;
                        gndConv.ModifiedDate = DateTime.Now;
                        gndConv.Commants = comment;
                        context.Entry(gndConv).State = EntityState.Modified;
                    }

                    if (context.Commit() > 0)
                    {
                        i = true;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return i;
        }

        public bool ValidateOtp(int otp, int gdnid)
        {
            bool valid = false;
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    valid = context.GoodsDescripancyNoteMasterDB.Any(z => z.OTP == otp && z.IsActive == true && z.IsDeleted == false && z.Id == gdnid);
                }
            }
            catch (Exception x)
            {
                valid = false;
            }
            return valid;
        }

        public List<GDNDC> GetGDNdetails(string GDNid)
        {

            long gdnid = Convert.ToInt64(GDNid);

            //AES256 aes = new AES256(); 
            //string redisAesKey = DateTime.Now.ToString("yyyyMMdd") + "1201";
            //var id = aes.Decrypt(GDNid, redisAesKey);

            var result = new List<GDNDC>();
            try
            {
                using (AuthContext context = new AuthContext())
                {

                    var GDNidparam = new SqlParameter
                    {
                        ParameterName = "@GDNid",
                        Value = gdnid
                    };

                    var list = context.Database.SqlQuery<GDNDC>("GetGDNDetails @GDNid", GDNidparam).ToList();
                    if (list.Any() && list != null)
                    {
                        result = list;
                    }
                }
            }
            catch (Exception x)
            {
                result = null;
            }
            return result;
        }

        public List<GDNDC> GetGDNdetailsv7(long GDNid)
        {

            var result = new List<GDNDC>();
            try
            {
                using (AuthContext context = new AuthContext())
                {

                    var GDNidparam = new SqlParameter
                    {
                        ParameterName = "@GDNid",
                        Value = GDNid
                    };

                    var list = context.Database.SqlQuery<GDNDC>("GetGDNDetails @GDNid", GDNidparam).ToList();
                    if (list.Any() && list != null)
                    {
                        result = list;
                    }
                }
            }
            catch (Exception x)
            {
                result = null;
            }
            return result;
        }

        public List<GDNDC> GetGDNdetailOnIR(long Poid, int GrSNo)
        {
            var result = new List<GDNDC>();
            try
            {
                using (AuthContext context = new AuthContext())
                {

                    var Poidparam = new SqlParameter
                    {
                        ParameterName = "@PurchaseOrderId",
                        Value = Poid
                    };
                    var GrSNoparam = new SqlParameter
                    {
                        ParameterName = "@GrSno",
                        Value = GrSNo
                    };

                    var list = context.Database.SqlQuery<GDNDC>("GetGDNDataOnIr @PurchaseOrderId,@GrSno", Poidparam, GrSNoparam).ToList();
                    if (list.Any() && list != null)
                    {
                        result = list;
                    }
                }
            }
            catch (Exception x)
            {
                result = null;
            }
            return result;
        }

        enum GDNStatus
        {
            Approved,
            Rejected,
            Cancel,
            Pending
        }

    }
}