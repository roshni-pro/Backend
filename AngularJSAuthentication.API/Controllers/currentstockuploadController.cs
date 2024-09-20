using AngularJSAuthentication.Model;
using NLog;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/currentstockupload")]
    public class currentstockuploadController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();
        string msg;
        string strJSON = null;
        string col0, col1, col2, col3, col4, col5, col6;
        [HttpPost]
        public string UploadFile()
        {
            return "";
        }

        internal bool UploadDiffcurrentstock(List<CurrentStockUploadDTO> currentstkcollection, string userid, AuthContext context)
        {
            int PeopleId = Convert.ToInt32(userid);
            var User = context.Peoples.FirstOrDefault(x => x.PeopleID == PeopleId);
            List<CurrentStock> UpdateCurrentStock = new List<CurrentStock>();
            List<CurrentStockUploadDTO> CurrentStockUploadDTO = new List<CurrentStockUploadDTO>();
            List<CurrentStockHistory> AddCurrentStockHistory = new List<CurrentStockHistory>();
            if (User.PeopleID > 0)
            {
                int Warehouseid = currentstkcollection[0].WarehouseId;
                var CurrentstokNumberList = currentstkcollection.Select(x => x.ItemNumber).ToList();
                List<CurrentStock> CurrentStockList = context.DbCurrentStock.Where(c => CurrentstokNumberList.Contains(c.ItemNumber) && c.WarehouseId == Warehouseid).ToList();
                foreach (var o in currentstkcollection)
                {
                    CurrentStock cst = CurrentStockList.Where(c => c.ItemNumber == o.ItemNumber && c.WarehouseId == o.WarehouseId && c.ItemMultiMRPId == o.ItemMultiMRPId).FirstOrDefault(); ;


                    var IsValidateQty = (cst.CurrentInventory + (o.DiffStock));//If Inventory is Positve 
                    if (IsValidateQty >= 0)
                    {
                        if (cst != null)
                        {
                            cst.UpdatedDate = DateTime.Now;
                            cst.CurrentInventory = (cst.CurrentInventory + (o.DiffStock));
                            //this.Entry(cst).State = EntityState.Modified;
                            UpdateCurrentStock.Add(cst);
                            CurrentStockHistory Oss = new CurrentStockHistory();
                            Oss.StockId = cst.StockId;
                            Oss.ItemMultiMRPId = cst.ItemMultiMRPId;
                            Oss.ManualReason = "From Uploader";
                            Oss.ItemNumber = cst.ItemNumber;
                            Oss.itemBaseName = cst.itemBaseName;
                            Oss.itemname = cst.itemname;
                            Oss.TotalInventory = cst.CurrentInventory;
                            Oss.ManualInventoryIn = o.DiffStock;
                            Oss.WarehouseName = cst.WarehouseName;
                            Oss.Warehouseid = cst.WarehouseId;
                            Oss.CompanyId = cst.CompanyId;
                            Oss.CreationDate = DateTime.Now;
                            Oss.ManualReason = o.Reason;
                            Oss.userid = User.PeopleID;
                            Oss.UserName = User.DisplayName;
                            AddCurrentStockHistory.Add(Oss);
                        }
                    }
                    //else
                    //{
                    //    CurrentStockUploadDTO.Add(o);
                    //}
                }
                foreach (var item in UpdateCurrentStock)
                {
                    context.Entry(item).State = EntityState.Modified;
                }
                context.CurrentStockHistoryDb.AddRange(AddCurrentStockHistory);
            }

            if (context.Commit() > 0) { return true; } else { return false; }
        }
    }
}




