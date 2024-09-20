using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.Model;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    public class ItemMastersController : BaseAuthController
    {
      
        // GET: api/ItemMasters
        public IQueryable<ItemMaster> GetitemMasters()
        {
            using (var db = new AuthContext())
            {
                return db.itemMasters;
            }
        }
        // GET: api/ItemMasters/5
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
        // PUT: api/ItemMasters/5
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
        // POST: api/ItemMasters
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
        // DELETE: api/ItemMasters/5
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