//'use strict';
//app.controller('projectsTaskController', ['$scope', 'projectsTasksService', 'projectsService', 'customerService', "$filter", "$http", "ngTableParams", '$modal', function ($scope, projectsTasksService, projectsService, customerService, $filter, $http, ngTableParams, $modal) {

//    console.log("reached");

//    $scope.currentPageStores = {};

//    $scope.open = function () {
//        console.log("Modal opened");
//        var modalInstance;
//        modalInstance = $modal.open(
//            {
//                templateUrl: "myProjetsTaskModal.html",
//                controller: "ModalInstanceCtrlProjectsTask", resolve: { project: function () { return $scope.items } }
//            }), modalInstance.result.then(function (selectedItem) {
//                $scope.currentPageStores.push(selectedItem);
//                //$scope.currentPageStores = _.sortBy($scope.currentPageStores, 'ProjectID').reverse();
//                //$scope.selected = selectedItem;
//            },
//            function () {
//                console.log("Cancel Condintion");
//                // $scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
//                $log.info("Modal dismissed at: " + new Date)
//            })
//    };


//    $scope.edit = function (item) {
//        console.log("Open Dialog called");
//        var modalInstance;
//        modalInstance = $modal.open(
//            {
//                templateUrl: "myProjetsTaskModalPut.html",
//                controller: "ModalInstanceCtrlProjectsTask", resolve: { project: function () { return item } }
//            }), modalInstance.result.then(function (selectedItem) {

//                $scope.projects.push(selectedItem);
//                _.find($scope.projects, function (project) {
//                    if (project.id == selectedItem.id) {
//                        project = selectedItem;
//                    }
//                });

//                $scope.projects = _.sortBy($scope.projects, 'ProjectId').reverse();
//                $scope.selected = selectedItem;
//                //$scope.customers.push(selectedItem);

//                //$scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
//                //$scope.selected = selectedItem;


//            },
//            function () {
//                console.log("Cancel Condintion");
//                // $scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
//                //  $log.info("Modal dismissed at: " + new Date)
//            })
//    };

//    $scope.opendelete = function (data) {
//        console.log(data);
//        console.log("Open Dialog called");
//        var modalInstance;
//        modalInstance = $modal.open(
//            {
//                templateUrl: "myTaskTypeModaldeleteproject.html",
//                controller: "ModalInstanceCtrldeleteproject", resolve: { project: function () { return data } }
//            }), modalInstance.result.then(function (selectedItem) {


//                //$scope.tasktypes.push(selectedItem);

//                //_.filter($scope.tasktypes, function (a) {

//                //    if (a.id == selectedItem.id) {

//                //        a.Name = selectedItem.Name;
//                //        a.Desc = selectedItem.Desc;
//                //    }

//                //});

//                //$scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
//                //$scope.selected = selectedItem;


//            },
//            function () {
//                console.log("Cancel Condintion");
//                // $scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
//                //  $log.info("Modal dismissed at: " + new Date)
//            })
//    };

//    $scope.projects = [];

//    projectsService.getprojects().then(function (results) {

//        $scope.projects = results.data;
//        $scope.callmethod();
//    }, function (error) {
//        //alert(error.data.message);
//    });
//    $scope.callmethod = function () {

//        var init;
//        return $scope.stores = $scope.projects,

//            $scope.searchKeywords = "",
//            $scope.filteredStores = [],
//            $scope.row = "",

//            $scope.select = function (page) {
//                var end, start; console.log("select"); console.log($scope.stores);
//                return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
//            },

//            $scope.onFilterChange = function () {
//                console.log("onFilterChange"); console.log($scope.stores);
//                return $scope.select(1), $scope.currentPage = 1, $scope.row = ""
//            },

//            $scope.onNumPerPageChange = function () {
//                console.log("onNumPerPageChange"); console.log($scope.stores);
//                return $scope.select(1), $scope.currentPage = 1
//            },

//            $scope.onOrderChange = function () {
//                console.log("onOrderChange"); console.log($scope.stores);
//                return $scope.select(1), $scope.currentPage = 1
//            },

//            $scope.search = function () {
//                console.log("search");
//                console.log($scope.stores);
//                console.log($scope.searchKeywords);

//                return $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange()
//            },

//            $scope.order = function (rowName) {
//                console.log("order"); console.log($scope.stores);
//                return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0
//            },

//            $scope.numPerPageOpt = [3, 5, 10, 20],
//            $scope.numPerPage = $scope.numPerPageOpt[2],
//            $scope.currentPage = 1,
//            $scope.currentPageStores = [],
//            (init = function () {
//                return $scope.search(), $scope.select($scope.currentPage)
//            })

//        ()


//    }
//}]);

//app.controller("ModalInstanceCtrlProjectsTask", ["$scope", '$http', 'ngAuthSettings', "projectsService", 'customerService', "$modalInstance", "project", function ($scope, $http, ngAuthSettings, projectsService, customerService, $modalInstance, project) {
//    console.log("Project");
//    $scope.customers = [];

