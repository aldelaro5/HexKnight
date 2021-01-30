using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIPage : MonoBehaviour
{
  [SerializeField] private GameObject defaultSelectedObject;
  private EventSystem evtSystem;

  private void Awake()
  { 
    evtSystem = EventSystem.current;
  }

  private void OnEnable()
  {
    // OnEnable is too early for this, so we wait at the end of the frames before selecting
    StartCoroutine(WaitThenSelectDefault());
  }

  private IEnumerator WaitThenSelectDefault()
  {
    yield return new WaitForEndOfFrame();
    evtSystem.SetSelectedGameObject(defaultSelectedObject);
    yield break;
  }
}
