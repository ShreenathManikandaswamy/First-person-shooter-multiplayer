using UnityEngine;
using System.Collections;

public class roomMan : Photon.MonoBehaviour {

	public string verNum = "0.1";
	public string roomName = "room01";
	public string playerName = "player 420";
	public Transform spawnPoint;
	public GameObject playerPref;
	public GameObject playerPref2;
	public bool isConnected = false;
	public bool isInRoom = false;
	public GameObject[] Ais;
	public GameObject[] curAis;
	public int kD;



	public InRoomChat chat;

	public Transform[] spawnPoints;


	void Update(){
		if (isInRoom) {
			chat.enabled = true;
		} else {
			//chat.enabled = false;
		}

		curAis = GameObject.FindGameObjectsWithTag ("Ai");

		if (Input.GetKeyDown (KeyCode.E) && isInRoom == false) {
			spawnAi ();
		}
		if (PlayerPrefs.GetInt ("kills") >= 1) {
			kD = PlayerPrefs.GetInt ("kills") / PlayerPrefs.GetInt ("deaths"); 
		} else {
			kD = 0;
		}

		PhotonNetwork.player.SetScore (PlayerPrefs.GetInt ("kills"));
	}

	void Start(){

		roomName = "Room " + Random.Range (0, 999);
		playerName = "Player " + Random.Range (0, 999);
		PhotonNetwork.ConnectUsingSettings (verNum);
		Debug.Log ("Starting Connection!");

		
		//PlayerPrefs.SetInt ("kills", 0);
		//PlayerPrefs.SetInt ("deaths", 0);

	}

	public void OnJoinedLobby(){
		//PhotonNetwork.JoinOrCreateRoom (roomName, null, null);
		isConnected = true;
		Debug.Log ("Starting Server!");
	}

	public void OnJoinedRoom(){
		PhotonNetwork.playerName = playerName;

		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		foreach(GameObject pl in players){
			if(pl.name == PhotonNetwork.playerName){
				PhotonNetwork.playerName = playerName + " " + Random.Range (0,999);
			}
		}


		isConnected = false;
		isInRoom = true;
		//spawnPlayer ();
	}


	public void spawnPlayer(string prefName){
		isInRoom = false;

		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");

		foreach(PhotonPlayer pl2 in PhotonNetwork.playerList){
			if(pl2.name == PhotonNetwork.playerName){
				PhotonNetwork.playerName = playerName + " " + Random.Range (0,999);
			}
		}


		GameObject pl = PhotonNetwork.Instantiate (prefName, spawnPoints[Random.Range (0, spawnPoints.Length)].position, spawnPoint.rotation, 0) as GameObject;
		pl.GetComponent<RigidbodyFPSWalker> ().enabled = true;
		pl.GetComponent<RigidbodyFPSWalker> ().fpsCam.SetActive (true);
		pl.GetComponent<RigidbodyFPSWalker> ().graphics.SetActive (false);
		
	}

	public void spawnAi(){
		GameObject ai1 = PhotonNetwork.Instantiate (Ais[Random.Range(0,Ais.Length)].name, spawnPoints[Random.Range (0, spawnPoints.Length)].position, spawnPoint.rotation, 0) as GameObject;
		ai1.GetComponent<aiCon> ().isMine = true;

	}

	void OnGUI(){

		if (isConnected) {
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			GUILayout.BeginArea (new Rect (Screen.width / 2 - 250, Screen.height / 2 - 250, 500,500));
			playerName = GUILayout.TextField (playerName);
			roomName = GUILayout.TextField (roomName);

			if (GUILayout.Button ("Create")) {
				PhotonNetwork.JoinOrCreateRoom (roomName, null, null);
			}

			foreach (RoomInfo game in PhotonNetwork.GetRoomList()) {
				if (GUILayout.Button (game.name + " " + game.playerCount + "/" + game.maxPlayers)) {
					PhotonNetwork.JoinOrCreateRoom (game.name, null, null);
				}
			}
			GUILayout.EndArea ();
		}

		if (isInRoom) {
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			GUILayout.BeginArea (new Rect (Screen.width / 2 - 250, Screen.height / 2 - 250, 500,500));
			GUILayout.Box("Score: " + PhotonNetwork.player.GetScore () + " \n \nBots: x" + curAis.Length + "\n");
			GUILayout.Box("Kills: " + PlayerPrefs.GetInt ("kills") + " | "  + "Deaths: " + PlayerPrefs.GetInt ("deaths") + " | K/D: " + kD)  ;



			if (GUILayout.Button ("Assault")) {
				spawnPlayer(playerPref2.name);
			}
			if (GUILayout.Button ("SWAT")) {
				spawnPlayer(playerPref.name);
			}
			if (GUILayout.Button ("Disconnect")) {
				PhotonNetwork.Disconnect ();
				Application.LoadLevel (0);
			}
			if (GUILayout.Button ("Spawn 2 Bots")) {
				spawnAi ();
				spawnAi ();
			}
			if (GUILayout.Button ("Delete Bot")) {
				GameObject.Find("_NETWORKSCRIPTS").GetComponent<PhotonView>().RPC ("deleteBot", PhotonTargets.AllBuffered, null);
			}
			GUILayout.EndArea ();
		}
	}


}
