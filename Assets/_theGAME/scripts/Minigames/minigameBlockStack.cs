using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon;

public class minigameBlockStack : PunBehaviour
{

	// Keeps track of spawned blocks
	List<GameObject> Blocks = new List<GameObject>();

	// Is minigame being played?
	public bool GameActive { get; private set; }

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
		GameActive = false;
	}

	void OnDrawGizmos()
	{
		var boxes = Physics.OverlapBox(winBoxPosition, winBoxExtents, Quaternion.identity, 1 << LayerMask.NameToLayer("Interact"));
		Gizmos.color = boxes.Length >= 3 ? Color.green : Color.yellow;
		Gizmos.DrawWireCube(winBoxPosition, winBoxExtents * 2f);
	}

	void Update()
	{
		if(GameActive)
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
					Block.transform.position = new Vector3(transform.position.x + Random.Range(-5, 5), blockSpawnHeight, transform.position.z + Random.Range(-5, 5));
				}
			}
		}
		else
		{
			resetTimer += Time.deltaTime;
		}
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if(stream.isWriting)
		{
			stream.SendNext(winTimer);
			stream.SendNext(resetTimer);
		}
		else
		{
			winTimer = (float)stream.ReceiveNext();
			resetTimer = (float)stream.ReceiveNext();
		}
	}

	public void Interact()
	{
		if(!GameActive)
		{
			if(resetTimer < resetTimeThreshold)
			{
				Debug.Log("Cannot reset game quite yet.");
				return;
			}

			photonView.RPC("RemoveAllBufferedRPCs", PhotonTargets.MasterClient, photonView.viewID);
			photonView.RPC("SetGameRunning", PhotonTargets.MasterClient, true);

			Debug.Log("Starting minigame!");

			Vector3 offset = new Vector3(0, blockSpawnHeight, 0);
			for(int i = 0; i < blockCount; i++)
			{
				offset.x = Random.Range(-5, 5);
				offset.z = Random.Range(-5, 5);

				photonView.RPC("InstantiateMinigameBlockInScene", PhotonTargets.MasterClient, transform.position + offset, Quaternion.identity, 0, null);
			}
		}
		else
		{
			Debug.Log("Minigame in progress");
		}
	}

	public void MinigameWin()
	{
		photonView.RPC("RemoveAllBufferedRPCs", PhotonTargets.MasterClient, photonView.viewID);
		photonView.RPC("SetGameRunning", PhotonTargets.All, false);
		photonView.RPC("DestroyBlocks", PhotonTargets.MasterClient);
		Blocks.Clear();

		winTimer = 0.0f;
		resetTimer = 0.0f;
		PhotonNetwork.Instantiate("Coin", transform.position + new Vector3(Random.Range(-5, 5), blockSpawnHeight, Random.Range(-5, 5)), Quaternion.identity, 0);

		Debug.Log("We win!! Awesomeness for everyone here :)");
	}

	[PunRPC]
	void InstantiateMinigameBlockInScene(Vector3 Position, Quaternion Rotation, int Group, object[] Data)
	{
		GameObject block = PhotonNetwork.InstantiateSceneObject("Minigame Block", Position, Rotation, Group, Data);
		Blocks.Add(block); // Concerned about when the master client changes. Possibly test if blocks delete themselves when 
						   // the master client changes...
	}

	[PunRPC]
	void RemoveAllBufferedRPCs(int photonViewID)
	{
		PhotonNetwork.RemoveRPCs(PhotonView.Find(photonViewID));
	}

	[PunRPC]
	void SetGameRunning(bool Running)
	{
		GameActive = Running;
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
}