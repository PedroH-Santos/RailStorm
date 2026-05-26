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

    public void ShowMenu(List<int> blocked, int coins)
    {
        menuPanel.SetActive(true);
        messageText.text = "";
        RefreshSlots(blocked, coins);
    }

    public void UpdateMenu(List<int> blocked, int unlockedSpline, int coins)
    {
        RefreshSlots(blocked, coins);
    }

    void RefreshSlots(List<int> blocked, int coins)
    {
        coinsText.text = $"Moedas: {coins}";

        for (int i = 0; i < slotLabels.Length; i++)
            slotLabels[i].gameObject.SetActive(false);

        for (int i = 0; i < blocked.Count && i < slotLabels.Length; i++)
        {
            int splineIndex = blocked[i];
            int cost = SplineRuntimeState.Instance.GetUnlockCost(splineIndex);
            string name = SplineRuntimeState.Instance.GetDisplayName(splineIndex);

            slotLabels[i].gameObject.SetActive(true);
            slotLabels[i].text = $"[{i + 1}]  {name}  —  {cost} moedas";
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