using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class MPPlayerMovement : NetworkBehaviour
{
    public float movementSpeed = 5f;
    public float rotationSpeed = 150f;
    public Transform camT;
    CharacterController mpCharController;
    Rigidbody rb;
    [SerializeField] float jumpForce;

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
        if(Input.GetKeyDown(KeyCode.Space)) 
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        if (IsOwner)
        {
            MPMovePlayer();
        }


    }

    void MPMovePlayer()
    {
        transform.Rotate(0, Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime, 0);
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        mpCharController.SimpleMove(forward * movementSpeed * Input.GetAxis("Vertical"));

    }
}
