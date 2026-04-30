angular.module('VirtoCommerce.ProductSnapshot')
    .factory('VirtoCommerce.ProductSnapshot.webApi', ['$resource', function ($resource) {
        return $resource('api/product-snapshots/:id', { id: '@Id' }, {
            getByOrderAndProductId: { url: 'api/product-snapshots/order/:orderId/product/:productId' },
        });
    }]);
