using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// BoxManager.cs
// 박스 생성, 퍼뜨리기, 정렬, 모으기 사이클을 관리하는 스크립트
public class BoxManager : MonoBehaviour
{
    public GameObject boxPrefab;     // 박스 프리팹
    public Transform spawnPoint;     // 박스 생성 및 모임 위치
    public int numBoxes = 10;        // 박스 개수
    public float spacing = 2f;       // 퍼뜨릴 때 박스 간 거리
    public float moveSpeed = 5f;     // 이동 속도

    public List<GameObject> boxes = new List<GameObject>();
    private Vector3[] targetPositions;   // 퍼뜨려질 위치 배열

    void Start()
    {
        // 박스 초기 생성: 모든 박스를 spawnPoint 위치에 생성
        for (int i = 0; i < numBoxes; i++)
        {
            GameObject box = Instantiate(boxPrefab, spawnPoint.position, Quaternion.identity);
            boxes.Add(box);
        }

        // 퍼뜨릴 때 각 박스의 목표 위치 계산 (가로 방향 일정 간격)
        targetPositions = new Vector3[numBoxes];
        float totalWidth = spacing * (numBoxes - 1);
        float startX = spawnPoint.position.x - totalWidth / 2f;
        for (int i = 0; i < numBoxes; i++)
        {
            targetPositions[i] = new Vector3(startX + i * spacing, spawnPoint.position.y, spawnPoint.position.z);
        }

        // 초기 랜덤 값 설정
        RandomizeValues();

        // 정렬 사이클 시작
        StartCoroutine(RunSortCycle());
    }

    // 정렬 사이클: 퍼뜨리기 -> 삽입 정렬 -> 모으기 -> 값 랜덤화 반복
    IEnumerator RunSortCycle()
    {
        while (true)
        {
            // 박스 퍼뜨리기 애니메이션
            yield return StartCoroutine(SpreadBoxes());

            // 삽입 정렬 애니메이션
            yield return StartCoroutine(InsertionSort());

            // 박스 모으기 애니메이션
            yield return StartCoroutine(CollapseBoxes());

            // 값 랜덤화
            RandomizeValues();
        }
    }

    // 박스를 targetPositions 위치로 이동시킴 (모두 동시에)
    IEnumerator SpreadBoxes()
    {
        bool anyMoving = true;
        while (anyMoving)
        {
            anyMoving = false;
            for (int i = 0; i < boxes.Count; i++)
            {
                GameObject box = boxes[i];
                Vector3 targetPos = targetPositions[i];
                if (Vector3.Distance(box.transform.position, targetPos) > 0.01f)
                {
                    box.transform.position = Vector3.MoveTowards(box.transform.position, targetPos, moveSpeed * Time.deltaTime);
                    anyMoving = true;
                }
            }
            yield return null;
        }
    }

    // 삽입 정렬 알고리즘을 박스 이동으로 시각화
    // 삽입 정렬 알고리즘을 박스 이동으로 시각화
    IEnumerator InsertionSort()
    {
        for (int i = 1; i < boxes.Count; i++)
        {
            GameObject keyBox = boxes[i];
            BoxElement keyElement = keyBox.GetComponent<BoxElement>();
            int keyValue = keyElement.Value;
            int j = i - 1;

            // 🔺 1단계: 위로 들어올리기 (Y축)
            Vector3 liftedPos = new Vector3(keyBox.transform.position.x, keyBox.transform.position.y + 1.5f, 0f);
            yield return StartCoroutine(MoveBoxToPosition(keyBox, liftedPos));

            // 🔄 2단계: 밀리는 박스들 오른쪽으로 이동
            while (j >= 0 && boxes[j].GetComponent<BoxElement>().Value > keyValue)
            {
                // 오른쪽으로 한 칸
                yield return StartCoroutine(MoveBoxToPosition(boxes[j], targetPositions[j + 1]));
                boxes[j + 1] = boxes[j];
                j--;
            }

            // ➡️ 3단계: keyBox 수평 이동 (Y 유지한 채 X만 이동)
            Vector3 insertXPos = new Vector3(targetPositions[j + 1].x, liftedPos.y, 0f);
            yield return StartCoroutine(MoveBoxToPosition(keyBox, insertXPos));

            // 🔻 4단계: keyBox 아래로 내리기
            Vector3 dropPos = targetPositions[j + 1];
            yield return StartCoroutine(MoveBoxToPosition(keyBox, dropPos));

            // 리스트 순서 업데이트
            boxes[j + 1] = keyBox;
        }
    }


    // 한 박스를 목표 위치로 부드럽게 이동시키는 코루틴
    IEnumerator MoveBoxToPosition(GameObject box, Vector3 target)
    {
        while (Vector3.Distance(box.transform.position, target) > 0.01f)
        {
            box.transform.position = Vector3.MoveTowards(box.transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    // 박스를 spawnPoint 위치로 모두 모으는 애니메이션
    IEnumerator CollapseBoxes()
    {
        bool anyMoving = true;
        while (anyMoving)
        {
            anyMoving = false;
            foreach (GameObject box in boxes)
            {
                if (Vector3.Distance(box.transform.position, spawnPoint.position) > 0.01f)
                {
                    box.transform.position = Vector3.MoveTowards(box.transform.position, spawnPoint.position, moveSpeed * Time.deltaTime);
                    anyMoving = true;
                }
            }
            yield return null;
        }
    }

    // 박스의 값을 무작위로 설정
    void RandomizeValues()
    {
        foreach (GameObject box in boxes)
        {
            int randomValue = Random.Range(0, 100); // 0~99 범위 랜덤
            box.GetComponent<BoxElement>().SetValue(randomValue);
        }
    }

    // 정렬 사이클을 재시작 (필요할 경우 호출 가능)
    public void RestartSortCycle()
    {
        StopAllCoroutines();
        // 박스 위치를 spawnPoint로 즉시 리셋
        foreach (GameObject box in boxes)
        {
            box.transform.position = spawnPoint.position;
        }
        // 값 랜덤화 후 재시작
        RandomizeValues();
        StartCoroutine(RunSortCycle());
    }
}
