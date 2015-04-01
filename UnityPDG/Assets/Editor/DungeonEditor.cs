using UnityEngine;
using UnityEditor;
using System.Collections;

public class DungeonEditor :  EditorWindow{

    public  DungeonGenerator dungeonGeneratorPrefab = null;
    private DungeonGenerator dungeonGeneratorInstance = null;

    string dungeonName = "";

    [MenuItem("Window/Dungeon Generator Editor")]
    static void Init()
    {
        //UnityEditor.EditorWindow window = GetWindow(typeof(EditorGUI.ObjectField));
    }

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
        dungeonGeneratorInstance = (DungeonGenerator) EditorGUILayout.ObjectField("Dungeon Generator", dungeonGeneratorPrefab, typeof(DungeonGenerator), false);
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Generate a Dungeon"))
        {
            dungeonGeneratorInstance.CreateDungeon(dungeonName);
        }
	}
}
