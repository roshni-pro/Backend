using AngularJSAuthentication.BusinessLayer.Helpers.ElasticDataHelper;
using AngularJSAuthentication.DataContracts.ElasticSearch;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.BatchManager.Helpers
{
    public class ElasticItemHelper
    {
        public bool InsertInElasticWithItemId(ItemIdCls itemId)
        {

            ElasticSalesAppClusterItemDataHelper helper = new ElasticSalesAppClusterItemDataHelper();
            helper.InsertInElasticWithItemId(itemId);

            return true;

        }

        public bool InsertInElasticWithMrpId(MultiMrpIdCls itemId)
        {

            ElasticSalesAppClusterItemDataHelper helper = new ElasticSalesAppClusterItemDataHelper();
            helper.InsertInElasticWithMrpId(itemId);

            return true;

        }
    }
}
