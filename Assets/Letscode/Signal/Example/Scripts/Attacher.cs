using UnityEngine;
using System.Collections.Generic;
using Letscode.Signal;

public class Attacher : MonoBehaviour {
	string eventSpawn = "Attach";

	public void OnClick()
	{
		Mediator.Publish (eventSpawn, gameObject, null);
	}

	public void Hide()
	{
		gameObject.SetActive (false);
	}
}
