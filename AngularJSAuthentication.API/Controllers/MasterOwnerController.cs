using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Models;
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
    [RoutePrefix("api/MasterOwner")]
    public class MasterOwnerController : BaseAuthController
    {
        
        private static Logger logger = LogManager.GetCurrentClassLogger();

        int compid = 0, userid = 0;

        public MasterOwnerController()
        {
            var identity = User.Identity as ClaimsIdentity;
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

        }


        [Route("GetMasterList")]
        public List<ExportMaster> Getdetail()
        {
            using (var db = new AuthContext())
            {
               
                try
                {
                    using (var authContext = new AuthContext())
                    {
                        var query = authContext.Masters.Where(x => x.IsDeleted == false).Select(x => x).OrderByDescending(x => x.MasterId).ToList();
                        return query;
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }

            }
        }
        [Route("GetMastersList")]
        public IEnumerable<ExportMaster> Getdetails()
        {
            using (var db = new AuthContext())
            {
               
                List<ExportMaster> ass = new List<ExportMaster>();
                try
                {
                    logger.Info("User ID : {0} , Company Id : {1} Get Sactions");
                    ass = db.Masters.Where(a => a.IsDeleted == false).ToList();
                    logger.Info("End MasterList: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in MasterList " + ex.Message);
                    logger.Info("End  MasterList: ");
                    return null;
                }
            }
        }



        [HttpGet]
        [Route("GetMasterbyId")]
        public ExportMasterOwner GetMasterbyId(int ID)
        {

            using (var db = new AuthContext())
            {
               
                ExportMasterOwner ass = new ExportMasterOwner();
                try
                {
                    
                    ass = db.MasterOwners.Where(a => a.IsDeleted == false && a.Id == ID).SingleOrDefault();

                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in AppHomeDynamic " + ex.Message);
                    logger.Info("End  AppHomeDynamic: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(ExportMasterOwner))]
        [Route("")]
        [AcceptVerbs("POST")]
        public ExportMasterOwner addMaster(ExportMasterOwner item)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    ExportMasterOwner Master = new ExportMasterOwner();
                    item.IsActive = true;

                    db.MasterOwners.Add(item);
                    db.Commit();


                    {

                        {
                            Master = db.MasterOwners.Where(x => x.MasterId == item.MasterId && x.Id == item.Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                            if (Master != null)
                            {


                                Master.Id = item.Id;
                                Master.MasterId = item.MasterId;

                                Master.CreatedBy = item.CreatedBy;
                                Master.CreatedDate = Convert.ToDateTime(item.CreatedDate);
                                Master.UpdatedDate = Convert.ToDateTime(item.UpdatedDate);
                                //db.itemMasters.Attach(itemMaster);

                                db.Commit();
                            }

                        }

                    }


                    return item;
                }
                catch (Exception ee)
                {
                    return null;
                }
            }
        }

        [Route("SaveMasterOwner")]
        [AcceptVerbs("POST")]
        public MasterOwnerViewModel SaveMasterOwner(MasterOwnerViewModel vm)
        {
            int userid = 0;
            MasterOwnerHelper helper = new MasterOwnerHelper();
            return helper.SaveMasterOwner(vm, userid);
        }

        [Route("GetByExportMasterId/{exportMasterOwnerId}")]
        [HttpGet]
        public MasterOwnerViewModel GetByExportMasterId(int exportMasterOwnerId)
        {
            MasterOwnerHelper helper = new MasterOwnerHelper();
            MasterOwnerViewModel vm = helper.GetById(exportMasterOwnerId);
            return vm;
        }

        [Route("DeleteExportMaster/{exportMasterId}")]
        [HttpGet]
        public bool DeleteExportMaster(int exportMasterId)
        {
            MasterOwnerHelper helper = new MasterOwnerHelper();
            bool result = helper.DeleteExportMaster(exportMasterId, userid);
            return result;
        }

        [Route("DeleteExportMasterOwner/{exportMasterOwnerId}")]
        [HttpGet]
        public bool DeleteExportMasterOwner(int exportMasterOwnerId)
        {
            MasterOwnerHelper helper = new MasterOwnerHelper();
            bool result = helper.DeleteExportMasterOwner(exportMasterOwnerId, userid);
            return result;
        }



        [Route("update")]
        [HttpPut]
        public IHttpActionResult update(mastersDTO item)
        {
            using (AuthContext context = new AuthContext())
            {

                People people = context.Peoples.Where(c => c.PeopleID.Equals(item.ApproverId) && c.Deleted == false).FirstOrDefault();
                ExportMasterOwner master = context.MasterOwners.Where(c => c.MasterId.Equals(item.MasterId) && c.IsDeleted == false).FirstOrDefault();

                //var Masterss = context.MasterOwners.Where(c => c.MasterName == item.MasterName).FirstOrDefault();
                if (master != null)
                {
                    master.MasterName = item.MasterName;
                    master.MasterId = item.MasterId;
                    master.IsActive = item.IsActive;
                    master.ApproverId = item.ApproverId;
                    master.Id = item.Id;
                    master.UpdatedDate = item.UpdatedDate;
                    // context.Entry(Masterss).State = EntityState.Modified;
                    context.Commit();
                    return Ok(master);

                }
                else
                {
                    return Ok("Data Already Exist");
                }

            }


        }



        [ResponseType(typeof(ExportMasterOwner))]
        [Route("Delete")]
        [AcceptVerbs("PUT")]
        public ExportMasterOwner DeleteOwner(ExportMasterOwner item)
        {
            using (var db = new AuthContext())
            {
                ExportMasterOwner Master = new ExportMasterOwner();
                try
                {
                    ExportMasterOwner ahd = db.MasterOwners.Where(a => a.Id == item.Id).SingleOrDefault();
                    if (ahd != null)
                    {
                        ahd.IsDeleted = true;

                        db.Commit();



                        {
                            Master = db.MasterOwners.Where(x => x.MasterId == item.MasterId && x.Id == item.Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                            if (Master != null)
                            {


                                Master.Id = item.Id;
                                Master.MasterId = item.MasterId;

                                Master.CreatedBy = item.CreatedBy;
                                Master.CreatedDate = Convert.ToDateTime(item.CreatedDate);
                                Master.UpdatedDate = Convert.ToDateTime(item.UpdatedDate);
                                //db.itemMasters.Attach(itemMaster);

                                db.Commit();
                            }

                        }

                    }

                    return item;
                }
                catch (Exception ee)
                {
                    return null;
                }
            }
        }



        public class mastersDTO
        {
            public int Id { get; set; }
            public string MasterName { get; set; }
            public int MasterId { get; set; }
            public int ApproverId { get; set; }
            public string ApproverName { get; set; }
            public bool IsActive { get; set; }
            public int CreatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public int UpdatedBy { get; set; }
            public DateTime UpdatedDate { get; set; }
            public bool IsDeleted { get; set; }
        }
    }
}
