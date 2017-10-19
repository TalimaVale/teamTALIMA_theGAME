using UnityEngine;
using System.Collections;

public enum CharacterState
{
	Idle,
	Box
};

public class CrafterControllerFREE : MonoBehaviour
{
	private Animator animator;
	private GameObject box;
	float rotationSpeed = 5;
	Vector3 inputVec;
	bool isMoving;
	bool isPaused;
	public CharacterState charState;

	void Awake()
	{
		animator = this.GetComponent<Animator>();
		box = GameObject.Find("Carry");
	}

	void Start()
	{
		StartCoroutine(COShowItem("none", 0f));
		charState = CharacterState.Idle;
	}

	void Update()
	{
		//Get input from controls
		float z = Input.GetAxisRaw("Horizontal");
		float x = -(Input.GetAxisRaw("Vertical"));
		inputVec = new Vector3(x, 0, z);
		animator.SetFloat("VelocityX", -x);
		animator.SetFloat("VelocityY", z);

		//if there is some input
		if(x != 0 || z != 0)
		{  
			//set that character is moving
			animator.SetBool("Moving", true);
			isMoving = true;

			//if we are running, set the animator
			if(Input.GetButton("Jump"))
			{
				animator.SetBool("Running", true);
			}
			else
			{
				animator.SetBool("Running", false);
			}
		}
		else
		{
			//character is not moving
			animator.SetBool("Moving", false);
			isMoving = false;
		}

		//update character position and facing
		UpdateMovement();

		if(Input.GetKey(KeyCode.R))
		{
			this.gameObject.transform.position = new Vector3(0, 0, 0);
		}

		//sent velocity to animator
		animator.SetFloat("Velocity", UpdateMovement());  
	}

	//face character along input direction
	void RotateTowardsMovementDir()
	{
		if(!isPaused)
		{
			if(inputVec != Vector3.zero)
			{
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(inputVec), Time.deltaTime * rotationSpeed);
			}
		}
	}
	
	//movement of character
	float UpdateMovement()
	{
		//get movement input from controls
		Vector3 motion = inputVec;  

		//reduce input for diagonal movement
		motion *= (Mathf.Abs(inputVec.x) == 1 && Mathf.Abs(inputVec.z) == 1) ? 0.7f : 1;
		
		if(!isPaused)
		{
			//if not paused, face character along input direction
			RotateTowardsMovementDir();
		}

		return inputVec.magnitude;
	}

	void OnGUI()
	{
		if(charState == CharacterState.Idle && !isMoving)
		{
			isPaused = false;
			if(GUI.Button(new Rect(25, 25, 150, 30), "Pickup Box"))
			{
				animator.SetTrigger("CarryPickupTrigger");
				StartCoroutine(COMovePause(1.2f));
				StartCoroutine(COShowItem("box", .5f));
				charState = CharacterState.Box;
			}
			if(GUI.Button(new Rect(25, 65, 150, 30), "Recieve Box"))
			{
				animator.SetTrigger("CarryRecieveTrigger");
				StartCoroutine(COMovePause(1.2f));
				StartCoroutine(COShowItem("box", .5f));
				charState = CharacterState.Box;
			}
		}
		if(charState == CharacterState.Box && !isMoving)
		{
			if(GUI.Button(new Rect(25, 25, 150, 30), "Put Down Box"))
			{
				animator.SetTrigger("CarryPutdownTrigger");
				StartCoroutine(COMovePause(1.2f));
				StartCoroutine(COShowItem("none", .7f));
				charState = CharacterState.Idle;
			}
			if(GUI.Button(new Rect(25, 65, 150, 30), "Give Box"))
			{
				animator.SetTrigger("CarryHandoffTrigger");
				StartCoroutine(COMovePause(1.2f));
				StartCoroutine(COShowItem("none", .6f));
				charState = CharacterState.Idle;
			}
		}
	}

	public IEnumerator COMovePause(float pauseTime)
	{
		isPaused = true;
		yield return new WaitForSeconds(pauseTime);
		isPaused = false;
	}

	public IEnumerator COChangeCharacterState(float waitTime, CharacterState state)
	{
		yield return new WaitForSeconds(waitTime);
		charState = state;
	}

	public IEnumerator COShowItem(string item, float waittime)
	{
		yield return new WaitForSeconds(waittime);
		
		if(item == "none")
		{
			box.SetActive(false);
		}
		else if(item == "box")
		{
			box.SetActive(true);
		}

		yield return null;
	}
}