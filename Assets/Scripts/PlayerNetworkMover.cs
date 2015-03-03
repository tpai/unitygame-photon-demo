using UnityEngine;
using System.Collections;

public class PlayerNetworkMover : Photon.MonoBehaviour {

	public delegate void Respawn (float time);
	public event Respawn RespawnMe;
	public delegate void SendMessage(string MessageOverlay);
	public event SendMessage SendNetworkMessage;

	Vector3 position;
	Quaternion rotation;
	float smoothing = 10f;
	float health = 100f;
//	bool aim = false;
//	bool sprint = false;
//	Animator anim;

	bool initialLoad = true;

	void Start () {

//		anim = GetComponentInChildren<Animator> ();

		if (photonView.isMine) {
			GetComponent<MouseLook>().enabled = true;
			GetComponent<CharacterMotor>().enabled = true;
			GetComponent<FPSInputController>().enabled = true;
			GetComponentInChildren<Camera>().enabled = true;
			GetComponentInChildren<MouseLook>().enabled = true;
		}
		else {
			StartCoroutine("UpdateData");
		}
	}

	IEnumerator UpdateData () {

		if (initialLoad) {
			initialLoad = false;
			transform.position = position;
			transform.rotation = rotation;
		}

		while (true) {
			transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * smoothing);
			transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * smoothing);
//			anim.SetBool("Aim", aim);
//			anim.SetBool("Sprint", sprint);
			yield return null;
		}
	}

	void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info) {
		if (stream.isWriting) {
			stream.SendNext (transform.position);
			stream.SendNext (transform.rotation);
			stream.SendNext (health);
			// sync animation state
//			stream.SendNext (anim.GetBool("Aim"));
//			stream.SendNext (anim.GetBool("Sprint"));
		} else {
			position = (Vector3)stream.ReceiveNext();
			rotation = (Quaternion)stream.ReceiveNext();
			health = (float)stream.ReceiveNext();
//			aim = (bool)stream.ReceiveNext();
//			sprint = (bool)stream.ReceiveNext();
		}
	}

	[RPC]
	public void GetShot (float damage, string enemyName) {
		health -= damage;
		if (health <= 0 && photonView.isMine) {

			if(SendNetworkMessage != null)
				SendNetworkMessage(PhotonNetwork.player.name + " was killed by " + enemyName);

			if(RespawnMe != null) {
				RespawnMe (3f);
			}
			PhotonNetwork.Destroy(gameObject);
		}
	}
}
