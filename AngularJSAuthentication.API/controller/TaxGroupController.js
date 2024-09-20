

(function () {
    'use strict';

    angular
        .module('app')
        .controller('TaxGroupController', TaxGroupController);

    TaxGroupController.$inject = ['$scope', 'TaxGroupService', 'TaxMasterService', 'TaxGroupDetailsService', "$filter", "$http", "ngTableParams", '$modal'];

    function TaxGroupController($scope, TaxGroupService, TaxMasterService, TaxGroupDetailsService, $filter, $http, ngTableParams, $modal) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log(" TaxGroup Controller reached");
        //User Tracking
        $scope.AddTrack = function (Atype, page, Detail) {

            console.log("Tracking Code");
            var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
            $http.post(url).success(function (results) { });
        }
        //End User Tracking
        $scope.currentPageStores = {};

        $scope.open = function () {
            console.log("Modal opened tax");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myaddTaxGroupModal.html",
                    controller: "ModalInstanceCtrlTaxGroup", resolve: { taxgroup: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.push(selectedItem);
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
        };

        $scope.view = function (item) {
            console.log("view Dialog called ");
            console.log(item);
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModalviewTaxGroup.html",
                    controller: "ModalInstanceCtrlTaxGroupview", resolve: { taxgroup: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.taxgroups.push(selectedItem);
                    _.find($scope.taxgroups, function (taxgroup) {
                        if (taxgroup.id == selectedItem.id) {
                            taxgroup = selectedItem;
                        }
                    });
                    $scope.taxgroups = _.sortBy($scope.taxgroup, 'Id').reverse();
                    $scope.selected = selectedItem;
                    console.log("view");
                    console.log(selectedItem);
                },
                    function () {
                        console.log("Cancel Condintion");

                    })
        };

        $scope.edit = function (item) {
            console.log("Edit Dialog called ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myTaxGroupModalPut.html",
                    controller: "ModalInstanceCtrlTaxGroup", resolve: { taxgroup: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {

                    $scope.taxgroups.push(selectedItem);
                    _.find($scope.taxgroups, function (taxgroup) {
                        if (taxgroup.id == selectedItem.id) {
                            taxgroup = selectedItem;
                        }
                    });
                    $scope.taxgroups = _.sortBy($scope.taxgroup, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                        console.log("Cancel Condintion");

                    })
        };

        $scope.opendelete = function (data, $index) {
            console.log(data);

            console.log("Delete Dialog called for state");



            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteTaxGroup.html",
                    controller: "ModalInstanceCtrldeletetaxGroup", resolve: { taxgroup: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                },
                    function () {
                        console.log("Cancel Condintion");

                    })
        };
        $scope.taxgroups = [];
        TaxGroupService.getTaxGroup().then(function (results) {
            $scope.taxgroups = results.data;
            console.log("tax group");
            console.log(results.data);
            $scope.callmethod();
            $scope.AddTrack("View", "Taxgroups", "");
        }, function (error) { });

        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.taxgroups;

            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

               
            $scope.numPerPageOpt = [3, 5, 10, 20];
            $scope.numPerPage = $scope.numPerPageOpt[2];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(); $scope.select(1);
        }
        $scope.select = function (page) {
            var end, start; console.log("select"); console.log($scope.stores);
            start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage; $scope.currentPageStores = $scope.filteredStores.slice(start, end);
        }

        $scope.onFilterChange = function () {
            console.log("onFilterChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1; $scope.row = "";
        }

        $scope.onNumPerPageChange = function () {
            console.log("onNumPerPageChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.onOrderChange = function () {
            console.log("onOrderChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1;
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
        .controller('ModalInstanceCtrlTaxGroup', ModalInstanceCtrlTaxGroup);

    ModalInstanceCtrlTaxGroup.$inject = ["$scope", '$http', 'ngAuthSettings', 'TaxGroupService', "TaxMasterService", "TaxGroupDetailsService", "$modalInstance", "taxgroup"];

    function ModalInstanceCtrlTaxGroup($scope, $http, ngAuthSettings, TaxGroupService, TaxMasterService, TaxGroupDetailsService, $modalInstance, taxgroup) {
        console.log("tax group");
        //User Tracking
        $scope.AddTrack = function (Atype, page, Detail) {

            console.log("Tracking Code");
            var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
            $http.post(url).success(function (results) { });
        }
        //End User Tracking
        $scope.TaxGroupData = {
        };
        $scope.taxmasters = [];

        $scope.examplemodel = [];
        $scope.exampledata = $scope.taxmasters;

        $scope.examplesettings = {
            displayProp: 'TaxName', idProp: 'TaxID',
            scrollableHeight: '300px',
            scrollableWidth: '450px',
            enableSearch: true,
            scrollable: true
        };
        //$scope.taxmasters = [];
        TaxMasterService.getTaxmaster().then(function (results) {
            $scope.taxmasters = results.data;
            console.log("tax types");
            console.log(results.data);
        }, function (error) {
        });

        $scope.taxdetails = [];
        TaxGroupDetailsService.gettaxdetails().then(function (results) {
            $scope.taxdetails = results.data;
            console.log("tax details");
            console.log(results.data);
        }, function (error) {
        });

        if (taxgroup) {
            console.log(" if conditon");
            $scope.TaxData = taxgroup;
            console.log($scope.TaxGroupData.TGrpName);

        }


        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.AddTaxGroup = function (data) {
                console.log("add Tax group ");
                console.log(data);
                console.log($scope.examplemodel);
                console.log($scope.examplemodel.TaxID);
                console.log("add Tax grousxsxsxsxsxsxp ");

                var url = serviceBase + "api/TaxGroup";
                var dataToPost = {
                    TGrpName: $scope.TaxGroupData.TGrpName,
                    TGrpAlias: $scope.TaxGroupData.TGrpAlias,
                    TGrpDiscription: $scope.TaxGroupData.TGrpDiscription,
                    CompanyId: $scope.TaxGroupData.CompanyId,
                    Active: $scope.TaxGroupData.Active,
                };

                console.log(dataToPost);
                $http.post(url, dataToPost)
                    .success(function (data) {
                        console.log("Error Got Here");
                        console.log(data);
                        console.log(data.GruopID);
                        if (data.GruopID != 0) {
                            addintaxdetail(data.GruopID);
                            //$scope.gotErrors = true;
                            //if (data[0].exception == "Already") {
                            //    console.log("Got This User Already Exist");
                            //    $scope.AlreadyExist = true;
                            //}
                        }
                        else {
                            $scope.AddTrack("Add(TaxGroup)", "TaxGroupName:", dataToPost.TGrpName);
                            $modalInstance.close(data);
                        }
                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })
            };
        function addintaxdetail(id) {
            console.log("Tax details  Called");
            console.log(id);

            var serviceBase = ngAuthSettings.apiServiceBaseUri;
            console.log("Called save row data");
            console.log($scope.examplemodel);
            console.log($scope.examplemodel);
            var postData = [];
            _.each($scope.examplemodel, function (o2) {
                var Row =
                {
                    "TaxID": o2.id,
                    "GruopID": id
                };
                postData.push(Row);
            })
            var turl = serviceBase + 'api/TaxGroupDetails';

            console.log("gooooooooot it");
            console.log(postData);

            $http.post(turl, JSON.stringify(postData)).success(function (data, status) {
                console.log(data);
            })
        }
        $scope.PutTaxGroup = function (data) {
            $scope.TaxGroupData = {

            };
            if (taxgroup) {
                $scope.TaxGroupData = taxgroup;
                console.log("found Puttt state");
                console.log(taxgroup);
                console.log($scope.TaxGroupData);

            }
            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

                console.log("Update ");



            var url = serviceBase + "api/TaxGroup";
            var dataToPost = {
                TaxID: $scope.TaxData.TaxID,
                TaxName: $scope.TaxData.TaxName,
                TAlias: $scope.TaxData.TAlias,
                TPercent: $scope.TaxData.TPercent,
                TDiscription: $scope.TaxData.TDiscription,
                CompanyId: $scope.TaxData.CompanyId,
                Active: $scope.TaxData.Active,
                CreatedDate: $scope.TaxData.CreatedDate,
                UpdatedDate: $scope.TaxData.UpdatedDate,
                Deleted: $scope.TaxData.Deleted
            };
            console.log(dataToPost);


            $http.put(url, dataToPost)
                .success(function (data) {

                    console.log("Error Got Here");
                    console.log(data);
                    if (data.id == 0) {

                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }

                    }
                    else {
                        $modalInstance.close(data);
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);

                })

        };
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrldeletetaxGroup', ModalInstanceCtrldeletetaxGroup);

    ModalInstanceCtrldeletetaxGroup.$inject = ["$scope", '$http', "$modalInstance", "TaxGroupService", 'ngAuthSettings', 'taxgroup'];

    function ModalInstanceCtrldeletetaxGroup($scope, $http, $modalInstance, TaxGroupService, ngAuthSettings, taxgroup) {
        console.log("delete modal opened");
        //User Tracking
        $scope.AddTrack = function (Atype, page, Detail) {

            console.log("Tracking Code");
            var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
            $http.post(url).success(function (results) { });
        }
        //End User Tracking
        $scope.taxgroups = [];
        if (taxgroup) {
            $scope.TaxGroupData = taxgroup;
            console.log("found");
            console.log(taxgroup);
            console.log($scope.TaxGroupData);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.deleteTaxGroup = function (dataToPost, $index) {
                console.log("Delete  controller");
                TaxGroupService.deleteTaxGroup(dataToPost).then(function (results) {
                    console.log("Del");

                    // console.log("index of item " + $index);
                    console.log($scope.deleteTaxGroups);
                    //$scope.state.splice($index, 1);

                    $scope.AddTrack("Delete(TxtGroup)", "TxtGroupName:", dataToPost.TGrpName);
                    $modalInstance.close(dataToPost);


                }, function (error) {
                    alert(error.data.message);
                });
            }
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlTaxGroupview', ModalInstanceCtrlTaxGroupview);

    ModalInstanceCtrlTaxGroupview.$inject = ["$scope", '$http', "$modalInstance", 'TaxGroupService', 'TaxMasterService', 'TaxGroupDetailsService', 'ngAuthSettings', 'taxgroup'];

    function ModalInstanceCtrlTaxGroupview($scope, $http, $modalInstance, TaxGroupService, TaxMasterService, TaxGroupDetailsService, ngAuthSettings, taxgroup) {
        console.log("View modal opened");
        //User Tracking
        $scope.AddTrack = function (Atype, page, Detail) {

            console.log("Tracking Code");
            var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
            $http.post(url).success(function (results) { });
        }
        //End User Tracking
        console.log("tax group");

        $scope.TaxGroupData = {
        };

        if (taxgroup) {
            console.log("state if conditon");
            console.log(taxgroup);
            $scope.TaxGroupData = taxgroup;
            console.log($scope.TaxGroupData.GruopID);
            console.log($scope.TaxGroupData.TGrpName);
        }

        $scope.taxdetails = [];
        TaxGroupDetailsService.gettaxdetails($scope.TaxGroupData.GruopID).then(function (results) {
            $scope.taxdetails = results.data;
            console.log("tax details");

            $scope.AddTrack("View(Details)", "TxtGroupName:", $scope.TaxGroupData.TGrpName);
            console.log(results.data);
        }, function (error) {
        });

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
    }
})();



