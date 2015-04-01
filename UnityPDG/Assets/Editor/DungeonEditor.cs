using UnityEngine;
using UnityEditor;
using System.Collections;

public class DungeonEditor :  EditorWindow{

    //public  DungeonGenerator dungeonGeneratorPrefab;
    public DungeonGenerator dungeonGeneratorInstance;

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
        Event e = Event.current;

        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        dungeonName = EditorGUILayout.TextField("Dungeon Name", dungeonName);
        EditorGUILayout.BeginHorizontal();
        dungeonGeneratorInstance = (DungeonGenerator)EditorGUILayout.ObjectField("Dungeon Generator", dungeonGeneratorInstance, typeof(DungeonGenerator), false);
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Generate a Dungeon"))
        {
            dungeonGeneratorInstance.CreateDungeon(dungeonName);
        }
	}
}
