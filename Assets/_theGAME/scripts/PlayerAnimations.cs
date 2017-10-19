using UnityEngine;
using Photon;

public class PlayerAnimations : PunBehaviour {

    public bool isLocalPlayer { get { return photonView.isMine; } }
    Animator animator;

	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
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
    }
}
