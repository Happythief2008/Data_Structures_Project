using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

public class GameHistoryDisplay : MonoBehaviour
{
    [Header("History UI (4개의 TMP 컴포넌트)")]
    // ★ 4개의 별도 TMP 컴포넌트 배열을 인스펙터에 할당해야 합니다.
    public TextMeshProUGUI[] historyDisplays = new TextMeshProUGUI[4]; 
    
    // 기록이 저장된 파일 경로 (QuizManager나 GameManager와 동일해야 함)
    private string saveFilePath;
    private const int TotalQuestionsPerGame = 10; // 퀴즈의 총 문제 수 (표시를 위해 가정)

    void Start()
    {
        // 저장 파일 경로 설정
        saveFilePath = Path.Combine(Application.persistentDataPath, "quizHistory.json");
        
        // 스크립트 시작 시 기록을 읽어 4개의 TMP에 출력
        DisplayHistory(); 
    }

    /// <summary>
    /// 외부에서 JSON 파일이 업데이트되었을 때 기록을 갱신하기 위해 호출하는 함수입니다.
    /// </summary>
    public void DisplayHistory()
    {
        GameHistory history = LoadHistory();
        
        // 1. 최신순으로 정렬 후 4개만 가져오기
        var recentResults = history.results
                                    .OrderByDescending(r => DateTime.Parse(r.playDate)) 
                                    .Take(4) 
                                    .ToList();

        // 2. 4개의 TMP 컴포넌트에 각각 할당
        for (int i = 0; i < historyDisplays.Length; i++)
        {
            if (historyDisplays[i] == null) continue;

            if (i < recentResults.Count)
            {
                // 기록이 있을 경우 출력
                GameResult r = recentResults[i];
                string formattedTime = r.totalTime.ToString("F2"); 
                
                // 출력 포맷: "정답: X / 10 | 시간: Y.YY초"
                historyDisplays[i].text = 
                    $"Score: {r.correctCount} / {TotalQuestionsPerGame} | Time: {formattedTime}sec";
            }
            else
            {
                // 기록이 없는 슬롯은 초기화
                historyDisplays[i].text = "--- Nothing ---";
            }
        }
    }

    // JSON 파일에서 모든 기록을 불러오는 헬퍼 함수
    private GameHistory LoadHistory()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                if (string.IsNullOrWhiteSpace(json)) return new GameHistory();
                
                return JsonUtility.FromJson<GameHistory>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"기록 불러오기 실패: {e.Message}. 파일을 확인하세요.");
                return new GameHistory(); 
            }
        }
        return new GameHistory(); 
    }
}