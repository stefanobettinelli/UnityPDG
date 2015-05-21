using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;

public class DungeonEditor :  EditorWindow{
    
    [SerializeField] private static DungeonGenerator dungeonGeneratorInstance;
    private static Dungeon dungeonInstance;

    string dungeonName = "";
    public int minWidth;
    public int minHeight;
    public int maxWidth;
    public int maxHeight;
    public int roomNum;
    public int minPadding;
    public int minShitValue;
    public bool seedInput = false;
    public int seed = 0;
    public bool showRNG = true;
    public bool showCorridors = true;

    private bool dungeonGenerated = false;
    private string seedString = "";

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
        seedInput = EditorGUILayout.BeginToggleGroup("Input Seed?", seedInput);
        seed = EditorGUILayout.IntField("Seed:", seed);
        if (seedInput)
        {
            Random.seed = seed;
        }
        else
        {
            Random.seed = System.Environment.TickCount;
            seed = Random.seed;
        }
        EditorGUILayout.EndToggleGroup();
        showRNG = EditorGUILayout.Toggle("Show/Hide RNG graph", showRNG);
        showCorridors = EditorGUILayout.Toggle("Show/Hide Corridors", showCorridors);
        if (GUILayout.Button("Generate a Dungeon"))
        {
            dungeonInstance = dungeonGeneratorInstance.CreateDungeon(dungeonName, minWidth, maxWidth, minHeight, maxHeight, roomNum, minShitValue);            
            Debug.Log("Last generation seed: " + seed);
            dungeonGenerated = true;
        }        
        if (dungeonGenerated)
        {
            seedString = "" + seed;                       
            dungeonGenerated = false;
        }
        EditorGUILayout.LabelField("Last generation seed: ", seedString);        
        if (dungeonInstance)
        {
            dungeonInstance.showRNGGraph(showRNG);
            dungeonInstance.ToggleGizmos(showRNG);
            dungeonInstance.showCorridors(showCorridors);
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
