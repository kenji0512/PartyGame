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
        // �o�^����葽����΃����_���⃋�[�v�őΉ�
        if (spawnPoints.Count == 0) return null;
        return spawnPoints[playerIndex % spawnPoints.Count];
    }
    public void StartGame()
    {
        Debug.Log("Game Start!");
        // �X�R�A�������Ƃ��A�^�C�}�[�J�n�Ƃ�
    }

    public void EndGame()
    {
        Debug.Log("Game End!");
        // ���ʏW�v�AGameFlowManager�ɒʒm�Ȃ�
        GameFlowManager.Instance.ChangeState(GameState.Title);
    }
}
