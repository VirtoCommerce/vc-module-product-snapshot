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
    .run(['platformWebApp.mainMenuService', '$state',
        function (mainMenuService, $state) {
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
        }
    ]);
