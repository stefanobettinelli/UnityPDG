using UnityEngine;
using System.Collections;

public class DungeonGenerator : MonoBehaviour {

	public Dungeon dungeonPrefab;
	private Dungeon dungeonInstance;

	// Use this for initialization
	void Start () {
		CreateDungeon();	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void CreateDungeon(){
		dungeonInstance = Instantiate(dungeonPrefab) as Dungeon;
		dungeonInstance.Generate();
	}	
}
