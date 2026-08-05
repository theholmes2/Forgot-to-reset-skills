using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemySoundController : MonoBehaviour
{
    [SerializeField] private AudioClip hitClip;
 
    [SerializeField] private AudioClip attackClip;

    [SerializeField] private AudioClip deathClip;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    public void PlayHitSound()
    {
        if (hitClip == null)
            return;

        audioSource.PlayOneShot(hitClip);
    }


    public void PlayAttackSound()
    {
        if (attackClip == null)
            return;

        audioSource.PlayOneShot(attackClip);
    }
    public void PlayDeathSound()
    {
        if (deathClip == null)
            return;

        GameObject soundObject = new GameObject("EnemyDeathSound");
        AudioSource deathSource = soundObject.AddComponent<AudioSource>();

        deathSource.clip = deathClip;
        deathSource.volume = audioSource.volume;
        deathSource.spatialBlend = 0f;
        deathSource.Play();

        Destroy(soundObject, deathClip.length);
    }
}