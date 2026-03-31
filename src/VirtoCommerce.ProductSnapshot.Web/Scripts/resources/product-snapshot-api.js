angular.module('VirtoCommerce.ProductSnapshot')
    .factory('VirtoCommerce.ProductSnapshot.webApi', ['$resource', function ($resource) {
        return $resource('api/product-snapshot');
    }]);
