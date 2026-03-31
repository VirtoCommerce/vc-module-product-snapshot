angular.module('VirtoCommerce.ProductSnapshot')
    .controller('VirtoCommerce.ProductSnapshot.helloWorldController', ['$scope', 'VirtoCommerce.ProductSnapshot.webApi', function ($scope, api) {
        var blade = $scope.blade;
        blade.title = 'ProductSnapshot';

        blade.refresh = function () {
            api.get(function (data) {
                blade.title = 'ProductSnapshot.blades.hello-world.title';
                blade.data = data.result;
                blade.isLoading = false;
            });
        };

        blade.refresh();
    }]);
