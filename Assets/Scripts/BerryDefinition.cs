// BerryDefinition.cs
using UnityEngine;

[CreateAssetMenu(menuName="Berries/Berry Definition")]
public class BerryDefinition : ScriptableObject
{
    public string displayName;       
    public Sprite sprite;            
    public int minBerries;
    public int maxBerries;
    public float regrowTimeMin; 
    public float regrowTimeMax; 
    public Color tint; 
}
