using AngularJSAuthentication.BusinessLayer.PackingMaterial.BO;
using AngularJSAuthentication.BusinessLayer.PackingMaterial.IF;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.External;
using AngularJSAuthentication.Model;
using Nito.AspNetBackgroundTasks;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;

namespace AngularJSAuthentication.API.App_Code.PackingMaterial
{
    public class ItemMaterialRepository : IDisposable, IItemMaterial
    {
        private bool disposed = false;
        private AuthContext Context;
        private static DateTime CurrentDatetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
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

        public ItemMaterialRepository(AuthContext Context)
        {
            this.Context = Context;
        }
        public ItemMaterialResponse ItemMaterialResponse()
        {
            try
            {
                ItemMaterialResponse objItemMaterialResponse = new ItemMaterialResponse();
                objItemMaterialResponse.PackingBagMaster = GetBagDetails();
                //objItemMaterialResponse.Supplier = GetSupplierDetails();
                //objItemMaterialResponse.City = GetCityDetails();
                return objItemMaterialResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<PackingBagMaster> GetBagDetails()
        {
            try
            {


                List<PackingBagMaster> objPackingBagMaster = Context.Database.SqlQuery<PackingBagMaster>("GetPackingBagDetails").ToList();
                return objPackingBagMaster;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public List<Supplier> GetSupplierDetails()
        {
            try
            {

                List<Supplier> objListSupplier = Context.Suppliers.ToList();

                return objListSupplier;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public List<AngularJSAuthentication.Model.City> GetCityDetails()
        {
            try
            {

                List<AngularJSAuthentication.Model.City> objListSupplier = Context.Cities.ToList();

                return objListSupplier;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<Warehouse> Warehouse(int CityId)
        {
            try
            {
                IEnumerable<AngularJSAuthentication.Model.Warehouse> objWarehouses = Context.Warehouses.Where(x => x.Cityid == CityId);
                return objWarehouses;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<Buyer> Buyer()
        {

            try
            {
                IEnumerable<Buyer> objBuyer = Context.Database.SqlQuery<Buyer>("GetBuyer");
                return objBuyer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool InsertMaterialItemMaster(MaterialItemMaster ObjMaterialItemMaster)
        {
            try
            {
                bool Res = false;
                int MaterialItemId = 0;

                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope Scope = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    var BagId = new SqlParameter
                    {
                        ParameterName = "BagId",
                        Value = ObjMaterialItemMaster.BagId
                    };



                    var UnitOfMeasurement = new SqlParameter
                    {
                        ParameterName = "UnitOfMeasurement",
                        Value = ObjMaterialItemMaster.UnitOfMeasurement
                    };
                    var FromConversion = new SqlParameter
                    {
                        ParameterName = "FromConversion",
                        Value = ObjMaterialItemMaster.FromConversion
                    };
                    var ToConversion = new SqlParameter
                    {
                        ParameterName = "ToConversion",
                        Value = ObjMaterialItemMaster.ToConversion
                    };
                    var FromValue = new SqlParameter
                    {
                        ParameterName = "FromValue",
                        Value = ObjMaterialItemMaster.FromValue
                    };
                    var ToValue = new SqlParameter
                    {
                        ParameterName = "ToValue",
                        Value = ObjMaterialItemMaster.ToValue
                    };
                    var CreateBy = new SqlParameter
                    {
                        ParameterName = "CreateBy",
                        Value = ObjMaterialItemMaster.CreatedBy
                    };
                    var Brand = new SqlParameter
                    {
                        ParameterName = "Brand",
                        Value = ObjMaterialItemMaster.Brand
                    };
                    var POUom1 = new SqlParameter
                    {
                        ParameterName = "POUom1",
                        Value = ObjMaterialItemMaster.POUom1 == null ? DBNull.Value : (object)ObjMaterialItemMaster.POUom1
                    };
                    var POUom2 = new SqlParameter
                    {
                        ParameterName = "POUom2",
                        Value = ObjMaterialItemMaster.POUom2 == null ? DBNull.Value : (object)ObjMaterialItemMaster.POUom2
                    };


                    var GRIRUom1 = new SqlParameter
                    {
                        ParameterName = "GRIRUom1",
                        Value = ObjMaterialItemMaster.GRIRUom1 == null ? DBNull.Value : (object)ObjMaterialItemMaster.GRIRUom1
                    };
                    var GRIRUom2 = new SqlParameter
                    {
                        ParameterName = "GRIRUom2",
                        Value = ObjMaterialItemMaster.GRIRUom2 == null ? DBNull.Value : (object)ObjMaterialItemMaster.GRIRUom2
                    };
                    var IssueDispatchUom1 = new SqlParameter
                    {
                        ParameterName = "IssueDispatchUom1",
                        Value = ObjMaterialItemMaster.IssueDispatchUom1 == null ? DBNull.Value : (object)ObjMaterialItemMaster.IssueDispatchUom1
                    };
                    var IssueDispatchUom2 = new SqlParameter
                    {
                        ParameterName = "IssueDispatchUom2",
                        Value = ObjMaterialItemMaster.IssueDispatchUom2 == null ? DBNull.Value : (object)ObjMaterialItemMaster.IssueDispatchUom2
                    };
                    var Inventory1 = new SqlParameter
                    {
                        ParameterName = "Inventory1",
                        Value = ObjMaterialItemMaster.Inventory1 == null ? DBNull.Value : (object)ObjMaterialItemMaster.Inventory1
                    };
                    var Inventory2 = new SqlParameter
                    {
                        ParameterName = "Inventory2",
                        Value = ObjMaterialItemMaster.Inventory2 == null ? DBNull.Value : (object)ObjMaterialItemMaster.Inventory2
                    };
                    var ItemNumber = new SqlParameter
                    {
                        ParameterName = "ItemNumber",
                        Value = ObjMaterialItemMaster.ItemNumber
                    };

                    var MaterialItemIdOP = new SqlParameter
                    {
                        ParameterName = "MaterialItemIdOP",
                        DbType = DbType.Int32,
                        Value = 0,
                        Direction = ParameterDirection.Output
                    };


                    int Result = Context.Database.ExecuteSqlCommand("InsertMaterialItemMaster @BagId ,@UnitOfMeasurement ,@FromConversion ,@ToConversion ,@FromValue ,@ToValue ,@CreateBy ,@Brand ,@POUom1 ,@POUom2,@GRIRUom1 ,@GRIRUom2,@IssueDispatchUom1,@IssueDispatchUom2 ,@Inventory1,@Inventory2,@ItemNumber, @MaterialItemIdOP OUT ", BagId,
                          UnitOfMeasurement, FromConversion, ToConversion, FromValue, ToValue, CreateBy, Brand, POUom1, POUom2, GRIRUom1, GRIRUom2, IssueDispatchUom1, IssueDispatchUom2, Inventory1, Inventory2, ItemNumber, MaterialItemIdOP);
                    MaterialItemId = (int)MaterialItemIdOP.Value;

                    bool FinalResult = AddItemMaster(ObjMaterialItemMaster.ItemMasterCentral, MaterialItemId);

                    if (Result > 0 && FinalResult)
                    {

                        Res = true;
                        Context.SaveChanges();
                        Scope.Complete();

                    }

                    else
                    {
                        throw new Exception("Something went wrong please try again later!!");
                    }
                    return Res;
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
        }

        public int InsertMaterialItemDetails(List<MaterialItemDetails> ObjMaterialItemDetails, string ItemNumber)
        {
            try
            {
                int Result = 0;
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (TransactionScope Scope = new TransactionScope(TransactionScopeOption.Required, option))
               // using (TransactionScope Scope = new TransactionScope())
                {
                    Context.MaterialItemDetails.AddRange(ObjMaterialItemDetails);
                  //  bool UpdtBom = UpdateBomId(ItemNumber, ObjMaterialItemDetails[0].BomId, ObjMaterialItemDetails[0].CreatedBy);
                    if (ObjMaterialItemDetails != null)
                    {
                        Context.SaveChanges();
                        Scope.Complete();
                        Result = ObjMaterialItemDetails[0].BomId;

                    }
                }
                return Result;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //public IEnumerable<MaterialItemDetails> GetMaterialItemDetails(int MaterialItemId)
        //{
        //    IEnumerable<MaterialItemDetails> objMaterialItemDetails = Context.MaterialItemDetails.Where(x => x.OuterBagId == MaterialItemId).ToList();
        //    return objMaterialItemDetails;
        //}
        public ItemMaterialResponse ItemMaterialResponseDetails()
        {
            try
            {
                ItemMaterialResponse objItemMaterialResponse = new ItemMaterialResponse();
                objItemMaterialResponse.PackingBagMaster = GetBagDetails();
                objItemMaterialResponse.Supplier = GetSupplierDetails();
                objItemMaterialResponse.City = GetCityDetails();
                return objItemMaterialResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<OuterBagMaster> OuterBagDetails()
        {

            try
            {
                IEnumerable<OuterBagMaster> objOuterBagMaster = Context.OuterBagMaster.ToList();
                return objOuterBagMaster;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<BagDescriptionDTO> BagDetails(int OuterBagid)
        {

            try
            {
                IEnumerable<BagDescriptionDTO> objBagDescription = (from O in Context.OuterBagMaster
                                                                    join B in Context.BagDescription
                                                                      on O.OuterBagId equals B.OuterBagId
                                                                    where B.OuterBagId == OuterBagid
                                                                    select new BagDescriptionDTO
                                                                    {
                                                                        BagDescId = B.BagDescId,
                                                                        BagName = B.BagName
                                                                    }).AsEnumerable();

                return objBagDescription;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private bool AddItemMaster(ItemMasterCentral itemmaster, int MaterialItemId)
        {
            bool FinalResult = false;

            try
            {
                People people = Context.Peoples.Where(x => x.PeopleID == itemmaster.userid).SingleOrDefault();
                List<ItemMasterCentral> itemMasterexits = Context.ItemMasterCentralDB.Where(c => c.Number.Trim().Equals(itemmaster.Number.Trim()) && c.CompanyId == itemmaster.CompanyId).ToList();
                ItemMasterCentral Check = Context.ItemMasterCentralDB.Where(c => c.SellingSku.Trim().Equals(itemmaster.SellingSku.Trim()) && c.CompanyId == itemmaster.CompanyId).FirstOrDefault();
                Category category = new Category();
                SubCategory subcategory = new SubCategory();
                SubsubCategory Subsubcategory = new SubsubCategory();
                TaxGroupDetails taxgroup = new TaxGroupDetails();
                TaxGroup Tg = new TaxGroup();
                double TotalTax = 0;
                double TotalCessTax = 0;
                // this tax group
                TaxGroupDetails Cessgroup = new TaxGroupDetails();
                TaxGroup CessTg = new TaxGroup();

                if (itemMasterexits.Count > 0)
                {


                    ItemMasterCentral Createditem = itemMasterexits[0];
                    TotalTax = Createditem.TotalTaxPercentage;//
                    TotalCessTax = Createditem.TotalCessPercentage;//
                    taxgroup.GruopID = Createditem.GruopID;//
                    Tg.TGrpName = Createditem.TGrpName;
                    Cessgroup.GruopID = Createditem.CessGrpID ?? 0;
                    CessTg.TGrpName = Createditem.CessGrpName;
                    category = Context.Categorys.Where(x => x.Categoryid == Createditem.Categoryid && x.Deleted == false).Select(x => x).FirstOrDefault();
                    subcategory = Context.SubCategorys.Where(x => x.SubCategoryId == Createditem.SubCategoryId && x.Deleted == false).Select(x => x).FirstOrDefault();
                    Subsubcategory = Context.SubsubCategorys.Where(x => x.SubsubCategoryid == Createditem.SubsubCategoryid && x.Deleted == false).Select(x => x).FirstOrDefault();
                }
                else
                {
                    category = Context.Categorys.Where(x => x.Categoryid == itemmaster.Categoryid && x.Deleted == false).Select(x => x).FirstOrDefault();
                    subcategory = Context.SubCategorys.Where(x => x.SubCategoryId == itemmaster.SubCategoryId && x.Deleted == false).Select(x => x).FirstOrDefault();
                    Subsubcategory = Context.SubsubCategorys.Where(x => x.SubsubCategoryid == itemmaster.SubsubCategoryid && x.Deleted == false).Select(x => x).FirstOrDefault();

                    taxgroup = Context.DbTaxGroupDetails.Where(x => x.GruopID == itemmaster.GruopID && x.CompanyId == itemmaster.CompanyId).Select(x => x).FirstOrDefault();
                    if (taxgroup != null)
                    {
                        itemmaster.GruopID = taxgroup.GruopID;
                    }
                    Tg = Context.DbTaxGroup.Where(x => x.GruopID == itemmaster.GruopID && x.Deleted == false && x.CompanyId == itemmaster.CompanyId).Select(x => x).FirstOrDefault();
                    if (Tg != null) { itemmaster.TGrpName = Tg.TGrpName; }
                    List<TaxGroupDetails> TaxG = Context.DbTaxGroupDetails.Where(x => x.GruopID == itemmaster.GruopID && x.CompanyId == itemmaster.CompanyId).Select(x => x).ToList();
                    if (TaxG.Count != 0)
                    {
                        foreach (var i in TaxG)
                        {
                            TotalTax += i.TPercent;
                        }
                    }



                    if (itemmaster.CessGrpID > 0)
                    {
                        Cessgroup = Context.DbTaxGroupDetails.Where(x => x.GruopID == itemmaster.CessGrpID && x.CompanyId == itemmaster.CompanyId).Select(x => x).FirstOrDefault();
                        CessTg = Context.DbTaxGroup.Where(x => x.GruopID == itemmaster.CessGrpID && x.Deleted == false && x.CompanyId == itemmaster.CompanyId).Select(x => x).FirstOrDefault();
                        List<TaxGroupDetails> CesstaxG = Context.DbTaxGroupDetails.Where(x => x.GruopID == itemmaster.CessGrpID && x.CompanyId == itemmaster.CompanyId).Select(x => x).ToList();
                        if (CesstaxG.Count != 0)
                        {
                            foreach (var i in CesstaxG)
                            {
                                TotalCessTax += i.TPercent;
                            }
                        }
                    }



                }
                if (Check == null)
                {
                   // ItemMultiMRP MultiMRPitem = new ItemMultiMRP();
                    ItemMultiMRP MultiMRPitem = AddItemMultiMRP(itemmaster);
                    bool Resitemmaster = AddItemCentralMaster(itemmaster, Cessgroup, CessTg, TotalTax,
                     TotalCessTax, category, subcategory, Subsubcategory, MultiMRPitem);
                    bool Res = AddIteminWarehouse(taxgroup, Subsubcategory, itemmaster, TotalTax, TotalCessTax, category, Tg, MultiMRPitem, people, MaterialItemId);
                    if (Res  && Resitemmaster)
                    {
                        FinalResult = true;
                    }
                    TradeItemMaster(itemmaster, Subsubcategory, people);
                    ///first record in Central for MRP
                    //ItemMultiMRP MultiMRPitem = new ItemMultiMRP();
                    //ItemMultiMRP recordExits = Context.ItemMultiMRPDB.Where(x => x.ItemNumber == itemmaster.Number && x.Deleted == false && x.MRP == itemmaster.price).FirstOrDefault();
                    //if (recordExits != null)
                    //{
                    //    MultiMRPitem = recordExits;
                    //}
                    //else
                    //{
                    //    MultiMRPitem = Context.ItemMultiMRPDB.Where(x => x.ItemNumber == itemmaster.Number && x.ItemMultiMRPId == itemmaster.ItemMultiMRPId).SingleOrDefault();
                    //    if (MultiMRPitem == null)
                    //    {
                    //        ItemMultiMRP Immrp = new ItemMultiMRP();
                    //        Immrp.CompanyId = itemmaster.CompanyId;

                    //        Immrp.ItemNumber = itemmaster.Number;
                    //        Immrp.MRP = itemmaster.price;

                    //        Immrp.UnitofQuantity = itemmaster.UnitofQuantity;
                    //        Immrp.UOM = itemmaster.UOM;
                    //        Immrp.CreatedDate = indianTime;
                    //        Immrp.UpdatedDate = indianTime;
                    //        //ItemMultiMRPDB.Add(Immrp);
                    //        //Commit();
                    //        MultiMRPitem = Immrp;
                    //    }
                    //}

                    //itemmaster.MRP = itemmaster.price;

                    //itemmaster.TotalTaxPercentage = TotalTax;
                    ////cesss

                    //itemmaster.CessGrpID = Cessgroup.GruopID;
                    //itemmaster.CessGrpName = CessTg.TGrpName;
                    //itemmaster.TotalCessPercentage = TotalCessTax;


                    //itemmaster.BaseCategoryid = category.BaseCategoryId;
                    //itemmaster.LogoUrl = itemmaster.LogoUrl;
                    //itemmaster.UpdatedDate = CurrentDatetime;
                    //itemmaster.CreatedDate = CurrentDatetime;
                    //itemmaster.CategoryName = category.CategoryName;
                    //itemmaster.Categoryid = category.Categoryid;
                    //itemmaster.SubcategoryName = subcategory.SubcategoryName;
                    //itemmaster.SubCategoryId = subcategory.SubCategoryId;
                    //itemmaster.SubsubcategoryName = Subsubcategory.SubsubcategoryName;
                    //itemmaster.SubsubCategoryid = Subsubcategory.SubsubCategoryid;
                    //itemmaster.SubSubCode = Subsubcategory.Code;

                    //if (itemmaster.Margin > 0)
                    //{
                    //    var rs = Context.RetailerShareDb/*.Where(r => r.cityid == itemmaster.Cityid)*/.FirstOrDefault();
                    //    if (rs != null)
                    //    {
                    //        var cf = Context.RPConversionDb.FirstOrDefault();

                    //        double mv = (itemmaster.PurchasePrice * (itemmaster.Margin / 100) * (rs.share / 100) * cf.point);
                    //        var value = Math.Round(mv, MidpointRounding.AwayFromZero);

                    //    }
                    //}

                    ////Display Name binding
                    //itemmaster.itemBaseName = itemmaster.itemBaseName;
                    //itemmaster.itemname = itemmaster.itemname;
                    //itemmaster.ItemMultiMRPId = MultiMRPitem.ItemMultiMRPId;

                    ////For Type
                    //itemmaster.Type = itemmaster.Type;

                    //this.Commit();
                    //List<Warehouse> warehouses = Context.Warehouses.Where(x => x.Deleted == false && x.CompanyId == itemmaster.CompanyId).ToList();

                    //foreach (var o in warehouses)
                    //{

                    //    ItemMaster it = new ItemMaster();
                    //    if (taxgroup != null) { it.GruopID = taxgroup.GruopID; it.TGrpName = Tg.TGrpName; it.TotalTaxPercentage = TotalTax; }
                    //    if (Cessgroup != null) { it.CessGrpID = Cessgroup.GruopID; it.CessGrpName = CessTg.TGrpName; it.TotalCessPercentage = TotalCessTax; }
                    //    it.CatLogoUrl = Subsubcategory.LogoUrl;
                    //    it.WarehouseId = o.WarehouseId;
                    //    it.WarehouseName = o.WarehouseName;
                    //    it.BaseCategoryid = category.BaseCategoryId;
                    //    it.LogoUrl = itemmaster.LogoUrl;
                    //    it.UpdatedDate = indianTime;
                    //    it.CreatedDate = indianTime;
                    //    it.CategoryName = itemmaster.CategoryName;
                    //    it.Categoryid = itemmaster.Categoryid;
                    //    it.SubcategoryName = itemmaster.SubcategoryName;
                    //    it.SubCategoryId = itemmaster.SubCategoryId;
                    //    it.SubsubcategoryName = itemmaster.SubsubcategoryName;
                    //    it.SubsubCategoryid = itemmaster.SubsubCategoryid;
                    //    it.SubSubCode = Subsubcategory.Code;
                    //    it.itemcode = itemmaster.itemcode;
                    //    it.marginPoint = itemmaster.marginPoint;
                    //    it.Number = itemmaster.Number;
                    //    it.PramotionalDiscount = itemmaster.PramotionalDiscount;
                    //    it.MinOrderQty = itemmaster.MinOrderQty;
                    //    it.NetPurchasePrice = itemmaster.NetPurchasePrice;
                    //    it.GeneralPrice = itemmaster.GeneralPrice;
                    //    it.price = itemmaster.price;
                    //    it.promoPerItems = itemmaster.promoPerItems;
                    //    it.promoPoint = itemmaster.promoPoint;
                    //    it.PurchaseMinOrderQty = itemmaster.PurchaseMinOrderQty;
                    //    it.PurchasePrice = itemmaster.PurchasePrice;
                    //    it.PurchaseSku = itemmaster.PurchaseSku;
                    //    it.PurchaseUnitName = itemmaster.PurchaseUnitName;
                    //    it.SellingSku = itemmaster.SellingSku;
                    //    it.SellingUnitName = itemmaster.SellingUnitName;
                    //    it.SizePerUnit = itemmaster.SizePerUnit;
                    //    it.UnitPrice = itemmaster.UnitPrice;
                    //    it.VATTax = itemmaster.VATTax;
                    //    it.HSNCode = itemmaster.HSNCode;
                    //    it.HindiName = itemmaster.HindiName;
                    //    it.CompanyId = itemmaster.CompanyId;
                    //    it.Reason = itemmaster.Reason;
                    //    it.DefaultBaseMargin = itemmaster.DefaultBaseMargin;
                    //    it.Deleted = false;
                    //    it.active = false;
                    //    it.itemname = itemmaster.itemname;
                    //    it.itemBaseName = itemmaster.itemBaseName;
                    //    it.UOM = itemmaster.UOM;
                    //    it.UnitofQuantity = itemmaster.UnitofQuantity;
                    //    it.ItemMultiMRPId = MultiMRPitem.ItemMultiMRPId;
                    //    it.IsSensitive = itemmaster.IsSensitive;
                    //    it.IsSensitiveMRP = itemmaster.IsSensitiveMRP;
                    //    it.ShelfLife = itemmaster.ShelfLife;
                    //    it.IsReplaceable = itemmaster.IsReplaceable;

                    //    it.Type = itemmaster.Type;//Type
                    //    Context.itemMasters.Add(it);
                    //    this.Commit();
                    //    CurrentStock cntstock = new CurrentStock();
                    //    cntstock = DbCurrentStock.Where(x => x.ItemNumber == itemmaster.Number && x.WarehouseId == o.WarehouseId && x.CompanyId == itemmaster.CompanyId && x.ItemMultiMRPId == itemmaster.ItemMultiMRPId && x.Deleted == false).SingleOrDefault();
                    //    //if (itemmaster.IsSensitiveMRP)
                    //    //{
                    //    //    cntstock = DbCurrentStock.Where(x => x.ItemNumber == itemmaster.Number && x.WarehouseId == o.WarehouseId && x.CompanyId == itemmaster.CompanyId && x.Deleted == false).FirstOrDefault();
                    //    //}
                    //    //else
                    //    //{
                    //    //    cntstock = DbCurrentStock.Where(x => x.ItemNumber == itemmaster.Number && x.WarehouseId == o.WarehouseId && x.CompanyId == itemmaster.CompanyId && x.ItemMultiMRPId == itemmaster.ItemMultiMRPId && x.Deleted == false).SingleOrDefault();
                    //    //}
                    //    if (cntstock == null)
                    //    {
                    //        CurrentStock newCstk = new CurrentStock();
                    //        newCstk.ItemNumber = itemmaster.Number;
                    //        newCstk.WarehouseId = o.WarehouseId;
                    //        newCstk.WarehouseName = o.WarehouseName;
                    //        newCstk.CompanyId = itemmaster.CompanyId;
                    //        newCstk.CurrentInventory = 0;
                    //        newCstk.CreationDate = indianTime;
                    //        newCstk.UpdatedDate = indianTime;
                    //        // Multimrp
                    //        newCstk.MRP = itemmaster.price;
                    //        newCstk.UnitofQuantity = itemmaster.UnitofQuantity;
                    //        newCstk.UOM = itemmaster.UOM;
                    //        newCstk.ItemMultiMRPId = MultiMRPitem.ItemMultiMRPId;
                    //        newCstk.itemname = itemmaster.itemname;
                    //        newCstk.itemBaseName = itemmaster.itemBaseName;
                    //        Context.DbCurrentStock.Add(newCstk);
                    //        this.Commit();

                    //        CurrentStockHistory Oss = new CurrentStockHistory();
                    //        Oss.StockId = newCstk.StockId;
                    //        Oss.ItemNumber = newCstk.ItemNumber;

                    //        Oss.TotalInventory = newCstk.CurrentInventory;
                    //        Oss.WarehouseName = newCstk.WarehouseName;
                    //        Oss.Warehouseid = newCstk.WarehouseId;
                    //        Oss.CompanyId = newCstk.CompanyId;
                    //        Oss.CreationDate = indianTime;
                    //        // Multimrp
                    //        Oss.MRP = newCstk.MRP;

                    //        Oss.UnitofQuantity = newCstk.UnitofQuantity;
                    //        Oss.UOM = newCstk.UOM;
                    //        Oss.ItemMultiMRPId = newCstk.ItemMultiMRPId;
                    //        Oss.itemname = newCstk.itemname;
                    //        Oss.itemBaseName = newCstk.itemBaseName;
                    //        Oss.userid = people.PeopleID;
                    //        Oss.UserName = people.DisplayName;
                    //        Context.CurrentStockHistoryDb.Add(Oss);
                    //        int idd = this.Commit();
                    //    }
                    //    else
                    //    {
                    //        cntstock.ItemNumber = itemmaster.Number;
                    //        cntstock.WarehouseId = o.WarehouseId;
                    //        cntstock.WarehouseName = o.WarehouseName;
                    //        cntstock.CompanyId = itemmaster.CompanyId;
                    //        cntstock.Deleted = false;
                    //        cntstock.UpdatedDate = indianTime;
                    //        // Multimrp
                    //        cntstock.MRP = itemmaster.price;
                    //        cntstock.UnitofQuantity = itemmaster.UnitofQuantity;
                    //        cntstock.UOM = itemmaster.UOM;
                    //        cntstock.ItemMultiMRPId = MultiMRPitem.ItemMultiMRPId;
                    //        cntstock.itemname = itemmaster.itemname;
                    //        cntstock.itemBaseName = itemmaster.itemBaseName;

                    //        //DbCurrentStock.Attach(cntstock);
                    //        this.Entry(cntstock).State = EntityState.Modified;
                    //        this.Commit();

                    //        CurrentStockHistory Oss = new CurrentStockHistory();
                    //        Oss.StockId = cntstock.StockId;
                    //        Oss.ItemNumber = cntstock.ItemNumber;
                    //        Oss.TotalInventory = cntstock.CurrentInventory;
                    //        Oss.WarehouseName = cntstock.WarehouseName;
                    //        Oss.Warehouseid = cntstock.WarehouseId;
                    //        Oss.CompanyId = cntstock.CompanyId;
                    //        Oss.CreationDate = indianTime;
                    //        Oss.userid = people.PeopleID;
                    //        Oss.UserName = people.DisplayName;
                    //        // Multimrp
                    //        Oss.MRP = cntstock.MRP;
                    //        Oss.UnitofQuantity = cntstock.UnitofQuantity;
                    //        Oss.UOM = cntstock.UOM;
                    //        Oss.ItemMultiMRPId = cntstock.ItemMultiMRPId;
                    //        Oss.itemname = cntstock.itemname;
                    //        Oss.itemBaseName = cntstock.itemBaseName;
                    //        Context.CurrentStockHistoryDb.Add(Oss);
                    //        int idd = this.Commit();
                    //    }

                    //    ///***** set all items current stock behalf of all warehouses *****///
                    //}


                    // CommonHelper.refreshItemMaster(itemMasters.WarehouseId);

                    //if (itemmaster.IsTradeItem)
                    //{
                    //    TradeItemMasterDc tradeItem = new TradeItemMasterDc
                    //    {
                    //        BaseCategoryId = itemmaster.BaseCategoryid,
                    //        CategoryId = itemmaster.Categoryid,
                    //        SubCategoryId = itemmaster.SubCategoryId,
                    //        BrandId = itemmaster.SubsubCategoryid,
                    //        BaseCategoryName = itemmaster.BaseCategoryName,
                    //        BasePrice = itemmaster.UnitPrice,
                    //        BrandImagePath = Subsubcategory.LogoUrl,
                    //        BrandName = itemmaster.SubsubcategoryName,
                    //        CategoryName = itemmaster.CategoryName,
                    //        CreatedBy = people.PeopleID,
                    //        CreatedDate = indianTime,
                    //        ImagePath = itemmaster.LogoUrl,
                    //        IsActive = false,
                    //        IsDelete = false,
                    //        ItemId = itemmaster.Id,
                    //        ItemName = itemmaster.itemname,
                    //        MRP = itemmaster.price,
                    //        UnitOfMeasurement = itemmaster.UOM,
                    //        SubCategoryName = itemmaster.SubcategoryName,
                    //        UnitOfQuantity = itemmaster.UnitofQuantity,
                    //        HSNCode = itemmaster.HSNCode,
                    //        TotalTaxPercent = Convert.ToString(itemmaster.TotalTaxPercentage),
                    //        CGST = Convert.ToString(itemmaster.TotalTaxPercentage / 2),
                    //        SGST = Convert.ToString(itemmaster.TotalTaxPercentage / 2),
                    //        CESS = Convert.ToString(itemmaster.TotalCessPercentage)
                    //    };

                    //    BackgroundTaskManager.Run(() =>
                    //    {
                    //        try
                    //        {
                    //            var tradeUrl = ConfigurationManager.AppSettings["TradeURL"] + "/api/TradeItem/Insert";
                    //            TextFileLogHelper.LogError(tradeUrl, false);
                    //            using (GenericRestHttpClient<TradeItemMasterDc, string> memberClient = new GenericRestHttpClient<TradeItemMasterDc, string>(tradeUrl, "", null))
                    //            {
                    //                tradeItem = AsyncContext.Run(() => memberClient.PostAsync(tradeItem));
                    //            }
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            TextFileLogHelper.LogError("Error while saving item in Trade: " + ex.ToString());
                    //        }
                    //    });
                    //}

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return FinalResult;

        }

        private ItemMultiMRP AddItemMultiMRP( ItemMasterCentral itemmaster)
        {
            //bool Result = false;
            try
            {
                ItemMultiMRP MultiMRPitem = new ItemMultiMRP();

                ItemMultiMRP recordExits = Context.ItemMultiMRPDB.Where(x => x.ItemNumber == itemmaster.Number && x.Deleted == false && x.MRP == itemmaster.price).FirstOrDefault();
                if (recordExits != null)
                {
                    MultiMRPitem = recordExits;
                }
                else
                {
                    MultiMRPitem = Context.ItemMultiMRPDB.Where(x => x.ItemNumber == itemmaster.Number && x.ItemMultiMRPId == itemmaster.ItemMultiMRPId).SingleOrDefault();
                    if (MultiMRPitem == null && itemmaster != null)
                    {
                        ItemMultiMRP Immrp = new ItemMultiMRP();
                        Immrp.CompanyId = itemmaster.CompanyId;

                        Immrp.ItemNumber = itemmaster.Number;
                        Immrp.MRP = itemmaster.price;

                        Immrp.UnitofQuantity = itemmaster.UnitofQuantity;
                        Immrp.UOM = itemmaster.UOM;
                        Immrp.CreatedDate = CurrentDatetime;
                        Immrp.UpdatedDate = CurrentDatetime;
                        Context.ItemMultiMRPDB.Add(Immrp);
                        Context.Commit();
                        MultiMRPitem = Immrp;
                        //Result = true;


                    }
                }
                return MultiMRPitem;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private bool AddItemCentralMaster(ItemMasterCentral itemmaster, TaxGroupDetails Cessgroup, TaxGroup CessTg, double TotalTax,
              double TotalCessTax, Category category, SubCategory subcategory, SubsubCategory Subsubcategory, ItemMultiMRP MultiMRPitem)
        {
            bool Result = false;
            try
            {
                itemmaster.MRP = itemmaster.price;

                itemmaster.TotalTaxPercentage = TotalTax;
                //cesss

                itemmaster.CessGrpID = Cessgroup.GruopID;
                itemmaster.CessGrpName = CessTg.TGrpName;
                itemmaster.TotalCessPercentage = TotalCessTax;


                itemmaster.BaseCategoryid = category.BaseCategoryId;
                itemmaster.LogoUrl = itemmaster.LogoUrl;
                itemmaster.UpdatedDate = CurrentDatetime;
                itemmaster.CreatedDate = CurrentDatetime;
                itemmaster.CategoryName = category.CategoryName;
                itemmaster.Categoryid = category.Categoryid;
                itemmaster.SubcategoryName = subcategory.SubcategoryName;
                itemmaster.SubCategoryId = subcategory.SubCategoryId;
                itemmaster.SubsubcategoryName = Subsubcategory.SubsubcategoryName;
                itemmaster.SubsubCategoryid = Subsubcategory.SubsubCategoryid;
                itemmaster.SubSubCode = Subsubcategory.Code;
                //Add Season Config
                itemmaster.SeasonId = itemmaster.SeasonId;
                //
                if (itemmaster.Margin > 0)
                {
                    var rs = Context.RetailerShareDb/*.Where(r => r.cityid == itemmaster.Cityid)*/.FirstOrDefault();
                    if (rs != null)
                    {
                        var cf = Context.RPConversionDb.FirstOrDefault();

                        double mv = (itemmaster.PurchasePrice * (itemmaster.Margin / 100) * (rs.share / 100) * cf.point);
                        var value = Math.Round(mv, MidpointRounding.AwayFromZero);

                    }
                }

                //Display Name binding
                itemmaster.itemBaseName = itemmaster.itemBaseName;
                itemmaster.itemname = itemmaster.itemname;
                itemmaster.ItemMultiMRPId = MultiMRPitem.ItemMultiMRPId;

                Context.ItemMasterCentralDB.Add(itemmaster);
                if (itemmaster != null)
                {
                    Result = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Result;
        }

        private bool AddIteminWarehouse(TaxGroupDetails taxgroup, SubsubCategory Subsubcategory, ItemMasterCentral itemmaster, double TotalTax,
              double TotalCessTax, Category category, TaxGroup Tg, ItemMultiMRP MultiMRPitem, People people, int MaterialItemId)
        {
            bool Result = false;
            bool UpdatecurrentstockResult = true;
            try
            {
                List<Warehouse> warehouses = Context.Warehouses.Where(x => x.Deleted == false && x.CompanyId == itemmaster.CompanyId).ToList();
                //CurrentStock newCstk = new CurrentStock();
                // CurrentStock cntstock = new CurrentStock();
                // CurrentStockHistory Oss = new CurrentStockHistory();
                List<CurrentStockHistory> LOss = new List<CurrentStockHistory>();
                List<ItemMaster> Liit = new List<ItemMaster>();
                List<CurrentStock> Licntstock = new List<CurrentStock>();
                foreach (var o in warehouses)
                {
                    ItemMaster it = new ItemMaster();

                    if (taxgroup != null)
                    {
                        it.GruopID = taxgroup.GruopID;
                        it.TGrpName = Tg.TGrpName;
                        it.TotalTaxPercentage = TotalTax;
                        it.TotalCessPercentage = TotalCessTax;
                    }
                    //if (taxgroup != null)
                    //{
                    //    it.CessGrpID = taxgroup.GruopID;
                    //    it.CessGrpName = Tg.TGrpName;
                    //    it.TotalCessPercentage = TotalCessTax;
                    //}
                    it.CatLogoUrl = Subsubcategory.LogoUrl;
                    it.WarehouseId = o.WarehouseId;
                    it.WarehouseName = o.WarehouseName;
                    it.BaseCategoryid = category.BaseCategoryId;
                    it.LogoUrl = itemmaster.LogoUrl;
                    it.UpdatedDate = CurrentDatetime;
                    it.CreatedDate = CurrentDatetime;
                    it.CategoryName = itemmaster.CategoryName;
                    it.Categoryid = itemmaster.Categoryid;
                    it.SubcategoryName = itemmaster.SubcategoryName;
                    it.SubCategoryId = itemmaster.SubCategoryId;
                    it.SubsubcategoryName = itemmaster.SubsubcategoryName;
                    it.SubsubCategoryid = itemmaster.SubsubCategoryid;
                    it.SubSubCode = Subsubcategory.Code;
                    it.itemcode = itemmaster.itemcode;
                    it.marginPoint = itemmaster.marginPoint;
                    it.Number = itemmaster.Number;
                    it.PramotionalDiscount = itemmaster.PramotionalDiscount;
                    it.MinOrderQty = itemmaster.MinOrderQty;
                    it.NetPurchasePrice = itemmaster.NetPurchasePrice;
                    it.GeneralPrice = itemmaster.GeneralPrice;
                    it.price = itemmaster.price;
                    it.promoPerItems = itemmaster.promoPerItems;
                    it.promoPoint = itemmaster.promoPoint;
                    it.PurchaseMinOrderQty = itemmaster.PurchaseMinOrderQty;
                    it.PurchasePrice = itemmaster.PurchasePrice;
                    it.PurchaseSku = itemmaster.PurchaseSku;
                    it.PurchaseUnitName = itemmaster.PurchaseUnitName;
                    it.SellingSku = itemmaster.SellingSku;
                    it.SellingUnitName = itemmaster.SellingUnitName;
                    it.SizePerUnit = itemmaster.SizePerUnit;
                    it.UnitPrice = itemmaster.UnitPrice;
                    it.VATTax = itemmaster.VATTax;
                    it.HSNCode = itemmaster.HSNCode;
                    it.HindiName = itemmaster.HindiName;
                    it.CompanyId = itemmaster.CompanyId;
                    it.Reason = itemmaster.Reason;
                    it.DefaultBaseMargin = itemmaster.DefaultBaseMargin;
                    it.Deleted = false;
                    it.active = false;
                    it.itemname = itemmaster.itemname;
                    it.itemBaseName = itemmaster.itemBaseName;
                    it.UOM = itemmaster.UOM;
                    it.UnitofQuantity = itemmaster.UnitofQuantity;
                    it.ItemMultiMRPId = MultiMRPitem.ItemMultiMRPId;
                    it.IsSensitive = itemmaster.IsSensitive;
                    it.IsSensitiveMRP = itemmaster.IsSensitiveMRP;
                    it.ShelfLife = itemmaster.ShelfLife;
                    it.IsReplaceable = itemmaster.IsReplaceable;

                    it.Type = itemmaster.Type;//Type
                    it.MaterialItemId = MaterialItemId;
  
                    Liit.Add(it);
                    // Context.itemMasters.Add(it);
                    //this.Commit();
                    CurrentStock cntstock = new CurrentStock();
                    cntstock = Context.DbCurrentStock.Where(x => x.ItemNumber == itemmaster.Number && x.WarehouseId == o.WarehouseId && x.CompanyId == itemmaster.CompanyId && x.ItemMultiMRPId == itemmaster.ItemMultiMRPId && x.Deleted == false).SingleOrDefault();
                    //if (itemmaster.IsSensitiveMRP)
                    //{
                    //    cntstock = DbCurrentStock.Where(x => x.ItemNumber == itemmaster.Number && x.WarehouseId == o.WarehouseId && x.CompanyId == itemmaster.CompanyId && x.Deleted == false).FirstOrDefault();
                    //}
                    //else
                    //{
                    //    cntstock = DbCurrentStock.Where(x => x.ItemNumber == itemmaster.Number && x.WarehouseId == o.WarehouseId && x.CompanyId == itemmaster.CompanyId && x.ItemMultiMRPId == itemmaster.ItemMultiMRPId && x.Deleted == false).SingleOrDefault();
                    //}
                    if (cntstock == null)
                    {
                        CurrentStock newCstk = new CurrentStock();
                        newCstk.ItemNumber = itemmaster.Number;
                        newCstk.WarehouseId = o.WarehouseId;
                        newCstk.WarehouseName = o.WarehouseName;
                        newCstk.CompanyId = itemmaster.CompanyId;
                        newCstk.CurrentInventory = 0;
                        newCstk.CreationDate = CurrentDatetime;
                        newCstk.UpdatedDate = CurrentDatetime;
                        // Multimrp
                        newCstk.MRP = itemmaster.price;
                        newCstk.UnitofQuantity = itemmaster.UnitofQuantity;
                        newCstk.UOM = itemmaster.UOM;
                        newCstk.ItemMultiMRPId = MultiMRPitem.ItemMultiMRPId;
                        newCstk.itemname = itemmaster.itemname;
                        newCstk.itemBaseName = itemmaster.itemBaseName;

                        Licntstock.Add(newCstk);
                        //Context.DbCurrentStock.Add(newCstk);
                        //this.Commit();

                        CurrentStockHistory Oss = new CurrentStockHistory();
                        Oss.StockId = newCstk.StockId;
                        Oss.ItemNumber = newCstk.ItemNumber;

                        Oss.TotalInventory = newCstk.CurrentInventory;
                        Oss.WarehouseName = newCstk.WarehouseName;
                        Oss.Warehouseid = newCstk.WarehouseId;
                        Oss.CompanyId = newCstk.CompanyId;
                        Oss.CreationDate = CurrentDatetime;
                        // Multimrp
                        Oss.MRP = newCstk.MRP;

                        Oss.UnitofQuantity = newCstk.UnitofQuantity;
                        Oss.UOM = newCstk.UOM;
                        Oss.ItemMultiMRPId = newCstk.ItemMultiMRPId;
                        Oss.itemname = newCstk.itemname;
                        Oss.itemBaseName = newCstk.itemBaseName;
                        Oss.userid = people.PeopleID;
                        Oss.UserName = people.DisplayName;
                        LOss.Add(Oss);
                        // Context.CurrentStockHistoryDb.Add(Oss);
                        //int idd = this.Commit();
                    }
                    else
                    {
                        UpdatecurrentstockResult = false;
                        //cntstock.ItemNumber = itemmaster.Number;
                        //cntstock.WarehouseId = o.WarehouseId;
                        //cntstock.WarehouseName = o.WarehouseName;
                        //cntstock.CompanyId = itemmaster.CompanyId;
                        //cntstock.Deleted = false;
                        //cntstock.UpdatedDate = CurrentDatetime;
                        //// Multimrp
                        //cntstock.MRP = itemmaster.price;
                        //cntstock.UnitofQuantity = itemmaster.UnitofQuantity;
                        //cntstock.UOM = itemmaster.UOM;
                        //cntstock.ItemMultiMRPId = MultiMRPitem.ItemMultiMRPId;
                        //cntstock.itemname = itemmaster.itemname;
                        //cntstock.itemBaseName = itemmaster.itemBaseName;

                        //Licntstock.Add(cntstock);
                        //DbCurrentStock.Attach(cntstock);
                        //this.Entry(cntstock).State = EntityState.Modified;
                        //this.Commit();
                        UpdatecurrentstockResult = UpdateCurrentStock(cntstock, itemmaster, o.WarehouseId, o.WarehouseName, MultiMRPitem);
                        if (!UpdatecurrentstockResult)
                        {
                            return false;
                            //throw new Exception("Something went wrong please try again later..");
                        }

                        CurrentStockHistory Oss = new CurrentStockHistory();
                        Oss.StockId = cntstock.StockId;
                        Oss.ItemNumber = cntstock.ItemNumber;
                        Oss.TotalInventory = cntstock.CurrentInventory;
                        Oss.WarehouseName = cntstock.WarehouseName;
                        Oss.Warehouseid = cntstock.WarehouseId;
                        Oss.CompanyId = cntstock.CompanyId;
                        Oss.CreationDate = CurrentDatetime;
                        Oss.userid = people.PeopleID;
                        Oss.UserName = people.DisplayName;
                        // Multimrp
                        Oss.MRP = cntstock.MRP;
                        Oss.UnitofQuantity = cntstock.UnitofQuantity;
                        Oss.UOM = cntstock.UOM;
                        Oss.ItemMultiMRPId = cntstock.ItemMultiMRPId;
                        Oss.itemname = cntstock.itemname;
                        Oss.itemBaseName = cntstock.itemBaseName;
                        LOss.Add(Oss);
                        // Context.CurrentStockHistoryDb.Add(Oss);
                        //int idd = this.Commit();
                    }

                    ///***** set all items current stock behalf of all warehouses *****///
                }
                Context.itemMasters.AddRange(Liit);
                Context.DbCurrentStock.AddRange(Licntstock);
                Context.CurrentStockHistoryDb.AddRange(LOss);

                if (Liit.Any() && Licntstock.Any() && LOss.Any() && UpdatecurrentstockResult)
                {
                    Result = true;
                }
                return Result;
                //if (Liit != null && Liit.Any())
                //{
                //    foreach (var item in Liit)
                //    {

                //        Context.Entry(Context.itemMasters).State = EntityState.Modified;
                //    }

                //}
                //Context.itemMasters.Add(it);

                //if (Licntstock != null && Licntstock.Any())
                //{
                //    foreach (var item in Licntstock)
                //    {

                //        Context.Entry(Context.DbCurrentStock).State = EntityState.Modified;
                //    }

                //}


                //if (LOss != null && LOss.Any())
                //{
                //    foreach (var item in LOss)
                //    {

                //        Context.Entry(Context.CurrentStockHistoryDb).State = EntityState.Modified;
                //    }

                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void TradeItemMaster(ItemMasterCentral itemmaster, SubsubCategory Subsubcategory, People people)
        {
            try
            {
                if (itemmaster.IsTradeItem)
                {
                    TradeItemMasterDc tradeItem = new TradeItemMasterDc
                    {
                        BaseCategoryId = itemmaster.BaseCategoryid,
                        CategoryId = itemmaster.Categoryid,
                        SubCategoryId = itemmaster.SubCategoryId,
                        BrandId = itemmaster.SubsubCategoryid,
                        BaseCategoryName = itemmaster.BaseCategoryName,
                        BasePrice = itemmaster.UnitPrice,
                        BrandImagePath = Subsubcategory.LogoUrl,
                        BrandName = itemmaster.SubsubcategoryName,
                        CategoryName = itemmaster.CategoryName,
                        CreatedBy = people.PeopleID,
                        CreatedDate = CurrentDatetime,
                        ImagePath = itemmaster.LogoUrl,
                        IsActive = false,
                        IsDelete = false,
                        ItemId = itemmaster.Id,
                        ItemName = itemmaster.itemname,
                        MRP = itemmaster.price,
                        UnitOfMeasurement = itemmaster.UOM,
                        SubCategoryName = itemmaster.SubcategoryName,
                        UnitOfQuantity = itemmaster.UnitofQuantity,
                        HSNCode = itemmaster.HSNCode,
                        TotalTaxPercent = Convert.ToString(itemmaster.TotalTaxPercentage),
                        CGST = Convert.ToString(itemmaster.TotalTaxPercentage / 2),
                        SGST = Convert.ToString(itemmaster.TotalTaxPercentage / 2),
                        CESS = Convert.ToString(itemmaster.TotalCessPercentage)
                    };

                    BackgroundTaskManager.Run(() =>
                    {
                        try
                        {
                            var tradeUrl = ConfigurationManager.AppSettings["TradeURL"] + "/api/TradeItem/Insert";
                            TextFileLogHelper.LogError(tradeUrl, false);
                            using (GenericRestHttpClient<TradeItemMasterDc, string> memberClient = new GenericRestHttpClient<TradeItemMasterDc, string>(tradeUrl, "", null))
                            {
                                tradeItem = AsyncContext.Run(() => memberClient.PostAsync(tradeItem));
                            }
                        }
                        catch (Exception ex)
                        {
                            TextFileLogHelper.LogError("Error while saving item in Trade: " + ex.ToString());
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool UpdateCurrentStock(CurrentStock cntstock, ItemMasterCentral itemmaster, int WarehouseId, string WarehouseName, ItemMultiMRP MultiMRPitem)
        {
            bool Result = false;
            try
            {
                cntstock.ItemNumber = itemmaster.Number;
                cntstock.WarehouseId = WarehouseId;
                cntstock.WarehouseName = WarehouseName;
                cntstock.CompanyId = itemmaster.CompanyId;
                cntstock.Deleted = false;
                cntstock.UpdatedDate = CurrentDatetime;
                // Multimrp
                cntstock.MRP = itemmaster.price;
                cntstock.UnitofQuantity = itemmaster.UnitofQuantity;
                cntstock.UOM = itemmaster.UOM;
                cntstock.ItemMultiMRPId = MultiMRPitem.ItemMultiMRPId;
                cntstock.itemname = itemmaster.itemname;
                cntstock.itemBaseName = itemmaster.itemBaseName;
                Context.DbCurrentStock.Attach(cntstock);
                this.Context.Entry(cntstock).State = EntityState.Modified;
                Result = true;
                return Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        

        public IEnumerable<MaterialItemDetails> GetAddedMaterialItemDetails(int BomId)
        {
            IEnumerable<MaterialItemDetails> objMaterialItemDetails = Context.MaterialItemDetails.Where(x => x.BomId == BomId).AsQueryable();
            // IEnumerable<MaterialItemDetails> objMaterialItemDetails = Context.MaterialItemDetails.AsQueryable();
            return objMaterialItemDetails;
        }

        private OuterBagDescriptionDTO GetOuterBagAcItem(int MaterialIitemId)
        {
            try
            {
                OuterBagDescriptionDTO objOuterBagDescriptionDTO = (from O in Context.OuterBagMaster
                                                                    join M in Context.MaterialItemMaster
                                                                      on O.OuterBagId equals M.BagId
                                                                    where M.Id == MaterialIitemId
                                                                    select new OuterBagDescriptionDTO
                                                                    {
                                                                        OuterBagId = O.OuterBagId,
                                                                        OuterBagName = O.BagName
                                                                    }).FirstOrDefault();



                //int OuterBagid = Context.MaterialItemMaster.Where(x => x.Id == MaterialIitemId).Select(x => x.BagId).FirstOrDefault();
                return objOuterBagDescriptionDTO;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public OuterBagDTO GetOuterBagDetails(int MaterialItemId)
        //{
        //    IEnumerable<OuterBagMaster> ObjOuterBagMaster = OuterBagDetails();
        //    OuterBagDescriptionDTO ObjOuterbagDescription = GetOuterBagAcItem(MaterialItemId);
        //    IEnumerable<AddedBomMaterialDtls> objAddedBomMaterialDtls = AddedBomMaterialDtls(MaterialItemId);
        //    OuterBagDTO ObjOuterBagDTO = new OuterBagDTO
        //    {
        //        OuterBagMaster = ObjOuterBagMaster,
        //        OuterBagDescriptionDTO = ObjOuterbagDescription,
        //        AddedBomMaterialDtls = objAddedBomMaterialDtls
        //    };
        //    return ObjOuterBagDTO;

        //}

        public IEnumerable<AddedBomMaterialDtls> AddedBomMaterialDtls(int BomId)
        {
            var BomIdParam = new SqlParameter
            {
                ParameterName = "BomId",
                Value = BomId
            };
            IEnumerable<AddedBomMaterialDtls> ObjAddedBomMaterialDtls = Context.Database.SqlQuery<AddedBomMaterialDtls>("GetAddedBomItemAcBomId @BomId", BomIdParam).ToList();
            return ObjAddedBomMaterialDtls;
        }

        public bool UpdateBomId(string ItemNumber, int BomID, int UpdatedBy)
        {
            bool FinalResult = false;
            try
            {

                var ItemNumberParam = new SqlParameter
                {
                    ParameterName = "ItemNumber",
                    Value = ItemNumber
                };

                var BomIDParam = new SqlParameter
                {
                    ParameterName = "BomID",
                    Value = BomID
                };
                var UpdatedByParam = new SqlParameter
                {
                    ParameterName = "UpdatedBy",
                    Value = UpdatedBy
                };

                int Result = Context.Database.ExecuteSqlCommand("UpdateBomID @ItemNumber ,@BomID ,@UpdatedBy  ", ItemNumberParam,
                       BomIDParam, UpdatedByParam);
                if (Result > 0)
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

        public IEnumerable<GetBomNameDtls> GetBomName(int WareHouseId, int ItemId)
        {
            var WareHouseIdParam = new SqlParameter
            {
                ParameterName = "WareHouseId",
                Value = WareHouseId
            };

            var ItemIdParam = new SqlParameter
            {
                ParameterName = "ItemId",
                Value = ItemId
            };

            IEnumerable<GetBomNameDtls> ObjGetBomNameDtls = Context.Database.SqlQuery<GetBomNameDtls>("GetBomName @WareHouseID ,@ItemId ", WareHouseIdParam,
                    ItemIdParam).ToList();
            return ObjGetBomNameDtls;
        }

        public MaterialItemMaster Getmaterial(string ItemNumber)
        {
            MaterialItemMaster objMaterialItemMaster = Context.MaterialItemMaster.Where(x => x.ItemNumber == ItemNumber).FirstOrDefault();
            return objMaterialItemMaster;
        }
       
    }
}
