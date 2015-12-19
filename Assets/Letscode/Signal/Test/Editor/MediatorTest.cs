using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSubstitute;
using NUnit.Framework;

namespace Letscode.Signal.Test
{
	public class MediatorTest
	{
		public interface IDummyGameObject
		{
			void DoSomething (object sender, Dictionary<string, object> args);
		}

		object sender = null;
		Dictionary<string, object> args = new Dictionary<string, object> {
			{ "stringkey", "stringValue" },
			{ "intkey", 1 }
		};

		[Test]
		public void EventIsDispatched_WhenEventIsPublished()
		{
			// Given
			string eventName = "TestSignal";
			ISubscriber subscriber = Substitute.For<ISubscriber> ();
			Mediator.Instance.ignoreFrameCount = true;
			Mediator.Subscribe (eventName, subscriber);

			// When
			Mediator.Publish (eventName, sender, args);

			// Then
			subscriber.Received(1).EventDispatcher(eventName, sender, args);
		}

		[Test]
		public void EventIsDispatched_WhenSubscribedAfterPublishWithinSameFrame()
		{
			// Given
			string eventName = "FastSignal";
			ISubscriber subscriber = Substitute.For<ISubscriber> ();
			Mediator.Instance.ignoreFrameCount = true;

			// When
			Mediator.Publish (eventName, sender, args);
			Mediator.Subscribe (eventName, subscriber);

			// Then
			subscriber.Received(1).EventDispatcher(eventName, sender, args);
		}

		[Test]
		public void EventIsNotDispatched_WhenSubscribedOnNextFrame()
		{
			// Given
			string eventName = "TooFastSignal";
			ISubscriber subscriber = Substitute.For<ISubscriber> ();
			Mediator.Instance.ignoreFrameCount = true;

			// When
			Mediator.Publish (eventName, sender, args);
			// simulates jump to next frame
			Mediator.Instance.ClearFrameEvents ();
			Mediator.Subscribe (eventName, subscriber);

			// Then
			subscriber.DidNotReceiveWithAnyArgs().EventDispatcher(eventName, sender, args);
		}

		[Test]
		public void EventIsDispatchedToCallback_WhenEventWasPublished()
		{
			// Given
			string eventName = "Signal";
			IDummyGameObject dgo = Substitute.For<IDummyGameObject>();
			Mediator.Instance.ignoreFrameCount = true;

			// When
			Mediator.Subscribe(eventName, dgo.DoSomething);
			Mediator.Publish (eventName, sender, args);

			// Then
			dgo.Received(1).DoSomething(sender, args);
		}

		[Test]
		public void EventIsDispatchedToCallback_WhenSubscribedAfterPublishWithinSameFrame()
		{
			// Given
			string eventName = "LateConnect";
			IDummyGameObject dgo = Substitute.For<IDummyGameObject> ();
			Mediator.Instance.ignoreFrameCount = true;

			// When
			Mediator.Publish (eventName, sender, args);
			Mediator.Subscribe (eventName, dgo.DoSomething);

			// Then
			dgo.Received(1).DoSomething (sender, args);
		}

		[Test]
		public void EventIsNotDispatched_WhenFrameChangesAfterPublishBeforeConnect()
		{
			// Given
			string eventName = "TooLateConnected";
			IDummyGameObject dgo = Substitute.For<IDummyGameObject> ();
			Mediator.Instance.ignoreFrameCount = true;

			// When
			Mediator.Publish(eventName, sender, args);
			Mediator.Instance.ClearFrameEvents ();
			Mediator.Subscribe (eventName, dgo.DoSomething);

			// Then
			dgo.DidNotReceiveWithAnyArgs().DoSomething(sender, args);
		}

		[Test]
		public void EventIsDispatchedToMultipleSubscribers()
		{
			// Given
			string eventName = "MultiDispatchEvent";
			IDummyGameObject dgo1 = Substitute.For<IDummyGameObject> ();
			IDummyGameObject dgo2 = Substitute.For<IDummyGameObject> ();
			IDummyGameObject dgo3 = Substitute.For<IDummyGameObject> ();
			ISubscriber sub1 = Substitute.For<ISubscriber> ();
			ISubscriber sub2 = Substitute.For<ISubscriber> ();
			ISubscriber sub3 = Substitute.For<ISubscriber> ();
			Mediator.Instance.ignoreFrameCount = true;

			// When
			Mediator.Subscribe(eventName, dgo1.DoSomething);
			Mediator.Subscribe (eventName, sub1);
			Mediator.Publish (eventName, sender, args);
			Mediator.Subscribe (eventName, dgo2.DoSomething);
			Mediator.Subscribe (eventName, sub2);
			Mediator.Instance.ClearFrameEvents ();
			Mediator.Subscribe (eventName, dgo3.DoSomething);
			Mediator.Subscribe (eventName, sub3);

			// Then
			dgo1.Received(1).DoSomething(sender, args);
			dgo2.Received (1).DoSomething (sender, args);
			sub1.Received (1).EventDispatcher (eventName, sender, args);
			sub2.Received (1).EventDispatcher (eventName, sender, args);
			dgo3.DidNotReceiveWithAnyArgs ().DoSomething (sender, args);
			sub3.DidNotReceiveWithAnyArgs ().EventDispatcher (eventName, sender, args);
		}

		[Test]
		public void MultipleEventsAreDispatchedToSameSubscribers()
		{
			// Given
			string eventName = "EventDispatchedMultipleTimes";
			IDummyGameObject dgo1 = Substitute.For<IDummyGameObject> ();
			ISubscriber sub1 = Substitute.For<ISubscriber> ();
			IDummyGameObject lateCallbackSub = Substitute.For<IDummyGameObject> ();
			ISubscriber lateDirectSub = Substitute.For<ISubscriber> ();
			Mediator.Instance.ignoreFrameCount = true;

			// When
			Mediator.Subscribe(eventName, dgo1.DoSomething);
			Mediator.Subscribe (eventName, sub1);
			Mediator.Publish (eventName, sender, args);
			Mediator.Instance.ClearFrameEvents ();
			Mediator.Subscribe (eventName, lateDirectSub);
			Mediator.Subscribe (eventName, lateCallbackSub.DoSomething);
			Mediator.Publish (eventName, sender, args);

			// Then
			dgo1.Received(2).DoSomething(sender, args);
			sub1.Received (2).EventDispatcher (eventName, sender, args);
			lateCallbackSub.Received (1).DoSomething (sender, args);
			lateDirectSub.Received (1).EventDispatcher (eventName, sender, args);
		}
	}
}