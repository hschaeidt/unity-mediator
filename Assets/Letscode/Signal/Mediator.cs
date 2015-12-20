using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Letscode.Signal
{
	/// <summary>
	/// Complete subscriber delegate.
	/// </summary>
	public delegate void CallbackSubscriberWithSenderAndArgs(object sender, Dictionary<string, object> args);

	/// <summary>
	/// Minimal subscriber delegate with sender.
	/// </summary>
	public delegate void CallbackSubscriberWithSender(object sender);

	/// <summary>
	/// Minimal subscriber delegate with arguments.
	/// </summary>
	public delegate void CallbackSubscriberWithArgs(Dictionary<string, object> args);

	/// <summary>
	/// Minimal subscriber delegate.
	/// </summary>
	public delegate void CallbackSubscriber();

	/// <summary>
	/// Mediator is a communication interface for components.
	/// 
	/// The class implements the singleton pattern, make usage of getter Mediator.Instance to catch the active instance.
	/// 
	/// Objects can subscribe and publish messages to it. Additionally the mediator has a "per frame state",
	/// messages within the same frame will be consumed even if the event was fired before the subscriber
	/// subscribed.
	/// </summary>
	public class Mediator
	{
		/// <summary>
		/// Instance of Mediator.
		/// </summary>
		private static Mediator instance = null;

		/// <summary>
		/// The frame number from the last interaction.
		/// </summary>
		int lastFrame = 0;

		/// <summary>
		/// Dictionary containing a list of all events that have been
		/// published during this frame
		/// </summary>
		Dictionary<string, List<Dictionary<string, object>>> frameEvents;

		/// <summary>
		/// Direct subscribers call the ISubscriber SignalDispatcher method on the subscribed object.
		/// </summary>
		Dictionary<string, List<ISubscriber>> directSubscribers;

		/// <summary>
		/// Callback subscribers use the native delegate approach to handle callbacks on, for the handler anonymous, objects.
		/// </summary>
		Dictionary<string, CallbackSubscriberWithSenderAndArgs> callbackSubscribersWithSenderAndArgs;

		/// <summary>
		/// The callback subscribers with arguments.
		/// </summary>
		Dictionary<string, CallbackSubscriberWithArgs> callbackSubscribersWithArgs;

		/// <summary>
		/// The minimal callback subscribers.
		/// </summary>
		Dictionary<string, CallbackSubscriber> callbackSubscribers;

		protected Mediator()
		{
			//Init
			directSubscribers = new Dictionary<string, List<ISubscriber>> ();
			callbackSubscribersWithSenderAndArgs = new Dictionary<string, CallbackSubscriberWithSenderAndArgs> ();
			callbackSubscribersWithArgs = new Dictionary<string, CallbackSubscriberWithArgs> ();
			callbackSubscribers = new Dictionary<string, CallbackSubscriber> ();
			frameEvents = new Dictionary<string, List<Dictionary<string, object>>>();
		}

		/// <summary>
		/// Gets the instance of Mediator.
		/// </summary>
		/// <value>The instance.</value>
		public static Mediator Instance
		{
			get
			{
				if (instance == null)
					instance = new Mediator();

				return instance;
			}
		}

		/// <summary>
		/// Subscribe the specified callback to eventName.
		/// Unsubscribe must be handled manually on the objects destructor using the Mediator.Unsubscribe
		/// </summary>
		/// <param name="eventName">Event name.</param>
		/// <param name="callback">Callback.</param>
		public static void Subscribe(string eventName, CallbackSubscriberWithSenderAndArgs callback)
		{
			Instance.CallbackSubscribe (eventName, callback);
		}
		public static void Subscribe(string eventName, CallbackSubscriberWithArgs callback)
		{
			Instance.CallbackSubscribe (eventName, callback);
		}
		public static void Subscribe(string eventName, CallbackSubscriber callback)
		{
			Instance.CallbackSubscribe (eventName, callback);
		}

		/// <summary>
		/// Static subscriber wrapper for shorter syntax.
		/// </summary>
		/// <param name="eventName">Event name.</param>
		/// <param name="subscriber">Subscriber.</param>
		public static void Subscribe(string eventName, ISubscriber subscriber)
		{
			Instance.DirectSubscribe (eventName, subscriber);
		}

		/// <summary>
		/// Unsubscribe the specified callback from eventName.
		/// </summary>
		/// <param name="eventName">Event name.</param>
		/// <param name="callback">Callback.</param>
		public static void Unsubscribe(string eventName, CallbackSubscriberWithSenderAndArgs callback)
		{
			Instance.CallbackUnsubscribe (eventName, callback);
		}
		public static void Unsubscribe(string eventName, CallbackSubscriberWithArgs callback)
		{
			Instance.CallbackUnsubscribe (eventName, callback);
		}
		public static void Unsubscribe(string eventName, CallbackSubscriber callback)
		{
			Instance.CallbackUnsubscribe (eventName, callback);
		}

		/// <summary>
		/// Unsubscribe the specified subscriber from eventName.
		/// </summary>
		/// <param name="eventName">Event name.</param>
		/// <param name="subscriber">Subscriber.</param>
		public static void Unsubscribe(string eventName, ISubscriber subscriber)
		{
			Instance.DirectUnsubscribe (eventName, subscriber);
		}

		/// <summary>
		/// Subscribe the specified subscriber to eventName.
		/// </summary>
		/// <param name="eventName">Event name.</param>
		/// <param name="subscriber">Subscriber.</param>
		public void DirectSubscribe(string eventName, ISubscriber subscriber)
		{
			UpdateFrameCount ();

			if (directSubscribers.ContainsKey (eventName))
				directSubscribers [eventName].Add (subscriber);
			else {
				List<ISubscriber> list = new List<ISubscriber>();
				list.Add (subscriber);
				directSubscribers.Add (eventName, list);
			}

			UpdateSubscriber (eventName, subscriber);
		}

		/// <summary>
		/// Subscribe to a given signal with a callback. The callbacks signature must match the CallbackSubscriber delegates signature.
		/// If the signal was already published earlier this frame, the callback is instantianted additionally with the arguments in memory.
		/// </summary>
		/// <param name="eventName">Event name.</param>
		/// <param name="callback">Callback.</param>
		public void CallbackSubscribe(string eventName, CallbackSubscriberWithSenderAndArgs callback)
		{
			UpdateFrameCount ();
			if (callbackSubscribersWithSenderAndArgs.ContainsKey (eventName)) {
				callbackSubscribersWithSenderAndArgs [eventName] += callback;
			} else {
				callbackSubscribersWithSenderAndArgs [eventName] = callback;
			}

			if (WasEventPublishedThisFrame (eventName)) {
				foreach (Dictionary<string, object> item in frameEvents[eventName]) {
					callback(item["sender"], (Dictionary<string, object>)item["args"]);
				}
			}
		}
		public void CallbackSubscribe(string eventName, CallbackSubscriberWithArgs callback)
		{
			UpdateFrameCount ();
			if (callbackSubscribersWithArgs.ContainsKey (eventName)) {
				callbackSubscribersWithArgs [eventName] += callback;
			} else {
				callbackSubscribersWithArgs [eventName] = callback;
			}

			if (WasEventPublishedThisFrame (eventName)) {
				foreach (Dictionary<string, object> item in frameEvents[eventName]) {
					callback((Dictionary<string, object>)item["args"]);
				}
			}
		}
		public void CallbackSubscribe(string eventName, CallbackSubscriber callback)
		{
			UpdateFrameCount ();
			if (callbackSubscribers.ContainsKey (eventName)) {
				callbackSubscribers [eventName] += callback;
			} else {
				callbackSubscribers [eventName] = callback;
			}

			if (WasEventPublishedThisFrame (eventName)) {
				foreach (Dictionary<string, object> item in frameEvents[eventName]) {
					callback();
				}
			}
		}

		/// <summary>
		/// Unsubscribes the callback.
		/// </summary>
		/// <param name="eventName">Event name.</param>
		/// <param name="callback">Callback.</param>
		void CallbackUnsubscribe(string eventName, CallbackSubscriberWithSenderAndArgs callback)
		{
			UpdateFrameCount ();
			if (callbackSubscribersWithSenderAndArgs.ContainsKey (eventName)) {
				callbackSubscribersWithSenderAndArgs [eventName] -= callback;
			}
		}
		void CallbackUnsubscribe(string eventName, CallbackSubscriberWithArgs callback)
		{
			UpdateFrameCount ();
			if (callbackSubscribersWithArgs.ContainsKey (eventName)) {
				callbackSubscribersWithArgs [eventName] -= callback;
			}
		}
		void CallbackUnsubscribe(string eventName, CallbackSubscriber callback)
		{
			UpdateFrameCount ();
			if (callbackSubscribers.ContainsKey (eventName)) {
				callbackSubscribers [eventName] -= callback;
			}
		}

		/// <summary>
		/// Unsubscribes the subscriber.
		/// </summary>
		/// <param name="eventName">Event name.</param>
		/// <param name="subscriber">Subscriber.</param>
		void DirectUnsubscribe(string eventName, ISubscriber subscriber)
		{
			UpdateFrameCount ();
			if (directSubscribers.ContainsKey (eventName)) {
				if (directSubscribers [eventName].Contains (subscriber)) {
					directSubscribers [eventName].Remove (subscriber);
				}
			}
		}

		/// <summary>
		/// Static publisher wrapper for shorter syntax.
		/// </summary>
		/// <param name="eventName">Event name.</param>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Arguments.</param>
		public static void Publish(string eventName, object sender, Dictionary<string, object> args = null)
		{
			Instance.DirectPublish (eventName, sender, args);
			Instance.CallbackPublish (eventName, sender, args);
			Instance.AddFrameEvent (eventName, sender, args);
		}
		public static void Publish(string eventName, Dictionary<string, object> args)
		{
			Instance.DirectPublish (eventName, null, args);
			Instance.CallbackPublish (eventName, null, args);
			Instance.AddFrameEvent (eventName, null, args);
		}
		public static void Publish(string eventName)
		{
			Instance.DirectPublish (eventName, null, null);
			Instance.CallbackPublish (eventName, null, null);
			Instance.AddFrameEvent (eventName, null, null);
		}

		/// <summary>
		/// Publish the specified eventName, sender and args.
		/// Everys ISubscriber EventDispatcher will be called.
		/// </summary>
		/// <param name="eventName">Event name.</param>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Arguments.</param>
		public void DirectPublish(string eventName, object sender, Dictionary<string, object> args = null)
		{
			UpdateFrameCount ();
			List<ISubscriber> invalids = new List<ISubscriber> ();
			if (directSubscribers.ContainsKey (eventName)) {
				foreach (ISubscriber subscriber in directSubscribers[eventName]) {
					if (subscriber == null || subscriber.Equals(null)) {
						invalids.Add (subscriber);
						continue;
					}

					subscriber.EventDispatcher(eventName, sender, args);
				}
			}

			foreach (ISubscriber sub in invalids) {
				directSubscribers[eventName].Remove(sub);
			}
		}

		/// <summary>
		/// Instantiate all subscribed delegates from a given signal.
		/// </summary>
		/// <param name="eventName">Event name.</param>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Arguments.</param>
		public void CallbackPublish(string eventName, object sender, Dictionary<string, object> args = null)
		{
			UpdateFrameCount ();
			if (callbackSubscribersWithSenderAndArgs.ContainsKey (eventName)) {
				if (callbackSubscribersWithSenderAndArgs [eventName] != null)
					callbackSubscribersWithSenderAndArgs [eventName] (sender, args);
			}
			if (callbackSubscribersWithArgs.ContainsKey (eventName)) {
				if (callbackSubscribersWithArgs [eventName] != null)
					callbackSubscribersWithArgs [eventName] (args);
			}
			if (callbackSubscribers.ContainsKey (eventName)) {
				if (callbackSubscribers [eventName] != null)
					callbackSubscribers [eventName] ();
			}
		}

		/// <summary>
		/// Updates the subscribers with events that may have occured earlier this frame.
		/// </summary>
		/// <param name="eventName">Event name.</param>
		void UpdateSubscriber(string eventName, ISubscriber subscriber)
		{
			if (frameEvents.ContainsKey (eventName) && directSubscribers.ContainsKey(eventName)) {
				foreach (Dictionary<string, object> item in frameEvents[eventName]) {
					subscriber.EventDispatcher(
						eventName,
						item["sender"],
						(Dictionary<string, object>)item["args"]
					);
				}
			}
		}

		/// <summary>
		/// Checks if the event was already published this frame.
		/// </summary>
		/// <param name="eventName">Event name.</param>
		bool WasEventPublishedThisFrame(string eventName)
		{
			return frameEvents.ContainsKey (eventName);
		}

		/// <summary>
		/// Adds the event available through the current frame for subscribers coming too late.
		/// </summary>
		/// <param name="eventName">Event name.</param>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Arguments.</param>
		public void AddFrameEvent(string eventName, object sender, Dictionary<string, object> args = null)
		{
			if (!WasEventPublishedThisFrame (eventName)) {
				frameEvents [eventName] = new List<Dictionary<string, object>>();
			}

			frameEvents[eventName].Add (new Dictionary<string, object> {
				{ "sender", sender },
				{ "args", args }
			});
		}

		/// <summary>
		/// Updates the frame count and clears the frameEvents in case it changed.
		/// </summary>
		void UpdateFrameCount()
		{
			// We need this here because Time methods access native code which is not available
			// while testing. It is recommended to only disable it for unit testing or if you want
			// to have full control over eventing without being frame dependant
			if (!ignoreFrameCount) {
				if (Time.frameCount > lastFrame) {
					lastFrame = Time.frameCount;
					frameEvents.Clear ();
				}
			}
		}

		/// <summary>
		/// Use with caution!
		/// It is only used by unit-tests in our use-cases!
		/// </summary>
		public void ClearFrameEvents()
		{
			frameEvents.Clear ();
		}

		/// <summary>
		/// Ignores the frame count.
		/// Used to disable frameCounting. Use it only if you want to have a frame independant Mediator instance.
		/// </summary>
		public bool ignoreFrameCount = false;
	}
}