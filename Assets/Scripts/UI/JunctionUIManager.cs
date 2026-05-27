using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class JunctionUIManager : MonoBehaviour
{
    public static JunctionUIManager Instance;

    [Header("Painel")]
    public GameObject menuPanel;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI messageText;

    [Header("Slots")]
    public TextMeshProUGUI[] slotLabels;

    void Awake()
    {
        Instance = this;
        menuPanel.SetActive(false);
    }

    public void ShowMenu(IReadOnlyList<SplineEntry> blocked, int coins)
    {
        menuPanel.SetActive(true);
        messageText.text = "";
        RefreshSlots(blocked, coins);
    }

    public void UpdateMenu(IReadOnlyList<SplineEntry> blocked, int coins)
    {
        RefreshSlots(blocked, coins);
    }

    void RefreshSlots(IReadOnlyList<SplineEntry> blocked, int coins)
    {
        coinsText.text = $"Moedas: {coins}";

        for (int i = 0; i < slotLabels.Length; i++)
            slotLabels[i].gameObject.SetActive(false);

        for (int i = 0; i < blocked.Count && i < slotLabels.Length; i++)
        {
            SplineEntry entry = blocked[i];
            slotLabels[i].gameObject.SetActive(true);
            slotLabels[i].text = $"[{i + 1}]  {entry.displayName}  —  {entry.unlockCost} moedas";
        }
    }

    public void HideMenu() => menuPanel.SetActive(false);

    public void ShowInsufficientFunds()
    {
        messageText.text = "Moedas insuficientes!";
        Invoke(nameof(Clear), 1.5f);
    }

    void Clear() => messageText.text = "";
}