/// <reference path="/Scripts/angular.js"/>

var app = angular.module("Help", ["ngAnimate", "ngCookies", "zlFeatures", "zlSignalR", "keypress"]);
app.filter("keylength", function () {
    return function (input) {
        if (!angular.isObject(input)) {
            throw Error("Usage of non-objects with keylength filter!!");
        }
        return Object.keys(input).length;
    };
}).filter("toArray", function () {
    return function (obj) {
        if (!(obj instanceof Object)) {
            return obj;
        }
        return Object.keys(obj).map(function (key) { return Object.defineProperty(obj[key], "$key", { __proto__: null, value: key }); });
    };
}).filter("countdown", function () {
    return function (num) {
        if (num >= 0) {
            var min = parseInt(num / 60);
            min = (min < 10 ? "0" : "") + min;

            var sec = parseInt(num % 60);
            sec = (sec < 10 ? "0" : "") + sec;

            return min + ":" + sec;
        } else {
            var min = parseInt(num / 60);
            if (min == 0) {
                min = "-0" + min;
            } else {
                if (min > -10) {
                    var m = Math.abs(min);
                    min = "-0" + m;
                }

            }
            var sec = Math.abs(parseInt(num % 60));

            return min + ":" + sec;
        }
    }
});