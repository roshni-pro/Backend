using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/SalesSettlement")]
    public class SalesSettlementController : ApiController
    {
       
        [Route("saless")]
        [HttpGet]
        public PaggingData salessettlement(int list, int page)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                using (var db = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {
                        PaggingData data = new PaggingData();
                        var total_count = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.WarehouseId == Warehouse_id && (x.Status == "sattled" || x.Status == "Partial settled" || x.Status == "Partial receiving -Bounce")).Count();
                        var ordermaster = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.WarehouseId == Warehouse_id && (x.Status == "sattled" || x.Status == "Partial settled" || x.Status == "Partial receiving -Bounce")).OrderByDescending(x => x.OrderId).Skip((page - 1) * list).Take(list).Include("orderDetails").ToList();
                        data.ordermaster = ordermaster;
                        data.total_count = total_count;
                        return data;
                    }
                    else
                    {
                        PaggingData data = new PaggingData();
                        var total_count = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.CompanyId == compid && (x.Status == "sattled" || x.Status == "Partial settled" || x.Status == "Partial receiving -Bounce")).Count();
                        var ordermaster = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.CompanyId == compid && (x.Status == "sattled" || x.Status == "Partial settled" || x.Status == "Partial receiving -Bounce")).OrderByDescending(x => x.OrderId).Skip((page - 1) * list).Take(list).Include("orderDetails").ToList();
                        data.ordermaster = ordermaster;
                        data.total_count = total_count;
                        return data;
                    }
                }


            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [Route("exportall")]
        [HttpGet]
        public HttpResponseMessage get() //get all export sales settlemt 
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                using (var db = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {
                        var saleSorder = db.OrderDispatchedMasters.Where(x => x.WarehouseId == Warehouse_id && x.Status == "sattled" || x.Status == "Partial settled" || x.Status == "Partial receiving -Bounce").ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, saleSorder);
                    }
                    else
                    {
                        var saleSorder = db.OrderDispatchedMasters.Where(x => x.CompanyId == compid && x.Status == "sattled" || x.Status == "Partial settled" || x.Status == "Partial receiving -Bounce").ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, saleSorder);
                    }
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [Route("search")]
        [HttpGet]
        public dynamic search(DateTime? start, DateTime? end, int? OrderId, double? totalAmount)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                using (var db = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {
                        if (OrderId != 0 && OrderId > 0)
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.WarehouseId == Warehouse_id && (x.OrderId == OrderId) && (x.Status == "sattled" || x.Status == "Partial settled" || x.Status == "Partial receiving -Bounce")).ToList();

                            return data;
                        }
                        else if ((OrderId > 0) && start != null)
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.WarehouseId == Warehouse_id && (x.OrderId == OrderId) && (x.Status == "sattled" || x.Status == "Partial settled" || x.Status == "Partial receiving -Bounce") && (x.CreatedDate > start && x.CreatedDate <= end)).ToList();

                            return data;
                        }
                        else if (totalAmount != 0 && totalAmount > 0)
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.WarehouseId == Warehouse_id && (x.GrossAmount == totalAmount) && (x.Status == "sattled" || x.Status == "Partial settled" || x.Status == "Partial receiving -Bounce")).ToList();

                            return data;
                        }



                        else
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.CompanyId == compid && (x.Status == "sattled" || x.Status == "Partial settled" || x.Status == "Partial receiving -Bounce") && (x.CreatedDate > start && x.CreatedDate < end)).ToList();

                            return data;
                        }
                    }
                    else
                    {
                        if (OrderId != 0 && OrderId > 0)
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.CompanyId == compid && (x.OrderId == OrderId) && (x.Status == "sattled" || x.Status == "Partial settled" || x.Status == "Partial receiving -Bounce")).ToList();

                            return data;
                        }
                        else if ((OrderId > 0) && start != null)
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.CompanyId == compid && (x.OrderId == OrderId) && (x.Status == "sattled" || x.Status == "Partial settled" || x.Status == "Partial receiving -Bounce") && (x.CreatedDate > start && x.CreatedDate <= end)).ToList();

                            return data;
                        }
                        else if (totalAmount != 0 && totalAmount > 0)
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.CompanyId == compid && (x.GrossAmount == totalAmount) && (x.Status == "sattled" || x.Status == "Partial settled" || x.Status == "Partial receiving -Bounce")).ToList();

                            return data;
                        }
                        else
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.CompanyId == compid && (x.Status == "sattled" || x.Status == "Partial settled" || x.Status == "Partial receiving -Bounce") && (x.CreatedDate > start && x.CreatedDate < end)).ToList();

                            return data;
                        }
                    }
                }


            }
            catch (Exception ex)
            {

                return false;
            }
        }

        [Route("cashstatus")]
        [HttpGet, HttpPut]
        public dynamic cashstatus(OrderDispatchedMaster data)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }

                data.CompanyId = compid;
                data.WarehouseId = Warehouse_id;
                using (var db = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {

                        if (data == null)
                        {
                            throw new ArgumentNullException(" null");
                        }
                        var comp = db.OrderDispatchedMasters.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                        if (data.CheckAmount == 0 && data.ElectronicAmount == 0)
                        {
                            comp.cash = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }
                        }
                        else if (data.CheckAmount == 0 && data.ElectronicAmount != 0 && data.electronic == true)
                        {
                            comp.cash = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();


                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }

                        }
                        else if (data.CheckAmount != 0 && data.ElectronicAmount == 0 && data.cheq == true)
                        {
                            comp.cash = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }
                        }
                        else if (data.CheckAmount != 0 && data.ElectronicAmount != 0 && data.cheq == true && data.electronic == true)
                        {
                            comp.cash = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }
                        }
                        else
                        {
                            comp.cash = true;
                            comp.Status = "Partial settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                obj.Status = "Partial settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }
                        }
                        #region Order Master History

                        try
                        {
                            var UserName = db.Peoples.Where(x => x.PeopleID == userid).Select(a => a.DisplayName).SingleOrDefault();

                            OrderMasterHistories h1 = new OrderMasterHistories();

                            h1.orderid = comp.OrderId;
                            h1.Status = comp.Status;
                            h1.Reasoncancel = "Settled";
                            h1.Warehousename = comp.WarehouseName;
                            h1.userid = userid;
                            h1.username = UserName;
                            h1.CreatedDate = DateTime.Now;


                            db.OrderMasterHistoriesDB.Add(h1);
                            db.Commit();

                        }
                        catch (Exception ex)
                        {
                        }
                        #endregion
                        return comp;

                    }


                    else
                    {
                        if (data == null)
                        {
                            throw new ArgumentNullException(" null");
                        }
                        var comp = db.OrderDispatchedMasters.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                        if (data.CheckAmount == 0 && data.ElectronicAmount == 0)
                        {
                            comp.cash = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }
                        }
                        else if (data.CheckAmount == 0 && data.ElectronicAmount != 0 && data.electronic == true)
                        {
                            comp.cash = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();


                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }

                        }
                        else if (data.CheckAmount != 0 && data.ElectronicAmount == 0 && data.cheq == true)
                        {
                            comp.cash = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }
                        }
                        else if (data.CheckAmount != 0 && data.ElectronicAmount != 0 && data.cheq == true && data.electronic == true)
                        {
                            comp.cash = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }
                        }
                        else
                        {
                            comp.cash = true;
                            comp.Status = "Partial settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                obj.Status = "Partial settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }
                        }
                        #region Order Master History

                        try
                        {
                            var UserName = db.Peoples.Where(x => x.PeopleID == userid).Select(a => a.DisplayName).SingleOrDefault();

                            OrderMasterHistories h1 = new OrderMasterHistories();

                            h1.orderid = comp.OrderId;
                            h1.Status = comp.Status;
                            h1.Reasoncancel = "Settled";
                            h1.Warehousename = comp.WarehouseName;
                            h1.userid = userid;
                            h1.username = UserName;
                            h1.CreatedDate = DateTime.Now;


                            db.OrderMasterHistoriesDB.Add(h1);
                            db.Commit();

                        }
                        catch (Exception ex)
                        {
                        }
                        #endregion
                        return comp;
                    }
                }

            }
            catch (Exception ex)
            {
            }
            return null;
        }

        [Route("Bulkcashstatus")]
        [HttpGet, HttpPut]
        public dynamic Bulkcashstatus(List<OrderDispatchedMaster> Listdata)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                using (var db = new AuthContext())
                {
                    foreach (var data in Listdata)
                    {
                        data.CompanyId = compid;
                        data.WarehouseId = Warehouse_id;

                        if (Warehouse_id > 0)
                        {

                            if (data == null)
                            {
                                throw new ArgumentNullException(" null");
                            }
                            var comp = db.OrderDispatchedMasters.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                            if (data.CheckAmount == 0 && data.ElectronicAmount == 0)
                            {
                                comp.cash = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }
                            }
                            else if (data.CheckAmount == 0 && data.ElectronicAmount != 0 && data.electronic == true)
                            {
                                comp.cash = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();


                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }

                            }
                            else if (data.CheckAmount != 0 && data.ElectronicAmount == 0 && data.cheq == true)
                            {
                                comp.cash = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }
                            }
                            else if (data.CheckAmount != 0 && data.ElectronicAmount != 0 && data.cheq == true && data.electronic == true)
                            {
                                comp.cash = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }
                            }
                            //else as above
                            else
                            {
                                comp.cash = true;
                                comp.Status = "Partial settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                    obj.Status = "Partial settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }
                            }
                            //return comp;

                        }


                        else
                        {
                            if (data == null)
                            {
                                throw new ArgumentNullException(" null");
                            }
                            var comp = db.OrderDispatchedMasters.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                            if (data.CheckAmount == 0 && data.ElectronicAmount == 0)
                            {
                                comp.cash = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }
                            }
                            else if (data.CheckAmount == 0 && data.ElectronicAmount != 0 && data.electronic == true)
                            {
                                comp.cash = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();


                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }

                            }
                            else if (data.CheckAmount != 0 && data.ElectronicAmount == 0 && data.cheq == true)
                            {
                                comp.cash = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }
                            }
                            else if (data.CheckAmount != 0 && data.ElectronicAmount != 0 && data.cheq == true && data.electronic == true)
                            {
                                comp.cash = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }
                            }
                            else
                            {
                                comp.cash = true;
                                comp.Status = "Partial settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                    obj.Status = "Partial settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }
                            }
                            // return comp;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return true;
        }

        [Route("chequestatus")]
        [HttpGet, HttpPut]
        public dynamic chequestatus(OrderDispatchedMaster data)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                data.CompanyId = compid;
                data.WarehouseId = Warehouse_id;
                using (var db = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {
                        var comp = db.OrderDispatchedMasters.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();
                        if (data.CashAmount == 0 && data.ElectronicAmount == 0)
                        {
                            comp.cheq = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }
                            catch
                            {

                            }

                        }
                        else if (data.CashAmount == 0 && data.ElectronicAmount != 0 && data.electronic == true)
                        {
                            comp.cheq = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }

                        }
                        else if (data.CashAmount != 0 && data.ElectronicAmount == 0 && data.cash == true)
                        {
                            comp.cheq = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();

                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }



                        }
                        else if (data.CashAmount != 0 && data.ElectronicAmount != 0 && data.cash == true && data.electronic == true)
                        {
                            comp.cheq = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }

                        }
                        else
                        {
                            comp.cheq = true;
                            comp.Status = "Partial settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();
                                obj.Status = "Partial settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }

                        }
                        return comp;


                    }
                    else
                    {
                        var comp = db.OrderDispatchedMasters.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();
                        if (data.CashAmount == 0 && data.ElectronicAmount == 0)
                        {
                            comp.cheq = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();

                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }

                        }
                        else if (data.CashAmount == 0 && data.ElectronicAmount != 0 && data.electronic == true)
                        {
                            comp.cheq = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }

                        }
                        else if (data.CashAmount != 0 && data.ElectronicAmount == 0 && data.cash == true)
                        {
                            comp.cheq = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();

                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }



                        }
                        else if (data.CashAmount != 0 && data.ElectronicAmount != 0 && data.cash == true && data.electronic == true)
                        {
                            comp.cheq = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }

                        }
                        else
                        {
                            comp.cheq = true;
                            comp.Status = "Partial settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();
                                obj.Status = "Partial settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }

                        }
                        return comp;
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return null;
        }

        [Route("Bulkchequestatus")]
        [HttpGet, HttpPut]
        public dynamic Bulkchequestatus(List<OrderDispatchedMaster> datalist)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                using (var db = new AuthContext())
                {
                    foreach (var data in datalist)
                    {
                        data.CompanyId = compid;
                        data.WarehouseId = Warehouse_id;
                        if (Warehouse_id > 0)
                        {
                            var comp = db.OrderDispatchedMasters.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();
                            if (data.CashAmount == 0 && data.ElectronicAmount == 0)
                            {
                                comp.cheq = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }
                                catch
                                {

                                }

                            }
                            else if (data.CashAmount == 0 && data.ElectronicAmount != 0 && data.electronic == true)
                            {
                                comp.cheq = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }

                            }
                            else if (data.CashAmount != 0 && data.ElectronicAmount == 0 && data.cash == true)
                            {
                                comp.cheq = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();

                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }



                            }
                            else if (data.CashAmount != 0 && data.ElectronicAmount != 0 && data.cash == true && data.electronic == true)
                            {
                                comp.cheq = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }

                            }
                            else
                            {
                                comp.cheq = true;
                                comp.Status = "Partial settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();
                                    obj.Status = "Partial settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }

                            }
                            //return comp;


                        }
                        else
                        {
                            var comp = db.OrderDispatchedMasters.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();
                            if (data.CashAmount == 0 && data.ElectronicAmount == 0)
                            {
                                comp.cheq = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();

                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }

                            }
                            else if (data.CashAmount == 0 && data.ElectronicAmount != 0 && data.electronic == true)
                            {
                                comp.cheq = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }

                            }
                            else if (data.CashAmount != 0 && data.ElectronicAmount == 0 && data.cash == true)
                            {
                                comp.cheq = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();

                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }



                            }
                            else if (data.CashAmount != 0 && data.ElectronicAmount != 0 && data.cash == true && data.electronic == true)
                            {
                                comp.cheq = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }

                            }
                            else
                            {
                                comp.cheq = true;
                                comp.Status = "Partial settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();
                                    obj.Status = "Partial settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }

                            }
                            // return comp;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
            }

            return true;
        }

        [Route("electronicstatus")]
        [HttpGet, HttpPut]
        public dynamic put(OrderDispatchedMaster data)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                data.CompanyId = compid;
                data.WarehouseId = Warehouse_id;
                using (var db = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {

                        var comp = db.OrderDispatchedMasters.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                        if (data.CashAmount == 0 && data.CheckAmount == 0)
                        {
                            comp.electronic = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }
                        }

                        else if (data.CashAmount == 0 && data.CheckAmount != 0 && data.cheq == true)
                        {
                            comp.electronic = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }
                        }
                        else if (data.CashAmount != 0 && data.CheckAmount == 0 && data.cash == true)
                        {
                            comp.electronic = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }
                        }
                        else if (data.CashAmount != 0 && data.CheckAmount != 0 && data.cheq == true && data.cash == true)
                        {
                            comp.electronic = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }
                        }
                        else
                        {
                            comp.electronic = true;
                            comp.Status = "Partial settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                obj.Status = "Partial settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }
                        }

                        return comp;
                    }


                    else
                    {
                        var comp = db.OrderDispatchedMasters.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                        if (data.CashAmount == 0 && data.CheckAmount == 0)
                        {
                            comp.electronic = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }
                        }

                        else if (data.CashAmount == 0 && data.CheckAmount != 0 && data.cheq == true)
                        {
                            comp.electronic = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }
                        }
                        else if (data.CashAmount != 0 && data.CheckAmount == 0 && data.cash == true)
                        {
                            comp.electronic = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }
                        }
                        else if (data.CashAmount != 0 && data.CheckAmount != 0 && data.cheq == true && data.cash == true)
                        {
                            comp.electronic = true;
                            comp.Status = "Account settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                obj.Status = "Account settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }
                        }
                        else
                        {
                            comp.electronic = true;
                            comp.Status = "Partial settled";
                            comp.UpdatedDate = DateTime.Now;
                            //db.OrderDispatchedMasters.Attach(comp);
                            db.Entry(comp).State = EntityState.Modified;
                            db.Commit();
                            try
                            {
                                var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                obj.Status = "Partial settled";
                                obj.UpdatedDate = DateTime.Now;
                                //db.DbOrderMaster.Attach(obj);
                                db.Entry(obj).State = EntityState.Modified;
                                db.Commit();
                            }

                            catch
                            {

                            }
                        }

                        return comp;
                    }

                }

            }
            catch (Exception ex)
            {
            }
            return null;
        }

        [Route("Bulkelectronicstatus")]
        [HttpGet, HttpPut]
        public dynamic Bulkelectronicstatus(List<OrderDispatchedMaster> Listdata)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }

                using (var db = new AuthContext())
                {
                    foreach (var data in Listdata)
                    {
                        data.CompanyId = compid;
                        data.WarehouseId = Warehouse_id;
                        if (Warehouse_id > 0)
                        {

                            var comp = db.OrderDispatchedMasters.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                            if (data.CashAmount == 0 && data.CheckAmount == 0)
                            {
                                comp.electronic = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }
                            }

                            else if (data.CashAmount == 0 && data.CheckAmount != 0 && data.cheq == true)
                            {
                                comp.electronic = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }
                            }
                            else if (data.CashAmount != 0 && data.CheckAmount == 0 && data.cash == true)
                            {
                                comp.electronic = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }
                            }
                            else if (data.CashAmount != 0 && data.CheckAmount != 0 && data.cheq == true && data.cash == true)
                            {
                                comp.electronic = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }
                            }
                            else
                            {
                                comp.electronic = true;
                                comp.Status = "Partial settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                                    obj.Status = "Partial settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }
                            }
                        }
                        else
                        {
                            var comp = db.OrderDispatchedMasters.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                            if (data.CashAmount == 0 && data.CheckAmount == 0)
                            {
                                comp.electronic = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }
                            }

                            else if (data.CashAmount == 0 && data.CheckAmount != 0 && data.cheq == true)
                            {
                                comp.electronic = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }
                            }
                            else if (data.CashAmount != 0 && data.CheckAmount == 0 && data.cash == true)
                            {
                                comp.electronic = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }
                            }
                            else if (data.CashAmount != 0 && data.CheckAmount != 0 && data.cheq == true && data.cash == true)
                            {
                                comp.electronic = true;
                                comp.Status = "Account settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                    obj.Status = "Account settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }

                                catch
                                {

                                }
                            }
                            else
                            {
                                comp.electronic = true;
                                comp.Status = "Partial settled";
                                comp.UpdatedDate = DateTime.Now;
                                //db.OrderDispatchedMasters.Attach(comp);
                                db.Entry(comp).State = EntityState.Modified;
                                db.Commit();
                                try
                                {
                                    var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                                    obj.Status = "Partial settled";
                                    obj.UpdatedDate = DateTime.Now;
                                    //db.DbOrderMaster.Attach(obj);
                                    db.Entry(obj).State = EntityState.Modified;
                                    db.Commit();
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return true;
        }

        [Route("Bounce")]
        [HttpGet, HttpPut]
        public dynamic Bounce(OrderDispatchedMaster data)
        {
            try
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                data.CompanyId = compid;
                data.WarehouseId = Warehouse_id;
                using (var db = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {


                        var comp = db.OrderDispatchedMasters.Where(x => x.OrderId == data.OrderId && data.WarehouseId == data.WarehouseId).FirstOrDefault();


                        var stt = "Partial receiving -Bounce";
                        comp.cheq = true;
                        comp.Status = stt;
                        comp.UpdatedDate = DateTime.Now;
                        comp.BounceCheqAmount = 200;
                        //db.OrderDispatchedMasters.Attach(comp);
                        db.Entry(comp).State = EntityState.Modified;
                        db.Commit();
                        try
                        {
                            var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.WarehouseId == data.WarehouseId).FirstOrDefault();

                            obj.Status = "Partial receiving -Bounce";
                            obj.UpdatedDate = DateTime.Now;
                            //db.DbOrderMaster.Attach(obj);
                            db.Entry(obj).State = EntityState.Modified;
                            db.Commit();
                        }

                        catch
                        {

                        }



                        return comp;
                    }
                    else
                    {

                        var comp = db.OrderDispatchedMasters.Where(x => x.OrderId == data.OrderId && data.CompanyId == data.CompanyId).FirstOrDefault();


                        var stt = "Partial receiving -Bounce";
                        comp.cheq = true;
                        comp.Status = stt;
                        comp.UpdatedDate = DateTime.Now;
                        comp.BounceCheqAmount = 200;
                        //db.OrderDispatchedMasters.Attach(comp);
                        db.Entry(comp).State = EntityState.Modified;
                        db.Commit();
                        try
                        {
                            var obj = db.DbOrderMaster.Where(x => x.OrderId == data.OrderId && x.CompanyId == data.CompanyId).FirstOrDefault();

                            obj.Status = "Partial receiving -Bounce";
                            obj.UpdatedDate = DateTime.Now;
                            //db.DbOrderMaster.Attach(obj);
                            db.Entry(obj).State = EntityState.Modified;
                            db.Commit();
                        }

                        catch
                        {

                        }



                        return comp;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        [Route("history")]
        [HttpGet]
        public PaggingData salessettlementhistory(int list, int page)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                using (var db = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {
                        PaggingData data = new PaggingData();
                        var total_count = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.WarehouseId == Warehouse_id && (x.Status == "Account settled" || x.Status == "Partial receiving -Bounce")).Count();
                        var ordermaster = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.WarehouseId == Warehouse_id && (x.Status == "Account settled" || x.Status == "Partial receiving -Bounce")).OrderByDescending(x => x.OrderId).Skip((page - 1) * list).Take(list).ToList();
                        data.ordermaster = ordermaster;
                        data.total_count = total_count;
                        return data;
                    }
                    else
                    {
                        PaggingData data = new PaggingData();
                        var total_count = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.CompanyId == compid && (x.Status == "Account settled" || x.Status == "Partial receiving -Bounce")).Count();
                        var ordermaster = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.CompanyId == compid && (x.Status == "Account settled" || x.Status == "Partial receiving -Bounce")).OrderByDescending(x => x.OrderId).Skip((page - 1) * list).Take(list).ToList();
                        data.ordermaster = ordermaster;
                        data.total_count = total_count;
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [Route("Historyexportsales")]
        [HttpGet]
        public HttpResponseMessage Historyexportsales() //get all export sales settlemt 
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                using (var db = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {
                        var saleSHistory = db.OrderDispatchedMasters.Where(x => x.Status == "Account settled" || x.Status == "Partial receiving -Bounce" && x.WarehouseId == Warehouse_id).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, saleSHistory);
                    }
                    else
                    {
                        var saleSHistory = db.OrderDispatchedMasters.Where(x => x.Status == "Account settled" || x.Status == "Partial receiving -Bounce" && x.CompanyId == compid).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, saleSHistory);
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [Route("historysearch")]
        [HttpGet]
        public dynamic historysearch(DateTime? start, DateTime? end, int? OrderId, double totalAmount)
        {

            try
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                using (var db = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {
                        if (OrderId != 0 && OrderId > 0)
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && (x.OrderId == OrderId && x.WarehouseId == Warehouse_id) && (x.Status == "Account settled" || x.Status == "Partial receiving -Bounce")).ToList();

                            return data;
                        }
                        else if ((OrderId > 0) && start != null)
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && (x.OrderId == OrderId && x.WarehouseId == Warehouse_id) && (x.Status == "Account settled" || x.Status == "Partial receiving -Bounce") && (x.CreatedDate > start && x.CreatedDate <= end)).ToList();

                            return data;
                        }
                        else if (totalAmount != 0 && totalAmount > 0)
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && (x.GrossAmount == totalAmount && x.WarehouseId == Warehouse_id) && (x.Status == "Account settled" || x.Status == "Partial receiving -Bounce")).ToList();

                            return data;
                        }
                        else
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.WarehouseId == Warehouse_id && (x.Status == "Account settled" || x.Status == "Partial receiving -Bounce") && (x.CreatedDate > start && x.CreatedDate < end)).ToList();

                            return data;
                        }
                    }

                    else
                    {
                        if (OrderId != 0 && OrderId > 0)
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && (x.OrderId == OrderId && x.CompanyId == compid) && (x.Status == "Account settled" || x.Status == "Partial receiving -Bounce")).ToList();

                            return data;
                        }
                        else if ((OrderId > 0) && start != null)
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && (x.OrderId == OrderId && x.CompanyId == compid) && (x.Status == "Account settled" || x.Status == "Partial receiving -Bounce") && (x.CreatedDate > start && x.CreatedDate <= end)).ToList();

                            return data;
                        }
                        else if (totalAmount != 0 && totalAmount > 0)
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && (x.GrossAmount == totalAmount && x.CompanyId == compid) && (x.Status == "Account settled" || x.Status == "Partial receiving -Bounce")).ToList();

                            return data;
                        }
                        else
                        {
                            var data = db.OrderDispatchedMasters.Where(x => x.Deleted == false && x.CompanyId == compid && (x.Status == "Account settled" || x.Status == "Partial receiving -Bounce") && (x.CreatedDate > start && x.CreatedDate < end)).ToList();

                            return data;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                return false;
            }
        }

    }
}
