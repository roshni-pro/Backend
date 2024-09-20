using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers.GstVerifySupplier;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Transaction.Customer;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Transactions;
using System.Web;


namespace AngularJSAuthentication.API.Helper.SupplierOnboard
{
    public class SupplierHelper
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        public bool AddnewSupplier(SupplierOnBoardDC supplierdc)
        {
            bool result = false; bool bransresult = false;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);

            using (TransactionScope Scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                try
                {
                    supplierdc.IsLoanRequirement = (supplierdc.loanAmount > 0) ? "Yes" : "No";

                    using (var context = new AuthContext())
                    {
                        //code for skscode autogenerate created by anjali
                        GstverifyController gs = new GstverifyController();
                        var skscode = gs.getautoscode();
                        //
                        supplierdc.SUPPLIERCODES = skscode;
                        var cityname = context.Cities.Where(x => x.Cityid == supplierdc.Cityid && x.Deleted == false).FirstOrDefault().CityName;
                        var statename = context.Cities.Where(x => x.Stateid == supplierdc.Stateid && x.Deleted == false).FirstOrDefault().StateName;
                        SupplierTemp supplier = new SupplierTemp();
                        {
                            supplier.Name = supplierdc.SupplierName;
                            supplier.TypeofFirm = supplierdc.TypeofFirm;
                            supplier.OwnerName = supplierdc.ProprietorName;
                            supplier.Pancard = supplierdc.PanNumber;
                            supplier.PanCardImage = supplierdc.PanNumberImage[0];

                            supplier.AddressProofType = supplierdc.AddressProofType;
                            supplier.AddressProofImage = supplierdc.AddressProofImage == null ? null : supplierdc.AddressProofImage[0];
                            supplier.EmailId = supplierdc.EmailId;
                            supplier.MobileNo = supplierdc.MobileNo;
                            supplier.CibilScore = supplierdc.CreditScore;
                            supplier.FatherName = supplierdc.FatherName;
                            supplier.IsLoanRequirement = supplierdc.IsLoanRequirement;
                            supplier.LoanAmount = supplierdc.loanAmount;
                            supplier.SupplierType = supplierdc.SupplierType;
                            supplier.EstablishmentYear = supplierdc.BusinessEstablishmentYear;
                            supplier.GstInNumber = supplierdc.GSTINNO;
                            supplier.TINNo = supplierdc.GSTINNO;
                            supplier.FSSAI = supplierdc.FSSAINO;
                            supplier.FSSAIImage = supplierdc.FSSAIImage != null && supplierdc.FSSAIImage.Any() ? supplierdc.FSSAIImage[0] : null;
                            supplier.SupplierAddress = supplierdc.FullBusinessAddress;
                            supplier.HeadOffice = supplierdc.HeadOffice;
                            supplier.Pincode = supplierdc.PinCode;
                            supplier.BankAccountType = supplierdc.BankAccType;
                            supplier.Bank_AC_No = supplierdc.BankAccNo;
                            supplier.BankAddress = supplierdc.BankAddress;
                            supplier.Bank_Name = supplierdc.BankName;
                            supplier.Bank_Ifsc = supplierdc.IFSCCode;
                            supplier.Createdby = supplierdc.userid;
                            supplier.CreatedDate = DateTime.Now;
                            supplier.Deleted = false;
                            supplier.Active = true;
                            supplier.Status = supplierStatus.Pending.ToString();

                            //supplier.//commented and created by anjali
                            // supplier.//SUPPLIERCODES = supplierdc.SUPPLIERCODES,
                            supplier.SUPPLIERCODES = supplierdc.SUPPLIERCODES;
                            //Added Bank pin code and proprietor phone no. by anjali
                            supplier.BankPINno = supplierdc.bankPinCode;
                            supplier.proprietorphonenumber = supplierdc.proprietorphonenumber;
                            supplier.IsStopAdvancePr = supplierdc.IsStopAdvancePr;
                            supplier.IsIRNInvoiceRequired = supplierdc.IsIRNInvoiceRequired;
                            supplier.Password = supplierdc.Password;
                            supplier.WebUrl = supplierdc.WebUrl;
                            supplier.ContactPerson = supplierdc.ContactPerson;
                            supplier.bussinessType = supplierdc.bussinessType;
                            supplier.StartedBusiness = supplierdc.StartedBusiness;
                            supplier.PaymentTerms = supplierdc.PaymentTerms;
                            supplier.CompanyId = supplierdc.CompanyId;

                            supplier.GstImage = supplierdc.GSTImage[0];
                            supplier.ChequeImageUrl = supplierdc.ChequeImage[0];
                            supplier.SupplierSegment = supplierdc.SupplierSegment;
                            supplier.ContactPersonMobileNo = supplierdc.ContactPersonMobileNo;
                            supplier.ExpiryDays = supplierdc.ExpiryDays;
                            supplier.City = cityname;
                            supplier.Cityid = supplierdc.Cityid;
                            supplier.Stateid = supplierdc.Stateid;
                            supplier.StateName = statename;
                            supplier.ProprietorPanNumber = supplierdc.ProprietorPanNumber;
                            supplier.MSMEType = supplierdc.MSMEType;
                            supplier.MSMEImage = supplierdc.MSMEImage != null && supplierdc.MSMEImage.Any() ? supplierdc.MSMEImage[0] : null;
                            supplier.BuyerId = supplierdc.BuyerId;
                        };

                        context.SupplierTempDB.Add(supplier);

                        if (context.Commit() > 0)
                        {
                            if (supplierdc.SellingBrandDCs != null && supplierdc.SellingBrandDCs.Any())
                            {

                                foreach (var a in supplierdc.SellingBrandDCs)
                                {
                                    var IsAdd = context.SupplierBrandMaps.Where(x => x.BrandId == a && x.SupplierId == supplier.Id).Count();
                                    if (IsAdd == 0)
                                    {
                                        var supplierbrandmap = new SupplierBrandMap();
                                        supplierbrandmap.BrandId = a;
                                        supplierbrandmap.SupplierId = supplier.Id;
                                        supplierbrandmap.Active = true;
                                        supplierbrandmap.Deleted = true;
                                        context.SupplierBrandMaps.Add(supplierbrandmap);
                                        if (context.Commit() > 0)
                                        {
                                            bransresult = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                bransresult = true;
                            }

                            if (supplierdc.PanNumberImage.Any())
                            {
                                List<SupplierDocument> panimglist = new List<SupplierDocument>();
                                foreach (var item in supplierdc.PanNumberImage)
                                {
                                    SupplierDocument doc = new SupplierDocument()
                                    {
                                        SupplierTempId = supplier.Id,
                                        SupplierId = supplier.Id,
                                        DocumentPath = item,
                                        DocumentName = "PanCard",
                                        CreatedBy = supplier.Createdby,
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreateDate = DateTime.Now
                                    };
                                    panimglist.Add(doc);
                                }
                                context.SupplierDocumentDB.AddRange(panimglist);
                                context.Commit();
                            }
                            if (supplier.TypeofFirm == "Proprietor" || supplier.TypeofFirm == "Partnership" || supplier.TypeofFirm == "LLP")
                            {
                                if (supplierdc.AddressProofImage.Any())
                                {
                                    List<SupplierDocument> addimglist = new List<SupplierDocument>();
                                    foreach (var item in supplierdc.AddressProofImage)
                                    {
                                        SupplierDocument doc = new SupplierDocument()
                                        {
                                            SupplierTempId = supplier.Id,
                                            SupplierId = supplier.Id,
                                            DocumentPath = item,
                                            DocumentName = "AddessProof",
                                            CreatedBy = supplier.Createdby,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.SupplierDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }
                            }
                            //if(supplier.SupplierSegment== "Food" || supplier.SupplierSegment == "Both")
                            //{
                            if (supplierdc.FSSAIImage != null && supplierdc.FSSAIImage.Any())
                            {
                                List<SupplierDocument> addimglist = new List<SupplierDocument>();
                                foreach (var item in supplierdc.FSSAIImage)
                                {
                                    SupplierDocument doc = new SupplierDocument()
                                    {
                                        SupplierTempId = supplier.Id,
                                        SupplierId = supplier.Id,
                                        DocumentPath = item,
                                        DocumentName = "FSSAI",
                                        CreatedBy = supplier.Createdby,
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreateDate = DateTime.Now
                                    };
                                    addimglist.Add(doc);
                                }
                                context.SupplierDocumentDB.AddRange(addimglist);
                                context.Commit();
                            }
                            //}

                            if (supplierdc.ChequeImage.Any())
                            {
                                List<SupplierDocument> addimglist = new List<SupplierDocument>();
                                foreach (var item in supplierdc.ChequeImage)
                                {
                                    SupplierDocument doc = new SupplierDocument()
                                    {
                                        SupplierTempId = supplier.Id,
                                        SupplierId = supplier.Id,
                                        DocumentPath = item,
                                        DocumentName = "BankAccount",
                                        CreatedBy = supplier.Createdby,
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreateDate = DateTime.Now
                                    };
                                    addimglist.Add(doc);
                                }
                                context.SupplierDocumentDB.AddRange(addimglist);
                                context.Commit();
                            }
                            if (supplierdc.GSTImage.Any())
                            {
                                List<SupplierDocument> addimglist = new List<SupplierDocument>();
                                foreach (var item in supplierdc.GSTImage)
                                {
                                    SupplierDocument doc = new SupplierDocument()
                                    {
                                        SupplierTempId = supplier.Id,
                                        SupplierId = supplier.Id,
                                        DocumentPath = item,
                                        DocumentName = "GSTN",
                                        CreatedBy = supplier.Createdby,
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreateDate = DateTime.Now
                                    };
                                    addimglist.Add(doc);
                                }
                                context.SupplierDocumentDB.AddRange(addimglist);
                                context.Commit();
                            }
                            if (supplierdc.AgreementImage != null && supplierdc.AgreementImage.Any())
                            {
                                List<SupplierDocument> addimglist = new List<SupplierDocument>();
                                foreach (var item in supplierdc.AgreementImage)
                                {
                                    SupplierDocument doc = new SupplierDocument()
                                    {
                                        SupplierTempId = supplier.Id,
                                        SupplierId = supplier.Id,
                                        DocumentPath = item,
                                        DocumentName = "Agreement",
                                        CreatedBy = supplier.Createdby,
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreateDate = DateTime.Now
                                    };
                                    addimglist.Add(doc);
                                }
                                context.SupplierDocumentDB.AddRange(addimglist);
                                context.Commit();
                            }
                            //if (supplier.TypeofFirm == "Proprietor")
                            //{
                            if (supplierdc.ProprietorPanImage != null && supplierdc.ProprietorPanImage.Any())
                            {
                                List<SupplierDocument> addimglist = new List<SupplierDocument>();
                                foreach (var item in supplierdc.ProprietorPanImage)
                                {
                                    SupplierDocument doc = new SupplierDocument()
                                    {
                                        SupplierTempId = supplier.Id,
                                        SupplierId = supplier.Id,
                                        DocumentPath = item,
                                        DocumentName = "ProprietorPanCard",
                                        CreatedBy = supplier.Createdby,
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreateDate = DateTime.Now
                                    };
                                    addimglist.Add(doc);
                                }
                                context.SupplierDocumentDB.AddRange(addimglist);
                                context.Commit();
                            }
                            //}

                            if (supplierdc.MSMEImage != null && supplierdc.MSMEImage.Any())
                            {
                                List<SupplierDocument> addimglist = new List<SupplierDocument>();
                                foreach (var item in supplierdc.MSMEImage)
                                {
                                    SupplierDocument doc = new SupplierDocument()
                                    {
                                        SupplierTempId = supplier.Id,
                                        SupplierId = supplier.Id,
                                        DocumentPath = item,
                                        DocumentName = "MSME",
                                        CreatedBy = supplier.Createdby,
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreateDate = DateTime.Now
                                    };
                                    addimglist.Add(doc);
                                }
                                context.SupplierDocumentDB.AddRange(addimglist);
                                context.Commit();
                            }
                            result = true;
                        }

                        #region Add Hisotry 
                        SupplierOnboardHistory history = new SupplierOnboardHistory()
                        {
                            Suppliercode = supplier.SUPPLIERCODES,
                            /*Suppliercode = supplierdc.SUPPLIERCODES*/
                            Comments = "New Supplier Added",
                            CreatedBy = supplierdc.userid,
                            CreatedDate = DateTime.Now,
                            IsActive = true,
                            IsDeleted = false,
                            SupplierId = supplier.Id,
                            Status = supplierStatus.Pending.ToString()
                        };
                        InsertSupplierOnboardHistory(history);

                        #endregion
                    }
                    if (result && bransresult)
                    {
                        Scope.Complete();
                    }
                    else
                    {
                        Scope.Dispose();
                    }
                }

                catch (Exception ex)
                {
                    logger.Error("Error in Add new Supplier  " + ex.Message);
                    Scope.Dispose();
                    throw ex;
                }
            }

            return result;
        }


        public SupplierOnBoardDCList GetSupplierOnboardList(string status, string KeyWord, int skip, int take, int userid = 0)
        {
            SupplierOnBoardDCList SupplierOnBoardDCList = new SupplierOnBoardDCList();
            //List<SupplierOnBoardDC> suppliers = new List<SupplierOnBoardDC>();

            using (var context = new AuthContext())
            {
                var Status = new SqlParameter("@Status", status);
                var Keyword = new SqlParameter("@KeyWord", KeyWord== null ? "":KeyWord);
                var Skip = new SqlParameter("@skip", skip);
                var Take = new SqlParameter("@take", take);
                var id = new SqlParameter("@userid", userid);

                SupplierOnBoardDCList.SupplierOnBoardDC = context.Database.SqlQuery<SupplierOnBoardDC>("exec GetSupplierOnboradList @Status,@userid,@KeyWord,@skip,@take", Status, id, Keyword, Skip, Take).ToList();
                List<SupplierBrandMap> SupplierBrandMaps = null;
                List<Supplier> dbSuppliers = null;
                List<SupplierDocument> SupplierDocuments = null;
                if (SupplierOnBoardDCList.SupplierOnBoardDC.Any())
                {
                    SupplierOnBoardDCList.Totalcount= SupplierOnBoardDCList.SupplierOnBoardDC.FirstOrDefault().Totalcount;

                    var scodes = SupplierOnBoardDCList.SupplierOnBoardDC.Select(x => x.SUPPLIERCODES).ToList();
                    dbSuppliers = context.Suppliers.Where(x => scodes.Contains(x.SUPPLIERCODES)  && x.Deleted == false).ToList();
                    var sIds = dbSuppliers != null && dbSuppliers.Any() ? dbSuppliers.Select(x => x.SupplierId).ToList() : new List<int>();
                    SupplierBrandMaps = context.SupplierBrandMaps.Where(x => sIds.Contains(x.SupplierId) && x.Active == true).ToList();
                    SupplierDocuments = context.SupplierDocumentDB.Where(x => sIds.Contains(x.SupplierId) && x.IsActive == true && x.IsDeleted == false).ToList();

                    foreach (var item in SupplierOnBoardDCList.SupplierOnBoardDC)
                    {
                        int supId = 0;
                        if (supId == 0 && item.id > 0)
                        {
                            var dbsupplierid = dbSuppliers.FirstOrDefault(x => x.SUPPLIERCODES == item.SUPPLIERCODES)?.SupplierId;
                            supId = dbsupplierid.HasValue ? dbsupplierid.Value : item.id;
                            if (!dbsupplierid.HasValue)
                            {
                                SupplierBrandMaps.AddRange(context.SupplierBrandMaps.Where(x => x.SupplierId == supId && x.Active == true).ToList());
                                SupplierDocuments.AddRange(context.SupplierDocumentDB.Where(x => x.SupplierId == supId && x.IsActive == true && x.IsDeleted == false).ToList());
                            }
                        }
                        else if (item.id == 0)
                        {
                            supId = item.SupplierId;
                        }

                        item.SellingBrandDCs = SupplierBrandMaps.Where(x => x.SupplierId == supId && x.Active == true && x.Deleted == true).Select(x => x.BrandId).ToList();
                        var subcat = context.SubCategorys.Where(x => item.SellingBrandDCs.Contains(x.SubCategoryId) && x.Deleted == false).Select(z => z.SubcategoryName).ToList();
                        item.SubcategoryName = string.Join(",", subcat);


                        var documents = SupplierDocuments.Where(x => x.SupplierId == supId && x.SupplierTempId == item.id && x.IsActive == true && x.IsDeleted == false).ToList();
                        if (documents.Count > 0)
                        {
                            item.PanNumberImage = documents.Where(x => x.DocumentName == "PanCard").Select(y => y.DocumentPath).ToList();
                            item.AddressProofImage = documents.Where(x => x.DocumentName == "AddessProof").Select(y => y.DocumentPath).ToList();
                            item.AgreementImage = documents.Where(x => x.DocumentName == "Agreement").Select(y => y.DocumentPath).ToList();
                            item.ChequeImage = documents.Where(x => x.DocumentName == "BankAccount").Select(y => y.DocumentPath).ToList();
                            item.FSSAIImage = documents.Where(x => x.DocumentName == "FSSAI").Select(y => y.DocumentPath).ToList();
                            item.GSTImage = documents.Where(x => x.DocumentName == "GSTN").Select(y => y.DocumentPath).ToList();
                            item.ProprietorPanImage = documents.Where(x => x.DocumentName == "ProprietorPanCard").Select(y => y.DocumentPath).ToList();
                            item.MSMEImage = documents.Where(x => x.DocumentName == "MSME").Select(y => y.DocumentPath).ToList();
                        }



                        if (item.id == 0)
                        {
                            //&& x.Active == true
                            var oldSupplierList = dbSuppliers.Where(x => x.SupplierId == supId && x.Deleted == false).FirstOrDefault();
                            if (oldSupplierList != null)
                            {

                                if (oldSupplierList.PanCardImage != null || oldSupplierList.PanCardImage != "")
                                {
                                    List<string> panimage = new List<string>();
                                    panimage.Add(oldSupplierList.PanCardImage);
                                    //item.PanNumberImage.Add(oldSupplierList.PanCardImage);
                                    item.PanNumberImage = panimage;

                                }
                                if (oldSupplierList.AddressProofImage != null || oldSupplierList.AddressProofImage != "")
                                {
                                    List<string> addressimage = new List<string>();
                                    addressimage.Add(oldSupplierList.AddressProofImage);
                                    item.AddressProofImage = addressimage;
                                }
                                if (oldSupplierList.FSSAIImage != null || oldSupplierList.FSSAIImage != "")
                                {
                                    List<string> fssaiimage = new List<string>();
                                    fssaiimage.Add(oldSupplierList.FSSAIImage);
                                    item.FSSAIImage = fssaiimage;
                                }
                                if (oldSupplierList.GstImage != null || oldSupplierList.GstImage != "")
                                {
                                    List<string> gstimage = new List<string>();
                                    gstimage.Add(oldSupplierList.GstImage);
                                    item.GSTImage = gstimage;
                                }
                                if (oldSupplierList.MSMEImage != null || oldSupplierList.MSMEImage != "")
                                {
                                    List<string> msmeimage = new List<string>();
                                    msmeimage.Add(oldSupplierList.MSMEImage);
                                    item.MSMEImage = msmeimage;
                                }
                                item.GSTINNO = oldSupplierList.TINNo;
                                item.ExpiryDays = oldSupplierList.ExpiryDays;
                            }
                        }
                    }
                }
            }
            return SupplierOnBoardDCList;
        }


        public List<SupplierOnBoardDC> GetSupplierOnboardListbyFilter(string status, string Keyword, int userid = 0)
        {
            List<SupplierOnBoardDC> suppliers = new List<SupplierOnBoardDC>();

            using (var context = new AuthContext())
            {
                var paramStatus = new SqlParameter()
                {
                    ParameterName = "@Status",
                    Value = status
                };
                var paramuserid = new SqlParameter()
                {
                    ParameterName = "@userid",
                    Value = userid
                };
                var paramKeyword = new SqlParameter()
                {
                    ParameterName = "@userid",
                    Value = Keyword
                };

                suppliers = context.Database.SqlQuery<SupplierOnBoardDC>("GetSupplierOnboradList @Status,@userid", paramStatus, paramuserid).ToList();

                foreach (var item in suppliers)
                {
                    int supId = item.SupplierId == 0 ? item.id : item.SupplierId;
                    item.SellingBrandDCs = context.SupplierBrandMaps.Where(x => x.SupplierId == supId && x.Active == true && x.Deleted == false).Select(x => x.BrandId).ToList();
                    var subcat = context.SubCategorys.Where(x => item.SellingBrandDCs.Contains(x.SubCategoryId) && x.Deleted == false).Select(z => z.SubcategoryName).ToList();
                    item.SubcategoryName = string.Join(",", subcat);
                }
            }
            return suppliers;
        }


        public SupplierOnBoardDC GetSupplierOnboardByid(int id, int supplierid)
        {
            SupplierOnBoardDC onBordDC = new SupplierOnBoardDC();
            using (var context = new AuthContext())
            {
                var paramId = new SqlParameter()
                {
                    ParameterName = "@Id",
                    Value = id
                };

                var sparamId = new SqlParameter()
                {
                    ParameterName = "@SupplierId",
                    Value = supplierid
                };
                onBordDC = context.Database.SqlQuery<SupplierOnBoardDC>("GetSupplierOnboradDetailsbyid @Id,@SupplierId", paramId, sparamId).FirstOrDefault();
                if (onBordDC != null)
                {
                    int supId = onBordDC.SupplierId;
                    if (supId == 0 && onBordDC.id > 0)
                    {
                        var dbsupplierid = context.Suppliers.FirstOrDefault(x => x.SUPPLIERCODES == onBordDC.SUPPLIERCODES)?.SupplierId;
                        supId = dbsupplierid.HasValue ? dbsupplierid.Value : onBordDC.id;
                    }

                    onBordDC.SellingBrandDCs = context.SupplierBrandMaps.Where(x => x.SupplierId == supId && x.Active == true).Select(x => x.BrandId).ToList();
                    var subcat = context.SubCategorys.Where(x => onBordDC.SellingBrandDCs.Contains(x.SubCategoryId) && x.Deleted == false).Select(z => new { z.SubcategoryName, z.SubCategoryId }).ToList();
                    onBordDC.SubcategoryName = string.Join(",", subcat.Select(x => x.SubcategoryName));

                    var documents = context.SupplierDocumentDB.Where(x => x.SupplierId == supId && x.SupplierTempId == onBordDC.id && x.IsActive == true && x.IsDeleted == false).ToList();
                    if (documents.Count > 0)
                    {
                        onBordDC.PanNumberImage = documents.Where(x => x.DocumentName == "PanCard").Select(y => y.DocumentPath).ToList();
                        onBordDC.AddressProofImage = documents.Where(x => x.DocumentName == "AddessProof").Select(y => y.DocumentPath).ToList();
                        onBordDC.AgreementImage = documents.Where(x => x.DocumentName == "Agreement").Select(y => y.DocumentPath).ToList();
                        onBordDC.ChequeImage = documents.Where(x => x.DocumentName == "BankAccount").Select(y => y.DocumentPath).ToList();
                        onBordDC.FSSAIImage = documents.Where(x => x.DocumentName == "FSSAI").Select(y => y.DocumentPath).ToList();
                        onBordDC.GSTImage = documents.Where(x => x.DocumentName == "GSTN").Select(y => y.DocumentPath).ToList();
                        onBordDC.ProprietorPanImage = documents.Where(x => x.DocumentName == "ProprietorPanCard").Select(y => y.DocumentPath).ToList();
                        onBordDC.PanNumberImages = documents.Where(x => x.DocumentName == "PanCard").ToList();
                        onBordDC.AddressProofImages = documents.Where(x => x.DocumentName == "AddessProof").ToList();
                        onBordDC.AgreementImages = documents.Where(x => x.DocumentName == "Agreement").ToList();
                        onBordDC.ChequeImages = documents.Where(x => x.DocumentName == "BankAccount").ToList();
                        onBordDC.FSSAIImages = documents.Where(x => x.DocumentName == "FSSAI").ToList();
                        onBordDC.GSTImages = documents.Where(x => x.DocumentName == "GSTN").ToList();
                        onBordDC.ProprietorPanImages = documents.Where(x => x.DocumentName == "ProprietorPanCard").ToList();
                        onBordDC.MSMEImage = documents.Where(x => x.DocumentName == "MSME").Select(y => y.DocumentPath).ToList();
                        onBordDC.MSMEImages = documents.Where(x => x.DocumentName == "MSME").ToList();
                    }
                     if (onBordDC.id == 0)
                     {
                        //&& x.Active == true
                        var oldSupplierList = context.Suppliers.Where(x => x.SupplierId == supId  && x.Deleted == false).ToList();
                        if (oldSupplierList.Count > 0)
                        {
                            onBordDC.PanNumberImage = oldSupplierList.Any(x => x.PanCardImage != null) ? oldSupplierList.Where(x => x.PanCardImage != null).Select(y => y.PanCardImage).ToList() : new List<string>();
                            onBordDC.AddressProofImage = oldSupplierList.Any(x => x.AddressProofImage != null) ? oldSupplierList.Where(x => x.AddressProofImage != null).Select(y => y.AddressProofImage).ToList() : new List<string>();
                            onBordDC.FSSAIImage = oldSupplierList.Any(x => x.FSSAIImage != null) ? oldSupplierList.Where(x => x.FSSAIImage != null).Select(y => y.FSSAIImage).ToList() : new List<string>();
                            onBordDC.GSTImage = oldSupplierList.Any(x => x.GstImage != null) ? oldSupplierList.Where(x => x.GstImage != null).Select(y => y.GstImage).ToList() : new List<string>();
                            //var gstno = oldSupplierList.Any(x => x.TINNo != null)? oldSupplierList.Where(x => x.TINNo != null).Select(y => y.TINNo).ToList(): new List<string>();
                            onBordDC.GSTINNO = oldSupplierList.Any(x => x.TINNo != null) ? oldSupplierList.FirstOrDefault(x => x.TINNo != null).TINNo : "";
                        }
                    }

                }
            }
            return onBordDC;
        }


        public SupplierOnBoardDC GetSupplierOnboardBySupplierid(int id)
        {
            SupplierOnBoardDC onBordDC = new SupplierOnBoardDC();
            using (var context = new AuthContext())
            {
                var paramId = new SqlParameter()
                {
                    ParameterName = "@SupplierId",
                    Value = id
                };
                onBordDC = context.Database.SqlQuery<SupplierOnBoardDC>("GetSupplierOnboradDetailsbySupplierid @SupplierId", paramId).FirstOrDefault();
                if (onBordDC != null)
                {
                    int supId = onBordDC.SupplierId == 0 ? onBordDC.id : onBordDC.SupplierId;
                    onBordDC.SellingBrandDCs = context.SupplierBrandMaps.Where(x => x.SupplierId == supId && x.Active == true).Select(x => x.BrandId).ToList();
                    var subcat = context.SubCategorys.Where(x => onBordDC.SellingBrandDCs.Contains(x.SubCategoryId) && x.Deleted == false).Select(z => new { z.SubcategoryName, z.SubCategoryId }).ToList();
                    onBordDC.SubcategoryName = string.Join(",", subcat.Select(x => x.SubcategoryName));
                    var documents = context.SupplierDocumentDB.Where(x => x.SupplierId == supId && x.IsActive == true && x.IsDeleted == false).ToList();
                    if (documents.Count > 0)
                    {
                        onBordDC.PanNumberImage = documents.Where(x => x.DocumentName == "PanCard").Select(y => y.DocumentPath).ToList();
                        onBordDC.AddressProofImage = documents.Where(x => x.DocumentName == "AddessProof").Select(y => y.DocumentPath).ToList();
                        onBordDC.AgreementImage = documents.Where(x => x.DocumentName == "Agreement").Select(y => y.DocumentPath).ToList();
                        onBordDC.ChequeImage = documents.Where(x => x.DocumentName == "BankAccount").Select(y => y.DocumentPath).ToList();
                        onBordDC.FSSAIImage = documents.Where(x => x.DocumentName == "FSSAI").Select(y => y.DocumentPath).ToList();
                        onBordDC.GSTImage = documents.Where(x => x.DocumentName == "GSTN").Select(y => y.DocumentPath).ToList();
                        onBordDC.ProprietorPanImage = documents.Where(x => x.DocumentName == "ProprietorPanCard").Select(y => y.DocumentPath).ToList();
                    }
                }

            }
            return onBordDC;
        }

        public bool ActionOnSupplierOnboard(SupplierOnBoardActionDC actionDc)
        {
            bool result = false;
            try
            {
                using (var context = new AuthContext())
                {
                    List<object> parameters = new List<object>();
                    var idparam = new SqlParameter()
                    {
                        ParameterName = "@id",
                        Value = actionDc.id
                    };
                    parameters.Add(idparam);
                    var Supplieridparam = new SqlParameter()
                    {
                        ParameterName = "@SupplierID",
                        Value = actionDc.SupplierId
                    };
                    parameters.Add(Supplieridparam);
                    var Statusparam = new SqlParameter()
                    {
                        ParameterName = "@Status",
                        Value = actionDc.Status
                    };
                    parameters.Add(Statusparam);
                    var Commentsparam = new SqlParameter()
                    {
                        ParameterName = "@Comments",
                        Value = actionDc.Comments
                    };
                    parameters.Add(Commentsparam);

                    var useridparam = new SqlParameter()
                    {
                        ParameterName = "@UserId",
                        Value = actionDc.Userid
                    };
                    parameters.Add(useridparam);

                    var PaymentTermsparam = new SqlParameter()
                    {
                        ParameterName = "@PaymentTerms",
                        Value = actionDc.PaymentTerms
                    };
                    parameters.Add(PaymentTermsparam);

                    var SupplierTypeparam = new SqlParameter()
                    {
                        ParameterName = "@SupplierType",
                        Value = actionDc.SupplierType
                    };
                    parameters.Add(SupplierTypeparam);

                    var rowsaffected = context.Database.ExecuteSqlCommand("SupplierOnboardAction @id,@SupplierID,@Status,@Comments,@UserId,@SupplierType,@PaymentTerms ", parameters.ToArray());
                    if (rowsaffected > 0)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Action on new Supplier  " + ex.Message);
                throw ex;
            }
            return result;
        }
        public bool EditSupplierTemp(SupplierOnBoardDC supplierdc)
        {
            bool result = false; bool bransresult = false;
            int supplierid = 0;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope Scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                try
                {

                    supplierdc.IsLoanRequirement = (supplierdc.loanAmount > 0) ? "Yes" : "No";
                    using (var context = new AuthContext())
                    {

                        var supplier = context.SupplierTempDB.Where(x => x.Id == supplierdc.id && x.Status != "Pending").FirstOrDefault();
                        var cityname = context.Cities.Where(x => x.Cityid == supplierdc.Cityid && x.Deleted == false).FirstOrDefault()?.CityName;
                        var statename = context.Cities.Where(x => x.Stateid == supplierdc.Stateid && x.Deleted == false).FirstOrDefault()?.StateName;

                        if (supplier != null)
                        {
                            supplier.Name = supplierdc.SupplierName;
                            supplier.TypeofFirm = supplierdc.TypeofFirm;
                            supplier.OwnerName = supplierdc.ProprietorName;
                            supplier.Pancard = supplierdc.PanNumber;
                            supplier.PanCardImage = supplierdc.PanNumberImage != null && supplierdc.PanNumberImage.Any() ? supplierdc.PanNumberImage[0] : null;
                            supplier.AddressProofType = supplierdc.AddressProofType;
                            supplier.AddressProofImage = supplierdc.AddressProofImage != null && supplierdc.AddressProofImage.Any() ? supplierdc.AddressProofImage[0] : null;
                            supplier.EmailId = supplierdc.EmailId;
                            supplier.MobileNo = supplierdc.MobileNo;
                            supplier.CibilScore = supplierdc.CreditScore;
                            supplier.FatherName = supplierdc.FatherName;
                            supplier.IsLoanRequirement = supplierdc.IsLoanRequirement;
                            supplier.LoanAmount = supplierdc.loanAmount;
                            supplier.SupplierType = supplierdc.SupplierType;
                            supplier.EstablishmentYear = supplierdc.BusinessEstablishmentYear;
                            supplier.GstInNumber = supplierdc.GSTINNO;
                            supplier.TINNo = supplierdc.GSTINNO;
                            supplier.FSSAI = supplierdc.FSSAINO;
                            supplier.FSSAIImage = supplierdc.FSSAIImage != null && supplierdc.FSSAIImage.Any() ? supplierdc.FSSAIImage[0] : null;
                            supplier.SupplierAddress = supplierdc.FullBusinessAddress;
                            supplier.HeadOffice = supplierdc.HeadOffice;
                            supplier.Pincode = supplierdc.PinCode;
                            supplier.BankAccountType = supplierdc.BankAccType;
                            supplier.Bank_AC_No = supplierdc.BankAccNo;
                            supplier.BankAddress = supplierdc.BankAddress;
                            supplier.Bank_Name = supplierdc.BankName;
                            supplier.Bank_Ifsc = supplierdc.IFSCCode;
                            //supplier.Deleted = false;
                            //supplier.Active = true;
                            //---done by anjali
                            supplier.Deleted = supplierdc.Deleted;
                            supplier.Active = supplierdc.Active;
                            supplier.IsVerified = supplierdc.IsVerified;
                            supplier.Comments = supplierdc.Comments;
                            //
                            supplier.BankPINno = supplierdc.bankPinCode;
                            supplier.proprietorphonenumber = supplierdc.proprietorphonenumber;
                            supplier.Status = supplierStatus.Pending.ToString();
                            supplier.SUPPLIERCODES = supplierdc.SUPPLIERCODES;

                            supplier.IsStopAdvancePr = supplierdc.IsStopAdvancePr;
                            supplier.IsIRNInvoiceRequired = supplierdc.IsIRNInvoiceRequired;
                            supplier.Password = supplierdc.Password;
                            supplier.WebUrl = supplierdc.WebUrl;
                            supplier.ContactPerson = supplierdc.ContactPerson;
                            supplier.bussinessType = supplierdc.bussinessType;
                            supplier.StartedBusiness = supplierdc.StartedBusiness;
                            supplier.PaymentTerms = supplierdc.PaymentTerms;
                            // supplier.CompanyId = supplierdc.CompanyId;

                            supplier.ContactPersonMobileNo = supplierdc.ContactPersonMobileNo;
                            supplier.SupplierSegment = supplierdc.SupplierSegment;
                            supplier.GstImage = supplierdc.GSTImage != null && supplierdc.GSTImage.Any() ? supplierdc.GSTImage[0] : null;
                            supplier.ChequeImageUrl = supplierdc.ChequeImage != null && supplierdc.ChequeImage.Any() ? supplierdc.ChequeImage[0] : null;


                            supplier.City = cityname;
                            supplier.Cityid = supplierdc.Cityid;
                            supplier.Stateid = supplierdc.Stateid;
                            supplier.StateName = statename;

                            supplier.ExpiryDays = supplierdc.ExpiryDays;
                            supplier.ModifyBy = supplierdc.userid;
                            supplier.ProprietorPanNumber = supplierdc.ProprietorPanNumber;
                            supplier.MSMEType = supplierdc.MSMEType;
                            supplier.MSMEImage = supplierdc.MSMEImage != null && supplierdc.MSMEImage.Any() ? supplierdc.MSMEImage[0] : null;
                            supplier.BuyerId = supplierdc.BuyerId;
                            context.Entry(supplier).State = System.Data.Entity.EntityState.Modified;

                            if (context.Commit() > 0)
                            {

                                supplierid = supplierdc.SupplierId == 0 ? supplierdc.id : supplierdc.SupplierId;
                                if (supplierdc.SellingBrandDCs.Count == 0)
                                {
                                    var Remove = context.SupplierBrandMaps.Where(x => x.SupplierId == supplierid).ToList();
                                    if (Remove.Count != 0)
                                    {
                                        foreach (var b in Remove)

                                        {
                                            b.Active = false;
                                            b.Deleted = true;
                                            context.Entry(b).State = System.Data.Entity.EntityState.Modified;
                                            bransresult = true;
                                        }
                                    }
                                    else
                                    {
                                        bransresult = true;
                                    }
                                }
                                else
                                {

                                    var Remove = context.SupplierBrandMaps.Where(x => x.SupplierId == supplierid).ToList();
                                    if (Remove.Count != 0)
                                    {
                                        foreach (var b in Remove)

                                        {
                                            b.Active = false;
                                            b.Deleted = true;
                                            context.Entry(b).State = System.Data.Entity.EntityState.Modified;
                                            bransresult = true;
                                        }
                                    }
                                    foreach (var a in supplierdc.SellingBrandDCs)
                                    {
                                        //var IsAdd = context.SupplierBrandMaps.Where(x => x.BrandId == a.id && x.SupplierId == supplier.SupplierId && x.Deleted == false && x.Active == true).FirstOrDefault();
                                        //if (IsAdd == null)
                                        //{
                                        var supplierbrandmap = new SupplierBrandMap();
                                        supplierbrandmap.BrandId = a;
                                        supplierbrandmap.SupplierId = supplierid;
                                        supplierbrandmap.Active = true;
                                        supplierbrandmap.Deleted = true;
                                        context.SupplierBrandMaps.Add(supplierbrandmap);
                                        context.Commit();
                                        bransresult = true;
                                        //}
                                    }
                                }


                                #region Image insert 
                                if (supplierdc.PanNumberImage != null && supplierdc.PanNumberImage.Any())
                                {
                                    var panimg = context.SupplierDocumentDB.Where(x => x.SupplierId == supplierid && x.SupplierTempId == supplier.Id && x.DocumentName == "PanCard").ToList();
                                    if (panimg.Count > 0)
                                    {
                                        foreach (var item in panimg)
                                        {
                                            item.IsDeleted = true;
                                            item.IsActive = false;
                                            context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                        }
                                        context.Commit();
                                    }
                                    List<SupplierDocument> panimglist = new List<SupplierDocument>();
                                    foreach (var item in supplierdc.PanNumberImage)
                                    {
                                        SupplierDocument doc = new SupplierDocument()
                                        {
                                            SupplierTempId= supplier.Id,
                                            SupplierId = supplierid,
                                            DocumentPath = item,
                                            DocumentName = "PanCard",
                                            CreatedBy = supplier.Createdby,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        panimglist.Add(doc);
                                    }
                                    context.SupplierDocumentDB.AddRange(panimglist);
                                    context.Commit();
                                }
                                if (supplierdc.AddressProofImage != null && supplierdc.AddressProofImage.Any())
                                {
                                    var Addimg = context.SupplierDocumentDB.Where(x => x.SupplierId == supplierid && x.SupplierTempId == supplier.Id && x.DocumentName == "AddessProof").ToList();
                                    if (Addimg.Count > 0)
                                    {
                                        foreach (var item in Addimg)
                                        {
                                            item.IsDeleted = true;
                                            item.IsActive = false;
                                            context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                        }
                                        context.Commit();
                                    }
                                    List<SupplierDocument> addimglist = new List<SupplierDocument>();
                                    foreach (var item in supplierdc.AddressProofImage)
                                    {
                                        SupplierDocument doc = new SupplierDocument()
                                        {
                                            SupplierTempId = supplier.Id,
                                            SupplierId = supplierid,
                                            DocumentPath = item,
                                            DocumentName = "AddessProof",
                                            CreatedBy = supplier.Createdby,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.SupplierDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }
                                if (supplierdc.FSSAIImage != null && supplierdc.FSSAIImage.Any())
                                {
                                    var fssaiimg = context.SupplierDocumentDB.Where(x => x.SupplierId == supplierid && x.SupplierTempId == supplier.Id && x.DocumentName == "FSSAI").ToList();
                                    if (fssaiimg.Count > 0)
                                    {
                                        foreach (var item in fssaiimg)
                                        {
                                            item.IsDeleted = true;
                                            item.IsActive = false;
                                            context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                        }
                                        context.Commit();
                                    }
                                    List<SupplierDocument> addimglist = new List<SupplierDocument>();
                                    foreach (var item in supplierdc.FSSAIImage)
                                    {
                                        SupplierDocument doc = new SupplierDocument()
                                        {
                                            SupplierTempId = supplier.Id,
                                            SupplierId = supplierid,
                                            DocumentPath = item,
                                            DocumentName = "FSSAI",
                                            CreatedBy = supplier.Createdby,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.SupplierDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }
                                if (supplierdc.ChequeImage != null && supplierdc.ChequeImage.Any())
                                {
                                    var bankimg = context.SupplierDocumentDB.Where(x => x.SupplierId == supplierid && x.SupplierTempId == supplier.Id && x.DocumentName == "BankAccount").ToList();
                                    if (bankimg.Count > 0)
                                    {
                                        foreach (var item in bankimg)
                                        {
                                            item.IsDeleted = true;
                                            item.IsActive = false;
                                            context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                        }
                                        context.Commit();
                                    }
                                    List<SupplierDocument> addimglist = new List<SupplierDocument>();
                                    foreach (var item in supplierdc.ChequeImage)
                                    {
                                        SupplierDocument doc = new SupplierDocument()
                                        {
                                            SupplierTempId = supplier.Id,
                                            SupplierId = supplierid,
                                            DocumentPath = item,
                                            DocumentName = "BankAccount",
                                            CreatedBy = supplier.Createdby,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.SupplierDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }
                                if (supplierdc.GSTImage != null && supplierdc.GSTImage.Any())
                                {
                                    var gstimg = context.SupplierDocumentDB.Where(x => x.SupplierId == supplierid && x.SupplierTempId == supplier.Id && x.DocumentName == "GSTN").ToList();
                                    if (gstimg.Count > 0)
                                    {
                                        foreach (var item in gstimg)
                                        {
                                            item.IsDeleted = true;
                                            item.IsActive = false;
                                            context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                        }
                                        context.Commit();
                                    }
                                    List<SupplierDocument> addimglist = new List<SupplierDocument>();
                                    foreach (var item in supplierdc.GSTImage)
                                    {
                                        SupplierDocument doc = new SupplierDocument()
                                        {
                                            SupplierTempId = supplier.Id,
                                            SupplierId = supplierid,
                                            DocumentPath = item,
                                            DocumentName = "GSTN",
                                            CreatedBy = supplier.Createdby,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.SupplierDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }
                                if (supplierdc.AgreementImage != null && supplierdc.AgreementImage.Any())
                                {
                                    var aggimg = context.SupplierDocumentDB.Where(x => x.SupplierId == supplierid && x.SupplierTempId == supplier.Id && x.DocumentName == "Agreement").ToList();
                                    if (aggimg.Count > 0)
                                    {
                                        foreach (var item in aggimg)
                                        {
                                            item.IsDeleted = true;
                                            item.IsActive = false;
                                            context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                        }
                                        context.Commit();
                                    }
                                    List<SupplierDocument> addimglist = new List<SupplierDocument>();
                                    foreach (var item in supplierdc.AgreementImage)
                                    {
                                        SupplierDocument doc = new SupplierDocument()
                                        {
                                            SupplierTempId = supplier.Id,
                                            SupplierId = supplierid,
                                            DocumentPath = item,
                                            DocumentName = "Agreement",
                                            CreatedBy = supplier.Createdby,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.SupplierDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }
                                if (supplierdc.ProprietorPanImage != null && supplierdc.ProprietorPanImage.Any())
                                {
                                    var proppanimg = context.SupplierDocumentDB.Where(x => x.SupplierId == supplierid && x.SupplierTempId == supplier.Id && x.DocumentName == "ProprietorPanCard").ToList();
                                    if (proppanimg.Count > 0)
                                    {
                                        foreach (var item in proppanimg)
                                        {
                                            item.IsDeleted = true;
                                            item.IsActive = false;
                                            context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                        }
                                        context.Commit();
                                    }
                                    List<SupplierDocument> addimglist = new List<SupplierDocument>();
                                    foreach (var item in supplierdc.ProprietorPanImage)
                                    {
                                        SupplierDocument doc = new SupplierDocument()
                                        {
                                            SupplierTempId = supplier.Id,
                                            SupplierId = supplierid,
                                            DocumentPath = item,
                                            DocumentName = "ProprietorPanCard",
                                            CreatedBy = supplier.Createdby,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.SupplierDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }
                                var msmeimg = context.SupplierDocumentDB.Where(x => x.SupplierId == supplierid && x.SupplierTempId == supplier.Id && x.DocumentName == "MSME").ToList();
                                if (msmeimg.Count > 0)
                                {
                                    foreach (var item in msmeimg)
                                    {
                                        item.IsDeleted = true;
                                        item.IsActive = false;
                                        context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    context.Commit();
                                }
                                if (supplierdc.MSMEImage != null && supplierdc.MSMEImage.Any())     //MSME
                                {
                                    List<SupplierDocument> msmeimglist = new List<SupplierDocument>();
                                    foreach (var item in supplierdc.MSMEImage)
                                    {
                                        SupplierDocument doc = new SupplierDocument()
                                        {
                                            SupplierTempId = supplier.Id,
                                            SupplierId = supplierid,
                                            DocumentPath = item,
                                            DocumentName = "MSME",
                                            CreatedBy = supplier.Createdby,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        msmeimglist.Add(doc);
                                    }
                                    context.SupplierDocumentDB.AddRange(msmeimglist);
                                    context.Commit();
                                }
                                #endregion

                                result = true;
                            }
                            #region Add Hisotry 
                            SupplierOnboardHistory history = new SupplierOnboardHistory()
                            {
                                Suppliercode = supplierdc.SUPPLIERCODES,
                                Comments = "Supplier Edited",
                                CreatedBy = supplierdc.userid,
                                CreatedDate = DateTime.Now,
                                IsActive = true,
                                IsDeleted = false,
                                SupplierId = supplierdc.SupplierId,
                                Status = supplierStatus.Pending.ToString()
                            };
                            InsertSupplierOnboardHistory(history);
                            #endregion
                        }
                        else
                        {
                            supplier = new SupplierTemp();
                            supplier.Name = supplierdc.SupplierName;
                            supplier.TypeofFirm = supplierdc.TypeofFirm;
                            supplier.OwnerName = supplierdc.ProprietorName;
                            supplier.Pancard = supplierdc.PanNumber;
                            supplier.PanCardImage = supplierdc.PanNumberImage != null && supplierdc.PanNumberImage.Any() ? supplierdc.PanNumberImage[0] : null;
                            supplier.AddressProofType = supplierdc.AddressProofType;
                            supplier.AddressProofImage = supplierdc.AddressProofImage != null && supplierdc.AddressProofImage.Any() ? supplierdc.AddressProofImage[0] : null;
                            supplier.EmailId = supplierdc.EmailId;
                            supplier.MobileNo = supplierdc.MobileNo;
                            supplier.CibilScore = supplierdc.CreditScore;
                            supplier.FatherName = supplierdc.FatherName;
                            supplier.IsLoanRequirement = supplierdc.IsLoanRequirement;
                            supplier.LoanAmount = supplierdc.loanAmount;
                            supplier.SupplierType = supplierdc.SupplierType;
                            supplier.EstablishmentYear = supplierdc.BusinessEstablishmentYear;
                            supplier.GstInNumber = supplierdc.GSTINNO;
                            supplier.TINNo = supplierdc.GSTINNO;
                            supplier.FSSAI = supplierdc.FSSAINO;
                            supplier.FSSAIImage = supplierdc.FSSAIImage != null && supplierdc.FSSAIImage.Any() ? supplierdc.FSSAIImage[0] : null;
                            supplier.SupplierAddress = supplierdc.FullBusinessAddress;
                            supplier.HeadOffice = supplierdc.HeadOffice;
                            supplier.Pincode = supplierdc.PinCode;
                            supplier.BankAccountType = supplierdc.BankAccType;
                            supplier.Bank_AC_No = supplierdc.BankAccNo;
                            supplier.BankAddress = supplierdc.BankAddress;
                            supplier.Bank_Name = supplierdc.BankName;
                            supplier.Bank_Ifsc = supplierdc.IFSCCode;
                            supplier.Createdby = supplierdc.userid;
                            supplier.CreatedDate = DateTime.Now;
                            //supplier.Deleted = false;
                            //supplier.Active = true;
                            //---done by anjali
                            supplier.Deleted = supplierdc.Deleted;
                            supplier.Active = supplierdc.Active;
                            supplier.IsVerified = supplierdc.IsVerified;
                            supplier.Comments = supplierdc.Comments;
                            supplier.SupplierId = supplierdc.SupplierId;
                            //
                            supplier.BankPINno = supplierdc.bankPinCode;
                            supplier.proprietorphonenumber = supplierdc.proprietorphonenumber;
                            supplier.Status = supplierStatus.Pending.ToString();
                            supplier.SUPPLIERCODES = supplierdc.SUPPLIERCODES;

                            supplier.IsStopAdvancePr = supplierdc.IsStopAdvancePr;
                            supplier.IsIRNInvoiceRequired = supplierdc.IsIRNInvoiceRequired;
                            supplier.Password = supplierdc.Password;
                            supplier.WebUrl = supplierdc.WebUrl;
                            supplier.ContactPerson = supplierdc.ContactPerson;
                            supplier.bussinessType = supplierdc.bussinessType;
                            supplier.StartedBusiness = supplierdc.StartedBusiness;
                            supplier.PaymentTerms = supplierdc.PaymentTerms;
                            // supplier.CompanyId = supplierdc.CompanyId;

                            supplier.ContactPersonMobileNo = supplierdc.ContactPersonMobileNo;
                            supplier.SupplierSegment = supplierdc.SupplierSegment;
                            supplier.GstImage = supplierdc.GSTImage != null && supplierdc.GSTImage.Any() ? supplierdc.GSTImage[0] : null;
                            supplier.ChequeImageUrl = supplierdc.ChequeImage != null && supplierdc.ChequeImage.Any() ? supplierdc.ChequeImage[0] : null;


                            supplier.City = cityname;
                            supplier.Cityid = supplierdc.Cityid;
                            supplier.Stateid = supplierdc.Stateid;
                            supplier.StateName = statename;

                            supplier.ExpiryDays = supplierdc.ExpiryDays;
                            supplier.CreatedDate = DateTime.Now;
                            supplier.Createdby = supplierdc.userid;
                            supplier.ProprietorPanNumber = supplierdc.ProprietorPanNumber;
                            context.SupplierTempDB.Add(supplier);

                            if (context.Commit() > 0)
                            {
                                supplierid = supplierdc.SupplierId == 0 ? supplierdc.id : supplierdc.SupplierId;
                                if (supplierdc.SellingBrandDCs.Count == 0)
                                {
                                    var Remove = context.SupplierBrandMaps.Where(x => x.SupplierId == supplierid).ToList();
                                    if (Remove.Count != 0)
                                    {
                                        foreach (var b in Remove)

                                        {
                                            b.Active = false;
                                            b.Deleted = true;
                                            context.Entry(b).State = System.Data.Entity.EntityState.Modified;
                                            bransresult = true;
                                        }
                                    }
                                    else
                                    {
                                        bransresult = true;
                                    }
                                }
                                else
                                {

                                    var Remove = context.SupplierBrandMaps.Where(x => x.SupplierId == supplierid).ToList();
                                    if (Remove.Count != 0)
                                    {
                                        foreach (var b in Remove)

                                        {
                                            b.Active = false;
                                            b.Deleted = true;
                                            context.Entry(b).State = System.Data.Entity.EntityState.Modified;
                                            bransresult = true;
                                        }
                                    }
                                    foreach (var a in supplierdc.SellingBrandDCs)
                                    {
                                        //var IsAdd = context.SupplierBrandMaps.Where(x => x.BrandId == a.id && x.SupplierId == supplier.SupplierId && x.Deleted == false && x.Active == true).FirstOrDefault();
                                        //if (IsAdd == null)
                                        //{
                                        var supplierbrandmap = new SupplierBrandMap();
                                        supplierbrandmap.BrandId = a;
                                        supplierbrandmap.SupplierId = supplierid;
                                        supplierbrandmap.Active = true;
                                        supplierbrandmap.Deleted = true;
                                        context.SupplierBrandMaps.Add(supplierbrandmap);
                                        context.Commit();
                                        bransresult = true;
                                        //}
                                    }
                                }


                                #region Image insert 

                                var panimg = context.SupplierDocumentDB.Where(x => x.SupplierId == supplierid && x.DocumentName == "PanCard").ToList();
                                if (panimg.Count > 0)
                                {
                                    foreach (var item in panimg)
                                    {
                                        item.IsDeleted = true;
                                        item.IsActive = false;
                                        context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    context.Commit();
                                }
                                List<SupplierDocument> panimglist = new List<SupplierDocument>();
                                if (supplierdc.PanNumberImage != null && supplierdc.PanNumberImage.Any())
                                {
                                    foreach (var item in supplierdc.PanNumberImage)
                                    {
                                        SupplierDocument doc = new SupplierDocument()
                                        {
                                            SupplierId = supplierid,
                                            DocumentPath = item,
                                            DocumentName = "PanCard",
                                            CreatedBy = supplier.Createdby,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        panimglist.Add(doc);
                                    }
                                    context.SupplierDocumentDB.AddRange(panimglist);
                                    context.Commit();
                                }

                                var Addimg = context.SupplierDocumentDB.Where(x => x.SupplierId == supplierid && x.DocumentName == "AddessProof").ToList();
                                if (Addimg.Count > 0)
                                {
                                    foreach (var item in Addimg)
                                    {
                                        item.IsDeleted = true;
                                        item.IsActive = false;
                                        context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    context.Commit();
                                }
                                List<SupplierDocument> addimglist = new List<SupplierDocument>();
                                if (supplierdc.AddressProofImage != null && supplierdc.AddressProofImage.Any())
                                {
                                    foreach (var item in supplierdc.AddressProofImage)
                                    {
                                        SupplierDocument doc = new SupplierDocument()
                                        {
                                            SupplierId = supplierid,
                                            DocumentPath = item,
                                            DocumentName = "AddessProof",
                                            CreatedBy = supplier.Createdby,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.SupplierDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }

                                var fssaiimg = context.SupplierDocumentDB.Where(x => x.SupplierId == supplierid && x.DocumentName == "FSSAI").ToList();
                                if (fssaiimg.Count > 0)
                                {
                                    foreach (var item in fssaiimg)
                                    {
                                        item.IsDeleted = true;
                                        item.IsActive = false;
                                        context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    context.Commit();
                                }
                                addimglist = new List<SupplierDocument>();
                                if (supplierdc.FSSAIImage != null && supplierdc.FSSAIImage.Any())
                                {
                                    foreach (var item in supplierdc.FSSAIImage)
                                    {
                                        SupplierDocument doc = new SupplierDocument()
                                        {
                                            SupplierId = supplierid,
                                            DocumentPath = item,
                                            DocumentName = "FSSAI",
                                            CreatedBy = supplier.Createdby,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.SupplierDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }

                                var bankimg = context.SupplierDocumentDB.Where(x => x.SupplierId == supplierid && x.DocumentName == "BankAccount").ToList();
                                if (bankimg.Count > 0)
                                {
                                    foreach (var item in bankimg)
                                    {
                                        item.IsDeleted = true;
                                        item.IsActive = false;
                                        context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    context.Commit();
                                }
                                addimglist = new List<SupplierDocument>();
                                if (supplierdc.ChequeImage != null && supplierdc.ChequeImage.Any())
                                {
                                    foreach (var item in supplierdc.ChequeImage)
                                    {
                                        SupplierDocument doc = new SupplierDocument()
                                        {
                                            SupplierId = supplierid,
                                            DocumentPath = item,
                                            DocumentName = "BankAccount",
                                            CreatedBy = supplier.Createdby,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.SupplierDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }

                                var gstimg = context.SupplierDocumentDB.Where(x => x.SupplierId == supplierid && x.DocumentName == "GSTN").ToList();
                                if (gstimg.Count > 0)
                                {
                                    foreach (var item in gstimg)
                                    {
                                        item.IsDeleted = true;
                                        item.IsActive = false;
                                        context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    context.Commit();
                                }
                                addimglist = new List<SupplierDocument>();
                                if (supplierdc.GSTImage != null && supplierdc.GSTImage.Any())
                                {
                                    foreach (var item in supplierdc.GSTImage)
                                    {
                                        SupplierDocument doc = new SupplierDocument()
                                        {
                                            SupplierId = supplierid,
                                            DocumentPath = item,
                                            DocumentName = "GSTN",
                                            CreatedBy = supplier.Createdby,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.SupplierDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }

                                var aggimg = context.SupplierDocumentDB.Where(x => x.SupplierId == supplierid && x.DocumentName == "Agreement").ToList();
                                if (aggimg.Count > 0)
                                {
                                    foreach (var item in aggimg)
                                    {
                                        item.IsDeleted = true;
                                        item.IsActive = false;
                                        context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    context.Commit();
                                }
                                addimglist = new List<SupplierDocument>();
                                if (supplierdc.AgreementImage != null && supplierdc.AgreementImage.Any())
                                {
                                    foreach (var item in supplierdc.AgreementImage)
                                    {
                                        SupplierDocument doc = new SupplierDocument()
                                        {
                                            SupplierId = supplierid,
                                            DocumentPath = item,
                                            DocumentName = "Agreement",
                                            CreatedBy = supplier.Createdby,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.SupplierDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }


                                var proppanimg = context.SupplierDocumentDB.Where(x => x.SupplierId == supplierid && x.DocumentName == "ProprietorPanCard").ToList();
                                if (proppanimg.Count > 0)
                                {
                                    foreach (var item in proppanimg)
                                    {
                                        item.IsDeleted = true;
                                        item.IsActive = false;
                                        context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    context.Commit();
                                }
                                addimglist = new List<SupplierDocument>();
                                if (supplierdc.ProprietorPanImage != null && supplierdc.ProprietorPanImage.Any())
                                {
                                    foreach (var item in supplierdc.ProprietorPanImage)
                                    {
                                        SupplierDocument doc = new SupplierDocument()
                                        {
                                            SupplierId = supplierid,
                                            DocumentPath = item,
                                            DocumentName = "ProprietorPanCard",
                                            CreatedBy = supplier.Createdby,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.SupplierDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }
                                #endregion

                                result = true;
                            }
                            #region Add Hisotry 
                            SupplierOnboardHistory history = new SupplierOnboardHistory()
                            {
                                Suppliercode = supplierdc.SUPPLIERCODES,
                                Comments = "Supplier Edited",
                                CreatedBy = supplierdc.userid,
                                CreatedDate = DateTime.Now,
                                IsActive = true,
                                IsDeleted = false,
                                SupplierId = supplierdc.SupplierId,
                                Status = supplierStatus.Pending.ToString()
                            };
                            InsertSupplierOnboardHistory(history);
                            #endregion
                        }

                    }
                    if (result && bransresult)
                    {
                        Scope.Complete();
                    }
                    else
                    {
                        Scope.Dispose();
                    }
                }

                catch (Exception ex)
                {
                    logger.Error("Error in Add new Supplier  " + ex.Message);
                    Scope.Dispose();
                    throw ex;
                }
            }

            return result;

        }


        public bool ActiveDecativeSupplier(SupplierOnBoardActionDC actionDc)
        {
            bool result = false;

            try
            {
                using (var context = new AuthContext())
                {
                    //SupplierOnboardActiveDective
                    List<object> parameters = new List<object>();

                    var Supplieridparam = new SqlParameter()
                    {
                        ParameterName = "@SupplierID",
                        Value = actionDc.SupplierId
                    };
                    parameters.Add(Supplieridparam);

                    var Commentsparam = new SqlParameter()
                    {
                        ParameterName = "@Comment",
                        Value = actionDc.Comments
                    };
                    parameters.Add(Commentsparam);

                    var useridparam = new SqlParameter()
                    {
                        ParameterName = "@UserId",
                        Value = actionDc.Userid
                    };
                    parameters.Add(useridparam);

                    var activetypeparam = new SqlParameter()
                    {
                        ParameterName = "@ActiveType",
                        Value = actionDc.ActiveType
                    };
                    parameters.Add(activetypeparam);


                    var rowsaffected = context.Database.ExecuteSqlCommand("SupplierOnboardActiveDective @SupplierID,@ActiveType,@UserId,@Comment ", parameters.ToArray());
                    if (rowsaffected > 0)
                        result = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in ActiveDecativeSupplier on new Supplier  " + ex.Message);
                throw ex;
            }

            return result;
        }

        public bool InsertSupplierOnboardHistory(SupplierOnboardHistory history)
        {
            bool hisresult = false;
            try
            {
                using (var context = new AuthContext())
                {
                    context.SupplierOnboardHistoryDB.Add(history);
                    if (context.Commit() > 0)
                        hisresult = true;
                }
            }

            catch (Exception ex)
            {
                logger.Error("Error in InsertSupplierOnboardHistory on new Supplier  " + ex.Message);
            }
            return hisresult;
        }
        public List<SupplierOnboardHisotryDC> GetSupplierOnboardHisotry(int Supplierid, string SupplierCode)
        {
            List<SupplierOnboardHisotryDC> suppliers = new List<SupplierOnboardHisotryDC>();

            using (var context = new AuthContext())
            {
                var paramSupplierid = new SqlParameter()
                {
                    ParameterName = "@supplierID",
                    Value = Supplierid
                };
                var paramSupplierCode = new SqlParameter()
                {
                    ParameterName = "@SupplierCode",
                    Value = SupplierCode
                };

                suppliers = context.Database.SqlQuery<SupplierOnboardHisotryDC>("GetSupplierOnboardHisotry @supplierID,@SupplierCode", paramSupplierid, paramSupplierCode).ToList();

            }
            return suppliers;
        }
        public bool AddNewDepo(DepoOnBoardingDC depodc)
        {
            bool result = false;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope Scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                try
                {
                    using (var context = new AuthContext())
                    {
                        var supplier = context.Suppliers.Where(x => x.SupplierId == depodc.SupplierId && x.Deleted == false).FirstOrDefault();


                        var cityname = context.Cities.Where(x => x.Cityid == depodc.Cityid && x.Deleted == false).FirstOrDefault().CityName;
                        var statename = context.Cities.Where(x => x.Stateid == depodc.Stateid && x.Deleted == false).FirstOrDefault().StateName;

                        DepoMasterTemp demo = new DepoMasterTemp()
                        {
                            Stateid = depodc.Stateid,
                            StateName = statename,
                            Cityid = depodc.Cityid,
                            CityName = cityname,
                            DepoName = depodc.DepoName,
                            GSTin = depodc.GSTin,
                            DepoCodes = depodc.DepoCodes,
                            Address = depodc.Address,
                            Email = depodc.Email,
                            PhoneNumber = depodc.PhoneNumber,
                            ContactPerson = depodc.ContactPerson,
                            FSSAI = depodc.FSSAI,
                            CityPincode = depodc.CityPincode,
                            Bank_Name = depodc.Bank_Name,
                            Bank_AC_No = depodc.Bank_AC_No,
                            BankAddress = depodc.BankAddress,
                            Bank_Ifsc = depodc.Bank_Ifsc,
                            BankPinCode = depodc.BankPinCode,
                            //
                            BankAccountType = depodc.BankAccountType,
                            //
                            PANCardNo = depodc.PANCardNo,
                            OpeningHours = depodc.OpeningHours,
                            PRPOStopAfterValue = depodc.PRPOStopAfterValue,
                            GstImage = depodc.GstImage != null && depodc.GstImage.Any() ? depodc.GstImage[0] : null,
                            FSSAIImage = depodc.FSSAIImage != null && depodc.FSSAIImage.Any() ? depodc.FSSAIImage[0] : null,
                            PanCardImage = depodc.PanCardImage != null && depodc.PanCardImage.Any() ? depodc.PanCardImage[0] : null,
                            CancelCheque = depodc.CancelCheque != null && depodc.CancelCheque.Any() ? depodc.CancelCheque[0] : null,
                            Status = supplierStatus.Pending.ToString(),
                            SupplierCode = supplier.SUPPLIERCODES,
                            SupplierName = supplier.Name,
                            SupplierId = supplier.SupplierId,
                            DepoId = 0,
                            CreatedDate = DateTime.Now,
                            IsActive = true,
                            Deleted = false,
                            CreatedBy = Convert.ToString(depodc.CreatedBy)

                        };

                        context.DepoMasterTempDB.Add(demo);

                        if (context.Commit() > 0)
                        {
                            if (depodc.GstImage != null && depodc.GstImage.Any())
                            {
                                List<DepoDocument> addimglist = new List<DepoDocument>();
                                foreach (var item in depodc.GstImage)
                                {
                                    DepoDocument doc = new DepoDocument()
                                    {
                                        SupplierId = supplier.SupplierId,
                                        DepoId = demo.Id,
                                        DocumentPath = item,
                                        DocumentName = "GST",
                                        CreatedBy = Convert.ToInt32(depodc.CreatedBy),
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreateDate = DateTime.Now
                                    };
                                    addimglist.Add(doc);
                                }
                                context.DepoDocumentDB.AddRange(addimglist);
                                context.Commit();
                            }
                            if (depodc.FSSAIImage != null && depodc.FSSAIImage.Any())
                            {
                                List<DepoDocument> addimglist = new List<DepoDocument>();
                                foreach (var item in depodc.FSSAIImage)
                                {
                                    DepoDocument doc = new DepoDocument()
                                    {
                                        SupplierId = supplier.SupplierId,
                                        DepoId = demo.Id,
                                        DocumentPath = item,
                                        DocumentName = "FSSAI",
                                        CreatedBy = Convert.ToInt32(depodc.CreatedBy),
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreateDate = DateTime.Now
                                    };
                                    addimglist.Add(doc);
                                }
                                context.DepoDocumentDB.AddRange(addimglist);
                                context.Commit();
                            }
                            if (depodc.PanCardImage != null && depodc.PanCardImage.Any())
                            {
                                List<DepoDocument> addimglist = new List<DepoDocument>();
                                foreach (var item in depodc.PanCardImage)
                                {
                                    DepoDocument doc = new DepoDocument()
                                    {
                                        SupplierId = supplier.SupplierId,
                                        DepoId = demo.Id,
                                        DocumentPath = item,
                                        DocumentName = "PanCard",
                                        CreatedBy = Convert.ToInt32(depodc.CreatedBy),
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreateDate = DateTime.Now
                                    };
                                    addimglist.Add(doc);
                                }
                                context.DepoDocumentDB.AddRange(addimglist);
                                context.Commit();
                            }
                            if (depodc.CancelCheque != null && depodc.CancelCheque.Any())
                            {
                                List<DepoDocument> addimglist = new List<DepoDocument>();
                                foreach (var item in depodc.CancelCheque)
                                {
                                    DepoDocument doc = new DepoDocument()
                                    {
                                        SupplierId = supplier.SupplierId,
                                        DepoId = demo.Id,
                                        DocumentPath = item,
                                        DocumentName = "CancelCheque",
                                        CreatedBy = Convert.ToInt32(depodc.CreatedBy),
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreateDate = DateTime.Now
                                    };
                                    addimglist.Add(doc);
                                }
                                context.DepoDocumentDB.AddRange(addimglist);
                                context.Commit();
                            }
                            result = true;
                        }

                    }
                    if (result)
                    {
                        Scope.Complete();
                    }
                    else
                    {
                        Scope.Dispose();
                    }
                }

                catch (Exception ex)
                {
                    logger.Error("Error in Add new Depo  " + ex.Message);
                    Scope.Dispose();
                    throw ex;
                }
            }
            return result;
        }
        public DepoOnBoardingDCList GetDepoOnboardList(string status, string KeyWord, int skip, int take, int userid = 0)
        {
            DepoOnBoardingDCList DepoOnBoardingDCs = new DepoOnBoardingDCList();
            //List<DepoOnBoardingDC> depos = new List<DepoOnBoardingDC>();

            using (var context = new AuthContext())
            {
                var paramStatus = new SqlParameter("@Status", status);
                var id = new SqlParameter("@userid", userid);
                var Keyword = new SqlParameter("@KeyWord", KeyWord == null ? "" : KeyWord);
                var Skip = new SqlParameter("@skip", skip);
                var Take = new SqlParameter("@take", take);

                DepoOnBoardingDCs.DepoOnBoardingDC = context.Database.SqlQuery<DepoOnBoardingDC>("exec GetDepoOnboradList @Status,@userid,@skip,@take,@KeyWord", paramStatus, id, Skip, Take, Keyword).ToList();
                if (DepoOnBoardingDCs.DepoOnBoardingDC.Any())
                {
                    DepoOnBoardingDCs.totalcount = DepoOnBoardingDCs.DepoOnBoardingDC.FirstOrDefault().totalcount;

                    foreach (var item in DepoOnBoardingDCs.DepoOnBoardingDC)
                    {
                        int DepoId = item.DepoId == 0 ? item.Id : item.DepoId;
                        var documents = context.DepoDocumentDB.Where(x => x.DepoId == DepoId && x.IsActive == true && x.IsDeleted == false).ToList();
                        if (documents.Count > 0)
                        {
                            item.PanCardImage = documents.Where(x => x.DocumentName == "PanCard").Select(y => y.DocumentPath).ToList();
                            item.GstImage = documents.Where(x => x.DocumentName == "GST").Select(y => y.DocumentPath).ToList();
                            item.FSSAIImage = documents.Where(x => x.DocumentName == "FSSAI").Select(y => y.DocumentPath).ToList();
                            item.CancelCheque = documents.Where(x => x.DocumentName == "CancelCheque").Select(y => y.DocumentPath).ToList();
                        }
                        else
                        {
                            //&& x.IsActive == true
                            var images = context.DepoMasters.Where(x => x.DepoId == DepoId && x.Deleted == false).FirstOrDefault();
                            if (images != null)
                            {
                                if (images.PanCardImage != null || images.PanCardImage != "")
                                {
                                    List<string> panimage = new List<string>();
                                    panimage.Add(images.PanCardImage);
                                    item.PanCardImage = panimage;
                                }
                                if (images.FSSAIImage != null || images.FSSAIImage != "")
                                {
                                    List<string> fssimage = new List<string>();
                                    fssimage.Add(images.FSSAIImage);
                                    item.FSSAIImage = fssimage;
                                }
                                if (images.GstImage != null || images.GstImage != "")
                                {
                                    List<string> gstimage = new List<string>();
                                    gstimage.Add(images.GstImage);
                                    item.GstImage = gstimage;
                                }
                                if (images.CancelCheque != null || images.CancelCheque != "")
                                {
                                    List<string> canimage = new List<string>();
                                    canimage.Add(images.CancelCheque);
                                    item.CancelCheque = canimage;
                                }
                            }
                        }
                    }
                }

            }
            return DepoOnBoardingDCs;
        }
        public DepoOnBoardingDC GetDepoOnboardByid(int id, int DepId)
        {
            DepoOnBoardingDC onBordDC = new DepoOnBoardingDC();
            using (var context = new AuthContext())
            {
                var paramId = new SqlParameter()
                {
                    ParameterName = "@Id",
                    Value = id
                };
                var paramidd = new SqlParameter()
                {
                    ParameterName = "@DepId",
                    Value = DepId
                };
                onBordDC = context.Database.SqlQuery<DepoOnBoardingDC>("GetDepoOnboradDetailsbyid @Id,@DepId", paramId, paramidd).FirstOrDefault();
                if (onBordDC != null)
                {
                    int DepoId = onBordDC.DepoId == 0 ? onBordDC.Id : onBordDC.DepoId;
                    var documents = context.DepoDocumentDB.Where(x => x.DepoId == DepoId && x.IsActive == true && x.IsDeleted == false).ToList();
                    if (documents.Count > 0)
                    {
                        onBordDC.PanCardImage = documents.Where(x => x.DocumentName == "PanCard").Select(y => y.DocumentPath).ToList();
                        onBordDC.GstImage = documents.Where(x => x.DocumentName == "GST").Select(y => y.DocumentPath).ToList();
                        onBordDC.FSSAIImage = documents.Where(x => x.DocumentName == "FSSAI").Select(y => y.DocumentPath).ToList();
                        onBordDC.CancelCheque = documents.Where(x => x.DocumentName == "CancelCheque").Select(y => y.DocumentPath).ToList();

                        onBordDC.PanCardImages = documents.Where(x => x.DocumentName == "PanCard").ToList();
                        onBordDC.GstImages = documents.Where(x => x.DocumentName == "GST").ToList();
                        onBordDC.FSSAIImages = documents.Where(x => x.DocumentName == "FSSAI").ToList();
                        onBordDC.CancelCheques = documents.Where(x => x.DocumentName == "CancelCheque").ToList();
                    }
                    else
                    {
                        //&& x.IsActive == true
                        var images = context.DepoMasters.Where(x => x.DepoId == DepoId && x.Deleted == false).FirstOrDefault();
                        if (images != null)
                        {
                            if (images.PanCardImage != null || images.PanCardImage != "")
                            {
                                List<string> panimage = new List<string>();
                                panimage.Add(images.PanCardImage);
                                onBordDC.PanCardImage = panimage;
                            }
                            if (images.FSSAIImage != null || images.FSSAIImage != "")
                            {
                                List<string> fssimage = new List<string>();
                                fssimage.Add(images.FSSAIImage);
                                onBordDC.FSSAIImage = fssimage;
                            }
                            if (images.GstImage != null || images.GstImage != "")
                            {
                                List<string> gstimage = new List<string>();
                                gstimage.Add(images.GstImage);
                                onBordDC.GstImage = gstimage;
                            }
                            if (images.CancelCheque != null || images.CancelCheque != "")
                            {
                                List<string> canimage = new List<string>();
                                canimage.Add(images.CancelCheque);
                                onBordDC.CancelCheque = canimage;
                            }
                        }
                    }
                }
            }
            return onBordDC;
        }

        public List<DepoOnBoardingDC> GetDepoOnboardListBySupplierID(int Supplierid)
        {
            List<DepoOnBoardingDC> depos = new List<DepoOnBoardingDC>();

            using (var context = new AuthContext())
            {
                var paramSupplierid = new SqlParameter()
                {
                    ParameterName = "@SupplierId",
                    Value = Supplierid
                };
                depos = context.Database.SqlQuery<DepoOnBoardingDC>("GetDepoOnboradDetailsbySupplierId @SupplierId", paramSupplierid).ToList();
                if (depos != null)
                {
                    foreach (var item in depos)
                    {
                        int DepoId = item.DepoId == 0 ? item.Id : item.DepoId;

                        var documents = context.DepoDocumentDB.Where(x => x.DepoId == DepoId && x.IsActive == true && x.IsDeleted == false).ToList();
                        if (documents.Count > 0)
                        {
                            item.PanCardImage = documents.Where(x => x.DocumentName == "PanCard").Select(y => y.DocumentPath).ToList();
                            item.GstImage = documents.Where(x => x.DocumentName == "GST").Select(y => y.DocumentPath).ToList();
                            item.FSSAIImage = documents.Where(x => x.DocumentName == "FSSAI").Select(y => y.DocumentPath).ToList();
                            item.CancelCheque = documents.Where(x => x.DocumentName == "CancelCheque").Select(y => y.DocumentPath).ToList();
                        }
                    }
                }
            }
            return depos;
        }
        public bool ActionOnDepoOnboard(DepoOnBoardingActionDC actionDc)
        {
            bool result = false;
            try
            {
                using (var context = new AuthContext())
                {
                    List<object> parameters = new List<object>();
                    var idparam = new SqlParameter()
                    {
                        ParameterName = "@id",
                        Value = actionDc.id
                    };
                    parameters.Add(idparam);
                    var Supplieridparam = new SqlParameter()
                    {
                        ParameterName = "@DepoId",
                        Value = actionDc.DepoId
                    };
                    parameters.Add(Supplieridparam);
                    var Statusparam = new SqlParameter()
                    {
                        ParameterName = "@Status",
                        Value = actionDc.Status
                    };
                    parameters.Add(Statusparam);
                    var Commentsparam = new SqlParameter()
                    {
                        ParameterName = "@Comments",
                        Value = actionDc.Comments
                    };
                    parameters.Add(Commentsparam);

                    var useridparam = new SqlParameter()
                    {
                        ParameterName = "@UserId",
                        Value = actionDc.Userid
                    };
                    parameters.Add(useridparam);


                    var rowsaffected = context.Database.ExecuteSqlCommand("DepoOnboardAction @id,@DepoId,@Status,@Comments,@UserId ", parameters.ToArray());
                    if (rowsaffected > 0)
                        result = true;

                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in Action on new depo  " + ex.Message);
                throw ex;
            }
            return result;
        }

        public bool EditDepotemp(DepoOnBoardingDC depodc)
        {
            bool result = false;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope Scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                try
                {

                    int depoid = 0;
                    using (var context = new AuthContext())
                    {
                        var cityname = context.Cities.Where(x => x.Cityid == depodc.Cityid && x.Deleted == false).FirstOrDefault().CityName;
                        var statename = context.Cities.Where(x => x.Stateid == depodc.Stateid && x.Deleted == false).FirstOrDefault().StateName;

                        var depo = context.DepoMasterTempDB.Where(x => x.Id == depodc.Id && x.Status != "Pending").FirstOrDefault();
                        if (depo != null)
                        {
                            depo.Stateid = depodc.Stateid;
                            depo.StateName = statename;
                            depo.Cityid = depodc.Cityid;
                            depo.CityName = cityname;
                            depo.DepoName = depodc.DepoName;
                            depo.GSTin = depodc.GSTin;
                            //depo.DepoCodes = depodc.DepoCodes;
                            depo.Address = depodc.Address;
                            depo.Email = depodc.Email;
                            depo.PhoneNumber = depodc.PhoneNumber;
                            depo.ContactPerson = depodc.ContactPerson;
                            depo.FSSAI = depodc.FSSAI;
                            depo.CityPincode = depodc.CityPincode;
                            depo.Bank_Name = depodc.Bank_Name;
                            depo.Bank_AC_No = depodc.Bank_AC_No;
                            depo.BankAddress = depodc.BankAddress;
                            depo.Bank_Ifsc = depodc.Bank_Ifsc;
                            depo.BankPinCode = depodc.BankPinCode;
                            //
                            depo.BankAccountType = depodc.BankAccountType;
                            //
                            depo.PANCardNo = depodc.PANCardNo;
                            depo.OpeningHours = depodc.OpeningHours;
                            //
                            depo.IsActive = depodc.IsActive;
                            //
                            depo.PRPOStopAfterValue = depodc.PRPOStopAfterValue;

                            depo.GstImage = depodc.GstImage != null && depodc.GstImage.Any() ? depodc.GstImage[0] : null;
                            depo.FSSAIImage = depodc.FSSAIImage != null && depodc.FSSAIImage.Any() ? depodc.FSSAIImage[0] : null;
                            depo.PanCardImage = depodc.PanCardImage != null && depodc.PanCardImage.Any() ? depodc.PanCardImage[0] : null;
                            depo.CancelCheque = depodc.CancelCheque != null && depodc.CancelCheque.Any() ? depodc.CancelCheque[0] : null;
                            depo.Status = supplierStatus.Pending.ToString();
                            context.Entry(depo).State = System.Data.Entity.EntityState.Modified;

                            if (context.Commit() > 0)
                            {
                                depoid = depo.DepoId == 0 ? depo.Id : depo.DepoId;
                                if (depodc.GstImage != null && depodc.GstImage.Any())
                                {
                                    var depodoc = context.DepoDocumentDB.Where(x => x.DepoId == depoid && x.DocumentName == "GST").ToList();
                                    if (depodoc.Count > 0)
                                    {
                                        foreach (var item in depodoc)
                                        {
                                            item.IsActive = false;
                                            item.IsDeleted = true;
                                            context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                        }
                                        context.Commit();
                                    }
                                    List<DepoDocument> addimglist = new List<DepoDocument>();
                                    foreach (var item in depodc.GstImage)
                                    {
                                        DepoDocument doc = new DepoDocument()
                                        {
                                            SupplierId = (int)depo.SupplierId,
                                            DepoId = depoid,
                                            DocumentPath = item,
                                            DocumentName = "GST",
                                            CreatedBy = Convert.ToInt32(depodc.CreatedBy),
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.DepoDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }
                                if (depodc.FSSAIImage != null && depodc.FSSAIImage.Any())
                                {
                                    var depodoc = context.DepoDocumentDB.Where(x => x.DepoId == depoid && x.DocumentName == "FSSAI").ToList();
                                    if (depodoc.Count > 0)
                                    {
                                        foreach (var item in depodoc)
                                        {
                                            item.IsActive = false;
                                            item.IsDeleted = true;
                                            context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                        }
                                        context.Commit();
                                    }
                                    List<DepoDocument> addimglist = new List<DepoDocument>();
                                    foreach (var item in depodc.FSSAIImage)
                                    {
                                        DepoDocument doc = new DepoDocument()
                                        {
                                            SupplierId = (int)depo.SupplierId,
                                            DepoId = depoid,
                                            DocumentPath = item,
                                            DocumentName = "FSSAI",
                                            CreatedBy = Convert.ToInt32(depodc.CreatedBy),
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.DepoDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }
                                if (depodc.PanCardImage != null && depodc.PanCardImage.Any())
                                {
                                    var depodoc = context.DepoDocumentDB.Where(x => x.DepoId == depoid && x.DocumentName == "PanCard").ToList();
                                    if (depodoc.Count > 0)
                                    {
                                        foreach (var item in depodoc)
                                        {
                                            item.IsActive = false;
                                            item.IsDeleted = true;
                                            context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                        }
                                        context.Commit();
                                    }
                                    List<DepoDocument> addimglist = new List<DepoDocument>();
                                    foreach (var item in depodc.PanCardImage)
                                    {
                                        DepoDocument doc = new DepoDocument()
                                        {
                                            SupplierId = (int)depo.SupplierId,
                                            DepoId = depoid,
                                            DocumentPath = item,
                                            DocumentName = "PanCard",
                                            CreatedBy = Convert.ToInt32(depodc.CreatedBy),
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.DepoDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }
                                if (depodc.CancelCheque != null && depodc.CancelCheque.Any())
                                {
                                    var depodoc = context.DepoDocumentDB.Where(x => x.DepoId == depoid && x.DocumentName == "CancelCheque").ToList();
                                    if (depodoc.Count > 0)
                                    {
                                        foreach (var item in depodoc)
                                        {
                                            item.IsActive = false;
                                            item.IsDeleted = true;
                                            context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                        }
                                        context.Commit();
                                    }
                                    List<DepoDocument> addimglist = new List<DepoDocument>();
                                    foreach (var item in depodc.CancelCheque)
                                    {
                                        DepoDocument doc = new DepoDocument()
                                        {
                                            SupplierId = (int)depo.SupplierId,
                                            DepoId = depoid,
                                            DocumentPath = item,
                                            DocumentName = "CancelCheque",
                                            CreatedBy = Convert.ToInt32(depodc.CreatedBy),
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.DepoDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }
                                result = true;

                            }
                        }
                        else
                        {
                            depo = new DepoMasterTemp();
                            depo.DepoId = depodc.DepoId;
                            depo.SupplierId = depodc.SupplierId;
                            depo.SupplierName = depodc.SupplierName;
                            depo.SupplierCode = depodc.SupplierCode;
                            depo.CompanyId = depodc.CompanyId;
                            depo.Deleted = depodc.Deleted;
                            depo.GruopID = depodc.GruopID;
                            depo.TGrpName = depodc.TGrpName;
                            depo.WarehouseId = depodc.WarehouseId;
                            depo.WarehouseName = depodc.WarehouseName;
                            depo.OfficePhone = depodc.OfficePhone;
                            depo.TINNo = depodc.TINNo;
                            depo.Stateid = depodc.Stateid;
                            depo.StateName = statename;
                            depo.Cityid = depodc.Cityid;
                            depo.CityName = cityname;
                            depo.DepoName = depodc.DepoName;
                            depo.GSTin = depodc.GSTin;
                            depo.DepoId = depodc.DepoId;
                            depo.DepoCodes = depodc.DepoCodes;
                            depo.Address = depodc.Address;
                            depo.Email = depodc.Email;
                            depo.PhoneNumber = depodc.PhoneNumber;
                            depo.ContactPerson = depodc.ContactPerson;
                            depo.FSSAI = depodc.FSSAI;
                            depo.CityPincode = depodc.CityPincode;
                            depo.Bank_Name = depodc.Bank_Name;
                            depo.Bank_AC_No = depodc.Bank_AC_No;
                            depo.BankAddress = depodc.BankAddress;
                            depo.Bank_Ifsc = depodc.Bank_Ifsc;
                            depo.BankPinCode = depodc.BankPinCode;
                            //
                            depo.BankAccountType = depodc.BankAccountType;
                            //
                            depo.PANCardNo = depodc.PANCardNo;
                            depo.OpeningHours = depodc.OpeningHours;
                            //
                            depo.IsActive = depodc.IsActive;
                            depo.CreatedDate = DateTime.Now;
                            depo.Comments = depodc.Comments;
                            //
                            depo.PRPOStopAfterValue = depodc.PRPOStopAfterValue;

                            depo.GstImage = depodc.GstImage != null && depodc.GstImage.Any() ? depodc.GstImage[0] : null;
                            depo.FSSAIImage = depodc.FSSAIImage != null && depodc.FSSAIImage.Any() ? depodc.FSSAIImage[0] : null;
                            depo.PanCardImage = depodc.PanCardImage != null && depodc.PanCardImage.Any() ? depodc.PanCardImage[0] : null;
                            depo.CancelCheque = depodc.CancelCheque != null && depodc.CancelCheque.Any() ? depodc.CancelCheque[0] : null;
                            depo.Status = supplierStatus.Pending.ToString();
                            context.DepoMasterTempDB.Add(depo);

                            if (context.Commit() > 0)
                            {
                                depoid = depodc.DepoId == 0 ? depodc.Id : depodc.DepoId;
                                if (depodc.GstImage != null && depodc.GstImage.Any())
                                {
                                    var depodoc = context.DepoDocumentDB.Where(x => x.DepoId == depoid && x.DocumentName == "GST").ToList();
                                    if (depodoc.Count > 0)
                                    {
                                        foreach (var item in depodoc)
                                        {
                                            item.IsActive = false;
                                            item.IsDeleted = true;
                                            context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                        }
                                        context.Commit();
                                    }
                                    List<DepoDocument> addimglist = new List<DepoDocument>();
                                    foreach (var item in depodc.GstImage)
                                    {
                                        DepoDocument doc = new DepoDocument()
                                        {
                                            SupplierId = (int)depodc.SupplierId,
                                            DepoId = depoid,
                                            DocumentPath = item,
                                            DocumentName = "GST",
                                            CreatedBy = 0,
                                            //CreatedBy = userid,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.DepoDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }
                                if (depodc.FSSAIImage != null && depodc.FSSAIImage.Any())
                                {
                                    var depodoc = context.DepoDocumentDB.Where(x => x.DepoId == depoid && x.DocumentName == "FSSAI").ToList();
                                    if (depodoc.Count > 0)
                                    {
                                        foreach (var item in depodoc)
                                        {
                                            item.IsActive = false;
                                            item.IsDeleted = true;
                                            context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                        }
                                        context.Commit();
                                    }
                                    List<DepoDocument> addimglist = new List<DepoDocument>();
                                    foreach (var item in depodc.FSSAIImage)
                                    {
                                        DepoDocument doc = new DepoDocument()
                                        {
                                            SupplierId = (int)depodc.SupplierId,
                                            DepoId = depoid,
                                            DocumentPath = item,
                                            DocumentName = "FSSAI",
                                            CreatedBy = 0,
                                            //CreatedBy = userid,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.DepoDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }
                                if (depodc.PanCardImage != null && depodc.PanCardImage.Any())
                                {
                                    var depodoc = context.DepoDocumentDB.Where(x => x.DepoId == depoid && x.DocumentName == "PanCard").ToList();
                                    if (depodoc.Count > 0)
                                    {
                                        foreach (var item in depodoc)
                                        {
                                            item.IsActive = false;
                                            item.IsDeleted = true;
                                            context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                        }
                                        context.Commit();
                                    }
                                    List<DepoDocument> addimglist = new List<DepoDocument>();
                                    foreach (var item in depodc.PanCardImage)
                                    {
                                        DepoDocument doc = new DepoDocument()
                                        {
                                            SupplierId = (int)depodc.SupplierId,
                                            DepoId = depoid,
                                            DocumentPath = item,
                                            DocumentName = "PanCard",
                                            CreatedBy = 0,
                                            //CreatedBy = userid,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.DepoDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }
                                if (depodc.CancelCheque != null && depodc.CancelCheque.Any())
                                {
                                    var depodoc = context.DepoDocumentDB.Where(x => x.DepoId == depoid && x.DocumentName == "CancelCheque").ToList();
                                    if (depodoc.Count > 0)
                                    {
                                        foreach (var item in depodoc)
                                        {
                                            item.IsActive = false;
                                            item.IsDeleted = true;
                                            context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                        }
                                        context.Commit();
                                    }
                                    List<DepoDocument> addimglist = new List<DepoDocument>();
                                    foreach (var item in depodc.CancelCheque)
                                    {
                                        DepoDocument doc = new DepoDocument()
                                        {
                                            SupplierId = (int)depodc.SupplierId,
                                            DepoId = depoid,
                                            DocumentPath = item,
                                            DocumentName = "CancelCheque",
                                            CreatedBy = 0,
                                            //CreatedBy = userid,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreateDate = DateTime.Now
                                        };
                                        addimglist.Add(doc);
                                    }
                                    context.DepoDocumentDB.AddRange(addimglist);
                                    context.Commit();
                                }
                                result = true;

                            }
                        }
                    }
                    if (result)
                    {
                        Scope.Complete();
                    }
                    else
                    {
                        Scope.Dispose();
                    }
                }

                catch (Exception ex)
                {
                    logger.Error("Error in Edit new Depo  " + ex.Message);
                    Scope.Dispose();
                    throw ex;
                }
            }

            return result;

        }

        public bool ActiveDecativeDepo(DepoOnBoardingActionDC actionDc)
        {
            bool result = false;

            try
            {
                using (var context = new AuthContext())
                {
                    //SupplierOnboardActiveDective
                    List<object> parameters = new List<object>();

                    var Supplieridparam = new SqlParameter()
                    {
                        ParameterName = "@DepoId",
                        Value = actionDc.DepoId
                    };
                    parameters.Add(Supplieridparam);

                    var Commentsparam = new SqlParameter()
                    {
                        ParameterName = "@Comment",
                        Value = actionDc.Comments
                    };
                    parameters.Add(Commentsparam);

                    var useridparam = new SqlParameter()
                    {
                        ParameterName = "@UserId",
                        Value = actionDc.Userid
                    };
                    parameters.Add(useridparam);

                    var activetypeparam = new SqlParameter()
                    {
                        ParameterName = "@ActiveType",
                        Value = actionDc.ActiveType
                    };
                    parameters.Add(activetypeparam);


                    var rowsaffected = context.Database.ExecuteSqlCommand("DepoOnboardActiveDective @DepoId,@ActiveType,@UserId,@Comment ", parameters.ToArray());
                    if (rowsaffected > 0)
                        result = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in ActiveDecativeSupplier on new Depo  " + ex.Message);
                throw ex;
            }

            return result;
        }


        public async System.Threading.Tasks.Task<GSTdetailsDc> GSTVeifyAsync(string GSTNO)
        {

            //bool isverify = false;
            GSTdetailsDc dc = new GSTdetailsDc();
            string path = ConfigurationManager.AppSettings["GetCustomerGstUrl"];
            path = path.Replace("[[GstNo]]", GSTNO);
            var gst = new CustomerGst();

            using (GenericRestHttpClient<CustomerGst, string> memberClient
                   = new GenericRestHttpClient<CustomerGst, string>(path,
                   string.Empty, null))
            {
                try
                {
                    gst = await memberClient.GetAsync();
                }
                catch (Exception ex)
                {
                    logger.Error("GST API error: " + ex.ToString());
                    gst.error = true;
                }

                if (gst.error == false)
                {

                    CustGSTverifiedRequest GSTdata = new CustGSTverifiedRequest();
                    GSTdata.RequestPath = path;
                    GSTdata.RefNo = gst.taxpayerInfo.gstin;
                    GSTdata.Name = gst.taxpayerInfo.lgnm;
                    GSTdata.ShopName = gst.taxpayerInfo.tradeNam;
                    GSTdata.Active = gst.taxpayerInfo.sts;
                    GSTdata.ShippingAddress = gst.taxpayerInfo.pradr?.addr?.st;
                    GSTdata.State = gst.taxpayerInfo.pradr?.addr?.stcd;
                    GSTdata.City = gst.taxpayerInfo.pradr?.addr?.loc;
                    GSTdata.lat = gst.taxpayerInfo.pradr?.addr?.lt;
                    GSTdata.lg = gst.taxpayerInfo.pradr?.addr?.lg;
                    GSTdata.Zipcode = gst.taxpayerInfo.pradr?.addr?.pncd;
                    GSTdata.RegisterDate = gst.taxpayerInfo.rgdt;
                    GSTdata.LastUpdate = gst.taxpayerInfo.lstupdt;
                    GSTdata.HomeName = gst.taxpayerInfo.pradr?.addr?.bnm;
                    GSTdata.HomeNo = gst.taxpayerInfo.pradr?.addr?.bno;
                    GSTdata.CustomerBusiness = gst.taxpayerInfo.nba != null && gst.taxpayerInfo.nba.Any() ? gst.taxpayerInfo.nba[0] : "";
                    GSTdata.Citycode = gst.taxpayerInfo.ctjCd;
                    GSTdata.PlotNo = gst.taxpayerInfo.pradr?.addr?.flno;
                    GSTdata.Message = gst.error;
                    GSTdata.UpdateDate = DateTime.Now;
                    GSTdata.CreateDate = DateTime.Now;
                    GSTdata.Delete = false;

                    string Active = gst.taxpayerInfo.sts;
                    dc.Address = string.Format("{0}, {1}, {2}, {3}, {4}-{5}", GSTdata.HomeNo, GSTdata.HomeName, GSTdata.ShippingAddress, GSTdata.City, GSTdata.State, GSTdata.Zipcode);
                    dc.City = gst.taxpayerInfo.pradr?.addr?.loc;
                    dc.Pincode = gst.taxpayerInfo.pradr?.addr?.pncd;
                    dc.RegisterDate = gst.taxpayerInfo.rgdt;
                    dc.GSTNno = gst.taxpayerInfo.gstin;

                    if (Active == "Active")
                    {
                        dc.IsVerify = true;
                    }
                    else
                    {
                        dc.IsVerify = false;
                    }
                }
                else
                {
                    dc.IsVerify = false;
                }
            }
            return dc;
        }

    }

    public class SupplierOnBoardDCList
    {
        public List<SupplierOnBoardDC> SupplierOnBoardDC { get; set; }
        public int Totalcount { get; set; }
    }

    public class SupplierOnBoardDC
    {
        public int id { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string TypeofFirm { get; set; }
        public string ProprietorName { get; set; }
        public string PanNumber { get; set; }
        public List<string> PanNumberImage { get; set; }
        public string AddressProofType { get; set; }
        public List<string> AddressProofImage { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public int CreditScore { get; set; }
        public string FatherName { get; set; }
        public string IsLoanRequirement { get; set; } // YEs No
        public double loanAmount { get; set; }
        public string SupplierType { get; set; }
        public List<int> SellingBrandDCs { get; set; }
        public int BusinessEstablishmentYear { get; set; }
        public string GSTINNO { get; set; }
        public string TINNo { get; set; }
        public string FSSAINO { get; set; }
        public List<string> FSSAIImage { get; set; }
        public string FullBusinessAddress { get; set; }
        public string HeadOffice { get; set; }
        public string PinCode { get; set; }
        public string BankAccType { get; set; }
        public string BankAccNo { get; set; }
        public string BankAddress { get; set; }
        public string BankName { get; set; }
        public string IFSCCode { get; set; }

        //--created by anjali
        public bool IsVerified { get; set; }
        public bool Deleted { get; set; }
        public string proprietorphonenumber { get; set; }
        public int? bankPinCode { get; set; }

        //
        public int userid { get; set; }
        public string Comments { get; set; }
        public string SUPPLIERCODES { get; set; }
        public string CreatedBy { get; set; }
        public string ModifyBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedDate { get; set; }
        public string Status { get; set; }
        public string SubcategoryName { get; set; }

        public bool IsStopAdvancePr { get; set; }
        public bool IsIRNInvoiceRequired { get; set; }
        public string Password { get; set; }
        public string WebUrl { get; set; }
        public string ContactPerson { get; set; }
        public string bussinessType { get; set; }
        public DateTime? StartedBusiness { get; set; }
        public int PaymentTerms { get; set; }
        public int CompanyId { get; set; }
        //  public List<SellingBrandDC> SellingBrandDCs { get; set; }
        public int Stateid { get; set; }
        public string StateName { get; set; }
        public int Cityid { get; set; }
        public string City { get; set; }

        public List<string> GSTImage { get; set; }
        public List<string> ChequeImage { get; set; }
        public string SupplierSegment { get; set; }
        public string ContactPersonMobileNo { get; set; }
        public bool Active { get; set; }
        public int ExpiryDays { get; set; }
        public string ProprietorPanNumber { get; set; }
        public List<string> AgreementImage { get; set; }
        public List<string> ProprietorPanImage { get; set; }
        public int Totalcount { get; set; }
        public List<SupplierDocument> PanNumberImages { get; set; }
        public List<SupplierDocument> AddressProofImages { get; set; }
        public List<SupplierDocument> AgreementImages { get; set; }
        public List<SupplierDocument> ChequeImages { get; set; }
        public List<SupplierDocument> FSSAIImages { get; set; }
        public List<SupplierDocument> GSTImages { get; set; }
        public List<SupplierDocument> ProprietorPanImages { get; set; }
        public List<SupplierDocument> MSMEImages { get; set; }
        public string MSMEType { get; set; }
        public List<string> MSMEImage { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
    }


    public class SellingBrandDC
    {
        public int Categoryid { get; set; }
        public string HindiName { get; set; }
        public string LogoUrl { get; set; }
        public int SubCategoryId { get; set; }
        public string SubcategoryName { get; set; }
        public int itemcount { get; set; }
    }
    public class SupplierOnBoardActionDC
    {
        public int id { get; set; }
        public int? SupplierId { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }
        public int Userid { get; set; }
        public bool ActiveType { get; set; }
        public string PaymentTerms { get; set; }
        public string SupplierType { get; set; }
    }


    public class SupplierOnboardHisotryDC
    {
        public string Status { get; set; }
        public string Comment { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string SupplierCode { get; set; }
        public string Suppliername { get; set; }
    }
    public class DepoOnBoardingActionDC
    {
        public int id { get; set; }
        public int? DepoId { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }
        public int Userid { get; set; }
        public bool ActiveType { get; set; }
    }

    public class DepoOnBoardingDCList
    {
        public List<DepoOnBoardingDC> DepoOnBoardingDC { get; set; }
        public int totalcount { get; set; }
    }
    public class DepoOnBoardingDC
    {
        public int Id { get; set; }
        public int DepoId { get; set; }
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierCode { get; set; }
        public string DepoName { get; set; }
        public int CompanyId { get; set; }
        public string GSTin { get; set; }

        public bool IsActive { get; set; }

        public int? Stateid { get; set; }
        public string StateName { get; set; }
        public int? Cityid { get; set; }
        public string CityName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdateBy { get; set; }
        public bool Deleted { get; set; }
        public int? GruopID { get; set; }
        public string TGrpName { get; set; }

        public string ImageUrl { get; set; }
        public string ContactPerson { get; set; }
        public int? WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string DepoCodes { get; set; }
        public string OfficePhone { get; set; }
        public string Bank_Name { get; set; }
        public string Bank_AC_No { get; set; }
        public string Bank_Ifsc { get; set; }
        public string PhoneNumber { get; set; }
        public string TINNo { get; set; }


        public string BankAddress { get; set; }
        public int BankPinCode { get; set; }
        public string FSSAI { get; set; }
        public string PANCardNo { get; set; }
        public string OpeningHours { get; set; }
        public int CityPincode { get; set; }


        public List<string> GstImage { get; set; }
        public List<string> FSSAIImage { get; set; }
        public List<string> PanCardImage { get; set; }
        public List<string> CancelCheque { get; set; }
        public List<DepoDocument> GstImages { get; set; }
        public List<DepoDocument> FSSAIImages { get; set; }
        public List<DepoDocument> PanCardImages { get; set; }
        public List<DepoDocument> CancelCheques { get; set; }
        //------------------------------
        public int PRPOStopAfterValue { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }
        public string BankAccountType { get; set; }
        public int totalcount { get; set; }
    }

    public class GSTdetailsDc
    {
        public string GSTNno { get; set; }
        public string Address { get; set; }
        public string Pincode { get; set; }
        public string City { get; set; }
        public string RegisterDate { get; set; }
        public bool IsVerify { get; set; }
    }

    enum supplierStatus
    {
        Approved,
        Rejected,
        Pending,
        Modified
    }
}