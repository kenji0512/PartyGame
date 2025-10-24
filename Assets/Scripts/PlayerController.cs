using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerController;

[RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    public enum PlayerID
    {
        Player1,
        Player2,
        Player3,
        Player4
    }


    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Player Colors")]
    [SerializeField]
    private Color[] playerColors = new Color[4]
    {
        Color.blue, Color.red, Color.green, Color.yellow
    };

    private Rigidbody rb;
    private PlayerInput playerInput;
    private Renderer bodyRenderer;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool isGrounded;

    public PlayerID playerID;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        playerInput = GetComponent<PlayerInput>();
        bodyRenderer = GetComponentInChildren<Renderer>(); // モデルを持つ子オブジェクトを想定
        playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
        SetupInputActionsAsync(playerInput).Forget();
    }
    private void Start()
    {
        // 入力設定を非同期で初期化
        //await SetupInputActionsAsync(playerInput);

        // プレイヤーIndexをもとにPlayerIDを設定
        int index = playerInput.playerIndex;
        playerID = (PlayerID)Mathf.Clamp(index, 0, 3);

        // 色設定（Enum or IndexベースどちらでもOK）
        ApplyColor();

        // スポーン位置設定
        var spawn = GameManager.Instance?.GetSpawnPoint(index);
        if (spawn != null)
        {
            transform.position = spawn.position;
            transform.rotation = spawn.rotation;
        }
    }
    public async UniTask SetupInputActionsAsync(PlayerInput input)
    {
        await UniTask.WaitUntil(() => input.currentActionMap != null);
        //var moveAction = map.FindAction("Move", false);
        //var jumpAction = map.FindAction("Jump", false);

        // Playerマップに切り替え
        if (input.actions.FindActionMap("Player", false) != null)
        {
            input.SwitchCurrentActionMap("Player");
            Debug.Log($"[{input.name}] ActionMapをPlayerに切り替えました！");
        }
        else
        {
            Debug.LogError("ActionMap 'Player' が存在しません。InputActionsを確認してください。");
            return;
        }
        var map = input.currentActionMap;

        var moveAction = map.FindAction("Move", false);
        var jumpAction = map.FindAction("Jump", false);

        if (moveAction == null || jumpAction == null)
        {
            Debug.LogError($"[{map.name}] に Move / Jump が存在しません！");
            return;
        }
        map.FindAction("Move", true).performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        map.FindAction("Move", true).canceled += _ => moveInput = Vector2.zero;

        map.FindAction("Jump", true).performed += _ => jumpPressed = true;
        Debug.Log($"{input.name} input ready");
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
        CheckGrounded();
    }

    private void HandleMovement()
    {
        if (moveInput.sqrMagnitude < 0.01f) return;
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        Transform cam = Camera.main.transform;
        Vector3 forward = cam.forward; forward.y = 0; forward.Normalize();
        Vector3 right = cam.right; right.y = 0; right.Normalize();
        Vector3 moveDir = forward * move.z + right * move.x;

        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(moveDir), Time.fixedDeltaTime * turnSpeed));
    }

    private void HandleJump()
    {
        if (jumpPressed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpPressed = false;
        }
    }

    private void CheckGrounded()
    {
        if (groundCheck != null)
            isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundLayer);
        else
            isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
    }
    void ApplyColor()
    {
        int index = (int)playerID;
        Color color = playerColors[index % playerColors.Length];
        if (bodyRenderer != null)
            bodyRenderer.material.color = color;
    }
}
