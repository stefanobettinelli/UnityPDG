using UnityEngine;
using System.Collections;

public class DungeonGenerator : MonoBehaviour, DungeonGeneratorInterface {

	public Dungeon dungeonPrefab;
	private Dungeon _dungeonInstance;

    public void CreateDungeon(string dungeonName, int minWidth, int maxWidth, int minHeight, int maxHeight)
    {
		_dungeonInstance = Instantiate(dungeonPrefab) as Dungeon;
        _dungeonInstance.transform.localPosition = new Vector3(0,0,0);
        _dungeonInstance.DungeonName = dungeonName;        
		_dungeonInstance.Generate(minWidth, maxWidth, minHeight, maxHeight);
	}	
}
