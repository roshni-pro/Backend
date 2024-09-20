using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using AngularJSAuthentication.API;
using AngularJSAuthentication.Model;

namespace pos.Model
{
    public class Data
    {
        public class CategoryData
        {
            private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            public List<Category> Get()
            {
                var _context = new AuthContext();
                var _data = _context.Categorys.Where(x => x.Deleted == false).ToList();
                return _data;
            }
            public Category GetById(int Id)
            {
                var _context = new AuthContext();
                var _data = _context.Categorys.Where(x => x.Categoryid == Id && x.Deleted == false).SingleOrDefault();
                return _data;
            }
            public List<Category> GetByLevel(int Id)
            {
                var _context = new AuthContext();
                var _data = _context.Categorys.Where(x => x.Level == Id && x.Deleted == false).ToList();
                return _data;
            }
            public bool Post(Category _Data)
            {
                var _context = new AuthContext();
                var _datacat = _context.Categorys.Where(x => x.CategoryName == _Data.CategoryName).ToList();
                if (_datacat == null)
                {
                    _Data.CreatedDate = indianTime;
                    _Data.UpdatedDate = indianTime;
                    if (_Data.BaseCategoryId == 0)
                    {
                        _Data.Level = 0;
                    }
                    else {
                        var parent = GetById(Convert.ToInt32(_Data.BaseCategoryId));
                        _Data.Level = parent.Level + 1;
                    }
                    _context.Categorys.Add(_Data);
                    int i = _context.SaveChanges();
                    if (i == 0)
                    {
                        return false;
                    }

                }
                return true;
            }
            public bool Put(Category _mc)
            {
                using (var _context = new AuthContext())
                {
                   
                    _mc.UpdatedDate = indianTime;
                    _context.Categorys.Attach(_mc);
                    _context.Entry(_mc).State = EntityState.Modified;

                    int i = _context.SaveChanges();
                    if (i == 1)
                    {
                        return true;
                    }
                    return false;
                }
            }
            public bool Delete(int Id)
            {

                using (var _context = new AuthContext())
                {
                    var _data = _context.Categorys.Where(x => x.Categoryid == Id).SingleOrDefault();
                    var checkParent = _context.Categorys.Where(x => x.BaseCategoryId == _data.Categoryid).SingleOrDefault();
                    if (checkParent == null)
                    {
                        _data.UpdatedDate = indianTime;
                        _data.Deleted = true;
                        _context.Categorys.Attach(_data);
                        _context.Entry(_data).State = EntityState.Modified;
                        int i = _context.SaveChanges();
                        if (i == 1)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
        }  
              
        public class CustomerData
        {
            private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            public List<Customer> Get()
            {
                AuthContext _context = new AuthContext();
                var _data = _context.Customers.Where(x => x.Deleted == false).ToList();
                return _data;
            }
            public Customer GetById(int Id)
            {
                var _context = new AuthContext();
                var _data = _context.Customers.Where(x => x.CustomerId == Id && x.Deleted == false).SingleOrDefault();
                return _data;
            }
            public Customer GetByMobile(string contactNumber1)
            {
                var _context = new AuthContext();
                var _data = _context.Customers.Where(x => x.Mobile == contactNumber1 && x.Deleted == false).SingleOrDefault();
                return _data;
            }

            public bool Post(Customer _Data)
            {
                var _context = new AuthContext();
                _Data.CreatedDate = indianTime;
                _Data.UpdatedDate = indianTime;
                _context.Customers.Add(_Data);
                int i = _context.SaveChanges();
                if (i == 0)
                {
                    return false;
                }
                return true;
            }
            public bool Put(Customer _mc)
            {
                using (var _context = new AuthContext())
                {
                    _mc.UpdatedDate = indianTime;
                    _context.Customers.Attach(_mc);
                    _context.Entry(_mc).State = EntityState.Modified;

                    int i = _context.SaveChanges();
                    if (i == 1)
                    {
                        return true;
                    }
                    return false;
                }
            }
            public bool Delete(int Id)
            {

                using (var _context = new AuthContext())
                {
                    var _data = _context.Customers.Where(x => x.CustomerId == Id).SingleOrDefault();
                    _data.UpdatedDate = indianTime;
                    _data.Deleted = true;
                    _context.Customers.Attach(_data);
                    _context.Entry(_data).State = EntityState.Modified;
                    int i = _context.SaveChanges();
                    if (i == 1)
                    {
                        return true;
                    }
                    return false;
                }
            }

        }
        
        public class ProductData
        {
            private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            public List<ItemMaster> Get()
            {
                AuthContext _context = new AuthContext();
                var _data = _context.itemMasters.Where(x => x.Deleted == false).ToList();
                return _data;
            }
            public ItemMaster GetById(int Id)
            {
                var _context = new AuthContext();
                var _data = _context.itemMasters.Where(x => x.ItemId == Id && x.Deleted == false).SingleOrDefault();
                return _data;
            }
            public ItemMaster GetByName(string name)
            {
                var _context = new AuthContext();
                var _data = _context.itemMasters.Where(x => x.itemname == name).FirstOrDefault();
                return _data;
            }

            public bool Post(ItemMaster _Data)
            {
                var _context = new AuthContext();
                _Data.CreatedDate = indianTime;
                _Data.UpdatedDate = indianTime;
                _context.itemMasters.Add(_Data);
                int i = _context.SaveChanges();
                if (i == 0)
                {
                    return false;
                }
                return true;
            }
            public bool Put(ItemMaster _mc)
            {
                using (var _context = new AuthContext())
                {
                    _mc.UpdatedDate = indianTime;
                    _context.itemMasters.Attach(_mc);
                    _context.Entry(_mc).State = EntityState.Modified;

                    int i = _context.SaveChanges();
                    if (i == 1)
                    {
                        return true;
                    }
                    return false;
                }
            }
            public bool Delete(int Id)
            {

                using (var _context = new AuthContext())
                {
                    var _data = _context.itemMasters.Where(x => x.ItemId == Id).SingleOrDefault();
                    _data.UpdatedDate = indianTime;
                    _data.Deleted = true;
                    _context.itemMasters.Attach(_data);
                    _context.Entry(_data).State = EntityState.Modified;
                    int i = _context.SaveChanges();
                    if (i == 1)
                    {
                        return true;
                    }
                    return false;
                }
            }

            public List<ItemMaster> AddBulkMstProduct(List<ItemMaster> itemcollection)
            {
                try
                {
                    var _context = new AuthContext();
                    foreach (var product in itemcollection)
                    {
                        List<ItemMaster> productlist = _context.itemMasters.Where(x => x.itemname == product.itemname).ToList();
                        if (productlist.Count == 0)
                        {
                            product.CreatedDate = indianTime;
                            product.UpdatedDate = indianTime;
                            _context.itemMasters.Add(product);
                            int i = _context.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                }
                return null;
            }
        }
        
        public class SupplierData
        {
            private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            public List<Supplier> Get()
            {
                AuthContext _context = new AuthContext();
                var _data = _context.Suppliers.Where(x => x.Deleted == false).ToList();
                return _data;
            }
            public Supplier GetById(int Id)
            {
                var _context = new AuthContext();
                var _data = _context.Suppliers.Where(x => x.SupplierId == Id && x.Deleted == false).SingleOrDefault();
                return _data;
            }

            public bool Post(Supplier _Data)
            {
                var _context = new AuthContext();
                _Data.CreatedDate = indianTime;
                _Data.UpdatedDate = indianTime;
                _context.Suppliers.Add(_Data);
                int i = _context.SaveChanges();
                if (i == 0)
                {
                    return false;
                }
                return true;
            }
            public bool Put(Supplier _mc)
            {
                using (var _context = new AuthContext())
                {
                    _mc.UpdatedDate = indianTime;
                    _context.Suppliers.Attach(_mc);
                    _context.Entry(_mc).State = EntityState.Modified;

                    int i = _context.SaveChanges();
                    if (i == 1)
                    {
                        return true;
                    }
                    return false;
                }
            }
            public bool Delete(int Id)
            {

                using (var _context = new AuthContext())
                {
                    var _data = _context.Suppliers.Where(x => x.SupplierId == Id).SingleOrDefault();
                    _data.UpdatedDate = indianTime;
                    _data.Deleted = true;
                    _context.Suppliers.Attach(_data);
                    _context.Entry(_data).State = EntityState.Modified;
                    int i = _context.SaveChanges();
                    if (i == 1)
                    {
                        return true;
                    }
                    return false;
                }
            }

        }
        
        public class WarehouseData
        {
            private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            public List<Warehouse> Get()
            {
                var _context = new AuthContext();
                var _data = _context.Warehouses.Where(x => x.Deleted == false).ToList();
                return _data;
            }
            public Warehouse GetById(int? Id)
            {
                var _context = new AuthContext();
                var _data = _context.Warehouses.Where(x => x.Warehouseid == Id && x.Deleted == false).SingleOrDefault();
                return _data;
            }

            public bool Post(Warehouse _Data)
            {
                var _context = new AuthContext();
                _Data.UpdatedDate = indianTime;
                _context.Warehouses.Add(_Data);
                int i = _context.SaveChanges();
                if (i == 0)
                {
                    return false;
                }
                return true;
            }
            public bool Put(Warehouse _mc)
            {
                using (var _context = new AuthContext())
                {
                    _mc.UpdatedDate = indianTime;
                    _context.Warehouses.Attach(_mc);
                    _context.Entry(_mc).State = EntityState.Modified;

                    int i = _context.SaveChanges();
                    if (i == 1)
                    {
                        return true;
                    }
                    return false;
                }
            }
            public bool Delete(int Id)
            {
                using (var _context = new AuthContext())
                {
                    var _data = _context.Warehouses.Where(x => x.Warehouseid == Id).SingleOrDefault();
                    _data.UpdatedDate = indianTime;
                    _data.Deleted = true;
                    _context.Warehouses.Attach(_data);
                    _context.Entry(_data).State = EntityState.Modified;
                    int i = _context.SaveChanges();
                    if (i == 1)
                    {
                        return true;
                    }
                    return false;
                }
            }
        }
        
    }
}
