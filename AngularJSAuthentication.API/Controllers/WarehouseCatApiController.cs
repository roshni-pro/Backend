using AngularJSAuthentication.Model;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    public class WarehouseCatApiController : ApiController
    {
        // GET: api/WarehouseCatApi
        public IQueryable<WarehouseCategory> GetDbWarehouseCategory()
        {
            using (AuthContext db = new AuthContext())
            {
                var items = from i in db.DbWarehouseCategory.Include("ItemMaster") select i;
                return items;
            }
            //return db.DbWarehouseCategory;
        }

        // GET: api/WarehouseCatApi/5
        [ResponseType(typeof(WarehouseCategory))]
        public IHttpActionResult GetWarehouseCategory(int id)
        {
            using (AuthContext db = new AuthContext())
            {
                WarehouseCategory warehouseCategory = db.DbWarehouseCategory.Find(id);
                if (warehouseCategory == null)
                {
                    return NotFound();
                }

                return Ok(warehouseCategory);
            }
        }

        // PUT: api/WarehouseCatApi/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutWarehouseCategory(int id, WarehouseCategory warehouseCategory)
        {
            using (AuthContext db = new AuthContext())
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != warehouseCategory.WhCategoryid)
                {
                    return BadRequest();
                }

                db.Entry(warehouseCategory).State = EntityState.Modified;

                try
                {
                    db.Commit();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WarehouseCategoryExists(id))
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

        // POST: api/WarehouseCatApi
        [ResponseType(typeof(WarehouseCategory))]
        public IHttpActionResult PostWarehouseCategory(WarehouseCategory warehouseCategory)
        {
            using (AuthContext db = new AuthContext())
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                db.DbWarehouseCategory.Add(warehouseCategory);
                db.Commit();

                return CreatedAtRoute("DefaultApi", new { id = warehouseCategory.WhCategoryid }, warehouseCategory);
            }
        }

        // DELETE: api/WarehouseCatApi/5
        [ResponseType(typeof(WarehouseCategory))]
        public IHttpActionResult DeleteWarehouseCategory(int id)
        {
            using (AuthContext db = new AuthContext())
            {
                WarehouseCategory warehouseCategory = db.DbWarehouseCategory.Find(id);
                if (warehouseCategory == null)
                {
                    return NotFound();
                }

                db.DbWarehouseCategory.Remove(warehouseCategory);
                db.Commit();

                return Ok(warehouseCategory);
            }
        }

        protected override void Dispose(bool disposing)
        {
            using (AuthContext db = new AuthContext())
            {
                if (disposing)
                {
                    db.Dispose();
                }
                base.Dispose(disposing);
            }
        }

        private bool WarehouseCategoryExists(int id)
        {
            using (AuthContext db = new AuthContext())
            {
                return db.DbWarehouseCategory.Count(e => e.WhCategoryid == id) > 0;
            }
        }
    }
}