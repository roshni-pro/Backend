

(function () {
    'use strict';

    angular
        .module('app')
        .controller('AgentExcelController', AgentExcelController);

    AgentExcelController.$inject = ['$scope', '$http'];

    function AgentExcelController($scope, $http) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log("AgentsDashboard Controller reached");
        //.................Get Agent method start..................    

        $scope.dataselect = [];
        $scope.examplemodel = [];
        $scope.exampledata = $scope.dataselect;
        $scope.examplesettings = {
            displayProp: 'DisplayName', idProp: 'PeopleID',
            //externalIdProp: 'Mobile',
            scrollableHeight: '300px',
            scrollableWidth: '450px',
            enableSearch: true,
            scrollable: true
        };

        $scope.Mobile = "";
        $http.get(serviceBase + 'api/Agents').success(function (data) {

            $scope.dataselect = data;

        });

        $scope.datrange = '';
        $scope.datefrom = '';
        $scope.type = 1;
        $scope.dateto = '';

        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'DD/MM/YYYY'
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });

        });
        $scope.getFormatedDate = function (date) {
            console.log("here get for")
            date = new Date(date);
            var dd = date.getDate();
            var mm = date.getMonth() + 1; //January is 0!
            var yyyy = date.getFullYear();
            if (dd < 10) {
                dd = '0' + dd;
            }
            if (mm < 10) {
                mm = '0' + mm;
            }
            var date1 = yyyy + '-' + mm + '-' + dd;//instead of date used date1
            return date;
        }


        //............................Exel export Method...........................// Anil

        $scope.genereteExcel = function () {

            $scope.dataforsearch = { datefrom: "", dateto: "" };
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');

            if (!$('#dat').val()) {
                $scope.dataforsearch.datefrom = '';
                $scope.dataforsearch.dateto = '';
            }
            else {
                $scope.dataforsearch.datefrom = f.val();
                $scope.dataforsearch.dateto = g.val();
            }

            var ids = [];
            _.each($scope.examplemodel, function (o2) {
                console.log(o2);
                for (var i = 0; i < $scope.dataselect.length; i++) {
                    if ($scope.dataselect[i].PeopleID == o2.id) {
                        var Row =
                        {
                            "id": o2.id, "AgentCode": $scope.dataselect[i].AgentCode
                        };
                        ids.push(Row);
                    }
                }
            })
            var datatopost = {
                datefrom: $scope.dataforsearch.datefrom,
                dateto: $scope.dataforsearch.dateto,
                ids: ids
            }

            console.log(datatopost);
            $scope.NewExportData = [];
            $scope.datas = {};
            var url = serviceBase + "api/Agents/GetAgentDboyName";
            $http.post(url, datatopost).success(function (data) {

                $scope.NewExportData = [];
                $scope.OrderByDate = data;
                console.log("$scope.OrderByDate", $scope.OrderByDate);
                var Allbydate = [];

                var filterBydate = [];
                var finallistoflist = [];
                $scope.filtype = "";
                $scope.type = "";

                console.log("export");
                if ($scope.OrderByDate.length <= 0) {
                    alert("No data available between two date ")
                }
                else {
                    for (var i = 0; i < $scope.OrderByDate.length; i++) {
                        var OrderId = $scope.OrderByDate[i].OrderId;
                        //var Skcode = $scope.OrderByDate[i].Skcode;
                        //var ShopName = $scope.OrderByDate[i].ShopName;
                        var OrderBy = $scope.OrderByDate[i].OrderBy;
                        var Mobile = $scope.OrderByDate[i].Customerphonenum;
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

                        var ReasonCancle = $scope.OrderByDate[i].ReasonCancle;
                        var comments = $scope.OrderByDate[i].comments;
                        var UnitPricei = $scope.OrderByDate[i].UnitPrice;
                        var qtyi = $scope.OrderByDate[i].qty;
                        var TotalAmti = $scope.OrderByDate[i].TotalAmt;
                        var DboyName = $scope.OrderByDate[i].DboyName;
                        var DboyMobileNo = $scope.OrderByDate[i].DboyMobileNo;
                        var IFRQty = '';
                        var IFRVlu = '';
                        var ItemFillRate = '';
                        var RetailerName = $scope.OrderByDate[i].CustomerName;
                        //var ShopName = $scope.OrderByDate[i].ShopName;
                        //var Skcode = $scope.OrderByDate[i].Skcode;
                        var Mobile1 = $scope.OrderByDate[i].Mobile; //instead of Mobile used Mobile1
                        var Warehouse = $scope.OrderByDate[i].WarehouseName;
                        var OrderDispatchedDetailss = $scope.OrderByDate[i].orderDispatchedDetailsExport;
                        for (var j = 0; j < OrderDispatchedDetailss.length; j++) {
                            //var tts = {
                            //    OrderId: '', Skcode: '', ShopName: '', Mobile: '', RetailerName: '', RetailerID: '', Warehouse: '', BillingAddress: '', Date: '',
                            //    ItemID: '', ItemName: '', itemNumber: '', MRP: '', MOQ: '', UnitPrice: '', dUnitPrice: '', Quantity: '', DispatchedQuantity: '', IFRQty: '', IFRVlu: '', MOQPrice: '', Discount: '',
                            //    DiscountPercentage: '', TaxPercentage: '', qtychangeR: '', Tax: '', TotalAmt: '', DispatchedTotalAmt: '', CategoryName: '', BrandName: '', Status: '',
                            //}
                            var tts = {
                                OrderId: '', Mobile: '', RetailerName: '', RetailerID: '', Warehouse: '', BillingAddress: '', Date: '',
                                ItemID: '', ItemName: '', itemNumber: '', MRP: '', MOQ: '', UnitPrice: '', dUnitPrice: '', Quantity: '', DispatchedQuantity: '', IFRQty: '', IFRVlu: '', MOQPrice: '', Discount: '',
                                DiscountPercentage: '', TaxPercentage: '', qtychangeR: '', Tax: '', TotalAmt: '', DispatchedTotalAmt: '', CategoryName: '', BrandName: '', Status: '', DboyName: '', DboyMobileNo: '',
                            }
                            tts.ItemID = OrderDispatchedDetailss[j].ItemId;
                            tts.ItemName = OrderDispatchedDetailss[j].itemname;
                            tts.qtychangeR = OrderDispatchedDetailss[j].QtyChangeReason;
                            tts.itemNumber = OrderDispatchedDetailss[j].itemNumber;
                            tts.MRP = OrderDispatchedDetailss[j].price;
                            tts.UnitPrice = UnitPricei;
                            tts.dUnitPrice = OrderDispatchedDetailss[j].dUnitPrice;
                            tts.Quantity = qtyi;
                            tts.DispatchedQuantity = OrderDispatchedDetailss[j].dqty;
                            tts.MOQPrice = OrderDispatchedDetailss[j].MinOrderQtyPrice;
                            tts.Discount = OrderDispatchedDetailss[j].DiscountAmmount;
                            tts.DiscountPercentage = OrderDispatchedDetailss[j].DiscountPercentage;
                            tts.TaxPercentage = OrderDispatchedDetailss[j].TaxPercentage;
                            tts.Tax = OrderDispatchedDetailss[j].TaxAmmount;
                            tts.TotalAmt = TotalAmti;
                            tts.DispatchedTotalAmt = OrderDispatchedDetailss[j].dTotalAmt;
                            tts.CategoryName = OrderDispatchedDetailss[j].CategoryName;
                            tts.BrandName = OrderDispatchedDetailss[j].BrandName;
                            tts.OrderId = OrderId;
                            tts.RetailerID = CustomerId;
                            tts.OrderBy = OrderBy;
                            //tts.Skcode = Skcode;
                            tts.Mobile = Mobile;
                            tts.RetailerName = CustomerName;
                            tts.Warehouse = Warehouse;
                            //tts.ShopName = ShopName;
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
                            //tts.ReasonCancle = ReasonCancle;
                            //tts.comments = comments;
                            tts.IFRQty = (OrderDispatchedDetailss[j].dqty / qtyi) * 100;
                            tts.IFRVlu = (OrderDispatchedDetailss[j].dTotalAmt / TotalAmti) * 100;
                            tts.ItemFillRate = (OrderDispatchedDetailss[j].dTotalAmt / TotalAmti) * 100;


                            $scope.NewExportData.push(tts);
                        }
                    }
                    alasql.fn.myfmt = function (n) {
                        return Number(n).toFixed(2);
                    }
                    alasql('SELECT OrderId,RetailerName,Mobile,ItemID,qtychangeR,ItemName,CategoryName,BrandName,Warehouse,Date,MRP,MOQPrice,UnitPrice,Quantity,TotalAmt,DispatchedQuantity,DispatchedTotalAmt,ItemFillRate,Discount,DiscountPercentage,Tax,TaxPercentage,Status,DboyName,DboyMobileNo INTO XLSX("OrderDetailReport.xlsx",{headers:true}) FROM ?', [$scope.NewExportData]);

                    //alasql('SELECT OrderId,Skcode,ShopName,RetailerName,Mobile,ItemID,qtychangeR,ItemName,CategoryName,BrandName,Warehouse,Date,MRP,MOQPrice,UnitPrice,Quantity,TotalAmt,DispatchedQuantity,DispatchedTotalAmt,ItemFillRate,Discount,DiscountPercentage,Tax,TaxPercentage,Status INTO XLSX("OrderDetailReport.xlsx",{headers:true}) FROM ?', [$scope.NewExportData]);
                }

                $scope.customerdata = $scope.NewExportData;
                $scope.getChart();
            });
        };
        //-------------------------------------------------------------------------//
        //for date conversion
        function convertDate(inputFormat) {
            //var month = new Array();
            var month = [];
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

        $scope.getChart = function () {

            Allbydate = [];
            var avgData = [];
            _.map($scope.customerdata, function (obj) {

                console.log($scope.customerdata);

                var formatdate = convertDate(obj.Date);

                if (avgData.length == 0) {
                    var ob = { y: obj.ItemFillRate, label: formatdate, count: 1 }
                    avgData.push(ob);
                }

                else {

                    var checkexits = false;
                    angular.forEach(avgData, function (value, key) {
                        if (value.label.replace(/^\s+|\s+$/g, '').toLowerCase() == formatdate.replace(/^\s+|\s+$/g, '').toLowerCase()) {
                            value.y += obj.ItemFillRate;
                            value.count += 1;
                            checkexits = true;
                        }
                    });
                    if (checkexits == false) {
                        var ob1 = { y: obj.ItemFillRate, label: formatdate, count: 1 }//instead of ob used ob1
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
})();


