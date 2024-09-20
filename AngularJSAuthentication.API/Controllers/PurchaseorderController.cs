using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;
using AngularJSAuthentication.Model.PurchaseOrder;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using System.Threading.Tasks;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/PurchaseOrder")]
    public class PurchaseorderController : ApiController
    {
        
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //[Authorize]
        [Route("")]
        public IEnumerable<Model.PurchaseOrder.PurchaseOrder> Get()
        {
            logger.Info("start PurchaseOrder: ");
            List<Model.PurchaseOrder.PurchaseOrder> ass = new List<Model.PurchaseOrder.PurchaseOrder>();
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

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                using (var context = new AuthContext())
                {
                    ass = context.AllPurchaseOrder(compid).ToList();
                }
                logger.Info("End  PurchaseOrder: ");
                return ass;
            }
            catch (Exception ex)
            {
                logger.Error("Error in PurchaseOrder " + ex.Message);
                logger.Info("End  PurchaseOrder: ");
                return null;
            }
        }

        [ResponseType(typeof(Model.PurchaseOrder.PurchaseOrder))]
        [Route("")]
        [AcceptVerbs("POST")]
        public List<Model.PurchaseOrder.PurchaseOrder> add(List<PurchaseOrderList> po)
        {
            logger.Info("start add PurchaseOrder: ");
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
                //po.CompanyId = compid;
                //if (item == null)
                //{
                //    throw new ArgumentNullException("item");
                //}
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                using (var context = new AuthContext())
                {
                    context.AddPurchaseOrder(po, compid);
                }
                logger.Info("End  Warehouse: ");
                return null;
            }
            catch (Exception ex)
            {
                logger.Error("Error in addQuesAns " + ex.Message);
                logger.Info("End  addWarehouse: ");
                return null;
            }
        }

        [ResponseType(typeof(HttpResponseMessage))]
        [Route("")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage add(Model.PurchaseOrder.PurchaseOrder po, string a)
        {
            logger.Info("start add PurchaseOrder: ");
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
                po.CompanyId = compid;

                if (po == null)
                {
                    throw new ArgumentNullException("item");
                }

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                using (var context = new AuthContext())
                {
                    //context.AddPurchaseItem(po);
                }
                logger.Info("End  Warehouse: ");
                return Request.CreateResponse(HttpStatusCode.OK, po);
            }
            catch (Exception ex)
            {
                logger.Error("Error in addQuesAns " + ex.Message);
                logger.Info("End  addWarehouse: ");
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex); ;
            }
        }

        #region GR IR settlement
        [Route("GRIR")]
        [HttpGet]
        public HttpResponseMessage GetGRIR(int poid)
        {
            GRResposeDTO res;
            try
            {
                using (var context = new AuthContext())
                {
                    List<GRIds> GRIdsO = new List<GRIds>();
                    List<IRIds> IRIdsO = new List<IRIds>();
                    PurchaseOrderMaster pom = context.DPurchaseOrderMaster.Where(a => a.PurchaseOrderId == poid).SingleOrDefault();
                    List<PurchaseOrderDetailRecived> podr = context.PurchaseOrderRecivedDetails.Where(a => a.PurchaseOrderId == poid).ToList();
                    List<IRMaster> irm = context.IRMasterDB.Where(a => a.PurchaseOrderId == poid).ToList();
                    List<IR_Confirm> irc = context.IR_ConfirmDb.Where(a => a.PurchaseOrderId == poid).ToList();

                    if (pom != null)
                    {
                        if (pom.Gr1_Amount != 0 && pom.Gr1Status != null)
                        {
                            List<GRItems> objGrItem = new List<GRItems>();

                            foreach (var a in podr)
                            {
                                GRItems gritem = new GRItems()
                                {
                                    ItemId = a.ItemId,
                                    ItemName = a.ItemName1,
                                    Itemnumber = a.ItemNumber,
                                    PurchaseOrderDetailRecivedId = a.PurchaseOrderDetailRecivedId,
                                    Quantity = a.QtyRecived1
                                };
                                objGrItem.Add(gritem);
                            }

                            GRIds obj = new GRIds()
                            {
                                PurchaseOrderId = pom.PurchaseOrderId,
                                GrNumber = pom.Gr1Number,
                                gritem = objGrItem
                            };
                            GRIdsO.Add(obj);
                        }
                        if (pom.Gr2_Amount != 0 && pom.Gr2Status != null)
                        {
                            List<GRItems> objGrItem = new List<GRItems>();

                            foreach (var a in podr)
                            {
                                GRItems gritem = new GRItems()
                                {
                                    ItemId = a.ItemId,
                                    ItemName = a.ItemName2,
                                    Itemnumber = a.ItemNumber,
                                    PurchaseOrderDetailRecivedId = a.PurchaseOrderDetailRecivedId,
                                    Quantity = a.QtyRecived2
                                };
                                objGrItem.Add(gritem);
                            }

                            GRIds obj = new GRIds()
                            {
                                PurchaseOrderId = pom.PurchaseOrderId,
                                GrNumber = pom.Gr2Number,
                                gritem = objGrItem
                            };
                            GRIdsO.Add(obj);
                        }
                        if (pom.Gr3_Amount != 0 && pom.Gr3Status != null)
                        {
                            List<GRItems> objGrItem = new List<GRItems>();

                            foreach (var a in podr)
                            {
                                GRItems gritem = new GRItems()
                                {
                                    ItemId = a.ItemId,
                                    ItemName = a.ItemName3,
                                    Itemnumber = a.ItemNumber,
                                    PurchaseOrderDetailRecivedId = a.PurchaseOrderDetailRecivedId,
                                    Quantity = a.QtyRecived3
                                };
                                objGrItem.Add(gritem);
                            }

                            GRIds obj = new GRIds()
                            {
                                PurchaseOrderId = pom.PurchaseOrderId,
                                GrNumber = pom.Gr3Number,
                                gritem = objGrItem
                            };
                            GRIdsO.Add(obj);
                        }
                        if (pom.Gr4_Amount != 0 && pom.Gr4Status != null)
                        {
                            List<GRItems> objGrItem = new List<GRItems>();

                            foreach (var a in podr)
                            {
                                GRItems gritem = new GRItems()
                                {
                                    ItemId = a.ItemId,
                                    ItemName = a.ItemName4,
                                    Itemnumber = a.ItemNumber,
                                    PurchaseOrderDetailRecivedId = a.PurchaseOrderDetailRecivedId,
                                    Quantity = a.QtyRecived4
                                };
                                objGrItem.Add(gritem);
                            }


                            GRIds obj = new GRIds()
                            {
                                PurchaseOrderId = pom.PurchaseOrderId,
                                GrNumber = pom.Gr4Number,
                                gritem = objGrItem
                            };
                            GRIdsO.Add(obj);
                        }
                        if (pom.Gr5_Amount != 0 && pom.Gr5Status != null)
                        {
                            List<GRItems> objGrItem = new List<GRItems>();

                            foreach (var a in podr)
                            {
                                GRItems gritem = new GRItems()
                                {
                                    ItemId = a.ItemId,
                                    ItemName = a.ItemName5,
                                    PurchaseOrderDetailRecivedId = a.PurchaseOrderDetailRecivedId,
                                    Itemnumber = a.ItemNumber,
                                    Quantity = a.QtyRecived5
                                };
                                objGrItem.Add(gritem);
                            }

                            GRIds obj = new GRIds()
                            {
                                PurchaseOrderId = pom.PurchaseOrderId,
                                GrNumber = pom.Gr5Number,
                                gritem = objGrItem
                            };
                            GRIdsO.Add(obj);
                        }
                    }

                    if (irm != null)
                    {
                        foreach (IRMaster a in irm)
                        {
                            List<IRItems> ObjIRitem = new List<IRItems>();
                            //if (a.IRType == "IR1") {
                            //    foreach (var ir in irc)
                            //    {

                            //        IRItems iritem = new IRItems()
                            //        {
                            //            IRreceiveid = ir.IRreceiveid,
                            //            ItemName = ir.ItemName,
                            //            Quantity = ir.QtyRecived1
                            //        };
                            //        ObjIRitem.Add(iritem);
                            //    }
                            //}
                            switch (a.IRType)
                            {
                                case "IR1":
                                    foreach (var ir in irc)
                                    {

                                        IRItems iritem = new IRItems()
                                        {
                                            IRreceiveid = ir.IRreceiveid,
                                            ItemName = ir.ItemName,
                                            Quantity = ir.QtyRecived1
                                        };
                                        ObjIRitem.Add(iritem);
                                    }
                                    break;
                                case "IR2":
                                    foreach (var ir in irc)
                                    {

                                        IRItems iritem = new IRItems()
                                        {
                                            IRreceiveid = ir.IRreceiveid,
                                            ItemName = ir.ItemName,
                                            Quantity = ir.QtyRecived2
                                        };
                                        ObjIRitem.Add(iritem);
                                    }
                                    break;
                                case "IR3":
                                    foreach (var ir in irc)
                                    {
                                        IRItems iritem = new IRItems()
                                        {
                                            IRreceiveid = ir.IRreceiveid,
                                            ItemName = ir.ItemName,
                                            Quantity = ir.QtyRecived3
                                        };
                                        ObjIRitem.Add(iritem);
                                    }
                                    break;
                                case "IR4":
                                    foreach (var ir in irc)
                                    {

                                        IRItems iritem = new IRItems()
                                        {
                                            IRreceiveid = ir.IRreceiveid,
                                            ItemName = ir.ItemName,
                                            Quantity = ir.QtyRecived4
                                        };
                                        ObjIRitem.Add(iritem);
                                    }
                                    break;
                                case "IR5":
                                    foreach (var ir in irc)
                                    {
                                        IRItems iritem = new IRItems()
                                        {
                                            IRreceiveid = ir.IRreceiveid,
                                            ItemName = ir.ItemName,
                                            Quantity = ir.QtyRecived5
                                        };
                                        ObjIRitem.Add(iritem);
                                    }
                                    break;
                                default:
                                    break;
                            }

                            IRIds obj = new IRIds()
                            {
                                IrId = a.Id,
                                IrNumber = a.IRID,
                                IrType = a.IRType,
                                Iritem = ObjIRitem
                            };
                            IRIdsO.Add(obj);
                        }
                    }

                    res = new GRResposeDTO()
                    {
                        GRIdsObj = GRIdsO,
                        IRIdsObj = IRIdsO
                    };
                }
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [Route("SettledGRIR")]
        [HttpPut]

        public HttpResponseMessage Settled(GRResposeDTO data)
        {
            GRResposeDTO res = new GRResposeDTO();

            try
            {
                int poid = data.GRIdsObj[0].PurchaseOrderId;
                var selectedIR = data.IRIdsObj.Where(a => a.IsSelected == true);
                if (selectedIR.Count() == 1)
                {
                    var singledata = selectedIR.SingleOrDefault();
                    string IRType = singledata.IrType;
                    string GrSetteldIds = string.Empty;
                    using (var context = new AuthContext())
                    {
                        List<IR_Confirm> irc = context.IR_ConfirmDb.Where(a => a.PurchaseOrderId == poid).ToList();

                        var selectedGR = data.GRIdsObj.Where(a => a.IsSelected == true);

                        foreach (IR_Confirm i in irc)
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

                            switch (IRType)
                            {
                                case "IR1":
                                    i.Q1Settled = GrSetteldIds;
                                    break;
                                case "IR2":
                                    i.Q2Settled = GrSetteldIds;
                                    break;
                                case "IR3":
                                    i.Q3Settled = GrSetteldIds;
                                    break;
                                case "IR4":
                                    i.Q4Settled = GrSetteldIds;
                                    break;
                                case "IR5":
                                    i.Q5Settled = GrSetteldIds;
                                    break;
                                default:
                                    break;
                            }
                            context.Commit();
                            GrSetteldIds = string.Empty;
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "GR settled with only one IR.");

                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }


        [Route("SettledData")]
        [HttpGet]
        public HttpResponseMessage SettledData(int poid)
        {
            List<IR_Confirm> _IRC = new List<IR_Confirm>();
            try
            {
                using (var _context = new AuthContext())
                {
                    _IRC = _context.IR_ConfirmDb.Where(a => a.PurchaseOrderId == poid).ToList();
                    bool _IfAnySettled = _IRC.Any(a => a.Q1Settled != null || a.Q2Settled != null || a.Q3Settled != null || a.Q4Settled != null || a.Q5Settled != null);

                    if (_IfAnySettled)
                    {
                        foreach (var s in _IRC)
                        {
                            if (s.Q1Settled != null)
                            {

                                var SettledGR = s.Q1Settled;
                                var SplitGR = SettledGR.Split(',');
                                s.Q1Settled = null;
                                foreach (var i in SplitGR)
                                {
                                    if (i == "1")
                                    {
                                        s.Q1Settled = poid + "A";
                                    }
                                    if (i == "2")
                                    {
                                        s.Q1Settled += ", " + poid + "B";
                                    }
                                    if (i == "3")
                                    {
                                        s.Q1Settled += ", " + poid + "C";
                                    }
                                    if (i == "4")
                                    {
                                        s.Q1Settled += ", " + poid + "D";
                                    }
                                    if (i == "5")
                                    {
                                        s.Q1Settled += ", " + poid + "E";
                                    }
                                }
                                s.Q1Settled = s.Q1Settled.TrimStart(',');
                            }

                            if (s.Q2Settled != null)
                            {
                                var SettledGR = s.Q2Settled;
                                var SplitGR = SettledGR.Split(',');
                                s.Q2Settled = null;
                                foreach (var i in SplitGR)
                                {
                                    if (i == "1")
                                    {
                                        s.Q2Settled = poid + "A";
                                    }
                                    if (i == "2")
                                    {
                                        s.Q2Settled += ", " + poid + "B";
                                    }
                                    if (i == "3")
                                    {
                                        s.Q2Settled += ", " + poid + "C";
                                    }
                                    if (i == "4")
                                    {
                                        s.Q2Settled += ", " + poid + "D";
                                    }
                                    if (i == "5")
                                    {
                                        s.Q2Settled += ", " + poid + "E";
                                    }
                                }
                                s.Q2Settled = s.Q2Settled.TrimStart(',');

                            }

                            if (s.Q3Settled != null)
                            {

                                var SettledGR = s.Q3Settled;
                                var SplitGR = SettledGR.Split(',');
                                s.Q3Settled = null;
                                foreach (var i in SplitGR)
                                {
                                    if (i == "1")
                                    {
                                        s.Q3Settled = poid + "A";
                                    }
                                    if (i == "2")
                                    {
                                        s.Q3Settled += ", " + poid + "B";
                                    }
                                    if (i == "3")
                                    {
                                        s.Q3Settled += ", " + poid + "C";
                                    }
                                    if (i == "4")
                                    {
                                        s.Q3Settled += ", " + poid + "D";
                                    }
                                    if (i == "5")
                                    {
                                        s.Q3Settled += ", " + poid + "E";
                                    }
                                }
                                s.Q3Settled = s.Q3Settled.TrimStart(',');

                            }

                            if (s.Q4Settled != null)
                            {
                                var SettledGR = s.Q4Settled;
                                var SplitGR = SettledGR.Split(',');
                                s.Q4Settled = null;
                                foreach (var i in SplitGR)
                                {
                                    if (i == "1")
                                    {
                                        s.Q4Settled = poid + "A";
                                    }
                                    if (i == "2")
                                    {
                                        s.Q4Settled += ", " + poid + "B";
                                    }
                                    if (i == "3")
                                    {
                                        s.Q4Settled += ", " + poid + "C";
                                    }
                                    if (i == "4")
                                    {
                                        s.Q4Settled += ", " + poid + "D";
                                    }
                                    if (i == "5")
                                    {
                                        s.Q4Settled += ", " + poid + "E";
                                    }
                                }
                                s.Q4Settled = s.Q4Settled.TrimStart(',');
                            }

                            if (s.Q5Settled != null)
                            {
                                var SettledGR = s.Q5Settled;
                                var SplitGR = SettledGR.Split(',');
                                s.Q5Settled = null;
                                foreach (var i in SplitGR)
                                {
                                    if (i == "1")
                                    {
                                        s.Q5Settled = poid + "A";
                                    }
                                    if (i == "2")
                                    {
                                        s.Q5Settled += ", " + poid + "B";
                                    }
                                    if (i == "3")
                                    {
                                        s.Q5Settled += ", " + poid + "C";
                                    }
                                    if (i == "4")
                                    {
                                        s.Q5Settled += ", " + poid + "D";
                                    }
                                    if (i == "5")
                                    {
                                        s.Q5Settled += ", " + poid + "E";
                                    }
                                }
                                s.Q5Settled = s.Q5Settled.TrimStart(',');
                            }
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "No Settlement.");
                    }

                }
                return Request.CreateResponse(HttpStatusCode.OK, _IRC);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [Route("UnSettledGRIR")]
        [HttpPut]

        public HttpResponseMessage UnSettled(GRResposeDTO data)
        {
            try
            {
                int poid = data.GRIdsObj[0].PurchaseOrderId;
                using (var context = new AuthContext())
                {
                    List<IR_Confirm> irc = context.IR_ConfirmDb.Where(a => a.PurchaseOrderId == poid).ToList();
                    foreach (IR_Confirm a in irc)
                    {
                        a.Q1Settled = null;
                        a.Q2Settled = null;
                        a.Q3Settled = null;
                        a.Q4Settled = null;
                        a.Q5Settled = null;
                    }
                    context.Commit();
                }
                return Request.CreateResponse(HttpStatusCode.OK, "UnSettled.");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
        #endregion

        //[Route("GetItems")]
        //[HttpGet]
        //public  async Task<List<ItemMaster>> GetItems(int id, int Wid, string searchKey)
        //{
        //    List<ItemMaster> item = new List<ItemMaster>();

        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;

        //        foreach (Claim claim in identity.Claims)
        //        {
        //            if (claim.Type == "compid")
        //            {
        //                compid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "userid")
        //            {
        //                userid = int.Parse(claim.Value);
        //            }
        //        }
        //        int CompanyId = compid;
        //        using (AuthContext db = new AuthContext())
        //        {


        //            item = db.itemMasters.Where(c => c.WarehouseId == Wid && c.CompanyId == CompanyId && c.itemname.Contains(searchKey)).ToList();

        //            List<ItemClassificationDC> ABCitemsList = item.Select(itqem => new ItemClassificationDC { ItemNumber = itqem.ItemNumber, WarehouseId = itqem.WarehouseId }).ToList();

        //            var manager = new ItemLedgerManager();
        //            var GetItem = await manager.GetItemClassificationsAsync(ABCitemsList);


        //            foreach (var it in item)
        //            {

        //                if (GetItem != null && GetItem.Any())
        //                {
        //                    if (GetItem.Any(x => x.ItemNumber == it.ItemNumber))
        //                    {
        //                        it.Category = GetItem.FirstOrDefault(x => x.ItemNumber == it.ItemNumber).Category;
        //                    }
        //                    else { it.Category = "D"; }
        //                }
        //                else { it.Category = "D"; }

        //            }

        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        item = null;
        //    }

        //    return item;
        //}

        [HttpGet]
        [Route("GetItems")]
        public async Task<List<ItemMaster>> GetItems(int id, int Wid, string searchKey)
        {
            logger.Info("start Item Master: ");
            List<ItemMaster> itemList = new List<ItemMaster>();
            List<ItemMaster> result = new List<ItemMaster>();

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            int CompanyId = compid;
            using (AuthContext db = new AuthContext())
            {
                if (searchKey != null && Wid > 0)
                {
                    itemList = db.itemMasters.Where(t => t.WarehouseId == Wid && (t.itemname.Contains(searchKey) || t.Number.Contains(searchKey)) && t.Deleted == false).ToList();
                    List<ItemClassificationDC> ABCitemsList = itemList.Select(item => new ItemClassificationDC { ItemNumber = item.ItemNumber, WarehouseId = item.WarehouseId }).ToList();
                    var manager = new ItemLedgerManager();
                    var GetItem = await manager.GetItemClassificationsAsync(ABCitemsList);
                    foreach (var item in itemList)
                    {
                        if (itemList.Any(t => t.PurchaseSku == item.PurchaseSku))
                        {
                            var maxPurchasePrice = itemList.Where(t => t.PurchaseSku == item.PurchaseSku && t.ItemMultiMRPId == item.ItemMultiMRPId).Max(x => x.POPurchasePrice);
                            var items = itemList.Where(t => t.PurchaseSku == item.PurchaseSku && t.POPurchasePrice == maxPurchasePrice && t.ItemMultiMRPId == item.ItemMultiMRPId).FirstOrDefault();
                            items.Category = GetItem.Where(x => x.ItemNumber == items.ItemNumber).Select(x => x.Category).FirstOrDefault() != null ? GetItem.Where(x => x.ItemNumber == items.ItemNumber).Select(x => x.Category).FirstOrDefault() : "D";
                            if (items != null && !result.Any(x => x.PurchaseSku == items.PurchaseSku && x.ItemMultiMRPId == item.ItemMultiMRPId)) /*/*!PurchaseSku.Any(x => x == items.PurchaseSku))*/
                            {
                                result.Add(items);
                            }
                        }
                    }
                }
            }
            return result;
        }
    }

    public class GRIds
    {
        public int PurchaseOrderId { get; set; }
        public string GrNumber { get; set; }
        public bool IsSelected { get; set; }
        public List<GRItems> gritem { get; set; }
    }

    public class GRItems
    {
        public int PurchaseOrderDetailRecivedId { get; set; }
        public int ItemId { get; set; }
        public string Itemnumber { get; set; }
        public string ItemName { get; set; }
        public int? Quantity { get; set; }
        public bool IsSelected { get; set; }
    }

    public class IRIds
    {
        public int IrId { get; set; }
        public string IrNumber { get; set; }
        public string IrType { get; set; }
        public bool IsSelected { get; set; }
        public List<IRItems> Iritem { get; set; }
    }

    public class IRItems
    {
        public int IRreceiveid { get; set; }
        public string ItemName { get; set; }
        public double? Quantity { get; set; }
    }

    public class GRResposeDTO
    {
        public List<GRIds> GRIdsObj { get; set; }
        public List<IRIds> IRIdsObj { get; set; }

    }

    //
    public class GRs
    {
        public string GrNumber { get; set; }
        public string IRID { get; set; }
    }
    //

}



