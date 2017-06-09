using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;

public class PlayerController : PunBehaviour {
    
    GameManager gameManager;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject localPlayer;
    
    public float rotationSlerpSpeed = 224f;

    public Transform playerCanvas;
    public Vector3 ScreenOffset = new Vector3(0f, 30f, 0f);
    public float _characterControllerHeight = 0f;
    public Vector3 _targetPosition;

    [Tooltip("The Player's UI GameObject Prefab")]
    public Text txtPlayerUsername;

    void Awake() {
        gameManager = FindObjectOfType<GameManager>();
        
        // keep track of the localPlayer to prevent instantiation when levels are synchronized
        if (photonView.isMine) {
            localPlayer = this.gameObject;
            Camera.main.GetComponent<CameraController>().target = transform;
        }
    }

    // Use this for initialization
    void Start () {
        txtPlayerUsername = GetComponentInChildren<Text>();
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
        
        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 3.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

        if (Input.GetMouseButton(0) && Input.GetMouseButton(1)) {
            if (z == 0) z = 1 * Time.deltaTime * 3.0f;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f), Time.deltaTime * rotationSlerpSpeed);
        } else if (!Input.GetMouseButton(0) && (x != 0f || z != 0f)) {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f), Time.deltaTime * rotationSlerpSpeed);
        }
        
        transform.Translate(x, 0, z);
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