using System;
using System.Collections.Generic;
using Cafe.Commands.Tab;
using Cafe.Events.Tab;
using Cafe.Infrastructure;
using NUnit.Framework;

namespace Cafe.Tests
{
    [TestFixture]
    public class TabTests : BddTest<TabAggregate>
    {
        [SetUp]
        public void Setup()
        {
            _testId = Guid.NewGuid();
            _testTable = 42;
            _testWaiter = "Derek";

            _testDrink1 = new OrderedItem
                          {
                              MenuNumber = 4,
                              Description = "Sprite",
                              Price = 1.50M,
                              IsDrink = true
                          };
            _testDrink2 = new OrderedItem
                          {
                              MenuNumber = 10,
                              Description = "Beer",
                              Price = 2.50M,
                              IsDrink = true
                          };

            _testFood1 = new OrderedItem
                         {
                             MenuNumber = 16,
                             Description = "Beef Noodles",
                             Price = 7.50M,
                             IsDrink = false
                         };
            _testFood2 = new OrderedItem
                         {
                             MenuNumber = 25,
                             Description = "Vegetable Curry",
                             Price = 6.00M,
                             IsDrink = false
                         };
        }

        private Guid _testId;
        private int _testTable;
        private string _testWaiter;
        private OrderedItem _testDrink1;
        private OrderedItem _testDrink2;
        private OrderedItem _testFood1;
        private OrderedItem _testFood2;

        [Test]
        public void CanCloseTabByPayingExactAmount()
        {
            Test(Given(new TabOpened
                       {
                           Id = _testId,
                           TableNumber = _testTable,
                           Waiter = _testWaiter
                       },
                       new FoodOrdered
                       {
                           Id = _testId,
                           Items = new List<OrderedItem> {_testFood1, _testFood2}
                       },
                       new FoodPrepared
                       {
                           Id = _testId,
                           MenuNumbers = new List<int> {_testFood1.MenuNumber, _testFood2.MenuNumber}
                       },
                       new FoodServed
                       {
                           Id = _testId,
                           MenuNumbers = new List<int> {_testFood2.MenuNumber, _testFood1.MenuNumber}
                       }),
                 When(new CloseTab
                      {
                          Id = _testId,
                          AmountPaid = _testFood1.Price + _testFood2.Price
                      }),
                 Then(new TabClosed
                      {
                          Id = _testId,
                          AmountPaid = _testFood1.Price + _testFood2.Price,
                          OrderValue = _testFood1.Price + _testFood2.Price,
                          TipValue = 0.00M
                      }));
        }

        [Test]
        public void CanCloseTabWithTip()
        {
            Test(Given(new TabOpened
                       {
                           Id = _testId,
                           TableNumber = _testTable,
                           Waiter = _testWaiter
                       },
                       new DrinksOrdered
                       {
                           Id = _testId,
                           Items = new List<OrderedItem> {_testDrink2}
                       },
                       new DrinksServed
                       {
                           Id = _testId,
                           MenuNumbers = new List<int> {_testDrink2.MenuNumber}
                       }),
                 When(new CloseTab
                      {
                          Id = _testId,
                          AmountPaid = _testDrink2.Price + 0.50M
                      }),
                 Then(new TabClosed
                      {
                          Id = _testId,
                          AmountPaid = _testDrink2.Price + 0.50M,
                          OrderValue = _testDrink2.Price,
                          TipValue = 0.50M
                      }));
        }

        [Test]
        public void CanNotCloseTabTwice()
        {
            Test(Given(new TabOpened
                       {
                           Id = _testId,
                           TableNumber = _testTable,
                           Waiter = _testWaiter
                       },
                       new DrinksOrdered
                       {
                           Id = _testId,
                           Items = new List<OrderedItem> {_testDrink2}
                       },
                       new DrinksServed
                       {
                           Id = _testId,
                           MenuNumbers = new List<int> {_testDrink2.MenuNumber}
                       },
                       new TabClosed
                       {
                           Id = _testId,
                           AmountPaid = _testDrink2.Price + 0.50M,
                           OrderValue = _testDrink2.Price,
                           TipValue = 0.50M
                       }),
                 When(new CloseTab
                      {
                          Id = _testId,
                          AmountPaid = _testDrink2.Price
                      }),
                 ThenFailWith<TabNotOpen>());
        }

