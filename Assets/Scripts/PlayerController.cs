using Cysharp.Threading.Tasks;
using System.Linq;
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
    private Vector2 currentMoveInput;
    private bool _isGrounded = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotation;

        _playerInput = GetComponent<PlayerInput>();
        _playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;

        SetupInputActionsAsync(_playerInput).Forget();
    }

    // === Input Setup ===
    public async UniTask SetupInputActionsAsync(PlayerInput playerInput)
    {
        // currentActionMap がセットされるまで待機
        await UniTask.WaitUntil(() => playerInput.currentActionMap != null);

        var actionMap = playerInput.currentActionMap;

        var moveAction = actionMap.FindAction("Move", true);
        moveAction.performed += ctx => HandleMovement(ctx.ReadValue<Vector2>());
        moveAction.canceled += ctx => HandleMovement(Vector2.zero);

        var jumpAction = actionMap.FindAction("Jump", true);
        jumpAction.performed += HandleJump;

        Debug.Log($"{playerInput.name} input actions ready!");

    }
    private void FixedUpdate()
    {
        HandleMovement(currentMoveInput);
        CheckGrounded();
    }

    // === Movement ===
    private void HandleMovement(Vector2 moveInput)
    {
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        if (move.magnitude < 0.01f) return;

        Transform cam = Camera.main.transform;
        Vector3 forward = cam.forward; forward.y = 0; forward.Normalize();
        Vector3 right = cam.right; right.y = 0; right.Normalize();
        Vector3 moveDir = forward * move.z + right * move.x;

        _rb.MovePosition(_rb.position + moveDir * _moveSpeed * Time.fixedDeltaTime);
        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, Quaternion.LookRotation(moveDir), Time.fixedDeltaTime * _turnSpeed));

        //Vector2 rawInput = _playerInput.actions["Move"].ReadValue<Vector2>();
        //_smoothedMoveInput = Vector2.Lerp(
        //    _smoothedMoveInput,
        //    rawInput,
        //    Time.fixedDeltaTime * _inputSmoothSpeed
        //);

        //// 入力をカメラ基準の方向に変換
        //Vector3 moveDirection = new Vector3(_smoothedMoveInput.x, 0, _smoothedMoveInput.y);

        //if (moveDirection.magnitude > 0.1f)
        //{
        //    Transform cam = Camera.main.transform;

        //    // カメラ基準の前後左右を計算
        //    Vector3 forward = cam.forward;
        //    Vector3 right = cam.right;
        //    forward.y = 0;
        //    right.y = 0;
        //    forward.Normalize();
        //    right.Normalize();

        //    // 入力をワールド座標に変換
        //    Vector3 move = forward * moveDirection.z + right * moveDirection.x;

        //    // 移動処理
        //    _rb.MovePosition(_rb.position + move * _moveSpeed * Time.fixedDeltaTime);

        //    // キャラの向きを移動方向に合わせる
        //    Quaternion targetRotation = Quaternion.LookRotation(move);
        //    _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRotation, Time.fixedDeltaTime * _turnSpeed));
    //}
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
