

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ExcelOrderCtrl', ExcelOrderCtrl);

    ExcelOrderCtrl.$inject = ['$scope', '$http', 'WarehouseService'];

    function ExcelOrderCtrl($scope, $http, WarehouseService) {
        
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));

        function convertDate(inputFormat) {
            //var month = new Array();
            var month = [];//pz
            month[0] = "January";
            month[1] = "February";
            month[2] = "March";
            month[3] = "April";
            month[4] = "May";
            month[5] = "June";
            month[6] = "July";
            month[7] = "August";
            month[8] = "September";
            month[9] = "October";
            month[10] = "November";
            month[11] = "December";
            function pad(s) { return (s < 10) ? '0' + s : s; }
            var d = new Date(inputFormat);
            var n = month[d.getMonth()];
            return n;
        }



       {
            $scope.warehouse = [];
            WarehouseService.getwarehouse().then(function (results) {
                console.log(results.data);
                console.log("data");
                $scope.warehouse = results.data;
            }, function (error) {
            });
            $(function () {
                $('input[name="daterange"]').daterangepicker({
                    timePicker: true,
                    timePickerIncrement: 5,
                    timePicker12Hour: true,
                    format: 'MM/DD/YYYY h:mm A'
                });
            });


            //(by neha : 11/09/2019 -date range )
            $(function () {
                $('input[name="daterange"]').daterangepicker({
                    maxDate: moment(),

                    "dateLimit": {
                        "month": 1
                    },
                    timePicker: true,

                    timePickerIncrement: 5,
                    timePicker12Hour: true,
                    format: 'DD/MM/YYYY '
                }), 

                $('.input-group-addon').click(function () {
                    $('input[name="daterange"]').trigger("select");
                    

                });
                

            });







            var Allbydate = [];
            $scope.dataforsearch = { datefrom: "", dateto: "" };
            //............................Exel export Method...........................// Anil
            $scope.dataforsearch1 = { Cityid: "", Warehouseid: "", datefrom: "", dateto: "" };
            $scope.exportData = function (WarehouseId) {
                
                if (WarehouseId == undefined) {
                    alert("Select Warehouse");
                    return false;
                }
                $scope.WarehouseId = parseInt(WarehouseId);


                var f = $('input[name=daterangepicker_start]');
                var g = $('input[name=daterangepicker_end]');                
                $scope.OrderByDate = [];
                var start = f.val();
                var end = g.val();
                end = end + "11:59 PM";
                if ($('#dat').val()) {
                    alert("Select Date Range ");
                    return false;
                }

                $scope.NewExportData = [];
                $scope.customerdata = [];

                var url = serviceBase + "api/ExcelOrder?type=export&start=" + start + "&end=" + end + "&Warehouseid=" + $scope.WarehouseId;
                $http.get(url).success(function (response) {
                    
                    $scope.NewExportData = [];
                    $scope.OrderByDate = response;
                    console.log("$scope.OrderByDate", $scope.OrderByDate);
                    var Allbydate = [];

                    var filterBydate = [];
                    var finallistoflist = [];
                    $scope.filtype = "";
                    $scope.type = "";

                    console.log("export");
                    if (response.length == 0) {
                        alert("No data available between two date ")
                    }
                    else {
                        for (var i = 0; i < $scope.OrderByDate.length; i++) {
                            var OrderId = $scope.OrderByDate[i].OrderId;
                            //var Skcode = $scope.OrderByDate[i].Skcode;
                            var ShopName = $scope.OrderByDate[i].ShopName;
                            var OrderBy = $scope.OrderByDate[i].OrderBy;
                            var Mobile = $scope.OrderByDate[i].Mobile;
                            var CustomerName = $scope.OrderByDate[i].CustomerName;
                            var CustomerId = $scope.OrderByDate[i].CustomerId;
                            var WarehouseName = $scope.OrderByDate[i].WarehouseName;
                            var ClusterName = $scope.OrderByDate[i].ClusterName;
                            var Deliverydate = $scope.OrderByDate[i].Deliverydate;
                            var CompanyId = $scope.OrderByDate[i].CompanyId;
                            var BillingAddress = $scope.OrderByDate[i].BillingAddress;
                            var Date = $scope.OrderByDate[i].Date;
                            var SalesPerson = $scope.OrderByDate[i].SalesPerson;
                            var ShippingAddress = $scope.OrderByDate[i].ShippingAddress;
                            var delCharge = $scope.OrderByDate[i].deliveryCharge;
                            var Status = $scope.OrderByDate[i].Status;
                            var DeliveryIssuanceIdOrderDeliveryMaster = $scope.OrderByDate[i].DeliveryIssuanceIdOrderDeliveryMaster;
                            var CategoryName = $scope.OrderByDate[i].CategoryName;
                            var BrandName = $scope.OrderByDate[i].BrandName;
                            var ReasonCancle = $scope.OrderByDate[i].ReasonCancle;
                            var comments = $scope.OrderByDate[i].comments;
                            var UnitPricei = $scope.OrderByDate[i].UnitPrice;
                            var qtyi = $scope.OrderByDate[i].qty;
                            var TotalAmti = $scope.OrderByDate[i].TotalAmt;
                            var DboyName = $scope.OrderByDate[i].DboyName;
                            var DboyMobileNo = $scope.OrderByDate[i].DboyMobileNo;
                            var TaxAmmount = $scope.OrderByDate[i].TaxAmmount;
                            var IFRQty = '';
                            var IFRVlu = '';
                            var ItemFillRate = '';
                            var RetailerName = $scope.OrderByDate[i].CustomerName;
                            //var ShopName = $scope.OrderByDate[i].ShopName;
                            //var Skcode = $scope.OrderByDate[i].Skcode;
                           ////// var Mobile = $scope.OrderByDate[i].Mobile;
                            var Warehouse = $scope.OrderByDate[i].WarehouseName;
                            var HSNCode = $scope.OrderByDate[i].HSNCode;
                            var GST_No = $scope.OrderByDate[i].Tin_No;
                            var AmtWithoutTaxDisc = $scope.OrderByDate[i].AmtWithoutTaxDisc; // for Amount without  tax
                            var SGSTTaxPercentage = $scope.OrderByDate[i].SGSTTaxPercentage;  // for SGSTT percentage
                            var SGSTTaxAmmount = $scope.OrderByDate[i].SGSTTaxAmmount; // for SGSTT Amount
                            var CGSTTaxPercentage = $scope.OrderByDate[i].CGSTTaxPercentage; // for CGSTT Percentage
                            var CGSTTaxAmmount = $scope.OrderByDate[i].CGSTTaxAmmount; // for CGSTT Amount
                            var TotalCessPercentage = $scope.OrderByDate[i].TotalCessPercentage; // for Cess Percentage
                            var IGSTTaxAmount = $scope.OrderByDate[i].IGSTTaxAmount;
                            var IGSTTaxPercent = $scope.OrderByDate[i].IGSTTaxPercent;
                            var CessTaxAmount = $scope.OrderByDate[i].CessTaxAmount; // for Cess Amount
                            var invoice_no = $scope.OrderByDate[i].invoice_no;                 

                            var OrderDispatchedDetailss = $scope.OrderByDate[i].orderDispatchedDetailsExport;
                            for (var j = 0; j < OrderDispatchedDetailss.length; j++) {
                                //var tts = {
                                //    OrderId: '', Skcode: '', ShopName: '', Mobile: '', RetailerName: '', RetailerID: '', Warehouse: '', BillingAddress: '', Date: '',
                                //    ItemID: '', ItemName: '', itemNumber: '', MRP: '', MOQ: '', UnitPrice: '', dUnitPrice: '', Quantity: '', DispatchedQuantity: '', IFRQty: '', IFRVlu: '', MOQPrice: '', Discount: '',
                                //    DiscountPercentage: '', TaxPercentage: '', qtychangeR: '', Tax: '', TotalAmt: '', DispatchedTotalAmt: '', CategoryName: '', BrandName: '', Status: '',
                                //}
                                var tts = {
                                    OrderId: '', Mobile: '', RetailerName: '', RetailerID: '', Warehouse: '', BillingAddress: '', Date: '',
                                    ItemID: '', ItemName: '', itemNumber: '', MRP: '', MOQ: '', UnitPrice: '', dUnitPrice: '', Quantity: '', DeliveryIssuanceIdOrderDeliveryMaster: '', DispatchedQuantity: '', IFRQty: '', IFRVlu: '', MOQPrice: '', Discount: '',
                                    DiscountPercentage: '', TaxPercentage: '', qtychangeR: '', TaxAmmount: '', TotalAmt: '', DispatchedTotalAmt: '', CategoryName: '', BrandName: '', Status: '', DboyName: '', DboyMobileNo: '', HSNCode: '', GST_No: '', 
                                    SGSTTaxPercentage: '', SGSTTaxAmmount: '', CGSTTaxPercentage: '', CGSTTaxAmmount: '', TotalCessPercentage: '', IGSTTaxAmount: '', IGSTTaxPercent : '', CessTaxAmount: '', invoice_no: '', AmtWithoutTaxDisc: '', ShopName: ''
                                }
                                tts.ItemID = OrderDispatchedDetailss[j].ItemId;
                                tts.ItemName = OrderDispatchedDetailss[j].itemname;
                                tts.qtychangeR = OrderDispatchedDetailss[j].QtyChangeReason;
                                tts.itemNumber = OrderDispatchedDetailss[j].itemNumber;
                                tts.MRP = OrderDispatchedDetailss[j].price;
                                tts.UnitPrice = UnitPricei;
                                tts.invoice_no = invoice_no;
                                tts.AmtWithoutTaxDisc = AmtWithoutTaxDisc;
                                tts.dUnitPrice = OrderDispatchedDetailss[j].dUnitPrice;
                                tts.Quantity = qtyi;
                                tts.DispatchedQuantity = OrderDispatchedDetailss[j].dqty;
                                tts.MOQPrice = OrderDispatchedDetailss[j].MinOrderQtyPrice;
                                tts.Discount = OrderDispatchedDetailss[j].DiscountAmmount;
                                tts.DiscountPercentage = OrderDispatchedDetailss[j].DiscountPercentage;
                                tts.TaxPercentage = OrderDispatchedDetailss[j].TaxPercentage;
                                tts.TaxAmmount = OrderDispatchedDetailss[j].TaxAmmount;
                                tts.TotalAmt = TotalAmti;
                                tts.DispatchedTotalAmt = OrderDispatchedDetailss[j].dTotalAmt;
                                tts.CategoryName = CategoryName;
                                tts.BrandName = BrandName;
                                tts.OrderId = OrderId;
                                tts.RetailerID = CustomerId;
                                tts.OrderBy = OrderBy;
                                //tts.Skcode = Skcode;
                                tts.Mobile = Mobile;
                                tts.RetailerName = CustomerName;
                                tts.Warehouse = Warehouse;
                                tts.ShopName = ShopName;
                                //tts.Deliverydate = Deliverydate;
                                //tts.CompanyId = CompanyId;
                                tts.BillingAddress = BillingAddress;
                                tts.Date = Date;
                                //tts.Excecutive = SalesPerson;
                                tts.ShippingAddress = ShippingAddress;
                                //tts.deliveryCharge = delCharge;
                                tts.Status = Status;
                                tts.DboyName = DboyName;
                                tts.DboyMobileNo = DboyMobileNo;
                                tts.HSNCode = HSNCode;
                                tts.GST_No = GST_No;
                                tts.DeliveryIssuanceIdOrderDeliveryMaster = DeliveryIssuanceIdOrderDeliveryMaster;
                                //tts.ReasonCancle = ReasonCancle;
                                //tts.comments = comments;
                                tts.AmtWithoutTaxDisc = AmtWithoutTaxDisc; //for Amount without  tax
                                tts.SGSTTaxPercentage = SGSTTaxPercentage;  // for SGSTT percentage
                                tts.SGSTTaxAmmount = SGSTTaxAmmount; // for SGSTT Amount
                                tts.CGSTTaxPercentage = CGSTTaxPercentage;  // for CGSTT Percentage
                                tts.CGSTTaxAmmount = CGSTTaxAmmount;   // for CGSTT Amount
                                tts.TotalCessPercentage = TotalCessPercentage; // for Cess Percentage
                                tts.IGSTTaxAmount = IGSTTaxAmount; //for Cess Amount
                                tts.IGSTTaxPercent = IGSTTaxPercent;
                                tts.IFRQty = (OrderDispatchedDetailss[j].dqty / qtyi) * 100;
                                tts.IFRVlu = (OrderDispatchedDetailss[j].dTotalAmt / TotalAmti) * 100;
                                tts.ItemFillRate = (OrderDispatchedDetailss[j].dTotalAmt / TotalAmti) * 100;
                                $scope.NewExportData.push(tts);
                            }
                        }


                        $scope.customerdata = $scope.NewExportData;
                        $scope.getChart();
                    }
                });

            };
            $scope.ExcelExportData = function () {
                

                if ($scope.NewExportData == undefined) {
                    alert("No record");
                }
                if ($scope.NewExportData.length > 0) {

                    alasql.fn.myfmt = function (n) {
                        return Number(n).toFixed(2);
                    }
                    alasql('SELECT OrderId,invoice_no,RetailerName,ShopName,Mobile,ItemID,qtychangeR,ItemName,CategoryName,BrandName,Warehouse,Date,MRP,MOQPrice,UnitPrice,Quantity,TotalAmt,DispatchedQuantity,DispatchedTotalAmt,ItemFillRate,Discount,DiscountPercentage,TaxPercentage,Status,DboyName,DboyMobileNo,HSNCode,GST_No,DeliveryIssuanceIdOrderDeliveryMaster,AmtWithoutTaxDisc,SGSTTaxPercentage,SGSTTaxAmmount,CGSTTaxPercentage,CGSTTaxAmmount,TaxAmmount,IGSTTaxPercent,IGSTTaxAmount,TotalCessPercentage,CessTaxAmount INTO XLSX("OrderDetailReport.xlsx",{headers:true}) FROM ?', [$scope.NewExportData]);
                }
                else {
                    alert("No Record");
                }
            }
            //-------------------------------------------------------------------------//
            //for date conversion


            $scope.getChart = function () {
                
                Allbydate = [];
                var avgData = [];
                _.map($scope.customerdata, function (obj) {

                    console.log($scope.customerdata);

                    var formatdate = convertDate(obj.Date);

                    if (avgData.length == 0) {

                        if (isNaN(parseFloat(obj.ItemFillRate))) {
                            obj.ItemFillRate = 0;
                        }

                        var ob = { y: obj.ItemFillRate, label: formatdate, count: 1 }
                        avgData.push(ob);
                    }

                    else {

                        var checkexits = false;
                        angular.forEach(avgData, function (value, key) {
                            if (value.label.replace(/^\s+|\s+$/g, '').toLowerCase() == formatdate.replace(/^\s+|\s+$/g, '').toLowerCase()) {

                                if (isNaN(parseFloat(obj.ItemFillRate))) {
                                    obj.ItemFillRate = 0;
                                }
                                value.y += obj.ItemFillRate;
                                value.count += 1;
                                checkexits = true;
                            }
                        });
                        if (checkexits == false) {
                            if (isNaN(parseFloat(obj.ItemFillRate))) {
                                obj.ItemFillRate = 0;
                            }
                            var ob1 = { y: obj.ItemFillRate, label: formatdate, count: 1 }
                            avgData.push(ob1);
                        }
                    }
                });
                angular.forEach(avgData, function (value, key) {

                    if (value.y.toString() != 'NaN') {
                        var obj1 = { y: (value.y / value.count), label: value.label }
                        Allbydate.push(obj1);
                    } else { }

                });

                var chart = new CanvasJS.Chart("chartContainer",
                    {
                        title: {
                            text: "Total Order FillRate"
                        },
                        animationEnabled: true,
                        axisX: {
                            valueFormatString: "MMM",
                            interval: 1,
                            intervalType: "month"
                        },
                        axisY: {
                            includeZero: false

                        },
                        theme: "theme2",
                        data: [
                            {
                                type: "line",
                                dataPoints: Allbydate
                            }]
                    });
                chart.render();
            }
        }
        
    }
})();


