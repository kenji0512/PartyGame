using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _turnSpeed = 100f;
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _inputSmoothSpeed = 10f;

    [Header("Ground Check")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundRadius = 0.2f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Input Settings")]
    [SerializeField] private string _jumpActionName = "Jump";


    // === Private ===
    private Rigidbody _rb;
    private PlayerInput _playerInput;
    private Vector2 _smoothedMoveInput = Vector2.zero;
    private bool _isGrounded = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotation;

        _playerInput = GetComponent<PlayerInput>();
        _playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;

        SetupInputActions();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        CheckGrounded();
    }

    // === Input Setup ===
    private void SetupInputActions()
    {
        _playerInput.SwitchCurrentActionMap("Player");

        _playerInput.actions["Jump"].performed += HandleJump;
        if (!_playerInput.actions.Contains(_playerInput.actions[_jumpActionName]))
        {
            Debug.LogError($"Action {_jumpActionName} is not found in PlayerInput.");
        }
    }

    // === Movement ===
    private void HandleMovement()
    {
        Vector2 rawInput = _playerInput.actions["Move"].ReadValue<Vector2>();
        _smoothedMoveInput = Vector2.Lerp(
            _smoothedMoveInput,
            rawInput,
            Time.fixedDeltaTime * _inputSmoothSpeed
        );

        // 入力をカメラ基準の方向に変換
        Vector3 moveDirection = new Vector3(_smoothedMoveInput.x, 0, _smoothedMoveInput.y);

        if (moveDirection.magnitude > 0.1f)
        {
            Transform cam = Camera.main.transform;

            // カメラ基準の前後左右を計算
            Vector3 forward = cam.forward;
            Vector3 right = cam.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            // 入力をワールド座標に変換
            Vector3 move = forward * moveDirection.z + right * moveDirection.x;

            // 移動処理
            _rb.MovePosition(_rb.position + move * _moveSpeed * Time.fixedDeltaTime);

            // キャラの向きを移動方向に合わせる
            Quaternion targetRotation = Quaternion.LookRotation(move);
            _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRotation, Time.fixedDeltaTime * _turnSpeed));
        }
    }

    // === Jump ===
    private void HandleJump(InputAction.CallbackContext context)
    {
        if (context.performed && _isGrounded)
        {
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
    }

    // === Ground Check ===
    private void CheckGrounded()
    {
        if (_groundCheck != null)
        {
            _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundRadius, _groundLayer);
        }
        else
        {
            // 足元チェックオブジェクトが無ければ本体から Raycast
            _isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, _groundLayer);
        }
    }

    // === Debug Gizmos ===
    private void OnDrawGizmosSelected()
    {
        if (_groundCheck != null)
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundRadius);
        }
    }
}
