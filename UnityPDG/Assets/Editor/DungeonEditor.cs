using UnityEngine;
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
    public int roomNum;
    public int minPadding;
    int minShitValue;


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
        GUILayout.Label("Dunegon Generator Parameter", EditorStyles.boldLabel);
        dungeonName = EditorGUILayout.TextField("Dungeon Name", dungeonName);
        dungeonGeneratorInstance = (DungeonGenerator)EditorGUILayout.ObjectField("Dungeon Generator", dungeonGeneratorInstance, typeof(DungeonGenerator), false);
        minWidth = EditorGUILayout.IntField("Min Room Width:", minWidth);
        maxWidth = EditorGUILayout.IntField("Max Room Width:", maxWidth);
        minHeight = EditorGUILayout.IntField("Min Room Height:", minHeight);
        maxHeight = EditorGUILayout.IntField("Max Room Height:", maxHeight);
        roomNum = EditorGUILayout.IntField("Room Number:", roomNum);
        minShitValue = EditorGUILayout.IntField("Min. Shift Value:", minShitValue);
        if (GUILayout.Button("Generate a Dungeon"))
        {
            dungeonInstance = dungeonGeneratorInstance.CreateDungeon(dungeonName, minWidth, maxWidth, minHeight, maxHeight, roomNum, minShitValue);        
        }
        if (GUILayout.Button("Destroy Dungeon"))
        {
            DestroyImmediate(dungeonInstance.gameObject);
            var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }
	}
}
