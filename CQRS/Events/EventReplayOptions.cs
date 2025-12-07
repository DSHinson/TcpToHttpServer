using System;
using System.Collections.Generic;
using System.Text;

namespace CQRS.Events
{
    [Flags]
    public enum EventReplayOptions
    {
        None = 0,

        /// <summary>
        /// Indicates the event is safe to replay and will behave deterministically.
        /// </summary>
        Replayable = 1 << 0,

        /// <summary>
        /// Indicates the event mutates application state or data.
        /// </summary>
        MutatesData = 1 << 1,

        /// <summary>
        /// Indicates the event depends on real-time data (e.g., current time, external APIs).
        /// </summary>
        TimeSensitive = 1 << 2,

        /// <summary>
        /// Indicates the event should never be replayed automatically.
        /// </summary>
        DoNotReplay = 1 << 3
    }
}
