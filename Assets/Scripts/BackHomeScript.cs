using UnityEngine;
using UnityEngine.SceneManagement;

public class BackHomeScript : MonoBehaviour
{
    public void BackHome()
    {
        SceneManager.LoadScene("StartScene");
    }
}

