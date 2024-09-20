'use strict'
app.controller('orderMasterController', ['$scope', 'OrderMasterService', 'OrderDetailsService', 'WarehouseService', 'DeliveryService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal',
    function ($scope, OrderMasterService, OrderDetailsService, WarehouseService, DeliveryService, $http, ngAuthSettings, $filter, ngTableParams, $modal) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        var warehouseids = UserRole.Warehouseids;//JSON.parse(localStorage.getItem('warehouseids'));


        $scope.UserRoleBackend = JSON.parse(localStorage.getItem('RolePerson'));

        $scope.currentPageStores = {};
        $scope.statuses = [];
        $scope.orders = [];
        $scope.filterdata = [];
        $scope.customers = [];
        $scope.selectedd = {};
        $scope.cities = [];
        $scope.city = [];
        $scope.statusname = {};
        $scope.PaymentType = {};
        $scope.OrderType = -1;
        $scope.OrderDirection = "DESC";
        $('input[name="daterange"]').daterangepicker({
            //maxDate: moment(),
            "dateLimit": {
                "month": 1
            },
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A'
        });





        $('.input-group-addon').click(function () {

            $('input[name="daterange"]').trigger("select");


        });

        $scope.getlevelcolor = function () {
            var url = serviceBase + "api/TargetModule/GetLevelcolour";
            $http.get(url).success(function (response) {
                $scope.level = response;
            }, function (error) {
            });

        };

        $scope.openpopupRebook = function (OrderId, ParentOrderId, Status, whid) {

            if (ParentOrderId == null) {
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "Rebookorder.html",
                        controller: "ModalInstanceCtrlBookOrder",
                        size: 200,
                        resolve: { OrderId: function () { return OrderId; }, WarehouseId: function () { return whid; } }

                    }), modalInstance.result.then(function (selectedItem) {

                    }, function () { });
            }
        };

        $scope.getlevelcolor();

        //$scope.level = new Array("Level 0", "Level 1", "Level 2", "Level 3", "Level 4", "Level 5");          

        $scope.srch = { Cityid: 0, orderId: 0, invoice_no: "", skcode: "", shopName: "", mobile: "", status: "", paymentMode: "", PaymentFrom: "", WarehouseId: 0, TimeLeft: null };

        var data = [];
        var url = serviceBase + "api/Warehouse/getSpecificCitiesforuser";             //Vinayak refer to show specific cities for login user  
        $http.get(url).success(function (results) {
            $scope.city = results;
            // $scope.warehouses = results;
            //    for (var j = 0; j < $scope.city.length; j++) {
            //        for (var i = 0; i < $scope.warehouses.length; i++) {
            //            if ($scope.warehouses[i].Cityid == $scope.city[j].Cityid) {
            //                data.push($scope.city[j]);
            //            }
            //        }
            //    }
            //    $scope.city = data;
            //    // console.log("cities:", $scope.city);
        });

        OrderMasterService.getcitys().then(function (results) {
            $scope.cities = results.data;

        });




        $scope.cities = $scope.data;
        $scope.vm = {
            rowsPerPage: 20,
            currentPage: 1,
            count: null,
            numberOfPages: null,
        };

        $scope.vmIRN = {
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

        //$scope.onNumPerPageChange = function () {
        //    $scope.itemsPerPage = $scope.selected;
        //}
        $scope.selected = $scope.numPerPageOpt[0];  //for Html page dropdown
        $scope.warehouse = [];
        $scope.KPPwarehouse = [];
        $scope.getWarehosues = function (CityId) {
            $scope.MultiWarehouseModel = [];
            var citystts = [];
            if ($scope.MultiCityModel != '') {
                _.each($scope.MultiCityModel, function (item) {
                    citystts.push(item.id);
                });
            }

            var url = serviceBase + "api/Warehouse/GetWarehouseCitiesOnOrder";             //Vinayak refer to show specific cities for login user             
            if (citystts != '') {
                $http.post(url, citystts).success(function (response) {

                    var assignWarehouses = [];
                    var KPPassignWarehouses = [];
                    if (response) {
                        angular.forEach(response, function (value, key) {
                            if (!value.IsKPP) {
                                if (warehouseids.indexOf(value.WarehouseId) !== -1) {
                                    assignWarehouses.push(value);
                                }
                            } else if (value.IsKPP) {
                                if (warehouseids.indexOf(value.WarehouseId) !== -1) {
                                    KPPassignWarehouses.push(value);
                                }
                            }
                        });
                    }
                    $scope.warehouse = assignWarehouses;
                    $scope.KPPwarehouse = KPPassignWarehouses;
                    if (assignWarehouses.length == 1 && warehouseids.indexOf(",") == -1) {
                        $scope.srch.WarehouseId = warehouseids;
                    }
                });
            }

            //var url = serviceBase + "api/Warehouse/GetWarehouseCityOnOrder?cityid=" + CityId;             //Vinayak refer to show specific cities for login user             

            //$http.get(url).success(function (response) {   

            //    var assignWarehouses = [];
            //    if (response) {
            //        angular.forEach(response, function (value, key) {
            //            if (warehouseids.indexOf(value.WarehouseId) !== -1) {
            //                assignWarehouses.push(value);
            //            }
            //        });
            //    }
            //    $scope.warehouse = assignWarehouses;
            //    if (assignWarehouses.length == 1 && warehouseids.indexOf(",") == -1) {
            //        $scope.srch.WarehouseId = warehouseids;
            //    }
            //});

        }

        $scope.getlevel = function (level) {
        }

        $scope.getData = function (pageno) {
            //In practice this should be in a factory.
            if ($scope.srch.WarehouseId > 0 && $scope.srch.orderId == 0 && $scope.srch.invoice_no == "" && $scope.srch.skcode == "" && $scope.srch.shopName == "" && $scope.srch.mobile == "" && $scope.statusname.name == "" && pageno != undefined) {
                $scope.customers = [];

                //$http.get(serviceBase + 'api/Warehouse').then(function (results) {

                //    $scope.warehou1 = results.data;
                //    console.log(results.data); 
                //})

                var url = serviceBase + "api/OrderMaster" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&WarehouseId=" + $scope.srch.WarehouseId;
                $http.get(url).success(function (response) {

                    $scope.customers = response.ordermaster;  //ajax request to fetch data into vm.data

                    $scope.dataList = angular.copy(response.ordermaster);





                    //debugger;
                    //for (var i in $scope.warehou1) {
                    //    if ($scope.srch.WarehouseId === $scope.warehou1[i].WarehouseId) {
                    //        $scope.dataList.StateName = $scope.warehou1[i].StateName;

                    //    }
                    //}



                    $scope.orders = $scope.customers;
                    $scope.total_count = response.total_count;
                    $scope.tempuser = response.ordermaster;
                });
                console.log($scope.dataList)
            }

            else {

                $scope.searchdata($scope.srch);
            }
        };
        $scope.Refresh = function () {
            $scope.srch = "";
            //$scope.AdvanceSearch();

            location.reload();
        }
        $scope.getTemplate = function (data) {
            if (data.OrderId === $scope.selectedd.OrderId) {
                myfunc();
                return 'edit';
            }
            else return 'display';
        };
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
                //$scope.AddTrack("View(History)", "OrderId:", data.OrderId);
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
                // $scope.AddTrack("View(History)", "OrderId:", data.OrderId);
            })
                .error(function (data) {
                });
        },

            $scope.itemHistroy = function (OfferCode) {
                $scope.dataordermasterHistrory1 = [];
                var url = serviceBase + "api/OrderMaster/OfferCode?OfferCode=" + OfferCode;
                $http.get(url).success(function (response) {

                    $scope.dataordermasterHistrory1 = response;
                    // console.log($scope.dataordermasterHistrory1);
                    $scope.AddTrack("View(History)", "OfferCode:", data.OfferCode);
                })
                    .error(function (data) {
                    });
            },

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
                    { id: 12, name: 'Post Order Canceled' },
                    { id: 13, name: 'Failed' },
                    { id: 14, name: 'Payment Pending' },
                    { id: 15, name: 'InTransit' },
                    { id: 16, name: 'Delivery Canceled Request' },
                    { id: 17, name: 'ReadyToPick' }
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


        $scope.dataselectV = [
            { id: "ePaylater", label: "ePaylater" },
            { id: "mPos", label: "mPos" },
            { id: "truepay", label: "truepay" },
            { id: "Cash", label: "Cash" },
            { id: "hdfc", label: "hdfc" },
            { id: "Cheque", label: "Cheque" },
            { id: "Gullak", label: "Gullak" },
            { id: "credit hdfc", label: "credit hdfc" },
            { id: "chqbook", label: "chqbook" },
            { id: "Razorpay QR", label: "Razorpay QR" },
            { id: "DirectUdhar", label: "DirectUdhar" },
            { id: "RTGS/NEFT", label: "RTGS/NEFT" },
            { id: "UPI", label: "UPI" },
            { id: "ScaleUp", label: "ScaleUp" },
            { id: "PayLater", label: "PayLater" },
            { id: "razorpay", label: "razorpay" }
        ];
        $scope.examplemodelV = [];
        $scope.exampledataV = [
            { id: "ePaylater", label: "ePaylater" },
            { id: "mPos", label: "mPos" },
            { id: "truepay", label: "truepay" },
            { id: "Cash", label: "Cash" },
            { id: "hdfc", label: "hdfc" },
            { id: "Cheque", label: "Cheque" },
            { id: "Gullak", label: "Gullak" },
            { id: "credit hdfc", label: "credit hdfc" },
            { id: "chqbook", label: "chqbook" },
            { id: "Razorpay QR", label: "Razorpay QR" },
            { id: "DirectUdhar", label: "DirectUdhar" },
            { id: "RTGS/NEFT", label: "RTGS/NEFT" },
            { id: "UPI", label: "UPI" },
            { id: "ScaleUp", label: "ScaleUp" },
            { id: "PayLater", label: "PayLater" },
            { id: "razorpay", label: "razorpay" }
        ];
        $scope.examplesettingsV = {};


        $scope.editstatus = function (data) {
            if ($scope.UserRoleBackend.rolenames.indexOf('HQ Master login') > -1 || $scope.UserRoleBackend.rolenames.indexOf('HQ CS EXECUTIVE') > -1) {
                $scope.selectedd = data;
                if (data.Status == 'Inactive' || data.Status == 'Payment Pending' || data.Status == 'Failed') {
                    if (data.OrderType == 4) {
                        $scope.statuses = [
                            { value: 1, text: "Pending" }, { value: 2, text: "InTransit" }
                        ];
                    }
                    else {
                        $scope.statuses = [
                            { value: 1, text: "Pending" }
                        ];
                    }

                }
                else if (data.Status == "Pending") {
                    $scope.statuses = [
                        { value: 1, text: "Order Canceled" }
                    ];
                }
                else if (data.Status == "InTransit") {
                    $scope.statuses = [
                        { value: 1, text: "Pending" }
                    ];
                }
                else {
                    $scope.statuses = data.Status;
                }
            }
            else {
                alert("You have not permission to change order status.");
            }
        };

        function myfunc() {
            $('#orderStatus').on('change', function () {
                $(".saveBtn").prop("disabled", false);
            })
        }

        $scope.reset = function () {
            $scope.selectedd = {};
        };

        $scope.updatestatus = function (data) {
            debugger
            if ((data.Status == 'Pending' || data.Status == 'Failed') && data.ReasonCancle == 'Agreed for COD') {
                var IsCODBlocked = false;
                OrderMasterService.IsCODBlocked(data.CustomerId).then(function (results) {
                    IsCODBlocked = results;

                    if (IsCODBlocked.data == true) {

                        if (confirm('इस कस्टमर की COD  लिमिट ब्लॉक है क्यों की डायरेक्ट उधार का पेमेंट overdue है फिर भी आप COD पेमेंट करना चाहते है ?')) {
                            OrderMasterService.editstatus(data).then(function (results) {
                                $scope.reset();
                                alert(results.data);
                                return results;
                            });
                        }
                    }
                    else {
                        OrderMasterService.editstatus(data).then(function (results) {
                            console.log("overdue", results)
                            $scope.reset();
                            alert(results.data);
                            return results;
                        });
                    }

                });


            }
            else {
                OrderMasterService.editstatus(data).then(function (results) {
                    $scope.reset();
                    alert(results.data);
                    return results;
                });
            }
        }

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
            //else if ($scope.srch.orderId == 0 || ($scope.srch.skcode == "" && $scope.srch.shopName == "" && $scope.srch.mobile == ""))
            //{
            //    alert("Please select one parameter");
            //    return;
            //}
            else if ($scope.srch == "" && $('#dat').val()) {
                $scope.srch = { orderId: 0, invoice_no: "", skcode: "", shopName: "", mobile: "", status: '' }
            }
            else if ($scope.srch != "" && !$('#dat').val()) {
                start = null;
                end = null;
                if (!$scope.srch.orderId) {
                    $scope.srch.orderId = 0;
                }
                if (!$scope.srch.invoice_no) {
                    $scope.srch.invoice_no = "";
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
                if (!$scope.srch.invoice_no) {
                    $scope.srch.invoice_no = 0;
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

            var url = serviceBase + "api/SearchOrder?start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&invoice_no=" + $scope.srch.invoice_no + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + stts + "&WarehouseId=" + $scope.srch.WarehouseId + "&CityId=" + $scope.srch.CityId;
            $http.get(url).success(function (response) {

                //
                $scope.customers = response;  //ajax request to fetch data into vm.data
                $scope.total_count = response.length;
                //$scope.orders = response;
            });
            location.reload();

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

            var warehousesttsA = [];
            if ($scope.MultiWarehouseModel != '') {
                _.each($scope.MultiWarehouseModel, function (item) {
                    warehousesttsA.push(item.id);
                });
            }

            var url = serviceBase + "api/OrderMaster/getselforder?start=" + start + "&end=" + end;

            $http.post(url, warehousesttsA).success(function (response) {
                $scope.filterdata = response;

                angular.forEach($scope.filterdata, function (value, key) {

                    if (value.OrderType == 0 || value.OrderType == 1)
                        value.OrderTypestr = "General";
                    else if (value.OrderType == 2)
                        value.OrderTypestr = "Bundle";
                    else if (value.OrderType == 3)
                        value.OrderTypestr = "Return";
                    else if (value.OrderType == 4)
                        value.OrderTypestr = "Distributor";
                    else
                        value.OrderTypestr = "C";
                });

                alasql('SELECT OrderTypestr,OrderId,SalesPerson,CustomerId,Skcode,CRMTags,ShopName,Status,invoice_no,BillingAddress,ShippingAddress,TotalAmount,GrossAmount,DiscountAmount,TaxAmount,TCSAmount,CityId,WarehouseId,WarehouseName,ClusterId,ClusterName,Tin_No,CreatedDate,Deliverydate,UpdatedDate INTO XLSX("Ordermaster.xlsx",{headers:true}) FROM ? ', [response]);
                //alasql('SELECT a.GrossAmount,b.GrossAmount from OrderMaster a inner join OrderDispatchedMaster b on a.OrderId = b.OrderId ');
            });

        };

        // string sqlCountQuery = "Select Count(*) from OrderMasters o left join OrderDispatchedMasters od on o.OrderId = od.OrderId where o.Deleted = 0 and o.status <> 'Inactive' and o.status <> 'Dummy Order Cancelled' " + whereclause;

        //$scope.selfData();

        //SelfOrderExport..On DateRange and WareHouseWise.03/04/19./end/
        $scope.dataforsearch1 = { Cityid: "", Warehouseid: "", datefrom: "", dateto: "" };
        $scope.statusname.name = "";

        $scope.exportData = function (data, WarehouseId, Cityid) {
            debugger;
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();

            if ($scope.MultiWarehouseModel == '' || $scope.MultiWarehouseModel.length == 0) {
                alert("Please select atleast 1 Warehouse");
                return;
            }

            if (!$('#dat').val()) {
                start = null;
                end = null;
                alert("Please select date range");
                return;
            }
            else if ($scope.srch == "" && $('#dat').val()) {
                $scope.srch = { Cityid: 0, orderId: 0, skcode: "", shopName: "", mobile: "", status: '', PaymentFrom: '', OrderType: 0, CustomerType: "" }
            }
            else if ($scope.srch != "" && !$('#dat').val()) {
                start = null;
                end = null;
                if (!$scope.Cityid) {
                    $scope.Cityid = 0;
                }
                if (!$scope.srch.orderId) {
                    $scope.srch.orderId = 0;
                }
                if (!$scope.srch.invoice_no) {
                    $scope.srch.invoice_no = "";
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
                if (!$scope.srch.PaymentFrom) {
                    $scope.srch.PaymentFrom = "";
                }
                if (!$scope.srch.OrderType) {
                    $scope.srch.OrderType = "";
                }
                if (!$scope.srch.CustomerType) {
                    $scope.srch.CustomerType = "";
                }
            }
            else {
                if (!$scope.Cityid) {
                    $scope.Cityid = 0;
                }

                if (!$scope.srch.orderId) {
                    $scope.srch.orderId = 0;
                }
                if (!$scope.srch.invoice_no) {
                    $scope.srch.invoice_no = "";
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
                if (!$scope.srch.PaymentFrom) {
                    $scope.srch.PaymentFrom = "";
                }
                if (!$scope.srch.OrderType) {
                    $scope.srch.OrderType = "";
                }
                if (!$scope.srch.CustomerType) {
                    $scope.srch.CustomerType = "";
                }
            }

            //// time cunsume code  
            var stts = "";
            if ($scope.statusname.name && $scope.statusname.name != "Show All") {
                stts = $scope.statusname.name;
            }


            var warehousestts = [];
            if ($scope.MultiWarehouseModel != '') {
                _.each($scope.MultiWarehouseModel, function (item) {
                    warehousestts.push(item.id);
                });
            }

            var citysttsE = [];
            if ($scope.MultiCityModel != '') {
                _.each($scope.MultiCityModel, function (item) {
                    citysttsE.push(item.id);
                });
            }

            var paymentstts = [];
            if ($scope.examplemodelV != '') {
                _.each($scope.examplemodelV, function (item) {
                    paymentstts.push(item.id);
                });
            }
            //var url = serviceBase + "api/SearchOrder?start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + stts + "&WarehouseId=" + $scope.srch.WarehouseId;
            //var postData = {

            //    ItemPerPage: $scope.vm.rowsPerPage,
            //    PageNo: $scope.vm.currentPage,
            //    WarehouseIds: warehousestts,
            //    WarehouseId: $scope.srch.WarehouseId,
            //    end: end,
            //    start: start,
            //    OrderId: $scope.srch.orderId,
            //    Skcode: $scope.srch.skcode,
            //    ShopName: $scope.srch.shopName,
            //    Mobile: $scope.srch.mobile,
            //    status: stts,
            //    PaymentFrom: paymentstts
            //};

            //$http.post(serviceBase + "api/SearchOrder/ExportOrderMasterDb", postData).success(function (response) {

            //    // $scope.OrderByDate = response;   
            //    if (response!="")
            //    window.open(response, '_blank');             
            //});
            var postdata = {
                WarehouseIds: warehousestts,
                CityIds: citysttsE,
                PaymentFrom: paymentstts
            };

            debugger;
            //var url = serviceBase + "api/SearchOrder?type=export&start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + $scope.srch.status;
            var url = ERPWebUrl + "api/SearchOrder/ExportOrderMaster?type=export&start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + $scope.statusname.name + "&LevelName=" + $scope.levelname + "&OrderType=" + $scope.OrderType + "&CustomerType=" + $scope.CustomerType;

            $http.post(url, postdata).success(function (response) {

                if (!response)
                    alert("No data available between two date ");
                else {
                    download(response, "OrderMasterExport.zip");
                }
                return false;


                //$scope.OrderByDate = response;
                //angular.forEach($scope.OrderByDate, function (value, key) {
                //    if (value.OrderType == 0 || value.OrderType == 1)
                //        value.OrderTypestr = "General";
                //    else if (value.OrderType == 2)
                //        value.OrderTypestr = "Bundle";
                //    else if (value.OrderType == 3)
                //        value.OrderTypestr = "Return";
                //    else if (value.OrderType == 4)
                //        value.OrderTypestr = "Distributor";
                //    else if (value.OrderType == 5)
                //        value.OrderTypestr = "Zaruri";
                //    else if (value.OrderType == 6)
                //        value.OrderTypestr = "Damage";
                //});




                //console.log("export");
                //if ($scope.OrderByDate.length <= 0) {
                //    alert("No data available between two date ");
                //}
                //else {

                //    var i = 0;
                //    var styleRows = {};
                //    $scope.OrderByDate.forEach(function (item) {
                //        var rwcolor = "#000000";

                //        if (item.OrderColor === "Red") {
                //            rwcolor = "#F71708";
                //        }
                //        else if (item.OrderColor === "Blue") {
                //            rwcolor = "#5F08F7";
                //        }

                //        styleRows[i] = { style: { Font: { Color: rwcolor } } };

                //        i++;


                //    });
                //    var mystyle = {
                //        headers: true,
                //        column: { style: { Font: { Bold: "1" } } },
                //        rows: styleRows
                //    };
                //    alasql('SELECT OrderColor,OrderTypestr,OrderId,OfferCode,Skcode,Customerphonenum,ShopName,SalesPerson,CustomerName,invoice_no,CreditNoteNumber,CreditNoteDate,ItemId,itemname,ABC_Classification,CategoryName,SubcategoryName,BrandName,HSNCode,sellingSKU,WarehouseName,ClusterName,CreatedDate,OrderBy,TotalAmt,AvailableStockAmt, OrderedTotalAmt,UnitPrice,MinOrderQtyPrice,qty,DiscountAmount,DiscountPercentage,TaxAmmount,TaxPercentage,SGSTTaxAmmount,SGSTTaxPercentage,CGSTTaxPercentage,IGSTTaxAmount,IGSTTaxPercent,deliveryCharge,GSTN_No,Status,ReasonCancle,comments,ItemMultiMRPId,DeliveryIssuanceIdOrderDeliveryMaster,IsPrimeCustomer,StoreName INTO XLSX("Ordermaster.xlsx",?) FROM ? ', [mystyle, $scope.OrderByDate]);
                //}
            }).error(function (error) {
                alert(error.ErrorMessage);
            });
        };

        function download(file, text) {

            //creating an invisible element 
            var element = document.createElement('a');
            element.setAttribute('download', text);
            element.setAttribute("href", file);
            document.body.appendChild(element);

            //onClick property 
            element.click();
            document.body.removeChild(element);
        }

        $scope.exportSaleData = function (data, WarehouseId, Cityid) {

            var start = "";
            var end = "";
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            $scope.OrderByDate = [];
            start = f.val();
            end = g.val();
            if (!$('#dat').val()) {
                end = '';
                start = '';
                alert("Select Start and End Date");
                return;
            }
            else {
                start = f.val();
                end = g.val();
            }

            if (!$('#dat').val() && $scope.srch == "") {
                start = null;
                end = null;
                alert("Please select one parameter");
                return;
            }
            else if ($scope.srch == "" && $('#dat').val()) {
                $scope.srch = { Cityid: 0, orderId: 0, skcode: "", shopName: "", mobile: "", status: '' }
            }
            else if ($scope.srch != "" && !$('#dat').val()) {
                start = null;
                end = null;
                if (!$scope.srch.Cityid) {
                    $scope.srch.Cityid = 0;
                }
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
                if (!$scope.srch.Cityid) {
                    $scope.srch.Cityid = 0;
                }
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
                } if (!$scope.levelname) {
                    $scope.levelname = "";
                }



            }

            var citysttsE = [];
            if ($scope.MultiCityModel != '') {
                _.each($scope.MultiCityModel, function (item) {
                    citysttsE.push(item.id);
                });
            }

            var warehousesttsE = [];
            if ($scope.MultiWarehouseModel != '') {
                _.each($scope.MultiWarehouseModel, function (item) {
                    warehousesttsE.push(item.id);
                });
            }
            var postdata = {
                WarehouseIds: warehousesttsE,
                CityIds: citysttsE
            };
            //var url = serviceBase + "api/SearchOrder?type=export&start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + $scope.srch.status;
            var url = serviceBase + "api/SearchOrder/ExportSalesOrderMaster?type=export&start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + $scope.statusname.name + "&LevelName=" + $scope.levelname;

            $http.post(url, postdata).success(function (response) {

                $scope.OrderByDate = response;
                angular.forEach($scope.OrderByDate, function (value, key) {
                    if (value.OrderType == 0 || value.OrderType == 1)
                        value.OrderTypestr = "General";
                    else if (value.OrderType == 2)
                        value.OrderTypestr = "Bundle";
                    else if (value.OrderType == 3)
                        value.OrderTypestr = "Return";
                    else if (value.OrderType == 4)
                        value.OrderTypestr = "Distributor";
                    else
                        value.OrderTypestr = "C";
                });


                if ($scope.OrderByDate.length <= 0) {
                    alert("No data available between two date ");
                }
                else {

                    var i = 0;
                    var styleRows = {};
                    $scope.OrderByDate.forEach(function (item) {
                        var rwcolor = "#000000";
                        if (item.OrderColor === "Red") {
                            rwcolor = "#F71708";
                        }
                        else if (item.OrderColor === "Blue") {
                            rwcolor = "#5F08F7";
                        }
                        styleRows[i] = { style: { Font: { Color: rwcolor } } };
                        i++;
                    });
                    var mystyle = {
                        headers: true,
                        column: { style: { Font: { Bold: "1" } } },
                        rows: styleRows
                    };
                    alasql('SELECT OrderTypestr,OrderId,OfferCode,Skcode,CRMTags,Customerphonenum,ShopName,SalesPerson,CustomerName,invoice_no,CreditNoteNumber,CreditNoteDate,ItemId,itemname,ABC_Classification,CategoryName,SubcategoryName,BrandName,HSNCode,sellingSKU,WarehouseName,ClusterName,CreatedDate,OrderBy,TotalAmt, OrderedTotalAmt,UnitPrice,MinOrderQtyPrice,qty,DiscountAmount,DiscountPercentage,TaxAmmount,TaxPercentage,SGSTTaxAmmount,SGSTTaxPercentage,CGSTTaxPercentage,IGSTTaxAmount,IGSTTaxPercent,deliveryCharge,GSTN_No,Status,ReasonCancle,comments,ItemMultiMRPId,DeliveryIssuanceIdOrderDeliveryMaster,IsPrimeCustomer,StoreName,IsFirstOrder INTO XLSX("Ordermaster.xlsx",?) FROM ? ', [mystyle, $scope.OrderByDate]);
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
                        var OfferCode = $scope.OrderByDate[i].OfferCode;
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
                        var orderDetails = $scope.OrderByDate[i].orderDetailsExport;
                        for (var j = 0; j < orderDetails.length; j++) {
                            var tts = {
                                OrderId: '', Skcode: '', OfferCode: '', ShopName: '', Mobile: '', OrderBy: '', RetailerName: '', RetailerID: '', Warehouse: '', ClusterName: '', Deliverydate: '', CompanyId: '', BillingAddress: '', Date: '',
                                Excecutive: '', ShippingAddress: '', deliveryCharge: '', ItemID: '', ItemName: '', SKU: '', itemNumber: '', MRP: '', MOQ: '', UnitPrice: '', Quantity: '', MOQPrice: '', Discount: '',
                                DiscountPercentage: '', TaxPercentage: '', Tax: '', TotalAmt: '', CategoryName: '', BrandName: '', Status: '', ReasonCancle: '', comments: '', ItemMultiMRPId: ''
                            };
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
                            tts.ItemMultiMRPId = orderDetails[j].ItemMultiMRPId;
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
                            tts.OfferCode = OfferCode;

                            $scope.NewExportData.push(tts);

                        }
                    }
                    //alasql.fn.myfmt = function (n) {
                    //    return Number(n).toFixed(2);
                    //}
                    alasql('SELECT OrderId,OfferCode,Skcode,ShopName,RetailerName,Mobile,ItemID,ItemName,CategoryName,SubcategoryName,BrandName,SKU,Warehouse,ClusterName,Date,OrderBy,Excecutive,MRP,UnitPrice,MOQPrice,Quantity,Discount,DiscountPercentage,Tax,TaxPercentage,TotalAmt,deliveryCharge,Status,ReasonCancle,comments,ItemMultiMRPId INTO XLSX("OrderDetails.xlsx",{headers:true}) FROM ?', [$scope.OrderByDate]);
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
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                },
                    function () {
                        console.log("Cancel Condintion");
                    });
        };

        $scope.callmethod = function () {
            var init;
            return $scope.stores = $scope.orders,
                $scope.searchKeywords = "",
                $scope.filteredStores = [],
                $scope.row = "",

                $scope.select = function (page) {
                    var end, start; console.log("select"); console.log($scope.stores);
                    return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
                },

                $scope.onFilterChange = function () {
                    console.log("onFilterChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1, $scope.row = ""
                },

                $scope.onNumPerPageChange = function () {
                    console.log("onNumPerPageChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1
                },

                $scope.onOrderChange = function () {
                    console.log("onOrderChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1
                },

                $scope.search = function () {
                    console.log("search");
                    console.log($scope.stores);
                    console.log($scope.searchKeywords);

                    return $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange()
                },

                $scope.order = function (rowName) {
                    console.log("order"); console.log($scope.stores);
                    return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0
                },

                $scope.numPerPageOpt = [3, 5, 10, 20],
                $scope.numPerPage = $scope.numPerPageOpt[2],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                });
        };

        $scope.Return = function (data) {


            OrderMasterService.saveReturn(data);
            console.log("Order Detail Dialog called ...");
        };
        $scope.mydata = {};

        $scope.showDetail = function (data) {

            $http.get(serviceBase + 'api/OrderMaster?id=' + data.OrderId).then(function (results) {

                $scope.myData = results.data;
                OrderMasterService.save($scope.myData);
                setTimeout(function () {
                    console.log("Modal opened Orderdetails");
                    var modalInstance;
                    modalInstance = $modal.open(
                        {
                            templateUrl: "myOrderdetail.html",
                            controller: "orderdetailsController", resolve: { Ord: function () { return $scope.items; }, PaymentType: function () { return data.Trupay; }, OrderColor: function () { return data.OrderColor; } }
                        }), modalInstance.result.then(function (selectedItem) {

                            if (selectedItem.Status == "Ready to Dispatch") {
                                $("#st" + selectedItem.OrderId).html(selectedItem.Status);
                                $("#st" + selectedItem.OrderId).removeClass('canceled');
                                $('#re' + selectedItem.OrderId).hide();
                            }
                        },
                            function () {
                            });
                }, 500);

            });

        };

        $scope.showOrderOtp = function (otp, OrderId, status, SkCode, MobileNo, orderRow) {
            debugger
            var otpstatus = "";
            $http.get(serviceBase + 'api/OrderMaster/GetLastOtpStatus?OrderId=' + OrderId).then(function (results) {
                otpstatus = results.data;
               
                status = otpstatus != "" ? otpstatus : status;
                if (status == "You are not authorized")
                    var msg = status;
                else
                    var msg = "I hereby confirm that I called the " + SkCode + " (" + MobileNo + ") and confirms that the stock is delivered to the customer. ";
                if (status == "Delivery Canceled")
                    msg = "I hereby confirm that I called the " + SkCode + " (" + MobileNo + ") and confirms that the order Delivery Canceled.";
                else if (status == "Delivery Redispatch")
                    msg = "I hereby confirm that I called the " + SkCode + " (" + MobileNo + ") and confirms that the order Delivery ReDispatch.";
                var result = confirm(msg);
                if (status != "You are not authorized") {
                    if (result) {
                        $http.get(serviceBase + 'api/OrderMaster/ShowOtp?OrderId=' + OrderId + '&Otp=' + otp).then(function (results) {

                        });
                        if ((orderRow.IsVideoSeen || orderRow.VideoUrl == null || orderRow.VideoUrl == '')  || orderRow.UserType != 'HQ Operation(ReAttempt)') {
                            var modalInstance;
                            modalInstance = $modal.open(
                                {
                                    templateUrl: "OrderDeliveryOTP.html",
                                    controller: "ModalInstanceCtrlOrderOTP",
                                    size: 200,
                                    resolve: { OrderOtp: function () { return otp; }, OrderId: function () { return OrderId; } }
                                }), modalInstance.result.then(function (selectedItem) {

                                }, function () { });
                        } else {


                            var modalInstance;
                            modalInstance = $modal.open(
                                {
                                    templateUrl: "ShowVideo.html",
                                    controller: "ShowVideo",
                                    size: 200,
                                    resolve: {
                                        OrderData: function () { return orderRow; }, OrderOtp: function () { return otp; }, OrderId: function () { return OrderId; }, modal: function () { return $modal; }, 
                                    }
                                }), modalInstance.result.then(function (selectedItem) {

                                }, function () { });
                            //alert('do work of show video: ' + JSON.stringify(orderRow))
                        }

                    }
                }

            });
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
                            return data;
                        }
                    }
                }), modalInstance.result.then(function () {
                },
                    function () {
                        console.log("Cancel Condintion");

                    });
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
            return $scope.stores = $scope.orderDetails,
                $scope.searchKeywords = "",
                $scope.filteredStores = [],
                $scope.row = "",

                $scope.select = function (page) {
                    var end, start; console.log("select"); console.log($scope.stores);
                    return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
                },

                $scope.onFilterChange = function () {
                    console.log("onFilterChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1, $scope.row = "";
                },

                $scope.onNumPerPageChange = function () {
                    console.log("onNumPerPageChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1;
                },

                $scope.onOrderChange = function () {
                    console.log("onOrderChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1;
                },

                $scope.search = function () {
                    console.log("search");
                    console.log($scope.stores);
                    console.log($scope.searchKeywords);

                    return $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange()
                },

                $scope.order = function (rowName) {
                    console.log("order"); console.log($scope.stores);
                    return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0
                },

                $scope.numPerPageOpt = [3, 5, 10, 20],
                $scope.numPerPage = $scope.numPerPageOpt[2],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                })
                    ()
        };

        $scope.chkdb = true;
        $scope.DBoys = [];
        DeliveryService.getdboys().then(function (results) {
            $scope.DBoys = results.data;
        }, function (error) {
        });



        $scope.Mobile = '';
        $scope.orderAutomation = function () {

            if ($scope.Mobile != null && $scope.Mobile != undefined) {


                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "orderAutomation.html",
                        controller: "orderAutomationCtrl", resolve: { mobile: function () { return $scope.Mobile } }
                    });
            }
            else {
                alert();
            }
        };
        $scope.onNumPerPageChange = function () {
            $scope.vm.rowsPerPage = $scope.selected;
            $scope.AdvanceSearch();
        };

        $scope.changePage = function (pagenumber) {
            setTimeout(function () {
                $scope.vm.currentPage = pagenumber;
                $scope.AdvanceSearch();
            }, 100);
        };


        $scope.AdvanceSearch = function () {
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();

            if ($scope.MultiWarehouseModel == '' || $scope.MultiWarehouseModel.length == 0) {
                alert("Please select atleast 1 Warehouse");
                return;
            }

            if (!$('#dat').val() && $scope.srch == "") {
                start = null;
                end = null;
                alert("Please select one parameter");
                return;
            }
            else if ($scope.srch == "" && $('#dat').val()) {
                $scope.srch = { Cityid: 0, orderId: 0, skcode: "", shopName: "", mobile: "", status: '', PaymentFrom: '' }
            }
            else if ($scope.srch != "" && !$('#dat').val()) {
                start = null;
                end = null;
                if (!$scope.Cityid) {
                    $scope.Cityid = 0;
                }
                if (!$scope.srch.orderId) {
                    $scope.srch.orderId = 0;
                }
                if (!$scope.srch.invoice_no) {
                    $scope.srch.invoice_no = "";
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
                if (!$scope.srch.PaymentFrom) {
                    $scope.srch.PaymentFrom = "";
                }

            }
            else {
                if (!$scope.Cityid) {
                    $scope.Cityid = 0;
                }

                if (!$scope.srch.orderId) {
                    $scope.srch.orderId = 0;
                }
                if (!$scope.srch.invoice_no) {
                    $scope.srch.invoice_no = "";
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
                if (!$scope.srch.PaymentFrom) {
                    $scope.srch.PaymentFrom = "";
                }

            }
            $scope.orders = [];
            $scope.customers = [];
            //// time cunsume code  
            var stts = "";
            if ($scope.statusname.name && $scope.statusname.name != "Show All") {
                stts = $scope.statusname.name;
            }
            var citystts = [];
            if ($scope.MultiCityModel != '') {
                _.each($scope.MultiCityModel, function (item) {
                    citystts.push(item.id);
                });
            }

            var warehousestts = [];
            if ($scope.MultiWarehouseModel != '') {
                _.each($scope.MultiWarehouseModel, function (item) {
                    warehousestts.push(item.id);
                });
            }

            var paymentstts = [];
            if ($scope.examplemodelV != '') {
                _.each($scope.examplemodelV, function (item) {
                    paymentstts.push(item.id);
                });
            }
            //var url = serviceBase + "api/SearchOrder?start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + stts + "&WarehouseId=" + $scope.srch.WarehouseId;
            var postData = {

                ItemPerPage: $scope.vm.rowsPerPage,
                PageNo: $scope.vm.currentPage,
                Cityids: citystts,
                WarehouseIds: warehousestts,
                WarehouseId: $scope.srch.WarehouseId,
                Cityid: $scope.Cityid,
                end: end,
                start: start,
                OrderId: $scope.srch.orderId,
                Skcode: $scope.srch.skcode,
                ShopName: $scope.srch.shopName,
                Mobile: $scope.srch.mobile,
                status: stts,
                PaymentFrom: paymentstts,
                TimeLeft: $scope.srch.TimeLeft,
                LevelName: $scope.levelname,
                invoice_no: $scope.srch.invoice_nom,
                OrderType: $scope.OrderType,
                SortDirection: $scope.OrderDirection,
                CustomerType: $scope.CustomerType
            };

            $http.post(serviceBase + "api/OrderMaster/GetOrderAdvanceSearchMongo", postData).success(function (data, status) {
                console.log("OrderData:", data.ordermaster);
                //for (var item in data.ordermaster) {
                //    if (item.OrderType == 8) {
                //        item.OrderColor = 'lightblue';
                //        for (var detail in item.orderDetails) {
                //            item.OrderColor = 'White';
                //        }
                //    }
                //}

                debugger;
                $scope.isLoctaionIssue = false;
                data.ordermaster.forEach(function (om) {
                    if (om.Distance != null) {
                        var split_string = om.Distance.split(/(\d+)/)
                        console.log("Text:" + split_string[0] + " & Number:" + split_string[1])

                        if (split_string[1] > 250) {
                            om.isLoctaionIssue = true;
                        } else {
                            om.isLoctaionIssue = false;
                        }
                    }
                });

                $scope.customers = data.ordermaster;  //ajax request to fetch data into vm.data
                $scope.dataList = angular.copy(data.ordermaster);





                //$http.get(serviceBase + 'api/OrderMaster/GetStateCode' + "?WarehouseId=" + $scope.srch.WarehouseId).then(function (results) {
                //    $scope.statesData = results;
                //    console.log("-----------------------------------------", results);
                //})




                //$http.get(serviceBase + 'api/Warehouse').then(function (results) {

                //    $scope.warehou1 = results.data;
                //    console.log(results.data);

                //    debugger;
                //    for (var i in $scope.warehou1) {
                //        for (var j in $scope.dataList) {
                //            if ($scope.dataList[j].WarehouseId === $scope.warehou1[i].WarehouseId) {
                //                $scope.dataList[j].StateName = $scope.warehou1[i].StateName;

                //            }
                //        }

                //    }

                //    // $scope.offerbill();


                //})
                // console.log('order master: ', data.ordermaster);
                $scope.orders = $scope.customers;
                $scope.total_count = data.total_count;
                $scope.vm.count = data.total_count;
                $scope.tempuser = data.ordermaster;
                $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);


            });
            // $scope.srch.WarehouseId = 0;



        }

        $scope.PaymentDetail = function (orderId) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "paymentdetail.html",
                    controller: "ModalInstanceCtrlOrderPayment", resolve: { orderId: function () { return orderId; } }
                }), modalInstance.result.then(function (selectedItem) {
                    //$scope.currentPageStores.push(selectedItem);
                },
                    function () {
                    });
        }
        //$scope.ok = function () { modalInstance.close(); },
        //    $scope.cancel = function () { modalInstance.dismiss('Canceled'); },


        $scope.GetOrderTime = function (countdown) {

            var millis = 0;
            millis = countdown * 1000;

            var seconds = Math.floor((millis / 1000) % 60);
            var minutes = Math.floor(((millis / (1000 * 60)) % 60));
            var hours = Math.floor(((millis / (1000 * 60 * 60)) % 24));
            var days = Math.floor(((millis / (1000 * 60 * 60)) / 24));
            if (days != 0)
                hours += (days * 24);

            var template = '';
            if (hours >= 0 && hours <= 48)
                template = '<span class="label HIT ng-binding" style="background-color:#033e15">' + hours + ':' + minutes + ':' + seconds + '</span>';
            else if (hours >= 48 && hours <= 72)
                template = '<span class="label Miss ng-binding" style="background-color:#a58902">' + hours + ':' + minutes + ':' + seconds + '</span>';
            else if (hours >= 72 && hours <= 100)
                template = '<span class="label Fail ng-binding" style="background-color:#eb5a00">' + hours + ':' + minutes + ':' + seconds + '</span>';
            else if (hours > 100)
                template = '<span class="label BOLD ng-binding" style="background-color:#f90808">' + hours + ':' + minutes + ':' + seconds + '</span>';
            return template;
        }

        // Multieare select city and warehouse code
        $scope.MultiCityModel = [];
        $scope.MultiCity = $scope.city;
        $scope.MultiCityModelsettings = {
            displayProp: 'CityName', idProp: 'Cityid',
            scrollableHeight: '450px',
            scrollableWidth: '550px',
            enableSearch: true,
            scrollable: true
        };

        $scope.MultiWarehouseModel = [];
        $scope.MultiWarehouse = $scope.warehouse;
        $scope.MultiWarehouseModelsettings = {
            displayProp: 'WarehouseName', idProp: 'WarehouseId',
            scrollableHeight: '450px',
            scrollableWidth: '550px',
            enableSearch: true,
            scrollable: true
        };

        $scope.maxSizeIRN = 5;     // Limit number for pagination display number.  
        $scope.totalCountIRN = 0;  // Total number of items in all pages. initialize as a zero  
        $scope.pageIndexIRN = 1;   // Current page number. First page is 1.--&gt;  
        $scope.pageSizeSelectedIRN = 20; // Maximum number of items per page.  

        ////// Create Functions for IRN 
        $scope.IRNDataSearch = function () {
            debugger
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();

            if ($scope.MultiWarehouseModel == '' || $scope.MultiWarehouseModel.length == 0) {
                alert("Please select atleast 1 Warehouse");
                return;
            }
            else {
                if (!$scope.Cityid) {
                    $scope.Cityid = 0;
                }
                if (!$scope.srch.orderId) {
                    $scope.srch.orderId = 0;
                }

            }
            $scope.orders = [];
            $scope.customers = [];
            //// time cunsume code  
            var stts = "";
            if ($scope.statusname.name && $scope.statusname.name != "Show All") {
                stts = $scope.statusname.name;
            }
            var citystts = [];
            if ($scope.MultiCityModel != '') {
                _.each($scope.MultiCityModel, function (item) {
                    citystts.push(item.id);
                });
            }

            var warehousestts = [];
            if ($scope.MultiWarehouseModel != '') {
                _.each($scope.MultiWarehouseModel, function (item) {
                    warehousestts.push(item.id);
                });
            }


            //var url = serviceBase + "api/SearchOrder?start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + stts + "&WarehouseId=" + $scope.srch.WarehouseId;
            var postData = {

                Cityids: citystts,
                WarehouseIds: warehousestts,
                OrderId: $scope.srch.orderId,
                ItemPerPage: 20,
                PageNo: $scope.vmIRN.currentPage,
            };

            $http.post(serviceBase + "api/IRNReGenerate/GetSearchOrderMaster", postData).success(function (data, status) {

                $scope.IRNcustomers = data.ordermaster;
                $scope.total_countIRN = data.total_count;
                $scope.vmIRN.count = data.total_count;
                $scope.tempuserIRN = data.ordermaster;
                $scope.vmIRN.numberOfPages = Math.ceil($scope.vmIRN.count / 20);
                console.log(data); //ajax request to fetch data into vm.data
            });
            // $scope.srch.WarehouseId = 0;



        }

        $scope.changePageIRN = function (pagenumber) {
            setTimeout(function () {
                $scope.vmIRN.currentPage = pagenumber;
                $scope.IRNDataSearch();
            }, 100);

        };

        $scope.ConvertB2C = function (orderid) {

            if (confirm("Are you sure?")) {
                var postData = {

                    OrderId: orderid,
                };

                $http.post(serviceBase + "api/IRNReGenerate/ConvertB2C", postData).success(function (data, status) {
                    console.log(data);
                    if (data) {
                        alert("Converted B2C");

                        $scope.IRNDataSearch();
                    }
                    else {
                        alert("Error: Order not converted to B2C");
                        console.log(data);
                    }//ajax request to fetch data into vm.data
                }).error(function (data) {

                    alert("Error: Order not converted to B2C");
                });
            }
        };
        ///////////////////////////////////
        $scope.show = function (IRNErrorData) {

            console.log("Modal opened IRNErrorData");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myShowModalPut.html",
                    controller: "showctrl", resolve: { IRNErrorData: function () { return IRNErrorData } }
                });
            modalInstance.result.then(function (selectedItem) {

            },
                function () {
                    console.log("Cancel Condintion");

                });
        };
        ///// Calling a api for Retry for IRN Generate
        $scope.RegenerateIRN = function () {
            debugger
            var Orderid = $scope.IRNaction.OrderId;

            var postData = {

                OrderId: Orderid,
            };
            if (confirm("Are you sure?")) {
                $http.post(serviceBase + "api/IRNReGenerate/RegenerateIRN", postData).success(function (data, status) {
                    console.log(data);
                    if (data) {
                        alert("IRN Re Generated");
                        $('#IRNActionModel').modal('hide');
                        $scope.IRNDataSearch();
                    }
                    else {
                        alert("IRN Not Re Generated");
                        console.log(data);
                    }//ajax request to fetch data into vm.data
                }).error(function (data) {

                    alert("error: ", data);
                });
            }
        }
        $scope.test = function (trade) {
            debugger
            $scope.IRNaction = trade;
            // alert('hi showctrl');
            $('#IRNActionModel').modal('show');
        }

        $scope.GetIRNNumber = function (irn) {
            debugger
            if (irn == undefined || irn == "" || irn == "0") {
                alert(" Please enter IRN Number ");
                return false;
            }
            var Orderid = $scope.IRNaction.OrderId;

            var postData = {

                OrderId: Orderid,
                irn: irn
            };

            $http.post(serviceBase + "api/IRNReGenerate/GettingEInvoicebyIRN", postData).success(function (data, status) {
                console.log(data);
                if (data) {
                    alert("IRN Generated");
                    $('#IRNActionModel').modal('hide');
                }
                else {
                    alert("IRN Not Generated");
                }
                console.log(data); //ajax request to fetch data into vm.data
            }).error(function (data) {

                alert("error: ", data);
            });
        }

        $scope.ExporttoExcelIRN = function (orderid) {
            if (orderid == undefined || orderid == "" || orderid == "0") {
                alert(" Please enter Order ID ");
                return false;
            }
            $http.get(serviceBase + 'api/IRNReGenerate/GenerateExcelIRN?orderid=' + orderid).then(function (results) {

                if (results.data != "")
                    window.open(results.data, '_blank');
                else
                    alert("File not created");

            });
        }

    }]);


