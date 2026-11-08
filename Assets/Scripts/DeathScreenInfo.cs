using UnityEngine;
using UnityEngine.UI;

public class DeathScreenInfo : MonoBehaviour
{
    public string PrefabNameDeath = "LastTouched";

    public string PrefabNameMoney = "PlayerMoney";
    [SerializeField] private Text text;
    void Start()
    {
        text.text = "Player Died to:           " + PlayerPrefs.GetString(PrefabNameDeath) + "\n\nPlayer's Money:          " + PlayerPrefs.GetInt(PrefabNameMoney);
    }

    
}
