using UnityEngine;
using Photon;

public class minigameBlock : PunBehaviour, IPunObservable {

	private Rigidbody rb;
	private PlayerController Owner;

	private Vector3 MostRecentNetworkPos;
	private Quaternion MostRecentNetworkRotation;

	private bool HasReceivedUpdates { get; set; }

	public bool HasOwner
	{
		get
		{
			return (Owner != null);
		}
	}

	private Vector3 HoldLocalVector = new Vector3(0f, .5f, 1.1f);

	// Use this for initialization
	void Start ()
	{
        rb = GetComponent<Rigidbody>();

		MostRecentNetworkPos = new Vector3();
		MostRecentNetworkRotation = new Quaternion();
		HasReceivedUpdates = false;
	}

	void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if(stream.isWriting)
		{
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
		}
		else
		{
			HasReceivedUpdates = true;
			MostRecentNetworkPos = (Vector3)stream.ReceiveNext();
			MostRecentNetworkRotation = (Quaternion)stream.ReceiveNext();
		}
	}

    void Update()
	{ 
		if(!HasOwner && HasReceivedUpdates)
		{
			if(transform.position != MostRecentNetworkPos)
			{
				transform.position = Vector3.Lerp(transform.position, MostRecentNetworkPos, Time.deltaTime * 1f);
			}

			if(transform.rotation != MostRecentNetworkRotation)
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, MostRecentNetworkRotation, Time.deltaTime * 1f);
			}
		}
    }

	public void Interact(int playerViewID)
	{
		GameObject InteractingPlayer = PhotonView.Find(playerViewID).gameObject;
		if(!HasOwner)
		{
            Debug.Log("Block is being picked up by player: " + InteractingPlayer.GetPhotonView().owner.NickName);
            photonView.RPC("PickUpBlock", PhotonTargets.AllBuffered, playerViewID);
        }
		else if(HasOwner && Owner.photonView.viewID == playerViewID)
		{
            Debug.Log("Block is being DROPPED by player: " + InteractingPlayer.GetPhotonView().owner.NickName);
            photonView.RPC("DropBlock", PhotonTargets.AllBuffered);
        }
		else
		{
            Debug.Log("Attempt made to steal block");
        }
    }

    [PunRPC]
    void PickUpBlock(int playerViewID)
	{
		Owner = PhotonView.Find(playerViewID).GetComponent<PlayerController>();
		Owner.heldItem = gameObject;

		transform.SetParent(Owner.transform, false);
		transform.localPosition = HoldLocalVector;
		transform.localRotation = Quaternion.identity;
		rb.isKinematic = true;
	}

    [PunRPC]
    void DropBlock()
	{
		Owner.heldItem = null;
		Owner = null;
		
		transform.SetParent(null, true);
		rb.isKinematic = false;
	}
}
