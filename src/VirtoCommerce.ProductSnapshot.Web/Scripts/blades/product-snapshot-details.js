angular.module('VirtoCommerce.ProductSnapshot')
    .controller('VirtoCommerce.ProductSnapshot.ProductSnapshotDetails', [
        '$scope',
        function ($scope) {
            var blade = $scope.blade;

            blade.refresh = function () {
                blade.localizedNames = buildLocalizedNames(blade.snapshot);
                blade.dimensionFields = buildDimensionFields(blade.snapshot);
                blade.otherFields = buildOtherFields(blade.snapshot);

                blade.isLoading = false;
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
                return [
                    { label: 'Weight', value: snapshot.weight },
                    { label: 'Weight unit', value: snapshot.weightUnit },
                    { label: 'Height', value: snapshot.height },
                    { label: 'Width', value: snapshot.width },
                    { label: 'Length', value: snapshot.length },
                    { label: 'Measure unit', value: snapshot.measureUnit },
                ];
            }

            function buildOtherFields(snapshot) {
                return [
                    { label: 'Vendor', value: snapshot.vendor },
                    { label: 'Tax type', value: snapshot.taxType },
                    { label: 'Shipping type', value: snapshot.shippingType },
                    { label: 'Package type', value: snapshot.packageType },
                    { label: 'GTIN', value: snapshot.gtin },
                    { label: 'Manufacturer part #', value: snapshot.manufacturerPartNumber },
                    { label: 'Enable review', value: snapshot.enableReview },
                    { label: 'Has user agreement', value: snapshot.hasUserAgreement },
                ];
            }

            blade.refresh();
        }
    ]);
