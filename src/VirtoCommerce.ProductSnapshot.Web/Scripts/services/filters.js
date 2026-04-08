angular.module('VirtoCommerce.ProductSnapshot')
    .filter('truncate', function () {
        return function (text, size) {
            return text ? text.slice(0, size) + '…' : '';
        };
    })
    .filter('truncateStart', function () {
        return function (name, size) {
            if (!name) {
                return '';
            }

            return name.length > size ? '…' + name.slice(2 + size * -1) : name;
        };
    })
    .filter('sanitize', ['$sanitize', function ($sanitize) {
        return function (html) {
            if (!html) {
                return '';
            }

            return $sanitize(html);
        };
    }]);
