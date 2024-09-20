using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AngularJSAuthentication.API.DataContract
{
    public class CurrencySettlementSourceDc
    {
        public long Id { get; set; }
        public string WarehouseName { get; set; }
        public int WarehouseId { get; set; }
        public decimal TotalCashAmt { get; set; }
        public decimal TotalChequeAmt { get; set; }
        [StringLength(255)]
        [Required]
        public string SettlementSource { get; set; }
        public DateTime SettlementDate { get; set; }
        public int HandOverPerson { get; set; }
        public string HandOverPersonName { get; set; }
        public int Status { get; set; }
        [StringLength(1000)]
        public string Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public int DepositType { get; set; }
        public string Statustext
        {
            get
            {
                if (Status == 0)
                    return "Un-Verified";
                else
                    return "Verified";
            }
        }
        public string ProcessTransId { get; set; }
        public List<CashSettlementDc> CashSettlements { get; set; }
        public List<CurrencySettlementImagesDc> SettlementProofImages { get; set; }
        public List<CurrencySettlementImagesDc> SlipImages { get; set; }
    }

    public class CashSettlementDc
    {
        public long Id { get; set; }
        public int CurrencyDenominationId { get; set; }
        public int CurrencyCount { get; set; }
        public string CurrencyDenominationTitle { get; set; }
        public int CurrencyDenominationValue { get; set; }

        public int CurrencyDenominationTotal
        {
            get
            {
                return CurrencyCount * CurrencyDenominationValue;
            }
        }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public long CurrencySettlementSourceId { get; set; }
    }

    public class CurrencySettlementImagesDc
    {
        public long Id { get; set; }
        public long CurrencySettlementSourceId { get; set; }
        [StringLength(1000)]
        public string SettlementImage { get; set; }
        public string SettlementImagename { get; set; }
        [StringLength(250)]
        public string SettlementImageType { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }

    }

    public class CurrencySettlementBankDc
    {
        public long Id { get; set; }
        public string BankName { get; set; }
        public string BankImage { get; set; }
    }

    public class BankDepositDetailDc
    {
        public long Id { get; set; }
        public string SettlementSource { get; set; }
        public DateTime SettlementDate { get; set; }
        public int HandOverPerson { get; set; }
        public string HandOverPersonName { get; set; }
        public int DepositType { get; set; }
        [StringLength(1000)]
        public string Note { get; set; }
        public long? currencyHubStockId { get; set; }
        public int WarehouseId { get; set; }
        public List<CurrencySettlementBankDc> CurrencySettlementBankDcs { get; set; }
        public List<ChequeCollectionDc> ChequeCollectionDcs { get; set; }
        public List<hubCashCollectionDc> hubCashCollectionDcs { get; set; }
    }
    public class ChequeBounceDc
    {

        public int Id { get; set; }
        public string Address { get; set; }
        public string SKCode { get; set; }
        public string ChequeNumber { get; set; }
        public decimal ChequeAmt { get; set; }
        public int Orderid { get; set; }
        public string Date { get; set; }
        public string PartyName { get; set; }
        public string ReturnComment { get; set; }

        [NotMapped]
        public bool IsDisable { get; set; }





    }



}


