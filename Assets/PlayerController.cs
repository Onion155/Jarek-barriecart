using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using System;
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


    // Dictionary to store each player’s model index based on their OwnerClientId
    private static Dictionary<ulong, int> playerModels = new Dictionary<ulong, int>();

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

    // NetworkVariable to store each player's model index and synchronize it across clients
    private NetworkVariable<int> playerModelIndex = new NetworkVariable<int>(-1); // Initialize to -1 to ensure it's set

    public override void OnNetworkSpawn()
    {
        if (IsOwner && fpcam != null)
        {
            topcam = Camera.main;
            topcam.enabled = false;
            fpcam.GetComponent<Camera>().enabled = true;
        }
        else
        {
            fpcam.GetComponent<Camera>().enabled = false;
        }

        if (!IsOwner)
        {
            // Disable inputs for non-owner clients
            gameObject.GetComponent<PlayerInput>().enabled = false;
        }

        if (IsOwner)
        {
            transform.position = new Vector3(6.4f, 1f, UnityEngine.Random.Range(-22f, -33f));
            // Request to set the model for this player on the server
            SetPlayerModelServerRpc();
        }

        // Update the player model visibility on this client whenever the model index changes
        playerModelIndex.OnValueChanged += (oldIndex, newIndex) => SetPlayerModel(newIndex);
    }

    [ServerRpc]
    private void SetPlayerModelServerRpc()
    {
        // Calculate which model to show based on the OwnerClientId
        int modelIndex = Convert.ToInt32(OwnerClientId) % (transform.childCount - 1);

        // Set the model index on the server, which will automatically sync to clients
        playerModelIndex.Value = modelIndex;
    }

    private void SetPlayerModel(int modelIndex)
    {
        // Loop through children, enabling only the specified model
        for (int i = 0; i < transform.childCount - 1; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i == modelIndex);
        }
    }

    private void OnDestroy()
    {
        // Clean up the event subscription when the player is destroyed
        playerModelIndex.OnValueChanged -= (oldIndex, newIndex) => SetPlayerModel(newIndex);
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
