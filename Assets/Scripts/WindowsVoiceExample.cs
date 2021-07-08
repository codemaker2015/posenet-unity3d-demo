using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowsVoiceExample : MonoBehaviour
{
    [SerializeField] InputField inputField;
    int i = 0;
    public void Run()
    {
       for(int i=0;i<10;i++)
            WindowsVoice.speak("Helloo" + i);
    }
    
}
