'use strict'
app.controller('IRNRegerateCotroller', ['$scope', 'OrderMasterService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal',
    function ($scope, OrderMasterService, $http, ngAuthSettings, $filter, ngTableParams, $modal) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        var warehouseids = UserRole.Warehouseids;//JSON.parse(localStorage.getItem('warehouseids'));
        $scope.UserRoleBackend = JSON.parse(localStorage.getItem('RolePerson'));

        console.log("orderMasterController start loading OrderDetailsService");
        $scope.currentPageStores = {};
        $scope.statuses = [];
        $scope.orders = [];
        $scope.filterdata = [];
        $scope.customers = [];
        $scope.selectedd = {};
        $scope.cities = [];
        $scope.city = [];
        $scope.statusname = {};
        $scope.PaymentType = {};

       

         
        $scope.srch = { Cityid: 0, orderId: 0, invoice_no: "", skcode: "", shopName: "", mobile: "", status: "", paymentMode: "", PaymentFrom: "", WarehouseId: 0, TimeLeft: null };

        var data = [];
        var url = serviceBase + "api/Warehouse/getSpecificCitiesforuser";             //Vinayak refer to show specific cities for login user  
        $http.get(url).success(function (results) {
            $scope.city = results;
            
        });

        OrderMasterService.getcitys().then(function (results) {
            $scope.cities = results.data;

        });




        $scope.cities = $scope.data;
        

        $scope.vmIRN = {
            rowsPerPage: 20,
            currentPage: 1,
            count: null,
            numberOfPages: null,
        };

        // new pagination 
        $scope.pageno = 1; //initialize page no to 1
        $scope.total_count = 0;

        $scope.itemsPerPage = 20;  //this could be a dynamic value from a drop down

        $scope.numPerPageOpt = [20, 30, 50, 100];  //dropdown options for no. of Items per page

        //$scope.onNumPerPageChange = function () {
        //    $scope.itemsPerPage = $scope.selected;
        //}
        $scope.selected = $scope.numPerPageOpt[0];  //for Html page dropdown
        $scope.warehouse = [];
        $scope.getWarehosues = function (CityId) {
            $scope.MultiWarehouseModel = [];
            var citystts = [];
            if ($scope.MultiCityModel != '') {
                _.each($scope.MultiCityModel, function (item) {
                    citystts.push(item.id);
                });
            }

            var url = serviceBase + "api/Warehouse/GetWarehouseCitiesOnOrder";             //Vinayak refer to show specific cities for login user             
            if (citystts != '') {
                $http.post(url, citystts).success(function (response) {

                    var assignWarehouses = [];
                    if (response) {
                        angular.forEach(response, function (value, key) {
                            if (warehouseids.indexOf(value.WarehouseId) !== -1) {
                                assignWarehouses.push(value);
                            }
                        });
                    }
                    $scope.warehouse = assignWarehouses;
                    if (assignWarehouses.length == 1 && warehouseids.indexOf(",") == -1) {
                        $scope.srch.WarehouseId = warehouseids;
                    }
                });
            }

            //var url = serviceBase + "api/Warehouse/GetWarehouseCityOnOrder?cityid=" + CityId;             //Vinayak refer to show specific cities for login user             

            //$http.get(url).success(function (response) {   

            //    var assignWarehouses = [];
            //    if (response) {
            //        angular.forEach(response, function (value, key) {
            //            if (warehouseids.indexOf(value.WarehouseId) !== -1) {
            //                assignWarehouses.push(value);
            //            }
            //        });
            //    }
            //    $scope.warehouse = assignWarehouses;
            //    if (assignWarehouses.length == 1 && warehouseids.indexOf(",") == -1) {
            //        $scope.srch.WarehouseId = warehouseids;
            //    }
            //});

        }

        
       
        $scope.getTemplate = function (data) {
            if (data.OrderId === $scope.selectedd.OrderId) {
                myfunc();
                return 'edit';
            }
            else return 'display';
        };
     
       
       
        $scope.customers = [];
        
         
        $scope.show = true;
        $scope.order = false;
         
        $scope.onNumPerPageChange = function () {
            $scope.vm.rowsPerPage = $scope.selected;
            $scope.AdvanceSearch();
        };
         
        // Multieare select city and warehouse code
        $scope.MultiCityModel = [];
        $scope.MultiCity = $scope.city;
        $scope.MultiCityModelsettings = {
            displayProp: 'CityName', idProp: 'Cityid',
            scrollableHeight: '450px',
            scrollableWidth: '550px',
            enableSearch: true,
            scrollable: true
        };

        $scope.MultiWarehouseModel = [];
        $scope.MultiWarehouse = $scope.warehouse;
        $scope.MultiWarehouseModelsettings = {
            displayProp: 'WarehouseName', idProp: 'WarehouseId',
            scrollableHeight: '450px',
            scrollableWidth: '550px',
            enableSearch: true,
            scrollable: true
        };

        $scope.maxSizeIRN = 5;     // Limit number for pagination display number.  
        $scope.totalCountIRN = 0;  // Total number of items in all pages. initialize as a zero  
        $scope.pageIndexIRN = 1;   // Current page number. First page is 1.--&gt;  
        $scope.pageSizeSelectedIRN = 20; // Maximum number of items per page.  

        ////// Create Functions for IRN 
        $scope.IRNDataSearch = function () {
            
         
            if ($scope.MultiCityModel == '' || $scope.MultiCityModel.length == 0) {
                alert("Please select atleast 1 City");
                return;
            }

            if ($scope.MultiWarehouseModel == '' || $scope.MultiWarehouseModel.length == 0) {
                alert("Please select atleast 1 Warehouse");
                return;
            }
            else {
                if (!$scope.Cityid) {
                    $scope.Cityid = 0;
                }
                if (!$scope.srch.orderId) {
                    $scope.srch.orderId = 0;
                }

            }
            $scope.orders = [];
            $scope.customers = [];
            //// time cunsume code  
            var stts = "";
            if ($scope.statusname.name && $scope.statusname.name != "Show All") {
                stts = $scope.statusname.name;
            }
            var citystts = [];
            if ($scope.MultiCityModel != '') {
                _.each($scope.MultiCityModel, function (item) {
                    citystts.push(item.id);
                });
            }

            var warehousestts = [];
            if ($scope.MultiWarehouseModel != '') {
                _.each($scope.MultiWarehouseModel, function (item) {
                    warehousestts.push(item.id);
                });
            }


            //var url = serviceBase + "api/SearchOrder?start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + stts + "&WarehouseId=" + $scope.srch.WarehouseId;
            var postData = {

                Cityids: citystts,
                WarehouseIds: warehousestts,
                OrderId: $scope.srch.orderId,
                ItemPerPage: 20,
                PageNo: $scope.vmIRN.currentPage,
            };

            $http.post(serviceBase + "api/IRNReGenerate/GetSearchOrderMaster", postData).success(function (data, status) {

                $scope.IRNcustomers = data.ordermaster;
                $scope.total_countIRN = data.total_count;
                $scope.vmIRN.count = data.total_count;
                $scope.tempuserIRN = data.ordermaster;
                $scope.vmIRN.numberOfPages = Math.ceil($scope.vmIRN.count / 20);
                console.log(data); //ajax request to fetch data into vm.data
            });
            // $scope.srch.WarehouseId = 0;



        }

        $scope.changePageIRN = function (pagenumber) {
            setTimeout(function () {
                $scope.vmIRN.currentPage = pagenumber;
                $scope.IRNDataSearch();
            }, 100);

        };

        $scope.ConvertB2C = function (orderid) {

            if (confirm("Are you sure?")) {
                var postData = {

                    OrderId: orderid,
                };

                $http.post(serviceBase + "api/IRNReGenerate/ConvertB2C", postData).success(function (data, status) {
                    console.log(data);
                    if (data) {
                        alert("Converted B2C");

                        $scope.IRNDataSearch();
                    }
                    else {
                        alert("Error: Order not converted to B2C");
                        console.log(data);
                    }//ajax request to fetch data into vm.data
                }).error(function (data) {

                    alert("Error: Order not converted to B2C");
                });
            }
        };
        ///////////////////////////////////
        $scope.show = function (IRNErrorData) {

            console.log("Modal opened IRNErrorData");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myShowModalPut.html",
                    controller: "showctrl", resolve: { IRNErrorData: function () { return IRNErrorData } }
                });
            modalInstance.result.then(function (selectedItem) {

            },
                function () {
                    console.log("Cancel Condintion");

                });
        };
        ///// Calling a api for Retry for IRN Generate
        $scope.RegenerateIRN = function () {
            debugger
            var Orderid = $scope.IRNaction.OrderId;

            var postData = {
                GenerationId: $scope.IRNaction.GenerationId,
                OrderId: Orderid,
            };
            if (confirm("Are you sure?")) {
                $http.post(serviceBase + "api/IRNReGenerate/RegenerateIRN", postData).success(function (data, status) {
                    console.log(data);
                    if (data) {
                        alert("IRN Re Generated");
                        $('#IRNActionModel').modal('hide');
                        $scope.IRNDataSearch();
                    }
                    else {
                        alert("IRN Not Re Generated");
                        console.log(data);
                    }//ajax request to fetch data into vm.data
                }).error(function (data) {

                    alert("error: ", data);
                });
            }
        }
        $scope.test = function (trade) {
            debugger
            $scope.IRNaction = trade;
            // alert('hi showctrl');
            $('#IRNActionModel').modal('show');
        }

        $scope.GetIRNNumber = function (irn) {
            debugger
            if (irn == undefined || irn == "" || irn == "0") {
                alert(" Please enter IRN Number ");
                return false;
            }
            var Orderid = $scope.IRNaction.OrderId;

            var postData = {
                GenerationId: $scope.IRNaction.GenerationId,
                OrderId: Orderid,
                irn: irn
            };

            $http.post(serviceBase + "api/IRNReGenerate/GettingEInvoicebyIRN", postData).success(function (data, status) {
                console.log(data);
                if (data) {
                    alert("IRN Generated");
                    $('#IRNActionModel').modal('hide');
                }
                else {
                    alert("IRN Not Generated");
                }
                console.log(data); //ajax request to fetch data into vm.data
            }).error(function (data) {

                alert("error: ", data);
            });
        }

        $scope.ExporttoExcelIRN = function (orderid) {
            if (orderid == undefined || orderid == "" || orderid == "0") {
                alert(" Please enter Order ID ");
                return false;
            }
            $http.get(serviceBase + 'api/IRNReGenerate/GenerateExcelIRN?orderid=' + orderid).then(function (results) {
                debugger;
                if (results.data != "")
                    window.open(results.data, '_blank');
                else
                    alert("File not created");

            });
        }

    }]);
 

(function () {
    'use strict';

    angular
        .module('app')
        .controller('showctrl', showctrl);

    showctrl.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "IRNErrorData", 'FileUploader'];

    function showctrl($scope, $http, ngAuthSettings, $modalInstance, IRNErrorData, FileUploader) {
        $scope.IRNDataSingle = IRNErrorData;
      //  $scope.errordata = JSON.parse(IRNErrorData.Error);

        try {
            $scope.errordata = JSON.parse(IRNErrorData.Error);
        } catch (e) {
            alert(IRNErrorData.Error);
        }

        $scope.cancel = function () {
             
            $modalInstance.dismiss('canceled');
        };
    }



})();