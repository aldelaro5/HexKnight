using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
  [SerializeField] private UIPage currentPage;
  [SerializeField] private Image fader;
  [SerializeField] private float fadeTimeSeconds = 2;

  readonly Color blackTransparent = new Color(0, 0, 0, 0);
  readonly Color blackOpaque = new Color(0, 0, 0, 1);

  public bool Fading { get => fading; }
  private bool fading = false;

  private EventSystem evtSystem;

  private void Awake()
  {
    evtSystem = EventSystem.current;
  }

  public IEnumerator FadeOut(bool instant)
  {
    fading = true;
    fader.color = blackTransparent;
    if (!instant)
    {
      Color faderColor = blackTransparent;
      float ellapsedTimeSeconds = 0;
      while (ellapsedTimeSeconds < fadeTimeSeconds)
      {
        yield return null;
        ellapsedTimeSeconds += Time.deltaTime;
        faderColor.a = Mathf.Lerp(0, 1, ellapsedTimeSeconds / fadeTimeSeconds);
        fader.color = faderColor;
      }
    }
    fader.color = blackOpaque;
    fading = false;
    yield break;
  }

  public IEnumerator FadeIn(bool instant)
  {
    fading = true;
    fader.color = blackOpaque;
    if (!instant)
    {
      Color faderColor = blackOpaque;
      float ellapsedTimeSeconds = 0;
      while (ellapsedTimeSeconds < fadeTimeSeconds)
      {
        yield return null;
        ellapsedTimeSeconds += Time.deltaTime;
        faderColor.a = Mathf.Lerp(1, 0, ellapsedTimeSeconds / fadeTimeSeconds);
        fader.color = faderColor;
      }
    }
    fader.color = blackTransparent;
    fading = false;
    yield break;
  }

  public void ChangePage(UIPage newPage)
  {
    if (currentPage != null)
      currentPage.gameObject.SetActive(false);

    evtSystem.SetSelectedGameObject(null);
    currentPage = newPage;

    if (newPage != null)
    {
      newPage.gameObject.SetActive(true);
      evtSystem.SetSelectedGameObject(newPage.defaultSelectedObject);
    }
  }
}
