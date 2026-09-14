using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShapeController)), CanEditMultipleObjects]
public class ShapeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Have serializedObject update size
        serializedObject.Update();
        var size = serializedObject.FindProperty("size");
        serializedObject.ApplyModifiedProperties();

        // If a single value is set to size, resize the object(s) and check legality of new size
        if (!size.hasMultipleDifferentValues)
        {
            Transform[] targetTransforms = serializedObject.targetObjects.Select(obj => obj.GetComponent<Transform>()).ToArray();
            foreach (Transform target in targetTransforms)
            {
                target.localScale = Vector3.one * size.floatValue;
            }

            // Get selected shapes
            List<Shape> selectedShapes = GetSelectedShapes();

            // On setting a value, check legality
            if (size.floatValue <= 0)
            {
                EditorGUILayout.HelpBox("Size cannot be 0 or below", MessageType.Error);
            }
            else if (selectedShapes.Contains(Shape.Cube) && size.floatValue > 5)
            {
                EditorGUILayout.HelpBox("Size of cubes should not be above 5", MessageType.Warning);
            }
            if (selectedShapes.Contains(Shape.Sphere) && size.floatValue > 3)
            {
                EditorGUILayout.HelpBox("Size of spheres should not be above 3", MessageType.Warning);
            }
        }

        // Buttons
        GameObject[] ObjectsWithSameShape = GetObjectsOfShape();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Select All Same Shapes"))
        {
            // Select all GameObjects in the array
            Selection.objects = ObjectsWithSameShape;
        }

        if (GUILayout.Button("Clear Selection"))
        {
            Selection.objects = null;
        }

        EditorGUILayout.EndHorizontal();

        // Default to disabling all selected objects
        string buttonName = "Disable All Same Shapes";
        Color buttonColor = Color.red;
        bool disable = true;

        // If any objects are disabled, instead enable all objects
        foreach (GameObject obj in ObjectsWithSameShape)
        {
            if (!obj.activeInHierarchy)
            {
                buttonName = "Enable All Same Shapes";
                buttonColor = Color.green;
                disable = false;
            }
        }

        var originalColor = GUI.backgroundColor;
        GUI.backgroundColor = buttonColor;
        if (GUILayout.Button(buttonName, GUILayout.Height(40)))
        {
            if (disable)
            {
                foreach (GameObject obj in ObjectsWithSameShape)
                    obj.SetActive(false);
            }
            else
            {
                foreach (GameObject obj in ObjectsWithSameShape)
                    obj.SetActive(true);
            }
        }
        GUI.backgroundColor = originalColor;
    }

    private GameObject[] GetObjectsOfShape()
    {
        // Get selected shapes
        List<Shape> selectedShapes = GetSelectedShapes();

        // Find all ShapeControllers
        var allShapeControllers = FindObjectsByType<ShapeController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        // Create a list of the ShapeControllers with the same shape
        List<ShapeController> shapeList = new List<ShapeController>();
        for (int i = 0; i < allShapeControllers.Length; i++)
        {
            foreach (Shape selected in selectedShapes)
                if (allShapeControllers[i].shape == selected)
                    shapeList.Add(allShapeControllers[i]);
        }

        // Get the GameObjects of all ShapeControllers in the list
        var allShapeObjects = shapeList.ToArray()
        .Select(enemy => enemy.gameObject)
        .ToArray();

        return allShapeObjects;
    }

    private List<Shape> GetSelectedShapes()
    {
        // Get all selected objects
        ShapeController[] targets = serializedObject.targetObjects.Select(obj => obj.GetComponent<ShapeController>()).ToArray();

        // Get all selected shapes
        List<Shape> selectedShapes = new List<Shape>();
        foreach (ShapeController controller in targets)
        {
            if (!selectedShapes.Contains(controller.shape))
                selectedShapes.Add(controller.shape);
        }
        return selectedShapes;
    }
}
