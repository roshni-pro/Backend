using NLog;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [Authorize]
    [RoutePrefix("api/DispachedOrderPage")]
    public class DispachedOrderListController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
       
        //public IHost _environment { get; set; }
        int compid = 0, userid = 0;

        public DispachedOrderListController()
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
        [Route("")]
        [AcceptVerbs("GET")]
        public async Task<IHttpActionResult> GetDispachedOrderList()
        {
            using (var db = new AuthContext())
            {
                var date = DateTime.Today;
                var d = date.AddDays(1);
                var query = from om in db.DbOrderMaster
                            join od in db.OrderDispatchedMasters
                            on om.OrderId equals od.OrderId
                            join omh in db.OrderMasterHistoriesDB
                            on om.OrderId equals omh.orderid
                            where omh.Status == "Issued" && omh.CreatedDate >= date && omh.CreatedDate < d

                            select new
                            {
                                OrderID = om.OrderId,
                                DateOFDispached = omh.CreatedDate,
                                OrderedAmount = om.GrossAmount,
                                DispachedAmount = od.GrossAmount,
                                DBoyName = od.DboyName,
                                AssignmentID = od.DeliveryIssuanceIdOrderDeliveryMaster,
                                WarehouseName = od.WarehouseName,
                                Status = od.Status,
                                UpdatedDate = od.UpdatedDate,

                            };
                var result = await query.ToListAsync();
                return Ok(result);
            }
        }

        //[route("")]
        //[acceptverbs("get")]
        //public async task<ihttpactionresult> getdispachedorderlistbydate(datetime d)
        //{
        //    var date = d.adddays(1);
        //    var query = from om in db.dbordermaster
        //                join od in db.orderdispatchedmasters
        //                    on om.orderid equals od.orderid
        //                where od.status == "ready to dispatch" && om.readytodispatcheddate >= d && om.readytodispatcheddate < date
        //                select new
        //                {
        //                    orderid = om.orderid,
        //                    dateofdispached = od.createddate,
        //                    orderedamount = om.grossamount,
        //                    dispachedamount = od.grossamount,
        //                    dboyname = od.dboyname,
        //                    assignmentid = od.deliveryissuanceidorderdeliverymaster,
        //                };
        //    var result = await query.tolistasync();
        //    return ok(result);

        //}
        //[route("getdispachedorderexportlist")]
        //[acceptverbs("get")]
        ////[httpget]
        //public async task<ihttpactionresult> getdispachedorderexportlist(datetime d, int warehouseid, string dboy)
        //{

        //    var date = d.adddays(1);
        //    var today = datetime.now.tostring("dd-mm-yyyy hh:mm");
        //    var query = from om in db.dbordermaster
        //                join od in db.orderdispatchedmasters
        //                    on om.orderid equals od.orderid
        //                where od.status == "ready to dispatch" && om.readytodispatcheddate >= d && om.readytodispatcheddate < date
        //                select new
        //                {
        //                    orderid = om.orderid,
        //                    dateofdispached = od.createddate,
        //                    orderedamount = om.grossamount,
        //                    dispachedamount = od.grossamount,
        //                    dboyname = od.dboyname,
        //                    assignmentid = od.deliveryissuanceidorderdeliverymaster,
        //                };
        //    var result = await query.tolistasync();


        //    datatable dt = listtodatatableconverter.todatatable(result);

        //    dataset ds = new dataset();
        //    ds.tables.add(dt);

        //    exportservices.dataset_to_excel(ds, @"c:\users\shopkirana\desktop\angular_backend\shopkirana-backend\angularjsauthentication.api\reports\orderlist1.csv");

        //    return ok();

        //}
        //[Route("GetData")]
        //[AcceptVerbs("POST")]
        //public async Task<IHttpActionResult> SearchDispachedOrderListData(int warehouseId, string DboyMobileNo, DateTime? start, DateTime? end)
        //{

        //    //var date = data.Date.AddDays(1);
        //    var query = from om in db.DbOrderMaster
        //                join od in db.OrderDispatchedMasters
        //                on om.OrderId equals od.OrderId
        //                join omh in db.OrderMasterHistoriesDB
        //                on om.OrderId equals omh.orderid
        //                where omh.Status == "Issued"
        //                && (od.WarehouseId == warehouseId) || ((string.IsNullOrEmpty(DboyMobileNo) || (od.DboyMobileNo == DboyMobileNo)) 
        //                || (start == null || (omh.CreatedDate >= start && omh.CreatedDate <= end)))
        //                select new
        //                {
        //                    OrderID = om.OrderId,
        //                    DateOFDispached = omh.CreatedDate,
        //                    OrderedAmount = om.GrossAmount,
        //                    DispachedAmount = od.GrossAmount,
        //                    DBoyName = od.DboyName,
        //                    AssignmentID = od.DeliveryIssuanceIdOrderDeliveryMaster
        //                };
        //    var result = await query.OrderBy(x => x.DateOFDispached).ToListAsync();
        //    //if (data.IsGenerateExcel)
        //    //{
        //    //    DataTable dt = ListtoDataTableConverter.ToDataTable(result);

        //    //    DataSet ds = new DataSet();
        //    //    ds.Tables.Add(dt);

        //    //    ExportServices.DataSet_To_Excel(ds, @"C:\Users\ShopKirana\Desktop\angular_backend\Shopkirana-Backend\AngularJSAuthentication.API\Reports\orderlist1.csv");

        //    //}
        //    return Ok(result);


        //}
        // }


        [Route("GetData")]
        [AcceptVerbs("POST")]
        public async Task<IHttpActionResult> SearchDispachedOrderListData(string warehouseId, string DboyMobileNo, DateTime? start, DateTime? end)
        {
            string whereclause = "";

            using (AuthContext context = new AuthContext())
            {
                // if (warehouseId != null)
                if (!string.IsNullOrEmpty(warehouseId))

                    // whereclause += " and ODM.WarehouseId = " + warehouseId;
                    whereclause += " and ODM.WarehouseId IN (" + warehouseId + ")"; 
                if (!string.IsNullOrEmpty(DboyMobileNo))
                    whereclause += " and ODM.DboyMobileNo Like " + "'%" + DboyMobileNo + "%'";

                if (start.HasValue && end.HasValue)
                    whereclause += " and (OMH.CreatedDate >= " + "'" + start.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'" + " And  OMH.CreatedDate <=" + "'" + end.Value.ToString("yyyy-MM-dd HH:mm:ss") + "')";

                string sqlquery = " SELECT OM.OrderId,OMH.CreatedDate AS DateOFDispached,ODM.Status,ODM.WarehouseName,ODM.UpdatedDate,OM.GrossAmount AS OrderedAmount,ODM.GrossAmount AS DispachedAmount," +
                    " ODM.DboyName AS DBoyName,ODM.DeliveryIssuanceIdOrderDeliveryMaster AS AssignmentID" +
                    " FROM OrderMasters OM INNER JOIN OrderDispatchedMasters ODM ON OM.OrderId = ODM.OrderId" +
                    " INNER JOIN OrderMasterHistories OMH ON OM.OrderId = OMH.orderid where OMH.Status = 'Issued' " + whereclause
                   + " Order by OMH.CreatedDate desc";

                var result = await context.Database.SqlQuery<DispachedOrderListDc>(sqlquery).ToListAsync();
                return Ok(result);
            }

        }
    }
    public class DispatchedOrderViewModel
    {
        public DateTime Date { get; set; }
        public int warehouseId { get; set; }
        public string dBoy { get; set; }
        public Boolean IsGenerateExcel { get; set; }
    }

    public class DispachedOrderListDc
    {
        public int OrderID { get; set; }
        public DateTime? DateOFDispached { get; set; }
        public double OrderedAmount { get; set; }
        public double DispachedAmount { get; set; }
        public string DBoyName { get; set; }
        public int? AssignmentID { get; set; }
        public string WarehouseName { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string Status { get; set; }

    }
}