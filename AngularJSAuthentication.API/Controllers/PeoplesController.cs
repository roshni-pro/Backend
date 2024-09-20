using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Infrastructure;
using AngularJSAuthentication.Model;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System.Data.SqlClient;
using System.Web;
using System.IO;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.Shared;
using AgileObjects.AgileMapper;
using LinqKit;
using AngularJSAuthentication.DataContracts;
using System.Configuration;
using System.Text;
using AngularJSAuthentication.Model.Store;
using AngularJSAuthentication.API.Helper.Notification;


namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Peoples")]
    public class PeoplesController : BaseAuthController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        [Route("")]
        public IEnumerable<People> Get()
        {
            logger.Info("Get Peoples: ");
            int compid = 0, userid = 0;
            int Warehouse_id = 0;
            string email = "";
            using (var context = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;

                    // Access claims
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
                        if (claim.Type == "email")
                        {
                            email = claim.Value;
                        }
                    }
                    logger.Info("End Get Company: ");
                    if (Warehouse_id > 0)
                    {
                        List<People> person = context.AllPeoplesWid(compid, Warehouse_id).ToList();
                        return person;
                    }
                    else
                    {
                        List<People> person = context.AllPeoples(compid).ToList();
                        return person;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting Peoples " + ex.Message);
                    return null;
                }
        }
        //[Authorize(Roles="Admin")]
        [Route("GetAll")]
        public IEnumerable<PeopleAll> GetAll()
        {
            logger.Info("Get Peoples: ");
            int compid = 0, userid = 0;
            int Warehouse_id = 0;
            string email = "";
            using (var context = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;

                    // Access claims
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
                        if (claim.Type == "email")
                        {
                            email = claim.Value;
                        }
                    }
                    using (AuthContext db = new AuthContext())
                    {
                        List<PeopleAll> person = new List<PeopleAll>();

                        if (Warehouse_id > 0)
                        {
                            person = (from pd in db.Peoples
                                      join ps in db.PeoplesSalaryDB
                                      on pd.PeopleID equals ps.PeopleID into pdps
                                      from x in pdps.DefaultIfEmpty()
                                      join pDoc in db.PeopleDocumentDB
                                      on pd.PeopleID.ToString() equals pDoc.PeopleId into pdpDoc
                                      from y in pdpDoc.DefaultIfEmpty()
                                      where pd.CompanyId == compid && pd.WarehouseId == Warehouse_id
                                      select new PeopleAll
                                      {
                                          PeopleID = pd.PeopleID,
                                          CompanyId = pd.CompanyId,
                                          WarehouseId = pd.WarehouseId,
                                          Email = pd.Email,
                                          Country = pd.Country,
                                          Stateid = pd.Stateid,
                                          state = pd.state,
                                          Cityid = pd.Cityid,
                                          city = pd.city,
                                          Mobile = pd.Mobile,
                                          Password = pd.Password,
                                          RoleId = pd.RoleId,
                                          DisplayName = pd.DisplayName,
                                          Department = pd.Department,
                                          BillableRate = pd.BillableRate,
                                          CostRate = pd.CostRate,
                                          //Permissions = pd.Permissions,
                                          SUPPLIERCODES = pd.SUPPLIERCODES,
                                          Type = pd.Type,
                                          Approved = pd.Approved,
                                          PeopleFirstName = pd.PeopleFirstName,
                                          PeopleLastName = pd.PeopleLastName,
                                          Active = pd.Active,
                                          CreatedDate = pd.CreatedDate,
                                          UpdatedDate = pd.UpdatedDate,
                                          CreatedBy = pd.CreatedBy,
                                          UpdateBy = pd.UpdateBy,
                                          Skcode = pd.Skcode,
                                          AgentCode = pd.AgentCode,
                                          Salesexecutivetype = pd.Salesexecutivetype,
                                          AgentAmount = pd.AgentAmount,
                                          Desgination = pd.Desgination,
                                          Status = pd.Status,
                                          DOB = pd.DOB,
                                          DataOfJoin = pd.DataOfJoin,
                                          DataOfMarriage = pd.DataOfMarriage,
                                          EndDate = pd.EndDate,
                                          Unit = pd.Unit,
                                          Reporting = pd.Reporting,
                                          IfscCode = pd.IfscCode,
                                          Account_Number = pd.Account_Number,
                                          UserName = pd.UserName,
                                          Salary = x.Salary,
                                          B_Salary = x.B_Salary,
                                          Hra_Salary = x.Hra_Salary,
                                          CA_Salary = x.CA_Salary,
                                          DA_Salary = x.DA_Salary,
                                          Lta_Salary = x.Lta_Salary,
                                          PF_Salary = x.PF_Salary,
                                          ESI_Salary = x.ESI_Salary,
                                          M_Incentive = x.M_Incentive,
                                          Y_Incentive = x.Y_Incentive,
                                          MarkSheet = y.MarkSheet,
                                          Id_Proof = y.Id_Proof,
                                          Address_Proof = y.Address_Proof,
                                          PanCard_Proof = y.PanCard_Proof,
                                          Pre_SalarySlip = y.Pre_SalarySlip,
                                          tempdel = pd.tempdel,
                                          ReportPersonId = pd.ReportPersonId
                                      }).ToList();
                            return person;
                        }
                        else
                        {
                            person = (from pd in db.Peoples
                                      join ps in db.PeoplesSalaryDB
                                      on pd.PeopleID equals ps.PeopleID into pdps
                                      from x in pdps.DefaultIfEmpty()
                                      join pDoc in db.PeopleDocumentDB
                                      on pd.PeopleID.ToString() equals pDoc.PeopleId into pdpDoc
                                      from y in pdpDoc.DefaultIfEmpty()
                                      where pd.CompanyId == compid && pd.Deleted == false//by Sudhir 22-04-2019
                                      orderby pd.PeopleID
                                      select new PeopleAll
                                      {
                                          PeopleID = pd.PeopleID,
                                          CompanyId = pd.CompanyId,
                                          WarehouseId = pd.WarehouseId,
                                          Email = pd.Email,
                                          Country = pd.Country,
                                          Stateid = pd.Stateid,
                                          state = pd.state,
                                          Cityid = pd.Cityid,
                                          city = pd.city,
                                          Mobile = pd.Mobile,
                                          Password = pd.Password,
                                          RoleId = pd.RoleId,
                                          DisplayName = pd.DisplayName,
                                          Department = pd.Department,
                                          BillableRate = pd.BillableRate,
                                          CostRate = pd.CostRate,
                                          // Permissions = pd.Permissions,
                                          SUPPLIERCODES = pd.SUPPLIERCODES,
                                          Type = pd.Type,
                                          Approved = pd.Approved,
                                          PeopleFirstName = pd.PeopleFirstName,
                                          PeopleLastName = pd.PeopleLastName,
                                          Active = pd.Active,
                                          CreatedDate = pd.CreatedDate,
                                          UpdatedDate = pd.UpdatedDate,
                                          CreatedBy = pd.CreatedBy,
                                          UpdateBy = pd.UpdateBy,
                                          Skcode = pd.Skcode,
                                          AgentCode = pd.AgentCode,
                                          Salesexecutivetype = pd.Salesexecutivetype,
                                          AgentAmount = pd.AgentAmount,
                                          Desgination = pd.Desgination,
                                          Status = pd.Status,
                                          DOB = pd.DOB,
                                          DataOfJoin = pd.DataOfJoin,
                                          DataOfMarriage = pd.DataOfMarriage,
                                          EndDate = pd.EndDate,
                                          Unit = pd.Unit,
                                          Reporting = pd.Reporting,
                                          IfscCode = pd.IfscCode,
                                          Account_Number = pd.Account_Number,
                                          UserName = pd.UserName,
                                          Salary = x.Salary,
                                          B_Salary = x.B_Salary,
                                          Hra_Salary = x.Hra_Salary,
                                          CA_Salary = x.CA_Salary,
                                          DA_Salary = x.DA_Salary,
                                          Lta_Salary = x.Lta_Salary,
                                          PF_Salary = x.PF_Salary,
                                          ESI_Salary = x.ESI_Salary,
                                          M_Incentive = x.M_Incentive,
                                          Y_Incentive = x.Y_Incentive,

                                          MarkSheet = y.MarkSheet,
                                          Id_Proof = y.Id_Proof,
                                          PanCard_Proof = y.PanCard_Proof,
                                          Address_Proof = y.Address_Proof,
                                          Pre_SalarySlip = y.Pre_SalarySlip,
                                          tempdel = pd.tempdel,
                                          ReportPersonId = pd.ReportPersonId

                                      }).ToList();
                            return person;
                            logger.Info("End Get Company: ");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting Peoples " + ex.Message);
                    return null;
                }
        }
        [Route("Warehousebased")]
        [HttpGet]
        public IEnumerable<PeopleAll> Warehousebased(int WarehouseId)
        {
            logger.Info("Get Peoples: ");
            List<PeopleAll> person = new List<PeopleAll>();

            using (var context = new AuthContext())
            {

                if (WarehouseId > 0)
                {
                    person = (from pd in context.Peoples
                              join ps in context.PeoplesSalaryDB
                              on pd.PeopleID equals ps.PeopleID into pdps
                              from x in pdps.DefaultIfEmpty()
                              join pDoc in context.PeopleDocumentDB
                              on pd.PeopleID.ToString() equals pDoc.PeopleId into pdpDoc
                              from y in pdpDoc.DefaultIfEmpty()
                              where pd.WarehouseId == WarehouseId
                              select new PeopleAll
                              {
                                  PeopleID = pd.PeopleID,
                                  CompanyId = pd.CompanyId,
                                  WarehouseId = pd.WarehouseId,
                                  Email = pd.Email,
                                  Country = pd.Country,
                                  Stateid = pd.Stateid,
                                  state = pd.state,
                                  Cityid = pd.Cityid,
                                  city = pd.city,
                                  Mobile = pd.Mobile,
                                  Password = pd.Password,
                                  RoleId = pd.RoleId,
                                  DisplayName = pd.DisplayName,
                                  Department = pd.Department,
                                  BillableRate = pd.BillableRate,
                                  CostRate = pd.CostRate,
                                  //Permissions = pd.Permissions,
                                  SUPPLIERCODES = pd.SUPPLIERCODES,
                                  Type = pd.Type,
                                  Approved = pd.Approved,
                                  PeopleFirstName = pd.PeopleFirstName,
                                  PeopleLastName = pd.PeopleLastName,
                                  Active = pd.Active,
                                  CreatedDate = pd.CreatedDate,
                                  UpdatedDate = pd.UpdatedDate,
                                  CreatedBy = pd.CreatedBy,
                                  UpdateBy = pd.UpdateBy,
                                  Skcode = pd.Skcode,
                                  AgentCode = pd.AgentCode,
                                  Salesexecutivetype = pd.Salesexecutivetype,
                                  AgentAmount = pd.AgentAmount,
                                  Desgination = pd.Desgination,
                                  Status = pd.Status,
                                  DOB = pd.DOB,
                                  DataOfJoin = pd.DataOfJoin,
                                  DataOfMarriage = pd.DataOfMarriage,
                                  EndDate = pd.EndDate,
                                  Unit = pd.Unit,
                                  Reporting = pd.Reporting,
                                  IfscCode = pd.IfscCode,
                                  Account_Number = pd.Account_Number,
                                  UserName = pd.UserName,
                                  Salary = x.Salary,
                                  B_Salary = x.B_Salary,
                                  Hra_Salary = x.Hra_Salary,
                                  CA_Salary = x.CA_Salary,
                                  DA_Salary = x.DA_Salary,
                                  Lta_Salary = x.Lta_Salary,
                                  PF_Salary = x.PF_Salary,
                                  ESI_Salary = x.ESI_Salary,
                                  M_Incentive = x.M_Incentive,
                                  Y_Incentive = x.Y_Incentive,

                                  MarkSheet = y.MarkSheet,
                                  Id_Proof = y.Id_Proof,
                                  Address_Proof = y.Address_Proof,
                                  PanCard_Proof = y.PanCard_Proof,
                                  Pre_SalarySlip = y.Pre_SalarySlip,
                                  tempdel = pd.tempdel,
                                  ReportPersonId = pd.ReportPersonId

                              }).ToList();

                }
                return person;
            }


        }

        //Delivery Boy and Damage Assignment
        [Route("WarehouseRolebased")]
        [HttpGet]
        public dynamic WarehouseRolebased(int WarehouseId)
        {
            using (var context = new AuthContext())
            {
                List<SqlParameter> paramList = new List<SqlParameter>();
                paramList.Add(new SqlParameter("@WarehouseId", WarehouseId));
                string sqlquery = "GetDeliveryBoyDamageAssignmentRole @WarehouseId";
                List<DeliveryPeople> newdata = context.Database.SqlQuery<DeliveryPeople>(sqlquery, paramList.ToArray()).ToList();
                //string sqlquery = "select distinct p.PeopleID,p.PeopleFirstName,p.PeopleLastName,p.DisplayName from People p inner join AspNetUsers u on p.Email = u.Email inner join AspNetUserRoles ur on u.Id = ur.UserId inner join AspNetRoles r on ur.RoleId = r.Id where r.Name in('Delivery Boy','Damage Assignment') and ur.isActive = 1 and p.Active = 1 and p.Deleted = 0 and p.WarehouseId = " + WarehouseId;
                //List<DeliveryPeople> newdata = context.Database.SqlQuery<DeliveryPeople>(sqlquery).ToList();
                return newdata;
            }


        }


        #region
        [Route("GetAllV2")]
        public IEnumerable<PeopleAll> GetAllV2()
        {
            logger.Info("Get Peoples: ");
            int compid = 0, userid = 0;
            int Warehouse_id = 0;
            string email = "";
            using (var context = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;

                    // Access claims
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
                        if (claim.Type == "email")
                        {
                            email = claim.Value;
                        }
                    }
                    using (AuthContext db = new AuthContext())
                    {
                        List<PeopleAll> person = new List<PeopleAll>();

                        if (Warehouse_id > 0)
                        {
                            person = (from pd in db.Peoples
                                      join ps in db.PeoplesSalaryDB
                                      on pd.PeopleID equals ps.PeopleID into pdps
                                      from x in pdps.DefaultIfEmpty()
                                      join pDoc in db.PeopleDocumentDB
                                      on pd.PeopleID.ToString() equals pDoc.PeopleId into pdpDoc
                                      from y in pdpDoc.DefaultIfEmpty()
                                      where pd.CompanyId == compid
                                      select new PeopleAll
                                      {
                                          PeopleID = pd.PeopleID,
                                          Empcode = pd.Empcode,
                                          CompanyId = pd.CompanyId,
                                          WarehouseId = pd.WarehouseId,
                                          Email = pd.Email,
                                          Country = pd.Country,
                                          Stateid = pd.Stateid,
                                          state = pd.state,
                                          Cityid = pd.Cityid,
                                          city = pd.city,
                                          Mobile = pd.Mobile,
                                          Password = pd.Password,
                                          RoleId = pd.RoleId,
                                          DisplayName = pd.DisplayName,
                                          Department = pd.Department,
                                          BillableRate = pd.BillableRate,
                                          CostRate = pd.CostRate,
                                          //Permissions = pd.Permissions,
                                          SUPPLIERCODES = pd.SUPPLIERCODES,
                                          Type = pd.Type,
                                          Approved = pd.Approved,
                                          PeopleFirstName = pd.PeopleFirstName,
                                          PeopleLastName = pd.PeopleLastName,
                                          Active = pd.Active,
                                          CreatedDate = pd.CreatedDate,
                                          UpdatedDate = pd.UpdatedDate,
                                          CreatedBy = pd.CreatedBy,
                                          UpdateBy = pd.UpdateBy,
                                          Skcode = pd.Skcode,
                                          AgentCode = pd.AgentCode,
                                          Salesexecutivetype = pd.Salesexecutivetype,
                                          AgentAmount = pd.AgentAmount,
                                          Desgination = pd.Desgination,
                                          Status = pd.Status,
                                          DOB = pd.DOB,
                                          DataOfJoin = pd.DataOfJoin,
                                          DataOfMarriage = pd.DataOfMarriage,
                                          EndDate = pd.EndDate,
                                          Unit = pd.Unit,
                                          Reporting = pd.Reporting,
                                          IfscCode = pd.IfscCode,
                                          Account_Number = pd.Account_Number,
                                          UserName = pd.UserName,
                                          Salary = x.Salary,
                                          B_Salary = x.B_Salary,
                                          Hra_Salary = x.Hra_Salary,
                                          CA_Salary = x.CA_Salary,
                                          DA_Salary = x.DA_Salary,
                                          Lta_Salary = x.Lta_Salary,
                                          PF_Salary = x.PF_Salary,
                                          ESI_Salary = x.ESI_Salary,
                                          M_Incentive = x.M_Incentive,
                                          Y_Incentive = x.Y_Incentive,

                                          MarkSheet = y.MarkSheet,
                                          Id_Proof = y.Id_Proof,
                                          Address_Proof = y.Address_Proof,
                                          PanCard_Proof = y.PanCard_Proof,
                                          Pre_SalarySlip = y.Pre_SalarySlip,
                                          tempdel = pd.tempdel,
                                          ReportPersonId = pd.ReportPersonId

                                      }).ToList();
                            return person;
                        }
                        else
                        {

                            person = (from pd in db.Peoples
                                      join ps in db.PeoplesSalaryDB
                                      on pd.PeopleID equals ps.PeopleID into pdps
                                      from x in pdps.DefaultIfEmpty()
                                      join pDoc in db.PeopleDocumentDB
                                      on pd.PeopleID.ToString() equals pDoc.PeopleId into pdpDoc
                                      from y in pdpDoc.DefaultIfEmpty()
                                      where pd.CompanyId == compid && pd.Deleted == false//by Sudhir 22-04-2019
                                      orderby pd.PeopleID
                                      select new PeopleAll
                                      {
                                          PeopleID = pd.PeopleID,
                                          Empcode = pd.Empcode,
                                          CompanyId = pd.CompanyId,
                                          WarehouseId = pd.WarehouseId,
                                          Email = pd.Email,
                                          Country = pd.Country,
                                          Stateid = pd.Stateid,
                                          state = pd.state,
                                          Cityid = pd.Cityid,
                                          city = pd.city,
                                          Mobile = pd.Mobile,
                                          Password = pd.Password,
                                          RoleId = pd.RoleId,
                                          DisplayName = pd.DisplayName,
                                          Department = pd.Department,
                                          BillableRate = pd.BillableRate,
                                          CostRate = pd.CostRate,
                                          // Permissions = pd.Permissions,
                                          SUPPLIERCODES = pd.SUPPLIERCODES,
                                          Type = pd.Type,
                                          Approved = pd.Approved,
                                          PeopleFirstName = pd.PeopleFirstName,
                                          PeopleLastName = pd.PeopleLastName,
                                          Active = pd.Active,
                                          CreatedDate = pd.CreatedDate,
                                          UpdatedDate = pd.UpdatedDate,
                                          CreatedBy = pd.CreatedBy,
                                          UpdateBy = pd.UpdateBy,
                                          Skcode = pd.Skcode,
                                          AgentCode = pd.AgentCode,
                                          Salesexecutivetype = pd.Salesexecutivetype,
                                          AgentAmount = pd.AgentAmount,
                                          Desgination = pd.Desgination,
                                          Status = pd.Status,
                                          DOB = pd.DOB,
                                          DataOfJoin = pd.DataOfJoin,
                                          DataOfMarriage = pd.DataOfMarriage,
                                          EndDate = pd.EndDate,
                                          Unit = pd.Unit,
                                          Reporting = pd.Reporting,
                                          IfscCode = pd.IfscCode,
                                          Account_Number = pd.Account_Number,
                                          UserName = pd.UserName,
                                          Salary = x.Salary,
                                          B_Salary = x.B_Salary,
                                          Hra_Salary = x.Hra_Salary,
                                          CA_Salary = x.CA_Salary,
                                          DA_Salary = x.DA_Salary,
                                          Lta_Salary = x.Lta_Salary,
                                          PF_Salary = x.PF_Salary,
                                          ESI_Salary = x.ESI_Salary,
                                          M_Incentive = x.M_Incentive,
                                          Y_Incentive = x.Y_Incentive,

                                          MarkSheet = y.MarkSheet,
                                          Id_Proof = y.Id_Proof,
                                          PanCard_Proof = y.PanCard_Proof,
                                          Address_Proof = y.Address_Proof,
                                          Pre_SalarySlip = y.Pre_SalarySlip,
                                          tempdel = pd.tempdel,
                                          ReportPersonId = pd.ReportPersonId

                                      }).ToList();

                            return person;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting Peoples " + ex.Message);
                    return null;
                }
        }

        #endregion
        #region Get People Status
        [Route("Getpeople")]
        [HttpGet]
        public dynamic getPeople(string Mobile)
        {
            logger.Info("start PaymentResponse: ");
            using (AuthContext db = new AuthContext())
            {

                var list = db.Peoples.Where(p => p.Mobile == Mobile).Select(x => x.PeopleID).FirstOrDefault();
                return list;
            }
        }

        #endregion


        /// <summary>
        /// This page section is Removing People and show comment
        /// By Danish ---19/04/2019
        /// </summary>
        /// <param name="PeopleID"></param>
        /// <param name="DeleteComment"></param>
        [ResponseType(typeof(People))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int PeopleID, string DeleteComment)
        {
            logger.Info("DELETE Peoples: ");
            using (var context = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    // Access claims
                    String UserName = null;
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
                        if (claim.Type == "username")
                        {
                            UserName = (claim.Value);
                        }
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    int CompanyId = compid;
                    context.DeletePeople(PeopleID, UserName, DeleteComment);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add Peoples " + ex.Message);

                }
        }
        [ResponseType(typeof(People))]
        [Route("DeleteV7")]
        [AcceptVerbs("Delete")]
        public Boolean DeleteV7(int PeopleID, string DeleteComment)
        {
            logger.Info("DELETE Peoples: ");
            using (var context = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    // Access claims
                    String UserName = null;
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
                        if (claim.Type == "username")
                        {
                            UserName = (claim.Value);
                        }
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    int CompanyId = compid;
                    context.DeletePeople(PeopleID, UserName, DeleteComment);
                    return true;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add Peoples " + ex.Message);
                    return false;
                }
        }
        /// <summary>
        /// Re-active people after deleting 
        /// By Sudhir ---19/04/2019
        /// </summary>
        /// <param name="peopleid"></param>
        [Route("Undeleted")]
        [AcceptVerbs("Delete")]
        public void UnRemove(int peopleid)
        {
            logger.Info("start undelete Customer: ");
            using (var context = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

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

                    }
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.ActiveDeletedpeoples(peopleid);
                    logger.Info("End delete Customer: ");
                }
                catch (Exception ex)
                {
                    logger.Error("Error in undelete Customer " + ex.Message);
                }
        }
        /// <summary>
        /// Getting All Data which has been removed 
        /// By Danish ---19/04/2019
        /// </summary>
        /// <returns></returns>
        [Route("getremovedpeople")]
        [HttpGet]
        public IEnumerable<People> getremovedcpeople()
        {
            logger.Info("start Customer: ");

            List<People> ass = new List<People>();
            using (var context = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0, Warehouse_id = 0;
                    // Access claims
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
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid); using (AuthContext db = new AuthContext())
                    {
                        var list = db.Peoples.Where(x => x.Deleted == true).ToList();

                        return list;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in People " + ex.Message);
                    logger.Info("End People: ");
                    return null;
                }
        }

        [Route("user")]
        public People Get(int PeopleId)
        {

            int compid = 0, userid = 0;
            try
            {
                var identity = User.Identity as ClaimsIdentity;

                // Access claims
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
                }
                using (AuthContext db = new AuthContext())
                {
                    People person = db.Peoples.Where(u => u.PeopleID == PeopleId && u.CompanyId == compid).SingleOrDefault();
                    return person;
                }

            }

            catch (Exception ex)
            {
                logger.Error("Error in getting Peoples " + ex.Message);
                return null;
            }

        }

        ///// <summary>
        ///// created by 19/01/2019
        ///// get people profile
        ///// </summary>
        ///// <param name="PeopleId"></param>
        ///// <returns></returns>
        //[Route("myprofiler")]
        //[HttpGet]
        //public HttpResponseMessage Getprofile(int PeopleId)
        //{
        //    Peopleresponse res;
        //    using (var db = new AuthContext())
        //        try
        //        {
        //            People person = db.Peoples.Where(u => u.PeopleID == PeopleId).SingleOrDefault();
        //            if (person != null)
        //            {
        //                string query = "select distinct r.Name as Role from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where PeopleID='" + PeopleId  + "'and ur.isActive=1 and p.Active=1 and p.Deleted=0";
        //                var role = db.Database.SqlQuery<string>(query).ToList();

        //                var IsRole = role.Any(x => x.Contains("Hub sales lead"));
        //                if (IsRole)
        //                {
        //                    person.Role = "Hub sales lead";
        //                }
        //                else {
        //                    person.Role = "";

        //                }

        //                res = new Peopleresponse()
        //                {
        //                    people = person,
        //                    Status = true,
        //                    message = "Success."
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, res);
        //            }
        //            else
        //            {
        //                res = new Peopleresponse()
        //                {
        //                    people = person,
        //                    Status = false,
        //                    message = "People not exist."
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, res);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error in getting Peoples " + ex.Message);
        //            res = new Peopleresponse()
        //            {
        //                people = null,
        //                Status = false,
        //                message = "Failed."
        //            }; return Request.CreateResponse(HttpStatusCode.BadRequest, res);
        //        }
        //}

        [Route("")]
        public IEnumerable<People> Get(string department)
        {
            logger.Info("Get Peoples: ");
            int compid = 0, userid = 0;
            using (var context = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;

                    // Access claims
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
                    }
                    logger.Info("End Get Company: ");
                    int CompanyId = compid;
                    List<People> person = context.AllPeoplesDep(department, CompanyId).ToList();
                    return person;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting Peoples " + ex.Message);
                    return null;
                }

        }
        [Route("")]
        public People Get(string mob, string password)
        {
            logger.Info("Get Peoples: ");
            using (var context = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    // Access claims
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
                    }

                    int CompanyId = compid;
                    //    People check = context.CheckPeople(mob, password,CompanyId);
                    People check = context.CheckPeople(mob, password);
                    return check;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting Peoples " + ex.Message);
                    return null;
                }

        }

        /// <summary>
        ///  Sales persion login 
        ///  created by 15/01/2019
        /// </summary>   //tejas added some papameters to have fcmid and other phone details 
        /// <param name="mob"></param>
        /// <param name="password"></param>
        /// <returns></returns>

        //[Route("saleslogin")]
        //[HttpGet]
        //[AllowAnonymous]
        //public HttpResponseMessage Saleslogin(string mob, string password, string FcmId, string DeviceId, string CurrentAPKversion, string PhoneOSversion, string UserDeviceName, string IMEI)
        //{
        //    using (var db = new AuthContext())
        //    {
        //        logger.Info("Get Peoples: ");
        //        Peopleresponse res;
        //        People People = new People();
        //        People = db.Peoples.Where(x => x.Mobile == mob).FirstOrDefault();
        //        using (var context = new AuthContext())
        //            try
        //            {
        //                if (People != null)
        //                {
        //                    if (mob == People.Mobile)
        //                    {
        //                        People.FcmId = FcmId;
        //                        People.DeviceId = DeviceId;
        //                        People.CurrentAPKversion = CurrentAPKversion;   //tejas for device info 
        //                        People.PhoneOSversion = PhoneOSversion;
        //                        People.UserDeviceName = UserDeviceName;
        //                        People.IMEI = IMEI;
        //                        //sudhir for device info 
        //                                           //db.Peoples.Attach(People);
        //                        db.Entry(People).State = EntityState.Modified;
        //                        db.Commit();
        //                        #region Device History
        //                        var Customerhistory = db.Peoples.Where(x => x.Mobile == People.Mobile).FirstOrDefault();
        //                        try
        //                        {
        //                            PhoneRecordHistory phonerecord = new PhoneRecordHistory();
        //                            if (Customerhistory != null)
        //                            {
        //                                phonerecord.PeopleID = Customerhistory.PeopleID;
        //                                phonerecord.PeopleFirstName = Customerhistory.PeopleFirstName;
        //                                phonerecord.Department = Customerhistory.Department;
        //                                phonerecord.Mobile = Customerhistory.Mobile;
        //                                phonerecord.CurrentAPKversion = Customerhistory.CurrentAPKversion;
        //                                phonerecord.IMEI = Customerhistory.IMEI;
        //                                phonerecord.PhoneOSversion = Customerhistory.PhoneOSversion;
        //                                phonerecord.UserDeviceName = Customerhistory.UserDeviceName;
        //                                phonerecord.UpdatedDate = DateTime.Now;
        //                                db.PhoneRecordHistoryDB.Add(phonerecord);
        //                                int id = db.Commit();
        //                            }
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            logger.Error("Error loading  \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
        //                        }
        //                        #endregion
        //                    }




        //                    //    People check = context.CheckPeople(mob, password,CompanyId);
        //                    People check = People.Password == password && People.Deleted == false && People.Active == true ? People : null;
        //                    if (check != null)
        //                    {
        //                        var registeredApk = context.GetAPKUserAndPwd("SalesApp");
        //                        check.RegisteredApk = registeredApk;
        //                        string query = "select distinct r.Name as Role from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where PeopleID='" + check.PeopleID + "'and ur.isActive=1 and p.Active=1 and p.Deleted=0";
        //                        var role = context.Database.SqlQuery<string>(query).ToList();

        //                        var IsRole =role.Any(x => x.Contains("Hub sales lead"));
        //                        if (IsRole)
        //                        {
        //                            check.Role = "Hub sales lead";
        //                        }
        //                        else {

        //                            check.Role ="";

        //                        }
        //                        res = new Peopleresponse
        //                        {
        //                            people = check,
        //                            Status = true,
        //                            message = "Success."

        //                        };
        //                        return Request.CreateResponse(HttpStatusCode.OK, res);
        //                    }
        //                    else
        //                    {
        //                        res = new Peopleresponse
        //                        {
        //                            people = null,
        //                            Status = false,
        //                            message = "Wrong Password."

        //                        };
        //                        return Request.CreateResponse(HttpStatusCode.OK, res);
        //                    }
        //                }
        //                else
        //                {
        //                    res = new Peopleresponse
        //                    {
        //                        people = null,
        //                        Status = false,
        //                        message = "Failed."

        //                    };
        //                    return Request.CreateResponse(HttpStatusCode.OK, res);
        //                }


        //            }
        //            catch (Exception ex)
        //            {
        //                logger.Error("Error in getting Peoples " + ex.Message);
        //                res = new Peopleresponse
        //                {
        //                    people = null,
        //                    Status = false,
        //                    message = "Failed."

        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, res);

        //            }

        //    }
        //}
        #region for FreeLancerLogin  tejas 
        /// <summary>
        ///  Sales persion login 
        ///  created by tejas
        /// </summary>  
        /// <param name="mob"></param>
        /// <param name="password"></param>
        /// <returns></returns>

        [Route("FreeLancerLogin")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage FreeLancerLogin(string mob, string password)
        {
            using (var db = new AuthContext())
            {
                logger.Info("Get Peoples: ");
                Peopleresponse res;
                People People = new People();
                string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.Mobile='" + mob + "' and p.Password='" + password + "' and r.Name='Agent' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                People = db.Database.SqlQuery<People>(query).FirstOrDefault();
                //People = db.Peoples.Where(x => x.Mobile == mob && x.Password == password && x.Department == "Agent").FirstOrDefault();
                using (var context = new AuthContext())
                    try
                    {
                        if (People != null)
                        {
                            var registeredApk = db.GetAPKUserAndPwd("SalesApp");
                            People.RegisteredApk = registeredApk;

                            res = new Peopleresponse
                            {
                                people = People,
                                Status = true,
                                message = "Success."

                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);


                        }
                        else
                        {
                            res = new Peopleresponse
                            {
                                people = null,
                                Status = false,
                                message = "Wrong Password."

                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error in getting Peoples " + ex.Message);
                        res = new Peopleresponse
                        {
                            people = null,
                            Status = false,
                            message = "Failed."

                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
            }
        }
        #endregion


        #region Get agent list with there device info 
        /// <summary>
        /// Get Customer list with there device info  //by tejas 21-05-2019
        /// </summary>

        /// <returns></returns>
        //[Authorize]
        [Route("GetAgentsDeviceInfo")]
        [HttpGet]
        public HttpResponseMessage GetAgentsDeviceInfo(int Cityid)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                // Access claims
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
                }
                using (AuthContext db = new AuthContext())
                {
                    string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.Cityid=" + Cityid + " and r.Name='Sales Executive' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                    List<People> TotalItem = db.Database.SqlQuery<People>(query).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, TotalItem);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in TotalLineItem " + ex.Message);
                return Request.CreateResponse(HttpStatusCode.OK);
            }

        }

        #endregion

        [ResponseType(typeof(People))]
        [Route("")]
        [AcceptVerbs("POST")]
        public People add(People item)
        {
            logger.Info("Add Peoples: ");
            using (var context = new AuthContext())
                try
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    // Access claims
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
                    }

                    item.CompanyId = compid;
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    var pp = context.AddPeoplebyAdmin(item);
                    if (pp == null)
                    {
                        return null;
                    }
                    logger.Info("End  Add Peoples: ");
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add Peoples " + ex.Message);

                    return null;
                }
        }

        [ResponseType(typeof(People))]
        [Route("peopleupdate")]
        [AcceptVerbs("PUT")]
        public HttpResponseMessage peopleupdate(People item)
        {
            Peopleresponse res;
            logger.Info("Add Peoples: ");
            using (var db = new AuthContext())
                try
                {
                    People people = db.Peoples.Where(add => add.PeopleID == item.PeopleID).SingleOrDefault();


                    if (people == null)
                    {
                        res = new Peopleresponse
                        {
                            people = null,
                            Status = false,
                            message = "Not exist."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        people.PeopleFirstName = item.PeopleFirstName;
                        db.Entry(people).State = EntityState.Modified;
                        db.Commit();
                        res = new Peopleresponse
                        {
                            people = null,
                            Status = false,
                            message = "Failed."

                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    logger.Info("End  Add Peoples: ");

                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add Peoples " + ex.Message);
                    res = new Peopleresponse
                    {
                        people = null,
                        Status = false,
                        message = "Something went wrong."

                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
        }

        #region Selected Warehouse in edit people 

        [Route("GetWarehouseEditPeople")]
        [HttpGet]
        [AllowAnonymous]
        public dynamic GetWarehouseEditPeople(int? Peopleid)
        {
            try
            {
                using (AuthContext db = new AuthContext())
                {

                    var GetWarehouse = (from c in db.Warehouses.Where(x => x.IsKPP == false)
                                        join p in db.WarehousePermissionDB.Where(x => x.PeopleID == Peopleid && x.IsActive == true && x.IsDeleted == false)
                                        on c.WarehouseId equals p.WarehouseId into ps
                                        from p in ps.DefaultIfEmpty()
                                        select new
                                        {
                                            id = c.WarehouseId,
                                            name = c.WarehouseName + " " + c.CityName,
                                            Selected = p == null ? false : true
                                        }).ToList();
                    return GetWarehouse;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Warehouse " + ex.ToString());
                logger.Info("End  get Warehouse: ");
                return 0;
            }
        }
        #endregion
        [ResponseType(typeof(People))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public PeopleAll Put(PeopleAll item)// this Code Updated By Shoaib on 13/12/2018
        {
            using (var ac = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    String UserName = null;
                    // Access claims
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
                        if (claim.Type == "username")
                        {
                            UserName = (claim.Value);
                        }
                    }

                    item.CompanyId = compid;
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    try
                    {
                        People pd = ac.Peoples.Where(x => x.PeopleID == item.PeopleID).SingleOrDefault();
                        pd.PeopleFirstName = item.PeopleFirstName;
                        pd.PeopleLastName = item.PeopleLastName;
                        pd.Email = item.Email;
                        pd.state = item.state;
                        pd.Stateid = item.Stateid;
                        pd.city = item.city;
                        pd.Cityid = item.Cityid;
                        pd.Mobile = item.Mobile;
                        pd.Department = item.Department;
                        //pd.Permissions = item.Permissions;
                        pd.Active = item.Active;
                        pd.Desgination = item.Desgination;
                        pd.Status = item.Status;
                        pd.DOB = item.DOB;
                        pd.DataOfJoin = item.DataOfJoin;
                        pd.DataOfMarriage = item.DataOfMarriage;
                        pd.EndDate = item.EndDate;
                        pd.Unit = item.Unit;
                        pd.Reporting = item.Reporting;
                        pd.WarehouseId = item.WarehouseId;
                        pd.DepositAmount = item.DepositAmount;
                        pd.tempdel = item.tempdel;
                        pd.ReportPersonId = item.ReportPersonId;
                        ac.Entry(pd).State = EntityState.Modified;
                        ac.Commit();
                        // for add data history
                        //pd.UpdateBy = UserName;
                        //ac.AddPeopleHistory(pd);


                        if (item.Warehouses.Count > 0)
                        {
                            foreach (var wh in item.Warehouses)
                            {
                                using (var con = new AuthContext())
                                {
                                    var isadd = con.WarehousePermissionDB.Where(x => x.PeopleID == item.PeopleID && x.WarehouseId == wh.id).FirstOrDefault();
                                    if (isadd == null)
                                    {
                                        var wp = new WarehousePermission();
                                        wp.WarehouseId = wh.id;
                                        wp.PeopleID = item.PeopleID;
                                        wp.IsActive = true;
                                        wp.IsDeleted = false;
                                        wp.CreatedDate = DateTime.Now;
                                        wp.CreatedBy = userid;
                                        con.WarehousePermissionDB.Add(wp);
                                        con.Commit();
                                    }
                                    else
                                    {
                                        isadd.IsActive = true;
                                        isadd.IsDeleted = false;
                                        con.Commit();
                                    }
                                }
                            }
                        }
                        else
                        {
                            using (var con = new AuthContext())
                            {
                                var isupdate = con.WarehousePermissionDB.Where(x => x.PeopleID == item.PeopleID).ToList();
                                foreach (var up in isupdate)
                                {
                                    up.IsActive = false;
                                    up.IsDeleted = true;
                                    con.Commit();
                                }
                            }
                        }
                    }
                    catch (Exception ex) { }
                    try
                    {
                        PSalary Ps = ac.PeoplesSalaryDB.Where(x => x.PeopleID == item.PeopleID).SingleOrDefault();
                        if (Ps != null) // people Id Not Found In People Salary Table....
                        {
                            Ps.Salary = item.Salary;
                            Ps.B_Salary = item.B_Salary;
                            Ps.Hra_Salary = item.Hra_Salary;
                            Ps.CA_Salary = item.CA_Salary;
                            Ps.DA_Salary = item.DA_Salary;
                            Ps.Lta_Salary = item.Lta_Salary;
                            Ps.PF_Salary = item.PF_Salary;
                            Ps.ESI_Salary = item.ESI_Salary;
                            Ps.M_Incentive = item.M_Incentive;
                            Ps.Y_Incentive = item.Y_Incentive;
                            ac.Commit();
                        }
                        else
                        {
                            PSalary ps = new PSalary();
                            ps.PeopleID = item.PeopleID;
                            ps.Salary = item.Salary;
                            ps.B_Salary = item.B_Salary;
                            ps.Hra_Salary = item.Hra_Salary;
                            ps.CA_Salary = item.CA_Salary;
                            ps.DA_Salary = item.DA_Salary;
                            ps.Lta_Salary = item.Lta_Salary;
                            ps.PF_Salary = item.PF_Salary;
                            ps.ESI_Salary = item.ESI_Salary;
                            ps.M_Incentive = item.M_Incentive;
                            Ps.Y_Incentive = item.Y_Incentive;
                            ac.PeoplesSalaryDB.Add(Ps);
                            ac.Commit();
                        }
                    }
                    catch (Exception ex)
                    { }
                    //try
                    //{
                    //    PeopleDocument PDOC = ac.PeopleDocumentDB.Where(x => x.PeopleId == item.PeopleID.ToString()).SingleOrDefault();
                    //    if (PDOC != null) // people Id Not Found In People Document Table....
                    //    {
                    //        PDOC.Id_Proof = item.Id_Proof;
                    //        PDOC.Address_Proof = item.Address_Proof;
                    //        PDOC.MarkSheet = item.MarkSheet;
                    //        PDOC.Pre_SalarySlip = item.Pre_SalarySlip;
                    //        ac.SaveChanges();
                    //    }
                    //    else
                    //    {
                    //        PeopleDocument PDOC1 = new PeopleDocument();
                    //        PDOC1.PeopleId = item.PeopleID.ToString();
                    //        PDOC1.Id_Proof = item.Id_Proof;
                    //        PDOC1.Address_Proof = item.Address_Proof;
                    //        PDOC1.MarkSheet = item.MarkSheet;
                    //        PDOC1.Pre_SalarySlip = item.Pre_SalarySlip;
                    //        ac.PeopleDocumentDB.Add(PDOC1);
                    //        ac.SaveChanges();
                    //    }
                    //}
                    //catch (Exception ex)
                    //{ }
                    return item; // context.PutPeoplebyAdmin(item);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add Peoples " + ex.Message);

                    return null;
                }
        }

        #region Update after first step Save
        /// <param name="item"></param>
        /// <returns></returns>
        [ResponseType(typeof(People))]
        [Route("Putnew")]
        [AcceptVerbs("PUT")]
        public PeopleAll Putdata(PeopleAll item)// this Code Updated By Shoaib on 13/12/2018
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                String UserName = null;
                // Access claims
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
                    if (claim.Type == "username")
                    {
                        UserName = (claim.Value);
                    }
                }

                item.CompanyId = compid;
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }
                using (AuthContext ac = new AuthContext())
                {

                    try
                    {
                        PSalary Ps = ac.PeoplesSalaryDB.Where(x => x.PeopleID == item.PeopleID).SingleOrDefault();
                        if (Ps != null) // people Id Not Found In People Salary Table....
                        {
                            Ps.Salary = item.Salary;
                            Ps.B_Salary = item.B_Salary;
                            Ps.Hra_Salary = item.Hra_Salary;
                            Ps.CA_Salary = item.CA_Salary;
                            Ps.DA_Salary = item.DA_Salary;
                            Ps.Lta_Salary = item.Lta_Salary;
                            Ps.PF_Salary = item.PF_Salary;
                            Ps.ESI_Salary = item.ESI_Salary;
                            Ps.M_Incentive = item.M_Incentive;
                            ac.Commit();
                        }
                        else
                        {
                            PSalary ps = new PSalary();
                            ps.PeopleID = item.PeopleID;
                            ps.PeopleName = item.PeopleFirstName;
                            ps.Salary = item.Salary;
                            ps.B_Salary = item.B_Salary;
                            ps.Hra_Salary = item.Hra_Salary;
                            ps.CA_Salary = item.CA_Salary;
                            ps.DA_Salary = item.DA_Salary;
                            ps.Lta_Salary = item.Lta_Salary;
                            ps.PF_Salary = item.PF_Salary;
                            ps.ESI_Salary = item.ESI_Salary;
                            ps.M_Incentive = item.M_Incentive;
                            ps.Y_Incentive = item.Y_Incentive;
                            ac.PeoplesSalaryDB.Add(ps);
                            ac.Commit();
                        }
                    }
                    catch (Exception ex)
                    { }
                    try
                    {
                        PeopleDocument PDOC = ac.PeopleDocumentDB.Where(x => x.PeopleId == item.PeopleID.ToString()).SingleOrDefault();
                        if (PDOC != null) // people Id Not Found In People Document Table....
                        {
                            PDOC.Id_Proof = item.Id_Proof;
                            PDOC.Address_Proof = item.Address_Proof;
                            PDOC.MarkSheet = item.MarkSheet;
                            PDOC.Pre_SalarySlip = item.Pre_SalarySlip;
                            ac.Commit();
                        }
                        else
                        {
                            PeopleDocument PDOC1 = new PeopleDocument();
                            PDOC1.PeopleId = item.PeopleID.ToString();
                            PDOC1.Id_Proof = item.Id_Proof;
                            PDOC1.Address_Proof = item.Address_Proof;
                            PDOC1.MarkSheet = item.MarkSheet;
                            PDOC1.Pre_SalarySlip = item.Pre_SalarySlip;
                            ac.PeopleDocumentDB.Add(PDOC1);
                            ac.Commit();
                        }
                    }
                    catch (Exception ex)
                    { }
                    return item; // context.PutPeoplebyAdmin(item);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Add Peoples " + ex.Message);

                return null;
            }
        }
        #endregion

        [ResponseType(typeof(People))]
        [Route("retailer")]
        [AcceptVerbs("POST")]
        public People postfromaapp(string mob, string password)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                // Access claims
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
                }
                using (AuthContext db = new AuthContext())
                {
                    int CompanyId = compid;
                    //  People check = context.CheckPeople(mob, password,CompanyId);
                    People check = db.CheckPeople(mob, password);

                    return check;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Add Peoples " + ex.Message);
                return null;

            }

        }


        [Route("Email")]
        [HttpGet]
        public HttpResponseMessage CheckEmail(string Email)
        {

            try
            {
                logger.Info("Get Peoples: ");
                int compid = 0, userid = 0;
                int Warehouse_id = 0;
                string email = "";
                var identity = User.Identity as ClaimsIdentity;
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
                    if (claim.Type == "email")
                    {
                        email = claim.Value;
                    }
                }
                using (AuthContext db = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {
                        var RDEmail = db.Peoples.Where(x => x.Email == Email && x.WarehouseId == Warehouse_id).FirstOrDefault();

                        return Request.CreateResponse(HttpStatusCode.OK, RDEmail);
                    }
                    else
                    {
                        var RDEmail = db.Peoples.Where(x => x.Email == Email).FirstOrDefault();

                        return Request.CreateResponse(HttpStatusCode.OK, RDEmail);

                    }
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, false);
            }
        }
        [Route("Mobile")]
        [HttpGet]
        public HttpResponseMessage CheckMobile(string Mobile)
        {

            try
            {

                int compid = 0, userid = 0;
                int Warehouse_id = 0;
                string email = "";
                var identity = User.Identity as ClaimsIdentity;
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
                    if (claim.Type == "email")
                    {
                        email = claim.Value;
                    }
                }
                using (AuthContext db = new AuthContext())
                {
                    if (Warehouse_id > 0)
                    {
                        var RDMobile = db.Peoples.Where(x => x.Mobile == Mobile && x.WarehouseId == Warehouse_id).FirstOrDefault();

                        return Request.CreateResponse(HttpStatusCode.OK, RDMobile);
                    }
                    else
                    {
                        var RDMobile = db.Peoples.Where(x => x.Mobile == Mobile).FirstOrDefault();

                        return Request.CreateResponse(HttpStatusCode.OK, RDMobile);

                    }
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, false);
            }
        }

        #region Get PeopleImages
        [Route("PeopleImages")]
        [HttpGet]
        public dynamic PeopleImages(string PeopleId)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0, Warehouse_id = 0;
                // Access claims
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
                int CompanyId = compid;
                using (AuthContext db = new AuthContext())
                {

                    var data = db.PeopleDocumentDB.Where(x => x.PeopleId == PeopleId).ToList();
                    return data;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        [Route("AddImage")]
        [HttpPost]
        public HttpResponseMessage AddImage(PeopleDocument item, string name, string mobile)
        {

            logger.Info("Add Peopleimage:");
            try
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                // Access claims
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
                }
                using (AuthContext db = new AuthContext())
                {
                    People p = db.Peoples.Where(x => x.PeopleFirstName == name && x.Mobile == mobile).SingleOrDefault();
                    PeopleDocument ps = new PeopleDocument();
                    ps.PeopleId = p.PeopleID.ToString();
                    ps.Id_Proof = item.Id_Proof;
                    ps.Address_Proof = item.Address_Proof;
                    ps.MarkSheet = item.MarkSheet;
                    ps.Pre_SalarySlip = item.Pre_SalarySlip;
                    db.PeopleDocumentDB.Add(ps);
                    db.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK, "Success");
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Add Peoples " + ex.Message);

                return null;
            }
        }

        [Route("AddSalary")]
        [HttpPost]
        public HttpResponseMessage AddSalary(PSalary item, string name, string mobile)
        {

            logger.Info("Add PeopleSalary: ");
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                // Access claims
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
                }
                using (AuthContext db = new AuthContext())
                {
                    People p = db.Peoples.Where(x => x.PeopleFirstName == name && x.Mobile == mobile).SingleOrDefault();
                    PSalary ps = new PSalary();
                    ps.PeopleID = p.PeopleID;
                    ps.PeopleName = p.PeopleFirstName + p.PeopleLastName;
                    ps.Salary = item.Salary;
                    ps.B_Salary = item.B_Salary;
                    ps.Hra_Salary = item.Hra_Salary;
                    ps.CA_Salary = item.CA_Salary;
                    ps.DA_Salary = item.DA_Salary;
                    ps.Lta_Salary = item.Lta_Salary;
                    ps.PF_Salary = item.PF_Salary;
                    ps.ESI_Salary = item.ESI_Salary;
                    ps.M_Incentive = item.M_Incentive;
                    ps.Y_Incentive = item.Y_Incentive;
                    db.PeoplesSalaryDB.Add(ps);
                    db.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK, "Success");
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Add Peoples " + ex.Message);

                return null;
            }
        }

        [HttpPost]
        [Route("PeopleWarehousePermission")]
        public void PeopleWarehousePermission(int peopleid, List<warehouse> warehouse)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (warehouse.Count > 0)
            {
                foreach (var wh in warehouse)
                {
                    using (var con = new AuthContext())
                    {
                        var wp = new WarehousePermission();
                        wp.WarehouseId = wh.id;
                        wp.PeopleID = peopleid;
                        wp.IsActive = true;
                        wp.IsDeleted = false;
                        wp.CreatedDate = DateTime.Now;
                        wp.CreatedBy = userid;
                        con.WarehousePermissionDB.Add(wp);
                        con.Commit();
                    }
                }
            }
        }
        //[Route("UploadDocument")]
        //public async Task<HttpResponseMessage> Post()
        //{
        //    // Check whether the POST operation is MultiPart?
        //    if (!Request.Content.IsMimeMultipartContent())
        //    {
        //        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        //    }

        //    // Prepare CustomMultipartFormDataStreamProvider in which our multipart form
        //    // data will be loaded.
        //    string fileSaveLocation = HttpContext.Current.Server.MapPath("~/UploadedImages");
        //    CustomMultipartFormDataStreamProvider provider = new CustomMultipartFormDataStreamProvider(fileSaveLocation);
        //    List<string> files = new List<string>();

        //    try
        //    {
        //        // Read all contents of multipart message into CustomMultipartFormDataStreamProvider.
        //        var result=await Request.Content.ReadAsMultipartAsync(provider);
        //        var model = result.FormData["jsonData"];
        //        if (model == null)
        //        {
        //            throw new HttpResponseException(HttpStatusCode.BadRequest);
        //        }
        //        string CaseNumber = model;

        //        foreach (MultipartFileData file in provider.FileData)
        //        {
        //            files.Add(Path.GetFileName(file.LocalFileName));
        //            //CaseImage obj = new CaseImage()
        //            //     {
        //            //         CaseNumber = CaseNumber,
        //            //         CaseImageName = file.LocalFileName,
        //            //         active = true,
        //            //         Deleted = false,
        //            //         CreatedDate = indianTime,
        //            //         UpdatedDate = indianTime
        //            //     };
        //            //     db.CaseImage.Add(obj);
        //            //     db.SaveChanges();
        //        }

        //        // Send OK Response along with saved file names to the client.
        //        return Request.CreateResponse(HttpStatusCode.OK, files);
        //    }
        //    catch (System.Exception e)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
        //    }
        //}

        //// We implement MultipartFormDataStreamProvider to override the filename of File which
        //// will be stored on server, or else the default name will be of the format like Body-
        //// Part_{GUID}. In the following implementation we simply get the FileName from 
        //// ContentDisposition Header of the Request Body.
        //public class CustomMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
        //{
        //    public CustomMultipartFormDataStreamProvider(string path) : base(path) { }

        //    public override string GetLocalFileName(HttpContentHeaders headers)
        //    {
        //        return headers.ContentDisposition.FileName.Replace("\"", string.Empty);
        //    }
        //}
        [Route("mapcluster")]
        [AcceptVerbs("POST")]
        public Peoplecluster Post(clumap obj)
        {

            try
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                // Access claims
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
                }
                using (AuthContext db = new AuthContext())
                {
                    int CompanyId = compid;
                    //  People check = context.CheckPeople(mob, password,CompanyId);
                    //  Peoplecluster ass = db.Peoplecluster.Where(x => x.ClusterId == c).ToList();
                    People p = db.Peoples.Where(a => a.Mobile == obj.Mobile).SingleOrDefault();

                    foreach (var a in obj.ids)
                    {
                        Peoplecluster pc = new Peoplecluster();
                        pc.PeopleID = p.PeopleID;
                        pc.ClusterId = a.id;
                        db.PeopleclusterDB.Add(pc);
                        db.Commit();

                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Add Peoples " + ex.Message);
                return null;

            }
        }

        /// <summary>
        /// Get History Data in list After Removing
        /// // By Danish ------19/04/2019
        /// </summary>
        /// <param name="PeopleId"></param>
        /// <returns></returns>
        [Route("DeletedHistorydata")]
        public HttpResponseMessage GetHistroydata(int PeopleId)
        {

            int compid = 0, userid = 0;
            try
            {
                var identity = User.Identity as ClaimsIdentity;

                // Access claims
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
                }
                using (AuthContext db = new AuthContext())
                {
                    List<PeopleHistory> person = db.PeopleHistoryDB.Where(u => u.PeopleID == PeopleId).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, person);
                }
            }

            catch (Exception ex)
            {
                logger.Error("Error in getting Peoples " + ex.Message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, false);

            }
        }

        [ResponseType(typeof(People))]
        [Route("UpdatePeopleV7")]
        [AcceptVerbs("PUT")]
        public async Task<IHttpActionResult> UpdatePeopleV7(PeopleAll item)// this Code Updated By Shoaib on 13/12/2018
        {
            var peopleResponse = new Peopleresponse();
            PeopleOutputVM peopleOutputVm = new PeopleOutputVM();
            string userID = null;
            using (var ac = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    String UserName = null;
                    // Access claims
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
                        if (claim.Type == "username")
                        {
                            UserName = (claim.Value);
                        }
                    }
                    item.CompanyId = compid;
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    People pd = ac.Peoples.Where(x => x.PeopleID == item.PeopleID).SingleOrDefault();
                    int empCodecount = 0;
                    if (!string.IsNullOrEmpty(item.Empcode))
                    {
                        string EmpCodequery = "select count(PeopleID) from People where Empcode='" + item.Empcode + "'and PeopleID!="+ item.PeopleID  + "and Active = 1 and Deleted=0";
                        empCodecount = ac.Database.SqlQuery<int>(EmpCodequery).FirstOrDefault();
                    }
                    string empcode = null;
                    if (!string.IsNullOrEmpty(item.Mobile))
                    {
                        string query = "select empcode from People where Mobile='" + item.Mobile + "' and PeopleID!=" + item.PeopleID + "and Active = 1 and Deleted=0";
                        empcode = ac.Database.SqlQuery<string>(query).FirstOrDefault();
                    }

                    if (empcode != null)
                    {
                        peopleOutputVm.ErrorMessage = "Mobile Number Already Exists in EmpCode : "+empcode;
                        peopleOutputVm.Succeeded = false;
                        return Ok(peopleOutputVm);
                    }

                    if (empCodecount==0)
                    {
                        string email = item.Email;
                        var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new AuthContext()));
                        var user = manager.FindByEmail(email);
                        if (user == null)
                        {
                            byte Levels = 4;
                            //item.Email = item.Email.Split('@')[0];
                            var u = new ApplicationUser()
                            {

                                UserName = item.Email,
                                Email = item.Email + "@shopKirana.com",//+ "@shopKirana.com",
                                FirstName = item.PeopleFirstName,
                                LastName = item.PeopleLastName,
                                Level = Levels,
                                JoinDate = DateTime.Now.Date,
                                EmailConfirmed = true
                            };
                            IdentityResult result = await manager.CreateAsync(u, item.Password);
                            if (result.Succeeded)
                            {
                                var adminUser = manager.FindByName(u.UserName);//var adminUser = manager.FindByName(displayname);
                                userID = adminUser.Id;
                                Designation desg = ac.DesignationsDB.Where(x => x.DesignationName == item.Desgination).SingleOrDefault();
                                pd.PeopleFirstName = item.PeopleFirstName;
                                pd.PeopleLastName = item.PeopleLastName;
                                pd.Email = item.Email + "@shopKirana.com";
                                pd.state = item.state;
                                pd.Stateid = item.Stateid;
                                pd.city = item.city;
                                pd.Cityid = item.Cityid;
                                pd.Mobile = item.Mobile;
                                pd.Department = item.Department;
                                pd.Password = item.Password != null ? item.Password : pd.Password;
                                pd.Active = item.Active;
                                pd.Desgination = item.Desgination;
                                pd.Unit = item.Unit;
                                pd.Status = item.Status;
                                pd.Active = item.Active;
                                pd.DOB = item.DOB;
                                pd.DataOfJoin = item.DataOfJoin;
                                pd.DataOfMarriage = item.DataOfMarriage;
                                pd.EndDate = item.EndDate;
                                pd.Unit = item.Unit;
                                pd.Reporting = item.Reporting;
                                pd.WarehouseId = item.WarehouseId;
                                pd.DepositAmount = item.DepositAmount;
                                pd.tempdel = item.tempdel;
                                pd.ReportPersonId = item.ReportPersonId;
                                if (item.Cityid > 0)
                                {
                                    var city = ac.Cities.FirstOrDefault(x => x.Cityid == item.Cityid);
                                    pd.city = city.CityName;
                                    pd.state = city.StateName;
                                    pd.Stateid = city.Stateid;
                                }
                                ac.Entry(pd).State = EntityState.Modified;
                                ac.Commit();

                                if (!pd.Active && pd.FcmId != null)
                                {

                                    List<string> list = new List<string>();
                                    list.Add(pd.FcmId);
                                    UserLogoutNotification(list);
                                }
                                // for add data history
                                //pd.UpdateBy = UserName;
                                //ac.AddPeopleHistory(pd);


                                if (item.Warehouses != null && item.Warehouses.Count > 0)
                                {
                                    foreach (var wh in item.Warehouses)
                                    {
                                        var isadd = ac.WarehousePermissionDB.Where(x => x.PeopleID == item.PeopleID && x.WarehouseId == wh.id).FirstOrDefault();
                                        if (isadd == null)
                                        {
                                            var wp = new WarehousePermission();
                                            wp.WarehouseId = wh.id;
                                            wp.PeopleID = item.PeopleID;
                                            wp.IsActive = true;
                                            wp.IsDeleted = false;
                                            wp.CreatedDate = DateTime.Now;
                                            wp.CreatedBy = userid;
                                            ac.WarehousePermissionDB.Add(wp);
                                            ac.Commit();
                                        }
                                        else
                                        {
                                            isadd.IsActive = true;
                                            isadd.IsDeleted = false;
                                            ac.Commit();
                                        }
                                    }
                                    if (item.OldWarehouses != null && item.OldWarehouses.Count > 0)
                                    {

                                        var oldWare = item.OldWarehouses.Where(p => !item.Warehouses.Any(x => x.id == p.id)).ToList();
                                        foreach (var i in oldWare)
                                        {
                                            var isadd = ac.WarehousePermissionDB.Where(x => x.PeopleID == item.PeopleID && x.WarehouseId == i.id).FirstOrDefault();
                                            isadd.IsActive = false;
                                            isadd.IsDeleted = true;
                                            ac.Commit();
                                        }
                                    }
                                }
                                else
                                {
                                    var isupdate = ac.WarehousePermissionDB.Where(x => x.PeopleID == item.PeopleID).ToList();
                                    foreach (var up in isupdate)
                                    {
                                        up.IsActive = false;
                                        up.IsDeleted = true;
                                        ac.Commit();
                                    }

                                }
                                // }

                                peopleOutputVm.Succeeded = result.Succeeded;
                                peopleOutputVm.UserID = userID;
                                return Ok(peopleOutputVm);
                                //return Ok("Success");
                            }
                            else
                            {
                                peopleOutputVm.ErrorMessage = result.Errors.FirstOrDefault();
                                peopleOutputVm.Succeeded = result.Succeeded;
                                peopleOutputVm.UserID = userID;
                                return Ok(peopleOutputVm);
                                //return Ok(result.Errors.FirstOrDefault());
                            }
                        }
                        else
                        {
                            if (user.PasswordHash != null)
                            {
                                manager.RemovePassword(user.Id);
                            }
                            var result = manager.AddPassword(user.Id, item.Password);

                            if (result.Succeeded)
                            {
                                var adminUser = manager.FindByName(user.UserName);//var adminUser = manager.FindByName(displayname);
                                userID = adminUser.Id;
                                var oldWHid = pd.WarehouseId;
                                Designation desg = ac.DesignationsDB.Where(x => x.DesignationName == item.Desgination).SingleOrDefault();
                                pd.PeopleFirstName = item.PeopleFirstName;
                                pd.PeopleLastName = item.PeopleLastName;
                                pd.Email = item.Email;
                                pd.state = item.state;
                                pd.Stateid = item.Stateid;
                                pd.city = item.city;
                                pd.Cityid = item.Cityid;
                                pd.Mobile = item.Mobile;
                                pd.Department = item.Department;
                                pd.Password = item.Password != null ? item.Password : pd.Password;
                                pd.Active = item.Active;
                                pd.Desgination = item.Desgination;
                                pd.Empcode = item.Empcode;
                                pd.Unit = item.Unit;
                                pd.Status = item.Status;
                                pd.DOB = item.DOB;
                                pd.DataOfJoin = item.DataOfJoin;
                                pd.DataOfMarriage = item.DataOfMarriage;
                                pd.EndDate = item.EndDate;
                                pd.Unit = item.Unit;
                                pd.Reporting = item.Reporting;
                                pd.WarehouseId = item.WarehouseId;
                                pd.DepositAmount = item.DepositAmount;
                                pd.tempdel = item.tempdel;
                                pd.ReportPersonId = item.ReportPersonId;
                                if (item.Cityid > 0)
                                {
                                    var city = ac.Cities.FirstOrDefault(x => x.Cityid == item.Cityid);
                                    pd.city = city.CityName;
                                    pd.state = city.StateName;
                                    pd.Stateid = city.Stateid;
                                }
                                ac.Entry(pd).State = EntityState.Modified;

                                List<string> names = new List<string>();
                                string query = "select distinct r.Name from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.PeopleID='" + pd.PeopleID + "' and p.companyid=" + pd.CompanyId + "  and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                                names = ac.Database.SqlQuery<string>(query).ToList();

                                var IsSalesExecutiveRole = names != null ? names.FirstOrDefault(x => x == "Sales Executive" || x == "Telecaller") != null ? names.FirstOrDefault(x => x == "Sales Executive" || x == "Telecaller") : "" : "";

                                if (IsSalesExecutiveRole != null && (pd.Active == false || pd.WarehouseId != item.WarehouseId))
                                {
                                    var updatecustomerexecutive = ac.ClusterStoreExecutives.Where(x => x.ExecutiveId == item.PeopleID && x.IsDeleted == false && x.IsActive == true).ToList();
                                    if (updatecustomerexecutive != null && updatecustomerexecutive.Count() > 0)
                                    {
                                        foreach (var up in updatecustomerexecutive)
                                        {
                                            up.IsActive = false;
                                            up.IsDeleted = true;
                                            up.ModifiedDate = DateTime.Now;
                                            up.ExecutiveId = item.PeopleID;
                                            ac.Entry(up).State = EntityState.Modified;
                                            ac.Commit();
                                        }
                                        var updatecustomerexecutivee = ac.ClusterStoreExecutives.Where(x => x.ExecutiveId == item.PeopleID).ToList();
                                        foreach (var upp in updatecustomerexecutivee)
                                        {
                                            var data = ac.ClusterStoreExecutiveHistories.Where(x => x.ClusterStoreExecutiveId == upp.Id).FirstOrDefault();
                                            if (data != null)
                                            {
                                                data.ClusterId = upp.ClusterId;
                                                data.StoreId = upp.StoreId;
                                                data.ExecutiveId = upp.ExecutiveId;
                                                data.EndDate = DateTime.Now;
                                                ac.Entry(data).State = EntityState.Modified;
                                                ac.Commit();
                                            }
                                            else
                                            {
                                                ClusterStoreExecutiveHistory clusterStoreExecutiveHistory = new ClusterStoreExecutiveHistory();
                                                clusterStoreExecutiveHistory.ClusterId = upp.ClusterId;
                                                clusterStoreExecutiveHistory.StoreId = upp.StoreId;
                                                clusterStoreExecutiveHistory.ExecutiveId = upp.ExecutiveId;
                                                clusterStoreExecutiveHistory.StartDate = DateTime.Now;
                                                clusterStoreExecutiveHistory.EndDate = DateTime.Now;
                                                clusterStoreExecutiveHistory.ClusterStoreExecutiveId = upp.Id;
                                                ac.ClusterStoreExecutiveHistories.Add(clusterStoreExecutiveHistory);
                                                ac.Commit();
                                            }
                                        }
                                    }

                                    //var updatecustomerclusterexecutive = ac.ClusterStoreExecutiveHistories.Where(x => x.ExecutiveId == item.PeopleID).ToList();
                                    //if (updatecustomerclusterexecutive.Count > 0)
                                    //{
                                    //    foreach (var upp in updatecustomerclusterexecutive)
                                    //    {
                                    //        upp.EndDate = DateTime.Now;
                                    //        ac.Entry(upp).State = EntityState.Modified;
                                    //        ac.Commit();
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    foreach (var upp in updatecustomerexecutive)
                                    //    {
                                    //        ClusterStoreExecutiveHistory clusterStoreExecutiveHistory = new ClusterStoreExecutiveHistory();
                                    //        clusterStoreExecutiveHistory.ClusterId = upp.ClusterId;
                                    //        clusterStoreExecutiveHistory.StoreId = upp.StoreId;
                                    //        clusterStoreExecutiveHistory.ExecutiveId = upp.ExecutiveId;
                                    //        clusterStoreExecutiveHistory.EndDate = DateTime.Now;
                                    //        clusterStoreExecutiveHistory.ClusterStoreExecutiveId = upp.Id;
                                    //        ac.ClusterStoreExecutiveHistories.Add(clusterStoreExecutiveHistory);
                                    //        ac.Commit();
                                    //    }
                                    //}

                                }

                                if (oldWHid != item.WarehouseId)
                                {
                                    var updatecustomerexecutive = ac.ClusterStoreExecutives.Where(x => x.ExecutiveId == item.PeopleID && x.IsDeleted == false && x.IsActive == true).ToList();
                                    foreach (var up in updatecustomerexecutive)
                                    {
                                        up.IsActive = false;
                                        up.IsDeleted = true;
                                        up.ModifiedDate = DateTime.Now;
                                        up.ExecutiveId = item.PeopleID;
                                        up.ModifiedBy = userid;
                                        ac.Entry(up).State = EntityState.Modified;
                                        ac.Commit();
                                    }
                                }

                                //Update ClusterStoreExecutives Set IsActive=0, IsDeleted=1, ModifiedDate=getdate() Where IsActive=1 and IsDeleted=0 And ExecutiveId=2400

                                // for add data history
                                //pd.UpdateBy = UserName;
                                //ac.AddPeopleHistory(pd);

                                ac.Commit();

                                //Sned 


                                if (item.Warehouses != null && item.Warehouses.Count > 0)
                                {
                                    foreach (var wh in item.Warehouses)
                                    {
                                        var isadd = ac.WarehousePermissionDB.Where(x => x.PeopleID == item.PeopleID && x.WarehouseId == wh.id).FirstOrDefault();
                                        if (isadd == null)
                                        {
                                            var wp = new WarehousePermission();
                                            wp.WarehouseId = wh.id;
                                            wp.PeopleID = item.PeopleID;
                                            wp.IsActive = true;
                                            wp.IsDeleted = false;
                                            wp.CreatedDate = DateTime.Now;
                                            wp.CreatedBy = userid;
                                            ac.WarehousePermissionDB.Add(wp);
                                            ac.Commit();
                                        }
                                        else
                                        {
                                            isadd.IsActive = true;
                                            isadd.IsDeleted = false;
                                            ac.Commit();
                                        }

                                    }
                                    if (item.OldWarehouses != null && item.OldWarehouses.Count > 0)
                                    {

                                        var oldWare = item.OldWarehouses.Where(p => !item.Warehouses.Any(x => x.id == p.id)).ToList();
                                        foreach (var i in oldWare)
                                        {
                                            var isadd = ac.WarehousePermissionDB.Where(x => x.PeopleID == item.PeopleID && x.WarehouseId == i.id).FirstOrDefault();
                                            isadd.IsActive = false;
                                            isadd.IsDeleted = true;
                                            ac.Commit();
                                        }
                                    }
                                }
                                else
                                {

                                    var isupdate = ac.WarehousePermissionDB.Where(x => x.PeopleID == item.PeopleID).ToList();
                                    foreach (var up in isupdate)
                                    {
                                        up.IsActive = false;
                                        up.IsDeleted = true;
                                        ac.Commit();
                                    }

                                }
                            }
                            else
                            {
                                peopleOutputVm.ErrorMessage = result.Errors.FirstOrDefault();
                                peopleOutputVm.Succeeded = result.Succeeded;
                                peopleOutputVm.UserID = userID;
                                return Ok(peopleOutputVm);
                                //return Ok(result.Errors.FirstOrDefault());
                            }
                            peopleOutputVm.Succeeded = result.Succeeded;
                            peopleOutputVm.UserID = userID;
                            return Ok(peopleOutputVm);
                            //return Ok("Success");
                        }
                    }
                    else
                    {
                        peopleOutputVm.ErrorMessage = "Employee Code Already Exists";
                        peopleOutputVm.Succeeded = false;
                        return Ok(peopleOutputVm);
                        
                    }
                    // context.PutPeoplebyAdmin(item);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add Peoples " + ex.Message);

                    return InternalServerError(); ;
                }
        }
        #region Generate Sales App OTP
        /// <summary>
        /// Created by 29/04/2019 //by Sudhir
        /// OTP Genration code 
        /// </summary> //tejas added parameters 
        /// <returns></returns>

        //[Route("GenotpForSalesApp")]
        //[HttpPost]
        //[AllowAnonymous]
        //public HttpResponseMessage SalesAppLoginByotp(People customer)
        //{

        //    logger.Info("start Gen OTP: ");
        //    SalesDTO res;
        //    string error = "";
        //    People People = new People();
        //    string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.Mobile='" + customer.Mobile + "' and r.Name='Sales Executive' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
        //    People = db.Database.SqlQuery<People>(query).FirstOrDefault();
        //    //People = db.Peoples.Where(x => x.Mobile == customer.Mobile && x.Department == "Sales Executive" && x.Deleted == false && x.Active == true).FirstOrDefault();
        //    try
        //    {
        //        if (People.Mobile == customer.Mobile)
        //        {
        //            People.FcmId = customer.FcmId;
        //            People.DeviceId = customer.DeviceId;
        //            People.CurrentAPKversion = customer.CurrentAPKversion;   //tejas for device info 
        //            People.PhoneOSversion = customer.PhoneOSversion;
        //            People.UserDeviceName = customer.UserDeviceName;
        //            People.IMEI = customer.IMEI;//sudhir for device info 
        //            //People.UpdatedDate = indianTime;
        //            //db.Peoples.Attach(People);
        //            db.Entry(People).State = EntityState.Modified;
        //            db.Commit();
        //            #region Device History
        //            var Customerhistory = db.Peoples.Where(x => x.Mobile == People.Mobile).FirstOrDefault();
        //            try
        //            {
        //                PhoneRecordHistory phonerecord = new PhoneRecordHistory();
        //                if (Customerhistory != null)
        //                {
        //                    phonerecord.PeopleID = Customerhistory.PeopleID;
        //                    phonerecord.PeopleFirstName = Customerhistory.PeopleFirstName;
        //                    phonerecord.Department = Customerhistory.Department;
        //                    phonerecord.Mobile = Customerhistory.Mobile;
        //                    phonerecord.CurrentAPKversion = Customerhistory.CurrentAPKversion;
        //                    phonerecord.IMEI = Customerhistory.IMEI;
        //                    phonerecord.PhoneOSversion = Customerhistory.PhoneOSversion;
        //                    phonerecord.UserDeviceName = Customerhistory.UserDeviceName;
        //                    phonerecord.UpdatedDate = DateTime.Now;
        //                    db.PhoneRecordHistoryDB.Add(phonerecord);
        //                    int id = db.Commit();
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                logger.Error("Error loading  \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
        //            }
        //            #endregion

        //        }
        //        if (People == null)
        //        {
        //            res = new SalesDTO()
        //            {
        //                P = null,
        //                Status = false,
        //                Message = "Not a Registered Sales Executive"
        //            };
        //            return Request.CreateResponse(HttpStatusCode.OK, res);
        //        }
        //        else
        //        {
        //            using (var context = new AuthContext())
        //                try
        //                {
        //                    string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
        //                    string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);
        //                    string OtpMessage = " is Your Shopkirana login Code. :)";
        //                    //string CountryCode = "91";
        //                    //string Sender = "SHOPKR";
        //                    //string authkey = Startup.smsauthKey; //"100498AhbWDYbtJT56af33e3";
        //                    //int route = 4;
        //                    //string path = "http://bulksms.newrise.in/api/sendhttp.php?authkey=" + authkey + "&mobiles=" + People.Mobile + "&message=" + sRandomOTP + " :" + OtpMessage + " &sender=" + Sender + "&route=" + route + "&country=" + CountryCode;

        //                    ////string path ="http://bulksms.newrise.in/api/sendhttp.php?authkey=100498AhbWDYbtJT56af33e3&mobiles=9770838685&message= SK OTP is : " + sRandomOTP + " &sender=SHOPKR&route=4&country=91";

        //                    //var webRequest = (HttpWebRequest)WebRequest.Create(path);
        //                    //webRequest.Method = "GET";
        //                    //webRequest.ContentType = "application/json";
        //                    //webRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
        //                    //webRequest.ContentLength = 0; // added per comment 
        //                    //webRequest.Credentials = CredentialCache.DefaultCredentials;
        //                    //webRequest.Accept = "*/*";
        //                    //var webResponse = (HttpWebResponse)webRequest.GetResponse();
        //                    bool result = Common.Helpers.SendSMSHelper.SendSMS(People.Mobile, (sRandomOTP + " :" + OtpMessage), ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString());
        //                    if (!result)
        //                    {
        //                        logger.Info("OTP Genrated: " + sRandomOTP);
        //                    }
        //                    else
        //                    {
        //                        logger.Info("OTP Genrated: " + sRandomOTP);

        //                        var check = context.CheckPeopleSalesPersonData(customer.Mobile);
        //                        check.OTP = sRandomOTP;
        //                        if (check != null)
        //                        {
        //                            res = new SalesDTO
        //                            {
        //                                P = check,
        //                                Status = true,
        //                                Message = "Success."

        //                            };
        //                            return Request.CreateResponse(HttpStatusCode.OK, res);
        //                        }
        //                        else
        //                        {
        //                            res = new SalesDTO
        //                            {
        //                                P = null,
        //                                Status = false,
        //                                Message = "Not Success"

        //                            };
        //                            return Request.CreateResponse(HttpStatusCode.OK, res);
        //                        }
        //                    }
        //                }
        //                catch (Exception sdf)
        //                {

        //                }
        //        }
        //    }
        //    catch (Exception es)
        //    {
        //        error = error + es;
        //    }
        //    res = new SalesDTO()
        //    {
        //        P = null,
        //        Status = false,
        //        Message = ("This is something went wrong Sales Executive : " + error)
        //    };
        //    return Request.CreateResponse(HttpStatusCode.OK, res);
        //}

        /// <summary>
        /// Created by 29/04/2019 //by Sudhir
        /// OTP Genration code 
        /// </summary> //tejas added parameters 
        /// <returns></returns>
        //[Route("GenotpForSalesAppV2")]
        //[HttpPost]
        //[AllowAnonymous]
        //public HttpResponseMessage SalesAppLoginByotpV2(People customer)
        //{

        //    logger.Info("start Gen OTP: ");
        //    SalesDTO res;
        //    string error = "";
        //    People People = new People();
        //    string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.Mobile='" + customer.Mobile + "' and r.Name='Sales Executive' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
        //    People = db.Database.SqlQuery<People>(query).FirstOrDefault();
        //    //People = db.Peoples.Where(x => x.Mobile == customer.Mobile && x.Department == "Sales Executive" && x.Deleted == false && x.Active == true).FirstOrDefault();
        //    try
        //    {
        //        if (People == null)
        //        {
        //            res = new SalesDTO()
        //            {
        //                P = null,
        //                Status = false,
        //                Message = "Not a Registered Sales Executive"
        //            };
        //            return Request.CreateResponse(HttpStatusCode.OK, res);
        //        }
        //        else
        //        {
        //            using (var context = new AuthContext())
        //                try
        //                {
        //                    string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
        //                    string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);
        //                    string OtpMessage = " is Your Shopkirana login Code. :)";
        //                    //string CountryCode = "91";
        //                    //string Sender = "SHOPKR";
        //                    //string authkey = Startup.smsauthKey; //"100498AhbWDYbtJT56af33e3";
        //                    //int route = 4;
        //                    //string path = "http://bulksms.newrise.in/api/sendhttp.php?authkey=" + authkey + "&mobiles=" + People.Mobile + "&message=" + sRandomOTP + " :" + OtpMessage + " &sender=" + Sender + "&route=" + route + "&country=" + CountryCode;

        //                    ////string path ="http://bulksms.newrise.in/api/sendhttp.php?authkey=100498AhbWDYbtJT56af33e3&mobiles=9770838685&message= SK OTP is : " + sRandomOTP + " &sender=SHOPKR&route=4&country=91";

        //                    //var webRequest = (HttpWebRequest)WebRequest.Create(path);
        //                    //webRequest.Method = "GET";
        //                    //webRequest.ContentType = "application/json";
        //                    //webRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
        //                    //webRequest.ContentLength = 0; // added per comment 
        //                    //webRequest.Credentials = CredentialCache.DefaultCredentials;
        //                    //webRequest.Accept = "*/*";
        //                    //var webResponse = (HttpWebResponse)webRequest.GetResponse();
        //                    bool result = Common.Helpers.SendSMSHelper.SendSMS(People.Mobile, (sRandomOTP + " :" + OtpMessage), ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString());
        //                    if (!result)
        //                    {
        //                        logger.Info("OTP Genrated: " + sRandomOTP);
        //                    }
        //                    else
        //                    {
        //                        logger.Info("OTP Genrated: " + sRandomOTP);

        //                        var check = context.CheckPeopleSalesPersonData(customer.Mobile);
        //                        check.OTP = sRandomOTP;
        //                        if (check != null)
        //                        {
        //                            res = new SalesDTO
        //                            {

        //                                P = new People { OTP = sRandomOTP },
        //                                Status = true,
        //                                Message = "Success."

        //                            };
        //                            return Request.CreateResponse(HttpStatusCode.OK, res);
        //                        }
        //                        else
        //                        {
        //                            res = new SalesDTO
        //                            {
        //                                P = null,
        //                                Status = false,
        //                                Message = "Not Success"

        //                            };
        //                            return Request.CreateResponse(HttpStatusCode.OK, res);
        //                        }
        //                    }
        //                }
        //                catch (Exception sdf)
        //                {
        //                    res = new SalesDTO
        //                    {
        //                        P = null,
        //                        Status = false,
        //                        Message = "Not Success"

        //                    };
        //                    return Request.CreateResponse(HttpStatusCode.OK, res);
        //                }
        //        }
        //    }
        //    catch (Exception es)
        //    {
        //        error = error + es;
        //    }
        //    res = new SalesDTO()
        //    {
        //        P = null,
        //        Status = false,
        //        Message = ("This is something went wrong Sales Executive : " + error)
        //    };
        //    return Request.CreateResponse(HttpStatusCode.OK, res);
        //}


        /// <summary>
        /// Get Exist customer detail on login time
        /// </summary>   // tejas to save device info 
        /// <param name="MobileNumber"></param>
        /// <param name="IsOTPverified"></param>
        /// <returns></returns>
        //[Route("GetLogedSalesPerson")]
        //[AcceptVerbs("GET")]
        //[HttpGet]
        //[AllowAnonymous]

        //public HttpResponseMessage GetLogedSalesPerson(string MobileNumber, bool IsOTPverified, string fcmid, string CurrentAPKversion, string PhoneOSversion, string DeviceId, string UserDeviceName, string IMEI = "")
        //{
        //    SalesDTO res;
        //    using (var db = new AuthContext())
        //    {
        //        try
        //        {
        //            if (IsOTPverified == true)
        //            {
        //                People People = new People();
        //                string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.Mobile='" + MobileNumber + "' and r.Name='Sales Executive' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
        //                People = db.Database.SqlQuery<People>(query).FirstOrDefault();
        //                //People = db.Peoples.Where(x => x.Mobile == MobileNumber && x.Department == "Sales Executive" && x.Deleted == false && x.Active == true).FirstOrDefault();
        //                if (People != null)
        //                {
        //                    People.FcmId = fcmid;
        //                    People.DeviceId = DeviceId;
        //                    People.CurrentAPKversion = CurrentAPKversion;   //tejas for device info 
        //                    People.PhoneOSversion = PhoneOSversion;
        //                    People.UserDeviceName = UserDeviceName;
        //                    People.IMEI = IMEI;
        //                    //People.UpdatedDate = indianTime;
        //                    //db.Peoples.Attach(People);
        //                    db.Entry(People).State = EntityState.Modified;
        //                    db.Commit();
        //                    #region Device History
        //                    var Customerhistory = db.Peoples.Where(x => x.Mobile == People.Mobile).FirstOrDefault();
        //                    try
        //                    {
        //                        PhoneRecordHistory phonerecord = new PhoneRecordHistory();
        //                        if (Customerhistory != null)
        //                        {
        //                            phonerecord.PeopleID = Customerhistory.PeopleID;
        //                            phonerecord.PeopleFirstName = Customerhistory.PeopleFirstName;
        //                            phonerecord.Department = Customerhistory.Department;
        //                            phonerecord.Mobile = Customerhistory.Mobile;
        //                            phonerecord.CurrentAPKversion = Customerhistory.CurrentAPKversion;
        //                            phonerecord.PhoneOSversion = Customerhistory.PhoneOSversion;
        //                            phonerecord.UserDeviceName = Customerhistory.UserDeviceName;
        //                            phonerecord.IMEI = Customerhistory.IMEI;
        //                            phonerecord.UpdatedDate = DateTime.Now;
        //                            db.PhoneRecordHistoryDB.Add(phonerecord);
        //                            int id = db.Commit();
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        logger.Error("Error loading  \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
        //                    }
        //                    #endregion

        //                    var registeredApk = db.GetAPKUserAndPwd("SalesApp");
        //                    People.RegisteredApk = registeredApk;
        //                    string queryrole = "select distinct r.Name as Role from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where PeopleID='" + People.PeopleID + "'and ur.isActive=1 and p.Active=1 and p.Deleted=0";
        //                    var role = db.Database.SqlQuery<string>(queryrole).ToList();

        //                    var IsRole = role.Any(x => x.Contains("Hub sales lead"));
        //                    if (IsRole)
        //                    {
        //                        People.Role = "Hub sales lead";
        //                    }
        //                    else {

        //                        People.Role = "";

        //                    }
        //                    res = new SalesDTO()
        //                    {
        //                        P = People,
        //                        Status = true,
        //                        Message = "Success."
        //                    };
        //                    return Request.CreateResponse(HttpStatusCode.OK, res);
        //                }
        //                else
        //                {
        //                    res = new SalesDTO()
        //                    {
        //                        P = null,
        //                        Status = false,
        //                        Message = "SalesPerson does not exist or Incorect mobile number."
        //                    };
        //                    return Request.CreateResponse(HttpStatusCode.OK, res);
        //                }
        //            }
        //            else
        //            {

        //                res = new SalesDTO()
        //                {
        //                    P = null,
        //                    Status = false,
        //                    Message = "OTP not verified."
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, res);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error(ex.Message);
        //            res = new SalesDTO()
        //            {
        //                P = null,
        //                Status = false,
        //                Message = "Some Error Occurred."
        //            };
        //            return Request.CreateResponse(HttpStatusCode.OK, res);
        //        }
        //    }
        //}


        #endregion


        #region Generate Random OTP
        /// <summary>
        /// Created by 29/04/2019 
        /// Create rendom otp//By Sudhir
        /// </summary>
        /// <param name="iOTPLength"></param>
        /// <param name="saAllowedCharacters"></param>
        /// <returns></returns>
        private string GenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)
        {
            string sOTP = String.Empty;
            string sTempChars = String.Empty;
            Random rand = new Random();

            for (int i = 0; i < iOTPLength; i++)
            {
                int p = rand.Next(0, saAllowedCharacters.Length);
                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
                sOTP += sTempChars;
            }
            return sOTP;
        }
        #endregion


        #region  Get People History 
        /// <summary>
        /// Created Date 16-03-2019
        /// </summary>
        /// <param name="PeopleId"></param>
        /// <returns></returns>


        [Route("History")]
        public HttpResponseMessage GetHistroy(int PeopleId)
        {
            using (var db = new AuthContext())
            {
                int compid = 0, userid = 0;
                try
                {
                    var identity = User.Identity as ClaimsIdentity;

                    // Access claims
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
                    }
                    List<PeopleHistory> person = db.PeopleHistoryDB.Where(u => u.PeopleID == PeopleId).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, person);

                }

                catch (Exception ex)
                {
                    logger.Error("Error in getting Peoples " + ex.Message);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, false);

                }

            }
        }
        /// <summary>
        /// Created Date 16-03-2019
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>


        [Route("HistoryById")]
        public HttpResponseMessage GetbyidHistroy(int Id)
        {
            using (var db = new AuthContext())
            {
                int compid = 0, userid = 0;
                try
                {
                    var identity = User.Identity as ClaimsIdentity;

                    // Access claims
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
                    }
                    var person = db.PeopleHistoryDB.Where(u => u.Id == Id).FirstOrDefault();
                    return Request.CreateResponse(HttpStatusCode.OK, person);

                }

                catch (Exception ex)
                {
                    logger.Error("Error in getting Peoples " + ex.Message);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, false);

                }

            }
        }

        //Pravesh Starts : Get Agent by Filtering Warehouse
        [HttpGet]
        [Route("GetAgentWarehouse")]
        public IEnumerable<PeopleAll> WarehouseId(int WarehouseId)
        {
            logger.Info("Get Peoples: ");
            int compid = 0, userid = 0;
            int Warehouse_id = 0;
            string email = "";
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;

                    // Access claims
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
                        if (claim.Type == "email")
                        {
                            email = claim.Value;
                        }
                    }

                    List<PeopleAll> person = new List<PeopleAll>();

                    if (WarehouseId > 0)
                    {
                        person = (from pd in db.Peoples
                                  where pd.CompanyId == compid && pd.WarehouseId == WarehouseId && pd.Active == true
                                  select new PeopleAll
                                  {
                                      PeopleID = pd.PeopleID,
                                      DisplayName = pd.DisplayName,

                                  }).ToList();

                    }
                    return person;
                }

                catch (Exception ex)
                {
                    logger.Error("Error in getting Peoples " + ex.Message);
                    return null;
                }


            }
        }
        #endregion

        #region get Agent and Dbay Devicehistory
        /// <summary>
        /// tejas 28-05-2019
        /// </summary>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        [Route("AgentnDboyDevicehistory")]
        [HttpGet]
        public dynamic AgentnDboyDevicehistory(int PeopleID)
        {
            using (AuthContext odd = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0, Warehouse_id = 0;
                    // Access claims
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
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    var data = odd.PhoneRecordHistoryDB.Where(x => x.PeopleID == PeopleID).ToList();
                    return data;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            #endregion

            //Pravesh End  
        }


        #region Test Dapper for CRUD

        [Route("GetByIdAsync")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<People> GetByIdAsync(int peopleId)
        {
            var peopleManager = new PeopleManager();
            return peopleManager.GetPeopleById(peopleId);
        }


        #endregion

        [Route("GetAutoComplete/{keyword}")]
        [HttpGet]
        public List<PeopleAutoCompleteVM> GetAutoComplete(string keyword)
        {
            using (var context = new AuthContext())
            {
                var peopleList = context.Peoples.Where(p => (!string.IsNullOrEmpty(p.Email) && p.Email.ToLower().Contains(keyword.ToLower()))
                                          || (!string.IsNullOrEmpty(p.DisplayName) && p.DisplayName.ToLower().Contains(keyword.ToLower()))
                                          || (!string.IsNullOrEmpty(p.UserName) && p.UserName.ToLower().Contains(keyword.ToLower()))
                                          || (!string.IsNullOrEmpty(p.Mobile) && p.Mobile.ToLower().Contains(keyword.ToLower()))
                                      ).Select(x => new PeopleAutoCompleteVM
                                      {
                                          PeopleID = x.PeopleID,
                                          DisplayName = x.DisplayName,
                                          UserName = x.UserName,
                                          Email = x.Email,
                                          Mobile = x.Mobile
                                      }).Take(50).ToList();
                return peopleList;
            }
        }


        [Route("EmployeeLogin")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage EmployeeLogin(string mob, string password)
        {
            using (var db = new AuthContext())
            {
                Peopleresponse res;
                People People = new People();
                People = db.Peoples.Where(x => x.Mobile == mob && x.Active == true && x.Deleted == false).FirstOrDefault();
                if (People != null)
                {
                    if (People.Password == password)
                    {
                        #region Device History
                        PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                        phonerecord.PeopleID = People.PeopleID;
                        phonerecord.PeopleFirstName = People.PeopleFirstName;
                        phonerecord.Department = People.Department;
                        phonerecord.Mobile = People.Mobile;
                        phonerecord.CurrentAPKversion = People.CurrentAPKversion;
                        phonerecord.IMEI = People.IMEI;
                        phonerecord.PhoneOSversion = People.PhoneOSversion;
                        phonerecord.UserDeviceName = People.UserDeviceName;
                        phonerecord.UpdatedDate = DateTime.Now;
                        db.PhoneRecordHistoryDB.Add(phonerecord);
                        int id = db.Commit();
                        #endregion

                        res = new Peopleresponse
                        {
                            people = People,
                            Status = true,
                            message = "Success."

                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        res = new Peopleresponse
                        {
                            people = null,
                            Status = false,
                            message = "Wrong Password."

                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                else
                {
                    res = new Peopleresponse
                    {
                        people = null,
                        Status = false,
                        message = "People not found."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }

        [Route("GetEmployee")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetEmployee(string mob)
        {
            using (var db = new AuthContext())
            {
                Peopleresponse res;
                People People = new People();
                People = db.Peoples.Where(x => x.Mobile == mob && x.Active == true && x.Deleted == false).FirstOrDefault();
                if (People != null)
                {
                    #region Device History
                    PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                    phonerecord.PeopleID = People.PeopleID;
                    phonerecord.PeopleFirstName = People.PeopleFirstName;
                    phonerecord.Department = People.Department;
                    phonerecord.Mobile = People.Mobile;
                    phonerecord.CurrentAPKversion = People.CurrentAPKversion;
                    phonerecord.IMEI = People.IMEI;
                    phonerecord.PhoneOSversion = People.PhoneOSversion;
                    phonerecord.UserDeviceName = People.UserDeviceName;
                    phonerecord.UpdatedDate = DateTime.Now;
                    db.PhoneRecordHistoryDB.Add(phonerecord);
                    int id = db.Commit();
                    #endregion

                    res = new Peopleresponse
                    {
                        people = People,
                        Status = true,
                        message = "Success."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    res = new Peopleresponse
                    {
                        people = null,
                        Status = false,
                        message = "People not found."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }


        [Route("GenEmployeeotp")]
        [HttpGet]
        [AllowAnonymous]
        public OTP Getotp(string MobileNumber)
        {
            People emp = null;
            using (var context = new AuthContext())
            {
                emp = context.Peoples.Where(c => c.Mobile.Trim().Equals(MobileNumber.Trim())).FirstOrDefault();

            }
            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);
            // string OtpMessage = string.Format("<#> {0} : is Your Shopkirana Verification Code for complete process.{1}{2} Shopkirana", sRandomOTP, Environment.NewLine, "SKEMP");
            string OtpMessage = ""; //{#var1#} : is Your Verification Code for complete process. {#var2#} ShopKirana
            var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.SalesApp, "Customer_Verification_Code_AutoRead");
            OtpMessage = dltSMS == null ? "" : dltSMS.Template;
            OtpMessage = OtpMessage.Replace("{#var1#}", "<#> {0}");
            OtpMessage = OtpMessage.Replace("{#var2#}", sRandomOTP);
            //string message = OtpMessage;
            var status = dltSMS == null ? false : Common.Helpers.SendSMSHelper.SendSMS(MobileNumber, OtpMessage, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);

            OTP a = new OTP()
            {
                OtpNo = emp != null ? sRandomOTP : "Employee Not Found"
            };
            return a;
        }
        [Route("GetPeopleAll")]
        public ResPeopleAll GetAllV2(int skip, int take)
        {
            int Skiplist = (skip - 1) * take;
            logger.Info("Get Peoples: ");
            int compid = 0, userid = 0;
            int Warehouse_id = 0;
            string email = "";
            int count = 0;
            ResPeopleAll res = new ResPeopleAll();

            using (var context = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;

                    // Access claims
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
                        if (claim.Type == "email")
                        {
                            email = claim.Value;
                        }
                    }
                    using (AuthContext db = new AuthContext())
                    {
                        List<PeopleAll> personList = new List<PeopleAll>();

                        if (Warehouse_id > 0)
                        {
                            personList = (from pd in db.Peoples
                                          join ps in db.PeoplesSalaryDB
                                          on pd.PeopleID equals ps.PeopleID into pdps
                                          from x in pdps.DefaultIfEmpty()
                                          join pDoc in db.PeopleDocumentDB
                                          on pd.PeopleID.ToString() equals pDoc.PeopleId into pdpDoc
                                          from y in pdpDoc.DefaultIfEmpty()
                                          where pd.CompanyId == compid
                                          select new PeopleAll
                                          {
                                              PeopleID = pd.PeopleID,
                                              Empcode = pd.Empcode,
                                              CompanyId = pd.CompanyId,
                                              WarehouseId = pd.WarehouseId,
                                              Email = pd.Email,
                                              Country = pd.Country,
                                              Stateid = pd.Stateid,
                                              state = pd.state,
                                              Cityid = pd.Cityid,
                                              city = pd.city,
                                              Mobile = pd.Mobile,
                                              Password = pd.Password,
                                              RoleId = pd.RoleId,
                                              DisplayName = pd.DisplayName,
                                              Department = pd.Department,
                                              BillableRate = pd.BillableRate,
                                              CostRate = pd.CostRate,
                                              //Permissions = pd.Permissions,
                                              SUPPLIERCODES = pd.SUPPLIERCODES,
                                              Type = pd.Type,
                                              Approved = pd.Approved,
                                              PeopleFirstName = pd.PeopleFirstName,
                                              PeopleLastName = pd.PeopleLastName,
                                              Active = pd.Active,
                                              CreatedDate = pd.CreatedDate,
                                              UpdatedDate = pd.UpdatedDate,
                                              CreatedBy = pd.CreatedBy,
                                              UpdateBy = pd.UpdateBy,
                                              Skcode = pd.Skcode,
                                              AgentCode = pd.AgentCode,
                                              Salesexecutivetype = pd.Salesexecutivetype,
                                              AgentAmount = pd.AgentAmount,
                                              Desgination = pd.Desgination,
                                              Status = pd.Status,
                                              DOB = pd.DOB,
                                              DataOfJoin = pd.DataOfJoin,
                                              DataOfMarriage = pd.DataOfMarriage,
                                              EndDate = pd.EndDate,
                                              Unit = pd.Unit,
                                              Reporting = pd.Reporting,
                                              IfscCode = pd.IfscCode,
                                              Account_Number = pd.Account_Number,
                                              UserName = pd.UserName,
                                              Salary = x.Salary,
                                              B_Salary = x.B_Salary,
                                              Hra_Salary = x.Hra_Salary,
                                              CA_Salary = x.CA_Salary,
                                              DA_Salary = x.DA_Salary,
                                              Lta_Salary = x.Lta_Salary,
                                              PF_Salary = x.PF_Salary,
                                              ESI_Salary = x.ESI_Salary,
                                              M_Incentive = x.M_Incentive,
                                              Y_Incentive = x.Y_Incentive,

                                              MarkSheet = y.MarkSheet,
                                              Id_Proof = y.Id_Proof,
                                              Address_Proof = y.Address_Proof,
                                              PanCard_Proof = y.PanCard_Proof,
                                              Pre_SalarySlip = y.Pre_SalarySlip,
                                              tempdel = pd.tempdel,
                                              ReportPersonId = pd.ReportPersonId

                                          }).OrderByDescending(x => x.PeopleID).Skip(Skiplist).Take(take).ToList();
                            count = (from pd in db.Peoples
                                     join ps in db.PeoplesSalaryDB
                                     on pd.PeopleID equals ps.PeopleID into pdps
                                     from x in pdps.DefaultIfEmpty()
                                     join pDoc in db.PeopleDocumentDB
                                     on pd.PeopleID.ToString() equals pDoc.PeopleId into pdpDoc
                                     from y in pdpDoc.DefaultIfEmpty()
                                     where pd.CompanyId == compid
                                     select new PeopleAll { }).Count();

                            res.person = personList;
                            res.TotalCount = count;
                            //return res;
                        }
                        else
                        {

                            personList = (from pd in db.Peoples
                                          join ps in db.PeoplesSalaryDB
                                          on pd.PeopleID equals ps.PeopleID into pdps
                                          from x in pdps.DefaultIfEmpty()
                                          join pDoc in db.PeopleDocumentDB
                                          on pd.PeopleID.ToString() equals pDoc.PeopleId into pdpDoc
                                          from y in pdpDoc.DefaultIfEmpty()
                                          where pd.CompanyId == compid && pd.Deleted == false//by Sudhir 22-04-2019
                                          orderby pd.PeopleID
                                          select new PeopleAll
                                          {
                                              PeopleID = pd.PeopleID,
                                              Empcode = pd.Empcode,
                                              CompanyId = pd.CompanyId,
                                              WarehouseId = pd.WarehouseId,
                                              Email = pd.Email,
                                              Country = pd.Country,
                                              Stateid = pd.Stateid,
                                              state = pd.state,
                                              Cityid = pd.Cityid,
                                              city = pd.city,
                                              Mobile = pd.Mobile,
                                              Password = pd.Password,
                                              RoleId = pd.RoleId,
                                              DisplayName = pd.DisplayName,
                                              Department = pd.Department,
                                              BillableRate = pd.BillableRate,
                                              CostRate = pd.CostRate,
                                              // Permissions = pd.Permissions,
                                              SUPPLIERCODES = pd.SUPPLIERCODES,
                                              Type = pd.Type,
                                              Approved = pd.Approved,
                                              PeopleFirstName = pd.PeopleFirstName,
                                              PeopleLastName = pd.PeopleLastName,
                                              Active = pd.Active,
                                              CreatedDate = pd.CreatedDate,
                                              UpdatedDate = pd.UpdatedDate,
                                              CreatedBy = pd.CreatedBy,
                                              UpdateBy = pd.UpdateBy,
                                              Skcode = pd.Skcode,
                                              AgentCode = pd.AgentCode,
                                              Salesexecutivetype = pd.Salesexecutivetype,
                                              AgentAmount = pd.AgentAmount,
                                              Desgination = pd.Desgination,
                                              Status = pd.Status,
                                              DOB = pd.DOB,
                                              DataOfJoin = pd.DataOfJoin,
                                              DataOfMarriage = pd.DataOfMarriage,
                                              EndDate = pd.EndDate,
                                              Unit = pd.Unit,
                                              Reporting = pd.Reporting,
                                              IfscCode = pd.IfscCode,
                                              Account_Number = pd.Account_Number,
                                              UserName = pd.UserName,
                                              Salary = x.Salary,
                                              B_Salary = x.B_Salary,
                                              Hra_Salary = x.Hra_Salary,
                                              CA_Salary = x.CA_Salary,
                                              DA_Salary = x.DA_Salary,
                                              Lta_Salary = x.Lta_Salary,
                                              PF_Salary = x.PF_Salary,
                                              ESI_Salary = x.ESI_Salary,
                                              M_Incentive = x.M_Incentive,
                                              Y_Incentive = x.Y_Incentive,

                                              MarkSheet = y.MarkSheet,
                                              Id_Proof = y.Id_Proof,
                                              PanCard_Proof = y.PanCard_Proof,
                                              Address_Proof = y.Address_Proof,
                                              Pre_SalarySlip = y.Pre_SalarySlip,
                                              tempdel = pd.tempdel,
                                              ReportPersonId = pd.ReportPersonId

                                          }).OrderByDescending(x => x.PeopleID).Skip(Skiplist).Take(take).ToList();

                            count = (from pd in db.Peoples
                                     join ps in db.PeoplesSalaryDB
                                     on pd.PeopleID equals ps.PeopleID into pdps
                                     from x in pdps.DefaultIfEmpty()
                                     join pDoc in db.PeopleDocumentDB
                                     on pd.PeopleID.ToString() equals pDoc.PeopleId into pdpDoc
                                     from y in pdpDoc.DefaultIfEmpty()
                                     where pd.CompanyId == compid && pd.Deleted == false
                                     orderby pd.PeopleID
                                     select new PeopleAll { }).Count();

                            res.person = personList;
                            res.TotalCount = count;
                        }
                        return res;
                    }
                    //return res;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting Peoples " + ex.Message);
                    return null;
                }
        }

        [Route("GetPeopleByKey")]
        public IEnumerable<PeopleAll> GetPeopleByKey(string key)
        {
            logger.Info("Get Peoples: ");
            int compid = 0, userid = 0;
            int Warehouse_id = 0;
            string email = "";
            using (var context = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;

                    // Access claims
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
                        if (claim.Type == "email")
                        {
                            email = claim.Value;
                        }
                    }
                    using (AuthContext db = new AuthContext())
                    {
                        List<PeopleAll> person = new List<PeopleAll>();

                        if (Warehouse_id > 0)
                        {
                            person = (from pd in db.Peoples
                                      join ps in db.PeoplesSalaryDB
                                      on pd.PeopleID equals ps.PeopleID into pdps
                                      from x in pdps.DefaultIfEmpty()
                                      join pDoc in db.PeopleDocumentDB
                                      on pd.PeopleID.ToString() equals pDoc.PeopleId into pdpDoc
                                      from y in pdpDoc.DefaultIfEmpty()
                                      where pd.CompanyId == compid && (pd.PeopleFirstName.Contains(key) || pd.Mobile.Contains(key) || pd.Empcode.Contains(key))
                                      select new PeopleAll
                                      {
                                          PeopleID = pd.PeopleID,
                                          Empcode = pd.Empcode,
                                          CompanyId = pd.CompanyId,
                                          WarehouseId = pd.WarehouseId,
                                          Email = pd.Email,
                                          Country = pd.Country,
                                          Stateid = pd.Stateid,
                                          state = pd.state,
                                          Cityid = pd.Cityid,
                                          city = pd.city,
                                          Mobile = pd.Mobile,
                                          Password = pd.Password,
                                          RoleId = pd.RoleId,
                                          DisplayName = pd.DisplayName,
                                          Department = pd.Department,
                                          BillableRate = pd.BillableRate,
                                          CostRate = pd.CostRate,
                                          //Permissions = pd.Permissions,
                                          SUPPLIERCODES = pd.SUPPLIERCODES,
                                          Type = pd.Type,
                                          Approved = pd.Approved,
                                          PeopleFirstName = pd.PeopleFirstName,
                                          PeopleLastName = pd.PeopleLastName,
                                          Active = pd.Active,
                                          CreatedDate = pd.CreatedDate,
                                          UpdatedDate = pd.UpdatedDate,
                                          CreatedBy = pd.CreatedBy,
                                          UpdateBy = pd.UpdateBy,
                                          Skcode = pd.Skcode,
                                          AgentCode = pd.AgentCode,
                                          Salesexecutivetype = pd.Salesexecutivetype,
                                          AgentAmount = pd.AgentAmount,
                                          Desgination = pd.Desgination,
                                          Status = pd.Status,
                                          DOB = pd.DOB,
                                          DataOfJoin = pd.DataOfJoin,
                                          DataOfMarriage = pd.DataOfMarriage,
                                          EndDate = pd.EndDate,
                                          Unit = pd.Unit,
                                          Reporting = pd.Reporting,
                                          IfscCode = pd.IfscCode,
                                          Account_Number = pd.Account_Number,
                                          UserName = pd.UserName,
                                          Salary = x.Salary,
                                          B_Salary = x.B_Salary,
                                          Hra_Salary = x.Hra_Salary,
                                          CA_Salary = x.CA_Salary,
                                          DA_Salary = x.DA_Salary,
                                          Lta_Salary = x.Lta_Salary,
                                          PF_Salary = x.PF_Salary,
                                          ESI_Salary = x.ESI_Salary,
                                          M_Incentive = x.M_Incentive,
                                          Y_Incentive = x.Y_Incentive,

                                          MarkSheet = y.MarkSheet,
                                          Id_Proof = y.Id_Proof,
                                          Address_Proof = y.Address_Proof,
                                          PanCard_Proof = y.PanCard_Proof,
                                          Pre_SalarySlip = y.Pre_SalarySlip,
                                          tempdel = pd.tempdel,
                                          ReportPersonId = pd.ReportPersonId

                                      }).ToList();
                            return person;
                        }
                        else
                        {

                            person = (from pd in db.Peoples
                                      join ps in db.PeoplesSalaryDB
                                      on pd.PeopleID equals ps.PeopleID into pdps
                                      from x in pdps.DefaultIfEmpty()
                                      join pDoc in db.PeopleDocumentDB
                                      on pd.PeopleID.ToString() equals pDoc.PeopleId into pdpDoc
                                      from y in pdpDoc.DefaultIfEmpty()
                                      where pd.CompanyId == compid && pd.Deleted == false && (pd.PeopleFirstName.Contains(key) || pd.Mobile.Contains(key) || pd.Empcode.Contains(key))
                                      orderby pd.PeopleID
                                      select new PeopleAll
                                      {
                                          PeopleID = pd.PeopleID,
                                          Empcode = pd.Empcode,
                                          CompanyId = pd.CompanyId,
                                          WarehouseId = pd.WarehouseId,
                                          Email = pd.Email,
                                          Country = pd.Country,
                                          Stateid = pd.Stateid,
                                          state = pd.state,
                                          Cityid = pd.Cityid,
                                          city = pd.city,
                                          Mobile = pd.Mobile,
                                          Password = pd.Password,
                                          RoleId = pd.RoleId,
                                          DisplayName = pd.DisplayName,
                                          Department = pd.Department,
                                          BillableRate = pd.BillableRate,
                                          CostRate = pd.CostRate,
                                          // Permissions = pd.Permissions,
                                          SUPPLIERCODES = pd.SUPPLIERCODES,
                                          Type = pd.Type,
                                          Approved = pd.Approved,
                                          PeopleFirstName = pd.PeopleFirstName,
                                          PeopleLastName = pd.PeopleLastName,
                                          Active = pd.Active,
                                          CreatedDate = pd.CreatedDate,
                                          UpdatedDate = pd.UpdatedDate,
                                          CreatedBy = pd.CreatedBy,
                                          UpdateBy = pd.UpdateBy,
                                          Skcode = pd.Skcode,
                                          AgentCode = pd.AgentCode,
                                          Salesexecutivetype = pd.Salesexecutivetype,
                                          AgentAmount = pd.AgentAmount,
                                          Desgination = pd.Desgination,
                                          Status = pd.Status,
                                          DOB = pd.DOB,
                                          DataOfJoin = pd.DataOfJoin,
                                          DataOfMarriage = pd.DataOfMarriage,
                                          EndDate = pd.EndDate,
                                          Unit = pd.Unit,
                                          Reporting = pd.Reporting,
                                          IfscCode = pd.IfscCode,
                                          Account_Number = pd.Account_Number,
                                          UserName = pd.UserName,
                                          Salary = x.Salary,
                                          B_Salary = x.B_Salary,
                                          Hra_Salary = x.Hra_Salary,
                                          CA_Salary = x.CA_Salary,
                                          DA_Salary = x.DA_Salary,
                                          Lta_Salary = x.Lta_Salary,
                                          PF_Salary = x.PF_Salary,
                                          ESI_Salary = x.ESI_Salary,
                                          M_Incentive = x.M_Incentive,
                                          Y_Incentive = x.Y_Incentive,

                                          MarkSheet = y.MarkSheet,
                                          Id_Proof = y.Id_Proof,
                                          PanCard_Proof = y.PanCard_Proof,
                                          Address_Proof = y.Address_Proof,
                                          Pre_SalarySlip = y.Pre_SalarySlip,
                                          tempdel = pd.tempdel,
                                          ReportPersonId = pd.ReportPersonId

                                      }).ToList();
                            return person;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getting Peoples " + ex.Message);
                    return null;
                }
        }




        [Route("UploadPeopleProfileImage")]
        [HttpPost]
        [AllowAnonymous]
        public string UploadPeopleProfileImage()
        {
            string LogoUrl = "";
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/PeopleProfileImage")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/PeopleProfileImage"));

                        string extension = Path.GetExtension(httpPostedFile.FileName);
                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/PeopleProfileImage"), fileName);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(fileName, "~/PeopleProfileImage", LogoUrl);
                        LogoUrl = "/PeopleProfileImage/" + fileName;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in UploadPeopleProfileImage Method: " + ex.Message);
            }
            return LogoUrl;
        }

        [Route("GetPeopleCityInfo/{PeopleId}")]
        [HttpGet]
        public async Task<PeopleCityDc> GetPeopleCityInfo(int PeopleId)
        {
            var result = new PeopleCityDc();
            if (PeopleId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    var param = new SqlParameter("@PeopleId", PeopleId);
                    result = context.Database.SqlQuery<PeopleCityDc>("exec GetPeopleCityInfo @PeopleId", param).FirstOrDefault();
                }
            }
            return result;
        }

        [HttpGet]
        [Route("PeoplevisitList")]
        public bool PeoplevisitList()
        {
            bool res = false;
            List<PageVisitDC> pageVisitDCList = new List<PageVisitDC>();
            MongoDbHelper<PageVisits> mongoDbHelper = new MongoDbHelper<PageVisits>();
            List<PageVisits> pageVisitList = new List<PageVisits>();
            var searchPredicate = PredicateBuilder.New<PageVisits>(x => x.UserName != null);
            pageVisitList = mongoDbHelper.Select(searchPredicate).OrderByDescending(x => x.Id).ToList();
            pageVisitDCList = Mapper.Map(pageVisitList).ToANew<List<PageVisitDC>>();

            List<AppVisitDC> appVisitDCList = new List<AppVisitDC>();
            MongoDbHelper<AppVisits> mongoAppVisitDbHelper = new MongoDbHelper<AppVisits>();
            List<AppVisits> appVisitList = new List<AppVisits>();
            var searchPredicateDetail = PredicateBuilder.New<AppVisits>(x => x.UserName != null);
            appVisitList = mongoAppVisitDbHelper.Select(searchPredicateDetail).OrderByDescending(x => x.Id).ToList();
            appVisitDCList = Mapper.Map(appVisitList).ToANew<List<AppVisitDC>>();


            List<PeopleDC> peopledata = new List<PeopleDC>();
            InactivePeople inactivePeopleData = new InactivePeople();
            EmailRecipients emailRecipients = new EmailRecipients();
            using (var myContext = new AuthContext())
            {
                peopledata = myContext.Database.SqlQuery<PeopleDC>("GetPeopleData").ToList();
                var InActiveCustomerInHrs = myContext.CompanyDetailsDB.Where(x => x.IsActive && x.InActiveCustomerInHrs > 0).OrderByDescending(x => x.Id).FirstOrDefault();
                if (pageVisitDCList.Count > 0)
                {
                    foreach (var data in peopledata)
                    {
                        foreach (var pagedVisitata in pageVisitDCList)
                        {
                            if (data.UserName == pagedVisitata.UserName)
                            {
                                pagedVisitata.RemainingTimeinHrs = ((pagedVisitata.VisitedOn - DateTime.Now.AddMinutes(-InActiveCustomerInHrs.InActiveCustomerInHrs)).Minutes);

                                if (pagedVisitata.RemainingTimeinHrs > InActiveCustomerInHrs.InActiveCustomerInHrs || pagedVisitata.RemainingTimeinHrs < 0)//168
                                {
                                    var VisitedDate = pagedVisitata.VisitedOn.ToString("dd/MM/yyyy");
                                    var CurrentDate = DateTime.Today.AddMinutes(-InActiveCustomerInHrs.InActiveCustomerInHrs).ToString("dd/MM/yyyy");
                                    var we = Convert.ToDateTime(pagedVisitata.VisitedOn);
                                    var abc = Convert.ToDateTime(DateTime.Now.AddMinutes(-InActiveCustomerInHrs.InActiveCustomerInHrs));
                                    var people = myContext.Peoples.Where(x => x.PeopleID == data.PeopleID).FirstOrDefault();
                                    var inactivePeopleDetail = myContext.InactivePeopleDB.Where(x => x.PeopleID == data.PeopleID && !x.Active && !x.Deleted).OrderByDescending(x => x.Id).FirstOrDefault();
                                    if (inactivePeopleDetail == null && (Convert.ToDateTime(pagedVisitata.VisitedOn) < Convert.ToDateTime(DateTime.Now.AddMinutes(-InActiveCustomerInHrs.InActiveCustomerInHrs))))//DateTime.Today.AddDays(-7)
                                    {
                                        inactivePeopleData.Active = false;
                                        inactivePeopleData.PeopleFirstName = data.PeopleFirstName;
                                        inactivePeopleData.PeopleLastName = data.PeopleLastName;
                                        inactivePeopleData.UserName = data.UserName;
                                        inactivePeopleData.PeopleID = data.PeopleID;
                                        inactivePeopleData.Mobile = data.Mobile;
                                        inactivePeopleData.Empcode = data.Empcode;
                                        inactivePeopleData.Department = data.Department;
                                        inactivePeopleData.Desgination = data.Desgination;
                                        inactivePeopleData.DisplayName = data.DisplayName;
                                        inactivePeopleData.city = data.city;
                                        inactivePeopleData.CreatedBy = GetLoginUserId();
                                        inactivePeopleData.CreationDate = DateTime.Now;
                                        inactivePeopleData.LastVisitedDate = pagedVisitata.VisitedOn;
                                        myContext.InactivePeopleDB.Add(inactivePeopleData);

                                        people.Active = false;
                                        people.UpdatedDate = DateTime.Now;
                                        myContext.Entry(people).State = EntityState.Modified;
                                        myContext.Commit();
                                    }
                                }
                            }
                        }
                    }
                }
                if (appVisitDCList.Count > 0)
                {
                    foreach (var data in peopledata)
                    {
                        foreach (var appVisitata in appVisitDCList)
                        {
                                if (data.Mobile == appVisitata.UserName)
                                {

                                    appVisitata.RemainingTimeinHrs = ((appVisitata.VisitedOn - DateTime.Now.AddMinutes(-InActiveCustomerInHrs.InActiveCustomerInHrs)).Minutes);
                                    if (appVisitata.RemainingTimeinHrs > InActiveCustomerInHrs.InActiveCustomerInHrs || appVisitata.RemainingTimeinHrs < 0)//168
                                    {
                                        var people = myContext.Peoples.Where(x => x.PeopleID == data.PeopleID).FirstOrDefault();
                                        var inactivePeopleDetail = myContext.InactivePeopleDB.Where(x => x.PeopleID == data.PeopleID && !x.Active && !x.Deleted).OrderByDescending(x => x.Id).FirstOrDefault();
                                        if (inactivePeopleDetail == null && (Convert.ToDateTime(appVisitata.VisitedOn) < Convert.ToDateTime(DateTime.Now.AddMinutes(-InActiveCustomerInHrs.InActiveCustomerInHrs))))
                                        {
                                            inactivePeopleData.Active = false;
                                            inactivePeopleData.PeopleFirstName = data.PeopleFirstName;
                                            inactivePeopleData.PeopleLastName = data.PeopleLastName;
                                            inactivePeopleData.UserName = data.UserName;
                                            inactivePeopleData.PeopleID = data.PeopleID;
                                            inactivePeopleData.Mobile = data.Mobile;
                                            inactivePeopleData.Empcode = data.Empcode;
                                            inactivePeopleData.Department = data.Department;
                                            inactivePeopleData.Desgination = data.Desgination;
                                            inactivePeopleData.DisplayName = data.DisplayName;
                                            inactivePeopleData.city = data.city;
                                            inactivePeopleData.CreatedBy = GetLoginUserId();
                                            inactivePeopleData.CreationDate = DateTime.Now;
                                            inactivePeopleData.LastVisitedDate = appVisitata.VisitedOn;
                                            myContext.InactivePeopleDB.Add(inactivePeopleData);

                                            people.Active = false;
                                            people.UpdatedDate = DateTime.Now;
                                            myContext.Entry(people).State = EntityState.Modified;
                                            myContext.Commit();
                                        }

                                    }
                                }
                        }
                    }
                }

            }
            return res;

        }


        [HttpGet]
        [Route("PeopleInActiveList")]
        public async Task<List<PeopleInActiveDC>> PeopleInActiveList(string key)
        {
            List<PeopleInActiveDC> peopledata = new List<PeopleInActiveDC>();
            using (var myContext = new AuthContext())
            {
                var keyParam = new SqlParameter("@keyWord", key);
                peopledata = await myContext.Database.SqlQuery<PeopleInActiveDC>("GetInActivePeopleData @keyWord", keyParam).ToListAsync();
            }
            return peopledata;

        }



        [HttpGet]
        [Route("UpdateInActivePeopleId")]
        public PeopleInActiveExecutiveDC UpdateInActivePeopleId(int PeopleId, string Comment, bool Status)
        {
            bool res = false;
            PeopleInActiveExecutiveDC peopleRes = new PeopleInActiveExecutiveDC();
            using (var myContext = new AuthContext())
            {

                var inActivePeopledata = myContext.InactivePeopleDB.Where(x => x.PeopleID == PeopleId).FirstOrDefault();
                string queryForRole = "select distinct r.Name as Role from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where PeopleID='" + PeopleId + "'and ur.isActive=1 and p.Active=0 and p.Deleted=0";
                var roleList = myContext.Database.SqlQuery<string>(queryForRole).ToList();
                var IsRole = roleList.Any(x => x.Contains("Sales Executive"));
                if (IsRole)
                {
                    //string query = "select distinct r.Name as Role from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where PeopleID='" + PeopleId + "'and ur.isActive=1 and r.Name = 'Sourcing Executive' or r.Name = 'Sourcing Senior Executive' and p.Active=0 and p.Deleted=0";
                    //var role = myContext.Database.SqlQuery<string>(query).FirstOrDefault();
                    peopleRes.Role = "Sales Executive";
                }

                inActivePeopledata.Active = Status;
                inActivePeopledata.Comment = Comment;
                inActivePeopledata.UpdateBy = GetLoginUserId();
                inActivePeopledata.UpdatedDate = DateTime.Now;
                myContext.Entry(inActivePeopledata).State = EntityState.Modified;

                var peopleData = myContext.Peoples.Where(d => d.PeopleID == PeopleId).FirstOrDefault();
                var UpdatedByName = myContext.Peoples.Where(p => p.PeopleID == inActivePeopledata.UpdateBy).Select(x => x.DisplayName).FirstOrDefault();
                peopleData.Active = true;
                peopleData.UpdateBy = UpdatedByName;
                peopleData.UpdatedDate = DateTime.Now;
                myContext.Entry(peopleData).State = EntityState.Modified;

                //var clusterExecutive = myContext.ClusterStoreExecutives.Where(x => x.ExecutiveId == PeopleId && x.IsActive).OrderByDescending(x=>x.Id).FirstOrDefault();
                //clusterExecutive.IsActive = true;
                //clusterExecutive.ModifiedBy = inActivePeopledata.UpdateBy;
                //clusterExecutive.ModifiedDate = DateTime.Now;
                //myContext.Entry(clusterExecutive).State = EntityState.Modified;

                myContext.Commit();
                res = true;
                peopleRes.Status = res;

            }
            return peopleRes;

        }
        public void UserLogoutNotification(List<string> FCMIds)
        {

            if (FCMIds.Any())
            {
                string Key = ConfigurationManager.AppSettings["SalesFcmApiKey"];
                //var objNotificationList = FCMIds.Distinct().Select(x => new
                //{
                //    to = x,
                //    PeopleId = 0,
                //    data = new
                //    {
                //        title = "",
                //        body = "",
                //        icon = "",
                //        typeId = "",
                //        notificationCategory = "",
                //        notificationType = "",
                //        notificationId = "",
                //        notify_type = "logout",
                //        url = "",
                //    }
                //}).ToList();
                var data = new FCMData
                {
                    title = "",
                    body = "",
                    icon = "",
                    notificationCategory = "",
                    notificationType = "",
                    notify_type = "logout",
                    url = "",
                };
                ParallelLoopResult parellelResult = Parallel.ForEach(FCMIds.Distinct(), (x) =>
                {
                    try
                    {
                        var firebaseService = new FirebaseNotificationServiceHelper(Key);
                        //var fcmid = "fZGIeP5dTKGd-2JBZttcDj:APA91bGINtqXqKuCcEHd4qXZMN-VtX5KC2g98KkytmGdpc28_-duDu8Ry1P6Kk_Xb9RgRG0iDWrp8DkwE1EPCOPG0OLz2uRjo-HBg-Ysg-mDaMErlYLjJGZE7ScXKIkjmWZ0xNO6UyxN";
                        var result = firebaseService.SendNotificationForApprovalAsync(x, data);
                        if (result != null)
                        {
                            //AutoNotification.IsSent = true;
                        }
                        else
                        {
                            //AutoNotification.IsSent = false;
                        }
                        //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                        //tRequest.Method = "post";
                        //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(x);
                        //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                        //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                        //tRequest.ContentLength = byteArray.Length;
                        //tRequest.ContentType = "application/json";
                        //using (Stream dataStream = tRequest.GetRequestStream())
                        //{
                        //    dataStream.Write(byteArray, 0, byteArray.Length);
                        //    using (WebResponse tResponse = tRequest.GetResponse())
                        //    {
                        //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        //        {
                        //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                        //            {
                        //                String responseFromFirebaseServer = tReader.ReadToEnd();
                        //                AngularJSAuthentication.API.Controllers.NotificationController.FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<AngularJSAuthentication.API.Controllers.NotificationController.FCMResponse>(responseFromFirebaseServer);
                        //                if (response.success == 1)
                        //                {
                        //                    //totalSent.Add(1);
                        //                }
                        //                else if (response.failure == 1)
                        //                {
                        //                    //totalNotSent.Add(1);
                        //                }
                        //            }
                        //        }
                        //    }
                        //}
                    }
                    catch (Exception asd)
                    {
                    }
                });

            }

        }
    }
}

public class SalesDTO
{
    public People P { get; set; }
    public bool Status { get; set; }
    public string Message { get; set; }


}


public class PeopleInActiveExecutiveDC
{
    public string Role { get; set; }
    public bool Status { get; set; }
}


public class cid
{
    public int id { get; set; }
}

public class clumap
{
    public string Mobile { get; set; }
    public List<cid> ids { get; set; }
}
public class DeliveryPeople
{
    public string Role { get; set; }
    public int PeopleID { get; set; }
    public string DisplayName { get; set; }
    public string PeopleFirstName { get; set; }
    public string PeopleLastName { get; set; }

}

public class PeopleAll
{
    public int PeopleID { get; set; }
    public int CompanyId { get; set; }
    public int WarehouseId { get; set; }
    public string PeopleFirstName { get; set; }
    public string PeopleLastName { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public string Country { get; set; }
    public int? Stateid { get; set; }
    public string state { get; set; }
    public int? Cityid { get; set; }
    public string city { get; set; }
    public string Mobile { get; set; }
    public string Password { get; set; }
    public int? RoleId { get; set; }
    public string Department { get; set; }
    public double BillableRate { get; set; }
    public string CostRate { get; set; }
    public string Permissions { get; set; }
    public string SUPPLIERCODES { get; set; }
    public string Type { get; set; }
    public string ImageUrl { get; set; }
    public bool Deleted { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool Approved { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public string CreatedBy { get; set; }
    public string UpdateBy { get; set; }
    public int VehicleId { get; set; }
    public string VehicleName { get; set; }
    public string VehicleNumber { get; set; }
    public double VehicleCapacity { get; set; }
    public string Skcode { get; set; }
    public string AgentCode { get; set; }
    public string Salesexecutivetype { get; set; }
    public decimal AgentAmount { get; set; }
    public string Empcode { get; set; }
    public string Desgination { get; set; }
    public string Status { get; set; }
    public DateTime? DOB { get; set; }
    public DateTime? DataOfJoin { get; set; }
    public DateTime? DataOfMarriage { get; set; }
    public DateTime? EndDate { get; set; }
    public string Unit { get; set; }

    public string Reporting { get; set; }
    public string IfscCode { get; set; }
    public int Account_Number { get; set; }
    public string UserName { get; set; }
    // Salary
    public double? Salary { get; set; }
    public double? B_Salary { get; set; }
    public double? Hra_Salary { get; set; }
    public double? CA_Salary { get; set; }
    public double? DA_Salary { get; set; }
    public double? Lta_Salary { get; set; }
    public double? PF_Salary { get; set; }
    public double? ESI_Salary { get; set; }
    public double? M_Incentive { get; set; }
    public double? Y_Incentive { get; set; }
    // Document
    public string Id_Proof { get; set; }
    public string Address_Proof { get; set; }
    public string PanCard_Proof { get; set; }
    public string MarkSheet { get; set; }
    public string Pre_SalarySlip { get; set; }
    public double DepositAmount { get; set; }
    public bool tempdel { get; set; }
    public List<warehouse> Warehouses { get; set; }
    public int? ReportPersonId { get; set; }
    public List<warehouse> OldWarehouses { get; set; }
    public Boolean IsPasswordChange { get; set; }
}

public class warehouse
{
    public int id { get; set; }
}

/// <summary>
/// DTO class Crated by 15/01/2019
/// </summary>
public class Peopleresponse
{
    public People people { get; set; }
    public string message { get; set; }
    public bool Status { get; set; }
}
public class PeopleLoginDCs
{
    public PeopleLoginDC people { get; set; }
    public string message { get; set; }
    public bool Status { get; set; }
}


public class PeopleAutoCompleteVM
{
    public int PeopleID { get; set; }
    public string DisplayName { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Mobile { get; set; }
}

public class ResPeopleAll
{
    public List<PeopleAll> person { get; set; }
    public int TotalCount { get; set; }
}