app.controller("ModalInstanceCtrlOrderOTP", ["$scope", '$http', 'OrderOtp', 'OrderId', "$modalInstance",
    function ($scope, $http, OrderOtp, OrderId, $modalInstance) {

        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); },
            $scope.OrderOtp = OrderOtp;
        $scope.OrderNo = OrderId;
    }]);

app.controller("ShowVideo", ["$scope", '$http', 'OrderData', 'OrderOtp', 'OrderId', "$modalInstance", 'modal',  
    function ($scope, $http, OrderData, OrderOtp, OrderId, $modalInstance, modal) {

        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); },
            $scope.OrderData = OrderData;
        $scope.OrderOtp = OrderOtp;
        $scope.OrderNo = OrderId;
        
        $http.get(serviceBase + "api/OrderMaster/ShowReason?OrderId=" + $scope.OrderNo )
            .then(function (results) {
                console.log('comment is: ', results.data);
				$scope.Comment = results.data;
            });

        setTimeout(isEndedCheck, 500)

        function isEndedCheck() {
            document.querySelector("video").onended = function () {
                //if (this.played.end(0) - this.played.start(0) === this.duration) {
                //    console.log("Played all");
                //} else {
                //    console.log("Some parts were skipped");
                //}

                $http.get(serviceBase + "api/DeliveryApp/UpdateNotifyVideoSeen?Id=" + OrderData.DeliveryOtpId + "&OrderStatus=Delivery%20Redispatch&RequestId=2")
                    .then(function (results) {
                        $scope.cancel();
                        var modalInstance;
                        modalInstance = modal.open(
                            {
                                templateUrl: "OrderDeliveryOTP.html",
                                controller: "ModalInstanceCtrlOrderOTP",
                                size: 200,
                                resolve: { OrderOtp: function () { return $scope.OrderOtp; }, OrderId: function () { return $scope.OrderNo; } }
                            }), modalInstance.result.then(function (selectedItem) {

                            }, function () { });
                    });
            }
        }


        console.log('$scope.OrderData :', $scope.OrderData);
    }]);



