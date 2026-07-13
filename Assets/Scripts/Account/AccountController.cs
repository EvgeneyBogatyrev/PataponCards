using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Networking;

// Drives the Account scene: email/password sign-in, plus a Register button that hands off to
// the CreateAccount scene for new players.
public class AccountController : MonoBehaviour
{
    public GameObject emailField;
    public GameObject passwordField;
    public GameObject errorText;

    private bool busy = false;

    public void SignInButton()
    {
        if (busy)
        {
            return;
        }

        string email = emailField.GetComponent<TMP_InputField>().text.Trim();
        string password = passwordField.GetComponent<TMP_InputField>().text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("Please enter an email and password.");
            return;
        }

        SetBusy(true);
        StartCoroutine(FirebaseAuth.SignIn(email, password, OnAuthResult));
    }

    public void RegisterButton()
    {
        SceneManager.LoadScene("CreateAccount");
    }

    // Reuses the same email field as Sign In - type your email, press this instead of Sign In.
    public void ForgotPasswordButton()
    {
        if (busy)
        {
            return;
        }

        string email = emailField.GetComponent<TMP_InputField>().text.Trim();
        if (string.IsNullOrEmpty(email))
        {
            ShowError("Enter your email above, then press Forgot Password.");
            return;
        }

        SetBusy(true);
        StartCoroutine(FirebaseAuth.SendPasswordResetEmail(email, OnPasswordResetResult));
    }

    private void OnPasswordResetResult(bool success, string error)
    {
        SetBusy(false);
        ShowError(success ? "Password reset email sent - check your inbox." : error);
    }

    public void BackButton()
    {
        // Login is mandatory everywhere now - there's no unauthenticated screen left to
        // return to, so backing out just quits the app.
        Application.Quit();
    }

    private void OnAuthResult(bool success, string error)
    {
        SetBusy(false);
        if (!success)
        {
            ShowError(error);
            return;
        }
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
