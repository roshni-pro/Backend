
(function () {
    'use strict';

    angular
        .module('app')
        .controller('promItemController', promItemController);

    promItemController.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal', 'supplierService'];

    function promItemController($scope, $filter, $http, ngTableParams, $modal, supplierService) {
        $scope.isShow = false;
        $scope.SuPnt = false;
        $scope.supShow = false;
        $scope.currentPageStores = {};
        $scope.supplier = [];
        $scope.SupplierCode = "";
        // new pagination 
        $scope.pagenoOne = 0;
        $scope.pageno = 1;   //initialize page no to 1
        $scope.total_count = 0;
        $scope.numPerPageOpt = [30, 50, 100, 200];  //dropdown options for no. of Items per page
        $scope.itemsPerPage = $scope.numPerPageOpt[1]; //this could be a dynamic value from a drop down
        $scope.onNumPerPageChange = function () {
            $scope.itemsPerPage = $scope.selectedPagedItem;
            $scope.getData1($scope.pageno);
        };
        $scope.selectedPagedItem = $scope.numPerPageOpt[1];  // for Html page dropdown

        $scope.getData1 = function (pageno) {
            if ($scope.pagenoOne !== pageno) {
                $scope.pagenoOne = pageno;
                $scope.itemMasters = [];
                var url = serviceBase + "api/itemMaster/supplier" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&SupplierCode=" + $scope.SupplierCode;
                $http.get(url).success(function (response) {
                    if (response.total_count > 0) {
                        $scope.isShow = true;
                        $scope.total_count;
                       // $scope.total_count = '';
                        $scope.itemMasters = response.ordermaster;
                        $scope.total_count = response.total_count;
                        $scope.currentPageStores = $scope.itemMasters;

                        $http.get(serviceBase + "api/pointConversion/promopurchase?SupplierCode=" + $scope.SupplierCode).success(function (data) {
                            if (data !== null && data !== "null") {
                                $scope.SupplierPoint = data;
                                $scope.SuPnt = true;
                            }
                            else {
                                alert("You Didn't have Promo Point, please purchase Promo Point");
                                $scope.SuPnt = false;
                            }
                        });
                    }
                    else {
                        $scope.isShow = false;
                        alert("Item Not Found");
                    }
                });
            }
        };


        var User = JSON.parse(localStorage.getItem('RolePerson'));
        if (User.role === "Supplier") {
            $scope.supShow = false;
            $scope.SupplierCode = User.Skcode;
            $scope.getData1($scope.pageno);
        }
        else if (User.role === "HQ Master login") {
            $scope.supplier = [];
            $scope.supShow = true;
            supplierService.getsuppliers().then(function (results) {
                $scope.supplier = results.data;
            }, function (error) {
            });
        }
        else if (User.role === "WH Master login") {

            $scope.supplier = [];
            $scope.supShow = true;
            supplierService.getsuppliers().then(function (results) {
                $scope.supplier = results.data;
            }, function (error) {
            });
        }

        $scope.getSupplierStores = function (supplier) {
            $scope.pagenoOne = 0;
            $scope.pageno = 1;
            $scope.total_count = 0;
            $scope.SupplierCode = supplier;
            $scope.getData1($scope.pageno);
        };
        $scope.search = function () {

            console.log($scope.SupplierCode);

            var url = serviceBase + "api/itemMaster/Search?key=" + $scope.searchKeywords + "&SupplierCode=" + $scope.SupplierCode;
            $http.get(url).success(function (response) {
                $scope.currentPageStores = response;
            });
        };

        $scope.refresh = function () {
            $scope.currentPageStores = $scope.itemMasters;
            $scope.pagenoOne = 0;
        };

        $scope.edit = function (item) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myitemMasterPut.html",
                    controller: "ModalInstanceCtrlPromoitemMaster", resolve: { itemMaster: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                    })
        };

        $scope.open = function () {
            if ($scope.SupplierCode !== "" && $scope.SupplierCode !== null) {
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "purchaseModel.html",
                        controller: "purchaseModalInstanceCtrl", resolve: { itemMaster: function () { return $scope.SupplierCode } }
                    });
                modalInstance.result.then(function (selectedItem) {
                    },
                        function () {
                        })
            }
            else {
                alert("Please Select Supplier");
            }
        };

        $scope.pointConv = function () {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "promoADDModal.html",
                    controller: "promoAddController", resolve: { object: function () { return $scope.item } }
                });
            modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                    })
        };
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlPromoitemMaster', ModalInstanceCtrlPromoitemMaster);

    ModalInstanceCtrlPromoitemMaster.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "itemMaster"];

    function ModalInstanceCtrlPromoitemMaster($scope, $http, ngAuthSettings, $modalInstance, itemMaster) {
        var input = document.getElementById("file");
        $scope.itemMasterData = {};
        if (itemMaster) {
            $scope.itemMasterData = itemMaster;
        }
        $http.get(serviceBase + "api/pointConversion/promopurchase?SupplierCode=" + $scope.itemMasterData.SUPPLIERCODES).success(function (data) {
            if (data !== null && data !== "null") {
                $scope.SupplierPoint = data;
            }
            else {
                alert("You Didn't have Promo Point, please purchase Promo Point");
                $modalInstance.close();
            }
        });

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.PutitemMaster = function (data) {
            var url = serviceBase + "api/itemMaster";
            if ($scope.SupplierPoint.PromoPoint < $scope.itemMasterData.promoPoint) {
                alert("You not have Sufficient PromoPoint. Your Total Promo Point is :" + $scope.SupplierPoint.PromoPoint);
            }
            else {
                var dataToPost = {
                    ItemId: $scope.itemMasterData.ItemId,
                    itemname: $scope.itemMasterData.itemname,
                    itemcode: $scope.itemMasterData.itemcode,
                    Cityid: $scope.itemMasterData.Cityid,
                    Categoryid: $scope.itemMasterData.Categoryid,
                    SubCategoryId: $scope.itemMasterData.SubCategoryId,
                    SubsubCategoryid: $scope.itemMasterData.SubsubCategoryid,
                    SupplierId: $scope.itemMasterData.SupplierId,
                    MinOrderQty: $scope.itemMasterData.MinOrderQty,
                    PurchaseMinOrderQty: $scope.itemMasterData.PurchaseMinOrderQty,
                    UnitId: $scope.itemMasterData.UnitId,
                    Id: $scope.itemMasterData.Id,
                    UnitPrice: $scope.itemMasterData.UnitPrice,
                    Discount: $scope.itemMasterData.Discount,
                    GruopID: $scope.itemMasterData.GruopID,
                    Placeorder: $scope.itemMasterData.Placeorder,
                    GeneralPrice: $scope.itemMasterData.GeneralPrice,
                    Number: $scope.itemMasterData.Number,
                    Barcode: $scope.itemMasterData.Barcode,
                    PurchaseUnitName: $scope.itemMasterData.PurchaseUnitName,
                    SellingUnitName: $scope.itemMasterData.SellingUnitName,
                    price: $scope.itemMasterData.price,
                    PurchaseSku: $scope.itemMasterData.PurchaseSku,
                    PurchasePrice: $scope.itemMasterData.PurchasePrice,
                    SellingSku: $scope.itemMasterData.SellingSku,
                    SellingPrice: $scope.itemMasterData.UnitPrice,
                    VATTax: $scope.itemMasterData.VATTax,
                    UpdatedDate: $scope.itemMasterData.UpdatedDate,
                    LogoUrl: $scope.itemMasterData.loogourl,
                    active: $scope.itemMasterData.active,
                    IsDailyEssential: $scope.itemMasterData.IsDailyEssential,
                    Margin: $scope.itemMasterData.Margin,
                    promoPoint: $scope.itemMasterData.promoPoint,
                    HindiName: $scope.itemMasterData.HindiName,
                    warehouse_id: $scope.itemMasterData.warehouse_id,
                    promoPerItems: $scope.itemMasterData.promoPerItems
                };
                $http.put(url, dataToPost)
                    .success(function (data) {
                        if (data.id === 0) {
                            $scope.gotErrors = true;
                            if (data[0].exception === "Already") {
                                $scope.AlreadyExist = true;
                            }
                        }
                        else {
                            $modalInstance.close(data);
                        }
                    })
                    .error(function (data) {
                    });
            }
        };
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('purchaseModalInstanceCtrl', purchaseModalInstanceCtrl);

    purchaseModalInstanceCtrl.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", 'itemMaster'];

    function purchaseModalInstanceCtrl($scope, $http, ngAuthSettings, $modalInstance, itemMaster) {

        $scope.supplier = {};
        if (itemMaster) {
            $scope.supplier.SupplierCode = itemMaster;
            $scope.supplier.Amount = 0;
        }

        $scope.pointData = {};
        $http.get(serviceBase + "api/pointConversion/promo").success(function (data) {
            if (data!== null && data!== "null") {
                $scope.pointData = data;
            }
        })
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.PutitemMaster = function () {
                var url = serviceBase + "api/pointConversion/promopurchase";
                var dataToPost = {
                    id: $scope.supplier.id,
                    SupplierName: $scope.supplier.SupplierName,
                    SupplierCode: $scope.supplier.SupplierCode,
                    Point: $scope.supplier.Point,
                    Amount: $scope.supplier.Amount
                };
                $http.post(url, dataToPost)
                    .success(function (data) {
                        if (data.id === 0) {
                            $scope.gotErrors = true;
                            if (data[0].exception === "Already") {
                                $scope.AlreadyExist = true;
                            }
                        }
                        else {
                            $modalInstance.close(data);
                        }
                    })
                    .error(function (data) {
                    })
            };
        $scope.AmountCalculation = function () {
            $scope.supplier.Amount = ($scope.supplier.Point / $scope.pointData.point);
            $scope.Amount = $scope.supplier.Amount;
        };
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('promoAddController', promoAddController);

    promoAddController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", '$modal'];

    function promoAddController($scope, $http, ngAuthSettings, $modalInstance, $modal) {
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.pointData = {};
        $http.get(serviceBase + "api/pointConversion/promo").success(function (data) {
            if (data !== null && data !== "null") {
                $scope.pointData = data;
            }
        })
        $scope.Add = function () {
            var dataToPost = {
                Id: $scope.pointData.Id,
                point: $scope.pointData.point,
                rupee: $scope.pointData.rupee
            }
            console.log(dataToPost);
            var url = serviceBase + "api/pointConversion/promo";
            $http.post(url, dataToPost).success(function (data) {
                if (data !== null && data !== "null") {
                    alert("margin point Added Successfully... :-)");
                    $modalInstance.close();
                }
                else {
                    alert("got some error... :-)");
                    $modalInstance.close();
                }
            })
                .error(function (data) {
                });
        };
    }
})();