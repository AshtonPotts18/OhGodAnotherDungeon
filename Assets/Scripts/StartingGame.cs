using UnityEngine;

public class StartingGame : MonoBehaviour
{


    void Start()
    {
        if(GameObject.FindWithTag("Player") && GameObject.FindWithTag("Player").GetComponent<HealthandPlus>().slider.value == 0)
        {
            GameObject.FindWithTag("Player").GetComponent<HealthandPlus>().slider.value = 100;
        }
        GameManager.Instance.StartGame();
    }

}
