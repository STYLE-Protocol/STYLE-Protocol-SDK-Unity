using UnityEngine;

public class Loader : MonoBehaviour
{
    public void Show()
    {
        gameObject.transform.localScale = new Vector3(1, 1, 1);
    }

    public void Unshow()
    {
        gameObject.transform.localScale = new Vector3(0, 0, 0);
    }
}