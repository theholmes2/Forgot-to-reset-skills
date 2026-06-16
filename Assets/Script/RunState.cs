
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




}
