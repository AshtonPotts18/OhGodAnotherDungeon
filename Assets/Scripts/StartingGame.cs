using UnityEngine;
using System.Collections;
public class StartingGame : MonoBehaviour
{


    void Start()
    {
        
        GameManager.Instance.StartGame();

        new WaitForSeconds(0.1f);
        if (GameObject.FindWithTag("Player") && GameObject.FindWithTag("Player").GetComponent<HealthandPlus>().slider.value == 0)
        {
            GameObject.FindWithTag("Player").GetComponent<HealthandPlus>().slider.value = 100;
        }
    }

}
