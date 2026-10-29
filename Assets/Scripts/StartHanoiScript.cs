using UnityEngine;
using UnityEngine.SceneManagement;

public class StartHanoiScript : MonoBehaviour
{
    public void StartHanoi()
    {
        SceneManager.LoadScene("HanoiScene");
    }
}


