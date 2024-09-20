using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public class ResponseWrapper<T, U>
    {
        public List<T> OutputList { get; set; }
        public T Output { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsSuccess { get; set; }
        public U ErrorObject { get; set; }
    }
}
