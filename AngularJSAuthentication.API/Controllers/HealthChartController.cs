using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/HealthChart")]
    public class HealthChartController : ApiController
    {

        
        private static Logger logger = LogManager.GetCurrentClassLogger();


        [Route("GetChart")]
        [HttpPost]

        public dynamic GetChart(List<int> agentid)
        {
            List<ChartToShow> resultList = new List<ChartToShow>();

            //var agentid = agents.id;
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
                using (AuthContext db = new AuthContext())
                {
                    //var GetSecurityDetails = db.securityDetails.Where(x => x.Agent == agentid).Select(x => new { x.AmountDeposited, x.NeftChequeNumber, x.DateofTransfer, x.FromAccount, x.ToAccount, x.DateofRegistration, x.DateofJoining, x.BeatAlloted }).ToList();

                    //foreach (var i in agentid)
                    //{

                    var query = "SELECT Agent,DBoyName, ColumnName,  ColumnValue FROM  (SELECT v.Agent,p.DisplayName as DBoyName, isnull(v.vehiclenumber,'') vehiclenumber,  isnull(v.UploadRegistration,'') UploadRegistration, isnull(SalesTraining,'') SalesTraining, isnull(DeliveryTraining,'') DeliveryTraining, isnull(CommissionStructure,'') CommissionStructure, isnull(BehaviourTraining,'') BehaviourTraining, ISNULL(p.DisplayName,'') DisplayName,isnull(p.mobile,'')mobile,isnull(p.IdProof,'') IdProof,isnull(p.AddressProof,'') AddressProof,isnull(pVerificationCopy,'') pVerificationCopy, convert(nvarchar(max), isnull(s.AmountDeposited,'')) as AmountDeposited, ISNULL(s.NeftChequeNumber,'') as NeftChequeNumber,convert(nvarchar(max), isnull(s.DateofTransfer,'')) as DateofTransfer,ISNULL(s.FromAccount,'') as FromAccount,ISNULL(s.ToAccount,'') as ToAccount,   convert(nvarchar(max), isnull(s.DateofRegistration,'')) as DateofRegistration , convert(nvarchar(max), isnull(s.DateofJoining,'')) as DateofJoining, ISNULL(s.BeatAlloted,'') BeatAlloted  FROM Vehicles v inner join @agentids aa on v.agent = aa.IntValue left join TrainingDevelopments t on v.agent = t.Agent  left join People p on v.agent =p.PeopleID left join SecurityDetails s on v.agent =s.Agent where t.Active=1 and v.agent > 0 and v.isActive=1 and s.Active =1) p UNPIVOT (ColumnValue FOR ColumnName IN       (vehiclenumber, UploadRegistration, SalesTraining, DeliveryTraining, CommissionStructure, BehaviourTraining,DisplayName,mobile,IdProof,AddressProof,pVerificationCopy,AmountDeposited,NeftChequeNumber,DateofTransfer,FromAccount ,ToAccount,DateofRegistration,DateofJoining,BeatAlloted))AS unpvt  ";
                    var agentidDt = new DataTable();
                    agentidDt.Columns.Add("IntValue");
                    foreach (var item in agentid)
                    {
                        var dr = agentidDt.NewRow();
                        dr["IntValue"] = item;
                        agentidDt.Rows.Add(dr);
                    }

                    var param = new SqlParameter("@agentids", agentidDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    var chartData = db.Database.SqlQuery<ChartData>(query, param).ToList();

                    var columnNames = chartData.Select(x => new { x.ColumnName }).Distinct().ToList();

                    var groupedByAgent = chartData.GroupBy(x => new { x.Agent, x.dboyName });

                    foreach (var item in groupedByAgent)
                    {
                        var agent = new ChartToShow();
                        agent.dboyNames = item.Key.dboyName;
                        agent.Agent = item.Key.Agent;
                        var values = item.Where(x => x.ColumnValue != "").ToList();
                        var incvalues = item.Where(x => x.ColumnValue == "").Select(x => x.ColumnName).ToList();
                        var compValues = item.Where(x => x.ColumnValue != "").Select(x => x.ColumnName).ToList();
                        agent.IncompleteColums = incvalues;
                        agent.CompletColums = compValues;
                        double columncount = columnNames.Count();
                        double order = values.Count();
                        double difference = columncount - order;
                        agent.CompletePercent = (order / columncount) * 100.00;
                        agent.IncompletePercent = 100.00 - agent.CompletePercent;
                        resultList.Add(agent);
                    }



                    //}
                    return resultList;
                }

            }


            catch (Exception ex)
            {
                logger.Error("Error in Customer " + ex.ToString());
                logger.Info("End  Customer: ");
                return 0;
            }

        }






        [Route("IncompleteDocumentsList")]
        [HttpPost]

        public dynamic IncompleteDocumentsList(int agentid)
        {
            List<PendingData> resultP = new List<PendingData>();
            //var agentid = agents.id;
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
                using (AuthContext db = new AuthContext())
                {
                    //var GetSecurityDetails = db.securityDetails.Where(x => x.Agent == agentid).Select(x => new { x.AmountDeposited, x.NeftChequeNumber, x.DateofTransfer, x.FromAccount, x.ToAccount, x.DateofRegistration, x.DateofJoining, x.BeatAlloted }).ToList();

                    //foreach (var i in agentid)
                    //{

                    var query = "SELECT Agent,DBoyName, ColumnName,  ColumnValue FROM  (SELECT v.Agent,p.DisplayName as DBoyName, isnull(v.vehiclenumber,'') vehiclenumber,  isnull(v.UploadRegistration,'') UploadRegistration, isnull(SalesTraining,'') SalesTraining, isnull(DeliveryTraining,'') DeliveryTraining, isnull(CommissionStructure,'') CommissionStructure, isnull(BehaviourTraining,'') BehaviourTraining, ISNULL(p.DisplayName,'') DisplayName,isnull(p.mobile,'')mobile,isnull(p.IdProof,'') IdProof,isnull(p.AddressProof,'') AddressProof,isnull(pVerificationCopy,'') pVerificationCopy, convert(nvarchar(max), isnull(s.AmountDeposited,'')) as AmountDeposited, ISNULL(s.NeftChequeNumber,'') as NeftChequeNumber,convert(nvarchar(max), isnull(s.DateofTransfer,'')) as DateofTransfer,ISNULL(s.FromAccount,'') as FromAccount,ISNULL(s.ToAccount,'') as ToAccount,   convert(nvarchar(max), isnull(s.DateofRegistration,'')) as DateofRegistration , convert(nvarchar(max), isnull(s.DateofJoining,'')) as DateofJoining, ISNULL(s.BeatAlloted,'') BeatAlloted  FROM Vehicles v inner join @agentids aa on v.agent = aa.IntValue left join TrainingDevelopments t on v.agent = t.Agent  left join People p on v.agent =p.PeopleID left join SecurityDetails s on v.agent =s.Agent where t.Active=1 and v.agent > 0 and v.isActive=1 and s.Active =1) p UNPIVOT (ColumnValue FOR ColumnName IN       (vehiclenumber, UploadRegistration, SalesTraining, DeliveryTraining, CommissionStructure, BehaviourTraining,DisplayName,mobile,IdProof,AddressProof,pVerificationCopy,AmountDeposited,NeftChequeNumber,DateofTransfer,FromAccount ,ToAccount,DateofRegistration,DateofJoining,BeatAlloted))AS unpvt  ";
                    //var agentidDt = new DataTable();
                    //agentidDt.Columns.Add("IntValue");
                    //foreach (var item in agentid)
                    //{
                    //    var dr = agentidDt.NewRow();
                    //    dr["IntValue"] = item;
                    //    agentidDt.Rows.Add(dr);
                    //}

                    var param = new SqlParameter("@agentids", agentid);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    var chartData = db.Database.SqlQuery<ChartData>(query, param).ToList();

                    var columnNames = chartData.Select(x => new { x.ColumnName }).Distinct().ToList();

                    var groupedByAgent = chartData.GroupBy(x => new { x.Agent, x.dboyName });

                    foreach (var item in groupedByAgent)
                    {
                        var pendings = new PendingData();

                        var values = item.Where(x => x.ColumnValue == "").Select(x => x.ColumnName).ToList();
                        pendings.pending = values.ToString();
                        resultP.Add(pendings);
                    }



                    //}
                    return resultP;
                }

            }


            catch (Exception ex)
            {
                logger.Error("Error in Customer " + ex.ToString());
                logger.Info("End  Customer: ");
                return 0;
            }

        }


        [Route("GetPeoples")]
        [HttpGet]


        public dynamic GetPeoples()
        {


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
                using (AuthContext db = new AuthContext())
                {
                    var GetPeople = db.Peoples.Where(x => x.Active == true).Select(x => new { x.PeopleID, x.DisplayName }).ToList();


                    return GetPeople;

                }

            }


            catch (Exception ex)
            {
                logger.Error("Error in Customer " + ex.ToString());
                logger.Info("End  Customer: ");
                return 0;
            }

        }
















    }


    public class PostParams
    {
        public List<int> id { get; set; }
    }

    public class ChartData
    {
        public int Agent { get; set; }
        public string ColumnName { get; set; }
        public string ColumnValue { get; set; }
        public string dboyName { get; set; }
    }

    public class ChartToShow
    {
        public int Agent { get; set; }
        public List<string> IncompleteColums { get; set; }
        public List<string> CompletColums { get; set; }
        public double CompletePercent { get; set; }
        public double IncompletePercent { get; set; }
        public string dboyNames { get; set; }
    }


    public class PendingData
    {
        public string pending { get; set; }

    }

    public class PendingList
    {

        public string Pending { get; set; }
    }

    public class PrintIncData
    {
        public string FieldName { get; set; }
        public string Pending { get; set; }
        public string Complete { get; set; }

    }

}



