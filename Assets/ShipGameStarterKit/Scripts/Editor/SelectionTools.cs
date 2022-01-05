using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SelectionTools
{
	[MenuItem("Selection/Toggle 'Active' #&a")]
	static void ActivateDeactivate()
	{
		if (HasValidTransform())
		{
			GameObject[] gos = Selection.gameObjects;
			bool val = !Selection.activeGameObject.active;
			foreach (GameObject go in gos) go.SetActiveRecursively(val);
		}
	}

	[MenuItem("Selection/Clear Local Transform #&c")]
	static void ClearLocalTransform()
	{
		if (HasValidTransform())
		{
			Undo.RegisterSceneUndo("Clear Local Transform");

			Transform t = Selection.activeTransform;
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = Vector3.one;
		}
	}

	[MenuItem ("Selection/Add New Child #&n")]
	static void CreateLocalGameObject()
	{
		if (PrefabCheck())
		{
			// Make this action undoable
			Undo.RegisterSceneUndo("Add New Child");

			// Create our new GameObject
			GameObject newGameObject = new GameObject();
			newGameObject.name = "GameObject";

			// If there is a selected object in the scene then make the new object its child.
			if (Selection.activeTransform != null)
			{
				newGameObject.transform.parent = Selection.activeTransform;
				newGameObject.name = "Child";

				// Place the new GameObject at the same position as the parent.
				newGameObject.transform.localPosition = Vector3.zero;
				newGameObject.transform.localRotation = Quaternion.identity;
				newGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
				newGameObject.layer = Selection.activeGameObject.layer;
			}

			// Select our newly created GameObject
			Selection.activeTransform = newGameObject.transform;
		}
	}

	[MenuItem("Selection/List Dependencies #&i")]
	static void ListDependencies()
	{
		if (HasValidSelection())
		{
			Debug.Log("Asset dependencies:\n\n" + GetDependencyText(Selection.objects));
		}
	}

	//========================================================================================================

#region Helper Functions
	
	class AssetEntry
	{
		public string path;
		public List<System.Type> types = new List<System.Type>();
	}
	
	/// <summary>
	/// Helper function that checks to see if there are objects selected.
	/// </summary>

	static bool HasValidSelection()
	{
		if (Selection.objects == null || Selection.objects.Length == 0)
		{
			Debug.LogWarning("You must select an object first");
			return false;
		}
		return true;
	}
	
	/// <summary>
	/// Helper function that checks to see if there is an object with a Transform component selected.
	/// </summary>
	
	static bool HasValidTransform()
	{
		if (Selection.activeTransform == null)
		{
			Debug.LogWarning("You must select an object first");
			return false;
		}
		return true;
	}
	
	/// <summary>
	/// Helper function that checks to see if a prefab is currently selected.
	/// </summary>

	static bool PrefabCheck()
	{
		if (Selection.activeTransform != null)
		{
			// Check if the selected object is a prefab instance and display a warning
			PrefabType type = EditorUtility.GetPrefabType( Selection.activeGameObject );

			if (type == PrefabType.PrefabInstance)
			{
				return EditorUtility.DisplayDialog("Losing prefab",
					"This action will lose the prefab connection. Are you sure you wish to continue?",
					"Continue", "Cancel");
			}
			return true;
		}
		return false;
	}
	
	/// <summary>
	/// Function that collects a list of file dependencies from the specified list of objects.
	/// </summary>
	
	static List<AssetEntry> GetDependencyList (Object[] objects)
	{
		Object[] deps = EditorUtility.CollectDependencies(objects);
		
		List<AssetEntry> list = new List<AssetEntry>();
		
		foreach (Object obj in deps)
		{
			string path = AssetDatabase.GetAssetPath(obj);
			
			if (!string.IsNullOrEmpty(path))
			{
				bool found = false;
				System.Type type = obj.GetType();
				
				foreach (AssetEntry ent in list)
				{
					if (string.Equals(ent.path, path))
					{
						if (!ent.types.Contains(type)) ent.types.Add(type);
						found = true;
						break;
					}
				}
				
				if (!found)
				{
					AssetEntry ent = new AssetEntry();
					ent.path = path;
					ent.types.Add(type);
					list.Add(ent);
				}
			}
		}
		
		deps = null;
		objects = null;
		return list;
	}
	
	/// <summary>
	/// Helper function that removes the Unity class prefix from the specified string.
	/// </summary>
	
	static string RemovePrefix (string text)
	{
		text = text.Replace("UnityEngine.", "");
		text = text.Replace("UnityEditor.", "");
		return text;
	}
	
	/// <summary>
	/// Helper function that gets the dependencies of specified objects and returns them in text format.
	/// </summary>
	
	static string GetDependencyText (Object[] objects)
	{
		List<AssetEntry> dependencies = GetDependencyList(objects);
		List<string> list = new List<string>();
		string text = "";

		foreach (AssetEntry ae in dependencies)
		{
			text = ae.path.Replace("Assets/", "");
			
			if (ae.types.Count > 1)
			{
				text += " (" + RemovePrefix(ae.types[0].ToString());
			
				for (int i = 1; i < ae.types.Count; ++i)
				{
					text += ", " + RemovePrefix(ae.types[i].ToString());
				}
				
				text += ")";
			}
			list.Add(text);
		}
		
		list.Sort();
		
		text = "";
		foreach (string s in list) text += s + "\n";
		list.Clear();
		list = null;
		
		dependencies.Clear();
		dependencies = null;
		return text;
	}
#endregion
}
