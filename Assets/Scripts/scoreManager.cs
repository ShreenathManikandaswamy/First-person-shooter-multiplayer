using UnityEngine;
using System.Collections;

public class scoreManager : MonoBehaviour {


	public InRoomChat irc;


	[PunRPC]
	public void addFeed(string feed){
		if (GetComponent<PhotonView> ().isMine) {
			irc.addKill (feed);
		}
	}

	[PunRPC]
	public void deleteBot(){
		GameObject[] bots = GameObject.FindGameObjectsWithTag("Ai");
		Destroy (bots [Random.Range (0, bots.Length)]);
	}



}
