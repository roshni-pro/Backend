//'use strict';
//app.factory('getset', function () {
//    //$http, ngAuthSettings
//    var tempdata = [];
//    var data = {};
//    return data = {
//        Getdata: function () {
//            return tempdata;
//        },
//        Setdata: function (data1) {
//            tempdata = data1;
//        },

//        reset: function () {
//            tempdata = {};
//        }
//    };

//})

(function () {
    'use strict';

    angular
        .module('app')
        .factory('getset', getset);

    getset.$inject = ['$http'];

    function getset($http) {
        var tempdata = [];
        var data = {};
        return data = {
            Getdata: function () {
                return tempdata;
            },
            Setdata: function (data1) {
                tempdata = data1;
            },

            reset: function () {
                tempdata = {};
            }
        };
    }
})();