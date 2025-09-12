#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AniDrag.Utility
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = (ShowIfAttribute)attribute;

            var target = property.serializedObject.targetObject;
            var field = target.GetType().GetField(showIf.conditionName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (field != null && (bool)field.GetValue(target))
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = (ShowIfAttribute)attribute;
            var target = property.serializedObject.targetObject;
            var field = target.GetType().GetField(showIf.conditionName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (field != null && (bool)field.GetValue(target))
                return EditorGUI.GetPropertyHeight(property, label);
            return 0f; // hide
        }
    }
}
#endif
