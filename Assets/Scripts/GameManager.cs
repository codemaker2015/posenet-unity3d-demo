using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TensorFlowLite;

public class GameManager : MonoBehaviour
{
    bool isCameraPreview; 
    int part;
    int score;
    float game4Threshold;
    bool lockGame;
    PoseNet.Result[] results;
    [SerializeField] int gameId;
    [SerializeField] Button btnPreview;
    [SerializeField] GameObject imgPreview, leftObj, rightObj;
    [SerializeField] Text txtMessage, txtScore;
    [SerializeField] AudioSource audioSource;

    void Start()
    {
        Global.COMPLETED = false;
        part = 0;
        score = 0;
        lockGame = false;

        switch(gameId){
            case 1: txtMessage.text = "Raise your Right Hand to catch the Bird."; break;
            case 2: txtMessage.text = "Raise your Right Hand to pick the Apple."; break;
            case 3: txtMessage.text = "Move Right to collect the Apple."; break;
            case 4: txtMessage.text = "Move slightly to Right and\n look towards Right to collect the Box."; break;
            case 5: txtMessage.text = "Sit Down to catch the Balloon."; break;
            case 6: txtMessage.text = "Move towards Right and\n Raise your Right hand to collect the Balloon."; break;
            case 7: txtMessage.text = "Squat Down and raise your hands to throw the ball.;"; break;
            case 8: txtMessage.text = "Extend your both arms to the side. \n Move them up and down (Fly action) to collect the star."; break;
            default: txtMessage.text = "Wrong Game ID"; break;
        }
        StartCoroutine(Delay(1, ()=> { WindowsVoice.speak(txtMessage.text); }));
    }

    void Update()
    {
        results = PoseNetManager.results;
        if(PoseNetManager.results != null  && !lockGame)
            CalculateScore();
    } 

    public void CalculateScore() {
        switch(gameId) {
            case 1 : Game1(); break;
            default: Game1(); break;
        }
    }

    public void Game1() {
        PoseNet.Result result;
        if(!lockGame && part == 0) {
            result = results[10]; //Right Wrist
            //Debug.Log(result.part + " | " + result.x + " | " + result.y);
            if(result.y < 0.35f - game4Threshold / 500 && results[9].y > 0.5 + game4Threshold / 500 ){
                rightObj.SetActive(false);
                leftObj.SetActive(true);
                game4Threshold = UnityEngine.Random.Range(-65, 25);
                leftObj.GetComponent<RectTransform>().anchoredPosition += new Vector2(game4Threshold, game4Threshold);
                txtMessage.text = "Raise your Left Hand to catch the Bird.";
                txtScore.text = (++score).ToString();
                Global.ANALYTICS_DATA_UR++;
                part = 1;
                audioSource.Play();
                StartCoroutine(Delay(1, ()=> { WindowsVoice.speak(txtMessage.text); }));
                StartCoroutine(LockGame(1));
            }
        }else if(!lockGame && part == 1) {
            result = results[9]; //Left Wrist
            // Debug.Log(result.part + " | " + result.x + " | " + result.y);
            if(result.y < 0.35f - game4Threshold / 500 && results[10].y > 0.5 + game4Threshold / 500 ){
                leftObj.SetActive(false);
                rightObj.SetActive(true);
                game4Threshold = UnityEngine.Random.Range(-65, 25);
                rightObj.GetComponent<RectTransform>().anchoredPosition += new Vector2(game4Threshold, game4Threshold);
                txtMessage.text = "Raise your Right Hand to catch the Bird.";
                txtScore.text = (++score).ToString();
                Global.ANALYTICS_DATA_UL++;
                part = 0;
                audioSource.Play();
                StartCoroutine(Delay(1, ()=> { WindowsVoice.speak(txtMessage.text); }));
                StartCoroutine(LockGame(1));
            }
        }
    }

    public void TogglePreview() 
    {
        isCameraPreview = !isCameraPreview;
        if(isCameraPreview){
            btnPreview.transform.GetChild(0).GetComponent<Text>().text = "UI";
            imgPreview.SetActive(false);
        }else{
            btnPreview.transform.GetChild(0).GetComponent<Text>().text = "Preview";
            imgPreview.SetActive(true);
        }
    }

    IEnumerator LockGame(float sec){
        lockGame = true;
        yield return new WaitForSeconds(sec);
        lockGame = false;
    }

    IEnumerator Delay(float sec, Action callback){
        yield return new WaitForSeconds(sec);
        callback.Invoke();
    }

    public static float GetAnimationLength(Animator anim, string name) {
        RuntimeAnimatorController ac = anim.runtimeAnimatorController;    
        for(int i = 0; i<ac.animationClips.Length; i++)
            if(ac.animationClips[i].name == name)
                return ac.animationClips[i].length; 
        return 0;
    }

}