app.controller("ModalInstanceCtrlOrderDetail", ["$scope", '$http', 'OrderMasterService', "$modalInstance", 'ngAuthSettings', 'order',
    function ($scope, $http, OrderMasterService, $modalInstance, ngAuthSettings, order) {
        console.log("order detail modal opened");

        $scope.orderDetails = [];

        if (order) {

            $scope.OrderData = order;

            $scope.orderDetails = $scope.OrderData.orderDetails;

        }
        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

            $scope.Details = function (dataToPost, $index) {

                OrderMasterService.getDeatil.then(function (results) {
                    $scope.order.splice($index, 1);
                    $modalInstance.close(dataToPost);
                    ReloadPage();

                }, function (error) {
                    alert(error.data.message);
                });
            }
    }]);

app.controller("ModalInstanceCtrlOrderInvoice", ["$scope", '$http', 'OrderMasterService', "$modalInstance", 'ngAuthSettings', 'order',
    function ($scope, $http, OrderMasterService, $modalInstance, ngAuthSettings, order) {

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

    }]);

app.controller("ModalInstanceCtrldeleteOrder", ["$scope", '$http', 'OrderMasterService', "$modalInstance", 'ngAuthSettings', 'order',
    function ($scope, $http, OrderMasterService, $modalInstance, ngAuthSettings, myData) {
        console.log("delete modal opened");
        function ReloadPage() {
            location.reload();
        };

        $scope.orders = [];


        if (myData) {

            $scope.orders = myData.order1;

        }
        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); },


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
            }

    }]);

