using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// シーン内オブジェクトのコンポーネントの情報を一覧表示するクラス
public class DebugInspector : EditorWindow
{
    // スクロールの位置を管理する変数
    Vector2 scrollPosition = Vector2.zero; 
    // 検索文字列を管理する変数
    string search = string.Empty;
    // スクリプトのみを検索するかどうかを管理するフラグ
    bool searchScriptsOnly = false;
    

    // メニュー欄に追加
    [MenuItem("Tools/Debug Inspector")]
    public static void ShowWindow()
    {
        // エディターウィンドウを作成して表示し、そのインスタンスを返却
        EditorWindow.GetWindow(typeof(DebugInspector));
    }

    void OnGUI()
    {
        GUIStyle textFieldStyle = new GUIStyle(EditorStyles.textField);
        textFieldStyle.alignment = TextAnchor.MiddleLeft;
        // 水平スクロールの開始
        GUILayout.BeginHorizontal("HelpBox");
        // ラベルの表示
        GUILayout.Label("Search: ", EditorStyles.boldLabel);
        // テキストフィールドの表示
        search = EditorGUILayout.TextField(search);
        // トグルの表示
        searchScriptsOnly = EditorGUILayout.ToggleLeft("Scripts Only", searchScriptsOnly);
        // 水平スクロールの終了
        GUILayout.EndHorizontal();

        // オブジェクトのリストスクロールの開始
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // シーン内の全てのGameObjectに対して一つずつ処理を実行
        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
        {
            // 検索文字列が含まれないオブジェクトはスキップ
            if (!string.IsNullOrEmpty(search) && !go.name.ToLower().Contains(search.ToLower()))
                continue;

            // "Scripts Only"が選択されていて、オブジェクトが自作のスクリプトを持っていない場合はスキップ
            if (searchScriptsOnly && !HasUserScript(go))
                continue;

            // 垂直スクロールの開始
            EditorGUILayout.BeginVertical("box");

            //オブジェクト名を表示
            EditorGUILayout.LabelField("Object Name: " + go.name, EditorStyles.boldLabel);
            //オブジェクトが持つ全てのコンポーネントを調査
            foreach (Component comp in go.GetComponents<Component>())
            {
                // "Scripts Only"が選択されていて、コンポーネントが自作のスクリプトでない場合はスキップ
                if (searchScriptsOnly && !(comp is MonoBehaviour) || (comp.GetType().Namespace != null && comp.GetType().Namespace.StartsWith("UnityEngine")))
                    continue;
                //スプリクト名を表示
                EditorGUILayout.LabelField("  Script Name: " + comp.GetType().Name, EditorStyles.boldLabel);

                // 現在のコンポーネントをSerializedObjectとして取得
                SerializedObject so = new SerializedObject(comp);
                // SerializedObjectからプロパティのイテレータを取得
                SerializedProperty prop = so.GetIterator();
                // すべてのプロパティを順番に処理
                while (prop.NextVisible(true))
                {
                    // m_Scriptのプロパティはスキップ
                    if (prop.name == "m_Script") continue;

                    // 垂直スクロールの開始
                    EditorGUILayout.BeginHorizontal();
                    // プロパティ名と値を表示
                    EditorGUILayout.LabelField("    Variable Name: " + prop.name);
                    EditorGUILayout.LabelField("    Value: " + GetPropertyValue(prop));
                    // 垂直スクロールの終了
                    EditorGUILayout.EndHorizontal();
                }
            }
            // 垂直スクロールの開始
            EditorGUILayout.EndVertical();
        }
        // オブジェクトのリストスクロールの終了
        EditorGUILayout.EndScrollView();
    }

    // オブジェクトが自作スクリプトを持っているかどうかをチェックするメソッド
    bool HasUserScript(GameObject go)
    {
        foreach (Component comp in go.GetComponents<Component>())
        {
            // コンポーネントが自作のスクリプトでない場合はスキップ
            if (!(comp is MonoBehaviour) || (comp.GetType().Namespace != null && comp.GetType().Namespace.StartsWith("UnityEngine")))
                continue;
            
            return true; // 自作のスクリプトが見つかったらtrueを返却
        }

        return false; // 自作のスクリプトが見つからなかったらfalseを返却
    }

    // SerializedProperty の値を文字列で取得するメソッド
    string GetPropertyValue(SerializedProperty prop)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer:
                return prop.intValue.ToString();
            case SerializedPropertyType.Boolean:
                return prop.boolValue.ToString();
            case SerializedPropertyType.Float:
                return prop.floatValue.ToString();
            case SerializedPropertyType.String:
                return prop.stringValue;
            case SerializedPropertyType.Color:
                return prop.colorValue.ToString();
            // 必要に応じて他の型も追加する
            default:
                return "<unknown>"; // 他の型は<unknown>を返却
        }
    }
}
