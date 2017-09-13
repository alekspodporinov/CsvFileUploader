(function() {

    angular.module("fileUploaderApp").service('modelService', modelService);

    modelService.$inject = ['$http'];

    function modelService($http) {
        this.getData = function () {
            return $http({
                method: 'GET',
                url: '/home/getaccessiblecolumns'
            });
        }
    }
})();