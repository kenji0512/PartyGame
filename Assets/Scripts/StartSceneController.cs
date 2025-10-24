using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using DG.Tweening;

public class StartSceneController : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "MenuScene";
    [SerializeField] private CanvasGroup pressAnyButtonGroup;

    private bool isLoading = false;

    private void Start()
    {
        // テキスト点滅
        if (pressAnyButtonGroup != null)
        {
            pressAnyButtonGroup.alpha = 0;
            pressAnyButtonGroup.DOFade(1, 1f).SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void Update()
    {
        if (isLoading) return;

        // キーボード入力検出
        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            LoadNextScene();
        }

        // ゲームパッド入力検出
        foreach (var pad in Gamepad.all)
        {
            if (pad.startButton.wasPressedThisFrame || pad.buttonSouth.wasPressedThisFrame)
            {
                LoadNextScene();
                break;
            }
        }
    }

    private void LoadNextScene()
    {
        isLoading = true;
        DOTween.KillAll();
        SceneManager.LoadScene(nextSceneName);
    }
}
