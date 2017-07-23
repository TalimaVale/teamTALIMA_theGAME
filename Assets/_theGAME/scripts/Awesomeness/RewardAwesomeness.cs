using UnityEngine;
using Photon;

public class RewardAwesomeness : Awesomeness {

    Vector3 coinPosition;

    public float archSpeed = 7.0f;

    public double cooldown = 10.0f;
    private double collectCountdown;

    // Timestamp vars
    double startTime;
    double mostRecentTimestamp;

    // Use this for initialization
    void Start() {
        coinPosition = transform.position;
        
        startTime = PhotonNetwork.time;
        collectCountdown = cooldown;
    }

    // Update is called once per frame
    void Update() {
        if (!PhotonNetwork.isMasterClient) {
            if (transform.position != coinPosition) {
                transform.position = Vector3.Lerp(transform.position, coinPosition, archSpeed * Time.deltaTime);
            }
        }

        if(collectCountdown > 0) {
            double diff = PhotonNetwork.time - startTime;
            collectCountdown = cooldown - diff;

            if (collectCountdown < 0) collectCountdown = 0;
            //Debug.Log(collectCountdown);
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.isWriting) {
            stream.SendNext(transform.position);
        } else {
            coinPosition = (Vector3)stream.ReceiveNext();
        }
    }

    override public void CallAddBawesomeness(PlayerController player) {
        if (player.collectCountdown == 0) {
            Debug.Log("player.collectCountdown = " + player.collectCountdown);
            if (collectCountdown == 0) {
                Debug.Log("Coin countdown is AT ZERO");
                player.AddBawesomeness(value, PhotonNetwork.time);
                photonView.RPC("Collect", PhotonTargets.MasterClient, photonView.viewID);
            } else {
                Debug.Log("Coin is still COUNTING DOWN");
                player.AddBawesomeness(value, collectCountdown, PhotonNetwork.time);
                photonView.RPC("Collect", PhotonTargets.MasterClient, photonView.viewID);
            }
        } else {
            Debug.Log("We are on cooldown for collecting Reward Awesomeness");
        }
    }

    [PunRPC]
    void Collect(int viewID) {
        Debug.Log("REWARDAwesomeness Collect RPC");
        PhotonNetwork.Destroy(PhotonView.Find(viewID).gameObject);
    }
}