        [Test]
        public void CanNotCloseTabWithUnpreparedFoodItems()
        {
            Test(Given(new TabOpened
                       {
                           Id = _testId,
                           TableNumber = _testTable,
                           Waiter = _testWaiter
                       },
                       new FoodOrdered
                       {
                           Id = _testId,
                           Items = new List<OrderedItem> {_testFood1}
                       }),
                 When(new CloseTab
                      {
                          Id = _testId,
                          AmountPaid = _testFood1.Price
                      }),
                 ThenFailWith<TabHasUnservedItems>());
        }

        [Test]
        public void CanNotCloseTabWithUnservedDrinksItems()
        {
            Test(Given(new TabOpened
                       {
                           Id = _testId,
                           TableNumber = _testTable,
                           Waiter = _testWaiter
                       },
                       new DrinksOrdered
                       {
                           Id = _testId,
                           Items = new List<OrderedItem> {_testDrink2}
                       }),
                 When(new CloseTab
                      {
                          Id = _testId,
                          AmountPaid = _testDrink2.Price
                      }),
                 ThenFailWith<TabHasUnservedItems>());
        }

        [Test]
        public void CanNotCloseTabWithUnservedFoodItems()
        {
            Test(Given(new TabOpened
                       {
                           Id = _testId,
                           TableNumber = _testTable,
                           Waiter = _testWaiter
                       },
                       new FoodOrdered
                       {
                           Id = _testId,
                           Items = new List<OrderedItem> {_testFood1}
                       },
                       new FoodPrepared
                       {
                           Id = _testId,
                           MenuNumbers = new List<int> {_testFood1.MenuNumber}
                       }),
                 When(new CloseTab
                      {
                          Id = _testId,
                          AmountPaid = _testFood1.Price
                      }),
                 ThenFailWith<TabHasUnservedItems>());
        }

        [Test]
        public void CanNotMarkFoodAsPreparedTwice()
        {
            Test(Given(new TabOpened
                       {
                           Id = _testId,
                           TableNumber = _testTable,
                           Waiter = _testWaiter
                       },
                       new FoodOrdered
                       {
                           Id = _testId,
                           Items = new List<OrderedItem> {_testFood1, _testFood1}
                       },
                       new FoodPrepared
                       {
                           Id = _testId,
                           MenuNumbers = new List<int> {_testFood1.MenuNumber, _testFood1.MenuNumber}
                       }),
                 When(new MarkFoodPrepared
                      {
                          Id = _testId,
                          MenuNumbers = new List<int> {_testFood1.MenuNumber}
                      }),
                 ThenFailWith<FoodNotOutstanding>());
        }

        [Test]
        public void CanNotOrderWithUnopenedTab()
        {
            Test(Given(),
                 When(new PlaceOrder
                      {
                          Id = _testId,
                          Items = new List<OrderedItem> {_testDrink1}
                      }),
                 ThenFailWith<TabNotOpen>());
        }

        [Test]
        public void CanNotServeAnOrderedDrinkTwice()
        {
            Test(Given(new TabOpened
                       {
                           Id = _testId,
                           TableNumber = _testTable,
                           Waiter = _testWaiter
                       },
                       new DrinksOrdered
                       {
                           Id = _testId,
                           Items = new List<OrderedItem> {_testDrink1}
                       },
                       new DrinksServed
                       {
                           Id = _testId,
                           MenuNumbers = new List<int> {_testDrink1.MenuNumber}
                       }),
                 When(new MarkDrinksServed
                      {
                          Id = _testId,
                          MenuNumbers = new List<int> {_testDrink1.MenuNumber}
                      }),
                 ThenFailWith<DrinksNotOutstanding>());
        }

