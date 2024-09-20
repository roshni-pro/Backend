using AngularJSAuthentication.BusinessLayer.PackingMaterial.BO;
using AngularJSAuthentication.BusinessLayer.PackingMaterial.IF;
using AngularJSAuthentication.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;

namespace AngularJSAuthentication.API.App_Code.PackingMaterial
{
    public class RawMaterialRepository : IDisposable, IRawMaterial
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
        public RawMaterialRepository(AuthContext Context)
        {
            this.Context = Context;
        }

        public bool InsertRawMaterial(RawMaterailResponse ObjRawMaterailResponse)
        {
            bool FinalRawMst = false;
            bool FinalRawDetails = false;
            bool FinalResult = false;
            bool UpdateCureentStockResult = false;
            try
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope Scope = new TransactionScope(TransactionScopeOption.Required, option))
               // using (TransactionScope Scope = new TransactionScope())
                {
                    ObjRawMaterailResponse = RawMaterialRes(ObjRawMaterailResponse);
                    ObjRawMaterailResponse.RawMaterialMaster.Amount = ObjRawMaterailResponse.RawMaterialDetails.Sum(x => x.Amount);
                    ObjRawMaterailResponse.RawMaterialMaster.TaxableAmount = ObjRawMaterailResponse.RawMaterialDetails.Sum(x => x.TaxableAmount);
                    ObjRawMaterailResponse.RawMaterialMaster.TotalAmount = ObjRawMaterailResponse.RawMaterialMaster.Amount + ObjRawMaterailResponse.RawMaterialMaster.TaxableAmount;
                    ObjRawMaterailResponse.RawMaterialMaster.CsgtAmount = ObjRawMaterailResponse.RawMaterialDetails.Sum(x => x.CgstAmount);
                    ObjRawMaterailResponse.RawMaterialMaster.SgstAmount = ObjRawMaterailResponse.RawMaterialDetails.Sum(x => x.SgstAmount);
                    ObjRawMaterailResponse.RawMaterialMaster.IgstAmount = ObjRawMaterailResponse.RawMaterialDetails.Sum(x => x.IgstAmount);
                    ObjRawMaterailResponse.RawMaterialMaster.InvoiceType = GetInvoiceType(ObjRawMaterailResponse.RawMaterialMaster.WarehouseId, ObjRawMaterailResponse.RawMaterialMaster.BillToWarehouseId);
                    FinalRawMst = InsertRawMaterialMst(ObjRawMaterailResponse);
                    FinalRawDetails = InsertMaterialDetails(ObjRawMaterailResponse);
                    UpdateCureentStockResult = UpdateCurrentStock(ObjRawMaterailResponse);


                    if (FinalRawMst && FinalRawDetails && UpdateCureentStockResult)
                    {
                        Context.SaveChanges();
                        FinalResult = true;
                        Scope.Complete();

                    }
                    else
                    {
                        throw new Exception("Something went wrong please try again later!!!");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Dispose();
            }
            return FinalResult;
        }

        private bool InsertRawMaterialMst(RawMaterailResponse ObjRawMaterailResponse)
        {
            bool Result = false;
            try
            {
                Context.RawMaterialMaster.Add(ObjRawMaterailResponse.RawMaterialMaster);
                Result = true;

            }

            catch (Exception ex)
            {
                throw ex;
            }
            return Result;
        }

