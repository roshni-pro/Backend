using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Caching;
using AngularJSAuthentication.Model.Stocks.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Managers.Stocks
{
    public class StockTransactionManager
    {
        ICacheProvider _cacheProvider = new RedisCacheProvider();

        public List<StockTransactionMaster> GetMasterList()
        {
            List<StockTransactionMaster> masterList = StockTransactionMasterGet();
            if(masterList == null)
            {
                using(var context = new AuthContext())
                {
                    masterList = context.StockTransactionMasterDB.ToList();
                    StockTransactionMasterSet(masterList);
                }
            }
            return masterList;
        }
        public List<StockTransactionCondition> GetConditionList()
        {
            List<StockTransactionCondition> conditionList = StockTransactionConditionGet();
            if (conditionList == null)
            {
                using (var context = new AuthContext())
                {
                    conditionList = context.StockTransactionConditionDB.ToList();
                    StockTransactionConditionSet(conditionList);
                }
            }
            return conditionList;
        }
        public List<StockTransactionMaster> GetMasterByEntityName(string entityName)
        {
            List<StockTransactionMaster> list = GetMasterList();
            if (list != null && list.Any())
            {
                list = list.Where(x => x.IsActive == true && x.IsDeleted == false && x.EntityName == entityName ).ToList();
            }
            return list;
        }

        public List<StockTransactionCondition> GetConditionListByMasterId(long stockTransactionMasterId)
        {
            List<StockTransactionCondition> list = GetConditionList();
            return list.Where(x => x.IsActive == true && x.IsDeleted != true && x.StockTransactionMasterId == stockTransactionMasterId).ToList();
        }


        #region redis methods
        private void StockTransactionMasterSet(List<StockTransactionMaster> stockTransactionMasterList)
        {
            try
            {
                _cacheProvider.Set("StockTransactionMasterCache", stockTransactionMasterList);
            }
            catch (Exception ex) { }
        }
        private List<StockTransactionMaster> StockTransactionMasterGet()
        {
            try
            {
                List<StockTransactionMaster> data = _cacheProvider.Get<List<StockTransactionMaster>>("StockTransactionMasterCache");
                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private void StockTransactionConditionSet(List<StockTransactionCondition> stockTransactionConditionList)
        {
            try
            {
                _cacheProvider.Set("StockTransactionConditionCache", stockTransactionConditionList);
            }
            catch (Exception ex)
            {

            }

        }
        private List<StockTransactionCondition> StockTransactionConditionGet()
        {

            try
            {
                List<StockTransactionCondition> data = _cacheProvider.Get<List<StockTransactionCondition>>("StockTransactionConditionCache");
                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion
    }
}