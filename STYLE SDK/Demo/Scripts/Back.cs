using UnityEngine;
using UnityEngine.SceneManagement;

public class Back : MonoBehaviour
{
    public void OnBack()
    {      
        #if UNITY_WEBGL
        SceneManager.LoadScene("WebLogin");
        #else
        SceneManager.LoadScene("WalletLogin");
        #endif
    }
}