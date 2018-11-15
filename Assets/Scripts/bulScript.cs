using UnityEngine;
using System.Collections;

public class bulScript : MonoBehaviour {

	public int dmg;
	public string name;

	void Awake(){
		gameObject.name = "bullet";
	}

	void OnCollisionEnter(){
		Destroy (gameObject);
	}

}
