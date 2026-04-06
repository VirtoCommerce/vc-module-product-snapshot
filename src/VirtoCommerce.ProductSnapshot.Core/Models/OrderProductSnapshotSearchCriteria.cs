using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ProductSnapshot.Core.Models;

public class OrderProductSnapshotSearchCriteria : SearchCriteriaBase
{
    public IList<string> OrderIds { get; set; }
}
