using System;
using System.Collections.Generic;
using Akka.Persistence;
using Akka.Persistence.DocumentDb;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace sample_akka_persistence
{
    public class Command
    {
        public string Data { get; private set; }
        public Command(string data)
        {
            Data = data;
        }
    }

    public class Event
    {
        public string Data { get; private set; }
        public Event(string evt)
        {
            Data = evt;
        }
    }

    public class SampleState
    {
        public readonly IList<string> _events;
        public int Size => _events.Count;
        public SampleState(IList<string> events)
        {
            _events = events;
        }

        public SampleState() : this(new List<string>())
        {

        }

        public void Update(string str)
        {
            _events.Add(str);
        }

        public SampleState Copy()
        {
            return new SampleState(_events);
        }
    }

    public class SamplePersistenceActor : ReceivePersistentActor
    {
        private SampleState _state = new SampleState();
        private readonly string id;
        public SamplePersistenceActor(string id) : this()
        {
            this.id = id;
        }

        public SamplePersistenceActor()
        {
            Recover<Event>(evt =>
            {
                _state.Update(evt.Data);
            });

            Recover<SnapshotOffer>(snap =>
            {
                _state = snap.ToObject<SampleState>(); 
                //Extension method to deserialize the stored object
            });

            Command<Command>(message =>
            {
                string data = message.Data;
                Event evt1 = new Event($"{data}-{_state.Size}");
                Event evt2 = new Event($"{data}-{_state.Size + 1}");

                var events = new List<Event> { evt1, evt2 };

                PersistAll(events, evt =>
                {
                    _state.Update(evt.Data);
                    if (evt == evt2)
                    {
                        Context.System.EventStream.Publish(evt);
                    }
                });
            });

            Command<string>(msg => msg == "snap", message =>
            {
                SaveSnapshot(_state.Copy());
            });

            Command<string>(msg => msg == "print", message =>
            {
                Console.WriteLine(_state);
            });
        }

        public override string PersistenceId => id;
    }
}
