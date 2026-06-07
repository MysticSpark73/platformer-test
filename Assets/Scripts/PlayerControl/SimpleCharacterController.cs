using PlayerControl;
using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

public class SimpleCharacterController : MonoBehaviour, IPlayerObject
{
    private readonly int _moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private readonly int _isJumpingAnimHash = Animator.StringToHash("IsJumping");
    private readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");
    private readonly int _currentGaitHash = Animator.StringToHash("CurrentGait");
    private readonly int _strafeDirectionXHash = Animator.StringToHash("StrafeDirectionX");
    private readonly int _strafeDirectionZHash = Animator.StringToHash("StrafeDirectionZ");
    private readonly int _isStrafingHash = Animator.StringToHash("IsStrafing");
    private readonly int _isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int _isStoppedHash = Animator.StringToHash("IsStopped");
    private readonly int _movementInputHeldHash = Animator.StringToHash("MovementInputHeld");
    private readonly int _fallingDurationHash = Animator.StringToHash("FallingDuration");
    
    private const int MaxJumps = 2;
    private const float TerminalVelocity = -20;
    private const float FloatThreshold = 0.01f;

    [Header("Components")]
    [SerializeField] 
    private InputReader _inputReader;
    [SerializeField] 
    private Animator _animator;
    [SerializeField] 
    private CharacterController _controller;
    [SerializeField] 
    private Transform _modelTransform;

    [Header("Movement Settings")]
    [SerializeField] 
    private float _moveSpeed = 5f;
    [SerializeField] 
    private float _jumpForce = 10f;
    [SerializeField] 
    private float _fallGravityMultiplier = 2f;
    [SerializeField] 
    private float _jumpGravityMultiplier = 0.8f;
    [SerializeField] 
    private float _rotationSpeed = 10f;
    [SerializeField] 
    private float _maxSpeed = 7f;

    [Header("Ground Check")]
    [SerializeField] 
    private LayerMask _groundLayerMask;
    [SerializeField] 
    private float _groundedOffset = -0.14f;
    
    [Header("Respawn data")]
    [SerializeField] 
    private Vector3 _initialPosition = new Vector3(0f, 0f, 0f);

    private Vector3 _velocity;
    private bool _isGrounded = true;
    private float _speed2D;
    private Vector3 _moveDirection;
    private int _currentGait;
    private int _jumpCount = 2;
    private float _strafeDirectionX = 0f;
    private float _strafeDirectionZ = 1f;
    private float _fallingDuration;
    private bool _isWalking = false;
    private bool _isStopped = true;
    private bool _movementInputHeld = false;

    private void Start()
    {
        _inputReader.onJumpPerformed += OnJump;
    }

    private void Update()
    {
        CalculateMoveDirection();
        CheckIfStopped();
        FaceMoveDirection();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        Move();
        GroundedCheck();
        UpdateAnimator();
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = _controller.transform.position + Vector3.up * _groundedOffset;
        _isGrounded = Physics.CheckSphere(spherePosition, _controller.radius, _groundLayerMask, QueryTriggerInteraction.Ignore);
        
        if (_isGrounded)
        {
            _fallingDuration = 0;
            _velocity.y = 0;
            _jumpCount = MaxJumps;
            _animator.SetBool(_isJumpingAnimHash, false);
        }
    }

    private void CalculateMoveDirection()
    {
        _moveDirection = new Vector3(_inputReader._moveComposite.x, 0f, 0f);
        _movementInputHeld = _moveDirection.magnitude > FloatThreshold;

        _velocity.x += _moveDirection.x * _moveSpeed;
        _velocity.x = Mathf.Max(Mathf.Min(_velocity.x, _maxSpeed), -_maxSpeed);
        _velocity.z = 0f;

        _speed2D = new Vector3(_velocity.x, 0f, _velocity.z).magnitude;
        _speed2D = Mathf.Round(_speed2D * 1000f) / 1000f;

        CalculateGait();
    }

    private void CheckIfStopped()
    {
        _isStopped = Mathf.Approximately(_moveDirection.magnitude, 0);
        _isWalking = !_isStopped && _isGrounded;
        if (_isStopped)
        {
            _velocity.x = 0;
        }
    }

    private void FaceMoveDirection()
    {
        if (_modelTransform == null)
            return;

        if (_moveDirection.magnitude > FloatThreshold)
        {
            Vector3 faceDirection = new Vector3(_velocity.x, 0f, 0f);
            if (faceDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(faceDirection);
                _modelTransform.rotation = Quaternion.Slerp(_modelTransform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void ApplyGravity()
    {
        if (_isGrounded) return;
        
        _velocity.y += Physics.gravity.y * (_velocity.y > 0
            ? _jumpGravityMultiplier
            : _fallGravityMultiplier) * Time.fixedDeltaTime;

        _velocity.y = Mathf.Max(_velocity.y, TerminalVelocity);

        if (_velocity.y <= 0)
        {
            _fallingDuration += Time.fixedDeltaTime;
        }
    }

    private void Move()
    {
        _controller.Move(_velocity * Time.fixedDeltaTime);
    }

    private void UpdateAnimator()
    {
        _animator.SetFloat(_moveSpeedHash, _speed2D);
        _animator.SetInteger(_currentGaitHash, _currentGait);
        _animator.SetBool(_isGroundedHash, _isGrounded);
        _animator.SetFloat(_strafeDirectionXHash, _strafeDirectionX);
        _animator.SetFloat(_strafeDirectionZHash, _strafeDirectionZ);
        _animator.SetFloat(_isStrafingHash, 0f);
        _animator.SetBool(_isWalkingHash, _isWalking);
        _animator.SetBool(_isStoppedHash, _isStopped);
        _animator.SetBool(_movementInputHeldHash, _movementInputHeld);
        _animator.SetFloat(_fallingDurationHash, _fallingDuration);
    }

    private void CalculateGait()
    {
        if (_speed2D < 0.01f)
        {
            _currentGait = 0; // Idle
        }
        else if (_speed2D < _moveSpeed * 0.5f)
        {
            _currentGait = 1; // Walk
        }
        else
        {
            _currentGait = 2; // Run
        }
    }

    private void OnJump()
    {
        if (_isGrounded || _jumpCount > 0)
        {
            _velocity.y = _jumpForce;
            _animator.SetBool(_isJumpingAnimHash, true);
            _jumpCount--;
        }
    }

    public void KillZoneEntered()
    {
        _controller.enabled = false;
        transform.position = _initialPosition;
        _controller.enabled = true;

        _velocity = Vector3.zero;
        _moveDirection = Vector3.zero;
        _speed2D = 0f;
    }

    private void OnDestroy()
    {
        _inputReader.onJumpPerformed -= OnJump;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawSphere(_controller.transform.position + Vector3.up * _groundedOffset, _controller.radius);
    }
}
