using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor( typeof(Example) ) ]
public class GeneratorInspector : Editor {

	public override void OnInspectorGUI(){
		DrawDefaultInspector();
		GUILayout.Button("Generate Dungeon");
	}

}
