'use strict';
app.controller('unitEcoController', ['$scope', "$filter", "$http", "ngTableParams", '$modal', 'WarehouseService', function ($scope, $filter, $http, ngTableParams, $modal,WarehouseService) {

    console.log("UE Controller reached");
    $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
    $scope.compid = $scope.UserRole.compid;
    $scope.Warehouseid = $scope.UserRole.Warehouseid;
    var input = document.getElementById("file");
    $scope.unitEconomics = {};
    $scope.datrange = '';
    $scope.datefrom = '';
    $scope.dateto = '';
    $scope.CompanyLabel = [];
    $(function () {
        $('input[name="daterange"]').daterangepicker({
            timePicker: true,
            timePickerIncrement: 5,
            timePicker24Hour: true,
            format: 'YYYY-MM-DD h:mm A'
        });
        $('.input-group-addon').click(function () {
            $('input[name="daterange"]').trigger("select");
            //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

        });
    });
    $scope.getFormatedDate = function (date) {
        console.log("here get for")
        date = new Date(date);
        var dd = date.getDate();
        var mm = date.getMonth() + 1; //January is 0!
        var yyyy = date.getFullYear();
        if (dd < 10) {
            dd = '0' + dd;
        }
        if (mm < 10) {
            mm = '0' + mm;
        }
        var date = yyyy + '-' + mm + '-' + dd;
        return date;
    };

    WarehouseService.getwarehouse().then(function (results) {
        $scope.warehouses = results.data;
    }, function (error) {
    });
    $scope.GetCompanyLabel = function () {

        $http.get(serviceBase + "api/uniteconomic/GetComapnyLabel")
            .success(function (data) {
                
                if (data != null) {
                    $scope.CompanyLabel = data;

                }
            });

    }
    $scope.GetCompanyLabel();
    $scope.vm = {
        rowsPerPage: 20,
        currentPage: 0,
        count: null,
        numberOfPages: null,
        WarehouseId: 0,
        LabelName: null,
        dateto: null,
        datefrom:null

    };

    $scope.onNumPerPageChange = function () {
        $scope.Getuniteconomic();
    };

    $scope.changePage = function (pagenumber) {
        setTimeout(function () {
            $scope.vm.currentPage = (pagenumber-1)  * $scope.vm.rowsPerPage;
            $scope.Getuniteconomic();
        }, 100);

    };
   
    $scope.Getuniteconomic = function () {

        $http.get(serviceBase + "api/uniteconomic?totalitem=" + $scope.vm.rowsPerPage + "&page=" + $scope.vm.currentPage + "&WarehouseId=" + $scope.vm.WarehouseId + "&LabelName=" + $scope.vm.LabelName + "&dateto=" + $scope.vm.dateto + "&datefrom=" + $scope.vm.datefrom )
            .success(function (data) {
                if (data != null) {
                    $scope.unitEconomics = data.unitEconomics;
                    $scope.vm.count = data.Count;
                    $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);
                }
            });

    }
    $scope.Getuniteconomic();

    $scope.Searchdata = function (data) {

        if (data.WarehouseId == null) {
            alert('Please Select Warehouse');
            return;
        }
        var f = $('input[name=daterangepicker_start]');
        var g = $('input[name=daterangepicker_end]');

        if (!$('#dat').val()) {
            $scope.vm.datefrom = '';
            $scope.vm.dateto = '';
        }
        else {
            $scope.vm.datefrom = f.val();
            $scope.vm.dateto = g.val();
        }      
        $scope.vm = {
            rowsPerPage: 20,
            currentPage: $scope.vm.currentPage,
            count: null,
            numberOfPages: null,
            WarehouseId: data.WarehouseId,
            LabelName: data.LabelName,
            dateto: $scope.vm.dateto,
            datefrom: $scope.vm.datefrom

        };
        $scope.Getuniteconomic();
    }

    ////.................File Uploader method start..................
    $scope.uploadshow = true;
    $scope.toggle = function () {
        $scope.uploadshow = !$scope.uploadshow;
    }
    function sendFileToServer(formData, status) {
        formData.append("WareHouseId", $scope.Warehouseid);
        formData.append("compid", $scope.compid);
        var uploadURL = "/api/UnitEconomicupload/post"; //Upload URL
        var extraData = {}; //Extra Data.
        var jqXHR = $.ajax({
            xhr: function () {
                var xhrobj = $.ajaxSettings.xhr();
                if (xhrobj.upload) {
                    xhrobj.upload.addEventListener('progress', function (event) {
                        var percent = 0;
                        var position = event.loaded || event.position;
                        var total = event.total;
                        if (event.lengthComputable) {
                            percent = Math.ceil(position / total * 100);
                        }
                        //Set progress
                        status.setProgress(percent);
                    }, false);
                }
                return xhrobj;
            },
            url: uploadURL,
            type: "POST",
            contentType: false,
            processData: false,
            cache: false,
            data: formData,
            success: function (data) {
                status.setProgress(100);
                $("#status1").append("Data from Server: " + data + "<br>");
                //alert("Succesfully Submitted...........");               
            },
        });
        status.setAbort(jqXHR);
    }


    var rowCount = 0;
    function createStatusbar(obj) {
        rowCount++;
        var row = "odd";
        if (rowCount % 2 == 0) row = "even";
        this.statusbar = $("<div class='statusbar " + row + "'></div>");
        this.filename = $("<div class='filename'></div>").appendTo(this.statusbar);
        this.size = $("<div class='filesize'></div>").appendTo(this.statusbar);
        this.progressBar = $("<div class='progressBar'><div></div></div>").appendTo(this.statusbar);
        this.abort = $("<div class='abort'>Abort</div>").appendTo(this.statusbar);
        obj.after(this.statusbar);
        this.setFileNameSize = function (name, size) {
            var sizeStr = "";
            var sizeKB = size / 1024;
            if (parseInt(sizeKB) > 1024) {
                var sizeMB = sizeKB / 1024;
                sizeStr = sizeMB.toFixed(2) + " MB";
            }
            else {
                sizeStr = sizeKB.toFixed(2) + " KB";
            }
            this.filename.html(name);
            this.size.html(sizeStr);
        }
        this.setProgress = function (progress) {
            var progressBarWidth = progress * this.progressBar.width() / 100;
            this.progressBar.find('div').animate({ width: progressBarWidth }, 10).html(progress + "%&nbsp;");
            if (parseInt(progress) >= 100) {
                this.abort.hide();
            }
        }
        this.setAbort = function (jqxhr) {
            var sb = this.statusbar;
            this.abort.click(function () {
                jqxhr.abort();
                sb.hide();
            });
        }
    }
    function handleFileUpload(files, obj) {
        for (var i = 0; i < files.length; i++) {
            var fd = new FormData();
            fd.append('file', files[i]);
            var status = new createStatusbar(obj); //Using this we can set progress.
            status.setFileNameSize(files[i].name, files[i].size);
            sendFileToServer(fd, status);
        }
    }
    $(document).ready(function () {
        var obj = $("#dragandrophandler");
        obj.on('dragenter', function (e) {
            e.stopPropagation();
            e.preventDefault();
            $(this).css('border', '2px solid #0B85A1');
        });
        obj.on('dragover', function (e) {
            e.stopPropagation();
            e.preventDefault();
        });
        obj.on('drop', function (e) {

            $(this).css('border', '2px dotted #0B85A1');
            e.preventDefault();
            var files = e.originalEvent.dataTransfer.files;
            handleFileUpload(files, obj);
        });
        $(document).on('dragenter', function (e) {
            e.stopPropagation();
            e.preventDefault();
        });
        $(document).on('dragover', function (e) {
            e.stopPropagation();
            e.preventDefault();
            obj.css('border', '2px dotted #0B85A1');
        });
        $(document).on('drop', function (e) {
            e.stopPropagation();
            e.preventDefault();
        });

    });

    //............................Exel export Method.....................//
    alasql.fn.myfmt = function (n) {
        return Number(n).toFixed(2);
    };

    $scope.exportData = function () {

        $http.get(serviceBase + "api/uniteconomic/GetExcel")
            .success(function (data) {
                if (data != null) {
                    $scope.unitEconomicsExcel = data;
                 
                }
            });
        $scope.stores = $scope.unitEconomicsExcel;
        alasql('SELECT unitId,Label1,Label2,Label3,WarehouseId,Amount,CreatedDate,Discription,[IsActive],[Deleted],CompanyLabel,CompanyId INTO XLSX("Unit Economic.xlsx",{headers:true}) FROM ?', [$scope.stores]);
    };

    $scope.open = function () {
        console.log("Modal opened");
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "myUEModal.html",
                controller: "unitEcoAddController", resolve: { object: function () { return $scope.items } }
            }), modalInstance.result.then(function (selectedItem) {
                $scope.unitEconomics.push(selectedItem);
            },
            function () {
                console.log("Cancel Condintion");             
            })
    };

    $scope.edit = function (item) {
        console.log("Edit Dialog");
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "myUEModal.html",
                controller: "unitEcoAddController", resolve: { object: function () { return item } }
            }), modalInstance.result.then(function (selectedItem) {
                $scope.unitEconomics.push(selectedItem);
            },
            function () {
                console.log("Cancel Condintion");               
            })
    };

    $scope.opendelete = function (data,$index) {
        console.log(data);
        console.log($index);
        console.log("Delete Dialog called for city");
       
        var myData = { all: $scope.currentPageStores ,city1:data};

       
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "myModaldeleteCity.html",
                controller: "ModalInstanceCtrldeleteCity", resolve: { city: function () { return myData } }
            }), modalInstance.result.then(function (selectedItem) {

                $scope.currentPageStores.splice($index,1);
            },
            function () {
                console.log("Cancel Condintion");
             
            })
        //$scope.city.splice($scope.city.indexOf($scope.city), 1)
       // $scope.city.splice($index, 1);
    };       
}]);

