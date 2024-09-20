

(function () {
    'use strict';

    angular
        .module('app')
        .controller('AddPeopleCtrl', AddPeopleCtrl);

    AddPeopleCtrl.$inject = ['$scope', "$rootScope", "designationservice", "DepartmentService", "$filter", "$http", "ngTableParams", 'ngAuthSettings', "peoplesService", 'CityService', 'StateService', 'WarehouseService', 'FileUploader', 'authService', 'ClusterService'];

    function AddPeopleCtrl($scope, $rootScope, designationservice, DepartmentService, $filter, $http, ngTableParams, ngAuthSettings, peoplesService, CityService, StateService, WarehouseService, FileUploader, authService, ClusterService) {
       
    $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));


    console.log("People");

        window.onload = function () {

            var name = localStorage.getItem("People");
            if (name !== null) $('#inputName').val("People");

            // ...
        };

        

    $scope.PeopleData = {}; var alrt = {};
    $scope.showform = true;
    $scope.showform1 = false;
    $scope.showform2 = false;

    $scope.getRole = function () {

        var url = serviceBase + "api/usersroles/GetAllRoles";
        $http.get(url)
            .success(function (data) {

                $scope.roles = data;
                console.log(data);
            }, function (error) { });
    }
        $scope.getRole();

        $scope.warehouse = {};
        // function for get Warehosues from warehouse service
        $scope.getWarehosues = function () {
            WarehouseService.getwarehousewokpp().then(function (results) {
                
                if (results.data.length > 0) {
                    for (var a = 0; a < results.data.length; a++) {
                        results.data[a].WarehouseName = results.data[a].WarehouseName + " " + results.data[a].CityName;
                    }
                    $scope.warehouse = results.data;
                }
            }, function (error) {
            });
        };
        $scope.getWarehosues();

    //$scope.citys = [];
    //CityService.getByStateID(Stateid).then(function (results) {
    //    $scope.citys = results.data;
    //}, function (error) {
    //    });
    $scope.SelectFilterWh = function (data) {
        $scope.Citystatebased(data); // call city based area if add customer      
    };
    $scope.SelectFilterST = function (data) {
        $scope.warehousecitybased(data); // call city based area if add customer      
    };

        $scope.examplemodel = [];
        $scope.exampledata = $scope.warehouse;
        $scope.examplesettings = {
            displayProp: 'WarehouseName', idProp: 'WarehouseId',
            scrollableHeight: '300px',
            scrollableWidth: '450px',
            enableSearch: true,
            scrollable: true
        };

    $scope.citys = [];
    $scope.Citystatebased = function (Stateid) {
        CityService.getByStateID(Stateid).then(function (results) {
            $scope.citys = results.data;
        }, function (error) {
        });
    };

    $scope.warehouse = [];
    $scope.warehousecitybased = function (Cityid) {
        WarehouseService.warehousecitybased(Cityid).then(function (results) {
            $scope.warehouse = results.data;
        }, function (error) {
        });
    };

    //$scope.warehouse = [];
    //WarehouseService.getwarehouse().then(function (results) {
    //    $scope.warehouse = results.data;
    //}, function (error) {
    //});

    $scope.Designation = [];
    designationservice.getdesignations().then(function (results) {
        $scope.Designation = results.data;
    }, function (error) {
    });

    $scope.Cluster = [];
    ClusterService.getcluster().then(function (results) {

        $scope.Cluster = results.data;


    }, function (error) {

    });

    //............Multiselection Cluster..........//

    $scope.clusterModel = [];

    $scope.clusterModel = $scope.Cluster;
    $scope.clusterSetting = {
        displayProp: 'ClusterName', idProp: 'ClusterId',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };


    $scope.Departments = [];
    DepartmentService.getdepartments().then(function (results) {
        $scope.Departments = results.data;
    }, function (error) {
    });
    $scope.states = [];
    StateService.getstates().then(function (results) {

        $scope.states = results.data;
    }, function (error) {
    });

    //for validation

    $scope.CheckEmail = function (Email) {
    
        var url = serviceBase + "api/Peoples/Email?Email=" + Email;
        $http.get(url)
            .success(function (data) {
                if (data == null) {

                }
                else {

                    alert("Email Is already Exist");
                    //$scope.PeopleData.Email = '';
                }
                console.log(data);
            });
    }
    $scope.Salary_Calculation = function (data) {

        $scope.PeopleData.Salary.Hra_Salary = (parseInt(data) * 40) / 100;
        $scope.PeopleData.Salary.Lta_Salary = 0;
        $scope.PeopleData.Salary.CA_Salary = 0;
        $scope.PeopleData.Salary.DA_Salary = 0;
        $scope.PeopleData.Salary.PF_Salary = (((parseInt(data) * 75) / 100) * 12) / 100;
        $scope.PeopleData.Salary.ESI_Salary = (parseInt(data) * 6.5) / 100;
        $scope.PeopleData.Salary.B_Salary = 0;
        $scope.PeopleData.Salary.Y_Incentive = 0;
        $scope.PeopleData.Salary.M_Incentive = 0;

    }
    $scope.CheckMobile = function (Mobile) {
        var url = serviceBase + "api/Peoples/Mobile?Mobile=" + Mobile;
        $http.get(url)
            .success(function (data) {
                if (data == null) {
                }
                else {
                    alert("Mobile Number Is already Exist");

                }
                console.log(data);
            });
    }
    //Validation Form Details
    $scope.DetailsForm = function () {
        $scope.showform = true;
        $scope.showform1 = false;
        $scope.showform2 = false;
    }
    //Validation Form Salary
    $scope.SalaryForm = function () {
        $scope.showform = false;
        $scope.showform1 = true;
        $scope.showform2 = false;
    }
    //Validation Form Document
    $scope.DocumentForm = function () {
        $scope.showform = false;
        $scope.showform1 = false;
        $scope.showform2 = true;
    }

    $scope.dataToPost = [];
    $scope.dataToPUt = [];

    $scope.AddPeople = function (PeopleData) {
        //
        //var regx = /^ (?=.*[a - z])(?=.*[A - Z])(?=.*\d)(?=.*[@$!%*?&])[A - Za - z\d@$!%*?&]{ 8,}$/;
       // var strongRegex = new RegExp("^ (?=.*[a - z])(?=.*[A - Z])(?=.*\d)(?=.*[@$!%*?&])[A - Za - z\d@$!%*?&]{ 8,}$");
        
        //var abs = $('#Mobile').val().length;
        var ids = [];
        _.each($scope.clusterModel, function (o2) {
            console.log(o2);
            for (var i = 0; i < $scope.Cluster.length; i++) {
                if ($scope.Cluster[i].ClusterId == o2.id) {
                    var Row =
                    {
                        "id": o2.id
                    };
                    ids.push(Row);
                }
            }
        });

        var wids = [];
        var selectedWarehouse = [];
        $scope.SelectedWarehouse = [];
        _.each($scope.examplemodel, function (o2) {
            wids.push(o2.id);
        });

        var regexPasswordLength = /.{8,}/; // test for at least 8 characters
        // enter your regular expression to check for an uppercase letter here
        var regexPasswordContainsUpperCase = /[A-Z]/; //test for uppercase letter
        // enter your regular expression to check for a lowercase letter here
        var regexPasswordContainsLowerCase = /[a-z]/; //test for lowercase letter
        // enter your regular expression to check for a number here
        var regexPasswordContainsNumber = /\d/; //test for number 
        // enter your regular expression to check for a special character here
        var regexPasswordContainsSpecialChar = /\W/; //test for special character
        var bPasswordPasses = true;
        if (!regexPasswordLength.test(PeopleData.people.Password)) {
            alert('Password must be at least 8 characters.');
            bPasswordPasses = false;
        }
        if (!regexPasswordContainsUpperCase.test(PeopleData.people.Password)) {
            alert('Password must contain an uppercase character.');
            bPasswordPasses = false;
        }
        if (!regexPasswordContainsLowerCase.test(PeopleData.people.Password)) {
            alert('Password must contain an lowercase character.');
            bPasswordPasses = false;
        }
        if (!regexPasswordContainsNumber.test(PeopleData.people.Password)) {
            alert('Password must contain a number.');
            bPasswordPasses = false;
        }
        if (!regexPasswordContainsSpecialChar.test(PeopleData.people.Password)) {
            alert('Password must contain a special character.');
            bPasswordPasses = false;
        }

        if (PeopleData.people == null || PeopleData.people == "") {
            $scope.DetailsForm();
            alert('Please Enter Details');
        }
        else if (PeopleData.people.PeopleFirstName == null || PeopleData.people.PeopleFirstName == "") {
            $scope.DetailsForm();
            alert('Please Enter First Name');
        }
        else if (PeopleData.people.PeopleLastName == null || PeopleData.people.PeopleLastName == "") {
            $scope.DetailsForm();
            alert('Please Enter Last Name');
        }
        else if (PeopleData.people.Email == null || PeopleData.people.Email == "") {
            $scope.DetailsForm();
            alert('Please Enter Email');
        }
        else if (PeopleData.people.Password == null || PeopleData.people.Password == "") {
            $scope.DetailsForm();
            alert('Please Enter Password');
        }
        else if (bPasswordPasses == false) {
            $scope.DetailsForm();
            alert('Password between 8 and 20 characters; must contain at least one lowercase letter, one uppercase letter, one numeric digit, and one special character, but cannot contain whitespace');
        }
        else if (PeopleData.people.Mobile == null || PeopleData.people.Mobile == "") {
            $scope.DetailsForm();
            alert('Please Enter Mobile');
        }
        else if (PeopleData.people.Stateid == "") {
            $scope.DetailsForm();
            alert('Please Enter State');
        }
        else if (PeopleData.people.Cityid == "") {
            $scope.DetailsForm();
            alert('Please Enter City');
        }
        else if (PeopleData.people.WarehouseId == "") {
            $scope.DetailsForm();
            alert('Please Enter Warehouse');
        }
        else if (PeopleData.people.Department == "") {
            $scope.DetailsForm();
            alert('Please Enter Department');
        }        
       //else if (data.Mobile == null) {
        //    alert('Please Enter Mobile Number ');
        //}
        //else if (abs < 10 || abs > 10) {
        //    alert('Please Enter Mobile Number in 10 digit ');
        //}
        //else if (PeopleData.Salary.Salary == null || PeopleData.Salary.Salary == "" || PeopleData.Salary.Salary == undefined) {
        //    $scope.DetailsForm();
        //    alert('Please Enter Salary');
        //}
        else {


            $scope.PeopleData.people.id = wids;

            authService.saveRegistrationpeople($scope.PeopleData.people).then(function (response) {
                alert("Record Successfully Saved");
                alert("Record Successfully Saved");
                //
                //var wpdata = {
                //    peopleid: response.config.data.PeopleID,
                //    warehouse: wids
                //};

                //var url = serviceBase + "api/Peoples/PeopleWarehousePermission";
                //$http.post(url, wpdata).success(function (results) {
                //    console.log("Warehouse permission created");
                //});
                var datatopost = {
                    Mobile: response.config.data.Mobile,
                    PeopleID: response.config.data.PeopleID,
                    Active: response.config.data.Active,
                    Stateid: response.config.data.Stateid,
                    state: response.config.data.state,
                    Cityid: response.config.data.Cityid,
                    city: response.config.data.city,
                    //Mobile: $scope.PeopleData.Mobile,
                    PeopleFirstName: response.config.data.PeopleFirstName,
                    PeopleLastName: response.config.data.PeopleLastName,
                    WarehouseId: response.config.data.WarehouseId,
                    CreatedDate: response.config.data.CreatedDate,
                    UpdatedDate: response.config.data.UpdatedDate,
                    CreatedBy: response.config.data.CreatedBy,
                    UpdateBy: response.config.data.UpdateBy,
                    Email: response.config.data.Email,
                    Password: response.config.data.Password,
                    Department: response.config.data.Department,
                    Desgination: response.config.data.Desgination,
                    BillableRate: response.config.data.BillableRate,
                    CostRate: response.config.data.CostRate,
                    Permissions: response.config.data.Permissions,
                    Skcode: response.config.data.Skcode,
                    Type: response.config.data.Department,
                    Salesexecutivetype: response.config.data.Salesexecutivetype,
                    AgentCode: response.config.data.AgentCode,
                    DOB: response.config.data.DOB,
                    Status: response.config.data.Status,
                    DataOfMarriage: response.config.data.DataOfMarriage,
                    EndDate: response.config.data.EndDate,
                    DataOfJoin: response.config.data.DataOfJoin,
                    Unit: response.config.data.Unit,
                    Reporting: response.config.data.Reporting,
                    DepositAmount: response.config.data.DepositAmount

                };

                //$scope.clustdata = function () {

                //    var url = serviceBase + "api/Peoples";
                //    $http.post(url, datatopost).success(function (results) {
                //        alert("Record Successfully Saved");
                //        // window.location.reload();
                //    });
                //};
                //$scope.clustdata();
                $scope.dataToPUt = datatopost;
                $scope.page = response.config.data.Department;
                if (response.status == "200") {
                    //
                    //$scope.AddData = function () {

                    //    var url = serviceBase + "api/trackuser?action=AddPeople&item=" + $scope.page;
                    //    $http.post(url).success(function (results) {

                    //    });
                    //}

                    //$scope.AddData();




                    $scope.AddSalary = function () {
                        var url = serviceBase + "api/Peoples/AddSalary?name=" + PeopleData.people.PeopleFirstName + "&mobile=" + PeopleData.people.Mobile;
                        $http.post(url, PeopleData.Salary).success(function (results) {

                            $scope.Addimage = function () {

                                var url = serviceBase + "api/Peoples/AddImage?name=" + PeopleData.people.PeopleFirstName + "&mobile=" + PeopleData.people.Mobile;
                                $http.post(url, PeopleData).success(function (results) {
                                    alert("Record Inserted Successfull");
                                    window.location.reload();

                                });
                            }
                            $scope.Addimage();
                        });
                    }
                    $scope.AddSalary();
                }
            });
        }
    };

    //pUT People

    $scope.PplID = [];

    $scope.PutPeople = function (data) {

        var a = $scope.dataToPUt;
        var dataToPost = {};
        var Murl = serviceBase + "api/Peoples/Getpeople?Mobile=" + a.Mobile;
        $http.get(Murl).success(function (results) {
            $scope.PplID = results;

            if (data.Salary.Salary == null || data.Salary.Salary == "" || data.Salary.Salary == undefined) {
                alert('Please Enter Salary');
            }
            $scope.PeopleData = data.Salary;
            var idproff = data.Id_Proof;
            var Addprof = data.Address_Proof;
            var marksheet = data.MarkSheet;
            var salary = data.Pre_SalarySlip;
            var DisplayName = data.people.PeopleFirstName;
            //var displayName = data.displayName;
            var url = serviceBase + "api/Peoples/Putnew";

            dataToPost = {
                PeopleID: $scope.PplID,
                PeopleFirstName: DisplayName,
                Salary: $scope.PeopleData.Salary,
                B_Salary: $scope.PeopleData.B_Salary,
                Hra_Salary: $scope.PeopleData.Hra_Salary,
                CA_Salary: $scope.PeopleData.CA_Salary,
                DA_Salary: $scope.PeopleData.DA_Salary,
                Lta_Salary: $scope.PeopleData.Lta_Salary,
                PF_Salary: $scope.PeopleData.PF_Salary,
                ESI_Salary: $scope.PeopleData.ESI_Salary,
                M_Incentive: $scope.PeopleData.M_Incentive,
                Y_Incentive : $scope.PeopleData.Y_Incentive,
                Id_Proof: idproff,
                Address_Proof: Addprof,
                MarkSheet: marksheet,
                Pre_SalarySlip: salary
                
            };
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
                        console.log = "Record has been Update successfully";
                        window.location.reload();


                    }
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                })

        });

        
        console.log(dataToPost);
    

    };

