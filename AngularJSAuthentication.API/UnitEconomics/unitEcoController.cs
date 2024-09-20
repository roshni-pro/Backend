using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Expense;
using MongoDB.Bson;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;


namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/uniteconomic")]
    public class unitEcoController : ApiController
    {
       
        [Route("")]
        [HttpGet]
        public PagerList Get(int totalitem,int page,int? WarehouseId,string LabelName,DateTime? dateto,DateTime? datefrom)
        {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                using (var context = new AuthContext())
            {
                try
                {
                    PagerList pagerList = new PagerList();
                    Warehouse_id = WarehouseId ?? 0;
                    if (Warehouse_id > 0)
                    {

                        var query = from unit in context.UnitEconomicDb
                                    join ware in context.Warehouses
                                    on unit.WarehouseId equals ware.WarehouseId
                                    where (string.IsNullOrEmpty(LabelName)
                                    || (!string.IsNullOrEmpty(LabelName) && unit.CompanyLabel.ToLower().Contains(LabelName.ToLower()))) &&
                                    unit.IsActive == true && unit.Deleted == false && (datefrom.HasValue  || (unit.ExpenseDate >= datefrom && unit.ExpenseDate < dateto))
                                    && unit.WarehouseId== Warehouse_id
                                    select new UnitEconomicDc
                                    {
                                        CompanyLabel = unit.CompanyLabel,
                                        Label1 = unit.Label1,
                                        Label2 = unit.Label2,
                                        Label3 = unit.Label3,
                                        WarehouseName = ware.WarehouseName,
                                        Amount = unit.Amount,
                                        Discription = unit.Discription,
                                        ExpenseDate = unit.ExpenseDate
                                       
                                    };
                        pagerList.Count = query.Count();
                        pagerList.unitEconomics = query.OrderByDescending(x => x.ExpenseDate).Skip(page).Take(totalitem).ToList();


                        return pagerList;
                    }
                    else
                    {
                        var query = "Select unit.CompanyLabel,unit.Label1,unit.Label2,unit.Label3,ware.WarehouseName,unit.Amount,unit.Discription,unit.ExpenseDate  from UnitEconomics unit inner join Warehouses ware on  unit.WarehouseId=ware.WarehouseId where unit.Deleted=0 and unit.IsActive=1";
                        pagerList.unitEconomics = context.Database.SqlQuery<UnitEconomicDc>(query).OrderByDescending(x => x.ExpenseDate).Skip(page).Take(totalitem).ToList();
                        pagerList.Count = query.Count();

                        return pagerList;
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
                }
            
        }

        [Route("")]
        [HttpPost]
        public UnitEconomic add(UnitEconomic unitEconomic)
        {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                int Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                unitEconomic.CompanyId = compid;
            using (var context = new AuthContext())
            {
                if (unitEconomic.WarehouseId > 0)
                {
                    var unEco = context.UnitEconomicDb.Where(x => x.unitId == unitEconomic.unitId).FirstOrDefault();
                    if (unEco != null)
                    {
                        if (unitEconomic.Label1 != null)
                            unEco.Label1 = unitEconomic.Label1;
                        if (unitEconomic.Label2 != null)
                            unEco.Label2 = unitEconomic.Label2;
                        if (unitEconomic.Label3 != null)
                            unEco.Label3 = unitEconomic.Label3;
                        unEco.Amount = unitEconomic.Amount;
                        unEco.Discription = unitEconomic.Discription;
                        unEco.ModifyDate = DateTime.Now;
                        unEco.UpdatedBy = userid;
                        unEco.CompanyLabel = unitEconomic.CompanyLabel;
                        unEco.WarehouseId = unitEconomic.WarehouseId;
                        context.Entry(unEco).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        unitEconomic.CreatedDate = DateTime.Now;
                        unitEconomic.IsActive = true;
                        unitEconomic.CreatedBy = userid;
                        unitEconomic.WarehouseId = unitEconomic.WarehouseId;
                        context.UnitEconomicDb.Add(unitEconomic);
                    }
                    context.Commit();

                    
                }
                return unitEconomic;

            }
        }
   
        #region Add Label  in  Mongo Table
        /// <summary>
        /// Created Date:10/02/2020
        /// Created By Raj
        /// </summary>
        /// <param name="labelType"></param>
        /// <returns></returns>
        [Route("AddLabel")]
        [HttpPost]
       
        public ExpenseLabel AddLabel(ExpenseLabel label)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var context = new AuthContext())
            {
                MongoDbHelper<ExpenseLabel> mongoDbHelper = new MongoDbHelper<ExpenseLabel>();
                ExpenseLabel expenseLabel = mongoDbHelper.Select(x=>x.Id== label.Id).FirstOrDefault();
                if (expenseLabel == null)
                {
                    expenseLabel = new ExpenseLabel
                    {
                        CreatedBy = userid,
                        LabelName = label.LabelName,
                        LabelType=label.LabelType,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false

                    };
                    mongoDbHelper.Insert(expenseLabel);
                }
                else {
                    expenseLabel.LabelName = label.LabelName;
                    expenseLabel.LabelType = label.LabelType;
                    expenseLabel.ModifiedBy = userid;
                    expenseLabel.ModifiedDate= DateTime.Now;
                    mongoDbHelper.ReplaceWithoutFind(expenseLabel.Id, expenseLabel);
                }
                return expenseLabel;
            }


        }
        #endregion

        #region Get Label  entry 
        /// <summary>
        /// Created Date:10/02/2020
        /// Created By Raj
        /// </summary>
        /// <param name="labelType"></param>
        /// <returns></returns>
        [Route("GetLabel")]
        [HttpGet]
        public LabelListDTO GetlabelList()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var context = new AuthContext())
            {
                LabelListDTO labelListDTO = new LabelListDTO(); 
                MongoDbHelper<ExpenseLabel> mongoDbHelper = new MongoDbHelper<ExpenseLabel>();
                //get label1 entry
                labelListDTO.label1 = mongoDbHelper.Select(x => x.LabelType ==1).Select(x=>new LabelDTO{ Id=x.Id ,LabelName=x.LabelName}).ToList();
                //end
                //get label2 entry
                labelListDTO.label2 = mongoDbHelper.Select(x => x.LabelType == 2).Select(x => new LabelDTO { Id = x.Id, LabelName = x.LabelName }).ToList();
                //end
                //get label1 entry
                labelListDTO.label3 = mongoDbHelper.Select(x => x.LabelType == 3).Select(x => new LabelDTO { Id = x.Id, LabelName = x.LabelName }).ToList();
                //end
                //get Company label entry
                labelListDTO.companyLabel = mongoDbHelper.Select(x => x.LabelType == 4).Select(x => new LabelDTO { Id = x.Id, LabelName = x.LabelName }).ToList();
                //end

                return labelListDTO;
            }


        }
        #endregion
        #region Get for Excel Data   
        /// <summary>
        /// Created Date:12/02/2020
        /// Created By Raj
        /// </summary>
        /// <returns></returns>
        [Route("GetExcel")]
        [HttpGet]
        public List<UnitEconomic> GetExcelList()
        {
            List<UnitEconomic> unitEconomics = new List<UnitEconomic>();
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            int Warehouse_id = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

            using (var context = new AuthContext())
            {
                if (Warehouse_id > 0)
                {
                    unitEconomics = context.UnitEconomicDb.Where(x => x.Deleted == false && x.IsActive == true && x.WarehouseId == Warehouse_id).ToList();
                }
                else {
                    unitEconomics = context.UnitEconomicDb.Where(x => x.Deleted == false && x.IsActive == true ).ToList();
                }
            
                return unitEconomics;
            }


        }
        #endregion
        #region Get Company Label 
        /// <summary>
        /// Created Date:17/02/2020
        /// Created By Raj
        /// </summary>
        /// <returns></returns>
        [Route("GetComapnyLabel")]
        [HttpGet]
        public List<LabelDTO> GetComapnyLabel()
        {

            using (var context = new AuthContext())
            {
                List<LabelDTO> labelListDTO = new List<LabelDTO>();
                MongoDbHelper<ExpenseLabel> mongoDbHelper = new MongoDbHelper<ExpenseLabel>();

                //get Company label entry
                labelListDTO = mongoDbHelper.Select(x => x.LabelType == 4).Select(x => new LabelDTO { Id = x.Id, LabelName = x.LabelName }).ToList();
                //end

                return labelListDTO;
            }


        }
        #endregion
    }


    public class LabelDTO
    {
        public ObjectId Id { get; set; }
        public string LabelName { get; set; }
    }
    public class LabelListDTO {
        public List<LabelDTO>  label1 { get; set; }
        public List<LabelDTO> label2 { get; set; }
        public List<LabelDTO> label3 { get; set; }
        public List<LabelDTO> companyLabel { get; set; }
    }
    public class PagerList {
      public List<UnitEconomicDc> unitEconomics { get; set; }
      public int Count { get; set; }

    }
}



