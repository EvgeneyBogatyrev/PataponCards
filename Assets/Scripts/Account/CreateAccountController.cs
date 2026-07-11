using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Networking;

// Drives the CreateAccount scene: email/password/nickname sign-up.
public class CreateAccountController : MonoBehaviour
{
    public GameObject emailField;
    public GameObject passwordField;
    public GameObject nicknameField;
    public GameObject errorText;

    private bool busy = false;
    private string pendingNickname;

    public void SignUpButton()
    {
        if (busy)
        {
            return;
        }

        string email = emailField.GetComponent<TMP_InputField>().text.Trim();
        string password = passwordField.GetComponent<TMP_InputField>().text;
        string nickname = nicknameField.GetComponent<TMP_InputField>().text.Trim();

        if (string.IsNullOrEmpty(nickname))
        {
            nickname = "Player";
        }

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("Please enter an email and password.");
            return;
        }

        pendingNickname = nickname;
        SetBusy(true);
        StartCoroutine(FirebaseAuth.SignUp(email, password, nickname, OnAuthResult));
    }

    public void BackButton()
    {
        SceneManager.LoadScene("Account");
    }

    private void OnAuthResult(bool success, string error)
    {
        SetBusy(false);
        if (!success)
        {
            ShowError(error);
            return;
        }
        PlayerProfile.Nickname = pendingNickname;
        AccountFlow.OnLoginSuccess();
    }

    private void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.GetComponent<TextMeshProUGUI>().text = message;
        }
    }

    private void SetBusy(bool value)
    {
        busy = value;
        if (value)
        {
            ShowError("");
        }
    }
}
