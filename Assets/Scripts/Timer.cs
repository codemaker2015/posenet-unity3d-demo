 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour {

	public int timeLeft;
    public Text timeText;
    public AudioSource timeup;

    // Use this for initialization
    void Start () {
        Time.timeScale = 1;
        timeLeft = 60;
		StartCoroutine ("Countdown");
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		}

		if (timeLeft == 0) { 
			StopCoroutine ("Countdown");
            timeup.Pause();
            Global.COMPLETED = true;
            StartCoroutine(GotoMain());
        }

        if(timeLeft == 6)
        {
            timeup.Play();
        }
	}

    IEnumerator GotoMain()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("Menu");
    }

    IEnumerator Countdown(){
		while (true) {
			yield return new WaitForSeconds (1.0f);
			timeLeft--;
			timeText.text = timeLeft.ToString();
		}
	}

    public void StopTimer()
    {
        timeLeft = -1;
        timeup.Pause();
        StopCoroutine("Countdown");
        timeText.text = "0";
    }
}
