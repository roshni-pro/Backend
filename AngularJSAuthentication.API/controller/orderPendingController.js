   

(function () {
    'use strict';

    angular
        .module('app')
        .controller('orderPendingController', orderPendingController);

    orderPendingController.$inject = ['$scope', 'OrderMasterService', 'OrderDetailsService', 'WarehouseService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal'];

    function orderPendingController($scope, OrderMasterService, OrderDetailsService, WarehouseService, $http, ngAuthSettings, $filter, ngTableParams, $modal) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
       {
            console.log("orderMasterController start loading OrderDetailsService");
            $scope.currentPageStores = {};
            $scope.statuses = [];
            $scope.orders = [];
            $scope.customers = [];
            $scope.selectedd = {};

            // new pagination 
            $scope.pageno = 1;
            $scope.total_count = 0;
            $scope.itemsPerPage = 50;
            $scope.numPerPageOpt = [50, 100, 200, 300];
            $scope.onNumPerPageChange = function () {
                $scope.itemsPerPage = $scope.selected;

            }
            $scope.selected = $scope.numPerPageOpt[0];
            $scope.warehouse = [];
            //$scope.getWarehosues = function () {
            //    WarehouseService.getwarehouse().then(function (results) {
            //        $scope.warehouse = results.data;
            //        $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
            //        $scope.getData($scope.pageno);
            //    }, function (error) {
            //    })
            //};

            $scope.wrshse = function () {
                var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
                $http.get(url)
                    .success(function (data) {
                        $scope.warehouse = data;
                    $scope.WarehouseId = $scope.warehouse[0].value;
                    $scope.getData($scope.pageno);
                    });

            };
          




            //(by neha : 11/09/2019 -date range )
            //$(function () {
            //    $('input[name="daterange"]').daterangepicker({
            //        timePicker: true,

            //        timePickerIncrement: 5,
            //        timePicker12Hour: true,
            //        format: 'DD/MM/YYYY h:mm A'
            //    }, function (start, end, label) {
            //        console.log("A new date selection was made: " + start.format('YYYY/MM/DD') + ' to ' + end.format('YYYY/MM/DD'));
            //    });

            //    $('.input-group-addon').click(function () {
            //        $('input[name="daterange"]').trigger("select");
            //        document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            //    });
            //    //$('input[name="date"]').on('apply.daterangepicker', function (ev, picker) {
            //    //    $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            //    //});
            //    //$('input[name="date"]').on('cancel.daterangepicker', function (ev, picker) {
            //    //    $(this).val('');
            //    //});

            //});


            $scope.wrshse();

            //$scope.getWarehosues();

            $scope.getData = function (pageno) {
                

                $scope.srch = { orderId: 0, skcode: "", shopName: "", mobile: "", status: "", WarehouseId: 0 };

                if ($scope.WarehouseId > 0 && $scope.srch.orderId == 0 && $scope.srch.skcode == "" && $scope.srch.shopName == "" && $scope.srch.mobile == "" && pageno != undefined) {
                    var url = serviceBase + "api/OrderPending" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&WarehouseId=" + $scope.WarehouseId;
                    $http.get(url).success(function (response) {
                        $scope.customers = response.ordermaster;  //ajax request to fetch data into vm.data
                        $scope.dataList = angular.copy(response.ordermaster);
                        $scope.orders = $scope.customers;
                        $scope.total_count = response.total_count;
                        $scope.tempuser = response.ordermaster;
                    });
                }
                else {
                    $scope.searchdata($scope.srch);
                }
            };

            $scope.Refresh = function () {
                //    $scope.srch = "";
                $scope.customers = $scope.orders;
                $scope.total_count = response.total_count;
            }

            $scope.getTemplate = function (data) {
                if (data.OrderId === $scope.selectedd.OrderId) {
                    return 'edit';
                }
                else return 'display';
            };
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
            // status based filter
            $scope.filterOptions = {
                stores: [
                    { id: 2, name: 'Show All' },
                    { id: 3, name: 'Pending' },
                    { id: 4, name: 'Delivered' },
                    { id: 5, name: 'Ready to Dispatch' },
                    { id: 6, name: 'Order Canceled' },
                    { id: 7, name: "sattled" }
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
                if (data.Status == "Ready to Dispatch") {
                    $scope.statuses = [
                        // { value: 1, text: "Delivered" }
                    ];
                }
                else if (data.Status == "Pending") {
                    $scope.statuses = [
                        //{ value: 1, text: "Process" },
                        { value: 2, text: "Order Canceled" }
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
                })
                $('.input-group-addon').click(function () {
                    $('input[name="daterange"]').trigger("select");
                    //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

                });
            });






            $scope.cities = [];
            OrderMasterService.getcitys().then(function (results) {
                $scope.cities = results.data;
            }, function (error) {
            });

            //$scope.warehouse = [];
            //OrderMasterService.getwarehouse().then(function (results) {
            //    $scope.warehouse = results.data;
            //}, function (error) {
            //});
            $scope.InitialData = [];
            $scope.DemandDetails = [];
            $scope.srch = "";
            $scope.statusname = {};
            $scope.searchdata = function (data) {
                
                var f = $('input[name=daterangepicker_start]');
                var g = $('input[name=daterangepicker_end]');
                var start = f.val();
                var end = g.val();

                if (!$('#dat').val() && $scope.srch == "" || $scope.WarehouseId == 0) {
                    start = null;
                    end = null;
                    alert("Please select one parameter");
                    return;
                }
                else if ($scope.srch == "" && $('#dat').val()) {
                    $scope.srch = { orderId: 0, skcode: "", shopName: "", mobile: "", WarehouseId: 0 }
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
                }

                var url = serviceBase + "api/SearchOrder?start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&WarehouseId=" + $scope.WarehouseId + "&status=Pending";
                $http.get(url).success(function (response) {
                    $scope.total_count = response.length;
                    $scope.customers = response;
                });

            }

            //............................Exel export Method...........................// Anil
            $scope.dataforsearch1 = { Cityid: "", Warehouseid: "", datefrom: "", dateto: "" };
            
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
                    $scope.srch = { orderId: 0, skcode: "", shopName: "", mobile: "" }
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
                }
                var url = serviceBase + "api/SearchOrder?type=export&start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=Pending" + "&WarehouseId=" + $scope.WarehouseId;
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
                            var Mobile = $scope.OrderByDate[i].Customerphonenum;
                            var CustomerName = $scope.OrderByDate[i].CustomerName;
                            var CustomerId = $scope.OrderByDate[i].CustomerId;
                            var WarehouseName = $scope.OrderByDate[i].WarehouseName;
                            var Cluster = $scope.OrderByDate[i].Cluster;
                            var Deliverydate = $scope.OrderByDate[i].Deliverydate;
                            var CompanyId = $scope.OrderByDate[i].CompanyId;
                            var BillingAddress = $scope.OrderByDate[i].BillingAddress;
                            var CreatedDate = $scope.OrderByDate[i].CreatedDate;
                            var SalesPerson = $scope.OrderByDate[i].SalesPerson;
                            var ShippingAddress = $scope.OrderByDate[i].ShippingAddress;
                            var delCharge = $scope.OrderByDate[i].deliveryCharge;
                            var Status = $scope.OrderByDate[i].Status;
                            var orderDetails = $scope.OrderByDate[i].orderDetailsExport;
                            for (var j = 0; j < orderDetails.length; j++) {
                                var tts = {
                                    OrderId: '', Skcode: '', ShopName: '', Mobile: '', RetailerName: '', RetailerID: '', Warehouse: '', Cluster: '', Deliverydate: '', CompanyId: '', BillingAddress: '', Date: '',
                                    Excecutive: '', ShippingAddress: '', deliveryCharge: '', ItemID: '', ItemName: '', SKU: '', itemNumber: '', MRP: '', MOQ: '', UnitPrice: '', Quantity: '', MOQPrice: '', Discount: '',
                                    DiscountPercentage: '', TaxPercentage: '', Tax: '', TotalAmt: '', CategoryName: '', BrandName: '', Status: '',
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

                                tts.OrderId = OrderId;
                                tts.RetailerID = CustomerId;
                                tts.Skcode = Skcode;
                                tts.Mobile = Mobile;
                                tts.RetailerName = CustomerName;
                                tts.Warehouse = WarehouseName;
                                tts.Cluster = Cluster;
                                tts.ShopName = ShopName;
                                tts.Deliverydate = Deliverydate;
                                tts.CompanyId = CompanyId;
                                tts.BillingAddress = BillingAddress;
                                tts.Date = CreatedDate;
                                tts.Excecutive = orderDetails[j].ExecutiveName;
                                tts.ShippingAddress = ShippingAddress;
                                tts.deliveryCharge = delCharge;
                                tts.Status = Status;
                                $scope.NewExportData.push(tts);

                            }
                        }
                        alasql.fn.myfmt = function (n) {
                            return Number(n).toFixed(2);
                        }
                        alasql('SELECT OrderId,Skcode,ShopName,ItemID,ItemName,CategoryName,BrandName,SKU,Warehouse,Cluster,Date,Deliverydate,Excecutive,MRP,UnitPrice,MOQPrice,Quantity,Discount,DiscountPercentage,Tax,TaxPercentage,TotalAmt,deliveryCharge,Status INTO XLSX("PendingOrderDetails.xlsx",{headers:true}) FROM ?', [$scope.NewExportData]);
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
                    })
                $scope.apply();

            }
            $scope.Return = function (data) {

                OrderMasterService.saveReturn(data);
                console.log("Order Detail Dialog called ...");

            };
            $scope.mydata = {};
            $scope.showDetail = function (data) {
                $http.get(serviceBase + 'api/OrderMaster?id=' + data.OrderId).then(function (results) {
                    $scope.myData = results.data;
                    OrderMasterService.save($scope.myData)
                    setTimeout(function () {
                        console.log("Modal opened Orderdetails");
                        var modalInstance;
                        modalInstance = $modal.open(
                            {
                                templateUrl: "myOrderdetail.html",
                                controller: "orderdetailsController", resolve: { Ord: function () { return $scope.items } }
                            });
                        modalInstance.result.then(function (selectedItem) {

                                console.log("modal close");
                                console.log(selectedItem);
                                if (selectedItem.Status == "Ready to Dispatch") {
                                    $("#st" + selectedItem.OrderId).html(selectedItem.Status);
                                    $("#st" + selectedItem.OrderId).removeClass('canceled');
                                }

                            },
                                function () {
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
                    })
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
        }

    }
})();


