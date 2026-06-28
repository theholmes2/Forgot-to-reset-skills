
using System;
using System.Collections.Generic;

[Serializable]
public class PlayerProgress
{
    public List<string> unlockedAbilityIds = new();   // 특수 능력 (안 잃음)
    public List<string> achievementIds = new ();       // 업적 (안 잃음)
    public int rank;                           // 격(格) — 메타 진행
    public List<string> unlockedSkillPool = new();     // 실제로 사용할 수 있는 스킬 목록
    public List<string> unlockedSkillNodeIds = new(); //스킬트리에서 찍은 노드 기록
}
