(function() {
    'use strict';

    angular
        .module('fileUploaderApp')
        .controller('fileUploaderController', fileUploaderController);

    fileUploaderController.$inject = ['modelService', '$scope', '$window', '$timeout' ];

    function fileUploaderController(modelService, $scope, $window, $timeout) {
        var vm = this;

        $.connection.hub.start().done(function () {
            console.log("Connection Done");
        });

        vm.documentOriginalColumns = [];
        vm.accessibleColumns = [];
        vm.testMessage = "";
        vm.validationMessage = "";
        vm.alertShow = false;
        vm.fileName = "";
        vm._csvArrays = [];
        vm._uploaderHub = null;
        vm.totalProgress = 100;
        vm.currentProgress = 0;
        vm.uploadShow = false;
        vm._uploaderHub = $.connection.uploaderHub;
        var tableDb = {
            tableName: "",
            uploadsColumnName: [],
            records: []
        };

        function initialize(file) {
            vm._csvArrays = $.csv.toObjects(file);
            vm.totalProgress = vm._csvArrays.length;
            modelService.getData().then(function(response) {
                vm.accessibleColumns = response.data;
            });

            vm.documentOriginalColumns = Object.keys(vm._csvArrays[0]).map(function(colName) {
                return { originalName: colName, newName: "NoMapped", success: false, upload: false }
            });

            vm.validationMessage = "Require fields: SKU; Brand; Price;";
            vm.alertShow = true;
        }
        function getAllIndicesFromArray(arr, val) {
            var indices = [];
            for (var i = 0; i < arr.length; i++) {
                if (arr[i].newName === val)
                    indices.push(i);
            }
            return indices;
        }
        function checkToUpload() {
            var allUploadsColumn = vm.accessibleColumns.filter(elem => elem.upload);
            var index = -1;

            vm.documentOriginalColumns.forEach(function(element) {
                index = allUploadsColumn.findIndex(elem => elem.name === element.newName);

                if (index !== -1)
                    element.upload = true;
                else
                    element.upload = false;
            });
        }
        function checkToAlone() {
            var allRequiredsColumn = vm.accessibleColumns.filter(elem => elem.alone);
            var indices = [];

            for (var i = 0; i < allRequiredsColumn.length; i++) {
                indices = getAllIndicesFromArray(vm.documentOriginalColumns, allRequiredsColumn[i].name);

                if (indices.length > 1) {
                    indices.forEach(function(element) {
                        vm.documentOriginalColumns[element].success = false;
                    });
                    vm.uploadShow = false;
                } else if (indices.length === 1) {
                    vm.documentOriginalColumns[indices[0]].success = true;
                }
            }
        }
        function checkToRequire() {
            var allRequiredsColumn = vm.accessibleColumns.filter(elem => elem.required);
            var index = -1;
            var noCheckedNames = [];

            for (var i = 0; i < allRequiredsColumn.length; i++) {
                index = vm.documentOriginalColumns.findIndex(elem => elem.newName === allRequiredsColumn[i].name);
                if (index === -1) {
                    noCheckedNames.push(allRequiredsColumn[i].name);
                }
            }

            if (noCheckedNames.length !== 0) {
                vm.validationMessage = "Require fields: " + noCheckedNames.join();
                vm.alertShow = true;
            } else {
                vm.alertShow = false;
            }

            vm.uploadShow = !vm.alertShow;
        }
        function getUploadColumns() {
            var namesForChanges = vm.accessibleColumns.filter(elem => elem.upload && !elem.alone);
            var newUploadsName = vm.documentOriginalColumns.filter(elem => elem.upload).map(function(elem) {
                return angular.copy(elem);
            });

            for (var i = 0; i < namesForChanges.length; i++) {
                var indices = getAllIndicesFromArray(newUploadsName, namesForChanges[i].name);
                if (indices.length > 1)
                    for (var j = 0; j < indices.length; j++) {
                        newUploadsName[indices[j]].newName += (j + 1);
                    }
            }
            return newUploadsName;
        }
        function guid() {
            function s4() {
                return Math.floor((1 + Math.random()) * 0x10000)
                    .toString(16)
                    .substring(1);
            }
            return s4() + s4() + s4() + s4() + s4() + s4();
        }
        function nullInitialisation() {
            vm.documentOriginalColumns = [];
            vm.accessibleColumns = [];
            vm.testMessage = "";
            vm.validationMessage = "";
            vm.alertShow = false;
            vm.fileName = "";
            vm._csvArrays = [];
            vm.totalProgress = 100;
            vm.currentProgress = 0;
            vm.uploadShow = false;
            vm.fileName = "";
        }
        function finishUpload() {
            alert("All records was written");
            $.connection.hub.stop();
            nullInitialisation();
        }
        function saveToDb(startIndex, capacity) {
            tableDb.records = vm._csvArrays.slice(startIndex, startIndex + capacity).map(function (element) {
                return tableDb.uploadsColumnName.filter(elem => elem.upload).map(function (el) {
                    return {
                        columnName: el.newName.replace(/\s/g, '_'),
                        columnValue: element[el.originalName]
                    };
                });

            });

            vm._uploaderHub
                .server
                .saveData({ startId: startIndex, tableName: tableDb.tableName, records: tableDb.records})
                .done(function (response) {
                    var responseMessage = JSON.parse(response);
                    if (responseMessage.isSuccessful) {
                        console.log(responseMessage.message);
                        vm.currentProgress = startIndex;
                        $scope.$apply();
                        if (startIndex < vm._csvArrays.length)
                            saveToDb(startIndex + capacity, capacity);
                        else
                            $timeout(finishUpload, 800);
                    } else {
                        console.log(responseMessage.message);
                        vm.validationMessage = responseMessage.message + " Please change your file.";
                        vm.alertShow = true;
                        vm.uploadShow = false;
                        $.connection.hub.stop();
                        $scope.$apply();
                    }
                });
        }
        function uploadToServer() {
            vm._uploaderHub.server.connect().done(function () {
                vm._uploaderHub.server.checkConnection().done(function (response) {
                    var responseMessage = JSON.parse(response);
                    if (responseMessage.isSuccessful) {
                        console.log(responseMessage.message);
                        
                        saveToDb(0, 100);
                    } else {
                        console.log(responseMessage.message);
                        vm.validationMessage = responseMessage.message + "\n Please, refresh page and try again";
                        vm.alertShow = true;
                        vm.uploadShow = false;
                        $.connection.hub.stop();
                    }
                    $scope.$apply();
                });
            });
        }

        vm.onSelect = function(newIndex) {
            var newName = vm.documentOriginalColumns[newIndex].newName;
            var isRequire = vm.accessibleColumns.findIndex(elem => elem.name === newName && elem.required);

            if (isRequire === -1)
                vm.documentOriginalColumns[newIndex].success = true;

            checkToRequire();
            checkToAlone();
            checkToUpload();
        }
        vm.onUpload = function() {
            if ($.connection.hub.state === $.signalR.connectionState.disconnected) {
                $.connection.hub.start().done(function() {
                    vm._uploaderHub = $.connection.uploaderHub;
                    uploadToServer();
                });
            } else {
                uploadToServer();
            }
            vm.currentProgress = 100;
            tableDb.tableName = vm.fileName.replace(/\s|[\-\.]/g, '_') + "_" + guid();
            tableDb.uploadsColumnName = getUploadColumns();
        }
        vm.fileOnLoad = function (file, fileName) {
            nullInitialisation();
            if (!/(\.csv)$/.test(fileName)) {
                vm.validationMessage = "Require files only *.csv";
                vm.alertShow = true;
                return;
            }
            vm.fileName = fileName;
            initialize(file);
        }
        vm._uploaderHub.client.reponseMessage = function (message) {
            console.log(message);
            $scope.$apply();
        };
    }
})();
