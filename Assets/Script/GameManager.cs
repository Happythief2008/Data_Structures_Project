using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO; // 파일 저장을 위해 필요
using DhafinFawwaz.AnimationUILib;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public List<QuestionData> questions; 
    public float timeLimitPerQuestion = 10.0f; // 문제당 제한 시간

    [Header("Animations")]
    public AnimationUI nextQuizAnim;
    public AnimationUI resultAnim;

    [Header("References")]
    public UIManager uiManager;

    // 내부 상태 변수들
    private int currentQuestionIndex = 0;
    private int score = 0;
    private int correctCount = 0; // 맞춘 문제 수 추적
    private float currentTimer;
    private bool isTimerRunning = false;
    private bool isProcessingAnswer = false;

    // 결과 저장용 데이터
    private GameResult gameResult = new GameResult();

    private void Start()
    {
        uiManager.Init(this); // UIManager 초기화
        gameResult.playDate = System.DateTime.Now.ToString(); // 게임 시작 시간 기록
        LoadQuestion();
    }

    private void Update()
    {
        // 타이머 로직
        if (isTimerRunning && !isProcessingAnswer)
        {
            currentTimer -= Time.deltaTime;
            uiManager.UpdateTimerUI(currentTimer);

            if (currentTimer <= 0)
            {
                // 시간 초과! 오답 처리 (-1은 오답을 의미하는 임의의 값)
                HandleAnswer(-1); 
            }
        }
    }

    public void LoadQuestion()
    {
        isProcessingAnswer = false;
        
        if (currentQuestionIndex < questions.Count)
        {
            // 타이머 리셋
            currentTimer = timeLimitPerQuestion;
            isTimerRunning = true;

            // UI 갱신 (현재 맞춘 개수 / 전체 문제 수)
            uiManager.UpdateProgressUI(correctCount, questions.Count);
            uiManager.SetQuestionUI(questions[currentQuestionIndex]);
        }
        else
        {
            EndGame();
        }
    }

    // 버튼 클릭 시 호출됨
    public void AnswerClicked(int selectedIndex)
    {
        if (isProcessingAnswer) return;
        HandleAnswer(selectedIndex);
    }

    // 정답 처리 및 데이터 기록 로직
    private void HandleAnswer(int selectedIndex)
    {
        isProcessingAnswer = true;
        isTimerRunning = false; // 타이머 정지

        QuestionData currentQ = questions[currentQuestionIndex];
        bool isCorrect = (selectedIndex == currentQ.correctAnswerIndex);
        
        // 소요 시간 계산 (제한시간 - 남은시간)
        float timeTaken = timeLimitPerQuestion - currentTimer;
        if (timeTaken < 0) timeTaken = timeLimitPerQuestion; // 0초 이하 방지

        // 1. 결과 데이터 기록 (JSON 저장용)
        QuestionRecord record = new QuestionRecord
        {
            questionText = currentQ.questionText,
            isCorrect = isCorrect,
            timeTaken = timeTaken
        };
        gameResult.records.Add(record);

        // 2. 점수 및 정답 카운트 처리
        if (isCorrect)
        {
            score += 10;
            correctCount++;
            Debug.Log($"정답! ({timeTaken:F2}초 소요)");
        }
        else
        {
            Debug.Log($"오답/시간초과! ({timeTaken:F2}초 소요)");
        }

        // UI 즉시 갱신 (정답 수 올라가는 것 보여주기)
        uiManager.UpdateProgressUI(correctCount, questions.Count);

        // 3. 다음 단계로 이동 (코루틴)
        StartCoroutine(ProcessTransition());
    }

    IEnumerator ProcessTransition()
    {
        yield return new WaitForSeconds(1.0f);

        currentQuestionIndex++;

        if (nextQuizAnim != null)
        {
            nextQuizAnim.OnAnimationEnded = () => 
            {
                LoadQuestion();
                nextQuizAnim.OnAnimationEnded = null; 
            };
            nextQuizAnim.Play();
        }
        else
        {
            LoadQuestion();
        }
    }

    private void EndGame()
    {
        Debug.Log("게임 종료! 최종 점수: " + score);
        
        // 결과 데이터 마무리
        gameResult.totalScore = score;
        gameResult.correctCount = correctCount;

        // JSON 저장
        SaveResultToJson();

        uiManager.ShowGameOverUI(score);
        if (resultAnim != null) resultAnim.Play();
    }

    // ★ JSON 저장 함수
    private void SaveResultToJson()
    {
        // 1. 클래스를 JSON 문자열로 변환
        string json = JsonUtility.ToJson(gameResult, true); // true는 사람이 읽기 좋게 줄바꿈

        // 2. 저장 경로 설정 (PC, 모바일 모두 작동하는 경로)
        string path = Path.Combine(Application.persistentDataPath, "GameResult.json");

        // 3. 파일 쓰기
        File.WriteAllText(path, json);

        Debug.Log("결과 저장 완료: " + path);
    }
}