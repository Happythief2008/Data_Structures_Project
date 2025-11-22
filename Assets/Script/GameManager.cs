using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using DhafinFawwaz.AnimationUILib;
using System.Linq; 
using System; 


public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public List<QuestionData> questions; // 이거 랜덤하게 뽑히도록 바뀜
    [Tooltip("질문을 시작 전에 섞을지 여부")] public bool shuffleQuestions = true;
    [Tooltip("0 이면 전체 사용, 0보다 크면 섞은 뒤 최대 개수만큼 사용")]
    public int maxQuestions = 0; 
    public float totalGameTimeLimit = 60.0f; 

    [Header("Animations")]
    public AnimationUI nextQuizAnim;
    public AnimationUI resultAnim; 
    [Header("References")]
    public UIManager uiManager;

    private int currentQuestionIndex = 0;
    private int score = 0;
    private int correctCount = 0;
    private float remainingTotalTime;
    private float questionStartTime;
    private bool isGameActive = false;
    private bool isProcessingAnswer = false;

    private void Start()
    {
        uiManager.Init(this);

        // 질문 리스트를 섞고 필요하면 일부만 사용하도록 처리
        if (questions == null) questions = new List<QuestionData>();

        if (shuffleQuestions && questions.Count > 1)
            ShuffleQuestions();

        if (maxQuestions > 0 && maxQuestions < questions.Count)
            questions = questions.Take(maxQuestions).ToList();

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


        StartCoroutine(ProcessTransition());
    }

    IEnumerator ProcessTransition()
    {
        yield return new WaitForSeconds(1.0f);

        if (!isGameActive) yield break;

        currentQuestionIndex++;

        bool isLastQuestion = currentQuestionIndex >= questions.Count;

        if (isLastQuestion)
        {
            EndGame();
        }
        else 
        {
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
    }

    private void EndGame()
    {
        if (!isGameActive) return;
        isGameActive = false;

        float timeTaken = totalGameTimeLimit - remainingTotalTime;

        Debug.Log("게임 종료! 최종 점수: " + score);
        
        SaveResultToHistoryJson(timeTaken);

        uiManager.ShowGameOverUI(score);
        
        if (resultAnim != null) resultAnim.Play(); 
    }

    // JSON History 저장 함수
    private void SaveResultToHistoryJson(float timeTaken)
    {
        GameHistory history = LoadExistingHistory();

        GameResult newResult = new GameResult
        {
            playDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 
            correctCount = correctCount,
            totalTime = timeTaken
        };
        
        history.results.Add(newResult);

        // 업데이트된 리스트를 JSON 파일로 저장
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
    
    // JSON History 불러오기 헬퍼 함수
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

    // Fisher-Yates 섞기
    private void ShuffleQuestions()
    {
        if (questions == null || questions.Count <= 1) return;

        var rng = new System.Random();
        int n = questions.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var tmp = questions[k];
            questions[k] = questions[n];
            questions[n] = tmp;
        }
    }
}
