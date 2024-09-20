var app = angular.module("app", ["ngRoute", "ngInputDate", "ngAnimate", "ngAutocomplete", 'angularjs-dropdown-multiselect', 'angular-loading-bar', 'ngTableExport', 'LocalStorageModule', "ui.bootstrap", 'daterangepicker', 'ngTable', 'angularFileUpload', "easypiechart", "mgo-angular-wizard", "textAngular", "app.ui.ctrls", "app.ui.directives", "app.ui.services", "app.controllers", "app.directives", "app.form.validation", "app.ui.form.ctrls", "app.ui.form.directives", "app.tables", "app.task", "app.localization", "app.chart.ctrls", "app.chart.directives", 'angularUtils.directives.dirPagination', 'ngBootbox', 'dndLists', 'ngFileUpload'])
    .directive('showHistory', function () {
        return {
            controller: function ($rootScope, $scope, $http, $modal) {

                var entityId = $scope.this.$parent.Id;
                var entityName = $scope.this.$parent.entity;

                var url = serviceBase + "api/History?entityName=" + entityName + "&entityId=" + entityId;
                $http.get(url).then(function (results) {
                    if (results.data != "null") {
                        var modalInstance;
                        modalInstance = $modal.open(
                            {
                                size: 'lg',
                                //windowClass: 'my-modal-popup',
                                templateUrl: "myModalhistory.html",
                                controller: "HistoryCtrl", resolve: { obj: function () { return results.data } }
                            })

                        modalInstance.result.then(function () {
                            // called when you close the modal
                            // do your stuff

                            $scope.this.$parent.showMe = false;
                        }, function () {
                            // called when you dismiss modal by clicking anywhere outside modal
                            // do your stuff
                            $scope.this.$parent.showMe = false;
                        });
                    }
                });

                console.log($scope.this.$parent);
            },
        };
    })
    .directive('showLimitHistory', function () {
        return {
            controller: function ($rootScope, $scope, $http, $modal) {
                var entityId = $scope.this.$parent.LimitId;
                var entityName = $scope.this.$parent.entityName;

                var url = serviceBase + "api/History?entityName=" + entityName + "&entityId=" + entityId;
                $http.get(url).then(function (results) {
                    if (results.data != "null") {
                        var modalInstance;
                        modalInstance = $modal.open(
                            {
                                size: 'lg',
                                //windowClass: 'my-modal-popup',
                                templateUrl: "myModalhistory.html",
                                controller: "HistoryCtrl", resolve: { obj: function () { return results.data } }
                            })

                        modalInstance.result.then(function () {
                            // called when you close the modal
                            // do your stuff
                            $scope.this.$parent.showMe1 = false;
                        }, function () {
                            // called when you dismiss modal by clicking anywhere outside modal
                            // do your stuff
                            $scope.this.$parent.showMe1 = false;
                        });
                    }
                });

                console.log($scope.this.$parent);
            },
        };
    })
    .directive('autocomplete', ['autocomplete-keys', '$window', '$timeout', function (Keys, $window, $timeout) {
        var template =
            '<input type="text" class="autocomplete-input" placeholder="{{placeHolder}}"' +
            'ng-class="inputClass"' +
            'ng-model="searchTerm"' +
            'ng-keydown="keyDown($event)"' +
            'ng-keypress="keyPress($event)"' +
            'ng-blur="onBlur()"' +
            'ng-readonly="ngReadonly" />' +
            '<div class="autocomplete-options-container">' +
            '<div class="autocomplete-options-dropdown" ng-if="showOptions">' +
            '<div class="autocomplete-option" ng-if="!hasMatches">' +
            '<span>No matches</span>' +
            '</div>' +
            '<ul class="autocomplete-options-list">' +
            '<li class="autocomplete-option" ng-class="{selected: isOptionSelected(option)}" ' +
            'ng-style="{width: optionWidth}"' +
            'ng-repeat="option in matchingOptions"' +
            'ng-mouseenter="onOptionHover(option)"' +
            'ng-mousedown="selectOption(option)"' +
            'ng-if="!noMatches">' +
            '<span>{{option[displayProperty]}}</span>' +
            '</li>' +
            '</ul>' +
            '</div>' +
            '</div>';
        return {
            template: template,
            restrict: 'E',
            scope: {
                searchTerm: '=?ngModel',
                options: '=',
                onSelect: '=',
                ngReadonly: '=',
                displayProperty: '@',
                inputClass: '@',
                clearInput: '@',
                placeHolder: '@'
            },
            controller: function ($scope) {
                $scope.highlightedOption = null;
                $scope.showOptions = false;
                $scope.matchingOptions = [];
                $scope.hasMatches = false;
                $scope.selectedOption = null;

                $scope.isOptionSelected = function (option) {
                    return option === $scope.highlightedOption;
                };

                $scope.processSearchTerm = function (term) {
                    // console.log('ch-ch-ch-changin');
                    if (term.length > 0) {
                        if ($scope.selectedOption) {
                            if (term != $scope.selectedOption[$scope.displayProperty]) {
                                $scope.selectedOption = null;
                            } else {
                                $scope.closeAndClear();
                                return;
                            }
                        }

                        var matchingOptions = $scope.findMatchingOptions(term);
                        $scope.matchingOptions = matchingOptions;
                        if (!$scope.matchingOptions.indexOf($scope.highlightedOption) != -1) {
                            $scope.clearHighlight();
                        }
                        $scope.hasMatches = matchingOptions.length > 0;
                        $scope.showOptions = true;
                        $scope.setOptionWidth();
                    } else {
                        $scope.closeAndClear();
                    }
                };

                $scope.findMatchingOptions = function (term) {
                    if (!$scope.options) {
                        throw 'You must define a list of options for the autocomplete ' +
                        'or it took too long to load';
                    }
                    return $scope.options.filter(function (option) {
                        var searchProperty = option[$scope.displayProperty];
                        if (searchProperty) {
                            var lowerCaseOption = searchProperty.toLowerCase();
                            var lowerCaseTerm = term.toLowerCase();
                            return lowerCaseOption.indexOf(lowerCaseTerm) != -1;
                        }
                        return false;
                    });
                };

                $scope.findExactMatchingOptions = function (term) {
                    return $scope.options.filter(function (option) {
                        var lowerCaseOption = option[$scope.displayProperty].toLowerCase();
                        var lowerCaseTerm = term ? term.toLowerCase() : '';
                        return lowerCaseOption == lowerCaseTerm;
                    });
                };

                //$scope.keyDown = function (e) {
                //    switch (e.which) {
                //        case Keys.upArrow:
                //            e.preventDefault();
                //            if ($scope.showOptions) {
                //                $scope.highlightPrevious();
                //            }
                //            break;
                //        case Keys.downArrow:
                //            e.preventDefault();
                //            if ($scope.showOptions) {
                //                $scope.highlightNext();
                //            } else {
                //                $scope.showOptions = true;
                //                if ($scope.selectedOption) {
                //                    $scope.highlightedOption = $scope.selectedOption;
                //                }
                //            }
                //            break;
                //            case Keys.enter:
                //            e.preventDefault();
                //            case Keys.tab:
                //             if ($scope.highlightedOption) {
                //                $scope.selectOption($scope.highlightedOption);
                //            } else {
                //                var exactMatches = $scope.findExactMatchingOptions($scope.searchTerm);
                //                if (exactMatches[0]) {
                //                    $scope.selectOption(exactMatches[0]);
                //                }
                //            }
                //            break;
                //           case Keys.escape:
                //            $scope.closeAndClear();
                //            break;
                //    }
                //};

                $scope.keyPress = function (e) {
                    switch (e.which) {
                        case Keys.upArrow:
                        case Keys.downArrow:
                        case Keys.enter:
                        case Keys.escape:
                            break;
                        default:
                            $timeout(function () { $scope.processSearchTerm($scope.searchTerm); });
                            break;
                    }
                };

                //$scope.$watch('searchTerm', function(term, oldTerm) {
                //    if (term && term !== oldTerm) {
                //        $scope.processSearchTerm(term);
                //    }
                //});

                $scope.highlightNext = function () {
                    if (!$scope.highlightedOption) {
                        $scope.highlightedOption = $scope.matchingOptions[0];
                    } else {
                        var currentIndex = $scope.getCurrentOptionIndex();
                        var nextIndex = currentIndex + 1 == $scope.matchingOptions.length
                            ? 0 : currentIndex + 1;
                        $scope.highlightedOption = $scope.matchingOptions[nextIndex];
                    }
                };

                $scope.highlightPrevious = function () {
                    if (!$scope.highlightedOption) {
                        $scope.highlightedOption = $scope.matchingOptions[$scope.matchingOptions.length - 1];
                    } else {
                        var currentIndex = $scope.getCurrentOptionIndex();
                        var previousIndex = currentIndex == 0
                            ? $scope.matchingOptions.length - 1
                            : currentIndex - 1;
                        $scope.highlightedOption = $scope.matchingOptions[previousIndex];
                    }
                };

                $scope.onOptionHover = function (option) {
                    $scope.highlightedOption = option;
                };

                $scope.$on('app:clearInput', function () {
                    $scope.searchTerm = '';
                });

                $scope.clearHighlight = function () {
                    $scope.highlightedOption = null;
                };

                $scope.closeAndClear = function () {
                    $scope.showOptions = false;
                    $scope.clearHighlight();
                };

                $scope.selectOption = function (option) {
                    // console.log('selected the option');
                    $scope.selectedOption = option;
                    $scope.onSelect(option);

                    if ($scope.clearInput != 'False' && $scope.clearInput != 'false') {
                        $scope.searchTerm = '';
                    } else {
                        $scope.searchTerm = option[$scope.displayProperty];
                    }
                    $scope.closeAndClear();
                };

                $scope.onBlur = function () {
                    $scope.closeAndClear();
                };

                $scope.getCurrentOptionIndex = function () {
                    return $scope.matchingOptions.indexOf($scope.highlightedOption);
                };
            },
            link: function (scope, elem, attrs) {
                scope.optionWidth = '400px';
                var inputElement = elem.children('.autocomplete-input')[0];

                scope.setOptionWidth = function () {
                    // console.log(inputElement.offsetWidth);
                    $timeout(function () {
                        var pixelWidth = inputElement.offsetWidth > 400 ? 400 : inputElement.offsetWidth - 2;
                        scope.optionWidth = pixelWidth + 'px';
                    });
                };

                angular.element(document).ready(function () {
                    scope.setOptionWidth();
                });

                angular.element($window).bind('resize', function () {
                    scope.setOptionWidth();
                });
            }
        };
    }]).directive('reorder', function () {
        return {
            restrict: 'E',
            transclude: true,
            scope: {
                list: '='
            },
            template: "<li drop-data='{{$index}}' drag-data='{{$index}}' ng-repeat='element in list' draggable droppable ng-transclude ></li>",
            link: function (scope, element) {
                scope.ondrop = function resort(dragIndex, dropIndex, event) {
                    if (dragIndex == dropIndex) {
                        return;
                    }
                    // console.log("HERE");
                    scope.$apply(function () {
                        dragIndex = Number(dragIndex);
                        // grab element to move
                        var elementToMove = scope.list[dragIndex];
                        // remove the element
                        scope.list.splice(dragIndex, 1);
                        // insert the element
                        scope.list.splice(Number(dropIndex), 0, elementToMove);
                    });
                };
            }
        };
    })
    .factory('autocomplete-keys', function () {
        return {
            upArrow: 38,
            downArrow: 40,
            enter: 13,
            escape: 27,
            tab: 9
        };
    });
