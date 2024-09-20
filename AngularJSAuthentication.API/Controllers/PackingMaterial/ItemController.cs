
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.API.App_Code.PackingMaterial;
using System.Security.Claims;
using AngularJSAuthentication.BusinessLayer.PackingMaterial.BO;

namespace AngularJSAuthentication.API.Controllers.PackingMaterial
{
    [RoutePrefix("api/Item")]
    public class ItemController : ApiController
    {
        private ItemMasterRepository ItemMasterRepository;
        private static DateTime CurrentDatetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
        public ItemController()
        {
            this.ItemMasterRepository = new ItemMasterRepository(new AuthContext());
        }

        public ItemController(ItemMasterRepository ItemMasterRepository)
        {
            this.ItemMasterRepository = ItemMasterRepository;
        }
        [HttpGet]
        [Route("GetItemDetails")]
        public HttpResponseMessage GetItemDetails(int key,int WarehouseId)
        {
            try
            {
                ItemMaster ItemMaster = ItemMasterRepository.GetRawItemMaster(key, WarehouseId);
                if(ItemMaster != null) { 
                MaterialItemMaster MaterialItemMaster = ItemMasterRepository.GetMaterialMaster(ItemMaster.ItemNumber);
                PKItemDTO ObjPKItemDTO = new PKItemDTO
                {
                    ItemMaster = ItemMaster,
                    MaterialItemMaster = MaterialItemMaster
                };
                    return Request.CreateResponse(HttpStatusCode.OK, ObjPKItemDTO);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "");
                }
               
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());
            }
        }


       

    }
}
