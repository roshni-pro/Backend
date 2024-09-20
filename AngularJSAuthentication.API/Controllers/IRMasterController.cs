using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using AngularJSAuthentication.Model.PurchaseOrder;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/IRMaster")]
    public class IRMasterController : ApiController
    {
        [Route("GetGRDetail")]
        [HttpGet]
        public HttpResponseMessage GetGRDetail(int PurchaseOrderId)
        {
            GRResDTO res;
            try
            {
                using (var context = new AuthContext())
                {
                    List<GRIdsDTO> GRIdsO = new List<GRIdsDTO>();
                    PurchaseOrderMaster pom = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == PurchaseOrderId).SingleOrDefault();
                    List<PurchaseOrderDetailRecived> podr = context.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderId == PurchaseOrderId).ToList();

                    if (pom != null)
                    {
                        if (pom.Gr1_Amount != 0 && pom.Gr1Status != null)
                        {
                            List<GRItemDTO> objGrItem = new List<GRItemDTO>();

                            foreach (var a in podr)
                            {
                                GRItemDTO gritem = new GRItemDTO()
                                {
                                    ItemId = a.ItemId,
                                    ItemName = a.ItemName1,
                                    Itemnumber = a.ItemNumber,
                                    PurchaseOrderDetailRecivedId = a.PurchaseOrderDetailRecivedId,
                                    Quantity = a.QtyRecived1
                                };
                                objGrItem.Add(gritem);
                            }

                            GRIdsDTO obj = new GRIdsDTO()
                            {
                                PurchaseOrderId = pom.PurchaseOrderId,
                                GrNumber = pom.Gr1Number,
                                gritem = objGrItem
                            };
                            GRIdsO.Add(obj);
                        }
                        if (pom.Gr2_Amount != 0 && pom.Gr2Status != null)
                        {
                            List<GRItemDTO> objGrItem = new List<GRItemDTO>();

                            foreach (var a in podr)
                            {
                                GRItemDTO gritem = new GRItemDTO()
                                {
                                    ItemId = a.ItemId,
                                    ItemName = a.ItemName2,
                                    Itemnumber = a.ItemNumber,
                                    PurchaseOrderDetailRecivedId = a.PurchaseOrderDetailRecivedId,
                                    Quantity = a.QtyRecived2
                                };
                                objGrItem.Add(gritem);
                            }

                            GRIdsDTO obj = new GRIdsDTO()
                            {
                                PurchaseOrderId = pom.PurchaseOrderId,
                                GrNumber = pom.Gr2Number,
                                gritem = objGrItem
                            };
                            GRIdsO.Add(obj);
                        }
                        if (pom.Gr3_Amount != 0 && pom.Gr3Status != null)
                        {
                            List<GRItemDTO> objGrItem = new List<GRItemDTO>();

                            foreach (var a in podr)
                            {
                                GRItemDTO gritem = new GRItemDTO()
                                {
                                    ItemId = a.ItemId,
                                    ItemName = a.ItemName3,
                                    Itemnumber = a.ItemNumber,
                                    PurchaseOrderDetailRecivedId = a.PurchaseOrderDetailRecivedId,
                                    Quantity = a.QtyRecived3
                                };
                                objGrItem.Add(gritem);
                            }

                            GRIdsDTO obj = new GRIdsDTO()
                            {
                                PurchaseOrderId = pom.PurchaseOrderId,
                                GrNumber = pom.Gr3Number,
                                gritem = objGrItem
                            };
                            GRIdsO.Add(obj);
                        }
                        if (pom.Gr4_Amount != 0 && pom.Gr4Status != null)
                        {
                            List<GRItemDTO> objGrItem = new List<GRItemDTO>();

                            foreach (var a in podr)
                            {
                                GRItemDTO gritem = new GRItemDTO()
                                {
                                    ItemId = a.ItemId,
                                    ItemName = a.ItemName4,
                                    Itemnumber = a.ItemNumber,
                                    PurchaseOrderDetailRecivedId = a.PurchaseOrderDetailRecivedId,
                                    Quantity = a.QtyRecived4
                                };
                                objGrItem.Add(gritem);
                            }


                            GRIdsDTO obj = new GRIdsDTO()
                            {
                                PurchaseOrderId = pom.PurchaseOrderId,
                                GrNumber = pom.Gr4Number,
                                gritem = objGrItem
                            };
                            GRIdsO.Add(obj);
                        }
                        if (pom.Gr5_Amount != 0 && pom.Gr5Status != null)
                        {
                            List<GRItemDTO> objGrItem = new List<GRItemDTO>();

                            foreach (var a in podr)
                            {
                                GRItemDTO gritem = new GRItemDTO()
                                {
                                    ItemId = a.ItemId,
                                    ItemName = a.ItemName5,
                                    PurchaseOrderDetailRecivedId = a.PurchaseOrderDetailRecivedId,
                                    Itemnumber = a.ItemNumber,
                                    Quantity = a.QtyRecived5
                                };
                                objGrItem.Add(gritem);
                            }

                            GRIdsDTO obj = new GRIdsDTO()
                            {
                                PurchaseOrderId = pom.PurchaseOrderId,
                                GrNumber = pom.Gr5Number,
                                gritem = objGrItem
                            };
                            GRIdsO.Add(obj);
                        }
                    }
                    res = new GRResDTO()
                    {
                        GRIdsObj = GRIdsO
                    };
                }
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [Authorize]
        [Route("GetPostedIR")]
        public HttpResponseMessage GetPostedIR(int PurchaseOrderId)
        {
            using (var context = new AuthContext())
            {
                var IRM = context.IRMasterDB.Where(q => q.PurchaseOrderId == PurchaseOrderId).ToList();
                var IRCData = context.IR_ConfirmDb.Where(q => q.PurchaseOrderId == PurchaseOrderId).ToList();

                if (IRM.Count != 0)
                {
                    foreach (IRMaster ir in IRM)
                    {
                        ir.purDetails = IRCData;
                    }

                    PostedIRDTO response = new PostedIRDTO
                    {
                        IRM = IRM
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "IR not found.");
                }
            }
        }

        [Route("GetIRDetail")]
        [HttpPut]
        public HttpResponseMessage GetIRDetail(List<GRIdsDTO> obj)
        {
            int PurchaseOrderId = obj[0].PurchaseOrderId;
            using (AuthContext db = new AuthContext())
            {
                List<IR_Confirm> IRItemDetail = new List<IR_Confirm>();
                PurchaseOrderMaster pom = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == PurchaseOrderId).SingleOrDefault();
                pom.purDetails = db.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderId == PurchaseOrderId).ToList();
                var IRM = db.IRMasterDB.Where(a => a.PurchaseOrderId == PurchaseOrderId).Count();

                if (IRM > 0)
                {
                    IRMaster irm = new IRMaster();
                    irm.PurchaseOrderId = pom.PurchaseOrderId;
                    irm.supplierId = pom.SupplierId;
                    irm.SupplierName = pom.SupplierName;
                    irm.WarehouseId = pom.WarehouseId;
                    switch (pom.IRcount)
                    {
                        case 1:
                            irm.IRType = "IR2";
                            break;
                        case 2:
                            irm.IRType = "IR3";
                            break;
                        case 3:
                            irm.IRType = "IR4";
                            break;
                        case 4:
                            irm.IRType = "IR5";
                            break;
                        default:
                            break;
                    }
                    irm.purDetails = db.IR_ConfirmDb.Where(a => a.PurchaseOrderId == PurchaseOrderId).ToList();
                    foreach (var a in irm.purDetails)
                    {
                        a.Qty = 0;
                        foreach (var f in obj)
                        {
                            a.Qty += f.gritem.Where(e => e.Itemnumber == a.ItemNumber && e.IsSelected == true).Sum(e => e.Quantity);
                        }
                        a.QtyRecived = Convert.ToInt32(pom.purDetails.Where(e => e.ItemNumber == a.ItemNumber).Select(e => e.QtyRecived).SingleOrDefault());
                        a.TtlAmt = 0;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, irm);
                }
                else
                {
                    if (pom.purDetails != null)
                    {
                        IRMaster irm = new IRMaster();
                        List<IR_Confirm> Ilist = new List<IR_Confirm>();
                        irm.PurchaseOrderId = pom.PurchaseOrderId;
                        irm.supplierId = pom.SupplierId;
                        irm.SupplierName = pom.SupplierName;
                        irm.WarehouseId = pom.WarehouseId;
                        irm.IRType = "IR1";
                        irm.TotalAmount = 0;
                        pom.IRcount = 1; // Update IR count in Purchase order master
                        foreach (var s in pom.purDetails)
                        {
                            IR_Confirm irc = new IR_Confirm();
                            irc.ItemId = s.ItemId;
                            irc.PurchaseOrderDetailId = s.PurchaseOrderDetailId;
                            irc.SupplierId = s.SupplierId;
                            irc.CompanyId = s.CompanyId ?? 0;
                            irc.ItemName = s.ItemName;
                            irc.ItemNumber = s.ItemNumber;
                            irc.CessTaxPercentage = s.CessTaxPercentage;
                            irc.IRType = "IR1";
                            irc.MRP = s.MRP;
                            irc.Price = s.Price;
                            irc.TotalTaxPercentage = s.TotalTaxPercentage;
                            irc.TotalQuantity = s.TotalQuantity;
                            irc.WarehouseId = s.WarehouseId;
                            irc.WarehouseName = s.WarehouseName;
                            irc.PurchaseOrderId = s.PurchaseOrderId;
                            irc.PurchaseSku = s.PurchaseSku;
                            irc.PriceRecived = s.PriceRecived;
                            irc.IRQuantity = 0;
                            irc.QtyRecived = Convert.ToInt32(s.QtyRecived ?? 0);
                            irc.HSNCode = s.HSNCode;
                            irc.CreationDate = DateTime.Now;
                            irc.TtlAmt = 0;
                            irc.Qty = 0;
                            foreach (var f in obj)
                            {
                                irc.Qty += f.gritem.Where(e => e.Itemnumber == s.ItemNumber && e.IsSelected == true).Sum(e => e.Quantity);
                            }
                            Ilist.Add(irc);
                        }

                        irm.purDetails = Ilist;
                        return Request.CreateResponse(HttpStatusCode.OK, irm);
                    }
                }
            }
            return null;
        }

        [Route("PostIR")]
        [HttpPost]
        public HttpResponseMessage PostIR(PostIR obj)
        {
            string GrSetteldIds = string.Empty;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (obj.irmaster.IRID != null && obj.irmaster.IRID != "")
            {
                using (AuthContext db = new AuthContext())
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    var IRCount = db.IRMasterDB.Where(a => a.PurchaseOrderId == obj.irmaster.PurchaseOrderId).Count();
                    var PurchaseOrderMaster = db.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == obj.irmaster.PurchaseOrderId).SingleOrDefault();
                    var People = db.Peoples.Where(a => a.PeopleID == userid).SingleOrDefault();
                    var selectedGR = obj.grlist.Where(a => a.IsSelected == true);
                    if (IRCount == 0)
                    {
                        IRMaster irm = new IRMaster(); // Insert IR master data 
                        irm.PurchaseOrderId = PurchaseOrderMaster.PurchaseOrderId;
                        irm.supplierId = PurchaseOrderMaster.SupplierId;
                        irm.SupplierName = PurchaseOrderMaster.SupplierName;
                        irm.WarehouseId = PurchaseOrderMaster.WarehouseId;
                        irm.BuyerId = PurchaseOrderMaster.BuyerId;
                        irm.BuyerName = PurchaseOrderMaster.BuyerName;
                        irm.TotalAmount = obj.irmaster.TotalAmount;
                        irm.TotalAmountRemaining = obj.irmaster.TotalAmount;
                        irm.IRID = obj.irmaster.IRID;
                        irm.IRStatus = "IR Posted";
                        irm.IRType = "IR1";
                        irm.Gstamt = obj.irmaster.purDetails.Sum(a => a.gstamt) ?? 0;
                        irm.CreatedBy = People.DisplayName;
                        irm.CreationDate = DateTime.Now;
                        irm.IRAmountWithTax = obj.irmaster.TotalAmount;
                        irm.Discount = obj.irmaster.Discount;
                        irm.OtherAmount = obj.irmaster.OtherAmount;
                        irm.OtherAmountType = obj.irmaster.OtherAmountType;
                        irm.ExpenseAmount = obj.irmaster.ExpenseAmount;
                        irm.ExpenseAmountType = obj.irmaster.ExpenseAmountType;
                        irm.RoundofAmount = obj.irmaster.RoundofAmount;
                        irm.RoundoffAmountType = obj.irmaster.RoundoffAmountType;
                        db.IRMasterDB.Add(irm);
                        PurchaseOrderMaster.IRcount = 1;
                        db.Commit();
                        foreach (IR_Confirm i in obj.irmaster.purDetails)
                        {
                            foreach (var d in selectedGR)
                            {
                                foreach (var s in d.gritem.Where(a => a.Itemnumber == i.ItemNumber && a.IsSelected == true))
                                {
                                    string sAllCharacter = d.GrNumber;
                                    char GRType = sAllCharacter[sAllCharacter.Length - 1];

                                    switch (GRType)
                                    {
                                        case 'A':
                                            GrSetteldIds = "1";
                                            break;
                                        case 'B':
                                            GrSetteldIds += ",2";
                                            break;
                                        case 'C':
                                            GrSetteldIds += ",3";
                                            break;
                                        case 'D':
                                            GrSetteldIds += ",4";
                                            break;
                                        case 'E':
                                            GrSetteldIds += ",5";
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            GrSetteldIds = GrSetteldIds.TrimStart(',');


                            IR_Confirm irc = new IR_Confirm();
                            irc.PurchaseOrderId = i.PurchaseOrderId;
                            irc.PurchaseOrderDetailId = i.PurchaseOrderDetailId;
                            irc.CompanyId = i.CompanyId;
                            irc.ItemId = i.ItemId;
                            irc.ItemNumber = i.ItemNumber;
                            irc.ItemName = i.ItemName;
                            irc.HSNCode = i.HSNCode;
                            irc.MRP = i.MRP;
                            irc.Price = i.Price;
                            irc.TotalTaxPercentage = i.TotalTaxPercentage;
                            irc.IRQuantity = i.Qty;
                            irc.PriceRecived = i.PriceRecived;
                            irc.Price1 = i.Price;
                            irc.QtyRecived = i.QtyRecived;
                            irc.QtyRecived1 = i.Qty;
                            irc.DesA1 = i.DesA;
                            irc.DesP1 = i.DesP;
                            irc.distype1 = i.distype;
                            irc.IR1ID = obj.irmaster.IRID;
                            irc.Ir1PersonId = People.PeopleID;
                            irc.Ir1PersonName = People.DisplayName;
                            irc.dis1 = irc.discount;
                            irc.Q1Settled = GrSetteldIds;
                            irc.TtlAmt = i.TtlAmt;
                            irc.CreationDate = DateTime.Now;
                            db.IR_ConfirmDb.Add(irc);
                            db.Commit();
                            GrSetteldIds = string.Empty;
                        }

                    }
                    else if (IRCount > 0)
                    {
                        IRMaster irm = new IRMaster(); // Insert IR master data 
                        irm.PurchaseOrderId = PurchaseOrderMaster.PurchaseOrderId;
                        irm.supplierId = PurchaseOrderMaster.SupplierId;
                        irm.SupplierName = PurchaseOrderMaster.SupplierName;
                        irm.WarehouseId = PurchaseOrderMaster.WarehouseId;
                        irm.BuyerId = PurchaseOrderMaster.BuyerId;
                        irm.BuyerName = PurchaseOrderMaster.BuyerName;
                        irm.TotalAmount = obj.irmaster.TotalAmount;
                        irm.TotalAmountRemaining = obj.irmaster.TotalAmount;
                        irm.IRID = obj.irmaster.IRID;
                        irm.IRStatus = "IR Posted";
                        irm.IRType = obj.irmaster.IRType;
                        irm.Gstamt = obj.irmaster.purDetails.Sum(a => a.gstamt) ?? 0;
                        irm.CreatedBy = People.DisplayName;
                        irm.CreationDate = DateTime.Now;
                        irm.IRAmountWithTax = obj.irmaster.TotalAmount;
                        irm.Discount = obj.irmaster.Discount;
                        irm.OtherAmount = obj.irmaster.OtherAmount;
                        irm.OtherAmountType = obj.irmaster.OtherAmountType;
                        irm.ExpenseAmount = obj.irmaster.ExpenseAmount;
                        irm.ExpenseAmountType = obj.irmaster.ExpenseAmountType;
                        irm.RoundofAmount = obj.irmaster.RoundofAmount;
                        irm.RoundoffAmountType = obj.irmaster.RoundoffAmountType;
                        db.IRMasterDB.Add(irm);
                        PurchaseOrderMaster.IRcount = 1;

                        switch (PurchaseOrderMaster.IRcount)
                        {
                            case 1:
                                foreach (IR_Confirm i in obj.irmaster.purDetails)
                                {
                                    foreach (var d in selectedGR)
                                    {
                                        foreach (var s in d.gritem.Where(a => a.Itemnumber == i.ItemNumber && a.IsSelected == true))
                                        {
                                            string sAllCharacter = d.GrNumber;
                                            char GRType = sAllCharacter[sAllCharacter.Length - 1];

                                            switch (GRType)
                                            {
                                                case 'A':
                                                    GrSetteldIds = "1";
                                                    break;
                                                case 'B':
                                                    GrSetteldIds += ",2";
                                                    break;
                                                case 'C':
                                                    GrSetteldIds += ",3";
                                                    break;
                                                case 'D':
                                                    GrSetteldIds += ",4";
                                                    break;
                                                case 'E':
                                                    GrSetteldIds += ",5";
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                    GrSetteldIds = GrSetteldIds.TrimStart(',');

                                    IR_Confirm irc = db.IR_ConfirmDb.Where(a => a.IRreceiveid == i.IRreceiveid).SingleOrDefault();
                                    irc.Price2 = i.Price;
                                    irc.QtyRecived2 = i.Qty;
                                    irc.DesA2 = i.DesA;
                                    irc.DesP2 = i.DesP;
                                    irc.distype2 = i.distype;
                                    irc.IR2ID = obj.irmaster.IRID;
                                    irc.Ir2PersonId = People.PeopleID;
                                    irc.Ir2PersonName = People.DisplayName;
                                    irc.dis2 = irc.discount;
                                    irc.IRQuantity += i.Qty;
                                    irc.TtlAmt += i.TtlAmt;
                                    irc.Q2Settled = GrSetteldIds;
                                    db.Entry(irc).State = EntityState.Modified;

                                    PurchaseOrderMaster.IRcount = 2;
                                    GrSetteldIds = string.Empty;
                                }
                                break;
                            case 2:
                                foreach (IR_Confirm i in obj.irmaster.purDetails)
                                {
                                    foreach (var d in selectedGR)
                                    {
                                        foreach (var s in d.gritem.Where(a => a.Itemnumber == i.ItemNumber && a.IsSelected == true))
                                        {
                                            string sAllCharacter = d.GrNumber;
                                            char GRType = sAllCharacter[sAllCharacter.Length - 1];

                                            switch (GRType)
                                            {
                                                case 'A':
                                                    GrSetteldIds = "1";
                                                    break;
                                                case 'B':
                                                    GrSetteldIds += ",2";
                                                    break;
                                                case 'C':
                                                    GrSetteldIds += ",3";
                                                    break;
                                                case 'D':
                                                    GrSetteldIds += ",4";
                                                    break;
                                                case 'E':
                                                    GrSetteldIds += ",5";
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                    GrSetteldIds = GrSetteldIds.TrimStart(',');

                                    IR_Confirm irc = db.IR_ConfirmDb.Where(a => a.IRreceiveid == i.IRreceiveid).SingleOrDefault();
                                    irc.Price3 = i.Price;
                                    irc.QtyRecived3 = i.Qty;
                                    irc.DesA3 = i.DesA;
                                    irc.DesP3 = i.DesP;
                                    irc.distype3 = i.distype;
                                    irc.IR3ID = obj.irmaster.IRID;
                                    irc.Ir3PersonId = People.PeopleID;
                                    irc.Ir3PersonName = People.DisplayName;
                                    irc.dis3 = irc.discount;
                                    irc.IRQuantity += i.Qty;
                                    irc.TtlAmt += i.TtlAmt;
                                    irc.Q3Settled = GrSetteldIds;
                                    db.Entry(irc).State = EntityState.Modified;
                                    GrSetteldIds = string.Empty;

                                    PurchaseOrderMaster.IRcount = 3;
                                }
                                break;
                            case 3:
                                foreach (IR_Confirm i in obj.irmaster.purDetails)
                                {
                                    foreach (var d in selectedGR)
                                    {
                                        foreach (var s in d.gritem.Where(a => a.Itemnumber == i.ItemNumber && a.IsSelected == true))
                                        {
                                            string sAllCharacter = d.GrNumber;
                                            char GRType = sAllCharacter[sAllCharacter.Length - 1];

                                            switch (GRType)
                                            {
                                                case 'A':
                                                    GrSetteldIds = "1";
                                                    break;
                                                case 'B':
                                                    GrSetteldIds += ",2";
                                                    break;
                                                case 'C':
                                                    GrSetteldIds += ",3";
                                                    break;
                                                case 'D':
                                                    GrSetteldIds += ",4";
                                                    break;
                                                case 'E':
                                                    GrSetteldIds += ",5";
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                    GrSetteldIds = GrSetteldIds.TrimStart(',');

                                    IR_Confirm irc = db.IR_ConfirmDb.Where(a => a.IRreceiveid == i.IRreceiveid).SingleOrDefault();
                                    irc.Price4 = i.Price;
                                    irc.QtyRecived4 = i.Qty;
                                    irc.DesA4 = i.DesA;
                                    irc.DesP4 = i.DesP;
                                    irc.distype4 = i.distype;
                                    irc.IR4ID = obj.irmaster.IRID;
                                    irc.Ir4PersonId = People.PeopleID;
                                    irc.Ir4PersonName = People.DisplayName;
                                    irc.dis4 = irc.discount;
                                    irc.IRQuantity += i.Qty;
                                    irc.Q4Settled = GrSetteldIds;
                                    db.Entry(irc).State = EntityState.Modified;
                                    GrSetteldIds = string.Empty;

                                    PurchaseOrderMaster.IRcount = 4;
                                }
                                break;
                            case 4:
                                foreach (IR_Confirm i in obj.irmaster.purDetails)
                                {
                                    foreach (var d in selectedGR)
                                    {
                                        foreach (var s in d.gritem.Where(a => a.Itemnumber == i.ItemNumber && a.IsSelected == true))
                                        {
                                            string sAllCharacter = d.GrNumber;
                                            char GRType = sAllCharacter[sAllCharacter.Length - 1];

                                            switch (GRType)
                                            {
                                                case 'A':
                                                    GrSetteldIds = "1";
                                                    break;
                                                case 'B':
                                                    GrSetteldIds += ",2";
                                                    break;
                                                case 'C':
                                                    GrSetteldIds += ",3";
                                                    break;
                                                case 'D':
                                                    GrSetteldIds += ",4";
                                                    break;
                                                case 'E':
                                                    GrSetteldIds += ",5";
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                    GrSetteldIds = GrSetteldIds.TrimStart(',');

                                    IR_Confirm irc = db.IR_ConfirmDb.Where(a => a.IRreceiveid == i.IRreceiveid).SingleOrDefault();
                                    irc.Price5 = i.Price;
                                    irc.QtyRecived5 = i.Qty;
                                    irc.DesA5 = i.DesA;
                                    irc.DesP5 = i.DesP;
                                    irc.distype5 = i.distype;
                                    irc.IR5ID = obj.irmaster.IRID;
                                    irc.Ir5PersonId = People.PeopleID;
                                    irc.Ir5PersonName = People.DisplayName;
                                    irc.dis5 = irc.discount;
                                    irc.IRQuantity += i.Qty;
                                    irc.TtlAmt += i.TtlAmt;
                                    irc.Q5Settled = GrSetteldIds;
                                    db.Entry(irc).State = EntityState.Modified;
                                    GrSetteldIds = string.Empty;
                                    PurchaseOrderMaster.IRcount = 5;
                                }
                                break;
                            default:
                                break;
                        }
                    }

                    db.Commit();
                    dbContextTransaction.Commit();
                }
                return Request.CreateResponse(HttpStatusCode.OK, "IR Posted.");
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, "Enter Invoice Number.");
            }
        }

        [Authorize]
        [Route("GetInvoiceImage")]
        public HttpResponseMessage GetInvoiceImage(string InvoiceNumber, int PurchaseOrderId)
        {
            using (var context = new AuthContext())
            {
                var _response = context.InvoiceImageDb.Where(q => q.InvoiceNumber == InvoiceNumber && q.PurchaseOrderId == PurchaseOrderId).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, _response);
            }
        }

        [Route("getinvoiceNumbers")]
        [HttpGet]
        public HttpResponseMessage getinvoiceNumbers(int PurchaseOrderId)
        {
            using (var context = new AuthContext())
            {
                var _inviceNumbers = context.IRMasterDB.Where(a => a.PurchaseOrderId == PurchaseOrderId).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, _inviceNumbers);
            }
        }

        [Route("MoveGRDraft")]
        [HttpPost]
        public HttpResponseMessage MoveGrDraft(MoveGrDraftDTO _dataobj)
        {
            using (var context = new AuthContext())
            {
                InvoiceImage inv = new InvoiceImage();
                IRMaster ir = context.IRMasterDB.Where(a => a.PurchaseOrderId == _dataobj.PurchaseOrderId && a.IRID == _dataobj.IRID).FirstOrDefault();
                GrDraftInvoice GrD = context.GrDraftInvoiceDB.Where(a => a.PurchaseOrderId == _dataobj.PurchaseOrderId && a.GrNumber == _dataobj.GrNumber).FirstOrDefault();

                if (ir != null)
                {
                    if (ir.IRStatus == "IR Posted")
                    {
                        inv = new InvoiceImage();
                        inv.PurchaseOrderId = _dataobj.PurchaseOrderId;
                        inv.InvoiceNumber = _dataobj.IRID;
                        inv.CompanyId = 1;
                        inv.WarehouseId = ir.WarehouseId;
                        inv.IRAmount = ir.TotalAmount;
                        inv.IRLogoURL = _dataobj.ImagePath;
                        inv.CreationDate = DateTime.Now;
                        inv.InvoiceDate = _dataobj.IRDate;
                        ir.IRStatus = "IR Uploaded";
                        GrD.IsCorrect = _dataobj.IsCorrect;

                        context.InvoiceImageDb.Add(inv);

                        if (context.Commit() > 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "GR draft moved.");
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "Some issue in moving GR draft.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "IR Status should be IR Posted.");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "IR not found.");
                }

            }
        }


    }

    public class GRItemDTO
    {
        public int PurchaseOrderDetailRecivedId { get; set; }
        public int ItemId { get; set; }
        public string Itemnumber { get; set; }
        public string ItemName { get; set; }
        public int? Quantity { get; set; }
        public bool IsSelected { get; set; }
    }
    public class GRIdsDTO
    {
        public int PurchaseOrderId { get; set; }
        public string GrNumber { get; set; }
        public bool IsSelected { get; set; }
        public List<GRItemDTO> gritem { get; set; }
    }
    public class GRResDTO
    {
        public List<GRIdsDTO> GRIdsObj { get; set; }

    }
    public class PostIR
    {
        public IRMaster irmaster { get; set; }
        public List<GRIdsDTO> grlist { get; set; }
    }
    public class PostedIRDTO
    {
        public List<IRMaster> IRM { get; set; }
    }

    public class MoveGrDraftDTO
    {
        public int PurchaseOrderId { get; set; }
        public string GrNumber { get; set; }
        public string IRID { get; set; }

        public string ImagePath { get; set; }
        public DateTime IRDate { get; set; }
        public double TotalAmount { get; set; }
        public bool IsCorrect { get; set; }
    }

}
