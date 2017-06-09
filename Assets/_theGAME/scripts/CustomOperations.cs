using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;

public class CustomOperations : Photon.PunBehaviour {

    NetworkingPeer networkingPeer;

    void Awake() {
        networkingPeer = PhotonNetwork.networkingPeer;
    }

    // Request 'Hello World' operation
    public void OpHelloWorld() {
        if (networkingPeer != null) {
            var parameter = new Dictionary<byte, object>();
            parameter.Add((byte)100, "Hello World");

            networkingPeer.OpCustom((byte)190, parameter, true);
            Debug.Log("<Color=Magenta>Hello Server World!</Color>");
        }
    }

    // Receive 'HelloWorld' response
    public static void OpHelloWorldResponse(OperationResponse operationResponse) {
        if (operationResponse.ReturnCode == 0) {
            // show the response message
            Debug.Log("<Color=Magenta> SV: " + operationResponse.Parameters[100] + "</Color>");
        } else {
            // show the error message
            Debug.Log(operationResponse.DebugMessage);
        }
    }


    #region Implementation of IPhotonPeerListener

    //// We need to call this
    //void IPhotonPeerListener.OnOperationResponse(OperationResponse operationResponse) {
    //    Debug.Log("HELLO WORLD!!");

    //    #region Extra OnOperationResponse Code

    //    if (PhotonNetwork.networkingPeer.State == ClientState.Disconnecting) {
    //        if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational) {
    //            Debug.Log("OperationResponse ignored while disconnecting. Code: " + operationResponse.OperationCode);
    //        }
    //        return;
    //    }

    //    // extra logging for error debugging (helping developers with a bit of automated analysis)
    //    if (operationResponse.ReturnCode == 0) {
    //        if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
    //            Debug.Log(operationResponse.ToString());
    //    } else {
    //        if (operationResponse.ReturnCode == ErrorCode.OperationNotAllowedInCurrentState) {
    //            Debug.LogError("Operation " + operationResponse.OperationCode + " could not be executed (yet). Wait for state JoinedLobby or ConnectedToMaster and their callbacks before calling operations. WebRPCs need a server-side configuration. Enum OperationCode helps identify the operation.");
    //        } else if (operationResponse.ReturnCode == ErrorCode.PluginReportedError) {
    //            Debug.LogError("Operation " + operationResponse.OperationCode + " failed in a server-side plugin. Check the configuration in the Dashboard. Message from server-plugin: " + operationResponse.DebugMessage);
    //        } else if (operationResponse.ReturnCode == ErrorCode.NoRandomMatchFound) {
    //            Debug.LogWarning("Operation failed: " + operationResponse.ToStringFull());
    //        } else {
    //            Debug.LogError("Operation failed: " + operationResponse.ToStringFull() + " Server: " + networkingPeer.Server);
    //        }
    //    }

    //    // use the "secret" or "token" whenever we get it. doesn't really matter if it's in AuthResponse.
    //    if (operationResponse.Parameters.ContainsKey(ParameterCode.Secret)) {
    //        if (networkingPeer.AuthValues == null) {
    //            networkingPeer.AuthValues = new AuthenticationValues();
    //            // this.DebugReturn(DebugLevel.ERROR, "Server returned secret. Created AuthValues.");
    //        }

    //        networkingPeer.AuthValues.Token = operationResponse[ParameterCode.Secret] as string;
    //        //networkingPeer.tokenCache = networkingPeer.AuthValues.Token;
    //    }

    //    #endregion

    //    switch (operationResponse.OperationCode) {
    //        case OperationCode.HelloWorld:
    //            if (operationResponse.ReturnCode == 0) {
    //                // show the response message
    //                Debug.Log("<Color=Magenta> SV: " + operationResponse.Parameters[100] + "</Color>");
    //            } else {
    //                // show the error message
    //                Debug.Log(operationResponse.DebugMessage);
    //            }
    //            break;

    //        default:
    //            Debug.LogWarning(string.Format("OperationResponse unhandled: {0}", operationResponse.ToString()));
    //            break;
    //    }
    //}

    //#region Additional IPhotonPeerListener Callbacks

    //public void DebugReturn(DebugLevel level, string message) {
    //    ((IPhotonPeerListener)networkingPeer).DebugReturn(level, message);
    //}

    //public void OnStatusChanged(StatusCode statusCode) {
    //    ((IPhotonPeerListener)networkingPeer).OnStatusChanged(statusCode);
    //}

    //public void OnEvent(EventData eventData) {
    //    ((IPhotonPeerListener)networkingPeer).OnEvent(eventData);
    //}

    //#endregion

    #endregion
}
