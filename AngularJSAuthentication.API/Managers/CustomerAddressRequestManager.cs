using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.MapApi;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Transaction.Customer;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.CustomerDelight;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;

namespace AngularJSAuthentication.API.Managers
{
    public class CustomerAddressRequestManager
    {
        public List<CustomerAddressRequestDc> GetList(int warehouseId, int skip, int take)
        {
            string spName = "AddressNotVerifiedCustomerGet @WarehouseId, @Skip, @Take";
            using (var context = new AuthContext())
            {
                var warehouseParam = new SqlParameter
                {
                    ParameterName = "WarehouseId",
                    Value = warehouseId
                };

                var skipParam = new SqlParameter
                {
                    ParameterName = "Skip",
                    Value = skip
                };

                var takeParam = new SqlParameter
                {
                    ParameterName = "Take",
                    Value = take
                };

                var list = context.Database.SqlQuery<CustomerAddressRequestDc>(spName, warehouseParam, skipParam, takeParam).ToList();
                return list;
            }
        }

        public List<CustomerAddressOperationRequestVM> GetLists(int warehouseId, int skip, int take)
        {
            //string spName = "CustomerAddressOperationRequestGet '" + warehouseId + "','"+skip+"',
            using (var context = new AuthContext())
            {

                var param = new SqlParameter("@WarehouseId", warehouseId);
                var param1 = new SqlParameter("@Skip", skip);
                var param2 = new SqlParameter("@Take", take);
                var param3 = new SqlParameter("@IsGetCount", false);
                var List = context.Database.SqlQuery<CustomerAddressOperationRequestVM>("Exec CustomerAddressOperationRequestGet @WarehouseId, @IsGetCount, @Skip, @Take", param, param3, param1, param2).ToList();
                return List;
            }
        }

        public int GetCount(int warehouseId)
        {
            //string spName = "CustomerAddressOperationRequestGet '" + warehouseId + "','"+skip+"',
            using (var context = new AuthContext())
            {
                var warehouseParam = new SqlParameter
                {
                    ParameterName = "WarehouseId",
                    Value = warehouseId
                };

                var isGetCountParam = new SqlParameter
                {
                    ParameterName = "IsGetCount",
                    Value = true
                };



                int count = context.Database.SqlQuery<int>("Exec CustomerAddressOperationRequestGet @WarehouseId, @IsGetCount", warehouseParam, isGetCountParam).FirstOrDefault();
                return count;
            }
        }

        public bool MakeRequest(CustomerAddressRequestDc request)
        {
            if (request != null && request.NewLat.HasValue && request.NewLng.HasValue && request.PeopleId.HasValue)
            {
                CustomerAddressOperationRequest newRequest = new CustomerAddressOperationRequest
                {
                    CreatedBy = request.PeopleId.Value,
                    ImageUrl = request.ImagePath,
                    CreatedDate = DateTime.Now,
                    CustomerId = request.CustomerId,
                    IsActive = true,
                    IsApproved = false,
                    IsDeleted = false,
                    IsRejected = false,
                    Lat = request.NewLat.Value,
                    Lng = request.NewLng.Value,
                    ModifiedBy = null,
                    ModifiedDate = null,
                    ProcessedBy = null,
                    ProcessedDate = null
                };
                using (var context = new AuthContext())
                {
                    var dbRequest = context.CustomerAddressOperationRequestDb.FirstOrDefault(x => x.CustomerId == request.CustomerId && x.IsActive == true && x.IsDeleted == false && x.IsApproved == false && x.IsRejected == false);
                    if (dbRequest == null)
                    {
                        context.CustomerAddressOperationRequestDb.Add(newRequest);
                    }
                    else
                    {
                        dbRequest.Lat = request.NewLat.Value;
                        dbRequest.Lng = request.NewLng.Value;
                        dbRequest.ModifiedBy = request.PeopleId.Value;
                        dbRequest.ModifiedDate = DateTime.Now;

                    }
                    context.Commit();
                    return true;


                }
            }
            else
            {
                return false;
            }

        }


