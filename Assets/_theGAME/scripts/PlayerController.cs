using UnityEngine;
using UnityEngine.UI;
using Photon;

public class PlayerController : PunBehaviour {

    TTGameManager gameManager;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the scene")]
    public static GameObject localPlayer;
    public bool isLocalPlayer { get { return photonView.isMine; } }

    private int bawesomeness = 0;

    // Player camera
    Camera mainCamera;
    CameraController cameraController;
    public float fadeRate = 0.02f;
    
    // Player UI
    public Transform playerCanvas;
    public Text txtPlayerUsername;

    // Player movement
    CharacterController controller;
    public float walkSpeed = 2f;
    public float runSpeed = 6f;
    public float rotationSpeed = 124f;
    public float gravity = -12f;
    public float jumpHeight = 1f;
    public float speedSmoothTime = 0.1f;

    float speedSmoothVelocity;
    float currentSpeed;
    float velocityY;

    // Player interaction
    public float playerReach = 3.0f;
    public Vector3 holdLocalVector = new Vector3(0.0f, 0.5f, 1.1f);
    public GameObject heldItem;
    private bool hasItem;

    public double collectCooldown = 0;
    public double collectCountdown = 0;
    private double cdInitTime;

    void Awake() {
        gameManager = FindObjectOfType<TTGameManager>();

        mainCamera = Camera.main;
        cameraController = mainCamera.GetComponent<CameraController>();
        controller = GetComponent<CharacterController>();

        // Is this the localPlayer
        if (isLocalPlayer) {
            localPlayer = this.gameObject;
            cameraController.target = transform;
        }

        Debug.Log("My starting position: " + transform.position);
    }

    // Use this for initialization
    void Start() {
        playerCanvas = transform.Find("Player Canvas");
        txtPlayerUsername = GetComponentInChildren<Text>();

        txtPlayerUsername.text = photonView.owner.NickName; // PhotonNetwork.player.NickName;
        
        hasItem = false;

        // Is this the localPlayer
        if (isLocalPlayer) {
            GetComponent<MeshRenderer>().material.color = new Color(8 / 255f, 168 / 255f, 241 / 255f, 1);
            gameManager.txtBawesomeness.text = "Bawesomeness: " + bawesomeness;
        }

        Debug.Log("Our current bawesomeness: " + bawesomeness);
        Debug.Log("My starting position: " + transform.position);
    }

    void Update() {
        if (!isLocalPlayer) return;

        //// Player interaction
        hasItem = (heldItem == null) ? false : true;

        if (Input.GetButtonDown("Interact")) {
            if (hasItem) {
                Debug.Log("Interacting with our heldItem");
                heldItem.SendMessage("Interact", photonView.viewID, SendMessageOptions.RequireReceiver);
            } else {
                Collider closest;
                if (FindClosestInteract(out closest)) {
                    Debug.Log("Attempting to interact with: " + closest.name);
                    closest.gameObject.SendMessage("Interact", photonView.viewID, SendMessageOptions.RequireReceiver);
                } else {
                    Debug.Log("No Interact object within playerReach");
                }
            }
        }

        //// Reward-Awesomeness Cooldown
        if (collectCountdown > 0) {
            double diff = PhotonNetwork.time - cdInitTime;
            collectCountdown = collectCooldown - diff;

            if (collectCountdown < 0) collectCountdown = 0;
            //Debug.Log("Player cooldown: " + collectCountdown);
        }

        //// Player movement
        var x = Input.GetAxis("Horizontal");
        var z = Input.GetAxis("Vertical");

        // Rotation
        if (Input.GetMouseButton(0) && Input.GetMouseButton(1)) {
            if (z == 0) z = 1;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, mainCamera.transform.rotation.eulerAngles.y, 0f), Time.deltaTime * rotationSpeed);
        } else if (!Input.GetMouseButton(0) && (x != 0f || z != 0f)) {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, mainCamera.transform.rotation.eulerAngles.y, 0f), Time.deltaTime * rotationSpeed);
        }
        
        // Jump
        if (Input.GetKeyDown(KeyCode.Space)) Jump();

        // Running
        bool running = Input.GetKey(KeyCode.LeftShift);

        // Speed?
        float targetSpeed = ((running) ? runSpeed : walkSpeed) * ((x != 0 || z != 0) ? 1 : 0);
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

        // Velocity & Direction
        velocityY += Time.deltaTime * gravity;
        Vector3 velocity = (transform.rotation * new Vector3(x, 0.0f, z)) * currentSpeed + Vector3.up * velocityY;

        controller.Move(velocity * Time.deltaTime);

        // Grounded?
        if (controller.isGrounded) velocityY = 0;
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

    private void OnDestroy() {
        if (isLocalPlayer && heldItem != null) {
            RaycastHit hit;
            if (Physics.BoxCast(transform.position, new Vector3(.5f, .5f, .5f), Vector3.down, out hit, heldItem.transform.rotation, Mathf.Infinity, -1)) {
                Debug.Log("hit.distance: " + hit.distance);
                hit.point += new Vector3(0, heldItem.transform.localScale.y / 2, 0);
                heldItem.transform.position = hit.point;
            } else {
                Debug.Log("No hits detected. Drop heldItem at player's transform.position");
                heldItem.transform.position = transform.position;
            }
            heldItem.layer = LayerMask.NameToLayer("Interact");
            heldItem.transform.parent = null;
        }
    }

    void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info) {
        if (stream.isWriting) {
            //stream.SendNext(hasItem);
        } else {
            //hasItem = (bool)stream.ReceiveNext();
        }
    }

    void Jump() {
        if (controller.isGrounded) {
            float jumpVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight);
            velocityY = jumpVelocity;
        }
    }

    public void AddBawesomeness(int value, double cooldown = 0, double netTimestamp = 0) {
        Debug.Log("Calling AddBawesomeness");
        if (!isLocalPlayer) return;
        Debug.Log("Calling AddBawesomeness, isLocalPlayer");

        bawesomeness += value;
        gameManager.txtBawesomeness.text = "Bawesomeness: " + bawesomeness;
        Debug.Log("Our current bawesomeness: " + bawesomeness);

        if(cooldown != 0) {
            collectCooldown = cooldown;
            collectCountdown = collectCooldown;
            Debug.Log("player.collectCountdown = " + collectCountdown);
            cdInitTime = netTimestamp;
            //Debug.Log("Player's collectCooldown is: " + collectCooldown);
        }
    }

    public void Respawn() {
        if (!isLocalPlayer) return;

        // Default spawn point
        Vector3 spawnPoint = new Vector3(0, 3, 0);

        // If array of spawn points exists, choose a random one
        PlayerSpawnPoint[] spawnPoints = gameManager.spawnPoints;
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

    public bool FindClosestInteract(out Collider closest) {
        Collider closestFound = null;
        float distance = playerReach * playerReach;
        Collider[] colliders = Physics.OverlapSphere(transform.position, playerReach, 1<<LayerMask.NameToLayer("Interact"));
        foreach (Collider collider in colliders) {
            float sqrMagnitude = (collider.transform.position - transform.position).sqrMagnitude;
            if (sqrMagnitude < distance) {
                closestFound = collider;
                distance = sqrMagnitude;
            }
        };
        closest = closestFound;
        return closestFound != null;
    }
}

// TODO: Fix Player Jump (can multi-jump)
// TODO: If player hasItem, then turn on heldItem collider
// TODO: Consider -- When player is holding an item, scale item down