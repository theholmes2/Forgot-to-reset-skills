
using System;
using System.Collections.Generic;

[Serializable]
public class PlayerProgress
{
    public List<string> unlockedAbilityIds = new();   // 특수 능력 (안 잃음)
    public List<string> achievementIds = new ();       // 업적 (안 잃음)
    public int rank;                           // 격(格) — 메타 진행
    public List<string> unlockedSkillPool = new();     // 한번 해금한 스킬 풀

}
