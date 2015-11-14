using System;
using System.Collections.Generic;
using System.Linq;
using Cafe.Core;
using Cafe.Events.Tab;

namespace Cafe.ReadModels.Tab
{
    public class OpenTabs : IOpenTabQueries,
                            ISubscribeTo<TabOpened>,
                            ISubscribeTo<DrinksOrdered>,
                            ISubscribeTo<FoodOrdered>,
                            ISubscribeTo<FoodPrepared>,
                            ISubscribeTo<DrinksServed>,
                            ISubscribeTo<FoodServed>,
                            ISubscribeTo<TabClosed>
    {
        private readonly Dictionary<Guid, Tab> _todoByTab = new Dictionary<Guid, Tab>();

        public List<int> ActiveTableNumbers()
        {
            lock (_todoByTab)
                return _todoByTab.Select(tab => tab.Value.TableNumber).OrderBy(i => i).ToList();
        }

        public Dictionary<int, List<TabItem>> TodoListForWaiter(string waiter)
        {
            lock (_todoByTab)
                return _todoByTab.Where(tab => tab.Value.Waiter == waiter)
                                 .Select(tab => new
                                                {
                                                    TableNumber = tab.Value.TableNumber,
                                                    ToServe = CopyItems(tab.Value, t => t.ToServe)
                                                })
                                 .Where(t => t.ToServe.Count > 0)
                                 .ToDictionary(k => k.TableNumber, v => v.ToServe);
        }

        public Guid TabIdForTable(int table)
        {
            lock (_todoByTab)
                return _todoByTab.Where(tab => tab.Value.TableNumber == table)
                                 .Select(tab => tab.Key)
                                 .First();
        }

        public TabStatus TabForTable(int table)
        {
            lock (_todoByTab)
                return _todoByTab.Where(tab => tab.Value.TableNumber == table)
                                 .Select(tab => new TabStatus
                                                {
                                                    TabId = tab.Key,
                                                    TableNumber = tab.Value.TableNumber,
                                                    ToServe = CopyItems(tab.Value, t => t.ToServe),
                                                    InPreparation = CopyItems(tab.Value, t => t.InPreparation),
                                                    Served = CopyItems(tab.Value, t => t.Served)
                                                })
                                 .First();
        }

        public TabInvoice InvoiceForTable(int table)
        {
            KeyValuePair<Guid, Tab> tab;

            lock (_todoByTab)
                tab = _todoByTab.First(t => t.Value.TableNumber == table);

            lock (tab.Value)
                return new TabInvoice
                       {
                           TabId = tab.Key,
                           TableNumber = tab.Value.TableNumber,
                           Items = new List<TabItem>(tab.Value.Served),
                           Total = tab.Value.Served.Sum(i => i.Price),
                           HasUnservedItems = tab.Value.InPreparation.Any() || tab.Value.ToServe.Any()
                       };
        }

        public void Handle(TabOpened e)
        {
            lock (_todoByTab)
                _todoByTab.Add(e.Id, new Tab
                                     {
                                         TableNumber = e.TableNumber,
                                         Waiter = e.Waiter,
                                         ToServe = new List<TabItem>(),
                                         InPreparation = new List<TabItem>(),
                                         Served = new List<TabItem>()
                                     });
        }

        public void Handle(DrinksOrdered e)
        {
            AddItems(e.Id,
                     e.Items.Select(drink => new TabItem
                                             {
                                                 MenuNumber = drink.MenuNumber,
                                                 Description = drink.Description,
                                                 Price = drink.Price
                                             }),
                     t => t.ToServe);
        }

        public void Handle(FoodOrdered e)
        {
            AddItems(e.Id,
                     e.Items.Select(drink => new TabItem
                                             {
                                                 MenuNumber = drink.MenuNumber,
                                                 Description = drink.Description,
                                                 Price = drink.Price
                                             }),
                     t => t.InPreparation);
        }

        public void Handle(FoodPrepared e)
        {
            MoveItems(e.Id, e.MenuNumbers, t => t.InPreparation, t => t.ToServe);
        }

        public void Handle(DrinksServed e)
        {
            MoveItems(e.Id, e.MenuNumbers, t => t.ToServe, t => t.Served);
        }

        public void Handle(FoodServed e)
        {
            MoveItems(e.Id, e.MenuNumbers, t => t.ToServe, t => t.Served);
        }

        public void Handle(TabClosed e)
        {
            lock (_todoByTab)
                _todoByTab.Remove(e.Id);
        }

        private List<TabItem> CopyItems(Tab tableTodo, Func<Tab, List<TabItem>> selector)
        {
            lock (tableTodo)
                return new List<TabItem>(selector(tableTodo));
        }

        private Tab GetTab(Guid id)
        {
            lock (_todoByTab)
                return _todoByTab[id];
        }

        private void AddItems(Guid tabId, IEnumerable<TabItem> newItems, Func<Tab, List<TabItem>> to)
        {
            var tab = GetTab(tabId);
            lock (tab)
                to(tab).AddRange(newItems);
        }

        private void MoveItems(Guid tabId, List<int> menuNumbers,
                               Func<Tab, List<TabItem>> from, Func<Tab, List<TabItem>> to)
        {
            var tab = GetTab(tabId);
            lock (tab)
            {
                var fromList = from(tab);
                var toList = to(tab);

                foreach (var num in menuNumbers)
                {
                    var serveItem = fromList.First(f => f.MenuNumber == num);
                    fromList.Remove(serveItem);
                    toList.Add(serveItem);
                }
            }
        }

        public class TabItem
        {
            public int MenuNumber;
            public string Description;
            public decimal Price;
        }

        public class TabStatus
        {
            public Guid TabId;
            public int TableNumber;
            public List<TabItem> ToServe;
            public List<TabItem> InPreparation;
            public List<TabItem> Served;
        }

        public class TabInvoice
        {
            public Guid TabId;
            public int TableNumber;
            public List<TabItem> Items;
            public decimal Total;
            public bool HasUnservedItems;
        }

        private class Tab
        {
            public int TableNumber;
            public string Waiter;
            public List<TabItem> ToServe;
            public List<TabItem> InPreparation;
            public List<TabItem> Served;
        }
    }
}
