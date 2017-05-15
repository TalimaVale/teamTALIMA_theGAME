using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;

public class PlayerController : PunBehaviour {

    //MyNetworkManager networkManager;
    GameManager gameManager;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject localPlayer;

    public Transform playerCanvas;
    public Vector3 ScreenOffset = new Vector3(0f, 30f, 0f);
    public float _characterControllerHeight = 0f;
    public Vector3 _targetPosition;

    [Tooltip("The Player's UI GameObject Prefab")]
    public Text txtPlayerUsername;

    void Awake() {
        //networkManager = FindObjectOfType<MyNetworkManager>();
        gameManager = FindObjectOfType<GameManager>();
        
        // keep track of the localPlayer to prevent instantiation when levels are synchronized
        if (photonView.isMine) {
            localPlayer = this.gameObject;
        }
    }

    // Use this for initialization
    void Start () {
        txtPlayerUsername = GetComponentInChildren<Text>();
        //txtPlayerUsername.text = networkManager.playerUsername;
        playerCanvas = transform.Find("Player Canvas");
        txtPlayerUsername.text = photonView.owner.NickName;

        if (photonView.isMine) {
            GetComponent<MeshRenderer>().material.color = new Color(8/255f, 168/255f, 241/255f, 1);
            //GetComponent<MeshRenderer>().material.color = new Color(0x08/ 255f, 0xA8/255f, 0xF1/255f, 1);
            //GetComponent<MeshRenderer>().material.color = new Color32(8, 168, 241, 255);
            
        }
    }

    void Update() {
        if (!photonView.isMine) return;

        var y = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

        transform.Rotate(0, y, 0);
        transform.Translate(0, 0, -z);
    }

    void LateUpdate() {
        playerCanvas.rotation = Camera.main.transform.rotation;
    }

    // in an "observed" script:
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        /*
        if (stream.isWriting) {
            stream.SendNext(transform.position);
        } else {
            this.transform.position = (Vector3)stream.ReceiveNext();
        }
        */
    }

    private void OnTriggerEnter(Collider other) {
        if (!localPlayer) return;
        if (other.CompareTag("Respawn Shield")) Respawn();
    }
    
    public void Respawn() {
        if (!localPlayer) return;

        Debug.Log("Choosing a spawn point.");
        // Default spawn point
        Vector3 spawnPoint = new Vector3(0, 3, 0);

        // If array of spawn points exists, choose a random one
        SpawnPoint[] spawnPoints = gameManager.spawnPoints;
        if (spawnPoints != null && spawnPoints.Length > 0) {
            spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
        }

        Debug.Log("Spawn Point chosen: " + spawnPoint);
        transform.position = spawnPoint;
    }
}