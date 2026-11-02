using UnityEngine;

public class Shop : MonoBehaviour
{
    public static Shop Instance { get; private set; }

    //Qouta
    [SerializeField] private int quotaTarget = 100;
    [SerializeField] private int quotaProgress = 0;

    //Party Savings
    [SerializeField] private int partyMoney = 0;

    public int QuotaTarget => quotaTarget;
    public int QuotaProgress => quotaProgress;
    public int PartyMoney => partyMoney;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool SellItems(Item[] items)
    {
        if (items == null || items.Length == 0)
        {
            return false;
        }

        int totalValue = 0;

        foreach (Item item in items)
        {
            if (item == null)
            {
                return false;
            }

            totalValue += Mathf.Max(0, item.Value);
        }

        int remainingQuota = Mathf.Max(0, quotaTarget - quotaProgress);

        int moneyForQuota = Mathf.Min(totalValue, remainingQuota);

        int extraMoney = totalValue - moneyForQuota;

        quotaProgress += moneyForQuota;
        partyMoney += extraMoney;

        foreach (Item item in items)
        {
            Destroy(item.gameObject);
        }

        Debug.Log($"Sold items for ${totalValue}");
        Debug.Log($"Quota: ${quotaProgress} / ${quotaTarget}");
        Debug.Log($"Spendable Money: ${partyMoney}");

        if (quotaProgress >= quotaTarget)
        {
            Debug.Log("Quota completed!");
        }

        return true;
    }

    public bool BuyItem(int cost)
    {
        if (cost < 0 || partyMoney < 0 || cost > partyMoney)
        {
            return false;
        }
        else if (partyMoney >= cost)
        {
            partyMoney -= cost;
        }

        Debug.Log($"Purchased item for ${cost}");
        Debug.Log($"Remaining Money: ${partyMoney}");

        return true;
    }
}
