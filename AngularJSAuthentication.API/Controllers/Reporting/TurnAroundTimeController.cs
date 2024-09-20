using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model;
using Common.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.Reporting
{

    [RoutePrefix("api/TurnAroundTime")]
    public class TurnAroundTimeController : ApiController
    {
        [HttpPost]
        [Route("GetRepotData")]
        public async Task<string> GetRepotData(TATInputModel input)
        {
            List<TurnAroundTimeReportModel> turnAroundTimeReportModelList = TurnAroundTimeHelper.GetTurnAroundTimeReportModelList();
            DataSet dataset = TurnAroundTimeHelper.GenerateDataSet(input, turnAroundTimeReportModelList);
            turnAroundTimeReportModelList = turnAroundTimeReportModelList.Where(x => x.DataTable != null).ToList();
            var excelFileName = DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
            var folderPath = HttpContext.Current.Server.MapPath(@"~\Reports\Downloads");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fullPhysicalPath = folderPath + "\\" + excelFileName;

            TurnAroundTimeHelper.DataSet_To_Excel(dataset, fullPhysicalPath);
            //string path = System.Web.Hosting.HostingEnvironment.MapPath("C:\\D\\abc.xlsx");
            //string path = "C:\\D\\abc.xlsx";
            //byte[] pdf = System.IO.File.ReadAllBytes(path);

            //HttpResponseMessage result = Request.CreateResponse(HttpStatusCode.OK);
            //result.Content = new ByteArrayContent(pdf);
            //result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
            //result.Content.Headers.ContentDisposition.FileName = "new.xlsx";
            //result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xlsx");

            var fileUrl = HttpContext.Current.Request.UrlReferrer.AbsoluteUri + "/Reports/Downloads/" + excelFileName;

            return fileUrl;
        }


        [HttpGet]
        [Route("ExportTATReport")]
        public async Task<string> ExportTATReport()
        {

            DataSet ds = null;
            DateTime now = DateTime.Now;
            var fileName = "";
            string path = "";
            TATInputModel input = new TATInputModel
            {

                DboyMobileNo = "",
                EndDate = now,
                StartDate = new DateTime(now.Year, now.Month, 1),
                SPList = new List<string> { "TurnAroundTime" },
                WarehouseID = 0
            };
            TurnAroundTimeHelper turnAroundTimeHelper = new TurnAroundTimeHelper();
            WarehouseController warehouseController = new WarehouseController();
            var warehoues = warehouseController.WhForWarkingCapital();
            List<int> warehouseIdList = warehoues.Select(x => x.WarehouseId).Distinct().ToList();
            if (warehouseIdList != null && warehouseIdList.Any())
            {
                foreach (var wh in warehouseIdList)
                {
                    input.WarehouseID = wh;
                    List<TurnAroundTimeReportModel> turnAroundTimeReportModelList = TurnAroundTimeHelper.GetTurnAroundTimeReportModelList();
                    DataSet dataset = TurnAroundTimeHelper.GenerateDataSet(input, turnAroundTimeReportModelList);
                    if (ds == null)
                    {
                        ds = dataset;
                    }
                    else
                    {
                        ds.Tables[0].Merge(dataset.Tables[0]);
                    }
                }

                //var excelFileName = DateTime.Now.ToString("ddMMyyyyHHmmss") + "ExportTatReport.csv";
                var folderPath = HttpContext.Current.Server.MapPath(@"~\Reports\Downloads");
                
                fileName = DateTime.Now.ToString("yyyyddMHHmmss") + "_ExportTATReport.csv";
                

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fullPhysicalPath = folderPath + "\\" + fileName;                
                Common.Helpers.DataTableExtensions.WriteToCsvFile(ds.Tables[0], fullPhysicalPath);
                if (!string.IsNullOrEmpty(fullPhysicalPath))
                {

                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='ExportTATReport'", connection))
                        {
                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " Export TAT Report";

                    #region Show data In Url
                    string FileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                  , HttpContext.Current.Request.Url.DnsSafeHost
                                                                  , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                  , "/Reports/Downloads/" + fileName);
                    #endregion
                    string message = "Please find attach Export TAT Report:" + FileUrl;
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(From, To, Bcc, subject, message, FileUrl);
                    else
                        return "Export TAT Report To and From empty";
                }
                //EmailHelper.SendMail("amit.jain@shopkirana.com", "amit.jain@shopkirana.com", "", "Daily TAT report", "Please find the attachment.", fullPhysicalPath);
            }
            return "";
        }

        [HttpPost]
        [Route("GetDataSet")]
        public IHttpActionResult GetDataSet(TATInputModel input)
        {

            List<TurnAroundTimeReportModel> turnAroundTimeReportModelList = TurnAroundTimeHelper.GetTurnAroundTimeReportModelList();

            DataSet dataset = TurnAroundTimeHelper.GenerateDataSet(input, turnAroundTimeReportModelList);
            turnAroundTimeReportModelList = turnAroundTimeReportModelList.Where(x => x.DataTable != null).ToList();
            return Ok(turnAroundTimeReportModelList);
        }


        [HttpGet]
        [Route("GetDboyList")]
        public IHttpActionResult GetDboyList(int warehouseID)
        {
            using (var authContext = new AuthContext())
            {
                string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.WarehouseId=" + warehouseID + " and r.Name='Delivery Boy' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                var list = authContext.Database.SqlQuery<People>(query).ToList();

                // var list = authContext.Peoples.Where(x => x.WarehouseId == warehouseID && x.Department == "Delivery Boy").ToList();
                return Ok(list);
            }
        }

    }
}
