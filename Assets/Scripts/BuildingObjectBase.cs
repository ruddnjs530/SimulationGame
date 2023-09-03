using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType0
{
    Empty,
    White,
    Green,
    Red
}

[CreateAssetMenu(fileName = "Buildable", menuName = "BuildingObjects/Create Buildable")]
public class BuildingObjectBase : ScriptableObject
{
    [SerializeField] TileBase tileBase;
    [SerializeField] TileType0 TileType;

    public TileBase TileBase
    {
        get
        {
            return tileBase;
        }
    }
}