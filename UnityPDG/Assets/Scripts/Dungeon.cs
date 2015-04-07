 using UnityEngine;
using System.Collections;

public class Dungeon : MonoBehaviour {

	public IntVector2 size;
	public DungeonCell dungeonCellPrefab;
	public int minRoomWidth;
	public int maxRoomWidth;
	public int minRoomHeight;
	public int maxRoomHeight;


    private string dungeonName;
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

	public void Generate(){

		CreateRandomDungeonRoom();

		/*cells = new DungeonCell[size.x,size.z];
		IntVector2 coordinates = RandomCoordinates;
		while(ContainsCoordinates(coordinates) && GetCell (coordinates) == null){
			Debug.Log("new coordinates:" + coordinates.x + " " + coordinates.z );
			CreateCell(coordinates);
			//uso del metodo extended Directions.RandomValue restituisce un tipo Direction a cui è possibile applicare ToIntVector2
			coordinates += Directions.RandomValue.ToIntVector2();
		}*/
	}

	//crea una stanza con altezza e larghezza casuali
	public void CreateRandomDungeonRoom(){
		//cells = new DungeonCell[size.x,size.z]; forse non serve
		//origine della stanza (confinata all'interno delle dimensioni del dungeon)
		//NB le stanze possono anche estendersi oltre size.x/y, invece l'orgine rimane confinata su quei due limiti
		IntVector2 roomOrigin = new IntVector2(Random.Range(0,size.x),Random.Range(0,size.z));
		int roomWidth = Random.Range(minRoomWidth, maxRoomWidth);
		int roomHeight = Random.Range(minRoomWidth, maxRoomWidth);
		for(int x = 0; x < roomWidth; x++){
			for(int z = 0; z < roomHeight; z++){
				CreateCell(new IntVector2(x + roomOrigin.x,z + roomOrigin.z));
			}
		}
	}

	public void CreateCell(IntVector2 coordinates){
		DungeonCell newDungeonCell = Instantiate(dungeonCellPrefab) as DungeonCell;
		//cells[coordinates.x,coordinates.z] = newDungeonCell;
		newDungeonCell.name = "Dungeon Cell " + coordinates.x + ", " + coordinates.z;
		newDungeonCell.transform.parent = transform; //fa diventare tutte le celle generate figlie del game object Dungeon
		newDungeonCell.transform.localPosition = new Vector3(coordinates.x + 0.5f, 0f, coordinates.z + 0.5f);
	}

}
