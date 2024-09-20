using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class SupplierFilterDc
    {
        public int? cityid { get; set; }
        public int? SubCaegoryId { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }

    public class SupplierPaginationData
    {

        public List<SupplierBrandsVM> SupplierListDc { get; set; }

        public int total { get; set; }


    }

    public class SupplierBrandsVM
    {
        public string categorynameS { get; set; }
        public int SupplierId { get; set; }
        public int? SubCategoryId { get; set; }
        public string SubcategoryName { get; set; }
        public int CompanyId { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int SupplierCaegoryId { get; set; }
        public int? PeopleID { get; set; }
        public bool Deleted { get; set; }
        public string CategoryName { get; set; }
        public string Brand { get; set; }
        //  public int BrandId { get; set; }
        public string Name { get; set; }
        public string SUPPLIERCODES { get; set; }
        public int PaymentTerms { get; set; }

        //change
        public string OwnerName
        {
            get; set;
        }
        public string SupplierAddress
        {
            get; set;
        }
        public string Pincode
        {
            get; set;
        }
        public string GstInNumber   //this is not in use TINNo is GST
        {
            get; set;
        }
        public string Pancard
        {
            get; set;
        }
        public string HeadOffice
        {
            get; set;
        }
        public string OpeningHours
        {
            get; set;
        }
        public string Password
        {
            get; set;
        }
        public bool Active { get; set; }
        public string OfficePhone { get; set; }
        public string Bank_Name { get; set; } //-- mandatory
        public string Bank_AC_No { get; set; }   //-- mandatory
        public string Bank_Ifsc { get; set; }    //-- mandatory
        public string PhoneNumber { get; set; }
        public string TINNo { get; set; }      //-- mandatory  this is GST
        public int Avaiabletime { get; set; }
        public int rating { get; set; }
        public string BillingAddress { get; set; } //Address 
        public string ShippingAddress { get; set; }//Location
        public string Comments { get; set; }
        public bool IsVerified
        {
            get; set;
        }
        public string MobileNo { get; set; }   // main phone number mandetory 
        public string EmailId { get; set; }    //-- mandatory
        public string WebUrl { get; set; }
        public string SalesManager { get; set; }
        public string ContactPerson { get; set; }
        public string ContactImage { get; set; }
        //--------------------------------------------------tejas for supplier app
        public string ShopName { get; set; }
        public int EstablishmentYear { get; set; }
        public string bussinessType { get; set; }
        public string Description { get; set; }
        public DateTime? StartedBusiness { get; set; }
        public string FSSAI { get; set; }         //-- mandatory
        public string ManageAddress { get; set; }    // this is bank account address    //-- mandatory
        public string businessImageUrl { get; set; }   // shop or store image from supplier app    //-- mandatory
        public string ChequeImageUrl { get; set; }     // check image from supplier app    
        public int? BankPINno { get; set; }        //-- mandatory  bank account address pin number 
        public string fcmId { get; set; }   //-- to send notifications 
        public string DeviceId { get; set; }   //-- to send notifications 
                                               //-------------------------------------------------tejas for supplier app
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? Cityid { get; set; }  //-- mandatory
        public string City { get; set; }     //-- mandatory
        public string CityPincode { get; set; }     //-- mandatory 
        public int? Stateid { get; set; }
        public string StateName { get; set; }
        public bool IsCityVerified { get; set; }
        //by Anushka
        public int DepoId { get; set; }
        public string DepoName { get; set; }
        public int Amount { get; set; }
        public string ImageUrl { get; set; } //profile image from supplier app 


        //-------------------------------Anushka
        public string GstImage { get; set; }
        public string FSSAIImage { get; set; }
        public string PanCardImage { get; set; }
        public string CancelCheque { get; set; }
        //---------------------------------------

        public int? CibilScore { get; set; }

        public bool? IsStopAdvancePr { get; set; }
        public bool? IsIRNInvoiceRequired { get; set; }
    }
}
