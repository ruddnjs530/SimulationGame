using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public enum TileType
{
    Empty,
    White,
    Green,
    Red
}

public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem gridSystemScript;
    public GridLayout gridLayout;
    [SerializeField]
    private Tilemap mainTilemap;
    [SerializeField]
    public Tilemap subTileMap;

    public Building buildingScript = null;
    private BoundsInt prevArea;
    public Dictionary<TileType, TileBase> tileBases = new Dictionary<TileType, TileBase>();

    private GameObject target;
    private RaycastHit2D mHit;
    private string buildingID; // 건물 UI가 나온 후 클릭한 건물이 UI가 나온 건물과 같은지 판별하기 위한 변수
    private Building prevBuildingScript = null; // 이전에 누른 건물을 저장하기 위한 변수

    public Button[] button;

    Dictionary<string, Tilemap> tilemaps = new Dictionary<string, Tilemap>();
    [SerializeField] string filename = "tilemapData.json";
    Dictionary<TileBase, BuildingObjectBase> tileBaseToBuildingObject = new Dictionary<TileBase, BuildingObjectBase>(); // 저장용
    Dictionary<string, TileBase> guidToTileBase = new Dictionary<string, TileBase>(); // 로딩용

    private void Awake()
    {
        gridSystemScript = this;
        string tilePath = @"Tiles/";
        tileBases.Add(TileType.Empty, null);
        tileBases.Add(TileType.White, Resources.Load<TileBase>(tilePath + "white"));
        tileBases.Add(TileType.Green, Resources.Load<TileBase>(tilePath + "green"));
        tileBases.Add(TileType.Red, Resources.Load<TileBase>(tilePath + "red"));
    }

    private void Start()
    {
        InitTileMap();
        InitTileReferences();
        if (GameManager.GetGameManager().PlayerData.buildingNum == 0)
        {
            OnSave();
        }
        OnLoad();
    }
    // Update is called once per frame
    void Update()
    {
        Remove();
        MouseClickAndInstall();
        buttonUITrunOff();
    }

    private void MouseClickAndInstall()
    {
        if (EventSystem.current.IsPointerOverGameObject() && !GameManager.GetGameManager().notInstalled) return;

        if (Input.GetMouseButtonDown(0))
        {
            target = GetClickedObject();

            if (target == null) 
            {
                if (GameManager.GetGameManager().isBuildingUIOn)
                {
                    buildingScript.HidePopupUI(); // 빈 곳을 클릭하면 UI가 꺼지게함
                } 
                return;
            }

            buildingScript = target.GetComponent<Building>();

            if (GameManager.GetGameManager().notInstalled)
            {
                if (buildingID == buildingScript.BuildingData.id) GameManager.GetGameManager().isSameBuilding = true;
                else GameManager.GetGameManager().isSameBuilding = false;
                return;
            }

            if (target.CompareTag("Building")) 
            {
                if (GameManager.GetGameManager().isBuildingUIOn) // isBuildingUIOn은 UI가 켜질 때 true가 됨
                {
                    if (buildingID != buildingScript.BuildingData.id) // UI가 켜질 때 눌렀던 건물과 UI가 켜지고 난 후에 누른 건물의 id가 다르다면
                    {
                        if (prevBuildingScript != null)
                        {
                            prevBuildingScript.HidePopupUI(); // 켜진 UI를 끔
                        }
                        buildingScript.ShowPopupUI(); // 누른 건물의 UI를 킴
                        buildingID = buildingScript.BuildingData.id; // 비교를 위해 해당 건물의 id를 받아둠
                    }
                    else // UI가 켜질 때 눌렀던 건물과 UI가 켜지고 난 후에 누른 건물의 id가 같다면 (움직일 수 있게 됨)
                    {
                        buildingScript.HidePopupUI(); // UI를 끔
                        buildingScript.BuildingData.isPlaced = false; // 설치를 취소함
                        ReSetTile(); // 타일을 재설정 해줌
                    }
                }
                else // UI가 켜지지 않은 상태, 처음 상태 
                {
                    buildingScript.ShowPopupUI(); // 빌딩 태그를 갖고 있는 건물을 클릭했으므로 건물 UI를 켜줌
                    buildingID = buildingScript.BuildingData.id; // 비교를 위해 해당 건물의 id를 받아둠
                    prevBuildingScript = buildingScript;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (target == null) return;
            if (Installable() && !GameManager.GetGameManager().isBuildingUIOn) // 설치 됐을 때
            {
                Install();
                buildingID = null;
                prevBuildingScript = null;
                GameManager.GetGameManager().notInstalled = false;
            }
            else if (!Installable() && !GameManager.GetGameManager().isBuildingUIOn) // 설치 못했을 때
            {
                GameManager.GetGameManager().notInstalled = true; // 설치 안됨
            }                      
            target = null;
        }
    }

    private GameObject GetClickedObject()
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mHit = Physics2D.Raycast(worldPoint, Vector2.zero, 10.0f);

        if (mHit.collider != null)
        {
            target = mHit.collider.gameObject;
        }
       // Debug.Log(target);
        return target;
    }

    private void buttonUITrunOff()
    {
        if (GameManager.GetGameManager().notInstalled)
        {
            for (int i = 0; i < 3; i++)
            {
                button[i].enabled = false;
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                button[i].enabled = true;
            }
        }
    }

    private void Remove()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            RemoveArea(buildingScript.BuildingData.area);
            Destroy(buildingScript.gameObject);
        }
    }
   
    // 건물 관련 함수
    public bool CanTakeArea(BoundsInt area)
    {
        TileBase[] baseArray = GetTilesArea(area, mainTilemap);
        foreach (var b in baseArray)
        {
            if (b != tileBases[TileType.White])
            {
                return false;
            }
        }
        return true;
    }

    public void InstallArea(BoundsInt area)
    {
        ChangeTiles(area, TileType.Empty, subTileMap);
        ChangeTiles(area, TileType.Green, mainTilemap);
    }

    public void RemoveArea(BoundsInt area)
    {
        ChangeTiles(area, TileType.White, mainTilemap);
    }

    public void InitializeBuliding(Building building)
    {
        buildingScript.HidePopupUI(); // 원래 켜져 있던 건물 UI를 끔
        buildingScript = building;
        buildingID = buildingScript.BuildingData.id;
        ShowBuildingArea();
    }

    public bool Installable() // 설치 할 수 있는지 판단
    {
        Vector3Int positionInt = gridLayout.LocalToCell(buildingScript.transform.position);
        BoundsInt areaTemp = buildingScript.BuildingData.area;
        areaTemp.position = positionInt;

        if (CanTakeArea(areaTemp))
        {
            return true;
        }
        return false;
    }

    public void Install() // 설치할 때 메인타일맵을 초록색으로, 서브타일맵은 빈공간으로 만듦.
    {
        Vector3Int positionInt = gridLayout.LocalToCell(buildingScript.transform.position);
        BoundsInt areaTemp = buildingScript.BuildingData.area;
        areaTemp.position = positionInt;
        buildingScript.buildingData.isPlaced = true;
        InstallArea(areaTemp);
        GameManager.GetGameManager().notInstalled = false;
    }

    private void UnableToInstall() // 설치 불가능 할 때 범위표현
    {
        Vector3Int positionInt = gridLayout.LocalToCell(buildingScript.transform.position);
        BoundsInt areaTemp = buildingScript.BuildingData.area;
        areaTemp.position = positionInt;
        buildingScript.buildingData.isPlaced = false;
        ChangeTiles(areaTemp, TileType.Empty, subTileMap);
        ChangeTiles(areaTemp, TileType.Red, mainTilemap);
    }

    // 타일 관련 함수
    private void ClearSubTileMap()
    {
        TileBase[] prevAreaInSubTileMap = new TileBase[prevArea.size.x * prevArea.size.y * prevArea.size.z];
        FillTiles(prevAreaInSubTileMap, TileType.Empty);
        subTileMap.SetTilesBlock(prevArea, prevAreaInSubTileMap);
    }

    public void ShowBuildingArea() // 건물을 이동시킬 때 범위와 설치 가능성을 보여주는 함수, 메인타일맵이 하얀색이면 서브타일맵을 초록색으로 보여줌.
    {
        ClearSubTileMap();
        buildingScript.BuildingData.area.position = gridLayout.WorldToCell(buildingScript.gameObject.transform.position); // 건물위치저장
        BoundsInt buildingArea = buildingScript.BuildingData.area;

        TileBase[] baseArray = GetTilesArea(buildingArea, mainTilemap);

        int size = baseArray.Length;
        TileBase[] tileArray = new TileBase[size];

        for (int i = 0; i < baseArray.Length; i++)
        {
            if (baseArray[i] == tileBases[TileType.White]) // 메인타일맵이 하얀색
            {
                tileArray[i] = tileBases[TileType.Green]; // 서브타일맵을 초록색으로
            }
            else
            {
                FillTiles(tileArray, TileType.Red); // 서브타일맵을 빨간색으로
                break;
            }
        }
        subTileMap.SetTilesBlock(buildingArea, tileArray); // tileArray에 색이 담겨져 있고 buildinArea를 tileArray로 설정하는 것임
        prevArea = buildingArea;
    }

    public TileBase[] GetTilesArea(BoundsInt area, Tilemap ttilemap)
    {
        TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;

        foreach (var v in area.allPositionsWithin)
        {
            Vector3Int pos = new Vector3Int(v.x, v.y, 0);
            array[counter] = ttilemap.GetTile(pos);
            counter++;
        }

        return array;
    }

    public void ChangeTiles(BoundsInt area, TileType type, Tilemap ttilemap)
    {
        int size = area.size.x * area.size.y * area.size.z;
        TileBase[] tileArray = new TileBase[size];
        FillTiles(tileArray, type);
        ttilemap.SetTilesBlock(area, tileArray);
    }

    public void FillTiles(TileBase[] arr, TileType type)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = tileBases[type];
        }
    }

    public void ReSetTile()
    {
        BoundsInt areaTemp = buildingScript.BuildingData.area;
        TileBase[] baseArray = GetTilesArea(areaTemp, subTileMap);
        TileBase[] mainTileBase = GetTilesArea(areaTemp, mainTilemap);
        for (int i = 0; i < baseArray.Length; i++)
        {
            if (baseArray[i] == tileBases[TileType.Empty] && mainTileBase[i] == tileBases[TileType.Green]) // 설치 됐다면 메인타일맵은 초록색이고 서브타일맵은 빈공간임.
            {
                RemoveArea(areaTemp);
            }
            else if (mainTileBase[i] == tileBases[TileType.Red] && baseArray[i] == tileBases[TileType.Empty]) // 설치가 불가능하다면 메인타일맵은 빨간색이고 서브타일맵은 빈공간임.
            {
                ChangeTiles(areaTemp, TileType.Empty, subTileMap);
                ChangeTiles(areaTemp, TileType.Empty, mainTilemap);
            }
        }
    }

    //public void SaveTileMap()
    //{
    //    BoundsInt bounds = mainTilemap.cellBounds;

    //    MapData mapData = new MapData();

    //    for (int x = bounds.min.x; x < bounds.max.x; x++)
    //    {
    //        for (int y = bounds.min.y; y < bounds.max.y; y++)
    //        {
    //            TileBase temp = mainTilemap.GetTile(new Vector3Int(x, y, 0));
    //            CTile tempTile = tiles.Find(t => t.tile == temp);

    //            if (tempTile != null) //(temp != null)
    //            {
    //                //mapData.tiles.Add(temp);
    //                mapData.tiles.Add(tempTile.id);
    //                mapData.tilePoses.Add(new Vector3Int(x, y, 0));
    //            }
    //        }
    //    }

    //    string json = JsonUtility.ToJson(mapData);
    //    File.WriteAllText(Application.dataPath + "/testMap.json", json);

    //    BoundsInt bounds2 = subTileMap.cellBounds;

    //    MapData mapData2 = new MapData();

    //    for (int x = bounds2.min.x; x < bounds2.max.x; x++)
    //    {
    //        for (int y = bounds2.min.y; y < bounds2.max.y; y++)
    //        {
    //            TileBase temp2 = subTileMap.GetTile(new Vector3Int(x, y, 0));
    //            CTile tempTile = tiles.Find(t => t.tile == temp2);

    //            if (tempTile != null) //(tempTile != null)
    //            {
    //                mapData2.tiles.Add(tempTile.id);
    //                mapData2.tilePoses.Add(new Vector3Int(x, y, 0));
    //            }
    //        }
    //    }

    //    string json2 = JsonUtility.ToJson(mapData2);
    //    File.WriteAllText(Application.dataPath + "/testMap2.json", json2);
    //}

    //public void LoadMap()
    //{
    //    string json = File.ReadAllText(Application.dataPath + "/testMap.json");
    //    MapData data = JsonUtility.FromJson<MapData>(json);

    //    mainTilemap.ClearAllTiles();

    //    for (int i = 0; i < data.tilePoses.Count; i++)
    //    {
    //        //mainTilemap.SetTile(data.tilePoses[i], data.tiles[i]);
    //        mainTilemap.SetTile(data.tilePoses[i], tiles.Find(t => t.name == data.tiles[i]).tile);
    //    }

    //    string json2 = File.ReadAllText(Application.dataPath + "/testMap2.json");
    //    MapData data2 = JsonUtility.FromJson<MapData>(json2);

    //    subTileMap.ClearAllTiles();

    //    for (int i = 0; i < data2.tilePoses.Count; i++)
    //    {
    //        //subTileMap.SetTile(data2.tilePoses[i], data2.tiles[i]);
    //        subTileMap.SetTile(data.tilePoses[i], tiles.Find(t => t.name == data.tiles[i]).tile);
    //    }
    //}

    private void InitTileReferences()
    {
        BuildingObjectBase[] buildables = Resources.LoadAll<BuildingObjectBase>("Scriptables/Buildables");
        
        foreach(BuildingObjectBase buildable in buildables)
        {
            if(!tileBaseToBuildingObject.ContainsKey(buildable.TileBase))
            {
                tileBaseToBuildingObject.Add(buildable.TileBase, buildable);
                guidToTileBase.Add(buildable.name, buildable.TileBase);
            }
            else
            {
                Debug.LogError("TileBase " + buildable.TileBase.name + " is already in use by " + tileBaseToBuildingObject[buildable.TileBase].name);
            }
        }
    }

    public void InitTileMap()
    {
        // 씬에 있는 모든 타일맵을 받고
        // dictionary에 적음
        Tilemap[] maps = FindObjectsOfType<Tilemap>();

        foreach (var map in maps)
        {
            tilemaps.Add(map.name, map);
        }
    }

    public void OnSave()
    {
        List<TileMapData> data = new List<TileMapData>();

        foreach (var mapObj in tilemaps)
        {      
            TileMapData mapData = new TileMapData();
            mapData.key = mapObj.Key;

            BoundsInt boundsForThisMap = mapObj.Value.cellBounds;
            for (int x = boundsForThisMap.xMin; x < boundsForThisMap.xMax; x++)
            {
                for (int y = boundsForThisMap.yMin; y < boundsForThisMap.yMax; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    TileBase tile = mapObj.Value.GetTile(pos);
                    if (tile != null && tileBaseToBuildingObject.ContainsKey(tile))
                    {
                        string guid = tileBaseToBuildingObject[tile].name;
                        TileInfo ti = new TileInfo(pos, guid);
                        mapData.tiles.Add(ti);
                    }
                }
            }

            data.Add(mapData);
        }

        FileHandler.SaveToJSON<TileMapData>(data, filename);
    }

    public void OnLoad()
    {
        List<TileMapData> data = FileHandler.ReadFromJSON<TileMapData>(filename);

        foreach (var mapData in data)
        {
            if (!tilemaps.ContainsKey(mapData.key))
            {
               // Debug.Log(mapData.key);
                Debug.LogError("Found saved data for tilemap called " + mapData.key + " but tilemaps does not exist.");
                continue;
            }

            var map = tilemaps[mapData.key];

            map.ClearAllTiles();
            if (mapData.tiles != null && mapData.tiles.Count > 0)
            {
                foreach (var tile in mapData.tiles)
                {
                    if (guidToTileBase.ContainsKey(tile.guidForBuildable))
                    {
                        map.SetTile(tile.position, guidToTileBase[tile.guidForBuildable]);
                    }
                    else
                    {
                        Debug.LogError("Reference " + tile.guidForBuildable + " could not be found.");
                    }
                }
            }
        }
    }
}

[Serializable]
public class TileMapData
{
    public string key; // dictionary에 쓰일 key
    public List<TileInfo> tiles = new List<TileInfo>();
}

[Serializable]
public class TileInfo
{
    public string guidForBuildable;
    public Vector3Int position;
    public TileInfo(Vector3Int pos, string guid)
    {
        position = pos;
        guidForBuildable = guid;
    }
}
