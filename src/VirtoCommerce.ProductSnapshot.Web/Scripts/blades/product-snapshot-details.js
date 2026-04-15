angular.module('VirtoCommerce.ProductSnapshot')
    .controller('VirtoCommerce.ProductSnapshot.ProductSnapshotDetails', [
        '$scope',
        'platformWebApp.metaFormsService',
        'VirtoCommerce.ProductSnapshot.webApi',
        function ($scope, metaFormsService, snapshotResource) {
            var blade = $scope.blade;

            blade.metaFields = metaFormsService.getMetaFields("productSnapshotDetails");

            blade.refresh = function () {
                blade.isLoading = true;
                blade.notFound = false;
                blade.snapshot = null;

                snapshotResource.getByOrderAndProductId(
                    { orderId: blade.orderId, productId: blade.productId },
                    function (snapshot) {
                        if (snapshot && snapshot.id) {
                            blade.snapshot = snapshot;
                            blade.snapshotId = snapshot.id;
                            blade.localizedNames = buildLocalizedNames(snapshot);
                            blade.dimensionFields = buildDimensionFields(snapshot);
                            blade.otherFields = buildOtherFields(snapshot);
                            blade.notFound = false;
                        } else {
                            blade.notFound = true;
                        }
                        blade.isLoading = false;
                    },
                    function () {
                        blade.notFound = true;
                        blade.isLoading = false;
                    }
                );
            };

            function buildLocalizedNames(snapshot) {
                var values = snapshot.localizedName && snapshot.localizedName.values
                    ? snapshot.localizedName.values
                    : {};
                return Object.keys(values).map(function (lang) {
                    return { lang: lang, value: values[lang] };
                });
            }

            function buildDimensionFields(snapshot) {
                var p = 'ProductSnapshot.blades.product-snapshot-details.labels.';
                return [
                    { label: p + 'weight', value: snapshot.weight },
                    { label: p + 'weight-unit', value: snapshot.weightUnit },
                    { label: p + 'height', value: snapshot.height },
                    { label: p + 'width', value: snapshot.width },
                    { label: p + 'length', value: snapshot.length },
                    { label: p + 'measure-unit', value: snapshot.measureUnit },
                ];
            }

            function buildOtherFields(snapshot) {
                var p = 'ProductSnapshot.blades.product-snapshot-details.labels.';
                return [
                    { label: p + 'vendor', value: snapshot.vendor },
                    { label: p + 'tax-type', value: snapshot.taxType },
                    { label: p + 'shipping-type', value: snapshot.shippingType },
                    { label: p + 'package-type', value: snapshot.packageType },
                    { label: p + 'gtin', value: snapshot.gtin },
                    { label: p + 'manufacturer-part', value: snapshot.manufacturerPartNumber },
                    { label: p + 'enable-review', value: snapshot.enableReview },
                    { label: p + 'has-user-agreement', value: snapshot.hasUserAgreement },
                ];
            }

            blade.refresh();
        }
    ]);
