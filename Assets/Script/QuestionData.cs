using UnityEngine;

[CreateAssetMenu(fileName = "New Question", menuName = "Quiz/Question")]
public class QuestionData : ScriptableObject
{
    [Header("문제 설정")]
    [TextArea(2, 5)]
    public string questionText; 

    public Sprite questionImage; 

    [Header("답변 설정")]
    public string[] answers;    
    public int correctAnswerIndex; 
}