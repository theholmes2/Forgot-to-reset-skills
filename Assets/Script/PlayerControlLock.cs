using System.Collections.Generic;
using UnityEngine;

public class PlayerControlLock : MonoBehaviour
{
    private HashSet<string> lockKeys = new HashSet<string>(); // 잠금 이유 목록

    public bool IsLocked
    {
        get { return lockKeys.Count > 0; } // 하나라도 잠겨있으면 조작 불가
    }

    public void Lock(string key)
    {
        lockKeys.Add(key); // 같은 이유는 중복 추가 안 됨
    }

    public void Unlock(string key)
    {
        lockKeys.Remove(key); // 해당 이유의 잠금 해제
    }

    public void ClearAll()
    {
        lockKeys.Clear(); // 강제 전체 해제
    }
}