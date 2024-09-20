

(function () {
    'use strict';

    angular
        .module('app')
        .controller('Departmentctrl', Departmentctrl);

    Departmentctrl.$inject = ['$scope', "$http", '$modal', "$filter"];

    function Departmentctrl($scope, $http, $modal, $filter) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log("Departmentctrl reached");
        var User = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.data = null;
        $scope.currentPageStores = {};

        $scope.open = function () {

            console.log("Modal opened Skill");
            var modalInstance;
            modalInstance = $modal.open(
                {

                    templateUrl: "Departmentadd.html",
                    controller: "ModalInstanceCtrlDocument", resolve: { department: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.currentPageStores.push(selectedItem)
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };

        $scope.departmentGet = function () {


            var url = serviceBase + 'api/Department/GetDepartment';
            $http.get(url)
                .success(function (data) {

                    $scope.getdepartment = data;
                    console.log("$scope.getdepartment", $scope.getdepartment);
                    $scope.callmethod();
                });
        }
        $scope.departmentGet();


        $scope.edit = function (item) {
            console.log("Edit Dialog called ");
            var modalInstance;

            modalInstance = $modal.open(
                {
                    templateUrl: "myDepartmentModalPut.html",
                    controller: "ModalInstanceCtrlDocument", resolve: { department: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.departments.push(selectedItem);
                _.find($scope.departments, function (department) {
                    if (department.id === selectedItem.id) {
                        department = selectedItem;
                    }
                });

                $scope.departments = _.sortBy($scope.department, 'Id').reverse();
                $scope.selected = selectedItem;

            },
                function () {
                    console.log("Cancel Condintion");

                })
        };


        $scope.opendelete = function (data, $index) {
            console.log(data);
            console.log("Delete Dialog called for skill");



            var modalInstance;
            modalInstance = $modal.open(
                {

                    templateUrl: "myModaldeleteDepartment.html",
                    controller: "ModalInstanceCtrldeleteDepartment", resolve: { department: function () { return data } }
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
            $scope.stores = $scope.departments;

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
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlDocument', ModalInstanceCtrlDocument);

    ModalInstanceCtrlDocument.$inject = ["$scope", '$http', '$modal', 'ngAuthSettings', "department", "$modalInstance", 'WarehouseService', 'authService'];

    function ModalInstanceCtrlDocument($scope, $http, $modal, ngAuthSettings, department, $modalInstance, WarehouseService, authService) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log("department");


        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.Add = function (data) {

            var url = serviceBase + "api/Department/Post";

            var dataToPost = {

                DepName: data.DepName,
                Description: data.Description



            };
            alert('Save Data Successfully...')

            window.location.reload();

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


        }

        $scope.edit = function (data) {


            var url = serviceBase + 'api/Department/GetById?id=' + data.DepId;
            $http.get(url).success(function (data) {

                $scope.getdepartmentid = data;
                console.log("$scope.getdepartmentid", $scope.getdepartmentid);

            });
        }
        //$scope.DocumentGet();

        $scope.putDepartment = function (data) {

            $scope.putDepartment = {

            };
            if (department) {
                $scope.data = department;
                console.log("found Puttt skill");
                console.log(department);
                console.log($scope.putDepartment);

            }

            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            console.log("Update ");

            //var FileUrl = serviceBase + "../../UploadedFiles/" + $scope.uploadedfileName;
            //console.log(FileUrl);
            //console.log("Image name in Insert function :" + $scope.uploadedfileName);
            //$scope.AssetsCategoryData.FileUrl = FileUrl;
            //console.log($scope.AssetsCategoryData.FileUrl);

            var url = serviceBase + "api/Department/Put";
            var dataToPost = {
                DepId: $scope.data.DepId,
                DepName: data.DepName,
                Description: data.Description,
            };
            alert('Update Successfully...')

            window.location.reload();

            //var dataToPost = { SurveyId: $scope.SurveyData.SurveyId, SurveyCategoryName: $scope.SurveyData.SurveyCategoryName, Discription: $scope.SurveyData.Discription, CreatedDate: $scope.SurveyData.CreatedDate, UpdatedDate: $scope.SurveyData.UpdatedDate, CreatedBy: $scope.SurveyData.CreatedBy, UpdateBy: $scope.SurveyData.UpdateBy };
            console.log(dataToPost);


            $http.put(url, dataToPost)
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
                        $modalInstance.close(data);
                        window.location.reload();
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


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrldeleteDepartment', ModalInstanceCtrldeleteDepartment);

    ModalInstanceCtrldeleteDepartment.$inject = ["$scope", '$http', "$modalInstance", 'ngAuthSettings', "department"];

    function ModalInstanceCtrldeleteDepartment($scope, $http, $modalInstance, ngAuthSettings, department) {
        console.log("delete modal opened");

        $scope.department = [];

        if (department) {
            $scope.data = department;
            console.log("found skill");
            console.log(department);
            console.log($scope.data);

        }
        $scope.ok = function () { $modalInstance.close(); };

        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.delete = function (data, $index) {

            console.log("Delete  department controller");

            return $http.delete(serviceBase + 'api/department/DELETEById?id=' + data.DepId).then(function (results) {
                $modalInstance.close(data);
                window.location.reload();
                return results;

            }, function (error) {
                alert(error.data.message);
            });
        }   //}    $modalInstance.close(dataToPost);

    }
})();




