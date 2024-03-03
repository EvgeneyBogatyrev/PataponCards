using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;


public class QueueProcesser : MonoBehaviour
{
    public class MatchingMessage
    {
        public enum ActionType
        {
            Ping,
            GameSearch,
            GameFound,
        }
        public ActionType action;
        public int time;
        public int firstPlayerId;
        public int secondPlayerId;

        public static ActionType GetActionType(string action)
        {
            //Debug.Log(action);
            switch (action)
            {
                case "ping":
                return ActionType.Ping;
                
                case "game_search":
                return ActionType.GameSearch;

                case "game_found":
                return ActionType.GameFound;

                default:
                return ActionType.Ping;
            }
        }

    }
    public static QueueProcesser instance = null;
    
    private string BASE_URL = "https://docs.google.com/forms/u/0/d/e/1FAIpQLScuVqZ-NWfjUUSJvL22nvHg6ZXwGQzIIIMf4asT45RI2Z6Nog/formResponse";
    private string GOOGLE_API_URL = "https://script.google.com/macros/s/AKfycbwvmhaY4RRxGJtWQ89XumWwyp-NI4RezcL99c-1EO-rqcCjOuggFLEioL6kea1BuNOdnA/exec";

    private const int timeInterval = 60;
    private const int waitTime = 45;

    public int hash;

    public GameObject statusText;

    //public int messageId = 0;
    //public List<MessageFromServer> doneActions = new List<MessageFromServer>();
    //public List<MessageFromServer> messagesFromServer;
    //public float secondsBetweenServerUpdates = 5f;
    
    private void Awake() 
    { 
        // If there is an instance, and it's not me, delete myself.
        if (instance != null && instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            instance = this; 
        } 
    }

    private void Start()
    {
        statusText.GetComponent<TextMeshProUGUI>().text = "Searching for the opponent...";
        hash = UnityEngine.Random.Range(0, 99999);
        InfoSaver.myHash = hash;
        StartCoroutine(ObtainData(host:false));
    }

    private IEnumerator Post(string action, int myKey, int opponentKey=-1, string info="")
    {
        WWWForm form = new WWWForm();
        form.AddField("entry.62423198", "$" + action + "$");
        form.AddField("entry.1699162058", "$" + myKey.ToString() + "$");
        form.AddField("entry.996552422", "$" + opponentKey.ToString() + "$");
        form.AddField("entry.303482790", "$" + info + "$");
        
        using (UnityWebRequest www = UnityWebRequest.Post(BASE_URL, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
        }
        

        yield return new WaitForSeconds(1);  
    }

    public IEnumerator HostTheGame()
    {
        StartCoroutine(Post("game_search", hash));
        for (int i = 0; i < UnityEngine.Random.Range(3, 6); ++i)
        {
            yield return new WaitForSeconds(10f);
            StartCoroutine(ObtainData(host:true));
        }
        yield return new WaitForSeconds(10f);
        StartCoroutine(Post("game_found", hash, hash));
        hash = UnityEngine.Random.Range(0, 99999);
        InfoSaver.myHash = hash;
        StartCoroutine(ObtainData(host:false));
        yield return null;
    }

    
    public bool ProcessMessagesNormal(List<MatchingMessage> messages)
    {
        statusText.GetComponent<TextMeshProUGUI>().text = "Searching for active games...";
        int lastTime = messages[messages.Count - 1].time;

        List<int> potentialOpponents =  new List<int>();

        foreach (MatchingMessage message in messages)
        {
            int thisTime = message.time;
            while (thisTime > lastTime)
            {
                lastTime += 24 * 60 * 60;
            }
            int delta = lastTime - thisTime;
            //Debug.Log("Delta time " + delta.ToString());
            if (delta > timeInterval)
            {
                continue;
            }

            //Debug.Log("Action " + message.action.ToString());

            if (message.action == MatchingMessage.ActionType.GameSearch)
            {
                if (!potentialOpponents.Contains(message.firstPlayerId))
                    potentialOpponents.Add(message.firstPlayerId);
            }

            else if (message.action == MatchingMessage.ActionType.GameFound)
            {
                if (potentialOpponents.Contains(message.firstPlayerId))
                    potentialOpponents.Remove(message.firstPlayerId);

                if (potentialOpponents.Contains(message.secondPlayerId))
                    potentialOpponents.Remove(message.secondPlayerId);
            }
        }

        foreach (int hash_ in potentialOpponents)
        {
            Debug.Log(hash_);
        }

        if (potentialOpponents.Contains(hash))
        {
            potentialOpponents.Remove(hash);
        }

        if (potentialOpponents.Count == 0)
        {
            StartCoroutine(HostTheGame());
            return false;
        }
        else
        {
            int opponentHash = potentialOpponents[UnityEngine.Random.Range(0, potentialOpponents.Count - 1)];
            StartCoroutine(Post("game_found", hash, opponentHash));
            InfoSaver.opponentHash = opponentHash;
            SceneManager.LoadScene("Game");
            return true;
        }
    }

    public bool ProcessMessagesHost(List<MatchingMessage> messages)
    {
        Debug.Log("Host");
        statusText.GetComponent<TextMeshProUGUI>().text = "Hosting the game...";
        int lastTime = messages[messages.Count - 1].time;

        List<int> potentialOpponents =  new List<int>();

        foreach (MatchingMessage message in messages)
        {
            int thisTime = message.time;
            while (thisTime > lastTime)
            {
                lastTime += 24 * 60 * 60;
            }
            int delta = lastTime - thisTime;
            if (delta > timeInterval)
            {
                continue;
            }

            if (message.action == MatchingMessage.ActionType.GameFound)
            {
                Debug.Log("Found game found");
                if (message.firstPlayerId != message.secondPlayerId &&
                (message.firstPlayerId == hash || message.secondPlayerId == hash))
                {
                    int opponentHash = message.firstPlayerId;
                    if (opponentHash == hash)
                    {
                        opponentHash = message.secondPlayerId;
                    }
                    InfoSaver.opponentHash = opponentHash;
                    SceneManager.LoadScene("Game");
                    return true;
                }
            }
            
        }

        return false;

    }
    
    private int DateToSeconds(string date)
    {
        if (date.Split('T').Length == 1)
        {
            return -1;
        }
        string time = date.Split('T')[1].Split('.')[0];
        string[] batch = time.Split(':');
        int hours = Int32.Parse(batch[0]);
        int minutes = Int32.Parse(batch[1]);
        int seconds = Int32.Parse(batch[2]);

        return 60 * 60 * hours + 60 * minutes + seconds;

    }

    public void BackButton()
    {
        StartCoroutine(Post("game_found", hash, hash));
        StartCoroutine(GoBack());
    }

    private IEnumerator GoBack()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Lobby");
    }

