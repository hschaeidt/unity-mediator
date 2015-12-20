# unity-mediator
Lightweight message mediator for seamless communication between components.

# Concept
The components are communicating with each other without knowing about the message publisher directly.
In other words the Mediator abstracts the relation between objects and ensures message delivery within the same frame.

## Anonymous communication
The receiver of an event does not know about those publisher. Only the Mediator knows and connects them together. This resolves the issue of hard class dependencies in the code.

## Frame based communication
The main problem with event handlers if you register them too late (outside from Awake()) it may happen that the invoked delegate may not receive an early fired event. The need to handle events inside of the Awake() method, project wide, caused that no more game-functionality can be described in this method.

### The problem
```
// In this example we can never fire an event in another MonoBehaviours Awake without fixing manually the execution order.
void Awake()
{
  EventHandler += DoSomething;
  EventHandler += DoSomethingElse;
}
```

We solved this problem by keeping in memory all "signals" that have been fired within one same frame. Those "signals" will be passed to subscribers that registered on the "signal"-name within this same frame. This eliminates the need to register subscribers within the Awake() and the Awake() can be used again for actual game-functionality.

### The solution
```
using Letscode.Signal;

// We publish the signal before any subscribe happened
void Awake()
{
  Mediator.Publish("MySignal");
}

// We register the subscriber within the same frame (Awake and Start are executed in the same frame on initializing)
void Start()
{
  Mediator.Subscribe("MySignal", DoSomething);
}

// This method will be called in this example
void DoSomething(){}
```