app.config(["$routeProvider", function ($routeProvider) {
    return $routeProvider.when("/", {
        redirectTo: "/Welcome"
    })
        .when("/Welcome", {
            controller: "WelcomeController",
            templateUrl: "/views/Welcome.html"//
        })
        .when("/LoginToken/:token/:redirectto/:warehouseids/:userid/:userName/:rolenames/:Warehouseid", {
            controller: "LoginTokenController",
            templateUrl: "/views/WareHouse/WarehouseSelection.html"
        })
        ////default WarehouseSelection Page
        //.when("/warehouseSelection", {
        //    controller: "warehouseSelectionController",
        //    templateUrl: "/views/WareHouse/WarehouseSelection.html"
        //})
        //for HubPhase Harry/Vinayak
        .when("/HubPhase", {
            controller: "HubPhaseController",
            templateUrl: "/views/Reports/HubPhase.html"//
        })
        //for DispatchedOrderPage Harry/Vinayak
        .when("/DispatchedOrderPage", {
            controller: "DispatchedOrderPageController",
            templateUrl: "/views/Order/DispatchedOrderPage.html" //
        })
        //for InventoryEdit Vinayak (3/10/2019)
        .when("/InventoryEditForm", {
            controller: "InventoryEditController",
            templateUrl: "/views/Stock/InventoryEditForm.html" //
        })
        .when("/InventoryApprovalPage", {
            controller: "InventoryApprovalPagecontroller",
            templateUrl: "/views/Stock/InventoryDetails.html" //
        })
        //for OrderMasterUpdate Vinayak (12/10/2019)
        //.when("/OrderMasterUpdate", {
        //    controller: "OrderMasterUpdateController",
        //    templateUrl: "/views/Order/OrderMasterUpdate.html" //
        //})

        //for Ewaybill Vinayak (22/10/2019)
        //.when("/EwayBillOrder", {
        //    controller: "EwayBillOrderController",          
        //})
        .when("/InventoryCycle", {
            controller: "InventoryCycleController",
            templateUrl: "/views/InventoryCycle.html" //
        })
        //CurrencySettle : 13/06/2019
        .when("/HQCurrency", {
            controller: "HQCurrencyController",
            templateUrl: "/views/CurrencyManagement/HQCurrency.html"
        })
        //-------------------Assignment Processing By Harry 13/05/2019------------//
        //Create Assignment
        .when("/CreateDeliveryAssignment", {
            controller: "CreateDeliveryAssignmentController",
            templateUrl: "views/Delivery/CreateDeliveryAssignemnt.html"
        })
        //Review Assignment & shortlistItem, final settlement 
        .when("/DeliveryAssignment", {
            controller: "DeliveryAssignmentController",
            templateUrl: "views/Delivery/DeliveryAssignment.html"
        })
        // Vehicle Assissment Summary Change Status
        .when("/DeliveryOrderAssignmentChange/:id", {
            controller: "DeliveryOrderAssignmentChangeController",
            templateUrl: "views/Delivery/DeliveryOrderAssignmentChange.html"
        })

        // Procees assignment report
        .when("/AssignmentReport", {
            controller: "AssignmentReportController",
            templateUrl: "views/Delivery/AssignmentReport.html"
        })
        .when("/UnconfirmedGR/:id", {
            controller: "UnconfirmedGRController",
            templateUrl: "views/Reports/UnconfirmedGR.html"
        })
        .when("/UnconfirmedGRDetail", {
            controller: "UnconfirmedGRDetailController",
            templateUrl: "views/Reports/UnconfirmedGRDetail.html"
        })
        .when("/Tempcurrentstock", {
            controller: "TemporaryCurrentStockController",
            templateUrl: "/views/Stock/TemporaryCurrentStock.html"
        })
        //---------------------------------------------------------//
        //============add Group===========//
        .when("/group", {
            controller: "GroupSMSController",
            templateUrl: "/views/GroupSMS.html"
        })
        .when("/customersGroup", {
            controller: "CustomerGroup",
            templateUrl: "/views/CustomerGroup.html"
        })
        .when("/supplierGroup", {
            controller: "SupplierGroup",
            templateUrl: "/views/SupplierGroup.html"
        })
        .when("/SupplierPaymentDetails", {
            controller: "SupplierPaymentDetailsController",
            templateUrl: "/views/suppliers/SupplierPaymentDetails.html"
        })
        .when("/peopleGroup", {
            controller: "PeopleGroup",
            templateUrl: "/views/PeopleGroup.html"
        })
        .when("/VehicleAssissment", {
            controller: "VehicleAssissmentController",
            templateUrl: "views/Delivery/VehicleAssissment.html"
        })
        .when("/dashboard", {
            controller: "dashboardController",
            templateUrl: "/views/dashboard.html"
        })
        .when("/InActiveCustOrder", {
            controller: "InActiveCustOrderMasterController",
            templateUrl: "/InActiveCustomerOrder/InActiveCustOrderMaster.html"
        })
        // Digital sales Report
        .when("/DigitalSales", {
            controller: "DigitalSalesController",
            templateUrl: "views/Reports/DigitalSalesReport.html"
        })
        .when("/NetMargin", {
            controller: "NetprofitController",
            templateUrl: "/CRM/NetProfit.html"
        })
        .when("/CpMatrix", {
            controller: "CiMatrixController",
            templateUrl: "/views/Order/CiMatrix.html"
        })
        .when("/Document", {
            controller: "DocumentController",
            templateUrl: "/views/admin/Document.html"
        })
        .when("/ChangePassword", {
            controller: "ChangePassword",
            templateUrl: "/views/pages/ChangePassword.html"
        })
        .when("/TurnATime", {
            controller: "TurnATimeController",
            templateUrl: "views/Delivery/TurnATime.html"
        })
        .when("/Agent", {
            controller: "AgentsController",
            templateUrl: "/views/admin/Agent.html"
        })
        .when("/VisitReport", {
            controller: "NotVisitCtrl",
            templateUrl: "views/Customer/NotVisit.html"
        })
        .when("/DialValuePoint", {
            controller: "DialValuePointController",
            templateUrl: "/views/Pramotion/DialValuePoint.html"
        })
        .when("/DialPoint", {
            controller: "DialPointController",
            templateUrl: "/views/Pramotion/DialPoint.html"
        })
        .when("/brandwisepramotion", {
            controller: "BrandwisepramotionController",
            templateUrl: "/views/Pramotion/Brandwisepramotion.html"
        })
        .when("/Exclusivebrand", {
            controller: "ExclusiveBrandController",
            templateUrl: "/views/Pramotion/ExclusiveBrand.html"
        })
        .when("/AssignBrandCustomer", {
            controller: "AssignBrandCustomerController",
            templateUrl: "/views/Customer/AssignBrandCustomer.html"
        })
        .when("/PointInOut", {
            controller: "PointInOutController",
            templateUrl: "views/Delivery/PointInOut.html"
        })
        .when("/WarehouseExpensePoint", {
            controller: "WarehouseExpensePointController",
            templateUrl: "views/Wallet/WarehouseExpensePoint.html"
        })
        .when("/AssignBulkcustomers", {
            controller: "AssignBulkcustomersController",
            templateUrl: "/views/Customer/AssignBulkcustomers.html"
        })
        .when("/CreateCompany", {
            controller: "CreateCompanyController",
            templateUrl: "/views/admin/CreateCompany.html"
        })
        .when("/CurrencySettle", {
            controller: "CurrencySettleController",
            templateUrl: "/views/CurrencyModule/Currency.html"
        })
        .when("/BankSettle", {
            controller: "BankSettleController",
            templateUrl: "/views/CurrencyModule/BankSettle.html"
        })
        .when("/CurrencyStock", {
            controller: "CurrencyStockController",
            templateUrl: "views/CurrencyStock/CurrencyStock.html"
        })
        .when("/excelorder", {
            controller: "ExcelOrderCtrl",
            templateUrl: "/views/Order/Excelorder.html"
        })
        .when("/OrderProcessReport", {
            controller: "OrderProcessReportController",
            templateUrl: "views/Reports/OrderProcessReport.html"
        })
        .when("/ChannelPartner", {
            controller: "ChannelPartnerController",
            templateUrl: "views/Reports/ChannelPartner.html"
        })
        .when("/HealthChart", {
            controller: "HealthChartController",
            templateUrl: "views/Reports/HealthChart.html"
        })

        .when("/AgentsPerformance", {
            controller: "AgentsPerformanceController",
            templateUrl: "views/Reports/AgentPerformance.html"
        })
        //.when("/AssignCustomers", {
        //    controller: "AssignCustomersCtrl",
        //    templateUrl: "views/suppliers/AssignCustomers.html"
        //})
        .when("/CustomerWiseExecutive", {
            controller: "AssignCustomersCtrl",
            templateUrl: "views/suppliers/MyWarehouseCustomer.html"
        })
        //.when("/DamageStock", {
        //    controller: "DamageStockController",
        //    templateUrl: "views/DamageStock/DamageStock.html"
        //})   -----Transfer To DS
        .when("/DamageItemApproval", {
            controller: "DamageItemApprovalController",
            templateUrl: "views/DamageStock/DamageItemApproval.html"
        })
        .when("/CreateDamageOrder", {
            controller: "CreateDamageOrderController",
            templateUrl: "views/DamageStock/CreateDamageOrder.html"
        })
        .when("/DamageOrder", {
            controller: "DamageorderMasterController",
            templateUrl: "views/DamageStock/DamageorderMaster.html"
        })
        .when("/ProductReport", {
            controller: "ProductReportController",
            templateUrl: "/views/Reports/ProductReport.html"
        })
        .when("/notOrdered", {
            controller: "NotOrderedController",
            templateUrl: "views/Order/Notordered.html"
        })
        .when("/reqService", {
            controller: "ReqServiceController",
            templateUrl: "views/Feed_Report/ReqService.html"
        })
        .when("/CRM", {
            controller: "CRMCtrl",
            templateUrl: "/CRM/CRM.html"
        })
        .when("/    ", {
            controller: "NetprofitController",
            templateUrl: "/CRM/NetProfit.html"
        })
        .when("/filtCustomer", {
            controller: "CRMcust4ActionCtrl",
            templateUrl: "/CRM/CRMcust4Action.html"
        })
        .when("/CustomerIssue", {
            controller: "CustomerIssueController",
            templateUrl: "/views/Customer/CustomerIssue.html"
        })
        .when("/actionTask", {
            controller: "ActionCtrl",
            templateUrl: "/CRM/Action.html"
        })
        .when("/SupplierIR", {
            controller: "IRSupplierCtrl",
            templateUrl: "/views/IR/IRSupplier.html"
        })
        .when("/PDCABaseCategory", {
            controller: "PDCABaseCategoryController",
            templateUrl: "/views/WareHouse/WarehouseReport.html"
        })
        .when("/PDCACategory", {
            controller: "PDCACategoryController",
            templateUrl: "/views/WareHouse/WarehouseReportCategory.html"
        })
        .when("/PDCADetails", {
            controller: "PDCADetailsController",
            templateUrl: "/views/WareHouse/WareHouseReportDetails.html"
        })
        .when("/PDCADataCompair", {
            controller: "PDCADataCompairController",
            templateUrl: "/views/WareHouse/PDCADataCompair.html"
        })
        .when("/SupplierPromo", {
            controller: "SupplierPromoController",
            templateUrl: "/views/ItemCategory/supplierPromo.html"
        })
        .when("/NppHistoryPrice", {
            controller: "NppHistoryPriceController",
            templateUrl: "/views/ItemCategory/NppHistoryPrice.html"
        })
        .when("/redeemMaster", {
            controller: "RedeemOrderCtrl",
            templateUrl: "/views/Order/reedeemOrder.html"
        })
        .when("/Area", {
            controller: "AreaMasterController",
            templateUrl: "/views/WareHouse/Area.html"
        })
        .when("/unitEcoReport", {
            controller: "unitEcoReportCtlr",
            templateUrl: "views/UnitEconomic/ueReport.html"
        })
        .when("/RedeemItem", {
            controller: "RewardItemCtrl",
            templateUrl: "views/Wallet/rewardItem.html"
        })
        .when("/unitEconomic", {
            controller: "unitEcoController",
            templateUrl: "views/UnitEconomic/unitEconomics.html"
        })
        .when("/Promo", {
            controller: "promItemController",
            templateUrl: "views/ItemCategory/promoItems.html"
        })
        .when("/Offer", {
            controller: "OfferController",
            templateUrl: "views/Offer/Offer.html"
        })

        .when("/AddOffer", {
            controller: "OfferADDController",
            templateUrl: "views/Offer/AddOffer.html"
        })
        .when("/FreebiesUploader", {
            controller: "FreebiesUploderController",
            templateUrl: "views/Offer/FreebiesUploader.html"
        })
        .when("/TreeStructure", {
            controller: "TreeStructureController",
            templateUrl: "views/TreeStructure.html"
        })

        //bill discount page by  Anushka
        //.when("/OfferBillDiscount", {
        //    controller: "OfferController",
        //    templateUrl: "views/Offer/OfferBillDiscount.html"
        //})
        .when("/Reward", {
            controller: "RewardPointController",
            templateUrl: "views/Wallet/Reward.html"
        })
        .when("/Wallet", {
            controller: "WalletController",
            templateUrl: "views/Wallet/Wallet.html"
        })
        .when("/ManualWallet", {
            controller: "WalletController",
            templateUrl: "views/Wallet/ManualWallet.html"
        })
        .when("/MileStone", {
            controller: "MilestonePointController",
            templateUrl: "views/Wallet/Milestone.html"
        })
        .when("/pages/LogOut", {
            controller: "logoutController",
            templateUrl: "views/pages/logout.html"
        })
        .when("/ShortageSettle", {
            controller: "ShortageSettleController",
            templateUrl: "/views/Order/ShortageSettle.html"
        })
        .when("/returnitem", {
            controller: "ReturnItemCtrl",
            templateUrl: "/views/Reports/ReturnItem.html"
        })
        .when("/requestbrand", {
            controller: "SuggetionContoller",
            templateUrl: "views/Feed_Report/suggetion.html"
        })
        .when("/salestarget", {
            controller: "targetController",
            templateUrl: "views/Feed_Report/SalesTarget.html"
        })
        .when("/BounceCheq", {
            controller: "BounceCheqController",
            templateUrl: "views/SalesSettlement/BounceCheq.html"
        })
        .when("/VehicleAssissment", {
            controller: "VehicleAssissmentController",
            templateUrl: "views/Delivery/VehicleAssissment.html"
        })
        //.when("/OnHoldGR", {
        //    controller: "OnHoldGRController",
        //    templateUrl: "views/OnHoldGR/OnHoldGR.html"
        //})
        .when("/OnHoldGR", {
            controller: "AddDatainOnHoldGR",
            templateUrl: "views/OnHoldGR/AddDatainOnHoldGR.html"
        })
        .when("/PendingOrder", {
            controller: "orderPendingController",
            templateUrl: "views/Order/orderPending.html"
        })
        //.when("/SalesAsignDay", {
        //    controller: "AsignDayController",
        //    templateUrl: "/views/Customer/AsignDays.html"
        //})
        .when("/SalesSettlementHistory", {
            controller: "SalesSettlementHistoryController",
            templateUrl: "views/SalesSettlement/SalesSettlementHistory.html"
        })
        .when("/SalesSettlement", {
            controller: "SalesSettlementController",
            templateUrl: "views/SalesSettlement/SalesSettlement.html"
        })
        .when("/SaleCheqBounce", {
            controller: "SalesBounceController",
            templateUrl: "views/SalesSettlement/SaleCheqBounce.html"
        })
        //report
        .when("/report/time", {
            controller: "reportsController",
            templateUrl: "/views/report/time.html"
        })
        .when("/LiveHubKpi", {
            controller: "ReportController",
            templateUrl: "/views/Reports/Report.html"
        })
        .when("/Reports", {
            controller: "Report3Controller",
            templateUrl: "/views/Reports/Report3.html"
        })
        .when("/RetailersReport", {
            controller: "RetailersReportCtrl",
            templateUrl: "/views/Reports/RetailersReport.html"
        })
        .when("/DeliveryBoyReport", {
            controller: "DeliveryBoyReportCtrl",
            templateUrl: "/views/Reports/DeliveryBoyReport.html"
        })
        .when("/Comparison", {
            controller: "ComparisonCtrl",
            templateUrl: "/views/Reports/Comparison.html"
        })
        .when("/HubCity", {
            controller: "Comparison1Ctrl",
            templateUrl: "/views/Reports/Comparison1.html"
        })
        .when("/DashboardReport", {
            controller: "DashboardReportController",
            templateUrl: "/views/Reports/DashboardReport.html"
        })
        // neha 21-08-19
        .when("/DownloadItemLedger", {
            controller: "DownloadItemLedgerController",
            templateUrl: "/views/Reports/DownloadItemLedger.html"
        })
        .when("/DeliveryBoyHistory", {
            controller: "DeliveryBoyHistoryController",
            templateUrl: "views/Delivery/DeliveryBoyHistory.html"
        })
        .when("/Redispatch", {
            controller: "RedispatchCtrl",
            templateUrl: "views/Redispatch/RedispatchOrders.html"
        })
        .when("/RedispatchAutoCanceled", {
            controller: "RedispatchCtrlAutoCanceled",
            templateUrl: "views/Redispatch/RedispatchOrdersAutoCanceled.html"
        })
        .when("/deliveryCharge", {
            controller: "deliveryChargeController",
            templateUrl: "views/admin/DeliveryCharges.html"
        })
        .when("/ChangeDBoy", {
            controller: "ChangeDBoyCtrl",
            templateUrl: "views/Delivery/ChangeDBoy.html"
        })
        .when("/News", {
            controller: "NewsController",
            templateUrl: "views/admin/News.html"
        })
        .when("/MyOffer", {
            controller: "CouponController",
            templateUrl: "views/admin/Coupon.html"
        })
        .when("/Vehicles", {
            controller: "VehicleController",
            templateUrl: "views/Delivery/Vehicle.html"
        })
        .when("/DeliveryBoy", {
            controller: "DeliveryBoyController",
            templateUrl: "views/Delivery/DeliveryBoy.html"
        })
        .when("/Dboyphonedetails", {
            controller: "DboyphonedetailsController",      //tejas
            templateUrl: "views/Delivery/Dboyphonedetails.html"
        })
        .when("/RTV", {
            controller: "RTVController",
            templateUrl: "views/PurchaseOrder/RTVForm.html"
        })
        //removed due to upgradation in assignment 
        //.when("/Delivery", {
        //    controller: "DeliveryController",
        //    templateUrl: "views/Delivery/Delivery.html"
        //})
        .when("/pages/confirmEmail", {
            controller: "confirmEmailController",
            templateUrl: "views/pages/ConfirmEmail.html"
        })
        .when("/map", {
            controller: "mappController",
            templateUrl: "/views/new/mapp.html"
        })
        .when("/PurchaseOrderdetails", {
            controller: "SearchPODetailsController",
            templateUrl: "/views/Reports/PurchaseOrderdetails.html"
        })
        .when("/PurchaseOrderDetailsShow", {
            controller: "SearchPODetailsController",
            templateUrl: "/views/Reports/PurchaseOrderDetailsShow.html"
        })
        .when("/OnHoldGRAddData", {
            controller: "OnHoldGRAddDataController",
            templateUrl: "/views/OnHoldGR/OnHoldGRAddData.html"
        })
        .when("/PurchaseInvoice", {
            controller: "SearchPODetailsController",
            templateUrl: "/views/invoice/PurchaseInvoice.html"
        })
        .when("/goodsrecived", {
            controller: "GoodsRecivedController",
            templateUrl: "/views/reports/GoodsRecived.html"
        })
        .when("/IR", {
            controller: "IRController",
            templateUrl: "/views/IR/IR.html"
        })
        .when("/IRNew", {
            controller: "IRControllerNew",
            templateUrl: "/views/PurchaseOrder/IRNew.html"
        })
        .when("/IREdit/:id/:RIid", {
            controller: "IREditController",
            templateUrl: "/views/PurchaseOrder/IRedit.html"
        })
        .when("/IRBuyerApprovel/:id/:RIid", {
            controller: "IRDetailBuyerControllerNew",
            templateUrl: "/views/PurchaseOrder/IRBuyerApprovelNew.html"
        })
        .when("/IRDetailBuyer", {
            controller: "IRDetailBuyerController",
            templateUrl: "/views/IR/IRDetailBuyer.html"
        })
        .when("/FreeStockData", {
            controller: "FreestockController",
            templateUrl: "/views/FreeStock.html"
        })


        .when("/DamageStockitem", {
            controller: "DamageStockItemCntrl",
            templateUrl: "/views/DamageStock/DamageStockItem.html"
        })

        .when("/CurrentStock", {
            controller: "CurrentStockController",
            templateUrl: "/views/Stock/CurrentStock.html"
        })

        .when("/SafetyStock", {
            controller: "SafetystockController",
            templateUrl: "/views/SafetyStock.html"
        })

        .when("/AdjCurrentStock", {
            controller: "AdjustmentCurrentStockController",
            templateUrl: "/views/Stock/AdjustmentCurrentStock.html"
        })
        .when("/ItemMovement", {
            controller: "ItemMovementReportController",
            templateUrl: "/views/Stock/ItemMovements.html"
        })
        //By Sachin In CurrentStockController
        .when("/EmptyStockItem", {
            controller: "EmptyCurrentController",
            templateUrl: "/views/Stock/EmptyStockItem.html"
        })
        .when("/SearchPurchaseOrder", {
            controller: "searchPOController",
            templateUrl: "/views/Reports/SearchPurchaseOrder.html"
        })
        .when("/AddPurchaseOrder", {
            controller: "AddPOController",
            templateUrl: "/views/PurchaseOrder/AddPurchaseOrder.html"
        })
        .when("/PurchaseOrderMaster", {
            controller: "PurchaseOrderMasterController",
            templateUrl: "/views/PurchaseOrder/PurchaseOrderMaster.html"
        })
        .when("/AdvancePurchaseRequestMaster", {
            controller: "AdvancePurchaseRequestController",
            templateUrl: "/views/PurchaseOrder/AdvancePurchaseRequest.html"
        })

        .when("/PRApproval", {
            controller: "PRApprovalController",
            templateUrl: "/views/PurchaseOrder/PRApproval.html"
        })
        .when("/PRPaymentApproval", {
            controller: "PRPaymentApprovalController",
            templateUrl: "/views/PurchaseOrder/PRPaymentApproval.html"
        })
        .when("/ViewPRPaymentDone", {
            controller: "ViewPRPaymentDoneController",
            templateUrl: "/views/PurchaseOrder/ViewPRPaymentDone.html"
        })


        .when("/AddAdvancePORequest", {
            controller: "AddAdvancePORequestController",
            templateUrl: "/views/PurchaseOrder/AddAdvancePORequest.html"
        })
        .when("/AdvancePurchaseDetails", {
            controller: "AdvancePurchaseDetailsController",
            templateUrl: "/views/PurchaseOrder/AdvancePurchaseDetails.html"
        })

        .when("/PurchaseOrderDetail", {
            controller: "PurchaseOrderDetailNewController",
            templateUrl: "/views/PurchaseOrder/PurchaseOrderdetail.html"
        })
        .when("/AdvancePurchaseRequestDetail", {
            controller: "AdvancePurchaseRequestDetailController",
            templateUrl: "/views/PurchaseOrder/AdvancePurchaseRequestDetail.html"
        })

        .when("/OnHoldGR", {
            controller: "OnHoldGRController",
            templateUrl: "/views/Reports/OnHoldGR.html"
        })
        .when("/WarehouseSupplier", {
            controller: "WarehouseSupplierController",
            templateUrl: "/views/WareHouse/WarehouseSupplier.html"
        })
        .when("/Orderdetails", {
            controller: "orderdetailsController",
            templateUrl: "/views/Order/Orderdetails.html"
        })
        .when("/ReturnOrderdetails", {
            controller: "ReturnOrderdetailsController",
            templateUrl: "/views/Order/ReturnOrderdetails.html"
        })
        .when("/viewpurchase", {
            controller: "purchaseorderController",
            templateUrl: "/views/Reports/PurchaseOrder.html"
        })
        .when("/PurchaseOrderList", {
            controller: "PurchaseOrderListController",
            templateUrl: "/views/Reports/PurchaseOrderList.html"
        })
        .when("/invoice", {
            controller: "orderdetailsController",
            templateUrl: "/views/invoice/invoice.html"
        })
        .when("/demand", {
            controller: "demandController",
            templateUrl: "/views/WareHouse/AddDemand.html"
        })
        .when("/demandreport", {
            controller: "DemandController",
            templateUrl: "/views/Reports/DemandReport.html"
        })
        .when("/pramotion", {
            controller: "PramotionController",
            templateUrl: "/views/Pramotion/ItemPramotion.html"
        })
        .when("/billpramotion", {
            controller: "BillPramotionController",
            templateUrl: "/views/Pramotion/BillPramotion.html"
        })
        .when("/brandpramotion", {
            controller: "PramotionController",
            templateUrl: "/views/Pramotion/BrandPramotion.html"
        })
        .when("/unitMaster", {
            controller: "unitMasterController",
            templateUrl: "/views/new/UnitMasterCategory.html"
        })
        .when("/orderMaster", {
            controller: "orderMasterController",
            templateUrl: "/views/Order/orderMaster.html"
        })
        .when("/orderSettle", {
            controller: "OrderSettleController",
            templateUrl: "/views/Order/OrderSettle.html"
        })
        .when("/itemMaster", {
            controller: "itemMasterController",
            templateUrl: "/views/ItemCategory/itemMaster.html"
        })
        .when("/MOQItemMaster", {
            controller: "MOQItemMasterController",
            templateUrl: "/views/ItemCategory/MOQItemMaster.html"
        })
        .when("/ItemsSupplierMapping", {
            controller: "ItemSupplierMappingController",
            templateUrl: "/views/ItemCategory/ItemSupplierMapping.html"
        })
        .when("/HighestDreamPoint", {
            controller: "HighestDreamPointItemController",
            templateUrl: "/views/ItemCategory/HighestDreamPointItem.html"
        })
        .when("/editPrice", {
            controller: "editPriceController",
            templateUrl: "/views/ItemCategory/editPrice.html"
        })
        .when("/MoveItems", {
            controller: "CopyItemController",
            templateUrl: "/views/MoveItems/MoveItems.html"
        })
        .when("/MoveSubCategory", {
            controller: "CopyItemController",
            templateUrl: "/views/MoveItems/MoveSubCategory.html"
        })
        .when("/MoveCategory", {
            controller: "CopyItemController",
            templateUrl: "/views/MoveItems/MoveCategory.html"
        })
        .when("/MoveSub2Category", {
            controller: "CopyItemController",
            templateUrl: "/views/MoveItems/MoveSub2Category.html"
        })
        .when("/FilterItem", {
            controller: "FilterItemsController",
            templateUrl: "/views/ItemCategory/FilterItem.html"
        })
        .when("/TaxGroup", {
            controller: "TaxGroupController",
            templateUrl: "/views/TaxGroup/TaxGroup.html"
        })
        .when("/TaxMaster", {
            controller: "TaxmasterController",
            templateUrl: "/views/TaxGroup/TaxMaster.html"
        })
        .when("/Itembrandname", {
            controller: "itembrandController",
            templateUrl: "/views/new/Itembrandname.html"
        })
        .when("/Slider", {
            controller: "SliderCtrl",
            templateUrl: "/views/Slider/Slider.html"
        })
        .when("/userdashboard", {
            controller: "userdashboardController",
            templateUrl: "views/report/userdashboard.html"
        })
        .when("/clientdashboard/:id", {
            controller: "clientdashboardController",
            templateUrl: "views/report/clientdashboard.html"
        })
        .when("/projectdashboard/:id", {
            controller: "projectdashboardController",
            templateUrl: "views/report/projectdashboard.html"
        })
        .when("/taskdashboard/:id", {
            controller: "taskdashboardController",
            templateUrl: "views/report/taskdashboard.html"
        })
        .when("/settings", {
            controller: "settingsController",
            templateUrl: "/views/admin/setting.html"
        })
        .when("/settings/settingEdit", {
            controller: "settingsController",
            templateUrl: "/views/admin/settingEdit.html"
        })
        .when("/customers", {
            controller: "customerController",
            templateUrl: "/views/Customer/customers.html"
        })
        .when("/customersDeviceInfo", {
            controller: "customersDeviceInfoController",    //tejas 
            templateUrl: "/views/Customer/customersDeviceInfo.html"
        })
        .when("/Authcustomers", {
            controller: "AuthcustomerController",
            templateUrl: "/views/Customer/Authcustomers.html"
        })
        //.when("/Message", {
        //    controller: "MessageController",
        //    templateUrl: "/views/admin/Message.html"
        //})
        .when("/projects", {
            controller: "projectsController",
            templateUrl: "/views/admin/projects.html"
        })
        .when("/projecttasks", {
            controller: "tasksController",
            templateUrl: "/views/admin/projectsTasks.html"
        })
        .when("/tasktypes", {
            controller: "tasktypesController",
            templateUrl: "/views/admin/tasktypes.html"
        })
        .when("/assetsCategory", {
            controller: "assetsCategoryController",
            templateUrl: "/views/admin/assetsCategory.html"
        })
        .when("/assets", {
            controller: "assetsController",
            templateUrl: "/views/admin/assets.html"
        })
        .when("/role", {
            controller: "roleController",
            templateUrl: "/views/admin/Roles.html"
        })
        .when("/people", {
            controller: "peoplesController",
            templateUrl: "/views/admin/people.html"
        })
        .when("/UpdateVersion", {
            controller: "UpdateVersionController",
            templateUrl: "/views/admin/UpdateVersion.html"
        })
        .when("/Deletedpeople", {
            controller: "DeletepeoplesController",
            templateUrl: "/views/admin/Deletedpeople.html"
        })
        .when("/NotificationByDeviceId", {
            controller: "NotificationByDeviceIdController",
            templateUrl: "/views/notification/NotificationByDeviceId.html"
        })
        .when("/Notification", {
            controller: "NotificationController",
            templateUrl: "/views/notification/Notification.html"
        })
        .when("/NotificationUpdated", {
            controller: "NotificationUpdatedController",
            templateUrl: "/views/NotificationUpdated.html"
        })
        .when("/NotificationDashboard", {
            controller: "NotificationUpdatedController",
            templateUrl: "/views/NotificationDashboard.html"
        })
        .when("/NotificationDetail", {
            controller: "NotificationDetailController",
            templateUrl: "/views/NotificationDetail.html"
        })
        .when("/expenseReport", {
            controller: "expensesController",
            templateUrl: "/views/admin/expenseReport.html"
        })
        .when("/leaveReport", {
            controller: "leavesController",
            templateUrl: "/views/admin/leaveReport.html"
        })
        .when("/expenses", {
            controller: "expensesController",
            templateUrl: "/views/admin/expenses.html"
        })
        .when("/expensecat", {
            controller: "expensecatController",
            templateUrl: "/views/admin/expensecat.html"
        })
        .when("/tfsweektimesheet", {
            controller: "TFSweektimesheetController",
            templateUrl: "/views/timesheet/tfsweektimesheet.html"
        })
        .when("/weektimesheet", {
            controller: "weektimesheetController",
            templateUrl: "/views/timesheet/weektimesheet.html"
        })
        .when("/weektasktimesheet", {
            controller: "weektasktimesheetController",
            templateUrl: "/views/timesheet/weektasktimesheet.html"
        })
        .when("/daytimesheet", {
            controller: "daytimesheetController",
            templateUrl: "/views/timesheet/daytimesheet.html"
        })
        .when("/daytasktimesheet", {
            controller: "daytasktimesheetController",
            templateUrl: "/views/timesheet/daytasktimesheet.html"
        })
        .when("/monthtimesheet", {
            controller: "monthtimesheetController",
            templateUrl: "/views/timesheet/monthtimesheet.html"
        })
        .when("/leave", {
            controller: "leavesController",
            templateUrl: "/views/timesheet/leavepage.html"
        })
        .when("/leaveApprove", {
            controller: "leavesController",
            templateUrl: "/views/admin/leaveApprove.html"
        })
        .when("/expenseApprove", {
            controller: "expensesController",
            templateUrl: "/views/admin/expenseApprove.html"
        })
        .when("/travelRequestApprove", {
            controller: "travelRequestController",
            templateUrl: "/views/admin/travelRequestApprove.html"
        })
        .when("/travelRequestPage", {
            controller: "travelRequestController",
            templateUrl: "/views/timesheet/travelRequestPage.html"
        })
        .when("/supplierCategory", {
            controller: "supplierCategoryController",
            templateUrl: "/views/suppliers/supplierCategory.html"
        })
        .when("/supplier", {
            controller: "supplierController",
            templateUrl: "/views/suppliers/supplier.html"
        })
        //Anushka
        .when("/ViewDepo", {
            controller: "ViewDepoController",
            templateUrl: "/views/suppliers/ViewDepo.html"
        })
        //.when("/CustomerCategory",
        // {
        //     controller: "customerCategoryController",
        //     templateUrl: "/views/Customer/CustomerCategory.html"
        // })
        .when("/state", {
            controller: "stateController",
            templateUrl: "/views/new/State.html"
        })
        .when("/city", {
            controller: "cityController",
            templateUrl: "/views/new/City.html"
        })
        //.when("/Multipleimage", {
        //     controller: "multipleimageController",
        //     templateUrl: "/views/NewFolder1/multipleimages.html"
        // })
        .when("/warehouse", {
            controller: "warehouseController",
            templateUrl: "/views/WareHouse/Warehouse.html"
        })
        .when("/cluster", {
            controller: "clusterController",
            templateUrl: "/views/WareHouse/Cluster.html"
        })
        .when("/clustercitymap/:id", {
            controller: "ClusterCityMapController",
            templateUrl: "/views/WareHouse/ClusterCityMap.html"
        })
        .when("/clustermap", {
            controller: "ClusterMapController",
            templateUrl: "/views/WareHouse/ClusterMap.html"
        })
        .when("/AddCluster", {
            controller: "AddclusterController",
            templateUrl: "/views/WareHouse/AddNewCluster.html"
        })
        .when("/WarehouseCategory", {
            controller: "WarehousecategoryController",
            templateUrl: "/views/WareHouse/WarehouseCategory.html"
        })
        .when("/WarehouseSubCategory", {
            controller: "WarehousesubCategoryController",
            templateUrl: "/views/WareHouse/WarehousesubCategory.html"
        })
        .when("/WarehouseSubsubCategory", {
            controller: "WarehousesubsubCategoryController",
            templateUrl: "/views/WareHouse/WarehouseSubsubCategory.html"
        })

        .when("/basecategory", {
            controller: "basecategoryController",
            templateUrl: "/views/Category/BaseCategory.html"
        })
        .when("/category", {
            controller: "categoryController",
            templateUrl: "/views/Category/Category.html"
        })
        .when("/subCategory", {
            controller: "subcategoryController",
            templateUrl: "/views/Category/subCategory.html"
        })
        .when("/subsubcategory", {
            controller: "subsubCategoryController",
            templateUrl: "/views/Category/SubsubCategory.html"
        })
        .when("/WarehouseBaseCategory", {
            controller: "WarehouseBaseCategoryController",
            templateUrl: "/views/WareHouse/WarehouseBaseCategory.html"
        })
        .when("/MappWHBaseCategory", {
            controller: "WarehouseBaseCategoryController",
            templateUrl: "/views/WareHouse/MappWHBaseCategory.html"
        })
        .when("/MappWarehouseSubCategory", {
            controller: "WarehousesubCategoryController",
            templateUrl: "/views/WareHouse/MappWarehouseSubCategory.html"
        })
        .when("/MappWHSubSubCategory", {
            controller: "WarehousesubsubCategoryController",
            templateUrl: "/views/WareHouse/MappWHSubSubCategory.html"
        })
        .when("/Question", {
            controller: "QuestionController",
            templateUrl: "/views/Question/Question.html"
        })
        .when("/apphomemobile", {
            controller: "AppHomeMobileController",
            templateUrl: "views/admin/AppHomeMobile.html"
        })
        .when("/apphomeexclude", {
            controller: "AppHomeExcludeController",
            templateUrl: "views/admin/AppHomeExclude.html"
        })
        .when("/apphomemobileedit/:id", {
            controller: "AppHomeEditController",
            templateUrl: "views/admin/AppHomeMobileEdit.html"
        })
        //By Sachin For Intigrating Tally
        .when("/Tally", {
            controller: "TallyController",
            templateUrl: "/views/admin/Tally.html"
        })
        .when("/PrestloanCustomer", {
            controller: "PrestloanCustomerController",
            templateUrl: "/views/Customer/PrestloanCustomer.html"
        })
        .when("/warehousesubsubcategory", {
            controller: "WarehousesubsubCategoryController",
            templateUrl: "/views/WareHouse/WarehouseSubsubCategory.html"
        })
        .when("/OrderPaymentReport", {
            controller: "OrderPaymentReportController",
            templateUrl: "/views/Reports/OrderPaymentReport.html"
        })
        .when("/OrderDataDownload", {
            controller: "OrderDataController",
            templateUrl: "/CRM/OrdersData.html"
        })
        .when("/OrderDataDownloadKK", {
            controller: "OrderDataKKController",
            templateUrl: "/CRM/OrdersDataKK.html"
        })
        .when("/MappWarehouseCategory", {
            controller: "WarehousecategoryController",
            templateUrl: "/views/WareHouse/MappWarehouseCategory.html"
        })
        .when("/FirstTimeOrder", {
            controller: "FirstTimeOrderController",
            templateUrl: "/CRM/FirstTimeorder.html"
        })
        .when("/apphomemobile", {
            controller: "AppHomeMobileController",
            templateUrl: "/views/admin/AppHomeMobile.html"
        })
        .when("/ViewPayment/:SupplierId", {
            controller: "ViewPaymentsController",
            templateUrl: "/views/suppliers/SupplierPayments.html"
        })
        .when("/GameLevel", {
            controller: "GameLevelController",
            templateUrl: "/views/Question/GameLevel.html"
        })
        //.when("/BlankPO", {
        //    controller: "BlankPOController",
        //    templateUrl: "/views/Reports/BlankPurchaseOrder.html"
        //})
        //.when("/AddBlankPO", {
        //    controller: "BlankPOController",
        //    templateUrl: "/views/Reports/AddBlankPO.html"
        //})
        .when("/BlankPOdetails", {
            controller: "BlankPODetailsController",
            templateUrl: "/views/Reports/BlankPODetails.html"
        })
        .when("/BlankPOEdit", {
            controller: "BlankPOEditController",
            templateUrl: "/views/reports/BlankPOEdit.html"
        })
        .when("/invoice/creatingRecurringinvoice", {
            templateUrl: "/views/invoice/creatingRecurringinvoice.html"
        })
        .when("/invoice/3", {
            templateUrl: "/views/invoice/recurring.html"
        })
        .when("/ui/typography", {
            templateUrl: "views/ui/typography.html"
        })
        .when("/ui/buttons", {
            templateUrl: "views/ui/buttons.html"
        })
        .when("/ui/icons", {
            templateUrl: "views/ui/icons.html"
        })
        .when("/ui/grids", {
            templateUrl: "views/ui/grids.html"
        })
        .when("/ui/widgets", {
            templateUrl: "views/ui/widgets.html"
        })
        .when("/ui/components", {
            templateUrl: "views/ui/components.html"
        })
        .when("/ui/timeline", {
            templateUrl: "views/ui/timeline.html"
        })
        .when("/ui/pricing-tables", {
            templateUrl: "views/ui/pricing-tables.html"
        })
        .when("/forms/elements", {
            templateUrl: "views/forms/elements.html"
        })
        .when("/forms/layouts", {
            templateUrl: "views/forms/layouts.html"
        })
        .when("/forms/validation", {
            templateUrl: "views/forms/validation.html"
        })
        .when("/forms/wizard", {
            templateUrl: "views/forms/wizard.html"
        })
        .when("/tables/static", {
            templateUrl: "views/tables/static.html"
        })
        .when("/tables/responsive", {
            templateUrl: "views/tables/responsive.html"
        }).when("/tables/dynamic", {
            templateUrl: "views/tables/dynamic.html"
        })
        .when("/charts/others", {
            templateUrl: "views/charts/charts.html"
        })
        .when("/charts/morris", {
            templateUrl: "views/charts/morris.html"
        })
        .when("/charts/flot", {
            templateUrl: "views/charts/flot.html"
        })
        .when("/mail/inbox", {
            templateUrl: "views/mail/inbox.html"
        })
        .when("/mail/compose", {
            templateUrl: "views/mail/compose.html"
        })
        .when("/mail/single", {
            templateUrl: "views/mail/single.html"
        })
        .when("/pages/features", {
            templateUrl: "views/pages/features.html"
        })
        .when("/pages/signin", {
            controller: "loginController",
            templateUrl: "views/pages/signin.html"
        })
        .when("/pages/signup", {
            controller: "signupController",
            templateUrl: "views/pages/signup.html"
        })
        .when("/pages/lock-screen", {
            templateUrl: "views/pages/lock-screen.html"
        })
        .when("/pages/profile", {
            controller: "profilesController",
            templateUrl: "views/pages/profile.html"
        })
        .when("/pages/profile/profileEdit", {
            controller: "profilesController",
            templateUrl: "views/pages/profileEdit.html"
        })
        .when("/AppPromotion", {
            controller: "MarginImagePromotionController",
            templateUrl: "/views/AppPromotion/MarginImagePromotion.html"
        })
        //By Sachin This Controller is in MarginImagePromotionController
        .when("/TopAddedItem", {
            controller: "MarginImagePromotionController",
            templateUrl: "/views/AppPromotion/TopAddedItem.html"
        })
        //By Sachin This Controller is in MarginImagePromotionController
        .when("/TotalDhamaka", {
            controller: "TotalDhamakaController",
            templateUrl: "/views/AppPromotion/TodayDhamaka.html"
        })
        //By Sachin This Controller is in MarginImagePromotionController
        .when("/BulkDeal", {
            controller: "BulkDealController",
            templateUrl: "/views/AppPromotion/BulkDeal.html"
        })
        .when("/MostSelledItem", {
            controller: "MostSelledItemController",
            templateUrl: "/views/AppPromotion/MostSelledItem.html"
        })
        .when("/MostSelledBrand", {
            controller: "MostSelledBrandController",
            templateUrl: "/views/AppPromotion/MostSelledBrand.html"
        })
        .when("/categoryImage", {
            controller: "CategoryImageController",
            templateUrl: "/views/Category/CategoryImage.html"
        })
        .when("/AgentDashboard", {
            controller: "AgentsDashboardController",
            templateUrl: "/views/admin/AgentDashboard.html"
        })
        .when("/AgentExcel", {
            controller: "AgentExcelController",
            templateUrl: "/views/admin/AgentExcel.html"
        })
        .when("/Agentphoneinfo", {
            controller: "AgentphoneinfoController",            //tejas 
            templateUrl: "/views/admin/Agentphoneinfo.html"
        })
        .when("/TransferOrderSend", {
            controller: "TransferOrderController",
            templateUrl: "/views/TransferOrder/TransferOrderSend.html"
        })
        .when("/TransferOrderRequest", {
            controller: "TransferOrderRequestController",
            templateUrl: "/views/TransferOrder/TransferOrderRequest.html"
        })
        .when("/UserAccessPermission", {
            controller: "UserAccessPermissionController",
            templateUrl: "/views/admin/UserAccessPermission.html"
        })
        .when("/Murli", {
            controller: "CustomerVoiceController",
            templateUrl: "/views/admin/Murli.html"
        })
        .when("/Case", {
            controller: "casecontroller",
            templateUrl: "/views/admin/Case.html"
        })
        .when("/ViewCase", {
            controller: "CaseViewController",
            templateUrl: "/views/admin/ViewCase.html"
        })
        .when("/AddPeople", {
            controller: "AddPeopleCtrl",
            templateUrl: "/views/admin/AddPeople.html"
        })
        .when("/Department", {
            controller: "Departmentctrl",
            templateUrl: "views/admin/Department.html"
        })
        .when("/Designation", {
            controller: "designationController",
            templateUrl: "/views/admin/Designation.html"
        })
        .when("/skill", {
            controller: "SkillCtrl",
            templateUrl: "views/admin/skill.html"
        })
        .when("/orderprocess", {
            controller: "OrderProcessStatus",
            templateUrl: "/views/admin/orderprocess.html"
        })
        .when("/pages/blank", {
            templateUrl: "views/pages/blank.html"
        })
        .when("/pages/invoice", {
            templateUrl: "views/pages/invoice.html"
        })
        .when("/apphome", {
            controller: "AppHomeController",
            templateUrl: "views/admin/AppHome.html"
        })
        .when("/poapproval", {
            controller: "POapprovalController",
            templateUrl: "/views/admin/POApproval.html"
        })
        .when("/SupplierStatus", {
            controller: "StatusSupplierController",
            templateUrl: "/views/suppliers/SupplierStatus.html"
        })
        .when("/Approver&Reviewer", {
            controller: "PoDashboardController",
            templateUrl: "/views/Reports/PoDashboard.html"
        })
        .when("/PRApprover&PRReviewer", {
            controller: "PRDashboardController",
            templateUrl: "/views/PurchaseOrder/PRDashboard.html"
        })
        .when("/PODashboard", {
            controller: "PODashboardMainController",
            templateUrl: "/views/Reports/PoDashboardMain.html"
        })
        .when("/IRDashboard", {
            controller: "IRDashboardController",
            templateUrl: "/views/Reports/IRDashboard.html"
        })
        .when("/trackuser", {
            controller: "trackuser",
            templateUrl: "/views/admin/trackuser.html"
        })
        .when("/MyUdhar", {
            controller: "MyUdharController",
            templateUrl: "/views/Customer/MyUdhar.html"
        })
        .when("/WalletHistory", {
            controller: "WalletHistoryController",
            templateUrl: "/WalletHistory/WalletHistory.html"
        })
        .when("/IRBuyer", {
            controller: "IRBuyerController",
            templateUrl: "/views/IR/IRBuyer.html"
        })
        .when("/IRView", {
            controller: "IRViewController",
            templateUrl: "/views/IR/IRView.html"
        })
        .when("/RejectedIR", {
            controller: "RejectedIRController",
            templateUrl: "/views/IR/RejectedIR.html"
        })
        //.when("/RejectedIRDetail", {
        //    controller: "RejectedIRDetailController",
        //    templateUrl: "/views/IR/RejectedIRDetail.html"
        //})
        .when("/RejectedIRDetail/:id/:RIid", {
            controller: "IRRejecteddetailController",
            templateUrl: "/views/PurchaseOrder/IRRejected.html"
        })
        .when("/tasks", {
            templateUrl: "views/tasks/tasks.html"
        })
        .when("/404", {
            templateUrl: "views/pages/404.html"
        })
        .when("/pages/500", {
            templateUrl: "views/pages/500.html"
        })
        .when("/CurrentNetStock", {
            controller: "CurrentNetStockController",
            templateUrl: "/views/Stock/CurrentNetStock.html"
        })
        .when("/MarkettingBudget", {
            controller: "BudgetAllocationController",
            templateUrl: "/views/TargetModule/BudgetAllocation.html"
        })
        .when("/BudgetAllocatonList", {
            controller: "BudgetAllocationListController",
            templateUrl: "/views/TargetModule/BudgetAllocationListController.html"
        })
        .when("/TargetDashboard", {
            controller: "TargetDashboardController",
            templateUrl: "/views/TargetModule/LevelDashboard.html"
        })
        .when("/TargetAllocatonBandsList/:wid", {
            controller: "TargetAllocatonBandsListController",
            templateUrl: "/views/TargetModule/TargetAllocationBands.html"
        })
        .when("/PageButtonPermission", {
            controller: "PageButtonPermissionController",
            templateUrl: "/views/permission/PageButtonPermission.html"
        })
        //On hold gr routing
        .when("/OnHoldGR", {
            controller: "OnHoldGRController",
            templateUrl: "/views/Reports/OnHoldGR.html"
        })
        .when("/MasterOnHoldGR", {
            controller: "MasterOnHoldGRController",
            templateUrl: "/views/Reports/MasterOnHoldGR.html"
        })
        //clusterWiseReport
        .when("/ClusterWise", {
            controller: "ClusterWiseController",
            templateUrl: "/views/Reports/ClusterWise.html"
        })
        // IR draft controller
        .when("/IRDarfted", {
            controller: "IRDraftController",
            templateUrl: "/views/IR/IRDraft.html"
        })
        .when("/WarehouseCurrency", {
            controller: "WarehouseCashController",
            templateUrl: "/views/CurrencyManagement/WarehouseCurrency.html"
        })

        .when("/WarehouseCurrencyDashboard", {
            controller: "WarehouseLiveDashboardController",
            templateUrl: "/views/CurrencyManagement/WarehouseLiveDashboard.html"
        })
        .when("/WarehouseCurrencySettlement", {
            controller: "WarehouseSettlement",
            templateUrl: "/views/CurrencyManagement/WarehouseCurrencySettlement.html"
        })
        .when("/BankCurrencySettlement", {
            controller: "BankCurrencySettlement",
            templateUrl: "/views/CurrencyManagement/BankWithdraw.html"
        })
        .when("/HQCashHistory", {
            controller: "HQCashHistoryController",
            templateUrl: "/views/CurrencyManagement/HQCashHistory.html"
        })
        .when("/CashExchangeHistory", {
            controller: "CashExchangeHistoryController",
            templateUrl: "/views/CurrencyManagement/CashExchangeHistory.html"
        })
        .when("/HQCurrency", {
            controller: "HQCurrencyController",
            templateUrl: "/views/CurrencyManagement/HQCurrency.html"
        })
        .when("/ChequeHistory", {
            controller: "WareHouseChequeHistoryController",
            templateUrl: "/views/CurrencyManagement/ChequeHistory.html"
        })
        .when("/HQChequeHistory", {
            controller: "HQChequeHistoryController",
            templateUrl: "/views/CurrencyManagement/ChequeHistoryHQ.html"
        })
        // PO Task controller
        .when("/POTask", {
            controller: "POTaskController",
            templateUrl: "/views/PurchaseOrder/PoTask.html"
        })
        //PNLReport
        .when("/PNLReport", {
            controller: "PNLController",
            templateUrl: "/views/Reports/PNLReport.html"
        })
        .when("/CashHistory", {
            controller: "WarehouseCashHistoryController",
            templateUrl: "/views/CurrencyManagement/CashHistory.html"
        })
        .when("/HQReturnCheque", {
            controller: "HQReturnHistoryController",
            templateUrl: "/views/CurrencyManagement/HQReturnCheque.html"
        })
        .when("/CashExchangeHistory", {
            controller: "CashExchangeHistoryController",
            templateUrl: "/views/CurrencyManagement/CashExchangeHistory.html"
        })
        .when("/ChequeFineAppoved", {
            controller: "ChequeFineAppovalController",
            templateUrl: "/views/CurrencyManagement/ChequeFineAppoved.html"
        })
        .when("/HQLiveCurrencyDashboard", {
            controller: "HQLiveCurrencyController",
            templateUrl: "/views/CurrencyManagement/HQLiveCurrencyDashboard.html"
        })
        .when("/Logs", {
            controller: "TraceLogController",
            templateUrl: "/views/Logs/TraceLog.html"
        })
        .when("/growthDashboard", {
            controller: "GrowthDashboardController",
            templateUrl: "/views/Growth/GrowthDashboard.html"
        })
        .when("/GrowthModuleLogin", {
            controller: "GrowthModuleLoginController",
            templateUrl: "/views/Growth/GrowthModuleLogin.html"
        })
        .when("/DesignBarcode", {
            controller: "DesignBarcodeController",
            templateUrl: "/views/PrintBarcode/DesignBarcode.html"
        })
        .when("/HQOnlineHistory", {
            controller: "HQOnlineCashController",
            templateUrl: "/views/CurrencyManagement/HQOnlineHistory.html"
        })
        .when("/OnlineHistory", {
            controller: "OnlineCashController",
            templateUrl: "/views/CurrencyManagement/OnlineHistory.html"
        })
        .when("/AssignmentCollection", {
            controller: "HQAssignmentCurrencyController",
            templateUrl: "/views/CurrencyManagement/HQAssignmentCurrency.html"
        })
        .when("/PurchasePendingReport", {
            controller: "PurchasePendingReportController",
            templateUrl: "/views/Reports/PurchasePendingReport.html"
        })
        .when("/FillRateCutReport", {
            controller: "FillRateCutReportController",
            templateUrl: "/views/Reports/FillRateCutReport.html"
        })
        .when("/TATDashboard", {
            controller: "TurnAroundTimeCotroller",
            templateUrl: "/views/TurnAroundTime/TATDashboard.html"
        })
        .when("/CurrentStockReport", {
            controller: "CurrentStockReportController",
            controllerAs: 'vm',
            templateUrl: "/views/Reports/CurrentStockReport.html"
        })
        //created by Pooja.
        .when("/FreeStock", {
            controller: "FreeStockController",
            templateUrl: "/views/FreeStock.html"
        })
        .when("/OrderReporting", {
            controller: "OrderReportingController",
            templateUrl: "/views/Reports/DBoyOrderReporting.html"
        })
        .when("/apphomeupdated", {
            controller: "AppHomeUpdatedController",
            templateUrl: "/views/admin/AppHomeUpdated.html"
        })
        .when("/AutoProcessorder", {
            controller: "AutoProcessorderController",
            templateUrl: "/views/Order/AutoProcessorder.html"
        })
        .when("/PageMaster", {
            controller: "PageMasterController",
            templateUrl: "/views/permission/PageMaster.html"
        })
        .when("/RolePagePermission", {
            controller: "RolePagePermissionController",
            controllerAs: 'vm',
            templateUrl: "/views/permission/RolePagePermission.html"
        })
        .when("/BankCurrencyHistory", {
            controller: "BankCurrencyHistory",
            templateUrl: "/views/CurrencyManagement/BankCurrencyHistory.html"
        })
        .when("/PeoplePageAccessPermissions", {
            controller: "PeoplePageAccessPermissionsController",
            templateUrl: "/views/permission/PeoplePageAccessPermissions.html"
        })
        .when("/CityBaseCustomerReward", {
            controller: "CityBaseCustomerRewardController",
            templateUrl: "/views/Wallet/CityBaseCustomerReward.html"
        })
        .when("/InventoryReport", {
            controller: "InventoryReportController",
            templateUrl: "/views/Reports/InventoryReport.html"
        })
        .when("/PeoplePagePermission", {
            controller: "PeoplePagePermissionController",
            templateUrl: "/views/permission/PeoplePagePermission.html"
        })
        .when("/Epayletter", {
            controller: "EpayLetterController",


            templateUrl: "/views/Customer/Epayletter.html"


        })
        .when("/FlashDealReport", {
            controller: "FlashDealReportController",
            templateUrl: "/views/Reports/FlashDealReport.html"
        })
        // Online Transaction by Ashwin
        .when("/OnlineTransactionDashBoard", {
            controller: "OnlineTransactionDashBoardcontroller",
            templateUrl: "/views/Order/OnlineTransactionDashBoard.html"
        })

        .when("/PaymentUpload", {
            controller: "PaymentUploadController",
            templateUrl: "/views/Order/PaymentUpload.html"
        })

        .when("/ePayUploadDetails/:id", {
            controller: "PaymentUploadDetailsController",
            templateUrl: "/views/Order/PaymentUploadDetails.html"
        })
        .when("/HDFCUploadDetails/:id", {
            controller: "HDFCDetailsController",
            templateUrl: "/views/Order/HDFCUploadDetails.html"
        })
        .when("/ICICIUploadDetails/:id", {
            controller: "ICICIDetailsController",
            templateUrl: "/views/Order/ICICIUploadDetails.html"
        })
        .when("/UPIUploadDetails/:id", {
            controller: "UPIDetailsController",
            templateUrl: "/views/Order/UPIUploadDetails.html"
        })
        // HDFC-Credit by Sudhir
        .when("/HDFCCreditUploadDetails/:id", {
            controller: "HDFCCreditDetailsController",
            templateUrl: "/views/Order/HDFCCreditUploadDetails.html"
        })
        // Razorpay QR by Sudhir
        .when("/RazorpayQRUploadDetails/:id", {
            controller: "RazorpayQRDetailsController",
            templateUrl: "/views/Order/RazorpayQRUploadDetails.html"
        })
        .when("/MposUploadDetails/:id", {
            controller: "MposDetailsController",
            templateUrl: "/views/Order/MposUploadDetails.html"
        })
        .when("/GRApproval", {
            controller: "UnconfirmedGRDetailController",
            templateUrl: "/views/PurchaseOrder/GRApproval.html"
        })
        .when("/HDFCUPIUploadDetails/:id", {
            controller: "HDFCUPIDetailsController",
            templateUrl: "/views/Order/HDFCUPIUploadDetails.html"
        })
        .when("/HDFCNetBankingUploadDetails/:id", {
            controller: "HDFCNetBankingBDetailsController",
            templateUrl: "/views/Order/HDFCNetBankingUploadDetails.html"
        })

        .when("/AgentPayment", {
            controller: "AgentPaymentController",
            templateUrl: "/views/CurrencyManagement/AgentPayment.html"
        })
        .when("/AgentPaymentHistory", {
            controller: "AgentPaymentHistoryController",
            templateUrl: "/views/CurrencyManagement/AgentPaymentHistory.html"
        })
        .when("/RejectChequeFineStatus", {
            controller: "RejectChequeFineAppovalController",
            templateUrl: "/views/CurrencyManagement/RejectChequeFineStatus.html"
        })
        .when("/HQAgentPayment", {
            controller: "AgentPaymentController",
            templateUrl: "/views/CurrencyManagement/HQAgentPayment.html"
        })
        .when("/AddPayment", {
            controller: "AddDeuPaymentController",
            templateUrl: "/views/CurrencyManagement/AddPayment.html"
        })
        .when("/ReturnChequeCharge", {
            controller: "AddReturnChequeChargeController",
            templateUrl: "/views/CurrencyManagement/ReturnChequeCharge.html"
        })
        .when("/CashBalanceDetails", {
            controller: "CashBalanceController",
            templateUrl: "/views/CurrencyManagement/CashBalanceDetails.html"
        })
        .when("/CashBalanceHistory", {
            controller: "CashBalanceHistoryController",
            templateUrl: "/views/CurrencyManagement/CashBalanceHistory.html"
        })
        .when("/HQCashBalanceHistory", {
            controller: "HQCashBalanceHistoryController",
            templateUrl: "/views/CurrencyManagement/HQCashBalanceHistory.html"
        })
        //.when("/GRCancellation", {
        //    controller: "GRCancellationController",
        //    templateUrl: "/views/PurchaseOrder/GRCancellation.html"
        //})
        //.when("/GRIRMapping", {
        //    controller: "GRIRMappingController",
        //    templateUrl: "/views/Reports/GRIRMapp.html"
        //})
        //.when("/IRMaster", {
        //    controller: "IRMasterController",
        //    templateUrl: "/views/IR/IRMaster.html"
        //})
        .when("/HQChequeDetails", {
            controller: "HQChequeDetailsController",
            templateUrl: "/views/CurrencyManagement/HQChequeDetails.html"
        })

        .when("/GoodsRecivedNew/:id", {
            controller: "GoodsRecivedControllerNew",
            templateUrl: "/views/PurchaseOrder/GoodsRecivedNew.html"
        })
        //Created by-Vinayak Date:-2/07/2020
        .when("/GRDraftDetail/:POId/:GrNumber", {
            controller: "GRDraftDetailController",
            templateUrl: "/views/PurchaseOrder/IRImgDetail.html"
        })

        .when("/TreeStructure", {
            controller: "TreeStructureController",
            templateUrl: "/views/TreeStructure.html"
        })
        .when("/CreateBackendOrder", {
            controller: "CreateBackendController",
            templateUrl: "views/BackendOrder/CreateBackendOrder.html"
        })

        .when("/BackedOrderInvoice", {
            controller: "BackendOrderInvoiceController",
            templateUrl: "views/BackendOrder/BackendOrderInvoice.html"
        })
        // Online Transaction End


        .when("/CreateBackendOrder", {
            controller: "CreateBackendController",
            templateUrl: "views/BackendOrder/CreateBackendOrder.html"
        })
        .when("/BackendOrder", {
            controller: "BackendorderMasterController",
            templateUrl: "views/BackendOrder/BackendOrderMaster.html"
        })
        .when("/BackedOrderInvoice/:id", {
            controller: "BackendOrderInvoiceController",
            templateUrl: "views/BackendOrder/BackendOrderInvoice.html"
        })
        .when("/CustomerBackedOrderInvoice/:id", {
            controller: "CustomerBackendOrderInvoiceController",
            templateUrl: "views/BackendOrder/CustomerBackenInvoice.html"
        })
        .when("/CreditInvoice/:Id/:PoNumber", {
            controller: "CreditInvoiceController",
            templateUrl: "/views/PurchaseOrder/CreditInvoice.html"
        })
        .when("/DebitInvoice/:Id/:PoNumber", {
            controller: "CreditInvoiceController",
            templateUrl: "/views/PurchaseOrder/CreditInvoice.html"
        })
        .when("/POtoIRTATDashboard", {
            controller: "POtoIRTATController",
            templateUrl: "/views/PurchaseOrder/PotoIRTAT.html"
        })

        .when("/PartialPOReport", {
            controller: "PartialPOReportController",
            templateUrl: "/views/PurchaseOrder/PartialPOReport.html"
        })

        .when("/IRNRegenerate", {
            controller: "IRNRegerateCotroller",
            templateUrl: "/views/IRN/IRNRegererateList.html"
        })
        .when("/RTGS", {
            controller: "RTGSOrdersPaymentCotroller",
            templateUrl: "/views/RTGS/RTGSOrders.html"
        })
        .when("/GDN", {
            controller: "GDNController",
            templateUrl: "/views/PurchaseOrder/GDNDetail.html"
        })
        .when("/trip-planner", {
            controller: "TripPlannerController",
            templateUrl: "/views/TripPlanner/TripPlanner.html"
        })
        .when("/taxretrun", {
            controller: "GSTReturnController",
            templateUrl: "/views/TaxReturn/GSTRetrunPurpose.html"
        })
        .when("/ChequeBookDetails/:id", {
            controller: "ChequeUploadDetailsController",
            templateUrl: "/views/Order/ChequeUploadDetails.html"
        })

        .otherwise({
            redirectTo: "/404"

        });

}]);
//var serviceBase = 'https://uat.shopkirana.in/';

