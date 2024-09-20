
(function () {

    'use strict';

    angular
        .module('app')
        .controller('TreeStructureController', TreeStructureController);
    TreeStructureController.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal'];

    function TreeStructureController($scope, $filter, $http, ngTableParams, $modal) {

        window.onload = function () {
            //OrgChart.templates.ana.field_0 = '<text class="field_0"  style="font-size: 25px;" fill="#ffffff" x="125" y="30" text-anchor="middle">{val}</text>';
            //OrgChart.templates.ana.field_1 = '<text class="field_1"  style="font-size: 14px;" fill="#ffffff" x="125" y="50" text-anchor="middle">{val}</text>';
            OrgChart.templates.ana.field_2 = '<text class="field_2"  style="font-size: 16px;" fill="#ffffff" x="150" y="60" text-anchor="middle">{val}</text>';
            OrgChart.templates.ana.field_4 = '<text class="field_4"  style="font-size: 14px;" fill="#ffffff" x="180" y="20" text-anchor="right">{val}</text>';

            function pdf(nodeId) {
                chart.exportPDF({ filename: "MyFileName.pdf", expandChildren: true, nodeId: nodeId });
            }

            var chart = new OrgChart(document.getElementById("tree"), {
                enableDragDrop: true,
                nodeMouseClick: OrgChart.action.edit,
             
                nodeMenu: {
                    
                    details: { text: "Details" },
                    edit: { text: "Edit" },
                    add: { text: "Add" },
                    remove: { text: "Remove" }
                },

                menu: {
                    export_pdf: {
                        text: "Export PDF",
                        icon: OrgChart.icon.pdf(24, 24, "#7A7A7A"),
                        onClick: pdf
                    },               
                },
                nodeBinding: {
                    field_0: "name",
                   // field_1: "title",
                    field_2: "Designation",
                    //field_3: "Department",
                    field_4: "EmployeeLevel",
                    img_0: "img"
                },

            });
            
            LoadChart();
            // window.reload();

            function LoadChart() {
                
                var url = serviceBase + "/api/TreeStructure/getData";
                $http.get(url)
                    .success(function (data) {
                        
                        chart.load(data);
                        console.log(data,'data');
                    }).error(function (data) {
                    });


            }


            chart.on('update', function (sender, oldNode, newNode, data) {
                
                console.log("Nenode is", newNode);
                var url = serviceBase + "/api/TreeStructure/savedata";
                
                //location.reload();
                //  window.location.reload();
                $http.post(url, newNode)
                    .success(function (data) {
                        chart.load(data)
                        //  location.reload();
                        LoadChart();
                        window.location.reload();
                    }).error(function (data) {
                    });
            });

            $scope.nodes = [];
            chart.on('remove', function (data, nodeId) {
                

                // LoadChart();
                //$scope.nodes = data.nodes;
                //if ($scope.nodes == nodeId) {
                //    alert("gfreg");
                //}
                //else {
                //    for (var i = 0; i < $scope.nodes.length; i++) {
                //        if ($scope.nodes[i].id == nodeId) {
                //            if ($scope.nodes[i].children.length == 0) {
                //                alert("Please enter child");
                //            }
                //        }
                //    }
                //}

                angular.forEach(data.nodes, function (value, key) {
                    
                    if (value.id == nodeId) {
                        if (value.pid == value.id) {
                            alert("Firstly Delete Children then delete its Parent");
                        } else {
                            var url = serviceBase + "/api/TreeStructure/Delete?id=" + nodeId;
                            $http.get(url)
                                .success(function (data) {
                                    chart.load(data);
                                }).error(function (data) {
                                });
                        }

                    }

                });






            });


            chart.editUI.on('imageuploaded', function (sender, file, inputHtmlElement) {
                var formData = new FormData();
                formData.append('file', file);
                var url = serviceBase + "/api/TreeStructure/UploadOrganizationImage";
                $.ajax({
                    type: "POST",
                    url: url,
                    data: formData,
                    dataType: 'json',
                    contentType: false,
                    processData: false,
                    success: function (data) {
                        
                        if (serviceBase != null && serviceBase.length > 0) {
                            
                            serviceBase = serviceBase.substring(0, serviceBase.length - 1);
                        }
                        inputHtmlElement.value = serviceBase + "" + data;
                        //window.location.reload();
                    },
                    error: function (error) {
                        alert(error);
                    }
                });
            });
        };
    }
})();
