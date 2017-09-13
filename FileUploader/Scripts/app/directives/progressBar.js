(function () {
    'use strict';

    angular
        .module('fileUploaderApp')
        .directive('progressBar', progressBar);

    function progressBar() {
        var directive = {
            link: link,
            restrict: "A",
            scope: {
                total: "=",
                current: "=",
                finish: "&"
            }
        };
        return directive;

        function link(scope, element) {
            scope.$watch("current",
                function (value) {
                    var val = scope.current / scope.total * 100;
                    element.css("width", val + "%");
                    if (val >= 100)
                        scope.finish();
                });
            scope.$watch("total",
                function(value) {
                    element.css("width", scope.current / scope.total * 100 + "%");
                });
        }

    }
})();