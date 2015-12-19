using UnityEngine;
using System.Collections.Generic;
using Letscode.Signal;

public class Button : MonoBehaviour {
	public GameObject toSpawnOnPress;
	string eventSpawn = "Spawn";

	public void OnClick()
	{
		Mediator.Publish (eventSpawn, gameObject, new Dictionary<string, object> {
			{"objectToSpawn", toSpawnOnPress}
		});
	}
}
