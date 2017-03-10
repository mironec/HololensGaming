using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;

public class OffsetPhraseRecogniser : MonoBehaviour {
    KeywordRecognizer keywordRecognizer;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    public OffsetArucoRunner offsetRunner; //Reference the runner so we can adjust the offset from callbacks
	// Use this for initialization
	void Start () {
        keywords.Add("left", () => {
            offsetRunner.offset.x -= 0.005f;
        });
        keywords.Add("right", () => {
            offsetRunner.offset.x += 0.005f;
        });
        keywords.Add("up", () => {
            offsetRunner.offset.y += 0.005f;
        });
        keywords.Add("down", () => {
            offsetRunner.offset.y -= 0.005f;
        });
        keywords.Add("forwards", () => {
            offsetRunner.offset.z += 0.005f;
        });
        keywords.Add("backwards", () => {
            offsetRunner.offset.z -= 0.005f;
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
