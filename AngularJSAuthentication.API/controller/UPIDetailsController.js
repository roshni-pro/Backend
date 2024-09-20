'use strict';
app.controller('UPIDetailsController', ['$scope', "$filter", "$http", "ngTableParams", '$modal', 'localStorageService', '$routeParams', function ($scope, $filter, $http, ngTableParams, $modal, localStorageService, $routeParams) {
    $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
    debugger;
    $scope.upload = $routeParams.id;
    debugger;
    $scope.PRC = {
        rowsPerPage: 20,
        currentPage: 1,
        count: null,
        numberOfPages: null,
    };

    $scope.pageno = 1; //initialize page no to 1
    $scope.total_count = 0;

    $scope.itemsPerPage = 20;  //this could be a dynamic value from a drop down

    $scope.numPerPageOpt = [20, 30, 90, 100];  //dropdown options for no. of Items per page

    $scope.selected = $scope.numPerPageOpt[0];
    $scope.onNumPerPageChanges = function () {

        $scope.itemsPerPage = $scope.selected;
        $scope.Getdata($scope.pageno);
    };
    

    $scope.Getdata = function (pageno) {
    var url = serviceBase + "api/OnlineTransaction/GetUPIDetails?UploadId=" + $scope.upload + "&Skip=" + pageno + " &take=" + $scope.itemsPerPage;

        $http.get(url)
            .success(function (response) {

                $scope.paymentdetail = response.list;
                $scope.total_count = response.totalCount;
            })
            .error(function (data) {
                console.log("Error Got Heere is ");
                console.log(data);
            });
    };
    //$scope.Getdata();
    $scope.onNumPerPageChanges();

    $scope.Comment = function (item) {

        console.log("Edit Dialog called city");
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "UPIcomment.html",
                controller: "ModalInstanceUPIComment", resolve: { city: function () { return item } }
            }), modalInstance.result.then(function (selectedItem) {

                $scope.city.push(selectedItem);
                _.find($scope.city, function (city) {
                    if (city.id == selectedItem.id) {
                        city = selectedItem;
                    }
                });

                $scope.city = _.sortBy($scope.city, 'Id').reverse();
                $scope.selected = selectedItem;

            },
                function () {
                    console.log("Cancel Condintion");

                })
    };
    $scope.newstatus = [
        { stvalue: true, text: "Verfied" },
        { stvalue: false, text: "Unverified" }
    ];

    $scope.selectUPI = function (data) {

        $scope.paymentdetail = [];
        var url = serviceBase + "api/OnlineTransaction/OnchangeUPI?UploadId=" + $scope.upload + "&status=" + data.status;
        $http.get(url)
            .success(function (data) {
                if (data.length == 0) {
                    alert("Not Found");
                }
                $scope.paymentdetail = data;
                console.log(data);

            });


    }
    $scope.changeColor = function (trade) {
        if (trade.paymentxnId == null) {
            return { color: "#e31212" }
        }

    }

}]);
app.controller("ModalInstanceUPIComment", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "city", 'FileUploader', function ($scope, $http, ngAuthSettings, $modalInstance, city, FileUploader) {
    console.log("city");

    //User Tracking
    $scope.data = city;

    $scope.paymentdetail = {

    };


    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

        $scope.Active = function (data) {

            if (city) {
                $scope.paymentdetail = city;
                console.log("found Puttt City");
                console.log(city);
                console.log($scope.paymentdetail);
                console.log("selected City");

            }
            $scope.ok = function () { $modalInstance.close(); },
                $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

                console.log("PutCity");

            var url = serviceBase + "api/OnlineTransaction/StatusData";
            var dataToPost = {
                OrderId: $scope.paymentdetail.OrderId,
                GatewayTransId: $scope.paymentdetail.TXNID,
                IsSettled: data.IsSettled,
                SettleComments: data.Comment


            };


            console.log(dataToPost);

            $http.put(url, dataToPost)
                .success(function (data) {
                    console.log("Act");
                    console.log(data);
                    $modalInstance.close(data);
                    window.location.reload();
                    console.log("save data");


                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);

                })

        };

}])
