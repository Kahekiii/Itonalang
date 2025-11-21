using UnityEngine;
using UnityEngine.UI;

public class SetActiveController : MonoBehaviour
{
    [SerializeField] private GameObject overlayBackground;
    [SerializeField] private Button button;
    [SerializeField] private Button backgroundButton;

    private void Start()
    {
        if (overlayBackground != null)
            overlayBackground.SetActive(false);

        if (button != null)
            button.onClick.AddListener(OpenMenu);

        if (backgroundButton != null)
            backgroundButton.onClick.AddListener(CloseMenu);
    }

    private void OpenMenu()
    {
        overlayBackground.SetActive(true);
    }

    private void CloseMenu()
    {
        overlayBackground.SetActive(false);
    }
}
