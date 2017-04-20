using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UILevelGenerator : MonoBehaviour
{

    public List<string> levels;
    public GameObject levelSelectButtonPrefab;

    public const float topMargin = 70;
    public const float leftMargin = 50;
    public const float rightMargin = 50;
    public const float buttonSizeX = 50;
    public const float buttonSizeY = 50;
    public const int columns = 3;
    //public const int rows = 4;
    private const float padding = 5;

    void Start()
    {
        if (levelSelectButtonPrefab == null) return;
        RectTransform rect = GetComponent<RectTransform>();
        float canvasWidth = rect.rect.width;
        float canvasHeight = rect.rect.height;
        float x = -canvasWidth / 2 + padding + buttonSizeX / 2 + leftMargin;
        float y = topMargin + padding;
        int col = 0;
        int row = 0;
        foreach (string l in levels)
        {
            GameObject newButton = Instantiate(levelSelectButtonPrefab);
            newButton.transform.SetParent(transform, false);

            RectTransform newRect = newButton.GetComponent<RectTransform>();
            newRect.anchoredPosition = new Vector2(x, y);
            newRect.sizeDelta = new Vector2(buttonSizeX, buttonSizeY);

            UnityEngine.UI.Text text = newButton.GetComponentInChildren<UnityEngine.UI.Text>();
            text.text = "" + (col + row * columns + 1);

            UnityEngine.UI.Button button = newButton.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() => SceneManager.LoadScene(l));
            
            if (columns > 1)
                x += ((canvasWidth / 2 - padding - buttonSizeX / 2 - rightMargin) - (-canvasWidth / 2 + padding + buttonSizeX / 2 + leftMargin)) / (columns - 1);
            col++;
            if (col >= columns)
            {
                x = -canvasWidth / 2 + padding + buttonSizeX / 2 + leftMargin;
                col = 0;

                y -= buttonSizeY + padding * 2;
                row++;
            }
        }
    }

    void Update()
    {

    }
}
