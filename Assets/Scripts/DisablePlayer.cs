using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DisablePlayer : MonoBehaviour
{

    public string SceneChangeTo = "SampleScene";

    private PlayerInput Input_;

    [SerializeField] private AudioListener Audio;
    private int limiter = 0; // used to make the player not be activated or deactivated every frame

    private void Start()
    {
        Input_ = GetComponent<PlayerInput>();
    }


    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name == SceneChangeTo && limiter == 0)
        {
            Input_.ActivateInput();
            Audio.enabled = true;
            limiter = 1;
        }
        else if (limiter == 1 && SceneManager.GetActiveScene().name != SceneChangeTo)
        {
            Input_.DeactivateInput();
            Audio.enabled = false;
            limiter = 0;
        }
    }
}
