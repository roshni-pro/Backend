

(function () {
    'use strict';

    angular
        .module('app')
        .controller('SkillCtrl', SkillCtrl);

    SkillCtrl.$inject = ['$scope', "$http", '$modal', "$filter"];

    function SkillCtrl($scope, $http, $modal, $filter) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log("SkillCtrl reached");
        var User = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.data = null;
        $scope.currentPageStores = {};

        $scope.open = function () {

            console.log("Modal opened Skill");
            var modalInstance;
            modalInstance = $modal.open(
                {

                    templateUrl: "Skilladd.html",
                    controller: "ModalInstanceSkillCtrlDocument", resolve: { skill: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {

                    $scope.currentPageStores.push(selectedItem);
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
        };

        $scope.DocumentGet = function () {


            var url = serviceBase + 'api/Skill/GetSkill';
            $http.get(url)
                .success(function (data) {

                    $scope.getskill = data;
                    console.log("$scope.getskill", $scope.getskill);
                    $scope.callmethod();
                });
        }
        $scope.DocumentGet();


        $scope.edit = function (item) {
            console.log("Edit Dialog called ");
            var modalInstance;

            modalInstance = $modal.open(
                {
                    templateUrl: "mySkillModalPut.html",
                    controller: "ModalInstanceSkillCtrlDocument", resolve: { skill: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {

                    $scope.skills.push(selectedItem);
                    _.find($scope.skills, function (skill) {
                        if (skill.id === selectedItem.id) {
                            skill = selectedItem;
                        }
                    });

                    $scope.skills = _.sortBy($scope.skill, 'Id').reverse();
                    $scope.selected = selectedItem;
                    $scope.dBi();

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

                    templateUrl: "myModaldeleteSkill.html",
                    controller: "ModalInstanceCtrldeleteSkill", resolve: { skill: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                },
                    function () {
                        console.log("Cancel Condintion");

                    })
        };
        $scope.DocumentGet();
        //return $http.get(serviceBase + 'api/Skill/GetSkillid').then(function (results) {
        //    return results;
        //});
        $scope.dBi = function (data) {


            var url = serviceBase + 'api/Skill/GetSkillid?id=' + data.SkillId;
            $http.get(url).success(function (data) {

                $scope.getskillid = data;
                console.log("$scope.getskill", $scope.getskillid);

            });
        }

        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.skills;

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
            $scope.select(1), $scope.currentPage = 1;
        }

        $scope.onOrderChange = function () {
            console.log("onOrderChange"); console.log($scope.stores);
            $scope.select(1), $scope.currentPage = 1;
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
        .controller('ModalInstanceSkillCtrlDocument', ModalInstanceSkillCtrlDocument);

    ModalInstanceSkillCtrlDocument.$inject = ["$scope", '$http', '$modal', 'ngAuthSettings', "skill", "$modalInstance", 'WarehouseService', 'authService'];

    function ModalInstanceSkillCtrlDocument($scope, $http, $modal, ngAuthSettings, skill, $modalInstance, WarehouseService, authService) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log("Skill");




        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.Add = function (data) {

            var url = serviceBase + "api/Skill/Post";

            var dataToPost = {
                Name: data.Name



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


            var url = serviceBase + 'api/Skill/GetSkillid?id=' + data.SkillId;
            $http.get(url).success(function (data) {

                $scope.getskillid = data;
                console.log("$scope.getskill", $scope.getskillid);

            });
        }
        //$scope.DocumentGet();

        $scope.PutSkill = function (data) {

            $scope.PutSkill = {

            };
            if (skill) {
                $scope.data = skill;
                console.log("found Puttt skill");
                console.log(skill);
                console.log($scope.PutSkill);

            }

            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            console.log("Update ");

            //var FileUrl = serviceBase + "../../UploadedFiles/" + $scope.uploadedfileName;
            //console.log(FileUrl);
            //console.log("Image name in Insert function :" + $scope.uploadedfileName);
            //$scope.AssetsCategoryData.FileUrl = FileUrl;
            //console.log($scope.AssetsCategoryData.FileUrl);

            var url = serviceBase + "api/Skill/Put";
            var dataToPost = {
                SkillId: $scope.data.SkillId,
                Name: data.Name
            };

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
        .controller('ModalInstanceCtrldeleteSkill', ModalInstanceCtrldeleteSkill);

    ModalInstanceCtrldeleteSkill.$inject = ["$scope", '$http', "$modalInstance", 'ngAuthSettings', "skill"];

    function ModalInstanceCtrldeleteSkill($scope, $http, $modalInstance, ngAuthSettings, skill) {
        console.log("delete modal opened");



        $scope.skill = [];

        if (skill) {
            $scope.data = skill;
            console.log("found skill");
            console.log(skill);
            console.log($scope.data);

        }
        $scope.ok = function () { $modalInstance.close(); };

        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.delete = function (data, $index) {

                console.log("Delete  skill controller");

                return $http.delete(serviceBase + 'api/Skill/DELETEById?id=' + data.SkillId).then(function (results) {
                    $modalInstance.close(data);
                    window.location.reload();
                    return results;

                }, function (error) {
                    alert(error.data.message);
                });
            }   //}    $modalInstance.close(dataToPost);

    }
})();








