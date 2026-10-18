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
    private Image Change;

    [SerializeField] private float time = 1.5f;


    
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
    }
    void Update()
    {
        if(Sprint.IsPressed())
        {
            if (slider.value > 0)
            { slider.value -= 1; }
            Change.color = new Color(0f, 1, 1);

            time = 1.5f;
        }
        else
        {
            Change.color = new Color(0f, 0.4734296f, 0.7169812f);
            if (time > 0)
            {
                time -= Time.deltaTime;
            }
            else if (slider.value < 100)
            {
                slider.value += 1;
            }
        }

        
    }
}
