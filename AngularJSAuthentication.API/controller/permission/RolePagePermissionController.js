'use strict';
app.controller('RolePagePermissionController', ['$scope', "$http", 'ngAuthSettings', "ngTableParams", '$modal', 'RolePagePermissionService', function ($scope, $http, ngAuthSettings, ngTableParams, $modal, RolePagePermissionService) {
    $scope.rolePageList = [];
    $scope.roles = [];
    $scope.roleid = '';
    RolePagePermissionService.getRole().then(function (results) { 
      
        $scope.roles = results.data;
    }, function (error) {
    });


    $scope.getRolePage = function () {
   
        $scope.rolePageList = [];
        var url = serviceBase + "api/RolePagePermission/GetAllPagesForDropDown?Id=" + $scope.roleid;
        return $http.get(url).success(function (results) {            
            $scope.rolePageList = results;
            $scope.updateList(); 
            console.log(' $scope.rolePageList: ', $scope.rolePageList);
        });
    };

    $scope.onClickRolePage = function (rolePage) {
        $scope.rolePageList.forEach(function (item) {   
            if (rolePage != item) {
                item.IsShow = false;
            }
        });
        rolePage.IsShow = !rolePage.IsShow;
    }

    $scope.updateList = function () {
        if ($scope.rolePageList && $scope.rolePageList.length > 0) {
            $scope.rolePageList.forEach(function (item) {
                item.IsShow = false;
            });
        }
    }

    $scope.onCheckeUncheckParent = function (rolePage) {
        if (rolePage.ChildRolePagePermissionDcs && rolePage.ChildRolePagePermissionDcs.length > 0) {
            rolePage.ChildRolePagePermissionDcs.forEach(function (item) {
                item.IsChecked = rolePage.IsChecked;
            });
        }
    }

    $scope.onCheckeUncheckChild = function (rolePage, childPage) {
        var isAnyChecked = false;
        rolePage.ChildRolePagePermissionDcs.forEach(function (item) {
            if (item.IsChecked) {
                isAnyChecked = true;
            }
        });
        rolePage.IsChecked = isAnyChecked;
    };

    $scope.saveRole = function (rolePageList) {       
        var url = serviceBase + "api/RolePagePermission/SaveRolePageData?roleId=" + $scope.roleid;
        $http.post(url, rolePageList).success(function (results) {
            if (results) {
                alert('Role page permission update successfully.');
            }
            else {
                alert('Some error occurred during same Role page permission.');
            }
            });
    };

}]);



app.controller('PeoplePagePermissionController', ['$scope', "$http", 'ngAuthSettings', "ngTableParams", '$modal', 'peoplesService', 'WarehouseService', 'RolePagePermissionService', function ($scope, $http, ngAuthSettings, ngTableParams, $modal, peoplesService, WarehouseService, RolePagePermissionService) {
     
    $scope.agetWarehosues = function () {        
        WarehouseService.getwarehouse().then(function (results) {          
            $scope.warehouse = results.data;
            $scope.Warehouseid = $scope.warehouse[0].WarehouseId;           
        }, function (error) {
        });
    };
    $scope.agetWarehosues();

    $scope.roles = [];
    $scope.roleid = '';
    RolePagePermissionService.getRole().then(function (results) {       
        $scope.roles = results.data;
    }, function (error) {
    }); 
   
    $scope.rolePageList = [];
    $scope.Peoples = [];
    $scope.PeopleId = '';
    $scope.getPeoples = function () {
        
        var url = serviceBase + 'api/RolePagePermission/GetPeopleall';
        $http.get(url)
            .success(function (response) {               
                $scope.Peoples = response;
            });
    };
    $scope.getPeoples();


    $scope.GetWarehousePeople = function (WarehouseId) {       
        $scope.Peoples = [];
        $scope.rolePageList = [];
        if (WarehouseId) {
            var url = serviceBase + "api/RolePagePermission/GetWarehousePeople?WarehouseId=" + WarehouseId;
            return $http.get(url).success(function (results) {
                $scope.Peoples = results;
            });
        }
        else
            $scope.getPeoples();
    };


    $scope.getPeopledata = function (id) {      
        $scope.Peoples = [];
        $scope.rolePageList = [];
        if (id) {
            var url = serviceBase + "api/RolePagePermission/GetPeopleRoles?Id=" + id;
            $scope.Peoples = [];
            return $http.get(url).success(function (results) {
                $scope.Peoples = results;

            });
        }
        else
            $scope.GetWarehousePeople($scope.Warehouseid);
    };


    $scope.getRolePage = function () {      
        $scope.rolePageList = [];
        if ($scope.PeopleId) {
            var url = serviceBase + "api/RolePagePermission/GetAllPeoplePage?peopleId=" + $scope.PeopleId;
            return $http.get(url).success(function (results) {
                $scope.rolePageList = results;
                $scope.updateList();                
            });
        }
    };

    $scope.onClickRolePage = function (rolePage) {        
        $scope.rolePageList.forEach(function (item) {
            if (rolePage != item) {
                item.IsShow = false;
            }
        });
        rolePage.IsShow = !rolePage.IsShow;
    }

    $scope.updateList = function () {
        if ($scope.rolePageList && $scope.rolePageList.length > 0) {
            $scope.rolePageList.forEach(function (item) {
                item.IsShow = false;
            });
        }
    }

    $scope.onCheckeUncheckParent = function (rolePage) {        
        if (rolePage.OverrideRolePagePermissionDcs && rolePage.OverrideRolePagePermissionDcs.length > 0) {
            rolePage.OverrideRolePagePermissionDcs.forEach(function (item) {
                item.IsChecked = rolePage.IsChecked;
            });
        }
    }

    $scope.onCheckeUncheckChild = function (rolePage, childPage) {        
        var isAnyChecked = false;
        rolePage.OverrideRolePagePermissionDcs.forEach(function (item) {
            if (item.IsChecked) {
                isAnyChecked = true;
            }
        });
        rolePage.IsChecked = isAnyChecked;
    };

    $scope.saveRole = function (rolePageList) {
        if (rolePageList && rolePageList.length > 0) {
            var url = serviceBase + "api/RolePagePermission/SavePeoplePageData?peopleId=" + $scope.PeopleId;
            $http.post(url, rolePageList).success(function (results) {
                if (results) {
                    alert('People page permission update successfully.');
                }
                else {
                    alert('Some error occurred during same People page permission.');
                }
            });
        }
        else {
            alert('Please select atleast one item to save.');
        }
    };

}]);