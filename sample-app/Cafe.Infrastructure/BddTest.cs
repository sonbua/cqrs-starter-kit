using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Cafe.Core;
using NUnit.Framework;

namespace Cafe.Infrastructure
{
    /// <summary>
    /// Provides infrastructure for a set of tests on a given aggregate.
    /// </summary>
    /// <typeparam name="TAggregate"></typeparam>
    public class BddTest<TAggregate> where TAggregate : Aggregate, new()
    {
        private TAggregate _sut;

        [SetUp]
        public void BddTestSetup()
        {
            _sut = new TAggregate();
        }

        protected void Test(IEnumerable given, Func<TAggregate, object> when, Action<object> then)
        {
            then(when(ApplyEvents(_sut, given)));
        }

        protected IEnumerable Given(params object[] events)
        {
            return events;
        }

        protected Func<TAggregate, object> When<TCommand>(TCommand command)
        {
            return agg =>
                   {
                       try
                       {
                           return DispatchCommand(command).Cast<object>().ToArray();
                       }
                       catch (Exception e)
                       {
                           return e;
                       }
                   };
        }

        protected Action<object> Then(params object[] expectedEvents)
        {
            return got =>
                   {
                       var gotEvents = got as object[];
                       if (gotEvents != null)
                       {
                           if (gotEvents.Length == expectedEvents.Length)
                           {
                               for (var i = 0; i < gotEvents.Length; i++)
                               {
                                   if (gotEvents[i].GetType() == expectedEvents[i].GetType())
                                   {
                                       Assert.AreEqual(Serialize(expectedEvents[i]), Serialize(gotEvents[i]));
                                   }
                                   else
                                   {
                                       Assert.Fail("Incorrect event in results; expected a {0} but got a {1}", expectedEvents[i].GetType().Name, gotEvents[i].GetType().Name);
                                   }
                               }
                           }
                           else if (gotEvents.Length < expectedEvents.Length)
                           {
                               Assert.Fail("Expected event(s) missing: {0}", string.Join(", ", EventDiff(expectedEvents, gotEvents)));
                           }
                           else
                           {
                               Assert.Fail("Unexpected event(s) emitted: {0}", string.Join(", ", EventDiff(gotEvents, expectedEvents)));
                           }
                       }
                       else if (got is CommandHandlerNotDefiendException)
                       {
                           Assert.Fail(((Exception) got).Message);
                       }
                       else
                       {
                           Assert.Fail("Expected events, but got exception {0}", got.GetType().Name);
                       }
                   };
        }

        private string[] EventDiff(object[] a, object[] b)
        {
            var diff = a.Select(e => e.GetType().Name).ToList();
            foreach (var remove in b.Select(e => e.GetType().Name))
            {
                diff.Remove(remove);
            }
            return diff.ToArray();
        }

        protected Action<object> ThenFailWith<TException>()
        {
            return got =>
                   {
                       if (got is TException)
                       {
                           Assert.Pass("Got correct exception type");
                       }
                       else if (got is CommandHandlerNotDefiendException)
                       {
                           Assert.Fail(((Exception) got).Message);
                       }
                       else if (got is Exception)
                       {
                           Assert.Fail("Expected exception {0}, but got exception {1}", typeof (TException).Name, got.GetType().Name);
                       }
                       else
                       {
                           Assert.Fail("Expected exception {0}, but got event result", typeof (TException).Name);
                       }
                   };
        }

        private IEnumerable DispatchCommand<TCommand>(TCommand c)
        {
            var handler = _sut as IHandleCommand<TCommand>;

            if (handler == null)
            {
                throw new CommandHandlerNotDefiendException($"Aggregate {_sut.GetType().Name} does not yet handle command {c.GetType().Name}");
            }

            return handler.Handle(c);
        }

        private TAggregate ApplyEvents(TAggregate aggregate, IEnumerable events)
        {
            aggregate.ApplyEvents(events);
            return aggregate;
        }

        private string Serialize(object obj)
        {
            var ser = new XmlSerializer(obj.GetType());
            var ms = new MemoryStream();
            ser.Serialize(ms, obj);
            ms.Seek(0, SeekOrigin.Begin);
            return new StreamReader(ms).ReadToEnd();
        }

        private class CommandHandlerNotDefiendException : Exception
        {
            public CommandHandlerNotDefiendException(string msg) : base(msg)
            {
            }
        }
    }
}
