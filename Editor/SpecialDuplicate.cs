using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SpecialDuplicate : EditorWindow
{
    public GameObject parent;
    private Material _previewMaterial;

    // Default values for Transform
    public Vector3 inputPosition;
    public Vector3 inputRotation;
    public Vector3 inputScale = new Vector3(1, 1, 1);
    public int inputInstances;

    // Serializing Stuff
    SerializedObject _so;
    SerializedProperty _propParent;
    SerializedProperty _propPosition;
    SerializedProperty _propRotation;
    SerializedProperty _propScale;
    SerializedProperty _propInstances;

    [MenuItem("Tools/Special Duplicate %#D")]
    private static void Init()
    {
        GetWindow<SpecialDuplicate>();
    }

    private void OnEnable()
    {
        Selection.selectionChanged += Repaint;
        SceneView.duringSceneGui += DuringSceneGUI;

        _previewMaterial = new Material(Shader.Find("Shader Graphs/PreviewMatShader"));

        _so = new SerializedObject(this);
        _propParent = _so.FindProperty("parent");
        _propPosition = _so.FindProperty("inputPosition");
        _propRotation = _so.FindProperty("inputRotation");
        _propScale = _so.FindProperty("inputScale");
        _propInstances = _so.FindProperty("inputInstances");
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= Repaint;
        SceneView.duringSceneGui -= DuringSceneGUI;

        DestroyImmediate(_previewMaterial);
    }

    // Scene View GUI
    private void DuringSceneGUI(SceneView sceneView)
    {
        if (inputInstances > 0)
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                MeshFilter meshFilter = go.GetComponent<MeshFilter>();

                if (meshFilter != null)
                {
                    Mesh mesh = meshFilter.sharedMesh;

                    for (int i = 1; i <= inputInstances; i++)
                    {
                        Vector3 finalPosition = FinalPosition(go.transform.position.x, go.transform.position.y, go.transform.position.z, i);

                        float rotationX = (go.transform.localRotation.eulerAngles.x + inputRotation.x) * i;
                        float rotationY = (go.transform.localRotation.eulerAngles.y + inputRotation.y) * i;
                        float rotationZ = (go.transform.localRotation.eulerAngles.z + inputRotation.z) * i;
                        Vector3 finalRotation = new Vector3(rotationX, rotationY, rotationZ);

                        float scaleX = inputScale.x;
                        float scaleY = inputScale.y;
                        float scaleZ = inputScale.z;
                        Vector3 finalScale = new Vector3(scaleX, scaleY, scaleZ);

                        Matrix4x4 matrix = Matrix4x4.TRS(finalPosition, Quaternion.Euler(finalRotation), finalScale);

                        _previewMaterial.SetPass(0);
                        Graphics.DrawMeshNow(mesh, matrix);
                    }
                }
            }
        }
    }

    // Editor Window GUI
    private void OnGUI()
    {
        _so.Update();

        EditorGUILayout.PropertyField(_propParent, new GUIContent("Parent"));
        
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(_propPosition, new GUIContent("Position"));
        EditorGUILayout.PropertyField(_propRotation, new GUIContent("Rotation"));
        EditorGUILayout.PropertyField(_propScale, new GUIContent("Scale"));
        EditorGUILayout.PropertyField(_propInstances, new GUIContent("No. of Instances"));

        EditorGUILayout.Space();
        
        // If properties are modified, repaint SceneView
        if (_so.ApplyModifiedProperties())
        {
            SceneView.RepaintAll();
        }

        using (new EditorGUI.DisabledScope(!((Selection.gameObjects.Length == 1) && (inputInstances > 0))))
        {
            if (GUILayout.Button("Special Duplicate"))
            {
                DuplicateObject();
            }
        }
    }

    // Function to duplicate selected objects
    private void DuplicateObject()
    {
        for (int i = 0; i < inputInstances; i++)
        {
            Unsupported.DuplicateGameObjectsUsingPasteboard();

            // Current duplicated object
            Transform currSelection = Selection.activeGameObject.transform;

            // Assigning scale to instantiated object
            float scaleX = inputScale.x;
            float scaleY = inputScale.y;
            float scaleZ = inputScale.z;

            currSelection.localPosition = FinalPosition(currSelection.localPosition.x, currSelection.localPosition.y, currSelection.localPosition.z);
            currSelection.localRotation = FinalRotation(currSelection.localRotation.eulerAngles.x, currSelection.localRotation.eulerAngles.y, currSelection.localRotation.eulerAngles.z);
            currSelection.localScale = new Vector3(scaleX, scaleY, scaleZ);
        }
    }

    private Vector3 FinalPosition(float xPos, float yPos, float zPos)
    {
        return new Vector3(inputPosition.x + xPos, inputPosition.y + yPos, inputPosition.z + zPos);
    }

    private Vector3 FinalPosition(float xPos, float yPos, float zPos, int index)
    {
        return new Vector3((inputPosition.x * index) + xPos, (inputPosition.y * index) + yPos, (inputPosition.z * index) + zPos);
    }

    private Quaternion FinalRotation(float xRot, float yRot, float zRot)
    {
        return Quaternion.Euler(new Vector3(inputRotation.x + xRot, inputRotation.y + yRot, inputRotation.z + zRot));
    }
}
