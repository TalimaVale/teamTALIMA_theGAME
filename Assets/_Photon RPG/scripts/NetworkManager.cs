/*
 * NetworkManager script
 * ---
 * Connects to master server, Connects to game server (room),
 * Manages player connection,
 * Disconnects from game server (leaves room), Disconnects from master server
 * 
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon;

public class NetworkManager : Photon.PunBehaviour {

    static public NetworkManager instance = null;
    GameManager gameManager;
    
    [Tooltip("UI Text informing player the connection is in progress")]
    public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;

    string _gameVersion = "0.0.2";  // client version
    bool isConnecting;              // are we currently connecting
    
    void Awake() {
        // Check if instance already exists, if not set instance to 'this', if instance is not 'this' destory 'this'
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        // Sets this gameObject to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

        // #Critical, auto-set in PhotonServerSettings. We don't join a lobby
        PhotonNetwork.autoJoinLobby = false;

        // #Critical, we can use PhotonNetwork.LoadLevel() on the master client and all clients in the room sync their level automatically
        PhotonNetwork.automaticallySyncScene = true;

        // #NotImportant, force LogLevel
        PhotonNetwork.logLevel = Loglevel;
    }

    ///<summary>
    /// Start the connection process. If connected load 'Level 1', else connect to Photon Server.
    /// </summary>
    public void Connect() {
        isConnecting = true;
        Debug.Log("<Color=Blue>Connect()</Color> -- isConnecting was just set to: " + isConnecting);

        // are we connected
        if (PhotonNetwork.connected) {
            // join/create room 'Level 1'
            PhotonNetwork.JoinOrCreateRoom("Level 1", new RoomOptions { MaxPlayers = 14 }, null);
            Debug.Log("<Color=Blue>Connect()</Color> -- called JoinRoom('Level 1')");
        } else {
            // connect to Photon Online Server
            PhotonNetwork.ConnectUsingSettings(_gameVersion);
        }
    }

    public override void OnConnectedToMaster() {
        Debug.Log("<Color=Blue>OnConnectedToMaster()</Color>");

        Debug.Log("<Color=Blue>OnConnectedToMaster()</Color> -- isConnecting = " + isConnecting);
        // isConnecting is false typically when you lost or quit the game
        if (isConnecting) {
            // join/create room 'Level 1'
            PhotonNetwork.JoinOrCreateRoom("Level 1", new RoomOptions { MaxPlayers = 14 }, null);
            Debug.Log("<Color=Blue>OnConnectedToMaster()</Color> -- called JoinOrCreateRoom('Level 1')");
        }
    }

    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg) {
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

        Debug.Log("<Color=Blue>OnJoinedRoom()</Color> -- Master Server Address: " + PhotonNetwork.networkingPeer.MasterServerAddress);
        Debug.Log("<Color=Blue>OnJoinedRoom()</Color> -- Game Server Address: " + PhotonNetwork.networkingPeer.GameServerAddress);
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer) {
        Debug.Log("<Color=Blue>OnPhotonPlayerConnected()</Color> -- New player connected: " + newPlayer.NickName);
    }

    ///<summary>
    /// Leave game server.
    /// </summary>
    public void LeaveRoom() {
        Debug.Log("<Color=Blue>LeaveRoom()</Color> -- Leaving game server");
        PhotonNetwork.LeaveRoom();
    }
    
    public override void OnLeftRoom() {
        isConnecting = false;
        Debug.Log("<Color=Blue>LeaveRoom()</Color> -- isConnecting was just set to: " + isConnecting);
        Debug.Log("<Color=Blue>OnLeftRoom()</Color> -- Loading Main Menu");
        PhotonNetwork.LoadLevel("Main Menu");
    }

    public override void OnDisconnectedFromPhoton() {
        Debug.LogWarning("<Color=Red>OnDisconnectedFromPhoton()</Color>");
    }
}