var uploader = $scope.uploader = new FileUploader({
    
    url: serviceBase + 'api/logoUpload/UploadPeopleDocument'
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
    $scope.PeopleData.people.uploadedfileName = fileItem._file.name;
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
    $scope.PeopleData.Id_Proof = fileItem._file.name;
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
    alert("Image Uploaded Successfully");
};
uploader.onCompleteAll = function () {
    console.info('onCompleteAll');
};

console.info('uploader', uploader);

//Second Img
var uploader1 = $scope.uploader1 = new FileUploader({

    url: serviceBase + 'api/logoUpload/UploadPeopleDocument'
});
//FILTERS

uploader1.filters.push({

    name: 'customFilter',
    fn: function (item /*{File|FileLikeObject}*/, options) {
        return this.queue.length < 10;
    }
});

//CALLBACKS

uploader1.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
    console.info('onWhenAddingFileFailed', item, filter, options);

};
uploader1.onAfterAddingFile = function (fileItem) {
    console.info('onAfterAddingFile', fileItem);


};
uploader1.onAfterAddingAll = function (addedFileItems) {
    console.info('onAfterAddingAll', addedFileItems);

};
uploader1.onBeforeUploadItem = function (item) {
    console.info('onBeforeUploadItem', item);

};
uploader1.onProgressItem = function (fileItem, progress) {
    console.info('onProgressItem', fileItem, progress);


};
uploader1.onProgressAll = function (progress) {
    console.info('onProgressAll', progress);



};
uploader1.onSuccessItem = function (fileItem, response, status, headers) {
    console.info('onSuccessItem', fileItem, response, status, headers);

    $scope.PeopleData.Address_Proof =  fileItem._file.name;

};
uploader1.onErrorItem = function (fileItem, response, status, headers) {
    console.info('onErrorItem', fileItem, response, status, headers);

};
uploader1.onCancelItem = function (fileItem, response, status, headers) {
    console.info('onCancelItem', fileItem, response, status, headers);


};
uploader1.onCompleteItem = function (fileItem, response, status, headers) {
    console.info('onCompleteItem', fileItem, response, status, headers);
    console.log("File Name :" + fileItem._file.name);
    $scope.uploadedfileName = fileItem._file.name;
    alert("Image Uploaded Successfully");

};
uploader1.onCompleteAll = function () {
    console.info('onCompleteAll');

};

