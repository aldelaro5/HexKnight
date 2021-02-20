using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
  [SerializeField] private UIPage currentPage;
  [SerializeField] private Image fader;
  [SerializeField] private float fadeTimeSeconds = 2;

  readonly Color blackTransparent = new Color(0, 0, 0, 0);
  readonly Color blackOpaque = new Color(0, 0, 0, 1);

  public UIPage CurrentPage { get => currentPage; }
  public bool Fading { get => fading; }
  private bool fading = false;

  private EventSystem evtSystem;
  private InputSystemUIInputModule uiInputModule;
  private GameManager gameManager;

  private void Awake()
  {
    evtSystem = EventSystem.current;
    gameManager = FindObjectOfType<GameManager>();
    uiInputModule = evtSystem.GetComponent<InputSystemUIInputModule>();
    uiInputModule.point = InputActionReference.Create(gameManager.Inputs.UI.Point);
    uiInputModule.move = InputActionReference.Create(gameManager.Inputs.UI.Navigate);
    uiInputModule.submit = InputActionReference.Create(gameManager.Inputs.UI.Submit);
    uiInputModule.leftClick = InputActionReference.Create(gameManager.Inputs.UI.Click);
    uiInputModule.cancel = InputActionReference.Create(gameManager.Inputs.UI.Cancel);
    uiInputModule.scrollWheel = InputActionReference.Create(gameManager.Inputs.UI.Scroll);
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
