using UnityEngine;

[CreateAssetMenu(fileName = "NewHero", menuName = "Shop/Hero")]
public class HeroData : ScriptableObject
{
    public int itemID; // Thêm dòng này
    public string heroName;
    public int price;
    public Sprite icon;
    public GameObject heroPrefab;
}