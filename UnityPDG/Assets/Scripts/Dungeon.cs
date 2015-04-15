using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Dungeon : MonoBehaviour {

	public IntVector2 size;
    public DungeonRoom dungeonRoomPrefab;
	public DungeonCell dungeonCellPrefab;
    public WallUnit wallPrefab;

	private int _minRoomWidth;
	private int _maxRoomWidth;
	private int _minRoomHeight;
	private int _maxRoomHeight;
    private string dungeonName;
    private List<DungeonCell> activeCells = new List<DungeonCell>();

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
        int bbWidth=0;
        int bbHeight=0;

        DungeonRoom first = new DungeonRoom(minWidth, maxWidth, minHeight, maxHeight);

        AllocateRoomInSpace(first);
        bbWidth = first.Data.Width;
        bbHeight = first.Data.Height;
        //Debug.Log(bbWidth);
        //Debug.Log(Mathf.Ceil(bbWidth/2.0f));
        //Debug.Log(bbHeight);
        //Debug.Log(Mathf.Ceil(bbHeight/2.0f));

        for (int i = 0; i < 3; i++)
		{
            DungeonRoom aRoom = new DungeonRoom(minWidth, maxWidth, minHeight, maxHeight);
            int direction = Random.Range(0,2);
            switch (direction)
	        {
                case 0:
                    aRoom.Data.Origin = new IntVector2(Random.Range((int)Mathf.Ceil(bbWidth/2.0f), (int)Mathf.Ceil(bbWidth/2.0f)*2+10), Random.Range(0,bbHeight));
                    AllocateRoomInSpace(aRoom);
                    bbWidth = aRoom.Data.Origin.x + aRoom.Data.Width;
                    bbHeight = Mathf.Max(bbHeight,aRoom.Data.Height);
                    break;
                case 1:
                    aRoom.Data.Origin = new IntVector2(Random.Range(0, bbWidth), Random.Range((int)Mathf.Ceil(bbHeight / 2.0f), (int)Mathf.Ceil(bbHeight / 2.0f) * 2 + 10));
                    AllocateRoomInSpace(aRoom);
                    bbWidth = Mathf.Max(bbWidth, aRoom.Data.Width);
                    bbHeight = aRoom.Data.Origin.x + aRoom.Data.Width;
                    break;
	        }
		 
		}



	}

    public void AllocateRoomInSpace(DungeonRoom aRoom)
    {

        for (int x = 0; x < aRoom.Data.Width; x++)
        {
            for (int z = 0; z < aRoom.Data.Height; z++)
            {
                DungeonCell aCell = CreateCell((new IntVector2(x, z)) + aRoom.Data.Origin);
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
