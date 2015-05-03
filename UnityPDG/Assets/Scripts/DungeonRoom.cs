using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonRoom: MonoBehaviour {

    public DungeonCell dungeonCellPrefab;
    public WallUnit wallPrefab;

    private RoomData data;
    private List<DungeonCell> activeCells = new List<DungeonCell>();

    public RoomData Data { get { return data; } set { data = value; } }

    public DungeonRoom()
    {
    }

    public DungeonRoom(int minWidth, int maxWidth, int minHeight, int maxHeight)
    {
        generateRoomSize(minWidth, maxWidth, minHeight, maxHeight);
    }

    public void generateRoom(DungeonRoom aRoom)
    {
        data = aRoom.Data;
    }

    public void generateRoomSize(int minWidth, int maxWidth, int minHeight, int maxHeight)
    {
        //Debug.Log("generate room size, ranges...: " + minWidth + " " + maxWidth + " " + minHeight + " " + maxHeight + " ");
        int aWidth = Random.Range(minWidth, maxWidth);
        int aHeight = Random.Range(minHeight, maxHeight);
        data = new RoomData(aWidth, aHeight);
    }

    private void InstanciateWall(IntVector2 wallDirection, Direction direction, DungeonCell cell)
    {
        WallUnit aWall = Instantiate(wallPrefab) as WallUnit;
        aWall.transform.parent = cell.transform;
        aWall.transform.localPosition = new Vector3(wallDirection.x, 0.5f, wallDirection.z);
        aWall.transform.localRotation = direction.ToRotation();

    }

    private DungeonCell CreateCell(IntVector2 coordinates)
    {
        DungeonCell newDungeonCell = Instantiate(dungeonCellPrefab) as DungeonCell;
        //cells[coordinates.x,coordinates.z] = newDungeonCell;
        newDungeonCell.name = "Dungeon Cell " + coordinates.x + ", " + coordinates.z;
        newDungeonCell.Coordinates = coordinates;
        newDungeonCell.transform.parent = transform; //fa diventare tutte le celle generate figlie del game object Dungeon
        newDungeonCell.transform.localPosition = new Vector3(coordinates.x + 0.5f, 0f, coordinates.z + 0.5f);
        return newDungeonCell;
    }

    //i parametri sono i valori di shit usati nell'algoritmo che sistema le stanze in modo che non ci siano sovrapposizioni in Dungeon.cs
    public void moveRoom(int minShitValueX, int minShitValueZ)
    {
        int actualX = data.Origin.x;
        int actualZ = data.Origin.z;
        data.Origin = new IntVector2(actualX + minShitValueX, actualZ + minShitValueZ);
        data.Center = new IntVector2((int)Mathf.Floor(data.Width / 2) + data.Origin.x, (int)Mathf.Floor(data.Width / 2) + data.Origin.z);
    }

    public int distance(DungeonRoom aRoom)
    {
        int c1 = Mathf.Abs(data.Center.x - aRoom.data.Center.x) + 1;
        int c2 = Mathf.Abs(data.Center.z - aRoom.data.Center.z) + 1;
        int distance = (int) Mathf.Round(Mathf.Sqrt(Mathf.Pow(c1, 2) + Mathf.Pow(c2, 2)));
        return distance;
    }

    public List<DungeonCell> AllocateRoomInSpace()
    {
        for (int x = 0; x < Data.Width; x++)
        {
            for (int z = 0; z < Data.Height; z++)
            {
                DungeonCell aCell = CreateCell((new IntVector2(x, z)));
                activeCells.Add(aCell);
                //ogni volta che si crea una cella se questa fa parte del perimetro creo una unità muro
                //il controllo che sia nel perimetro viene effettuato nella funzione stessa
                CreateWall(x, z, Data.Width, Data.Height, aCell);
            }
        }
        return activeCells;
    }

    private void CreateWall(int x, int z, int width, int height, DungeonCell cell)
    {
        if (z == 0 && x >= 0 && x < width)
        {
            InstanciateWall(Directions.directionVectors[(int)Direction.South], Direction.South, cell);
        }
        if (z == height - 1 && x >= 0 && x < width)
        {
            InstanciateWall(Directions.directionVectors[(int)Direction.North], Direction.North, cell);
        }
        if (x == 0 && z >= 0 && z < height)
        {
            InstanciateWall(Directions.directionVectors[(int)Direction.West], Direction.West, cell);
        }
        if (x == width - 1 && z >= 0 && z < height)
        {
            InstanciateWall(Directions.directionVectors[(int)Direction.East], Direction.East, cell);
        }
    }

    public override string ToString()
    {
        string str = " " + data;
        return str;
    }

}
