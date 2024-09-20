'use strict';
app.controller('CreditInvoiceController', ['$scope', "$filter", '$http', '$routeParams', '$modal',
    function ($scope, $filter, $http, $routeParams, $modal) {
        $scope.Id = $routeParams.Id;

        if ($scope.Id > 0) {
            GetCreditInvoiceData();
        }
        function GetCreditInvoiceData() {
            
            var url = serviceBase + "api/PurchaseOrderNew/GetCreditNoteInvoice?Id=" + $scope.Id + "&PoId=" + $routeParams.PoNumber;
            $http.get(url).success(function (data) {
                

                $scope.IRMasterId = data.IRCreditNoteMasterDc.IRMasterId;
                $scope.SupplierName = data.IRCreditNoteMasterDc.SupplierName;
                $scope.SupplierAddress = data.IRCreditNoteMasterDc.SupplierAddress;
                $scope.SupplierGstNo = data.IRCreditNoteMasterDc.SupplierGstNo;
                $scope.SupplierState = data.IRCreditNoteMasterDc.SupplierState;
                $scope.Id = data.IRCreditNoteMasterDc.Id;
                $scope.CreatedDate = data.IRCreditNoteMasterDc.CreatedDate;
                $scope.CompanyName = data.IRCreditNoteMasterDc.CompanyName;
                $scope.CompanyAddress = data.IRCreditNoteMasterDc.CompanyAddress;
                $scope.CompanyGstInNo = data.IRCreditNoteMasterDc.CompanyGstInNo;
                $scope.CNNumber = data.IRCreditNoteMasterDc.CNNumber;
                $scope.InvoiceNumber = data.IRCreditNoteMasterDc.InvoiceNumber;
                $scope.InvoiceCreatedDate = data.IRCreditNoteMasterDc.InvoiceCreatedDate;  
                $scope.PoNumber = $routeParams.PoNumber;
                $scope.TotalShortQty = data.TotalShortQty;
                $scope.TotalDamageQty = data.TotalDamageQty;
                $scope.TotalExpiryQty = data.TotalExpiryQty;
                $scope.TotalQty = data.TotalQty;
                $scope.TotalAmountAfterTax = data.TotalAmountAfterTax;
                $scope.TotalTaxablevalue = data.TotalTaxablevalue;
                $scope.TotalSgstAmount = data.TotalSgstAmount;
                $scope.TotalCgstAmount = data.TotalCgstAmount;
                $scope.TotalIgstAmount = data.TotalIgstAmount;
                $scope.TotalAmountBeforeTax = data.TotalAmountBeforeTax;
                $scope.TotalCessAmount = data.TotalCessAmount;
                $scope.TotalDiscount = data.TotalDiscount;
                $scope.Creditnotedetails = data.IRCreditNoteDetailDc;
                $scope.Poid = data.Poid

            });
        };

        //$scope.Download = function () {

        //    html2canvas(document.getElementById('tblcreditinvoice'), {
        //        onrendered: function (canvas) {
        //            console.log(canvas);
        //            var data = canvas.toDataURL();
        //            var docDefinition = {
        //                content: [{
        //                    image: data,
        //                    width: 500
        //                }]
        //            };
        //            pdfMake.createPdf(docDefinition).download("Table.pdf");
        //        }
        //    });
        //}

        $scope.printReciept = function (content) {
            PrintReciept(content);
        }

        $scope.Back = function () {
            window.location = "#/IRNew?id=" + $routeParams.PoNumber;
        }
        function PrintReciept(contents) {

            var PrintContent = $(contents).html();
            var frame1 = $('<iframe />');
            frame1[0].name = "frame1";
            //frame1.css({ "position": "absolute", "top": "-1000000px" });
            $("body").append(frame1);
            var frameDoc = frame1[0].contentWindow ? frame1[0].contentWindow : frame1[0].contentDocument.document ? frame1[0].contentDocument.document : frame1[0].contentDocument;
            frameDoc.document.open();
            //Create a new HTML document.
            //frameDoc.document.write('<html><head><title>Reciept</title>');
            //frameDoc.document.write('<link href="/Dental/CSS/Print.css" rel="stylesheet" type="text/css" />');

            //frameDoc.document.write('<style>@page {size: auto;margin-bottom:10px;}a[href]:after{content:none!important}#btnsubmit{display:none;}</style>');
            //frameDoc.document.write('</head><body>');
            frameDoc.document.write(PrintContent);


            frameDoc.document.write('</body></html>');
            frameDoc.document.close();

            setTimeout(function () {
                window.frames["frame1"].focus();
                window.frames["frame1"].print();
                frame1.remove();
            }, 500);

        };
    }
]);