app.controller("unitEcoAddController", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", 'object', 'WarehouseService', function ($scope, $http, ngAuthSettings, $modalInstance, object, WarehouseService) {
    
    $scope.AddLabeldata = false;
    $scope.UnitEconomicData = {};
    $scope.label = {};
    if (object) {
        $scope.UnitEconomicData = object;
    }
    $scope.ShowLabelbox = function () {
        
        $scope.AddLabeldata = true;
    };
    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

    //    $scope.lablel1type = [
    //        { name: "Sales Exp" }, { name: "Logistic Exp" }, { name: "Operation Exp" }
    //    ]
    //$scope.lablel2type = [
    //    { name: "People" }, { name: "Marketing  & promo" }, { name: "Del Logistics" }, { name: "Pur logisticsp" }, { name: "Rent" }, { name: "Other exp" }
    //]
    //$scope.lablel3type = [
    //    { name: "Vehicle" }, { name: "People" }, { name: "Operation" }, { name: "Purchase" }, { name: "Helper" }
    //]

    WarehouseService.getwarehouse().then(function (results) {
        $scope.warehouses = results.data;
    }, function (error) {
    });
    $scope.AddUnitEconomic = function () {
        if ($scope.UnitEconomicData.CompanyLabel == null) {
            alert('Please select company label');
            return;
        } else if ($scope.UnitEconomicData.Amount == null) {
            alert('Please enter the amount');
            return;
        } else if ($scope.UnitEconomicData.Amount <= 0) {

            alert('Please enter the positive amount');
            return;
        }
        else if ($scope.UnitEconomicData.WarehouseId == null || parseInt($scope.UnitEconomicData.WarehouseId) <= 0) {
            alert('Please select Warehouse');
            return;
        } else {
            console.log("Add UnitEconomic");
            var url = serviceBase + "api/uniteconomic";
            var dataToPost = {
                unitId: $scope.UnitEconomicData.unitId,
                WarehouseId: $scope.UnitEconomicData.WarehouseId,
                Label1: $scope.UnitEconomicData.Label1,
                Label2: $scope.UnitEconomicData.Label2,
                Label3: $scope.UnitEconomicData.Label3,
                Amount: $scope.UnitEconomicData.Amount,
                Discription: $scope.UnitEconomicData.Discription,
                CompanyLabel: $scope.UnitEconomicData.CompanyLabel,
                ExpenseDate: $scope.UnitEconomicData.ExpenseDate
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
                        alert('Add  SuccessFully');
                        $modalInstance.close(data);
                    }
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                })
        }
    };
    $scope.Addlabel = function (label) {
        if (label.LabelType > 0) {
            var url = serviceBase + "api/uniteconomic/AddLabel";
            var dataToPost = {
                LabelName: label.Label,
                LabelType: label.LabelType

            };
            $http.post(url, dataToPost)
                .success(function (data) {
                    if (data != null) {
                        alert('Add Successfully');
                        $scope.cancellabel();
                        $scope.GetLabelEntry();
                    }
                });
        } else {
            alert('Please Select Label Type');
        }
    };

    $scope.cancellabel = function () {

        $scope.label = null;
        $scope.AddLabeldata = false;
    };
    
    $scope.GetLabelEntry = function () {
        $http.get(serviceBase + "api/uniteconomic/GetLabel")
            .success(function (data) {
                if (data!= null) {
                    $scope.lablel1type = data.label1;
                    $scope.lablel2type = data.label2;
                    $scope.lablel3type = data.label3;
                    $scope.companylabel = data.companyLabel;
                }
            });
    };
    $scope.GetLabelEntry();

}]);