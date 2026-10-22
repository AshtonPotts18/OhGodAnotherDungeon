using UnityEngine;

public class ObjectDisable : MonoBehaviour
{
    public GameObject[] Objects; // The Objects to appear and disappear

    public void Vanish()
    {
        foreach (GameObject obj in Objects)
        {
            if (obj != null)
            {
                if(obj.activeSelf)
                {
                    obj.SetActive(false);
                }
                else
                {
                    obj.SetActive(true);
                }
            }
        }
    }
}
