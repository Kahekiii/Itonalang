using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class changeScene : MonoBehaviour
{
    public string SceneName;

    public void ChangeSceneOnClick()
    {
        SceneManager.LoadScene(SceneName);
    }
}
