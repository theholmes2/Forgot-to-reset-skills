using UnityEngine;

public class GameSoundController : MonoBehaviour
{
    public static GameSoundController Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;

    [Header("Support Character")]
    [SerializeField] private AudioClip supportAppearClip;
    [SerializeField] private AudioClip supportAttackClip;

    [Header("Stage")]
    [SerializeField] private AudioClip stageClearClip;
    [SerializeField] private AudioClip rewardClip;
    [SerializeField] private AudioClip portalOpenClip;
    [SerializeField] private AudioClip portalEnterClip;

    [Header("UI")]
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip skillEquipClip;
    [SerializeField] private AudioClip nodeUnlockClip;
    [SerializeField] private AudioClip unlockDeniedClip;
    [SerializeField] private AudioClip tabSwitchClip;

    [Header("BGM")]
    [SerializeField] private AudioClip stageBgm;
    [SerializeField] private AudioClip bossBgm;
    [SerializeField] private bool playStageBgmOnStart;

    private void Awake()
    {
        Instance = this;

        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        if (bgmSource != null)
        {
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
        }

        if (sfxSource != null)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }
    }

    private void Start()
    {
        if (playStageBgmOnStart)
            PlayStageBgm();
    }

    public void PlaySupportAppear()
    {
        PlaySfx(supportAppearClip);
    }

    public void PlaySupportAttack()
    {
        PlaySfx(supportAttackClip);
    }

    public void PlayStageClear()
    {
        PlaySfx(stageClearClip);
    }

    public void PlayReward()
    {
        PlaySfx(rewardClip);
    }

    public void PlayPortalOpen()
    {
        PlaySfx(portalOpenClip);
    }

    public void PlayPortalEnter()
    {
        PlaySfx(portalEnterClip);
    }

    public void PlayButtonClick()
    {
        PlaySfx(buttonClickClip);
    }

    public void PlayPersistentButtonClick()
    {
        PlayPersistentSfx(buttonClickClip, GetSfxVolume());
    }

    public void PlaySkillEquip()
    {
        PlaySfx(skillEquipClip);
    }

    public void PlayNodeUnlock()
    {
        PlaySfx(nodeUnlockClip);
    }

    public void PlayUnlockDenied()
    {
        PlaySfx(unlockDeniedClip);
    }

    public void PlayTabSwitch()
    {
        PlaySfx(tabSwitchClip);
    }

    public void PlayStageBgm()
    {
        PlayBgm(stageBgm);
    }

    public void PlayBossBgm()
    {
        PlayBgm(bossBgm);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null)
            return;

        sfxSource.PlayOneShot(clip);
    }

    private void PlayBgm(AudioClip clip)
    {
        if (bgmSource == null || clip == null)
            return;

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    private float GetSfxVolume()
    {
        if (sfxSource == null)
            return 1f;

        return sfxSource.volume;
    }

    private static void PlayPersistentSfx(AudioClip clip, float volume)
    {
        if (clip == null)
            return;

        GameObject soundObject = new GameObject("PersistentButtonSound");
        DontDestroyOnLoad(soundObject);

        AudioSource source = soundObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.Play();

        Destroy(soundObject, clip.length);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
