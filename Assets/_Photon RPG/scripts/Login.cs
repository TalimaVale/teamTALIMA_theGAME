/*
 * Login script
 * ---
 * Receives login
 * 
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon;

public class Login : Photon.PunBehaviour, ISubmitHandler {

    [Tooltip("UI InputField - Accepts player username")]
    public InputField field;
    [Tooltip("UI Text informing player the connection is in progress")]
    public GameObject progressLabel;
    [Tooltip("UI Text informing player the connection is in progress")]
    public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;

    NetworkManager networkManager;

    void Awake() {
        networkManager = FindObjectOfType<NetworkManager>();
    }

    void Start() {
        field = GameObject.FindGameObjectWithTag("Username Input").GetComponent<InputField>();
        progressLabel.SetActive(false);
    }

    void Update() {
        // fire Submit event when InputField is not focused
        if (!field.isFocused && field.text != "" && Input.GetButtonDown("Submit")) {
            playerLogin();
        }
    }

    // fire Submit event when InputField is focused
    void ISubmitHandler.OnSubmit(BaseEventData eventData) {
        playerLogin();
    }

    // Pass inputed username to NetworkManager to instantiate new player - Button.onClick()
    public void playerLogin() {
        PhotonNetwork.playerName = (field.text + " ");
        progressLabel.SetActive(true);
        Debug.Log("<Color=Blue>playerLogin()</Color> -- We call Connect()");
        networkManager.Connect();
    }

    public override void OnDisconnectedFromPhoton() {
        progressLabel.SetActive(false);
        Debug.LogWarning("<Color=Red>OnDisconnectedFromPhoton()</Color>");
    }
}