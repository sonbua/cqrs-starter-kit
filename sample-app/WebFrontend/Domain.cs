using Cafe.Commands.Tab;
using Cafe.Core;
using Cafe.Infrastructure;
using Cafe.ReadModels.Tab;

namespace Cafe.Web
{
    public static class Domain
    {
        public static MessageDispatcher Dispatcher;
        public static IOpenTabQueries OpenTabQueries;
        public static IChefTodoListQueries ChefTodoListQueries;

        public static void Setup()
        {
            Dispatcher = new MessageDispatcher(new InMemoryEventStore());

            Dispatcher.ScanInstance(new TabAggregate());

            OpenTabQueries = new OpenTabs();
            Dispatcher.ScanInstance(OpenTabQueries);

            ChefTodoListQueries = new ChefTodoList();
            Dispatcher.ScanInstance(ChefTodoListQueries);
        }
    }
}
