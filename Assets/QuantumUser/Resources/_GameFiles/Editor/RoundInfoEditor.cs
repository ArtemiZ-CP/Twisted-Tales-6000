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

                        // Для каждой строки добавляем высоту
                        totalHeight += rowCount * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
                    }
                }
            }

            return totalHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Рисуем Foldout
            property.isExpanded = EditorGUI.Foldout(
                new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                property.isExpanded, label);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                // Смещаем позицию вниз для следующего элемента
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                // Получаем свойство IsPVE
                SerializedProperty isPVEProp = property.FindPropertyRelative("IsPVE");
                EditorGUI.PropertyField(
                    new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                    isPVEProp);

                // Применяем изменения, чтобы получить актуальные данные
                property.serializedObject.ApplyModifiedProperties();

                if (isPVEProp.boolValue)
                {
                    // Инициализируем PVEBoard, если он пустой
                    SerializedProperty boardProp = property.FindPropertyRelative("PVEBoard");
                    if (boardProp.arraySize == 0)
                    {
                        int boardSize = GameConfig.BoardSize;
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

                        // Применяем изменения
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
                                // Рисуем клетки строки
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
                                // Смещаем позицию вниз для следующей строки
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