using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI questionText;
    public Image questionImageDisplay;
    public AnswerButton[] answerButtons;
    
    [Header("New UI")]
    public TextMeshProUGUI timerText;    // 남은 시간 표시 (예: "10.5s")
    public TextMeshProUGUI progressText; // 진행 상황 표시 (예: "정답: 2/10")

    private GameManager gameManager;

    // 초기화
    public void Init(GameManager gm)
    {
        gameManager = gm;
    }

    public void SetQuestionUI(QuestionData question)
    {
        // 기존 텍스트/이미지 설정 로직 유지
        if (questionText != null) questionText.text = question.questionText;
        
        if (question.questionImage != null)
        {
            questionImageDisplay.sprite = question.questionImage;
            questionImageDisplay.gameObject.SetActive(true);
            questionImageDisplay.preserveAspect = true;
        }
        else
        {
            questionImageDisplay.gameObject.SetActive(false);
        }

        // 버튼 설정
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < question.answers.Length)
            {
                answerButtons[i].gameObject.SetActive(true);
                answerButtons[i].Setup(question.answers[i], i, gameManager);
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // 타이머 UI 갱신
    public void UpdateTimerUI(float time)
    {
        if (timerText != null)
            timerText.text = $"Time: {time:F1}s";
    }

    // 진행 상황(점수) UI 갱신
    public void UpdateProgressUI(int currentCorrect, int totalQuestions)
    {
        if (progressText != null)
            progressText.text = $"Progress: {currentCorrect} / {totalQuestions}";
    }

    public void ShowGameOverUI(int finalScore)
    {
        if (questionText != null) questionText.text = "Score : " + finalScore;
        if (questionImageDisplay != null) questionImageDisplay.gameObject.SetActive(false);
        if (timerText != null) timerText.text = "";
    }
}