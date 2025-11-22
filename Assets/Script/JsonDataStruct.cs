using System;
using System.Collections.Generic;

[System.Serializable]
public class GameResult
{
    public string playDate;      
    public int correctCount;     
    public float totalTime;      
    // (QuizManager의 questionsPerGame 변수를 직접 참조할 수 없으므로, 기본값 10을 사용하거나 History에 totalQuestions를 추가해야 합니다. 여기서는 편의상 10을 사용합니다.)
}
[System.Serializable]
public class GameHistory
{
    public List<GameResult> results = new List<GameResult>();
}
// -------