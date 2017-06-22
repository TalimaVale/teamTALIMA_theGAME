using UnityEngine;
using UnityEngine.UI;
using Photon;

public class PlayerController : PunBehaviour
{

	GameManager gameManager;

	[Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
	public static GameObject localPlayer;

	// Player camera
	Camera mainCamera;
	CameraController cameraController;
	public float fadeRate = 0.02f;

	// Player UI
	public Transform playerCanvas;
	[Tooltip("The Player's UI GameObject Prefab")]
	public Text txtPlayerUsername;

	// Player movement
	private Rigidbody rb;
	public float playerSpeed = 3.0f;
	public float jumpForce = 4.0f;
	public float rotationSlerpSpeed = 224f;

	// Player interaction
	public float playerReach = 3.0f;
	public GameObject heldItem;
	public GameObject MostRecentHeldItem;
	private bool hasItem;

	//public Vector3 ScreenOffset = new Vector3(0f, 30f, 0f);
	//public float _characterControllerHeight = 0f;
	//public Vector3 _targetPosition;

	void Awake()
	{
		gameManager = FindObjectOfType<GameManager>();

		mainCamera = Camera.main;
		cameraController = mainCamera.GetComponent<CameraController>();

		// Is this the localPlayer
		if(photonView.isMine)
		{
			localPlayer = this.gameObject;
			mainCamera.GetComponent<CameraController>().target = transform;
		}

		rb = GetComponent<Rigidbody>();
	}

	private void OnGUI()
	{
		if(!photonView.isMine) return;

		GUI.Box(new Rect(0, 0, 200, 150), string.Empty);
		GUI.Label(new Rect(10, 5, 250, 25), PhotonNetwork.isMasterClient ? "MasterClient: true" : "MasterClient: false");
	}

	// Use this for initialization
	void Start()
	{
		playerCanvas = transform.Find("Player Canvas");
		txtPlayerUsername = GetComponentInChildren<Text>();
		txtPlayerUsername.text = photonView.owner.NickName;

		hasItem = false;

		if(photonView.isMine)
		{
			GetComponent<MeshRenderer>().material.color = new Color(8 / 255f, 168 / 255f, 241 / 255f, 1);
		}
	}

	void Update()
	{
		if(!photonView.isMine) return;

		// Player interaction
		hasItem = (heldItem == null) ? false : true;

		if(Input.GetButtonDown("Interact"))
		{
			if(hasItem != false)
			{
				Debug.Log("Interacting with our heldItem");
				heldItem.SendMessage("Interact", photonView.viewID, SendMessageOptions.RequireReceiver);
			}
			else
			{
				Collider closest;
				if(FindClosestInteract(out closest))
				{
					Debug.Log("Attempting to interact with: " + closest.name);
					closest.gameObject.SendMessage("Interact", photonView.viewID, SendMessageOptions.RequireReceiver);
				}
				else
				{
					Debug.Log("No Interact object within playerReach");
				}
			}
		}

		// Player movement
		var x = Input.GetAxis("Horizontal") * Time.deltaTime * playerSpeed;
		var z = Input.GetAxis("Vertical") * Time.deltaTime * playerSpeed;

		if(Input.GetMouseButton(0) && Input.GetMouseButton(1))
		{
			if(z == 0) z = 1 * Time.deltaTime * 3.0f;
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, mainCamera.transform.rotation.eulerAngles.y, 0f), Time.deltaTime * rotationSlerpSpeed);
		}
		else if(!Input.GetMouseButton(0) && (x != 0f || z != 0f))
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, mainCamera.transform.rotation.eulerAngles.y, 0f), Time.deltaTime * rotationSlerpSpeed);
		}

		rb.MovePosition(transform.position + transform.rotation * new Vector3(x, 0.0f, z));
		if(Input.GetButtonDown("Jump")) rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
	}

	void LateUpdate()
	{
		playerCanvas.rotation = mainCamera.transform.rotation;

		// Handle player fade when camera moves between 1st and 3rd person views
		// if camera is within 1st-person view distance
		if(cameraController.curDistance < 2)
		{
			// if zooming to 1st person
			if(cameraController.distance <= cameraController.curDistance)
			{
				cameraController.distance = Mathf.Clamp(cameraController.distance - fadeRate * 3, 0, cameraController.distanceMax);
				playerFade(-1.0f);
				// else if zooming from 1st person
			}
			else
			{
				cameraController.distance += fadeRate;
				if(cameraController.curDistance > 1f)
				{
					playerFade(fadeRate);
				}
			}
			// if camera is within 3rd-person view distance
		}
		else if(cameraController.curDistance > 2.5f)
		{
			playerFade(1);
			// if camera is "between" optimal view distances, correct alpha if necesary
		}
		else if(this.GetComponent<MeshRenderer>().material.color.a != 1)
		{
			cameraController.distance += fadeRate;
			playerFade(fadeRate);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if(!localPlayer) return;
		if(other.CompareTag("Respawn Shield")) Respawn();
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if(stream.isWriting)
		{
			stream.SendNext(hasItem);
		}
		else
		{
			hasItem = (bool)stream.ReceiveNext();
		}
	}

	public void Respawn()
	{
		if(!localPlayer) return;

		// Default spawn point
		Vector3 spawnPoint = new Vector3(0, 3, 0);

		// If array of spawn points exists, choose a random one
		SpawnPoint[] spawnPoints = gameManager.spawnPoints;
		if(spawnPoints != null && spawnPoints.Length > 0)
		{
			spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
		}

		Debug.Log("Spawn Point chosen: " + spawnPoint);
		transform.position = spawnPoint;
	}

	public void playerFade(float alphaValue)
	{
		if(localPlayer)
		{
			MeshRenderer[] renderers = localPlayer.GetComponentsInChildren<MeshRenderer>();
			for(int i = 0; i < renderers.Length; i++)
			{
				Color color = renderers[i].material.color;
				color.a = Mathf.Clamp(color.a + alphaValue, 0, 1);
				renderers[i].material.color = color;
			}
		}
	}

	public bool FindClosestInteract(out Collider closest)
	{
		Collider closestFound = null;
		float distance = playerReach * playerReach;
		Collider[] colliders = Physics.OverlapSphere(transform.position, playerReach, 1 << LayerMask.NameToLayer("Interact"));
		foreach(Collider collider in colliders)
		{
			float sqrMagnitude = (collider.transform.position - transform.position).sqrMagnitude;
			if(sqrMagnitude < distance)
			{
				closestFound = collider;
				distance = sqrMagnitude;
			}
		};
		closest = closestFound;
		return closestFound != null;
	}

	public void TransferObjectOwnership(PhotonPlayer requestingPlayer)
	{
		photonView.TransferOwnership(requestingPlayer.ID);
	}
}