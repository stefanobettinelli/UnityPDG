 using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Dungeon : MonoBehaviour {

	public IntVector2 size;
	public DungeonCell dungeonCellPrefab;

    //per il momento provo a generare una singola stanza
    private DungeonRoom aRoom;

	private int _minRoomWidth;
	private int _maxRoomWidth;
	private int _minRoomHeight;
	private int _maxRoomHeight;

    public int MinRoomWidth { get { return _minRoomWidth; } set { _minRoomWidth = value; } }
    public int MaxRoomWidth { get { return _maxRoomWidth; } set { _maxRoomWidth = value; } }
    public int MinRoomHeight { get { return _minRoomHeight; } set { _minRoomHeight = value; } }
    public int MaxRoomHeight { get { return _maxRoomHeight; } set { _maxRoomHeight = value; } }


    private string dungeonName;
    private List<DungeonCell> activeCells = new List<DungeonCell>();
	//private DungeonCell[,] cells; //matrice di celle

    public string DungeonName
    {
        get
        {
            return dungeonName;
        }
        set
        {
            dungeonName = value;
        }
    }

	public IntVector2 RandomCoordinates{
		get{
			return new IntVector2(Random.Range(0,size.x), Random.Range(0,size.z));
		}
	}

	public bool ContainsCoordinates(IntVector2 coordinates){
		return (coordinates.x >= 0 && coordinates.x < size.x) && (coordinates.z>= 0 && coordinates.z < size.z); 
	}

	void Start(){
		//fa partire il dungeon dall'origine
		transform.position = new Vector3(0f,0f,0f);
	}

	/*public DungeonCell GetCell(IntVector2 coordinates){
		return cells[coordinates.x, coordinates.z];
	}*/

    public void Generate(int minWidth, int maxWidth, int minHeight, int maxHeight)
    {
        Debug.Log("Generating dungeon...size ranges: " + minWidth + " " + maxWidth + " " + minHeight + " " + maxHeight +" ");
		CreateRandomDungeonRoom(minWidth, maxWidth, minHeight, maxHeight);
        Debug.Log("Generation complete");
	}

	//crea una stanza con altezza e larghezza casuali
    public void CreateRandomDungeonRoom(int minWidth, int maxWidth, int minHeight, int maxHeight)
    {
        aRoom = new DungeonRoom();
        aRoom.generateRoomSize(minWidth, maxWidth, minHeight, maxHeight);
        Debug.Log(aRoom);

        for (int x = 0; x < aRoom.Data.Width; x++)
        {
            for (int z = 0; z < aRoom.Data.Height; z++)
            {
                activeCells.Add(CreateCell(new IntVector2(x, z)));
            }
        }
        foreach(DungeonCell cell in activeCells){
            //Debug.Log(cell.ToString());
        }
	}

	public DungeonCell CreateCell(IntVector2 coordinates){
		DungeonCell newDungeonCell = Instantiate(dungeonCellPrefab) as DungeonCell;
		//cells[coordinates.x,coordinates.z] = newDungeonCell;
		newDungeonCell.name = "Dungeon Cell " + coordinates.x + ", " + coordinates.z;
        newDungeonCell.Coordinates = coordinates;
		newDungeonCell.transform.parent = transform; //fa diventare tutte le celle generate figlie del game object Dungeon
		newDungeonCell.transform.localPosition = new Vector3(coordinates.x + 0.5f, 0f, coordinates.z + 0.5f);
        return newDungeonCell;
	}

}
