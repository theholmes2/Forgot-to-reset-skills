using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    public string id;               // 내부 저장용 ID
    public string displayName;      // 실제 게임에 보이는 이름
    public ItemCategory category;   // 아이템 큰 분류
    public string description;      // 설명
    public Sprite icon;             // 아이콘

    public int maxStack = 1;        // 최대 소지 개수
    public int price = 0;           // 가격
}
