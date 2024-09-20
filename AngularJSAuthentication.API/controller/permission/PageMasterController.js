'use strict';
app.controller('PageMasterController', ['$scope', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal', "PageMasterService", function ($scope, $http, ngAuthSettings, $filter, ngTableParams, $modal, PageMasterService) {
    $scope.currentPageStores = {};
    $scope.data = [];
    $scope.data.Id = "";
    $scope.getPageMaster = function (Id) {      
        PageMasterService.getPageMaster().then(function (results) {          
            $scope.Parent = results.data;
        }, function (error) {
        });
    };
    
    $scope.getPageMaster();  
    $scope.Isupdate = false;

    $scope.clear = function (data) {
        $scope.data = [];     
    }
   
    //$scope.ParentId = $scope.data.ParentId;
    $scope.AddDetails = function (data) {
        if (!data.PageName) {
            alert("Please enter page name.");
            return false;
        }
        //else if (!data.RouteName) {
        //    alert("Please enter Route Url name.");
        //    return false;
        //}
            var url = serviceBase + "api/PageMaster/SavePageMaster";
            var dataToPost = {
                Id: data.Id,
                ParentId: data.ParentId,
                PageName: data.PageName,
                RouteName: data.RouteName,
                ClassName: data.ClassName,
                IconClassName: data.IconClassName,
                IsNewPortalUrl: data.IsNewPortalUrl,
                IsGroup2PortalUrl: data.IsGroup2PortalUrl
            };
            $http.post(url, dataToPost)
                .success(function (data) {
                    if (data) {
                        alert("Page save Successfully");
                        window.location.reload();
                    }
                    else {
                        alert("Some error occurred during save page detail");
                    }
                })
                .error(function (data) {
                    alert("Some error occurred during save page detail");
                });
            $modelInstance.close(data);        
        };

    $scope.Parentlist = [];
  
    $scope.GetParent = function () {
        
        $scope.Parentlist = [];
        var url = serviceBase + "api/PageMaster/GetAllParentPages";
        $http.get(url)
            .success(function (response) {               
                $scope.Parentlist = response;                
            });
    };
    $scope.GetParent();
       
    $scope.onDrop = function (srcList, srcIndex, targetList, targetIndex) {       
        targetList.splice(targetIndex, 0, srcList[srcIndex]);      
        if (srcList == targetList && targetIndex <= srcIndex) srcIndex++;
        srcList.splice(srcIndex, 1);
       // 
        return true;
    };

    $scope.onDropChild = function (srcList, srcIndex, targetList, targetIndex) {
        targetList.splice(targetIndex, 0, srcList[srcIndex]);
        if (srcList == targetList && targetIndex <= srcIndex) srcIndex++;
        srcList.splice(srcIndex, 1);
        // 
        return true;
    };

    $scope.UpdateList = function (items,parentId) {       
        var url = serviceBase + "api/PageMaster/UpdatePageSequence?parentPageid=" + parentId;
        var i = 1;
        angular.forEach(items, function (data) {
            data.Sequence = i;
            i++;
        });
        $http.post(url, items)
            .success(function (data) {
                if (data) {
                    alert("Page sequence updated successfully");
                    window.location.reload();
                }
                else {
                    alert("Some error occurred during update page sequence");
                }
            })
            .error(function (data) {
                alert("Some error occurred during update page sequence");
            });
    };

    $scope.Chaildlist = [];
    $scope.trade = { Id:"" };
    $scope.GetChaild = function (data) {      
        $scope.Chaildlist = [];
        var url = serviceBase + "api/PageMaster/GetAllChildPages?pageMasterId=" + data;
        $http.get(url)
            .success(function (response) {
                $scope.Chaildlist = response;
            });
    }; 
    
    $scope.GetUpdateParent = function (editdata) {
        
        $scope.Isupdate = true;
        $scope.data = editdata;
        if (!$scope.data.RouteName || $scope.data.RouteName == "NULL" || $scope.data.RouteName == "null")
            $scope.data.RouteName = "";
        if (!$scope.data.IconClassName || $scope.data.IconClassName == "NULL" || $scope.data.IconClassName == "null")
            $scope.data.IconClassName = "";
        if (!$scope.data.ClassName || $scope.data.ClassName == "NULL" || $scope.data.ClassName == "null")
            $scope.data.ClassName = "";     
        //var id = editdata.Id;
        //$scope.ParentEdit = {};
        //var url = serviceBase + "api/PageMaster/GetAllParentPagesforEdit?id=" + id;
        //$http.get(url)
        //    .success(function (response) {
        //        $scope.data = response;
        //    });
    };
    //$scope.GetUpdateParent();

    $scope.opendelete = function (editdata) {
        
        console.log(editdata);
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "myModaldelete.html",
                controller: "ModalInstanceCtrldelete", resolve: { datadelete: function () { return editdata; } }
            }), modalInstance.result.then(function (selectedItem) {
                
                //$scope.currentPageStores.splice($index, 1);
              },
                function () {
                    console.log("Cancel Condintion");
                });

          
    }; 

}]);

app.controller("ModalInstanceCtrldelete", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "datadelete", function ($scope, $http, ngAuthSettings, $modalInstance, datadelete) {
    
    $scope.data = datadelete;
   
    $scope.delete = function () {
        
        console.log("Edit Dialog called ");
        var url = serviceBase + "api/PageMaster/Remove?id=" + datadelete.Id;
        $http.post(url)
            .success(function (data) {
                
                if (data) {
                    alert("Page sequence Delete successfully");
                    window.location.reload();
                }
                else {
                    alert("Some error occurred during Delete page sequence");
                }
            })
            .error(function (data) {
                alert("Some error occurred during Delete page sequence");
            });
    };
    $scope.ok = function () { $modalInstance.close(); };
    $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
}]);