        [Test]
        public void CanNotServeAnUnorderedDrink()
        {
            Test(Given(new TabOpened
                       {
                           Id = _testId,
                           TableNumber = _testTable,
                           Waiter = _testWaiter
                       },
                       new DrinksOrdered
                       {
                           Id = _testId,
                           Items = new List<OrderedItem> {_testDrink1}
                       }),
                 When(new MarkDrinksServed
                      {
                          Id = _testId,
                          MenuNumbers = new List<int> {_testDrink2.MenuNumber}
                      }),
                 ThenFailWith<DrinksNotOutstanding>());
        }

        [Test]
        public void CanNotServeOrderedButUnpreparedFood()
        {
            Test(Given(new TabOpened
                       {
                           Id = _testId,
                           TableNumber = _testTable,
                           Waiter = _testWaiter
                       },
                       new FoodOrdered
                       {
                           Id = _testId,
                           Items = new List<OrderedItem> {_testFood1}
                       }),
                 When(new MarkFoodServed
                      {
                          Id = _testId,
                          MenuNumbers = new List<int> {_testFood1.MenuNumber}
                      }),
                 ThenFailWith<FoodNotPrepared>());
        }

        [Test]
        public void CanNotServePreparedFoodTwice()
        {
            Test(Given(new TabOpened
                       {
                           Id = _testId,
                           TableNumber = _testTable,
                           Waiter = _testWaiter
                       },
                       new FoodOrdered
                       {
                           Id = _testId,
                           Items = new List<OrderedItem> {_testFood1, _testFood2}
                       },
                       new FoodPrepared
                       {
                           Id = _testId,
                           MenuNumbers = new List<int> {_testFood1.MenuNumber, _testFood2.MenuNumber}
                       },
                       new FoodServed
                       {
                           Id = _testId,
                           MenuNumbers = new List<int> {_testFood2.MenuNumber, _testFood1.MenuNumber}
                       }),
                 When(new MarkFoodServed
                      {
                          Id = _testId,
                          MenuNumbers = new List<int> {_testFood2.MenuNumber, _testFood1.MenuNumber}
                      }),
                 ThenFailWith<FoodNotPrepared>());
        }

        [Test]
        public void CanNotServeUnorderedFood()
        {
            Test(Given(new TabOpened
                       {
                           Id = _testId,
                           TableNumber = _testTable,
                           Waiter = _testWaiter
                       },
                       new FoodOrdered
                       {
                           Id = _testId,
                           Items = new List<OrderedItem> {_testFood1}
                       }),
                 When(new MarkFoodServed
                      {
                          Id = _testId,
                          MenuNumbers = new List<int> {_testFood2.MenuNumber}
                      }),
                 ThenFailWith<FoodNotPrepared>());
        }

        [Test]
        public void CanOpenANewTab()
        {
            Test(Given(),
                 When(new OpenTab
                      {
                          Id = _testId,
                          TableNumber = _testTable,
                          Waiter = _testWaiter
                      }),
                 Then(new TabOpened
                      {
                          Id = _testId,
                          TableNumber = _testTable,
                          Waiter = _testWaiter
                      }));
        }

        [Test]
        public void CanPlaceDrinksOrder()
        {
            Test(Given(new TabOpened
                       {
                           Id = _testId,
                           TableNumber = _testTable,
                           Waiter = _testWaiter
                       }),
                 When(new PlaceOrder
                      {
                          Id = _testId,
                          Items = new List<OrderedItem> {_testDrink1, _testDrink2}
                      }),
                 Then(new DrinksOrdered
                      {
                          Id = _testId,
                          Items = new List<OrderedItem> {_testDrink1, _testDrink2}
                      }));
        }

        [Test]
        public void CanPlaceFoodAndDrinkOrder()
        {
            Test(Given(new TabOpened
                       {
                           Id = _testId,
                           TableNumber = _testTable,
                           Waiter = _testWaiter
                       }),
                 When(new PlaceOrder
                      {
                          Id = _testId,
                          Items = new List<OrderedItem> {_testFood1, _testDrink2}
                      }),
                 Then(new DrinksOrdered
                      {
                          Id = _testId,
                          Items = new List<OrderedItem> {_testDrink2}
                      },
                      new FoodOrdered
                      {
                          Id = _testId,
                          Items = new List<OrderedItem> {_testFood1}
                      }));
        }

