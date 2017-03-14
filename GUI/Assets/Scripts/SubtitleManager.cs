using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR.WSA.Input;
using UnityEngine.Windows.Speech;

public class SubtitleManager : MonoBehaviour {
    GestureRecognizer _gestureRecognizer;

    bool _isSleeping = true;
    public Text _statusText;
    public RawImage _statusImage;
    public Text _subtitleText;
    DictationRecognizer _dictationRecognizer; 
    // Use this for initialization
    void Start () {
        _gestureRecognizer = new GestureRecognizer();
        _gestureRecognizer.TappedEvent += _gestureRecognizer_TappedEvent;
        _gestureRecognizer.StartCapturingGestures(); 
	
	}

    void Awake()
    {
        _dictationRecognizer = new DictationRecognizer();

        _dictationRecognizer.DictationHypothesis += _dictationRecognizer_DictationHypothesis;

        _dictationRecognizer.DictationResult += _dictationRecognizer_DictationResult;

        _dictationRecognizer.DictationComplete += _dictationRecognizer_DictationComplete;
    }

    private void _dictationRecognizer_DictationComplete(DictationCompletionCause cause)
    {
        _dictationRecognizer.Stop();
        SetSleeping();
    }

    private void _dictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
        this._subtitleText.text = text;
        SetListening();
    }

    private void _dictationRecognizer_DictationHypothesis(string text)
    {
        SetThinking();
    }

    private void _gestureRecognizer_TappedEvent(InteractionSourceKind source, int tapcount, Ray headRay)
    {
        _isSleeping = !_isSleeping;

        if(_isSleeping)
        {
            SetSleeping();
            _dictationRecognizer.Stop();
        }
        else
        {
            SetListening();
            _dictationRecognizer.Start();
        }
    }

    private void SetListening()
    {
        var listeningTexture = (Texture2D)Resources.Load("Listening");
        this._statusImage.texture = listeningTexture;
        this._statusText.text = "Listening";
    }

    private void SetSleeping()
    {
        var sleepingTexture = (Texture2D)Resources.Load("Sleeping");
        this._statusImage.texture = sleepingTexture; 
        this._statusText.text = "Sleeping";
        this._subtitleText.text = string.Empty;
    }

    private void SetThinking()
    {
        var thinkingTexture = (Texture2D)Resources.Load("Thinking");
        this._statusImage.texture = thinkingTexture;
        this._statusText.text = "Thinking";
    }


}
