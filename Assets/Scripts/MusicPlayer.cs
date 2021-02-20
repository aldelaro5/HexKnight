using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
  [SerializeField] bool testLoop = false;

  private AudioSource source;
  private GameManager gameManager;

  private void Awake()
  {
    source = GetComponent<AudioSource>();
    gameManager = FindObjectOfType<GameManager>();
  }

  public IEnumerator PlayLoopableMusic(LoopableAudio music)
  {
    source.clip = music.clip;
    source.volume = gameManager.Settings.musicVolume;
    source.Play();
    if (testLoop && Application.isEditor)
      source.time = music.timeEnd - 5f;

    while (true)
    {
      if (testLoop && Application.isEditor)
      {
        while (source?.time < music.timeStart + 5f)
          yield return null;

        source.time = music.timeEnd - 5f;

        while (source?.time < music.timeEnd)
          yield return null;

        if (source != null)
          source.time = music.timeStart;
      }
      else
      {
        while (source?.time < music.timeEnd)
          yield return null;

        if (source != null)
          source.time = music.timeStart;
      }
    }
  }

  public void StopPlaying()
  {
    source.Stop();
  }
}
