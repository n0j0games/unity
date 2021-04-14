using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text.RegularExpressions;

public class Leaderboard : MonoBehaviour
{

    /// <summary>
    ///
    ///      | |                  | |         | |                       | |
    ///      | |     ___   __ _ __| | ___ _ __| |__   ___   __ _ _ __  _| |
    ///      | |    / _ \/ _` |/ _` |/ _ \ '__| '_ \ / _ \ / _` | '__/ _` |
    ///      | |___|  __/ (_| | (_| |  __/ |  | |_) | (_) | (_| | | | (_| |
    ///      |______\___|\__,_|\__,_|\___|_|  |_.__/ \___/ \__,_|_|  \__,_|           
    ///      
    ///     Leaderboard System using Dreamlo Leaderboard API (dreamlo.com) with unique user-ID's and nicknames.
    ///     By Noah Schütte (noahschuette.de)
    ///     Do whatever you want with this script. Instructions below.
    ///                                                        
    /// </summary>

    string privateURL = "http://dreamlo.com/lb/{PRIVATE-KEY}";
    bool removePreviousScore = true;
    bool useCustomUUID = true;
    bool advancedLog = true;

    /// <summary>
    /// 
    ///     -> FIRST STEP:
    ///     Create your own leaderboard on dreamlo.com and replace '{PRIVATE-KEY}' with your PRIVATE KEY
    ///     
    ///     -> FUNCTIONS
    ///     (Must be called as a new coroutine: "StartCoroutine(function(params))" or inside another coroutine: "yield return function(params)")
    ///         addScore(score) - Add score
    ///         checkScore(score) - Add score if the current score is higher than the local highscore
    ///         removeScore() - Remove user's score
    ///         resetAny() - Reset whole leaderboard
    ///         setNickname() - Set Nickname
    ///         getLeaderboard() - Returns leaderboard as list of tuplets including nicknames/"UNNAMED" and scores
    ///         printLeaderboard() - Returns leaderboard as a string
    ///         
    ///     -> REMOVE PREVIOUS SCORE
    ///         Set 'removePreviousScore' to true so the system replaces the previous score with the new highscore.
    ///         Otherwise there will be more than one entry from the same UUID in the leaderboard
    ///     
    ///     -> NICKNAME RULES:
    ///         To save the nickname call saveNickname(string nickname)
    ///         The function will NOT SAVE nicknames containing "ERROR:" or the following characters:
    ///         '<' '>' '{' '}' '[' ']' '|' '*' '/' '\' to prevent errors.
    /// 
    ///     -> UUID (UNIQUE USER ID):
    ///         You can set "useCustomUUID" to false if the target platform has access to the mac address.
    ///         e.g. WebGL can't request mac addresses so you have to use the custom system.
    ///         
    ///     -> Make sure you don't use any of these PlayerPref Keys: "Highscore","UUID","Username"
    ///      
    /// </summary>

    //Output generated from requests 
    public string output = "";

    public Tuple<string, int>[] listoutput;
    public string outputAsString;

    /// 
    /// ADD SCORES
    ///

    public IEnumerator checkScore(int score)
    {
        int temphighscore = PlayerPrefs.GetInt("Highscore", 0);
        if (temphighscore < score) yield return addScore(score);
    }

    public IEnumerator addScore(int score)
    {
        PlayerPrefs.SetInt("Highscore", score);
        string username = PlayerPrefs.GetString("Username", "UNNAMED");
        if (username.Contains("ERROR:"))
        {
            username = "UNNAMED";
            Debug.LogWarning($"<b>[LEADERBOARD]</b> <color=red>Username 'ERROR:' is not allowed to prevent Database errors</color>");
        }
        string uuid = getUUID();

        //Requesting previous leaderboard entries containing the UUID
        yield return getLeaderboardOf(uuid);

        if (output == "")
        {
            //Saving highscore
            yield return addScoreRequest(uuid, score, username);
        }
        else if (output != "error")
        {
            if (removePreviousScore) yield return removeScoreRequest(uuid);
            yield return addScoreRequest(uuid, score, username);
        }
    }

    IEnumerator addScoreRequest(string uuid, int score, string username)
    {
        string url = privateURL + "/add/" + uuid + "/" + score.ToString() + "/0/" + username;
        yield return sendRequest(url);
    }

    /// 
    /// REMOVE/RESET SCORES
    ///

    public IEnumerator removeScore()
    {
        PlayerPrefs.DeleteKey("Highscore");
        string uuid = getUUID();
        yield return removeScoreRequest(uuid);
    }

    public IEnumerator resetAny()
    {
        PlayerPrefs.DeleteKey("Highscore");
        string url = privateURL + "/clear";
        yield return sendRequest(url);
    }

    IEnumerator removeScoreRequest(string uuid)
    {
        string url = privateURL + "/delete/" + uuid;
        yield return sendRequest(url);
    }

    /// 
    /// GET LEADERBOARD
    ///

    public IEnumerator printLeaderboard()
    {
        outputAsString = "";
        yield return getLeaderboard();
        if (output != "error" && listoutput != null)
        {
            Tuple<string, int>[] list = listoutput;
            for (int i = 0; i < list.Length; i++)
            {
                outputAsString += (i+1).ToString() + ". " + list[i].Item1 + " (" + list[i].Item2.ToString() + "P)\n";
            }
            
            //textobject.text = outputAsString <- Requires Text (UnityEngine.UI) or TMP_Text (TMPro)
        }
    }

