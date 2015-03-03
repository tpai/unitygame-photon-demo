using UnityEngine;
using System.Collections;

public class PlayerShoot : MonoBehaviour {

	public LineRenderer gunLine;
	Vector3 startPos;
	float damage = 25f;
	
	void Update () {

		startPos = transform.parent.position;
		gunLine.enabled = true;
		gunLine.SetPosition(0, startPos);

		Ray ray = new Ray (startPos, transform.forward);
		RaycastHit hit;
		if (Input.GetButtonDown ("Fire1")) {
			if (Physics.Raycast (ray, out hit, 3f)) {
				if (hit.transform.tag == "Player") {
					hit.transform.GetComponent<PhotonView>().RPC ("GetShot", PhotonTargets.All, damage, PhotonNetwork.player.name);
				}
			}

		}
		gunLine.SetPosition(1, ray.origin + ray.direction * 3f);
		Debug.DrawRay(startPos, transform.forward * 3f, Color.green);
	}
}
