using System;

namespace Cafe.Commands.Tab
{
    public class OpenTab
    {
        public Guid Id;

        public int TableNumber { get; set; }

        public string Waiter { get; set; }
    }
}
