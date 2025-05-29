using TMPro;
using UnityEngine;

public class GameResultUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private TextMeshProUGUI popText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI customerText;
    [SerializeField] private GameObject panel;

    public void Show()
    {
        Debug.Log("GameResultUI.Show() called");
        var tr = SessionRewardTracker.Instance;
        expText.text = tr.sessionExp.ToString();
        popText.text = tr.sessionPopularity.ToString();
        moneyText.text = tr.sessionMoney.ToString();
        customerText.text = tr.sessionCustomerFlow.ToString();
        panel.SetActive(true);
    }

    public void Confirm()
    {
        SessionRewardTracker.Instance.ApplyToPlayerData();
        SessionRewardTracker.Instance.ResetSession();
        panel.SetActive(false);
    }
}