var serviceBase = 'http://localhost:26265/';
var saralUIPortal = 'https://saraluat.shopkirana.in/#/';
//var serviceBase = 'http://111.118.252.170:8989/';
app.constant('ngAuthSettings', {
    apiServiceBaseUri: serviceBase,
    clientId: 'ngAuthApp'
});
app.config(function ($httpProvider) {
    $httpProvider.interceptors.push('authInterceptorService');
});


app.factory('CurrencyManagementLocalStorageService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
    // CMCODE   --- otp verification code
    // CMTCODE   --- otp verification code
    localStorage.getItem('RolePerson');

    var service = {};
    service.IsOtpVarified = 'false';
    service.CTime = null;
    service.VerificationTimeInMins = 15;


    service.setVerified = function () {
        localStorage.setItem('CMCODE', 'true');
        localStorage.setItem('CMTCODE', (new Date()).toUTCString());
    }

    service.isVerified = function () {
        try {
            var isVerified = localStorage.getItem('CMCODE');
            var verifiedTime = new Date(localStorage.getItem('CMTCODE'));
            var time = new Date(verifiedTime.getTime() + (service.VerificationTimeInMins * 60000));
            var t = verifiedTime.getTime();

            var thisTime = new Date();
            var tt = thisTime.getTime();

            if (isVerified === 'true' && time.getTime() >= thisTime.getTime()) {
                return true;
            }
            else {
                return false;
            }

        } catch{
            return false;
        }

    }

    return service;
}]);


