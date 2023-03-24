using UnityEditor;
using UnityEngine;

public class LayerMatrixControl
{
    // TODO maybe just use https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/Physics2DEditor/Managed/Settings/LayerCollisionMatrix2D.cs
    private bool[,] matrix;
    private GUIContent[] rowLabels;
    private GUIContent[] columnLabels;
    private GUIStyle cellStyle;
    private int cellWidth = 20;

    public LayerMatrixControl(bool[,] matrix, GUIContent[] rowLabels, GUIContent[] columnLabels)
    {
        this.matrix = matrix;
        this.rowLabels = rowLabels;
        this.columnLabels = columnLabels;
        this.cellStyle = new GUIStyle(GUI.skin.toggle);
        this.cellStyle.margin = new RectOffset(0, 0, 0, 0);
        this.cellStyle.padding = new RectOffset(0, 0, 0, 0);
    }

    public void Draw()
    {
        GUILayout.BeginVertical();

        // Draw column labels
        GUILayout.BeginHorizontal();
        GUILayout.Space(EditorGUIUtility.labelWidth);
        var oldMatrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(90, GUI.matrix.GetPosition());
        for (int j = 0; j < columnLabels.Length; j++)
        {
            GUILayout.Label(columnLabels[j], GUILayout.Width(cellWidth));
        }

        GUI.matrix = oldMatrix;
        GUILayout.EndHorizontal();

        // Draw matrix
        for (int i = 0; i < rowLabels.Length; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(rowLabels[i], GUILayout.Width(EditorGUIUtility.labelWidth));
            for (int j = 0; j < columnLabels.Length; j++)
            {
                matrix[i, j] = GUILayout.Toggle(matrix[i, j], "", cellStyle, GUILayout.Width(cellWidth));
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }
}
