using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonRoom: MonoBehaviour {

    public DungeonCell dungeonCellPrefab;
    public WallUnit wallPrefab;

    private RoomData data;
    public List<DungeonCell> activeCells = new List<DungeonCell>();

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

    private WallUnit InstanciateWall(IntVector2 wallDirection, Direction direction, DungeonCell cell, string wallType)
    {
        WallUnit aWall = Instantiate(wallPrefab) as WallUnit;
        aWall.name = wallType;
        aWall.transform.parent = cell.transform;
        aWall.transform.localPosition = new Vector3(wallDirection.x, 0.5f, wallDirection.z);
        aWall.transform.localRotation = direction.ToRotation();
        return aWall;

    }


    //crea una mattonella del pavimento nelle coordinate "coordinates"
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

    //crea il pavimento intero invece di piccole mattonelle, usato per questioni di efficenza date dalla reduzione del carico di mesh da renderizzare
    private void CreateBaseFloor(IntVector2 coordinates, int width, int height)
    {
        DungeonCell roomFloor = Instantiate(dungeonCellPrefab) as DungeonCell;
        //cells[coordinates.x,coordinates.z] = newDungeonCell;
        roomFloor.name = "DungeonRoomFloor_origin";
        roomFloor.Coordinates = coordinates;
        roomFloor.transform.parent = transform; //fa diventare tutte le celle generate figlie del game object Dungeon
        roomFloor.transform.localPosition = new Vector3(coordinates.x + 0.5f, 0f, coordinates.z + 0.5f);
        roomFloor.transform.localScale = new Vector3(width, 0, height);
        //roomFloor.transform.GetChild(0).transform.localScale = new Vector3(width, 0, height);
        //dato che lo scaling in unity parte dal centro devo poi traslare in avanti e sopra della metà + 0.5f (0.5 perché la singola mattonella altrimenti non combacia con lunità di unity)
        roomFloor.transform.position = new Vector3(roomFloor.transform.position.x + roomFloor.transform.localScale.x / 2 - 0.5f, 0, roomFloor.transform.position.z + roomFloor.transform.localScale.z / 2 - 0.5f);        
    }

    //i parametri sono i valori di shit usati nell'algoritmo che sistema le stanze in modo che non ci siano sovrapposizioni in Dungeon.cs
    public void moveRoom(int minShitValueX, int minShitValueZ)
    {
        int actualX = data.Origin.x;
        int actualZ = data.Origin.z;
        data.Origin = new IntVector2(actualX + minShitValueX, actualZ + minShitValueZ);
        data.Center = new IntVector2((int)Mathf.Floor(data.Width / 2) + data.Origin.x, (int)Mathf.Floor(data.Height / 2) + data.Origin.z);
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
                aCell.transform.GetChild(0).GetComponent<Renderer>().enabled = false;
                activeCells.Add(aCell);
                //ogni volta che si crea una cella se questa fa parte del perimetro creo una unità muro
                //il controllo che sia nel perimetro viene effettuato nella funzione stessa
                CreateWall(x, z, Data.Width, Data.Height, aCell);
            }
        }
        CreateBaseFloor(new IntVector2(0, 0), Data.Width, Data.Height);//costruisce un quad unico delle dimensioni passate, piccola ottimizzazione in modo da non avere wxh celle  
        return activeCells;
    }

    //questa funzione oltre ad istanziare un game object muro per un cella, salva anche il riferimento al
    //muro nella cella in modo che in fase di creazine dei corridoi si riescano a rimuovere le celle di muro
    //per creare i collegamenti
    private void CreateWall(int x, int z, int width, int height, DungeonCell cell)
    {
        if (z == 0 && x >= 0 && x < width)
        {
            WallUnit aWall = InstanciateWall(Directions.directionVectors[(int)Direction.South], Direction.South, cell,"south wall");
            cell.addWallRefenceToCell(aWall,"south");
        }
        if (z == height - 1 && x >= 0 && x < width)
        {
            WallUnit aWall = InstanciateWall(Directions.directionVectors[(int)Direction.North], Direction.North, cell,"north wall");
            cell.addWallRefenceToCell(aWall, "north");
        }
        if (x == 0 && z >= 0 && z < height)
        {
            WallUnit aWall = InstanciateWall(Directions.directionVectors[(int)Direction.West], Direction.West, cell,"west wall");
            cell.addWallRefenceToCell(aWall, "west");
        }
        if (x == width - 1 && z >= 0 && z < height)
        {
            WallUnit aWall = InstanciateWall(Directions.directionVectors[(int)Direction.East], Direction.East, cell,"east wall");
            cell.addWallRefenceToCell(aWall, "east");
        }
    }

    public override string ToString()
    {
        string str = " " + data;
        return str;
    }

}
