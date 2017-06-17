using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon;

public class minigameBlockStack : PunBehaviour {

    // Block Prefab
    public GameObject block;

    // Room custom properties hashtable
    Hashtable roomProps = new Hashtable();

    // Is minigame being played?
    //public bool gameActive { get; private set; }

    public int blockCount = 5;
    public float blockSpawnHeight = 2;

    Vector3 winBoxPosition { get { return transform.position + new Vector3(0f, 2.01f, 0f); } }
    Vector3 winBoxExtents = new Vector3(0.5f, 1.5f, 0.5f);
    public bool win { get; private set; }
    private float winTimer = 0.0f;
    public float winTimeThreshold = 3.0f;

    private float resetTimer = 0.0f;
    public float resetTimeThreshold = 10.0f;
    
    void Start () {
        if (block == null) Debug.LogError("Minigame Console's 'block' prefab = null");

        //gameActive = (bool)PhotonNetwork.room.CustomProperties["M1"];
        //Debug.Log("<Color=Red>Setting gameActive to: </Color>" + gameActive);
        win = false;
        resetTimer = resetTimeThreshold;
    }

    void OnDrawGizmos() {
        var boxes = Physics.OverlapBox(winBoxPosition, winBoxExtents, Quaternion.identity, 1 << LayerMask.NameToLayer("Interact"));
        Gizmos.color = boxes.Length >= 3 ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(winBoxPosition, winBoxExtents * 2f);
    }

    void Update() {
        if ((bool)PhotonNetwork.room.CustomProperties["M1"] == true) {
            if (Physics.OverlapBox(winBoxPosition, winBoxExtents, Quaternion.identity, 1 << LayerMask.NameToLayer("Interact")).Length == 3) {
                winTimer += Time.deltaTime;
                if (winTimer >= winTimeThreshold) MinigameWin();
            } else winTimer = 0.0f;
        } else {
            resetTimer += Time.deltaTime;
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.isWriting) {
            //stream.SendNext(gameActive);
            stream.SendNext(winTimer);
            stream.SendNext(resetTimer);
        } else {
            //gameActive = (bool)stream.ReceiveNext();
            winTimer = (float)stream.ReceiveNext();
            resetTimer = (float)stream.ReceiveNext();
        }
    }

    public void Interact() {
        if ((bool)PhotonNetwork.room.CustomProperties["M1"] == false) {
            if (resetTimer < resetTimeThreshold) {
                Debug.Log("Cannot reset game quite yet.");
                return;
            }

            win = false;
            //gameActive = true;
            if(!roomProps.ContainsKey("M1")) {
                roomProps.Add("M1", true);
            } else {
                roomProps["M1"] = true;
            }
            PhotonNetwork.room.SetCustomProperties(roomProps);

            // Debug.Log("<Color=Red>Setting gameActive to: </Color>" + gameActive);

            Debug.Log("Starting minigame!");

            Vector3 offset = new Vector3(0, blockSpawnHeight, 0);
            for (int i = 0; i < blockCount; i++) {
                offset.x = Random.Range(-5, 5);
                offset.z = Random.Range(-5, 5);
                GameObject block = PhotonNetwork.Instantiate("Minigame Block", transform.position + offset, Quaternion.identity, 0);
                photonView.RPC("SetBlockParent", PhotonTargets.AllBuffered, block.GetPhotonView().viewID);
            }
        } else {
            Debug.Log("Minigame in progress");
        }
    }

    [PunRPC]
    void SetBlockParent(int blockViewID) {
        PhotonView.Find(blockViewID).transform.SetParent(transform);
    }

    public void MinigameWin() {
        win = true;
        //gameActive = false;
        if (!roomProps.ContainsKey("M1")) {
            roomProps.Add("M1", false);
        } else {
            roomProps["M1"] = false;
        }
        PhotonNetwork.room.SetCustomProperties(roomProps);
        // Debug.Log("<Color=Red>Setting gameActive to: </Color>" + gameActive);
        winTimer = 0.0f;
        resetTimer = 0.0f;
        PhotonNetwork.Instantiate("Coin", transform.position + new Vector3(Random.Range(-5, 5), blockSpawnHeight, Random.Range(-5, 5)), Quaternion.identity, 0);

        Debug.Log("We win!! Awesomeness for everyone here :)");
    }
}