app.run(['authService', '$rootScope', 'CurrencyManagementLocalStorageService', '$location', '$http', 'ngAuthSettings', function (authService, $rootScope, CurrencyManagementLocalStorageService, $location, $http, ngAuthSettings) {
    authService.fillAuthData();

    $rootScope.$on("$locationChangeStart", function (event, next, current) {
        var pagename = next.substr(next.lastIndexOf("/") + 1);
        //if (next.includes('WarehouseCurrencyDashboard') || next.includes('WarehouseCurrency') || next.includes('ChequeHistory') || next.includes('WarehouseCurrencySettlement') || next.includes('BankCurrencySettlement')) {
        if (pagename == 'WarehouseCurrencyDashboard' || pagename == 'WarehouseCurrency' || pagename == 'ChequeHistory' || pagename == 'WarehouseCurrencySettlement' || pagename == 'BankCurrencySettlement') {
            if (CurrencyManagementLocalStorageService.isVerified()) {
                CurrencyManagementLocalStorageService.setVerified();
            }
            else {
                $location.path('WarehouseCurrencyDashboard');
            }
        }
    });

    $rootScope.$on('$routeChangeSuccess', function (e, current, pre) {
        console.log('Current route name: ' + $location.path());
        // Get all URL parameter
        //console.log('Get all URL parameter', $routeParams);
        let loginData = localStorage.getItem('ls.authorizationData');
        if (loginData) {
            let loginDataObject = JSON.parse(loginData);
            if (loginDataObject && loginDataObject.userName) {
                //inser into database
                var serviceBase = ngAuthSettings.apiServiceBaseUri;
                let obj = {
                    Route: $location.path(),
                    UserName: loginDataObject.userName
                };
                $http.post(serviceBase + 'api/History/InsertPageVisits', obj);
            }
        }

    });

}]);
app.directive("uiSpinner", [function () {
    return {
        restrict: "A",
        compile: function (ele) {
            return ele.addClass("ui-spinner"), {
                post: function () {
                    return ele.spinner();
                }
            }
        }
    }

    // $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
}]);

