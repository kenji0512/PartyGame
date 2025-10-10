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
        // �X�R�A�������Ƃ��A�^�C�}�[�J�n�Ƃ�
    }

    public void EndGame()
    {
        Debug.Log("Game End!");
        // ���ʏW�v�AGameFlowManager�ɒʒm�Ȃ�
        GameFlowManager.Instance.ChangeState(GameState.Title);
    }
}
