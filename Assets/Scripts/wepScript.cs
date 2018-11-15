using UnityEngine;
using System.Collections;

public class wepScript : MonoBehaviour {

	public Camera fpsCam;
	public GameObject hitPar;
	public int damage = 30;
	public int maxDamage = 60;
	public int range = 10000;
	public int ammo = 10;
	public int clipSize = 10;
	public int clipCount = 5;
	public float recoilPower = 30;
	public Animation am;
	public AnimationClip shoot;
	public AnimationClip reloadA;

	public Texture hitMarker;
	public int hitCD = 0;

	public AudioClip shootSound;
	public AudioClip reloadSound;

	public animManager amM;

	public string  weaponName = "";

	public GameObject[] objectsToDisable;
	public bool canAim = false;
	public float aimFOV = 20;
	public float regFOV = 60;
	public float regOffset = 0;
	public float aimOffset = -75;
	public PhotonView pv;

	public Texture scope;
	public bool isAimed = false;

	void Awake(){

	}

	void Update(){
		if (Input.GetMouseButton (0)) {
			fireShot ();
		}

		if (Input.GetMouseButtonDown (1)) {
			aim (true);
		}

		if (Input.GetMouseButtonUp (1)) {
			aim (false);
		}

		if (Input.GetKeyDown (KeyCode.R)) {
			reload();
		}

		hitCD += -1;
	}

	public void fireShot(){
		if (!am.IsPlaying (reloadA.name) && ammo >= 1) {
			if(!am.IsPlaying (shoot.name)){
				am.CrossFade (shoot.name);

				ammo = ammo - 1;

				pv.transform.GetComponent<RigidbodyFPSWalker>().getClip(shootSound);
				Debug.Log ("playing Sound!");
				pv.RPC ("playSound", PhotonTargets.AllBuffered, null);

				fpsCam.transform.Rotate (Vector3.right, -recoilPower * Time.deltaTime);
				
				RaycastHit hit;
				Ray ray = fpsCam.ScreenPointToRay (new Vector3 (Screen.width / 2, Screen.height / 2 + regOffset, 0));
		
				if (Physics.Raycast (ray, out hit, range)) {
					if (hit.transform.tag == "Player") {
						hit.transform.GetComponent<PhotonView> ().RPC ("applyDamage", PhotonTargets.AllBuffered, Random.Range (damage, maxDamage), PhotonNetwork.playerName, weaponName);
						PhotonNetwork.player.AddScore(1);
						Debug.Log (PhotonNetwork.player.GetScore());
						hitCD = 20;
					}

					if (hit.transform.tag == "Ai") {
						hit.transform.GetComponent<PhotonView> ().RPC ("AiDamage", PhotonTargets.AllBuffered, Random.Range (damage, maxDamage), PhotonNetwork.playerName, weaponName);
						PhotonNetwork.player.AddScore(1);
						Debug.Log (PhotonNetwork.player.GetScore());
						hitCD = 20;
					}


					GameObject particleClone;
					particleClone = PhotonNetwork.Instantiate (hitPar.name, hit.point, Quaternion.LookRotation (hit.normal), 0) as GameObject;
					Destroy (particleClone, 2);
					Debug.Log (hit.transform.name);
			
				}
			}
	
		}
	}

	public void reload(){
		if (clipCount >= 1) {
			am.CrossFade (reloadA.name);
			GetComponent<AudioSource>().PlayOneShot(reloadSound);
			ammo = clipSize;
			clipCount = clipCount - 1;
			amM.reload ();
		}

	}

	public void aim(bool isIn){
		if (canAim) {
			if (isIn) {
				fpsCam.fieldOfView = aimFOV;
				disable ();
				regOffset = aimOffset;
				isAimed = true;
			} else {
				fpsCam.fieldOfView = regFOV;
				enable ();
				regOffset = 0;
				isAimed = false;
			}
		}
	}

	public void disable(){
		foreach (GameObject part in objectsToDisable) {
			part.SetActive (false);
		}
	}
	public void enable(){
		foreach (GameObject part in objectsToDisable) {
			part.SetActive (true);
		}
	}


	void OnGUI(){
		GUI.Box (new Rect(110,10,150,30), "Ammo: " + ammo + "/" + clipSize + "/" + clipCount);
		GUI.Box (new Rect (10, 45, 100, 30), "Score: " + PhotonNetwork.player.GetScore());

		if (hitCD >= 0) {
			GUI.DrawTexture (new Rect(Screen.width/2 - 25, Screen.height / 2 -25, 50,50), hitMarker, ScaleMode.StretchToFill);
		}


		if (isAimed) {
			GUI.DrawTexture (new Rect(0,0,Screen.width, Screen.height), scope, ScaleMode.StretchToFill);
		}
	}
}
