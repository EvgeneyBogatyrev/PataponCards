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

    private void Start()
    {
        // Rejects a non-Latin keystroke the moment it's typed (TMP_InputField's own validation
        // hook - returning '\0' tells it to ignore the character) rather than only catching it
        // later when FirebaseAuth.SignUp re-checks the whole nickname on submit.
        if (nicknameField != null)
        {
            nicknameField.GetComponent<TMP_InputField>().onValidateInput +=
                (string text, int charIndex, char addedChar) => TextValidation.IsLatinChar(addedChar) ? addedChar : '\0';
        }
    }

    public void SignUpButton()
    {
        if (busy)
        {
            return;
        }
        AudioController.PlaySound("click");

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
        AudioController.PlaySound("click");
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
