using TMPro;
using UnityEngine;
using UnityEngine.UI;

// BoxElement.cs
// 박스 개체의 값을 저장하고 업데이트하는 스크립트
public class BoxElement : MonoBehaviour
{
    public TMP_Text valueText;    // 값 표시를 위한 UI 텍스트
    private int value;
    public int Value { get { return value; } }

    // 값을 설정하고 텍스트를 업데이트
    public void SetValue(int val)
    {
        value = val;
        if (valueText != null)
        {
            valueText.text = val.ToString();
        }
    }
}