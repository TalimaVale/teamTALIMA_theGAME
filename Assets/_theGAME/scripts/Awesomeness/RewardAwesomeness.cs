using UnityEngine;
using Photon;

public class RewardAwesomeness : Awesomeness {

    Vector3 coinPosition;

    public float archSpeed = 7.0f;

    // Use this for initialization
    void Start() {
        coinPosition = transform.position;
    }

    // Update is called once per frame
    void Update() {
        if (!PhotonNetwork.isMasterClient) {
            if (transform.position != coinPosition) {
                transform.position = Vector3.Lerp(transform.position, coinPosition, archSpeed * Time.deltaTime);
            }
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.isWriting) {
            stream.SendNext(transform.position);
        } else {
            coinPosition = (Vector3)stream.ReceiveNext();
        }
    }

    [PunRPC]
    void Collect(int viewID) {
        PhotonNetwork.Destroy(PhotonView.Find(viewID).gameObject);
    }
}