        private RawMaterailResponse RawMaterialRes(RawMaterailResponse ObjRawMaterailResponse)
        {

            try
            {

                ObjRawMaterailResponse.RawMaterialMaster.IvchNo = GenerateUniqueNumber();
                int RawDetailID = GenerateUniqueNumber();

                ObjRawMaterailResponse.RawMaterialDetails.ForEach(x =>
                {
                    ItemMaster objItemMaster = GetItemMaster(x.ItemNumber, ObjRawMaterailResponse.RawMaterialMaster.WarehouseId);
                    x.IvchNo = ObjRawMaterailResponse.RawMaterialMaster.IvchNo;
                    x.RawDetailId = RawDetailID;
                    x.Rate = objItemMaster.UnitPrice;
                    int QuantityNoOfpiece = Conversion(x.ItemNumber, x.Qty, x.Uom);
                    x.QtyNoOfPieces = QuantityNoOfpiece;
                    x.Amount = x.Qty * objItemMaster.UnitPrice;
                    x.Discount = objItemMaster.Discount;
                    x.TotalTax = objItemMaster.TotalTaxPercentage;
                    x.Cgst = x.TotalTax / 2;
                    x.Sgst = x.TotalTax / 2;
                    x.CgstAmount = (x.Amount * x.Cgst) / 100;
                    x.SgstAmount = (x.Amount * x.Sgst) / 100;
                    double Igst = WareHouseDetails(ObjRawMaterailResponse.RawMaterialMaster.WarehouseId, ObjRawMaterailResponse.RawMaterialMaster.BillToWarehouseId, x.ItemNumber);
                    x.Igst = Igst;
                    x.IgstAmount = x.Amount * Igst / 100;
                    x.Uom = objItemMaster.UOM;
                    x.TaxableAmount = x.CgstAmount + x.SgstAmount + x.IgstAmount;

                    x.TotalPrice = (x.Amount) * (x.Discount / 100) + x.TaxableAmount;
                    x.CreatedBy = ObjRawMaterailResponse.RawMaterialMaster.CreatedBy;
                    x.CreatedDate = ObjRawMaterailResponse.RawMaterialMaster.CreatedDate;
                    x.ItemReceived = 0;
                    x.UpdatedDate = ObjRawMaterailResponse.RawMaterialMaster.CreatedDate;


                });

                //Context.RawMaterialDetails.AddRange(ObjRawMaterailResponse.RawMaterialDetails);

            }

            catch (Exception ex)
            {
                throw ex;
            }
            return ObjRawMaterailResponse;
        }

