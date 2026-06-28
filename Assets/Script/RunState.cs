
using System;
using System.Collections.Generic;

[Serializable]
public class RunState
{
    public int currentStage = 1;
    public int level = 1;
    public int exp = 0;
    public int hp = 100, mp = 50;
    public List<string> inventory = new();        // 아이템 (잃음)
    public string equippedActiveSkillId = "";
    public string equippedPassiveSkillId = "";

    public List<string> availableSkillPool = new(); // 이번 회차에서 사용 가능한 스킬 ID
    public List<string> temporarySkillPool = new(); // 이번 회차 임시 스킬 ID


}
