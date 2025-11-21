using UnityEngine;
using UnityEngine.SceneManagement;

public class TermsManager : MonoBehaviour
{
    [SerializeField] private GameObject termsPanel;
    [SerializeField] private string nextSceneName;

    private const string TermsAcceptedKey = "TermsAccepted";

    public void OnGetStartedButton()
    {
        if (PlayerPrefs.GetInt(TermsAcceptedKey, 0) == 1)
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            termsPanel.SetActive(true);
        }
    }

    public void OnAcceptAndContinue()
    {
        PlayerPrefs.SetInt(TermsAcceptedKey, 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(nextSceneName);
    }
}
