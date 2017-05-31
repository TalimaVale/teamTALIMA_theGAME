using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon;

public class GameManager : PunBehaviour {

    static public GameManager instance = null;

    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;

    [Tooltip("Array of player spawn points")]
    public SpawnPoint[] spawnPoints;

    void Awake() {
        // Check if instance already exists, if not set instance to 'this', if instance is not 'this' destory 'this'
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        // Sets this gameObject to not be destroyed when reloading scene
        //DontDestroyOnLoad(gameObject);

        spawnPoints = FindObjectsOfType<SpawnPoint>();
    }

    void Start() {
        if (playerPrefab == null) {
            Debug.LogError("<Color=Red>Missing</Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'");
        } else if (PlayerController.localPlayer == null) {
            Debug.Log("We are Instantiating LocalPlayer from " + SceneManager.GetActiveScene().name);
            PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
        }
    }
}



// #username#exppoints#money
// #username2#exppoints#money