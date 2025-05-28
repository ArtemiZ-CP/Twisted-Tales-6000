using UnityEngine;
using UnityEditor;

namespace Quantum.Game
{
    [CustomPropertyDrawer(typeof(RoundInfo))]
    public class RoundInfoDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = EditorGUIUtility.singleLineHeight;

            if (property.isExpanded)
            {
                SerializedProperty isPVEProp = property.FindPropertyRelative("IsPVE");
                totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                if (isPVEProp.boolValue)
                {
                    SerializedProperty boardProp = property.FindPropertyRelative("PVEBoard");

                    if (boardProp != null && boardProp.isArray)
                    {
                        int rowCount = boardProp.arraySize;

                        totalHeight += rowCount * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
                    }
                }
            }

            return totalHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            property.isExpanded = EditorGUI.Foldout(
                new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                property.isExpanded, label);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                SerializedProperty isPVEProp = property.FindPropertyRelative("IsPVE");
                EditorGUI.PropertyField(
                    new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                    isPVEProp);

                property.serializedObject.ApplyModifiedProperties();

                if (isPVEProp.boolValue)
                {
                    SerializedProperty boardProp = property.FindPropertyRelative("PVEBoard");
                    if (boardProp.arraySize == 0)
                    {
                        int boardSize = GameplayConstants.BoardSize;
                        boardProp.arraySize = boardSize / 2;

                        for (int i = 0; i < boardSize / 2; i++)
                        {
                            SerializedProperty rowProp = boardProp.GetArrayElementAtIndex(i);
                            SerializedProperty cellsProp = rowProp.FindPropertyRelative("Cells");

                            cellsProp.arraySize = boardSize;

                            for (int j = 0; j < boardSize; j++)
                            {
                                SerializedProperty cellProp = cellsProp.GetArrayElementAtIndex(j);
                                cellProp.intValue = -1;
                            }
                        }

                        property.serializedObject.ApplyModifiedProperties();
                    }

                    position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                    if (boardProp != null && boardProp.isArray)
                    {
                        int rowCount = boardProp.arraySize;

                        float cellWidth = (position.width - 20) / 8 - 4;

                        for (int i = 0; i < rowCount; i++)
                        {
                            SerializedProperty rowProp = boardProp.GetArrayElementAtIndex(i);
                            SerializedProperty cellsProp = rowProp.FindPropertyRelative("Cells");

                            if (cellsProp != null && cellsProp.isArray)
                            {
                                for (int j = 0; j < cellsProp.arraySize; j++)
                                {
                                    SerializedProperty cellProp = cellsProp.GetArrayElementAtIndex(j);

                                    Rect cellRect = new Rect(
                                        position.x + 10 + j * (cellWidth + 4),
                                        position.y,
                                        cellWidth,
                                        EditorGUIUtility.singleLineHeight);
                                    EditorGUI.PropertyField(cellRect, cellProp, GUIContent.none);
                                }
                                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                            }
                        }
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
}