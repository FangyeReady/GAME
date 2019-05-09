using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Utilities;

[System.Serializable]
public class Worksheet : ScriptableObject {
	#region Properties
	[SerializeField]
	private DataFile _file;
	public DataFile file {
		get { return _file; }
		private set { _file = value; }
	}

	new public string name { get; private set; }
	#endregion

	#region Tab Properties
	[SerializeField]
	private Rect _tab;
	public Rect tab {
		get { return _tab; }
		set { _tab = value; }
	}

	private static int tabWidth {
		get {
			return (int)Mathf.Min((DatabaseEditor.instance.worksheetPos.width - tabSpacing * (DatabaseEditor.instance.worksheetCount - 1)) / DatabaseEditor.instance.worksheetCount,
			                      tabWidthMax);
		}
	}

	public static int tabHeight = 20;

	private static int tabWidthMax = 80;

	private static int tabSpacing = 3;
	#endregion

	#region State Properties
	private int index {
		get { return DatabaseEditor.instance.worksheets.IndexOf(this); }
	}

	public bool isChanged {
		get { return file != null && file.isChanged; }
	}
	#endregion

	#region Styling Properties
	private static GUIStyle _buttonStyle;
	private static GUIStyle buttonStyle {
		get {
			if (_buttonStyle == null) {
				_buttonStyle = new GUIStyle(GUI.skin.box);
				_buttonStyle.padding.bottom = 8;
				_buttonStyle.alignment = TextAnchor.MiddleLeft;
				_buttonStyle.active = new GUIStyleState();
				_buttonStyle.font = Resources.Load<Font>("Fira Mono/FiraMono-Regular");
				_buttonStyle.fontSize = 11;
				_buttonStyle.richText = true;
				_buttonStyle.wordWrap = false;
				_buttonStyle.normal.background = Resources.Load<Texture2D>("Images/box");	
			}
			return _buttonStyle;
		}
	}
	#endregion

	#region Unity Methods
	public void OnEnable() {
		hideFlags = HideFlags.HideAndDontSave;
		if (DatabaseEditor.instance != null) {
			tab = new Rect(DatabaseEditor.instance.worksheetPos.x + index * (tabWidth + tabSpacing),
			               DatabaseEditor.instance.worksheetPos.y,
			               tabWidth,
			               tabHeight * (4f / 3f));
		}
	}
	#endregion

	#region File Methods
	public Worksheet HasFile(DataFile file) {
		this.file = file;
		return this;
	}

	public Worksheet Save(FileInfo info = null) {
		if (file is JsonFile) {
			(file as JsonFile).Save(info);
		}
		if (info != null)
			name = Path.GetFileNameWithoutExtension(info.FullName);
		return this;
	}

	public Worksheet Open(FileInfo info) {
		file = CreateInstance<JsonFile>()
			.Load(info);
		return this
			.HasName(Path.GetFileNameWithoutExtension(info.FullName));
	}
	#endregion

	#region Name Methods
	public Worksheet HasName(string name) {
		this.name = name;
		return this;
	}
	#endregion

	#region Tab Methods
	public void SnapTabPosition() {
		Rect pos = tab;
		pos.x = DatabaseEditor.instance.worksheetPos.x;
		if (index != 0)
			pos.x += index * (tabWidth + tabSpacing);
		pos.width = tabWidth;
		tab = pos;
	}

	public void DragTabPosition(float delta) {
		Rect pos = tab;
		pos.x += delta;
		pos.x = Mathf.Clamp(pos.x, DatabaseEditor.instance.worksheetPos.x, DatabaseEditor.instance.worksheetPos.xMax - tab.width);
		tab = pos;
		if (index > 0) {
			Rect tabPrev = DatabaseEditor.instance.worksheets[index - 1].tab;
			if (tab.center.x < tabPrev.xMax) {
				Utility.Swap<Worksheet>(DatabaseEditor.instance.worksheets, index, index - 1);
				DatabaseEditor.instance.worksheets[index + 1].SnapTabPosition();
			}
		}
		if (index + 1 < DatabaseEditor.instance.worksheetCount) {
			Rect tabPrev = DatabaseEditor.instance.worksheets[index + 1].tab;
			if (tab.center.x > tabPrev.xMin) {
				Utility.Swap<Worksheet>(DatabaseEditor.instance.worksheets, index, index + 1);
				DatabaseEditor.instance.worksheets[index - 1].SnapTabPosition();
			}
		}
	}
	#endregion

	#region Rendering Methods
	public void Render(Rect position, bool active) {
		RenderTab(active);
		if (active)
			RenderSheet(position);
	}

	private void RenderTab(bool isActive) {
		GUI.backgroundColor = isActive ? new Color(96 / 255f, 125 / 255f, 139 / 255f) : new Color(69 / 255f, 90 / 255f, 100 / 255f);
		GUI.Label(tab,
		          string.Format("<b><color=#212121ff>{0}</color></b>", name),
		          buttonStyle);
		GUI.backgroundColor = Color.white;
	}

	private void RenderSheet(Rect position) {
		position.yMin += tabHeight;
		GUI.backgroundColor = new Color(207 / 255f, 216 / 255f, 220 / 255f);	// light primary color
		GUI.Box(position, "");
		GUI.backgroundColor = Color.white;
		GUILayout.BeginArea(position);
		if (file != null)
			file.Render();
		GUILayout.EndArea();
	}
	#endregion
}