app.directive('ngMax', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, elem, attr, ctrl) {
            scope.$watch(attr.ngMax, function () {
                ctrl.$setViewValue(ctrl.$viewValue);
            });
            var maxValidator = function (value) {
                var max = scope.$eval(attr.ngMax) || 0;
                if (value && value > max) {
                    ctrl.$setValidity('ngMax', false);
                    return undefined;
                } else {
                    ctrl.$setValidity('ngMax', true);
                    return value;
                }
            };
            ctrl.$parsers.push(maxValidator);
            ctrl.$formatters.push(maxValidator);
        }
    };
});

//app.filter('total', function () {
//    return function (input, property) {
//        var i = input instanceof Array ? input.length : 0;
//        if (typeof property === 'undefined' || i === 0) {
//            return i;
//        } else if (isNaN(input[0][property])) {
//            throw 'filter total can count only numeric values';
//        } else {
//            var total = 0;
//            while (i--)
//                total += input[i][property];
//            return total;
//        }
//    };
//})
app.filter('total', function () {
    return function (data, key1, key2) {
        if (angular.isUndefined(data) || angular.isUndefined(key1) || angular.isUndefined(key2))
            return 0;
        var sum = 0;
        angular.forEach(data, function (value) {
            sum = sum + (parseInt(value[key1], 10) * parseInt(value[key2], 10));
        });
        return sum;
    }
})

