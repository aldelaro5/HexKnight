using UnityEngine;

public class UIManager : MonoBehaviour
{
  [SerializeField] private UIPage currentPage;

  public void ChangePage(UIPage newPage)
  {
    currentPage.gameObject.SetActive(false);
    currentPage = newPage;
    newPage.gameObject.SetActive(true);
  }
}
