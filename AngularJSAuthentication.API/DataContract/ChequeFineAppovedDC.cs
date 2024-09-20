using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.DataContract
{
    public class ChequeFineAppovedDC
    {
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public long ChequeCollectionId { get; set; }
        public decimal FineAmount { get; set; }
        public decimal ChangeAmount { get; set; }
        public int AppovedBy { get; set; }
        public string Note { get; set; }
        public int Status { get; set; }

    }
    public class ChequeFineDc
    {
        public long ChequeCollectionId { get; set; }

        public decimal FineAmount { get; set; }
        public decimal ChangeAmount { get; set; }
        public int AppovedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ChequeNumber { get; set; }
        public decimal ChequeAmt { get; set; }
        public DateTime ChequeDate { get; set; }
        public int OrderId { get; set; }
        public int Status { get; set; }
        public string Note { get; set; }
        public string StatusText
        {
            get
            {
                ChequeAppovedStatusEnum chequeFineEnum = (ChequeAppovedStatusEnum)Status;
                if (Status > 0)
                    return chequeFineEnum.ToString();
                else
                    return "";
            }
        }
        public int WarehouseId { get; set; }
        public int RejectedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }

        public int ReturnStatus { get; set; }

    }



}