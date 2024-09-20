
(function () {
    'use strict';

    angular
        .module('app')
        .controller('casecontroller', casecontroller);

    casecontroller.$inject = ['$scope', 'CaseService', "$filter", "$http", "ngTableParams", 'peoplesService', '$modal'];

    function casecontroller($scope, CaseService, $filter, $http, ngTableParams, peoplesService, $modal) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        

        $scope.currentPageStores = {};

        //$scope.items = {};
        //$scope.open = function () {

        //    console.log("Modal opened Case");

        //    var modalInstance;

        //    modalInstance = $modal.open(
        //        {
        //            templateUrl: "myCaseModal.html",
        //            controller: "ModalInstanceCtrlCase", resolve: { cases: function () { return $scope.items } }
        //        });
        //    modalInstance.result.then(function (selectedItem) {


        //            $scope.currentPageStores.push(selectedItem);

        //        },
        //            function () {
        //                console.log("Cancel Condintion");

        //            })
        //};
        //add Detail

        $scope.open1 = function (Demanddata) {

            console.log("Modal opened Case");

            var modalInstance;

            modalInstance = $modal.open(
                {
                    templateUrl: "myAddDetailmodal.html",
                    controller: "ModalInstanceCtrlCase", resolve: { cases: function () { return Demanddata } }
                });
            modalInstance.result.then(function (selectedItem) {


                    $scope.currentPageStores.push(selectedItem);

                },
                    function () {
                        console.log("Cancel Condintion");

                    })
        };

        $scope.CaseHistroy = function (data) {

            $scope.CasedataHistroy = [];
            var url = serviceBase + "api/Cases/Casehistory?CaseId=" + data.CaseId;
            $http.get(url).success(function (response) {

                $scope.CasedataHistroy = response;
                console.log($scope.CasedataHistroy);
                $scope.AddTrack("View(History)", "caseid:", data.CaseId);
            })
                .error(function (data) {
                })
        }

        //$scope.edit = function (item) {
        //    var newrodata = {};
        //    if ($scope.casess.length > 0) {
        //        for (var i = 0; i < $scope.casess.length; i++) {
        //            if ($scope.casess[i].CaseId == item) {
        //                newrodata = $scope.casess[i];
        //            }
        //        }
        //    }
        //    var modalInstance;
        //    modalInstance = $modal.open(
        //        {
        //            templateUrl: "myCaseModalPut.html",
        //            controller: "ModalInstanceCtrlCase", resolve: { cases: function () { return newrodata } }
        //        });
        //    modalInstance.result.then(function (selectedItem) {

        //            $scope.casess.push(selectedItem);
        //            _.find($scope.casess, function (cases) {
        //                if (cases.id == selectedItem.id) {
        //                    cases = selectedItem;
        //                }
        //            });

        //            $scope.casess = _.sortBy($scope.cases, 'Id').reverse();
        //            $scope.selected = selectedItem;
        //        },
        //            function () {
        //                console.log("Cancel Condintion");
        //            })
        //};


        //$scope.comment = function (item) {

        //    var newrodata = {};
        //    if ($scope.casess.length > 0) {
        //        for (var i = 0; i < $scope.casess.length; i++) {
        //            if ($scope.casess[i].CaseId == item) {
        //                newrodata = $scope.casess[i];
        //            }
        //        }
        //    }
        //    var modalInstance;
        //    modalInstance = $modal.open(
        //        {
        //            templateUrl: "myCaseModalComment.html",
        //            controller: "ModalInstanceCtrlCase", resolve: { cases: function () { return newrodata } }
        //        });
        //    modalInstance.result.then(function (selectedItem) {

        //            $scope.casess.push(selectedItem);
        //            _.find($scope.casess, function (cases) {
        //                if (cases.id == selectedItem.id) {
        //                    cases = selectedItem;
        //                }
        //            });

        //            $scope.casess = _.sortBy($scope.cases, 'Id').reverse();
        //            $scope.selected = selectedItem;
        //        },
        //            function () {
        //                console.log("Cancel Condintion");
        //            })
        //};


        $scope.opendelete = function (data) {
            console.log(data);
            console.log("Delete Dialog called for Case");

            var newrodata = {};
            if ($scope.casess.length > 0) {
                for (var i = 0; i < $scope.casess.length; i++) {
                    if ($scope.casess[i].CaseId == data) {
                        newrodata = $scope.casess[i];
                    }
                }
            }


            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteCase.html",
                    controller: "ModalInstanceCtrldeleteCase", resolve: { cases: function () { return newrodata } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                },
                    function () {
                        console.log("Cancel Condintion");

                    })
        };

        //$scope.casess = [];

        //CaseService.getcases().then(function (results) {

        //    $scope.casess = results.data;


        //    $scope.callmethod();
        //    if ($scope.casess.length > 0) {
        //        for (var i = 0; i < $scope.casess.length; i++) {

        //            if ($scope.casess[i].Status == "ToDo") {
        //                $scope.Createtablerow($scope.casess[i])
        //            }
        //            else if ($scope.casess[i].Status == "InProgress") {
        //                $scope.CreatetablerowProgress($scope.casess[i])
        //            }
        //            else if ($scope.casess[i].Status == "DONE") {
        //                $scope.CreatetablerowDone($scope.casess[i])
        //            }
        //        }
        //    }

        //}, function (error) {

        //});

        $scope.Createtablerow = function (rowdata) {

            var htmlelement = '<article class="kanban-entry grab" id="' + rowdata.CaseId + '" draggable="true" >' + '<div  onclick="angular.element(this).scope().edit(' + rowdata.CaseId + ')">' + '<h3 class="text-primary" title="Project Name">' + rowdata.ProjectName + '</h3>' + '<h4 class="text-danger" title="Customer Name">' + rowdata.CustomerName + '</h4>' + '<label  class="btn-sm btn-success" title="Priority">' + rowdata.Priority + '</label>' + '</div>' + '</br>' + '<button  class="btn-sm btn-info" onclick="angular.element(this).scope().comment(' + rowdata.CaseId + ')"><i>Comment</i></button>' + '<button class="btn-sm btn-primary" onclick="angular.element(this).scope().edit(' + rowdata.CaseId + ')">Edit</button>' + '<button   ng-show="(UserRole.email== admin@shopkirana.com)"  class="btn-sm btn-danger" onclick="angular.element(this).scope().opendelete(' + rowdata.CaseId + ')">Remove</button>' + '</div>' + '</article>';
            $('#ToDo').append(htmlelement);


        }

        $scope.CreatetablerowProgress = function (rowdata) {


            var htmlelement = '<article class="kanban-entry grab" id="' + rowdata.CaseId + '" draggable="true"  >' + '<div  onclick="angular.element(this).scope().edit(' + rowdata.CaseId + ')">' + '<h4 class="text-primary" title="Project Name">' + rowdata.ProjectName + '</h4>' + '<h4 class="text-danger" title=" Customer Name">' + rowdata.CustomerName + '</h4>' + '<label class="btn-sm btn-success" title="Priority">' + rowdata.Priority + '</label>' + '</div>' + '</br>' + '<button  class="btn-sm btn-info" onclick="angular.element(this).scope().comment(' + rowdata.CaseId + ')"><i>Comment</i></button>' + '<button class="btn-sm btn-primary" onclick="angular.element(this).scope().edit(' + rowdata.CaseId + ')">Edit</button>' + '<button  ng-show="(UserRole.email== admin@shopkirana.com)"  class="btn-sm btn-danger" onclick="angular.element(this).scope().opendelete(' + rowdata.CaseId + ')">Remove</button>' + '</div>' + '</article>';
            $('#InProgress').append(htmlelement);


        }

        $scope.CreatetablerowDone = function (rowdata) {


            $scope.rowdata = rowdata;
            var htmlelement = '<article class="kanban-entry grab" id="' + rowdata.CaseId + '"  draggable="true" >' + '<div  onclick="angular.element(this).scope().edit(' + rowdata.CaseId + ')">' + '<h4 class="text-primary" title="Project Name">' + rowdata.ProjectName + '</h4>' + '<h4 class="text-danger" title="Customer Name">' + rowdata.CustomerName + '</h4>' + '<label  class="btn-sm btn-success" title="Priority">' + rowdata.Priority + '</label>' + '</div>' + '</br>' + '<button  class="btn-sm btn-info" onclick="angular.element(this).scope().comment(' + rowdata.CaseId + ')"><i>Comment</i></button>' + '<button class="btn-sm btn-primary" onclick="angular.element(this).scope().edit(' + rowdata.CaseId + ')">Edit</button>' + '<button ng-show="(UserRole.email== admin@shopkirana.com)"  class="btn-sm btn-danger" onclick="angular.element(this).scope().opendelete(' + rowdata.CaseId + ')">Remove</button>' + '</div>' + '</article>';
            $('#DONE').append(htmlelement);


        }
        //+ '<button class="btn-sm btn-primary" ng-click="edit(trade)">EDIT</button>'
        //                                + '<button class="btn-sm btn-danger"  ng-click="opendelete(trade,$index)">Remove</button>'
        $(function () {

            var kanbanCol = $('.panel-body');

            kanbanCol.css('max-height', (window.innerHeight - 150) + 'px');

            var kanbanColCount = parseInt(kanbanCol.length);

            $('.container-fluid').css('min-width', (kanbanColCount * 350) + 'px');
            draggableInit();
            $('.panel-heading').click(function () {
                var $panelBody = $(this).parent().children('.panel-body');
                $panelBody.slideToggle();
            });
        });

        function draggableInit() {
            var sourceId;

            $('[draggable=true]').bind('dragstart', function (event) {
                sourceId = $(this).parent().attr('id');

                event.originalEvent.dataTransfer.setData("text/plain", event.target.getAttribute('id'));


            });


            $('.panel-body').bind('dragover', function (event) {
                event.preventDefault();
            });

            $('.panel-body').bind('drop', function (event) {
                draggableInit();

                var children = $(this).children();
                var targetId = children.attr('id');


                if (sourceId != targetId) {
                    var elementId = event.originalEvent.dataTransfer.getData("text/plain");
                    //$('#processing-modal').modal('toggle'); //before post

                    //  setTimeout(function () {
                    var element = document.getElementById(elementId);
                    children.prepend(element);

                    var url = serviceBase + "api/Cases/EditCase";
                    var dataToPost = {
                        CaseId: elementId,
                        Status: targetId
                    };
                    console.log(dataToPost);
                    $http.put(url, dataToPost)
                        .success(function (data) {
                            console.log("Error Gor Here");
                            console.log(data);
                            if (data.id == 0) {
                                $scope.gotErrors = true;
                                if (data[0].exception == "Already") {
                                    console.log("Got This User Already Exist");
                                    $scope.AlreadyExist = true;
                                }
                            }
                            else {
                                //$modalInstance.close(data);
                            }
                        })
                        .error(function (data) {
                            console.log("Error Got Heere is ");
                            console.log(data);
                        })

                }
                event.preventDefault();
            });
        }

        //case management
        $scope.dataforsearch = { mobile: "", skcode: "" };
        $scope.Search = function (data) {


            var url = serviceBase + 'api/Cases/search1?mobile=' + data.mobile + '&&' + 'skcode=' + data.skcode;
            $http.get(url).success(function (results) {
                $scope.customers = results;

                if ($scope.customers)
                {
                    var url = serviceBase + 'api/Cases/casesdata?mobile=' + data.mobile + '&&' + 'skcode=' + data.skcode;
                    $http.get(url).success(function (results)
                    {

                        $scope.AllCases = results;
                    }); }
                

                $scope.data = $scope.customers;

                $scope.allcusts = true;
                $scope.tableParams = new ngTableParams({
                    page: 1,
                    count: 100,
                    ngTableParams: $scope.customers
                }, {
                        total: $scope.data.length,
                        getData: function ($defer, params) {
                            var orderedData = params.sorting() ? $filter('orderBy')($scope.data, params.orderBy()) : $scope.data;
                            orderedData = params.filter() ?
                                $filter('filter')(orderedData, params.filter()) :
                                orderedData;
                            $defer.resolve(orderedData.slice((params.page() - 1) * params.count(), params.page() * params.count()));
                        }
                    });
            });
        }
        //end
        //
        $scope.dataforsearch = { Assignto: "" };


        $scope.open = function (Demanddata) {

            console.log("Modal opened Case");

            var modalInstance;
            // alert(Demanddata.Status);
            if (Demanddata.Status == 'Open' || Demanddata.Status == 'Reassign') {
                modalInstance = $modal.open(
                    {
                        templateUrl: "mySetStatus.html",
                        controller: "ModalInstanceCtrlCase", resolve: { cases: function () { return Demanddata } }
                    });
                modalInstance.result.then(function (selectedItem) {

                    },
                        function () {
                            console.log("Cancel Condintion");

                        })
            }
        };

        $scope.callmethod = function () {

            var init;
            //$scope.stores = $scope.casess;

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

        //search
        //$scope.srch = "";
        //$scope.searchdata = function (data) {

        //    if ($scope.srch == "") {
        //        alert("Please Enter Value for Search!!")
        //        return;
        //    }
        //    else {
        //        $scope.currentPageStores = [];
        //        var url = serviceBase + "api/Cases/search?&ProjectName=" + $scope.srch.pn;
        //        $http.get(url).success(function (response) {
        //            $scope.currentPageStores = response;
        //            $scope.total_count = response.length;
        //        });
        //    }
        //};

        //$scope.peoples = [];
        //peoplesService.getpeoples().then(function (results) {

        //    $scope.peoples = results.data;
        //    console.log("peoples");
        //    console.log($scope.peoples);
        //}, function (error) {
        //});

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlCase', ModalInstanceCtrlCase);

    ModalInstanceCtrlCase.$inject = ["$scope", '$http', 'ngAuthSettings', "CaseService", 'customerService', 'ProjectService', 'peoplesService', "$modalInstance", "cases", 'FileUploader'];

    function ModalInstanceCtrlCase($scope, $http, ngAuthSettings, CaseService, customerService, ProjectService, peoplesService, $modalInstance, cases, FileUploader) {
        console.log("cases");
        

        var Casedata = cases;
        var User = JSON.parse(localStorage.getItem('RolePerson'));
        function noDisplay() {
            var count = localStorage.getItem("count") == null ? 1 : localStorage.getItem("count");
            count = Number(count) + 1;
            localStorage.setItem("count", count);
            $scope.count = count;
            console.log($scope.count);
        }
        noDisplay();

        //1. Used to list all selected files
        $scope.files = [];

        //2. a simple model that want to pass to Web API along with selected files
        $scope.jsonData = {
            CaseNumber: $scope.count
        };
        //3. listen for the file selected event which is raised from directive
        $scope.$on("seletedFile", function (event, args) {
            $scope.$apply(function () {
                //add the file object to the scope's files collection
                $scope.files.push(args.file);
            });
        });

        var url = serviceBase + 'api/Cases/PostFileWithData';

        //4. Post data and selected files.
        $scope.Upload = function () {

            $http({
                method: 'POST',
                url: url,
                headers: { 'Content-Type': undefined },

                transformRequest: function (data) {

                    var formData = new FormData();
                    formData.append("jsonData", angular.toJson(data.jsonData));
                    for (var i = 0; i < data.files.length; i++) {
                        formData.append("file" + i, data.files[i]);
                    }
                    return formData;

                },
                data: { jsonData: $scope.jsonData.CaseNumber, files: $scope.files }

            }).
                success(function (data, status, headers, config) {
                    alert("success!");
                    console.log($scope.files);


                }).
                error(function (data, status, headers, config) {
                    alert("failed!");
                });
        };

        $scope.CaseData = {};

        $scope.CaseData.skcode = cases.skcode;
        $scope.CaseData.mobile = cases.mobile;

        $scope.issuetype = [
            { type: 'Internal' },
            { type: 'External' } //,
            //{ type: 'Bug' },
            //{ type: 'Epic' }
        ];

        // $scope.issue = $scope.issuetype[0];

        $scope.prioritytype = [
            { ptype: 'priority-Medium' },
            { ptype: 'priority-Highest' },
            { ptype: 'priority-High' },
            { ptype: 'priority-Low' },
            { ptype: 'priority-Lowest' }
        ];

        //    $scope.typs = [
        //{ type: 'Internal' },
        //{ type: 'External' },

        //    ];


        $scope.status = [
            { stype: 'Open' },
            { stype: 'Close' },
            { stype: 'Reassign' },

        ];
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
        //end

        //call Type
        $scope.statuscall = [
            { ctype: 'Incoming' },
            { ctype: 'Outgoing' },
            { ctype: 'Info' },
            { ctype: 'Wix' },
            { ctype: 'App Rating' },
            { ctype: 'Ticket Generation' },
        ];
        //end
        $scope.linkedissue = [
            //{ lissue: 'related to' },
            //{ lissue: 'blocks' },
            //{ lissue: 'is blocked by' },
            //{ lissue: 'clones' },
            //{ lissue: 'is cloned by' },
            //{ lissue: 'duplicates' },
            //{ lissue: 'is duplicated by' },
            //{ lissue: 'caused' },
            //{ lissue: 'is caused by' }
            { lissue: 'Bug' },
            { lissue: 'Epic' },
            { lissue: 'Story' },
            { lissue: 'duplicates' },
            { lissue: 'blocks' }

        ];


        //$scope.peoples = [];
        //peoplesService.getpeoples().then(function (results) {

        //    $scope.peoples = results.data;
        //    console.log("peoples");
        //    console.log($scope.peoples);
        //}, function (error) {
        //});

       // $scope.customers = [];
        //customerService.getcustomers().then(function (results) {

        //    $scope.customers = results.data;
        //    console.log("customers");
        //    console.log($scope.customers);
        //}, function (error) {
        //});

        //$scope.projects = [];
        //ProjectService.getProjects().then(function (results) {

        //    $scope.projects = results.data;
        //    console.log("projects");
        //    console.log($scope.projects);

        //}, function (error) {
        //});
        //Add Detail
        $scope.issueCategorys = [];
        ProjectService.getIssueCategorys().then(function (results) {

            $scope.issueCategorys = results.data;
            console.log("projects");
            console.log($scope.projects);

        }, function (error) {
        });

        $scope.issueSubCategorys = [];
        $scope.GetIssueSubCategory = function (IssueCategoryId) {

            var url = serviceBase + "api/Projects/GetIssueSubCategory?CaseProjectId=" + IssueCategoryId;
            $http.get(url).success(function (response) {
                $scope.issueSubCategorys = response;
            });
        }

        //end
        if (cases) {
            console.log("case if conditon");
            $scope.CaseData = cases;

            console.log("");
        }

        //$scope.CommentGet = function () {

        //    if ($scope.CaseData.CaseId) {
        //        var url = serviceBase + 'api/Cases/GetComment?CaseId=' + $scope.CaseData.CaseId;
        //        $http.get(url)
        //            .success(function (data) {

        //                $scope.getComment = data;
        //                console.log("$scope.getComment", $scope.getComment);

        //            });}
           
        //}
        //$scope.CommentGet();

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.AddCase = function(data) {
            
                var summary = document.getElementById('site-name-summary');
                var project = document.getElementById('site-name-project');
                var customer = document.getElementById('site-name-Customer');
                var issuetype = document.getElementById('site-name-issue');

            if (data.ptype === null || data.ptype == "") {
                data.ptype = '--Select--';
            }
            if (data.Issue === null || data.Issue == "") {
                data.Issue = '--Select--';
            } if (data.Summary == null || data.Summary == "") {
                alert("Please Enter Discription ");
                return;
            } if (data.stype == null || data.stype == "" || data.stype == '--Select--') {
                alert("Please Select Status! ");
                return;
            } if (data.skcode == null || data.skcode == undefined || data.skcode == "") {
                alert("Please Enter Skcode");
                return;
            } if (data.mobile == null || data.mobile == "") {
                alert("Please Enter MobileNumber");
                return;
            } if (data.IssueCategoryId == null) {
                alert("Please Select IssueCategory");
                return;

            } if (data.issueSubCategoryId == null) {
                alert("Please Select issueSubCategory");
                return;
            } if (data.ptype == null) {
                alert("Please Select Priority");
                return;
            } if (data.ctype == null) {
                alert("Please Select Call type");
                return;
            }

                // var LogoUrl = serviceBase + "../../UploadedLogos/" + $scope.files;
                // $scope.CaseData.LogoUrl = LogoUrl;
                // console.log($scope.CaseData.LogoUrl);

                var url = serviceBase + "api/Cases/AddCase";
                var dataToPost = {

                    //LogoUrl: $scope.CaseData.LogoUrl,
                    CaseProjectId: data.CaseProjectId,
                    CustomerId: data.CustomerId,
                    IssueCategoryId: data.IssueCategoryId,
                    IssueSubCategoryId: data.issueSubCategoryId,
                    Department: data.Department,
                    PeopleID: data.PeopleID,
                    skcode: data.skcode,
                    Summary: data.Summary,
                    MobileNumber: data.mobile,
                    Createdby: User.userid,
                    CreatedbyName: User.userName,
                    //Description: data.Description,

                    Priority: data.ptype,
                    //Issues: data.lissue,
                    //Labels: data.Labels,
                    Issue: data.Issue,
                    //EpicLink: data.EpicLink,
                    Status: data.stype,
                    Statuscall: data.ctype,
                    CreatedDate: data.CreatedDate,
                    CaseId: data.CaseId,
                    Assignto: data.atype,
                    CaseNumber: $scope.count
                };
                console.log(dataToPost);

                $http.post(url, dataToPost)

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
        //Set Status
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.SetCase = function (data) {

                console.log("case");
                console.log(data);
                var summary = document.getElementById('site-name-summary');
                var project = document.getElementById('site-name-project');
                var customer = document.getElementById('site-name-Customer');
                var issuetype = document.getElementById('site-name-issue');
                if (data.Summary == null || data.Summary == "") {
                    alert("Please Enter Summary!!");
                    summary.focus();
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
                    skcode: data.SKCode,
                    Summary: data.Summary,
                    Description: data.Description,
                    IssueCategoryName: data.IssueCategoryName,
                    IssueSubCategoryName: data.IssueSubCategoryName,
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

        $scope.PutCase = function (data) {

            $scope.CaseData = {
            };
            $scope.loogourl = cases.LogoUrl;
            if (cases) {
                $scope.CaseData = cases;
                console.log("found Puttt case");
                console.log(cases);
                console.log($scope.CaseData);

            }
            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

                console.log("Update ");

            if ($scope.uploadedfileName == null || $scope.uploadedfileName == '') {
                console.log("if looppppppppppp");
                var url = serviceBase + "api/Cases/PutCase";
                var dataToPost = {
                    LogoUrl: $scope.loogourl,
                    CaseId: $scope.CaseData.CaseId,
                    CaseProjectId: data.CaseProjectId,
                    Department: data.Department,
                    PeopleID: data.PeopleID,
                    IssueType: data.IssueType,
                    Summary: data.Summary,
                    Description: data.Description,
                    Priority: data.Priority,
                    Issues: data.Issues,
                    Labels: data.Labels,
                    Issue: data.Issue,
                    EpicLink: data.EpicLink,
                    Status: data.Status
                };

                //var dataToPost = { SurveyId: $scope.SurveyData.SurveyId, SurveyCategoryName: $scope.SurveyData.SurveyCategoryName, Discription: $scope.SurveyData.Discription, CreatedDate: $scope.SurveyData.CreatedDate, UpdatedDate: $scope.SurveyData.UpdatedDate, CreatedBy: $scope.SurveyData.CreatedBy, UpdateBy: $scope.SurveyData.UpdateBy };
                console.log(dataToPost);
                $http.put(url, dataToPost)
                    .success(function (data) {
                        console.log("Error Gor Here");
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
                alert("Data Update Successfully!!!")
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })
            }

            else {
                console.log("Else loppppppppp ");
                var LogoUrl = serviceBase + "../../UploadedLogos/" + $scope.uploadedfileName;
                console.log(LogoUrl);
                console.log("Image name in Insert function :" + $scope.uploadedfileName);
                $scope.CaseData.LogoUrl = LogoUrl;
                console.log($scope.CaseData.LogoUrl);
                var url1 = serviceBase + "api/Cases";//instead of url used url1
                var dataToPost1 = {//instead of dataToPost used dataToPost1
                    LogoUrl: $scope.CaseData.LogoUrl,
                    CaseId: data.CaseId,
                    CaseProjectId: data.CaseProjectId,
                    Department: data.Department,
                    PeopleID: data.PeopleID,
                    IssueType: data.IssueType,
                    Summary: data.Summary,
                    Description: data.Description,
                    Priority: data.Priority,
                    Issues: data.Issues,
                    Labels: data.Labels,
                    Issue: data.Issue,
                    EpicLink: data.EpicLink,
                    Status: data.Status
                };
                console.log(dataToPost1);
                $http.put(url1, dataToPost1)
                    .success(function (data) {
                        console.log("Error Gor Here");
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
            }
        };

        $scope.AddComment = function (data) {


            console.log("case");
            console.log(data);

            var url = serviceBase + "api/Cases/AddComment";
            var dataToPost = {
                CaseId: $scope.CaseData.CaseId,
                Comments: data.Comments,
                Status: data.Status,
                UserName: $scope.peoples[0].PeopleID
            };
            console.log(dataToPost);

            $http.post(url, dataToPost)
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

                    }

                    alert("comment save successfully");
                })
                .error(function (data) {
                    console.log("Error Got Here is ");
                    console.log(data);
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })

            var url2 = serviceBase + "api/Cases";//instead of url used url2 

            var dataToPost2 = { //instead of dataToPost used dataToPost2  
                LogoUrl: $scope.CaseData.LogoUrl,
                CaseId: data.CaseId,
                CaseProjectId: data.CaseProjectId,
                Department: data.Department,
                PeopleID: data.PeopleID,
                IssueType: data.IssueType,
                Summary: data.Summary,
                Description: data.Description,
                Priority: data.Priority,
                Issues: data.Issues,
                Labels: data.Labels,
                Issue: data.Issue,
                EpicLink: data.EpicLink,
                Status: data.Status
            };

            //var dataToPost = { SurveyId: $scope.SurveyData.SurveyId, SurveyCategoryName: $scope.SurveyData.SurveyCategoryName, Discription: $scope.SurveyData.Discription, CreatedDate: $scope.SurveyData.CreatedDate, UpdatedDate: $scope.SurveyData.UpdatedDate, CreatedBy: $scope.SurveyData.CreatedBy, UpdateBy: $scope.SurveyData.UpdateBy };
            console.log(dataToPost2);
            $http.put(url2, dataToPost2)
                .success(function (data) {
                    console.log("Error Gor Here");
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


        /////////////////////////////////////////////////////// angular upload code
        var uploader = $scope.uploader = new FileUploader({
            url: serviceBase + 'api/logoUpload'
        });
        //FILTERS
        uploader.filters.push({
            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });
        //CALLBACKS
        uploader.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
            console.info('onWhenAddingFileFailed', item, filter, options);
        };
        uploader.onAfterAddingFile = function (fileItem) {
            console.info('onAfterAddingFile', fileItem);
        };
        uploader.onAfterAddingAll = function (addedFileItems) {
            console.info('onAfterAddingAll', addedFileItems);
        };
        uploader.onBeforeUploadItem = function (item) {
            console.info('onBeforeUploadItem', item);
        };
        uploader.onProgressItem = function (fileItem, progress) {
            console.info('onProgressItem', fileItem, progress);
        };
        uploader.onProgressAll = function (progress) {
            console.info('onProgressAll', progress);
        };
        uploader.onSuccessItem = function (fileItem, response, status, headers) {
            console.info('onSuccessItem', fileItem, response, status, headers);
        };
        uploader.onErrorItem = function (fileItem, response, status, headers) {
            console.info('onErrorItem', fileItem, response, status, headers);
        };
        uploader.onCancelItem = function (fileItem, response, status, headers) {
            console.info('onCancelItem', fileItem, response, status, headers);
        };
        uploader.onCompleteItem = function (fileItem, response, status, headers) {
            console.info('onCompleteItem', fileItem, response, status, headers);
            console.log("File Name :" + fileItem._file.name);
            $scope.uploadedfileName = fileItem._file.name;
        };
        uploader.onCompleteAll = function () {
            console.info('onCompleteAll');
        };
        console.info('uploader', uploader);


    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrldeleteCase', ModalInstanceCtrldeleteCase);

    ModalInstanceCtrldeleteCase.$inject = ["$scope", '$http', "$modalInstance", "CaseService", 'ngAuthSettings', "cases"];

    function ModalInstanceCtrldeleteCase($scope, $http, $modalInstance, CaseService, ngAuthSettings, cases) {
        console.log("delete modal opened");

        
        $scope.casess = [];

        if (cases) {
            $scope.CaseData = cases;
            console.log("found case");
            console.log(cases);
            console.log($scope.CaseData);

        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


            $scope.deletecase = function (dataToPost, $index) {

                console.log("Delete  case controller");


                CaseService.deletecase(dataToPost).then(function (results) {
                    console.log("Del");

                    console.log("index of item " + $index);
                    console.log($scope.casess.length);
                    console.log($scope.casess.length);

                    $modalInstance.close(dataToPost);


                }, function (error) {
                    alert(error.data.message);
                });
            }

    }
})();
