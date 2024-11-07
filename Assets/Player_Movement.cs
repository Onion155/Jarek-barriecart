using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Movement : MonoBehaviour
{
    PlayerInput input;
    InputAction action;

    [SerializeField] float speed = 5;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        input = GetComponent<PlayerInput>();
        action = input.actions.FindAction("Move");
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
         Vector2 direction = action.ReadValue<Vector2>();
        transform.position += new Vector3(direction.y, 0, 0) * speed * Time.deltaTime;
    }
}
