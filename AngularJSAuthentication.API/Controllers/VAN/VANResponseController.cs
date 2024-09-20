using AngularJSAuthentication.DataContracts.VAN;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;


namespace AngularJSAuthentication.API.Controllers.VAN
{
    [RoutePrefix("api/Payments")]
    public class VANResponseController : ApiController
    {
        [Route("GetVANResponse")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<VANReturnResponse> GetVANResponse(VANResponse response)
        {

            VANReturnResponse returnResponse = new VANReturnResponse();
            string skCode = "";
            if (response != null && response.GenericCorporateAlertRequest != null && response.GenericCorporateAlertRequest.Any())
            {
                string replacetext = ConfigurationManager.AppSettings["VANPrefix"].ToString();
                string benefDetail = !string.IsNullOrEmpty(response.GenericCorporateAlertRequest.FirstOrDefault().BenefDetails2) ? response.GenericCorporateAlertRequest.FirstOrDefault().BenefDetails2.ToUpper() : "";
                skCode = benefDetail.Replace(replacetext, "");
            }
            using (var context = new AuthContext())
            {
                if (!string.IsNullOrEmpty(skCode))
                {
                    var alertSequenceNos = response.GenericCorporateAlertRequest.Select(x => x.AlertSequenceNo).FirstOrDefault();
                    var vanResponsedbs = response.GenericCorporateAlertRequest.Select(x => new Model.VAN.VANResponse
                    {
                        Accountnumber = x.Accountnumber,
                        AlertSequenceNo = x.AlertSequenceNo,
                        Amount = x.Amount,
                        BenefDetails2 = x.BenefDetails2,
                        ChequeNo = x.ChequeNo,
                        CreatedBy = 1,
                        CreatedDate = DateTime.Now,
                        DebitCredit = x.DebitCredit,
                        IsActive = false,
                        IsDeleted = false,
                        MnemonicCode = x.MnemonicCode,
                        RemitterAccount = x.RemitterAccount,
                        RemitterBank = x.RemitterBank,
                        RemitterIFSC = x.RemitterIFSC,
                        RemitterName = x.RemitterName,
                        TransactionDate = x.TransactionDate,
                        TransactionDescription = x.TransactionDescription,
                        UserReferenceNumber = x.UserReferenceNumber,
                        IsAmountAdd = false
                    }).FirstOrDefault();
                    context.VANResponses.Add(vanResponsedbs);//
                    try
                    {
                        if (context.Commit() > 0)
                        {
                            var item = vanResponsedbs;
                            if (!context.VANResponses.Any(x => alertSequenceNos == x.AlertSequenceNo && x.IsActive))
                            {
                                if (!string.IsNullOrEmpty(item.BenefDetails2))
                                {
                                    item.IsActive = true;
                                    var customerId = context.Customers.FirstOrDefault(x => x.Skcode == skCode && !x.Deleted)?.CustomerId;
                                    if (customerId.HasValue)
                                    {
                                        item.IsAmountAdd = true;
                                        context.VANTransactiones.Add(new Model.VAN.VANTransaction
                                        {
                                            Amount = item.Amount,
                                            CreatedBy = 1,
                                            CreatedDate = DateTime.Now,
                                            CustomerId = customerId.Value,
                                            IsActive = true,
                                            IsDeleted = false,
                                            ObjectId = item.Id,
                                            Comment = "Payment received through " + item.MnemonicCode,
                                            ObjectType = "VANResponse"
                                        });
                                        returnResponse = new VANReturnResponse
                                        {
                                            GenericCorporateAlertResponse = new ReturnResp
                                            {
                                                errorCode = "0",
                                                domainReferenceNo = alertSequenceNos,
                                                errorMessage = "Success"
                                            }
                                        };
                                    }
                                    else
                                    {
                                        returnResponse = new VANReturnResponse
                                        {
                                            GenericCorporateAlertResponse = new ReturnResp
                                            {
                                                errorCode = "1",
                                                domainReferenceNo = alertSequenceNos,
                                                errorMessage = "Technical Reject"
                                            }
                                        };
                                    }
                                    context.Entry(item).State = EntityState.Modified;
                                    context.Commit();
                                }
                            }
                            else
                            {
                                returnResponse = new VANReturnResponse
                                {
                                    GenericCorporateAlertResponse = new ReturnResp
                                    {
                                        errorCode = "0",
                                        domainReferenceNo = item.AlertSequenceNo,
                                        errorMessage = "Duplicate"
                                    }
                                };
                            }
                        }
                        else
                        {
                            returnResponse = new VANReturnResponse
                            {
                                GenericCorporateAlertResponse = new ReturnResp
                                {
                                    errorCode = "1",
                                    domainReferenceNo = alertSequenceNos,
                                    errorMessage = "Technical Reject"
                                }
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.ToString().Contains("Cannot insert duplicate key row in object 'dbo.VANResponses' with unique index"))
                        {
                            returnResponse = new VANReturnResponse
                            {
                                GenericCorporateAlertResponse = new ReturnResp
                                {
                                    errorCode = "0",
                                    domainReferenceNo = alertSequenceNos,
                                    errorMessage = "Duplicate"
                                }
                            };
                        }
                        else
                        {
                            returnResponse = new VANReturnResponse
                            {
                                GenericCorporateAlertResponse = new ReturnResp
                                {
                                    errorCode = "1",
                                    domainReferenceNo = alertSequenceNos,
                                    errorMessage = "Technical Reject"
                                }
                            };
                        }
                    }
                }
            }
            return returnResponse;
        }

        [Route("GetCustomerRTGSAmount")]
        [HttpGet]
        public double GetCustomerRTGSAmount(int customerId)
        {
            double remainingAmount = 0;
            using (var context = new AuthContext())
            {
                var CustomerId = new SqlParameter("@CustomerId", customerId);
                remainingAmount = context.Database.SqlQuery<double>("EXEC GetCustomerVANAmount @CustomerId", CustomerId).FirstOrDefault();
            }
            return remainingAmount;
        }

        [Route("GetTopRTGSTrans")]
        [HttpGet]
        public async Task<List<RTGSTransactionResponse>> GetTopRTGSTrans(int customerId,int skip,int totalRecord)
        {
            List<RTGSTransactionResponse> RTGSTransactionResponses = new List<RTGSTransactionResponse>();
            using (var context = new AuthContext())
            {
                var CustomerId = new SqlParameter("@CustomerId", customerId);
                var Skip = new SqlParameter("@Skip", skip);
                var Take = new SqlParameter("@Take", totalRecord);
                RTGSTransactionResponses =await context.Database.SqlQuery<RTGSTransactionResponse>("EXEC GetTopRTGSTrans @CustomerId,@Skip,@Take", CustomerId,Skip,Take).ToListAsync();
                //await context.VANTransactiones.Where(x => x.CustomerId == customerId && x.IsActive).OrderByDescending(x => x.Id)
                //.Select(x => new RTGSTransactionResponse
                //{
                //    Amount = x.Amount,
                //    comment = x.Comment,
                //    ObjectId = x.ObjectId,
                //    CreatedDate=x.CreatedDate
                //}).Skip(skip).Take(totalRecord).ToListAsync();
            }
            return RTGSTransactionResponses;
        }
        

    }
}
