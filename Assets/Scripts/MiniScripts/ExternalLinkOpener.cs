using UnityEngine;

public class ExternalLinkOpener : MonoBehaviour
{
    [SerializeField] private string facebookPageUrl = "https://www.facebook.com/YourPageName";

    public void OpenFacebookPage()
    {
        Application.OpenURL(facebookPageUrl);
    }
}
