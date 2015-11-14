using System;

namespace Cafe.Events.Tab
{
    public class TabClosed
    {
        public Guid Id;

        public decimal AmountPaid;

        public decimal OrderValue;

        public decimal TipValue;
    }
}
