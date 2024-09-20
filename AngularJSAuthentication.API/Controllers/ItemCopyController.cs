using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    public class ItemCopyController : ApiController
    {
       

        // GET: api/ItemCopy
        public IQueryable<ItemMaster> GetitemMasters()
        {
            using (var db = new AuthContext())
            {
                return db.itemMasters;
            }
        }

        // GET: api/ItemCopy/5
        [ResponseType(typeof(ItemMaster))]
        public IHttpActionResult GetItemMaster(int id)
        {
            using (var db = new AuthContext())
            {
                ItemMaster itemMaster = db.itemMasters.Find(id);
                if (itemMaster == null)
                {
                    return NotFound();
                }

                return Ok(itemMaster);
            }
        }

        // POST: api/ItemCopy/5
        [ResponseType(typeof(void))]
        public List<MoveWarehouse> PostItemMaster(List<MoveWarehouse> item)
        {
            using (var db = new AuthContext())
            {
                int Warehid = item[0].WarehouseId;


                try
                {

                    //item.CompanyId = compid;
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }

                    db.AddItemMove(item, Warehid);

                    return item;

                }
                catch (Exception ex)
                {


                    return null;
                }



            }
        }



        // DELETE: api/ItemCopy/5
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