using System;
using System.Collections.Generic;

[Serializable]
public class GameResult
{
    public string playDate;      // 플레이 날짜
    public int totalScore;       // 최종 점수
    public int correctCount;     // 맞춘 개수
    public List<QuestionRecord> records = new List<QuestionRecord>(); // 문제별 기록 리스트
}

[Serializable]
public class QuestionRecord
{
    public string questionText;  // 문제 내용
    public bool isCorrect;       // 정답 여부
    public float timeTaken;      // 푸는데 걸린 시간 (초)
}