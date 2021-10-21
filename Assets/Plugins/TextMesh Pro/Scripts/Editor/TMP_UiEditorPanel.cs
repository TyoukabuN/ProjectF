using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace TMPro.EditorUtilities
{

    [CustomEditor(typeof(TextMeshProUGUI), true), CanEditMultipleObjects]
    public class TMP_UiEditorPanel : TMP_BaseEditorPanel
    {
        static readonly GUIContent k_RaycastTargetLabel = new GUIContent("Raycast Target", "Whether the text blocks raycasts from the Graphic Raycaster.");
        static readonly GUIContent k_m_IsOutlineGrayLabel = new GUIContent("IsOutlineGray", "Whether Outline Gray");
        static readonly GUIContent k_m_UseSpriteGradient = new GUIContent("UseSpriteGradient", "Whether Use SpriteGradient");
        static readonly GUIContent k_m_SpriteTopColor = new GUIContent("SpriteTopColor", "Sprite Top Color");
        static readonly GUIContent k_m_SpriteBottomColor = new GUIContent("SpriteBottomColor", "Sprite Bottom Color");

        

        SerializedProperty m_RaycastTargetProp;
        SerializedProperty m_IsOutlineGray;
        SerializedProperty m_UseSpriteGradient;
        SerializedProperty m_SpriteTopColor;
        SerializedProperty m_SpriteBottomColor;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_RaycastTargetProp = serializedObject.FindProperty("m_RaycastTarget");
            m_IsOutlineGray = serializedObject.FindProperty("m_IsOutlineGray");
            m_UseSpriteGradient = serializedObject.FindProperty("m_UseSpriteGradient");
            m_SpriteTopColor = serializedObject.FindProperty("m_SpriteTopColor");
            m_SpriteBottomColor = serializedObject.FindProperty("m_SpriteBottomColor");

        }

        protected override void DrawMaterialSettings()
        {
            Foldout.materialSettings = EditorGUILayout.Foldout(Foldout.materialSettings, K_MaterialSettingLabel, true, TMP_UIStyleManager.boldFoldout);
            if (Foldout.materialSettings)
            {
                EditorGUI.indentLevel += 1;
                
                DrawMaterial();

                EditorGUI.indentLevel -= 1;
            }
        }

        protected override void DrawExtraSettings()
        {
            Foldout.extraSettings = EditorGUILayout.Foldout(Foldout.extraSettings, k_ExtraSettingsLabel, true, TMP_UIStyleManager.boldFoldout);
            if (Foldout.extraSettings)
            {
                EditorGUI.indentLevel += 1;

                DrawExtraSetting();

                EditorGUI.indentLevel -= 1;
            }
        }

        protected virtual void DrawExtraSetting()
        {
            DrawMargins();

            DrawGeometrySorting();

            DrawRichText();

            DrawRaycastTarget();

            DrawParsing();

            DrawKerning();

            DrawPadding();
            
            DrawCanSortingMask();

            DrawIsOutlineGray();

            DrawSpriteAssetGradient();
        }
        protected void DrawRaycastTarget()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_RaycastTargetProp, k_RaycastTargetLabel);
            if (EditorGUI.EndChangeCheck())
            {
                // Change needs to propagate to the child sub objects.
                Graphic[] graphicComponents = m_TextComponent.GetComponentsInChildren<Graphic>(true);
                for (int i = 1; i < graphicComponents.Length; i++)
                    graphicComponents[i].raycastTarget = m_RaycastTargetProp.boolValue;

                m_HavePropertiesChanged = true;
            }
        }

        // Method to handle multi object selection
        protected override bool IsMixSelectionTypes()
        {
            GameObject[] objects = Selection.gameObjects;
            if (objects.Length > 1)
            {
                for (int i = 0; i < objects.Length; i++)
                {
					if (objects[i].GetComponent<TextMeshProUGUI>() == null)
                        return true;
                }
            }
            return false;
        }
        protected override void OnUndoRedo()
        {
            int undoEventId = Undo.GetCurrentGroup();
            int lastUndoEventId = s_EventId;

            if (undoEventId != lastUndoEventId)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    //Debug.Log("Undo & Redo Performed detected in Editor Panel. Event ID:" + Undo.GetCurrentGroup());
                    TMPro_EventManager.ON_TEXTMESHPRO_UGUI_PROPERTY_CHANGED(true, targets[i] as TextMeshProUGUI);
                    s_EventId = undoEventId;
                }
            }
        }
        protected void DrawIsOutlineGray()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_IsOutlineGray, k_m_IsOutlineGrayLabel);
            if (EditorGUI.EndChangeCheck())
            {
                m_HavePropertiesChanged = true;
            }
        }
        protected void DrawSpriteAssetGradient()
        {
            if (m_SpriteAssetProp.objectReferenceValue == null) return;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_UseSpriteGradient, k_m_UseSpriteGradient);
            if (!m_UseSpriteGradient.boolValue) return;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_SpriteTopColor, k_m_SpriteTopColor);
            EditorGUILayout.PropertyField(m_SpriteBottomColor, k_m_SpriteBottomColor);
            if (EditorGUI.EndChangeCheck())
            {
                m_HavePropertiesChanged = true;
            }
        }
    }
}