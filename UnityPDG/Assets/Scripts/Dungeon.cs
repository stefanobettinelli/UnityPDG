using UnityEngine;
using System.Collections;

public class Dungeon : MonoBehaviour {

	public DungeonCell dungeonCellPrefab;

	private DungeonCell[,] cells; //matrice di celle

	void Start(){
		//fa partire il dungeon dall'origine
		transform.position = new Vector3(0f,0f,0f);
	}

	public void Generate(){
		cells = new DungeonCell[20,20];
		for(int x = 0; x < 20; x++){
			for(int z = 0; z < 20; z++){
				CreateCell(x, z);
			}
		}
	}

	public void CreateCell(int x, int z){
		DungeonCell newDungeonCell = Instantiate(dungeonCellPrefab) as DungeonCell;
		cells[x,z] = newDungeonCell;
		newDungeonCell.name = "Dungeon Cell " + x + ", " + z;
		newDungeonCell.transform.parent = transform; //fa diventare tutte le celle generate figlie del game object Dungeon
		newDungeonCell.transform.localPosition = new Vector3(x - 20 * 0.5f + 0.5f, 0f, z - 20 * 0.5f + 0.5f);
	}

}
