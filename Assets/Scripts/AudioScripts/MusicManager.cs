using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    [SerializeField] private AudioSource musicSource;

    private AudioClip currentClip;
    private Coroutine currentFade;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if(musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 0.5f)
    {
        if (clip == null) return;
        if (currentClip == clip && musicSource.isPlaying) return;
        if (currentFade != null)
            StopCoroutine(currentFade);

        if (musicSource.isPlaying)
            currentFade = StartCoroutine(FadeOutAndPlayNew(clip, fadeDuration));
        else
            currentFade = StartCoroutine(FadeInNew(clip, fadeDuration));
    }
    public void StopMusic(float fadeDuration = 0.5f)
    {
        if (currentFade != null)
        {
            StopCoroutine(currentFade);
        }
        currentFade = StartCoroutine(FadeOutAndStop(fadeDuration));
    }
    private IEnumerator FadeOutAndPlayNew(AudioClip newClip, float duration)
    {
        yield return FadeOut(duration);
        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();
        yield return FadeIn(duration);
        currentClip = newClip;
        currentFade = null;
    }
    private IEnumerator FadeInNew(AudioClip newClip, float duration)
    {
        musicSource.clip = newClip;
        musicSource.volume = 0f;
        musicSource.Play();
        yield return FadeIn(duration);
        currentClip = newClip;
        currentFade = null;
    }
    private IEnumerator FadeOutAndStop(float duration)
    {
        if (duration > 0f)
            yield return FadeOut(duration);
        else
            musicSource.volume = 0f;

        musicSource.Stop();
        musicSource.volume = 1f;
        currentClip = null;
        currentFade = null;
    }
    private IEnumerator FadeOut(float duration)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        musicSource.volume = 0f;
    }
    private IEnumerator FadeIn(float duration)
    {
        float targetVolume = 1f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }
        musicSource.volume = targetVolume;
    }
}
