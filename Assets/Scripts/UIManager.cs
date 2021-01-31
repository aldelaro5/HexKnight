using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
  [SerializeField] private UIPage currentPage;

  private EventSystem evtSystem;

  private void Awake()
  { 
    evtSystem = EventSystem.current;
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
