using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cafe.Core;
using Cafe.Events.Tab;

namespace Cafe.Commands.Tab
{
    public class TabAggregate : Aggregate,
                                IHandleCommand<OpenTab>,
                                IHandleCommand<PlaceOrder>,
                                IHandleCommand<MarkDrinksServed>,
                                IHandleCommand<MarkFoodPrepared>,
                                IHandleCommand<MarkFoodServed>,
                                IHandleCommand<CloseTab>,
                                IApplyEvent<TabOpened>,
                                IApplyEvent<DrinksOrdered>,
                                IApplyEvent<FoodOrdered>,
                                IApplyEvent<DrinksServed>,
                                IApplyEvent<FoodPrepared>,
                                IApplyEvent<FoodServed>,
                                IApplyEvent<TabClosed>
    {
        private readonly List<OrderedItem> _outstandingDrinks = new List<OrderedItem>();
        private readonly List<OrderedItem> _outstandingFood = new List<OrderedItem>();
        private readonly List<OrderedItem> _preparedFood = new List<OrderedItem>();
        private bool _open;
        private decimal _servedItemsValue;

        public IEnumerable Handle(OpenTab c)
        {
            yield return new TabOpened
                         {
                             Id = c.Id,
                             TableNumber = c.TableNumber,
                             Waiter = c.Waiter
                         };
        }

        public IEnumerable Handle(PlaceOrder c)
        {
            if (!_open)
            {
                throw new TabNotOpen();
            }

            var drink = c.Items.Where(i => i.IsDrink).ToList();
            if (drink.Any())
            {
                yield return new DrinksOrdered
                             {
                                 Id = c.Id,
                                 Items = drink
                             };
            }

            var food = c.Items.Where(i => !i.IsDrink).ToList();
            if (food.Any())
            {
                yield return new FoodOrdered
                             {
                                 Id = c.Id,
                                 Items = food
                             };
            }
        }

        public IEnumerable Handle(MarkDrinksServed c)
        {
            if (!AreDrinksOutstanding(c.MenuNumbers))
                throw new DrinksNotOutstanding();

            yield return new DrinksServed
            {
                Id = c.Id,
                MenuNumbers = c.MenuNumbers
            };
        }

        public IEnumerable Handle(MarkFoodPrepared c)
        {
            if (!IsFoodOutstanding(c.MenuNumbers))
                throw new FoodNotOutstanding();

            yield return new FoodPrepared
            {
                Id = c.Id,
                MenuNumbers = c.MenuNumbers
            };
        }

        public IEnumerable Handle(MarkFoodServed c)
        {
            if (!IsFoodPrepared(c.MenuNumbers))
                throw new FoodNotPrepared();

            yield return new FoodServed
            {
                Id = c.Id,
                MenuNumbers = c.MenuNumbers
            };
        }

        public IEnumerable Handle(CloseTab c)
        {
            if (!_open)
                throw new TabNotOpen();
            if (HasUnservedItems())
                throw new TabHasUnservedItems();
            if (c.AmountPaid < _servedItemsValue)
                throw new MustPayEnough();

            yield return new TabClosed
            {
                Id = c.Id,
                AmountPaid = c.AmountPaid,
                OrderValue = _servedItemsValue,
                TipValue = c.AmountPaid - _servedItemsValue
            };
        }

        private bool AreDrinksOutstanding(List<int> menuNumbers)
        {
            return AreAllInList(want: menuNumbers, have: _outstandingDrinks);
        }

        private bool IsFoodOutstanding(List<int> menuNumbers)
        {
            return AreAllInList(want: menuNumbers, have: _outstandingFood);
        }

        private bool IsFoodPrepared(List<int> menuNumbers)
        {
            return AreAllInList(want: menuNumbers, have: _preparedFood);
        }

        private static bool AreAllInList(List<int> want, List<OrderedItem> have)
        {
            var curHave = new List<int>(have.Select(i => i.MenuNumber));
            foreach (var num in want)
                if (curHave.Contains(num))
                    curHave.Remove(num);
                else
                    return false;
            return true;
        }

        public bool HasUnservedItems()
        {
            return _outstandingDrinks.Any() || _outstandingFood.Any() || _preparedFood.Any();
        }

        public void Apply(TabOpened e)
        {
            _open = true;
        }

        public void Apply(DrinksOrdered e)
        {
            _outstandingDrinks.AddRange(e.Items);
        }

        public void Apply(FoodOrdered e)
        {
            _outstandingFood.AddRange(e.Items);
        }

        public void Apply(DrinksServed e)
        {
            foreach (var num in e.MenuNumbers)
            {
                var item = _outstandingDrinks.First(d => d.MenuNumber == num);
                _outstandingDrinks.Remove(item);
                _servedItemsValue += item.Price;
            }
        }

        public void Apply(FoodPrepared e)
        {
            foreach (var num in e.MenuNumbers)
            {
                var item = _outstandingFood.First(f => f.MenuNumber == num);
                _outstandingFood.Remove(item);
                _preparedFood.Add(item);
            }
        }

        public void Apply(FoodServed e)
        {
            foreach (var num in e.MenuNumbers)
            {
                var item = _preparedFood.First(f => f.MenuNumber == num);
                _preparedFood.Remove(item);
                _servedItemsValue += item.Price;
            }
        }

        public void Apply(TabClosed e)
        {
            _open = false;
        }
    }
}
