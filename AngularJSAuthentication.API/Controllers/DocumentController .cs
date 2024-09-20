using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Documents")]
    public class DocumentController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
       

        [Route("GetDocument")]
        public IEnumerable<DocumentList> Get()
        {
            logger.Info("Get Document: ");
            int compid = 0, userid = 0;
            int Warehouse_id = 0;
            using (AuthContext context = new AuthContext())
            {

                try
                {
                    var identity = User.Identity as ClaimsIdentity;

                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }

                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }
                    logger.Info("End Get Company: ");
                    if (Warehouse_id > 0)
                    {
                        List<DocumentList> document = context.AllDocumentWid(compid, Warehouse_id).ToList();
                        return document;
                    }
                    else
                    {
                        List<DocumentList> document = context.AllDocument(compid).ToList();
                        return document;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting Peoples " + ex.Message);
                    return null;
                }
            }


        }

        //////////[Route("DocumentData")]
        //////////[AcceptVerbs("POST")]
        //////////public int DocumentData(DocumentList aj)
        //////////{
        //////////    using (AuthContext context = new AuthContext())
        //////////    {

        //////////        try
        //////////        {
        //////////            var identity = User.Identity as ClaimsIdentity;
        //////////            int compid = 0, userid = 0; int Warehouse_id = 0;
        //////////            // Access claims
        //////////            foreach (Claim claim in identity.Claims)
        //////////            {
        //////////                if (claim.Type == "compid")
        //////////                {
        //////////                    compid = int.Parse(claim.Value);
        //////////                }
        //////////                if (claim.Type == "userid")
        //////////                {
        //////////                    userid = int.Parse(claim.Value);
        //////////                }
        //////////                if (claim.Type == "Warehouseid")
        //////////                {
        //////////                    Warehouse_id = int.Parse(claim.Value);
        //////////                }
        //////////            }
        //////////            int result = context.DocumentAdd(compid, Warehouse_id, aj);
        //////////            return 1;
        //////////        }
        //////////        catch (Exception ex)
        //////////        {
        //////////            logger.Error("Error in Add Peoples " + ex.Message);
        //////////            return 0;
        //////////        }
        //////////    }
        //////////}temp comment
        ///


        [Route("DocumentData")]
        [AcceptVerbs("POST")]
        public IHttpActionResult DocumentData(DocumentList aj)
        {
            using (AuthContext context = new AuthContext())
            {

                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0; int Warehouse_id = 0;
                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }

                    if (context.IsDocExists(aj))
                    {
                        return Ok("Already Exists");
                    }
                    else
                    {
                        context.DocumentAdd(compid, Warehouse_id, aj);
                        return Ok(aj);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add Peoples " + ex.Message);
                    return InternalServerError();
                }
            }
        }



        //[ResponseType(typeof(DocumentList))]
        //[Route("put")]
        //[AcceptVerbs("PUT")]
        //public DocumentData Put(DocumentList item)
        //{
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        foreach (Claim claim in identity.Claims)
        //        {
        //            if (claim.Type == "compid")
        //            {
        //                compid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "userid")
        //            {
        //                userid = int.Parse(claim.Value);
        //            }
        //        }
        //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //        return context.PutPeople(item);
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}


        //[ResponseType(typeof(DocumentList))]
        //[Route("put")]
        //[AcceptVerbs("PUT")]
        //public DocumentList Put(DocumentList item)
        //{
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        foreach (Claim claim in identity.Claims)
        //        {
        //            if (claim.Type == "compid")
        //            {
        //                compid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "userid")
        //            {
        //                userid = int.Parse(claim.Value);
        //            }
        //        }
        //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //        return context.PutPeople(item);
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        ////////[Route("PutTemp")]
        ////////[HttpPut]
        ////////public IHttpActionResult PutTemp(DocumentList sl)
        ////////{
        ////////    if (!ModelState.IsValid)
        ////////        return BadRequest("Not a valid model");

        ////////    using (var ctx = new AuthContext())
        ////////    {
        ////////        var ExistingDocument = ctx.DocumentLists.Where(s => s.DocumentId == sl.DocumentId)
        ////////                                                .FirstOrDefault<DocumentList>();

        ////////        if (ExistingDocument != null)
        ////////        {

        ////////            ExistingDocument.DocumentName = sl.DocumentName;
        ////////            ExistingDocument.DocumentId = sl.DocumentId;
        ////////            ExistingDocument.Doc_Point = sl.Doc_Point;

        ////////            ctx.SaveChanges();
        ////////        }
        ////////        else
        ////////        {
        ////////            return NotFound();
        ////////        }
        ////////    }

        ////////    return Ok();
        ////////}temp comment

        [Route("Put")]
        [HttpPut]
        public IHttpActionResult Put(DocumentList sl)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            using (var ctx = new AuthContext())
            {
                var ExistingDocument = ctx.DocumentLists.Where(s => s.DocumentId == sl.DocumentId)
                                                        .FirstOrDefault<DocumentList>();


                var ExistingDocuments = ctx.DocumentLists.Where(c => c.DocumentName == sl.DocumentName).FirstOrDefault();
                if (ExistingDocuments == null)
                {

                    ExistingDocument.DocumentName = sl.DocumentName;
                    ExistingDocument.DocumentId = sl.DocumentId;
                    ExistingDocument.Doc_Point = sl.Doc_Point;

                    ctx.Commit();
                    return Ok(ExistingDocument);
                }
                else
                {
                    //return NotFound();
                    return Ok("Data Already Exists!");
                }
            }


        }


        [ResponseType(typeof(DocumentList))]
        [Route("DeleteV7")]
        //[AcceptVerbs("Delete")]
        [HttpDelete]
        public Boolean DeleteV7(int id)
        {
            using (AuthContext context = new AuthContext())
            {

                logger.Info("start deleteDocument: ");
                try
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.DeleteDocument(id);
                    logger.Info("End  delete Document: ");
                    return true;
                }
                catch (Exception ex)
                {

                    logger.Error("Error in deleteDocument" + ex.Message);
                    return false;

                }
            }
        }



    }
}


//[ResponseType(typeof(Skill))]
//[Route("Delete")]
////[AcceptVerbs("Delete")]
//[HttpDelete]
//public void Remove(int id)
//{
//    logger.Info("start deleteSkill: ");
//    try
//    {

//        var identity = User.Identity as ClaimsIdentity;
//        int compid = 0, userid = 0;
//        foreach (Claim claim in identity.Claims)
//        {
//            if (claim.Type == "compid")
//            {
//                compid = int.Parse(claim.Value);
//            }
//            if (claim.Type == "userid")
//            {
//                userid = int.Parse(claim.Value);
//            }
//        }
//        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
//        context.DeleteSkill(id);
//        logger.Info("End  delete Skill: ");
//    }
//    catch (Exception ex)
//    {

//        logger.Error("Error in deleteSkill" + ex.Message);


//    }
//}








