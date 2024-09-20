
(function () {
    'use strict';

    angular
        .module('app')
        .controller('OnHoldGRController', OnHoldGRController);

    OnHoldGRController.$inject = ['$scope', 'SearchPOService', 'WarehouseService', 'CityService', 'PurchaseODetailsService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal'];

    function OnHoldGRController($scope, SearchPOService, WarehouseService, CityService, PurchaseODetailsService, $http, ngAuthSettings, $filter, ngTableParams, $modal) {

        $scope.Warehouseid = 1;
        $scope.getWarehosues = function () {
            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;
                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;

            }, function (error) {
            })
        };
        $scope.getWarehosues();

        //ExportMethod//
        $scope.exportdatewiseData1 = function () {

            $scope.exdatedata = $scope.OnHoldGR;

            alasql('SELECT OnHoldGRId,CityName,WarehouseId,WarehouseName,ItemName,Discription,MRP,Price,Qty,InvoiceNo,CreatedDate,UpdatedDate,CreatedBy,UpdateBy,comments,Image  INTO XLSX("OnHoldGR.xlsx",{headers:true}) FROM ?', [$scope.exdatedata]);
        };


        //ExportMethod//

        $scope.getOnHoldGR = function () {
            var url = serviceBase + 'api/OnHoldGR/getAllGR';
            $http.get(url).success(function (results) {

                $scope.OnHoldGR = results;
                $scope.callmethod();
            })
        }
        $scope.getOnHoldGR();

        $scope.AddGR = function (data) {

            console.log("OnHoldGR");
            console.log(data);


            var url = serviceBase + "api/OnHoldGR/AddGR";
            var dataToPost = {
                WarehouseId: data.WarehouseId,
                InvoiceNo: data.InvoiceNo,
                Itemname: data.Itemname,
                MRP: data.MRP,
                Price: data.Price,
                Qty: data.Qty,
                Discription: data.Discription


            };
            console.log(dataToPost);

            $http.post(url, dataToPost)

                .success(function (data) {

                    console.log("Error Got Here");
                    console.log(data);
                    if (data.id == 0) {

                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }
                    }

                    else {
                        console.log(data);
                        console.log(data);

                    }
                    confirm("SAVE DATA SUCCESSFULLY!!!");

                })

                .error(function (data) {
                    console.log("Error Got Here is ");
                    console.log(data);
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })


        };

        //ForPopUp-Image//
        //$scope.openimg = function (data) {
        $scope.opening = function (data) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "imageViewOnHold.html",
                    controller: "ImageControllerOnHold", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (data) {
                },
                    function () { })
        };

        //ForPopUp-Image//



        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.OnHoldGR;

            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";


            $scope.numPerPageOpt = [10, 20, 30];
            $scope.numPerPage = $scope.numPerPageOpt[0];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(), $scope.select(1);
        }

        $scope.select = function (page) {
            var end, start; console.log("select"); console.log($scope.stores);
            start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage; $scope.currentPageStores = $scope.filteredStores.slice(start, end);
        }

        $scope.onFilterChange = function () {
            console.log("onFilterChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1; $scope.row = "";
        }

        $scope.onNumPerPageChange = function () {
            console.log("onNumPerPageChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1;
        }

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

        $scope.AddComment = function (data) {

            var url = serviceBase + "api/OnHoldGR/UpdateGR";
            $http.post(url, data)

                .success(function (data) {

                    console.log("Error Got Here");
                    console.log(data);

                    if (data.id === 0) {

                        $scope.gotErrors = true;
                        if (data[0].exception === "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }
                    }

                    else {
                        console.log(data);
                        console.log(data);

                    }
                    confirm("SAVE DATA SUCCESSFULLY!!!");
                    window.location.reload();
                })

                .error(function (data) {
                    console.log("Error Got Here is ");
                    console.log(data);
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })
        }

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ImageControllerOnHold', ImageControllerOnHold);

    ImageControllerOnHold.$inject = ["$scope", "$modalInstance", "object", '$modal'];

    function ImageControllerOnHold($scope, $modalInstance, object, $modal) {

        $scope.itemMasterrr = {};
        $scope.saveData = [];
        if (object) {
            $scope.ImageOnHold = object;
        }

        $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
    }
})();
