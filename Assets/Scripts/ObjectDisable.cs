using UnityEngine;
using UnityEngine.UI;

public class ObjectDisable : MonoBehaviour
{
    public GameObject[] Objects; // The Objects to appear and disappear

    public void Vanish()
    {


        foreach (GameObject obj in Objects)
        {
            var Sel = obj.GetComponent<Selectable>();
            if (Sel != null)
            {
                Sel.enabled = !Sel.enabled;
            }
            else
            {
                obj.SetActive(!obj.activeSelf);
            }
        }
    }

}
