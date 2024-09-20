'use strict'
app.controller('RTGSOrdersPaymentCotroller', ['$scope', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal',
    function ($scope, $http, ngAuthSettings, $filter, ngTableParams, $modal) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        var warehouseids = UserRole.Warehouseids;//JSON.parse(localStorage.getItem('warehouseids'));
        $scope.UserRoleBackend = JSON.parse(localStorage.getItem('RolePerson'));

        console.log("RTGSOrdersPaymentCotroller start loading OrderDetailsService");
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


        // alert('RTGSOrdersPaymentCotroller');

        $scope.srch = { Cityid: 0, orderId: 0, invoice_no: "", skcode: "", shopName: "", mobile: "", status: "", paymentMode: "", PaymentFrom: "", WarehouseId: 0, TimeLeft: null };

        var data = [];
        var url = serviceBase + "api/Warehouse/getSpecificCitiesforuser";             //Vinayak refer to show specific cities for login user  
        $http.get(url).success(function (results) {
            $scope.city = results;

        });



        $scope.vmRTGS = {
            rowsPerPage: 20,
            currentPage: 1,
            count: null,
            numberOfPages: null,
        };

        // new pagination 
        $scope.pageno = 1; //initialize page no to 1
        $scope.total_count = 0;

        $scope.itemsPerPage = 20;  //this could be a dynamic value from a drop down
        $scope.itemsPerPageRTGS = 20;  //this could be a dynamic value from a drop down

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



        $scope.MultiWarehouseModel = [];
        $scope.MultiWarehouse = $scope.warehouse;
        $scope.MultiWarehouseModelsettings = {
            displayProp: 'WarehouseName', idProp: 'WarehouseId',
            scrollableHeight: '450px',
            scrollableWidth: '550px',
            enableSearch: true,
            scrollable: true
        };


        /////////////--------------- RTGS --------------/////////////////////////
        $scope.isedit = true;

        $scope.getAllWarehosues = function () {

            $scope.MultiWarehouseModel = [];

            var url = serviceBase + "api/Warehouse/GetAllWarehouse";             //Vinayak refer to show specific cities for login user             

            $http.get(url).success(function (response) {

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

        $scope.GetRTGSPayList = function () {

            var warehousestts = [];
            if ($scope.MultiWarehouseModel != '') {
                _.each($scope.MultiWarehouseModel, function (item) {
                    warehousestts.push(item.id);
                });
            }
            if ($scope.MultiWarehouseModel == '' || $scope.MultiWarehouseModel.length == 0) {
                alert("Please select atleast 1 Warehouse");
                return;
            }
            if ($scope.paymode == '' || $scope.paymode == undefined) {
                alert("Please select Payment Mode");
                return false;
            }
            if ($scope.RTGSstatus == '' || $scope.RTGSstatus == undefined) {
                alert("Please select Status");
                return false;
            }

            var postData = {
                WarehouseIds: warehousestts,
                RefNo: $scope.refno,
                Skcode: $scope.skcode,
                type: $scope.RTGSstatus,
                PaymentFrom: $scope.paymode,
                Take: 20,
                Skip: $scope.vmRTGS.currentPage,
                type: $scope.RTGSstatus,
            };

            $scope.rpttype = $scope.RTGSstatus;

            $http.post(serviceBase + "api/RTGSOrdersApprove/GetRTGSOrderList", postData).success(function (data, status) {
                console.log(data);
                if (data != null) {
                    $scope.RTGSPayList = data.ordermaster;
                    $scope.total_countRTGS = data.total_count;
                    $scope.vmRTGS.count = data.total_count;
                    $scope.tempuser = data.ordermaster;
                    $scope.vmRTGS.numberOfPages = Math.ceil($scope.vmIRN.count / 20);
                }
                else {
                    alert("No Data Found");
                }
                console.log(data); //ajax request to fetch data into vm.data
            }).error(function (data) {

                alert("error: ", data);
            });
        };
        //$scope.EditBtn = function (refno) {
        //    $scope.isedit = true;
        //    $scope.refEditvalue = refno;
        //};
        $scope.Cancelbtn = function () {
            $scope.NewRefNo = "";
            // $scope.isedit = false;
        };
        $scope.UpdateRefNo = function (Orderid,RefNo, NewRTGSNo) {
            debugger
             
            if (NewRTGSNo == undefined || NewRTGSNo == "") {
                alert("Please enter RTGS number");
                return false;
            }
            if ($scope.paymode == '' || $scope.paymode == undefined) {
                alert("Please select Payment Mode");
                return false;
            }
            if (confirm("Are you sure want to update ?")) {
                var data = {
                    NewRefNo: NewRTGSNo,
                    PayMode: $scope.paymode,
                    OrderId: Orderid,
                    RefNo: RefNo
                }

               
                $http.post(serviceBase + 'api/RTGSOrdersApprove/UpdateRTGSRefNo', data).success(function (response) {
                    if (response) {
                        alert(NewRTGSNo + " RTGS Number is Updated")
                        $scope.GetRTGSPayList();
                        $scope.NewRefNo = "";
                    }
                    else {
                        alert("Error: Reference number already exists!!")
                        $scope.isedit = false;
                        $scope.NewRefNo = "";
                        return false;
                    }
                });
            };
        };
        $scope.ApproveRTGS = function (refno) {

            if (confirm("Are you sure want to Approve ???")) {
                $http.get(serviceBase + 'api/RTGSOrdersApprove/ApproveRTGSNumber?RTGSNo=' + refno).success(function (response) {
                    if (response) {
                        alert(refno + " RGTS Number is Approved...")
                        $scope.GetRTGSPayList();
                    }
                    else {
                        alert('Error: Apporve is failed');
                        return false;
                    }
                });
            }

        };
        $scope.GetRTGSdtlsOrderidWise = function (Refno, paymentfrom) {

            $('#RTGSModel').modal('show');
            $http.get(serviceBase + 'api/RTGSOrdersApprove/GetRTGSOrderIdWise?Refno=' + Refno + '&type=' + $scope.RTGSstatus + '&PaymentFrom=' + paymentfrom).success(function (response) {
                $scope.OrderWiseList = response;
                $scope.Paygatwayid = Refno;
            });
        };


        $scope.changePageRTGS = function (pagenumber) {
            setTimeout(function () {
                $scope.vmRTGS.currentPage = pagenumber;
                $scope.GetRTGSPayList();
            }, 100);

        };

        $scope.ExporttoExcel = function () {
            var warehousestts = [];
            if ($scope.MultiWarehouseModel != '') {
                _.each($scope.MultiWarehouseModel, function (item) {
                    warehousestts.push(item.id);
                });
            }
            if ($scope.MultiWarehouseModel == '' || $scope.MultiWarehouseModel.length == 0) {
                alert("Please select atleast 1 Warehouse");
                return;
            }
            if ($scope.paymode == '' || $scope.paymode == undefined) {
                alert("Please select Payment Mode");
                return false;
            }
            if ($scope.RTGSstatus == '' || $scope.RTGSstatus == undefined) {
                alert("Please select Status");
                return false;
            }
            var postData = {
                WarehouseIds: warehousestts,
                RefNo: $scope.refno,
                Skcode: $scope.skcode,
                type: 0,
                PaymentFrom: $scope.paymode,
                Take: 20,
                Skip: $scope.vmRTGS.currentPage,
                type: $scope.RTGSstatus,
            };

            $http.post(serviceBase + "api/RTGSOrdersApprove/GenExcel", postData).success(function (results, status) {
                debugger;
                if (results != "")
                    window.open(results, '_blank');
                else
                    alert("File not created");

            });
        }

    }]);


