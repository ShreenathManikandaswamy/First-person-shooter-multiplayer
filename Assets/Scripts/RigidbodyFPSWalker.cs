using UnityEngine;
using System.Collections;

public class RigidbodyFPSWalker : MonoBehaviour {

	
	public float speed = 10.0f;
	public float gravity = 10.0f;
	public float maxVelocityChange = 10.0f;
	public bool canJump = true;
	public float jumpHeight = 2.0f;
	private bool grounded = false;
	public PhotonView name;

	public int kills = 0;
	public int deaths = 0;


	public Texture blood;
	public int bloodTimer;

	public string theKiller = "";
	public string killerWep = "";

	public GameObject me;
	public GameObject graphics;
	public GameObject ragDoll;
	public GameObject[] bots;

	public int health = 100;

	public GameObject fpsCam;

	public bool isPause = false;

	public AudioSource sound1;
	public AudioClip soundClip;

	public GameObject[] activePlayers;
	
	
	void Awake () {
		GetComponent<Rigidbody>().freezeRotation = true;
		GetComponent<Rigidbody>().useGravity = false;
		name.RPC ("updateName", PhotonTargets.AllBuffered, PhotonNetwork.playerName, health);
		gameObject.name = PhotonNetwork.playerName;

	}
	
	void FixedUpdate () {
		kills = PlayerPrefs.GetInt ("kills");
		deaths = PlayerPrefs.GetInt ("deaths");



		if (grounded) {
			// Calculate how fast we should be moving
			Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			targetVelocity = transform.TransformDirection(targetVelocity);
			targetVelocity *= speed;
			
			// Apply a force that attempts to reach our target velocity
			Vector3 velocity = GetComponent<Rigidbody>().velocity;
			Vector3 velocityChange = (targetVelocity - velocity);
			velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
			velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
			velocityChange.y = 0;
			GetComponent<Rigidbody>().AddForce(velocityChange, ForceMode.VelocityChange);
			
			// Jump
			if (canJump && Input.GetButton("Jump")) {
				GetComponent<Rigidbody>().velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
			}
		}

		bloodTimer += -1;

		if (Input.GetKeyDown (KeyCode.Escape)) {
			isPause = true;
		}
		if(Input.GetKeyUp (KeyCode.Escape)){
			isPause = false;
		}

		if (isPause) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		} else {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		
		// We apply gravity manually for more tuning control
		GetComponent<Rigidbody>().AddForce(new Vector3 (0, -gravity * GetComponent<Rigidbody>().mass, 0));
		
		grounded = false;

		if (health <= 0) {
			GetComponent<PhotonView>().RPC ("DIE", PhotonTargets.AllBuffered, null);
		}

		bots = GameObject.FindGameObjectsWithTag("Ai");
	}
	
	void OnCollisionStay () {
		grounded = true;    
	}
	
	float CalculateJumpVerticalSpeed () {
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt(2 * jumpHeight * gravity);
	}



	void OnGUI(){
		GUI.Box (new Rect (10, 10, 100, 30), "HP | " + health);

		if (bloodTimer >= 1) {
			GUI.DrawTexture (new Rect(0,0,Screen.width, Screen.height), blood, ScaleMode.StretchToFill);
		}



		if (isPause) {

			GUILayout.BeginArea (new Rect(Screen.width/2 - 250, Screen.height/2 - 250, 500,500));
			GUILayout.Box ("ActiveBots: " + "x" + bots.Length);
			GUILayout.Box ("Score:" + "\nKills: " + kills + " / " + "Deaths: " + deaths);
			GUILayout.Box ("Players:    Name         K/D   ");  
			foreach(PhotonPlayer pl in PhotonNetwork.playerList){
				GUILayout.Box (pl.name + " | " + pl.GetScore ());
			}
			if(GUILayout.Button ("Disconnect")){
				PhotonNetwork.Disconnect ();
				Application.LoadLevel (0);
			}
			GUILayout.EndArea ();
		}
	}

	void OnCollisionEnter(Collision col){
		if (col.transform.tag == "Bullet") {
			Debug.Log ("Hit");
			int dmg = col.transform.GetComponent<bulScript>().dmg;
			string aiName = col.transform.GetComponent<bulScript>().name;
			GetComponent<PhotonView>().RPC ("applyDamage", PhotonTargets.All, dmg, aiName, "M4");
		}
	}

	public void getClip (AudioClip soundToRecieve){
		Debug.Log (soundToRecieve.name);
		soundClip = soundToRecieve;
	}



	[PunRPC]
	public void applyDamage(int dmg, string killerName, string wepName){
		health = health - dmg;
		//GameObject.Find (killerName).GetComponent<PhotonView>().RPC ("addKill", PhotonTargets.AllBuffered);
		name.RPC ("updateName", PhotonTargets.AllBuffered, PhotonNetwork.playerName, health);
		theKiller = killerName;
		killerWep = wepName;
		Debug.Log ("hit!" + health);
		bloodTimer = 20;
	}

	[PunRPC]
	public void DIE(){
		if (GetComponent<PhotonView> ().isMine) {
			PhotonNetwork.Destroy (me);
			Destroy (me);
			GameObject rDoll = PhotonNetwork.Instantiate (ragDoll.name, transform.position, transform.rotation, 0);
			Destroy (rDoll, 3);
			GameObject.Find ("_ROOM").GetComponent<roomMan> ().OnJoinedRoom ();
			GameObject.Find ("_NETWORKSCRIPTS").GetComponent<PhotonView>().RPC ("addFeed", PhotonTargets.All, theKiller + " [" + killerWep + "] " + PhotonNetwork.playerName);

			Debug.Log ("die");
			PlayerPrefs.SetInt ("deaths", deaths + 1);

			GameObject killer = GameObject.Find (theKiller);
			killer.GetComponent<PhotonView>().RPC ("exitTrig", PhotonTargets.AllBuffered, null);
			killer.GetComponent<PhotonView>().RPC ("addKill", PhotonTargets.AllBuffered, null);


		}
			

	}

	[PunRPC]
	public void addKill(){
		PlayerPrefs.SetInt ("kills", kills + 1);
		Debug.Log ("added kill! " + kills);
	}


	public AudioClip[] sounds;


	[PunRPC]
	public void playSound(){
		Debug.Log ("Sound: " + soundClip.name);
		sound1.PlayOneShot (soundClip);
	}
}

