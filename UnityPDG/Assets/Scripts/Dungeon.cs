 using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Dungeon : MonoBehaviour {

	public IntVector2 size;
	public DungeonCell dungeonCellPrefab;
    public WallUnit wallPrefab;

	private int _minRoomWidth;
	private int _maxRoomWidth;
	private int _minRoomHeight;
	private int _maxRoomHeight;
    private string dungeonName;
    private const int roomNumCluster = 3; 
    private List<DungeonCell> activeCells = new List<DungeonCell>();

    private struct DungeonCluster
    {
        public DungeonRoom[] _cluster;
        public int _boundingBoxWidth;
        public int _boundingBoxHeight;

        public DungeonCluster(int boundingBoxWidth, int boundingBoxHeight, DungeonRoom[] cluster)
        {
            _cluster = cluster;
            _boundingBoxWidth = boundingBoxWidth;
            _boundingBoxHeight = boundingBoxHeight;
        }

        public override string ToString()
        {
            string str = "Bounding box size. Width: " + _boundingBoxWidth + ", Height: " + _boundingBoxHeight + "\n";
            for (int i = 0; i < roomNumCluster; i++)
            {
                str += " " + _cluster[i] + "\n" ;
            }
            return str;
        }
    };

    public int MinRoomWidth { get { return _minRoomWidth; } set { _minRoomWidth = value; } }
    public int MaxRoomWidth { get { return _maxRoomWidth; } set { _maxRoomWidth = value; } }
    public int MinRoomHeight { get { return _minRoomHeight; } set { _minRoomHeight = value; } }
    public int MaxRoomHeight { get { return _maxRoomHeight; } set { _maxRoomHeight = value; } }
    public string DungeonName{ get { return dungeonName; } set { dungeonName = value; } }

	public bool ContainsCoordinates(IntVector2 coordinates){
		return (coordinates.x >= 0 && coordinates.x < size.x) && (coordinates.z>= 0 && coordinates.z < size.z); 
	}

	/*public DungeonCell GetCell(IntVector2 coordinates){
		return cells[coordinates.x, coordinates.z];
	}*/

    public void Generate(int minWidth, int maxWidth, int minHeight, int maxHeight)
    {
        //Debug.Log("Generating dungeon...size ranges: " + minWidth + " " + maxWidth + " " + minHeight + " " + maxHeight +" ");
        createCluster(minWidth, maxWidth, minHeight, maxHeight);
        //Debug.Log("Generation complete");

        foreach (DungeonCell aCell in activeCells)
        {
            DestroyImmediate(aCell.gameObject);
        }
	}

    //crea un cluster di 3 stanze nello spazio...disposizione ancora da stabilire
    public DungeonRoom[] createCluster(int minWidth, int maxWidth, int minHeight, int maxHeight)
    {
        int boundingBoxWidht = 0;
        int boundingBoxHeight = 0;

        DungeonRoom[] cluster = new DungeonRoom[roomNumCluster];
        for (int i = 0; i < roomNumCluster; i++)
        {
            cluster[i] = CreateRandomDungeonRoom(minWidth, maxWidth, minHeight, maxHeight);
            boundingBoxWidht += cluster[i].Data.Width;
            boundingBoxHeight += cluster[i].Data.Height;
        }

        DungeonCluster aDungeonCluster = new DungeonCluster(boundingBoxWidht, boundingBoxHeight, cluster);
        Debug.Log(aDungeonCluster);
        return cluster;
    }

	//crea una stanza con altezza e larghezza casuali
    public DungeonRoom CreateRandomDungeonRoom(int minWidth, int maxWidth, int minHeight, int maxHeight)
    {
        //solo una prova
        IntVector2 offset = new IntVector2(Random.Range(1,50),Random.Range(1,50));
        //per il momento provo a generare una singola stanza
        DungeonRoom aRoom = new DungeonRoom();
        aRoom.generateRoomSize(minWidth, maxWidth, minHeight, maxHeight);

        for (int x = 0; x < aRoom.Data.Width; x++)
        {
            for (int z = 0; z < aRoom.Data.Height; z++)
            {
                DungeonCell aCell = CreateCell((new IntVector2(x, z)) + offset);
                activeCells.Add(aCell);
                //ogni volta che si crea una cella se questa fa parte del perimetro creo una unità muro
                //il controllo che sia nel perimetro viene effettuato nella funzione stessa
                CreateWall(x, z, aRoom.Data.Width, aRoom.Data.Height, aCell);
            }
        }
        return aRoom;
	}

    private void CreateWall(int x, int z, int width, int height, DungeonCell cell){
        if ( z == 0 && x >= 0 && x < width )
        {
            InstanciateWall(Directions.directionVectors[(int)Direction.South], Direction.South, cell);
        } 
        if( z == height-1 && x >= 0 && x < width )
        {
            InstanciateWall(Directions.directionVectors[(int)Direction.North], Direction.North, cell);
        } 
        if( x == 0 && z >= 0 && z < height )
        {
            InstanciateWall(Directions.directionVectors[(int)Direction.West], Direction.West, cell);
        }
        if ( x == width-1 && z >= 0 && z < height)
        {
            InstanciateWall(Directions.directionVectors[(int)Direction.East], Direction.East, cell);
        }
    }

    private void InstanciateWall(IntVector2 wallDirection, Direction direction ,DungeonCell cell)
    {
        WallUnit aWall = Instantiate(wallPrefab) as WallUnit;
        aWall.transform.parent = cell.transform;
        aWall.transform.localPosition = new Vector3(wallDirection.x,0.5f,wallDirection.z);
        aWall.transform.localRotation = direction.ToRotation();

    }

	private DungeonCell CreateCell(IntVector2 coordinates){
		DungeonCell newDungeonCell = Instantiate(dungeonCellPrefab) as DungeonCell;
		//cells[coordinates.x,coordinates.z] = newDungeonCell;
		newDungeonCell.name = "Dungeon Cell " + coordinates.x + ", " + coordinates.z;
        newDungeonCell.Coordinates = coordinates;
		newDungeonCell.transform.parent = transform; //fa diventare tutte le celle generate figlie del game object Dungeon
		newDungeonCell.transform.localPosition = new Vector3(coordinates.x + 0.5f, 0f, coordinates.z + 0.5f);
        return newDungeonCell;
	}

}
