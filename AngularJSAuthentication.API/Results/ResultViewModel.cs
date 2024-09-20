using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Results
{
    public class ResultViewModel<T>
    {
        public bool IsSuceess { get; set; }
        public T ResultItem { get; set; }
        public List<T> ResultList { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
    }
}