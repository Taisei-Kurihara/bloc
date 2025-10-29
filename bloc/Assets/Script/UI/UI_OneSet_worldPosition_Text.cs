using TMPro;
using UnityEngine;

public class UI_OneSet_worldPosition_Text : UI_OneSet_worldPosition
{
    [SerializeField]
    private TextMeshProUGUI _textMeshProUGUI;

    // TextMeshProUGUIのプロパティ.
    public TextMeshProUGUI TextMeshProUGUI => _textMeshProUGUI;

    protected override void Initialize()
    {

    }
    // 初期化処理を行うメソッド (UI_OneSet_abstractから継承).
    public void Initialize(string text,float size = 100f)
    {
        Text = text;

        FontSize = size;

    }
    // テキストの設定用プロパティ.
    public string Text
    {
        get => _textMeshProUGUI != null ? _textMeshProUGUI.text : string.Empty;
        set
        {
            if (_textMeshProUGUI != null)
            {
                _textMeshProUGUI.text = value;
            }
        }
    }

    // テキストの色設定用プロパティ.
    public Color TextColor
    {
        get => _textMeshProUGUI != null ? _textMeshProUGUI.color : Color.white;
        set
        {
            if (_textMeshProUGUI != null)
            {
                _textMeshProUGUI.color = value;
            }
        }
    }

    // フォントサイズ設定用プロパティ.
    public float FontSize
    {
        get => _textMeshProUGUI != null ? _textMeshProUGUI.fontSize : 0f;
        set
        {
            if (_textMeshProUGUI != null)
            {
                _textMeshProUGUI.fontSize = value;
            }
        }
    }

    // 垂直方向の整列設定用プロパティ.
    public TextAlignmentOptions Alignment
    {
        get => _textMeshProUGUI != null ? _textMeshProUGUI.alignment : TextAlignmentOptions.Center;
        set
        {
            if (_textMeshProUGUI != null)
            {
                _textMeshProUGUI.alignment = value;
            }
        }
    }

    // 自動サイズ調整有効化プロパティ.
    public bool EnableAutoSizing
    {
        get => _textMeshProUGUI != null ? _textMeshProUGUI.enableAutoSizing : false;
        set
        {
            if (_textMeshProUGUI != null)
            {
                _textMeshProUGUI.enableAutoSizing = value;
            }
        }
    }

    // 自動サイズ調整の最小フォントサイズプロパティ.
    public float FontSizeMin
    {
        get => _textMeshProUGUI != null ? _textMeshProUGUI.fontSizeMin : 0f;
        set
        {
            if (_textMeshProUGUI != null)
            {
                _textMeshProUGUI.fontSizeMin = value;
            }
        }
    }

    // 自動サイズ調整の最大フォントサイズプロパティ.
    public float FontSizeMax
    {
        get => _textMeshProUGUI != null ? _textMeshProUGUI.fontSizeMax : 0f;
        set
        {
            if (_textMeshProUGUI != null)
            {
                _textMeshProUGUI.fontSizeMax = value;
            }
        }
    }

    // テキストの折り返し設定プロパティ.
    public bool EnableWordWrapping
    {
        get => _textMeshProUGUI != null ? _textMeshProUGUI.enableWordWrapping : false;
        set
        {
            if (_textMeshProUGUI != null)
            {
                _textMeshProUGUI.enableWordWrapping = value;
            }
        }
    }

    // テキストオーバーフローモード設定プロパティ.
    public TextOverflowModes OverflowMode
    {
        get => _textMeshProUGUI != null ? _textMeshProUGUI.overflowMode : TextOverflowModes.Overflow;
        set
        {
            if (_textMeshProUGUI != null)
            {
                _textMeshProUGUI.overflowMode = value;
            }
        }
    }
    
}
