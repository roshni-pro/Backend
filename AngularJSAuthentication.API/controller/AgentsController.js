

(function () {
    'use strict';

    angular
        .module('app')
        .controller('AgentsController', AgentsController);

    AgentsController.$inject = ['$scope', 'peoplesService', 'CityService', 'StateService', "$filter", "$http", "ngTableParams", '$modal', 'WarehouseService'];

    function AgentsController($scope, peoplesService, CityService, StateService, $filter, $http, ngTableParams, $modal, WarehouseService) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log("People Controller reached");
        //.................File Uploader method start..................    
        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,

                //locale: {
                format: 'MM/DD/YYYY h:mm A'
                //}
            });
        });
        $scope.citys = [];


        CityService.getcitys().then(function (results) {

            $scope.citys = results.data;
        }, function (error) {
        });
        $scope.warehouse = [];
        WarehouseService.getwarehouse().then(function (results) {
            $scope.warehouse = results.data;

        }, function (error) {
        });
        $scope.currentPageStores = {};

        $scope.View = function (item) {

            console.log("Edit Dialog called people");
            $scope.peopleid = item.PeopleID;
            //User Tracking
            console.log("Tracking Code");
            $scope.AddTrack = function () {

                var url = serviceBase + "api/trackuser?action=View(Record)&item=AgentCode:" + item.AgentCode;
                $http.post(url).success(function (results) { });
            }
            $scope.AddTrack();
            //End User Tracking
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myPeoplDDeModalPut.html",
                    controller: "ModalInstanceCtrlAgent", resolve: { people: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.peoples.push(selectedItem);
                _.find($scope.peoples, function (people) {
                    if (people.id == selectedItem.id) {
                        people = selectedItem;
                    }
                });
                $scope.peoples = _.sortBy($scope.peoples, 'Id').reverse();
                $scope.selected = selectedItem;
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };
        $scope.opendelete = function (data, $index) {
            console.log(data);
            console.log("Delete Dialog called for people");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeletepeople.html",
                    controller: "ModalInstanceCtrldeletepeople", resolve: { people: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.splice($index, 1);
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };
        $scope.Salesagent = function () {
            var url = serviceBase + 'api/Agents';
            $http.get(url)
                .success(function (data) {
                    $scope.getAgent = data;
                    $scope.AddData = function () {

                        var url = serviceBase + "api/trackuser?action=View&item=Agent";
                        $http.post(url).success(function (results) {

                        });

                    }
                    $scope.AddData();
                    console.log("$scope.getAgent", $scope.getAgent);
                    $scope.callmethod();
                });
        }
        $scope.Salesagent();
        $scope.DeleteAgent = function (trade) {

            $http.delete(serviceBase + 'api/Agents/DeleteAgent?AgentId=' + trade.PeopleID).then(function (results) {
                if (results.data == "true") {
                    //User Tracking
                    console.log("Tracking Code");
                    $scope.AddTrack = function () {

                        var url = serviceBase + "api/trackuser?action=Delete(Agent)&item=AgentCode:" + trade.AgentCode;
                        $http.post(url).success(function (results) { });
                    }
                    $scope.AddTrack();
                    //End User Tracking
                    alert("Deleted Successfully");
                    $modalInstance.close();
                }
                else {
                    alert('There is some Problem');
                    $modalInstance.close();
                }
            });
        }
        $scope.warehouse = [];
        WarehouseService.getwarehouse().then(function (results) {

            $scope.warehouse = results.data;
        }, function (error) {
        });


        $scope.AddAgentAmount = function (trade) {

            console.log("Modal opened people");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "AddAgentAmountModel.html",
                    controller: "ModalAddAgentAmount", resolve: { Agent: function () { return trade } }
                });
            modalInstance.result.then(function (selectedItem) {
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };

        $scope.AddAgent = function () {
            console.log("Modal opened people");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "AddAgentModel.html",
                    controller: "ModalAddAgent"
                });
            modalInstance.result.then(function (selectedItem) {
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };
        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.getAgent,

                $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";



            $scope.numPerPageOpt = [3, 5, 10, 20];
            $scope.numPerPage = $scope.numPerPageOpt[1];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];

            $scope.search(); $scope.select(1);

        };
        $scope.select = function (page) {
            var end, start; console.log("select"); console.log($scope.stores);
            start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage;
            $scope.currentPageStores = $scope.filteredStores.slice(start, end);
        }

        $scope.onFilterChange = function () {
            console.log("onFilterChange"); console.log($scope.stores);
            $scope.select(1), $scope.currentPage = 1;
            $scope.row = "";
        }

        $scope.onNumPerPageChange = function () {
            console.log("onNumPerPageChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.onOrderChange = function () {
            console.log("onOrderChange"); console.log($scope.stores);
            $scope.select(1), $scope.currentPage = 1;
        }

        $scope.search = function () {
            console.log("search");
            console.log($scope.stores);
            console.log($scope.searchKeywords);

            $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords); $scope.onFilterChange();
        }

        $scope.order = function (rowName) {
            console.log("order"); console.log($scope.stores);
            $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0;
        }
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlAgent', ModalInstanceCtrlAgent);

    ModalInstanceCtrlAgent.$inject = ["$scope", '$http', '$modal', 'ngAuthSettings', "peoplesService", 'CityService', 'StateService', "$modalInstance", "people", 'WarehouseService', 'authService'];

    function ModalInstanceCtrlAgent($scope, $http, $modal, ngAuthSettings, peoplesService, CityService, StateService, $modalInstance, people, WarehouseService, authService) {
        $scope.pageno = 1; //initialize page no to 1


        $scope.total_count = 0;

        $scope.itemsPerPage = 30;  //this could be a dynamic value from a drop down

        $scope.numPerPageOpt = [30, 60, 90, 100];  //dropdown options for no. of Items per page

        $scope.onNumPerPageChange = function () {
            $scope.itemsPerPage = $scope.selected;

        }
        $scope.selected = $scope.numPerPageOpt[0];  //for Html page dropdown

        $scope.$on('$viewContentLoaded', function () {
            $scope.oldStocks($scope.pageno);
        });
        var AgentCode = people.AgentCode;


        $scope.oldStocks = function (pageno) {

            $scope.agentamountData = [];
            var url = serviceBase + "api/Agents/GetAgentOrderData?AgentCode=" + AgentCode + "&list=" + $scope.itemsPerPage + "&page=" + pageno;
            $http.get(url)
                .success(function (response) {
                    $scope.agentamountData = response.agentamount;
                    $scope.total_count = response.total_count;

                })
        }

        $scope.oldStocks($scope.pageno);

        $scope.closeModal = function () {

            $modalInstance.close();
        }

        $scope.open = function (trade) {

            console.log("Modal opened people");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "AgentOrderData.html",
                    controller: "AgentOrderDataCtrl", resolve: { Orders: function () { return trade } }
                });
            modalInstance.result.then(function (selectedItem) {
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('AgentOrderDataCtrl', AgentOrderDataCtrl);

    AgentOrderDataCtrl.$inject = ["$scope", '$http', '$modal', 'ngAuthSettings', "peoplesService", 'CityService', 'StateService', "$modalInstance", "Orders", 'WarehouseService', 'authService'];

    function AgentOrderDataCtrl($scope, $http, $modal, ngAuthSettings, peoplesService, CityService, StateService, $modalInstance, Orders, WarehouseService, authService) {
        var issurenceid = Orders.DeliveryIssuanceId;
        $scope.oldorders = [];
        var url = serviceBase + "api/Agents/GetAgentDataById?issurenceid=" + issurenceid;
        $http.get(url)
            .success(function (data) {
                $scope.OrderData = data;

            });
        $scope.cancel = function () {
            $modalInstance.close();
        };
        $scope.oldorderdata = [];
        var url1 = serviceBase + "api/Agents/GetOrderData?issurenceid=" + issurenceid;//instead of url used url1
        $http.get(url1)
            .success(function (data) {
                $scope.OrderData = data;

            });

    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalAddAgentAmount', ModalAddAgentAmount);

    ModalAddAgentAmount.$inject = ["$scope", '$http', '$modal', 'ngAuthSettings', "peoplesService", 'Agent', 'CityService', 'StateService', "$modalInstance", 'WarehouseService', 'authService'];

    function ModalAddAgentAmount($scope, $http, $modal, ngAuthSettings, peoplesService, Agent, CityService, StateService, $modalInstance, WarehouseService, authService) {
        //$scope.cancel = function () {
        //    $modalInstance.close();
        //}
        $scope.AgentCash = {};
        $scope.AgentCash.AgentId = Agent.PeopleID;
        $scope.AddAgentAmountData = function (AgentCash) {
            var Action = document.getElementById("btnAgentAmountAdd").getAttribute("value");
            if (Action == "Submit") {
                if (AgentCash.AgentId != "" && AgentCash.AgentAmount != "" && AgentCash.AgentAmount != undefined && AgentCash.AgentAmount > 0) {
                    var url = serviceBase + "/api/Agents/AgentAmountData";
                    var dataToPost = {
                        AgentId: AgentCash.AgentId,
                        AgentAddedAmount: AgentCash.AgentAmount,
                    };
                    $http.post(url, dataToPost).success(function (data) {
                        if (data == 0 || data == "0") {
                            alert("There is some problem");
                        }
                        else if (data == 2 || data == "2") {
                            alert("There is some Problem");
                        }
                        else {
                            alert('Amount Added Successfully');
                            $modalInstance.close(data);
                        }
                    })

                }
                else {
                    alert('Wrong Amount');
                }
            }
        }
        $scope.cancel = function () {
            $modalInstance.close();
        };
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalAddAgent', ModalAddAgent);

    ModalAddAgent.$inject = ["$scope", '$http', '$modal', 'ngAuthSettings', "peoplesService", 'CityService', 'StateService', "$modalInstance", 'WarehouseService', 'authService'];

    function ModalAddAgent($scope, $http, $modal, ngAuthSettings, peoplesService, CityService, StateService, $modalInstance, WarehouseService, authService) {
        $scope.PeopleData = {};
        $scope.getAgentCodes = function () {
            $http.get(serviceBase + 'api/Agents/GenerateAgentCode').then(function (results) {
                $scope.PeopleData.Agentcode = parseInt(results.data);
            });
        }
        $scope.getAgentCodes();
        $scope.AddAgentData = function (PeopleData) {
            var Action = document.getElementById("btnAgentAdd").getAttribute("value");
            if (Action == "Submit") {
                if (PeopleData.AgentFullName != "" && PeopleData.AgentFullName != undefined && PeopleData.Agentcode != "" && PeopleData.Agentcode != undefined) {
                    var url = serviceBase + "/api/Agents/AddAgent";
                    var dataToPost = {
                        DisplayName: PeopleData.AgentFullName,
                        Agentcode: PeopleData.Agentcode,
                    };
                    $http.post(url, dataToPost).success(function (data) {
                        if (data == 0 || data == "0") {
                            alert("There is some problem");
                        }
                        else if (data == 2 || data == "2") {
                            alert("Amount is already exist For this Agent Please edit it");
                        }
                        else {
                            alert('Agent Added Successfully');
                            $modalInstance.close(data);
                        }
                    })

                }
                else {
                    alert('Please fill both the field');
                }
            }
        }

        $scope.DeleteAgent = function (trade) {
            $http.delete(serviceBase + 'api/Agents/DeleteAgent?AgentId=' + trade.PeopleID).then(function (results) {
                if (results.data == "true") {
                    alert("Deleted Successfully");
                    $modalInstance.close();
                }
                else {
                    alert('There is some Problem');
                    $modalInstance.close();
                }
            });
        }

        $scope.cancel = function () {
            $modalInstance.close();
        };
    }
})();

