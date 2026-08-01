using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSoundController : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landClip;

    [Header("Skill")]
    [SerializeField] private AudioClip fireSlashClip;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    public void PlayAttackSound()
    {
        PlayOneShot(attackClip);
    }

    public void PlayHitSound()
    {
        PlayOneShot(hitClip);
    }

    public void PlayJumpSound()
    {
        PlayOneShot(jumpClip);
    }

    public void PlayLandSound()
    {
        PlayOneShot(landClip);
    }

    public void PlayFireSlashSound()
    {
        PlayOneShot(fireSlashClip);
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }
}