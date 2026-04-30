using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.ProductSnapshot.Core.Events;

public class ProductSnapshotChangedEvent(IEnumerable<GenericChangedEntry<Models.OrderProductSnapshot>> changedEntries)
    : GenericChangedEntryEvent<Models.OrderProductSnapshot>(changedEntries);
