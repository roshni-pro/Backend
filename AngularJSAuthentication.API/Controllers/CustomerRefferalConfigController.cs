using AngularJSAuthentication.Model.CustomerReferral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/CustomerRefferalConfig")]
    public class CustomerRefferalConfigController : ApiController
    {
        [Route("GetCustReffConfigList")]
        [HttpGet]
        public List<CustomerRefferalConfigurationDC> GetCustRefConfig(int ReferralType, int cityID)
        {
            List<CustomerRefferalConfigurationDC> List = new List<CustomerRefferalConfigurationDC>();
            using (AuthContext context = new AuthContext())
            {
                List = context.GetAllCustReffConfigList(context, ReferralType, cityID);

            }
            return List;
        }


        [Route("CheckCustReffConfig")]
        [HttpPost]
        public string SubmitCustRefConfigData(CustomerRefferalConfigurationDC Obj)
        {
            using (AuthContext context = new AuthContext())
            {
                var Check = context.CustomerReferralConfigurationDb.Where(x => x.CityId == Obj.CityId && x.ReferralType == Obj.ReferralType && x.OnOrder == Obj.OnOrder && x.IsActive==true && x.IsDeleted==false).FirstOrDefault();
                if (Check != null)
                {
                    return "Alredy Exists!!";
                }
                else
                {

                    return "Checked Successfully!!";
                }
            }
        }


        [Route("RemoveCustReffConfig")]
        [HttpDelete]
        public int DeleteCustRefConfig(int deleteID)
        {
            //List<DONPinCodeDc> List = new List<DONPinCodeDc>();
            using (AuthContext context = new AuthContext())
            {
                var tt = context.deleteCustRefConfig(deleteID);
                //return List;
            return tt;
            }
        }


        //[Route("SetCustReffConfigActiveStatus")]
        //[HttpGet]
        //public string setCustReffConfigActiveStatus(Boolean IsActive, int id)
        //{
        //    //List<CustomerRefferalConfigurationDC> List = new List<CustomerRefferalConfigurationDC>();
        //    using (AuthContext context = new AuthContext())
        //    {
        //        string res = context.SetCustReffConfigActiveStatus(context, IsActive, id);

        //        return res;
        //    }
        //}

        [Route("SetCustReffConfigActiveStatus")]
        [HttpPost]
        public string setCustReffConfigActiveStatus(CustomerRefferalConfigurationDC Obj)
        {
            //List<CustomerRefferalConfigurationDC> List = new List<CustomerRefferalConfigurationDC>();
            using (AuthContext context = new AuthContext())
            {


                //var Check = context.CustomerReferralConfigurationDb.Where(x => x.CityId == Obj.CityId && x.ReferralType == Obj.ReferralType && x.OnOrder == Obj.OnOrder && x.IsActive == Obj.IsActive && x.IsDeleted == false).FirstOrDefault();
                //if(Check != null)
                //{
                //    return "Conflict Detected";
                //}
                //else
                //{
                //    string res = context.SetCustReffConfigActiveStatus(context, Obj.IsActive, Obj.id);
                //           return res;
                //}


                if (Obj.IsActive == false)
                {
                    string res = context.SetCustReffConfigActiveStatus(context, Obj.IsActive, Obj.id);
                    return res;
                }
                else {
                    var Check = context.CustomerReferralConfigurationDb.Where(x => x.CityId == Obj.CityId && x.ReferralType == Obj.ReferralType && x.OnOrder == Obj.OnOrder &&  x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if(Check != null)
                    {
                           return "Conflict Detected";
                    }
                    else
                    {
                        string res = context.SetCustReffConfigActiveStatus(context, Obj.IsActive, Obj.id);
                        return res;

                    }
                }
            }
        }


        [Route("GetCustRefStatus")]
        [HttpGet]
        public List<CustomerRefStatusDC> GetCustRefStatus()
        {
            List<CustomerRefStatusDC> List = new List<CustomerRefStatusDC>();
            using (AuthContext context = new AuthContext())
            {
                List = context.GetCustRefStatus(context);

            }
            return List;
        }

        [Route("SubmitBulkCustReferralData")]
        [HttpPost]
        public string SubmitCustRefConfigBulkData(List<CustomerRefferalConfigurationDC> customerRefferalConfigurationlist)
        {
            int UserId = 0;
            UserId = GetUserId();
            //List<DONPinCodeDc> List = new List<DONPinCodeDc>();
            using (AuthContext context = new AuthContext())
            {
                List<CustomerReferralConfiguration> addList = new List<CustomerReferralConfiguration>();
               
                foreach (var element in customerRefferalConfigurationlist)
                {
                    CustomerReferralConfiguration obj = new CustomerReferralConfiguration();
                    obj.CityId = element.CityId;
                    obj.OnOrder = element.OnOrder;
                    obj.OnDeliverd = element.OnDeliverd;
                    obj.IsActive = true;
                    obj.IsDeleted = false;
                    obj.CreatedBy = UserId;
                    obj.CreatedDate = DateTime.Now;
                    obj.ReferralType = element.ReferralType;
                    obj.ReferralWalletPoint = element.ReferralWalletPoint;
                    obj.CustomerWalletPoint = element.CustomerWalletPoint;
                    addList.Add(obj);                            
                }
                context.CustomerReferralConfigurationDb.AddRange(addList);
                context.Commit();
            }
            return "Success!!";
        }
        private int GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }
    }
}


public class CustomerRefferalConfigurationDC
{
    public int id { get; set; }
    public int CityId { get; set; }
    public string CityName { get; set; }

    public int OnOrder { get; set; }
    public int OnDeliverd { get; set; }
    public double ReferralWalletPoint { get; set; }
    public double CustomerWalletPoint { get; set; }
    public int ReferralType { get; set; }
    public string ErrorMsg { get; set; }

    public Boolean IsActive { get; set; }

}


//public class CustomerReferralConfigBulkDC <list>
//{
//    public int? id { get; set; }
//    public int CityId { get; set; }
//    public string CityName { get; set; }

//    public int OnOrder { get; set; }
//    public int OnDeliverd { get; set; }
//    public double ReferralWalletPoint { get; set; }
//    public double CustomerWalletPoint { get; set; }
//    public int ReferralType { get; set; }
//    public string ErrorMsg { get; set; }

//    public Boolean IsActive { get; set; }

//}


public class CustomerRefStatusDC
{
    public int? id { get; set; }

    public string OrderStatus { get; set; }

    public Boolean IsActive { get; set; }

    public Boolean isDeleted { get; set; }


}