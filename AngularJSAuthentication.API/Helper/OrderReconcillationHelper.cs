using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Gullak;
using GenricEcommers.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class OrderReconcillationHelper
    {
        public bool UpdatingSettledOrders()
        {
            bool res = false;
            using (var context = new AuthContext())
            {

                //var identity = User.Identity as ClaimsIdentity;
                //int userid = 0;

                //if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                //    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                DateTime dateTime = DateTime.Now;
                int userid = 1;
                List<OrderReconcillationDetailOrderWiseDC> orders = context.Database.SqlQuery<OrderReconcillationDetailOrderWiseDC>("exec GetSettledorder").ToList();
                //var gullakreferncenumbers = orders.Where(x => x.ModeOfPayment == "Gullak").Select(y => y.RefNo).ToList();
                //var ordersgullakdata = orders.Where(x => x.ModeOfPayment == "Gullak").Select(y => y.RefNo).ToList();
                //if (ordersgullakdata.Count() > 0 && ordersgullakdata.Any())
                //{
                //foreach (var d in ordersgullakdata)
                //{

                var gullakdata = context.Database.SqlQuery<GullakTransaction>("exec GetgullaktransactionListforo2o ").ToList();
                if (gullakdata.Count() > 0 && gullakdata.Any())
                {
                    foreach (var d1 in gullakdata)
                    {
                        OrderReconcillationFileUploadDetail od = new OrderReconcillationFileUploadDetail();
                        d1.IsUsed = true;
                        od.ModeOfPayment = "GullakInPayment";
                        od.TransactionAmount = d1.Amount;
                        od.RemainingAmount = d1.Amount;
                        od.SettledAmount = 0;
                        od.IsProcess = false;
                        od.Status = "Not Verified";
                        od.GullakID = d1.GullakId;
                        od.GullakTransactionId = d1.Id;
                        od.Comment = d1.Comment;
                        od.CreatedBy = userid;
                        od.CreatedDate = dateTime;
                        od.IsActive = true;
                        od.IsDeleted = false;
                        context.Entry(d1).State = System.Data.Entity.EntityState.Modified;
                        context.OrderReconcillationFileUploadDetails.Add(od);

                    }
                    context.Commit();
                }
                // }

                //}






                //var directudhardata = context.OrderReconcillationDirectUdharAmounts.Where(x => x.ModifiedBy == null).ToList();
                List<OrderReconcillationBankDetail> bankdatas = new List<OrderReconcillationBankDetail>();
                if (orders.Count > 0 && orders.Any())
                {
                    if (orders.Any(x => x.ModeOfPayment == "RTGS/NEFT"))
                    {
                        bankdatas = context.orderReconcillationBankDetails.Where(x => x.RemainingAmount > 0 && x.IsActive == true && x.IsDeleted == false && x.IsProcess == false && x.Type == "C" && x.ModeofPayment == "RTGS/NEFT").ToList();
                    }
                    foreach (var o in orders)
                    {
                        if (o.ModeOfPayment == "RTGS/NEFT") //directly verify to bank
                        {
                            OrderReconcillationHistory o1 = new OrderReconcillationHistory();
                            OrderReconcillationDetail ord = new OrderReconcillationDetail();
                            ord.OrderId = o.OrderId;
                            ord.Refno = o.RefNo;
                            ord.ModeofPayment = o.ModeOfPayment;
                            ord.TotalAmount = o.TotalAmount;
                            ord.OrderDate = o.OrderDate;
                            ord.TransactionDate = o.TransactionDate;
                            ord.PaymentResponseRetailerAppId = o.PaymentResponseRetailerId;
                            ord.CreatedDate = dateTime;
                            ord.CreatedBy = userid;
                            ord.IsActive = true;
                            ord.IsDeleted = false;
                            ord.WarehouseId = o.WarehouseId;
                            var data = bankdatas.FirstOrDefault(x => x.RefrenceNumber.Split(',').Contains(ord.Refno));
                            if (data != null)
                            {
                                if (data.RemainingAmount >= ord.TotalAmount)
                                {
                                    ord.orderReconcillationBankDetailId = data.Id;
                                    data.RemainingAmount -= ord.TotalAmount;
                                    data.SettledAmount += ord.TotalAmount;
                                    ord.RemainingAmount = 0;
                                    ord.IsProcess = ord.RemainingAmount == 0 ? true : false;
                                    ord.Status = ord.RemainingAmount == 0 ? "Verified" : "Not Verified";
                                    data.IsProcess = data.TransactionAmount == data.SettledAmount ? true : false;
                                    data.Status = data.TransactionAmount == data.SettledAmount ? "Verified" : "Not Verified";
                                    context.Entry(data).State = System.Data.Entity.EntityState.Modified;
                                }
                                else
                                {
                                    ord.RemainingAmount = ord.TotalAmount;
                                    ord.IsProcess = false;
                                    ord.Status = "Not Verified";
                                }
                            }
                            else
                            {
                                ord.RemainingAmount = ord.TotalAmount;
                                ord.IsProcess = false;
                                ord.Status = "Not Verified";
                            }
                            o1.OrderId = ord.OrderId;
                            o1.ModeofPayment = ord.ModeofPayment;
                            o1.Status = ord.Status;
                            o1.CreatedDate = dateTime;
                            o1.CreatedBy = userid;
                            o1.IsActive = true;
                            o1.IsDeleted = false;
                            context.orderReconcillationHistories.Add(o1);
                            context.OrderReconcillationDetails.Add(ord);
                        }
                        else if (o.ModeOfPayment == "Cheque") //directly verify to bank
                        {
                            string strrefno = o.RefNo.TrimStart(new Char[] { '0' });
                            OrderReconcillationHistory o1 = new OrderReconcillationHistory();
                            OrderReconcillationDetail ord = new OrderReconcillationDetail();
                            ord.OrderId = o.OrderId;
                            ord.Refno = o.RefNo;
                            ord.ModeofPayment = o.ModeOfPayment;
                            ord.TotalAmount = o.TotalAmount;
                            ord.OrderDate = o.OrderDate;
                            ord.TransactionDate = o.TransactionDate;
                            ord.PaymentResponseRetailerAppId = o.PaymentResponseRetailerId;
                            ord.CreatedDate = dateTime;
                            ord.CreatedBy = userid;
                            ord.IsActive = true;
                            ord.IsDeleted = false;
                            ord.WarehouseId = o.WarehouseId;
                            var data = context.orderReconcillationBankDetails.FirstOrDefault(x => x.RefrenceNumber == strrefno && x.IsActive == true && x.IsDeleted == false && x.IsProcess == false && x.ModeofPayment == "Cheque" && x.RemainingAmount > 0 && x.Type == "C");
                            if (data != null)
                            {
                                if (data.RemainingAmount >= ord.TotalAmount)
                                {
                                    ord.orderReconcillationBankDetailId = data.Id;
                                    data.RemainingAmount -= ord.TotalAmount;
                                    data.SettledAmount += ord.TotalAmount;
                                    ord.RemainingAmount = 0;
                                    ord.IsProcess = ord.RemainingAmount == 0 ? true : false;
                                    ord.Status = ord.RemainingAmount == 0 ? "Verified" : "Not Verified";
                                    data.IsProcess = data.TransactionAmount == data.SettledAmount ? true : false;
                                    data.Status = data.TransactionAmount == data.SettledAmount ? "Verified" : "Not Verified";
                                    context.Entry(data).State = System.Data.Entity.EntityState.Modified;
                                }
                                else
                                {
                                    ord.RemainingAmount = ord.TotalAmount;
                                    ord.IsProcess = false;
                                    ord.Status = "Not Verified";
                                }
                            }
                            else
                            {
                                ord.RemainingAmount = ord.TotalAmount;
                                ord.IsProcess = false;
                                ord.Status = "Not Verified";
                            }
                            o1.OrderId = ord.OrderId;
                            o1.ModeofPayment = ord.ModeofPayment;
                            o1.Status = ord.Status;
                            o1.CreatedDate = dateTime;
                            o1.CreatedBy = userid;
                            o1.IsDeleted = false;
                            o1.IsActive = true;
                            context.orderReconcillationHistories.Add(o1);
                            context.OrderReconcillationDetails.Add(ord);
                        }
                        else if (o.ModeOfPayment == "UPI") //first verify to mis then bank
                        {
                            OrderReconcillationHistory o1 = new OrderReconcillationHistory();
                            OrderReconcillationDetail ord = new OrderReconcillationDetail();
                            ord.OrderId = o.OrderId;
                            ord.Refno = o.RefNo;
                            ord.ModeofPayment = o.ModeOfPayment;
                            ord.TotalAmount = o.TotalAmount;
                            ord.OrderDate = o.OrderDate;
                            ord.TransactionDate = o.TransactionDate;
                            ord.PaymentResponseRetailerAppId = o.PaymentResponseRetailerId;
                            ord.CreatedDate = dateTime;
                            ord.CreatedBy = userid;
                            ord.IsActive = true;
                            ord.IsDeleted = false;
                            ord.WarehouseId = o.WarehouseId;
                            var data = context.OrderReconcillationFileUploadDetails.FirstOrDefault(x => x.RefNo == o.RefNo && x.IsActive == true && x.IsDeleted == false && x.RemainingAmount > 0 && x.ModeOfPayment == "UPI");
                            if (data != null)
                            {
                                if (data.RemainingAmount >= ord.TotalAmount)
                                {
                                    ord.OrderReconcillationFileUploadDetailId = data.Id;
                                    ord.RemainingAmount = 0;
                                    ord.IsProcess = true;
                                    ord.Status = "Partially Verified";
                                    data.RemainingAmount -= ord.TotalAmount;
                                    data.SettledAmount += ord.TotalAmount;
                                    data.IsProcess = data.TransactionAmount == data.SettledAmount ? true : false;
                                    //data.Status = data.TransactionAmount == data.SettledAmount ? "Verified" : "Not Verified";
                                    context.Entry(data).State = System.Data.Entity.EntityState.Modified;
                                }
                                else
                                {
                                    ord.RemainingAmount = ord.TotalAmount;
                                    ord.IsProcess = false;
                                    ord.Status = "Not Verified";
                                }

                            }
                            else
                            {
                                ord.RemainingAmount = ord.TotalAmount;
                                ord.IsProcess = false;
                                ord.Status = "Not Verified";
                            }
                            o1.OrderId = ord.OrderId;
                            o1.ModeofPayment = ord.ModeofPayment;
                            o1.Status = ord.Status;
                            o1.CreatedDate = dateTime;
                            o1.CreatedBy = userid;
                            o1.IsActive = true;
                            o1.IsDeleted = false;
                            context.orderReconcillationHistories.Add(o1);
                            context.OrderReconcillationDetails.Add(ord);
                        }
                        else if (o.ModeOfPayment == "Cash") //directly verify to bank
                        {
                            var datas = context.orderReconcillationBankDetails.Where(x => x.ModeofPayment == "Cash" && x.IsProcess == false && x.IsActive == true && x.IsDeleted == false && x.RemainingAmount > 0 && x.WarehouseId > 0 && x.Type == "C").ToList();
                            //double amount = o.TotalAmount;
                            var data = datas.Where(x => x.WarehouseId == o.WarehouseId && x.RemainingAmount > 0).ToList();
                            if (data.Any() && data.Count > 0)
                            {
                                double amount = o.TotalAmount;
                                foreach (var od in data)
                                {
                                    OrderReconcillationDetail ord = new OrderReconcillationDetail();
                                    OrderReconcillationHistory o1 = new OrderReconcillationHistory();

                                    if (amount > 0 && od.RemainingAmount > 0)
                                    {
                                        if (od.RemainingAmount >= amount)
                                        {
                                            ord.OrderId = o.OrderId;
                                            ord.Refno = o.RefNo;
                                            ord.ModeofPayment = o.ModeOfPayment;
                                            ord.TotalAmount = o.TotalAmount;
                                            ord.OrderDate = o.OrderDate;
                                            ord.TransactionDate = o.TransactionDate;
                                            ord.PaymentResponseRetailerAppId = o.PaymentResponseRetailerId;
                                            ord.orderReconcillationBankDetailId = od.Id;
                                            ord.CreatedDate = dateTime;
                                            ord.CreatedBy = userid;
                                            ord.IsActive = true;
                                            ord.IsDeleted = false;
                                            ord.WarehouseId = o.WarehouseId;
                                            ord.RemainingAmount = o.TotalAmount - amount;
                                            ord.IsProcess = true;
                                            ord.Status = "Verified";
                                            od.RemainingAmount -= amount;
                                            od.SettledAmount += amount;
                                            od.IsProcess = od.TransactionAmount == od.SettledAmount ? true : false;
                                            od.Status = od.TransactionAmount == od.SettledAmount ? "Verified" : "Not Verified";
                                            o1.OrderId = o.OrderId;
                                            o1.Status = ord.Status;
                                            o1.ModeofPayment = o.ModeOfPayment;
                                            o1.CreatedDate = dateTime;
                                            o1.CreatedBy = userid;
                                            o1.IsActive = true;
                                            o1.IsDeleted = false;
                                            amount -= amount;
                                            context.orderReconcillationHistories.Add(o1);
                                            context.OrderReconcillationDetails.Add(ord);
                                            context.Entry(od).State = System.Data.Entity.EntityState.Modified;

                                        }
                                        else
                                        {
                                            ord.OrderId = o.OrderId;
                                            ord.Refno = o.RefNo;
                                            ord.ModeofPayment = o.ModeOfPayment;
                                            ord.TotalAmount = o.TotalAmount;
                                            ord.OrderDate = o.OrderDate;
                                            ord.TransactionDate = o.TransactionDate;
                                            ord.PaymentResponseRetailerAppId = o.PaymentResponseRetailerId;
                                            ord.orderReconcillationBankDetailId = od.Id;
                                            ord.CreatedDate = dateTime;
                                            ord.CreatedBy = userid;
                                            ord.IsActive = true;
                                            ord.IsDeleted = false;
                                            ord.WarehouseId = o.WarehouseId;
                                            ord.RemainingAmount = amount > od.RemainingAmount ? amount - od.RemainingAmount : od.RemainingAmount - amount;
                                            ord.IsProcess = amount > od.RemainingAmount ? true : false;
                                            ord.Status = amount > od.RemainingAmount ? "Verified" : "Not Verified";
                                            od.SettledAmount += amount - ord.RemainingAmount;
                                            amount = amount > od.RemainingAmount ? amount - od.RemainingAmount : 0;
                                            od.RemainingAmount = 0; //od.RemainingAmount >= o.TotalAmount ? 0 : o.TotalAmount - od.RemainingAmount;//o.TotalAmount >= od.RemainingAmount ? o.TotalAmount : od.RemainingAmount;
                                                                    //od.RemainingAmount >= o.TotalAmount ? 0 : o.TotalAmount - od.RemainingAmount;//o.TotalAmount >= od.RemainingAmount ? o.TotalAmount : od.RemainingAmount;
                                            od.IsProcess = od.TransactionAmount == od.SettledAmount ? true : false;
                                            od.Status = od.TransactionAmount == od.SettledAmount ? "Verified" : "Not Verified";
                                            o1.OrderId = o.OrderId;
                                            o1.Status = ord.Status;
                                            o1.ModeofPayment = o.ModeOfPayment;
                                            o1.CreatedDate = dateTime;
                                            o1.CreatedBy = userid;
                                            o1.IsActive = true;
                                            o1.IsDeleted = false;
                                            context.orderReconcillationHistories.Add(o1);
                                            context.OrderReconcillationDetails.Add(ord);
                                            context.Entry(od).State = System.Data.Entity.EntityState.Modified;

                                        }
                                    }
                                    else
                                    {
                                        if (amount > 0)
                                        {
                                            OrderReconcillationHistory o2 = new OrderReconcillationHistory();
                                            OrderReconcillationDetail ords = new OrderReconcillationDetail();
                                            ords.OrderId = o.OrderId;
                                            ords.Refno = o.RefNo;
                                            ords.ModeofPayment = o.ModeOfPayment;
                                            ords.TotalAmount = amount;
                                            ords.OrderDate = o.OrderDate;
                                            ords.TransactionDate = o.TransactionDate;
                                            ords.PaymentResponseRetailerAppId = o.PaymentResponseRetailerId;
                                            ords.CreatedDate = dateTime;
                                            ords.CreatedBy = userid;
                                            ords.IsActive = true;
                                            ords.IsDeleted = false;
                                            ord.WarehouseId = o.WarehouseId;
                                            ords.RemainingAmount = amount;
                                            ords.IsProcess = false;
                                            ords.Status = "Not Verified";
                                            o2.OrderId = ord.OrderId;
                                            o2.ModeofPayment = ord.ModeofPayment;
                                            o2.Status = ord.Status;
                                            o2.CreatedDate = dateTime;
                                            o2.CreatedBy = userid;
                                            o2.IsActive = true;
                                            o2.IsDeleted = false;
                                            context.orderReconcillationHistories.Add(o2);
                                            context.OrderReconcillationDetails.Add(ords);

                                        }
                                    }
                                }
                                if (amount > 0)
                                {
                                    OrderReconcillationHistory o2 = new OrderReconcillationHistory();
                                    OrderReconcillationDetail ords = new OrderReconcillationDetail();
                                    ords.OrderId = o.OrderId;
                                    ords.Refno = o.RefNo;
                                    ords.ModeofPayment = o.ModeOfPayment;
                                    ords.TotalAmount = amount;
                                    ords.OrderDate = o.OrderDate;
                                    ords.TransactionDate = o.TransactionDate;
                                    ords.PaymentResponseRetailerAppId = o.PaymentResponseRetailerId;
                                    ords.CreatedDate = dateTime;
                                    ords.CreatedBy = userid;
                                    ords.IsActive = true;
                                    ords.IsDeleted = false;
                                    ords.WarehouseId = o.WarehouseId;
                                    ords.RemainingAmount = amount;
                                    ords.IsProcess = false;
                                    ords.Status = "Not Verified";
                                    o2.OrderId = ords.OrderId;
                                    o2.ModeofPayment = ords.ModeofPayment;
                                    o2.Status = ords.Status;
                                    o2.CreatedDate = dateTime;
                                    o2.CreatedBy = userid;
                                    o2.IsActive = true;
                                    o2.IsDeleted = false;
                                    context.orderReconcillationHistories.Add(o2);
                                    context.OrderReconcillationDetails.Add(ords);

                                }
                            }
                            else
                            {
                                OrderReconcillationHistory o1 = new OrderReconcillationHistory();
                                OrderReconcillationDetail ord = new OrderReconcillationDetail();
                                ord.OrderId = o.OrderId;
                                ord.Refno = o.RefNo;
                                ord.ModeofPayment = o.ModeOfPayment;
                                ord.TotalAmount = o.TotalAmount;
                                ord.OrderDate = o.OrderDate;
                                ord.TransactionDate = o.TransactionDate;
                                ord.PaymentResponseRetailerAppId = o.PaymentResponseRetailerId;
                                ord.CreatedDate = dateTime;
                                ord.CreatedBy = userid;
                                ord.IsActive = true;
                                ord.IsDeleted = false;
                                ord.RemainingAmount = ord.TotalAmount;
                                ord.IsProcess = false;
                                ord.WarehouseId = o.WarehouseId;
                                ord.Status = "Not Verified";
                                context.OrderReconcillationDetails.Add(ord);
                                o1.OrderId = ord.OrderId;
                                o1.Status = ord.Status;
                                o1.ModeofPayment = o.ModeOfPayment;
                                o1.CreatedDate = dateTime;
                                o1.CreatedBy = userid;
                                o1.IsActive = true;
                                o1.IsDeleted = false;
                                context.orderReconcillationHistories.Add(o1);

                            }
                        }
                        else if (o.ModeOfPayment == "hdfc") // first verify to mis then bank
                        {
                            OrderReconcillationHistory o1 = new OrderReconcillationHistory();
                            OrderReconcillationDetail ord = new OrderReconcillationDetail();
                            ord.OrderId = o.OrderId;
                            ord.Refno = o.RefNo;
                            ord.ModeofPayment = o.ModeOfPayment;
                            ord.TotalAmount = o.TotalAmount;
                            ord.OrderDate = o.OrderDate;
                            ord.TransactionDate = o.TransactionDate;
                            ord.PaymentResponseRetailerAppId = o.PaymentResponseRetailerId;
                            ord.CreatedDate = dateTime;
                            ord.CreatedBy = userid;
                            ord.IsActive = true;
                            ord.IsDeleted = false;
                            ord.WarehouseId = o.WarehouseId;
                            var data = context.OrderReconcillationFileUploadDetails.FirstOrDefault(x => x.OrderId == o.OrderId && x.IsActive == true && x.IsDeleted == false && x.RefNo == o.RefNo && x.RemainingAmount > 0 && (x.ModeOfPayment == "HDFC_CCDC" || x.ModeOfPayment == "HDFC_UPI" || x.ModeOfPayment == "HDFC_NetBanking"));
                            if (data != null)
                            {
                                if (data.RemainingAmount >= ord.TotalAmount)
                                {
                                    ord.OrderReconcillationFileUploadDetailId = data.Id;
                                    ord.RemainingAmount = 0;
                                    ord.IsProcess = true;
                                    ord.Status = "Partially Verified";
                                    data.RemainingAmount -= ord.TotalAmount;
                                    data.SettledAmount += ord.TotalAmount;
                                    data.IsProcess = data.TransactionAmount == data.SettledAmount ? true : false;
                                    //data.Status = data.TransactionAmount == data.SettledAmount ? "Verified" : "Not Verified";
                                    context.Entry(data).State = System.Data.Entity.EntityState.Modified;
                                }
                                else
                                {
                                    ord.RemainingAmount = ord.TotalAmount;
                                    ord.IsProcess = false;
                                    ord.Status = "Not Verified";
                                }

                            }
                            else
                            {
                                ord.RemainingAmount = ord.TotalAmount;
                                ord.IsProcess = false;
                                ord.Status = "Not Verified";
                            }
                            o1.OrderId = ord.OrderId;
                            o1.ModeofPayment = ord.ModeofPayment;
                            o1.Status = ord.Status;
                            o1.CreatedDate = dateTime;
                            o1.CreatedBy = userid;
                            o1.IsActive = true;
                            o1.IsDeleted = false;
                            context.orderReconcillationHistories.Add(o1);
                            context.OrderReconcillationDetails.Add(ord);
                        }
                        else if (o.ModeOfPayment == "Gullak")
                        {
                            double amount = o.TotalAmount;
                            double gullakidamount = 0;
                            var gullakorder = context.OrderReconcillationFileUploadDetails.Where(x => x.ModeOfPayment == "GullakInPayment" && x.IsActive == true && x.IsDeleted == false && x.IsProcess == false && x.RemainingAmount > 0).ToList();
                            var gullakidorder = gullakorder.Where(x => x.IsProcess = false && x.GullakID == o.GullakId && x.RemainingAmount > 0).ToList();
                            gullakidamount = gullakidorder.Sum(x => x.RemainingAmount);
                            if (amount > gullakidamount)
                            {
                                OrderReconcillationDetail ord = new OrderReconcillationDetail();
                                OrderReconcillationHistory o1 = new OrderReconcillationHistory();
                                ord.OrderId = o.OrderId;
                                ord.Refno = o.RefNo;
                                ord.ModeofPayment = o.ModeOfPayment;
                                ord.TotalAmount = o.TotalAmount;
                                ord.RemainingAmount = o.TotalAmount;
                                ord.OrderDate = o.OrderDate;
                                ord.PaymentResponseRetailerAppId = o.PaymentResponseRetailerId;
                                ord.IsProcess = false;
                                ord.Status = "Not Verified";
                                ord.WarehouseId = o.WarehouseId;
                                ord.IsActive = true;
                                ord.IsDeleted = false;
                                ord.CreatedBy = userid;
                                ord.CreatedDate = dateTime;
                                ord.WarehouseId = o.WarehouseId;
                                o1.OrderId = o.OrderId;
                                o1.Status = ord.Status;
                                o1.ModeofPayment = ord.ModeofPayment;
                                o1.CreatedDate = dateTime;
                                o1.CreatedBy = userid;
                                o1.IsActive = true;
                                o1.IsDeleted = false;
                                context.orderReconcillationHistories.Add(o1);
                                context.OrderReconcillationDetails.Add(ord);
                            }
                            else
                            {
                                foreach (var d in gullakidorder)
                                {
                                    OrderReconcillationDetail ord = new OrderReconcillationDetail();
                                    OrderReconcillationHistory o1 = new OrderReconcillationHistory();
                                    if (amount > 0)
                                    {
                                        if (d.RemainingAmount >= amount)
                                        {
                                            ord.OrderId = o.OrderId;
                                            ord.Refno = o.RefNo;
                                            ord.ModeofPayment = o.ModeOfPayment;
                                            ord.TotalAmount = amount;
                                            ord.RemainingAmount = 0;
                                            ord.OrderDate = o.OrderDate;
                                            ord.PaymentResponseRetailerAppId = o.PaymentResponseRetailerId;
                                            ord.OrderReconcillationFileUploadDetailId = d.Id;
                                            ord.IsProcess = true;
                                            ord.Status = "Verified";
                                            ord.WarehouseId = o.WarehouseId;
                                            ord.IsActive = true;
                                            ord.IsDeleted = false;
                                            ord.CreatedBy = userid;
                                            ord.CreatedDate = dateTime;
                                            ord.WarehouseId = o.WarehouseId;
                                            d.RemainingAmount -= amount;
                                            d.SettledAmount += amount;
                                            d.IsProcess = d.TransactionAmount == d.SettledAmount ? true : false;
                                            d.Status = d.TransactionAmount == d.SettledAmount ? "Verified" : "Not Verified";
                                            o1.OrderId = ord.OrderId;
                                            o1.Status = ord.Status;
                                            o1.ModeofPayment = ord.ModeofPayment;
                                            o1.CreatedDate = dateTime;
                                            o1.CreatedBy = userid;
                                            o1.IsActive = true;
                                            o1.IsDeleted = false;
                                            amount = 0;
                                            context.orderReconcillationHistories.Add(o1);
                                            context.OrderReconcillationDetails.Add(ord);
                                            context.Entry(d).State = System.Data.Entity.EntityState.Modified;
                                        }
                                        else
                                        {
                                            ord.OrderId = o.OrderId;
                                            ord.Refno = o.RefNo;
                                            ord.ModeofPayment = o.ModeOfPayment;
                                            ord.TotalAmount = o.TotalAmount;
                                            ord.RemainingAmount = o.TotalAmount - d.RemainingAmount;
                                            ord.OrderDate = o.OrderDate;
                                            ord.PaymentResponseRetailerAppId = o.PaymentResponseRetailerId;
                                            ord.OrderReconcillationFileUploadDetailId = d.Id;
                                            ord.IsProcess = true;
                                            ord.Status = "Verified";
                                            ord.WarehouseId = o.WarehouseId;
                                            ord.IsActive = true;
                                            ord.IsDeleted = false;
                                            ord.CreatedBy = userid;
                                            ord.CreatedDate = dateTime;
                                            ord.WarehouseId = o.WarehouseId;
                                            amount -= d.RemainingAmount;
                                            d.SettledAmount += d.RemainingAmount;
                                            d.RemainingAmount = 0;
                                            //d.SettledAmount += d.RemainingAmount;
                                            d.IsProcess = d.TransactionAmount == d.SettledAmount ? true : false;
                                            d.Status = d.TransactionAmount == d.SettledAmount ? "Verified" : "Not Verified";
                                            o1.OrderId = ord.OrderId;
                                            o1.Status = ord.Status;
                                            o1.ModeofPayment = ord.ModeofPayment;
                                            o1.CreatedDate = dateTime;
                                            o1.CreatedBy = userid;
                                            o1.IsActive = true;
                                            o1.IsDeleted = false;
                                            //amount -= d.RemainingAmount;
                                            context.orderReconcillationHistories.Add(o1);
                                            context.OrderReconcillationDetails.Add(ord);
                                            context.Entry(d).State = System.Data.Entity.EntityState.Modified;
                                        }
                                    }
                                }
                            }
                        }
                        else if (o.ModeOfPayment == "DirectUdhar")
                        {
                            OrderReconcillationHistory o1 = new OrderReconcillationHistory();
                            OrderReconcillationDetail ord = new OrderReconcillationDetail();
                            ord.OrderId = o.OrderId;
                            ord.Refno = o.RefNo;
                            ord.ModeofPayment = o.ModeOfPayment;
                            ord.TotalAmount = o.TotalAmount;
                            ord.OrderDate = o.OrderDate;
                            ord.TransactionDate = o.TransactionDate;
                            ord.PaymentResponseRetailerAppId = o.PaymentResponseRetailerId;
                            ord.CreatedDate = dateTime;
                            ord.CreatedBy = userid;
                            ord.IsActive = true;
                            ord.IsDeleted = false;
                            ord.WarehouseId = o.WarehouseId;
                            //var data = context.orderReconcillationBankDetails.FirstOrDefault(x => x.RefrenceNumber == o.RefNo && x.RemainingAmount >= o.TotalAmount && x.IsProcess == false);
                            //if (data != null)
                            //{
                            //    ord.RemainingAmount = 0;
                            //    ord.orderReconcillationBankDetailId = data.Id;
                            //    ord.IsProcess = true;
                            //    ord.Status = "Verified";
                            //    data.RemainingAmount -= ord.TotalAmount;
                            //    data.SettledAmount += ord.TotalAmount;
                            //    data.IsProcess = data.TransactionAmount == data.SettledAmount ? true : false;
                            //    data.Status = data.TransactionAmount == data.SettledAmount ? "Verified" : "Not Verified";
                            //    context.Entry(data).State = System.Data.Entity.EntityState.Modified;
                            //    context.OrderReconcillationDetails.Add(ord);
                            //}
                            //else
                            //{

                            var param1 = new SqlParameter("@transactionnumber", o.RefNo);
                            var odata = context.Database.SqlQuery<DirectUdharPaymentPaidDC>("exec Sp_directudharpaidcheck @transactionnumber", param1).FirstOrDefault();
                            if (odata != null && odata.amount >= ord.RemainingAmount)
                            {

                                ord.RemainingAmount = 0;
                                ord.OrderReconcillationFileUploadDetailId = odata.Id;
                                ord.IsProcess = true;
                                ord.Status = "Verified";
                                context.OrderReconcillationDetails.Add(ord);


                            }
                            else
                            {
                                ord.RemainingAmount = ord.TotalAmount;
                                ord.IsProcess = false;
                                ord.Status = "Not Verified";
                                context.OrderReconcillationDetails.Add(ord);

                            }
                            // }
                            o1.OrderId = ord.OrderId;
                            o1.ModeofPayment = ord.ModeofPayment;
                            o1.Status = ord.Status;
                            o1.CreatedDate = dateTime;
                            o1.CreatedBy = userid;
                            o1.IsDeleted = false;
                            o1.IsActive = true;
                            context.orderReconcillationHistories.Add(o1);
                        }
                        else
                        {
                            OrderReconcillationHistory o1 = new OrderReconcillationHistory();
                            OrderReconcillationDetail ord = new OrderReconcillationDetail();
                            ord.OrderId = o.OrderId;
                            ord.Refno = o.RefNo;
                            ord.ModeofPayment = o.ModeOfPayment;
                            ord.TotalAmount = o.TotalAmount;
                            ord.OrderDate = o.OrderDate;
                            ord.TransactionDate = o.TransactionDate;
                            ord.PaymentResponseRetailerAppId = o.PaymentResponseRetailerId;
                            ord.CreatedDate = dateTime;
                            ord.CreatedBy = userid;
                            ord.IsActive = true;
                            ord.IsDeleted = false;
                            ord.WarehouseId = o.WarehouseId;
                            ord.RemainingAmount = ord.TotalAmount;
                            ord.IsProcess = false;
                            ord.Status = "Not Verified";
                            o1.OrderId = ord.OrderId;
                            o1.ModeofPayment = ord.ModeofPayment;
                            o1.Status = ord.Status;
                            o1.CreatedBy = 1;
                            o1.CreatedDate = dateTime;
                            o1.IsActive = true;
                            o1.IsDeleted = false;
                            context.orderReconcillationHistories.Add(o1);
                            context.OrderReconcillationDetails.Add(ord);
                        }
                        context.Commit();
                        res = true;
                    }


                }
                else { res = false; }
            }
            return res;
        }

        public bool Notverifiedtoverifiedorders()
        {
            bool res = true;
            using (var context = new AuthContext())
            {
                DateTime dateTime = DateTime.Now;
                int userid = 1;
                List<PaymentResponseRetailerApp> paymentsdata = new List<PaymentResponseRetailerApp>();
                //DateTime dates = DateTime.Now; ;
                //DateTime date1 = DateTime.Now.AddDays(-19).Date;
                //DateTime date2 = DateTime.Now.AddDays(-18).Date;
                //DateTime date3 = date2.AddDays(1).Date;
                //List<OrderReconcillationDetail> orders = context.OrderReconcillationDetails.Where(x => x.RemainingAmount > 0 && x.IsActive == true && x.IsDeleted == false && x.IsProcess == false && x.Status == "Not Verified" && ((x.OrderDate >= date1 && x.OrderDate < date3) )).ToList();
                List<OrderReconcillationDetail> orders = context.OrderReconcillationDetails.Where(x => x.RemainingAmount > 0 && x.IsActive == true && x.IsDeleted == false && x.IsProcess == false && x.Status == "Not Verified" ).ToList();
                //var querys = "select o.*  from OrderReconcillationDetails o with(nolock) inner join OrderMasters om with(nolock) on o.OrderId=om.OrderId where o.IsActive=1 and o.IsDeleted=0 and o.RemainingAmount>0 and o.IsProcess=0 and o.Status='Not Verified' and cast(om.UpdatedDate as date)='2023-09-06' and ModeofPayment='Cash' ";
                //List<OrderReconcillationDetail> orders = context.Database.SqlQuery<OrderReconcillationDetail>(querys).ToList();
                //var orders = context.Database.SqlQuery<Notverifiedtoverifieddc>("exec Sp_NotVerifiedtoVerifiedorder").ToList();
                //var gullakorders = orders.Where(x => x.ModeofPayment == "Gullak").Select(x => x.PaymentResponseRetailerAppId).ToList();

                //if (gullakorders.Count > 0 && gullakorders.Any())
                //{
                //    paymentsdata = context.PaymentResponseRetailerAppDb.Where(x => gullakorders.Contains(x.id)).ToList();
                //}


                //var directudhardata = context.OrderReconcillationDirectUdharAmounts.Where(x => x.ModifiedBy == null && x.IsActive == true && x.IsDeleted == false).ToList();

                var gullakrefrencenumberdata = context.OrderReconcillationFileUploadDetails.Where(x => x.IsGullakVerified == false && x.IsActive == true && x.IsDeleted == false && (x.GullakReferenceNumber != null || x.GullakReferenceNumber != "")).ToList();
                var gullakinpaymentdata = context.OrderReconcillationFileUploadDetails.Where(x => x.ModeOfPayment == "GullakInPayment" && x.IsActive == true && x.IsDeleted == false).ToList();
                if (gullakrefrencenumberdata.Count() > 0 && gullakrefrencenumberdata.Any())
                {
                    foreach (var d in gullakrefrencenumberdata)
                    {
                        var data = gullakinpaymentdata.Where(x => x.RefNo == d.GullakReferenceNumber).FirstOrDefault();
                        if (data != null)
                        {
                            //d.IsProcess = true;
                            d.IsGullakVerified = true;
                            context.Entry(d).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                }
                List<UpdatePaymentDates> alldatesid = new List<UpdatePaymentDates>();
                //List<DirectUdharAmountDC> directUdharAmounts = new List<DirectUdharAmountDC>();
                //List<string> remainingref = context.Database.SqlQuery<string>("exec Sp_GetDirectUdharRefernceNumber").ToList();
                ////if (remainingref.Count > 0 && orders.Any())
                ////{
                ////List<string> directudharorder = orders.Where(x => x.ModeOfPayment == "DirectUdhar").Select(x => x.RefNo).ToList();
                ////directudharorder.AddRange(remainingref);
                //if (remainingref.Count > 0 && remainingref.Any())
                //{
                //    var transaction = new DataTable();
                //    transaction.Columns.Add("stringValue");
                //    foreach (var obj in remainingref)
                //    {
                //        var dr = transaction.NewRow();
                //        dr["stringValue"] = obj;
                //        transaction.Rows.Add(dr);
                //    }
                //    var param1 = new SqlParameter("@TransactionNumber", transaction);
                //    param1.SqlDbType = SqlDbType.Structured;
                //    param1.TypeName = "dbo.stringValues";

                //    directUdharAmounts = context.Database.SqlQuery<DirectUdharAmountDC>("exec Sp_GetDirectUdharAmount @TransactionNumber ", param1).ToList();
                //    if (directUdharAmounts.Count > 0 && directUdharAmounts.Any())
                //    {
                //        OrderReconcillationDirectUdharAmount ordu = new OrderReconcillationDirectUdharAmount();
                //        foreach (var obj in directUdharAmounts)
                //        {
                //            ordu.TransactionNumber = obj.TrasanctionId;
                //            ordu.Amount = obj.Amount;
                //            ordu.DirectUdharId = obj.Id;
                //            ordu.ModeofPayment = obj.MOP;
                //            ordu.TransactionDate = obj.CreatedDate;
                //            ordu.CreatedBy = userid;
                //            ordu.CreatedDate = dateTime;
                //            ordu.PaymentRefNo = obj.refno;
                //            ordu.IsActive = true;
                //            ordu.IsDeleted = false;
                //            ordu.Isprocess = false;
                //            ordu.Status = "Not Verified";
                //            ordu.OrderReconcillationDetailsId = 0;
                //            context.OrderReconcillationDirectUdharAmounts.Add(ordu);
                //        }
                //    }
                //    context.Commit();
                //}

                //}
                List<OrderReconcillationBankDetail> bankdatas = new List<OrderReconcillationBankDetail>();
                List<OrderReconcillationHistory> histories = new List<OrderReconcillationHistory>();
                List<OrderReconcillationDetail> orderdetail = new List<OrderReconcillationDetail>();
                List<OrderReconcillationFileUploadDetail> fileuploaddata = new List<OrderReconcillationFileUploadDetail>();
                List<OrderReconcillationFileUploadDetail> gullakdata = new List<OrderReconcillationFileUploadDetail>();
                if (orders.Any(x => x.ModeofPayment == "RTGS/NEFT" || x.ModeofPayment == "Cheque" || x.ModeofPayment== "Cash" || x.ModeofPayment == "Gullak"))
                {
                    bankdatas = context.orderReconcillationBankDetails.Where(x => x.RemainingAmount > 0 && x.IsActive == true && x.IsDeleted == false && x.IsProcess == false && x.Type == "C" ).ToList();
                }
                if (orders.Any(x=>x.ModeofPayment == "UPI" || x.ModeofPayment == "hdfc" || x.ModeofPayment == "Gullak"))
                {
                    fileuploaddata = context.OrderReconcillationFileUploadDetails.Where(x =>  x.IsActive == true && x.IsDeleted == false && x.RemainingAmount > 0 && (x.ModeOfPayment == "UPI" || x.ModeOfPayment == "HDFC_CCDC" || x.ModeOfPayment == "HDFC_UPI" || x.ModeOfPayment == "HDFC_NetBanking" || x.ModeOfPayment == "GullakInPayment")).ToList();
                }
                if(orders.Any(x=>x.ModeofPayment == "Gullak"))
                {
                    gullakdata = fileuploaddata.Where(x => x.ModeOfPayment == "GullakInPayment" && x.IsProcess == false && x.IsActive == true && x.IsDeleted == false && x.RemainingAmount > 0).ToList();
                }
                if (orders.Count > 0 && orders.Any())
                {
                    

                    foreach (var o in orders)
                    {
                        if (o.ModeofPayment == "RTGS/NEFT") //directly verify to bank
                        {
                            OrderReconcillationHistory o1 = new OrderReconcillationHistory();
                            
                            var data = bankdatas.FirstOrDefault(x => x.RefrenceNumber.Split(',').Contains(o.Refno) && x.ModeofPayment == "RTGS/NEFT");
                            if (data != null)
                            {
                                if (data.RemainingAmount >= o.RemainingAmount)
                                {
                                    
                                    o.orderReconcillationBankDetailId = data.Id;
                                    data.RemainingAmount -= o.RemainingAmount;
                                    data.SettledAmount += o.RemainingAmount;
                                    o.RemainingAmount = 0;
                                    o.IsProcess = o.RemainingAmount == 0 ? true : false;
                                    o.Status = o.RemainingAmount == 0 ? "Verified" : "Partially Verified";
                                    data.IsProcess = data.TransactionAmount == data.SettledAmount ? true : false;
                                    data.Status = data.TransactionAmount == data.SettledAmount ? "Verified" : "Not Verified";
                                    context.Entry(data).State = System.Data.Entity.EntityState.Modified;
                                    o.ModifiedBy = userid;
                                    o.ModifiedDate = dateTime;
                                    o1.OrderId = o.OrderId;
                                    o1.ModeofPayment = o.ModeofPayment;
                                    o1.Status = o.Status;
                                    o1.CreatedDate = dateTime;
                                    o1.CreatedBy = userid;
                                    o1.IsActive = true;
                                    o1.IsDeleted = false;
                                    histories.Add(o1);
                                    context.Entry(o).State = System.Data.Entity.EntityState.Modified;
                                    UpdatePaymentDates dates = new UpdatePaymentDates();
                                    dates.Id = o.Id;
                                    dates.Moq = o.ModeofPayment;
                                    alldatesid.Add(dates);
                                }
                            }
                        }
                        else if (o.ModeofPayment == "Cheque") //directly verify to bank
                        {
                            string strrefno = o.Refno.TrimStart(new Char[] { '0' });
                            OrderReconcillationHistory o2 = new OrderReconcillationHistory();
                            var data = bankdatas.FirstOrDefault(x => x.RefrenceNumber == strrefno && x.IsProcess == false && x.IsActive == true && x.IsDeleted == false && x.ModeofPayment == "Cheque" && x.RemainingAmount > 0 && x.Type == "C");
                            if (data != null)
                            {
                                if (data.RemainingAmount >= o.RemainingAmount)
                                {
                                    o.orderReconcillationBankDetailId = data.Id;
                                    data.RemainingAmount -= o.RemainingAmount;
                                    data.SettledAmount += o.RemainingAmount;
                                    o.RemainingAmount = 0;
                                    o.IsProcess = o.RemainingAmount == 0 ? true : false;
                                    o.Status = o.RemainingAmount == 0 ? "Verified" : "Partially Verified";
                                    data.IsProcess = data.TransactionAmount == data.SettledAmount ? true : false;
                                    data.Status = data.TransactionAmount == data.SettledAmount ? "Verified" : "Not Verified";
                                    context.Entry(data).State = System.Data.Entity.EntityState.Modified;
                                    o.ModifiedBy = userid;
                                    o.ModifiedDate = dateTime;
                                    o2.OrderId = o.OrderId;
                                    o2.ModeofPayment = o.ModeofPayment;
                                    o2.Status = o.Status;
                                    o2.CreatedDate = dateTime;
                                    o2.CreatedBy = userid;
                                    o2.IsDeleted = false;
                                    o2.IsActive = true;
                                    context.Entry(o).State = System.Data.Entity.EntityState.Modified;
                                    histories.Add(o2);
                                    UpdatePaymentDates dates = new UpdatePaymentDates();
                                    dates.Id = o.Id;
                                    dates.Moq = o.ModeofPayment;
                                    alldatesid.Add(dates);
                                }

                            }
                        }
                        else if (o.ModeofPayment == "UPI") //first verify to mis then bank
                        {
                            OrderReconcillationHistory o1 = new OrderReconcillationHistory();
                            var data = fileuploaddata.FirstOrDefault(x => x.RefNo == o.Refno && x.IsActive == true && x.IsDeleted == false && x.RemainingAmount > 0 && x.ModeOfPayment == "UPI");
                            if (data != null)
                            {
                                if (data.RemainingAmount >= o.RemainingAmount)
                                {
                                    o.OrderReconcillationFileUploadDetailId = data.Id;
                                    o.RemainingAmount = 0;
                                    o.IsProcess = o.RemainingAmount == 0 ? true : false;
                                    o.Status = o.RemainingAmount == 0 ? "Partially Verified" : "Not Verified";
                                    data.RemainingAmount -= o.RemainingAmount;
                                    data.SettledAmount += o.RemainingAmount;
                                    data.IsProcess = data.TransactionAmount == data.SettledAmount ? true : false;
                                    //data.Status = data.TransactionAmount == data.SettledAmount ? "Verified" : "Not Verified";
                                    context.Entry(data).State = System.Data.Entity.EntityState.Modified;
                                    o.ModifiedBy = userid;
                                    o.ModifiedDate = dateTime;
                                    o1.OrderId = o.OrderId;
                                    o1.ModeofPayment = o.ModeofPayment;
                                    o1.Status = o.Status;
                                    o1.CreatedDate = dateTime;
                                    o1.CreatedBy = userid;
                                    o1.IsActive = true;
                                    o1.IsDeleted = false;
                                    histories.Add(o1);
                                    context.Entry(o).State = System.Data.Entity.EntityState.Modified;
                                    UpdatePaymentDates dates = new UpdatePaymentDates();
                                    dates.Id = o.Id;
                                    dates.Moq = o.ModeofPayment;
                                    alldatesid.Add(dates);
                                }

                            }
                        }
                        else if (o.ModeofPayment == "hdfc") // first verify to mis then bank
                        {
                            OrderReconcillationHistory o1 = new OrderReconcillationHistory();
                            var data = fileuploaddata.FirstOrDefault(x => x.OrderId == o.OrderId && x.RefNo == o.Refno && x.IsActive == true && x.IsDeleted == false && x.RemainingAmount > 0 && (x.ModeOfPayment == "HDFC_CCDC" || x.ModeOfPayment == "HDFC_UPI" || x.ModeOfPayment == "HDFC_NetBanking"));
                            if (data != null)
                            {
                                if (data.RemainingAmount >= o.RemainingAmount)
                                {
                                    o.OrderReconcillationFileUploadDetailId = data.Id;
                                    o.RemainingAmount = 0;
                                    o.IsProcess = o.RemainingAmount == 0 ? true : false;
                                    o.Status = o.RemainingAmount == 0 ? "Partially Verified" : "Not Verified";
                                    data.RemainingAmount -= o.RemainingAmount;
                                    data.SettledAmount += o.RemainingAmount;
                                    data.IsProcess = data.TransactionAmount == data.SettledAmount ? true : false;
                                    //data.Status = data.TransactionAmount == data.SettledAmount ? "Verified" : "Not Verified";
                                    context.Entry(data).State = System.Data.Entity.EntityState.Modified;
                                    o.ModifiedBy = userid;
                                    o.ModifiedDate = dateTime;
                                    o1.OrderId = o.OrderId;
                                    o1.ModeofPayment = o.ModeofPayment;
                                    o1.Status = o.Status;
                                    o1.CreatedDate = dateTime;
                                    o1.CreatedBy = userid;
                                    o1.IsActive = true;
                                    o1.IsDeleted = false;
                                    context.Entry(o).State = System.Data.Entity.EntityState.Modified;
                                    histories.Add(o1);
                                    UpdatePaymentDates dates = new UpdatePaymentDates();
                                    dates.Id = o.Id;
                                    dates.Moq = o.ModeofPayment;
                                    alldatesid.Add(dates);
                                }

                            }
                        }
                        else if (o.ModeofPayment == "Cash")
                        {
                            var datas = bankdatas.Where(x => x.ModeofPayment == "Cash" && x.IsActive == true && x.IsDeleted == false && x.IsProcess == false && x.RemainingAmount > 0 && x.WarehouseId > 0 && x.Type == "C").ToList();
                            if (datas.Count > 0 && datas.Any())
                            {
                                var data = datas.Where(x => x.WarehouseId == o.WarehouseId && x.RemainingAmount > 0).ToList();
                                if (data.Any() && data.Count > 0)
                                {
                                    double amount = o.RemainingAmount;
                                    foreach (var od in data)
                                    {
                                        OrderReconcillationHistory o1 = new OrderReconcillationHistory();
                                        if (amount > 0 && od.RemainingAmount > 0)
                                        {
                                            if (od.RemainingAmount >= amount)
                                            {
                                                o.orderReconcillationBankDetailId = od.Id;
                                                o.ModifiedDate = dateTime;
                                                o.ModifiedBy = userid;
                                                o.RemainingAmount = o.RemainingAmount - amount;
                                                o.IsProcess = true;
                                                o.Status = "Verified";
                                                od.RemainingAmount -= amount;
                                                od.SettledAmount += amount;
                                                od.IsProcess = od.TransactionAmount == od.SettledAmount ? true : false;
                                                od.Status = od.TransactionAmount == od.SettledAmount ? "Verified" : "Not Verified";
                                                o1.OrderId = o.OrderId;
                                                o1.Status = o.Status;
                                                o1.ModeofPayment = o.ModeofPayment;
                                                o1.CreatedDate = dateTime;
                                                o1.CreatedBy = userid;
                                                o1.IsActive = true;
                                                o1.IsDeleted = false;
                                                amount = 0;
                                                histories.Add(o1);
                                                context.Entry(o).State = System.Data.Entity.EntityState.Modified;
                                                context.Entry(od).State = System.Data.Entity.EntityState.Modified;
                                                UpdatePaymentDates dates = new UpdatePaymentDates();
                                                dates.Id = o.Id;
                                                dates.Moq = o.ModeofPayment;
                                                alldatesid.Add(dates);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (o.ModeofPayment == "Gullak")
                        {
                            var query = "select top 1 isnull(g.Id,0) Id  from OrderMasters o with(nolock) inner join Gullaks g with(nolock) on o.CustomerId=g.CustomerId where o.OrderId=" + o.OrderId;
                            long id = context.Database.SqlQuery<long>(query).FirstOrDefault();
                            //string id = paymentsdata.FirstOrDefault(x => x.id == o.PaymentResponseRetailerAppId).GatewayOrderId;
                            long gullakid = Convert.ToInt64(id);
                            double amount = o.RemainingAmount;
                            double gullakidamount = 0;
                            //var gullakorder = context.OrderReconcillationFileUploadDetails.Where(x => x.ModeOfPayment == "GullakInPayment" && x.IsProcess == false && x.IsActive == true && x.IsDeleted == false && x.RemainingAmount > 0).ToList();
                            var gullakidorder = gullakdata.Where(x => x.IsProcess == false && x.GullakID == gullakid && x.RemainingAmount > 0).ToList();
                            gullakidamount = gullakidorder.Sum(x => x.RemainingAmount);
                            if (gullakidamount >= amount)
                            {
                                o.IsActive = false;
                                o.IsDeleted = true;
                                o.ModifiedBy = userid;
                                o.ModifiedDate = dateTime;
                                context.Entry(o).State = System.Data.Entity.EntityState.Modified;
                                foreach (var d in gullakidorder)
                                {
                                    OrderReconcillationDetail ord = new OrderReconcillationDetail();
                                    OrderReconcillationHistory o1 = new OrderReconcillationHistory();
                                    if (amount > 0)
                                    {
                                        if (d.RemainingAmount >= amount)
                                        {
                                            ord.OrderId = o.OrderId;
                                            ord.Refno = o.Refno;
                                            ord.ModeofPayment = o.ModeofPayment;
                                            ord.TotalAmount = amount;
                                            ord.RemainingAmount = 0;
                                            ord.OrderDate = o.OrderDate;
                                            ord.PaymentResponseRetailerAppId = o.PaymentResponseRetailerAppId;
                                            ord.OrderReconcillationFileUploadDetailId = d.Id;
                                            ord.IsProcess = true;
                                            ord.Status = "Verified";
                                            ord.WarehouseId = o.WarehouseId;
                                            ord.IsActive = true;
                                            ord.IsDeleted = false;
                                            ord.CreatedBy = userid;
                                            ord.CreatedDate = dateTime;
                                            ord.WarehouseId = o.WarehouseId;
                                            d.RemainingAmount -= amount;
                                            d.SettledAmount += amount;
                                            d.IsProcess = d.TransactionAmount == d.SettledAmount ? true : false;
                                            d.Status = d.TransactionAmount == d.SettledAmount ? "Verified" : "Not Verified";
                                            o1.OrderId = ord.OrderId;
                                            o1.Status = ord.Status;
                                            o1.ModeofPayment = ord.ModeofPayment;
                                            o1.CreatedDate = dateTime;
                                            o1.CreatedBy = userid;
                                            o1.IsActive = true;
                                            o1.IsDeleted = false;
                                            amount = 0;
                                            histories.Add(o1);
                                            orderdetail.Add(ord);
                                            context.Entry(d).State = System.Data.Entity.EntityState.Modified;
                                            UpdatePaymentDates dates = new UpdatePaymentDates();
                                            dates.Id = o.Id;
                                            dates.Moq = o.ModeofPayment;
                                            alldatesid.Add(dates);
                                        }
                                        else
                                        {
                                            ord.OrderId = o.OrderId;
                                            ord.Refno = o.Refno;
                                            ord.ModeofPayment = o.ModeofPayment;
                                            ord.TotalAmount = o.TotalAmount;
                                            ord.RemainingAmount = o.TotalAmount - d.RemainingAmount;
                                            ord.OrderDate = o.OrderDate;
                                            ord.PaymentResponseRetailerAppId = o.PaymentResponseRetailerAppId;
                                            ord.OrderReconcillationFileUploadDetailId = d.Id;
                                            ord.IsProcess = true;
                                            ord.Status = "Verified";
                                            ord.WarehouseId = o.WarehouseId;
                                            ord.IsActive = true;
                                            ord.IsDeleted = false;
                                            ord.CreatedBy = userid;
                                            ord.CreatedDate = dateTime;
                                            ord.WarehouseId = o.WarehouseId;
                                            amount -= d.RemainingAmount;
                                            d.SettledAmount += d.RemainingAmount;
                                            d.RemainingAmount = 0;
                                            d.IsProcess = d.TransactionAmount == d.SettledAmount ? true : false;
                                            d.Status = d.TransactionAmount == d.SettledAmount ? "Verified" : "Not Verified";
                                            o1.OrderId = ord.OrderId;
                                            o1.Status = ord.Status;
                                            o1.ModeofPayment = ord.ModeofPayment;
                                            o1.CreatedDate = dateTime;
                                            o1.CreatedBy = userid;
                                            o1.IsActive = true;
                                            o1.IsDeleted = false;

                                            histories.Add(o1);
                                            orderdetail.Add(ord);
                                            context.Entry(d).State = System.Data.Entity.EntityState.Modified;
                                            UpdatePaymentDates dates = new UpdatePaymentDates();
                                            dates.Id = o.Id;
                                            dates.Moq = o.ModeofPayment;
                                            alldatesid.Add(dates);
                                        }
                                    }
                                }

                            }
                        }
                        else if (o.ModeofPayment == "DirectUdhar")
                        {
                            
                            //var data = context.orderReconcillationBankDetails.FirstOrDefault(x => x.RefrenceNumber == o.Refno && x.RemainingAmount >= o.RemainingAmount && x.IsProcess == false);
                            //if (data != null)
                            //{
                            //    o.RemainingAmount = 0;
                            //    o.orderReconcillationBankDetailId = data.Id;
                            //    o.IsProcess = true;
                            //    o.Status = "Verified";
                            //    data.RemainingAmount -= o.TotalAmount;
                            //    data.SettledAmount += o.TotalAmount;
                            //    data.IsProcess = data.TransactionAmount == data.SettledAmount ? true : false;
                            //    data.Status = data.TransactionAmount == data.SettledAmount ? "Verified" : "Not Verified";
                            //    context.Entry(data).State = System.Data.Entity.EntityState.Modified;
                            //}
                            //else
                            //{
                            var param1 = new SqlParameter("@transactionnumber", o.Refno);
                            var odata = context.Database.SqlQuery<DirectUdharPaymentPaidDC>("exec Sp_directudharpaidcheck @transactionnumber", param1).FirstOrDefault();
                            if (odata != null && odata.amount >= o.RemainingAmount)
                            {
                                OrderReconcillationHistory o1 = new OrderReconcillationHistory();
                                //OrderReconcillationDetail ord = new OrderReconcillationDetail();
                                o.OrderId = o.OrderId;
                                o.Refno = o.Refno;
                                o.ModeofPayment = o.ModeofPayment;
                                o.TotalAmount = o.TotalAmount;
                                o.OrderDate = o.OrderDate;
                                o.TransactionDate = o.TransactionDate;
                                o.PaymentResponseRetailerAppId = o.PaymentResponseRetailerAppId;
                                o.ModifiedDate = dateTime;
                                o.ModifiedBy = userid;
                                o.IsActive = true;
                                o.IsDeleted = false;
                                o.WarehouseId = o.WarehouseId;
                                o.RemainingAmount = 0;
                                o.OrderReconcillationFileUploadDetailId = odata.Id;
                                o.IsProcess = true;
                                o.Status = "Verified";
                                UpdatePaymentDates dates = new UpdatePaymentDates();
                                dates.Id = o.Id;
                                dates.Moq = o.ModeofPayment;
                                alldatesid.Add(dates);
                                o1.OrderId = o.OrderId;
                                o1.ModeofPayment = o.ModeofPayment;
                                o1.Status = o.Status;
                                o1.CreatedDate = dateTime;
                                o1.CreatedBy = userid;
                                o1.IsDeleted = false;
                                o1.IsActive = true;
                                context.Entry(o).State = System.Data.Entity.EntityState.Modified;
                                histories.Add(o1);
                            }
                            //else
                            //{
                            //    o.IsProcess = false;
                            //    o.Status = "Not Verified";
                            //}
                            //}
                            
                        }
                        else
                        {

                        }
                        
                    }

                    if(histories.Count() > 0 && histories.Any())
                    {
                        context.orderReconcillationHistories.AddRange(histories);
                        if(orderdetail.Count() >0 && orderdetail.Any())
                        {
                            context.OrderReconcillationDetails.AddRange(orderdetail);
                        }
                        context.Commit();
                        OrderReconcillationController ctrl = new OrderReconcillationController();
                        res = ctrl.UpdatePaymentDate(alldatesid);
                    }
                }
                else { res = false; }
            }
            return res;
        }

        public bool MistoBankVerifieds()
        {
            bool res = false;
            DateTime cdate = DateTime.Now;
            int userid = 1;
            using (var context = new AuthContext())
            {
                List<MisDataDC> data = context.Database.SqlQuery<MisDataDC>("exec Sp_misreport").ToList();
                var datas = data.Count > 0 && data.Any() ? data.GroupBy(y => new { y.TransactionDate, y.TransactionAmount, y.ModeOfPayment, y.Status, y.IsProcess })
                    .Select(x => new
                    {
                        x.Key.TransactionAmount,
                        x.Key.TransactionDate,
                        x.Key.ModeOfPayment,
                        x.Key.IsProcess
                    }).ToList() : null;
                //var bankdatas = context.orderReconcillationBankDetails.Where(x => x.IsProcess == false && x.Type == "C" && (x.ModeofPayment == "HDFC_CCDC" || x.ModeofPayment == "HDFC_UPI" || x.ModeofPayment == "HDFC_NetBanking" || x.ModeofPayment == "UPI" || x.ModeofPayment == "DirectUdhaar_UPI" || x.ModeofPayment == "DirectUdhaar_Bank"));

                List<DirectUdharDataDC> directudhar = context.Database.SqlQuery<DirectUdharDataDC>("exec Sp_Directudharpayment").ToList();
                var directudhardatas = directudhar.Count > 0 && directudhar.Any() ? directudhar.GroupBy(y => new { y.TransactionDate, y.TransactionAmount, y.ModeOfPayment }).
                    Select(x => new
                    {
                        x.Key.TransactionAmount,
                        x.Key.TransactionDate,
                        x.Key.ModeOfPayment
                    }).ToList() : null;
                //var directudharlist = directudhar.Select(x => x.Id).ToList();
                //List<OrderReconcillationFileUploadDetail> directudharuploaddetail = context.OrderReconcillationFileUploadDetails.Where(x => x.IsActive == true && x.IsDeleted == false && directudharlist.Contains(x.Id)).ToList();
                var didlist = directudhar.Select(x => x.Id).ToList();
                List<OrderReconcillationDirectUdharAmount> damountlist = context.OrderReconcillationDirectUdharAmounts.Where(x => didlist.Contains(x.Id)).ToList();

                var idlist = data.Select(x => x.Id).ToList();
                List<OrderReconcillationFileUploadDetail> orderreconcilefileuploaddata = context.OrderReconcillationFileUploadDetails.Where(x => x.IsActive == true && idlist.Contains(x.Id)).ToList();

                if (datas.Any() && datas.Count > 0)
                {
                    foreach (var d in datas)
                    {
                        if (d.ModeOfPayment == "HDFC_CCDC" || d.ModeOfPayment == "HDFC_UPI" || d.ModeOfPayment == "HDFC_NetBanking" || d.ModeOfPayment == "UPI")
                        {
                            DateTime dates = d.TransactionDate.Date; ;
                            DateTime date1 = d.TransactionDate.AddDays(3).Date;
                            //DateTime date2 = DateTime.Now.AddDays(-18).Date;

                            var bankdata = context.orderReconcillationBankDetails.FirstOrDefault(x => x.IsActive == true && x.IsDeleted == false && x.IsProcess == false && x.Type == "C" && x.ModeofPayment == d.ModeOfPayment && x.RemainingAmount >= d.TransactionAmount && (x.TransactionDate >= dates && x.TransactionDate <= date1));
                            //bankdatas.FirstOrDefault(x => x.ModeofPayment == d.ModeOfPayment && x.RemainingAmount >= d.TransactionAmount && (x.TransactionDate >= dates && x.TransactionDate <= date1));
                            if (bankdata != null)
                            {
                                bankdata.RemainingAmount -= d.TransactionAmount;
                                bankdata.SettledAmount += d.TransactionAmount;
                                bankdata.IsProcess = bankdata.RemainingAmount == 0 ? true : false;
                                bankdata.Status = bankdata.RemainingAmount == 0 ? "Verified" : "Not Verified";
                                bankdata.ModifiedBy = userid;
                                bankdata.ModifiedDate = cdate;
                                context.Entry(bankdata).State = System.Data.Entity.EntityState.Modified;
                                var data1 = data.Where(x => x.ModeOfPayment == d.ModeOfPayment && x.TransactionAmount == d.TransactionAmount && x.TransactionDate == d.TransactionDate).ToList();
                                foreach (var di in data1)
                                {
                                    var up = context.OrderReconcillationDetails.Where(x => x.OrderReconcillationFileUploadDetailId == di.Id && x.IsProcess == true && x.OrderReconcillationFileUploadDetailId > 0).ToList();
                                    if (up.Count > 0 && up.Any())
                                    {
                                        foreach (var u in up)
                                        {
                                            OrderReconcillationHistory hist = new OrderReconcillationHistory();
                                            u.IsProcess = true;
                                            u.Status = "Verified";
                                            u.ModifiedBy = userid;
                                            u.ModifiedDate = cdate;
                                            u.orderReconcillationBankDetailId = bankdata.Id;
                                            hist.OrderId = u.OrderId;
                                            hist.ModeofPayment = u.ModeofPayment;
                                            hist.Status = u.Status;
                                            hist.CreatedDate = cdate;
                                            hist.CreatedBy = userid;
                                            hist.IsActive = true;
                                            hist.IsDeleted = false;
                                            context.orderReconcillationHistories.Add(hist);
                                            context.Entry(u).State = System.Data.Entity.EntityState.Modified;
                                        }
                                    }
                                    var filedata = orderreconcilefileuploaddata.Where(x => x.Id == di.Id).FirstOrDefault();
                                    if (filedata != null)
                                    {
                                        filedata.IsProcess = true;
                                        filedata.Status = "Verified";
                                        filedata.ModifiedBy = userid;
                                        filedata.ModifiedDate = cdate;
                                        filedata.BankDetailId = bankdata.Id;
                                        context.Entry(filedata).State = System.Data.Entity.EntityState.Modified;
                                    }
                                }
                            }
                        }
                        context.Commit();
                    }
                }

                if (directudhardatas.Count > 0 && directudhardatas.Any())
                {
                    foreach (var d in directudhardatas)
                    {
                        if (d.ModeOfPayment.ToUpper() == "UPI")
                        {
                            DateTime dates = d.TransactionDate.Date; ;
                            DateTime date1 = d.TransactionDate.AddDays(2).Date;
                            var bankdata = context.orderReconcillationBankDetails.FirstOrDefault(x => x.IsActive == true && x.IsDeleted == false && x.ModeofPayment == "DirectUdhaar_UPI" && x.RemainingAmount >= d.TransactionAmount && (x.TransactionDate >= dates && x.TransactionDate <= date1));
                            if (bankdata != null)
                            {
                                bankdata.RemainingAmount -= d.TransactionAmount;
                                bankdata.SettledAmount += d.TransactionAmount;
                                bankdata.IsProcess = bankdata.RemainingAmount == 0 ? true : false;
                                bankdata.Status = bankdata.RemainingAmount == 0 ? "Verified" : "Not Verified";
                                bankdata.ModifiedBy = userid;
                                bankdata.ModifiedDate = cdate;
                                context.Entry(bankdata).State = System.Data.Entity.EntityState.Modified;
                                var datalist = directudhar.Where(x => x.ModeOfPayment == d.ModeOfPayment && x.TransactionAmount == d.TransactionAmount && x.TransactionDate == d.TransactionDate).ToList();
                                foreach (var i in datalist)
                                {
                                    var data1 = damountlist.FirstOrDefault(x => x.Id == i.Id);
                                    if (data1 != null)
                                    {
                                        data1.Status = "Verified";
                                        data1.Isprocess = true;
                                        data1.OrderReconcillationDetailsId = bankdata.Id;
                                        context.Entry(data1).State = System.Data.Entity.EntityState.Modified;
                                    }
                                }
                            }
                        }
                        if (d.ModeOfPayment.ToUpper() == "NACH")
                        {
                            DateTime dates3 = d.TransactionDate.Date; ;
                            DateTime dates4 = d.TransactionDate.AddDays(2).Date;
                            var bankdata = context.orderReconcillationBankDetails.FirstOrDefault(x => x.IsActive == true && x.IsDeleted == false && x.ModeofPayment == "DirectUdhaar_Bank" && x.RemainingAmount >= d.TransactionAmount && (x.TransactionDate >= dates3 && x.TransactionDate <= dates4));
                            if (bankdata != null)
                            {
                                bankdata.RemainingAmount -= d.TransactionAmount;
                                bankdata.SettledAmount += d.TransactionAmount;
                                bankdata.IsProcess = bankdata.RemainingAmount == 0 ? true : false;
                                bankdata.Status = bankdata.RemainingAmount == 0 ? "Verified" : "Not Verified";
                                bankdata.ModifiedBy = userid;
                                bankdata.ModifiedDate = cdate;
                                context.Entry(bankdata).State = System.Data.Entity.EntityState.Modified;
                                var datalist = directudhar.Where(x => x.ModeOfPayment == d.ModeOfPayment && x.TransactionAmount == d.TransactionAmount && x.TransactionDate == d.TransactionDate).ToList();
                                foreach (var i in datalist)
                                {
                                    var data1 = damountlist.FirstOrDefault(x => x.Id == i.Id);
                                    if (data1 != null)
                                    {
                                        data1.Status = "Verified";
                                        data1.Isprocess = true;
                                        data1.OrderReconcillationDetailsId = bankdata.Id;
                                        context.Entry(data1).State = System.Data.Entity.EntityState.Modified;
                                    }
                                }
                            }
                        }
                        if (d.ModeOfPayment.ToUpper() == "NEFT")
                        {
                            var directudhardata = directudhar.Where(x => x.ModeOfPayment == d.ModeOfPayment && x.TransactionDate == d.TransactionDate && x.TransactionAmount == d.TransactionAmount).FirstOrDefault();
                            if (directudhardata != null)
                            {
                                var directudharrtgsdata = damountlist.FirstOrDefault(x => x.Id == directudhardata.Id);
                                if (directudharrtgsdata != null)
                                {
                                    var bankdata = context.orderReconcillationBankDetails.FirstOrDefault(x => x.IsActive == true && x.IsDeleted == false && x.RemainingAmount >= directudharrtgsdata.Amount && x.RefrenceNumber == directudharrtgsdata.PaymentRefNo);
                                    if (bankdata != null)
                                    {
                                        bankdata.RemainingAmount -= directudharrtgsdata.Amount;
                                        bankdata.SettledAmount += directudharrtgsdata.Amount;
                                        bankdata.IsProcess = bankdata.RemainingAmount == 0 ? true : false;
                                        bankdata.Status = bankdata.RemainingAmount == 0 ? "Verified" : "Not Verified";
                                        bankdata.ModifiedBy = userid;
                                        bankdata.ModifiedDate = cdate;
                                        context.Entry(bankdata).State = System.Data.Entity.EntityState.Modified;
                                        directudharrtgsdata.Isprocess = true;
                                        directudharrtgsdata.Status = "Verified";
                                        context.Entry(directudharrtgsdata).State = System.Data.Entity.EntityState.Modified;
                                    }
                                }
                            }

                        }
                    }
                    context.Commit();
                }

                if (datas.Count > 0 && datas.Any())
                {
                    OrderReconcillationHelper helper = new OrderReconcillationHelper();
                    res = helper.AutoMistoOrderVerify();
                }
                else
                {
                    res = false;
                }
                //if (context.Commit() > 0)
                //{


                //}
                //else
                //{
                //    res = false;
                //}
            }


            return res;
        }

        public bool AutoMistoOrderVerify()
        {
            using (AuthContext context = new AuthContext())
            {
                bool res = false;
                var orderfiledata = context.OrderReconcillationFileUploadDetails.Where(x => x.Status == "Verified" && x.IsActive == true && x.IsDeleted == false && (x.ModeOfPayment == "HDFC_CCDC" || x.ModeOfPayment == "HDFC_UPI" || x.ModeOfPayment == "HDFC_NetBanking" || x.ModeOfPayment == "UPI")).ToList();
                var orderdata = context.OrderReconcillationDetails.Where(x => x.IsActive == true && x.IsDeleted == false && x.OrderReconcillationFileUploadDetailId > 0 && x.IsProcess == true && (x.ModeofPayment == "hdfc" || x.ModeofPayment == "UPI") && (x.Status == "Partially Verified")).ToList();
                if (orderdata.Count > 0 && orderdata.Any())
                {
                    foreach (var o in orderdata)
                    {
                        var ispresent = orderfiledata.FirstOrDefault(x => x.Id == o.OrderReconcillationFileUploadDetailId);
                        if (ispresent != null)
                        {
                            OrderReconcillationHistory oh = new OrderReconcillationHistory();
                            o.ModifiedBy = 1;
                            o.ModifiedDate = DateTime.Now;
                            o.IsProcess = true;
                            o.Status = "Verified";
                            oh.OrderId = o.OrderId;
                            oh.ModeofPayment = o.ModeofPayment;
                            oh.Status = o.Status;
                            oh.CreatedDate = DateTime.Now;
                            oh.IsActive = true;
                            oh.IsDeleted = false;
                            context.orderReconcillationHistories.Add(oh);
                            context.Entry(o).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                }
                if (context.Commit() > 0)
                {
                    res = true;
                }
                else { res = true; }
                return res;
            }
        }

        //public bool MisVerified(AuthContext context)
        //{
        //    bool res = false;
        //    var data = context.OrderReconcillationFileUploadDetails.Where(x => x.IsActive == true && x.IsDeleted == false &&
        //    (x.ModeOfPayment == "HDFC_NetBanking" || x.ModeOfPayment == "UPI" || x.ModeOfPayment == "HDFC_CCDC" || x.ModeOfPayment == "HDFC_UPI")&& (x.IsProcess==true || x.Status=="Verified")).ToList();
        //    foreach(var d in data)
        //    {
        //        bool isverified = false;
        //        isverified = d.RemainingAmount == 0 ? true : false;
        //        if (isverified == true)
        //        {
        //            d.Status = "Verified";
        //            d.IsProcess = true;
        //            context.Entry(d).State = System.Data.Entity.EntityState.Modified;
        //        }
        //    }
        //    if(context.Commit() > 0)
        //    {
        //        res = true;
        //    }
        //    return res;
        //}

    }
    public class UploadBankFileDC
    {
        public DateTime TransactionDate { get; set; }
        public string TransactionDescription { get; set; }
        public string ReferenceNumber { get; set; }
        public DateTime ValueDate { get; set; }
        public double TransactionAmount { get; set; }
        public string WarehouseName { get; set; }
        public string Type { get; set; }
    }

    public class UploadUPIFileDC
    {
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public string TransactionStatus { get; set; }
        public string TransactionDescription { get; set; }
        public string MOP { get; set; }
        public string ReferenceNumber { get; set; }
        public string GullakReferenceNumber { get; set; }
        public double TransactionAmount { get; set; }
        public double RemainingAmount { get; set; }
        public double SettledAmount { get; set; }
        public int? OrderId { get; set; }
        public bool IsProcess { get; set; }
        public string Status { get; set; }
        public double Charges { get; set; }

        public int? WarehouseId { get; set; }
        public bool IsgullakVerified { get; set; }
    }
    public class OrderReconcillationHistoryDC
    {
        public int OrderId { get; set; }
        public string ModeOfPayment { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

    }
    public class OrderReconcillationDetailOrderWiseDC
    {
        public int OrderId { get; set; }
        public int WarehouseId { get; set; }
        public string ModeOfPayment { get; set; }
        public double TotalAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime OrderDate { get; set; }
        public string RefNo { get; set; }
        public int PaymentResponseRetailerId { get; set; }
        public long? GullakId { get; set; }

    }

    public class Notverifiedtoverifieddc
    {
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }

        public int OrderId { get; set; }
        public string Refno { get; set; }
        public string ModeofPayment { get; set; }
        public int WarehouseId { get; set; }
        public double TotalAmount { get; set; }
        public double RemainingAmount { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? TransactionDate { get; set; }
        public bool IsProcess { get; set; }
        public string Status { get; set; } //Verified , Not Verified ,Partially Verified
        public long? OrderReconcillationFileUploadDetailId { get; set; }
        public long? orderReconcillationBankDetailId { get; set; }
        public int? PaymentResponseRetailerAppId { get; set; }
        public string Comment { get; set; }
        public long? GullakID { get; set; }

    }
    public class MisDataDC
    {
        public long Id { get; set; }
        public double TransactionAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ModeOfPayment { get; set; }
        public bool IsProcess { get; set; }
        public string Status { get; set; }
    }
    public class DirectUdharDataDC
    {
        public long Id { get; set; }
        public double TransactionAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ModeOfPayment { get; set; }
        public bool IsProcess { get; set; }
        public string Status { get; set; }
    }
    public class DirectUdharAmountDC
    {
        public long Id { get; set; }
        public string TrasanctionId { get; set; }
        public string MOP { get; set; }
        public double Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string refno { get; set; }
    }
    public class DirectUdharPaymentPaidDC
    {
        public long Id { get; set; }
        public double amount { get; set; }
    }


    #region work by priyanka
    public class BankStatementFileDetailsListDC
    {
        public long Id { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string TransactionDescription { get; set; }
        public string RefrenceNumber { get; set; }
        public DateTime? ValueDate { get; set; }
        public double TransactionAmount { get; set; }
        public string Status { get; set; }
        public int TotalCount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Type { get; set; }
    }
    public class ShowReconcillationHistoryDC
    {
        public string Status { get; set; }
        public string ModeOfPayment { get; set; }
        public string UpdatedBy { get; set; }
        public int CreatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }

    }
    public class GetOrderToOrderReconcilationListDC
    {
        public long Id { get; set; }
        public string Skcode { get; set; }
        public string Refno { get; set; }
        public string WarehouseName { get; set; }
        public int OrderId { get; set; }
        public DateTime SettleDate { get; set; }
        public double InvoiceAmount { get; set; }
        public string ModeofPayment { get; set; }
        public double AmountReceived { get; set; }
        public string Reconstatus { get; set; }
        public string Status { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? PaymentReceivedDate { get; set; }
        public int TotalCount { get; set; }
    }
    public class OrderReconcillationfilterDc
    {
        public List<int> WarehouseIds { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string ReconStatus { get; set; }
        public string MOP { get; set; }
        public string keyword { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
    }

    public class DashboardfilterDC
    {
        public List<int> warehouseId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsMonth { get; set; }
        public bool IsToday { get; set; }
    }
    public class OrderToOrderReconciliationDashboardListDC
    {
        public string ModeOfPayment { get; set; }
        public string Status { get; set; }
        public double Amount { get; set; }
        public int Orders { get; set; }
        public int TotalOrders { get; set; }
        public double TotalAmount { get; set; }
        public int VerifiedOrder { get; set; }
        public double VerifiedAmount { get; set; }
        public int NotVerifiedOrder { get; set; }
        public double NotVerifiedAmount { get; set; }
        public int PartialVerifiedOrder { get; set; }
        public double PartialVerifiedAmount { get; set; }
        public double RefundAmount { get; set; }
        public double ExcessAmount { get; set; }

    }
    public class OrderToOrderReconciliationDashboardShowListDC
    {
        public double CashVerifiedAmount { get; set; }
        public int CashVerifiedOrder { get; set; }
        public double CashNotVerifiedAmount { get; set; }
        public int CashNotVerifiedOrder { get; set; }
        public double CashPartiallyVerifiedAmount { get; set; }
        public int CashPartiallyVerifiedOrder { get; set; }
        public double ChequeVerifiedAmount { get; set; }
        public int ChequeVerifiedOrder { get; set; }
        public double ChequeNotVerifiedAmount { get; set; }
        public int ChequeNotVerifiedOrder { get; set; }
        public double ChequePartiallyVerifiedAmount { get; set; }
        public int ChequePartiallyVerifiedOrder { get; set; }
        public double DirectUdharVerifiedAmount { get; set; }
        public int DirectUdharVerifiedOrder { get; set; }
        public double DirectUdharNotVerifiedAmount { get; set; }
        public int DirectUdharNotVerifiedOrder { get; set; }
        public double DirectUdharPartiallyVerifiedAmount { get; set; }
        public int DirectUdharPartiallyVerifiedOrder { get; set; }
        public double GullakVerifiedAmount { get; set; }
        public int GullakVerifiedOrder { get; set; }
        public double GullakNotVerifiedAmount { get; set; }
        public int GullakNotVerifiedOrder { get; set; }
        public double GullakPartiallyVerifiedAmount { get; set; }
        public int GullakPartiallyVerifiedOrder { get; set; }
        public double hdfcVerifiedAmount { get; set; }
        public int hdfcVerifiedOrder { get; set; }
        public double hdfcNotVerifiedAmount { get; set; }
        public int hdfcNotVerifiedOrder { get; set; }
        public double hdfcPartiallyVerifiedAmount { get; set; }
        public int hdfcPartiallyVerifiedOrder { get; set; }
        public double RTGS_NEFTVerifiedAmount { get; set; }
        public int RTGS_NEFTVerifiedOrder { get; set; }
        public double RTGS_NEFTNotVerifiedAmount { get; set; }
        public int RTGS_NEFTNotVerifiedOrder { get; set; }
        public double RTGS_NEFTPartiallyVerifiedAmount { get; set; }
        public int RTGS_NEFTPartiallyVerifiedOrder { get; set; }
        public double UPIVerifiedAmount { get; set; }
        public int UPIVerifiedOrder { get; set; }
        public double UPINotVerifiedAmount { get; set; }
        public int UPINotVerifiedOrder { get; set; }
        public double UPIPartiallyVerifiedAmount { get; set; }
        public int UPIPartiallyVerifiedOrder { get; set; }
        public int TotalOrders { get; set; }
        public double TotalAmount { get; set; }
        public int TotalVerifiedOrder { get; set; }
        public double TotalVerifiedAmount { get; set; }
        public int TotalNotVerifiedOrder { get; set; }
        public double TotalNotVerifiedAmount { get; set; }
        public int TotalPartialVerifiedOrder { get; set; }
        public double TotalPartialVerifiedAmount { get; set; }
        public double RefundAmount { get; set; }
        public double ExcessAmount { get; set; }
    }

    #endregion

    public class GetBankdetailPayload
    {
        public string Status { get; set; }
        public string Keyword { get; set; }
        public bool isExport { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public DateTime? Fromdate { get; set; }
        public DateTime? Todate { get; set; }
        public string Type { get; set; }
    }
    public class MisPayload
    {
        public string mop { get; set; }
        public string Status { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public DateTime? Fromdate { get; set; }
        public DateTime? Todate { get; set; }
        public string Keyword { get; set; }
        public bool IsExport { get; set; }

    }
    public class GetMisDetail
    {
        public DateTime TransactionDate { get; set; }
        public string ModeOfPayment { get; set; }
        public string TransactionId { get; set; }
        public string CreatedBy { get; set; }
        public long OrderId { get; set; }
        public int TotalCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string OrderStatus { get; set; }
        public string Status { get; set; }
        public double TransactionAmount { get; set; }
        public double OrderAmount { get; set; }
        public DateTime? OrderSettlmentDate { get; set; }
        public DateTime? BankSettlmentDate { get; set; }
    }
    public class GetMisToBankDetail
    {
        public string ModeoFPayment { get; set; }
        public double MisAmount { get; set; }
        public double MisCharges { get; set; }
        public double BankAmount { get; set; }
        public DateTime TransactionDate { get; set; }
    }
    public class ExportorderListData
    {
        public string OrderTypes { get; set; }
        public int OrderId { get; set; }
        public string OrderStatus { get; set; }
        public string Skcode { get; set; }
        public double TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string WarehouseName { get; set; }
        public int AssignmentNo { get; set; }
        public string AssignmentStatus { get; set; }
        public DateTime? AssignmentFreezeDate { get; set; }
        public DateTime? TaxInvoiceDate { get; set; }
        public DateTime? PocCreditNoteDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime SettlementDate { get; set; }
        public double DispatchedTotalAmt { get; set; }
        public double DeliveryCharge { get; set; }
        public double TCSAmount { get; set; }
        public double WalletAmount { get; set; }
        public double BillDiscountAmount { get; set; }
        public double Orderplacevalue { get; set; }
        public double IGSTTaxAmmount { get; set; }
        public double CGST_SGST_Amount { get; set; }
        public double CessTaxAmount { get; set; }
        public double TotalInvoiceValueBackend { get; set; }
        public string MOP { get; set; }
        public string MOPReference { get; set; }
        public double Cash { get; set; }
        public double hdfc { get; set; }
        public double Gullak { get; set; }
        public double RTGS_NEFT { get; set; }
        public double UPI { get; set; }
        public double Cheque { get; set; }
        public double DirectUdhar { get; set; }
        public string Paymentdate { get; set; }
        public string PaymentReceiveddate { get; set; }
        public double TotalAmountMIS { get; set; }
        public double Differencevalue { get; set; }
        public double Charges { get; set; }
        public double Refund { get; set; }
        public double Excess_Short { get; set; }
        public string Reconciliationstatus { get; set; }
        public string Verifiedby { get; set; }
        public string Verified_Date { get; set; }
        public string Comment { get; set; }
    }
    public class GetunPaymentData
    {
        public long Id { get; set; }
        public string ModeofPayment { get; set; }
    }
    public class TestcheckfileDC
    {
        public string ReferenceNumber { get; set; }
        public long OrderId { get; set; }
    }
    public class UpdatePaymentDates
    {
        public long Id { get; set; }
        public string Moq { get; set; }
    }
}