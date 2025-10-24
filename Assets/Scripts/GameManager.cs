using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public Transform GetSpawnPoint(int playerIndex)
    {
        // 登録数より多ければランダムやループで対応
        if (spawnPoints.Count == 0) return null;
        return spawnPoints[playerIndex % spawnPoints.Count];
    }
    public void StartGame()
    {
        Debug.Log("Game Start!");
        // スコア初期化とか、タイマー開始とか
    }

    public void EndGame()
    {
        Debug.Log("Game End!");
        // 結果集計、GameFlowManagerに通知など
        GameFlowManager.Instance.ChangeState(GameState.Title);
    }
}
