﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI ;
using System.Collections;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public TMPro.TextMeshProUGUI ScoreText = null;
    public TMPro.TextMeshProUGUI ScoreOneLineText = null;

    public InputField name;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (ScoreOneLineText != null){
            Debug.Log("Score 2 ");
            ScoreOneLineText.text = "Score: " + DataManager.Score().ToString();
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Score()
    {
        StartCoroutine(GetText("https://dev.steelants.cz/GGJ2021/GeorgeJones/Server/api.php"));
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(DataManager.Level());
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    public void ScoreSubmit()
    {
        WWWForm form = new WWWForm();
        form.AddField("name", name.text);
        form.AddField("score", DataManager.Score().ToString());

        StartCoroutine(PostText("https://dev.steelants.cz/GGJ2021/GeorgeJones/Server/api.php", form));
        DataManager.Level(0);
        SceneManager.LoadScene(0);
    }

    IEnumerator GetText(string uri) {
        UnityWebRequest www = UnityWebRequest.Get(uri);
        yield return www.SendWebRequest();
 
        if(www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        }
        else {
            // Show results as text
            Debug.Log(www.downloadHandler.text);
            ScoreText.text = www.downloadHandler.text;
        }
    }

    IEnumerator PostText(string uri, WWWForm data) {
        UnityWebRequest www = UnityWebRequest.Post(uri, data);
        yield return www.SendWebRequest();
 
        if(www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        }
    }
}
