using UnityEngine;
using System.Collections.Generic;
using Letscode.Signal;

public class PillSpawner : MonoBehaviour {
	string eventSpawn = "Spawn";
	string eventAttach = "Attach";
	bool attached = false;

	void Start ()
	{
		Mediator.Subscribe (eventAttach, Attach);
	}

	void Attach(object sender, Dictionary<string, object> args)
	{
		if (!attached) {
			attached = true;
			Mediator.Subscribe (eventSpawn, SpawnPill);
			Mediator.Unsubscribe (eventAttach, Attach);
		}
	}

	void SpawnPill (object sender, Dictionary<string, object> args)
	{
		Instantiate ((GameObject)args["objectToSpawn"], transform.position, Quaternion.identity);
	}

	void OnDestroy()
	{
		Mediator.Unsubscribe (eventSpawn, SpawnPill);
		Mediator.Unsubscribe (eventAttach, Attach);
	}
}
