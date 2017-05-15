using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Photon;

public class MyNetworkManager : PunBehaviour {

    static public MyNetworkManager instance = null;

    // Inputed player username
    public string playerUsername;

    void Awake() {
        // Check if instance already exists, if not set instance to 'this', if instance is not 'this' destory 'this'
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        // Sets this gameObject to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }

    // Receive username from 'Main Menu' input field
    public void ReceiveUsername(string username) {
        playerUsername = username;
        PhotonNetwork.playerName = username;
    }
}





//{

//    // Inputed player name
//    public string playerUsername;

//    // Receive username from 'Main Menu' input field
//    public void ReceiveUsername(string username) {
//        playerUsername = username;

//        PhotonNetwork.ConnectUsingSettings("1");
//        //StartCoroutine(ConnectOrHost());
//    }

//    // <summary>
//    // A coroutine that attempts to connect to a game server already hosted before hosting one itself.
//    // </summary>
//    IEnumerator ConnectOrHost() {
//        if (NetworkClient.active || NetworkServer.active) {
//            Debug.Log("A client or server is already active in this game instance.");
//            yield break;
//        }

//        Debug.Log("Connecting...");
//        var client = StartClient();
//        yield return WaitForClient(client);

//        if (client != null && client.isConnected) {
//            Debug.Log("Connected successfully.");
//        } else {
//            Debug.Log("Failed to connect. Attempting to host...");
//            client = StartHost();
//            yield return WaitForClient(client);

//            if (client != null && client.isConnected) {
//                Debug.Log("Hosted successfully.");
//            } else {
//                Debug.Log("Failed to host.");
//            }
//        }
//    }

//    // <summary>
//    // Returns a WaitUntil instruction for 'client' to be connected or disconnected. 
//    // It will shut 'client' down if the timeout is exceeded.
//    // </summary>
//    WaitUntil WaitForClient(NetworkClient client, float timeout = 10f) {
//        // This null check is because StartHost() will return null if the port is already in use:
//        if (client == null) return new WaitUntil(() => true);

//        bool connected = false, failed = false, disconnected = false;
//        float start = Time.unscaledTime;

//        client.RegisterHandler(MsgType.Connect, (NetworkMessage msg) => { connected = true; });
//        client.RegisterHandler(MsgType.Error, (NetworkMessage msg) => { failed = true; });
//        client.RegisterHandler(MsgType.Disconnect, (NetworkMessage msg) => { disconnected = true; });

//        return new WaitUntil(() => {
//            bool shutdown = (Time.unscaledTime - start) > timeout;
//            if (shutdown || disconnected || failed)
//                client.Shutdown();

//            // The client.isConnected condition here is just in case the client is already connected and 
//            //   the MsgType.Connect handler never fires:
//            return connected || failed || disconnected || shutdown || client.isConnected;
//        });
//    }

//    // Eventually is called during the StartHost() and StartClient() processes
//    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
//        Debug.Log("OnServerAddPlayer() called");
//        GameObject player = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
//        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
//    }
//}