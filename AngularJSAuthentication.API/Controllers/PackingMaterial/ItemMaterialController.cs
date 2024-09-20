using AngularJSAuthentication.API.App_Code.PackingMaterial;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.BusinessLayer.PackingMaterial.BO;
using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.PackingMaterial
{
    [RoutePrefix("api/ItemMaterial")]
   [Authorize]
    public class ItemMaterialController : ApiController
    {
        private ItemMaterialRepository ItemMaterialRepository;
        private static DateTime CurrentDatetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
        public ItemMaterialController()
        {
            this.ItemMaterialRepository = new ItemMaterialRepository(new AuthContext());
        }

        public ItemMaterialController(ItemMaterialRepository ItemMaterialRepository)
        {
            this.ItemMaterialRepository = ItemMaterialRepository;
        }

        [Route("GetItemMaterial")]
        [HttpGet]
        public HttpResponseMessage GetItemMaterial()
        {
            try
            {
                ItemMaterialResponse ItemMaterialResponse = ItemMaterialRepository.ItemMaterialResponse();
                return Request.CreateResponse(HttpStatusCode.OK, ItemMaterialResponse);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }
        }
        [Route("GetWarehouse")]
        [HttpGet]
        public HttpResponseMessage GetWarehouse(int CityId)
        {
            try
            {
                IEnumerable<Warehouse> Warehouse = ItemMaterialRepository.Warehouse(CityId);
                return Request.CreateResponse(HttpStatusCode.OK, Warehouse);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());

            }
        }

        [Route("GetBuyer")]
        [HttpGet]
        public HttpResponseMessage GetBuyer()
        {
            try
            {
                IEnumerable<Buyer> Buyer = ItemMaterialRepository.Buyer();
                return Request.CreateResponse(HttpStatusCode.OK, Buyer);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());

            }
        }
        [Route("CreateMaterialItemMaster")]
        [HttpPost]
        public HttpResponseMessage CreateMaterialItemMaster(MaterialItemMaster objMaterialitemMaster)
        {
            try
            {
               
                int UserID = GetUserId();
                objMaterialitemMaster.ItemMasterCentral.userid = UserID; 
                objMaterialitemMaster.CreatedBy = UserID; 
                objMaterialitemMaster.ItemMasterCentral.CompanyId = GetCompanyId();
                objMaterialitemMaster.BagId = 0;
                int UniqueNumber = GenerateUniqueNumber();
                objMaterialitemMaster.ItemNumber = objMaterialitemMaster.ItemMasterCentral.Number;
                bool Res = ItemMaterialRepository.InsertMaterialItemMaster(objMaterialitemMaster);
                return Request.CreateResponse(HttpStatusCode.OK, Res);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());

            }
        }

        [Route("GetItemMaterialDetails")]
        [HttpGet]
        public HttpResponseMessage GetItemMaterialDetails()
        {
            try
            {
                ItemMaterialResponse ItemMaterialResponse = ItemMaterialRepository.ItemMaterialResponse();
                return Request.CreateResponse(HttpStatusCode.OK, ItemMaterialResponse);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }
        }

        [Route("GetOuterBag")]
        [HttpGet]
        public HttpResponseMessage GetOuterBag()
        {
            try
            {
                IEnumerable<OuterBagMaster> ObjOuterBag = ItemMaterialRepository.OuterBagDetails();
                return Request.CreateResponse(HttpStatusCode.OK, ObjOuterBag);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }
        }

        [Route("GetBagDescription")]
        [HttpGet]
        public HttpResponseMessage GetBagDescription(int OuterBagId)
        {
            try
            {
                IEnumerable<BagDescriptionDTO> ObjBagDescription = ItemMaterialRepository.BagDetails(OuterBagId);
                return Request.CreateResponse(HttpStatusCode.OK, ObjBagDescription);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }
        }

        [Route("CreateMaterialItemDetails")]
        [HttpPost]
        public HttpResponseMessage CreateItemDetails(MaterialItemDetailsRequest objMaterialMaterialItemDetailsRequest)
        {
            try
            {
                int UserID = GetUserId();
                //int UniqueNumber = GenerateUniqueNumber();
                objMaterialMaterialItemDetailsRequest.MaterialItemDetails.ForEach(x =>
                {
                    x.CreatedBy = UserID;
                    x.CreatedDate = CurrentDatetime;
                    //x.BomId = UniqueNumber;
                    x.Number =  objMaterialMaterialItemDetailsRequest.ItemNumber;

                });




                //objMaterialItemDetails.ForEach(x => x.CreatedBy = UserID; x => x.CreatedDate = CurrentDatetime)
                //objMaterialItemDetails.ForEach(x => x.CreatedBy = UserID, x => x.CreatedDate = CurrentDatetime);
                //objMaterialItemDetails.ForEach(x => x.CreatedDate = CurrentDatetime);

                //objMaterialItemDetails.ForEach(x => x.ItemDetailId = UniqueNumber);
                int Res = ItemMaterialRepository.InsertMaterialItemDetails(objMaterialMaterialItemDetailsRequest.MaterialItemDetails, objMaterialMaterialItemDetailsRequest.ItemNumber);
                IEnumerable<MaterialItemDetails> objMaterialItemDetailsie = ItemMaterialRepository.GetAddedMaterialItemDetails(Res);
                return Request.CreateResponse(HttpStatusCode.OK, objMaterialItemDetailsie);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());

            }
        }
        [Route("GetAddedBomDetails")]
        [HttpGet]
        public HttpResponseMessage GetAddedBomDetails(int BomId)
        {
            try
            {
                IEnumerable<AddedBomMaterialDtls> ObjMaterialItemDetails = ItemMaterialRepository.AddedBomMaterialDtls(BomId);
                return Request.CreateResponse(HttpStatusCode.OK, ObjMaterialItemDetails);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.InnerException.ToString());
            }

        }
        [Route("GetBomNameDtls")]
        [HttpGet]
        public HttpResponseMessage GetBomName(int WareHouseId, int  ItemId)
        {
            try
            {
                IEnumerable<GetBomNameDtls> ObjGetBomNameDtls = ItemMaterialRepository.GetBomName(WareHouseId, ItemId);
                return Request.CreateResponse(HttpStatusCode.OK, ObjGetBomNameDtls);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.InnerException.ToString());
            }
        }
        [Route("GetMaterialMaster")]
        [HttpGet]
        public HttpResponseMessage GetMaterialMaster(string ItemNumber)
        {
            try
            {
              MaterialItemMaster ObjMaterialItemMaster = ItemMaterialRepository.Getmaterial(ItemNumber);
                return Request.CreateResponse(HttpStatusCode.OK, ObjMaterialItemMaster);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.InnerException.ToString());
            }
        }


        private int GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }

        private int GetCompanyId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int CompId = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                CompId = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
            return CompId;
        }


        public int GenerateUniqueNumber()
        {
            int number = Convert.ToInt32(String.Format("{0:d9}", (DateTime.Now.Ticks / 10) % 1000000000));
            return number;
        }
    }
}