        private bool InsertMaterialDetails(RawMaterailResponse ObjRawMaterailResponse)
        {
            bool Result = false;
            try
            {
                Context.RawMaterialDetails.AddRange(ObjRawMaterailResponse.RawMaterialDetails);
                Result = true;
                return Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private double WareHouseDetails(int WareHouseId, int BillWareHouseId, string ItemNumber)
        {

            try
            {
                int StateId = Context.Warehouses.Where(x => x.WarehouseId == WareHouseId).Select(x => x.Stateid).FirstOrDefault();
                int BillStateId = Context.Warehouses.Where(x => x.WarehouseId == BillWareHouseId).Select(x => x.Stateid).FirstOrDefault();
                ItemMaster objItemMaster = GetItemMaster(ItemNumber, WareHouseId);
                double Igst = StateId == BillStateId ? 0 : objItemMaster.TotalTaxPercentage;
                return Igst;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ItemMaster GetItemMaster(string ItemNumber, int WarehouseId)
        {
            ItemMaster objItemMaster = Context.itemMasters.Where(x => x.Number == ItemNumber && x.WarehouseId == WarehouseId).FirstOrDefault();

            return objItemMaster;
        }

        private int GenerateUniqueNumber()
        {
            int number = Convert.ToInt32(String.Format("{0:d9}", (DateTime.Now.Ticks / 10) % 1000000000));
            return number;
        }

        public string GetInvoiceType(int WarehouseId, int BillwarehouseId)
        {
            string WHStateName = string.Empty;
            string BHStateName = string.Empty;
            string InvoiceType = string.Empty;
            try
            {
                WHStateName = Context.Warehouses.Where(x => x.WarehouseId == WarehouseId).FirstOrDefault().StateName;
                BHStateName = Context.Warehouses.Where(x => x.WarehouseId == BillwarehouseId).FirstOrDefault().StateName;
                if (WHStateName.Equals(BHStateName))
                {
                    InvoiceType = "Tax Invoice";
                }
                else
                {
                    InvoiceType = "Delivery Challan";
                }
                return InvoiceType;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ItemMasterDetailsActype GetItemMasterDetailsActype(int Type, int WareHouseId, string ItemNumber)
        {
            try
            {
                SqlParameter ParamType = new SqlParameter
                {
                    ParameterName = "Type",
                    Value = Type
                };
                SqlParameter ParamWareHouseId = new SqlParameter
                {
                    ParameterName = "WareHouseId",
                    Value = WareHouseId
                };
                SqlParameter ParamItemNumber = new SqlParameter
                {
                    ParameterName = "ItemNumber",
                    Value = ItemNumber
                };

                ItemMasterDetailsActype objItemMasterDetailsActype = Context.Database.SqlQuery<ItemMasterDetailsActype>("GetItemMasterDetailsActype @Type,@WareHouseId,@ItemNumber", ParamType, ParamWareHouseId, ParamItemNumber).FirstOrDefault();
                return objItemMasterDetailsActype;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private GetRawMaterialMstDetails GetRawMaterialMstDetails(int InvoiceChallanNo)
        {

            try
            {

                var InvchNo = new SqlParameter
                {
                    ParameterName = "InvoiceChallanNo",
                    Value = InvoiceChallanNo
                };

                GetRawMaterialMstDetails objGetRawMaterialMstDetails = Context.Database.SqlQuery<GetRawMaterialMstDetails>("GetRawMaterialMstAcInvoiceChallan @InvoiceChallanNo", InvchNo).FirstOrDefault();


                return objGetRawMaterialMstDetails;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<RawMaterialDetailsDTO> GetRawMaterialDetails(int InvoiceChallanNo, int BuyerId, int WareHouseId)
        {

            try
            {

                var InvoiceChallanNO = new SqlParameter
                {
                    ParameterName = "InvoiceChallanNo",
                    Value = InvoiceChallanNo
                };
                var BuyerID = new SqlParameter
                {
                    ParameterName = "BuyerId",
                    Value = BuyerId
                };
                var WareHouseID = new SqlParameter
                {
                    ParameterName = "WareHouseId",
                    Value = WareHouseId
                };
                IEnumerable<RawMaterialDetailsDTO> ObjRawMaterialDetails = Context.Database.SqlQuery<RawMaterialDetailsDTO>("GetRawMaterialDetailsAcInvoiceChallanNo @InvoiceChallanNo,@BuyerId,@WareHouseId ", InvoiceChallanNO, BuyerID, WareHouseID).ToList();

                return ObjRawMaterialDetails;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public GetRawMaterialDetailsInvoiceResponse RawMaterialInvoiceDtls(int InvoiceChallanNo)
        {
            try
            {
                GetRawMaterialMstDetails ObjGetRawMaterialMstDetails = GetRawMaterialMstDetails(InvoiceChallanNo);
                IEnumerable<RawMaterialDetailsDTO> ObjGetRawMaterialDetails = GetRawMaterialDetails(InvoiceChallanNo, ObjGetRawMaterialMstDetails.BuyerId, ObjGetRawMaterialMstDetails.WareHouseId);
                GetRawMaterialDetailsInvoiceResponse ObjGetRawMaterialDetailsInvoiceResponse = new GetRawMaterialDetailsInvoiceResponse()
                {
                    GetRawMaterialMstDetails = ObjGetRawMaterialMstDetails,
                    GetRawMaterialDetails = ObjGetRawMaterialDetails
                };

                return ObjGetRawMaterialDetailsInvoiceResponse;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public GetRawMaterialMstDetailsResponse GetRawMaterialMstDetailsAcBuyer(int BuyerId, int Skip, int Take)
        {


            try
            {

                var Buyer = new SqlParameter
                {
                    ParameterName = "BuyerId",
                    Value = BuyerId
                };

                var SKIP = new SqlParameter
                {
                    ParameterName = "Skip",
                    Value = Skip
                };

                var TAKE = new SqlParameter
                {
                    ParameterName = "Take",
                    Value = Take
                };



                IEnumerable<GetRawMaterialMstDetails> ObjGetRawMaterialMstDetails = Context.Database.SqlQuery<GetRawMaterialMstDetails>("GetRawMaterialMstAcBuyer @BuyerId,@Skip,@Take ", Buyer, SKIP, TAKE).ToList();

                int Count = GetRawMaterialCount(BuyerId);

                GetRawMaterialMstDetailsResponse objGetRawMaterialMstDetailsResponse = new GetRawMaterialMstDetailsResponse();

               
                objGetRawMaterialMstDetailsResponse.GetRawMaterialMstDetails = ObjGetRawMaterialMstDetails.OrderByDescending(x=>x.CreatedDate);
                //objGetRawMaterialMstDetailsResponse.Count = Count;
                objGetRawMaterialMstDetailsResponse.Count = objGetRawMaterialMstDetailsResponse.GetRawMaterialMstDetails.Count();
                objGetRawMaterialMstDetailsResponse.GetRawMaterialMstDetails = objGetRawMaterialMstDetailsResponse.GetRawMaterialMstDetails.Skip(Skip).Take(Take).ToList();
                return objGetRawMaterialMstDetailsResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public int GetRawMaterialCount(int BuyerId)
        {
            int Records = Context.RawMaterialMaster.Where(x => x.BuyerId == BuyerId).Count();
            return Records;
        }

        private bool UpdateCurrentStock(RawMaterailResponse ObjRawMaterailResponse)
        {
            bool Result = false;
            List<CurrentStockHistory> AddCurrentStockHistory = new List<CurrentStockHistory>();
            List<CurrentStock> UpdateCurrentStock = new List<CurrentStock>();
            List<CurrentStockHistory> AddCurrentStockHistory1 = new List<CurrentStockHistory>();
            List<CurrentStock> UpdateCurrentStock1 = new List<CurrentStock>();
            string DisplayName = Context.Peoples.Where(x => x.PeopleID == ObjRawMaterailResponse.RawMaterialMaster.CreatedBy).FirstOrDefault().DisplayName.ToString();


            foreach (var data in ObjRawMaterailResponse.RawMaterialDetails)
            {
                CurrentStock currentStockItem = Context.DbCurrentStock.Where(x => x.ItemNumber == data.ItemNumber && x.WarehouseId == ObjRawMaterailResponse.RawMaterialMaster.WarehouseId).FirstOrDefault();
                if (currentStockItem != null)
                {
                    CurrentStockHistory Oss = new CurrentStockHistory();
                    Oss.StockId = currentStockItem.StockId;
                    Oss.ItemNumber = currentStockItem.ItemNumber;
                    Oss.itemname = currentStockItem.itemname;
                    Oss.ItemMultiMRPId = currentStockItem.ItemMultiMRPId;
                    Oss.OdOrPoId = ObjRawMaterailResponse.RawMaterialMaster.IvchNo;
                    Oss.CurrentInventory = currentStockItem.CurrentInventory;
                    int NoOfPieces = Conversion(data.ItemNumber, data.Qty, data.Uom);

                    Oss.TotalInventory = Convert.ToInt32(currentStockItem.CurrentInventory - NoOfPieces);
                    Oss.ManualReason = "Raw Material Out";

                    Oss.UOM = "Pc";
                    Oss.InventoryOut = NoOfPieces;
                    Oss.WarehouseName = currentStockItem.WarehouseName;

                    Oss.Warehouseid = currentStockItem.WarehouseId;
                    Oss.CompanyId = currentStockItem.CompanyId;
                    Oss.userid = ObjRawMaterailResponse.RawMaterialMaster.CreatedBy;
                    Oss.UserName = DisplayName;

                    Oss.CreationDate = ObjRawMaterailResponse.RawMaterialMaster.CreatedDate;
                    AddCurrentStockHistory.Add(Oss);
                    currentStockItem.CurrentInventory = Convert.ToInt32(currentStockItem.CurrentInventory - NoOfPieces);
                    currentStockItem.UpdatedDate = ObjRawMaterailResponse.RawMaterialMaster.CreatedDate;
                    UpdateCurrentStock.Add(currentStockItem);

                    if (ObjRawMaterailResponse.RawMaterialMaster.ShipToType == 2)
                    {
                        CurrentStock currentStockItem1 = Context.DbCurrentStock.Where(x => x.ItemNumber == data.ItemNumber && x.WarehouseId == ObjRawMaterailResponse.RawMaterialMaster.ShipToSupplierWarehouseId).FirstOrDefault();
                        CurrentStockHistory Oss1 = new CurrentStockHistory();
                        Oss1.StockId = currentStockItem1.StockId;
                        Oss1.ItemNumber = currentStockItem1.ItemNumber;
                        Oss1.itemname = currentStockItem1.itemname;
                        Oss1.ItemMultiMRPId = currentStockItem1.ItemMultiMRPId;
                        Oss1.OdOrPoId = ObjRawMaterailResponse.RawMaterialMaster.IvchNo;
                        Oss1.CurrentInventory = currentStockItem1.CurrentInventory;
                        int NoOfPieces1 = data.Qty; //Conversion(data.ItemNumber, data.Qty, data.Uom);

                        Oss1.TotalInventory = Convert.ToInt32(currentStockItem1.CurrentInventory + NoOfPieces1);
                        Oss1.ManualReason = "Raw Material In";

                        Oss1.UOM = "Pc";
                        Oss1.InventoryIn = NoOfPieces1;
                        Oss1.WarehouseName = currentStockItem1.WarehouseName;

                        Oss1.Warehouseid = currentStockItem1.WarehouseId;
                        Oss1.CompanyId = currentStockItem1.CompanyId;
                        Oss1.userid = ObjRawMaterailResponse.RawMaterialMaster.CreatedBy;
                        Oss1.UserName = DisplayName;

                        Oss1.CreationDate = ObjRawMaterailResponse.RawMaterialMaster.CreatedDate;
                        AddCurrentStockHistory1.Add(Oss1);
                        currentStockItem1.CurrentInventory = Convert.ToInt32(currentStockItem1.CurrentInventory + NoOfPieces1);
                        currentStockItem1.UpdatedDate = ObjRawMaterailResponse.RawMaterialMaster.CreatedDate;
                        UpdateCurrentStock1.Add(currentStockItem1);
                    }
                }

            }

            if (AddCurrentStockHistory != null && AddCurrentStockHistory.Any())
            {

                Context.CurrentStockHistoryDb.AddRange(AddCurrentStockHistory);
                Result = true;
            }
            if (UpdateCurrentStock != null && UpdateCurrentStock.Any())
            {
                foreach (var item in UpdateCurrentStock)
                {
                    item.UpdatedDate = ObjRawMaterailResponse.RawMaterialMaster.CreatedDate;
                    //db.Entry(item).State = EntityState.Deleted;
                    Context.Entry(item).State = EntityState.Modified;
                }
                Result = true;
            }
            if (AddCurrentStockHistory1 != null && AddCurrentStockHistory1.Any())
            {

                Context.CurrentStockHistoryDb.AddRange(AddCurrentStockHistory1);
                Result = true;
            }

            return Result;
        }

        public bool UpdateItemReceivedmaterialDetails(int Quantity, string Uom, int PurchaseOrderId, string ItemNumber, int UpdatedBy)
        {
            bool FinalResult = false;
            try
            {


                var PurchaseOrderIdParam = new SqlParameter
                {
                    ParameterName = "PurchaseOrderId",
                    Value = PurchaseOrderId
                };

                var ItemNumberParam = new SqlParameter
                {
                    ParameterName = "ItemNumber",
                    Value = ItemNumber
                };
                var UomParam = new SqlParameter
                {
                    ParameterName = "Uom",
                    Value = Uom
                };

                var QuantityParam = new SqlParameter
                {
                    ParameterName = "Quantity",
                    Value = Quantity
                };

                var UpdatedByParam = new SqlParameter
                {
                    ParameterName = "UpdatedBy",
                    Value = UpdatedBy
                };



                int Result = Context.Database.ExecuteSqlCommand("UpdateItemReceivedmaterialDetails @PurchaseOrderId,@ItemNumber,@Uom,@Quantity,@UpdatedBy ", @PurchaseOrderIdParam, ItemNumberParam, UomParam, QuantityParam, UpdatedByParam);

                if (Result > 1)
                {
                    FinalResult = true;
                }


                return FinalResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private int Conversion(string ItemNumber, int Quantity, string Uom)
        {
            if (!Uom.ToUpper().Equals("PC"))
            {
                int Number = Context.MaterialItemMaster.Where(x => x.ItemNumber == ItemNumber).FirstOrDefault().ToValue;
                int NoOfPieces = Quantity * Number;
                return NoOfPieces;
            }
            return Quantity;
        }

        //public IEnumerable<GetRawMaterialMstDetails> PackingMaterialReport(int CreatedBy, int SKIP, int TAKE, int WareHouseId)
        //{
        //    try
        //    {

        //        var CreatedByParam = new SqlParameter
        //        {
        //            ParameterName = "CreatedBy",
        //            Value = CreatedBy
        //        };
        //        var SKIPParam = new SqlParameter
        //        {
        //            ParameterName = "SKIP",
        //            Value = SKIP
        //        };


        //        var TAKEParam = new SqlParameter
        //        {
        //            ParameterName = "TAKE",
        //            Value = TAKE
        //        };

        //        var WareHouseIdParam = new SqlParameter
        //        {
        //            ParameterName = "WareHouseId",
        //            Value = WareHouseId
        //        };



        //        IEnumerable<GetRawMaterialMstDetails> objGetRawMaterialMstDetails = Context.Database.SqlQuery<GetRawMaterialMstDetails>("FinalPackingMaterialReport @CreatedBy, @SKIP,@TAKE,@WareHouseId ", CreatedByParam, SKIPParam, TAKEParam, WareHouseIdParam).ToList();
        //        return objGetRawMaterialMstDetails;
        //    }



        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

       public List<GetRawMaterialMstDetails> PackingMaterialReport(int CreatedBy, int SKIP, int TAKE, int WareHouseId,int? SupplierId)
        {
            try
            {

                var CreatedByParam = new SqlParameter
                {
                    ParameterName = "CreatedBy",
                    Value = CreatedBy
                };
                var SKIPParam = new SqlParameter
                {
                    ParameterName = "SKIP",
                    Value = SKIP
                };


                var TAKEParam = new SqlParameter
                {
                    ParameterName = "TAKE",
                    Value = TAKE
                };

                var WareHouseIdParam = new SqlParameter
                {
                    ParameterName = "WareHouseId",
                    Value = WareHouseId
                };
                var SupplierIdParam = new SqlParameter
                {
                    ParameterName = "SupplierId",
                    Value = SupplierId
                };



                List<GetRawMaterialMstDetails> objGetRawMaterialMstDetails = Context.Database.SqlQuery<GetRawMaterialMstDetails>("FinalPackingMaterialReport @CreatedBy, @SKIP,@TAKE,@WareHouseId,@SupplierId ", CreatedByParam, SKIPParam, TAKEParam, WareHouseIdParam, SupplierIdParam).ToList();
                return objGetRawMaterialMstDetails;
            }



            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}