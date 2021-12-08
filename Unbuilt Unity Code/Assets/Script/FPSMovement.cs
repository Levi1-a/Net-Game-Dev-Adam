using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class FPSMovement : NetworkBehaviour
{
    [SerializeField] float speed;
    public Transform camT;
    CharacterController mpCharController;
    Rigidbody rb;
    [SerializeField] float jumpForce;

    [SerializeField] Transform groundChecker;
    [SerializeField] float checkRadius;
    [SerializeField] LayerMask groundLayer;

    [SerializeField] float sprintMultiplier = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        mpCharController = GetComponent<CharacterController>();
        if (IsOwner)
        {
            GetComponent<MeshRenderer>().material.color = Color.red;
        }
        else
        {
            GetComponent<MeshRenderer>().material.color = Color.blue;
        }
        if (!IsOwner)
        {
            camT.GetComponent<Camera>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            MPMovePlayer();
        }


    }

    void MPMovePlayer()
    {
        //WASD movement
        rb = GetComponent<Rigidbody>();
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 moveBy = transform.right * x + transform.forward * z;
        rb.MovePosition(transform.position + moveBy.normalized * speed * Time.deltaTime);

        //Jump with space only if on the ground
        if (Input.GetKeyDown(KeyCode.Space) && IsOnGround())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        //LeftShift to sprint
        float actualSpeed = speed;
        if (Input.GetKey(KeyCode.LeftShift) && IsOnGround())
        {
            actualSpeed *= sprintMultiplier;
        }
        rb.MovePosition(transform.position + moveBy.normalized * actualSpeed * Time.deltaTime);

        //Check if the player is on the ground
        bool IsOnGround()
        {
            Collider[] colliders = Physics.OverlapSphere(groundChecker.position, checkRadius, groundLayer);
            if (colliders.Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }


}
