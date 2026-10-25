using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class HealthandPlus : MonoBehaviour, DamagingEnemy.IDamageable
{

    public Slider slider;

    public string GameOver;

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


    private void Update()
    {
        if(slider.value == 0)
        {
            SceneManager.LoadScene(GameOver);
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

    public void TakeDamage(float amount)
    {
        if (slider == null || amount <= 0f)
            return;

        slider.value = Mathf.Max(0f, slider.value - amount);
    }


}
