using UnityEditor;

[CustomEditor(typeof(InteractActivator))]
public class InteractActivatorEditor : Editor
{
    SerializedProperty interactionText;
    SerializedProperty objectName;
    SerializedProperty triggerText;

    SerializedProperty triggerOnInteract;
    SerializedProperty onEnterTriggered;
    SerializedProperty onExitTriggered;

    SerializedProperty interactionType;
    SerializedProperty retriggerInterval;
    SerializedProperty canInteractOnlyOnce;

    void OnEnable()
    {
        interactionText = serializedObject.FindProperty("interactionText");
        objectName = serializedObject.FindProperty("objectName");
        triggerText = serializedObject.FindProperty("triggerText");

        triggerOnInteract = serializedObject.FindProperty("triggerOnInteract");
        onEnterTriggered = serializedObject.FindProperty("onEnterTriggered");
        onExitTriggered = serializedObject.FindProperty("onExitTriggered");

        interactionType = serializedObject.FindProperty("interactionType");
        retriggerInterval = serializedObject.FindProperty("retriggerInterval");
        canInteractOnlyOnce = serializedObject.FindProperty("canInteractOnlyOnce");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("========== Text Settings ==========", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(interactionText);
        EditorGUILayout.PropertyField(objectName);
        EditorGUILayout.PropertyField(triggerText);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("========== Settings ==========", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(interactionType);

        if (canInteractOnlyOnce != null)
        {
            EditorGUILayout.PropertyField(canInteractOnlyOnce);

            if (!canInteractOnlyOnce.boolValue)
            {
                EditorGUILayout.PropertyField(retriggerInterval);
            }
        }
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("========== Events ==========", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(triggerOnInteract);

        if (interactionType != null &&
            (InteractActivator.InteractionType)interactionType.enumValueIndex
            == InteractActivator.InteractionType.WhenItemPlacedInZone)
        {
            EditorGUILayout.PropertyField(onEnterTriggered);
            EditorGUILayout.PropertyField(onExitTriggered);
        }

        
        serializedObject.ApplyModifiedProperties();
    }
}
