using System;
using System.Collections.Generic;
using System.Linq;
using Cafe.Core;
using Cafe.Events.Tab;

namespace Cafe.ReadModels.Tab
{
    public class ChefTodoList : IChefTodoListQueries,
                                ISubscribeTo<FoodOrdered>,
                                ISubscribeTo<FoodPrepared>
    {
        private readonly List<TodoListGroup> _todoList = new List<TodoListGroup>();

        public List<TodoListGroup> GetTodoList()
        {
            lock (_todoList)
            {
                return _todoList.Select(grp => new TodoListGroup
                                               {
                                                   Tab = grp.Tab,
                                                   Items = new List<TodoListItem>(grp.Items)
                                               })
                                .ToList();
            }
        }

        public void Handle(FoodOrdered e)
        {
            var group = new TodoListGroup
                        {
                            Tab = e.Id,
                            Items = new List<TodoListItem>(e.Items.Select(i => new TodoListItem
                                                                               {
                                                                                   MenuNumber = i.MenuNumber,
                                                                                   Description = i.Description
                                                                               }))
                        };

            lock (_todoList)
                _todoList.Add(group);
        }

        public void Handle(FoodPrepared e)
        {
            lock (_todoList)
            {
                var group = _todoList.First(g => g.Tab == e.Id);

                foreach (var num in e.MenuNumbers)
                {
                    group.Items.Remove(group.Items.First(i => i.MenuNumber == num));
                }

                if (group.Items.Count == 0)
                {
                    _todoList.Remove(group);
                }
            }
        }

        public class TodoListItem
        {
            public int MenuNumber;
            public string Description;
        }

        public class TodoListGroup
        {
            public Guid Tab;
            public List<TodoListItem> Items;
        }
    }
}
