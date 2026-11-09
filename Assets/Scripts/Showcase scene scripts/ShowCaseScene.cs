using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ShowCaseScene : MonoBehaviour
{
    [SerializeField] private InputActionReference showcase;
    [SerializeField] private string sceneName;

   
    private void OnEnable()
    {
        if (showcase != null)
        {
            showcase.action.performed += OnLoad;
        }
    }

    private void OnDisable()
    {
        if (showcase != null)
        {
            showcase.action.performed -= OnLoad;
        }
    }
   
    private void LoadShowCase()
    {
        if (showcase == null && showcase.action == null) return;

        if(showcase.action.WasPressedThisFrame())
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("Showcase scene name is missing");
                return;
            }

            SceneManager.LoadScene(sceneName);

        }
    }

    private void OnLoad(InputAction.CallbackContext context)
    {
       LoadShowCase();
    }
}
