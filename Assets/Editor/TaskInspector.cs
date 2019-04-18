using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


public class TaskDrawer : PropertyAttribute
{

}


[System.Serializable]
public class Tasks
{
    public List<Task> _Tasks = new List<Task>();

    [System.Serializable]
    public class Task
    {
        public string _Title;
        public string _Member;
        public string _Discription;
        public List<string> _ReferenceAssetsPass;

        public Task()
        {
            _Title = "タイトルを入力";
            _Member = "担当者";
            _Discription = "説明";
        }
    }
}





public class TaskInspector : EditorWindow
{

    [MenuItem("Tool/TaskInspector")]
    static void ShowWindow()
    {
        TaskInspector taskInspector = EditorWindow.GetWindow<TaskInspector>();
    }

    Tasks tasks = new Tasks();
    string cachePath = "";
    ReorderableList reorderableList;

    private void OnGUI()
    {
        //日本語入力の許可
        Input.imeCompositionMode = IMECompositionMode.On;

        //選択しているオブジェクトを取得
        string[] selectedAsset = Selection.assetGUIDs;
        if (selectedAsset.Length <= 0) return;
        string targetPath = AssetDatabase.GUIDToAssetPath(selectedAsset[0]);
        string userData = AssetImporter.GetAtPath(targetPath).userData;

        //新しいオブジェクトが選択されたか
        if (targetPath != cachePath)
        {
            cachePath = targetPath;
            tasks = new Tasks();
            //userデータにタスクが登録されていれば取得
            if (userData != "")
            {
                tasks = JsonUtility.FromJson<Tasks>(userData);
            }
        }

        //debug用
        GUILayout.Label("userData:" + userData);

        //タスクを追加
        if (GUILayout.Button("Add Task"))
        {
            if (userData == "")
            {
                tasks = new Tasks();
                tasks._Tasks.Add(new Tasks.Task());
            }
            else
            {
                tasks = JsonUtility.FromJson<Tasks>(userData);
                tasks._Tasks.Add(new Tasks.Task());
            }
        }

        reorderableList = new ReorderableList(
                                          elements: tasks._Tasks,    //要素
                                          elementType: typeof(Tasks.Task), //要素の種類
                                          draggable: true,           //ドラッグして要素を入れ替えられるか
                                          displayHeader: true,           //ヘッダーを表示するか
                                          displayAddButton: true,           //要素追加用の+ボタンを表示するか
                                          displayRemoveButton: true            //要素削除用の-ボタンを表示するか
                                        );
        reorderableList.DoLayoutList();
        bool isUpdate = false;

        //タスクを追加
        for (int i = 0; i < tasks._Tasks.Count; i++)
        {
            string title = tasks._Tasks[i]._Title;
            string discription = tasks._Tasks[i]._Discription;
            string member = tasks._Tasks[i]._Member;

            EditorGUILayout.LabelField("タイトル");
            tasks._Tasks[i]._Title = EditorGUILayout.TextField(tasks._Tasks[i]._Title);
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("説明");
            tasks._Tasks[i]._Discription = EditorGUILayout.TextArea(tasks._Tasks[i]._Discription, GUILayout.Height(100));
            tasks._Tasks[i]._Member = EditorGUILayout.TextField(tasks._Tasks[i]._Member);

            EditorGUI.indentLevel--;
            if (tasks._Tasks[i]._Title != title ||
                tasks._Tasks[i]._Discription != discription ||
                tasks._Tasks[i]._Member != member)
            {
                isUpdate = true;
            }
        }

        //タスクをセーブ
        if (tasks._Tasks.Count > 0 && isUpdate)
        {
            SaveTask(targetPath, tasks);
        }
    }

    private static void SaveTask(string path, Tasks tasks)
    {
        AssetImporter.GetAtPath(path).userData = JsonUtility.ToJson(tasks);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }
}
