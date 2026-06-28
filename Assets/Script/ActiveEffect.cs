using System;

[Serializable]
public class ActiveEffect
{
    public EffectData effectData;         // 원본 효과 데이터
    public string sourceId;               // 어떤 장비/스킬에서 왔는지
    public float remainingDuration;       // 남은 지속 시간 
    public int stackCount = 1;            // 중첩 수
    public bool isExpired = false;        // 만료 여부
    public bool isPermanent = false;      // 무한지속여부

    public ActiveEffect(EffectData effectData, string sourceId,
                        float remainingDuration, int stackCount,
                        bool isExpired ,bool isPermanent )
    {
        this.effectData = effectData;
        this.sourceId = sourceId;
        this.remainingDuration = remainingDuration;
        this.stackCount = stackCount;
        this.isExpired = isExpired;
        this.isPermanent = isPermanent;
    }


    // TODO:
    // - 버프/디버프 시작 시 초기화할 값 넣기
    // - 필요하면 casterId / targetId 추가
}
