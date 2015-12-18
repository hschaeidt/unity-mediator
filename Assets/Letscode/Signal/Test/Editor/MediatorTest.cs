using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSubstitute;
using UnityEngine;
using NUnit.Framework;

namespace Letscode.Signal.Test
{
	public class MediatorTest
	{

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
	}
}