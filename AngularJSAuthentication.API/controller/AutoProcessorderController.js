

(function () {
    'use strict';

    angular
        .module('app')
        .controller('AutoProcessorderController', AutoProcessorderController);

    AutoProcessorderController.$inject = ['$scope', 'OrderMasterService', 'OrderDetailsService', 'DeliveryService', 'ClusterService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal'];

    function AutoProcessorderController($scope, OrderMasterService, OrderDetailsService, DeliveryService, ClusterService, $http, ngAuthSettings, $filter, ngTableParams, $modal) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        function myfunc() {
            $('#orderStatus').on('change', function () {
                $(".saveBtn").prop("disabled", false);
            })
        }

       {
            console.log("AutoProcessorderController start loading OrderDetailsService");
            $scope.currentPageStores = {};
            $scope.statuses = [];
            $scope.orders = [];
            $scope.filterdata = [];
            $scope.customers = [];
            $scope.selectedd = {};
            $scope.cities = [];
            $scope.statusname = {};
            $scope.srch = { clusterid: "", delboy: "", WarehouseId: 0 };
            $scope.deliveryBoyId = "";
            OrderMasterService.getcitys().then(function (results) {
                $scope.cities = results.data;
            }, function (error) {
            });


            $scope.vm = {
                rowsPerPage: 20,
                currentPage: 1,
                count: null,
                numberOfPages: null,
            };

            // new pagination 
            $scope.pageno = 1; //initialize page no to 1
            $scope.total_count = 0;

            $scope.itemsPerPage = 20;  //this could be a dynamic value from a drop down

            $scope.numPerPageOpt = [20, 30, 50, 100];  //dropdown options for no. of Items per page

            $scope.selected = $scope.numPerPageOpt[0];
            $scope.warehouse = [];
            OrderMasterService.getwarehouse().then(function (results) {

                $scope.warehouse = results.data;
                $scope.srch.WarehouseId = $scope.warehouse[0].WarehouseId;
                $scope.getWarehouseClusters();
                $scope.getdelboy();
            }, function (error) {
            });



            $scope.Cluster = [];

            $scope.getWarehouseClusters = function () {

                if ($scope.srch.WarehouseId) {
                    return $http.get(serviceBase + 'api/cluster/hubwise?WarehouseId=' + $scope.srch.WarehouseId).then(function (results) {
                        $scope.Cluster = results.data;
                        $scope.getdelboy();
                    });


                }
            };
            $scope.onNumPerPageChange = function () {
                
                $scope.vm.rowsPerPage = $scope.selected;
                $scope.getSearch(pageno);
            };
            
            $scope.changePage = function (pageno) {
                
                setTimeout(function () {
                    $scope.vm.currentPage = pageno;
                    $scope.getSearch(pageno);
                }, 100);

            };

            $scope.getSearch = function (pageno) {
                
                if ($scope.srch.WarehouseId > 0) {
                    $scope.customers = [];
                    $scope.clusterid = [];
                    var clusterId = '';
                    if ($scope.srch.ClusterId)
                        clusterId = $scope.srch.ClusterId;

                    var url = serviceBase + "api/AutoProcessorder/SearchData?WarehouseId=" + $scope.srch.WarehouseId + "&list=" + $scope.itemsPerPage + "&pageNo=" + pageno + "&clusterid=" + clusterId;
                    $http.get(url).success(function (response) {                       
                        $scope.total_count = response.total_count;
                        itemsPerPage: $scope.vm.rowsPerPage,
                        $scope.customers = response.ordermaster;
                        $scope.vm.count = $scope.total_count;
                        $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);
                        $scope.vm.currentPage = pageno;

                    });
                }
                else {

                    $scope.searchdata($scope.srch);
                }
            };

           
                $scope.delboy = [];
            $scope.getdelboy = function () {
                var url = serviceBase + "api/AutoProcessorder/GetDelBoy?warehouseId=" + $scope.srch.WarehouseId;
                $http.get(url).success(function (results) {
                    $scope.delboy = results;
                });


            };

          

            $scope.checkAll = function () {

                $scope.disable = "sccxz";
                if ($scope.selectedAll) {
                    $scope.selectedAll = false;
                } else {
                    $scope.selectedAll = true;
                }
                angular.forEach($scope.customers, function (trade) {
                    trade.check = $scope.selectedAll;
                });

            };

            $scope.selectedsettled = function () {
                
                if ($scope.deliveryBoyId == '') {
                    alert("Selecte Delivery Boy");
                    return;
                }

                if ($scope.customers.length == 0) {
                    alert("Select atleast one order to process.");
                    return;
                }
                //$scope.trade.check = length;
                $scope.assignedorders = [];
                $scope.selectedorders = [];
                for (var i = 0; i < $scope.customers.length; i++) {
                    if ($scope.customers[i].check == true) {
                        $scope.assignedorders.push($scope.customers[i]);
                    }

                }

                $scope.selectedorders = angular.copy($scope.assignedorders);
                var dataToPost = {
                    OrderDispatchs: $scope.selectedorders,
                    DeliveryBoyId: $scope.deliveryBoyId


                };

                var url = serviceBase + "api/OrderDispatchedDetails/AutoOrderProcess";

                $http.post(url, dataToPost)
                    .success(function (data) {
                        if (data != null) {
                            alert(data);
                            window.location.reload();
                        }
                        else
                        {
                            alert("Selected not succefully");
                        }

                    })
                    .error(function (data) {
                        alert(JSON.parse(data.ErrorMessage).ExceptionMessage);
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })


            }

            // update orderdispatch master
            $scope.ReDispatch = function (Dboy) {

                try { var obj = JSON.parse(Dboy); } catch (err) { alert("Select Delivery boy") }
                var dboyMob = 'obj.Mobile';
                console.log(dboyMob);
                $scope.Did = $scope.orderDetailsDisp[0].OrderDispatchedMasterId;
                var url = serviceBase + 'api/OrderDispatchedMaster?id=' + $scope.Did + '&DboyNo=' + 'obj.Mobile';
                $http.put(url)
                    .success(function (data) {

                        if (data.length > 0) {

                        }
                        alert("Delivey Boy update successfully");
                        window.location = "#/orderMaster";
                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);

                    });

            }



           

            $scope.Refresh = function () {
                $scope.srch = "";
                $scope.getData($scope.pageno);
            }

            $scope.getTemplate = function (data) {             
                if (data.OrderId === $scope.selectedd.OrderId) {
                    myfunc();
                    return 'edit';
                }
                else return 'display';
            };
            $scope.CheckETADate = function (d,id) {
                debugger;
                let today = new Date();
                let delivryDate = new Date(d.Deliverydate);
                delivryDate.setDate(delivryDate.getDate() - 2);
                if (today >= delivryDate) {
                } else {
                    document.getElementById('cst' + id).checked = false;
                    alert("Currently you are not able to dispatched this Order on " + delivryDate); return false;
                }
            }
            // end pagination
            $scope.set_color = function (trade) {
                if (trade.Status == "Process") {
                    return { background: "#81BEF7" }
                }
                else if (trade.Status == "Ready to Dispatch") {
                    return { background: "#00FF7F" }
                }
                else if (trade.Status == "Order Canceled") {
                    return { background: "#F78181" }
                }
            }
            // order History get data 
            $scope.oldHistroy = function (data) {


                $scope.dataordermasterHistrory = [];
                var url = serviceBase + "api/OrderMaster/orderhistory?orderId=" + data.OrderId;
                $http.get(url).success(function (response) {

                    $scope.dataordermasterHistrory = response;
                    console.log($scope.dataordermasterHistrory);
                    $scope.AddTrack("View(History)", "OrderId:", data.OrderId);
                })
                    .error(function (data) {
                    })
            }
            //end order History 
            // item History get data 
            $scope.itemHistroy = function (orderId) {


                $scope.dataordermasterHistrory1 = [];
                var url = serviceBase + "api/OrderMaster/Itemhistory?orderId=" + orderId;
                $http.get(url).success(function (response) {

                    $scope.dataordermasterHistrory1 = response;
                    console.log($scope.dataordermasterHistrory1);
                    $scope.AddTrack("View(History)", "OrderId:", data.OrderId);
                })
                    .error(function (data) {
                    })
            }
            //end order History 
            // status based filter
            $scope.filterOptions = {
                stores: [
                    { id: 1, name: 'Show All' },
                    { id: 2, name: 'Pending' },
                    { id: 3, name: 'Ready to Dispatch' },
                    { id: 4, name: 'Issued' },
                    { id: 12, name: 'Shipped' },
                    { id: 5, name: 'Delivery Redispatch' },
                    { id: 6, name: 'Delivered' },
                    { id: 7, name: 'sattled' },
                    { id: 8, name: 'Partial settled' },
                    { id: 9, name: 'Account settled' },
                    { id: 10, name: 'Delivery Canceled' },
                    { id: 11, name: 'Order Canceled' },
                    { id: 12, name: 'Post Order Canceled' }
                ]
            };

            $scope.filterItem = {
                store: $scope.filterOptions.stores[0]
            }
            $scope.customFilter = function (data) {
                if (data.Status === $scope.filterItem.store.name) {
                    return true;
                } else if ($scope.filterItem.store.name === 'Show All') {
                    return true;
                } else {
                    return false;
                }
            };

            $scope.editstatus = function (data) {

                $scope.selectedd = data;
                if (data.Status == "Delivery Canceled") {
                    $scope.statuses = [
                        { value: 1, text: "Order Canceled" }
                    ];
                }
                else if (data.Status == "Pending") {
                    $scope.statuses = [
                        { value: 1, text: "Order Canceled" }
                        // ,{ value: 2, text: "Process" }
                    ];
                }
                else {
                    // $scope.statuses = [
                    //{ value: 1, text: data.Status },
                    // ];
                    $scope.statuses = data.Status;
                }
            };



            $scope.reset = function () {
                $scope.selectedd = {};
            };

            $scope.updatestatus = function (data) {


                OrderMasterService.editstatus(data).then(function (results) {
                    $scope.reset();
                    return results;
                });
            }

            $(function () {

                $('input[name="daterange"]').daterangepicker({
                    timePicker: true,
                    timePickerIncrement: 5,
                    timePicker12Hour: true,
                    format: 'MM/DD/YYYY h:mm A'
                });
            });
            $(function () {
                $('input[name="daterangedata"]').daterangepicker({
                    timePicker: true,
                    timePickerIncrement: 5,
                    timePicker12Hour: true,
                    format: 'MM/DD/YYYY h:mm A'
                });
            });


            $scope.InitialData = [];
            $scope.DemandDetails = [];


            $scope.searchdata = function (data) {

                var f = $('input[name=daterangepicker_start]');
                var g = $('input[name=daterangepicker_end]');
                var start = f.val();
                var end = g.val();

                if (!$('#dat').val() && $scope.srch == "") {
                    start = null;
                    end = null;
                    alert("Please select one parameter");
                    return;
                }
               
                else if ($scope.srch == "" && $('#dat').val()) {
                    $scope.srch = { orderId: 0, skcode: "", shopName: "", mobile: "", status: '' }
                }
                else if ($scope.srch != "" && !$('#dat').val()) {
                    start = null;
                    end = null;
                    if (!$scope.srch.orderId) {
                        $scope.srch.orderId = 0;
                    }
                    if (!$scope.srch.skcode) {
                        $scope.srch.skcode = "";
                    }
                    if (!$scope.srch.shopName) {
                        $scope.srch.shopName = "";
                    }
                    if (!$scope.srch.mobile) {
                        $scope.srch.mobile = "";
                    }
                    if (!$scope.srch.status) {
                        $scope.srch.status = "";
                    }
                }
                else {
                    if (!$scope.srch.orderId) {
                        $scope.srch.orderId = 0;
                    }
                    if (!$scope.srch.skcode) {
                        $scope.srch.skcode = "";
                    }
                    if (!$scope.srch.shopName) {
                        $scope.srch.shopName = "";
                    }
                    if (!$scope.srch.mobile) {
                        $scope.srch.mobile = "";
                    }
                    if (!$scope.srch.status) {
                        $scope.srch.status = "";
                    }
                }
                $scope.orders = [];
                $scope.customers = [];
                //// time cunsume code  
                var stts = "";
                if ($scope.statusname.name && $scope.statusname.name != "Show All") {
                    stts = $scope.statusname.name;
                }
                var url = serviceBase + "api/SearchOrder?start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + stts + "&WarehouseId=" + $scope.srch.WarehouseId;
                $http.get(url).success(function (response) {
                    
                    $scope.customers = response;  //ajax request to fetch data into vm.data
                    $scope.total_count = response.length;
                    //$scope.orders = response;
                });

            }

            //............................Exel export Method...........................// Anil

            //SelfOrderExport..On DateRange and WareHouseWise.03/04/19./start/
            $scope.customers = [];
            $scope.selfData = function (data) {

                var start = "";
                var end = "";
                var f = $('input[name=daterangepicker_start]');
                var g = $('input[name=daterangepicker_end]');
                if (!$('#dat').val()) {
                    end = '';
                    start = '';
                    alert("Select Start and End Date")
                    return;
                }
                else {
                    start = f.val();
                    end = g.val();
                }
                var url = serviceBase + "api/OrderMaster/getselforder?start=" + start + "&end=" + end + "&WarehouseId=" + data.WarehouseId + "&SubcategoryName=" + data.SubcategoryName;

                $http.get(url).success(function (response) {
                    $scope.filterdata = response;
                    alasql('SELECT OrderId,SalesPerson,CustomerId,CustomerName,Skcode,ShopName,Status,invoice_no,Customerphonenum,BillingAddress,ShippingAddress,TotalAmount,GrossAmount,DiscountAmount,TaxAmount,CityId,WarehouseId,WarehouseName,SubcategoryName,ClusterId,ClusterName,latitute,longitute,Tin_No,CreatedDate,Deliverydate,UpdatedDate INTO XLSX("Ordermaster.xlsx",{headers:true}) FROM ? ', [response]);
                });
            };
            //$scope.selfData();

            //SelfOrderExport..On DateRange and WareHouseWise.03/04/19./end/
            $scope.dataforsearch1 = { Cityid: "", Warehouseid: "", datefrom: "", dateto: "" };
            $scope.statusname.name = "";

            $scope.exportData = function (data) {

                var f = $('input[name=daterangepicker_start]');
                var g = $('input[name=daterangepicker_end]');
                $scope.OrderByDate = [];
                var start = f.val();
                var end = g.val();

                if (!$('#dat').val() && $scope.srch == "") {
                    start = null;
                    end = null;
                    alert("Please select one parameter");
                    return;
                }
                else if ($scope.srch == "" && $('#dat').val()) {
                    $scope.srch = { orderId: 0, skcode: "", shopName: "", mobile: "", status: '' }
                }
                else if ($scope.srch != "" && !$('#dat').val()) {
                    start = null;
                    end = null;
                    if (!$scope.srch.orderId) {
                        $scope.srch.orderId = 0;
                    }
                    if (!$scope.srch.skcode) {
                        $scope.srch.skcode = "";
                    }
                    if (!$scope.srch.shopName) {
                        $scope.srch.shopName = "";
                    }
                    if (!$scope.srch.mobile) {
                        $scope.srch.mobile = "";
                    }
                    if (!$scope.srch.status) {
                        $scope.srch.status = "";
                    }
                }
                else {
                    if (!$scope.srch.orderId) {
                        $scope.srch.orderId = 0;
                    }
                    if (!$scope.srch.skcode) {
                        $scope.srch.skcode = "";
                    }
                    if (!$scope.srch.shopName) {
                        $scope.srch.shopName = "";
                    }
                    if (!$scope.srch.mobile) {
                        $scope.srch.mobile = "";
                    }
                    if (!$scope.srch.status) {
                        $scope.srch.status = "";
                    }
                }
                //var url = serviceBase + "api/SearchOrder?type=export&start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + $scope.srch.status;
                var url = serviceBase + "api/SearchOrder?type=export&start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + $scope.statusname.name + "&WarehouseId=" + $scope.srch.WarehouseId;


                $http.get(url).success(function (response) {

                    $scope.OrderByDate = response;

                    console.log("export");
                    if ($scope.OrderByDate.length <= 0) {
                        alert("No data available between two date ")
                    }
                    else {
                        $scope.NewExportData = [];
                        for (var i = 0; i < $scope.OrderByDate.length; i++) {

                            var OrderId = $scope.OrderByDate[i].OrderId;
                            var Skcode = $scope.OrderByDate[i].Skcode;
                            var ShopName = $scope.OrderByDate[i].ShopName;
                            var OrderBy = $scope.OrderByDate[i].OrderBy;
                            var Mobile = $scope.OrderByDate[i].Customerphonenum;
                            var CustomerName = $scope.OrderByDate[i].CustomerName;
                            var CustomerId = $scope.OrderByDate[i].CustomerId;
                            var WarehouseName = $scope.OrderByDate[i].WarehouseName;
                            var ClusterName = $scope.OrderByDate[i].ClusterName;
                            var Deliverydate = $scope.OrderByDate[i].Deliverydate;
                            var CompanyId = $scope.OrderByDate[i].CompanyId;
                            var BillingAddress = $scope.OrderByDate[i].BillingAddress;
                            var CreatedDate = $scope.OrderByDate[i].CreatedDate;
                            var Excecutive = $scope.OrderByDate[i].SalesPerson;
                            var ShippingAddress = $scope.OrderByDate[i].ShippingAddress;
                            var delCharge = $scope.OrderByDate[i].deliveryCharge;
                            var GST_NO = $scope.OrderByDate[i].Tin_No;
                            var Status = $scope.OrderByDate[i].Status;
                            var ReasonCancle = $scope.OrderByDate[i].ReasonCancle;
                            var comments = $scope.OrderByDate[i].comments;
                            var ItemMultiMRPId = $scope.OrderByDate[i].ItemMultiMRPId;
                            var orderDetails = $scope.OrderByDate[i].orderDetailsExport;
                            for (var j = 0; j < orderDetails.length; j++) {

                                var tts = {
                                    OrderId: '', Skcode: '', ShopName: '', Mobile: '', OrderBy: '', RetailerName: '', RetailerID: '', Warehouse: '', ClusterName: '', Deliverydate: '', CompanyId: '', BillingAddress: '', Date: '',
                                    Excecutive: '', ShippingAddress: '', deliveryCharge: '', GST_NO: '', ItemID: '', ItemName: '', SKU: '', itemNumber: '', MRP: '', MOQ: '', UnitPrice: '', Quantity: '', MOQPrice: '', Discount: '',
                                    DiscountPercentage: '', TaxPercentage: '', Tax: '', SGSTTaxPercentage: '', SGSTTaxAmmount: '', IGSTTaxPercentage: '', IGSTTaxAmmount: '', TotalAmt: '', CategoryName: '', BrandName: '', SubcategoryName: '', HSNCode: '', Status: '', ReasonCancle: '', comments: '', ItemMultiMRPId: '',
                                }
                                tts.ItemID = orderDetails[j].ItemId;
                                tts.ItemName = orderDetails[j].itemname;
                                tts.SKU = orderDetails[j].sellingSKU;
                                tts.itemNumber = orderDetails[j].itemNumber;
                                tts.MRP = orderDetails[j].price;
                                tts.UnitPrice = orderDetails[j].UnitPrice;
                                tts.Quantity = orderDetails[j].qty;
                                tts.MOQPrice = orderDetails[j].MinOrderQtyPrice;
                                tts.Discount = orderDetails[j].DiscountAmmount;
                                tts.DiscountPercentage = orderDetails[j].DiscountPercentage;
                                tts.TaxPercentage = orderDetails[j].TaxPercentage;
                                tts.Tax = orderDetails[j].TaxAmmount;
                                tts.SGSTTaxPercentage = orderDetails[j].SGSTTaxPercentage;
                                tts.SGSTTaxAmmount = orderDetails[j].SGSTTaxAmmount;
                                tts.IGSTTaxPercentage = orderDetails[j].IGSTTaxPercentage;
                                tts.IGSTTaxAmmount = orderDetails[j].IGSTTaxAmmount;
                                tts.TotalAmt = orderDetails[j].TotalAmt;
                                tts.CategoryName = orderDetails[j].CategoryName;
                                tts.BrandName = orderDetails[j].BrandName;
                                tts.SubcategoryName = orderDetails[j].SubcategoryName;
                                tts.HSNCode = orderDetails[j].HSNCode;
                                tts.OrderId = OrderId;
                                tts.RetailerID = CustomerId;
                                tts.OrderBy = OrderBy;
                                tts.Skcode = Skcode;
                                tts.Mobile = Mobile;
                                tts.RetailerName = CustomerName;
                                tts.Warehouse = WarehouseName;
                                tts.ClusterName = ClusterName;
                                tts.ShopName = ShopName;
                                tts.Deliverydate = Deliverydate;
                                tts.CompanyId = CompanyId;
                                tts.BillingAddress = BillingAddress;
                                tts.Date = CreatedDate;
                                tts.Excecutive = Excecutive;
                                tts.ShippingAddress = ShippingAddress;
                                tts.deliveryCharge = delCharge;
                                tts.GST_NO = GST_NO;
                                tts.Status = Status;
                                tts.ReasonCancle = ReasonCancle;
                                tts.comments = comments;
                                tts.ItemMultiMRPId = ItemMultiMRPId;
                                $scope.NewExportData.push(tts);

                            }
                        }
                        alasql.fn.myfmt = function (n) {
                            return Number(n).toFixed(2);
                        }
                        alasql('SELECT OrderId,Skcode,ShopName,RetailerName,Mobile,ItemID,ItemName,CategoryName,BrandName,SubcategoryName,HSNCode,SKU,Warehouse,ClusterName,latitute,longitute,Date,OrderBy,Excecutive,MRP,UnitPrice,MOQPrice,Quantity,Discount,DiscountPercentage,Tax,TaxPercentage,SGSTTaxAmmount,SGSTTaxPercentage,IGSTTaxAmmount,IGSTTaxPercentage,TotalAmt,deliveryCharge,GST_NO,Status,ReasonCancle,comments,ItemMultiMRPId INTO XLSX("OrderDetails.xlsx",{headers:true}) FROM ?', [$scope.NewExportData]);
                    }
                });
            };
            //-------------------------------------------------------------------------//

            //............................Exel export Method...........................// Anil
            $scope.dataforsearch1 = { Cityid: "", Warehouseid: "", datefrom: "", dateto: "" };
            $scope.exportDataAll = function (data) {

                var f = $('input[name=daterangepicker_start]');
                var g = $('input[name=daterangepicker_end]');
                $scope.OrderByDate = [];
                var start = f.val();
                var end = g.val();

                if (!$('#dat').val() && $scope.srch == "") {
                    start = null;
                    end = null;
                    alert("Please select one parameter");
                    return;
                }
                else if ($scope.srch == "" && $('#dat').val()) {
                    $scope.srch = { orderId: 0, skcode: "", shopName: "", mobile: "", status: '' }
                }
                else if ($scope.srch != "" && !$('#dat').val()) {
                    start = null;
                    end = null;
                    if (!$scope.srch.orderId) {
                        $scope.srch.orderId = 0;
                    }
                    if (!$scope.srch.skcode) {
                        $scope.srch.skcode = "";
                    }
                    if (!$scope.srch.shopName) {
                        $scope.srch.shopName = "";
                    }
                    if (!$scope.srch.mobile) {
                        $scope.srch.mobile = "";
                    }
                    if (!$scope.srch.status) {
                        $scope.srch.status = "";
                    }
                }
                else {
                    if (!$scope.srch.orderId) {
                        $scope.srch.orderId = 0;
                    }
                    if (!$scope.srch.skcode) {
                        $scope.srch.skcode = "";
                    }
                    if (!$scope.srch.shopName) {
                        $scope.srch.shopName = "";
                    }
                    if (!$scope.srch.mobile) {
                        $scope.srch.mobile = "";
                    }
                    if (!$scope.srch.status) {
                        $scope.srch.status = "";
                    }
                }
                var url = serviceBase + "api/SearchOrder/all?type=export&start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + $scope.srch.status;
                $http.get(url).success(function (response) {
                    $scope.OrderByDate = response;
                    console.log("export");
                    if ($scope.OrderByDate.length <= 0) {
                        alert("No data available between two date ")
                    }
                    else {
                        $scope.NewExportData = [];
                        for (var i = 0; i < $scope.OrderByDate.length; i++) {

                            var OrderId = $scope.OrderByDate[i].OrderId;
                            var Skcode = $scope.OrderByDate[i].Skcode;
                            var ShopName = $scope.OrderByDate[i].ShopName;
                            var OrderBy = $scope.OrderByDate[i].OrderBy;
                            var Mobile = $scope.OrderByDate[i].Customerphonenum;
                            var CustomerName = $scope.OrderByDate[i].CustomerName;
                            var CustomerId = $scope.OrderByDate[i].CustomerId;
                            var WarehouseName = $scope.OrderByDate[i].WarehouseName;
                            var ClusterName = $scope.OrderByDate[i].ClusterName;
                            var Deliverydate = $scope.OrderByDate[i].Deliverydate;
                            var CompanyId = $scope.OrderByDate[i].CompanyId;
                            var BillingAddress = $scope.OrderByDate[i].BillingAddress;
                            var CreatedDate = $scope.OrderByDate[i].CreatedDate;
                            var SalesPerson = $scope.OrderByDate[i].SalesPerson;
                            var ShippingAddress = $scope.OrderByDate[i].ShippingAddress;
                            var delCharge = $scope.OrderByDate[i].deliveryCharge;
                            var Status = $scope.OrderByDate[i].Status;
                            var ReasonCancle = $scope.OrderByDate[i].ReasonCancle;
                            var comments = $scope.OrderByDate[i].comments;
                            var ItemMultiMRPId = $scope.OrderByDate[i].ItemMultiMRPId;
                            var orderDetails = $scope.OrderByDate[i].orderDetailsExport;
                            for (var j = 0; j < orderDetails.length; j++) {
                                var tts = {
                                    OrderId: '', Skcode: '', ShopName: '', Mobile: '', OrderBy: '', RetailerName: '', RetailerID: '', Warehouse: '', ClusterName: '', Deliverydate: '', CompanyId: '', BillingAddress: '', Date: '',
                                    Excecutive: '', ShippingAddress: '', deliveryCharge: '', ItemID: '', ItemName: '', SKU: '', itemNumber: '', MRP: '', MOQ: '', UnitPrice: '', Quantity: '', MOQPrice: '', Discount: '',
                                    DiscountPercentage: '', TaxPercentage: '', Tax: '', TotalAmt: '', CategoryName: '', BrandName: '', SubcategoryName: '', Status: '', ReasonCancle: '', comments: '', ItemMultiMRPId: '',
                                }
                                tts.ItemID = orderDetails[j].ItemId;
                                tts.ItemName = orderDetails[j].itemname;
                                tts.SKU = orderDetails[j].sellingSKU;
                                tts.itemNumber = orderDetails[j].itemNumber;
                                tts.MRP = orderDetails[j].price;
                                tts.UnitPrice = orderDetails[j].UnitPrice;
                                tts.Quantity = orderDetails[j].qty;
                                tts.MOQPrice = orderDetails[j].MinOrderQtyPrice;
                                tts.Discount = orderDetails[j].DiscountAmmount;
                                tts.DiscountPercentage = orderDetails[j].DiscountPercentage;
                                tts.TaxPercentage = orderDetails[j].TaxPercentage;
                                tts.Tax = orderDetails[j].TaxAmmount;
                                tts.TotalAmt = orderDetails[j].TotalAmt;
                                tts.CategoryName = orderDetails[j].CategoryName;
                                tts.BrandName = orderDetails[j].BrandName;
                                tts.SubcategoryName = orderDetails[j].SubcategoryName;

                                tts.OrderId = OrderId;
                                tts.RetailerID = CustomerId;
                                tts.OrderBy = OrderBy;
                                tts.Skcode = Skcode;
                                tts.Mobile = Mobile;
                                tts.RetailerName = CustomerName;
                                tts.Warehouse = WarehouseName;
                                tts.ClusterName = ClusterName;
                                tts.ShopName = ShopName;
                                tts.Deliverydate = Deliverydate;
                                tts.CompanyId = CompanyId;
                                tts.BillingAddress = BillingAddress;
                                tts.Date = CreatedDate;
                                tts.Excecutive = SalesPerson;
                                tts.ShippingAddress = ShippingAddress;
                                tts.deliveryCharge = delCharge;
                                tts.Status = Status;
                                tts.ReasonCancle = ReasonCancle;
                                tts.comments = comments;
                                tts.ItemMultiMRPId = ItemMultiMRPId;
                                $scope.NewExportData.push(tts);

                            }
                        }
                        alasql.fn.myfmt = function (n) {
                            return Number(n).toFixed(2);
                        }
                        alasql('SELECT OrderId,Skcode,ShopName,RetailerName,Mobile,ItemID,ItemName,CategoryName,BrandName, SubcategoryName,SKU,Warehouse,ClusterName,latitute,longitute,Date,OrderBy,Excecutive,MRP,UnitPrice,MOQPrice,Quantity,Discount,DiscountPercentage,Tax,TaxPercentage,TotalAmt,deliveryCharge,Status,ReasonCancle,comments,ItemMultiMRPId INTO XLSX("OrderDetails.xlsx",{headers:true}) FROM ?', [$scope.NewExportData]);
                    }
                });
            };
            //-------------------------------------------------------------------------//




            $scope.show = true;
            $scope.order = false;

            $scope.showalldetails = function () {
                $scope.order = !$scope.order;
                $scope.show = !$scope.show;
            };

            $scope.opendelete = function (data, $index) {

                var myData = { all: $scope.orders, order1: data };
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myModaldeleteOrder.html",
                        controller: "ModalInstanceCtrldeleteOrder",
                        resolve:
                        {
                            order: function () {
                                return myData
                            }
                        }
                    });
                modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
            };

            $scope.callmethod = function () {
                var init;
                $scope.stores = $scope.orders;
                $scope.searchKeywords = "";
                $scope.filteredStores = [];
                $scope.row = "";



                $scope.numPerPageOpt = [3, 5, 10, 20];
                $scope.numPerPage = $scope.numPerPageOpt[2];
                $scope.currentPage = 1;
                $scope.currentPageStores = [];
                $scope.search(); $scope.select(1);
                $scope.apply();

            };
            $scope.select = function (page) {
                //
                var end, start; console.log("select"); console.log($scope.stores);
                start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage; $scope.currentPageStores = $scope.filteredStores.slice(start, end);
            }

            $scope.onFilterChange = function () {
                console.log("onFilterChange"); console.log($scope.stores);
                $scope.select(1); $scope.currentPage = 1; $scope.row = "";
            }

            $scope.onNumPerPageChange = function () {
                
                console.log("onNumPerPageChange"); console.log($scope.stores);
                return $scope.select(1), $scope.currentPage = 1
            },

            $scope.onOrderChange = function () {
                console.log("onOrderChange"); console.log($scope.stores);
                $scope.select(1); $scope.currentPage = 1;
            }

            $scope.search = function () {
                console.log("search");
                console.log($scope.stores);
                console.log($scope.searchKeywords);

                $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords); $scope.onFilterChange();
            }

            $scope.order = function (rowName) {
                console.log("order"); console.log($scope.stores);
                $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0;
            }

            $scope.Return = function (data) {

                OrderMasterService.saveReturn(data);
                console.log("Order Detail Dialog called ...");
            };
            $scope.mydata = {};

            $scope.showDetail = function (data) {

                $scope.myData = {};
                $http.get(serviceBase + 'api/OrderMaster?id=' + data.OrderId).then(function (results) {
                    $scope.myData = results.data;
                angular.forEach($scope.customers, function (value, key) {
                    if (value.OrderId == data.OrderId) {
                        
                        angular.forEach(value.orderDetails, function (valued, key) {
                            angular.forEach($scope.myData.orderDetails, function (valuemd, key) {
                                if (valued.OrderDetailsId == valuemd.OrderDetailsId) {
                                    valuemd.qty = valued.qty;
                                    valuemd.Noqty = valued.Noqty;
                                };
                            });
                        });
                    };

                });

                  // $scope.myData = results.data;
                    OrderMasterService.save($scope.myData)
                    setTimeout(function () {
                        console.log("Modal opened Orderdetails");
                        var modalInstance;
                        modalInstance = $modal.open(
                            {
                                templateUrl: "myOrderdetail.html",
                                controller: "ProcessorderdetailsController", resolve: { Ord: function () { return $scope.items } }
                            });
                        modalInstance.result.then(function (selectedItem) {
                            
                            $scope.UpdatedOrderData = selectedItem.orderData;
                            OrderMasterService.save($scope.UpdatedOrderData);
                             angular.forEach($scope.customers, function (value, key) {
                                if (value.OrderId == $scope.UpdatedOrderData.OrderId) {
                                    value.orderDetails = $scope.UpdatedOrderData.orderDetails;
                                };

                             });

                            console.log("modal close");
                            console.log(selectedItem);
                            if (selectedItem.Status == "Ready to Dispatch") {
                                $("#st" + selectedItem.OrderId).html(selectedItem.Status);
                                $("#st" + selectedItem.OrderId).removeClass('canceled');
                                $('#re' + selectedItem.OrderId).hide();
                            }
                        },
                            function (selectedItem) {
                                 
                                $scope.UpdatedOrderData = selectedItem.orderData;
                                OrderMasterService.save($scope.UpdatedOrderData);
                                angular.forEach($scope.customers, function (value, key) {
                                    
                                    if (value.OrderId == $scope.UpdatedOrderData.OrderId) {
                                        value.orderDetails = $scope.UpdatedOrderData.orderDetails;
                                    };

                                });

                                var isChecked = false;
                                if (selectedItem.list && selectedItem.list.length > 0) {

                                    selectedItem.list.forEach(function (item) {
                                        if (item.qty > item.CurrentStock) {
                                            isChecked = true;
                                        }
                                    });
                                }


                                if (!isChecked) {
                                    $("#cst" + selectedItem.orderData.OrderId).attr('disabled', false);
                                    $("#tr" + selectedItem.orderData.OrderId).removeAttr("style");
                                }

                                

                                console.log("Cancel Condintion");

                            })
                    }, 500);

                });
                console.log("Order Detail Dialog called ...");
            };

            $scope.showInvoice = function (data) {

                OrderMasterService.save1(data);
                console.log("Order Invoice  called ...");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myModaldeleteOrderInvoice.html",
                        controller: "ModalInstanceCtrlOrderInvoice",
                        resolve:
                        {
                            order: function () {
                                return data
                            }
                        }
                    });
                modalInstance.result.then(function () {
                },
                    function () {
                        console.log("Cancel Condintion");

                    })
            };


            $scope.open = function (data) {


                OrderMasterService.save(data);
            };

            $scope.invoice = function (data) {
                OrderMasterService.view(data).then(function (results) {
                }, function (error) {
                });
            };

            $scope.callmethoddetails = function () {
                var init;
                $scope.stores = $scope.orderDetails;
                $scope.searchKeywords = "";
                $scope.filteredStores = [];
                $scope.row = "";

               

                $scope.numPerPageOpt = [3, 5, 10, 20];
                $scope.numPerPage = $scope.numPerPageOpt[2];
                $scope.currentPage = 1;
                $scope.currentPageStores = [];
                $scope.search(); $scope.select(1);
            }
            $scope.select = function (page) {
                var end, start; console.log("select"); console.log($scope.stores);
                return start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage; $scope.currentPageStores = $scope.filteredStores.slice(start, end);
            }

            $scope.onFilterChange = function () {
                console.log("onFilterChange"); console.log($scope.stores);
                return $scope.select(1); $scope.currentPage = 1; $scope.row = "";
            }

            $scope.onNumPerPageChange = function () {
                console.log("onNumPerPageChange"); console.log($scope.stores);
                return $scope.select(1), $scope.currentPage = 1
            },

            $scope.onOrderChange = function () {
                console.log("onOrderChange"); console.log($scope.stores);
                return $scope.select(1); $scope.currentPage = 1;
            }

            $scope.search = function () {
                console.log("search");
                console.log($scope.stores);
                console.log($scope.searchKeywords);

                return  $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords); $scope.onFilterChange();
            }

            $scope.order = function (rowName) {
                console.log("order"); console.log($scope.stores);
                $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0;
            }

            $scope.chkdb = true;
            $scope.DBoys = [];
            DeliveryService.getdboys().then(function (results) {
                $scope.DBoys = results.data;
            }, function (error) {
            });


            $scope.AsignBoyorders = function () {
                $scope.chkdb = false;
            }


            $scope.Mobile = '';
            $scope.orderAutomation = function () {

                if ($scope.Mobile != null && $scope.Mobile != undefined) {


                    var modalInstance;
                    modalInstance = $modal.open(
                        {
                            templateUrl: "orderAutomation.html",
                            controller: "orderAutomationCtrl", resolve: { mobile: function () { return $scope.Mobile } }
                        })
                }
                else {
                    alert();
                }
            }
        }
       
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlOrderDetail', ModalInstanceCtrlOrderDetail);

    ModalInstanceCtrlOrderDetail.$inject = ["$scope", '$http', 'OrderMasterService', "$modalInstance", 'ngAuthSettings', 'order'];

    function ModalInstanceCtrlOrderDetail($scope, $http, OrderMasterService, $modalInstance, ngAuthSettings, order) {
        console.log("order detail modal opened");

        $scope.orderDetails = [];
        if (order) {
            $scope.OrderData = order;

            $scope.orderDetails = $scope.OrderData.orderDetails;
            console.log("found order");
            console.log($scope.OrderData);
            console.log($scope.OrderData.orderDetails);
            console.log($scope.orderDetails);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.Details = function (dataToPost, $index) {

            console.log("Show Order Details  controller");
            console.log(dataToPost);

            OrderMasterService.getDeatil.then(function (results) {

                console.log("Details");

                console.log("index of item " + $index);
                console.log($scope.order.length);
                $scope.order.splice($index, 1);
                console.log($scope.order.length);

                $modalInstance.close(dataToPost);
                ReloadPage();

            }, function (error) {
                alert(error.data.message);
            });
        }
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlOrderInvoice', ModalInstanceCtrlOrderInvoice);

    ModalInstanceCtrlOrderInvoice.$inject = ["$scope", '$http', 'OrderMasterService', "$modalInstance", 'ngAuthSettings', 'order'];

    function ModalInstanceCtrlOrderInvoice($scope, $http, OrderMasterService, $modalInstance, ngAuthSettings, order) {

        console.log("order invoice modal opened");

        $scope.OrderDetails = {};
        $scope.OrderData = {};
        var d = OrderMasterService.getDeatil();

        $scope.OrderData = d;
        $scope.orderDetails = d.orderDetails;


        $scope.Itemcount = 0;


        for (var i = 0; i < $scope.orderDetails.length; i++) {


            $scope.Itemcount = $scope.Itemcount + $scope.orderDetails[i].qty;
        }

        $scope.totalfilterprice = 0;
        _.map($scope.OrderData.orderDetails, function (obj) {
            console.log("count total");

            $scope.totalfilterprice = $scope.totalfilterprice + obj.TotalAmt;
            console.log(obj.TotalAmt);
            console.log($scope.totalfilterprice);

        })
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrldeleteOrder', ModalInstanceCtrldeleteOrder);

    ModalInstanceCtrldeleteOrder.$inject = ["$scope", '$http', 'OrderMasterService', "$modalInstance", 'ngAuthSettings', 'order'];

    function ModalInstanceCtrldeleteOrder($scope, $http, OrderMasterService, $modalInstance, ngAuthSettings, myData) {
        console.log("delete modal opened");
        function ReloadPage() {
            location.reload();
        }

        $scope.orders = [];


        if (myData) {

            $scope.orders = myData.order1;

        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


        $scope.deleteorders = function (dataToPost, $index) {

            console.log("Delete  controller");
            console.log(dataToPost);
            OrderMasterService.deleteorder(dataToPost).then(function (results) {
                console.log("Del");

                // myData.all.splice($index, 1);

                $modalInstance.close(dataToPost);
                //ReloadPage();

            }, function (error) {
                alert(error.data.message);
            });
        };

    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('orderAutomationCtrl', orderAutomationCtrl);

    orderAutomationCtrl.$inject = ["$scope", '$http', '$modal', 'DeliveryService', "$modalInstance", 'ordersService', "mobile"];

    function orderAutomationCtrl($scope, $http, $modal, DeliveryService, $modalInstance, ordersService, mobile) {

        $scope.chkdb1 = true;
        $scope.DisableProceed = function () {
            $scope.chkdb1 = false;
        }
        var dates = $('#date').val();
        var splitedDate = dates.split('-');
        var startDate = splitedDate[0];
        var endDate = splitedDate[1];
        var url = serviceBase + 'api/OrderMaster/GetOrderData?startdate=' + startDate + '&enddate=' + endDate;
        $http.get(url).then(function (results) {
            $scope.PendingOrderByDate = results.data;
        });
        $scope.checkAll = function () {
            if ($scope.selectedAll) {
                $scope.selectedAll = false;
            } else {
                $scope.selectedAll = true;
            }
            angular.forEach($scope.PendingOrderByDate, function (trade) {
                trade.check = $scope.selectedAll;
            });

        };
        $scope.DBoys = [];
        DeliveryService.getdboys().then(function (results) {
            $scope.DBoys = results.data;
        }, function (error) {
        });
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.time = 24;
        var alrt = {};
        $scope.Proceed = function (Mobile) {

            if (Mobile == "") {
                alert('Please select Delivary Boy')
            }
            else {

                $("#sM" + Mobile).prop("disabled", true);

                $scope.assignedorders = [];
                for (var i = 0; i < $scope.PendingOrderByDate.length; i++) {
                    if ($scope.PendingOrderByDate[i].check == true) {
                        $scope.assignedorders.push($scope.PendingOrderByDate[i]);
                    }
                }
                var data = {
                    assignedorders: $scope.assignedorders,
                    mobile: Mobile
                }
                var url = serviceBase + 'api/OrderMaster/priority';
                $http.post(url, data).then(function (results) {
                    $modalInstance.close();
                    if (results.statusText == "OK") {
                        alert('Order Updated Successfully');
                        location.reload();
                        alrt.msg = "Order Update successfully";
                        $modal.open(
                            {
                                templateUrl: "PopUpModel.html",
                                controller: "PopUpController", resolve: { message: function () { return alrt.msg } }
                            },
                            function () {
                            })
                    }
                });
            }



        }

    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlQuantityeditAuto', ModalInstanceCtrlQuantityeditAuto);

    ModalInstanceCtrlQuantityeditAuto.$inject = ["$scope", '$http', "$modalInstance", "inventory"];

    function ModalInstanceCtrlQuantityeditAuto($scope, $http, $modalInstance, inventory) {
        console.log("order invoice modal opened");
        
         
        $scope.xy = true;
        $scope.inventoryData = {};
        $scope.isRemoveStyle = inventory.OrderData.IsLessCurrentStock;
        if (inventory.OrderData) {
            $scope.OrderData = inventory.OrderData;
            $scope.orderDetails = $scope.OrderData.orderDetails;
        }
        if (inventory.OrderDetail) {
            console.log("category if conditon");
            $scope.Quantity = inventory.OrderDetail;
        }
        $scope.updatelineitem = function (data) {

            if (data.qty >= 0) {
                if ($scope.Quantity.qty <= 0) {

                    alert("Quantiy should not be negative");
                    $scope.Quantity.qty = 0;
                } else {
                    if (data.MinOrderQty > 0) {
                        $scope.Quantity.qty = data.qty - data.MinOrderQty;
                    } else {
                        $scope.Quantity.qty = data.qty - 1;
                    }
                }
            }
        }
        $scope.updatelineitem1 = function (data) {

            if (data.MinOrderQty == 0) {
                $scope.Quantity.qty = data.qty + 1;
            }
            else if (data.qty != data.Noqty) {
                $scope.Quantity.qty = data.qty + data.MinOrderQty;
            }
            else {
                alert("Quantiy should not be greater then Max Quantity")
            }
        }
        $scope.ok = function () {
             
            if ($scope.Quantity.QtyChangeReason == undefined) {
                alert("Select reason");
            } else {
                // 
                // add for free  item qty change
                var url = serviceBase + 'api/OrderDispatchedMaster/GetFreebiesItem?OrderId=' + $scope.Quantity.OrderId + '&ItemNumber=' + $scope.Quantity.itemNumber + '&WarehouseId=' + $scope.Quantity.WarehouseId;
                $http.get(url).success(function (results) {
                    $http.get(url).success(function (results) {
                        var freebiesdata = results;
                        
                        if (freebiesdata != null) {
                            angular.forEach($scope.orderDetails, function (value, key) {

                                if (value.ItemId == freebiesdata.FreeItemId) {

                                    var multiply = $scope.Quantity.qty / freebiesdata.MinOrderQuantity;
                                    var totalquantity = parseInt(multiply) * freebiesdata.NoOffreeQuantity;
                                    value.qty = totalquantity;
                                    value.Noqty = totalquantity;

                                };
                            });
                        }
                    });
                    $scope.OrderData.IsLessCurrentStock = $scope.isRemoveStyle;
                    $scope.OrderData.orderDetails = $scope.orderDetails;
                    $modalInstance.close($scope.OrderData);
                });

                //end
                //for (var i = 0; i < $scope.Quantity.length; i++) {
                //    //
                //    if ($scope.Quantity[i].qty > $scope.Quantity[i].CurrentStock)
                //        //$scope.isRemoveStyle = false;
                //}
                //$scope.OrderData.IsLessCurrentStock = $scope.isRemoveStyle;
                //// add for free  item qty change
                //var url = serviceBase + 'api/OrderDispatchedMaster/GetFreebiesItem?OrderId=' + $scope.Quantity.OrderId + '&ItemId=' + $scope.Quantity.ItemId;
                //$http.get(url).success(function (results) {
                //    var freebiesdata = results;
                //    if (freebiesdata != null) {
                //        $scope.orderDetails = $scope.OrderData.orderDetails;
                //        angular.forEach($scope.Quantity, function (value, key) {

                //            if (value.ItemId == freebiesdata.FreeItemId) {

                //                var multiply = $scope.Quantity.qty / freebiesdata.MinOrderQuantity;
                //                var totalquantity = parseInt(multiply) * freebiesdata.NoOffreeQuantity;
                //                value.qty = totalquantity;
                //                value.Noqty = totalquantity;

                //            };
                //        });
                //    }
                //});

                //end
                //$modalInstance.close($scope.OrderData);
            }
        };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.Putinventory = function (data) {

            $scope.orderDetail.qty = data.qty;
            $scope.orderDetail.QtyChangeReason = data.OrderDetailsId;
        }
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ProcessorderdetailsController', ProcessorderdetailsController);

    ProcessorderdetailsController.$inject = ['$scope', 'OrderMasterService', 'OrderDetailsService', '$filter', 'WarehouseService', '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", "peoplesService", '$modal', 'BillPramotionService', "$modalInstance"];

    function ProcessorderdetailsController($scope, OrderMasterService, OrderDetailsService, $filter, WarehouseService, $http, $window, $timeout, ngAuthSettings, ngTableParams, peoplesService, $modal, BillPramotionService, $modalInstance) {
        $scope.ok = function () { $modalInstance.close($scope.UpdatedOrderData); };
        $scope.cancel = function () { $modalInstance.dismiss({ orderData: $scope.UpdatedOrderData }); };//, list: $scope.orderDetails
        console.log("orderdetailsController start loading OrderDetailsService");
        

        $scope.Payments = {};
        $scope.currentPageStores = {};
        $scope.isShoW = false;
        $scope.isShoWOffer = false;
        $scope.finaltable = false;
        $scope.dispatchtable = false;
        $scope.OrderDetails = {};
        $scope.OrderData = {};
        $scope.OrderData1 = {};

        var d = OrderMasterService.getDeatil();
        console.log(d);
        $scope.signdata = {};
        $scope.viewsign = function () {

            var Signimg = $scope.signdata.Signimg;
            console.log("$scope.signdata");
            if (Signimg != null) {
                window.open(Signimg);
            } else { alert("no sign present") }
        }
        $scope.count = 1;
        $scope.OrderData = d;
        console.log("$scope.OrderDatamk");
        console.log($scope.OrderData);
        if ($scope.OrderData.Status == "Order Canceled") { //$scope.OrderData.Status == "Cancel" ||
            $scope.finaltable = false;
            $scope.dispatchtable = false;
            
        }
        if ($scope.OrderData.Status == "Ready to Dispatch") {
            
        }

        $scope.orderDetails = d.orderDetails;
        $scope.orderDetailsINIT = d.orderDetails;
        $scope.checkInDispatchedID = $scope.orderDetails[0].OrderId;
        $scope.pramotions = {};
        $scope.selectedpramotion = {};
        BillPramotionService.getbillpramotion().then(function (results) {
            $scope.pramotions = results.data;
        }, function (error) {
        });
        //for display info in print final order
        OrderMasterService.saveinfo($scope.OrderData);
        // end 
        $scope.Finalbutton = false;
        $scope.displayDiscountamount = true;
        if ($scope.OrderData.Status == "Cancel") {
            $scope.cancledispatch = true;
            $scope.Finalbutton = true;
            $scope.ReDispatchButton = true;
        }
        $scope.callForDropdown = function () {

            var url = serviceBase + 'api/OrderDispatchedMaster?id=' + $scope.checkInDispatchedID;
            $http.get(url)
                .success(function (data) {

                    if (data == "null") {
                        $scope.dispatchtable = false;
                    } else {
                        $scope.signdata = data;

                        OrderMasterService.saveDispatch(data);
                        $scope.dispatchtable = true;
                        $scope.DBname = {};
                        $scope.DBname = data.DboyName;
                        $scope.OrderData1 = data;
                        if ($scope.OrderData1.ReDispatchCount > 1) {
                            $scope.ReDispatchButton = true;
                        }
                    }
                });
        };
        $scope.callForDropdown();
        // checking order is dispatched or not here
        var url = serviceBase + 'api/OrderDispatchedDetails?id=' + $scope.checkInDispatchedID;
        $http.get(url).success(function (data1) {

            if (data1.length > 0) {
                $scope.count = 0;
                $scope.orderDetails11 = data1;
                $scope.orderDetailsDisp = data1;
                $scope.msg = "Order is Dispatched";
                document.getElementById("btnSave").hidden = true;
            }
            $http.get(serviceBase + "api/freeitem/SkFree?oderid=" + $scope.checkInDispatchedID).then(function (results) {

                $scope.freeitem = results.data;
                try {
                    if (results.data.length > 0) {
                        $scope.isShoW = true;
                    }
                } catch (ex) { }
            }, function (error) {
            });
            $http.get(serviceBase + "api/offer/GetOfferItem?oderid=" + $scope.checkInDispatchedID).then(function (results) {
                $scope.Offeritem = results.data;
                try {
                    if (results.data.length > 0) {
                        $scope.isShoWOffer = true;
                    }
                } catch (ex) { }
            }, function (error) {
            });
        });
        //end

        // check Final master Sattled order done
        var url5 = serviceBase + 'api/OrderDispatchedMasterFinal?id=' + $scope.checkInDispatchedID;//instead of url used url5
        $http.get(url5).success(function (data1) {

            if (data1 == "null") {
                $scope.SHOWPAYMENT = false;
            } else {
                $scope.SHOWPAYMENT = true;
                $scope.SHOWPAYMENTTABLE = data1;
                $scope.Finalbutton = true;
                $scope.myMasterbutton = true;
                $scope.finalLastReturn = true;
                //$scope.cancledispatch = true;
                $scope.FinalbuttonLAST = true;
                $scope.cancledispatch = true;
                $scope.ReDispatchButton = true;
                $scope.FinalinvoiceButtonWithoutMasterLast = true;
            }
        });
        //end

        // update orderdispatch master
        $scope.ReDispatch = function (Dboy) {
            try { var obj = JSON.parse(Dboy); } catch (err) { alert("Select Delivery boy") }
            var dboyMob = 'obj.Mobile';
            console.log(dboyMob);
            $scope.Did = $scope.orderDetailsDisp[0].OrderDispatchedMasterId;
            var url = serviceBase + 'api/OrderDispatchedMaster?id=' + $scope.Did + '&DboyNo=' + 'obj.Mobile';
            $http.put(url)
                .success(function (data) {

                    if (data.length > 0) {

                    }
                    alert("Delivey Boy update successfully");
                    window.location = "#/orderMaster";
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);

                });

        }
        // end

        $scope.Itemcount = 0;
        ///check for return

        var url2 = serviceBase + 'api/OrderDispatchedDetailsReturn?id=' + $scope.checkInDispatchedID;//instead of url used url2
        $http.get(url2).success(function (data) {

            if (data.length > 0) {
                $scope.count = 0;
                $scope.Finalbutton = true;  // disable chckbox
               
                $scope.orderDetailsR = data;
                $scope.myVar = true;
                $scope.myVar1 = true;
                $scope.finalobj = [];
                document.getElementById("FinalInvoice").hidden = false;
                for (var i = 0; i < $scope.orderDetailsINIT.length; i++) {
                    for (var j = 0; j < $scope.orderDetailsR.length; j++) {
                        if ($scope.orderDetailsINIT[i].ItemId == $scope.orderDetailsR[j].ItemId) {
                           
                            $scope.orderDetailsINIT[i].qty = $scope.orderDetailsINIT[i].qty - $scope.orderDetailsR[j].qty;

                            
                            $scope.finalobj.push($scope.orderDetailsINIT[i]);
                        }
                    }
                }
            }
            else {
                $scope.myVar = false;
                //document.getElementById("btnFinalInvoice").hidden = true;
            }
        });
        //end check return
        $scope.settleAmountLast = function () {

            return $scope.PaymentsLAST.CheckAmount + $scope.PaymentsLAST.ElectronicAmount + $scope.PaymentsLAST.CashAmount;
        }

        $scope.duensettleequalLAST = function () {
            var payamount = Math.round($scope.myMaster.GrossAmount);
            if ($scope.settleAmountLast() == payamount) {
                return true;
            } else {

                return false;
            }
        }

        $scope.settleAmount = function () {

            return $scope.Payments.CheckAmount + $scope.Payments.ElectronicAmount + $scope.Payments.CashAmount;
        }

        $scope.duensettleequal = function () {

            if ($scope.settleAmount() == $scope.OrderData1.GrossAmount) {
                return true;
            } else {

                return false;
            }
        }

        //check for final detail Sattled
        var url3 = serviceBase + 'api/OrderDispatchedDetailsFinal?id=' + $scope.checkInDispatchedID;//instead of url used url3
        $http.get(url3).success(function (data) {
            if (data.length > 0) {

                $scope.count = 0;
                $scope.orderDetailsFinal = data;
                $scope.finaltable = true;
                $scope.dispatchtable = false;
                $scope.finalAmountLAST = 0;
                $scope.finalTaxAmountLAST = 0;
                $scope.finalGrossAmountLAST = 0;
                $scope.finalTotalTaxAmountLAST = 0;
                $scope.finalLast = true;

                if ($scope.orderDetailsFinal[0].FinalOrderDispatchedMasterId == 0) {
                    for (var i = 0; i < $scope.orderDetailsFinal.length; i++) {
                        $scope.finalAmountLAST = $scope.finalAmountLAST + $scope.orderDetailsFinal[i].TotalAmt;
                        $scope.finalTaxAmountLAST = $scope.finalTaxAmountLAST + $scope.orderDetailsFinal[i].TaxAmmount;
                        $scope.finalGrossAmountLAST = $scope.finalGrossAmountLAST + $scope.orderDetailsFinal[i].TotalAmountAfterTaxDisc;
                        $scope.finalTotalTaxAmountLAST = $scope.finalTotalTaxAmountLAST + $scope.orderDetailsFinal[i].TotalAmountAfterTaxDisc;
                    }


                    $scope.TotalAmount = $scope.finalAmountLAST;
                    $scope.TaxAmount = $scope.finalTaxAmountLAST;
                    $scope.GrossAmount = $scope.finalGrossAmountLAST;
                    $scope.DiscountAmount = $scope.finalTotalTaxAmountLAST - $scope.finalAmountLAST;


                    $scope.myDetail = {};
                    $scope.myMaster = {};
                    $scope.myDetail = $scope.orderDetailsFinal;
                    $scope.myMaster = [];
                    var newdata = angular.copy($scope.OrderData1);
                    $scope.myMaster = newdata;

                    $scope.myMaster.TotalAmount = $scope.TotalAmount;
                    $scope.myMaster.TaxAmount = $scope.TaxAmount;
                    $scope.myMaster.GrossAmount = $scope.GrossAmount;
                    $scope.myMaster.WalletAmount = $scope.WalletAmount;
                    $scope.myMaster.DiscountAmount = $scope.DiscountAmount;
                    $scope.FinalinvoiceButtonLast = true;
                    $scope.showpaybleButton = true;
                    $scope.finalLastReturn = true;

                }

            }
            else {
                $scope.finalLastReturn = true;
                $scope.FinalinvoiceButtonWithoutMasterLast = true;
            }
        });

        $scope.showInvoiceWithoutMasterFinal = function () {

            OrderMasterService.saveDispatch($scope.myDetail);
            OrderMasterService.save1($scope.myMaster);
            console.log("Order Invoice  called ...");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "OrderInvoiceModel.html",
                    controller: "ModalInstanceCtrlOrderInvoiceDispatch",
                    resolve:
                    {
                        order: function () {
                            return $scope.myDetail
                        }
                    }
                });
            modalInstance.result.then(function () {
            },
                function () {
                    console.log("Cancel Condintion");

                })
        };

        $scope.showInvoice = function (data) {

            OrderMasterService.save1(data);
            console.log("Order Invoice  called ...");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "OrderInvoiceModel.html",
                    controller: "ModalInstanceCtrlOrderInvoice1",
                    resolve:
                    {
                        order: function () {
                            return data
                        }
                    }
                });
            modalInstance.result.then(function () {
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };
        $scope.showInvoiceDispatch = function (Detail, master) {

            OrderMasterService.saveDispatch(Detail);
            OrderMasterService.save1(master);
            console.log("Order Invoice  called ...");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "OrderInvoiceModel.html",
                    controller: "ModalInstanceCtrlOrderInvoiceDispatch",
                    resolve:
                    {
                        order: function () {
                            return Detail
                        }
                    }
                });
            modalInstance.result.then(function () {
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };

        $http.get(serviceBase + 'api/DeliveryBoy').then(function (results) {
            $scope.NewUser = results.data;
            var wid = parseInt(d.WarehouseId);
            $scope.DeliveryBoyfilterData = $filter('filter')($scope.NewUser, function (value) {
                return value.WarehouseId === wid;
            });

            $scope.User = [];
            $scope.User = $scope.DeliveryBoyfilterData;
            console.log("Got Dboy people collection");
            console.log($scope.User);
        });

        // end
        for (var i = 0; i < $scope.orderDetails.length; i++) {
            $scope.Itemcount = $scope.Itemcount + $scope.orderDetails[i].qty;
        }

        $scope.totalfilterprice = 0;

        _.map($scope.OrderData.orderDetails, function (obj) {
            $scope.totalfilterprice = $scope.totalfilterprice + obj.TotalAmt;
            console.log("$scope.OrderData");
            console.log($scope.totalfilterprice);

        });

        var s_col = false;
        var del = '';

        var ss_col = '';

        $scope.set_color = function (orderDetail) {

            //if (ss_col == orderDetail.itemNumber) before multi mrp
            if (ss_col == orderDetail.ItemMultiMRPId) {
                return { background: "#ff9999" }
            }
            //set color for red for less inventroy for dispatched
            else if (orderDetail.qty > orderDetail.CurrentStock) {
                s_col = true;
                return { background: "#ff9999" }
            }
            else {
                s_col = false;
            }

        }

        $scope.giveDiscount = function (DISCOUNT) {

        }

        $scope.selcteddboy = function (db) {
            $scope.Dboy = JSON.parse(db);
        }

        $scope.selcteddQTR = function (data) {

            $scope.QtyChangeReason = data;
        }

        $scope.selectedItemChanged = function (id) {

            $('#' + id).removeClass('hd');
        }

        $scope.CheckForReason = function (id) {

            if ($('#' + id).QtyChangeReason == undefined) {
                $scope.skills = [];
                $scope.skills.push({ 'id': id })
                alert('Please fill the Reason');
            }
        }
        $scope.save = function (orderDetail) {

            $("#btnSave").prop("disabled", true);
            var data = $scope.orderDetails;

            var url = serviceBase + 'api/OrderDispatchedMaster/GetFreeItemStock?orderId=' + data[0].OrderId;
            $http.get(url).success(function (results) {
                // if (results != "0") { 
                console.log($scope.Dboy);
                var flag = true;
                angular.forEach($scope.skills, function (value, key) {
                    for (var i = 0; i < $scope.orderDetails.length; i++) {
                        data = $scope.orderDetails[i];
                        if (data.ItemId == value.id) {
                            if (data.QtyChangeReason == undefined) {
                                alert("Please Fill Reason");
                                flag = false;
                                break;
                            }
                        }
                    }
                });
                for (var i = 0; i < $scope.orderDetails.length; i++) {
                    data = $scope.orderDetails[i];

                    if ((data.qty || data.qty == null || data.qty == undefined) > data.CurrentStock && data.Deleted == false) {

                        alert("your stock not sufficient please purchase or remove item then dispatched");
                        $("#btnSave").removeAttr("disabled");
                        flag = false;
                        break;
                    }
                }

                //double check for inventory
                for (var i1 = 0; i1 < $scope.orderDetails.length; i1++) {//instead of i used i1
                    $scope.CheckStockWithNumber = {};
                    var first = true;
                    for (var j = i1; j < $scope.orderDetails.length; j++) {
                        if (first) {
                            $scope.CheckStockWithNumber = $scope.orderDetails[j];
                            first = false;
                        }
                       
                        else if ($scope.CheckStockWithNumber.ItemMultiMRPId == $scope.orderDetails[j].ItemMultiMRPId) {
                            var Stockcount = 0;
                            Stockcount = $scope.CheckStockWithNumber.qty + $scope.orderDetails[j].qty;

                            if (Stockcount > $scope.orderDetails[j].CurrentStock && Stockcount > 0) {
                                alert("your stock not sufficient please purchase or remove item: ( " + $scope.orderDetails[j].itemname + " )This much Qty required : " + Stockcount);
                                $("#btnSave").removeAttr("disabled");
                                flag = false;
                                return false;
                            }

                        }
                    }
                }

                if ($scope.Dboy == undefined) {
                    alert("Please Select Delivery Boy");
                    $("#btnSave").removeAttr("disabled");
                    flag = false;

                }
                if (flag == true) {
                    try {
                        var obj = ($scope.Dboy);
                    } catch (err) {
                        alert("Please Select Delivery Boy");
                        console.log(err);

                    }
                    $scope.OrderData.DboyName = 'obj.DisplayName';
                    $scope.OrderData.DboyMobileNo = 'obj.Mobile';
                    $scope.OrderData();
                    console.log($scope.OrderData);
                    console.log("save orderdetailfunction");
                    console.log("Selected Pramtion");
                    console.log($scope.selectedpramotion);
                    for (var i6 = 0; i6 < $scope.orderDetails.length; i6++) { //instead of i used i6
                        console.log($scope.orderDetails[i6]);
                        console.log($scope.orderDetails[i6].DiscountPercentage);
                        console.log($scope.orderDetails[i6].DiscountPercentage);
                    }
                    $("#btnSave").prop("disabled", true);
                    //var url = serviceBase + 'api/OrderDispatchedMaster';
                    var url = serviceBase + 'api/OrderDispatchedDetails/V1';
                    $http.post(url, $scope.OrderData)
                        .success(function (data) {
                            $scope.dispatchedMasterID = data.OrderDispatchedMasterId;
                            $scope.orderDetails = data.orderDetails;
                            // $scope.dispatchedDetail();
                            $modalInstance.close(data);
                            console.log("Error Gor Here");
                            console.log(data);
                            if (data.id == 0) {
                                $scope.gotErrors = true;
                                if (data[0].exception == "Already") {
                                    console.log("Got This User Already Exist");
                                    $scope.AlreadyExist = true;
                                }
                            }
                            else {
                            }
                        })
                        .error(function (data) {
                            console.log("Error Got Heere is ");
                            console.log(data);
                        })
                }
            });
        };


        $scope.open = function (item) {
            console.log("in open");
            console.log(item);


        };

        $scope.invoice = function (invoice) {
            console.log("in invoice Section");
            console.log(invoice);

        };
        // cancle dispatch
        $scope.CancleDispatch = function () {
            var status = "cancle";

            var url = serviceBase + 'api/OrderDispatchedDetails?cancle=' + status;
            $http.put(url, $scope.orderDetailsDisp)
                .success(function (data) {

                    alert('Cancle successfully');
                    // location.reload();
                    window.location = "#/orderMaster";
                    console.log("Error Gor Here");
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                })
        }
        //end

        // add payment FOR DISPATCH FINAL
        $scope.Payments = {
            PaymentAmount: null,
            CheckNo: null,
            CheckAmount: null,
            ElectronicPaymentNo: null,
            ElectronicAmount: null,
            CashAmount: null
        }
        // final bill sattled
        $scope.SaveFinalSattled = function (Payments) {

            $scope.Payments();
            $scope.Payments.PaymentAmount = $scope.OrderData1.GrossAmount
            $scope.finalDataMaster = $scope.OrderData1;
            $scope.finalDataMaster.PaymentAmount = $scope.Payments.PaymentAmount;
            $scope.finalDataMaster.CheckNo = $scope.Payments.CheckNo;
            $scope.finalDataMaster.CheckAmount = $scope.Payments.CheckAmount;
            $scope.finalDataMaster.ElectronicPaymentNo = $scope.Payments.ElectronicPaymentNo;
            $scope.finalDataMaster.ElectronicAmount = $scope.Payments.ElectronicAmount;
            $scope.finalDataMaster.CashAmount = $scope.Payments.CashAmount;


            var url = serviceBase + 'api/OrderDispatchedMasterFinal';
            $http.post(url, $scope.finalDataMaster)
                .success(function (data) {

                    $scope.FdispatchedMasterID = data.FinalOrderDispatchedMasterId;
                    alert('payment insert successfully');
                    // location.reload();
                    window.location = "#/orderMaster";
                    $scope.dispatchedDetailFinal();
                    console.log("Error Gor Here");
                    console.log(data);
                    if (data.FinalOrderDispatchedMasterId == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }

                    }
                    else {
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                })
        }
        // dispatch detail final add
        $scope.dispatchedDetailFinal = function () {
            for (var i = 0; i < $scope.orderDetailsDisp.length; i++) {
                $scope.orderDetailsDisp[i].FinalOrderDispatchedMasterId = $scope.FdispatchedMasterID;
               
            }

            $scope.orderDetailsDisp();

            var url = serviceBase + 'api/OrderDispatchedDetailsFinal';
            $http.post(url, $scope.orderDetailsDisp)
                .success(function (data) {

                    alert('insert successfully');
                    // location.reload();
                    window.location = "#/orderMaster";
                    console.log("Error Gor Here");
                    console.log(data);
                    if (data.id == 0) {

                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                    }
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);

                })
        }
        // FOR AFTER RETURN LAST PAYMENT

        $scope.PaymentsLAST = {
            PaymentAmount: null,
            CheckNo: null,
            CheckAmount: null,
            ElectronicPaymentNo: null,
            ElectronicAmount: null,
            CashAmount: null
        }
        $scope.SaveFinalSattledLAST = function (PaymentsLAST) {
            $scope.PaymentsLAST();


            var payamount = parseInt($scope.myMaster.GrossAmount);
            $scope.PaymentsLAST.PaymentAmount = payamount;



            $scope.finalDataMasterLAST = $scope.OrderData1;

            if ($scope.finalAmountLAST > 0) {
                $scope.finalDataMasterLAST.TotalAmount = $scope.finalAmountLAST;
            }


            $scope.finalDataMasterLAST.PaymentAmount = $scope.PaymentsLAST.PaymentAmount;
            $scope.finalDataMasterLAST.CheckNo = $scope.PaymentsLAST.CheckNo;
            $scope.finalDataMasterLAST.CheckAmount = $scope.PaymentsLAST.CheckAmount;
            $scope.finalDataMasterLAST.ElectronicPaymentNo = $scope.PaymentsLAST.ElectronicPaymentNo;
            $scope.finalDataMasterLAST.ElectronicAmount = $scope.PaymentsLAST.ElectronicAmount;
            $scope.finalDataMasterLAST.CashAmount = $scope.PaymentsLAST.CashAmount;
            $scope.finalDataMasterLAST.TotalAmount = $scope.TotalAmount;
            $scope.finalDataMasterLAST.TaxAmount = $scope.TaxAmount;
            $scope.finalDataMasterLAST.GrossAmount = $scope.GrossAmount;
            $scope.finalDataMasterLAST.DiscountAmount = $scope.DiscountAmount;

            var url = serviceBase + 'api/OrderDispatchedMasterFinal';
            $http.post(url, $scope.finalDataMasterLAST)
                .success(function (data) {

                    $scope.FdispatchedMasterIDLAST = data.FinalOrderDispatchedMasterId;
                    $scope.FdispatchedMasterORDERIDLAST = data.OrderId;
                    alert('payment insert successfully');
                    // location.reload();
                    //   window.location = "#/orderMaster";
                    $scope.dispatchedDetailFinalLAST();
                    console.log("Error Gor Here");
                    console.log(data);
                    if (data.FinalOrderDispatchedMasterId == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }

                    }
                    else {
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                })
        }

        $scope.dispatchedDetailFinalLAST = function () {


            var url = serviceBase + 'api/OrderDispatchedDetailsFinal?oID=' + $scope.FdispatchedMasterORDERIDLAST + '&fID=' + $scope.FdispatchedMasterIDLAST;
            $http.put(url)
                .success(function (data) {

                    alert('insert successfully');
                    // location.reload();
                    window.location = "#/orderMaster";
                    console.log("Error Gor Here");
                    console.log(data);
                    if (data.id == 0) {

                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                    }
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);

                })
        }
        $scope.freeitem1 = function (item) {

            var modalInstance;
            modalInstance = $modal.open({
                templateUrl: "addFreeItem.html",
                controller: "ModalInstanceCtrlFreeItems", resolve: { order: function () { return item } }
            });
            modalInstance.result.then(function () {
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };

        $scope.moq = 0;

        $scope.OrderQuantityDic = function (moq) {

            if (moq.Noqty >= moq.MinOrderQty) {
                moq.Noqty = moq.Noqty - moq.MinOrderQty;
                alert("hhh");
            } else {

            }
        }

        $scope.OrderQuantityInc = function (moq) {
            moq.Noqty = moq.Noqty + moq.MinOrderQty;
            alert("hhh");
        }



        $scope.edit = function (item) {
             
            console.log("Edit Dialog called survey");
            var order = { OrderDetail: item, OrderData: $scope.OrderData }
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "ModalInstanceCtrlQuantityedit.html",
                    controller: "ModalInstanceCtrlQuantityeditAuto", resolve: { inventory: function () { return order } }
                });
            modalInstance.result.then(function (selectedItem) {
                
                $scope.UpdatedOrderData = selectedItem;
                
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };

        $window.setInterval(function () {
            $scope.Chekstock();
        }, 2000);

        var first = true;//double dispatched check
        $scope.Chekstock = function () {

            for (var i = 0; i < $scope.orderDetails.length; i++) {
                $scope.CheckStockWithNumber = {};
                var first = true;
                for (var j = i; j < $scope.orderDetails.length; j++) {
                    if (first) {
                        $scope.CheckStockWithNumber = $scope.orderDetails[j];
                        first = false;
                    }
                  
                    else if ($scope.CheckStockWithNumber.ItemMultiMRPId == $scope.orderDetails[j].ItemMultiMRPId) {
                        var Stockcount = 0;
                        Stockcount = $scope.CheckStockWithNumber.qty + $scope.orderDetails[j].qty;
                      
                        if (Stockcount > $scope.orderDetails[j].CurrentStock && Stockcount > 0) {
                           
                            $scope.$apply(function () {
                              
                                ss_col = $scope.orderDetails[j].ItemMultiMRPId;
                                $scope.set_color($scope.orderDetails[j]);
                            });
                        }
                        else { ss_col = ''; }
                    }
                }
            }
        }
    }
})();