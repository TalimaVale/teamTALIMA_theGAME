using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon;

public class minigameBlockStack : PunBehaviour
{

	// Keeps track of spawned blocks
	List<GameObject> Blocks = new List<GameObject>();

	// Is minigame being played?
	public bool GameRunning { get; private set; }

	public int blockCount = 5;
	public float blockSpawnHeight = 2;

	Vector3 winBoxPosition { get { return transform.position + new Vector3(0f, 2.01f, 0f); } }
	Vector3 winBoxExtents = new Vector3(0.5f, 1.5f, 0.5f);
	private float winTimer = 0.0f;
	public float winTimeThreshold = 3.0f;

	private float resetTimer = 0.0f;
	public float resetTimeThreshold = 10.0f;

	void Start()
	{
		resetTimer = resetTimeThreshold;
		GameRunning = false;
	}

	void OnDrawGizmos()
	{
		var boxes = Physics.OverlapBox(winBoxPosition, winBoxExtents, Quaternion.identity, 1 << LayerMask.NameToLayer("Interact"));
		Gizmos.color = boxes.Length >= 3 ? Color.green : Color.yellow;
		Gizmos.DrawWireCube(winBoxPosition, winBoxExtents * 2f);
	}

	void Update()
	{
		if(GameRunning)
		{
			if(Physics.OverlapBox(winBoxPosition, winBoxExtents, Quaternion.identity, 1 << LayerMask.NameToLayer("Interact")).Length >= 3)
			{
				winTimer += Time.deltaTime;
				if(winTimer >= winTimeThreshold)
				{
					MinigameWin();
				}
			}
			else
			{
				winTimer = 0.0f;
			}

			foreach(GameObject Block in Blocks)
			{
				if(Block.transform.parent == null && Block.transform.position.y <= -5)
				{
					Block.transform.position = new Vector3(Random.Range(-5, 5), blockSpawnHeight, Random.Range(-5, 5));
				}
			}
		}
		else
		{
			resetTimer += Time.deltaTime;
		}
	}

	[PunRPC]
	public void DestroyBlocks()
	{
		if(Blocks.Count > 0)
		{
			foreach(GameObject Block in Blocks)
			{
				PhotonNetwork.Destroy(Block);
			}
		}
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if(stream.isWriting)
		{
			//stream.SendNext(gameActive);
			stream.SendNext(winTimer);
			stream.SendNext(resetTimer);
		}
		else
		{
			//gameActive = (bool)stream.ReceiveNext();
			winTimer = (float)stream.ReceiveNext();
			resetTimer = (float)stream.ReceiveNext();
		}
	}

	public void Interact()
	{
		if(!GameRunning)
		{
			if(resetTimer < resetTimeThreshold)
			{
				Debug.Log("Cannot reset game quite yet.");
				return;
			}

			photonView.RPC("SetGameRunning", PhotonTargets.AllBuffered, true);
			Debug.Log("Starting minigame!");

			Vector3 offset = new Vector3(0, blockSpawnHeight, 0);
			for(int i = 0; i < blockCount; i++)
			{
				offset.x = Random.Range(-5, 5);
				offset.z = Random.Range(-5, 5);

				photonView.RPC("InstantiatePrefabInScene", PhotonTargets.MasterClient, "Minigame Block", transform.position + offset, Quaternion.identity, 0, null);
			}
		}
		else
		{
			Debug.Log("Minigame in progress");
		}
	}

	[PunRPC]
	public void InstantiatePrefabInScene(string PrefabName, Vector3 Position, Quaternion Rotation, int Group, object[] Data)
	{
		GameObject block = PhotonNetwork.InstantiateSceneObject(PrefabName, Position, Rotation, Group, Data);
		Blocks.Add(block);
	}
	
	[PunRPC]
	public void SetGameRunning(bool Running)
	{
		GameRunning = Running;
	}

	public void MinigameWin()
	{
		photonView.RPC("SetGameRunning", PhotonTargets.AllBuffered, false);
		this.photonView.RPC("DestroyBlocks", PhotonTargets.MasterClient);
		Blocks.Clear();

		winTimer = 0.0f;
		resetTimer = 0.0f;
		PhotonNetwork.Instantiate("Coin", transform.position + new Vector3(Random.Range(-5, 5), blockSpawnHeight, Random.Range(-5, 5)), Quaternion.identity, 0);

		Debug.Log("We win!! Awesomeness for everyone here :)");
	}
}