

(function () {
    'use strict';

    angular
        .module('app')
        .controller('CRMcust4ActionCtrl', CRMcust4ActionCtrl);

    CRMcust4ActionCtrl.$inject = ['$scope', "WarehouseService", "$filter", "$http", "ngTableParams", '$modal', 'CityService', 'supplierService', 'ClusterService', 'Service'];

    function CRMcust4ActionCtrl($scope, WarehouseService, $filter, $http, ngTableParams, $modal, CityService, supplierService, ClusterService, Service) {

        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A',
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });
        });
        $scope.CRMData = {};
        $scope.CRMData.id = 0;
        $scope.CRMData.GL_Type = 1;
        $scope.CRMData.values = 0;

        $scope.selectType = [
            //{ value: 1, text: "Retailer" },
            { value: 2, text: "Hub" },
            { value: 3, text: "City" },
            { value: 4, text: "Executive" },
            { value: 5, text: "Cluster" }
        ];

        $scope.catType = [
            { value: 1, text: "Category" },
            { value: 2, text: "SubCatogary" },
            { value: 3, text: "SubSubCatogory" }
        ];

        $scope.GL_Type = [
            { value: 1, text: "Greater Then" },
            { value: 2, text: "Less Than" }
        ];

        $scope.selType = [
            { value: 1, text: "Total Order" },
            { value: 2, text: "Total sale" }
        ];

        $scope.selectedtypeChanged = function (data) {
            $scope.dataselect = [];
            var url = serviceBase + "api/Report/select?value=" + data.type;
            $http.get(url)
                .success(function (data) {
                    if (data.length == 0) {
                        alert("Not Found");
                    }
                    $scope.dataselect = data;
                    console.log(data);
                });
        };

        $scope.selectedItemChanged = function (data) {
            $scope.dataselect1 = [];
            var url = serviceBase + "api/Report/Catogory?value=" + data.cattype;
            $http.get(url)
                .success(function (data) {
                    if (data.length == 0) {
                        alert("Not Found");
                    }
                    $scope.dataselect1 = data;
                    console.log(data);
                });
        };

        $scope.customers = [];

        $scope.getdata = function (data) {
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
            if ($scope.CRMData.id != "" && $scope.CRMData.id != 0) {
                var url = "ActionTask/getcustomer?id=" + $scope.CRMData.id + "&filter=" + data.type + "&start=" + start + "&end=" + end + "&cattype=" + $scope.CRMData.cattype + "&catid=" + $scope.CRMData.catid + "&GL_Type=" + $scope.CRMData.GL_Type + "&values=" + $scope.CRMData.values;
                Service.get(url).then(function (results) {
                    console.log(results);


                    $scope.customers = results.data;
                    if (data.length == 0) {
                        alert("Not Found");
                    }
                    else {
                    }
                    console.log(data);

                }, function (error) {
                })
            } else { alert("select type") }
        }

     




        $scope.open = function (data) {
            console.log("Modal opened Role");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myActionModal.html",
                    controller: "CRMcustActionTaskCtrl", resolve: { role: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {


                    $scope.currentPageStores.push(selectedItem);

                },
                    function () {
                        console.log("Cancel Condintion");

                    })
        };
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('CRMcustActionTaskCtrl', CRMcustActionTaskCtrl);

    CRMcustActionTaskCtrl.$inject = ['$scope', "$http", '$modal', "$modalInstance", 'role', "peoplesService"];

    function CRMcustActionTaskCtrl($scope, $http, $modal, $modalInstance, role, peoplesService) {
        $scope.customers = {};
        $scope.peoples = {};

        if (role) {
            $scope.customers = role;
        }

        $scope.peoples = [];
        peoplesService.getpeoples().then(function (results) {
            console.log("Peoples");
            console.log(results.data);
            $scope.peoples = results.data;
        }, function (error) {
        });

        $scope.Action = function (data) {
            console.log("Action");
            var url = serviceBase + "api/ActionTask";
            var dataToPost = {
                CustomerId: $scope.customers.CustomerId,
                ShopName: $scope.customers.ShopName,
                Skcode: $scope.customers.Skcode,
                WarehouseId: $scope.customers.Warehouseid,
                PeopleID: $scope.customers.PeopleID,
                Action: $scope.customers.Action,
                Status: $scope.customers.Status,
                CompletionDate: $scope.customers.CompletionDate
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
                        $modalInstance.close(data);
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })
        };
    }
})();