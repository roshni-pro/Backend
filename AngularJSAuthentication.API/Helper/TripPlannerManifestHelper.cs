using AngularJSAuthentication.DataContracts.Transaction.TripPlanner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class TripPlannerManifestHelper
    {

        private string _headerFilePath { get; set; }
        private string _bodyHtmlFilePath { get; set; }
        private string _bodyCSSFilePath { get; set; }

        public TripPlannerManifestHelper(string headerFilePath, string bodyHtmlFilePath, string bodyCssFilePath)
        {
            this._headerFilePath = headerFilePath;
            this._bodyHtmlFilePath = bodyHtmlFilePath;
            this._bodyCSSFilePath = bodyCssFilePath;
        }
        public string GetHeaderHtml(ShipmentManifestDc summary)
        {
            string headerHtml = "";
            string address = "";
            string gst = "";
            if (File.Exists(this._headerFilePath))
            {
                // Create a file to write to.
                headerHtml = File.ReadAllText(this._headerFilePath, Encoding.UTF8);
            }

            return headerHtml;

        }

        public string GetBodyHtml(ShipmentManifestDc summary)
        {
            string bodyHtml = "";

            if (File.Exists(this._bodyHtmlFilePath))
            {
                // Create a file to write to.
                bodyHtml = File.ReadAllText(this._bodyHtmlFilePath, Encoding.UTF8);
            }
            double tripDistance = ((double)summary.shipmentManifest.TotalEstimatedRoundtripKM) / 1000.00;
            bodyHtml = bodyHtml.Replace("##TRIPNO##", summary.shipmentManifest.TripNo.ToString());

            bodyHtml = bodyHtml.Replace("##TRIPDATE##", summary.shipmentManifest.Date);

            bodyHtml = bodyHtml.Replace("##TRIPTYPE##", summary.shipmentManifest.TripType);

            bodyHtml = bodyHtml.Replace("##HUB##", summary.shipmentManifest.WarehouseName);

            bodyHtml = bodyHtml.Replace("##NOOFASSIGNMENTS##", summary.shipmentManifest.NoOfAssignment.ToString());

            bodyHtml = bodyHtml.Replace("##TOTALLOAD##", summary.shipmentManifest.TotalLoadInKg.ToString());

            bodyHtml = bodyHtml.Replace("##TRIPDISTANCE##", tripDistance.ToString());

            bodyHtml = bodyHtml.Replace("##VEHICLENO##", summary.shipmentManifest.vehicleNo.ToString());

            bodyHtml = bodyHtml.Replace("##VEHICLETYPE##", summary.shipmentManifest.VehicleType.ToString());

            bodyHtml = bodyHtml.Replace("##VEHICLEINTIME##", summary.shipmentManifest.vehicleInTime.ToString("dd/MM/yyyy HH:mm:ss"));

            bodyHtml = bodyHtml.Replace("##VEHICLEKMREADING##", summary.shipmentManifest.VehicleKMReading.ToString());

            bodyHtml = bodyHtml.Replace("##AGENTNAME##", summary.shipmentManifest.AgentName);
            
            bodyHtml = bodyHtml.Replace("##DRIVERNAME##", summary.shipmentManifest.DriverName);
            
            bodyHtml = bodyHtml.Replace("##DRIVERNUMBER##", summary.shipmentManifest.DriverCellNo);
            
            bodyHtml = bodyHtml.Replace("##DBOYNAME##", summary.shipmentManifest.DrliveryBoyName);

            bodyHtml = bodyHtml.Replace("##DBOYNUMBER##", summary.shipmentManifest.DrliveryBoyNo);

            string assignmentRows = "";
            if (summary.Assignmentlist != null && summary.Assignmentlist.Count() > 0)
            {
                int rowNumber = 1;
                foreach (var assignment in summary.Assignmentlist)
                {
                    assignmentRows += @"<tr>"
                            + "<td>" + rowNumber.ToString() + "</td>"
                            + "<td>" + assignment.AssignmentId + "</td>"
                            + "<td>" + assignment.AssignmentValue + "</td>"
                            + "<td>" + assignment.AssignmentWeight + "</td>"
                            + "<td>" + (assignment.EWBApplicable ? "YES" : "NO") + "</td>"
                        + "</tr>";

                    rowNumber++;
                }
            }
            else
            {
                //assignmentRows = @"<td colspan="5" style ='text -aligh=center'>No record found</td>";
            }
            bodyHtml = bodyHtml.Replace("##ASSIGNMENTROWS##", assignmentRows);

            string ewbrows = "";
            if (summary.EWBDetails!= null && summary.EWBDetails.Count() > 0)
            {
                int rowNumber = 1;
                foreach (var ewBill in summary.EWBDetails)
                {
                    ewbrows += "<tr>"
                        + "<td>" + rowNumber.ToString() + "</td>"
                        + "<td>" + ewBill.OrderId.ToString() + "</td>"
                        + "<td>" + ewBill.OrderDate.ToString() + "</td>"
                        + "<td>" + ewBill.OrderValue.ToString() + "</td>"
                        + "<td>" + ewBill.EWBNo.ToString() + "</td>"
                        + "<td>" + ewBill.EWBDate.ToString() + "</td>"
                    + "</tr>";

                    rowNumber++;
                }            
            }
            else
            {
                //ewbrows = @"<td colspan='5' style ='text-aligh=center'>No record found</td>";
            }
            bodyHtml = bodyHtml.Replace("##EWBROWS##", ewbrows);


            return bodyHtml;
        }

        public string GetBodyCSS()
        {
            string css = "";
            if (File.Exists(this._bodyCSSFilePath))
            {
                // Create a file to write to.
                css = File.ReadAllText(this._bodyCSSFilePath, Encoding.UTF8);
            }

            return css;
        }
    }
}