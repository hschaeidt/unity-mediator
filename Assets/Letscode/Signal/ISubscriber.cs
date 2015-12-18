using System;
using System.Collections.Generic;

namespace Letscode.Signal
{
	/// <summary>
	/// ISubscriber specifies the implementation of event subscribers.
	/// </summary>
	public interface ISubscriber
	{
		/// <summary>
		/// EventDispatcher is a collective callback for any fired event.
		/// 
		/// The dispatching has to be done manually in case the ISubscriber is subscribed to multiple events.
		/// A good way would be a if/else or switch on eventName.
		/// </summary>
		/// <param name="eventName">Event name.</param>
		/// <param name="sender">Sender.</param>
		void EventDispatcher(string eventName, object sender, Dictionary<string, object> args);
	}
}

