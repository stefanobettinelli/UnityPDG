﻿using UnityEngine;
using UnityEditor;
using System.Collections;

public class DungeonEditor :  EditorWindow{

    private static DungeonGenerator dungeonGeneratorInstance;
    private static Dungeon dungeonInstance;

    string dungeonName = "";
    public int minWidth;
    public int minHeight;
    public int maxWidth;
    public int maxHeight;


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
        dungeonGeneratorInstance = (DungeonGenerator)EditorGUILayout.ObjectField("Dungeon Generator", dungeonGeneratorInstance, typeof(DungeonGenerator), false);
        minWidth = EditorGUILayout.IntField("Min Room Width:", minWidth);
        maxWidth = EditorGUILayout.IntField("Max Room Width:", maxWidth);
        minHeight = EditorGUILayout.IntField("Min Room Height:", minHeight);
        maxHeight = EditorGUILayout.IntField("Max Room Height:", maxHeight);
        if (GUILayout.Button("Generate a Dungeon"))
        {
            dungeonInstance = dungeonGeneratorInstance.CreateDungeon(dungeonName, minWidth, maxWidth, minHeight, maxHeight);        
        }
        if (GUILayout.Button("Destroy Dungeon"))
        {
            DestroyImmediate(dungeonInstance.gameObject);
        }
	}
}
