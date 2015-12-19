using UnityEngine;
using System.Collections.Generic;
using Letscode.Signal;

public class PillSpawner : MonoBehaviour {
	string eventSpawn = "Spawn";
	bool attached = false;

	void Start ()
	{
		Mediator.Subscribe ("Attach", Attach);
	}

	void Attach(object sender, Dictionary<string, object> args)
	{
		if (!attached) {
			attached = true;
			Mediator.Subscribe (eventSpawn, SpawnPill);
		}
	}

	void SpawnPill (object sender, Dictionary<string, object> args)
	{
		Instantiate ((GameObject)args["objectToSpawn"], transform.position, Quaternion.identity);
	}
}