app.filter('totalSingle', function () {
    return function (data, key1, key2) {
        if (angular.isUndefined(data) || angular.isUndefined(key1))
            return 0;
        var sum = 0;
        angular.forEach(data, function (value) {
            sum = sum + (parseInt(value[key1], 10));
        });
        return sum;
    }
})


app.controller("HistoryCtrl", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "obj", function ($scope, $http, ngAuthSettings, $modalInstance, obj) {

    $scope.EntityHistory = obj;

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); }


}]);


app.directive('timer', function ($timeout, $compile) {
    return {
        restrict: 'E',
        scope: {
            interval: '=', //don't need to write word again, if property name matches HTML attribute name
            startTimeAttr: '=?startTime', //a question mark makes it optional
            countdownAttr: '=?countdown' //what unit?
        },
        template: '<div data-ng-if="hours>=0 && hours<=48"><span class="label ng-binding" style="background-color:#033e15">{{ hours }}' + ':' + '{{ minutes }}' + ':' + '{{ seconds }}</span></div>' +
            '<div data-ng-if="hours>48 && hours<=60"><span class="label label-info ng-binding" style="background-color:#a58902">{{ hours }}' + ':' + '{{ minutes }}' + ':' + '{{ seconds }}</span></div>' +
            '<div data-ng-if="hours>60 && hours<=90"><span class="label label-warning ng-binding" style="background-color:#eb5a00">{{ hours }}' + ':' + '{{ minutes }}' + ':' + '{{ seconds }}</span></div>' +
            '<div data-ng-if="hours>90"><span class="label ng-binding" style="background-color:#f90808" >{{ hours }}' + ':' + '{{ minutes }}' + ':' + '{{ seconds }}</span></div>',
        //template: '<div><p>' +
        //    '<p>Time ends in : {{ hours }} hour<span data-ng-show="hours > 1">s</span>, ' +
        //    '{{ minutes }} minutes, ' +
        //    '{{ seconds }} seconds ' +
        //    '<span data-ng-if="millis">, milliseconds: {{millis}}</span></p>' +

        //    '<p>Interval ID: {{ intervalId  }}<br>' +
        //    'Start Time: {{ startTime | date:"mediumTime" }}<br>' +
        //    'Stopped Time: {{ stoppedTime || "Not stopped" }}</p>' +
        //    '</p>' +
        //    '<button data-ng-click="resume()" data-ng-disabled="!stoppedTime">Resume</button>' +
        //    '<button data-ng-click="stop()" data-ng-disabled="stoppedTime">Stop</button>',

        link: function (scope, elem, attrs) {
            //Properties
            scope.startTime = scope.startTimeAttr ? new Date(scope.startTimeAttr) : new Date();
            //var countdown = (scope.countdownAttr && parseInt(scope.countdownAttr, 10) > 0) ? parseInt(scope.countdownAttr, 10) : 60; //defaults to 60 seconds
            var countdown = parseInt(scope.countdownAttr, 10);

            function tick() {
                //How many milliseconds have passed: current time - start time
                scope.millis = new Date() - scope.startTime;

                // if (countdown > 0) {
                scope.millis = countdown * 1000;
                countdown++;
                // }
                //else if (countdown <= 0) {
                //    scope.stop();
                //    console.log('Your time is up!');
                //}

                scope.seconds = Math.floor((scope.millis / 1000) % 60);
                scope.minutes = Math.floor(((scope.millis / (1000 * 60)) % 60));
                scope.hours = Math.floor(((scope.millis / (1000 * 60 * 60)) % 24));
                scope.days = Math.floor(((scope.millis / (1000 * 60 * 60)) / 24));
                if (scope.days != 0)
                    scope.hours += (scope.days * 24);
                //is this necessary? is there another piece of unposted code using this?
                scope.$emit('timer-tick', {
                    intervalId: scope.intervalId,
                    millis: scope.millis
                });

                scope.$apply();

            }

            function resetInterval() {
                if (scope.intervalId) {
                    clearInterval(scope.intervalId);
                    scope.intervalId = null;
                }
            }

            scope.stop = function () {
                scope.stoppedTime = new Date();
                resetInterval();
            }

            //if not used anywhere, make it a regular function so you don't pollute the scope
            function start() {
                resetInterval();
                scope.intervalId = setInterval(tick, scope.interval);
            }

            scope.resume = function () {
                scope.stoppedTime = null;
                scope.startTime = new Date() - (scope.stoppedTime - scope.startTime);
                start();
            }

            start(); //start timer automatically

            //Watches
            scope.$on('time-start', function () {
                start();
            });

            scope.$on('timer-resume', function () {
                scope.resume();
            });

            scope.$on('timer-stop', function () {
                scope.stop();
            });

            //Cleanup
            elem.on('$destroy', function () {
                resetInterval();
            });

        }
    };
});

