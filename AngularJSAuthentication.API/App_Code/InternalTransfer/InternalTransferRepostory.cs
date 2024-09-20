using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;
using AngularJSAuthentication.BusinessLayer.InternalTransfer.BO;
using AngularJSAuthentication.BusinessLayer.InternalTransfer.IF;
using AngularJSAuthentication.Model;

namespace AngularJSAuthentication.API.App_Code.InternalTransfer
{
    public class InternalTransferRepostory : IInternalTransfer, IDisposable
    {
        private bool disposed = false;
        private AuthContext Context;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Context.Dispose();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public InternalTransferRepostory(AuthContext Context)
        {
            this.Context = Context;
        }
        public List<InternalTransferDetails> GetInternalTransferDetails(int Warehouseid)
        {
            try
            {


                using (AuthContext context = new AuthContext())
                {
                    var Param = new SqlParameter
                    {
                        ParameterName = "Warehouseid",
                        Value = Warehouseid

                    };
                    List<InternalTransferDetails> objInternalTransferDetails = context.Database.SqlQuery<InternalTransferDetails>("Intertransferdetails @Warehouseid", Param).ToList();
                    return objInternalTransferDetails;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool PostItemTransfer(PostTransferData objPostTransferData)
        {
            try
            {
                if (objPostTransferData.TransferOrderDetails.Count == 0)
                {
                    throw new Exception("Atleast contain 1 item");

                }


                bool Res = PostItemTransfertDetails(objPostTransferData);

                return Res;


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool PostItemTransfertDetails(PostTransferData objPostTransferData)
        {
            try
            {
                bool Result = false;
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (AuthContext context = new AuthContext())
                    {
                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();
                        List<ItemMaster> ObjItemmaster = context.itemMasters.Where(x => x.WarehouseId == objPostTransferData.RequestFromWarehouseid).ToList();
                        SqlParameter param1 = new SqlParameter("CompanyId", objPostTransferData.CompanyId);

                        SqlParameter param2 = new SqlParameter("RequestFromWarehouseid", objPostTransferData.RequestFromWarehouseid);

                        SqlParameter param3 = new SqlParameter("Peopleid", objPostTransferData.Peopleid);


                        DataTable dt = new DataTable();
                        dt.Columns.Add("CityId");
                        dt.Columns.Add("WareHouseId");
                        dt.Columns.Add("ItemNumber");
                        dt.Columns.Add("WareHouseName");
                        dt.Columns.Add("RequestToWarehouseId");
                        dt.Columns.Add("RequestToWarehouseName");
                        dt.Columns.Add("ItemName");
                        dt.Columns.Add("TotalQuantity");
                        dt.Columns.Add("ItemMultipMrpId");
                        dt.Columns.Add("ItemId");
                        dt.Columns.Add("HSNCode");
                        //objPostTransferData.TransferOrderDetails.ForEach(x =>
                        //{
                        //    DataRow dr = dt.NewRow();
                        //    dr["CityId"] = x.CityId;
                        //    dr["WareHouseId"] = x.WareHouseId;
                        //    dr["ItemNumber"] = x.ItemNumber;
                        //    dr["WareHouseName"] = x.WareHouseName;
                        //    dr["RequestToWarehouseId"] = x.RequestToWarehouseId;
                        //    dr["RequestToWarehouseName"] = x.RequestToWarehouseName;
                        //    dr["ItemName"] = x.ItemName;
                        //    dr["TotalQuantity"] = x.TotalQuantity;
                        //    dr["ItemMultipMrpId"] = x.ItemMultipMrpId;
                        //    dr["ItemId"] = GetItemId(objPostTransferData.RequestFromWarehouseid, x.ItemNumber,ObjItemmaster);
                        //    dr["HSNCode"] = x.HSNCode;
                        //    dt.Rows.Add(dr);

                        //});


                        for (int i = 0; i < objPostTransferData.TransferOrderDetails.Distinct().Count(); i++)
                        {
                            int countRequestToWareHouseId = objPostTransferData.TransferOrderDetails.Where(x => x.RequestToWarehouseId == objPostTransferData.TransferOrderDetails[i].RequestToWarehouseId).Count();
                            var data = objPostTransferData.TransferOrderDetails.Where(x => x.RequestToWarehouseId == objPostTransferData.TransferOrderDetails[i].RequestToWarehouseId).ToList();
                            for (int j = 0; j < countRequestToWareHouseId; j++)
                            {
                                DataRow dr = dt.NewRow();
                                dr["CityId"] = data[j].CityId;
                                dr["WareHouseId"] = data[j].WareHouseId;
                                dr["ItemNumber"] = data[j].ItemNumber;
                                dr["WareHouseName"] = data[j].WareHouseName;
                                dr["RequestToWarehouseId"] = data[j].RequestToWarehouseId;
                                dr["RequestToWarehouseName"] = data[j].RequestToWarehouseName;
                                dr["ItemName"] = data[j].ItemName;
                                dr["TotalQuantity"] = data[j].TotalQuantity;
                                dr["ItemMultipMrpId"] = data[j].ItemMultipMrpId;
                                dr["ItemId"] = GetItemId(objPostTransferData.RequestFromWarehouseid, data[j].ItemNumber, ObjItemmaster);
                                dr["HSNCode"] = data[j].HSNCode;
                                dt.Rows.Add(dr);
                            }
                            
                            if (CalculateAmount(objPostTransferData, ObjItemmaster) > 90000)
                            {
                                throw new Exception("Total Amount cannot be greater than Rs900000/-");
                            }
                            SqlParameter param = new SqlParameter("InternalTransferItemparam", dt);
                            param.SqlDbType = SqlDbType.Structured;
                            param.TypeName = "dbo.InternalTransferItem";
                            var cmd = context.Database.Connection.CreateCommand();
                            cmd.CommandText = "[dbo].[PostInterNalTransferItem]";
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.Parameters.Add(param);
                            cmd.Parameters.Add(param1);
                            cmd.Parameters.Add(param2);
                            cmd.Parameters.Add(param3);
                            //cmd.Parameters.Add(objPostTransferData.RequestFromWarehouseid);
                            //cmd.Parameters.Add(objPostTransferData.Peopleid);
                            int NoofRowsAffected = cmd.ExecuteNonQuery();
                            if (NoofRowsAffected > 2)
                            {
                                Result = true;
                                

                            }
                            else
                            {
                                throw new Exception("Something went wrong please try again later!!!");
                            }
                            dt.Clear();
                            cmd.Parameters.Clear();
                            i = i + (countRequestToWareHouseId-1);
                        }
                        if (Result)
                        {
                            context.SaveChanges();
                            scope.Complete();
                        }
                    }
                }
                return Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private int GetItemId(int WarehouseId, string ItemNumber, List<ItemMaster> ObjItemmaster)
        {

            int ItemId = ObjItemmaster.Where(x => x.ItemNumber == ItemNumber && x.WarehouseId == WarehouseId).Select(x => x.ItemId).FirstOrDefault();
            return ItemId;

        }
        private double CalculateAmount(PostTransferData objPostTransferData, List<ItemMaster> ObjItemmaster)
        {

            List<ItemMaster> ObjItemaster = ObjItemmaster.Where(x => x.WarehouseId == objPostTransferData.RequestFromWarehouseid).ToList();
            //List<ItemMaster> ObjItemaster = context.itemMasters.Where(x => x.WarehouseId == objPostTransferData.RequestFromWarehouseid).ToList();

            objPostTransferData.TransferOrderDetails.ForEach(x =>
            {
                int ItemId = x.ItemId;
                double Price = ObjItemaster.Where(y => y.ItemId == ItemId).Select(y => y.PurchasePrice).FirstOrDefault();
                if (Price == 0)
                {
                    Price = ObjItemaster.Where(y => y.ItemId == ItemId).Select(y => y.MRP).FirstOrDefault();

                }
                x.Price = x.TotalQuantity * Price;
            });

            double TotalAmount = objPostTransferData.TransferOrderDetails.Sum(z => z.Price);
            return TotalAmount;

        }
    }
}