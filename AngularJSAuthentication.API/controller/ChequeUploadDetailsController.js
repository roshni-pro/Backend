'use strict';
app.controller('ChequeUploadDetailsController', ['$scope', "$filter", "$http", "ngTableParams", '$modal', 'localStorageService', '$routeParams', function ($scope, $filter, $http, ngTableParams, $modal, localStorageService, $routeParams) {
    $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
 debugger;
    $scope.upload = $routeParams.id;

   
    $scope.Getdata = function () {
        debugger;
        var url = serviceBase + "api/OnlineTransaction/GetChequeUploadDetails?UploadId=" + $scope.upload;
        debugger;
        $http.get(url)
            .success(function (response) {

                $scope.chequeUpload = response;
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
                templateUrl: "ChequeUploadCommit.html",
                controller: "ModalInstanceChequeUpload", resolve: { city: function () { return item } }
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

    $scope.selectCheque = function (data) {
        
        $scope.chequeUpload = [];
        var url = serviceBase + "api/OnlineTransaction/OnchangeChequeUpload?UploadId=" + $scope.upload + "&status=" + data.status;
        $http.get(url)
            .success(function (data) {
                if (data.length == 0) {
                    alert("Not Found");
                }
                $scope.chequeUpload = data;
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
app.controller("ModalInstanceChequeUpload", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "city", 'FileUploader', function ($scope, $http, ngAuthSettings, $modalInstance, city, FileUploader) {
    console.log("city");

    //User Tracking
    $scope.data = city;

    $scope.chequeUpload = {

    };


    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

        $scope.Active = function (data) {

            if (city) {
                $scope.chequeUpload = city;
                console.log("found Puttt City");
                console.log(city);
                console.log($scope.chequeUpload);
                console.log("selected City");

            }
            $scope.ok = function () { $modalInstance.close(); },
                $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

            console.log("PutCity");

        var url = serviceBase + "api/OnlineTransaction/StatusData";
        var dataToPost = {
            OrderId: $scope.chequeUpload.OrderId,
            GatewayTransId: $scope.chequeUpload.TXNID,
            IsSettled: data.IsSettled,
            SettleComments: data.Comment


        };

            //var url = serviceBase + "api/OnlineTransaction/EnterComment";
            //var dataToPost = {
            //    GatewayTransId: $scope.paymentdetail.transaction_id,
            //    SettleComments: data.SettleComments


            //};

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
