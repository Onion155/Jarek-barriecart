using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    public float turnSpeed = 180;
    public float tiltSpeed = 180;
    float walkSpeed = 30;
    NetworkVariable<float> forward = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    NetworkVariable<float> turn = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    [SerializeField] private Transform fpcam;    // first person camera
    private Camera topcam;      // top view cam

    // Start is called before the first frame update
    void Start()
    {
        // Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            float forward = Input.GetAxis("Vertical");
            float turn = Input.GetAxis("Horizontal") + Input.GetAxis("Mouse X");
            float tilt = Input.GetAxis("Mouse Y");
            PlayerAcceleration();

            MoveCarServerRpc(forward, turn);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner && fpcam != null)
        {
            topcam = Camera.main;
            topcam.enabled = false;
            fpcam.GetComponent<Camera>().enabled = true;
        }
        else
            fpcam.GetComponent<Camera>().enabled = false;


        if (!IsOwner)
        {
            //If this is not the owner, turn off player inputs
            if (!IsOwner) gameObject.GetComponent<PlayerInput>().enabled = false;
        }

        if (IsOwner)
        {
            transform.position = new Vector3(6.4f, 1f, Random.Range(-22f, -33f));
        }

    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner && fpcam != null && topcam != null)
        {
            fpcam.GetComponent<Camera>().enabled = false;
            topcam.enabled = true;
        }
    }

    void PlayerAcceleration()
    {
        float maxspeed = 70;
        float acceleration = 1.4f;
        if (walkSpeed < maxspeed)
        {
            walkSpeed += acceleration * Time.deltaTime;
        }
    }

    [ServerRpc]
    void MoveCarServerRpc(float forward, float turn)
    {
        transform.Translate(new Vector3(0, 0, forward * walkSpeed * Time.deltaTime));
        transform.Rotate(new Vector3(0, turn * turnSpeed * Time.deltaTime, 0));
    }
}
