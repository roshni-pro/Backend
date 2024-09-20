using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.Model;
using System;
using System.Linq;
using System.Transactions;

namespace AngularJSAuthentication.API.Helper
{
    public class MasterOwnerHelper
    {
        public MasterOwnerViewModel SaveMasterOwner(MasterOwnerViewModel masterOwnerViewModel, int userid)
        {
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Required, option))
            //using (var transactionScope = new TransactionScope())
            {
                try
                {
                    using (var authContext = new AuthContext())
                    {
                        if (masterOwnerViewModel.MasterObject.MasterId > 0)
                        {
                            var oldOwner = authContext.Masters.Where(x => x.MasterId == masterOwnerViewModel.MasterObject.MasterId).FirstOrDefault();
                            oldOwner.MasterName = masterOwnerViewModel.MasterObject.MasterName;
                            oldOwner.UpdatedBy = userid;
                            oldOwner.UpdatedDate = DateTime.Now;
                            authContext.Commit();
                        }
                        else
                        {
                            masterOwnerViewModel.MasterObject.UpdatedBy = userid;
                            masterOwnerViewModel.MasterObject.UpdatedDate = DateTime.Now;
                            if (masterOwnerViewModel.MasterObject.MasterId < 1)
                            {
                                masterOwnerViewModel.MasterObject.CreatedBy = userid;
                                masterOwnerViewModel.MasterObject.CreatedDate = DateTime.Now;
                            }
                            else
                            {
                                masterOwnerViewModel.MasterObject.IsActive = true;
                                masterOwnerViewModel.MasterObject.IsDeleted = false;
                            }


                            authContext.Masters.Add(masterOwnerViewModel.MasterObject);
                            authContext.Commit();
                        }



                        if (masterOwnerViewModel.MasterOwnerList != null && masterOwnerViewModel.MasterOwnerList.Count > 0)
                        {
                            foreach (var masterOwner in masterOwnerViewModel.MasterOwnerList)
                            {
                                if (masterOwner.Id < 1)
                                {
                                    ExportMasterOwner masterOwner1 = new ExportMasterOwner();
                                    masterOwner1.MasterId = masterOwnerViewModel.MasterObject.MasterId;
                                    masterOwner1.MasterName = masterOwnerViewModel.MasterObject.MasterName;
                                    masterOwner1.IsActive = true;
                                    masterOwner1.IsDeleted = false;
                                    masterOwner1.CreatedBy = userid;
                                    masterOwner1.CreatedDate = DateTime.Now;
                                    masterOwner1.UpdatedBy = userid;
                                    masterOwner1.UpdatedDate = DateTime.Now;
                                    masterOwner1.ApproverId = masterOwner.ApproverId;
                                    authContext.MasterOwners.Add(masterOwner1);

                                }


                            }
                            authContext.Commit();
                        }
                    }
                    transactionScope.Complete();
                }
                catch (Exception ex)
                {
                    transactionScope.Dispose();
                    throw ex;
                }

            }
            return masterOwnerViewModel;
        }

        public MasterOwnerViewModel GetById(int masterId)
        {
            MasterOwnerViewModel vm = null;
            if (masterId > 0)
            {
                using (var authContext = new AuthContext())
                {
                    var master = authContext.Masters.Where(x => x.MasterId == masterId && x.IsDeleted == false && x.IsActive == true).FirstOrDefault();
                    if (master != null)
                    {
                        vm = new MasterOwnerViewModel();
                        vm.MasterObject = master;

                        var query = from mo in authContext.MasterOwners.Where(x => x.IsDeleted == false && x.IsActive == true)
                                    join p in authContext.Peoples
                                    on mo.ApproverId equals p.PeopleID
                                    where mo.MasterId == masterId
                                    select new ExportMasterOwnerDTO
                                    {
                                        ApproverId = mo.ApproverId,
                                        CreatedBy = mo.CreatedBy,
                                        CreatedDate = mo.CreatedDate,
                                        Field = (!string.IsNullOrEmpty(p.DisplayName) ? p.DisplayName : "")
                                                    + (!string.IsNullOrEmpty(p.Email) ? " - " + p.Email : "")
                                                    + (!string.IsNullOrEmpty(p.UserName) ? " - " + p.UserName : "")
                                                    + (!string.IsNullOrEmpty(p.Mobile) ? "(" + p.Mobile + ")" : ""),
                                        Id = mo.Id,
                                        IsActive = mo.IsActive,
                                        IsDeleted = mo.IsDeleted,
                                        MasterId = mo.MasterId,
                                        MasterName = mo.MasterName,
                                        UpdatedBy = mo.UpdatedBy,
                                        UpdatedDate = mo.UpdatedDate
                                    };
                        vm.MasterOwnerList = query.ToList();
                    }
                }
            }
            return vm;
        }

        public bool DeleteExportMaster(int exportMasterID, int userid)
        {
            using (var authContext = new AuthContext())
            {
                ExportMaster exportMaster = authContext.Masters.Where(x => x.MasterId == exportMasterID).FirstOrDefault();
                exportMaster.IsDeleted = true;
                exportMaster.IsActive = false;
                exportMaster.CreatedBy = userid;
                exportMaster.CreatedDate = DateTime.Now;
                authContext.Commit();
            }
            return true;
        }

        public bool DeleteExportMasterOwner(int exportMasterOwnerID, int userid)
        {
            using (var authContext = new AuthContext())
            {
                ExportMasterOwner exportMasterOwner = authContext.MasterOwners.Where(x => x.Id == exportMasterOwnerID).FirstOrDefault();
                exportMasterOwner.IsDeleted = true;
                exportMasterOwner.IsActive = false;
                exportMasterOwner.CreatedBy = userid;
                exportMasterOwner.CreatedDate = DateTime.Now;
                authContext.Commit();
            }
            return true;
        }
    }
}