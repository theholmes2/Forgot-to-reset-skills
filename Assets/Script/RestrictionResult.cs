using System;

[Serializable]
public class RestrictionResult
{
    public bool isAllowed = true;         // 최종 허용 여부
    public bool isBlocked = false;        // 완전 차단 여부
    public bool hasPenalty = false;       // 패널티 적용 여부

    public float penaltyMultiplier = 1f;  // 패널티 배율
    public string reasonKey;              // 실패/패널티 사유 키
}
