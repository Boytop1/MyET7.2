using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using ET;
//Object并非C#基础中的Object，而是 UnityEngine.Object
using Object = UnityEngine.Object;

[ExecuteInEditMode]
//自定义ReferenceCollector类在界面中的显示与功能
[CustomEditor(typeof (ReferenceCollector))]
public class ReferenceCollectorEditor: Editor
{
    //输入在textfield中的字符串
    private string searchKey
	{
		get
		{
			return _searchKey;
		}
		set
		{
			if (_searchKey != value)
			{
				_searchKey = value;
				heroPrefab = referenceCollector.Get<Object>(searchKey);
			}
		}
	}

	private ReferenceCollector referenceCollector;

	private Object heroPrefab;

	private string _searchKey = "";

	private void DelNullReference()
	{
		var dataProperty = serializedObject.FindProperty("data");
		for (int i = dataProperty.arraySize - 1; i >= 0; i--)
		{
			var gameObjectProperty = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("gameObject");
			if (gameObjectProperty.objectReferenceValue == null)
			{
				dataProperty.DeleteArrayElementAtIndex(i);
				EditorUtility.SetDirty(referenceCollector);
				serializedObject.ApplyModifiedProperties();
				serializedObject.UpdateIfRequiredOrScript();
			}
		}
	}

	private void OnEnable()
	{
        //将被选中的gameobject所挂载的ReferenceCollector赋值给编辑器类中的ReferenceCollector，方便操作
        referenceCollector = (ReferenceCollector) target;
	}

