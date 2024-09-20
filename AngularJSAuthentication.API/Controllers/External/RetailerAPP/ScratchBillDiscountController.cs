using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.BillDiscount;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.External.RetailerAPP
{
    [RoutePrefix("api/ScratchBillDiscountOfferApp")]
    public class ScratchBillDiscountController : BaseAuthController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        public static Logger logger = LogManager.GetCurrentClassLogger();


        #region all Active Bill Discount Offer On App
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
            List<OfferBDScratchDTO> BillDiscount = new List<OfferBDScratchDTO>();
            List<OfferBDScratchDTO> BillDiscountFinal = new List<OfferBDScratchDTO>();
            ResScratchDTOList res;
            DateTime CurrentDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    Customer Customer = context.Customers.Where(x => x.CustomerId == CustomerId && x.Deleted == false && x.Active == true).FirstOrDefault();
                    if (Customer != null)
                    {
                        BillDiscount = (from o in context.OfferDb
                                        where o.IsDeleted == false && o.WarehouseId == Customer.Warehouseid && o.start <= CurrentDate && o.end >= CurrentDate && o.IsActive == true && o.IsDeleted == false && o.OfferOn == "ScratchBillDiscount"

                                        select new OfferBDScratchDTO
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
                            //check if IsScratchBDCode used to customer
                            foreach (var bdcheck in BillDiscount)
                            {
                                BillDiscount found = context.BillDiscountDb.Where(x => x.CustomerId == CustomerId && x.OfferId == bdcheck.OfferId).FirstOrDefault();
                                if (found != null)
                                {
                                    if (found.IsScratchBDCode && found.OrderId > 0)
                                    {
                                        //BillDiscount.Remove(bdcheck);
                                    }
                                    else if (found.IsScratchBDCode)
                                    {
                                        bdcheck.IsScratchBDCode = true;
                                        BillDiscountFinal.Add(bdcheck);
                                    }
                                    else
                                    {
                                        bdcheck.IsScratchBDCode = false;
                                        BillDiscountFinal.Add(bdcheck);
                                    }

                                }
                            }
                            res = new ResScratchDTOList()
                            {
                                ScratchBillDiscount = BillDiscountFinal,
                                Status = true,
                                Message = "success"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                    res = new ResScratchDTOList()
                    {
                        ScratchBillDiscount = BillDiscountFinal,
                        Status = false,
                        Message = "fail"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }
            catch (Exception ee)
            {

                res = new ResScratchDTOList()
                {
                    ScratchBillDiscount = BillDiscount,
                    Status = false,
                    Message = ("something isse occurs : " + ee)
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        #endregion

        #region Check Customer Offer
        /// <summary>
        /// Check Customer Offer By Id (Offer is used or not or offer available for multi use)
        /// </summary>
        /// <param name="OfferId"></param>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        [Route("CheckScratchOffer")]
        [HttpGet]
        public HttpResponseMessage CheckCustomerScratchOfferById(int OfferId, int CustomerId)
        {
            OfferBDScratchDTO BillDiscount = new OfferBDScratchDTO();
            ResScratchDTO res;
            try
            {
                DateTime CurrentDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                using (AuthContext context = new AuthContext())
                {
                    Customer Customer = context.Customers.Where(x => x.CustomerId == CustomerId && x.Deleted == false && x.Active == true).FirstOrDefault();
                    List<BillDiscount> BillDiscountUsed = context.BillDiscountDb.Where(x => x.CustomerId == CustomerId && x.OfferId == OfferId).ToList();
                    if (Customer != null)
                    {
                        BillDiscount = (from o in context.OfferDb
                                        where o.IsDeleted == false && o.WarehouseId == Customer.Warehouseid && o.start <= CurrentDate && o.end >= CurrentDate && o.IsActive == true && o.IsDeleted == false && o.OfferOn == "ScratchBillDiscount" && o.OfferId == OfferId
                                        select new OfferBDScratchDTO
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
                                        }).FirstOrDefault();
                        if (BillDiscount != null)
                        {
                            //check if IsScratchBDCode used to customer
                            BillDiscount found = context.BillDiscountDb.Where(x => x.CustomerId == CustomerId && x.OfferId == BillDiscount.OfferId).FirstOrDefault();
                            if (found.IsScratchBDCode && found.OrderId > 0)
                            {
                                BillDiscount = new OfferBDScratchDTO();
                                res = new ResScratchDTO()
                                {
                                    ScratchBillDiscount = BillDiscount,
                                    Status = false,
                                    Message = "Scratch Code Already Used"
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                            else if (found.IsScratchBDCode && found.OrderId == 0)
                            {
                                BillDiscount.IsScratchBDCode = true;
                                res = new ResScratchDTO()
                                {
                                    ScratchBillDiscount = BillDiscount,
                                    Status = true,
                                    Message = "Scratch Code Already Open"
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                            else
                            {
                                BillDiscount.IsScratchBDCode = false;
                                res = new ResScratchDTO()
                                {
                                    ScratchBillDiscount = BillDiscount,
                                    Status = true,
                                    Message = "Scratch Code"
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                        }
                    }
                    res = new ResScratchDTO()
                    {
                        ScratchBillDiscount = BillDiscount,
                        Status = false,
                        Message = "fail : due to offer scratch code expired"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }
            catch (Exception ee)
            {

                res = new ResScratchDTO()
                {
                    ScratchBillDiscount = BillDiscount,
                    Status = false,
                    Message = ("something isse occurs : " + ee)
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }
        #endregion

        #region Check Customer Offer
        /// <summary>
        /// Check Customer Offer By Id (Offer is used or not or offer available for multi use)
        /// </summary>
        /// <param name="OfferId"></param>
        /// <param name="CustomerId"></param>
        /// /// <param name="IsScartched"></param>
        /// <returns></returns>
        [Route("UpdateScratchOfferById")]
        [HttpPut]
        public HttpResponseMessage UpdateCustomerScratchOfferById(int OfferId, int CustomerId, bool IsScartched)
        {
            OfferBDScratchDTO offer = new OfferBDScratchDTO();
            ResScratchDTO res;
            try
            {
                DateTime CurrentDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                using (AuthContext context = new AuthContext())
                {
                    Customer Customer = context.Customers.Where(x => x.CustomerId == CustomerId ).FirstOrDefault();
                    if (Customer != null)
                    {
                        var dbOffers = context.OfferDb.Where(o => o.IsDeleted == false && o.WarehouseId == Customer.Warehouseid && o.start <= CurrentDate && o.end >= CurrentDate && o.IsActive == true && o.IsDeleted == false && o.OfferOn == "ScratchBillDiscount" && o.OfferId == OfferId).Include(x => x.OfferScratchWeights).ToList();
                        offer = dbOffers.Select(o => new OfferBDScratchDTO
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
                        }).FirstOrDefault();
                        if (offer != null)
                        {
                            double billAmount = 0;
                            if (dbOffers.FirstOrDefault().BillDiscountOfferOn == "DynamicWalletPoint")
                            {
                                List<int> ScratchPoints = new List<int>();
                                WeightedRandomBag<int> itemDrops = new WeightedRandomBag<int>();
                                foreach (var item in dbOffers.FirstOrDefault().OfferScratchWeights)
                                {
                                    itemDrops.AddEntry(item.WalletPoint, item.Weight);
                                }
                                billAmount = itemDrops.GetRandom();

                            }
                            //check if IsScratchBDCode used to customer
                            BillDiscount found = context.BillDiscountDb.Where(x => x.CustomerId == CustomerId && x.OfferId == offer.OfferId  && x.OrderId==0).FirstOrDefault();
                            if (found != null)
                            {
                                if (!found.IsScratchBDCode)
                                {
                                    found.IsScratchBDCode = IsScartched;
                                    found.ModifiedDate = CurrentDate;
                                    found.ModifiedBy = Customer.CustomerId;
                                    context.Entry(found).State = EntityState.Modified;
                                    context.Commit();

                                    offer.IsScratchBDCode = found.IsScratchBDCode;
                                    res = new ResScratchDTO()
                                    {
                                        ScratchBillDiscount = offer,
                                        Status = true,
                                        Message = "Code Scratch Successfully"
                                    };
                                    
                                }
                                else
                                {
                                    res = new ResScratchDTO()
                                    {
                                        ScratchBillDiscount = offer,
                                        Status = true,
                                        Message = "Already Scratch."
                                    };
                                }
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                            else
                            {
                                BillDiscount billDiscount = new BillDiscount();

                                billDiscount.CustomerId = Customer.CustomerId;
                                billDiscount.OrderId = 0;
                                billDiscount.OfferId = offer.OfferId;
                                billDiscount.BillDiscountType = offer.OfferOn;                               
                                if (offer.OfferOn == "ScratchBillDiscount")
                                {
                                    billDiscount.BillDiscountTypeValue = billAmount;//// scratch amount
                                }
                                billDiscount.BillDiscountAmount = 0;
                                billDiscount.IsMultiTimeUse = offer.IsMultiTimeUse;
                                billDiscount.IsUseOtherOffer = offer.IsUseOtherOffer;
                                billDiscount.CreatedDate = CurrentDate;
                                billDiscount.ModifiedDate = CurrentDate;
                                billDiscount.IsActive = offer.IsActive;
                                billDiscount.IsDeleted = false;
                                billDiscount.CreatedBy = 0;
                                billDiscount.ModifiedBy = 0;
                                billDiscount.IsScratchBDCode = IsScartched;//scratched or not
                                context.BillDiscountDb.Add(billDiscount);
                                context.Commit();
                                res = new ResScratchDTO()
                                {
                                    ScratchBillDiscount = offer,
                                    Status = true,
                                    Message = "Code Scratch Successfully"
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, res);

                            }

                            //    res = new ResScratchDTO()
                            //    {
                            //        ScratchBillDiscount = BillDiscount,
                            //        Status = false,
                            //        Message = "some thing went wrong"
                            //    };
                            //    return Request.CreateResponse(HttpStatusCode.OK, res);
                            //}

                        }
                    }
                    res = new ResScratchDTO()
                    {
                        ScratchBillDiscount = offer,
                        Status = false,
                        Message = "fail : due to offer scratch code expired"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }
            catch (Exception ee)
            {

                res = new ResScratchDTO()
                {
                    ScratchBillDiscount = offer,
                    Status = false,
                    Message = ("something isse occurs : " + ee)
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        #endregion

        #region Agent app Update Scratch Offer(04-10-2019)
        /// <summary>
        /// Check Agent Offer By Id (Offer is used or not or offer available for multi use)
        /// </summary>
        /// <param name="OfferId"></param>
        /// <param name="CustomerId"></param>
        /// <param name="IsScartched"></param>
        /// <param name="PeopleId"></param>
        /// <param name="WarehouseId"></param>
        /// <returns></returns>
        [Route("AgentUpdateScratchOfferById")]
        [HttpPut]
        public HttpResponseMessage AgentUpdateScratchOfferById(int OfferId, int CustomerId, bool IsScartched, int? PeopleId, int WarehouseId)
        {
            OfferBDScratchDTO offer = new OfferBDScratchDTO();
            ResScratchDTO res;
            try
            {
                DateTime CurrentDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                using (AuthContext context = new AuthContext())
                {

                    Customer Customer = context.Customers.Where(x => x.CustomerId == CustomerId && x.Deleted == false && x.Active == true).FirstOrDefault();
                    if (Customer != null)
                    {
                        var dbOffers = context.OfferDb.Where(o => o.IsDeleted == false && o.WarehouseId == Customer.Warehouseid && o.start <= CurrentDate && o.end >= CurrentDate && o.IsActive == true && o.IsDeleted == false && o.OfferOn == "ScratchBillDiscount" && o.OfferId == OfferId).Include(x => x.OfferScratchWeights).ToList();
                        offer = dbOffers.Select(o => new OfferBDScratchDTO
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
                            ApplyType=o.ApplyType
                        }).FirstOrDefault();
                        if (offer != null)
                        {

                            double billAmount = 0;
                            if (offer.BillDiscountOfferOn == "DynamicWalletPoint")
                            {
                                List<int> ScratchPoints = new List<int>();
                                WeightedRandomBag<int> itemDrops = new WeightedRandomBag<int>();
                                foreach (var item in dbOffers.FirstOrDefault().OfferScratchWeights)
                                {
                                    itemDrops.AddEntry(item.WalletPoint, item.Weight);
                                }
                                billAmount = itemDrops.GetRandom();

                            }
                            else if (offer.BillDiscountOfferOn == "WalletPoint")
                            {
                                billAmount = offer.BillDiscountWallet.Value;
                            }
                            else if (offer.BillDiscountOfferOn == "Percentage")
                            {
                                billAmount = dbOffers.FirstOrDefault().MaxDiscount;
                            }
                            //check if IsScratchBDCode used to customer
                            BillDiscount found = context.BillDiscountDb.Where(x => x.CustomerId == CustomerId && x.OfferId == offer.OfferId && x.OrderId == 0).FirstOrDefault();
                            if (found != null)
                            {
                                if (!found.IsScratchBDCode)
                                {
                                    found.IsScratchBDCode = IsScartched;
                                    found.ModifiedDate = CurrentDate;
                                    found.ModifiedBy = Customer.CustomerId;
                                    context.Entry(found).State = EntityState.Modified;
                                    context.Commit();

                                    offer.BillDiscountWallet = found.BillDiscountAmount;
                                    if (offer.OfferOn == "ScratchBillDiscount")
                                    {
                                        offer.BillDiscountWallet = found.BillDiscountTypeValue;
                                    }
                                    offer.IsScratchBDCode = found.IsScratchBDCode;
                                    res = new ResScratchDTO()
                                    {
                                        ScratchBillDiscount = offer,
                                        Status = true,
                                        Message = "Code Scratch Successfully"
                                    };
                                }
                                else
                                {
                                    res = new ResScratchDTO()
                                    {
                                        ScratchBillDiscount = offer,
                                        Status = true,
                                        Message = "Already Scratch."
                                    };
                                }
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                            else
                            {
                                BillDiscount billDiscount = new BillDiscount();

                                billDiscount.CustomerId = Customer.CustomerId;
                                billDiscount.OrderId = 0;
                                billDiscount.OfferId = offer.OfferId;
                                billDiscount.BillDiscountType = offer.OfferOn;
                                if (offer.OfferOn == "ScratchBillDiscount")
                                {
                                    billDiscount.BillDiscountTypeValue = billAmount;//// scratch amount
                                }
                                else
                                {
                                    billDiscount.BillDiscountAmount = billAmount;
                                }
                                billDiscount.IsMultiTimeUse = offer.IsMultiTimeUse;
                                billDiscount.IsUseOtherOffer = offer.IsUseOtherOffer;
                                billDiscount.CreatedDate = CurrentDate;
                                billDiscount.ModifiedDate = CurrentDate;
                                billDiscount.IsActive = offer.IsActive;
                                billDiscount.IsDeleted = false;
                                billDiscount.CreatedBy = 0;
                                billDiscount.ModifiedBy = 0;
                                billDiscount.IsScratchBDCode = IsScartched;//scratched or not
                                context.BillDiscountDb.Add(billDiscount);
                                context.Commit();

                                offer.BillDiscountWallet = billAmount;
                               
                                res = new ResScratchDTO()
                                {
                                    ScratchBillDiscount = offer,
                                    Status = true,
                                    Message = "Code Scratch Successfully"
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, res);

                            }

                        }
                    }
                    res = new ResScratchDTO()
                    {
                        ScratchBillDiscount = offer,
                        Status = false,
                        Message = "fail : due to offer scratch code expired"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }
            catch (Exception ee)
            {

                res = new ResScratchDTO()
                {
                    ScratchBillDiscount = offer,
                    Status = false,
                    Message = ("something isse occurs : " + ee)
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }
        #endregion

    }

    // response class with Discount list
    public class ResScratchDTOList
    {
        public List<OfferBDScratchDTO> ScratchBillDiscount { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    //response class with Discount object
    public class ResScratchDTO
    {
        public OfferBDScratchDTO ScratchBillDiscount { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class OfferBDScratchDTO
    {

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

        public bool IsScratchBDCode { get; set; }//if not scratch or done
        public string OfferAppType { get; set; }
        public string ApplyType { get; set; }

    }
}
