using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using DhafinFawwaz.AnimationUILib;
using System.Linq; 
using System; 

// (GameResultHistory, GameHistory 구조체가 이 위에 정의되어 있다고 가정)

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public List<QuestionData> questions; 
    public float totalGameTimeLimit = 60.0f; 

    [Header("Animations")]
    public AnimationUI nextQuizAnim;
    public AnimationUI resultAnim;

    [Header("References")]
    public UIManager uiManager;

    // 내부 상태 변수들
    private int currentQuestionIndex = 0;
    private int score = 0;
    private int correctCount = 0;
    
    private float remainingTotalTime;
    private float questionStartTime;
    
    private bool isGameActive = false;
    private bool isProcessingAnswer = false;
    
    // (이전에 사용된 gameResult 객체는 현재 JSON History 저장에 필요하지 않으므로 사용하지 않습니다.)

    private void Start()
    {
        uiManager.Init(this);
        
        remainingTotalTime = totalGameTimeLimit;
        
        isGameActive = true;
        questionStartTime = Time.time; 

        LoadQuestion();
    }

    private void Update()
    {
        if (isGameActive && !isProcessingAnswer)
        {
            remainingTotalTime -= Time.deltaTime;
            uiManager.UpdateTimerUI(remainingTotalTime);

            if (remainingTotalTime <= 0)
            {
                remainingTotalTime = 0;
                uiManager.UpdateTimerUI(0);
                Debug.Log("전체 시간 종료!");
                EndGame(); 
            }
        }
    }

    public void LoadQuestion()
    {
        isProcessingAnswer = false;
        questionStartTime = Time.time; 

        if (currentQuestionIndex < questions.Count)
        {
            uiManager.UpdateProgressUI(correctCount, questions.Count);
            uiManager.SetQuestionUI(questions[currentQuestionIndex]);
        }
        else
        {
            EndGame();
        }
    }

    public void AnswerClicked(int selectedIndex)
    {
        if (!isGameActive || isProcessingAnswer) return;
        HandleAnswer(selectedIndex);
    }

    private void HandleAnswer(int selectedIndex)
    {
        isProcessingAnswer = true;

        QuestionData currentQ = questions[currentQuestionIndex];
        bool isCorrect = (selectedIndex == currentQ.correctAnswerIndex);
        
        float timeSpentOnThisQuestion = Time.time - questionStartTime;
        
        if (isCorrect)
        {
            score += 10;
            correctCount++;
            Debug.Log($"정답! (소요 시간: {timeSpentOnThisQuestion:F2}초)");
        }
        else
        {
            Debug.Log($"오답! (소요 시간: {timeSpentOnThisQuestion:F2}초)");
        }

        uiManager.UpdateProgressUI(correctCount, questions.Count);

        StartCoroutine(ProcessTransition());
    }

    IEnumerator ProcessTransition()
    {
        yield return new WaitForSeconds(1.0f);

        if (!isGameActive) yield break;

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
        if (!isGameActive) return;
        isGameActive = false;

        // 실제 소요 시간 계산
        float timeTaken = totalGameTimeLimit - remainingTotalTime;

        Debug.Log("게임 종료! 최종 점수: " + score);
        
        // 새로운 JSON 형식에 맞춰 저장 함수 호출
        SaveResultToHistoryJson(timeTaken);

        uiManager.ShowGameOverUI(score);
        if (resultAnim != null) resultAnim.Play();
    }

    // ★★★ JSON History 저장 함수 ★★★
    private void SaveResultToHistoryJson(float timeTaken)
    {
        // 1. 기존 기록 불러오기
        GameHistory history = LoadExistingHistory();

        // 2. 현재 게임 결과 생성 (영어 속성명 사용)
        GameResult newResult = new GameResult
        {
            playDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 
            correctCount = correctCount,
            totalTime = timeTaken
        };
        
        // 3. 리스트에 추가
        history.results.Add(newResult);

        // 4. 업데이트된 리스트를 JSON 파일로 저장
        string json = JsonUtility.ToJson(history, true);
        string path = Path.Combine(Application.persistentDataPath, "quizHistory.json"); 
        
        try
        {
            File.WriteAllText(path, json);
            Debug.Log($"게임 결과 기록 저장 완료: {path}. 정답: {newResult.correctCount}, 시간: {newResult.totalTime:F2}초");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"게임 결과 저장 실패: {e.Message}");
        }
    }
    
    // JSON History 불러오기 헬퍼 함수 (영어 속성명 사용)
    private GameHistory LoadExistingHistory()
    {
        string path = Path.Combine(Application.persistentDataPath, "quizHistory.json");
        
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json)) return new GameHistory();
                
                return JsonUtility.FromJson<GameHistory>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"기존 기록 불러오기 실패 (파일 형식 오류?): {e.Message}. 새 기록으로 시작합니다.");
                return new GameHistory(); 
            }
        }
        return new GameHistory();
    }
}