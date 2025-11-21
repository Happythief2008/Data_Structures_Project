using UnityEngine;
using TMPro; // TextMeshPro 사용 시

public class AnswerButton : MonoBehaviour
{
    public TextMeshProUGUI buttonText;
    private GameManager gameManager;
    private int myIndex;

    // 초기화 함수
    public void Setup(string text, int index, GameManager manager)
    {
        buttonText.text = text;
        myIndex = index;
        gameManager = manager;
    }

    // 버튼 컴포넌트의 OnClick 이벤트에 연결
    public void OnClick()
    {
        gameManager.AnswerClicked(myIndex);
    }
}