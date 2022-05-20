using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Collider2D _col;
    private PlayerInputActions _playerInputActions;
    private InputAction _horizontalMove;

    [SerializeField] private LayerMask environment;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    
    private void OnEnable()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Enable();

        _horizontalMove = _playerInputActions.Movement.Horizontal;
        
        _playerInputActions.Movement.Jump.started += OnJump;
    }

    private void OnDisable()
    {
        _playerInputActions.Movement.Jump.started -= OnJump;
        
        _playerInputActions.Disable();
    }

    private void FixedUpdate()
    {
        float moveDirection = _horizontalMove.ReadValue<float>();
        if (!CheckDirection(new Vector2(moveDirection, 0)))
        {
            _rb.velocity = new Vector2(_horizontalMove.ReadValue<float>() * moveSpeed, _rb.velocity.y);
        }
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (CheckDirection(Vector2.down)) _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private bool CheckDirection(Vector2 direction)
    {
        var bounds = _col.bounds;
        return Physics2D.BoxCast(bounds.center, bounds.size, 0f, direction, .1f, environment);
    }
}
