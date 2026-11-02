using UnityEngine;

public class ItemHighLight : MonoBehaviour
{
    [SerializeField] private Light highlight;

    private void Awake()
    {
        Transform lightObject = transform.Find("HighlightLight");

        if(lightObject != null )
        {
            highlight = GetComponentInChildren<Light>(true);
        }

        SetHighLighted(false);
    }

    public void SetHighLighted(bool On)
    {
        if(highlight != null)
        {
            highlight.enabled = On;
        }
    }
}
