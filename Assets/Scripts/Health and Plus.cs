using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
public class HealthandPlus : MonoBehaviour
{

    public Slider slider;

    [SerializeField] private string TagName;

    [SerializeField] private float InvincFrames = 0.25f;

    private float clock = 0;


    private void OnCollisionStay(Collision other)
    {
        if(other.gameObject.CompareTag(TagName) && clock <= 0)
        {
            slider.value -= 5;
            clock = InvincFrames;
        }
        else
        {
            clock -= Time.deltaTime;
        }
    }

    public void SetMax(int Max)
    {
        slider.maxValue = Max;
        slider.value = Max;
    }
    public void SetHealth(int health)
    {
        slider.value = health;
    }


}
