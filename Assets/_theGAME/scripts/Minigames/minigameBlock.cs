using UnityEngine;
using Photon;

public class minigameBlock : PunBehaviour, IPunObservable {

	private Rigidbody rb;
	public PlayerController owner;

	public bool HasOwner
	{
		get
		{
			return (owner != null);
		}
	}

	private Vector3 HoldLocalVector = new Vector3(0f, .5f, 1.1f);

	// Use this for initialization
	void Start ()
	{
        rb = GetComponent<Rigidbody>();
	}

	void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if(stream.isWriting)
		{
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);

			stream.SendNext(transform.localPosition);
			stream.SendNext(transform.localRotation);
		}
		else
		{
			transform.position = (Vector3)stream.ReceiveNext();
			transform.rotation = (Quaternion)stream.ReceiveNext();

			transform.localPosition = (Vector3)stream.ReceiveNext();
			transform.localRotation = (Quaternion)stream.ReceiveNext();
		}
	}

    void Update()
	{
        if (HasOwner)
		{
            rb.isKinematic = true;
            
			if(transform.parent != owner.transform)
			{
				photonView.RPC("UpdateParent", PhotonTargets.AllBuffered);
			}
        }
		else
		{
            rb.isKinematic = false;
			transform.parent = null;
        }
    }

	[PunRPC]
	public void UpdateParent()
	{
		transform.parent = owner.transform;
		transform.localPosition = HoldLocalVector;
		transform.localRotation = Quaternion.identity;
	}

	public void Interact(PlayerController player)
	{
        if(!HasOwner)
		{
            if (player.heldItem != null) return;

            Debug.Log("Block is being picked up by player: " + player.photonView.owner.NickName);
            photonView.RPC("PickUpBlock", PhotonTargets.All, player.photonView.viewID);
        }
		else if(HasOwner && player == owner)
		{
            Debug.Log("Block is being DROPPED by player: " + player.photonView.owner.NickName);
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
        owner = PhotonView.Find(playerViewID).GetComponent<PlayerController>();
        owner.heldItem = gameObject;
        
        rb.isKinematic = true;
    }

    [PunRPC]
    void DropBlock()
	{
        owner.heldItem = null;
        owner = null;
        
        rb.isKinematic = false;
    }
}
