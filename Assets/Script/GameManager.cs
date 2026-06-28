using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerProgress playerProgress;
    public RunState runState;

    private void Awake()
    {
        if (Instance == null) //인스턴스가 없다면 (최초)
        {
            Instance = this; //게임매니저 지정
            DontDestroyOnLoad(gameObject); //파괴불가

            playerProgress = SaveSystem.Load(); //영구 데이터 받아오기
            StartNewRun(); // 회차 데이터 생성
        }
        else //이미 있다면
        {
            Destroy(gameObject); //게임매니저 제거(중복 제거용)
        }
    }

    public void OnPlayerDeath() //플레이어가 죽으면 
    {
        SaveSystem.Save(playerProgress); // 영구 데이터 저장
        StartNewRun(); // 회차 데이터 초기화 + 영구 스킬 복사
        LoadStage(0); //최초 씬 가져오기
    }

    void LoadStage(int stage)
    {
        SceneManager.LoadScene(stage); //해당 번호의 씬 가져오기
    }

    public void StartNewRun()
    {
        runState = new RunState(); // 회차 데이터 새로 생성

        foreach (string skillId in playerProgress.unlockedSkillPool)
        {
            if (runState.availableSkillPool.Contains(skillId))
                continue;

            runState.availableSkillPool.Add(skillId); // 영구 해금 스킬을 이번 회차에 복사
        }
    }
}