using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;

public class PlayerController : PunBehaviour {
    
    GameManager gameManager;

    Camera mainCamera;
    CameraController cameraController;
    public float fadeRate = 0.02f;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject localPlayer;
    public bool isLocalPlayer { get { return photonView.isMine; } }
    
    public float rotationSlerpSpeed = 224f;

    public Transform playerCanvas;
    public Vector3 ScreenOffset = new Vector3(0f, 30f, 0f);
    public float _characterControllerHeight = 0f;
    public Vector3 _targetPosition;

    [Tooltip("The Player's UI GameObject Prefab")]
    public Text txtPlayerUsername;

    void Awake() {
        gameManager = FindObjectOfType<GameManager>();
        mainCamera = Camera.main;
        cameraController = mainCamera.GetComponent<CameraController>();
        
        // keep track of the localPlayer to prevent instantiation when levels are synchronized
        if (isLocalPlayer) {
            localPlayer = this.gameObject;
            cameraController.target = transform;
        }
    }

    // Use this for initialization
    void Start () {
        txtPlayerUsername = GetComponentInChildren<Text>();
        playerCanvas = transform.Find("Player Canvas");
        txtPlayerUsername.text = photonView.owner.NickName;

        if (isLocalPlayer) {
            GetComponent<MeshRenderer>().material.color = new Color(8/255f, 168/255f, 241/255f, 1);
            //GetComponent<MeshRenderer>().material.color = new Color(0x08/ 255f, 0xA8/255f, 0xF1/255f, 1);
            //GetComponent<MeshRenderer>().material.color = new Color32(8, 168, 241, 255);
            
        }
    }

    void Update() {
        if (!isLocalPlayer) return;
        
        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 3.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

        if (Input.GetMouseButton(0) && Input.GetMouseButton(1)) {
            if (z == 0) z = 1 * Time.deltaTime * 3.0f;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, mainCamera.transform.rotation.eulerAngles.y, 0f), Time.deltaTime * rotationSlerpSpeed);
        } else if (!Input.GetMouseButton(0) && (x != 0f || z != 0f)) {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, mainCamera.transform.rotation.eulerAngles.y, 0f), Time.deltaTime * rotationSlerpSpeed);
        }
        
        transform.Translate(x, 0, z);
    }

    void LateUpdate() {
        playerCanvas.rotation = mainCamera.transform.rotation;
        
        // Handle player fade when camera moves between 1st and 3rd person views
        // if camera is within 1st-person view distance
        if (cameraController.curDistance < 2) {
            // if zooming to 1st person
            if (cameraController.distance <= cameraController.curDistance) {
                cameraController.distance = Mathf.Clamp(cameraController.distance - fadeRate * 3, 0, cameraController.distanceMax);
                playerFade(-1.0f);
            // else if zooming from 1st person
            } else {
                cameraController.distance += fadeRate;
                if (cameraController.curDistance > 1f) {
                    playerFade(fadeRate);
                }
            }
        // if camera is within 3rd-person view distance
        } else if (cameraController.curDistance > 2.5f) {
            playerFade(1);
        // if camera is "between" optimal view distances, correct alpha if necesary
        } else if (this.GetComponent<MeshRenderer>().material.color.a != 1) {
            cameraController.distance += fadeRate;
            playerFade(fadeRate);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (!isLocalPlayer) return;
        if (other.CompareTag("Respawn Shield")) Respawn();
    }
    
    public void Respawn() {
        if (!isLocalPlayer) return;

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

    public void playerFade(float alphaValue) {
        if (!isLocalPlayer) return;

        MeshRenderer[] renderers = localPlayer.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++) {
            Color color = renderers[i].material.color;
            color.a = Mathf.Clamp(color.a + alphaValue, 0, 1);
            renderers[i].material.color = color;
        }
    }
}
