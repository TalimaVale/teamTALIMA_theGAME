using UnityEngine;
using Photon;

public class minigameBlock : PunBehaviour, IPunObservable
{
	private Rigidbody rb;
	private PlayerController Owner;

	private Vector3 MostRecentNetworkPos; // public for the debug window, make private when done
	private Quaternion MostRecentNetworkRotation; // same as above

	private Vector3 HoldLocalVector = new Vector3(0f, .5f, 1.1f); // We use the same local vector everytime so just cache it

	public bool HasReceivedUpdates { get; private set; }

	public bool HasOwner
	{
		get
		{
			return (Owner != null);
		} 
	}

	/*
	 * Awake() vs Start()
	 * Start() is called when the script is initialized and enabled.
	 * Awake() is called on initialization even if the script is disabled.
	 * 
	 * I chose to use Awake() here because Photon's buffered RPC calls seem to
	 * execute _before_ the start function, so the RigidBody component reference "rb"
	 * would be null when it is used in PickupBlock() & DropBlock() causing a
	 * Null Reference Exception
	 */

	private void Awake()
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
			MostRecentNetworkPos = (Vector3)stream.ReceiveNext();
			MostRecentNetworkRotation = (Quaternion)stream.ReceiveNext();

			if(!HasReceivedUpdates) // The first time just put he block into place.
			{
				transform.position = MostRecentNetworkPos;
				transform.rotation = MostRecentNetworkRotation;
			}

			HasReceivedUpdates = true;
		}
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		if(PhotonNetwork.isMasterClient && HasOwner)
		{
			photonView.RPC("ForceSync", newPlayer, Owner.photonView.viewID, transform.position, transform.rotation);
		}
	}

	[PunRPC]
	void ForceSync(int parentViewID, Vector3 serverPosition, Quaternion serverRotation)
	{
		MostRecentNetworkPos = serverPosition;
		MostRecentNetworkRotation = serverRotation;
		PickUpBlock(parentViewID);
	}

	void Update()
	{
		/* 
		 * I want to think through this absolutely to make sure the flags are completely correct.
		 * If some changes are needed I'm not sure if this will hold. 
		 */

		if(!PhotonNetwork.isMasterClient && !HasOwner && HasReceivedUpdates)
		{
			if(transform.position != MostRecentNetworkPos)
			{
				transform.position = Vector3.Lerp(transform.position, MostRecentNetworkPos, Time.deltaTime);
			}

			if(transform.rotation != MostRecentNetworkRotation)
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, MostRecentNetworkRotation, Time.deltaTime);
			}
		}

		/*
		 * This is to keep updating the most recent transform even if we are the Master Client.
		 * 
		 * This is important because if we don't do this and the master client changes, the 
		 * MostRecentNetworkPos and MostRecentNetworkRotation will be zero.
		 */
		if(PhotonNetwork.isMasterClient)
		{
			MostRecentNetworkPos = transform.position;
			MostRecentNetworkRotation = transform.rotation;
		}
    }

	public void Interact(int playerViewID)
	{
		if(!HasOwner)
		{
			photonView.RPC("PickUpBlock", PhotonTargets.All, playerViewID);
        }
		else if(HasOwner && Owner.photonView.viewID == playerViewID)
		{
			photonView.RPC("DropBlock", PhotonTargets.All);
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
		Debug.Log("Block is being PICKED UP by player: " + Owner.photonView.owner.NickName);

		Owner.heldItem = gameObject;
		Owner.MostRecentHeldItem = gameObject;

		transform.SetParent(Owner.transform, false);
		transform.localPosition = HoldLocalVector;
		transform.localRotation = Quaternion.identity;
		rb.isKinematic = true;
	}

    [PunRPC]
    void DropBlock()
	{
		Debug.Log("Block is being DROPPED by player: " + Owner.photonView.owner.NickName);
		Owner.heldItem = null;
		Owner = null;

		transform.parent = null;
		rb.isKinematic = false;
	}
}