app.controller("orderAutomationCtrl", ["$scope", '$http', '$modal', 'DeliveryService', "$modalInstance", 'ordersService', "mobile", function ($scope, $http, $modal, DeliveryService, $modalInstance, ordersService, mobile) {

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
    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },
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
}]);

app.controller("ModalInstanceCtrlOrderPayment", ["$scope", '$http', "$modalInstance", 'ngAuthSettings', 'orderId', function ($scope, $http, $modalInstance, ngAuthSettings, orderId) {

    $scope.dataorderpayment = [];
    var url = serviceBase + "api/OrderMastersAPI/Getpaymentstatus?OrderId=" + orderId;
    $http.get(url).success(function (response) {

        $scope.dataorderpayment = response;
    })
        .error(function (data) {
        });

    $scope.ok = function () { $modalInstance.close(); }
    $scope.cancel = function () { $modalInstance.dismiss('canceled'); }




}]);

app.controller("ModalInstanceCtrlBookOrder", ["$scope", '$http', 'OrderId', 'WarehouseId', "$modalInstance",
    function ($scope, $http, OrderId, WarehouseId, $modalInstance) {

        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); },
            $scope.WHID = WarehouseId;
        $scope.OrderNoR = OrderId;
        $scope.NewDate = new Date();


        $scope.RebookOrder = function (NewDate) {
            debugger

            let isValidDate = Date.parse(NewDate);

            if (isNaN(isValidDate)) {
                alert('Please enter correct date');
                return false;
            }

            //var varDate = new Date(NewDate); //dd-mm-YYYY
            //varDate.setHours(0, 0, 0, 0);
            //var today = new Date();
            //today.setHours(0, 0, 0, 0);
            //if (varDate < today) {
            //    //Do something..
            //    alert("Order date not less than today date !");
            //    return false;
            //}
            if (confirm("Are you sure?") == true) {

                var postdata = {
                    OrderId: $scope.OrderNoR,
                    NewDate: NewDate,
                    WarehouseId: $scope.WHID,
                };


                if ($scope.OrderNoR != undefined || $scope.OrderNoR > 0) {
                    var url = serviceBase + "api/OrderMaster/RebookOrder";
                    $http.post(url, postdata).success(function (response) {
                        console.log(response);
                        if (response.status) {
                            alert(response.message);
                            location.reload();
                        }
                        else {
                            alert(response.message);
                            $scope.ok();
                            location.reload();
                        }
                    });
                }
            }
        }

    }]);


(function () {
    'use strict';

    angular
        .module('app')
        .controller('showctrl', showctrl);

    showctrl.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "IRNErrorData", 'FileUploader'];

    function showctrl($scope, $http, ngAuthSettings, $modalInstance, IRNErrorData, FileUploader) {

        $scope.IRNDataSingle = IRNErrorData;
        $scope.errordata = JSON.parse(IRNErrorData.Error);

    }



})();