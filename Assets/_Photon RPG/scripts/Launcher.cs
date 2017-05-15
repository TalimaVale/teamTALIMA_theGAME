using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon;

public class Launcher : PunBehaviour, ISubmitHandler {

    [Tooltip("UI InputField for player username")]
    InputField field;
    [Tooltip("UI Text informing player the connection is in progress")]
    public GameObject progressLabel;

    public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;

    MyNetworkManager networkManager;
    string _gameVersion = "1";
    bool isConnecting;

    void Awake() {
        // #Critical, auto-set in PhotonServerSettings. We don't join a lobby
        PhotonNetwork.autoJoinLobby = false;

        // #Critical, we can use PhotonNetwork.LoadLevel() on the master client and all clients in the room sync their level automatically
        PhotonNetwork.automaticallySyncScene = true;

        // #NotImportant, force LogLevel
        PhotonNetwork.logLevel = Loglevel;
    }

    void Start() {
        networkManager = FindObjectOfType<MyNetworkManager>();
        field = GameObject.FindGameObjectWithTag("Username Input").GetComponent<InputField>();
        progressLabel.SetActive(false);
    }

    void Update() {
        // fire Submit event when InputField is not focused
        if (!field.isFocused && field.text != "" && Input.GetButtonDown("Submit")) {
            PassUsername();
        }
    }

    // fire Submit event when InputField is focused
    void ISubmitHandler.OnSubmit(BaseEventData eventData) {
        PassUsername();
    }

    // Pass inputed username to NetworkManager to instantiate new player - Button.onClick()
    public void PassUsername() {
        networkManager.ReceiveUsername(field.text + " ");
        Connect();
        Debug.Log("<Color=Blue>PassUsername()</Color> -- We call Connect()");
    }

    ///<summary>
    /// Start the connection process. If connected load 'Level 1', else connect to Photon Cloud Network.
    /// </summary>
    public void Connect() {
        isConnecting = true;
        progressLabel.SetActive(true);
        Debug.Log("<Color=Blue>Connect()</Color> -- isConnecting was just set to: " + isConnecting);

        // are we connected
        if (PhotonNetwork.connected) {
            // join/create room 'Level 1'
            PhotonNetwork.JoinOrCreateRoom("Level 1", new RoomOptions { MaxPlayers = 14 }, null);
            Debug.Log("<Color=Blue>Connect()</Color -- called JoinRoom('Level 1')");
        } else {
            // connect to Photon Online Server
            PhotonNetwork.ConnectUsingSettings(_gameVersion);
        }
    }

    public override void OnConnectedToMaster() {
        Debug.Log("<Color=Blue>OnConnectedToMaster()</Color>");

        // isConnecting is false typically when you lost or quit the game
        if (isConnecting) {
            // join/create room 'Level 1'
            PhotonNetwork.JoinOrCreateRoom("Level 1", new RoomOptions { MaxPlayers = 14 }, null);
            Debug.Log("<Color=Blue>OnConnectedToMaster()</Color> -- called JoinRoom('Level 1')");
        }
    }

    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg) {
        // PhotonNetwork.CreateRoom("Level 1", new RoomOptions() { MaxPlayers = 14 }, null);
        Debug.Log("<Color=Blue>OnPhotonJoinRoomFailed()</Color> -- we failed to join room 'Level 1'");
    }

    public override void OnJoinedRoom() {
        Debug.Log("<Color=Blue>OnJoinedRoom()</Color> -- now this client is in a room.");

        // #Critical, if we are the first player load level, else rely on PhotonNetwork.automaticallySyncScene to sync our instance scene
        if (PhotonNetwork.room.PlayerCount == 1) {
            // #Critical, load the level
            PhotonNetwork.LoadLevel("Level 1");
        } else {
            Debug.Log("<Color=Blue>OnJoinedRoom()</Color> -- no Level loaded because there is more than 1 player here");
        }
    }

    public override void OnDisconnectedFromPhoton() {
        Debug.LogWarning("<Color=Red>OnDisconnectedFromPhoton()</Color>");
        progressLabel.SetActive(false);
    }
}