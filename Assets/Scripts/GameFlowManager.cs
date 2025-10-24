using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
//�܂�������ł��Ă��Ȃ�
public enum GameState { Title, SelectPlayers, Join, Ready, Playing }

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance;
    public GameState CurrentState { get; private set; }

    [SerializeField] private PlayerInputManager _playerInputManager;

    private int targetPlayerCount = 1;
    private List<PlayerData> players = new();

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ChangeState(GameState.Title);
        if (_playerInputManager != null)
        {
            Debug.Log("PlayerInputManager found!");
            Debug.Log($"Default action map: {_playerInputManager.playerPrefab.GetComponent<PlayerInput>().defaultActionMap}");
            Debug.Log($"Input Action Asset: {_playerInputManager.joinAction}");

            _playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
            //_playerInputManager.maxPlayerCount = 4;

            _playerInputManager.onPlayerJoined -= HandlePlayerJoined;
            _playerInputManager.onPlayerJoined += HandlePlayerJoined;
        }
        else
        {
            Debug.LogError("PlayerInputManager not assigned!");
        }
        //�C�x���g�w�ǂ��R�[�h�Ő���
        //await TestJoinAsync();

    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log("Game State: " + newState);
    }

    // === �l���I�� ===
    //private async UniTask TestJoinAsync()
    //{
    //    await UniTask.Delay(1000); // 1�b�ҋ@
    //    ChangeState(GameState.Join);

    //    if (_playerInputManager != null)
    //    {
    //        Debug.Log("Manual Join triggered!");
    //        _playerInputManager.JoinPlayer();
    //    }
    //}
    public void SetTargetPlayers(int count)
    {
        targetPlayerCount = Mathf.Clamp(count, 1, 4);
        players.Clear();

        ChangeState(GameState.Join);
    }

    // === �Q������ ===

    public async void HandlePlayerJoined(PlayerInput playerInput)
    {
        if (CurrentState != GameState.Join) return;

        if (players.Count >= targetPlayerCount)
        {
            Destroy(playerInput.gameObject);
            return;
        }

        int playerIndex = players.Count + 1;
        PlayerData newPlayer = new PlayerData(playerIndex, playerInput);

        Debug.Log($"Player {playerIndex} joined! ({players.Count + 1}/{targetPlayerCount})");

        // �����X�C�b�`��OFF�i�s�v�Ȑ֖ؑh�~�j
        playerInput.neverAutoSwitchControlSchemes = true;

        // Join����̃f�o�C�X�F���҂�
        await UniTask.WaitUntil(() => playerInput.devices.Count > 0);
        var device = playerInput.devices.FirstOrDefault();
        Debug.Log($"Player {playerIndex} device: {device?.displayName ?? "Unknown"}");

        // === Control Scheme���������蓖�� ===
        var scheme = playerInput.actions.controlSchemes
           .FirstOrDefault(s => playerInput.devices.Any(d => s.SupportsDevice(d)));

        if (!string.IsNullOrEmpty(scheme.name))
        {
            playerInput.SwitchCurrentControlScheme(scheme.name, playerInput.devices.ToArray());
            Debug.Log($"[Player {playerIndex}] Control Scheme auto-detected: {scheme.name}");
        }
        else
        {
            Debug.LogWarning($"[Player {playerIndex}] No matching control scheme found!");
        }

        // === ActionMap�� Player �ɐ؂�ւ� ===
        if (playerInput.currentActionMap == null || playerInput.currentActionMap.name != "Player")
        {
            playerInput.SwitchCurrentActionMap("Player");
            Debug.Log($"Player {playerIndex} switched to 'Player' map");
        }

        // === PlayerController�������� ===
        var controller = playerInput.GetComponent<PlayerController>();
        if (controller != null)
        {
            await controller.SetupInputActionsAsync(playerInput);
            Debug.Log($"Player {playerIndex} input ready");
        }

        players.Add(newPlayer);

        // === �S���Q���ς݂Ȃ� Ready�� ===
        if (players.Count == targetPlayerCount)
        {
            ChangeState(GameState.Ready);
        }
    }

    // === Ready���� ===
    public void SetPlayerReady(int playerIndex)
    {
        if (CurrentState != GameState.Ready) return;

        var player = players.Find(p => p.PlayerIndex == playerIndex);
        if (player != null) player.IsReady = true;

        if (players.TrueForAll(p => p.IsReady))
        {
            AllReady();
        }
    }

    private void AllReady()
    {
        ChangeState(GameState.Playing);
        Debug.Log("All players ready! Game Start!");
        GameManager.Instance.StartGame(); // �� �����Ŗ{�ҊJ�n
    }
}

[System.Serializable]
public class PlayerData
{
    public int PlayerIndex { get; private set; }
    public PlayerInput Input { get; private set; }
    public bool IsReady { get; set; }

    public PlayerData(int index, PlayerInput input)
    {
        PlayerIndex = index;
        Input = input;
        IsReady = false;
    }
}
