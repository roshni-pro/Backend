using AngularJSAuthentication.BusinessLayer.PackingMaterial.BO;
using AngularJSAuthentication.BusinessLayer.PackingMaterial.IF;
using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AngularJSAuthentication.API.App_Code.PackingMaterial
{
    public class ItemMasterRepository : IItemMaster
    {
        private bool disposed = false;
        private AuthContext Context;
        private static DateTime CurrentDatetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Context.Dispose();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public ItemMasterRepository(AuthContext Context)
        {
            this.Context = Context;
        }


        public List<ItemMaster> GetItemMaster(string key, int Warehouseid)
        {
            try
            {
                List<ItemMaster> ItemMaster = Context.itemMasters.Where(t => t.WarehouseId == Warehouseid && (t.itemname.Contains(key) || t.Number.Contains(key)) && t.Deleted == false).ToList();
                return ItemMaster;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }

        public ItemMaster GetRawItemMaster(int key, int Warehouseid)
        {
            try
            {
                ItemMaster ItemMaster = Context.itemMasters.Where(t => t.ItemId == key && t.WarehouseId == Warehouseid && t.Type==2).FirstOrDefault();
                return ItemMaster;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }




        public MaterialItemMaster GetMaterialMaster(string ItemNumber)
        {
           try
            {
                MaterialItemMaster MaterialItemMaster = Context.MaterialItemMaster.Where(t => t.ItemNumber==ItemNumber).FirstOrDefault();
                return MaterialItemMaster;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public ItemMaster GetItemMaster(int key, int Warehouseid)
        {
            throw new NotImplementedException();
        }
    }
}
