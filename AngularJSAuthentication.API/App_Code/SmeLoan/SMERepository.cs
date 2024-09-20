using AngularJSAuthentication.BusinessLayer.SMELoan.BO;
using AngularJSAuthentication.BusinessLayer.SMELoan.IF;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.FileUpload;
using AngularJSAuthentication.Model;
using Newtonsoft.Json;
using Nito.AsyncEx;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Transactions;
using System.Web;

namespace AngularJSAuthentication.API.App_Code.SmeLoan
{
    public class SMERepository : ISME, IDisposable
    {
        string Url = ConfigurationManager.AppSettings["SmeUrl"];
        private bool disposed = false;
        private AuthContext Context;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Context.Dispose();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public SMERepository(AuthContext Context)
        {
            this.Context = Context;
        }


        public SMEDc PostCustomer(SMECustomerLoan objSmeLoan)
        {
            try
            {
                string Type = "POST";
                int id = 0;
                using (AuthContext context = new AuthContext())
                {
                    var data = context.SMECustomerLoan.Where(x => x.CustomerId == objSmeLoan.CustomerId).FirstOrDefault();
                    if (data != null)
                    {
                        bool HISResult = SMELoanHistory(data, context);
                        if (HISResult)
                        {
                            data.CustomerName = objSmeLoan.CustomerName;

                            data.CustomerId = objSmeLoan.CustomerId;
                            data.MobileNumber = objSmeLoan.MobileNumber;
                            data.EntityName = objSmeLoan.EntityName;
                            data.BusinessAddress = objSmeLoan.BusinessAddress;

                            data.EmailId = objSmeLoan.EmailId;
                            data.PanNo = objSmeLoan.PanNo;
                            data.BusinessEntityProof = objSmeLoan.PanNo;
                            data.ResidenceAddres = objSmeLoan.ResidenceAddres;
                            data.ResidencePincode = objSmeLoan.ResidencePincode;
                            data.DateofBirth = objSmeLoan.DateofBirth;
                            data.LoanAmount = objSmeLoan.LoanAmount;
                            data.Gender = objSmeLoan.Gender;
                            data.ResidenceOwned = objSmeLoan.ResidenceOwned;
                            data.OfficeOwned = objSmeLoan.OfficeOwned;
                            data.Status = 0;
                            data.UpdateDate = DateTime.Now;
                            data.UpdatedBy = objSmeLoan.UpdatedBy;
                            context.Entry(data).State = EntityState.Modified;
                            id = data.Id;
                            Type = "UPDATE";
                        }

                    }
                    else
                    {
                        Customer Objcustomer = context.Customers.Where(x => x.CustomerId == objSmeLoan.CustomerId).FirstOrDefault();
                        objSmeLoan.SKcode = Objcustomer.Skcode;
                        //objSmeLoan.CustomerName = Objcustomer.Name;
                        objSmeLoan.CreatedBy = objSmeLoan.CustomerId;
                        objSmeLoan.BusinessEntityProof = objSmeLoan.PanNo;

                        //objSmeLoan.BusinessAddress = Objcustomer.BillingAddress;
                        //objSmeLoan.MobileNumber = Objcustomer.Mobile;
                        context.SMECustomerLoan.Add(objSmeLoan);

                    }
                    if (context.Commit() > 0)
                    {
                        if (!Type.ToUpper().Equals("UPDATE"))
                        {
                            id = objSmeLoan.Id;
                        }

                        bool PostResult = PostDataTOSME(id, context);
                        if (PostResult)
                        {
                            SMECustomerLoan objCustomerLoan = GetCustomerDetail(id);
                            SMEDc ObjSmeDc = new SMEDc()
                            {
                                Id = objCustomerLoan.Id,
                                Status = objCustomerLoan.Status,
                                Description = objCustomerLoan.Description,
                                ResponseStatus = objCustomerLoan.ResponseStatus
                            };
                            return ObjSmeDc;
                        }
                        else
                        {
                            throw new Exception("Something went wrong please try again later!!");
                        }

                    }
                    else
                    {
                        throw new Exception("Something went wrong please try again later!!");

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public bool UpdateLeadId(SMEResponse ObjSmeResponse, int Id, SMEPostData ObjSmedata, int status, int UpdatedBy)
        {

            try
            {
                //using (TransactionScope Scope = new TransactionScope())
                //{
                bool Result = false;

                using (AuthContext context = new AuthContext())
                {
                    SMECustomerLoan ObjSMECustomerLoan = context.SMECustomerLoan.Where(x => x.Id == Id).FirstOrDefault();
                    bool ResultHistory = SMELoanHistory(ObjSMECustomerLoan, context);
                    bool UpdateLeadIdResult = UpdateStatus(Id, ObjSmeResponse, context, status, UpdatedBy);
                    bool UpdtApplicationid = LeadQualification(ObjSmeResponse.t, ObjSmedata.submitter_phone, context, ObjSmedata, ObjSMECustomerLoan, ObjSMECustomerLoan.Id);

                    if (ResultHistory && UpdateLeadIdResult)
                    {
                        context.Commit();
                        Result = true;
                        // Scope.Complete();
                    }
                    else
                    {
                        throw new Exception("Something went wrong please try again later!!");
                    }
                    return Result;
                }
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private bool SMELoanHistory(SMECustomerLoan ObjSMECustomerLoan, AuthContext context)
        {
            bool Result = false;
            SMECustomerLoanHistory objSMECustomerLoanHistory = new SMECustomerLoanHistory();
            objSMECustomerLoanHistory.Id = ObjSMECustomerLoan.Id;
            objSMECustomerLoanHistory.CustomerName = ObjSMECustomerLoan.CustomerName;
            objSMECustomerLoanHistory.SKcode = ObjSMECustomerLoan.SKcode;
            objSMECustomerLoanHistory.CustomerId = ObjSMECustomerLoan.CustomerId;
            objSMECustomerLoanHistory.MobileNumber = ObjSMECustomerLoan.MobileNumber;
            objSMECustomerLoanHistory.EntityName = ObjSMECustomerLoan.EntityName;
            objSMECustomerLoanHistory.BusinessAddress = ObjSMECustomerLoan.BusinessAddress;
            objSMECustomerLoanHistory.BusinessEntityProof = ObjSMECustomerLoan.BusinessEntityProof;
            objSMECustomerLoanHistory.EmailId = ObjSMECustomerLoan.EmailId;
            objSMECustomerLoanHistory.PanNo = ObjSMECustomerLoan.PanNo;
            objSMECustomerLoanHistory.ResidenceAddres = ObjSMECustomerLoan.ResidenceAddres;
            objSMECustomerLoanHistory.ResidencePincode = ObjSMECustomerLoan.ResidencePincode;
            objSMECustomerLoanHistory.DateofBirth = ObjSMECustomerLoan.DateofBirth;
            objSMECustomerLoanHistory.LoanAmount = ObjSMECustomerLoan.LoanAmount;
            objSMECustomerLoanHistory.Gender = ObjSMECustomerLoan.Gender;
            objSMECustomerLoanHistory.ResidenceOwned = ObjSMECustomerLoan.ResidenceOwned;
            objSMECustomerLoanHistory.OfficeOwned = ObjSMECustomerLoan.OfficeOwned;
            objSMECustomerLoanHistory.CreatedDate = ObjSMECustomerLoan.CreatedDate;
            objSMECustomerLoanHistory.CreatedBy = ObjSMECustomerLoan.CreatedBy;
            objSMECustomerLoanHistory.Status = ObjSMECustomerLoan.Status;
            objSMECustomerLoanHistory.UpdateDate = ObjSMECustomerLoan.UpdateDate;
            objSMECustomerLoanHistory.UpdatedBy = ObjSMECustomerLoan.UpdatedBy;
            objSMECustomerLoanHistory.LeadId = ObjSMECustomerLoan.LeadId;
            ObjSMECustomerLoan.ResponseStatus = ObjSMECustomerLoan.ResponseStatus;
            ObjSMECustomerLoan.Description = ObjSMECustomerLoan.Description;
            ObjSMECustomerLoan.ApplicationId = ObjSMECustomerLoan.ApplicationId;
            //ObjSMECustomerLoan.DocumentId = ObjSMECustomerLoan.DocumentId;
            //ObjSMECustomerLoan.DocumentPath = ObjSMECustomerLoan.DocumentPath;
            objSMECustomerLoanHistory.BackupDate = DateTime.Now;
            if (objSMECustomerLoanHistory != null)
            {
                Result = true;
                context.SMECustomerLoanHistory.Add(objSMECustomerLoanHistory);
            }

            return Result;

        }

        private bool UpdateStatus(int Id, SMEResponse ObjSmeResponse, AuthContext context, int status, int UpdatedBy)
        {
            bool Result = true;
            var data = context.SMECustomerLoan.Where(x => x.Id == Id).FirstOrDefault();
            data.UpdateDate = DateTime.Now;
            data.UpdatedBy = UpdatedBy;
            data.LeadId = ObjSmeResponse.t;
            data.Status = status;
            data.Description = data.Description;
            data.ResponseStatus = ObjSmeResponse.status;
            data.creditScore = ObjSmeResponse.creditScore;
            data.ApplicationId = ObjSmeResponse.applicationId;

            context.Entry(data).State = EntityState.Modified;
            Result = true;
            return Result;

        }

        private bool PostDataTOSME(int id, AuthContext context)
        {
            SMECustomerLoan ObjsMECustomerLoan = context.SMECustomerLoan.Where(x => x.Id == id).FirstOrDefault();
            try
            {
                bool Result = false;

                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("api_key", "5116528b6c24692d1a189750143297d3");
                client.DefaultRequestHeaders.Add("package_id", "SME_WEB_1.0");

                string DOb = ObjsMECustomerLoan.DateofBirth.ToString("yyyy-MM-dd");

                SMEPostData ObjSMEPostData = new SMEPostData()
                {
                    applicant_1_first_name = ObjsMECustomerLoan.CustomerName,
                    applicant_1_phone = ObjsMECustomerLoan.MobileNumber,
                    company = ObjsMECustomerLoan.EntityName,
                    applicant_1_email = ObjsMECustomerLoan.EmailId,
                    product = "Small Retailer Business Loan",
                    applicant_1_gender = ObjsMECustomerLoan.Gender,
                    applicant_1_pan = ObjsMECustomerLoan.PanNo,
                    applicant_1_residence_address = ObjsMECustomerLoan.ResidenceAddres,
                    residence_pincode = ObjsMECustomerLoan.ResidencePincode,
                    applicant_1_dob = DOb,
                    loan_requested = Convert.ToString(ObjsMECustomerLoan.LoanAmount),
                    itr_filed = ObjsMECustomerLoan.ItrFiled,
                    applicant_1_residence_Ownership = Convert.ToString(ObjsMECustomerLoan.ResidenceOwned),
                    applicant_1_residence_pincode = ObjsMECustomerLoan.ResidencePincode,
                    office_ownership = Convert.ToString(ObjsMECustomerLoan.OfficeOwned),
                    office_address = ObjsMECustomerLoan.BusinessAddress,
                    submitter_phone = "1234123482",
                    lead_source = "Unassisted",
                    source = "MSK007",
                    office_pincode = ObjsMECustomerLoan.ResidencePincode

                };

                var res = client.PostAsJsonAsync(Url + "/SME/loan/application/complete/save?phoneNo=" + ObjSMEPostData.submitter_phone, ObjSMEPostData).Result;
                bool SmeRequestResponse = AddSMERequestResponse(Url + "/SME/loan/application/complete/save?phoneNo=" + ObjSMEPostData.submitter_phone, ObjsMECustomerLoan.CustomerId, res.Content.ReadAsStringAsync().Result, JsonConvert.SerializeObject(ObjSMEPostData), "");
                string jsonString = res.Content.ReadAsStringAsync().Result.Replace("\\", "").Trim(new char[1] { '"' });

                if (res.IsSuccessStatusCode && SmeRequestResponse)
                {
                    SMEResponse objSmeResponse = JsonConvert.DeserializeObject<SMEResponse>(jsonString);

                    Result = UpdateLeadId(objSmeResponse, id, ObjSMEPostData, 1, ObjsMECustomerLoan.CustomerId);


                    return Result;

                }
                else
                {
                    throw new Exception("Something went wrong please try again later!!");
                }



            }
            catch (Exception ex)
            {
                bool SmeRequestResponse = AddSMERequestResponse("Post Data and Leadid Updation", ObjsMECustomerLoan.CustomerId, "", "", ex.GetBaseException().Message);

                throw ex;

            }
        }

        private bool LeadQualification(string LeadId, string PhoneNo, AuthContext context, SMEPostData ObjsMEPostData, SMECustomerLoan ObjCustomerLoan, int id)
        {
            bool FinalResult = false;
            try
            {

                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("api_key", "5116528b6c24692d1a189750143297d3");
                client.DefaultRequestHeaders.Add("package_id", "SME_WEB_1.0");
                var res = client.PostAsJsonAsync(Url + "/SME/lead/convert?leadId=" + LeadId + "&phoneNo=" + PhoneNo, "").Result;
                bool SmeRequestResponse = AddSMERequestResponse(Url + "/SME/lead/convert?leadId=" + LeadId + "&phoneNo=" + PhoneNo, ObjCustomerLoan.CustomerId, res.Content.ReadAsStringAsync().Result, "", "");
                string jsonString = res.Content.ReadAsStringAsync().Result.Replace("\\", "").Trim(new char[1] { '"' });
                if (res.IsSuccessStatusCode && SmeRequestResponse)
                {
                    SMEResponse objSmeResponse = JsonConvert.DeserializeObject<SMEResponse>(jsonString);
                    bool Result = UpdateApplication(ObjCustomerLoan, context, id, objSmeResponse);
                    if (Result)
                    {
                        FinalResult = true;
                    }
                }
            }

            catch (Exception ex)
            {
                bool exception = AddSMERequestResponse(Url + "/SME/lead/convert?leadId=" + LeadId + "&phoneNo=" + PhoneNo, ObjCustomerLoan.CustomerId, "", "", ex.GetBaseException().Message.ToString());

                throw ex;
            }
            return FinalResult;
        }

        private bool UpdateApplication(SMECustomerLoan ObjSMECustomerLoan, AuthContext Context, int Id, SMEResponse ObjsMEResponse)
        {
            bool Result = false;
            //using (TransactionScope Scope = new TransactionScope())
            //{

            //bool ResultHistory = SMELoanHistory(ObjSMECustomerLoan, Context);
            bool UpdateLeadIdResult = UpdateApplicationId(Id, ObjsMEResponse, Context, 1);
            if (UpdateLeadIdResult)
            {
                ////Context.Commit();
                Result = true;
                //Scope.Complete();
            }
            //}
            return Result;
        }

        private bool UpdateApplicationId(int Id, SMEResponse ObjSmeResponse, AuthContext context, int status)
        {
            bool Result = true;
            var data = context.SMECustomerLoan.Where(x => x.Id == Id).FirstOrDefault();
            data.UpdateDate = DateTime.Now;
            data.UpdatedBy = 0;

            data.Status = status;
            data.Description = Convert.ToString(ObjSmeResponse.desc);
            data.ResponseStatus = ObjSmeResponse.status;
            data.creditScore = ObjSmeResponse.creditScore;
            data.ApplicationId = ObjSmeResponse.applicationId;
            context.Entry(data).State = EntityState.Modified;
            Result = true;
            return Result;

        }

        public bool Uploaddocument()
        {
            bool FinalResult = false;
            bool Result = false;
            bool ResSME = false;
            using (TransactionScope scope = new TransactionScope())
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    // Get the uploaded image from the Files collection
                    var Id = HttpContext.Current.Request.Form["Id"];
                    //var Pan = HttpContext.Current.Request.Files["Pan"];
                    //var Aadhar = HttpContext.Current.Request.Files["Aadhar"];
                    //var BusProof = HttpContext.Current.Request.Files["BusProof"];
                    //var Bankstatemet = HttpContext.Current.Request.Files["Bankstatemet"];
                    int ID = Convert.ToInt32(Id);
                    SMECustomerLoan ObjCustomeLoan = Context.SMECustomerLoan.Where(x => x.Id == ID).FirstOrDefault();

                    bool Res = SMELoanHistory(ObjCustomeLoan, Context);
                    for (int i = 0; i < HttpContext.Current.Request.Files.Count; i++)
                    {
                        var httpPostedFile = HttpContext.Current.Request.Files[Convert.ToString(i + 1)];
                        if (httpPostedFile != null)
                        {
                            string extension = Path.GetExtension(httpPostedFile.FileName);
                            string DocText = ReturnDocIdPath(i + 1);
                            string newFileName = ObjCustomeLoan.SKcode + "_" + DocText + extension;

                            string ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/Images/SMELoan"), newFileName);

                            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/Images/SMELoan")))
                            {
                                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/Images/SMELoan"));
                            }

                            httpPostedFile.SaveAs(ImageUrl);

                            var uploader = new List<Uploader> { new Uploader() };
                            uploader.FirstOrDefault().FileName = httpPostedFile.FileName;
                            uploader.FirstOrDefault().RelativePath = "~/Images/SMELoan";


                            uploader.FirstOrDefault().SaveFileURL = ImageUrl;

                            uploader = AsyncContext.Run(() => FileUploadHelper.UploadFileToOtherApi(uploader));

                            if (File.Exists(ImageUrl))
                            {

                                //string DocText = ReturnDocIdPath(i + 1);

                                Result = UpdateDocumentPath(ImageUrl, ObjCustomeLoan, ObjCustomeLoan.UpdatedBy, DocText);

                                ResSME = SentDocument(ObjCustomeLoan, Context, ImageUrl, httpPostedFile, DocText);
                            }

                        }
                    }

                    if (Res && Result && ResSME)
                    {
                        FinalResult = true;
                        Context.Commit();
                        scope.Complete();
                    }

                }
            }
            return FinalResult;
        }

        private bool UpdateDocumentPath(string DocumentPath, SMECustomerLoan data, int UpdatedBy, string DocText)
        {
            bool Result = false;
            string Datadocumentpath = "data.DocumentPath" + DocText;
            data.UpdateDate = DateTime.Now;
            data.UpdatedBy = UpdatedBy;
            data.Status = 2;
            if (DocText.ToUpper().Equals("PAN"))
            {
                data.DocumentPathPan = DocumentPath;



            }
            if (DocText.ToUpper().Equals("ADHAR"))
            {
                data.DocumentPathAdhar = DocumentPath;

            }
            if (DocText.ToUpper().Equals("BUSINESSPROOF"))
            {
                data.DocumentPathBusinessProof = DocumentPath;

            }
            if (DocText.ToUpper().Equals("BANKSTATEMENT"))
            {
                data.DocumentPathBankstatement = DocumentPath;


            }

            Context.Entry(data).State = EntityState.Modified;
            Result = true;
            return Result;


        }
        private bool SentDocument(SMECustomerLoan ObjSmeCustomerLoan, AuthContext context, string DocumentPath, HttpPostedFile httpPostedFile, string DocText)
        {
            bool Result = false;

            try
            {
                //SMECustomerLoan ObjSmeCustomerLoan = Context.SMECustomerLoan.Where(x => x.Id == id).FirstOrDefault();
                string requestURL = Url + "/SME/refdata/multipart/process?phoneNo=1234123482" + "&leadid=" + ObjSmeCustomerLoan.LeadId + "&applicationId=" + ObjSmeCustomerLoan.ApplicationId + "&ref_name=document_upload&ref_code=document&docCategory=&docType=&isOgFileNameRetain=true";

                string fileName = DocumentPath;
                WebClient wc = new WebClient();
                byte[] bytes = wc.DownloadData(fileName); // You need to do this download if your file is on any other server otherwise you can convert that file directly to bytes  
                Dictionary<string, object> postParameters = new Dictionary<string, object>();
                // Add your parameters here  
                postParameters.Add("file", new FileUpload.FileParameter(bytes, Path.GetFileName(fileName), httpPostedFile.ContentType));
                string userAgent = "ShopKirana";
                HttpWebResponse webResponse = FileUpload.MultipartFormPost(requestURL, userAgent, postParameters, "api_key", "5116528b6c24692d1a189750143297d3");
                // Process response  
                StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
                var data = responseReader.ReadToEnd();
                string jsonString = data.Replace("\\", "").Trim(new char[1] { '"' });
                bool SmeRequestResponse = AddSMERequestResponse(requestURL, ObjSmeCustomerLoan.CustomerId, jsonString, "", "");

                DocApplication ObjDocApplication = JsonConvert.DeserializeObject<DocApplication>(jsonString);
                webResponse.Close();

                bool DocumentResult = UpdateDocumentId(ObjSmeCustomerLoan.Id, ObjDocApplication.t, ObjDocApplication, DocText);
                if (DocumentResult && SmeRequestResponse)
                {
                    Result = true;
                }

                return Result;
            }
            catch (Exception ex)
            {
                bool SmeRequestResponse = AddSMERequestResponse("Document Upload", ObjSmeCustomerLoan.Id, "", "", ex.GetBaseException().Message.ToString());

                throw ex;
            }

        }
        private SMECustomerLoan GetCustomerDetail(int id)
        {
            using (AuthContext context = new AuthContext())
            {

                SMECustomerLoan ObjSmeCustomerLoan = context.SMECustomerLoan.Where(x => x.Id == id).FirstOrDefault();
                return ObjSmeCustomerLoan;

            }
        }

        private bool UpdateDocumentId(int Id, DocumentResponse ObjDocumentResponse, DocApplication objDocApplication, string DocText)
        {
            bool Result = true;

            var data = Context.SMECustomerLoan.Where(x => x.Id == Id).FirstOrDefault();
            data.UpdateDate = DateTime.Now;
            data.UpdatedBy = 0;
            data.Status = 2;
            data.Description = objDocApplication.desc;
            data.ResponseStatus = objDocApplication.status;
            if (DocText.ToUpper().Equals("PAN"))
            {
                data.DocumentIdPan = Convert.ToString(ObjDocumentResponse.app_doc_id);



            }
            if (DocText.ToUpper().Equals("ADHAR"))
            {
                data.DocumentIdAdhar = Convert.ToString(ObjDocumentResponse.app_doc_id);

            }
            if (DocText.ToUpper().Equals("BUSINESSPROOF"))
            {
                data.DocumentIdBusinessProof = Convert.ToString(ObjDocumentResponse.app_doc_id);

            }
            if (DocText.ToUpper().Equals("BANKSTATEMENT"))
            {
                data.DocumentIdBankstatement = Convert.ToString(ObjDocumentResponse.app_doc_id);

            }
            //data.DocumentId = Convert.ToString(ObjDocumentResponse.app_doc_id);
            //DataDocumentId = Convert.ToString(ObjDocumentResponse.app_doc_id);
            Context.Entry(data).State = EntityState.Modified;
            Result = true;
            return Result;

        }

        public SMECustomerDC GetLoanAppliedDetails(int? Warehouseid, string SKCode, int First, int Last)
        {
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    var WarehouseidParam = new SqlParameter
                    {
                        ParameterName = "WarehouseId",
                        Value = Warehouseid == null ? DBNull.Value : (object)Warehouseid
                    };


                    var SkcodeParam = new SqlParameter
                    {
                        ParameterName = "Skcode",
                        Value = SKCode == null ? DBNull.Value : (object)SKCode
                    };

                    var FirstParam = new SqlParameter
                    {
                        ParameterName = "First",
                        Value = First
                    };

                    var LastParam = new SqlParameter
                    {
                        ParameterName = "Last",
                        Value = Last

                    };

                    List<SMECustomerLoanAppliedDc> objSMECustomerLoanDC = context.Database.SqlQuery<SMECustomerLoanAppliedDc>("GetSmeCustomerLoanData @WarehouseId ,@Skcode,@First,@Last ", WarehouseidParam, SkcodeParam,
                    FirstParam, LastParam).ToList();
                    int count = 0;
                    if (Last != 0)
                    {
                        count = objSMECustomerLoanDC.Count();
                        objSMECustomerLoanDC = objSMECustomerLoanDC.Skip(First).Take(Last).ToList();

                    }
                    else
                    {
                        objSMECustomerLoanDC = objSMECustomerLoanDC.ToList();
                        count = objSMECustomerLoanDC.Count();
                    }
                    //int Totalcount = context.SMECustomerLoan.Count();
                    SMECustomerDC ObjSmeCustomerDc = new SMECustomerDC()
                    {
                        SMECustomerLoanList = objSMECustomerLoanDC,
                        TotalCount = count

                    };
                    return ObjSmeCustomerDc;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool AddSMERequestResponse(string RequestUrl, int CustomerId, string ResponseJson, string RequestJson, string ErrorMessage)
        {
            bool Result = false;

            SMERequestResponse ObjSMERequestResponse = new SMERequestResponse();
            ObjSMERequestResponse.RequestUrl = RequestUrl;
            ObjSMERequestResponse.RequestJson = RequestJson;
            ObjSMERequestResponse.ResponseJson = ResponseJson;
            ObjSMERequestResponse.CustomerId = CustomerId;
            ObjSMERequestResponse.CreatedDate = DateTime.Now;
            ObjSMERequestResponse.ErrorMessage = ErrorMessage;
            Context.SMERequestResponse.Add(ObjSMERequestResponse);
            if (ObjSMERequestResponse != null)
            {
                Context.Commit();

                Result = true;


            }
            return Result;
        }

        private string ReturnDocIdPath(int Index)
        {
            string DocText = "";
            switch (Index)
            {
                case 1:
                    DocText = "Pan";
                    break;
                case 2:
                    DocText = "Adhar";
                    break;

                case 3:
                    DocText = "BusinessProof";
                    break;

                case 4:
                    DocText = "Bankstatement";
                    break;
            }
            return DocText;
        }


        private bool SMELoanHistoryDoc(SMECustomerLoan ObjSMECustomerLoan, AuthContext context)
        {
            bool Result = false;
            SMECustomerLoanHistory objSMECustomerLoanHistory = new SMECustomerLoanHistory();
            objSMECustomerLoanHistory.Id = ObjSMECustomerLoan.Id;
            objSMECustomerLoanHistory.CustomerName = ObjSMECustomerLoan.CustomerName;
            objSMECustomerLoanHistory.SKcode = ObjSMECustomerLoan.SKcode;
            objSMECustomerLoanHistory.CustomerId = ObjSMECustomerLoan.CustomerId;
            objSMECustomerLoanHistory.MobileNumber = ObjSMECustomerLoan.MobileNumber;
            objSMECustomerLoanHistory.EntityName = ObjSMECustomerLoan.EntityName;
            objSMECustomerLoanHistory.BusinessAddress = ObjSMECustomerLoan.BusinessAddress;
            objSMECustomerLoanHistory.BusinessEntityProof = ObjSMECustomerLoan.BusinessEntityProof;
            objSMECustomerLoanHistory.EmailId = ObjSMECustomerLoan.EmailId;
            objSMECustomerLoanHistory.PanNo = ObjSMECustomerLoan.PanNo;
            objSMECustomerLoanHistory.ResidenceAddres = ObjSMECustomerLoan.ResidenceAddres;
            objSMECustomerLoanHistory.ResidencePincode = ObjSMECustomerLoan.ResidencePincode;
            objSMECustomerLoanHistory.DateofBirth = ObjSMECustomerLoan.DateofBirth;
            objSMECustomerLoanHistory.LoanAmount = ObjSMECustomerLoan.LoanAmount;
            objSMECustomerLoanHistory.Gender = ObjSMECustomerLoan.Gender;
            objSMECustomerLoanHistory.ResidenceOwned = ObjSMECustomerLoan.ResidenceOwned;
            objSMECustomerLoanHistory.OfficeOwned = ObjSMECustomerLoan.OfficeOwned;
            objSMECustomerLoanHistory.CreatedDate = ObjSMECustomerLoan.CreatedDate;
            objSMECustomerLoanHistory.CreatedBy = ObjSMECustomerLoan.CreatedBy;
            objSMECustomerLoanHistory.Status = ObjSMECustomerLoan.Status;
            objSMECustomerLoanHistory.UpdateDate = ObjSMECustomerLoan.UpdateDate;
            objSMECustomerLoanHistory.UpdatedBy = ObjSMECustomerLoan.UpdatedBy;
            objSMECustomerLoanHistory.LeadId = ObjSMECustomerLoan.LeadId;
            ObjSMECustomerLoan.ResponseStatus = ObjSMECustomerLoan.ResponseStatus;
            ObjSMECustomerLoan.Description = ObjSMECustomerLoan.Description;
            ObjSMECustomerLoan.ApplicationId = ObjSMECustomerLoan.ApplicationId;
            ObjSMECustomerLoan.DocumentId = ObjSMECustomerLoan.DocumentId;
            ObjSMECustomerLoan.DocumentPath = ObjSMECustomerLoan.DocumentPath;
            objSMECustomerLoanHistory.BackupDate = DateTime.Now;
            if (objSMECustomerLoanHistory != null)
            {
                Result = true;
                context.SMECustomerLoanHistory.Add(objSMECustomerLoanHistory);
            }

            return Result;

        }


        public IEnumerable<Warehouse> GetWarehouse()
        {
            using (AuthContext context = new AuthContext())
            {
                var data = context.Warehouses.Where(x => x.Deleted == false && x.active == true).ToList();
                return data;
            }
        }

    }


}


