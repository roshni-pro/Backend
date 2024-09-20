using AngularJSAuthentication.Model;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Newtonsoft.Json;
using NLog;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace AngularJSAuthentication.API.Controllers
{
    public class Sms
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
       
        public bool sendOtp(string mn, string msg,string DLTId)
        {
            //string authKey = Startup.smsauthKey;

            ////Multiple mobiles numbers separated by comma
            //string mobileNumber = mn; //"9999999";
            //                          //Sender ID,While using route4 sender id should be 6 characters long.
            //                          //string senderId = "Moreye";
            //string senderId = "SHOPKI";
            ////Your message to send, Add URL encoding here.
            //string message = HttpUtility.UrlEncode(msg);

            ////Prepare you post parameters
            //StringBuilder sbPostData = new StringBuilder();
            //sbPostData.AppendFormat("authkey={0}", authKey);
            //sbPostData.AppendFormat("&mobiles={0}", mobileNumber);
            //sbPostData.AppendFormat("&message={0}", message);
            //sbPostData.AppendFormat("&sender={0}", senderId);
            //sbPostData.AppendFormat("&route={0}", "4");
            //sbPostData.AppendFormat("&country={0}", "91");
            //try
            //{
            //    //Call Send SMS API
            //    //string sendSMSUri = "http://sms.o2technology.in/api/sendhttp.php";
            //    string sendSMSUri = "http://bulksms.newrise.in/api/sendhttp.php";
            //    //Create HTTPWebrequest
            //    HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(sendSMSUri);
            //    //Prepare and Add URL Encoded data
            //    UTF8Encoding encoding = new UTF8Encoding();
            //    byte[] data = encoding.GetBytes(sbPostData.ToString());
            //    //Specify post method
            //    httpWReq.Method = "POST";
            //    httpWReq.ContentType = "application/x-www-form-urlencoded";
            //    httpWReq.ContentLength = data.Length;
            //    using (Stream stream = httpWReq.GetRequestStream())
            //    {
            //        stream.Write(data, 0, data.Length);
            //    }
            //    //Get the response
            //    HttpWebResponse response = (HttpWebResponse)httpWReq.GetResponse();
            //    StreamReader reader = new StreamReader(response.GetResponseStream());
            //    string responseString = reader.ReadToEnd();

            //    //Close the response
            //    reader.Close();
            //    response.Close();
            //}
            //catch (SystemException ex)
            //{
            //    logger.Error("Error while sending Password on " + mn + "  : " + ex.ToString());
            //}
            bool result = Common.Helpers.SendSMSHelper.SendSMS(mn, msg, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), DLTId);
            return true;
        }

        public bool createPdf(SendPdfDto2 obj, int PurchaseOrderId)
        {
            using (AuthContext db = new AuthContext())
            {
                Supplier sup = db.Suppliers.Where(q => q.SupplierId == obj.p.SupplierId).FirstOrDefault();
                Warehouse wh = db.Warehouses.Where(q => q.WarehouseId == obj.p.WarehouseId).FirstOrDefault();
                Document document = new Document(PageSize.A4.Rotate(), 43, 43, 43, 43);
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(@"C:\Test\" + PurchaseOrderId + ".pdf", FileMode.Create));
                Pharsecell p = new Pharsecell();
                PdfPTable table = null;
                document.Open();
                Chunk glue = new Chunk(new VerticalPositionMark());
                Paragraph para = new Paragraph();
                table = new PdfPTable(1);
                table.TotalWidth = 380f;
                table.LockedWidth = true;
                table.SpacingBefore = 20f;
                table.HorizontalAlignment = Element.ALIGN_CENTER;
                Phrase Head = new Phrase();
                Paragraph head1 = new Paragraph();
                Head.Add(new Chunk(Environment.NewLine));
                Head.Add(new Chunk("Shop Kirana Purchase Order", FontFactory.GetFont("Arial", 20, 1)));
                head1.Alignment = Element.ALIGN_CENTER;
                //Head.Add(new Chunk(Environment.NewLine));            
                head1.Add(Head);
                para.Add(head1);

                Phrase ph1 = new Phrase();
                Paragraph mm = new Paragraph();
                ph1.Add(new Chunk(Environment.NewLine));
                ph1.Add(new Chunk("SupplierName: " + obj.p.SupplierName + "", FontFactory.GetFont("Arial", 10, 1)));
                ph1.Add(glue);
                ph1.Add(new Chunk("Supplier Mob: " + sup.MobileNo + "", FontFactory.GetFont("Arial", 10, 1)));
                ph1.Add(glue);
                ph1.Add(new Chunk(Environment.NewLine));
                ph1.Add(new Chunk("Supplier Address: " + sup.SupplierAddress + "", FontFactory.GetFont("Arial", 10, 1)));
                ph1.Add(glue);
                ph1.Add(new Chunk(Environment.NewLine));
                ph1.Add(new Chunk("Purchase OrderId.:  " + obj.p.PurchaseOrderId + " ", FontFactory.GetFont("Arial", 10, 1)));
                ph1.Add(new Chunk(Environment.NewLine));
                ph1.Add(new Chunk("Warehouse Name:" + wh.WarehouseName + " ", FontFactory.GetFont("Arial", 10, 1)));
                ph1.Add(glue);
                ph1.Add(new Chunk(Environment.NewLine));
                ph1.Add(new Chunk("Warehouse Address:" + wh.Address + " ", FontFactory.GetFont("Arial", 10, 1)));
                ph1.Add(glue);
                ph1.Add(new Chunk(Environment.NewLine));
                ph1.Add(new Chunk("PO Created Date:" + obj.p.CreationDate + "                 ", FontFactory.GetFont("Arial", 10, 1)));
                ph1.Add(glue);
                ph1.Add(new Chunk(Environment.NewLine));
                ph1.Add(new Chunk("Create By:" + obj.p.CreatedBy + "       ", FontFactory.GetFont("Arial", 10, 1)));
                ph1.Add(glue);
                ph1.Add(new Chunk("Approved By:" + obj.p.ApprovedBy + " ", FontFactory.GetFont("Arial", 10, 1)));
                ph1.Add(new Chunk(Environment.NewLine));
                ph1.Add(new Chunk(" ", FontFactory.GetFont("Arial", 10, 1)));
                mm.Add(ph1);
                para.Add(mm);
                Phrase ph5 = new Phrase();
                Paragraph mmc = new Paragraph();
                ph5.Add(new Chunk(" ", FontFactory.GetFont("Arial", 10, 1)));
                mmc.Add(ph5);
                para.Add(mmc);

                document.Add(para);
                table = new PdfPTable(7);
                PdfPCell cell2 = new PdfPCell(new Phrase("PO order item detail"));
                cell2.Colspan = 7;
                cell2.HorizontalAlignment = 1;

                table.AddCell(cell2);
                table.AddCell("Item Id");
                table.AddCell("Item Name");
                table.AddCell("Purchase SKU");
                table.AddCell("Price");
                table.AddCell("MPQ");
                table.AddCell("Total Quantity");
                table.AddCell("Total Amount");

                foreach (var item in obj.pd)
                {
                    //cell.Colspan = 1;
                    //cell.PaddingBottom = 10f;
                    //table.AddCell(cell);
                    table.AddCell(item.ItemId.ToString());
                    table.AddCell(item.ItemName);
                    table.AddCell(item.PurchaseSku);
                    table.AddCell(item.Price.ToString());
                    table.AddCell(item.MOQ.ToString());
                    table.AddCell(item.TotalQuantity.ToString());
                    table.AddCell((item.Price * item.TotalQuantity).ToString());
                }
                document.Add(table);
                Paragraph para1 = new Paragraph();
                Phrase ph2 = new Phrase();
                Paragraph mm1 = new Paragraph();
                ph2.Add(new Chunk("Total Amount:" + obj.p.ETotalAmount, FontFactory.GetFont("Arial", 10, 1)));
                mm1.Add(ph2);
                mm1.Alignment = Element.ALIGN_RIGHT;
                para1.Add(mm1);
                document.Add(para1);

                Paragraph para3 = new Paragraph();
                Phrase ph3 = new Phrase();
                Paragraph mm3 = new Paragraph();
                ph3.Add(new Chunk("Supplier Sign.", FontFactory.GetFont("Arial", 10, 1)));
                ph3.Add(glue);
                mm3.Alignment = Element.ALIGN_RIGHT;
                mm3.Add(ph3);
                para3.Add(mm3);
                document.Add(para3);
                document.Close();
                return true;
            }
        }

        public bool SupplierSendNotification(string FCMId, string FCMNotification)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(FCMId) && !string.IsNullOrEmpty(FCMNotification))
            {
                DataContracts.Transaction.AutoNotification.FcmNotificationMaster fcmNotificationMaster = JsonConvert.DeserializeObject<DataContracts.Transaction.AutoNotification.FcmNotificationMaster>(FCMNotification);
                DataContracts.Transaction.AutoNotification.FcmNotification notifiation = new DataContracts.Transaction.AutoNotification.FcmNotification { data = fcmNotificationMaster, to = FCMId };

                string Key = ConfigurationManager.AppSettings["SupplierFcmApiKey"];
                string id = ConfigurationManager.AppSettings["SupplierFcmApiId"];

                WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                tRequest.Method = "post";

                string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(notifiation);
                Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                tRequest.Headers.Add(string.Format("Sender: id={0}", id));
                tRequest.ContentLength = byteArray.Length;
                tRequest.ContentType = "application/json";
                using (Stream dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    using (WebResponse tResponse = tRequest.GetResponse())
                    {
                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {
                            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {
                                String responseFromFirebaseServer = tReader.ReadToEnd();

                                NotificationController.FCMResponse response = JsonConvert.DeserializeObject<NotificationController.FCMResponse>(responseFromFirebaseServer);

                                if (response.success == 1)
                                {
                                    result = true;
                                }
                                else
                                {
                                    result = false;                                   
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
    public class Pharsecell
    {
        public PdfPCell Pc(Phrase phrase, int align)
        {
            PdfPCell cell = new PdfPCell(phrase);
            //cell.BorderColor = System.Drawing.Color.WHITE;
            //cell.VerticalAlignment = PdfCell.ALIGN_TOP;
            cell.HorizontalAlignment = align;
            cell.PaddingBottom = 2f;
            cell.PaddingTop = 0f;
            return cell;
        }
    }
}