app.directive('clickAndDisable', function () {
    return {
        scope: {
            clickAndDisable: '&'
        },
        link: function (scope, iElement, iAttrs) {
            iElement.bind('click', function () {
                iElement.prop('disabled', true);
                scope.clickAndDisable().finally(function () {
                    iElement.prop('disabled', false);
                })
            });
        }
    };
});

app.directive('ngEnter', function () { //a directive to 'enter key press' in elements with the "ng-enter" attribute

    return function (scope, element, attrs) {

        element.bind("keydown keypress", function (event) {
            if (event.which === 13) {
                scope.$apply(function () {
                    scope.$eval(attrs.ngEnter);
                });

                event.preventDefault();
            }
        });
    };
});

app.directive("buttonPermissionBinder", function ($rootScope) {
    return {
        restrict: "A",
        scope: {
            master: '@'
        },
        link: function (scope, element, attrs) {
            setTimeout(function () {
                var permissions = $rootScope.permissions;

                if (permissions && attrs.parentname) {

                    var element = permissions.filter(function (elem) {
                        return elem.PageName.toLowerCase() == attrs.parentname.toLowerCase();
                    })[0];

                    if (attrs.parentname && attrs.pagename && element) {
                        var parentelemnet = element
                        element = parentelemnet.ChildPageDcs.filter(function (elem) {
                            return elem.PageName.toLowerCase() == attrs.pagename.toLowerCase();
                        })[0];
                    }

                    if (element && element.PeoplePageButtonDcs && element.PeoplePageButtonDcs.length > 0) {
                        element.PeoplePageButtonDcs.forEach(function (item) {
                            if (!item.Active)
                                $("." + item.ButtonClassName).remove();
                        });

                    }
                }
            }, 600);
        }
    }
});
//app.directive("filesInput", function () {
//    return {
//        require: "ngModel",
//        link: function postLink(scope, elem, attrs, ngModel) {
//            elem.on("change", function (e) {
//                var files = elem[0].files;
//                ngModel.$setViewValue(files);
//            })
//        }
//    }
//});
app.filter('noFractionCurrency', ['$filter', '$locale', function (filter, locale) {
    var currencyFilter = filter('currency');
    var formats = locale.NUMBER_FORMATS;
    return function (amount, currencySymbol) {
        amount = amount ? (amount * 1).toFixed(2) : 0.00;
        var value = currencyFilter(amount, currencySymbol);
        // split into parts
        var parts = value.split(formats.DECIMAL_SEP);
        var dollar = parts[0];
        var cents = parts[1] || '00';
        cents = cents.substring(0, 2) == '00' ? cents.substring(2) : '.' + cents; // remove "00" cent amount
        return dollar + cents;
    };
}]);


