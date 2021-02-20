using UnityEngine;

[CreateAssetMenu(fileName = "LoopableAudio", menuName = "LoopableAudio", order = 15)]
public class LoopableAudio : ScriptableObject
{
  public AudioClip clip;
  public float timeStart;
  public float timeEnd;
}