using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Net;
using System.IO;
using System.ComponentModel;
using TMPro;
using System.Collections;

public class CheckVersionModule : MonoBehaviour
{
    public static string version = "0.0.5";
    public static bool isAndroid = true;

    private static string versionURL = "https://drive.google.com/uc?export=download&id=1ZQlz7Hmznk7C3k86EeZOC-ze4vnhegXc";
    private static string updateLinkAndroid = "https://drive.google.com/file/d/1kLOOJkdYZliRuFSI3tz49I33RoRP044t/view?usp=sharing";
    private static string updateLinkWindows = "https://drive.google.com/file/d/1Li4SWGsv-LirtbKLJ50_n2pc9HNHyQWc/view?usp=sharing";
    private string rootPath;
    public GameObject textbox;
    public GameObject button;
    
    private void Start() {
        button.SetActive(false);
        rootPath = Directory.GetCurrentDirectory();
        textbox.GetComponent<TextMeshProUGUI>().text = "Checking versions...";
        StartCoroutine(CheckVersion());
    }

    public IEnumerator CheckVersion()
    {
        WebClient client = new WebClient();
        //client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
        //client.DownloadFileAsync(new Uri(versionURL), Path.Combine(rootPath, "version.txt"));
        string onlineVersion = client.DownloadString(new Uri(versionURL));
        yield return new WaitForSeconds(2f);
        if (version == onlineVersion)
        {
            textbox.GetComponent<TextMeshProUGUI>().text = "Your version is up-to-date!";
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            textbox.GetComponent<TextMeshProUGUI>().text = "There is a newer version of the game.";
            button.SetActive(true);
        }
        
    }

    private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e)
    {
        try
        {
            string onlineVersion = File.ReadAllText(Path.Combine(rootPath, "version.txt"));
            if (version == onlineVersion)
            {
                textbox.GetComponent<TextMeshProUGUI>().text = "Your version is up-to-date!";
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                textbox.GetComponent<TextMeshProUGUI>().text = "There is a newer version of the game.";
                button.SetActive(true);
            }
        }
        catch (Exception ex)
        {
            textbox.GetComponent<TextMeshProUGUI>().text = $"Error finishing download: {ex}";
        }
    }

    public void Download()
    {
        if (isAndroid)
        {
            Application.OpenURL(updateLinkAndroid);
        }
        else
        {
            Application.OpenURL(updateLinkWindows);
        }
        //Application.Quit();
    }
}