    public IEnumerator ObtainData(bool host=false)
    {
        Debug.Log("In obtain " + host.ToString());
        StartCoroutine(Post("ping", 0));
        yield return new WaitForSeconds(1.5f);
        Debug.Log("Start obtaining Match....");
        UnityWebRequest www = UnityWebRequest.Get(GOOGLE_API_URL);
        yield return www.SendWebRequest();
        Debug.Log("Finished obtaining Match!");

        List<MatchingMessage> messagesFromServer = new List<MatchingMessage>();
        MatchingMessage currentMessage = new MatchingMessage();

        string[] batches = www.downloadHandler.text.Split('$');
        int iterIndex = 0;
        int batchIndex = 0;
        int curHash = 0;
        foreach (string s in batches)
        {
            
            if (iterIndex == 0)
            {
                iterIndex = 1;
                if (batchIndex == 0)
                {
                    string[] elems = s.Split('"');
                    try
                    {
                        string timeString = elems[elems.Length - 3];

                        int seconds = DateToSeconds(timeString);
                        if (seconds == -1)
                        {
                            break;
                        }
                        currentMessage.time = seconds;
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            else
            {
                iterIndex = 0;
                if (batchIndex == 0)
                {
                    currentMessage.action = MatchingMessage.GetActionType(s);
                }
                else if (batchIndex == 1)
                {
                    if (currentMessage.action != MatchingMessage.ActionType.Ping)
                    {
                        currentMessage.firstPlayerId = Int32.Parse(s);
                    }
                }
                else if (batchIndex == 2)
                {
                    if (currentMessage.action == MatchingMessage.ActionType.GameFound)
                    {
                        currentMessage.secondPlayerId = Int32.Parse(s);
                    }
                }
                else if (batchIndex == 3)
                {
                    messagesFromServer.Add(currentMessage);
                    currentMessage = new MatchingMessage();
                    batchIndex = -1;
                }
                batchIndex += 1;
            }
        }
        if (!host)
        {
            QueueProcesser.instance.ProcessMessagesNormal(messagesFromServer);
        }
        else
        {
            QueueProcesser.instance.ProcessMessagesHost(messagesFromServer);
        }
    
       
        yield return null;
    }
}