//    customerService.getcustomers().then(function (results) {

//        $scope.customers = results.data;
     
//    }, function (error) {
//        //alert(error.data.message);
//    });
//    $scope.ProjectData = {

//    };
//    if (project) {
//        console.log("Project if conditon");

//        $scope.ProjectData = project;

//        console.log($scope.ProjectData.ProjectName);
//        //console.log($scope.ProjectData.Description);
//    }
//    $scope.ok = function () { $modalInstance.close(); },
//    $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

//     $scope.AddProject = function (data) {


//         console.log("Project");
//         var url = serviceBase + "api/Projects";
//         var dataToPost = { Client: $scope.ProjectData.Client, ProjectName: $scope.ProjectData.ProjectName, Discription: $scope.ProjectData.Discription, CreatedDate: $scope.ProjectData.CreatedDate, UpdatedDate: $scope.ProjectData.UpdatedDate, CreatedBy: $scope.ProjectData.CreatedDate, UpdateBy: $scope.ProjectData.UpdateBy };
//         console.log(dataToPost);

//         $http.post(url, dataToPost)
//         .success(function (data) {

//             console.log("Error Gor Here");
//             console.log(data);
//             if (data.id == 0) {

//                 $scope.gotErrors = true;
//                 if (data[0].exception == "Already") {
//                     console.log("Got This User Already Exist");
//                     $scope.AlreadyExist = true;
//                 }

//             }
//             else {
//                 //console.log(data);
//                 //  console.log(data);
//                 $modalInstance.close(data);
//             }

//         })
//          .error(function (data) {
//              console.log("Error Got Heere is ");
//              console.log(data);
//              // return $scope.showInfoOnSubmit = !0, $scope.revert()
//          })
//     };

//    $scope.PutProject = function (data) {
//        $scope.ProjectData = {

//        };
//        if (project) {
//            $scope.ProjectData = project;
//            console.log("found Puttt Project");
//            console.log(project);
//            console.log($scope.ProjectData);
//            //console.log($scope.Customer.name);
//            //console.log($scope.Customer.description);
//        }
//        $scope.ok = function () { $modalInstance.close(); },
//    $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

//        console.log("Update Project");
//        var url = serviceBase + "api/projects";
//        var dataToPost = { Client: $scope.ProjectData.Client, ProjectID: $scope.ProjectData.ProjectID, ProjectName: $scope.ProjectData.ProjectName, Discription: $scope.ProjectData.Discription, CreatedDate: $scope.ProjectData.CreatedDate, UpdatedDate: $scope.ProjectData.UpdatedDate, CreatedBy: $scope.ProjectData.CreatedBy, UpdateBy: $scope.ProjectData.UpdateBy };
//        console.log(dataToPost);


//        $http.put(url, dataToPost)
//        .success(function (data) {

//            console.log("Error Gor Here");
//            console.log(data);
//            if (data.id == 0) {

//                $scope.gotErrors = true;
//                if (data[0].exception == "Already") {
//                    console.log("Got This User Already Exist");
//                    $scope.AlreadyExist = true;
//                }

//            }
//            else {                
//                $modalInstance.close(data);
//            }

//        })
//         .error(function (data) {
//             console.log("Error Got Heere is ");
//             console.log(data);

//             // return $scope.showInfoOnSubmit = !0, $scope.revert()
//         })

//    };

//}])

//app.controller("ModalInstanceCtrldeleteproject", ["$scope", '$http', "$modalInstance", "projectsService", 'customerService', 'ngAuthSettings', "project", function ($scope, $http, $modalInstance, projectsService,customerService, ngAuthSettings, project) {
//    console.log("delete modal opened");
    
//    $scope.customers = [];

//    customerService.getcustomers().then(function (results) {

//        $scope.customers = results.data;
      
//    }, function (error) {
//        //alert(error.data.message);
//    });
//    $scope.ProjectData = {

//    };
//    if (project) {
//        $scope.ProjectData = project;
//        console.log("found tasktype");
//        console.log(project);
//        console.log($scope.ProjectData);
//        //console.log($scope.Customer.name);
//        //console.log($scope.Customer.description);
//    }
//    $scope.ok = function () { $modalInstance.close(); },
//    $scope.cancel = function () { $modalInstance.dismiss('canceled'); }, 


//    $scope.deleteprojects = function (dataToPost, $index) {
        
//        console.log("Delete Project controller");
//        //alert(Id);
//        //Id = window.encodeURIComponent(Id);

//        projectsService.deleteprojects(dataToPost).then(function (results) {
//            console.log("Del");

//            console.log("index of item " + $index);
//            console.log($scope.customers.length);
//            //$scope.data.splice($index, 1);
//            //console.log($scope.customers.length);

//            $scope.customers.splice($index, 1);
//            $modalInstance.close(dataToPost);

//        }, function (error) {
//            alert(error.data.message);
//        });
//    }  

//}])