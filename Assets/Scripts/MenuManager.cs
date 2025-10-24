using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string battleSceneName = "BattleScene";
    [SerializeField] private PlayerInputManager inputManager;
    [SerializeField] private GameObject[] playerPanels; // 各プレイヤーUI表示

    private List<PlayerInput> joinedPlayers = new();

    private void Start()
    {
        inputManager.onPlayerJoined += OnPlayerJoined;
        inputManager.onPlayerLeft += OnPlayerLeft;

        // UI初期化
        foreach (var p in playerPanels)
            p.SetActive(false);
    }

    private void OnPlayerJoined(PlayerInput player)
    {
        int index = player.playerIndex;
        joinedPlayers.Add(player);

        // UI更新
        if (index < playerPanels.Length)
            playerPanels[index].SetActive(true);

        Debug.Log($"Player{index + 1} joined!");
    }

    private void OnPlayerLeft(PlayerInput player)
    {
        joinedPlayers.Remove(player);
        Debug.Log($"Player{player.playerIndex + 1} left!");
    }

    private void Update()
    {
        // 全員準備でスタート（例：全員Aボタン or Enter）
        if (joinedPlayers.Count > 0 && AllPlayersReady())
        {
            SceneManager.LoadScene(battleSceneName);
        }
    }

    private bool AllPlayersReady()
    {
        foreach (var player in joinedPlayers)
        {
            if (!player.actions["Join"].triggered) // 例："Join"を準備ボタンに使う
                return false;
        }
        return true;
    }
}
