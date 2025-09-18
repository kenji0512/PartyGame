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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ChangeState(GameState.Title);
        _playerInputManager.onPlayerJoined += HandlePlayerJoined;
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log("Game State: " + newState);
    }

    // === 人数選択 ===
    public void SetTargetPlayers(int count)
    {
        targetPlayerCount = Mathf.Clamp(count, 1, 4);
        players.Clear();

        // 1P はすでにいる前提
        ChangeState(GameState.Join);
        //_playerInputManager.maxPlayerCount = targetPlayerCount;
    }

    // === 参加処理 ===
    private void HandlePlayerJoined(PlayerInput playerInput)
    {
        if (CurrentState != GameState.Join) return;

        int playerIndex = players.Count + 1;
        players.Add(new PlayerData(playerIndex, playerInput));

        Debug.Log($"Player {playerIndex} joined! ({players.Count}/{targetPlayerCount})");

        if (players.Count == targetPlayerCount)
        {
            ChangeState(GameState.Ready);
        }
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
        // ゲーム開始処理をここに
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
