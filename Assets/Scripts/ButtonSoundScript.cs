using UnityEngine;

public class ButtonSoundScript : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;

    public void PlaySound()
    {
        Debug.Log("PlaySound called", this);

        if (audioSource == null || clip == null)
        {
            Debug.LogWarning("Audio Source or Clip is missing!", this);
            return;
        }

        audioSource.PlayOneShot(clip, volume);
    }
}