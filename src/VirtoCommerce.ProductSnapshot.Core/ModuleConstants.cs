using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.ProductSnapshot.Core;

public static class ModuleConstants
{
    public static class Security
    {
        public static class Permissions
        {
            public const string Access = "product-snapshot:access";
            public const string Create = "product-snapshot:create";
            public const string Read = "product-snapshot:read";
            public const string Update = "product-snapshot:update";
            public const string Delete = "product-snapshot:delete";

            public static string[] AllPermissions { get; } =
            [
                Access,
                Create,
                Read,
                Update,
                Delete,
            ];
        }
    }

    public static class Settings
    {
        public static class General
        {
            public static SettingDescriptor ProductSnapshotEnabled { get; } = new()
            {
                Name = "ProductSnapshot.Enabled",
                GroupName = "ProductSnapshot|General",
                ValueType = SettingValueType.Boolean,
                DefaultValue = false,
            };

            public static IEnumerable<SettingDescriptor> AllGeneralSettings
            {
                get
                {
                    yield return ProductSnapshotEnabled;
                }
            }
        }

        public static IEnumerable<SettingDescriptor> AllSettings
        {
            get
            {
                return General.AllGeneralSettings;
            }
        }
    }
}
