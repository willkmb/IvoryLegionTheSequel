using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public Vector2 moveInput;

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}
