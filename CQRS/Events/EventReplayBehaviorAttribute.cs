using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.Events
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EventReplayBehaviorAttribute : Attribute
    {
        public EventReplayOptions Options { get; }

        public EventReplayBehaviorAttribute(EventReplayOptions options)
        {
            Options = options;
        }
    }
}