    public IEnumerator getLeaderboard()
    {
        string url = privateURL + "/pipe";
        yield return printRequest(url);
        if (output != "error") yield return convertLeaderboard();
    }

    public IEnumerator getPlayerLeaderboard()
    {
        string uuid = getUUID();
        yield return getLeaderboardOf(uuid);
        if (output != "error") yield return convertLeaderboard();
    }

    IEnumerator getLeaderboardOf(string uuid)
    {
        string url = privateURL + "/pipe-get/" + uuid;
        yield return printRequest(url);
    }

    IEnumerator convertLeaderboard()
    {
        string[] splitted = output.Split('\n');
        if (splitted.Length <= 1)
        {
            Debug.Log($"<b>[LEADERBOARD]</b> <color=#fc7303>Warning: Empty Leaderboard</color>");
        }
        else
        {
            listoutput = new Tuple<string, int>[splitted.Length-1];
            for (int i = 0; i < splitted.Length-1; i++)
            {
                string[] temp = splitted[i].Split('|');
                if (temp.Length < 4)
                {
                    Debug.LogWarning($"<b>[LEADERBOARD]</b> <color=red>Error: Invalid Entry:</color> \'" + splitted[i] + "\'");
                    output = "error";
                }
                else
                {
                    listoutput[i] = new Tuple<string, int>(temp[3], Convert.ToInt32(temp[1]));
                }
            }
            if (advancedLog) Debug.Log($"<b>[LEADERBOARD]</b> <color=green>Converted Leaderboard</color>");
        }
        yield return null;
    }

    /// 
    /// HTTP REQUESTS
    /// 

    //Sending HTTP-request Successful answer will be 'OK'
    IEnumerator sendRequest(string uri)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(uri);
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            Debug.LogWarning($"<b>[LEADERBOARD]</b> <color=red>Error: Could not connect to Leaderboard</color>");
        }
        else if (uwr.downloadHandler.text == "OK")
        {
            if (advancedLog) Debug.Log($"<b>[LEADERBOARD]</b> <color=green>Request successful</color>");
        }
        else
        {
            //Dreamlo will sometimes return HTML pages with error messages.
            Debug.LogWarning($"<b>[LEADERBOARD]</b> <color=red>Error: Recieved \'</color>" + uwr.downloadHandler.text + "\'");
            output = "error";
        }
    }

    //Sending HTTP-requests and returning their answer
    IEnumerator printRequest(string uri)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(uri);     
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            output = "error";
            Debug.LogWarning($"<b>[LEADERBOARD]</b> <color=red>Error: Could not connect to Leaderboard</color>");
        }
        else
        {
            output = uwr.downloadHandler.text;
            if (output.Contains("ERROR:"))
            {
                Debug.LogWarning($"<b>[LEADERBOARD]</b> <color=red>Recieved Error: </color> \'" + output + "\'");
                output = "error";
            }
            else
            {
                if (advancedLog) Debug.Log($"<b>[LEADERBOARD]</b> <color=green>Recieved: </color> \'" + output + "\'");
            }           
        }
    }

    ///
    /// SAVE NICKNAME
    /// 

    public IEnumerator saveNickname(string nickname)
    {
        char[] invalids = { '<', '>', '{', '}', ']', '[', '|', '*', '/', '\\' };
        int index = nickname.IndexOfAny(invalids);
        if (index != -1 || (nickname.Contains("ERROR"))) Debug.Log($"<b>[LEADERBOARD]</b> <color=red>Nickname contains invalid characters: </color> \'" + nickname + "\'");
        else
        {
            PlayerPrefs.SetString("Username", nickname);
            if (removePreviousScore)
            {
                //Replace Nickname in Leaderboard (only if removePreviousScore is true)
                string uuid = getUUID();
                int score = PlayerPrefs.GetInt("Highscore", 0);
                yield return removeScoreRequest(uuid);
                yield return addScoreRequest(uuid, score, nickname);
                if (advancedLog) Debug.Log($"<b>[LEADERBOARD]</b> <color=green>Changed Nickname</color>");
            }
        }
    }

    ///
    /// (CUSTOM) UUID-GENERATION
    /// 

    string getUUID()
    {
        if (useCustomUUID) return getCustomUUID();
        else return SystemInfo.deviceUniqueIdentifier;
    }

    string getCustomUUID()
    {
        string uuid = PlayerPrefs.GetString("UUID", "");
        if (uuid == "")
        {
            Debug.Log($"<b>[LEADERBOARD]</b> <color=blue>UUID not found. Generating new one...</color>");
            uuid = uniqueID();
            PlayerPrefs.SetString("UUID", uuid);
        }
        if(advancedLog) Debug.Log($"<b>[LEADERBOARD]</b> <color=green>UUID:</color> \'" + uuid + "\'");
        return uuid;
    } 

    string uniqueID()
    {
        DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        int currentEpochTime = (int)(DateTime.UtcNow - epochStart).TotalSeconds;
        char[] chars = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'm', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        string uid = currentEpochTime.ToString();
        for (int i = 0; i < 9; i++)
        {
            uid += chars[UnityEngine.Random.Range(0, chars.Length)];
        }
        return uid;
    }
}
