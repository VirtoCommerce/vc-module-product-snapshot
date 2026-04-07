angular.module('VirtoCommerce.ProductSnapshot')
    .controller('VirtoCommerce.ProductSnapshot.ProductSnapshotDetails', [
        '$scope',
        '$translate',
        'platformWebApp.bladeNavigationService',
        'platformWebApp.settings',
        'virtoCommerce.catalogModule.items',
        'virtoCommerce.customerModule.members',
        'virtoCommerce.catalogModule.catalogs',
        'platformWebApp.metaFormsService',
        function (
            $scope,
            $translate,
            bladeNavigationService,
            settings,
            items,
            members,
            catalogs,
            metaFormsService
        ) {
            var blade = $scope.blade;

            blade.currentEntityId = blade.itemId;
            blade.isReadonly = true;
            blade.hasVendorsPermission = bladeNavigationService.checkPermission('customer:read');

            function makeReadonlyMetaFields(metaFormName) {
                var fields = angular.copy(metaFormsService.getMetaFields(metaFormName) || []);

                angular.forEach(fields, function (field) {
                    field.isReadOnly = true;

                    if (field.templateUrl) {
                        field.templateUrl = field.templateUrl.replace('.html', '-readonly.html');
                    }
                });

                return fields;
            }

            blade.metaFields = makeReadonlyMetaFields('productDetail');
            blade.metaFields1 = makeReadonlyMetaFields('productDetail1');
            blade.metaFields2 = makeReadonlyMetaFields('productDetail2');

            blade.refresh = function () {
                // product snapshot is already loaded
                var snapshot = blade.snapshot;

                catalogs.get({ id: snapshot.catalogId }, function (catalogResult) {
                    blade.catalog = catalogResult;
                    fillItem(snapshot);
                }, function (error) {
                    bladeNavigationService.setError('Error ' + error.status, blade);
                });
            };

            function fillItem(data) {
                blade.itemId = data.id;
                blade.title = data.code;
                blade.securityScopes = data.securityScopes;

                if (!data.productType) {
                    data.productType = 'Physical';
                }

                blade.subtitle = 'catalog.blades.item-detail.subtitle';
                blade.subtitleValues = {
                    productTypeName: $translate.instant('catalog.product-types.' + data.productType)
                };

                var linkWithPriority = getLinkWithPriority(data);
                data._priority = (linkWithPriority ? linkWithPriority.priority : data.priority) || 0;

                blade.item = angular.copy(data);
                blade.currentEntity = blade.item;
                blade.origItem = angular.copy(data);
                blade.isLoading = false;
            }

            function getLinkWithPriority(data) {
                var retVal;

                if (bladeNavigationService.catalogsSelectedCatalog && bladeNavigationService.catalogsSelectedCatalog.isVirtual) {
                    retVal = _.find(data.links, function (l) {
                        return l.catalogId === bladeNavigationService.catalogsSelectedCatalog.id &&
                            (!bladeNavigationService.catalogsSelectedCategoryId ||
                                l.categoryId === bladeNavigationService.catalogsSelectedCategoryId);
                    });
                }

                return retVal;
            }

            blade.fetchVendors = function (criteria) {
                return blade.hasVendorsPermission
                    ? members.search(criteria)
                    : criteria.objectIds.map(function (x) {
                        return {
                            id: x,
                            name: $translate.instant('catalog.blades.item-detail.labels.vendor-denied')
                        };
                    });
            };

            blade.taxTypes = settings.getValues({ id: 'VirtoCommerce.Core.General.TaxTypes' });

            blade.toolbarCommands = [];
            blade.onClose = function (closeCallback) {
                closeCallback();
            };

            blade.refresh();
        }
    ]);