console.info('uploader', uploader);

//Third Img
var uploader2 = $scope.uploader2 = new FileUploader({

    url: serviceBase + 'api/logoUpload/UploadPeopleDocument'
});
//FILTERS

uploader2.filters.push({

    name: 'customFilter',
    fn: function (item /*{File|FileLikeObject}*/, options) {
        return this.queue.length < 10;
    }
});

//CALLBACKS

uploader2.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
    console.info('onWhenAddingFileFailed', item, filter, options);
};
uploader2.onAfterAddingFile = function (fileItem) {
    console.info('onAfterAddingFile', fileItem);
    $scope.PeopleData.people.uploadedfileName = fileItem._file.name;
};
uploader2.onAfterAddingAll = function (addedFileItems) {
    console.info('onAfterAddingAll', addedFileItems);
};
uploader2.onBeforeUploadItem = function (item) {
    console.info('onBeforeUploadItem', item);
};
uploader2.onProgressItem = function (fileItem, progress) {
    console.info('onProgressItem', fileItem, progress);
};
uploader2.onProgressAll = function (progress) {
    console.info('onProgressAll', progress);
};
uploader2.onSuccessItem = function (fileItem, response, status, headers) {
    console.info('onSuccessItem', fileItem, response, status, headers);

    $scope.PeopleData.MarkSheet = fileItem._file.name;

};

