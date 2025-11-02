using Cysharp.Threading.Tasks;
using System;
using R3;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[System.Serializable]
public class UI_Mono_Button : UI_Mono_abstract
{
    [SerializeField]
    Button button;
    public Button Button => button;


    [SerializeField]
    UI_Mono_Text buttonText;
    public UI_Mono_Text ButtonText => buttonText;

    public void Initialize(float width = 0,float height = 0, string text = "Button", float fontsize = 36f)
    {
        if (width != 0) Width = width;
        if (height != 0) Height = height;

        buttonText.Text = text;
        buttonText.FontSize = fontsize;
    }

    // ボタンのサイズを設定するメソッド.
    public void SetSize(float width, float height)
    {
        if (button != null)
        {
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(width, height);
            }
        }
    }

    // ボタンの幅を設定するプロパティ.
    public float Width
    {
        get
        {
            if (button != null)
            {
                RectTransform rectTransform = button.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    return rectTransform.sizeDelta.x;
                }
            }
            return 0f;
        }
        set
        {
            if (button != null)
            {
                RectTransform rectTransform = button.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = new Vector2(value, rectTransform.sizeDelta.y);
                }
            }
        }
    }

    // ボタンの高さを設定するプロパティ.
    public float Height
    {
        get
        {
            if (button != null)
            {
                RectTransform rectTransform = button.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    return rectTransform.sizeDelta.y;
                }
            }
            return 0f;
        }
        set
        {
            if (button != null)
            {
                RectTransform rectTransform = button.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, value);
                }
            }
        }
    }
}
