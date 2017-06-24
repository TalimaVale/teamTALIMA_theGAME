using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon;

public class minigameBlockStack : PunBehaviour, IPunObservable {

    // Block Prefab
    public string block = "Minigame Block";

    // Keeps track of spawned blocks
    List<GameObject> Blocks = new List<GameObject>();

    // Is minigame being played?
    public bool gameActive { get; private set; }

    public int blockCount = 5;
    public float blockSpawnHeight = 2;

    Vector3 winBoxPosition { get { return transform.position + new Vector3(0f, 2.01f, 0f); } }
    Vector3 winBoxExtents = new Vector3(0.5f, 1.5f, 0.5f);
    private float winTimer = 0.0f;
    public float winTimeThreshold = 3.0f;

    private float resetTimer = 0.0f;
    public float resetTimeThreshold = 10.0f;
    
    void Start () {
        if (block == null) Debug.LogError("Minigame Console's 'block' prefab = null");

        gameActive = false;
        resetTimer = resetTimeThreshold;
    }

    void OnDrawGizmos() {
        var boxes = Physics.OverlapBox(winBoxPosition, winBoxExtents, Quaternion.identity, 1 << LayerMask.NameToLayer("Interact"));
        Gizmos.color = boxes.Length >= 3 ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(winBoxPosition, winBoxExtents * 2f);
    }

    void Update() {
        if (gameActive) {
            if (Physics.OverlapBox(winBoxPosition, winBoxExtents, Quaternion.identity, 1 << LayerMask.NameToLayer("Interact")).Length == 3) {
                winTimer += Time.deltaTime;
                if (winTimer >= winTimeThreshold) MinigameWin();
            } else winTimer = 0.0f;

            // if block falls, teleport it back
            foreach (GameObject Block in Blocks) {
                if (Block.transform.parent == null && Block.transform.position.y <= -5.0f) {
                    Block.transform.position = new Vector3(transform.position.x + Random.Range(-5, 5), blockSpawnHeight, transform.position.z + Random.Range(-5, 5));
                }
            }
        } else {
            resetTimer += Time.deltaTime;
        }
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.isWriting) {
            stream.SendNext(winTimer);
            stream.SendNext(resetTimer);
        } else {
            winTimer = (float)stream.ReceiveNext();
            resetTimer = (float)stream.ReceiveNext();
        }
    }

    public void Interact() {
        if (!gameActive) {
            if (resetTimer < resetTimeThreshold) {
                Debug.Log("Cannot reset game quite yet.");
                return;
            }

            photonView.RPC("RemoveAllBufferedRPCs", PhotonTargets.MasterClient, photonView.viewID);
            photonView.RPC("SetGameActive", PhotonTargets.AllBuffered, true);

            Debug.Log("Starting minigame!");

            Vector3 offset = new Vector3(0, blockSpawnHeight, 0);
            for (int i = 0; i < blockCount; i++) {
                offset.x = Random.Range(-5, 5);
                offset.z = Random.Range(-5, 5);

                photonView.RPC("InstantiatePrefabInScene", PhotonTargets.MasterClient, block, transform.position + offset, Quaternion.identity, 0, null);
            }
        } else {
            Debug.Log("Minigame in progress");
        }
    }

    public void MinigameWin() {
        photonView.RPC("RemoveAllBufferedRPCs", PhotonTargets.MasterClient, photonView.viewID);
        photonView.RPC("SetGameActive", PhotonTargets.AllBuffered, false);

        // Destroy minigame objects
        this.photonView.RPC("DestroyBlocks", PhotonTargets.MasterClient);
        Blocks.Clear();

        winTimer = 0.0f;
        resetTimer = 0.0f;
        PhotonNetwork.Instantiate("Coin", transform.position + new Vector3(Random.Range(-5, 5), blockSpawnHeight, Random.Range(-5, 5)), Quaternion.identity, 0);

        Debug.Log("We win!! Awesomeness for everyone here :)");
    }

    [PunRPC]
    public void SetGameActive(bool active) {
        gameActive = active;
    }

    [PunRPC]
    void RemoveAllBufferedRPCs(int photonViewID) {
        PhotonNetwork.RemoveRPCs(PhotonView.Find(photonViewID));
    }

    [PunRPC]
    public void InstantiatePrefabInScene(string PrefabName, Vector3 Position, Quaternion Rotation, int Group, object[] Data) {
        GameObject block = PhotonNetwork.InstantiateSceneObject(PrefabName, Position, Rotation, Group, Data);
        Blocks.Add(block);
    }

    [PunRPC]
    public void DestroyBlocks() {
        if (Blocks.Count > 0) {
            foreach (GameObject Block in Blocks) {
                PhotonNetwork.Destroy(Block);
            }
        }
    }
}


// TESTS

// How to sync positioning of blocks
        // Photon Transform View component
        // +
        // minigameBlock's Update() tracks position and rotation

        // OnPhotonSerializedView sends/receives position and rotation
        // +
        // minigameBlock's Update() sets position and rotation
// Calling RPC for PickUp and Drop PhotonTargets.All or .AllBuffered