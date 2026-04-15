// Call this to register your module to main application
var moduleName = 'VirtoCommerce.ProductSnapshot';

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .run(['platformWebApp.bladeNavigationService',
        'platformWebApp.toolbarService',
        function (bladeNavigationService, toolbarService) {
            // Register toolbar button on order line item detail blade
            toolbarService.register({
                name: 'ProductSnapshot.blades.product-snapshot-details.labels.product-snapshot',
                icon: 'fa fa-camera',
                executeMethod: function (blade) {
                    var item = blade.currentEntity;
                    var orderId = blade.order.id;

                    var newBlade = {
                        id: 'productSnapshotDetail',
                        controller: 'VirtoCommerce.ProductSnapshot.ProductSnapshotDetails',
                        template: 'Modules/$(VirtoCommerce.ProductSnapshot)/Scripts/blades/product-snapshot-details.html',
                        hideToolbar: false,
                        title: item.name,
                        orderId: orderId,
                        productId: item.productId
                    };

                    bladeNavigationService.showBlade(newBlade, blade);
                },
                canExecuteMethod: function (blade) {
                    return blade && blade.currentEntity && blade.currentEntity.productId;
                },
                index: 3
            }, 'virtoCommerce.orderModule.customerOrderItemDetailController');
        }
    ]);
