using UnityEngine;
using UnityEditor;
using System.Collections;

public class DungeonEditor :  EditorWindow{

    //public  DungeonGenerator dungeonGeneratorPrefab;
    public DungeonGenerator dungeonGeneratorInstance;

    string dungeonName = "";

	[MenuItem("Window/Dungeon Generator Editor")]
    static void  ShowWindow () 
    {

		EditorWindow.GetWindow(typeof(DungeonEditor));
	}

    void OnInspectorUpdate()
    {
        Repaint();
    }

	void OnGUI () 
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        dungeonName = EditorGUILayout.TextField("Dungeon Name", dungeonName);
        EditorGUILayout.BeginHorizontal();
        dungeonGeneratorInstance = (DungeonGenerator)EditorGUILayout.ObjectField("Dungeon Generator", dungeonGeneratorInstance, typeof(DungeonGenerator), false);
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Generate a Dungeon"))
        {
            dungeonGeneratorInstance.CreateDungeon(dungeonName);
        }
		if (GUILayout.Button("Clear Dungeon"))
		{
			//TODO...
		}
	}
}
