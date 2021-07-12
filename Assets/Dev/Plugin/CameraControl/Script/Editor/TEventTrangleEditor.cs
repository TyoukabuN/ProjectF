using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TriggerCondition = TMesh.TEvent.TriggerCondition;

namespace TMesh
{

    [CustomEditor(typeof(TEventTrangle))]
    public class TEventTrangleEditor : Editor
    {

        public enum EventTypeForEditor
        {
            Timeline,
            Animation,
            Do,
        }

        public enum ConditionTypeForEditor
        {
            进入网格时,
            离开网格时,
            在网格里等待一下,
            进入一次后每帧调用,
        }

        EventTypeForEditor display = EventTypeForEditor.Timeline;


        public static TEventTrangle obj;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            SerializedProperty listIterator = serializedObject.FindProperty("Events");

            obj = target as TEventTrangle;

            if (obj.Events.Count > 0)
            {
                for (int i = 0; i < obj.Events.Count; i++)
                {
                    EditorGUI.BeginChangeCheck();

                    var eventObject = obj.Events[i];
                    var type = eventObject.GetType();

                    var eventElement = listIterator.GetArrayElementAtIndex(i);

                    TriggerEventType eventType = (TriggerEventType)eventElement.FindPropertyRelative("eventType").enumValueIndex;

                    SerializedProperty playableDirector = eventElement.FindPropertyRelative("playableDirector");
                    SerializedProperty animation = eventElement.FindPropertyRelative("animation");
                    SerializedProperty onTrigger = eventElement.FindPropertyRelative("onTrigger");
                    SerializedProperty canTriggerTimes = eventElement.FindPropertyRelative("canTriggerTimes");
                    TriggerCondition condition = (TriggerCondition)(eventElement.FindPropertyRelative("condition").enumValueIndex);
                    var useUnscaledTime = eventElement.FindPropertyRelative("useUnscaledTime");
                    var waitingSceond = eventElement.FindPropertyRelative("waitingSceond");


                    //public PlayableDirector playableDirector;
                    //public Animation animation;
                    //public OnTargetEvent onTrigger = new OnTargetEvent();


                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.BeginVertical();
                    if (eventType == TriggerEventType.Timeline)
                    {
                        EditorGUILayout.PropertyField(playableDirector, new GUIContent("Playable Director"));
                    }
                    else if (eventType == TriggerEventType.Animation)
                    {
                        EditorGUILayout.PropertyField(animation, new GUIContent("Animation"));
                    }
                    else if (eventType == TriggerEventType.Do)
                    {
                        EditorGUILayout.PropertyField(onTrigger, new GUIContent("要触发的函数"));
                    }

                    condition = (TriggerCondition)EditorGUILayout.EnumPopup(new GUIContent("触发条件"), (ConditionTypeForEditor)condition);
                    if (condition == TEvent.TriggerCondition.WaitASecond)
                    {
                        EditorGUILayout.PropertyField(useUnscaledTime, new GUIContent("不受TimeScaleT影响"));
                        EditorGUILayout.PropertyField(waitingSceond, new GUIContent("等待时间"));
                    }

                    EditorGUILayout.PropertyField(canTriggerTimes, new GUIContent("可触发次数(<0时不限次数)"));

                    EditorGUILayout.EndVertical();

                    if (GUILayout.Button("删除"))
                    {
                        Undo.RecordObject(obj, "TEvent Remove Event");
                        obj.Events.RemoveAt(i);
                        i--;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (EditorGUI.EndChangeCheck())
                    {
                        eventObject.condition = condition;
                    }

                };
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField("要添加的类型:", GUILayout.Width(30f * 2.7f));
            display = (EventTypeForEditor)EditorGUILayout.EnumPopup(display, GUILayout.Width(80));
            if (GUILayout.Button("添加事件"))
            {
                switch (display)
                {
                    case EventTypeForEditor.Timeline:
                        Undo.RecordObject(obj, "TEvent Add PlayableDirector Event");
                        obj.Events.Add(new TEvent(TriggerEventType.Timeline));
                        break;
                    case EventTypeForEditor.Animation:
                        Undo.RecordObject(obj, "TEvent Add Animation Event");
                        obj.Events.Add(new TEvent(TriggerEventType.Animation));
                        break;
                    case EventTypeForEditor.Do:
                        Undo.RecordObject(obj, "TEvent Add Animation Event");
                        obj.Events.Add(new TEvent(TriggerEventType.Do));
                        break;
                    default:
                        break;
                }

                EditorUtility.SetDirty(obj);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(obj);
            }
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndHorizontal();


        }
    }
}