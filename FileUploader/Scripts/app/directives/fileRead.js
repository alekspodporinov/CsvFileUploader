(function () {
    'use strict';

    angular.module('fileUploaderApp').directive('fileRead', fileRead);

    function fileRead() {

        var directive = {
            link: link,
            restrict: 'A',
            scope: {
                textFileLoad: "&"
               // fileName: "="
            }
        };
        return directive;

        function link(scope, element) {
            element.bind("change", function (changeEvent) {
                var reader = new FileReader();
                reader.onload = (function (file) {
                    return function(evt) {
                        scope.$apply(function() {
                            var fileLoad = evt.target.result;
                            //scope.fileName = file.name;
                            scope.textFileLoad({ file: fileLoad, fileName: file.name });
                        });
                    }
                })(changeEvent.target.files[0]);
                reader.readAsText(changeEvent.target.files[0]);
            });
        }
    }

})();