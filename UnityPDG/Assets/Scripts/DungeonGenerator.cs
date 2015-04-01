using UnityEngine;
using System.Collections;

public class DungeonGenerator : MonoBehaviour, DungeonGeneratorInterface {

	public Dungeon dungeonPrefab;
	private Dungeon dungeonInstance;

	public void CreateDungeon(string dungeonName){
		dungeonInstance = Instantiate(dungeonPrefab) as Dungeon;
        dungeonInstance.DungeonName = dungeonName;
		dungeonInstance.Generate();
	}	
}
