using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Collider2D _col;
    private PlayerInputActions _playerInputActions;

    [SerializeField] private GameObject spawn; //spawn object
    [SerializeField] private LayerMask environment; //layer to check with boxcast
    [Space]
    [SerializeField] private float moveSpeed; //horizontal movement speed
    [SerializeField] private float jumpForce; //vertical impulse force for jumping
    private float _velocity;
    
    private float _timePassed;
    public static Queue<Action> RecordedActions;
    
    public struct Action
    {
        public readonly float Time;
        public readonly int ActionType;
        public readonly float Value;

        public Action(float time, int actionType, float value)
        {
            Time = time;
            ActionType = actionType;
            Value = value;
        }
    }
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        
        RecordedActions = new Queue<Action>();
    }
    
    //enable user input and subscribe to events
    private void OnEnable()
    {
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Enable();

        _playerInputActions.Movement.Horizontal.started += OnMove;
        _playerInputActions.Movement.Horizontal.performed += OnMove;
        _playerInputActions.Movement.Horizontal.canceled += OnMove;
        
        _playerInputActions.Movement.Jump.started += OnJump;

        KillOnContact.OnDeath += OnDeath;
    }

    //unsubscribes from events
    private void OnDisable()
    {
        _playerInputActions.Movement.Horizontal.started -= OnMove;
        _playerInputActions.Movement.Horizontal.performed -= OnMove;
        _playerInputActions.Movement.Horizontal.canceled -= OnMove;
        
        _playerInputActions.Movement.Jump.started -= OnJump;
        
        _playerInputActions.Disable();
        
        KillOnContact.OnDeath -= OnDeath;
    }
    
    private void Update()
    {
        _timePassed += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        _rb.velocity = new Vector2(_velocity, _rb.velocity.y);
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        //sets horizontal player movement
        float moveDirection = ctx.ReadValue<float>();
        if (CheckDirection(new Vector2(moveDirection, 0))) return; //will not move player if a wall is in that direction (prevents sticking to walls)

        _velocity = moveDirection * moveSpeed; //sets player velocity
        RecordedActions.Enqueue(new Action(_timePassed, 0, _velocity)); //records new direction and time to be replayed next loop
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (!CheckDirection(Vector2.down)) return; //checks if player is touching the ground before jumping
        
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); //applies vertical force to player
        RecordedActions.Enqueue(new Action(_timePassed, 1, jumpForce)); //records jump force and time to be replayed next loop
    }

    //checks if the player is touching a wall in the specified direction
    //used for ground checks and to prevent sticking to walls
    private bool CheckDirection(Vector2 direction)
    {
        var bounds = _col.bounds;
        return Physics2D.BoxCast(bounds.center, bounds.size, 0f, direction, .1f, environment);
    }

    //executed when the player collides with a lethal object
    private void OnDeath(GameObject player)
    {
        _rb.simulated = false;
        GetComponent<SpriteRenderer>().enabled = false;
        transform.position = spawn.transform.position;
    }
}


