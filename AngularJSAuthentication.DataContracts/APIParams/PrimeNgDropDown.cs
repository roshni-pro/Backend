using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public class PrimeNgDropDown<T>
    {
        public string label { get; set; }
        public T value { get; set; }
    }

    public class PrimeNgDropDownAddition<T,U>
    {
        public string label { get; set; }
        public T value { get; set; }
        public U AdditionalField { get; set; }
    }
}
