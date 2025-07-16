using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public void ExitGame()
    {
        SceneManager.LoadScene("Splash");
    }
}
