
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneNameAttribute : PropertyAttribute
{
    
}

#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(SceneNameAttribute))]
public class SceneNameDrawer : PropertyDrawer
{
    int sceneNameIndex = -1;
    GUIContent[] sceneNameOptions;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if(EditorBuildSettings.scenes.Length == 0) return;

        if(sceneNameIndex == -1)
            GetSceneNames(property);

        sceneNameIndex = EditorGUI.Popup(position, label, sceneNameIndex, sceneNameOptions);
        
        property.stringValue = sceneNameOptions[sceneNameIndex].text;

    }

    private void GetSceneNames(SerializedProperty property)
    {
        int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
        sceneNameOptions = new GUIContent[sceneCount];
        for(int i = 0; i < sceneCount; i++)
        {
            string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            sceneNameOptions[i] = new GUIContent(System.IO.Path.GetFileNameWithoutExtension(path));
        }
        
        if(String.IsNullOrEmpty(property.stringValue))
        {
            sceneNameIndex = 0;
        }
        else
        {
            for(int i = 0; i < sceneNameOptions.Length; i++)
            {
                if(sceneNameOptions[i].text == property.stringValue)
                {
                    sceneNameIndex = i;
                    break;
                }
            }
        }
        property.stringValue = sceneNameOptions[sceneNameIndex].text;
        
    }
}
#endif
