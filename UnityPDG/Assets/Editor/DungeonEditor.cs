using UnityEngine;
using UnityEditor;
using System.Collections;

public class DungeonEditor :  EditorWindow{
	[MenuItem ("Window/Dungeon Generator Editor")]

	public static void  ShowWindow () {
		EditorWindow.GetWindow(typeof(DungeonEditor));
	}

	void OnGUI () {
		// The actual window code goes here
	}
}
