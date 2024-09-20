using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.RazorPay.POS
{

    public class ErrorCodes
    {
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }

        public static  List<ErrorCodes> GetErrorCodes()
        {
            return new List<ErrorCodes>
            {
                new ErrorCodes{ ErrorCode="EZETAP_0000382",ErrorDescription="Device not found  - The correct device ID is not passed"},
                new ErrorCodes{ErrorCode="EZETAP_0000385",ErrorDescription=" Device is not in the Network."},
                new ErrorCodes{ErrorCode="EZETAP_0000039",ErrorDescription="Payment amount unsupported."},
                new ErrorCodes{ErrorCode="EZETAP_0000050",ErrorDescription="Transaction amount greater than Limit."},
                new ErrorCodes{ErrorCode="EZETAP_0000162",ErrorDescription="Transaction amount less than Limit."},
                new ErrorCodes{ErrorCode="EZETAP_0000048",ErrorDescription="Payment tip not enabled."},
                new ErrorCodes{ErrorCode="EZETAP_0000148",ErrorDescription=" Invalid Org, device does not belong to the    org."},
                new ErrorCodes{ErrorCode="EZETAP_0000047",ErrorDescription="Payment tip amount error."},
                new ErrorCodes{ErrorCode="EZETAP_0000387",ErrorDescription="ExternalRefNumber field is empty."},
                new ErrorCodes{ErrorCode="EZETAP_6000001",ErrorDescription="No such payment mode exists."},
                new ErrorCodes{ErrorCode="EZETAP_0000623",ErrorDescription="Device is busy with pending notification"},
                new ErrorCodes{ErrorCode="P2P_DEVICE_RECEIVED",ErrorDescription=" Notification received on POS device "},
                new ErrorCodes{ErrorCode="P2P_DEVICE_SENT",ErrorDescription=" If notification is sent to the device "},
                new ErrorCodes{ErrorCode="P2P_STATUS_QUEUED",ErrorDescription=" If notification is queued on the server "},
                new ErrorCodes{ErrorCode="P2P_STATUS_IN_EXPIRED",ErrorDescription="On Notification expiration"},
                new ErrorCodes{ErrorCode="P2P_DEVICE_TXN_DONE",ErrorDescription=" Transaction completed on POS device, need to look at txn fields."},
                new ErrorCodes{ErrorCode="P2P_STATUS_UNKNOWN",ErrorDescription=" POS Bridge Notification Status is in Unknown state. "},
                new ErrorCodes{ErrorCode="P2P_DEVICE_CANCELED",ErrorDescription="POS Bridge Notification has been Canceled on Receiving device."},
                new ErrorCodes{ErrorCode="P2P_STATUS_IN_CANCELED_FROM_EXTERNAL_SYSTEM",ErrorDescription=" Canceled successfully by POS Bridge Cancel API"}
            };
            
        }
        
    }

    

}

