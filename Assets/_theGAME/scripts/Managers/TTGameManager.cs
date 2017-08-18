using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon;

public class TTGameManager : PunBehaviour {

    static public TTGameManager instance = null;

    CustomOperations customOps;

    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;

    [Tooltip("Array of player spawn points")]
    public PlayerSpawnPoint[] spawnPoints;

    // UI Elements
    public Text txtBawesomeness;

    void Awake() {
        // Check if instance already exists, if not set instance to 'this', if instance is not 'this' destory 'this'
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        customOps = FindObjectOfType<NetworkManager>().GetComponent<CustomOperations>();

        spawnPoints = FindObjectsOfType<PlayerSpawnPoint>();

        txtBawesomeness.text = "Bawesomeness:";
    }

    void Start() {
        if (playerPrefab == null) {
            Debug.LogError("<Color=Red>Missing</Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'");
        } else if (PlayerController.localPlayer == null) {
            Debug.Log("<Color=Red>Player Instantiate</Color> We are Instantiating LocalPlayer from " + SceneManager.GetActiveScene().name);
            PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
        }
        Debug.Log("<Color=Red>Start() for TTGameManager</Color>");
    }

    void Update() {
        if(Input.GetButtonUp("Hello World")) {
            customOps.OpHelloWorld();
        }
    }

    public override void OnOwnershipRequest(object[] viewAndPlayer) {
        Debug.Log("ONOWNERSHIPREQUEST()");
        PhotonView view = viewAndPlayer[0] as PhotonView;
        PhotonPlayer requestingPlayer = viewAndPlayer[1] as PhotonPlayer;

        Debug.Log("OnOwnershipRequest(): Player " + requestingPlayer + " requests ownership of: " + view + ".");
        view.TransferOwnership(requestingPlayer.ID);
    }
}