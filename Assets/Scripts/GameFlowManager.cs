using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
            _playerInputManager.onPlayerJoined -= HandlePlayerJoined;
            _playerInputManager.onPlayerJoined += HandlePlayerJoined;
        }
        else
        {
            Debug.LogError("PlayerInputManager not assigned!");
        }
        //イベント購読をコードで制御
        await TestJoinAsync();

    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log("Game State: " + newState);
    }

    // === 人数選択 ===
    private async UniTask TestJoinAsync()
    {
        await UniTask.Delay(1000); // 1秒待機
        ChangeState(GameState.Join);

        if (_playerInputManager != null)
        {
            Debug.Log("Manual Join triggered!");
            _playerInputManager.JoinPlayer();
        }
    }
    public void SetTargetPlayers(int count)
    {
        targetPlayerCount = Mathf.Clamp(count, 1, 4);
        players.Clear();

        ChangeState(GameState.Join);
    }

    // === 参加処理 ===

    public async void HandlePlayerJoined(PlayerInput playerInput)
    {
        if (CurrentState != GameState.Join) return;

        if (players.Count >= targetPlayerCount)
        {
            // 予定人数を超えた参加は拒否
            Destroy(playerInput.gameObject);
            return;
        }

        int playerIndex = players.Count + 1;
        PlayerData newPlayer = new PlayerData(playerIndex, playerInput);

        Debug.Log($"Player {playerIndex} joined! ({players.Count}/{targetPlayerCount})");

        // デバイスと ControlScheme の安全なセットアップ
        playerInput.neverAutoSwitchControlSchemes = true;

        // PlayerController にイベントハンドラ登録
        await UniTask.WaitUntil(() => playerInput.actions != null);

        var controller = playerInput.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.SetupInputActionsAsync(playerInput).Forget();
        }

        players.Add(newPlayer);

        if (players.Count == targetPlayerCount)
        {
            ChangeState(GameState.Ready);
        }
        Debug.Log("Joined");
    }


    // === Ready処理 ===
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
        GameManager.Instance.StartGame(); // ← ここで本編開始
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
