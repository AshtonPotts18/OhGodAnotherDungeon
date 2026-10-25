using UnityEngine;
using UnityEngine.UI;

public class DeathScreenInfo : MonoBehaviour
{
    [SerializeField] private Text text;
    void Start()
    {
        text.text = "Player Died to: " + PlayerPrefs.GetString("LastTouched");
    }

    
}
