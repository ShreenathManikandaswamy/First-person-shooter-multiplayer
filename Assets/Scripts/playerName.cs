using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class playerName : MonoBehaviour {

	public Text nameTag;




	[PunRPC]
	public void updateName(string name, int health){
		nameTag.text = name + " / " + health;
		Debug.Log (nameTag.text);
	}

}
