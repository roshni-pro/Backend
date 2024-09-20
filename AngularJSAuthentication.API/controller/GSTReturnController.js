'use strict'
app.controller('GSTReturnController', ['$scope', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal',
    function ($scope, $http, ngAuthSettings, $filter, ngTableParams, $modal) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        //var warehouseids = UserRole.Warehouseids;//JSON.parse(localStorage.getItem('warehouseids'));
        $scope.UserRoleBackend = JSON.parse(localStorage.getItem('RolePerson'));


        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });
        });

        $scope.exportData = function (value) {

          
            if ($('#dat').val() == null || $('#dat').val() == "") {
                $('input[name=daterangepicker_start]').val("");
                $('input[name=daterangepicker_end]').val("");
            }
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();
            if (start != null && start != "") {
                $scope.POM = [];
                var url = serviceBase + "api/Report/GSTReport";
                var dataToPost = {
                    From: start,
                    TO: end,
                    Reporttype: value
                };
                $http.post(url, dataToPost).success(function (data, status) {
                    debugger;


                  /*  if (status == "0")
                    {
                       alert("data is not exists in these dates")
                    }*/

console.log(data);
                    if (data != "")
                        window.open(data, '_Self');
                    else
                        alert("Data is not Exists in these date Range");
                }).error(function (data) {
                    debugger;
                    alert("error: ", data);
                });
                 
            }
            else {
                alert('Please select Date parameter');
            }
            }

    

                }]);
