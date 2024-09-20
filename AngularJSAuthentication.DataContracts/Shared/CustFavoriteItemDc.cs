using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Shared
{
    public class CustFavoriteItemDc
    {
        public int id { get; set; }
        public int ItemId { get; set; }
        public int CustomerId { get; set; }
        public bool IsLike { get; set; }
    }
}
