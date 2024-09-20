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
    public class CategoryJsonController : ApiController
    {

        // GET: api/CategoryJson
        public IQueryable<Category> GetCategorys()
        {
            using (AuthContext db = new AuthContext())
            {

                var Category = from a in db.Categorys.Include("subCategory.subSubCategory.itemMaster") select a;

                return Category;
            }
        }

        //public IQueryable<ItemMaster> GetItems(int id)
        //{
        //    var Category = from a in db.Categorys.Include("subCategory.subSubCategory.itemMaster") select a;

        //    var items=
        //}
        // GET: api/CategoryJson/5
        [ResponseType(typeof(Category))]
        public IHttpActionResult GetCategory(int id)
        {
            using (AuthContext db = new AuthContext())
            {

                Category category = db.Categorys.Find(id);
                if (category == null)
                {
                    return NotFound();
                }

                return Ok(category);
            }
        }

        // PUT: api/CategoryJson/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutCategory(int id, Category category)
        {
            using (AuthContext db = new AuthContext())
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != category.Categoryid)
                {
                    return BadRequest();
                }

                db.Entry(category).State = EntityState.Modified;

                try
                {
                    db.Commit();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(id))
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

        // POST: api/CategoryJson
        [ResponseType(typeof(Category))]
        public IHttpActionResult PostCategory(Category category)
        {
            using (AuthContext db = new AuthContext())
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                db.Categorys.Add(category);
                db.Commit();

                return CreatedAtRoute("DefaultApi", new { id = category.Categoryid }, category);
            }
        }

        // DELETE: api/CategoryJson/5
        [ResponseType(typeof(Category))]
        public IHttpActionResult DeleteCategory(int id)
        {
            using (AuthContext db = new AuthContext())
            {

                Category category = db.Categorys.Find(id);
                if (category == null)
                {
                    return NotFound();
                }

                db.Categorys.Remove(category);
                db.Commit();

                return Ok(category);
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

        private bool CategoryExists(int id)
        {
            using (AuthContext db = new AuthContext())
            {

                return db.Categorys.Count(e => e.Categoryid == id) > 0;
            }
        }
    }
}