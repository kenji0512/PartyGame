using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
