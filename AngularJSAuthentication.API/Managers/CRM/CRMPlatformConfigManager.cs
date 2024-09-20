using AngularJSAuthentication.DataContracts.CRM;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.CRM;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Managers.CRM
{
   
    public class CRMPlatformConfigManager
    {
        #region For CRM
        public async Task<List<CRMPlatformConfigListDc>> CRMPlatformConfigGetList()
        {
            using (var context = new AuthContext())
            {
                var List = context.Database.SqlQuery<CRMPlatformConfigListDc>("Exec CRMPlatformConfigGetList").ToList();
                return List;
            }

        }

        public async Task<bool> ActiveInactiveCRMTag(long Id,bool IsActive)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var context = new AuthContext())
            {

                var param = new SqlParameter("@Id", Id);
                var param1 = new SqlParameter("@IsActive", IsActive);
                var param2 = new SqlParameter("@UserId", userid);
                var List = await context.Database.ExecuteSqlCommandAsync("Exec ActiveIactiveCRMSp @Id,@IsActive,@UserId", param, param1, param2);
                return true;
            }
            return false;
        }

        public async Task<bool> InsertCRMPlatformConfig(InsertCRMPlatformConfigDc insertCRMPlatformConfigDc,int userid)
        {
           
            using (var context = new AuthContext())
            {   
                var crmdatas = context.CRMs.Where(x => x.Name == insertCRMPlatformConfigDc.Name && x.IsActive && x.IsDeleted == false).FirstOrDefault();
                if (crmdatas == null)
                {
                    AngularJSAuthentication.Model.CRM.CRM crmdata = new AngularJSAuthentication.Model.CRM.CRM();
                    crmdata.Name = insertCRMPlatformConfigDc.Name;
                    crmdata.IsDigital = insertCRMPlatformConfigDc.IsDigital;
                    crmdata.IsActive = true;
                    crmdata.IsDeleted = false;
                    crmdata.CreatedBy = userid;
                    crmdata.CreatedDate = DateTime.Now;
                    context.CRMs.Add(crmdata);
                    context.Commit();

                    if (insertCRMPlatformConfigDc.Details != null && insertCRMPlatformConfigDc.Details.Any())
                    {
                        foreach (var item in insertCRMPlatformConfigDc.Details)
                        {
                            CRMDetail cRMDetail = new CRMDetail
                            {
                                CreatedBy = userid,
                                CreatedDate = DateTime.Now,
                                CRMId = crmdata.Id,
                                CRMTags = item,
                                IsActive = true,
                                IsDeleted = false                              
                            };
                            context.CRMDetails.Add(cRMDetail);
                            context.Commit();
                        }                                               
                    }
                }
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateCRMPlatformConfig(InsertCRMPlatformConfigDc insertCRMPlatformConfigDc,int userid)
        {
            using (var context = new AuthContext())
            {

                var crmdata = context.CRMs.Where(x => x.Id == insertCRMPlatformConfigDc.Id).FirstOrDefault();
                if (crmdata != null)
                {
                    crmdata.Name = insertCRMPlatformConfigDc.Name;
                    crmdata.IsDigital = insertCRMPlatformConfigDc.IsDigital;
                    crmdata.IsActive = true;
                    crmdata.IsDeleted = false;
                    crmdata.ModifiedBy = userid;
                    crmdata.ModifiedDate = DateTime.Now;
                    context.Entry(crmdata).State = EntityState.Modified;
                    context.Commit();

                    string query = "delete CRMDetails where CRMId ="+ crmdata.Id;
                    context.Database.ExecuteSqlCommand(query);
                    if (insertCRMPlatformConfigDc.Details != null && insertCRMPlatformConfigDc.Details.Any())
                    {
                        foreach (var item in insertCRMPlatformConfigDc.Details)
                        {
                            CRMDetail cRMDetail = new CRMDetail
                            {
                                ModifiedBy = userid,
                                ModifiedDate = DateTime.Now,
                                CreatedBy = userid,
                                CreatedDate = DateTime.Now,
                                CRMId = crmdata.Id,
                                CRMTags = item,
                                IsActive = true,
                                IsDeleted = false
                            };
                            context.CRMDetails.Add(cRMDetail);
                            context.Commit();
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteCRMPlatformConfig(long Id , int userid)
        {            
            using (var context = new AuthContext())
            {
                var DataDelete = context.CRMs.Where(x => x.Id == Id).FirstOrDefault();
                DataDelete.IsActive = false;
                DataDelete.IsDeleted = true;
                context.Entry(DataDelete).State = EntityState.Modified;
                context.Commit();
                return true;
            }
            return false;  
        }

        public async Task<List<string>> GetCRMPlatformConfigById(int Id)
        {
            List<string> Editdata = new List<string>();
            using (var context = new AuthContext())
            {
                Editdata =  context.CRMs.Where(x => x.Id == Id).Select(x => x.Name).ToList();               
            }
                return Editdata;
        }

        #endregion

        #region For CRMPlatform

        public async Task<bool> InsertCRMPlatform(InsertCRMPlatformConfigDc insertCRMPlatformConfigDc, int userid)
        {
            using (var context = new AuthContext())
            {
                var crmPlatformdatas = context.CRMPlatforms.Where(x => x.Name == insertCRMPlatformConfigDc.Name && x.IsActive && x.IsDeleted == false).FirstOrDefault();
                if (crmPlatformdatas == null)
                {
                    AngularJSAuthentication.Model.CRM.CRMPlatform crmPlatform = new AngularJSAuthentication.Model.CRM.CRMPlatform();
                    crmPlatform.Name = insertCRMPlatformConfigDc.Name;
                    crmPlatform.IsActive = true;
                    crmPlatform.IsDeleted = false;
                    crmPlatform.CreatedBy = userid;
                    crmPlatform.CreatedDate = DateTime.Now;
                    context.CRMPlatforms.Add(crmPlatform);
                    context.Commit();                 
                }
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateCRMPlatform(InsertCRMPlatformConfigDc insertCRMPlatformConfigDc, int userid)
        {
            using (var context = new AuthContext())
            {
                var crmPlatformdatas = context.CRMPlatforms.Where(x =>x.Id == insertCRMPlatformConfigDc.Id).FirstOrDefault();
                if (crmPlatformdatas != null)
                {                 
                    crmPlatformdatas.Name = insertCRMPlatformConfigDc.Name;
                    crmPlatformdatas.IsActive = true;
                    crmPlatformdatas.IsDeleted = false;
                    crmPlatformdatas.ModifiedBy = userid;
                    crmPlatformdatas.ModifiedDate = DateTime.Now;
                    context.Entry(crmPlatformdatas).State = EntityState.Modified;
                    context.Commit();
                }
                return true;
            }
            return false;
        }

        public async Task<List<CRMPlatformListDc>> CRMPlatformGetList()
        {
            using (var context = new AuthContext())
            {
                string query = "select Id,Name,CreatedDate from CRMPlatforms where IsActive=1 and IsDeleted=0";

                var List = context.Database.SqlQuery<CRMPlatformListDc>(query).ToList();
                return List;
            }          
        }

        public async Task<bool> DeleteCRMPlatform(int Id , int userid)
        {
            using (var context = new AuthContext())
            {
                var DataDelete = context.CRMPlatforms.Where(x => x.Id == Id).FirstOrDefault();
                DataDelete.IsActive = false;
                DataDelete.IsDeleted = true;
                DataDelete.ModifiedBy = userid;
                context.Entry(DataDelete).State = EntityState.Modified;
                context.Commit();
                return true;
            }
            return false;
        }
        #endregion

        #region For CRMPlatformMapping

        public async Task<List<CRMPlatformMappingListDc>> CRMPlatformMappingGetList()
        {
            using (var context = new AuthContext())
            {
                var List = context.Database.SqlQuery<CRMPlatformMappingListDc>("Exec CRMPlatformMappingGetList").ToList();
                return List;
            }
        }

        public async Task<bool> ActiveInactiveCRMPlatformMapping(long CrmId,int CrmPlatformId, bool UpdateCrmPlatformMapping,int userid)
        {
            using (var context = new AuthContext())
            {

                var param = new SqlParameter("@CrmId", CrmId);
                var param1 = new SqlParameter("@CrmPlatformId", CrmPlatformId);
                var param2 = new SqlParameter("@UpdateCrmPlatformMapping", UpdateCrmPlatformMapping);
                var param3 = new SqlParameter("@userId", userid);
                var List = await context.Database.ExecuteSqlCommandAsync("Exec CRMPlatformMappingActiveInactive @CrmId,@CrmPlatformId,@UpdateCrmPlatformMapping,@userId", param, param1, param2, param3);
                return true;
            }
            return false;
        }

        #endregion
    }

}