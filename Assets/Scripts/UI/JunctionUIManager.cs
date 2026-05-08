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

    public void ShowMenu(List<int> blocked, SplineJunction junction, int coins)
    {
        menuPanel.SetActive(true);
        messageText.text = "";
        UpdateMenu(blocked, junction, coins);
    }

    public void UpdateMenu(List<int> blocked, SplineJunction junction, int coins)
    {
        coinsText.text = $"Moedas: {coins}";

        for (int i = 0; i < slotLabels.Length; i++)
            slotLabels[i].gameObject.SetActive(false);

        for (int i = 0; i < blocked.Count && i < 4; i++)
        {
            int splineIndex = blocked[i];
            int cost = junction.GetUnlockCost(splineIndex); // custo individual
            slotLabels[i].gameObject.SetActive(true);
            slotLabels[i].text = $"[{i + 1}]  Caminho {splineIndex}  —  {cost} moedas";
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