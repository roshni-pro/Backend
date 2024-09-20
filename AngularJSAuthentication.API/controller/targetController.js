

(function () {
    'use strict';

    angular
        .module('app')
        .controller('targetController', targetController);

    targetController.$inject = ['$scope'];

    function targetController($scope, $modal, $http) {
        $scope.News = [];

        $scope.open = function () {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myADDModal.html",
                    controller: "targetADDController", resolve: { object: function () { return $scope.News } }
                });
            modalInstance.result.then(function (selectedNews) {
                    $scope.currentPageStores.push(selectedNews);
                },
                    function () { })
        };

        $scope.edit = function (News) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myADDModal.html",
                    controller: "targetADDController", resolve: { object: function () { return News } }
                });
            modalInstance.result.then(function (selectedNews) {
                    $scope.NewsDb.push(selectedNews);
                    _.find($scope.NewsDb, function (NewsDb) {
                        if (NewsDb.id == selectedNews.id) {
                            NewsDb = selectedNews;
                        }
                    });
                    $scope.NewsDb = _.sortBy($scope.NewsDb, 'Id').reverse();
                    $scope.selected = selectedNews;
                },
                    function () { })
        };

        $scope.opendelete = function (data, $index) {
            console.log(data);
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "mydeletemodal.html",
                    controller: "DeleteController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedComplain) {
                if (selectedComplain != null) {
                    $scope.currentPageStores.splice($index, 1);
                }
            },
                function () {
                });
        };
        var url = serviceBase + "api/target";
        $http.get(url)
            .success(function (data) {
                if (data.length == 0)
                    alert("Not Found");
                else
                    $scope.News = data;
                console.log(data);
            });

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('targetADDController', targetADDController);

    targetADDController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object"];

    function targetADDController($scope, $http, ngAuthSettings, $modalInstance, object, ItemService) {

        $scope.saveData = { Id: 0 };

        if (object) {
            $scope.saveData = object;
        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.Add = function () {
                var url = serviceBase + "api/target";
                var dataToPost = {
                    Id: $scope.saveData.Id,
                    name: $scope.saveData.name,
                    value: $scope.saveData.value,
                    monthValue: $scope.saveData.monthValue
                };
                $http.post(url, dataToPost)
                    .success(function (data) {
                        if (data.id == 0) {
                            $scope.gotErrors = true;
                            if (data[0].exception == "Already") {
                                $scope.AlreadyExist = true;
                            }
                        }
                        else {
                            $modalInstance.close(data);
                            location.reload();
                        }
                    })
                    .error(function (data) {
                    })
            };
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('DeleteController', DeleteController);

    DeleteController.$inject = ["$scope", '$http', "$modalInstance", "NewsService", 'ngAuthSettings', "object"];

    function DeleteController($scope, $http, $modalInstance, NewsService, ngAuthSettings, object) {
        $scope.News = [];
        if (object) {
            $scope.saveData = object;
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.delete = function (dataToPost, $index) {
                //NewsService.deleteNews(dataToPost).then(function (results) {
                //    if (results.data == '"error"') {
                //        alert("News Cannot Be Deleted As It Is Associated With Some Category!");
                //        $modalInstance.close(null);
                //        return false;
                //    } else if (results.data == '"success"') {
                //        alert("News Deleted Successfully!");
                //        $modalInstance.close(dataToPost);
                //    }
                //});
            }
    }
})();


