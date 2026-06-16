using System.IO;
using UnityEngine;

public static class SaveSystem 
{
 
    private static string SavePath => Path.Combine(Application.persistentDataPath, "playerProgress.json"); //저장위치

    public static void Save(PlayerProgress playerProgress)
    {
        string json = JsonUtility.ToJson(playerProgress, true);  //json 변환
        File.WriteAllText(SavePath, json); //저장
        Debug.Log("저장 완료: " + SavePath);
    }


    public static PlayerProgress Load()
    {
        if (!File.Exists(SavePath)) //json파일확인
        {
            Debug.Log("저장 파일 없음. 새 데이터 생성");
            return new PlayerProgress();
        }

        string json = File.ReadAllText(SavePath); //json파일읽기
        PlayerProgress data = JsonUtility.FromJson<PlayerProgress>(json); //데이터 가져옴

        if (data == null) //확인
            data = new PlayerProgress(); //없으면 생성

        return data; //데이터 반환 
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath)) //파일잇으면
        {
            File.Delete(SavePath); //파일삭제
            Debug.Log("저장 파일 삭제 완료");
        }
    }
}
