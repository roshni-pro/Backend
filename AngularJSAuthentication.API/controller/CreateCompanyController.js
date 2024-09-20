
(function () {
    'use strict';

    angular
        .module('app')
        .controller('CreateCompanyController', CreateCompanyController);

    CreateCompanyController.$inject = ['$scope', 'peoplesService', 'CityService', 'StateService', "$filter", "$http", "ngTableParams", '$modal', 'WarehouseService'];

    function CreateCompanyController($scope, peoplesService, CityService, StateService, $filter, $http, ngTableParams, $modal, WarehouseService) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        {
            $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));

            console.log("People Controller reached");
            //.................File Uploader method start..................    
            $(function () {
                $('input[name="daterange"]').daterangepicker({
                    timePicker: true,
                    timePickerIncrement: 5,
                    timePicker12Hour: true,

                    //locale: {
                    format: 'MM/DD/YYYY h:mm A'
                    //}
                });
            });
            $scope.currentPageStores = {};
            $scope.peoples = [];
            $http.get(serviceBase + 'api/CreateCompany').then(function (results) {

                $scope.peoples = results.data;

                $scope.callmethod();
            }, function (error) {
            });

            $scope.callmethod = function () {

                var init;
                $scope.stores = $scope.peoples;

                $scope.searchKeywords = "";
                $scope.filteredStores = [];
                $scope.row = "";


                $scope.numPerPageOpt = [30, 50, 100, 200];
                $scope.numPerPage = $scope.numPerPageOpt[1];
                $scope.currentPage = 1;
                $scope.currentPageStores = [];
                $scope.search(); $scope.select(1);
            }


            $scope.select = function (page) {
                var end, start; console.log("select"); console.log($scope.stores);
                start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage; $scope.currentPageStores = $scope.filteredStores.slice(start, end);
            };

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


            $scope.open = function () {
                console.log("Modal opened people");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myCompanyModal.html",
                        controller: "ModalInstanceCtrlCompany", resolve: { people: function () { return $scope.items } }
                    });
                modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
            };

            $scope.edit = function (item) {
                console.log("Edit Dialog called people");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myCompanyModalPut.html",
                        controller: "ModalInstanceCtrlCompany", resolve: { people: function () { return item } }
                    });
                modalInstance.result.then(function (selectedItem) {

                    $scope.peoples.push(selectedItem);
                    _.find($scope.peoples, function (people) {
                        if (people.id == selectedItem.id) {
                            people = selectedItem;
                        }
                    });
                    $scope.peoples = _.sortBy($scope.peoples, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
            };

            $scope.opendelete = function (data, $index) {
                console.log(data);
                console.log("Delete Dialog called for people");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myModaldeletecompany.html",
                        controller: "ModalInstanceCtrldeleteCompany", resolve: { people: function () { return data } }
                    });
                modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
            };
        }
       
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlCompany', ModalInstanceCtrlCompany);

    ModalInstanceCtrlCompany.$inject = ["$scope", '$http', '$modal', 'ngAuthSettings', "peoplesService", 'CityService', 'StateService', "$modalInstance", "people", 'WarehouseService', 'authService'];

    function ModalInstanceCtrlCompany($scope, $http, $modal, ngAuthSettings, peoplesService, CityService, StateService, $modalInstance, people, WarehouseService, authService) {

        console.log("People");
        $scope.PeopleData = {}; var alrt = {};
        if (people) {
            $scope.PeopleData = people;
            $scope.PeopleData.Permissions = "HQ Master login";

            console.log($scope.PeopleData);

        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.AddComp = function () {

            $scope.PeopleData.Permissions = "HQ Master login";
            authService.saveRegistrationpeople($scope.PeopleData).then(function (response) {
                console.log(response);
                $modalInstance.close(response);
                if (response.status == "ok") {
                    alrt.msg = "Record has been Save successfully";
                    $modal.open(
                        {
                            templateUrl: "PopUpModel.html",
                            controller: "PopUpController", resolve: { message: function () { return alrt } }
                        });
                    modalInstance.result.then(function (selectedItem) {
                    },
                        function () {
                            console.log("Cancel Condintion");
                        })
                }
                window.location.reload();


            });
        };
        $scope.PutComp = function (data) {
            $scope.PeopleData = {};

            if (people) {
                $scope.PeopleData = people;
                $scope.ok = function () { $modalInstance.close(); };
                $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
                    console.log("Update People");
                var url = serviceBase + "api/CreateCompany";
                var dataToPost = {
                    userId: $scope.PeopleData.userId,
                    CompanyId: $scope.PeopleData.CompanyId,
                    Email: $scope.PeopleData.Email,
                    Password: $scope.PeopleData.Password,
                    CompanyName: $scope.PeopleData.CompanyName,
                    Address: $scope.PeopleData.Address,
                    CompanyZip: $scope.PeopleData.CompanyZip,
                    CompanyPhone: $scope.PeopleData.CompanyPhone,
                    EmployeesCount: $scope.PeopleData.EmployeesCount,
                    Active: $scope.PeopleData.Active,
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
                            alrt.msg = "Record has been Update successfully";
                            $modal.open(
                                {
                                    templateUrl: "PopUpModel.html",
                                    controller: "PopUpController", resolve: { message: function () { return alrt } }
                                });
                            modalInstance.result.then(function (selectedItem) {
                                },
                                    function () {
                                        console.log("Cancel Condintion");
                                    })
                        }
                        window.location.reload();
                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })
            }
        }
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrldeleteCompany', ModalInstanceCtrldeleteCompany);

    ModalInstanceCtrldeleteCompany.$inject = ["$scope", '$http', '$modal', "$modalInstance", "peoplesService", 'ngAuthSettings', "people"];

    function ModalInstanceCtrldeleteCompany($scope, $http, $modal, $modalInstance, peoplesService, ngAuthSettings, people) {
        console.log("delete modal opened");
        var alrt = {};
        if (people) {
            $scope.PeopleData = people;
            console.log("found people");
            console.log(people);
            console.log($scope.PeopleData);

        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


        $scope.deletepeoples = function (dataToPost) {

            console.log("Delete people controller");


            peoplesService.deletepeoples(dataToPost).then(function (results) {
                console.log("Del");
                $modalInstance.close(dataToPost);
                alrt.msg = "Entry Deleted";
                $modal.open(
                    {
                        templateUrl: "PopUpModel.html",
                        controller: "PopUpController", resolve: { message: function () { return alrt } }
                    });
                modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
            }, function (error) {
                alert(error.data.message);
            });
        }
    }
})();