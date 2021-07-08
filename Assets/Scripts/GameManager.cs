using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TensorFlowLite;
using Cysharp.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    [SerializeField, FilePopup("*.tflite")] string fileName = "posenet_mobilenet_v1_100_257x257_multi_kpt_stripped.tflite";
    [SerializeField] RawImage cameraView = null;
    [SerializeField, Range(0f, 1f)] float threshold = 0.25f;
    [SerializeField, Range(0f, 1f)] float lineThickness = 0.7f;
    [SerializeField] bool runBackground;

    WebCamTexture webcamTexture;
    PoseNet poseNet;
    Vector3[] corners = new Vector3[4];
    PrimitiveDraw draw;
    UniTask<bool> task;
    PoseNet.Result[] results;
    CancellationToken cancellationToken;

    bool isCameraPreview; 
    int part;
    int score;
    float game4Threshold;
    bool lockGame = false;

    [SerializeField] int gameId;
    [SerializeField] Button btnPreview;
    [SerializeField] GameObject imgPreview, leftApple, rightApple;
    [SerializeField] Text txtMessage, txtScore;
    [SerializeField] AudioSource audioSource;
 
    void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        poseNet = new PoseNet(path);

        // Init camera
        string cameraName = WebCamUtil.FindName();
        webcamTexture = new WebCamTexture(cameraName, 640, 480, 30);
        webcamTexture.Play();
        cameraView.texture = webcamTexture;

        draw = new PrimitiveDraw()
        {
            color = Color.green,
        };

        cancellationToken = this.GetCancellationTokenOnDestroy();
        Global.COMPLETED = false;
        part = 0;
        score = 0;
        threshold = 0f;

        switch(gameId){
            case 1: txtMessage.text = "Raise your right hand to catch the bird."; break;
            case 2: txtMessage.text = "Raise your right hand to pick the apple."; break;
            case 3: txtMessage.text = "Move right to collect the apple."; break;
            case 4: txtMessage.text = "Move slightly towards right and\n look towards right to collect the box."; break;
            case 5: txtMessage.text = "Sit down to catch the balloon."; break;
            default: txtMessage.text = "Raise your right hand to pick the apple."; break;
        }
        WindowsVoice.speak(txtMessage.text);
    }

    void OnDestroy()
    {
        webcamTexture?.Stop();
        poseNet?.Dispose();
        draw?.Dispose();
    }

    void Update()
    {
        if (runBackground)
        {
            if (task.Status.IsCompleted())
            {
                task = InvokeAsync();
            }
        }
        else
        {
            poseNet.Invoke(webcamTexture);
            results = poseNet.GetResults();
            cameraView.material = poseNet.transformMat;
        }

        if (results != null)
        {
            DrawResult();
            if(!lockGame)
                CalculateScore();
        }
    } 

    void DrawResult()
    {
        var rect = cameraView.GetComponent<RectTransform>();
        rect.GetWorldCorners(corners);
        Vector3 min = corners[0];
        Vector3 max = corners[2];

        var connections = PoseNet.Connections;
        int len = connections.GetLength(0);
        for (int i = 0; i < len; i++)
        {
            var a = results[(int)connections[i, 0]];
            var b = results[(int)connections[i, 1]];
            if (a.confidence >= threshold && b.confidence >= threshold)
            {
                draw.Line3D(
                    MathTF.Lerp(min, max, new Vector3(a.x, 1f - a.y, 0)),
                    MathTF.Lerp(min, max, new Vector3(b.x, 1f - b.y, 0)),
                    lineThickness
                );
            }
        }

        draw.Apply();
    }

    async UniTask<bool> InvokeAsync()
    {
        results = await poseNet.InvokeAsync(webcamTexture, cancellationToken);
        cameraView.material = poseNet.transformMat;
        return true;
    }

    public void CalculateScore() {
        switch(gameId) {
            case 1 : Game1(); break;
            case 2 : Game2(); break;
            case 3 : Game3(); break;
            case 4 : Game4(); break;
            case 5 : Game5(); break;
            default: Game1(); break;
        }
    }

    public void Game1() {
        PoseNet.Result result;
        if(!lockGame && part == 0) {
            result = results[10]; //Right Wrist
            //Debug.Log(result.part + " | " + result.x + " | " + result.y);
            if(result.y < 0.35f - game4Threshold / 500 && results[9].y > 0.5 + game4Threshold / 500 ){
                rightApple.SetActive(false);
                leftApple.SetActive(true);
                game4Threshold = Random.Range(-65, 25);
                leftApple.GetComponent<RectTransform>().anchoredPosition += new Vector2(game4Threshold, game4Threshold);
                txtMessage.text = "Raise your left hand to catch the bird.";
                txtScore.text = (++score).ToString();
                part = 1;
                audioSource.Play();
                WindowsVoice.speak(txtMessage.text);
                StartCoroutine(Delay(1));
            }
        }else if(!lockGame && part == 1) {
            result = results[9]; //Left Wrist
            // Debug.Log(result.part + " | " + result.x + " | " + result.y);
            if(result.y < 0.35f - game4Threshold / 500 && results[10].y > 0.5 + game4Threshold / 500 ){
                leftApple.SetActive(false);
                rightApple.SetActive(true);
                game4Threshold = Random.Range(-65, 25);
                rightApple.GetComponent<RectTransform>().anchoredPosition += new Vector2(game4Threshold, game4Threshold);
                txtMessage.text = "Raise your right hand to catch the bird.";
                txtScore.text = (++score).ToString();
                part = 0;
                audioSource.Play();
                WindowsVoice.speak(txtMessage.text);
                StartCoroutine(Delay(1));
            }
        }
    }

    public void Game2() {
        PoseNet.Result result;
        if(!lockGame && part == 0) {
            result = results[10]; //Right Wrist
            // Debug.Log(result.part + " | " + result.x + " | " + result.y);
            if(result.y < 0.3f && results[9].y > 0.5){
                rightApple.SetActive(false);
                leftApple.SetActive(true);
                game4Threshold = Random.Range(-20, 20);
                leftApple.GetComponent<RectTransform>().anchoredPosition += new Vector2(game4Threshold, game4Threshold);
                txtMessage.text = "Raise your left hand to pick the apple.";
                txtScore.text = (++score).ToString();
                part = 1;
                audioSource.Play();
                WindowsVoice.speak(txtMessage.text);
                StartCoroutine(Delay(1));
            }
        }else if(!lockGame && part == 1) {
            result = results[9]; //Left Wrist
            // Debug.Log(result.part + " | " + result.x + " | " + result.y);
            if(result.y < 0.3f && results[10].y > 0.5){
                leftApple.SetActive(false);
                rightApple.SetActive(true);
                game4Threshold = Random.Range(-20, 20);
                rightApple.GetComponent<RectTransform>().anchoredPosition += new Vector2(game4Threshold, game4Threshold);
                txtMessage.text = "Raise your right hand to pick the apple.";
                txtScore.text = (++score).ToString();
                part = 0;
                audioSource.Play();
                WindowsVoice.speak(txtMessage.text);
                StartCoroutine(Delay(1));
            }
        }
    }

    public void Game3() {
        PoseNet.Result result = results[0]; //Nose
        // Debug.Log(result.part + " | " + result.x + " | " + result.y);
        if(!lockGame && part == 0) {
            if(result.x < 0.3f){
                rightApple.SetActive(false);
                leftApple.SetActive(true);
                txtMessage.text = "Move left to collect the apple.";
                txtScore.text = (++score).ToString();
                part = 1;
                audioSource.Play();
                WindowsVoice.speak(txtMessage.text);
                StartCoroutine(Delay(1));
            }
        }else if(!lockGame && part == 1) {
            if(result.x > 0.8f){
                leftApple.SetActive(false);
                rightApple.SetActive(true);
                txtMessage.text = "Move right to collect the apple.";
                txtScore.text = (++score).ToString();
                part = 0;
                audioSource.Play();
                WindowsVoice.speak(txtMessage.text);
                StartCoroutine(Delay(1));
            }
        }
    }
    
    public void Game4() {
        PoseNet.Result result = results[0]; //Nose
        // Debug.Log(result.part + " | " + result.x + " | " + result.y);
        if(!lockGame && part == 0) {
            if(result.x < 0.4f){
                rightApple.SetActive(false);
                leftApple.SetActive(true);
                txtMessage.text = "Move slightly towards left and\n look towards left to collect the box.";
                txtScore.text = (++score).ToString();
                part = 1;
                audioSource.Play();
                WindowsVoice.speak(txtMessage.text);
                StartCoroutine(Delay(1));
            }
        }else if(!lockGame && part == 1) {
            if(result.x > 0.6f){
                leftApple.SetActive(false);
                rightApple.SetActive(true);
                txtMessage.text = "Move slightly towards right and\n look towards right to collect the box.";
                txtScore.text = (++score).ToString();
                part = 0;
                audioSource.Play();
                WindowsVoice.speak(txtMessage.text);
                StartCoroutine(Delay(1));
            }
        }
    }

    public void Game5() {
        PoseNet.Result result = results[0]; //Nose
        if(!lockGame)
            Debug.Log(result.part + " | " + result.x + " | " + result.y);
        if(!lockGame && part == 0) {
            if(result.y > 0.7f){
                rightApple.SetActive(false);
                leftApple.SetActive(true);
                txtMessage.text = "Stand up to catch the balloon.";
                txtScore.text = (++score).ToString();
                part = 1;
                audioSource.Play();
                WindowsVoice.speak(txtMessage.text);
                StartCoroutine(Delay(2));
            }
        }else if(!lockGame && part == 1) {
            if(result.y < 0.25f){
                leftApple.SetActive(false);
                rightApple.SetActive(true);
                txtMessage.text = "Sit down to catch the balloon.";
                txtScore.text = (++score).ToString();
                part = 0;
                audioSource.Play();
                WindowsVoice.speak(txtMessage.text);
                StartCoroutine(Delay(2));
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

    IEnumerator Delay(int sec){
        lockGame = true;
        yield return new WaitForSeconds(sec);
        lockGame = false;
    }
}
