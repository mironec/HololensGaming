using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;

public class OffsetPhraseRecogniser : MonoBehaviour {
    KeywordRecognizer keywordRecognizer;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    public ArucoRunner offsetRunner; //Reference the runner so we can adjust the offset from callbacks
	// Use this for initialization
	void Start () {
        keywords.Add("left", () => {
            offsetRunner.offset.x -= 0.005f;
            Debug.Log("left");
            Debug.Log(offsetRunner.offset.x);
            Debug.Log(offsetRunner.offset.y);
            Debug.Log(offsetRunner.offset.z);
        });
        keywords.Add("right", () => {
            offsetRunner.offset.x += 0.005f;
            Debug.Log("right");
            Debug.Log(offsetRunner.offset.x);
            Debug.Log(offsetRunner.offset.y);
            Debug.Log(offsetRunner.offset.z);
        });
        keywords.Add("up", () => {
            offsetRunner.offset.y += 0.005f;
            Debug.Log("up");
            Debug.Log(offsetRunner.offset.x);
            Debug.Log(offsetRunner.offset.y);
            Debug.Log(offsetRunner.offset.z);
        });
        keywords.Add("down", () => {
            offsetRunner.offset.y -= 0.005f;
            Debug.Log("down");
            Debug.Log(offsetRunner.offset.x);
            Debug.Log(offsetRunner.offset.y);
            Debug.Log(offsetRunner.offset.z);
        });
        keywords.Add("to", () => {
            offsetRunner.offset.z += 0.005f;
            Debug.Log("to");
            Debug.Log(offsetRunner.offset.x);
            Debug.Log(offsetRunner.offset.y);
            Debug.Log(offsetRunner.offset.z);
        });
        keywords.Add("from", () => {
            offsetRunner.offset.z -= 0.005f;
            Debug.Log("from");
            Debug.Log(offsetRunner.offset.x);
            Debug.Log(offsetRunner.offset.y);
            Debug.Log(offsetRunner.offset.z);
        });

        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());

        keywordRecognizer.OnPhraseRecognized += onPhraseRecognized;

        keywordRecognizer.Start();
    }

    private void onPhraseRecognized(PhraseRecognizedEventArgs args) {
        System.Action keywordAction;
        if(keywords.TryGetValue(args.text, out keywordAction)) {
            keywordAction.Invoke();
        }
    }
}
