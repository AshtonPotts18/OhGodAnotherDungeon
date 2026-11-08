using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ShowCaseScene : MonoBehaviour
{
    [SerializeField] private InputActionReference showcase;
    [SerializeField] private string sceneName;

    private void Update()
    {
        LoadShowCase();
    }

    private void LoadShowCase()
    {
        if (showcase == null) return;

        if (sceneName == null) return;

        if(showcase != null && showcase.action.WasPressedThisFrame())
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
