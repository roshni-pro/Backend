app.controller("LevelDashboardController", ["$scope", '$http', 'ngAuthSettings', 'WarehouseService', 'CityService', "$modalInstance", 'FileUploader', "status", function ($scope, $http, ngAuthSettings, WarehouseService,CityService, $modalInstance, FileUploader, status) {
    var UserRole = JSON.parse(localStorage.getItem('RolePerson'));

    if (UserRole.role == 'WH Master login' && UserRole.CashManagment == 'True')
    {
        //choose city
        $scope.citys = [];
        CityService.getcitys().then(function (results) {
            $scope.citys = results.data;
        }, function (error) {
            });

        //choose hub

        $scope.getWarehouse = function (id)
        {
            $scope.warehouses = [];
            WarehouseService.warehousecitybased(id).then(function (results)
            {
                debugger;
                $scope.warehouses = results.data;

            }, function (error) {
            });

        }



        $scope.getLevelData = function ()
        {
            debugger;

        }
        

       
    }
    else {
        window.location = "#/404";
    }

}]);

