using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Networking;

// Drives the "Enter Code" scene - a single code input field plus a log/error text, redeeming
// through FirebaseCodes.RedeemCode. Code creation lives entirely outside the game now (see
// Tools/redeem_codes.py), so this only ever reads/consumes a code, never creates one.
public class RedeemCodeController : MonoBehaviour
{
    public GameObject codeField;
    public GameObject logText;

    private bool busy = false;

    private void Update()
    {
        if (!busy && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            RedeemButton();
        }
    }

    public void RedeemButton()
    {
        if (busy || codeField == null)
        {
            return;
        }
        AudioController.PlaySound("click");
        string code = codeField.GetComponent<TMP_InputField>().text;
        SetBusy(true);
        StartCoroutine(FirebaseCodes.RedeemCode(code, OnRedeemResult));
    }

    private void OnRedeemResult(bool success, string message)
    {
        SetBusy(false);
        ShowLog(message);
        if (success)
        {
            codeField.GetComponent<TMP_InputField>().text = "";
            // Same post-victory routing GameController.EndGame/OnEndRound already use - chests
            // just won are opened immediately, same place regardless of how they were earned.
            if (InfoSaver.chests > 0)
            {
                SceneManager.LoadScene("OpenChest");
            }
        }
    }

    public void BackButton()
    {
        AudioController.PlaySound("click");
        SceneManager.LoadScene("MainMenu");
    }

    private void ShowLog(string message)
    {
        if (logText != null)
        {
            logText.GetComponent<TextMeshProUGUI>().text = message ?? "";
        }
    }

    private void SetBusy(bool value)
    {
        busy = value;
    }
}
