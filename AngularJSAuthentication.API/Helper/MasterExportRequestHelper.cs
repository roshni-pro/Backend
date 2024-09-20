using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.DataContracts.Transaction.MasterExport;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Linq;

namespace AngularJSAuthentication.API.Helper
{
    public class MasterExportRequestHelper
    {
        private Logger logger;

        public MasterExportRequestHelper()
        {
            logger = LogManager.GetCurrentClassLogger();
        }
        public bool UpdateStatus(MasterExportRequestDC masterExportRequest)
        {
            try
            {
                using (var authContext = new AuthContext())
                {
                    MasterExportRequest request = authContext.MasterExportRequestDB.FirstOrDefault(x => x.Id == masterExportRequest.Id);
                    if (request != null)
                    {
                        request.ApproverId = masterExportRequest.ApproverId;
                        request.IsEmailSent = masterExportRequest.IsEmailSent;
                        request.IsGenerated = masterExportRequest.IsGenerated;
                        request.IsVerified = masterExportRequest.IsVerified;
                        request.MarkAsBriefcase = masterExportRequest.MarkAsBriefcase;
                        request.MasterId = masterExportRequest.MasterId;
                        request.Parameter = masterExportRequest.Parameter;
                        request.ParameterToShow = masterExportRequest.ParameterToShow;
                        request.Path = masterExportRequest.Path;
                        request.RequestedUserId = masterExportRequest.RequestedUserId;
                        request.VerifiedDate = masterExportRequest.VerifiedDate;
                    }
                    authContext.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in get all  " + ex.Message);
                logger.Info("End UpdateStatus : ");
                return false;
            }
        }



        public MasterExportRequestOutput GetList(MasterExportRequestPaginator paginator)
        {
            using (var authContext = new AuthContext())
            {
                var fromDate = paginator.FromDate.HasValue ? paginator.FromDate.Value.Date : paginator.FromDate;
                var toDate = paginator.ToDate.HasValue ? paginator.ToDate.Value.Date.AddDays(1).AddSeconds(-1) : paginator.ToDate;

                var query = from me in authContext.MasterExportRequestDB
                            join user in authContext.Peoples
                            on me.RequestedUserId equals user.PeopleID
                            join approver in authContext.Peoples
                            on me.ApproverId equals approver.PeopleID
                            where (!paginator.FromDate.HasValue || paginator.ToDate.HasValue) || (me.CreatedDate >= fromDate && me.CreatedDate <= toDate)
                                   && (!paginator.RequesterID.HasValue || paginator.RequesterID == me.RequestedUserId)
                                   && (!paginator.ApproverID.HasValue || paginator.ApproverID == me.ApproverId)
                                   && (string.IsNullOrEmpty(paginator.Contains) || user.Email.Contains(paginator.Contains)
                                            || user.UserName.Contains(paginator.Contains) || approver.Email.Contains(paginator.Contains)
                                            || approver.UserName.Contains(paginator.Contains))
                            select (new MasterExportRequestDC
                            {
                                Id = me.Id,
                                MasterId = me.MasterId,
                                CreatedDate = me.CreatedDate,
                                IsVerified = me.IsVerified,
                                Path = me.Path,
                                IsGenerated = me.IsGenerated,
                                IsEmailSent = me.IsEmailSent,
                                RequestedUserId = me.RequestedUserId,
                                VerifiedDate = me.VerifiedDate,
                                ApproverId = me.ApproverId,
                                Parameter = me.Parameter,
                                ParameterToShow = me.ParameterToShow,
                                MarkAsBriefcase = me.MarkAsBriefcase,
                                ApproverName = approver.DisplayName,
                                RequesterName = user.DisplayName,
                                GeneratedDate = me.CreatedDate
                            });

                MasterExportRequestOutput outpot = new MasterExportRequestOutput();
                outpot.Count = query.Count();
                outpot.MasterExportRequestListData = query.OrderByDescending(x => x.Id).Skip(paginator.Skip).Take(paginator.Take).ToList();
                return outpot;

            }

        }
    }
}