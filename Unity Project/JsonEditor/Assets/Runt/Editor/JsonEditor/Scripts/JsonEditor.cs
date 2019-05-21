using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utilities;

[System.Serializable]
public class DatabaseEditor : EditorWindow {
	#region Instance Property
	private static DatabaseEditor _instance;
	public static DatabaseEditor instance {
		get { return _instance; }
		private set { _instance = value; }
	}
	#endregion

	#region Mouse Control
	private IHasMouseInput _mouseInput;
	private IHasMouseInput mouseInput {
		get {
			if (_mouseInput == null) _mouseInput = new EditorMouseInput();
			return _mouseInput;
		}
	}
	#endregion

	#region Window Properties
	private int toolbarHeight { get { return 20; } }

	private Rect positionPrev { get; set; }

	private bool isWindowWidthChanged {
		get { return position.width != positionPrev.width; }
	}

	private bool isWindowHeightChanged {
		get { return position.height != positionPrev.height; }
	}

	private bool isWindowSizeChanged {
		get { return isWindowWidthChanged || isWindowHeightChanged; }
	}
	#endregion

	#region Sizing Properties
	private int windowBuffer {
		get { return 3; }
	}
	#endregion

	#region Event Properties
	private bool isRepainting {
		get { return Event.current.type == EventType.Repaint; }
	}

	private bool isLayouting {
		get { return Event.current.type == EventType.Layout; }
	}
	#endregion

	#region File Menu Properties
	private string[] _menuOptions = new string[] {
		"File"
	};
	private string[] menuOptions {
		get { return _menuOptions; }
	}

	private string[] _fileOptions = new string[] {
		"New",
		"Open",
		"Save",
		"Save As...",
		"Save All",
		"Close"
	};
	private string[] fileOptions {
		get { return _fileOptions; }
	}

	private Color highlightColor {
		get { return isMenuSelected ? highlightColorSelected : highlightColorUnselected; }
	}

	private Color highlightColorUnselected {
		get { return new Color(76f / 255f, 173f / 255f, 168f / 255f, 0.25f); }
	}

	private Color highlightColorSelected {
		get { return new Color(76f / 255f, 173f / 255f, 168f / 255f, 0.5f); }
	}

	private bool isMenuSelected { get; set; }

	private bool isOpeningMenu { get; set; }
	#endregion

	#region Worksheet Properties
	[SerializeField]
	private List<Worksheet> _worksheets;
	public List<Worksheet> worksheets {
		get {
			if (_worksheets == null) _worksheets = new List<Worksheet>();
			return _worksheets;
		}
		private set { _worksheets = value; }
	}

	public int worksheetCount {
		get { return worksheets.Count; }
	}

	//[SerializeField]
	private Worksheet _selectedWorksheet;
	public Worksheet selectedWorksheet {
		get { return _selectedWorksheet; } 
		private set { _selectedWorksheet = value; }
	}

	public Worksheet liftedWorksheet { get; private set; }

	private bool isWorksheetsOpen {
		get { return worksheets.Count > 0; }
	}

	private bool isWorksheetCountChanged {
		get { return worksheets.Count != worksheetCountPrev; }
	}

	private int worksheetCountPrev { get; set; }

	private int selectedWorksheetIndex {
		get { return worksheets.IndexOf(selectedWorksheet); }
	}

	public Rect worksheetPos {
		get {
			return new Rect(windowBuffer,
			                toolbarHeight,
			                position.width - windowBuffer * 2,
			                position.height - toolbarHeight - windowBuffer);
		}
	}
	#endregion

	#region Unity Methods
	[MenuItem("Window/Runt/Json Editor")]
	static void DisplayDataTableEditor() {
		instance = GetWindow<DatabaseEditor>();
		#if UNITY_5_2 || UNITY_5_3_OR_NEWER
		instance.titleContent = new GUIContent("Json Editor", Resources.Load<Texture2D>("Images/Runt-Logo-Mascot-Black"), "Create and modify Json data in editor.");
		#else
		instance.title = "Json Editor";
		#endif
		instance.wantsMouseMove = true;
	}

	private void Update() {
		Repaint();
	}

	private void OnGUI() {
		mouseInput.Update();
		ShowMenu();
		RenderWorksheets();
		positionPrev = position;
		worksheetCountPrev = worksheets.Count;
	}

	private void OnEnable() {
		instance = this;
	}

	private void OnDestroy() {
		for (int i = 0; i < worksheets.Count;) {
			if (!OnCloseClicked(worksheets[i]))
				i += 1;
		}
		if (worksheets.Count > 0) {
			instance = CreateInstance<DatabaseEditor>();
			instance.worksheets = worksheets;
			instance.selectedWorksheet = selectedWorksheet;
			instance.Show();
		}
	}
	#endregion

