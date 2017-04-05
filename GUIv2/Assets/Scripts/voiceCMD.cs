using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR.WSA.Input;
using UnityEngine.Windows.Speech;

public class voiceCMD : MonoBehaviour {

    public Canvas MainCanvas;
    public Canvas LevelCanvas;
    DictationRecognizer _dictationRecognizer;
    // Use this for initialization
    void Start()
    {
        /*_gestureRecognizer = new GestureRecognizer();
        _gestureRecognizer.TappedEvent += _gestureRecognizer_TappedEvent;
        _gestureRecognizer.StartCapturingGestures(); */
        _dictationRecognizer.Start();
    }

    void Awake()
    {
        _dictationRecognizer = new DictationRecognizer();  
    }

    public void ShowKeyword()
    {
        MainCanvas.gameObject.SetActive(false);
    }

    public void HideKeyword()
    {
        LevelCanvas.gameObject.SetActive(false);
        MainCanvas.gameObject.SetActive(false);

    }








}
