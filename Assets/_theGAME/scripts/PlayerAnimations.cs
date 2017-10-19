using UnityEngine;
using Photon;

public class PlayerAnimations : PunBehaviour {

    public bool isLocalPlayer { get { return photonView.isMine; } }
    public PlayerController playerController;

    Animator animator;
    CharacterController controller;

	// Use this for initialization
	void Start () {
        playerController = GetComponent<PlayerController>();

        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }
	
	// Update is called once per frame
	void Update () {
        if (!isLocalPlayer) return;

        var x = Input.GetAxis("Horizontal");
        var z = Input.GetAxis("Vertical");

        if(z > 0) animator.SetBool("isWalkFWD", true);
        else animator.SetBool("isWalkFWD", false);

        if(z < 0) animator.SetBool("isWalkBKD", true);
        else animator.SetBool("isWalkBKD", false);

        if(x > 0) animator.SetBool("isStrafeRIGHT", true);
        else animator.SetBool("isStrafeRIGHT", false);

        if(x < 0) animator.SetBool("isStrafeLEFT", true);
        else animator.SetBool("isStrafeLEFT", false);

        if(playerController.heldItem != null) animator.SetLayerWeight(1, 1);
        else animator.SetLayerWeight(1, 0);

        //if (Input.GetKey(KeyCode.Space) && controller.isGrounded) animator.SetBool("isJumping", true);
        //else animator.SetBool("isJumping", false);
    }
}
