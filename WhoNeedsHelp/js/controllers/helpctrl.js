var l;
angular.module("Help")
    .controller("HelpCtrl", [
        "$scope", "zlApplication", function($scope, application) {
            $scope.application = application;
            l = $scope;

            $scope.$watch("application", function() {
                console.log("Updated");
            });
        }
    ]);