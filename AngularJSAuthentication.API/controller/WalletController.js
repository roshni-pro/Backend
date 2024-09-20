

(function () {
    'use strict';

    angular
        .module('app')
        .controller('WalletController', WalletController);

    WalletController.$inject = ['$scope', 'OrderMasterService', "$filter", "$http", "ngTableParams", '$modal','CityService'];

    function WalletController($scope, OrderMasterService, $filter, $http, ngTableParams, $modal, CityService) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));

        {

            $scope.Refresh = function () {
                location.reload();
            }

            $(function () {
                $('input[name="daterange"]').daterangepicker({
                    timePicker: true,
                    timePickerIncrement: 5,
                    timePicker12Hour: true,
                    format: 'MM/DD/YYYY h:mm A'
                });
                $('.input-group-addon').click(function () {
                    $('input[name="daterange"]').trigger("select");
                    //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

                });
            });
            $scope.wallets = [];
            $scope.Getcurrent = [];
            $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
            $scope.getWalletData = function () {

                $scope.wallets = [];
                $scope.data = [];
                $scope.Getcurrent = [];
                var url = serviceBase + "api/wallet";
                $http.get(url).success(function (response) {
                    $scope.wallets = response;
                    $scope.data = $scope.wallets;
                    $scope.Getcurrent = angular.copy($scope.wallets);
                    $scope.allcusts = true;
                    $scope.tableParams = new ngTableParams({
                        page: 1,
                        count: 50
                    }, {
                            total: $scope.data.length,
                            getData: function ($defer, params) {
                                var orderedData = params.sorting() ? $filter('orderBy')($scope.data, params.orderBy()) : $scope.data;
                                orderedData = params.filter() ?
                                    $filter('filter')(orderedData, params.filter()) :
                                    orderedData;
                                $defer.resolve(orderedData.slice((params.page() - 1) * params.count(), params.page() * params.count()));
                            }
                        });
                });
            };






            OrderMasterService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;
            }, function (error) {
            });

            $scope.cities = [];

            CityService.getcitys().then(function (results) {
                $scope.cities = results.data;
            }, function (error) { });

            $scope.selectedwarehouse = function (data) {

                var id = parseInt(data);
                $scope.filterData = $filter('filter')($scope.Getcurrent, function (value) {
                    return value.WarehouseId === id;
                });

                $scope.wallets = [];
                $scope.data = [];
                $scope.tableParams = [];


                setTimeout(function () {
                    $scope.$apply(function () {
                        $scope.wallets = $scope.filterData;
                        $scope.data = $scope.wallets;
                        $scope.allcusts = true;

                        $scope.tableParams = new ngTableParams({
                            page: 1,
                            count: 50
                        }, {
                                total: $scope.data.length,
                                getData: function ($defer, params) {
                                    var orderedData = params.sorting() ? $filter('orderBy')($scope.data, params.orderBy()) : $scope.data;
                                    orderedData = params.filter() ?
                                        $filter('filter')(orderedData, params.filter()) :
                                        orderedData;
                                    $defer.resolve(orderedData.slice((params.page() - 1) * params.count(), params.page() * params.count()));
                                }
                            });

                    })
                }, 500);
            }

            $scope.SearchData = function (WarehouseId) {

                var f = $('input[name=daterangepicker_start]');
                var g = $('input[name=daterangepicker_end]');
                var start = f.val();
                var end = g.val();

                if (!$('#dat').val()) {
                    start = null;
                    end = null;
                    alert("Please select one parameter");
                    return false;
                }
                else if (WarehouseId == "" || WarehouseId == undefined) {
                    alert("Please select one Warehouse");
                    return false;
                }
                else {
                    $scope.wallets = [];
                    $scope.data = [];
                    $scope.tableParams = [];
                    var url = serviceBase + "api/wallet/Search?start=" + start + "&end=" + end + "&WarehouseId=" + WarehouseId;
                    $http.get(url).success(function (response) {

                        setTimeout(function () {
                            $scope.$apply(function () {

                                $scope.wallets = response;
                                $scope.data = $scope.wallets;
                                $scope.allcusts = true;
                                $scope.tableParams = new ngTableParams({
                                    page: 1,
                                    count: 50
                                }, {
                                        total: $scope.data.length,
                                        getData: function ($defer, params) {
                                            var orderedData = params.sorting() ? $filter('orderBy')($scope.data, params.orderBy()) : $scope.data;
                                            orderedData = params.filter() ?
                                                $filter('filter')(orderedData, params.filter()) :
                                                orderedData;
                                            $defer.resolve(orderedData.slice((params.page() - 1) * params.count(), params.page() * params.count()));
                                        }
                                    });


                            })
                        }, 500);
                    });

                }
            };



            $scope.AddManualWallet = function (data) {

                

                var url = serviceBase + "api/wallet/addmw";

                var dataToPost = {
                    Name: data.Name,
                    CityId: data.city
                }


                $http.post(url, dataToPost).success(function (data) {
                    if (data != null && data != "null") {
                        alert("Manual Wallet Added Successfully... :-)");
                        //$modalInstance.close();
                        location.reload();
                    }
                    else {
                        alert("got some error... :-)");
                        //$modalInstance.close();
                    }
                })
                    .error(function (data) {
                    })
            };    

            $scope.Manualwallet = [];
            $scope.filltable = function () {
                
                var url = serviceBase + "api/wallet/GetManualWallet";
                $http.get(url).success(function (response) {
                    $scope.Manualwallet = response;
                    $scope.callmethod();
                });
            }
            $scope.callmethod = function () {
               
                var init;
                return $scope.stores = $scope.Manualwallet,



                

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
                        ()
            }

            $scope.filltable();



            // For export data
            alasql.fn.myfmt = function (n) {
                return Number(n).toFixed(2);
            }

            //Wallet History
            $scope.exportData = function () {
                alasql('SELECT City,WarehouseName,CustomerId,Skcode,ShopName,TotalAmount,CreatedDate INTO XLSX("WalletPointCustomer.xlsx",{headers:true}) FROM ?', [$scope.wallets]);
            };
            $scope.getWalletData();
            $scope.open = function () {
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myADDModal.html",
                        controller: "WalletControllerAddController", resolve: { object: function () { return $scope.data } }
                    });
                modalInstance.result.then(function (selectedItem) {
                    },
                        function () {
                        })
            };
            $scope.edit = function (wallets) {
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myEDITModal.html",
                        controller: "WalletControllerAddController", resolve: { object: function () { return wallets } }
                    });
                modalInstance.result.then(function (wallets) {
                    },
                        function () {
                        })
            };
            $scope.openCashConvesion = function () {
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "cashConversionModal.html",
                        controller: "WalletConversionController", resolve: { object: function () { return $scope.data } }
                    });
                modalInstance.result.then(function (selectedItem) {
                    },
                        function () {
                        })
            };
            //-----for popupdial-----
            // new pagination 
            $scope.pageno = 1; //initialize page no to 1
            $scope.total_count = 0;

            $scope.itemsPerPage = 10;  //this could be a dynamic value from a drop down

            $scope.numPerPageOpt = [10, 30, 50, 200];  //dropdown options for no. of Items per page

            $scope.onNumPerPageChange = function () {
                $scope.itemsPerPage = $scope.selected;

            }
            $scope.selected = $scope.numPerPageOpt[0];  //for Html page dropdown

            $scope.$on('$viewContentLoaded', function () {
                // $scope.WalletHistory($scope.pageno);
            });

            $scope.CustomerId = 0;
            $scope.WalletHistorys = function (data) {

                $scope.CustomerId = data.CustomerId;
                $scope.WalletHistory($scope.pageno);
            }
            $scope.WalletHistory = function (pageno) {

                $scope.OldStockData = [];
                var url = serviceBase + "api/wallet" + "?CustomerId=" + $scope.CustomerId + "&list=" + $scope.itemsPerPage + "&page=" + pageno;
                $http.get(url).success(function (response) {

                    $scope.OldStockData = response.ordermaster;
                    $scope.total_count = response.total_count;
                })
                    .error(function (data) {
                    })
            }

            //Customer detail wallet history data 
            $scope.HistoryexportData = function () {

                $scope.exportDataRecord = [];
                var url = serviceBase + "api/wallet/Export" + "?CustomerId=" + $scope.CustomerId;
                $http.get(url).success(function (response) {

                    $scope.exportDataRecord = response;
                    alasql('SELECT TotalWalletAmount,NewAddedWAmount,NewOutWAmount,Through,OrderId,PeopleName,CreatedDate INTO XLSX("CustomerWalletHistory.xlsx",{headers:true}) FROM ? ', [$scope.exportDataRecord]);
                })
                    .error(function (data) {
                    })
            }

        }
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('WalletControllerAddController', WalletControllerAddController);

    WalletControllerAddController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal', 'customerService'];

    function WalletControllerAddController($scope, $http, ngAuthSettings, $modalInstance, object, $modal, customerService) {


        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.customer = {};
        $scope.GetCustomer = function (skcode) {
            $scope.customer = {};
            customerService.getcustomerBySkcode(skcode).then(function (results) {

                if (results.data != null) {
                    $scope.customer = results.data;
                    alert("SHOP NAME:" + $scope.customer.ShopName + " & MOBILE:" + $scope.customer.Mobile);
                }
                else {
                    $scope.customer = null;
                    alert("Enter Correct Skcode:");
                }
            }, function (error) {
                alert("InCorrect Skcode:");
            });
        };

        $scope.saveData = {};
        if (object) {
            object.CreditAmount = null;
            object.Through = null;
            $scope.saveData = object;
        }

        $scope.Manualwalletdropdown = [];
        $scope.filldropdown = function () {
            
            var url = serviceBase + "api/wallet/GetManualName";
            $http.get(url).success(function (response) {
                
                $scope.Manualwalletdropdown = response;


                if ($scope.Manualwalletdropdown && $scope.Manualwalletdropdown.length > 0) {
                    var tempList = $scope.Manualwalletdropdown.filter(function (element) {
                        return element.Name == 'Special';
                    });

                    if (tempList && tempList.length > 0) {
                        
                        $scope.specialWalletID = tempList[0].Name;
                    }

                }
            });
        }
        $scope.filldropdown();
        $scope.AddWallet = function () {
            
            console.log($scope.saveData.Through);
            //if ($scope.saveData.Through == undefined || $scope.saveData.Through == null) {
            //    alert("Insert comment");
            //    return false;
            //}
            if ($scope.saveData.CreditAmount == undefined || $scope.saveData.CreditAmount == null) {
                alert("Insert Add Wallet Amount");
                return false;
            }
            if (object) {
                $scope.customer.CustomerId = $scope.saveData.CustomerId;
                //$scope.customer.CreditAmount = $scope.saveData.CreditAmount;
                $scope.saveData.TotalAmount = parseInt($scope.saveData.CreditAmount) + parseInt($scope.saveData.TotalAmount);
            }
            var dataToPost = {
                CustomerId: $scope.customer.CustomerId,
                CreditAmount: $scope.saveData.CreditAmount,
                Through: $scope.saveData.Through,
                Comment: $scope.saveData.Comment,
            }
            console.log(dataToPost);

            $("#disabledButton").prop("disabled", true);

            var url = serviceBase + "api/wallet";
            $http.post(url, dataToPost).success(function (data) {
                if (data != null && data != "null") {
                    alert("Wallet Amount Added Successfully... :-)");
                    $modalInstance.close();
                    //location.reload();
                }
                else {
                    alert("got some error... :-)");
                    $modalInstance.close();
                }
            })
                .error(function (data) {
                })
        };    
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('WalletConversionController', WalletConversionController);

    WalletConversionController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", '$modal'];

    function WalletConversionController($scope, $http, ngAuthSettings, $modalInstance, $modal) {

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.pointData = [];

        $http.get(serviceBase + "api/wallet/cash").success(function (data) {
            if (data != null && data != "null") {
                $scope.pointData = data;
            }
        })
        $scope.AddcashData = function () {

            if ($scope.pointData.point == null || $scope.pointData.rupee == null) { alert('Insert parameter'); return false; }
            var dataToPost = {
                Id: $scope.pointData.Id,
                point: $scope.pointData.point,
                rupee: $scope.pointData.rupee
            }
            console.log(dataToPost);
            var url = serviceBase + "api/wallet/cash";

            $http.post(url, dataToPost).success(function (data) {
                if (data != null && data != "null") {
                    alert("margin point Added Successfully... :-)");
                    $modalInstance.close();
                }
                else {
                    alert("got some error... :-)");
                    $modalInstance.close();
                }
            })
                .error(function (data) {
                })
        };
    }
})();



