using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.BillDiscount;
using GenricEcommers.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.External.RetailerAPP
{
    [RoutePrefix("api/BillDiscountOfferApp")]
    public class BillDiscountController : BaseAuthController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        public static Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Get all Active Common Discount Offer On App
        /// </summary>
        /// <param name="CurrentDate"></param>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        [Route("CommonDiscountOffer")]
        [HttpGet]
        public HttpResponseMessage GetCommonDiscountOfferOnApp(int CustomerId)
        {
            List<OfferBDDTO> BillDiscount = new List<OfferBDDTO>();
            List<OfferBDDTO> FinalBillDiscount = new List<OfferBDDTO>();
            ResDTOList res;
            DateTime CurrentDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    Customer Customer = context.Customers.Where(x => x.CustomerId == CustomerId && x.Deleted == false && x.Active == true).FirstOrDefault();
                    if (Customer != null)
                    {
                        BillDiscount = context.OfferDb.Where(o => o.IsDeleted == false && o.WarehouseId == Customer.Warehouseid && o.start <= CurrentDate && o.end >= CurrentDate && o.IsActive == true
                            && (o.OfferAppType == "Retailer App" || o.OfferAppType == "Both") && (o.OfferOn == "BillDiscount" || o.OfferOn == "ScratchBillDiscount"))
                                         .Select(o => new OfferBDDTO
                                         {

                                             OfferId = o.OfferId,
                                             CustomerId = Customer.CustomerId,
                                             WarehouseId = o.WarehouseId,
                                             OfferName = o.OfferName,
                                             OfferOn = o.OfferOn,
                                             OfferCategory = o.OfferCategory,
                                             Description = o.Description,
                                             start = o.start,
                                             end = o.end,
                                             DiscountPercentage = o.DiscountPercentage,
                                             IsActive = o.IsActive,
                                             IsDeleted = o.IsDeleted,
                                             CreatedDate = o.CreatedDate,
                                             UpdateDate = o.UpdateDate,
                                             OfferCode = o.OfferCode,
                                             BillDiscountOfferOn = o.BillDiscountOfferOn,
                                             BillDiscountWallet = o.BillDiscountWallet,
                                             BillAmount = o.BillAmount,
                                             IsMultiTimeUse = o.IsMultiTimeUse,
                                             IsUseOtherOffer = o.IsUseOtherOffer,
                                         }).ToList();

                        if (BillDiscount.Count() > 0)
                        {
                            //check if offer not used 
                            foreach (var bdcheck in BillDiscount)
                            {
                                BillDiscount found = context.BillDiscountDb.Where(x => x.CustomerId == CustomerId && x.OfferId == bdcheck.OfferId && x.IsDeleted == false && x.IsActive == true).FirstOrDefault();
                                if (found != null)
                                {
                                    if (found.OrderId > 0)
                                    {
                                        //BillDiscount.Remove(bdcheck); offer used already
                                    }
                                    else if (found.IsScratchBDCode)
                                    {
                                        bdcheck.IsScratchBDCode = true;
                                        FinalBillDiscount.Add(bdcheck);
                                    }
                                    else
                                    {
                                        bdcheck.IsScratchBDCode = false;
                                        FinalBillDiscount.Add(bdcheck);
                                    }
                                }
                                else if (bdcheck.OfferOn == "BillDiscount")
                                {
                                    FinalBillDiscount.Add(bdcheck);
                                }

                            }
                            res = new ResDTOList()
                            {
                                BillDiscount = FinalBillDiscount,
                                Status = true,
                                Message = "Success"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                }
                res = new ResDTOList()
                {
                    BillDiscount = null,
                    Status = false,
                    Message = "fail"
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ee)
            {

                res = new ResDTOList()
                {
                    BillDiscount = null,
                    Status = false,
                    Message = ("something isse occurs : " + ee)
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        /// <summary>
        /// Get all Active Common Discount Offer On App V2
        /// </summary>
        /// <param name="CurrentDate"></param>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        [Route("CommonDiscountOffer/V2")]
        [HttpGet]
        //[AllowAnonymous]
        public HttpResponseMessage GetCommonDiscountOfferOnAppV2(int CustomerId)
        {
            List<OfferBDDTO> FinalBillDiscount = new List<OfferBDDTO>();
            ResDTOList res;

            using (AuthContext context = new AuthContext())
            {

                CustomersManager manager = new CustomersManager();

                List<AngularJSAuthentication.DataContracts.Masters.BillDiscountOfferDc> billDiscountOfferDcs = manager.GetCustomerBillDiscount(CustomerId, "Retailer App");
                if (billDiscountOfferDcs.Any())
                {
                    foreach (var billDiscountOfferDc in billDiscountOfferDcs)
                    {
                        var bdcheck = new OfferBDDTO
                        {
                            OfferId = billDiscountOfferDc.OfferId,
                            WarehouseId = billDiscountOfferDc.WarehouseId,
                            CustomerId = billDiscountOfferDc.CustomerId,
                            PeopleId = billDiscountOfferDc.PeopleId,
                            OfferName = billDiscountOfferDc.OfferName,
                            OfferCode = billDiscountOfferDc.OfferCode,
                            OfferCategory = billDiscountOfferDc.OfferCategory,
                            OfferOn = billDiscountOfferDc.OfferOn,
                            start = billDiscountOfferDc.start,
                            end = billDiscountOfferDc.end,
                            DiscountPercentage = billDiscountOfferDc.DiscountPercentage,
                            BillAmount = billDiscountOfferDc.BillAmount,
                            LineItem = billDiscountOfferDc.LineItem,
                            MaxBillAmount = billDiscountOfferDc.MaxBillAmount,
                            Description = billDiscountOfferDc.Description,
                            IsDeleted = billDiscountOfferDc.IsDeleted,
                            IsActive = billDiscountOfferDc.IsActive,
                            CreatedDate = billDiscountOfferDc.CreatedDate,
                            UpdateDate = billDiscountOfferDc.UpdateDate,
                            BillDiscountOfferOn = billDiscountOfferDc.BillDiscountOfferOn,
                            BillDiscountWallet = billDiscountOfferDc.BillDiscountWallet,
                            IsMultiTimeUse = billDiscountOfferDc.IsMultiTimeUse,
                            IsUseOtherOffer = billDiscountOfferDc.IsUseOtherOffer,
                            IsScratchBDCode = billDiscountOfferDc.IsScratchBDCode,
                            Category = billDiscountOfferDc.Category,
                            subCategory = billDiscountOfferDc.subCategory,
                            subSubCategory = billDiscountOfferDc.subSubCategory,
                            BillDiscountType = billDiscountOfferDc.BillDiscountType,
                            MaxDiscount = billDiscountOfferDc.MaxDiscount,
                            OfferUseCount = billDiscountOfferDc.OfferUseCount,
                            OfferAppType = billDiscountOfferDc.OfferAppType,
                            ApplyOn = billDiscountOfferDc.ApplyOn,
                            WalletType = billDiscountOfferDc.WalletType,
                            OfferBillDiscountItems = billDiscountOfferDc.OfferBillDiscountItems.Select(y => new OfferBillDiscountItem
                            {
                                CategoryId = y.CategoryId,
                                Id = y.Id,
                                IsInclude = y.IsInclude,
                                SubCategoryId = y.SubCategoryId
                            }).ToList(),
                            OfferItems = billDiscountOfferDc.OfferItems.Select(y => new OfferItem
                            {
                                IsInclude = y.IsInclude,
                                itemId = y.itemId
                            }).ToList()
                        };

                        List<BillDiscount> founds = context.BillDiscountDb.Where(x => x.CustomerId == CustomerId && x.OfferId == bdcheck.OfferId && x.IsDeleted == false && x.IsActive == true).ToList();
                        if (founds != null && founds.Any())
                        {
                            if (bdcheck.OfferOn == "ScratchBillDiscount")
                            {
                                if (founds.FirstOrDefault().OrderId > 0)
                                {
                                    //BillDiscount.Remove(bdcheck); offer used already
                                }
                                else if (founds.FirstOrDefault().IsScratchBDCode)
                                {
                                    bdcheck.IsScratchBDCode = true;
                                    FinalBillDiscount.Add(bdcheck);
                                }
                                else
                                {
                                    bdcheck.IsScratchBDCode = false;
                                    FinalBillDiscount.Add(bdcheck);
                                }
                            }
                            else
                            {
                                if (bdcheck.OfferOn == "BillDiscount" && bdcheck.IsMultiTimeUse)
                                {
                                    if (!bdcheck.OfferUseCount.HasValue)
                                        FinalBillDiscount.Add(bdcheck);
                                    else if (bdcheck.OfferUseCount.Value > founds.Count())
                                        FinalBillDiscount.Add(bdcheck);
                                }
                            }
                        }
                        else if (bdcheck.OfferOn == "BillDiscount")
                        {
                            FinalBillDiscount.Add(bdcheck);
                        }

                    }
                }
                res = new ResDTOList()
                {
                    BillDiscount = FinalBillDiscount,
                    Status = true,
                    Message = "Success"
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }

        }



        /// <summary>
        /// Get all Active Common Discount Offer On App V2
        /// </summary>
        /// <param name="CurrentDate"></param>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        [Route("GetDistributorOffer")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetDistributorOffer(int CustomerId)
        {
            List<OfferBDDTO> FinalBillDiscount = new List<OfferBDDTO>();
            ResDTOList res;

            using (AuthContext context = new AuthContext())
            {

                CustomersManager manager = new CustomersManager();

                List<AngularJSAuthentication.DataContracts.Masters.BillDiscountOfferDc> billDiscountOfferDcs = manager.GetCustomerBillDiscount(CustomerId, "Distributor App");
                if (billDiscountOfferDcs.Any())
                {
                    var offerIds = billDiscountOfferDcs.Where(x => x.BillDiscountOfferOn == "FreeItem").Select(x => x.OfferId).ToList();
                    List<BillDiscountFreeItem> BillDiscountFreeItems = offerIds.Any() ? context.BillDiscountFreeItem.Where(x => offerIds.Contains(x.offerId) && x.RemainingOfferStockQty < x.OfferStockQty).ToList() : new List<BillDiscountFreeItem>();
                    foreach (var billDiscountOfferDc in billDiscountOfferDcs)
                    {
                        var bdcheck = new OfferBDDTO
                        {
                            OfferId = billDiscountOfferDc.OfferId,
                            WarehouseId = billDiscountOfferDc.WarehouseId,
                            CustomerId = billDiscountOfferDc.CustomerId,
                            PeopleId = billDiscountOfferDc.PeopleId,
                            OfferName = billDiscountOfferDc.OfferName,
                            OfferCode = billDiscountOfferDc.OfferCode,
                            OfferCategory = billDiscountOfferDc.OfferCategory,
                            OfferOn = billDiscountOfferDc.OfferOn,
                            start = billDiscountOfferDc.start,
                            end = billDiscountOfferDc.end,
                            DiscountPercentage = billDiscountOfferDc.DiscountPercentage,
                            BillAmount = billDiscountOfferDc.BillAmount,
                            LineItem = billDiscountOfferDc.LineItem,
                            MaxBillAmount = billDiscountOfferDc.MaxBillAmount,
                            Description = billDiscountOfferDc.Description,
                            IsDeleted = billDiscountOfferDc.IsDeleted,
                            IsActive = billDiscountOfferDc.IsActive,
                            CreatedDate = billDiscountOfferDc.CreatedDate,
                            UpdateDate = billDiscountOfferDc.UpdateDate,
                            BillDiscountOfferOn = billDiscountOfferDc.BillDiscountOfferOn,
                            BillDiscountWallet = billDiscountOfferDc.BillDiscountWallet,
                            IsMultiTimeUse = billDiscountOfferDc.IsMultiTimeUse,
                            IsUseOtherOffer = billDiscountOfferDc.IsUseOtherOffer,
                            IsScratchBDCode = billDiscountOfferDc.IsScratchBDCode,
                            Category = billDiscountOfferDc.Category,
                            subCategory = billDiscountOfferDc.subCategory,
                            subSubCategory = billDiscountOfferDc.subSubCategory,
                            BillDiscountType = billDiscountOfferDc.BillDiscountType,
                            MaxDiscount = billDiscountOfferDc.MaxDiscount,
                            OfferUseCount = billDiscountOfferDc.OfferUseCount,
                            OfferAppType = billDiscountOfferDc.OfferAppType,
                            ApplyOn = billDiscountOfferDc.ApplyOn,
                            WalletType = billDiscountOfferDc.WalletType,
                            itemId = billDiscountOfferDc.itemId,
                            itemname = billDiscountOfferDc.itemname,
                            OffersaleQty = billDiscountOfferDc.OffersaleQty,
                            DistributorOfferType = billDiscountOfferDc.DistributorOfferType,
                            DistributorDiscountAmount = billDiscountOfferDc.DistributorDiscountAmount,
                            DistributorDiscountPercentage = billDiscountOfferDc.DistributorDiscountPercentage,
                            OfferBillDiscountItems = billDiscountOfferDc.OfferBillDiscountItems.Select(y => new OfferBillDiscountItem
                            {
                                CategoryId = y.CategoryId,
                                Id = y.Id,
                                IsInclude = y.IsInclude,
                                SubCategoryId = y.SubCategoryId
                            }).ToList(),
                            OfferItems = billDiscountOfferDc.OfferItems.Select(y => new OfferItem
                            {
                                IsInclude = y.IsInclude,
                                itemId = y.itemId
                            }).ToList(),
                            slapOfferDCs = context.OfferDb.Where(x => x.OfferId == billDiscountOfferDc.OfferId && x.OfferOn == "Slab").Select(y => new SlapOfferDC
                            {
                                BillDiscountOfferOn = y.BillDiscountOfferOn,
                                DiscountPercentage = y.DistributorDiscountPercentage,
                                OfferOnPrice = y.BillAmount,
                                WalletAmount = y.FreeWalletPoint,

                            }).ToList(),
                            postItemOfferDCs = context.DistributorOffer.Where(x => x.offerId == billDiscountOfferDc.OfferId).Select(y => new PostItemOfferDC
                            {
                                OffersaleQty = y.OffersaleQty ?? 0,
                                OffersaleAmount = y.OffersaleAmount ?? 0,
                                OffersaleWeight = y.OffersaleWeight ?? 0,
                                UOM = y.UOM,
                                MaxDiscount = billDiscountOfferDc.DistributorDiscountAmount,
                                //MaxDiscount = billDiscountOfferDc.MaxDiscount,
                                FreeOfferType = billDiscountOfferDc.FreeOfferType,
                            }).ToList(),
                            RetailerBillDiscountFreeItemDcs = BillDiscountFreeItems.Where(x => x.offerId == billDiscountOfferDc.OfferId).Select(x => new DataContracts.External.RetailerBillDiscountFreeItemDc
                            {
                                ItemId = x.ItemId,
                                ItemName = x.ItemName,
                                Qty = x.Qty
                            }).ToList()
                        };

                        List<BillDiscount> founds = context.BillDiscountDb.Where(x => x.CustomerId == CustomerId && x.OfferId == bdcheck.OfferId && x.IsDeleted == false && x.IsActive == true).ToList();
                        if (founds != null && founds.Any() && bdcheck.OfferAppType != "Distributor App")
                        {
                            if (bdcheck.OfferOn == "ScratchBillDiscount")
                            {
                                if (founds.FirstOrDefault().OrderId > 0)
                                {
                                    //BillDiscount.Remove(bdcheck); offer used already
                                }
                                else if (founds.FirstOrDefault().IsScratchBDCode)
                                {
                                    bdcheck.IsScratchBDCode = true;
                                    FinalBillDiscount.Add(bdcheck);
                                }
                                else
                                {
                                    bdcheck.IsScratchBDCode = false;
                                    FinalBillDiscount.Add(bdcheck);
                                }
                            }
                            else
                            {
                                if (bdcheck.OfferOn == "BillDiscount" && bdcheck.IsMultiTimeUse)
                                {
                                    if (!bdcheck.OfferUseCount.HasValue)
                                        FinalBillDiscount.Add(bdcheck);
                                    else if (bdcheck.OfferUseCount.Value > founds.Count())
                                        FinalBillDiscount.Add(bdcheck);
                                }
                            }
                        }
                        else if (bdcheck.OfferOn == "BillDiscount" || bdcheck.OfferOn == "ItemPost" || bdcheck.OfferOn == "Slab" || bdcheck.OfferOn == "ItemMarkDown")
                        {

                            if ((bdcheck.OfferOn == "BillDiscount") && bdcheck.IsMultiTimeUse)
                            {
                                if (!bdcheck.OfferUseCount.HasValue)
                                    FinalBillDiscount.Add(bdcheck);
                                else if (bdcheck.OfferUseCount.Value > founds.Count())
                                    FinalBillDiscount.Add(bdcheck);
                            }
                            else
                            {
                                FinalBillDiscount.Add(bdcheck);
                            }
                        }

                    }
                }
                res = new ResDTOList()
                {
                    BillDiscount = FinalBillDiscount,
                    Status = true,
                    Message = "Success"
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }

        }


        /// <summary>
        /// Get all Active Bill Discount Offer On App
        /// </summary>
        /// <param name="CurrentDate"></param>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetBillDiscountOfferOnApp(int CustomerId)
        {
            List<OfferBDDTO> BillDiscount = new List<OfferBDDTO>();
            List<OfferBDDTO> FinalBillDiscount = new List<OfferBDDTO>();
            ResDTOList res;
            DateTime CurrentDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    Customer Customer = context.Customers.Where(x => x.CustomerId == CustomerId && x.Deleted == false && x.Active == true).FirstOrDefault();
                    if (Customer != null)
                    {
                        BillDiscount = (from o in context.OfferDb
                                        where o.IsDeleted == false && o.WarehouseId == Customer.Warehouseid && o.start <= CurrentDate && o.end >= CurrentDate && o.IsActive == true && o.IsDeleted == false && o.OfferOn == "BillDiscount"
                                        select new OfferBDDTO
                                        {

                                            OfferId = o.OfferId,
                                            CustomerId = Customer.CustomerId,
                                            WarehouseId = o.WarehouseId,
                                            OfferName = o.OfferName,
                                            OfferOn = o.OfferOn,
                                            OfferCategory = o.OfferCategory,
                                            Description = o.Description,
                                            start = o.start,
                                            end = o.end,
                                            DiscountPercentage = o.DiscountPercentage,
                                            IsActive = o.IsActive,
                                            IsDeleted = o.IsDeleted,
                                            CreatedDate = o.CreatedDate,
                                            UpdateDate = o.UpdateDate,
                                            OfferCode = o.OfferCode,
                                            BillDiscountOfferOn = o.BillDiscountOfferOn,
                                            BillDiscountWallet = o.BillDiscountWallet,
                                            BillAmount = o.BillAmount,
                                            IsMultiTimeUse = o.IsMultiTimeUse,
                                            IsUseOtherOffer = o.IsUseOtherOffer
                                        }).ToList();

                        if (BillDiscount.Count() > 0)
                        {
                            //check if offer used by same customer (single type Bill Discount)
                            foreach (var bdcheck in BillDiscount)
                            {
                                bool found = context.BillDiscountDb.Any(x => x.CustomerId == CustomerId && x.OfferId == bdcheck.OfferId && x.IsDeleted == false && x.IsActive == true);
                                if (!found)
                                {
                                    FinalBillDiscount.Add(bdcheck);
                                }
                            }
                            res = new ResDTOList()
                            {
                                BillDiscount = FinalBillDiscount,
                                Status = true,
                                Message = "Success"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                }
                res = new ResDTOList()
                {
                    BillDiscount = null,
                    Status = false,
                    Message = "fail"
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ee)
            {

                res = new ResDTOList()
                {
                    BillDiscount = null,
                    Status = false,
                    Message = ("something isse occurs : " + ee)
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }


        /// <summary>
        /// Check Customer Offer By Id (Offer is used or not or offer available for multi use)
        /// </summary>
        /// <param name="OfferId"></param>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        /// <summary>
        /// Check Customer Offer By Id (Offer is used or not or offer available for multi use)
        /// </summary>
        /// <param name="OfferId"></param>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        [Route("CheckOffer")]
        [HttpGet]
        public HttpResponseMessage CheckCustomerOfferById(string OfferId, int CustomerId)
        {
            List<int> OfferIds = OfferId.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            List<OfferBDDTO> BillDiscount = new List<OfferBDDTO>();
            List<OfferValidation> OfferValidations = new List<OfferValidation>();
            try
            {
                DateTime CurrentDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                using (AuthContext context = new AuthContext())
                {
                    int? warehouseId = context.Customers.Where(x => x.CustomerId == CustomerId && x.Deleted == false && x.Active == true).Select(x => x.Warehouseid).FirstOrDefault();
                    List<BillDiscount> BillDiscountUsed = context.BillDiscountDb.Where(x => x.CustomerId == CustomerId && OfferIds.Contains(x.OfferId) && x.IsActive == true && x.IsDeleted == false).ToList();
                    if (warehouseId != null && warehouseId > 0)
                    {
                        BillDiscount = (from o in context.OfferDb
                                        where OfferIds.Contains(o.OfferId)
                                        select new OfferBDDTO
                                        {
                                            OfferId = o.OfferId,
                                            OfferOn = o.OfferOn,
                                            start = o.start,
                                            end = o.end,
                                            IsActive = o.IsActive,
                                            IsDeleted = o.IsDeleted,
                                            IsMultiTimeUse = o.IsMultiTimeUse,
                                            OfferUseCount = o.OfferUseCount,
                                            IsCRMOffer = o.IsCRMOffer
                                        }).ToList();
                        // check  customer used id or not  && o.start <= CurrentDate && o.end >= CurrentDate

                        foreach (var item in BillDiscount)
                        {
                            bool valid = true;
                            string message = "";

                            if (item.IsCRMOffer)
                            {
                                if (BillDiscountUsed.All(y => y.OfferId == item.OfferId && y.OrderId > 0))
                                {
                                    valid = false;
                                    message = "You have already taken this offer.";
                                }
                                //else if (BillDiscountUsed.All(y => y.OfferId == item.OfferId && item.OfferOn != "ScratchBillDiscount"))
                                //{
                                //    item.OfferUseCount = item.OfferUseCount.HasValue ? item.OfferUseCount.Value : (item.IsMultiTimeUse ? 1000 : 1);
                                //    if ((item.OfferUseCount.Value <= BillDiscountUsed.Count(x => x.OfferId == item.OfferId && x.OrderId > 0)))
                                //    {
                                //        valid = false;
                                //        message = "You have already taken this offer.";
                                //    }
                                //}
                            }
                            else
                            {
                                if (item.OfferOn == "ScratchBillDiscount" && BillDiscountUsed.All(y => y.OfferId == item.OfferId && y.OrderId > 0))
                                {
                                    valid = false;
                                    message = "You have already taken this scratch card";
                                }
                                else if (item.OfferOn != "ScratchBillDiscount" && BillDiscountUsed.Any(y => y.OfferId == item.OfferId))
                                {
                                    if (!item.IsMultiTimeUse || (item.OfferUseCount.HasValue && item.OfferUseCount.Value <= BillDiscountUsed.Count(x => x.OfferId == item.OfferId)))
                                    {
                                        valid = false;
                                        message = "You have already taken this offer.";
                                    }
                                }
                            }

                            if (valid && !(item.start <= CurrentDate && item.end >= CurrentDate))
                            {
                                valid = false;
                                message = (item.OfferOn == "ScratchBillDiscount" ? "Scratch Card " : "Offer ") + "expired.";
                            }

                            if (valid && !(item.IsActive && !item.IsDeleted))
                            {
                                valid = false;
                                message = (item.OfferOn == "ScratchBillDiscount" ? "Scratch Card " : "Offer ") + "no more available.";
                            }

                            OfferValidations.Add(new OfferValidation { Message = message, Valid = valid, OfferId = item.OfferId });
                        }

                    }
                    var res = new
                    {
                        BillDiscount = OfferValidations,
                        Status = OfferValidations.All(x => x.Valid)
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }
            catch (Exception ee)
            {

                var res = new
                {
                    BillDiscount = OfferValidations,
                    Status = false,
                    Message = ("something isse occurs : " + ee)
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }



        /// <summary>
        /// Get all Active Common Discount Offer On App
        /// </summary>
        /// <param name="CurrentDate"></param>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        [Route("V1/CommonDiscountOffer")]
        [HttpGet]
        public HttpResponseMessage GetCommonDiscountOfferOnAppV1(int CustomerId)
        {
            List<OfferBDDTO> BillDiscount = new List<OfferBDDTO>();
            List<OfferBDDTO> FinalBillDiscount = new List<OfferBDDTO>();
            ResDTOList res;
            DateTime CurrentDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    Customer Customer = context.Customers.Where(x => x.CustomerId == CustomerId && x.Deleted == false && x.Active == true).FirstOrDefault();
                    if (Customer != null)
                    {
                        BillDiscount = (from o in context.OfferDb
                                        where (o.IsDeleted == false && o.WarehouseId == Customer.Warehouseid && o.start <= CurrentDate && o.end >= CurrentDate && o.IsActive == true && (o.OfferAppType == "Retailer App" || o.OfferAppType == "Both")
                                        && (o.OfferOn == "BillDiscount" || o.OfferOn == "ScratchBillDiscount"))
                                        select new OfferBDDTO
                                        {

                                            OfferId = o.OfferId,
                                            CustomerId = Customer.CustomerId,
                                            WarehouseId = o.WarehouseId,
                                            OfferName = o.OfferName,
                                            OfferOn = o.OfferOn,
                                            OfferCategory = o.OfferCategory,
                                            Description = o.Description,
                                            start = o.start,
                                            end = o.end,
                                            DiscountPercentage = o.DiscountPercentage,
                                            IsActive = o.IsActive,
                                            IsDeleted = o.IsDeleted,
                                            CreatedDate = o.CreatedDate,
                                            UpdateDate = o.UpdateDate,
                                            OfferCode = o.OfferCode,
                                            BillDiscountOfferOn = o.BillDiscountOfferOn,
                                            BillDiscountWallet = o.BillDiscountWallet,
                                            BillAmount = o.BillAmount,
                                            IsMultiTimeUse = o.IsMultiTimeUse,
                                            IsUseOtherOffer = o.IsUseOtherOffer,
                                        }).ToList();

                        if (BillDiscount.Count() > 0)
                        {
                            //check if offer not used 
                            foreach (var bdcheck in BillDiscount)
                            {
                                BillDiscount found = context.BillDiscountDb.Where(x => x.CustomerId == CustomerId && x.OfferId == bdcheck.OfferId && x.IsDeleted == false && x.IsActive == true).FirstOrDefault();
                                if (found != null)
                                {
                                    if (found.OrderId > 0)
                                    {
                                        //BillDiscount.Remove(bdcheck); offer used already
                                    }
                                    else if (found.IsScratchBDCode)
                                    {
                                        bdcheck.IsScratchBDCode = true;
                                        FinalBillDiscount.Add(bdcheck);
                                    }
                                    else
                                    {
                                        bdcheck.IsScratchBDCode = false;
                                        FinalBillDiscount.Add(bdcheck);
                                    }
                                }
                                else if (bdcheck.OfferOn == "BillDiscount")
                                {
                                    FinalBillDiscount.Add(bdcheck);
                                }

                            }
                            res = new ResDTOList()
                            {
                                BillDiscount = FinalBillDiscount,
                                Status = true,
                                Message = "Success"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                }
                res = new ResDTOList()
                {
                    BillDiscount = null,
                    Status = false,
                    Message = "fail"
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ee)
            {

                res = new ResDTOList()
                {
                    BillDiscount = null,
                    Status = false,
                    Message = ("something isse occurs : " + ee)
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }


        #region Get all Active Common Discount Offer for Agent App (04-10-2019)
        /// <summary>
        /// Get all Active Common Discount Offer for Agent App
        /// </summary>
        /// <param name="CurrentDate"></param>
        /// <param name="CustomerId"></param>
        ///<param name="PeopleId"></param>
        /// <returns></returns>
        [Route("CommonDiscountOfferForAgent")]
        [HttpGet]
        //[AllowAnonymous]
        public DataContracts.External.OfferdataDc CommonDiscountOfferForAgent(int CustomerId, int? PeopleId, int WarehouseId)
        {

            List<AngularJSAuthentication.DataContracts.External.OfferDc> FinalBillDiscount = new List<AngularJSAuthentication.DataContracts.External.OfferDc>();
            AngularJSAuthentication.DataContracts.External.OfferdataDc res;
            using (AuthContext context = new AuthContext())
            {
                var offertypeConfigs = context.OfferTypeDefaultConfigs.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                CustomersManager manager = new CustomersManager();
                var query = string.Format("exec IsSalesAppLead  {0}", PeopleId);
                var isSalesLead = context.Database.SqlQuery<int>(query).FirstOrDefault();
                List<long> storeids = new List<long>();
                if (isSalesLead > 0)
                    storeids = context.StoreDB.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.Id).ToList();
                else
                {
                    storeids = context.ClusterStoreExecutives.Where(x => x.ExecutiveId == PeopleId && x.IsDeleted == false && x.IsActive).Select(x => x.StoreId).Distinct().ToList();
                    var universalStoreIds = context.StoreDB.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.IsUniversal).Select(x => x.Id).ToList();
                    if (universalStoreIds != null && universalStoreIds.Any())
                        storeids.AddRange(universalStoreIds);
                }
                List<AngularJSAuthentication.DataContracts.Masters.BillDiscountOfferDc> billDiscountOfferDcs = manager.GetCustomerBillDiscount(CustomerId, "Sales App");
                if (billDiscountOfferDcs.Any())
                {
                    billDiscountOfferDcs = billDiscountOfferDcs.Where(x => (storeids.Contains(x.StoreId) || x.StoreId == 0) && x.ApplyType != "PrimeCustomer").ToList();

                    var offerIds = billDiscountOfferDcs.Where(x => x.BillDiscountOfferOn == "FreeItem").Select(x => x.OfferId).ToList();
                    List<GenricEcommers.Models.BillDiscountFreeItem> BillDiscountFreeItems = offerIds.Any() ? context.BillDiscountFreeItem.Where(x => offerIds.Contains(x.offerId) && x.RemainingOfferStockQty < x.OfferStockQty).ToList() : new List<GenricEcommers.Models.BillDiscountFreeItem>();
                    //List<BillDiscount> billDiscountfounds = context.BillDiscountDb.Where(x => x.CustomerId == CustomerId && offerIds.Contains(x.OfferId)).ToList();
                    foreach (var billDiscountOfferDc in billDiscountOfferDcs)
                    {
                        var OfferDefaultdata = billDiscountOfferDc.OfferOn == "Item" ? offertypeConfigs.Where(x => x.OfferType == billDiscountOfferDc.OfferOn && x.IsActive == true && x.IsDeleted == false).FirstOrDefault()
                            : offertypeConfigs.Where(x => x.OfferType == billDiscountOfferDc.OfferOn && x.DiscountOn == billDiscountOfferDc.BillDiscountOfferOn && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                        OfferDefaultdata = OfferDefaultdata != null ? OfferDefaultdata : new Model.SalesApp.OfferTypeDefaultConfig();
                        var bdcheck = new AngularJSAuthentication.DataContracts.External.OfferDc
                        {
                            OfferId = billDiscountOfferDc.OfferId,
                            MaxBillAmount = billDiscountOfferDc.MaxBillAmount,
                            OfferName = billDiscountOfferDc.OfferName,
                            OfferCode = billDiscountOfferDc.OfferCode,
                            OfferCategory = billDiscountOfferDc.OfferCategory,
                            OfferOn = billDiscountOfferDc.OfferOn,
                            start = billDiscountOfferDc.start,
                            end = billDiscountOfferDc.end,
                            DiscountPercentage = billDiscountOfferDc.DiscountPercentage,
                            BillAmount = billDiscountOfferDc.BillAmount,
                            LineItem = billDiscountOfferDc.LineItem,
                            Description = billDiscountOfferDc.Description,
                            BillDiscountOfferOn = billDiscountOfferDc.BillDiscountOfferOn,
                            BillDiscountWallet = billDiscountOfferDc.BillDiscountWallet,
                            IsMultiTimeUse = billDiscountOfferDc.IsMultiTimeUse,
                            IsUseOtherOffer = billDiscountOfferDc.IsUseOtherOffer,
                            IsScratchBDCode = billDiscountOfferDc.IsScratchBDCode,
                            BillDiscountType = billDiscountOfferDc.BillDiscountType,
                            OfferAppType = billDiscountOfferDc.OfferAppType,
                            ApplyOn = billDiscountOfferDc.ApplyOn,
                            WalletType = billDiscountOfferDc.WalletType,
                            MaxDiscount = billDiscountOfferDc.MaxDiscount,
                            ColorCode = !string.IsNullOrEmpty(billDiscountOfferDc.ColorCode) ? billDiscountOfferDc.ColorCode : OfferDefaultdata.ColorCode,
                            ImagePath = !string.IsNullOrEmpty(billDiscountOfferDc.ImagePath) ? billDiscountOfferDc.ImagePath : OfferDefaultdata.ImagePath,
                            IsBillDiscountFreebiesItem = billDiscountOfferDc.IsBillDiscountFreebiesItem,
                            IsBillDiscountFreebiesValue = billDiscountOfferDc.IsBillDiscountFreebiesValue,
                            offerminorderquantity = billDiscountOfferDc.offerminorderquantity,
                            offeritemname = billDiscountOfferDc.offeritemname,
                            OfferBillDiscountItems = billDiscountOfferDc.OfferBillDiscountItems.Select(y => new AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc
                            {
                                CategoryId = y.CategoryId,
                                Id = y.Id,
                                IsInclude = y.IsInclude,
                                SubCategoryId = y.SubCategoryId
                            }).ToList(),
                            OfferItems = billDiscountOfferDc.OfferItems.Select(y => new AngularJSAuthentication.DataContracts.External.OfferItemdc
                            {
                                IsInclude = y.IsInclude,
                                itemId = y.itemId
                            }).ToList(),
                            RetailerBillDiscountFreeItemDcs = BillDiscountFreeItems.Where(x => x.offerId == billDiscountOfferDc.OfferId).Select(x => new AngularJSAuthentication.DataContracts.External.RetailerBillDiscountFreeItemDc
                            {
                                ItemId = x.ItemId,
                                ItemName = x.ItemName,
                                Qty = x.Qty
                            }).ToList(),
                            OfferLineItemValueDcs = billDiscountOfferDc.OfferLineItemValueDcs != null && billDiscountOfferDc.OfferLineItemValueDcs.Any() ? billDiscountOfferDc.OfferLineItemValueDcs.Where(x => x.offerId == billDiscountOfferDc.OfferId).ToList() : new List<DataContracts.Masters.OfferLineItemValueDc>(),
                            BillDiscountRequiredItems = billDiscountOfferDc.BillDiscountRequiredItems != null && billDiscountOfferDc.BillDiscountRequiredItems.Any() ? billDiscountOfferDc.BillDiscountRequiredItems.Where(x => x.offerId == billDiscountOfferDc.OfferId).ToList() : new List<DataContracts.Masters.BillDiscountRequiredItemDc>(),
                        };
                        if (billDiscountOfferDc.MaxDiscount > 0)
                        {
                            bdcheck.MaxDiscount = billDiscountOfferDc.MaxDiscount;
                        }
                        else if (billDiscountOfferDc.MaxBillAmount > 0)
                        {
                            bdcheck.MaxDiscount = billDiscountOfferDc.MaxBillAmount * billDiscountOfferDc.DiscountPercentage / 100;
                        }
                        else
                        {
                            bdcheck.MaxDiscount = billDiscountOfferDc.MaxDiscount;
                        }
                        if (bdcheck.BillDiscountOfferOn == "FreeItem" && bdcheck.RetailerBillDiscountFreeItemDcs.Any())
                            FinalBillDiscount.Add(bdcheck);
                        else
                            FinalBillDiscount.Add(bdcheck);


                    }
                }
                res = new DataContracts.External.OfferdataDc()
                {
                    offer = FinalBillDiscount,
                    Status = true,
                    Message = "Success"
                };
                return res;
            }
            //List<OfferBDDTO> BillDiscount = new List<OfferBDDTO>();
            //List<OfferBDDTO> FinalBillDiscount = new List<OfferBDDTO>();
            //ResDTOList res;
            //DateTime CurrentDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            //try
            //{
            //    using (AuthContext context = new AuthContext())
            //    {
            //        Customer Customer = context.Customers.Where(x => x.CustomerId == CustomerId && x.Deleted == false && x.Active == true).FirstOrDefault();
            //        if (Customer != null)
            //        {
            //            BillDiscount = context.OfferDb.Where(o => o.IsDeleted == false && o.WarehouseId == Customer.Warehouseid && o.start <= CurrentDate && o.end >= CurrentDate && o.IsActive == true
            //                            && (o.OfferAppType == "Sales App" || o.OfferAppType == "Both") && (o.OfferOn == "BillDiscount" || o.OfferOn == "ScratchBillDiscount"))
            //                             .Select(o => new OfferBDDTO
            //                             {

            //                                 OfferId = o.OfferId,
            //                                 CustomerId = Customer.CustomerId,
            //                                 WarehouseId = o.WarehouseId,
            //                                 OfferName = o.OfferName,
            //                                 OfferOn = o.OfferOn,
            //                                 OfferCategory = o.OfferCategory,
            //                                 Description = o.Description,
            //                                 start = o.start,
            //                                 end = o.end,
            //                                 DiscountPercentage = o.DiscountPercentage,
            //                                 IsActive = o.IsActive,
            //                                 IsDeleted = o.IsDeleted,
            //                                 CreatedDate = o.CreatedDate,
            //                                 UpdateDate = o.UpdateDate,
            //                                 OfferCode = o.OfferCode,
            //                                 BillDiscountOfferOn = o.BillDiscountOfferOn,
            //                                 BillDiscountWallet = o.BillDiscountWallet,
            //                                 BillAmount = o.BillAmount,
            //                                 LineItem = o.LineItem,
            //                                 MaxBillAmount = o.MaxBillAmount,
            //                                 IsMultiTimeUse = o.IsMultiTimeUse,
            //                                 IsUseOtherOffer = o.IsUseOtherOffer,
            //                                 BillDiscountType = o.BillDiscountType,
            //                                 Category = o.Category,
            //                                 subCategory = o.subCategory,
            //                                 subSubCategory = o.subSubCategory,
            //                                 MaxDiscount = o.MaxDiscount,
            //                                 OfferUseCount = o.OfferUseCount,
            //                                 ApplyOn = o.ApplyOn,
            //                                 WalletType = o.WalletType,
            //                                 OfferItems = o.OfferItemsBillDiscounts.Select(x => new OfferItem
            //                                 {
            //                                     IsInclude = x.IsInclude,
            //                                     itemId = x.itemId
            //                                 }).ToList(),
            //                                 OfferBillDiscountItems = o.BillDiscountOfferSections.Select(x => new OfferBillDiscountItem
            //                                 {
            //                                     IsInclude = x.IsInclude,
            //                                     Id = x.ObjId
            //                                 }).ToList()
            //                             }).ToList();

            //            if (BillDiscount.Count() > 0)
            //            {
            //                //check if offer not used 
            //                foreach (var bdcheck in BillDiscount)
            //                {
            //                    if (bdcheck.OfferOn == "BillDiscount")
            //                    {
            //                        string query = "";
            //                        if (bdcheck.BillDiscountType == "subcategory" && bdcheck.OfferBillDiscountItems.Any())
            //                        {
            //                            var mappingIds = string.Join(",", bdcheck.OfferBillDiscountItems.Select(x => x.Id).ToList());

            //                            query = "select distinct d.Categoryid,a.[SubCategoryId],b.SubCategoryMappingId MappingId from SubCategories a inner join SubcategoryCategoryMappings b on a.SubCategoryid=b.subCategoryid inner join Categories d on b.Categoryid=d.Categoryid and a.IsActive=1 and b.IsActive =1 and a.Deleted=0 and b.Deleted=0 and d.IsActive=1 and d.Deleted=0 and b.SubCategoryMappingId in (" + mappingIds + ")";
            //                            var offerCatSubCats = context.Database.SqlQuery<OfferCatSubCat>(query).ToList();
            //                            foreach (var item in bdcheck.OfferBillDiscountItems)
            //                            {
            //                                if (offerCatSubCats.Any(x => x.MappingId == item.Id))
            //                                {
            //                                    var offerCatSubCat = offerCatSubCats.FirstOrDefault(x => x.MappingId == item.Id);
            //                                    item.Id = offerCatSubCat.SubCategoryId.Value;
            //                                    item.CategoryId = offerCatSubCat.Categoryid.Value;
            //                                }
            //                            }
            //                        }
            //                        else if (bdcheck.BillDiscountType == "brand" && bdcheck.OfferBillDiscountItems.Any())
            //                        {
            //                            var mappingIds = string.Join(",", bdcheck.OfferBillDiscountItems.Select(x => x.Id).ToList());

            //                            query = "select distinct d.SubCategoryId, e.Categoryid,a.[SubsubCategoryid],b.BrandCategoryMappingId MappingId"
            //                                            + " from SubsubCategories a inner"
            //                                            + " join BrandCategoryMappings b on a.SubsubCategoryid = b.SubsubCategoryId"
            //                                            + " inner join SubcategoryCategoryMappings c on c.SubCategoryMappingId = b.SubCategoryMappingId"
            //                                            + " inner join SubCategories d on d.SubCategoryId = c.SubCategoryId"
            //                                            + " inner join Categories e on e.Categoryid = c.Categoryid"
            //                                            + " where a.IsActive = 1 and b.IsActive = 1 and a.Deleted = 0 and b.Deleted = 0 and d.IsActive = 1 and d.Deleted = 0 and c.IsActive = 1 " +
            //                                            " and c.Deleted = 0 and e.IsActive = 1 and e.Deleted = 0 and b.BrandCategoryMappingId in (" + mappingIds + ")";
            //                            var offerCatSubCats = context.Database.SqlQuery<OfferCatSubCat>(query).ToList();
            //                            foreach (var item in bdcheck.OfferBillDiscountItems)
            //                            {
            //                                if (offerCatSubCats.Any(x => x.MappingId == item.Id))
            //                                {
            //                                    var offerCatSubCat = offerCatSubCats.FirstOrDefault(x => x.MappingId == item.Id);
            //                                    item.Id = offerCatSubCat.SubSubCategoryId.Value;
            //                                    item.SubCategoryId = offerCatSubCat.SubCategoryId.Value;
            //                                    item.CategoryId = offerCatSubCat.Categoryid.Value;
            //                                }
            //                            }
            //                        }
            //                    }

            //                    List<BillDiscount> founds = context.BillDiscountDb.Where(x => x.CustomerId == CustomerId && x.OfferId == bdcheck.OfferId).ToList();
            //                    if (founds != null && founds.Any())
            //                    {
            //                        if (bdcheck.OfferOn == "ScratchBillDiscount")
            //                        {
            //                            if (founds.FirstOrDefault().OrderId > 0)
            //                            {
            //                                //BillDiscount.Remove(bdcheck); offer used already
            //                            }
            //                            else if (founds.FirstOrDefault().IsScratchBDCode)
            //                            {
            //                                bdcheck.IsScratchBDCode = true;
            //                                FinalBillDiscount.Add(bdcheck);
            //                            }
            //                            else
            //                            {
            //                                bdcheck.IsScratchBDCode = false;
            //                                FinalBillDiscount.Add(bdcheck);
            //                            }
            //                        }
            //                        else
            //                        {
            //                            if (bdcheck.OfferOn == "BillDiscount" && bdcheck.IsMultiTimeUse)
            //                            {
            //                                if (!bdcheck.OfferUseCount.HasValue)
            //                                    FinalBillDiscount.Add(bdcheck);
            //                                else if (bdcheck.OfferUseCount.Value > founds.Count())
            //                                    FinalBillDiscount.Add(bdcheck);
            //                            }
            //                        }
            //                    }
            //                    else if (bdcheck.OfferOn == "BillDiscount")
            //                    {
            //                        FinalBillDiscount.Add(bdcheck);
            //                    }

            //                }
            //                res = new ResDTOList()
            //                {
            //                    BillDiscount = FinalBillDiscount,
            //                    Status = true,
            //                    Message = "Success"
            //                };
            //                return Request.CreateResponse(HttpStatusCode.OK, res);
            //            }
            //        }
            //    }
            //    res = new ResDTOList()
            //    {
            //        BillDiscount = null,
            //        Status = false,
            //        Message = "fail"
            //    };
            //    return Request.CreateResponse(HttpStatusCode.OK, res);
            //}
            //catch (Exception ee)
            //{

            //    res = new ResDTOList()
            //    {
            //        BillDiscount = null,
            //        Status = false,
            //        Message = ("something isse occurs : " + ee)
            //    };
            //    return Request.CreateResponse(HttpStatusCode.OK, res);
            //}
        }
        #endregion

        #region Check Agent Offer By Id (04-10-2019)
        /// Check Agent Offer By Id (Offer is used or not or offer available for multi use)
        /// </summary>
        /// <param name="OfferId"></param>
        /// <param name="CustomerId"></param>
        /// <param name="PeopleId"></param>
        /// <returns></returns>
        [Route("CheckOfferForAgent")]
        [HttpGet]
        public HttpResponseMessage CheckOfferForAgent(string OfferId, int CustomerId, int? PeopleId, int WarehouseId)
        {
            List<int> OfferIds = OfferId.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            List<OfferBDDTO> BillDiscount = new List<OfferBDDTO>();
            List<OfferValidation> OfferValidations = new List<OfferValidation>();
            try
            {
                DateTime CurrentDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                using (AuthContext context = new AuthContext())
                {

                    int? warehouseId = context.Customers.Where(x => x.CustomerId == CustomerId && x.Deleted == false && x.Active == true).Select(x => x.Warehouseid).FirstOrDefault();
                    List<BillDiscount> BillDiscountUsed = context.BillDiscountDb.Where(x => x.CustomerId == CustomerId && OfferIds.Contains(x.OfferId) && x.IsActive == true && x.IsDeleted == false).ToList();
                    if (warehouseId != null && warehouseId > 0)
                    {
                        BillDiscount = (from o in context.OfferDb
                                        where OfferIds.Contains(o.OfferId)
                                        select new OfferBDDTO
                                        {
                                            OfferId = o.OfferId,
                                            OfferOn = o.OfferOn,
                                            start = o.start,
                                            end = o.end,
                                            IsActive = o.IsActive,
                                            IsDeleted = o.IsDeleted,
                                            IsMultiTimeUse = o.IsMultiTimeUse,
                                            OfferUseCount = o.OfferUseCount,
                                            OfferAppType = o.OfferAppType
                                        }).ToList();
                        // check  customer used id or not  && o.start <= CurrentDate && o.end >= CurrentDate

                        foreach (var item in BillDiscount)
                        {
                            bool valid = true;
                            string message = "";
                            if (item.OfferOn == "ScratchBillDiscount" && BillDiscountUsed.All(y => y.OfferId == item.OfferId && y.OrderId > 0))
                            {
                                valid = false;
                                message = "You have already taken this scratch card";
                            }
                            else if (item.OfferOn != "ScratchBillDiscount" && BillDiscountUsed.Any(y => y.OfferId == item.OfferId))
                            {
                                item.OfferUseCount = item.OfferUseCount.HasValue ? item.OfferUseCount.Value : (item.IsMultiTimeUse ? 1000 : 1);
                                if ((item.OfferUseCount.Value <= BillDiscountUsed.Count(x => x.OfferId == item.OfferId && x.OrderId > 0)))
                                {
                                    valid = false;
                                    message = "You have already taken this offer.";
                                }
                            }

                            if (valid && !(item.start <= CurrentDate && item.end >= CurrentDate))
                            {
                                valid = false;
                                message = (item.OfferOn == "ScratchBillDiscount" ? "Scratch Card " : "Offer ") + "expired.";
                            }

                            if (valid && !(item.IsActive && !item.IsDeleted))
                            {
                                valid = false;
                                message = (item.OfferOn == "ScratchBillDiscount" ? "Scratch Card " : "Offer ") + "no more available.";
                            }

                            OfferValidations.Add(new OfferValidation { Message = message, Valid = valid, OfferId = item.OfferId });
                        }

                    }
                    var res = new
                    {
                        BillDiscount = OfferValidations,
                        Status = OfferValidations.All(x => x.Valid)
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }
            catch (Exception ee)
            {

                var res = new
                {
                    BillDiscount = OfferValidations,
                    Status = false,
                    Message = ("something isse occurs : " + ee)
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        #endregion


        #region Get all Active Offer Sales App
        /// <returns></returns>
        [Route("GetAllOfferSalesApp")]
        [HttpGet]
        //[AllowAnonymous]
        public DataContracts.External.OfferdataDc GetAllOfferSalesApp(int PeopleId, int WarehouseId, int CustomerId)
        {

            List<AngularJSAuthentication.DataContracts.External.OfferDc> FinalBillDiscount = new List<AngularJSAuthentication.DataContracts.External.OfferDc>();
            AngularJSAuthentication.DataContracts.External.OfferdataDc res;
            using (AuthContext context = new AuthContext())
            {
                var offertypeConfigs = context.OfferTypeDefaultConfigs.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                
                CustomersManager manager = new CustomersManager();
                var query = string.Format("exec IsSalesAppLead {0}", PeopleId);
                var isSalesLead = context.Database.SqlQuery<int>(query).FirstOrDefault();
                List<long> storeids = new List<long>();
                if (isSalesLead > 0)
                    storeids = context.StoreDB.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.Id).ToList();
                else
                {
                    storeids = context.ClusterStoreExecutives.Where(x => x.ExecutiveId == PeopleId && x.IsDeleted == false && x.IsActive).Select(x => x.StoreId).Distinct().ToList();

                    var universalStoreIds = context.StoreDB.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.IsUniversal).Select(x => x.Id).ToList();
                    if (universalStoreIds != null && universalStoreIds.Any())
                        storeids.AddRange(universalStoreIds);
                }
                List<AngularJSAuthentication.DataContracts.Masters.BillDiscountOfferDc> billDiscountOfferDcs = manager.GetAllOfferSalesApp(WarehouseId, CustomerId, "Sales App");
                var offerIds = billDiscountOfferDcs.Where(x => x.BillDiscountOfferOn == "FreeItem").Select(x => x.OfferId).ToList();
                List<BillDiscountFreeItem> BillDiscountFreeItems = offerIds.Any() ? context.BillDiscountFreeItem.Where(x => offerIds.Contains(x.offerId) && x.RemainingOfferStockQty < x.OfferStockQty).ToList() : new List<BillDiscountFreeItem>();
                if (billDiscountOfferDcs.Any())
                {
                    billDiscountOfferDcs = billDiscountOfferDcs.Where(x => x.BillDiscountType != "ClearanceStock" && (storeids.Contains(x.StoreId) || x.StoreId == 0) && x.ApplyType != "PrimeCustomer").ToList();

                    foreach (var billDiscountOfferDc in billDiscountOfferDcs)
                    {
                        var OfferDefaultdata = billDiscountOfferDc.OfferOn == "Item" ? offertypeConfigs.Where(x => x.OfferType == billDiscountOfferDc.OfferOn && x.IsActive == true && x.IsDeleted == false).FirstOrDefault()
                            : offertypeConfigs.Where(x => x.OfferType == billDiscountOfferDc.OfferOn && x.DiscountOn == billDiscountOfferDc.BillDiscountOfferOn && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                        OfferDefaultdata = OfferDefaultdata != null ? OfferDefaultdata : new Model.SalesApp.OfferTypeDefaultConfig();
                        var bdcheck = new AngularJSAuthentication.DataContracts.External.OfferDc
                        {
                            OfferId = billDiscountOfferDc.OfferId,
                            MaxBillAmount = billDiscountOfferDc.MaxBillAmount,
                            OfferName = billDiscountOfferDc.OfferName,
                            OfferCode = billDiscountOfferDc.OfferCode,
                            OfferCategory = billDiscountOfferDc.OfferCategory,
                            OfferOn = billDiscountOfferDc.OfferOn,
                            start = billDiscountOfferDc.start,
                            end = billDiscountOfferDc.end,
                            DiscountPercentage = billDiscountOfferDc.DiscountPercentage,
                            BillAmount = billDiscountOfferDc.BillAmount,
                            LineItem = billDiscountOfferDc.LineItem,
                            Description = billDiscountOfferDc.Description,
                            BillDiscountOfferOn = billDiscountOfferDc.BillDiscountOfferOn,
                            BillDiscountWallet = billDiscountOfferDc.BillDiscountWallet,
                            IsMultiTimeUse = billDiscountOfferDc.IsMultiTimeUse,
                            IsUseOtherOffer = billDiscountOfferDc.IsUseOtherOffer,
                            IsScratchBDCode = billDiscountOfferDc.IsScratchBDCode,
                            BillDiscountType = billDiscountOfferDc.BillDiscountType,
                            OfferAppType = billDiscountOfferDc.OfferAppType,
                            ApplyOn = billDiscountOfferDc.ApplyOn,
                            WalletType = billDiscountOfferDc.WalletType,
                            MaxDiscount = billDiscountOfferDc.MaxDiscount,
                            ColorCode = !string.IsNullOrEmpty(billDiscountOfferDc.ColorCode) ? billDiscountOfferDc.ColorCode : OfferDefaultdata.ColorCode,
                            ImagePath = !string.IsNullOrEmpty(billDiscountOfferDc.ImagePath) ? billDiscountOfferDc.ImagePath : OfferDefaultdata.ImagePath,
                            IsBillDiscountFreebiesItem = billDiscountOfferDc.IsBillDiscountFreebiesItem,
                            IsBillDiscountFreebiesValue = billDiscountOfferDc.IsBillDiscountFreebiesValue,
                            offerminorderquantity = billDiscountOfferDc.offerminorderquantity,
                            offeritemname = billDiscountOfferDc.offeritemname,
                            OfferBillDiscountItems = billDiscountOfferDc.OfferBillDiscountItems != null && billDiscountOfferDc.OfferBillDiscountItems.Any() ? billDiscountOfferDc.OfferBillDiscountItems.Select(y => new DataContracts.Masters.OfferBillDiscountItemDc
                            {
                                CategoryId = y.CategoryId,
                                Id = y.Id,
                                IsInclude = y.IsInclude,
                                SubCategoryId = y.SubCategoryId
                            }).ToList() : new List<DataContracts.Masters.OfferBillDiscountItemDc>(),
                            OfferItems = billDiscountOfferDc.OfferItems != null && billDiscountOfferDc.OfferItems.Any() ? billDiscountOfferDc.OfferItems.Select(y => new DataContracts.External.OfferItemdc
                            {
                                IsInclude = y.IsInclude,
                                itemId = y.itemId
                            }).ToList() : new List<DataContracts.External.OfferItemdc>(),
                            RetailerBillDiscountFreeItemDcs = BillDiscountFreeItems != null && BillDiscountFreeItems.Any() ? BillDiscountFreeItems.Where(x => x.offerId == billDiscountOfferDc.OfferId).Select(x => new DataContracts.External.RetailerBillDiscountFreeItemDc
                            {
                                ItemId = x.ItemId,
                                ItemName = x.ItemName,
                                Qty = x.Qty
                            }).ToList() : new List<DataContracts.External.RetailerBillDiscountFreeItemDc>(),
                            OfferLineItemValueDcs = billDiscountOfferDc.OfferLineItemValueDcs != null && billDiscountOfferDc.OfferLineItemValueDcs.Any() ? billDiscountOfferDc.OfferLineItemValueDcs.Where(x => x.offerId == billDiscountOfferDc.OfferId).ToList() : new List<DataContracts.Masters.OfferLineItemValueDc>(),
                            BillDiscountRequiredItems = billDiscountOfferDc.BillDiscountRequiredItems != null && billDiscountOfferDc.BillDiscountRequiredItems.Any() ? billDiscountOfferDc.BillDiscountRequiredItems.Where(x => x.offerId == billDiscountOfferDc.OfferId).ToList() : new List<DataContracts.Masters.BillDiscountRequiredItemDc>(),
                        };
                        if (billDiscountOfferDc.MaxDiscount > 0)
                        {
                            bdcheck.MaxDiscount = billDiscountOfferDc.MaxDiscount;
                        }
                        else if (billDiscountOfferDc.MaxBillAmount > 0)
                        {
                            bdcheck.MaxDiscount = billDiscountOfferDc.MaxBillAmount * billDiscountOfferDc.DiscountPercentage / 100;
                        }
                        else
                        {
                            bdcheck.MaxDiscount = billDiscountOfferDc.MaxDiscount;
                        }
                        FinalBillDiscount.Add(bdcheck);
                    }
                }
                res = new DataContracts.External.OfferdataDc()
                {
                    offer = FinalBillDiscount,
                    Status = true,
                    Message = "Success"
                };
                return res;
            }

        }
        #endregion



    }

    // response class with Discount list
    public class ResDTOList
    {
        public List<OfferBDDTO> BillDiscount { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    //response class with Discount object
    public class ResDTO
    {
        public OfferBDDTO BillDiscount { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class OfferBDDTO
    {
        public bool IsCRMOffer { get; set; }

        public int OfferId { get; set; }
        public int WarehouseId { get; set; }
        public int CustomerId { get; set; }
        public int PeopleId { get; set; }
        public string OfferName { get; set; }
        public string OfferCode { get; set; }
        public string OfferCategory
        {
            get; set;
        }
        public string OfferOn { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public double DiscountPercentage
        {
            get; set;
        }
        public double BillAmount
        {
            get; set;
        }

        public int LineItem { get; set; }


        public double MaxBillAmount
        {
            get; set;
        }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string BillDiscountOfferOn
        {
            get; set;
        }
        public double? BillDiscountWallet
        {
            get; set;
        }
        public bool IsMultiTimeUse { get; set; }
        public bool IsUseOtherOffer { get; set; }
        public bool IsScratchBDCode { get; set; } //if not scratch or done
        public int? Category { get; set; }
        public int? subCategory { get; set; }

        public int? subSubCategory { get; set; }
        public string BillDiscountType { get; set; }
        public double MaxDiscount { get; set; }
        public int? OfferUseCount { get; set; }
        public string OfferAppType { get; set; }
        public string ApplyOn { get; set; }
        public string WalletType { get; set; }
        public int itemId { get; set; }
        public string itemname { get; set; }
        public bool DistributorOfferType { get; set; }
        public int? OffersaleQty { get; set; }
        public decimal DistributorDiscountAmount { get; set; }
        public decimal DistributorDiscountPercentage { get; set; }
        public List<OfferBillDiscountItem> OfferBillDiscountItems { get; set; }

        public List<OfferItem> OfferItems { get; set; }
        public List<SlapOfferDC> slapOfferDCs { get; set; }

        public List<PostItemOfferDC> postItemOfferDCs { get; set; }
        public List<DataContracts.External.RetailerBillDiscountFreeItemDc> RetailerBillDiscountFreeItemDcs { get; set; }
    }

    public class OfferItem
    {
        public int itemId { get; set; }
        public bool IsInclude { get; set; }
    }

    public class OfferBillDiscountItem
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public bool IsInclude { get; set; }
    }

    public class OfferValidation
    {
        public int OfferId { get; set; }
        public bool Valid { get; set; }
        public string Message { get; set; }
    }

    public class SlapOfferDC
    {
        public string BillDiscountOfferOn { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal WalletAmount { get; set; }
        public double OfferOnPrice { get; set; }

        public decimal DistributorDiscountPercentage { get; set; }

    }
    public class PostItemOfferDC
    {
        public int? OffersaleQty { get; set; }
        public int? OffersaleWeight { get; set; }
        public int? OffersaleAmount { get; set; }
        public decimal MaxDiscount { get; set; }
        public string FreeOfferType { get; set; }
        public string UOM { get; set; }



    }
}
