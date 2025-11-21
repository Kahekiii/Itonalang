using UnityEngine;

public class ToggleCatalogUI : MonoBehaviour
{
    [SerializeField] private GameObject scrollView;

    private bool isVisible = false;

    public void ToggleVisibility()
    {
        isVisible = !isVisible;
        scrollView.SetActive(isVisible);
    }
}
