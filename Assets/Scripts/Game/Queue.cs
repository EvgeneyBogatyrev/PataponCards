using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Newtonsoft.Json.Linq;
using Networking;

// Matchmaking. Waiting players sit under /queue/{hash} in Firebase Realtime Database;
// whoever notices another fresh, unmatched entry claims it by writing "matchedWith" on
// both entries in one atomic multi-location PATCH, then both sides load the Game scene.
public class QueueProcesser : MonoBehaviour
{
    public static QueueProcesser instance = null;

    private const float PollIntervalSeconds = 1.5f;
    private const double StaleEntrySeconds = 30;

    public int hash;
    public GameObject statusText;

    private bool matchmakingActive = true;

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

    // Safety net - AudioController survives scene loads via DontDestroyOnLoad, so if this scene
    // is ever left through a path that doesn't already call StopLoop() above, the loop would
    // otherwise keep playing forever into whatever scene comes next.
    private void OnDestroy()
    {
        AudioController.StopLoop();
    }

    private void Start()
    {
        statusText.GetComponent<TextMeshProUGUI>().text = "Searching for the opponent...";
        hash = UnityEngine.Random.Range(0, 99999);
        InfoSaver.myHash = hash;
        AudioController.PlayLoop("waiting_loop");
        StartCoroutine(FindMatch());
    }

    public void BackButton()
    {
        AudioController.PlaySound("click");
        matchmakingActive = false;
        StartCoroutine(LeaveQueueAndGoBack());
    }

    private IEnumerator LeaveQueueAndGoBack()
    {
        AudioController.StopLoop();
        // Best-effort cleanup - don't let a slow/failed delete strand the player here.
        StartCoroutine(FirebaseDb.Delete("queue/" + hash));
        SceneManager.LoadScene("Lobby");
        yield break;
    }

    private IEnumerator FindMatch()
    {
        yield return FirebaseDb.Put("queue/" + hash, JObject.Parse("{\"ts\":{\".sv\":\"timestamp\"}}"));

        while (matchmakingActive)
        {
            JToken queueSnapshot = null;
            yield return FirebaseDb.Get("queue", token => queueSnapshot = token);

            if (!matchmakingActive)
            {
                yield break;
            }

            if (queueSnapshot is JObject queue)
            {
                JToken mine = queue[hash.ToString()];
                if (mine != null && mine["matchedWith"] != null)
                {
                    InfoSaver.opponentHash = mine["matchedWith"].Value<int>();
                    AudioController.StopLoop();
                    StartCoroutine(FirebaseDb.Delete("queue/" + hash));
                    SceneManager.LoadScene("Game");
                    yield break;
                }

                int opponentHash = FindFreshCandidate(queue);
                if (opponentHash != -1)
                {
                    yield return ClaimMatch(opponentHash);
                    yield break;
                }
            }

            // Heartbeat: keep our own entry fresh so other clients don't treat us as abandoned
            // just because time has passed - only an actual crash/quit should go stale.
            yield return FirebaseDb.Patch("queue/" + hash, JObject.Parse("{\"ts\":{\".sv\":\"timestamp\"}}"));

            statusText.GetComponent<TextMeshProUGUI>().text = "Searching for active games...";
            yield return new WaitForSeconds(PollIntervalSeconds);
        }
    }

    private int FindFreshCandidate(JObject queue)
    {
        double now = FirebaseConfig.NowUnixSeconds();
        string myKey = hash.ToString();
        foreach (JProperty candidate in queue.Properties())
        {
            if (candidate.Name == myKey || candidate.Value["matchedWith"] != null)
            {
                continue;
            }
            double? tsMillis = candidate.Value["ts"]?.Value<double>();
            if (tsMillis == null || now - tsMillis.Value / 1000.0 > StaleEntrySeconds)
            {
                continue;
            }
            return int.Parse(candidate.Name);
        }
        return -1;
    }

    private IEnumerator ClaimMatch(int opponentHash)
    {
        JObject patch = new JObject
        {
            ["queue/" + hash + "/matchedWith"] = opponentHash,
            ["queue/" + opponentHash + "/matchedWith"] = hash
        };
        yield return FirebaseDb.Patch("", patch);

        InfoSaver.opponentHash = opponentHash;
        AudioController.StopLoop();
        StartCoroutine(FirebaseDb.Delete("queue/" + hash));
        SceneManager.LoadScene("Game");
    }
}
