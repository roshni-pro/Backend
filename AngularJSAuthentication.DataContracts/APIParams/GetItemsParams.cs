using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public class GetItemsParams
    {
        public List<int> WarehouseId { get; set; }
        public List<int> BrandId { get; set; }

    }
}
