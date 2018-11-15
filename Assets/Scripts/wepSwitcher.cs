using UnityEngine;
using System.Collections;

public class wepSwitcher : MonoBehaviour {

	public GameObject[] weps;

	void Awake(){
		changeWep (0);
	}

	void Update(){

		if(Input.GetKeyDown (KeyCode.Alpha1)){
			changeWep (0);
		}

		if(Input.GetKeyDown (KeyCode.Alpha2)){
			changeWep (1);
		}

		if(Input.GetKeyDown (KeyCode.Alpha3)){
			changeWep (2);
		}


	}

	public void changeWep(int seq){
		disableAll ();
		weps [seq].SetActive (true);
	}

	public void disableAll(){
		foreach (GameObject wep in weps) {
			wep.SetActive (false);
		}
	}

}
