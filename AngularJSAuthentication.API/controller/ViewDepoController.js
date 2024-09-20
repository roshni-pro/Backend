

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ViewDepoController', ViewDepoController);

    ViewDepoController.$inject = ['$scope', 'FileUploader', 'supplierService', "$filter", "$http", "ngTableParams", '$modal'];

    function ViewDepoController($scope, FileUploader, supplierService, $filter, $http, ngTableParams, $modal) {

        var viewdetail = supplierService.getviewdata();
        $scope.depodata = viewdetail;

        $scope.GetDepoData = function () {

            var url = serviceBase + "api/Suppliers/GetDepo?id=" + $scope.depodata.SupplierId;
            $http.get(url)
                .success(function (response) {

                    $scope.DepoDetaildata = response;
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                });
        };
        $scope.GetDepoData();

        $scope.EditDepo = function (data) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myDepoModalPut.html",
                    controller: "PutDepoController", resolve: { data: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.EditDepo = selectedItem;

            },
                function () {
                })
        };

        $scope.ActiveDepo = function (item) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myactiveDepomodal.html",
                    controller: "ActiveDepoctrl", resolve: { item: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };


        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.item;
            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";
            $scope.select = function (page) {
                var end, start; console.log("select"); console.log($scope.stores);
                start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage; $scope.currentPageStores = $scope.filteredStores.slice(start, end);
            }
            $scope.onFilterChange = function () {
                console.log("onFilterChange"); console.log($scope.stores);
                $scope.select(1); $scope.currentPage = 1, $scope.row = ""
            }
            $scope.onNumPerPageChange = function () {
                console.log("onNumPerPageChange"); console.log($scope.stores);
                $scope.select(1); $scope.currentPage = 1
            }
            $scope.onOrderChange = function () {
                console.log("onOrderChange"); console.log($scope.stores);
                $scope.select(1); $scope.currentPage = 1
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

            $scope.numPerPageOpt = [20, 50, 100, 200];
            $scope.numPerPage = $scope.numPerPageOpt[1];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(); $scope.select(1);
        }

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('PutDepoController', PutDepoController);

    PutDepoController.$inject = ["$scope", '$http', 'CityService', 'StateService', "$modalInstance", 'supplierService', 'data'];

    function PutDepoController($scope, $http, CityService, StateService, $modalInstance, supplierService, data) {

        $scope.EditDepo = data;

        var input = document.getElementById("file");
        console.log(input);

        var today = new Date();
        $scope.today = today.toISOString();


        

        $scope.states = [];
        StateService.getstates().then(function (results) {
            console.log("sumit");
            console.log(results.data);
            $scope.states = results.data;

        }); 


        $scope.GetDepoEdit = function (data) {

            var url = serviceBase + 'api/City/suppliercity?Statid=' + data;
            $http.get(url)
                .success(function (response) {

                    $scope.citys = response;

                });
        };


        $scope.citys = [];
        CityService.getcitys().then(function (results) {
            $scope.citys = results.data;

        }, function (error) {
        });


        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.PutDepo = function (data) {

            $scope.EditDepo = data;
            console.log("found Put Depo");
            console.log(data);
            console.log($scope.EditDepo);

            console.log("Update Depo");
            var url = serviceBase + "api/Suppliers/PutDepo";
            
            var dataToPost = {
                SupplierId: $scope.EditDepo.SupplierId,
                DepoName: $scope.EditDepo.DepoName,
                DepoId: $scope.EditDepo.DepoId,
                Address: $scope.EditDepo.Address,
                Email: $scope.EditDepo.Email,
                Phone: $scope.EditDepo.Phone,
                UpdatedDate: $scope.EditDepo.UpdatedDate,
                IsActive: $scope.EditDepo.IsActive,
                ContactPerson: $scope.EditDepo.ContactPerson,
                GSTin: $scope.EditDepo.GSTin,
                Stateid: $scope.EditDepo.Stateid,
                Cityid: $scope.EditDepo.Cityid,
                StateName: $scope.EditDepo.StateName,
                CityName: $scope.EditDepo.CityName,
                //--tejas
                FSSAI: $scope.EditDepo.FSSAI,
                CityPincode: $scope.EditDepo.CityPincode,
                Bank_Name: $scope.EditDepo.Bank_Name,
                Bank_AC_No: $scope.EditDepo.Bank_AC_No,
                BankAddress: $scope.EditDepo.BankAddress,
                Bank_Ifsc: $scope.EditDepo.Bank_Ifsc,
                BankPinCode: $scope.EditDepo.BankPinCode,
                PANCardNo: $scope.EditDepo.PANCardNo,
                OpeningHours: $scope.EditDepo.OpeningHours,
                PRPOStopAfterValue: $scope.EditDepo.PRPOStopAfterValue
                //--tejas
            };
            
            console.log(dataToPost);
            $http.put(url, dataToPost)
                .success(function (data) {
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
                        $modalInstance.close(data);
                        alert("Depo Update Successfully");
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);

                    return $scope.showInfoOnSubmit = !0, $scope.revert()
                });
        }

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ActiveDepoctrl', ActiveDepoctrl);

    ActiveDepoctrl.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "item", 'FileUploader'];

    function ActiveDepoctrl($scope, $http, ngAuthSettings, $modalInstance, item, FileUploader) {
        console.log("Active modal opened");

        var input = document.getElementById("file");
        var today = new Date();
        $scope.today = today.toISOString();
        $scope.$watch('files', function () {
            //$scope.upload($scope.files);
        });
        ////for image

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.ActiveData = item;
        $scope.ActDepo = function (item) {

            console.log("Update Supplier");
            var url = serviceBase + "api/Suppliers/ActivateDepo";
            var dataToPost = {
                DepoId: item.DepoId,
                SupplierId: item.SupplierId,
                IsActive: item.IsActive
            };
            console.log(dataToPost);
            $http.put(url, dataToPost)
                .success(function (item) {

                    console.log("Error Gor Here");
                    console.log(item);
                    if (item.id == 0) {
                        $scope.gotErrors = true;
                        if (item[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                        alert("Update Successfully");
                        $modalInstance.close(item);

                    }
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                })


        };

    }
})();











