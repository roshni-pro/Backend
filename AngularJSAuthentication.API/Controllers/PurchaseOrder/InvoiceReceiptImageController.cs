using AngularJSAuthentication.DataContracts.InvoiceReceipt;
using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.PurchaseOrder
{
    [RoutePrefix("api/InvoiceReceipt")]
    public class InvoiceReceiptImageController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);


        #region invoice Receipt image upload 
        /// <summary>
        /// invoice Receipt image upload 
        /// </summary>
        /// <returns></returns>
        [Route("InvoiceReceiptImage")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage InvoiceReceiptImage()
        {
            string ImageUrl = "";
            bool status = false;
            string resultMessage = "";
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                if (httpPostedFile != null)
                {
                    if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/images/GrDraftInvoices/")))
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/images/GrDraftInvoices/"));

                    ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/images/GrDraftInvoices/"), httpPostedFile.FileName);

                    httpPostedFile.SaveAs(ImageUrl);
                    AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/images/GrDraftInvoices", ImageUrl);
                    ImageUrl = "/images/GrDraftInvoices/" + httpPostedFile.FileName;
                }
            }
            else
                {
                    ImageUrl = "";
                    status = false;
                    resultMessage = "some thing went wrong";
                }
                var res = new
                {
                    ImageUrl = ImageUrl,
                    status = true,
                    Message = "Upload Image Successfully"
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);

        }
        #endregion

        #region Post api for Invoice Receipt
        /// <summary>
        /// post api for Invoice Receipt
        /// </summary>
        /// <param name="InvoiceReceiptImage"></param>
        /// <returns></returns>
        [Route("PostInvoiceReceipt")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage InvoiceReceiptImage(List<InvoiceReceiptImageDC> InvoiceReceiptImage)
        {
            string resultMessage = "";
            bool status = false;
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
           
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (AuthContext context = new AuthContext())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    int PurchaseOrderId = InvoiceReceiptImage.FirstOrDefault().PurchaseOrderId;
                    var DepoId = context.DPurchaseOrderMaster.FirstOrDefault(x => x.PurchaseOrderId == PurchaseOrderId).DepoId;
                    var depoGst = context.DepoMasters.FirstOrDefault(x => x.DepoId == DepoId).GSTin;
                    if (!string.IsNullOrEmpty(InvoiceReceiptImage.FirstOrDefault().GSTN) && InvoiceReceiptImage.FirstOrDefault().GSTN.Trim().ToLower()!= depoGst.Trim().ToLower()) 
                    {
                        var gres = new
                        {
                            invoiceReceipt = InvoiceReceiptImage,
                            status = false,
                            Message = "GST Number not matched with po GST"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, gres);
                    }
                    //var invoice = context.InvoiceReceiptImage.ToList();
                    if (InvoiceReceiptImage != null && InvoiceReceiptImage.Any())
                    {
                        var  AddData = new List<InvoiceReceiptImage>();
                        foreach (var invoice in InvoiceReceiptImage)
                        {
                            InvoiceReceiptImage items = new InvoiceReceiptImage();
                            items.PurchaseOrderId = invoice.PurchaseOrderId;
                            items.GrSerialNumber = invoice.GrSerialNumber;
                            items.InvoiceNumber = invoice.InvoiceNumber;
                            items.InvoiceDate = invoice.InvoiceDate;
                            items.InvoiceImage = invoice.InvoiceImage;
                            items.CreatedDate = indianTime;
                            items.CreatedBy = userid;
                            items.ModifiedDate =indianTime ;
                            items.ModifiedBy = userid;
                            items.IsActive = true;
                            items.IsDeleted = false;
                            AddData.Add(items);
                        }
                        if (AddData != null && AddData.Any())
                        {
                            context.InvoiceReceiptImage.AddRange(AddData);
                            
                        }
                        if (context.Commit() > 0)
                        {
                            status = true;
                            resultMessage = "Invoice Receipt uploaded successfully";
                            dbContextTransaction.Commit();
                        }
                        else
                        {
                            status = false;
                            resultMessage = "something went wrong";
                            dbContextTransaction.Rollback();
                        }
                    }
                    else
                    {
                        status = false;
                        resultMessage = "Record not found";
                    }

                    var res = new
                    {
                        invoiceReceipt = InvoiceReceiptImage,
                        status = status,
                        Message = resultMessage
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
            }
                 
        }
        #endregion

        #region Get Invoice Receipt
        /// <summary>
        /// Get Invoice Receipt also use for GRDraft Page
        /// </summary>
        /// <param name="PurchaseOrderId"></param>
        /// <param name="GrSerialNumber"></param>
        /// <returns></returns>
        [Route("GetInvoiceReceipt")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<InvoiceReceiptImage>> Get(int PurchaseOrderId , int GrSerialNumber)
        {
            using (AuthContext context = new AuthContext())
            {
                string query = "select * from InvoiceReceiptImages where PurchaseOrderId="+ PurchaseOrderId + " and GrSerialNumber="+GrSerialNumber;
                List<InvoiceReceiptImage> image = await context.Database.SqlQuery<InvoiceReceiptImage>(query).ToListAsync();
                string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

                foreach (var item in image)
                {
                    item.InvoiceImage = baseUrl + "/images/GrDraftInvoices/" + item.InvoiceImage;
                }
                return image;
            }
        }
        #endregion

    }
}