app.filter('convertToWord', function () {
    return function (amount) {
        var words = new Array();
        words[0] = '';
        words[1] = 'One';
        words[2] = 'Two';
        words[3] = 'Three';
        words[4] = 'Four';
        words[5] = 'Five';
        words[6] = 'Six';
        words[7] = 'Seven';
        words[8] = 'Eight';
        words[9] = 'Nine';
        words[10] = 'Ten';
        words[11] = 'Eleven';
        words[12] = 'Twelve';
        words[13] = 'Thirteen';
        words[14] = 'Fourteen';
        words[15] = 'Fifteen';
        words[16] = 'Sixteen';
        words[17] = 'Seventeen';
        words[18] = 'Eighteen';
        words[19] = 'Nineteen';
        words[20] = 'Twenty';
        words[30] = 'Thirty';
        words[40] = 'Forty';
        words[50] = 'Fifty';
        words[60] = 'Sixty';
        words[70] = 'Seventy';
        words[80] = 'Eighty';
        words[90] = 'Ninety';
        amount = amount.toString();
        var atemp = amount.split(".");
        var number = atemp[0].split(",").join("");
        var n_length = number.length;
        var words_string = "";
        if (n_length <= 9) {
            var n_array = new Array(0, 0, 0, 0, 0, 0, 0, 0, 0);
            var received_n_array = new Array();
            for (var i = 0; i < n_length; i++) {
                received_n_array[i] = number.substr(i, 1);
            }
            for (var i = 9 - n_length, j = 0; i < 9; i++ , j++) {
                n_array[i] = received_n_array[j];
            }
            for (var i = 0, j = 1; i < 9; i++ , j++) {
                if (i == 0 || i == 2 || i == 4 || i == 7) {
                    if (n_array[i] == 1) {
                        n_array[j] = 10 + parseInt(n_array[j]);
                        n_array[i] = 0;
                    }
                }
            }
            value = "";
            for (var i = 0; i < 9; i++) {
                if (i == 0 || i == 2 || i == 4 || i == 7) {
                    value = n_array[i] * 10;
                } else {
                    value = n_array[i];
                }
                if (value != 0) {
                    words_string += words[value] + " ";
                }
                if ((i == 1 && value != 0) || (i == 0 && value != 0 && n_array[i + 1] == 0)) {
                    words_string += "Crores ";
                }
                if ((i == 3 && value != 0) || (i == 2 && value != 0 && n_array[i + 1] == 0)) {
                    words_string += "Lakhs ";
                }
                if ((i == 5 && value != 0) || (i == 4 && value != 0 && n_array[i + 1] == 0)) {
                    words_string += "Thousand ";
                }
                if (i == 6 && value != 0 && (n_array[i + 1] != 0 && n_array[i + 2] != 0)) {
                    words_string += "Hundred and ";
                } else if (i == 6 && value != 0) {
                    words_string += "Hundred ";
                }
            }
            words_string = words_string.split("  ").join(" ");
        }
        return words_string;
    };
});


app.filter('trusted', ['$sce', function ($sce) {
    return $sce.trustAsResourceUrl;
}]);