	#region Menu Methods
	private void ShowMenu() {
		#if UNITY_EDITOR_WIN
		bool isClicked = false;
		#endif
		GUILayout.BeginHorizontal(EditorStyles.toolbar);
		for (int i = 0; i < menuOptions.Length; i++) {
			#if UNITY_EDITOR_WIN
			isClicked =
			#endif
			ShowMenuOption(i);
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		#if UNITY_EDITOR_WIN
		// Unselect menu when clicking outside of menu
		if (mouseInput.isLeftDown && !isClicked) isMenuSelected = false;
		#endif
	}

	private bool ShowMenuOption(int index) {
		Rect labelRect = GUILayoutUtility.GetRect(new GUIContent(menuOptions[index]), EditorStyles.label, GUILayout.ExpandWidth(false));
		bool isClicked = ShowMenuOptionButton(index, labelRect);
		GUI.Label(labelRect, new GUIContent(menuOptions[index]));
		if (index < (menuOptions.Length - 1)) GUILayout.Space(4);
		return isClicked;
	}

	private bool ShowMenuOptionButton(int index, Rect position) {
		bool isClicked = false;
		int overflow = 4;
		Rect buttonRect = new Rect(position.x - overflow, position.y, position.width + overflow * 2, position.height);
		if (buttonRect.Contains(Event.current.mousePosition)) {
			if (mouseInput.isLeftDown) {
				isMenuSelected = !isMenuSelected;
				#if UNITY_EDITOR_WIN
				isClicked = true;
				#endif
			}
			GUI.backgroundColor = highlightColor;
			GUI.Box(buttonRect, "");
			GUI.backgroundColor = Color.white;
			if (!isRepainting) {
				Event.current.Use();
				DisplayMenuOptionMenu(index, buttonRect);
			}
		}
		return isClicked;
	}

	private void DisplayMenuOptionMenu(int index, Rect position) {
		// A frame is needed to highlight the menu selection before showing the generic menu popup
		if (isMenuSelected) {
			if (isOpeningMenu) {
				switch (menuOptions[index]) {
					case "File":
						ShowFileOptions(position);
						break;
				}
				#if UNITY_EDITOR_OSX
				isMenuSelected = false;
				#endif
			}
			isOpeningMenu = !isOpeningMenu;
		}
	}
	#endregion

	#region File Option Methods
	private void ShowFileOptions(Rect rect) {
		GenericMenu genericMenu = new GenericMenu();
		for (int i = 0; i < fileOptions.Length; i++) {
			switch(fileOptions[i]) {
				case "New":
					genericMenu.AddItem(new GUIContent(fileOptions[i]), false, OnFileOptionClicked, i);
					break;
				case "Open":
					genericMenu.AddItem(new GUIContent(fileOptions[i]), false, OnFileOptionClicked, i);
					genericMenu.AddSeparator("");
					break;
				case "Save":
				case "Save As...":
				case "Close":
					if (isWorksheetsOpen) genericMenu.AddItem(new GUIContent(fileOptions[i]), false, OnFileOptionClicked, i);
					else genericMenu.AddDisabledItem(new GUIContent(fileOptions[i]));
					break;
				case "Save All":
					if (isWorksheetsOpen) genericMenu.AddItem(new GUIContent(fileOptions[i]), false, OnFileOptionClicked, i);
					else genericMenu.AddDisabledItem(new GUIContent(fileOptions[i]));
					genericMenu.AddSeparator("");
					break;
			}
		}
		genericMenu.DropDown(rect);
	}

	private void OnFileOptionClicked(object index) {
		switch(fileOptions[(int)index]) {
		case "New":
			OnNewClicked();
			break;
		case "Open":
			OnOpenClicked();
			break;
		case "Save":
			OnSaveClicked();
			break;
		case "Save As...":
			OnSaveAsClicked();
			break;
		case "Save All":
			OnSaveAllClicked();
			break;
		case "Close":
			OnCloseClicked();
			break;
		default:
			Debug.LogError("Implement " + fileOptions[(int)index]);
			break;
		}
		#if UNITY_EDITOR_WIN
		isMenuSelected = false;
		#endif
	}

	private void OnNewClicked() {
		selectedWorksheet = CreateInstance<Worksheet>()
			.HasFile(CreateInstance<JsonFile>())
			.HasName(GetNewWorksheetName());
		worksheets.Add(selectedWorksheet);
		foreach (Worksheet sheet in worksheets)
			sheet.SnapTabPosition();
		#if UNITY_EDITOR_WIN
		isMenuSelected = false;
		#endif
	}

	private void OnOpenClicked() {
		string path = EditorUtility.OpenFilePanel("Open Json File", "Assets", "json");
		if (!string.IsNullOrEmpty(path)) {
			FileInfo info = new FileInfo(path);
			if (!IsFileOpen(info)) {
				selectedWorksheet = CreateInstance<Worksheet>()
					.Open(info);
				worksheets.Add(selectedWorksheet);
				foreach (Worksheet sheet in worksheets)
					sheet.SnapTabPosition();
			}
		}
		#if UNITY_EDITOR_WIN
		isMenuSelected = false;
		#endif
	}

	private bool OnSaveClicked(Worksheet worksheet = null) {
		bool result = true;
		if (worksheet == null)
			worksheet = selectedWorksheet;
		if (worksheet.file.info != null)
			selectedWorksheet.Save();
		else
			result = OnSaveAsClicked(worksheet);
		#if UNITY_EDITOR_WIN
		// Unselect menu when selecting an option
		isMenuSelected = false;
		#endif
		AssetDatabase.Refresh();
		return result;
	}

	private bool OnSaveAsClicked(Worksheet worksheet = null) {
		bool result = true;
		if (worksheet == null) worksheet = selectedWorksheet;
		string path = EditorUtility.SaveFilePanel("Save As...", "Assets", worksheet.name, "json");
		if (!string.IsNullOrEmpty(path))
			worksheet.Save(new FileInfo(path));
		else
			result = false;
		#if UNITY_EDITOR_WIN
		// Unselect menu when selecting an option
		isMenuSelected = false;
		#endif
		AssetDatabase.Refresh();
		return result;
	}

	private void OnSaveAllClicked() {
		for (int i = 0; i < worksheets.Count; i++) {
			if (worksheets[i].file.info != null)
				worksheets[i].Save();
			else
				OnSaveAsClicked(worksheets[i]);
		}
		#if UNITY_EDITOR_WIN
		// Unselect menu when selecting an option
		isMenuSelected = false;
		#endif
	}

	private bool OnCloseClicked(Worksheet worksheet = null) {
		bool result = true;
		if (worksheet == null)
			worksheet = selectedWorksheet;
		if (worksheet.isChanged) {
			int saveSelection = EditorUtility.DisplayDialogComplex(string.Format("Closing \"{0}\"", worksheet.name),
			                                                       "Save Before Closing?",
			                                                       "Save",
			                                                       "Cancel",
			                                                       "Discard");
			switch (saveSelection) {
			case 0: // Save
				OnSaveClicked(worksheet);
				break;
			case 1:	// Cancel
				result = false;
				break;
			}
		}
		if (result) {
			int worksheetSelection = worksheets.IndexOf(worksheet);
			worksheets.Remove(worksheet);
			if (isWorksheetsOpen) {
				worksheetSelection = Mathf.Clamp(worksheetSelection - 1, 0, worksheets.Count);
				selectedWorksheet = worksheets[worksheetSelection];
				foreach (Worksheet sheet in worksheets)
					sheet.SnapTabPosition();
			}
			else
				selectedWorksheet = null;
		}
		#if UNITY_EDITOR_WIN
		// Unselect menu when selecting an option
		isMenuSelected = false;
		#endif
		return result;
	}

	private string GetNewWorksheetName() {
		string name = "Worksheet";
		int counter = 0;
		for (int i = 0; i < worksheets.Count;) {
			if (string.Equals(name, worksheets[i].name)) {
				counter += 1;
				name = string.Format("Worksheet{0}", counter);
				i = 0;
			} else
				i++;
		}
		return name;
	}

	private bool IsFileOpen(FileInfo info) {
		bool result = false;
		foreach (Worksheet worksheet in worksheets) {
			if (worksheet.file.info != null &&
			    worksheet.file.info.FullName.Equals(info.FullName)) {
				selectedWorksheet = worksheet;
				result = true;
			}
		}
		return result;
	}
	#endregion

	#region Worksheet Methods
	private void RenderWorksheets() {
		foreach (Worksheet sheet in worksheets) {
			if (isTabResizeTriggered)
				sheet.SnapTabPosition();
			if (mouseInput.isLeftDown &&
			    sheet.tab.Contains(mouseInput.position)) {
				selectedWorksheet = liftedWorksheet = sheet;
				Event.current.Use();
			}
			if (!sheet.Equals(selectedWorksheet))
				sheet.Render(worksheetPos, false);
		}
		if (liftedWorksheet != null)
			ProcessDragAndDrop();
		if (selectedWorksheet != null) {
			selectedWorksheet.Render(worksheetPos, true);
		}
	}

	private bool isTabResizeTriggered {
		get { return isWorksheetsOpen && isWindowWidthChanged; }
	}

	private void ProcessDragAndDrop() {
		if (mouseInput.isLeftUp) {
			liftedWorksheet.SnapTabPosition();
			liftedWorksheet = null;
		}
		else if (isLayouting) {
			liftedWorksheet.DragTabPosition(mouseInput.delta.x);
		}
	}
	#endregion
}