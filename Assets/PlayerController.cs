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

    // Dictionary to store each player’s model index on the server
    private static Dictionary<ulong, int> playerModelIndices = new Dictionary<ulong, int>();

    // NetworkVariable to store each player's model index and synchronize it across clients
    private NetworkVariable<int> playerModelIndex = new NetworkVariable<int>(-1);

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
            gameObject.GetComponent<PlayerInput>().enabled = false;
        }

        if (IsOwner)
        {
            transform.position = new Vector3(6.4f, 1f, UnityEngine.Random.Range(-22f, -33f));
            SetPlayerModelServerRpc();
        }

        playerModelIndex.OnValueChanged += (oldIndex, newIndex) => SetPlayerModel(newIndex);

        // For new players, request existing player data from the server
        if (IsClient && !IsOwner)
        {
            RequestExistingPlayersClientRpc();
        }
    }

    [ServerRpc]
    private void SetPlayerModelServerRpc()
    {
        int modelIndex = Convert.ToInt32(OwnerClientId) % (transform.childCount - 2);

        playerModelIndex.Value = modelIndex;
        playerModelIndices[OwnerClientId] = modelIndex;

        SetPlayerModel(modelIndex);
    }

    [ClientRpc]
    private void UpdateExistingPlayersClientRpc(ulong clientId, int modelIndex)
    {
        if (OwnerClientId == clientId)
        {
            SetPlayerModel(modelIndex);
        }
    }

    [ClientRpc]
    private void RequestExistingPlayersClientRpc()
    {
        foreach (var entry in playerModelIndices)
        {
            UpdateExistingPlayersClientRpc(entry.Key, entry.Value);
        }
    }

    private void SetPlayerModel(int modelIndex)
    {
        for (int i = 0; i < transform.childCount - 2; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i == modelIndex);
        }
    }

    private void OnDestroy()
    {
        playerModelIndex.OnValueChanged -= (oldIndex, newIndex) => SetPlayerModel(newIndex);

        if (IsServer)
        {
            playerModelIndices.Remove(OwnerClientId);
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
