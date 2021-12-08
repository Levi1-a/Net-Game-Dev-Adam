using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class MP_DestroyObjOnTouch : NetworkBehaviour
{
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(this.gameObject);
        }
    }
}
