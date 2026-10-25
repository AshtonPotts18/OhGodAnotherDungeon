using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
public class IfSprint : MonoBehaviour
{


    [SerializeField] private InputActionAsset actionmap;
    private InputAction Sprint;
    public Slider slider;
    public GameObject Stamina;
    [SerializeField] int stamiaDrain = 1;
    private Image Change;

    [SerializeField] private float staminaRegenCooldown = 1.5f;
    private float DelayTime;


    
    private void Awake()
    {
        Sprint = actionmap.FindAction("Sprint");
    }


    public IEnumerator WaitThenDoSomething()
    {
        yield return new WaitForSeconds(1f);
    }

    void Start()
    {
        Change = Stamina.GetComponent<Image>();
        DelayTime = staminaRegenCooldown;
    }
    void FixedUpdate()
    {
        //staminaRegenCooldown = staminaRegenCooldown * Time.deltaTime;
        if(Sprint.IsPressed())
        {
            if (slider.value > 0)
            { slider.value -= stamiaDrain; } // perframe stamina drain
            Change.color = new Color(0f, 1, 1);

            staminaRegenCooldown = DelayTime;
        }
        else
        {
            Change.color = new Color(0f, 0.4734296f, 0.7169812f);
            if (staminaRegenCooldown > 0) // 1.5 second cooldown
            {
                staminaRegenCooldown -= Time.deltaTime;
            }
            else if (slider.value < slider.maxValue)
            {
                slider.value += 1;
            }
        }

        
    }
}
