using TMPro;
using UnityEngine;

public class QuotaUI : MonoBehaviour
{
    [SerializeField] private TMP_Text quotaText;

    private void Update()
    {
        if (Shop.Instance == null || quotaText == null) return;

        int collected = Shop.Instance.QuotaProgress + Shop.Instance.PartyMoney;
        int required = Shop.Instance.QuotaTarget;

        quotaText.text = $"QUOTA: ${collected} / ${required}";
    }
}