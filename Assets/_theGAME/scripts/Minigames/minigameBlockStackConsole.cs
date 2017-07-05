using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon;

public class minigameBlockStackConsole : PunBehaviour, IPunObservable {

    // Block Prefab
    public string block = "Minigame Block";

    [Tooltip("Array of block spawn points")]
    public BlockSpawnPoint[] spawnPoints;

    // List of spawned blocks
    public List<GameObject> Blocks = new List<GameObject>();

    // Is minigame being played?
    public bool gameActive { get; private set; }

    public int blockCount = 5;
    public float blockSpawnHeight = 2;

    private Vector3 winBoxPosition { get { return transform.position + new Vector3(0.0f, 2.01f, 0.0f); } }
    private Vector3 winBoxExtents = new Vector3(0.5f, 1.5f, 0.5f);
    private float winTimer = 0.0f;
    public float winTimeThreshold = 3.0f;

    private float resetTimer = 0.0f;
    public float resetTimeThreshold = 10.0f;

    void Start() {
        if (block == null) Debug.LogError("Minigame Console - Block Stack's 'block' prefab = null");
        spawnPoints = GetComponentsInChildren<BlockSpawnPoint>();

        gameActive = false;
        resetTimer = resetTimeThreshold;
    }

    void Update() {
        if (gameActive) {
            //
        } else if (resetTimer <= resetTimeThreshold) resetTimer += Time.deltaTime;
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.isWriting) {
            //
        } else {
            //
        }
    }

    public void Interact() {
        if (!gameActive) {
            if(resetTimer < resetTimeThreshold) {
                Debug.Log("Cannot reset game quite yet.");
                return;
            }

            //photonView.RPC("RemoveAllBufferedRPCs", PhotonTargets.MasterClient, photonView.viewID);
            photonView.RPC("SetGameActive", PhotonTargets.AllBuffered, true);
            Debug.Log("Starting minigame!");
            
            // Instantiate [blockCount] blocks
            for (int i = 0; i < blockCount; i++) InstantiateBlock();
        } else {
            Debug.Log("Minigame in progress");
        }
    }

    public void CollectBlocks() {
        Debug.Log("Calling CollectBlocks()");
        Collider[] Colliders = Physics.OverlapSphere(transform.position, 2.0f);
        if(Colliders != null) {
            Debug.Log("Colliders is NOT null: " + Colliders.Count());
            foreach(Collider collider in Colliders) {
                minigameBlock block = collider.GetComponent<minigameBlock>();
                if(block != null) {
                    Debug.Log("The collider IS a minigameBlock");
                    if (!block.hasOwner) {
                        // Must add a 'pillar' collider to console for when the game is active (will have light beam particle effect)
                        // teleport block to top of console 'stack' of blocks (raycast from console? set block to last hit.point)
                        // remove block from 'Interact' layer
                        // add 1++ to console's winTotal
                    }
                }
            }
        }
    }

    public void InstantiateBlock() {        
        Vector3 spawnPoint = Vector3.zero;
        if (spawnPoints != null && spawnPoints.Length > 0) {
            BlockSpawnPoint[] freeSpawnPoints = spawnPoints.Where(spawnPoint2 => !spawnPoint2.inUse).ToArray();

            if (freeSpawnPoints != null && freeSpawnPoints.Length > 0) {
                BlockSpawnPoint chosenSP = freeSpawnPoints[Random.Range(0, freeSpawnPoints.Length)];
                spawnPoint = chosenSP.transform.position;
                chosenSP.inUse = true;
            } else spawnPoint = new Vector3(Random.Range(0.5f, 5.0f), 0, Random.Range(0.5f, 5.0f));
        } else spawnPoint = new Vector3(Random.Range(0.5f, 5.0f), 0, Random.Range(0.5f, 5.0f));

        photonView.RPC("InstantiateBlockInScene", PhotonTargets.MasterClient, block, spawnPoint, Quaternion.identity, 0, null);
    }

    [PunRPC]
    public void InstantiateBlockInScene(string PrefabName, Vector3 Position, Quaternion Rotation, int Group, object[] Data) {
        Debug.Log("<Color=Magenta>InstantiateBlockInScene()</Color> -- Calling InstantiateBlockInScene");
        GameObject block = PhotonNetwork.InstantiateSceneObject(PrefabName, Position, Rotation, Group, Data);
        photonView.RPC("SetBlockData", PhotonTargets.AllBuffered, block.GetPhotonView().viewID);
    }

    [PunRPC]
    public void SetBlockData(int viewID) {
        Debug.Log("<Color=Magenta>AddToBlocks()</Color> -- Calling AddToBlocks");
        GameObject block = PhotonView.Find(viewID).gameObject;

        block.GetComponent<minigameBlock>().console = this;
        Blocks.Add(PhotonView.Find(viewID).gameObject);
        block.SetActive(true);
    }

    [PunRPC]
    public void SetGameActive(bool active) {
        Debug.Log("<Color=Magenta>SetGameActive()</Color> -- Calling SetGameActive");
        gameActive = active;
    }

    //void OnDrawGizmos() {
    //    var boxes = Physics.OverlapBox(winBoxPosition, winBoxExtents, Quaternion.identity, 1 << LayerMask.NameToLayer("Interact"));
    //    Gizmos.color = boxes.Length >= 3 ? Color.green : Color.yellow;
    //    Gizmos.DrawWireCube(winBoxPosition, winBoxExtents * 2f);
    //}
}



//     void Update() {
//        if (gameActive) {
//            if (Physics.OverlapBox(winBoxPosition, winBoxExtents, Quaternion.identity, 1 << LayerMask.NameToLayer("Interact")).Length == 3) {
//                winTimer += Time.deltaTime;
//                if (winTimer >= winTimeThreshold) MinigameWin();
//            } else winTimer = 0.0f;
//        } else {
//            resetTimer += Time.deltaTime;
//        }
//    }

//    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
//        if (stream.isWriting) {
//            stream.SendNext(winTimer);
//            stream.SendNext(resetTimer);
//        } else {
//            winTimer = (float)stream.ReceiveNext();
//            resetTimer = (float)stream.ReceiveNext();
//        }
//    }

//    public void MinigameWin() {
//        photonView.RPC("RemoveAllBufferedRPCs", PhotonTargets.MasterClient, photonView.viewID);
//        photonView.RPC("SetGameActive", PhotonTargets.AllBuffered, false);

//        // Destroy minigame objects
//        this.photonView.RPC("DestroyBlocks", PhotonTargets.MasterClient);
//        Blocks.Clear();

//        winTimer = 0.0f;
//        resetTimer = 0.0f;
//        PhotonNetwork.Instantiate("Coin", transform.position + new Vector3(Random.Range(-5, 5), blockSpawnHeight, Random.Range(-5, 5)), Quaternion.identity, 0);

//        Debug.Log("We win!! Awesomeness for everyone here :)");
//    }

//    [PunRPC]
//    void RemoveAllBufferedRPCs(int photonViewID) {
//        PhotonNetwork.RemoveRPCs(PhotonView.Find(photonViewID));
//    }

//    [PunRPC]
//    public void DestroyBlocks() {
//        if (Blocks.Count > 0) {
//            foreach (GameObject Block in Blocks) {
//                PhotonNetwork.Destroy(Block);
//            }
//        }
//    }
//}