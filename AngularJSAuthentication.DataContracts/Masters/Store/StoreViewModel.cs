using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.Store
{
    public class StoreViewModel
    {
        public long Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public List<StoreBrandViewModel> BrandList { get; set; }
        public int OwnerId { get; set; }
        public bool IsUniversal { get; set; }
    }
}
