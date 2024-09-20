using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts
{
    public class TripBlockedOrderVM
    {
        public String EwayBillReason { get; set; }
        public String IRNReson { get; set; }
        public int DeliveryIssuanceId { get; set; }
        public int OrderId { get; set; }

    }

    public class GetReplaceVehicleListDc
    {
        public bool IsAlreadyEwaybillGenerate { get; set; }
        public bool IsReplacementVehicleNo { get; set; }
        public DateTime TripDate { get; set; }
        public bool IsExistsVehicle { get; set; }
    }
}
