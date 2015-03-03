using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour {

	[SerializeField] Text connectionText;
	[SerializeField] Transform[] spawnPoints;
	[SerializeField] Camera sceneCamera;

	[SerializeField] GameObject serverWindow;
	[SerializeField] InputField username;
	[SerializeField] InputField roomName;
	[SerializeField] InputField roomList;
	[SerializeField] InputField messageWindow;
	[SerializeField] GameObject exitButton;

	GameObject player;
	Queue<string> messages;
	const int messageCount = 7;
	PhotonView photonView;

	void Start () {

		photonView = GetComponent<PhotonView> ();
		messages = new Queue<string> (messageCount);

		PhotonNetwork.logLevel = PhotonLogLevel.Full;
		PhotonNetwork.ConnectUsingSettings ("1.0");
		StartCoroutine ("UpdateConnectionText");
	}
	
	IEnumerator UpdateConnectionText () {
		while (true) {
			connectionText.text = PhotonNetwork.connectionStateDetailed.ToString ();
			yield return null;
		}
	}

	void OnJoinedLobby () {
		serverWindow.SetActive (true);
		exitButton.SetActive (false);
	}

	public void JoinRoom () {
		PhotonNetwork.player.name = username.text;
		RoomOptions ro = new RoomOptions () { isVisible = true, maxPlayers = 10 };
		PhotonNetwork.JoinOrCreateRoom (roomName.text, ro, TypedLobby.Default);
	}

	void OnJoinedRoom () {
		serverWindow.SetActive (false);
		exitButton.SetActive (true);
		StopCoroutine ("UpdateConnectionText");
		connectionText.text = "";
		StartSpawnProcess (0f);
	}
	
	void OnReceivedRoomListUpdate () {
		roomList.text = "";
		RoomInfo[] rooms = PhotonNetwork.GetRoomList ();
		foreach (RoomInfo room in rooms) {
			roomList.text += room.name+"\n";
		}
	}

	public void LeaveRoom () {
		AddMessage (PhotonNetwork.player.name + " left this room.");
		PhotonNetwork.LeaveRoom ();
	}

	void OnLeftRoom () {
		sceneCamera.enabled = true;
	}

	IEnumerator SpawnPlayer (float respawnTime) {
		yield return new WaitForSeconds(respawnTime);

		int index = Random.Range (0, spawnPoints.Length);
		player = PhotonNetwork.Instantiate (
			"GamePlayer", 
			spawnPoints [index].position, 
			spawnPoints [index].rotation, 
			0
		);
		player.GetComponent<PlayerNetworkMover> ().RespawnMe += StartSpawnProcess;
		player.GetComponent<PlayerNetworkMover> ().SendNetworkMessage += AddMessage;
		sceneCamera.enabled = false;

		AddMessage ("Spawned player: " + PhotonNetwork.player.name);
	}

	void StartSpawnProcess (float respawnTime) {
		sceneCamera.enabled = true;
		StartCoroutine ("SpawnPlayer", respawnTime);
	}

	void AddMessage(string message) {
		photonView.RPC ("AddMessage_RPC", PhotonTargets.All, message);
	}

	[RPC]
	void AddMessage_RPC(string message) {
		messages.Enqueue (message);
		if(messages.Count > messageCount) {
			messages.Dequeue();
		}
		messageWindow.text = "";
		foreach (string m in messages) {
			messageWindow.text += m + "\n";
		}
	}
}
