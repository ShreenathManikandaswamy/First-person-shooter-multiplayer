using UnityEngine;
using System.Collections;

public class aiCon : MonoBehaviour {

	public bool isMine = false;
	public UnityEngine.AI.NavMeshAgent nma;
	public GameObject[] targets;
	public float fireRate = 1;
	public bool isRanged = false;
	public GameObject me;
	public GameObject ragDoll;
	public GameObject bullet;
	public float bulletSpeed = 300;
	public Transform muzzle;
	public int blDamage = 3;

	public Animation am;
	public AnimationClip reload;
	public AnimationClip run;

	public int ammo = 10;
	public int maxAmmo = 10;
	public bool inTrig = false;
	public bool isEmpty = true;

	public AudioClip shootSound;
	public AudioSource aS;

	public PhotonView pv;

	public int health = 100;
	public int damage = 20;
	public int timer = 30;
	public string killerName = "";
	public string killerWep = "";
	public string name;

	public PhotonView nameTag;

	public int locTimer = 20;

	void Awake(){
		name = "Bot " + Random.Range (0, 999);
		targets = GameObject.FindGameObjectsWithTag ("Player");
		if (isMine){
			Debug.Log ("setting targets!");
			GetComponent<PhotonView> ().RPC ("setTarget", PhotonTargets.AllBuffered, null);
			
		}
		target ();
		gameObject.name = name;

		nameTag.RPC ("updateName", PhotonTargets.AllBuffered, name, health);
	}

	void FixedUpdate(){

		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		GameObject[] Ais = GameObject.FindGameObjectsWithTag ("Ai");

		if (inTrig && isRanged) {
			Vector3 lookAt = new Vector3(nma.destination.x,this.transform.position.y,nma.destination.z);
			transform.LookAt (lookAt);
		}

		if (players.Length >= 1) {
			targets = players;
		} else {
			if(Ais.Length >= 1){
				targets = Ais;
			}
		}
		if (isRanged) {
			if (am.IsPlaying (reload.name)) {
				GetComponent<Rigidbody> ().isKinematic = true;
				nma.Stop();
			} else {
				GetComponent<Rigidbody> ().isKinematic = false;
				nma.Resume();
				pv.RPC ("playAnim", PhotonTargets.AllBuffered, run.name);
			}
		}

		float speedMultiplyer = 1.0f - 0.9f*Vector3.Angle(transform.forward, nma.steeringTarget-transform.position)/180.0f;
		nma.speed = 7 *speedMultiplyer;
		
		
		if (nma.hasPath == false) {
			Debug.Log ("det is null");
			target ();
		} 

		target ();
	
		if (health <= 0) {
			GetComponent<PhotonView> ().RPC ("DIE", PhotonTargets.AllBuffered, null);
		}



		if (!inTrig) {
			locTimer = timer;
		}

		if (inTrig) {
			locTimer += -1;

			//transform.LookAt (nma.destination);

			if (locTimer <= 0) {
				shoot ();
				locTimer = timer;
			}
		}
	}


	[PunRPC]
	public void playAnim(string animName){
		am.CrossFade (animName);
	}
	

	public void target(){
		Debug.Log ("target");
		if (isMine && !nma.hasPath && targets.Length >= 1){
			Debug.Log ("setting targets!");
			GetComponent<PhotonView> ().RPC ("setTarget", PhotonTargets.AllBuffered, null);
			
		}
	}



	void OnCollisionStay(Collision col){

		if (col.transform.tag == "Player") {
			col.transform.GetComponent<PhotonView> ().RPC ("applyDamage", PhotonTargets.AllBuffered, damage, name, "Melee");
		}

		if (col.transform.tag == "Ai") {
			col.transform.GetComponent<PhotonView> ().RPC ("AiDamage", PhotonTargets.AllBuffered, damage, name, "Melee");
		}
	}

	void OnTriggerEnter(Collider col){
		if (col.tag == "Player") {
			inTrig = true;
			nma.destination = col.transform.position;
			transform.LookAt (col.transform);
			Debug.Log ("entered!");

			if (col.gameObject == null) {
				inTrig = false;
			}

		}
		if (col.tag == "Ai") {
			inTrig = true;
			nma.destination = col.transform.position;
			//transform.LookAt (col.transform);
			Debug.Log ("entered!");
			
			if (col.gameObject == null) {
				inTrig = false;
			}
			
		}



	}
	
	void OnCollisionEnter(Collision col){
		if (col.transform.tag == "Bullet") {
			Debug.Log ("Hit");
			int dmg = col.transform.GetComponent<bulScript>().dmg;
			string aiName = col.transform.GetComponent<bulScript>().name;
			GetComponent<PhotonView>().RPC ("AiDamage", PhotonTargets.All, dmg, aiName, "M4");
		}
	}


	void OnTriggerExit(Collider col){
		if (col.tag == "Player") {
			inTrig = false;
		}
	}
	public GameObject muzzleFlash;

	public void shoot(){
		if (isRanged && inTrig) {
			if(ammo >= 1 && !am.IsPlaying (reload.name)){
				ammo += -1;
				GameObject muz = PhotonNetwork.Instantiate (muzzleFlash.name, muzzle.position, muzzle.rotation, 0);
				Destroy (muz, 0.5f);
				GameObject bl = PhotonNetwork.Instantiate (bullet.name, muzzle.position, muzzle.rotation, 0) as GameObject;
				bl.GetComponent<Rigidbody> ().AddForce (transform.forward * bulletSpeed);
				bl.GetComponent<bulScript> ().dmg = blDamage;
				bl.GetComponent<bulScript> ().name = name;
				pv.RPC ("playShoot", PhotonTargets.AllBuffered, null);
			} else {
				doReload();
			}
		}
	}

	public void doReload(){
		Debug.Log ("reload!");
		ammo = maxAmmo;
		pv.RPC ("playAnim", PhotonTargets.AllBuffered, reload.name);
	}

	[PunRPC]
	public void exitTrig(){
		inTrig = false;
	}


	[PunRPC]
	public void playShoot(){
		aS.PlayOneShot (shootSound);
	}



	[PunRPC]
	public void setTarget(){
		//if (targets.Length >= 1) {
			Debug.Log ("setting target");
			nma.SetDestination (targets [Random.Range (0, targets.Length)].transform.position);
		//}
	}

	[PunRPC]
	public void AiDamage(int theDamage, string pName, string wepName){
		killerName = pName;
		killerWep = wepName;
		health = health - theDamage;
		nma.destination = GameObject.Find (pName).transform.position;
		Debug.Log (health);
		
		nameTag.RPC ("updateName", PhotonTargets.AllBuffered, name, health);
	}

	[PunRPC]
	public void DIE(){
		if (GetComponent<PhotonView> ().isMine) {
			PhotonNetwork.Destroy (me);
			Destroy (me);
			GameObject rDoll = PhotonNetwork.Instantiate (ragDoll.name, transform.position, transform.rotation, 0);
			Destroy (rDoll, 3);
			GameObject.Find ("_ROOM").GetComponent<roomMan> ().spawnAi ();
			GameObject.Find ("_NETWORKSCRIPTS").GetComponent<PhotonView>().RPC ("addFeed", PhotonTargets.All, killerName + " [" + killerWep + "]" + name);

			GameObject killer = GameObject.Find (killerName);
			killer.GetComponent<PhotonView>().RPC ("exitTrig", PhotonTargets.AllBuffered, null);
			killer.GetComponent<PhotonView>().RPC ("addKill", PhotonTargets.AllBuffered, null);
		}
		
		
	}



}
