using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private Queue<AudioSource> sourcePool = new Queue<AudioSource>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    public void PlaySFX(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource source = GetPooledSource(position);
        source.clip = clip;
        source.volume = volume;
        source.Play();
        StartCoroutine(ReturnSourceAfterPlay(source));
    }


    private AudioSource GetPooledSource(Vector3 position)
    {
        AudioSource source;
        if(sourcePool.Count > 0)
        {
            source = sourcePool.Dequeue();
        }
        else
        {
            GameObject go = new GameObject("PooledAudioSource");
            go.transform.SetParent(transform);
            source = go.AddComponent<AudioSource>();
        }
        source.transform.position = position;
        return source;
    }
    private IEnumerator ReturnSourceAfterPlay(AudioSource source)
    {
        yield return new WaitForSeconds(source.clip.length + 0.1f);
        source.Stop();
        sourcePool.Enqueue(source);
    }
}