        [Test]
        public void CanPlaceFoodOrder()
        {
            Test(Given(new TabOpened
                       {
                           Id = _testId,
                           TableNumber = _testTable,
                           Waiter = _testWaiter
                       }),
                 When(new PlaceOrder
                      {
                          Id = _testId,
                          Items = new List<OrderedItem> {_testFood1, _testFood1}
                      }),
                 Then(new FoodOrdered
                      {
                          Id = _testId,
                          Items = new List<OrderedItem> {_testFood1, _testFood1}
                      }));
        }

        [Test]
        public void CanServePreparedFood()
        {
            Test(Given(new TabOpened
                       {
                           Id = _testId,
                           TableNumber = _testTable,
                           Waiter = _testWaiter
                       },
                       new FoodOrdered
                       {
                           Id = _testId,
                           Items = new List<OrderedItem> {_testFood1, _testFood2}
                       },
                       new FoodPrepared
                       {
                           Id = _testId,
                           MenuNumbers = new List<int> {_testFood1.MenuNumber, _testFood2.MenuNumber}
                       }),
                 When(new MarkFoodServed
                      {
                          Id = _testId,
                          MenuNumbers = new List<int> {_testFood2.MenuNumber, _testFood1.MenuNumber}
                      }),
                 Then(new FoodServed
                      {
                          Id = _testId,
                          MenuNumbers = new List<int> {_testFood2.MenuNumber, _testFood1.MenuNumber}
                      }));
        }

        [Test]
        public void FoodNotOrderedCanNotBeMarkedPrepared()
        {
            Test(Given(new TabOpened
                       {
                           Id = _testId,
                           TableNumber = _testTable,
                           Waiter = _testWaiter
                       }),
                 When(new MarkFoodPrepared
                      {
                          Id = _testId,
                          MenuNumbers = new List<int> {_testFood2.MenuNumber}
                      }),
                 ThenFailWith<FoodNotOutstanding>());
        }

        [Test]
        public void MustPayEnoughToCloseTab()
        {
            Test(Given(new TabOpened
                       {
                           Id = _testId,
                           TableNumber = _testTable,
                           Waiter = _testWaiter
                       },
                       new DrinksOrdered
                       {
                           Id = _testId,
                           Items = new List<OrderedItem> {_testDrink2}
                       },
                       new DrinksServed
                       {
                           Id = _testId,
                           MenuNumbers = new List<int> {_testDrink2.MenuNumber}
                       }),
                 When(new CloseTab
                      {
                          Id = _testId,
                          AmountPaid = _testDrink2.Price - 0.50M
                      }),
                 ThenFailWith<MustPayEnough>());
        }

        [Test]
        public void OrderedDrinksCanBeServed()
        {
            Test(Given(new TabOpened
                       {
                           Id = _testId,
                           TableNumber = _testTable,
                           Waiter = _testWaiter
                       },
                       new DrinksOrdered
                       {
                           Id = _testId,
                           Items = new List<OrderedItem> {_testDrink1, _testDrink2}
                       }),
                 When(new MarkDrinksServed
                      {
                          Id = _testId,
                          MenuNumbers = new List<int> {_testDrink1.MenuNumber, _testDrink2.MenuNumber}
                      }),
                 Then(new DrinksServed
                      {
                          Id = _testId,
                          MenuNumbers = new List<int> {_testDrink1.MenuNumber, _testDrink2.MenuNumber}
                      }));
        }

        [Test]
        public void OrderedFoodCanBeMarkedPrepared()
        {
            Test(Given(new TabOpened
                       {
                           Id = _testId,
                           TableNumber = _testTable,
                           Waiter = _testWaiter
                       },
                       new FoodOrdered
                       {
                           Id = _testId,
                           Items = new List<OrderedItem> {_testFood1, _testFood1}
                       }),
                 When(new MarkFoodPrepared
                      {
                          Id = _testId,
                          MenuNumbers = new List<int> {_testFood1.MenuNumber, _testFood1.MenuNumber}
                      }),
                 Then(new FoodPrepared
                      {
                          Id = _testId,
                          MenuNumbers = new List<int> {_testFood1.MenuNumber, _testFood1.MenuNumber}
                      }));
        }
    }
}
