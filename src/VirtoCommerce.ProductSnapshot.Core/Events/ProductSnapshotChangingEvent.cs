using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.ProductSnapshot.Core.Events;

public class ProductSnapshotChangingEvent(IEnumerable<GenericChangedEntry<Models.OrderProductSnapshot>> changedEntries)
    : GenericChangedEntryEvent<Models.OrderProductSnapshot>(changedEntries);
