/**
 * Api controller
 * @param {Angular.Scope} $scope 
 * @param {Application} application 
 * @returns {} 
 */
var apiCtrl = function ($scope, application) {
    $scope.application = application;
    application.isApi = true;
}

angular.module("Help")
    .controller("ApiCtrl", ["$scope", "zlApplication", apiCtrl]);