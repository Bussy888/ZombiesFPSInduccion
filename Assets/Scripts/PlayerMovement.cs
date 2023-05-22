using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController characterController;
    public float speed;
    public float walkSpeed= 15f;
    public float runSpeed = 25f;

    private Vector3 velocity;
    public float gravity = -9.81f;

    public bool isGrounded;
    public Transform groundCheck;
    public float groundDistance = 0.1f;
    public LayerMask groundLayerMask;

    public float jumpHeight = 10f;
    public float fuerzaExterna;
    public PhotonView photonView;
  

    // Update is called once per frame
    void Update()
    {
        if(PhotonNetwork.InRoom && !photonView.IsMine)
        {
            return;
        }
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayerMask);

        if(isGrounded && velocity.y < 0 )
        {   
            
            velocity.y = -fuerzaExterna;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward*z;

        characterController.Move(move*speed*Time.deltaTime);

        velocity.y +=gravity *Time.deltaTime;

        characterController.Move(velocity*Time.deltaTime);

        if(Input.GetButtonDown("Jump")&& isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight *-2*gravity);
        }

        if(Input.GetButton("Fire3")&&isGrounded)
        {
            speed = runSpeed;
        }
        else
        {
            speed = walkSpeed;
        }
    }
}
