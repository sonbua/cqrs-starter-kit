using System;
using System.Collections.Generic;

namespace Cafe.Events.Tab
{
    public class FoodOrdered
    {
        public Guid Id;

        public List<OrderedItem> Items;
    }
}
