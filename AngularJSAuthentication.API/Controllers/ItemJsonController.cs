using AngularJSAuthentication.Model;
using NLog;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    public class ItemJsonController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

       
        // GET: api/ItemJson
        public IQueryable<ItemMaster> GetitemMasters()
        {
            using (var db = new AuthContext())
            {
                logger.Info("Start GetitemMasters : ");
                //.Where(i=>i.warehouse_id.Equals(6))
                return db.itemMasters;
            }
        }

        public List<Itemsappdata> GetitemMasters(int id)
        {
            //.Where(i=>i.warehouse_id.Equals(6))
            //return db.itemMasters.Where(i => i.warehouse_id.Equals(id));
            //new 2-12-15
            // List<Itemsappdata> Itemslist= new List<Itemsappdata>();
            using (var db = new AuthContext())
            {
                var Itemslist = (from a in db.itemMasters
                                 where a.WarehouseId == id
                                 select new Itemsappdata
                                 {
                                     //ItemId = a.ItemId,
                                     //warehouse_id = a.warehouse_id,
                                     //WarehouseName = a.WarehouseName,
                                     //SupplierId = a.SupplierId,
                                     //itemcode = a.itemcode,
                                     //itemname = a.itemname,
                                     //Cityid = a.Cityid,
                                     //CityName = a.CityName,
                                     //PramotionalDiscount = a.PramotionalDiscount,
                                     //TotalTaxPercentage = a.TotalTaxPercentage,
                                     //LogoUrl = a.LogoUrl,
                                     //price = a.price,
                                     //UnitPrice = a.UnitPrice,
                                     //MinOrderQty = a.MinOrderQty,
                                     //Categoryid = a.Categoryid,
                                     CatLogoUrl = a.CatLogoUrl,
                                     CategoryName = a.CategoryName,
                                     SubCategoryId = a.SubCategoryId,
                                     SubcategoryName = a.SubcategoryName,
                                     SubsubCategoryid = a.SubsubCategoryid,
                                     SubsubcategoryName = a.SubsubcategoryName,
                                 }
                                   ).ToList();
                return Itemslist;
            }
        }
        // GET: api/ItemJson/5
        //[ResponseType(typeof(ItemMaster))]
        //public IHttpActionResult GetItemMaster(int id)
        //{
        //    ItemMaster itemMaster = db.itemMasters.Find(id);
        //    if (itemMaster == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(itemMaster);
        //}
        // PUT: api/ItemJson/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutItemMaster(int id, ItemMaster itemMaster)
        {
            using (var db = new AuthContext())
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != itemMaster.ItemId)
                {
                    return BadRequest();
                }

                db.Entry(itemMaster).State = EntityState.Modified;

                try
                {
                    db.Commit();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemMasterExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return StatusCode(HttpStatusCode.NoContent);
            }
        }

        // POST: api/ItemJson
        [ResponseType(typeof(ItemMaster))]
        public IHttpActionResult PostItemMaster(ItemMaster itemMaster)
        {
            using (var db = new AuthContext())
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                db.itemMasters.Add(itemMaster);
                db.Commit();

                return CreatedAtRoute("DefaultApi", new { id = itemMaster.ItemId }, itemMaster);
            }
        }

        // DELETE: api/ItemJson/5
        [ResponseType(typeof(ItemMaster))]
        public IHttpActionResult DeleteItemMaster(int id)
        {
            using (var db = new AuthContext())
            {

                ItemMaster itemMaster = db.itemMasters.Find(id);
                if (itemMaster == null)
                {
                    return NotFound();
                }

                db.itemMasters.Remove(itemMaster);
                db.Commit();

                return Ok(itemMaster);
            }
        }

        protected override void Dispose(bool disposing)
        {
            using (var db = new AuthContext())
            {
                if (disposing)
                {
                    db.Dispose();
                }
                base.Dispose(disposing);
            }
        }

        private bool ItemMasterExists(int id)
        {
            using (var db = new AuthContext())
            {
                return db.itemMasters.Count(e => e.ItemId == id) > 0;
            }
        }
    }
}