﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public class ConnsentDetailsVM
    {
        public int CustomerId { get; set; }
        public string Guid { get; set; }
        public string Consent { get; set; }
        public int  Status { get; set; }
    }
}
