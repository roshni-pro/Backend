using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public static class Validation
    {
        public static void ISvalidate(bool IsMandatory, Regex Regularexpression, string Message, string Field)
        {
            if (IsMandatory)
            {
                if (string.IsNullOrEmpty(Field))
                {
                    throw new HttpException( Message);
                }
            }

            if (!Regularexpression.IsMatch(Field))
            {
                throw new HttpException( Message);
            }
        }

        
    }
}