using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.Model.Expense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class VendorHelper
    {
        public VendorDC AddVendor(VendorDC vendorDC) {
            using (var context = new AuthContext())
            {
                Vendor vendor = new Vendor();
                vendor.Name = vendorDC.Name;
                vendor.Code = vendorDC.Code;
                vendor.AddressOne = vendorDC.AddressOne;
                vendor.AddressTwo = vendorDC.AddressTwo;
                vendor.StateId = vendorDC.StateId;
                vendor.DepartmentId = vendorDC.DepartmentId;
                vendor.WorkingCompanyId = vendorDC.WorkingCompanyId;
                vendor.WorkingLocationId = vendorDC.WorkingLocationId;
                vendor.IsTDSApplied = vendorDC.IsTDSApplied;
                vendor.ExpenseTDSMasterID = vendorDC.ExpenseTDSMasterID;
                vendor.VendorType = vendorDC.VendorType;
                vendor.CreatedBy = vendorDC.CreatedBy;
                vendor.CreatedDate = DateTime.Now;
                vendor.IsActive = true;
                vendor.IsDeleted = false;
                context.VendorDB.Add(vendor);
                context.Commit();
                vendorDC.Id = vendor.Id;
                // Start for create a ledger 
                LadgerHelper ladgerHelper = new LadgerHelper();
                ladgerHelper.GetOrCreateLadgerTypeAndLadger("Vendor",Convert.ToInt32(vendor.Id),vendor.CreatedBy, context);
                //end create a ledger 
                return vendorDC;
            }
        }

        public VendorPageDC GetList(VendorPager vendorPager)
        {
            using (var context = new AuthContext()) {
                var query = from v in context.VendorDB
                            join d in context.Departments
                            on v.DepartmentId equals d.DepId
                            join wc in context.WorkingCompanyDB
                            on (int)v.WorkingCompanyId equals wc.Id
                            join wl in context.WorkingLocationDB
                            on (int)v.WorkingCompanyId equals wl.Id
                            join s in context.States
                            on v.StateId equals s.Stateid
                            join exp in context.ExpenseTDSMasterDB
                            on v.ExpenseTDSMasterID equals exp.Id
                            into tempExpense
                            from k in tempExpense.DefaultIfEmpty()
                            where v.IsDeleted != true && v.IsActive == true 
                            select new VendorDC
                            {
                                Id = v.Id,
                                Code = v.Code,
                                Name = v.Name,
                                AddressOne = v.AddressOne,
                                AddressTwo = v.AddressTwo,
                                VendorType = v.VendorType,
                                WorkingCompanyId = v.WorkingCompanyId,
                                WorkingLocationId = v.WorkingLocationId,
                                DepartmentId = v.DepartmentId,
                                StateId = v.StateId,
                                ExpenseTDSMasterID = v.ExpenseTDSMasterID,
                                IsTDSApplied = v.IsTDSApplied,
                                DepartmentName = d.DepName,
                                WorkingCompanyName = wc.Name,
                                WorkingLocationName = wl.Name,
                                StateName = s.StateName,
                                ExpenseTDSMasterName = k != null ? k.SectionCode + "-" + k.RateOfTDS + "-" + k.Assessee : null,

                            };
                int count = query.Count();
                var list = query.OrderByDescending(x => x.Id).Skip(vendorPager.SkipCount).Take(vendorPager.Take).ToList();

                VendorPageDC vendorPageDC = new VendorPageDC();
                vendorPageDC.Count = count;
                vendorPageDC.PageList = list;
                return vendorPageDC;

               
            }
        }

        public VendorDC GetById(int Id) {

            using (var context = new AuthContext())
            {
                var query = from v in context.VendorDB
                            join exp in context.ExpenseTDSMasterDB
                            on v.ExpenseTDSMasterID equals exp.Id
                            into tempExpense
                            from k in tempExpense.DefaultIfEmpty()
                            where v.IsDeleted != true && v.IsActive == true && v.Id==Id

                            select new VendorDC
                            {
                                Id = v.Id,
                                Code = v.Code,
                                Name = v.Name,
                                AddressOne = v.AddressOne,
                                AddressTwo = v.AddressTwo,
                                VendorType = v.VendorType,
                                WorkingCompanyId = v.WorkingCompanyId,
                                WorkingLocationId = v.WorkingLocationId,
                                DepartmentId = v.DepartmentId,
                                StateId = v.StateId,
                                ExpenseTDSMasterID = v.ExpenseTDSMasterID,
                                IsTDSApplied = v.IsTDSApplied,
                                ExpenseTDSMasterName =k != null ? k.SectionCode + "-" + k.RateOfTDS + "-" + k.Assessee : null,

                            };
               
                var vendorDC = query.FirstOrDefault();

             
                return vendorDC;


            }
        }
    }
}