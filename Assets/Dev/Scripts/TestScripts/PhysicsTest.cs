using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsTest : MonoBehaviour
{
    private CharacterController characterController;
    private Rigidbody rigidbody;
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(characterController)
            Debug.Log(characterController.isGrounded);
        if(rigidbody)
            Debug.Log(rigidbody.velocity);
    }
}
