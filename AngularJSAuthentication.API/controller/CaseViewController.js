

(function () {
    'use strict';

    angular
        .module('app')
        .controller('CaseViewController', CaseViewController);

    CaseViewController.$inject = ['$scope', 'CaseService', "$filter", "$http", "ngTableParams", 'peoplesService', '$modal'];

    function CaseViewController($scope, CaseService, $filter, $http, ngTableParams, peoplesService, $modal) {

        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));

        console.log("Case Controller reached");
        $scope.getAllCase = function () {
            var url = serviceBase + 'api/Cases/getAllCase';
            $http.get(url).success(function (results) {
                $scope.AllCases = results;
                $scope.callmethod();
                if (AllCases == null) {
                    alert("Data Not Found");
                }
            })
        }
        $scope.getAllCase();

        //Exportdata
        alasql.fn.myfmt = function (n) {
            return Number(n).toFixed(2);
        }
        $scope.exportData = function () {
            alasql('SELECT Skcode,CategoryName,SubCategoryName,Priority,Statuscall,Summary,Status,Assignto,Description,CreatedByName,CreatedDate,UpdatedDate INTO XLSX("Caseslist.xlsx",{headers:true}) FROM ?', [$scope.AllCases]);
        };

        $scope.open = function (Demanddata) {

            console.log("Modal opened Case");

            var modalInstance;
            //alert(Demanddata.Status);
            if (Demanddata.Status == 'Open' || Demanddata.Status == 'Reassign') {
                modalInstance = $modal.open(
                    {
                        templateUrl: "mySetStatus.html",
                        controller: "CaseViewChangeStatusController", resolve: { cases: function () { return Demanddata } }
                    });
                modalInstance.result.then(function (selectedItem) {


                        $scope.currentPageStores.push(selectedItem);

                    },
                        function () {
                            console.log("Cancel Condintion");

                        })
            }
        };
        $scope.CaseHistroy = function (data) {

            $scope.CasedataHistroy = [];
            var url = serviceBase + "api/Cases/Casehistory?CaseId=" + data.CaseId;
            $http.get(url).success(function (response) {

                $scope.CasedataHistroy = response;
                console.log($scope.CasedataHistroy);
                $scope.AddTrack("View(History)", "CaseId:", data.CaseId);
            })
                .error(function (data) {
                })
        }

        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.AllCases;

            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

              

            $scope.numPerPageOpt = [10, 20, 50, 200];
            $scope.numPerPage = $scope.numPerPageOpt[1];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(), $scope.select(1);
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
        .controller('CaseViewChangeStatusController', CaseViewChangeStatusController);

    CaseViewChangeStatusController.$inject = ["$scope", '$http', 'ngAuthSettings', "CaseService", 'customerService', 'ProjectService', 'peoplesService', "$modalInstance", "cases", 'FileUploader'];

    function CaseViewChangeStatusController($scope, $http, ngAuthSettings, CaseService, customerService, ProjectService, peoplesService, $modalInstance, cases, FileUploader) {
        console.log("cases");

        var casesdata = cases;

        var User = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.CaseData = casesdata;
        $scope.CaseData.Skcode = casesdata.Skcode;

        $scope.status = [
            { stype: 'Open' },
            { stype: 'Close' },
            { stype: 'Reassign' },
            //{ stype: 'InProgress' },
            ////{ stype: 'PostFix' },
            ////{ stype: 'Ready for QA' },
            //{ stype: 'ToDo' }

        ];
        //Call type 
        $scope.statuscall = [
            { ctype: 'Incoming' },
            { ctype: 'Outgoing' },
            { ctype: 'Info' },
            { ctype: 'Wix' },
            { ctype: 'APP Rating' },
            { ctype: 'Ticket Generation' },
        ];
        //end
        //Assignto
        $scope.Assignto = [
            { atype: 'CUSTOMER DELIGHT' },
            { atype: 'DIGITAL LANDING' },
            { atype: 'FINANCE & ACCOUNT' },
            { atype: 'GROWTH' },
            { atype: 'KISAN KIRANA' },
            { atype: 'MARKETING' },
            { atype: 'OPERATIONS' },
            { atype: 'PRODUCT (IT)' },
            { atype: 'PURCHASE' },
            { atype: 'SALES' },
            { atype: 'SOURCING' },
        ];

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
            $scope.Setstatus = function (data) {

                console.log("case");
                console.log(data);
                var url = serviceBase + "api/Cases/Setstatus?CaseId=" + casesdata.CaseId + "&status=" + data.stype + "&discription=" + data.Summary;
                $http.post(url).success(function (data) {
                    console.log("cate updated successfully ");
                    $modalInstance.close();
                    alert("SAVE DATA SUCCESSFULLY!!!");
                })
            }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
            $scope.SetCase = function (data) {
                console.log("case");
                console.log(data);
                var summary = document.getElementById('site-name-summary');
                var project = document.getElementById('site-name-project');
                var customer = document.getElementById('site-name-Customer');
                var issuetype = document.getElementById('site-name-issue');
                if (data.stype == null || data.ptype == "") {
                    alert("Please Select Status!");
                    return;
                } else {
                    if (data.stype == "Reassign") {
                        if (data.atype == null) {
                            alert("Please Select Assignto!");
                            return;
                        }
                    }
                }

                if (data.Summary == null || data.Summary == "") {
                    alert("Please Enter Summary!!");
                    summary.focus();
                    return;
                }
                if (data.Description == null || data.Description == "") {
                    alert("Please Enter Description!");
                    return;
                }
                //if (data.ptype == null || data.ptype == "") {
                //    data.ptype = '--Select--';
                //}
                if (data.Issue == null || data.Issue == "") {
                    data.Issue = '--Select--';
                }
                // var LogoUrl = serviceBase + "../../UploadedLogos/" + $scope.files;
                // $scope.CaseData.LogoUrl = LogoUrl;
                // console.log($scope.CaseData.LogoUrl);

                var url = serviceBase + "api/Cases";
                var dataToPost = {
                    //LogoUrl: $scope.CaseData.LogoUrl,
                    CaseProjectId: data.CaseProjectId,
                    CustomerId: data.CustomerId,
                    IssueCategoryId: data.IssueCategoryId,
                    IssueSubCategoryId: data.IssueSubCategoryId,
                    Department: data.Department,
                    PeopleID: data.PeopleID,
                    SKCode: data.Skcode,
                    Summary: data.Summary,
                    Description: data.Description,
                    IssueCategoryName: data.CategoryName,
                    IssueSubCategoryName: data.SubCategoryName,
                    MobileNumber: data.MobileNumber,
                    Createdby: User.userid,
                    CreatedbyName: User.userName,
                    //Description: data.Description,
                    Priority: data.Priority,
                    //Issues: data.lissue,
                    //Labels: data.Labels,
                    //Issue: data.Issue,
                    //EpicLink: data.EpicLink,
                    Status: data.stype,
                    Statuscall: data.Statuscall,
                    CreatedDate: data.CreatedDate,
                    CaseId: data.CaseId,
                    Assignto: data.atype,
                    CaseNumber: $scope.count
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
                            console.log(data);
                            console.log(data);
                            $modalInstance.close(data);
                        }
                        confirm("SAVE DATA SUCCESSFULLY!!!");
                        window.location.reload();
                    })
                    .error(function (data) {
                        console.log("Error Got Here is ");
                        console.log(data);
                        // return $scope.showInfoOnSubmit = !0, $scope.revert()
                    })
            };
    }
})();