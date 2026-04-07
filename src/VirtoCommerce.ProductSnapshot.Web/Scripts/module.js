// Call this to register your module to main application
var moduleName = 'VirtoCommerce.ProductSnapshot';

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .config(['$stateProvider',
        function ($stateProvider) {
            $stateProvider
                .state('workspace.ProductSnapshotState', {
                    url: '/product-snapshot',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: [
                        'platformWebApp.bladeNavigationService',
                        function (bladeNavigationService) {
                            var newBlade = {
                                id: 'blade1',
                                controller: 'VirtoCommerce.ProductSnapshot.helloWorldController',
                                template: 'Modules/$(VirtoCommerce.ProductSnapshot)/Scripts/blades/hello-world.html',
                                isClosingDisabled: true,
                            };
                            bladeNavigationService.showBlade(newBlade);
                        }
                    ]
                });
        }
    ])
    .run(['platformWebApp.mainMenuService',
        '$state',
        '$q',
        'VirtoCommerce.ProductSnapshot.webApi',
        'platformWebApp.bladeNavigationService',
        'virtoCommerce.orderModule.productBladeResolver',
        function (mainMenuService, $state, $q, snapshotResource, bladeNavigationService, productBladeResolver) {
            //Register module in main menu
            var menuItem = {
                path: 'browse/product-snapshot',
                icon: 'fa fa-cube',
                title: 'ProductSnapshot',
                priority: 100,
                action: function () { $state.go('workspace.ProductSnapshotState'); },
                permission: 'product-snapshot:access',
            };
            mainMenuService.addMenuItem(menuItem);

            productBladeResolver.registerHandler(function (context) {
                var deferred = $q.defer();
                var item = context.item;
                var order = context.blade.order;
                var parentBlade = context.blade;

                snapshotResource.getByOrderAndProductId(
                    { orderId: order.id, productId: item.productId },
                    function (snapshot) {
                        if (snapshot && snapshot.id) {
                            var newBlade = {
                                id: 'productSnapshotDetail',
                                controller: 'VirtoCommerce.ProductSnapshot.ProductSnapshotDetails',
                                template: 'Modules/$(VirtoCommerce.ProductSnapshot)/Scripts/blades/product-snapshot-details.html',
                                title: item.name,
                                snapshot: snapshot,
                                snapshotId: snapshot.id,
                                productId: item.productId
                            };

                            bladeNavigationService.showBlade(newBlade, parentBlade);
                            deferred.resolve(true);   // handled
                        } else {
                            deferred.resolve(false);  // continue chain
                        }
                    },
                    function () {
                        deferred.resolve(false);      // fall back on error
                    }
                );

                return deferred.promise;
            }, 100); // higher priority than default
        }
    ]);
