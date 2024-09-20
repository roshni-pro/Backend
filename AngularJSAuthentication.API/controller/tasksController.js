

(function () {
    'use strict';

    angular
        .module('app')
        .controller('tasksController', tasksController);

    tasksController.$inject = ['$scope', 'tasksService', 'projectsService', 'customerService', 'tasktypesService', 'peoplesService', "$filter", "$http", "ngTableParams", '$modal', '$log'];

    function tasksController($scope, tasksService, projectsService, customerService, tasktypesService, peoplesService, $filter, $http, ngTableParams, $modal, $log) {

        console.log("reached");

        $scope.currentPageStores = {};

        $scope.open = function () {
            console.log("Modal opened task");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myTaskModal.html",
                    controller: "ModalInstanceCtrlTask", resolve: { task: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {


                    $scope.currentPageStores.push(selectedItem);

                    $scope.tasks = _.sortBy($scope.tasks, 'Id').reverse();
                    $scope.selected = selectedItem;


                },
                    function () {
                        console.log("Cancel Condintion");
                        // $scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                        $log.info("Modal dismissed at: " + new Date());
                    });
        };


        $scope.edit = function (item) {
            console.log("Edit Dialog called task");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myTaskModalPut.html",
                    controller: "ModalInstanceCtrlTask", resolve: { task: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {

                    $scope.tasks.push(selectedItem);
                    _.find($scope.tasks, function (task) {
                        if (task.id == selectedItem.id) {
                            task = selectedItem;
                        }
                    });

                    $scope.tasks = _.sortBy($scope.tasks, 'Id').reverse();
                    $scope.selected = selectedItem;
                    //$scope.currentPageStores.push(selectedItem);

                    //$scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                    //$scope.selected = selectedItem;


                },
                    function () {
                        console.log("Cancel Condintion");
                        // $scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                        //  $log.info("Modal dismissed at: " + new Date)
                    })
        };

        $scope.opendelete = function (data) {
            console.log(data);
            console.log("Delete Dialog called for Task");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteTask.html",
                    controller: "ModalInstanceCtrldeletetask", resolve: { task: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {


                    //$scope.tasktypes.push(selectedItem);

                    //_.filter($scope.tasktypes, function (a) {

                    //    if (a.id == selectedItem.id) {

                    //        a.Name = selectedItem.Name;
                    //        a.Desc = selectedItem.Desc;
                    //    }

                    //});

                    //$scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                    //$scope.selected = selectedItem;


                },
                    function () {
                        console.log("Cancel Condintion");
                        // $scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                        //  $log.info("Modal dismissed at: " + new Date)
                    })
        };

        $scope.tasks = [];

        tasksService.gettasks().then(function (results) {

            $scope.tasks = results.data;
            $scope.callmethod();
        }, function (error) {
            //alert(error.data.message);
        });
        $scope.callmethod = function () {

            var init;
            return $scope.stores = $scope.tasks,

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
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlTask', ModalInstanceCtrlTask);

    ModalInstanceCtrlTask.$inject = ["$scope", '$http', 'ngAuthSettings', "tasksService", 'projectsService', 'customerService', 'tasktypesService', 'peoplesService', "$modalInstance", "task"];

    function ModalInstanceCtrlTask($scope, $http, ngAuthSettings, tasksService, projectsService, customerService, tasktypesService, peoplesService, $modalInstance, task) {
        console.log("Task");

        $scope.customers = [];

        customerService.getcustomers().then(function (results) {

            $scope.customers = results.data;
            //var CustomerName = $scope.customers.CustomerName;
            //console.log("CustomerName");
            //console.log(CustomerName);

        }, function (error) {
            //alert(error.data.message);
        });

        $scope.projects = [];

        projectsService.getprojects().then(function (results) {

            $scope.projects = results.data;


        }, function (error) {
            //alert(error.data.message);
        });

        $scope.tasktypes = [];

        tasktypesService.gettasktypes().then(function (results) {

            $scope.tasktypes = results.data;

        }, function (error) {
            //alert(error.data.message);
        });

        $scope.peoples = [];
        peoplesService.getpeoples().then(function (results) {
            $scope.peoples = results.data;
        }, function (error) {
            //alert(error.data.message);
        });

        $scope.TaskData = {

        };
        if (task) {
            console.log("Task if conditon");

            $scope.TaskData = task;

            console.log($scope.TaskData.TaskName);
            //console.log($scope.ProjectData.Description);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.AddTask = function (data) {

                //var CustomerName = $scope.uploadedfileName;
                ////var ImageUrl = serviceBase + $scope.uploadedfileName;

                //console.log(ImageUrl);
                //console.log("Image name in Insert function :" + $scope.uploadedfileName);
                //$scope.peoples.ImageUrl = ImageUrl;
                //console.log($scope.peoples.ImageUrl);

                console.log("Task");
                var url = serviceBase + "api/ProjectTask";
                var dataToPost = { TaskTypeId: $scope.TaskData.TaskTypeId, ProjectID: $scope.TaskData.ProjectID, CustomerId: $scope.TaskData.CustomerId, Name: $scope.TaskData.Name, Discription: $scope.TaskData.Discription, CreatedDate: $scope.TaskData.CreatedDate, UpdatedDate: $scope.TaskData.UpdatedDate, CreatedBy: $scope.TaskData.CreatedBy, UpdateBy: $scope.TaskData.UpdateBy, AllocatedHours: $scope.TaskData.AllocatedHours, Priority: $scope.TaskData.Priority, Assignee: $scope.TaskData.Assignee, PeopleID: $scope.TaskData.PeopleID, StartDate: $scope.TaskData.StartDate, EndDate: $scope.TaskData.EndDate, CustomerName: $scope.TaskData.Name, ProjectName: $scope.TaskData.ProjectName };
                console.log(dataToPost);

                $http.post(url, dataToPost)
                    .success(function (data) {

                        console.log("Error Gor Here");
                        console.log(data);

                        //var searchproject = _.where($scope.projects,{ ProjectID: data.ProjectID });
                        //console.log(searchproject[0].ProjectName);
                        if (data.id == 0) {

                            $scope.gotErrors = true;
                            if (data[0].exception == "Already") {
                                console.log("Got This User Already Exist");
                                $scope.AlreadyExist = true;
                            }

                        }
                        else {
                            //console.log(data);
                            //  console.log(data);
                            $modalInstance.close(data);
                        }

                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                        // return $scope.showInfoOnSubmit = !0, $scope.revert()
                    })
            };

        $scope.PutTask = function (data) {
            $scope.TaskData = {

            };
            if (task) {
                $scope.TaskData = task;
                console.log("found Puttt Task");
                console.log(task);
                console.log($scope.TaskData);
                //console.log($scope.Customer.name);
                //console.log($scope.Customer.description);
            }
            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

                console.log("Update Task");
            var url = serviceBase + "api/ProjectTask";
            var dataToPost = { TaskId: $scope.TaskData.TaskId, TaskTypeId: $scope.TaskData.TaskTypeId, ProjectID: $scope.TaskData.ProjectID, CustomerId: $scope.TaskData.CustomerId, Name: $scope.TaskData.Name, Discription: $scope.TaskData.Discription, CreatedDate: $scope.TaskData.CreatedDate, UpdatedDate: $scope.TaskData.UpdatedDate, CreatedBy: $scope.TaskData.CreatedBy, UpdateBy: $scope.TaskData.UpdateBy, AllocatedHours: $scope.TaskData.AllocatedHours, Priority: $scope.TaskData.Priority, Assignee: $scope.TaskData.Assignee, StartDate: $scope.TaskData.StartDate, EndDate: $scope.TaskData.EndDate, CustomerName: $scope.TaskData.Name, ProjectName: $scope.TaskData.ProjectName };
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
        .controller('ModalInstanceCtrldeletetask', ModalInstanceCtrldeletetask);

    ModalInstanceCtrldeletetask.$inject = ["$scope", '$http', "$modalInstance", "tasksService", 'projectsService', 'customerService', 'peoplesService', 'ngAuthSettings', "task"];

    function ModalInstanceCtrldeletetask($scope, $http, $modalInstance, tasksService, projectsService, customerService, peoplesService, ngAuthSettings, task) {
        console.log("delete modal opened");


        $scope.TaskData = {

        };
        if (task) {
            $scope.TaskData = task;
            console.log("found tasktype");
            console.log(task);
            console.log($scope.TaskData);
            //console.log($scope.Customer.name);
            //console.log($scope.Customer.description);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


            $scope.deletetasks = function (dataToPost, index) {
                console.log("Delete Task controller");
                //alert(Id);
                //Id = window.encodeURIComponent(Id);
                tasksService.deletetasks(dataToPost).then(function (results) {
                    console.log("Del");
                    $modalInstance.close(dataToPost);

                }, function (error) {
                    alert(error.data.message);
                });
            }
    }
})();


