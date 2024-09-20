﻿'use strict';
app.controller('MposDetailsController', ['$scope', "$filter", "$http", "ngTableParams", '$modal', 'localStorageService', '$routeParams', function ($scope, $filter, $http, ngTableParams, $modal, localStorageService, $routeParams) {
    $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
 
    $scope.upload = $routeParams.id;

    $scope.Getdata = function () {

        var url = serviceBase + "api/OnlineTransaction/GetmPosDetails?UploadId=" + $scope.upload;

        $http.get(url)
            .success(function (response) {

                $scope.paymentdetail = response;
            })
            .error(function (data) {
                console.log("Error Got Heere is ");
                console.log(data);
            });
    };
    $scope.Getdata();

    $scope.Comment = function (item) {
   
        console.log("Edit Dialog called city");
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "Mposcomment.html",
                controller: "ModalInstanceMposComment", resolve: { city: function () { return item } }
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

    $scope.selectmpos = function (data) {
        
        $scope.paymentdetail = [];
        var url = serviceBase + "api/OnlineTransaction/Onchangempos?UploadId=" + $scope.upload + "&status=" + data.status;
        $http.get(url)
            .success(function (data) {
                if (data.length == 0) {
                    alert("Not Found");
                }
                $scope.paymentdetail = data;
                console.log(data);

            });


    }
    //Color Code
    //var s_col = false;
    //var del = '';

    //var ss_col = '';

    //$scope.set_color = function (trade) {
        
    //    if (trade.paymentxnId == null) {
    //        s_col = true;
    //        return { background: "#e08d8d" }
    //    }
    //    else {
    //        s_col = false;
    //    }

    //};
    $scope.changeColor = function (trade) {
        if (trade.paymentxnId == null) {
            return { color: "#e31212" }
        }

    }


}]);
app.controller("ModalInstanceMposComment", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "city", 'FileUploader', function ($scope, $http, ngAuthSettings, $modalInstance, city, FileUploader) {
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
                    //$scope.AddTrack(dataToPost);
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