        public bool Approve(CustomerAddressOperationRequestVM obj, int userId)
        {
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (AuthContext context = new AuthContext())
                {
                    CustomerAddressOperationRequestVM VM = new CustomerAddressOperationRequestVM();
                    {
                        bool result = false;
                        //  RequestId = userId();

                        var customeradd = context.CustomerAddressOperationRequestDb.FirstOrDefault(x => x.Id == obj.RequestId);
                        if (customeradd != null)
                        {
                            customeradd.IsApproved = true;
                            customeradd.ModifiedBy = userId;
                            customeradd.ModifiedDate = DateTime.Now;
                            context.Entry(customeradd).State = EntityState.Modified;
                        }
                        var customer = context.Customers.FirstOrDefault(x => x.CustomerId == obj.CustomerId);
                        customer.Addresslat = obj.NewLat;
                        customer.Addresslg = obj.NewLng;
                        customer.IsAddressVerified = true;
                        //customer.LastModifiedBy = 
                        customer.UpdatedDate = DateTime.Now;
                        context.Entry(customer).State = EntityState.Modified;
                        if (context.Commit() > 0)
                        {
                            result = true;
                            scope.Complete();
                        }
                        return result;
                    }
                }
            }
        }

        public bool Reject(CustomerAddressOperationRequestVM obj, int userId)
        {
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (AuthContext context = new AuthContext())
                {
                    CustomerAddressOperationRequestVM VM = new CustomerAddressOperationRequestVM();
                    {
                        bool result = false;
                        var customeradd = context.CustomerAddressOperationRequestDb.FirstOrDefault(x => x.Id == obj.RequestId);
                        if (customeradd != null)
                        {
                            customeradd.IsRejected = true;
                            customeradd.ModifiedBy = userId;
                            customeradd.ModifiedDate = DateTime.Now;
                            context.Entry(customeradd).State = EntityState.Modified;
                        }

                        if (context.Commit() > 0)
                        {
                            result = true;
                            scope.Complete();
                        }
                        return result;
                    }
                }
            }
        }

        public bool AddGSTCityAndState(string cityname, string pincode, string stateName, string stateCode, AuthContext context)
        {
            //State state = null;
            //if (!string.IsNullOrEmpty(stateName))
            //{
            //    state = context.States.FirstOrDefault(x => x.StateName.ToLower().Trim() == stateName.ToLower().Trim());
            //    if (state == null)
            //    {
            //        state = new State
            //        {
            //            active = true,
            //            AliasName = stateName,
            //            ClearTaxStateCode = stateCode,
            //            CompanyId = 1,
            //            CreatedBy = "System",
            //            CreatedDate = DateTime.Now,
            //            Deleted = false,
            //            GSTNo = 0,
            //            IsSupplier = false,
            //            StateManagerId = 0,
            //            StateManagerName = "",
            //            StateName = stateName,
            //            UpdatedDate = DateTime.Now
            //        };
            //        context.States.Add(state);
            //        context.Commit();
            //    }
            //}


            //if (!string.IsNullOrEmpty(cityname) && state != null && !context.Cities.Any(x => x.CityName.ToLower().Trim() == cityname.ToLower().Trim()))
            ////{
            ////    GooglePlaceApiHelper helper = new GooglePlaceApiHelper();
            ////    GeocodeApiResult address = null;
            ////    var citygeoCode= AsyncContext.Run(() => helper.GetCityList(cityname + ", " + stateName));
            ////    if(citygeoCode !=null && citygeoCode.Any())
            ////    {
            ////        address = AsyncContext.Run(() => helper.GetAddressByPlaceId(citygeoCode.First().place_id));
            ////    }
            ////    City city = new City
            ////    {
            ////        active = true,
            ////        CityName = cityname,
            ////        aliasName = cityname,
            ////        CompanyId = 1,
            ////        CreatedBy = "System",
            ////        CreatedDate = DateTime.Now,
            ////        Deleted = false,
            ////        IsSupplier = false,
            ////        UpdateBy = "System",
            ////        UpdatedDate = DateTime.Now,
            ////        Stateid = state.Stateid,
            ////        StateName = state.StateName,
            ////        Code= pincode                    
            ////    };
            //    if (address != null && address.results != null && address.results.Any())
            //    {
            //        city.CityPlaceId = citygeoCode.First().place_id;
            //        city.CityLatitude = address.results.First().geometry.location.lat;
            //        city.CityLongitude = address.results.First().geometry.location.lng;
            //    }

            //    context.Cities.Add(city);
            //    context.Commit();
            //}
            return true;
        }
    }
}