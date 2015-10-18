angular.module("Help")
    .directive("usermanage", () => {
        return {
            templateUrl: "/parts/usermanage.html",
            scope: false
        }
    });