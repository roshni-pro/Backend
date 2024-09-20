using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static AngularJSAuthentication.API.Controllers.WarehouseController;

namespace AngularJSAuthentication.API.Helper.BackendOrderProcess
{
    public class BackendOrderProcessHelper
    {
        public BackendOrderResponse CreateDboy_SalesExecutive_Cluster(int warehouseid, string WarehouseName,double latitude, double longitude, int Cityid, string CityName, int userid, string username)
        {
            using (var context = new AuthContext())
            {
                AccountController accountController = new AccountController();
                BackendOrderResponse result = new BackendOrderResponse();
                string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                string sRandomOTP = GenerateRandomOTP(5, saAllowedCharacters);
                string newname = WarehouseName.Replace(@"-", string.Empty);
                if (warehouseid > 0 && WarehouseName != null && Cityid > 0 && userid > 0)
                {
                    //CreateDboyBackendOrderDc Dboy = new CreateDboyBackendOrderDc()
                    //{
                    //    Warehouseid = warehouseid,
                    //    CityId = Cityid,
                    //    Name = newname.Trim() + "DB",
                    //    userid = userid.ToString(),
                    //    UserName = username,
                    //    MobileNo = "99999" + sRandomOTP,
                    //};

                    //if (Dboy != null)
                    //{
                     //  var res = accountController.Create_Dboy_ForBackendOrder(Dboy, context);
                    //}

                    string SalesRandomOTP = GenerateRandomOTP(5, saAllowedCharacters);
                    CreateDboyBackendOrderDc Sales = new CreateDboyBackendOrderDc()
                    {
                        Warehouseid = warehouseid,
                        CityId = Cityid,
                        Name = newname.Trim() + "Sales",
                        userid = userid.ToString(),
                        UserName = username,
                        MobileNo = "99999" + SalesRandomOTP,
                    };
                    if (Sales != null)
                    {
                        var res2 = accountController.Create_Executive_ForBackendOrder(Sales, context);
                    }

                    Cluster cluster = new Cluster();
                    cluster.CompanyId = 1;
                    cluster.ClusterName = WarehouseName+" - " + "store-1";
                    cluster.WarehouseName = WarehouseName;
                    cluster.WarehouseId = warehouseid;
                    cluster.CreatedDate = DateTime.Now;
                    cluster.UpdatedDate = DateTime.Now; 
                    cluster.Address = null;
                    cluster.Phone = "";
                    cluster.Active = true;
                    cluster.Deleted =false;
                    cluster.CityId = Cityid;
                    cluster.CityName = CityName;
                    cluster.DefaultLatitude = latitude.ToString();
                    cluster.DefaultLongitude = longitude.ToString();
                    cluster.AgentCode = 0;
                    cluster.WorkingCityName = CityName;
                    cluster.IsAutoOrderEnable = false;
                    cluster.IsSelleravailable = false;
                    context.Clusters.Add(cluster);
                    context.Commit();   

                }
                return result;
            }
        }
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
    }
}