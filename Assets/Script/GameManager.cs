using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerProgress playerProgress;
    public RunState runState;

    public int titleSceneIndex = 0;
    public int mainStageSceneIndex = 1;

    private void Awake()
    {
        if (Instance == null) //인스턴스가 없다면 (최초)
        {
            Instance = this; //게임매니저 지정
            DontDestroyOnLoad(gameObject); //파괴불가

            playerProgress = SaveSystem.Load(); //영구 데이터 받아오기
            EnsureProgressData();
            StartNewRun(); // 회차 데이터 생성
        }
        else //이미 있다면
        {
            Destroy(gameObject); //게임매니저 제거(중복 제거용)
        }
    }
    public void OnPlayerDeath()
    {
        SaveSystem.Save(playerProgress); // 영구 데이터 저장
        StartNewRun(); // 회차 데이터 초기화 + 영구 스킬 복사
        SceneManager.LoadScene(mainStageSceneIndex); // 타이틀 말고 메인 스테이지로 회귀
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

    public void StartNewGame()
    {
        SaveSystem.DeleteSave(); // 기존 저장 삭제

        playerProgress = new PlayerProgress(); // 완전 새 영구 데이터
        StartNewRun(); // 1회차 데이터 생성

        SceneManager.LoadScene(mainStageSceneIndex); // 메인 게임 시작
    }

    public void ContinueGame()
    {
        playerProgress = SaveSystem.Load(); // 저장된 영구 데이터 불러오기
        EnsureProgressData();
        StartNewRun(); // 저장된 영구 데이터 기반으로 회차 시작

        SceneManager.LoadScene(mainStageSceneIndex); // 메인 게임 시작
    }

    private void EnsureProgressData()
    {
        playerProgress ??= new PlayerProgress();
        playerProgress.unlockedAbilityIds ??= new List<string>();
        playerProgress.achievementIds ??= new List<string>();
        playerProgress.unlockedSkillPool ??= new List<string>();
        playerProgress.unlockedSkillNodeIds ??= new List<string>();
        playerProgress.unlockedSkillTreeIds ??= new List<string>();
        playerProgress.receivedRewardIds ??= new List<string>();
    }

}