uploader2.onErrorItem = function (fileItem, response, status, headers) {
    console.info('onErrorItem', fileItem, response, status, headers);
};
uploader2.onCancelItem = function (fileItem, response, status, headers) {
    console.info('onCancelItem', fileItem, response, status, headers);
};
uploader2.onCompleteItem = function (fileItem, response, status, headers) {
    console.info('onCompleteItem', fileItem, response, status, headers);
    console.log("File Name :" + fileItem._file.name);
    $scope.uploadedfileName = fileItem._file.name;
    alert("Image Uploaded Successfully");
};
uploader2.onCompleteAll = function () {
    console.info('onCompleteAll');
};

console.info('uploader', uploader);

//Fourth IMG
var uploader3 = $scope.uploader3 = new FileUploader({

    url: serviceBase + 'api/logoUpload/UploadPeopleDocument'

});
//FILTERS

uploader3.filters.push({

    name: 'customFilter',
    fn: function (item /*{File|FileLikeObject}*/, options) {
        return this.queue.length < 10;
    }
});

//CALLBACKS

uploader3.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
    console.info('onWhenAddingFileFailed', item, filter, options);
};
uploader3.onAfterAddingFile = function (fileItem) {
    console.info('onAfterAddingFile', fileItem);
    $scope.PeopleData.people.uploadedfileName = fileItem._file.name;
};
uploader3.onAfterAddingAll = function (addedFileItems) {
    console.info('onAfterAddingAll', addedFileItems);
};
uploader3.onBeforeUploadItem = function (item) {
    console.info('onBeforeUploadItem', item);
};
uploader3.onProgressItem = function (fileItem, progress) {
    console.info('onProgressItem', fileItem, progress);
};
uploader3.onProgressAll = function (progress) {
    console.info('onProgressAll', progress);
};
uploader3.onSuccessItem = function (fileItem, response, status, headers) {
    console.info('onSuccessItem', fileItem, response, status, headers);
    $scope.PeopleData.Pre_SalarySlip = fileItem._file.name;
};
uploader3.onErrorItem = function (fileItem, response, status, headers) {
    console.info('onErrorItem', fileItem, response, status, headers);
};
uploader3.onCancelItem = function (fileItem, response, status, headers) {
    console.info('onCancelItem', fileItem, response, status, headers);
};
uploader3.onCompleteItem = function (fileItem, response, status, headers) {
    console.info('onCompleteItem', fileItem, response, status, headers);
    console.log("File Name :" + fileItem._file.name);
    $scope.uploadedfileName = fileItem._file.name;
    alert("Image Uploaded Successfully");
};
uploader3.onCompleteAll = function () {
    console.info('onCompleteAll');
};

console.info('uploader', uploader);

    ////end
    }
})();