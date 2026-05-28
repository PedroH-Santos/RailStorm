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

    public void ShowMenu(IReadOnlyList<JunctionMenuEntry> entries, int coins)
    {
        menuPanel.SetActive(true);
        messageText.text = "";
        RefreshSlots(entries, coins);
    }

    public void UpdateMenu(IReadOnlyList<JunctionMenuEntry> entries, int coins)
    {
        RefreshSlots(entries, coins);
    }

    void RefreshSlots(IReadOnlyList<JunctionMenuEntry> entries, int coins)
    {
        coinsText.text = $"Moedas: {coins}";

        for (int i = 0; i < slotLabels.Length; i++)
            slotLabels[i].gameObject.SetActive(false);

        for (int i = 0; i < entries.Count && i < slotLabels.Length; i++)
        {
            JunctionMenuEntry e = entries[i];
            slotLabels[i].gameObject.SetActive(true);

            slotLabels[i].text = $"[{i + 1}]  {e.DirectionArrow}  {e.DestinationName}  —  {e.UnlockCost}";
        }
    }

    public void HideMenu() => menuPanel.SetActive(false);

    public void ShowInsufficientFunds()
    {
        messageText.text = "Moedas insuficientes!";
        StopAllCoroutines();
        StartCoroutine(ClearAfterDelay(1.5f));
    }

    System.Collections.IEnumerator ClearAfterDelay(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        messageText.text = "";
    }
}