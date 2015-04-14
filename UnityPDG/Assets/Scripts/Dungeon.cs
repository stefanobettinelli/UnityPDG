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
        //prova per vedere come si comporta creando 3 cluster
        IntVector2 clusterStartPosition = new IntVector2(0,0);
        for (int i = 0; i < 1; i++)
        {
            clusterStartPosition = createCluster(minWidth, maxWidth, minHeight, maxHeight, clusterStartPosition);
        }
	}

    //crea un cluster di 3 stanze nello spazio...disposizione ancora da stabilire
    public IntVector2 createCluster(int minWidth, int maxWidth, int minHeight, int maxHeight, IntVector2 clusterStartPosition)
    {
        int maxX = 0;
        int minX = 0;
        int maxZ = 0;
        int minZ = 0;
        DungeonRoom[] cluster = new DungeonRoom[roomNumCluster];

        //creo le stanze come oggetti e calcolo le dimensioni del cluster di contenimento
        for (int i = 0; i < 2; i++)
        {
            cluster[i] = CreateRandomDungeonRoom(minWidth, maxWidth, minHeight, maxHeight);
            if (i == 0)
            {
                AllocateRoomInSpace(cluster[i], new IntVector2(0, 0),1,1);
                maxX = cluster[i].Data.Width; minX = 0;
                maxZ = cluster[i].Data.Height; minZ = 0;
            }
            else
            {
                int direction = Random.Range(0, 4);
                if (direction == 0)
                {
                    Debug.Log("DX");
                    AllocateRoomInSpace(cluster[i], new IntVector2(maxX, 0),1,1);
                    maxZ = Mathf.Max(maxZ, cluster[i].Data.Height);
                    maxX += cluster[i].Data.Width;
                }
                if (direction == 1) {
                    Debug.Log("DOWN");
                    AllocateRoomInSpace(cluster[i], new IntVector2(0,minZ),1,-1);
                    maxX = Mathf.Max(maxX, cluster[i].Data.Width);
                    minZ -= cluster[i].Data.Height;
                }
                if (direction == 2) {
                    Debug.Log("SX");
                    AllocateRoomInSpace(cluster[i],new IntVector2(minX,0),-1,1);
                    maxZ = Mathf.Max(maxZ, cluster[i].Data.Height);
                    minX -= cluster[i].Data.Width;
                }
                if (direction == 3) {
                    Debug.Log("UP");
                    AllocateRoomInSpace(cluster[i], new IntVector2(0, maxZ), 1, 1);
                    maxX = Mathf.Max(maxX, cluster[i].Data.Width);
                    maxZ += cluster[i].Data.Height;
                }
            }
        }
        return new IntVector2(0,0);//per ora...
    }

	//crea una stanza con altezza e larghezza casuali
    public DungeonRoom CreateRandomDungeonRoom(int minWidth, int maxWidth, int minHeight, int maxHeight)
    {
        //per il momento provo a generare una singola stanza
        DungeonRoom aRoom = new DungeonRoom();
        aRoom.generateRoomSize(minWidth, maxWidth, minHeight, maxHeight);
        return aRoom;
	}

    public void AllocateRoomInSpace(DungeonRoom aRoom, IntVector2 offset, int directionX, int directionZ)
    {
        for (int x = offset.x; Mathf.Abs(x - offset.x) < aRoom.Data.Width; x += directionX)
        {
            for (int z = offset.z; Mathf.Abs(z - offset.z) < aRoom.Data.Height; z += directionZ)
            {
                DungeonCell aCell = CreateCell((new IntVector2(x, z)));
                activeCells.Add(aCell);
                //ogni volta che si crea una cella se questa fa parte del perimetro creo una unità muro
                //il controllo che sia nel perimetro viene effettuato nella funzione stessa
                CreateWall(x, z, aRoom.Data.Width, aRoom.Data.Height, aCell);
            }
        }
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
