using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(SpawnField))]
public class SpawnFieldEditor : Editor {

    private List<bool> m_showWaves = new List<bool>();

    public void OnEnable() {

    }
    public override void OnInspectorGUI() {
        SpawnField spawnField = target as SpawnField;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Waves");
        if (GUILayout.Button("Add Wave", GUILayout.ExpandWidth(false))) {
            spawnField.SpawnWaves.Add(new List<Spawn>());
            m_showWaves.Add(false);
            GUI.changed = true;
        }
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < spawnField.SpawnWaves.Count; i++) {
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            m_showWaves[i] = EditorGUILayout.Foldout(m_showWaves[i], "Wave " + i);
            if (m_showWaves[i]) {
                EditorGUI.indentLevel++;

                if (GUILayout.Button("-", GUILayout.ExpandWidth(false))) {
                    spawnField.SpawnWaves.RemoveAt(i);
                    GUI.changed = true;
                }
                EditorGUILayout.EndHorizontal();

                for (int j = 0; j < spawnField.SpawnWaves[i].Count; j++) {
                    EditorGUI.indentLevel++;
                    spawnField.SpawnWaves[i][j].Prefab = EditorGUILayout.ObjectField("Enemy Prefab", null, typeof(GameObject), true) as GameObject;
                    spawnField.SpawnWaves[i][j].Count = EditorGUILayout.IntField("Spawn count", 1);

                    if (GUILayout.Button("-", GUILayout.ExpandWidth(false))) {
                        spawnField.SpawnWaves[i].RemoveAt(j);
                        GUI.changed = true;
                    }
                    EditorGUI.indentLevel--;
                }

                if (GUILayout.Button("Add Spawn", GUILayout.ExpandWidth(false))) {
                    spawnField.SpawnWaves[i].Add(new Spawn());
                    GUI.changed = true;
                }

                EditorGUI.indentLevel--;
            } else {
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }

        if (GUI.changed) {
            EditorUtility.SetDirty(spawnField);
        }
    }
}
