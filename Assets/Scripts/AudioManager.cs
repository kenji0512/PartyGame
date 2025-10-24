using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public enum BGMType
    {
        Title,
        Battle,
        Result
    }
    public static AudioManager Instance { get; private set; }

    [Header("=== 各シーンのBGM設定 ===")]
    public AudioClip titleBGM;
    public AudioClip battleBGM;
    public AudioClip resultBGM;

    /// <summary>
    /// Inspectorで設定されていなければ自動生成される
    /// </summary>
    [Header("=== Audio Sources ===")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    [Header("=== 各シーンごとのBGM音量 ===")]
    [Range(0f, 1f)] public float titleVolume = 1f;
    [Range(0f, 1f)] public float battleVolume = 1f;
    [Range(0f, 1f)] public float resultVolume = 1f;

    public BGMType GetCurrentBGMType() => currentBGMType;

    private float bgmVolume = 1f;
    private float seVolume = 1f;
    private Tween bgmFadeTween;
    private BGMType currentBGMType;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // AudioSource設定
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
        }

        if (seSource == null)
        {
            GameObject seObj = new GameObject("SESource");
            seObj.transform.SetParent(transform);
            seSource = seObj.AddComponent<AudioSource>();
            seSource.playOnAwake = false;
        }

        // PlayerPrefsから音量をロード
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        seVolume = PlayerPrefs.GetFloat("SEVolume", 1f);
    }

    // ===== BGM =====
    public void PlayBGM(BGMType type, float fadeTime = 1f)
    {
        AudioClip clip = null;
        float targetVolume = 1f;

        switch (type)
        {
            case BGMType.Title:
                clip = titleBGM;
                targetVolume = titleVolume;
                break;
            case BGMType.Battle:
                clip = battleBGM;
                targetVolume = battleVolume;
                break;
            case BGMType.Result:
                clip = resultBGM;
                targetVolume = resultVolume;
                break;
        }

        if (clip == null || (bgmSource.clip == clip && bgmSource.isPlaying)) return;

        currentBGMType = type;
        bgmFadeTween?.Kill();

        bgmFadeTween = bgmSource.DOFade(0f, fadeTime).OnComplete(() =>
        {
            bgmSource.clip = clip;
            bgmSource.Play();
            bgmSource.DOFade(targetVolume * bgmVolume, fadeTime);
        });
    }

    public void StopBGM(float fadeTime = 1f)
    {
        bgmFadeTween?.Kill(true);
        bgmFadeTween = bgmSource.DOFade(0f, fadeTime).OnComplete(() =>
        {
            bgmSource.Stop();
            bgmSource.clip = null;
        });
    }

    // ===== SE =====
    public void PlaySE(AudioClip clip)
    {
        if (clip == null) return;
        seSource.PlayOneShot(clip, seVolume);
    }

    // ===== 音量設定 =====
    public void SetBGMVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);
        bgmSource.volume = bgmVolume;
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        PlayerPrefs.Save();
    }

    public void SetSEVolume(float value)
    {
        seVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat("SEVolume", seVolume);
        PlayerPrefs.Save();
    }

    public float GetBGMVolume() => bgmVolume;
    public float GetSEVolume() => seVolume;

    // ===== 現在のBGMタイプの音量更新 =====
    public void UpdateVolume(BGMType type)
    {
        if (type != currentBGMType) return;

        float baseVolume = 1f;
        switch (type)
        {
            case BGMType.Title: baseVolume = titleVolume; break;
            case BGMType.Battle: baseVolume = battleVolume; break;
            case BGMType.Result: baseVolume = resultVolume; break;
        }

        bgmSource.volume = baseVolume * bgmVolume;
    }
}