using System.Collections.Generic;

namespace Cafe.ReadModels.Tab
{
    public interface IChefTodoListQueries
    {
        List<ChefTodoList.TodoListGroup> GetTodoList();
    }
}
