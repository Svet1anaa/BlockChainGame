using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;

    private Vector2 moveInput;

    public void OnMove(InputValue input)
    {
        moveInput = input.Get<Vector2>();
    }

    void Update()
    {   
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);

        transform.Translate(movement * speed * Time.deltaTime, Space.World);
    }
}
