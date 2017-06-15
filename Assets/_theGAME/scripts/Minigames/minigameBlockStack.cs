using System.Collections.Generic;
using UnityEngine;
using Photon;

public class minigameBlockStack : PunBehaviour {

    // Block Prefab
    public GameObject block;

    // Is minigame being played?
    private bool gameActive;

    public int blockCount = 5;
    public float blockSpawnHeight = 2;

    // Use this for initialization
    void Start () {
        if (block == null) Debug.LogError("Minigame Console's 'block' prefab = null");

        gameActive = false;
    }

    void Update() {
        if (gameActive != true) return;

        if (Physics.OverlapBox(transform.position + Vector3.up * 2f, new Vector3(.1f, 3f, .1f), Quaternion.identity, 1 << LayerMask.NameToLayer("Interact")).Length >= 3) {
            Debug.Log("WE WIN!!");
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.isWriting) {
            stream.SendNext(gameActive);
        } else {
            gameActive = (bool)stream.ReceiveNext();
        }
    }

    public void Interact() {
        if (gameActive == false) {
            gameActive = true;
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

    }
}