	public override void OnInspectorGUI()
	{
        //使ReferenceCollector支持撤销操作，还有Redo，不过没有在这里使用
        Undo.RecordObject(referenceCollector, "Changed Settings");
		var dataProperty = serializedObject.FindProperty("data");
        //开始水平布局，如果是比较新版本学习U3D的，可能不知道这东西，这个是老GUI系统的知识，除了用在编辑器里，还可以用在生成的游戏中
		GUILayout.BeginHorizontal();
        //下面几个if都是点击按钮就会返回true调用里面的东西
		if (GUILayout.Button("添加引用"))
		{
            //添加新的元素，具体的函数注释
            // Guid.NewGuid().GetHashCode().ToString() 就是新建后默认的key
            AddReference(dataProperty, Guid.NewGuid().GetHashCode().ToString(), null);
		}
		if (GUILayout.Button("全部删除"))
		{
			referenceCollector.Clear();
		}
		if (GUILayout.Button("删除空引用"))
		{
			DelNullReference();
		}
		if (GUILayout.Button("排序"))
		{
			referenceCollector.Sort();
		}
		if (GUILayout.Button("生成脚本"))
		{
			this.GenerateScript();	
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
        //可以在编辑器中对searchKey进行赋值，只要输入对应的Key值，就可以点后面的删除按钮删除相对应的元素
        searchKey = EditorGUILayout.TextField(searchKey);
        //添加的可以用于选中Object的框，这里的object也是(UnityEngine.Object
        //第三个参数为是否只能引用scene中的Object
        EditorGUILayout.ObjectField(heroPrefab, typeof (Object), false);
		if (GUILayout.Button("删除"))
		{
			referenceCollector.Remove(searchKey);
			heroPrefab = null;
		}
		
		GUILayout.EndHorizontal();
		EditorGUILayout.Space();

		var delList = new List<int>();
        SerializedProperty property;
        //遍历ReferenceCollector中data list的所有元素，显示在编辑器中
        for (int i = referenceCollector.data.Count - 1; i >= 0; i--)
		{
			GUILayout.BeginHorizontal();
            //这里的知识点在ReferenceCollector中有说
            property = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("key");
            EditorGUILayout.TextField(property.stringValue, GUILayout.Width(150));
            property = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("gameObject");
            property.objectReferenceValue = EditorGUILayout.ObjectField(property.objectReferenceValue, typeof(Object), true);
			if (GUILayout.Button("X"))
			{
                //将元素添加进删除list
				delList.Add(i);
			}
			GUILayout.EndHorizontal();
		}
		var eventType = Event.current.type;
        //在Inspector 窗口上创建区域，向区域拖拽资源对象，获取到拖拽到区域的对象
        if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
		{
			// Show a copy icon on the drag
			DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

			if (eventType == EventType.DragPerform)
			{
				DragAndDrop.AcceptDrag();
				foreach (var o in DragAndDrop.objectReferences)
				{
					AddReference(dataProperty, o.name, o);
				}
			}

			Event.current.Use();
		}

        //遍历删除list，将其删除掉
		foreach (var i in delList)
		{
			dataProperty.DeleteArrayElementAtIndex(i);
		}
		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfRequiredOrScript();
	}

    //添加元素，具体知识点在ReferenceCollector中说了
    private void AddReference(SerializedProperty dataProperty, string key, Object obj)
	{
		int index = dataProperty.arraySize;
		dataProperty.InsertArrayElementAtIndex(index);
		var element = dataProperty.GetArrayElementAtIndex(index);
		element.FindPropertyRelative("key").stringValue = key;
		element.FindPropertyRelative("gameObject").objectReferenceValue = obj;
	}
    
    private void GenerateScript()
	{
		var prefab = this.referenceCollector.gameObject;
		//var assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab.gameObject);
		//var assetPath= AssetDatabase.GetAssetPath(prefab);
		var assetPath = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(prefab).prefabAssetPath;
		if (string.IsNullOrEmpty(assetPath))
			return;
		var cutPath = assetPath.Substring("Assets/Bundles/UI/".Length);
		cutPath = cutPath.Replace(".prefab", "");
		var scriptPath = "Assets/Scripts/Codes/ModelView/Client/Demo/UI";
		var systemPath = "Assets/Scripts/Codes/HotfixView/Client/Demo/UI";

		var useSB = new StringBuilder();
		var fieldSB = new StringBuilder();
		var initSB = new StringBuilder();
		//Component----
		useSB.AppendLine("using UnityEngine.UI;");
		useSB.AppendLine("using UnityEngine;");

		foreach (var data in this.referenceCollector.data)
		{
			fieldSB.AppendLine($"\t\tpublic GameObject {data.key};");
		}
		
		var componentTemp = File.ReadAllText("Assets/Scripts/Editor/ReferenceCollectorEditor/ComponentTemplete.txt");
		componentTemp = componentTemp.Replace("[USING]", useSB.ToString());
		componentTemp = componentTemp.Replace("[NAME]", this.referenceCollector.name);
		componentTemp = componentTemp.Replace("[FIELDS]", fieldSB.ToString());
		
		if (!Directory.Exists($"{scriptPath}/{cutPath}"))
			Directory.CreateDirectory($"{scriptPath}/{cutPath}");
		File.WriteAllText($"{scriptPath}/{cutPath}/{this.referenceCollector.name}Component.cs", componentTemp);

		//ComponentSystem---------------
		foreach (var data in this.referenceCollector.data)
		{
			initSB.AppendLine($"\t\t\t\tself.{data.key} = rc.Get<GameObject>(\"{data.key}\");");
		}
		
		var systemTemp = File.ReadAllText("Assets/Scripts/Editor/ReferenceCollectorEditor/ComponentSystemTemplete.txt");
		systemTemp = systemTemp.Replace("[USING]", useSB.ToString());
		systemTemp = systemTemp.Replace("[NAME]", this.referenceCollector.name);
		systemTemp = systemTemp.Replace("[INIT]", initSB.ToString());
		if (!Directory.Exists($"{systemPath}/{cutPath}"))
			Directory.CreateDirectory($"{systemPath}/{cutPath}");
		File.WriteAllText($"{systemPath}/{cutPath}/{this.referenceCollector.name}ComponentSystem.cs", systemTemp);

		//Event---------------
		var eventTemp = File.ReadAllText("Assets/Scripts/Editor/ReferenceCollectorEditor/EventTemplete.txt");
		eventTemp = eventTemp.Replace("[USING]", useSB.ToString());
		eventTemp = eventTemp.Replace("[NAME]", this.referenceCollector.name);
		if (!Directory.Exists($"{systemPath}/{cutPath}"))
			Directory.CreateDirectory($"{systemPath}/{cutPath}");
		File.WriteAllText($"{systemPath}/{cutPath}/{this.referenceCollector.name}Event.cs", eventTemp);

		//UIType--------
		var uiType = "Assets/Scripts/Codes/ModelView/Client/Module/UI/UIType.cs";
		var fields = new StringBuilder();
		fields.AppendLine("namespace ET.Client");
		fields.AppendLine("{");
		fields.AppendLine("\tpublic static class UIType");
		fields.AppendLine("\t{");
		var existKey = new HashSet<string>();
		if (File.Exists(uiType))
		{
			var regex = new Regex(@"public const string (\w*) = .*;");
			var lines = File.ReadAllLines(uiType);
			foreach (string line in lines)
			{
				if (regex.IsMatch(line))
				{
					var res = regex.Match(line);
					existKey.Add(res.Groups[1].Value);
					fields.AppendLine(line);
				}
			}
		}

		if (!existKey.Contains(this.referenceCollector.name))
		{
			fields.AppendLine($"\t\tpublic const string {this.referenceCollector.name} = \"{this.referenceCollector.name}\";");
			
		}

		fields.AppendLine("\t}");
		fields.AppendLine("}");
		File.WriteAllText(uiType, fields.ToString());
		
		
		AssetDatabase.Refresh();
		Debug.Log("UI script build success!");
	}
}
