
#if UNITY_EDITOR 

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

using ControlFreak2Editor.Internal;

namespace ControlFreak2Editor.ScriptConverter
{
public class ScriptConverter : EditorWindow
	{

	private TreeView 
		treeView;	

	private List<ScriptElem>
		scriptElems;
		
	public bool 
		hideEnabledScripts,
		hideIgnoredScripts;
	
	private const string
		DIALOG_TITLE	= "Control Freak 2 Script Converter";


	// ----------------------
	static private string[] mDirectoryBlackList = new string[]
		{	
		"/Editor/",								// Any editor scripts.
		"/Plugins/Control-Freak/",			// CF 1.x
		"/Assets/Control-Freak-Demos/",	// CF 1.x Demos
		"/Plugins/Control-Freak-2/",		// CF 2.x
		//"/Assets/PlayMaker/",				// Playmaker built-in actions
		"/CrossPlatformInput/",				// Unity Cross-Platform Input
	
		};


		
	// ------------------
	public ScriptConverter() : base()
		{
		this.minSize = new Vector2(700, 600);
		}


	// ----------------------
	[MenuItem("Control Freak 2/CF2 Script Converter")]
	static void ShowScriptConverter()
		{

		//ScriptConverter w = GetWindow<ScriptConverter>(true, "CF2 Converter", true);
		ScriptConverter w = ScriptableObject.CreateInstance<ScriptConverter>(); 
		if (w != null)
			{
			w.BuildTreeView();

			CFEditorUtils.SetWindowTitle(w, new GUIContent("CF2 Script Converter"));

			w.ShowUtility();
			w.Focus();
			w.Repaint();
			}
		}


	// ---------------------
	void OnEnable()
		{
		this.treeView = new TreeView(); 
		this.scriptElems = new List<ScriptElem>(128);
			
		this.Repaint();

		}
		
	// ---------------
	void OnDisable()
		{
		}

		

	private bool scriptInfoVisible = false;

	// ---------------------
	void OnGUI()
		{
		const float TOOLBAR_HEIGHT = 70;
		const float SHOW_SCRIPT_HEIGHT = 72;
			

		GUILayout.Box(GUIContent.none, CFEditorStyles.Inst.headerScriptConverter, GUILayout.ExpandWidth(true));

		// Draw the toolbar...

		EditorGUILayout.BeginHorizontal(CFEditorStyles.Inst.toolbarBG, GUILayout.Height(TOOLBAR_HEIGHT));

		if (GUILayout.Button("Convert selected scripts", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
			{
			if (this.ConvertScripts(this.scriptElems))
				{ 
				this.Close();
				AssetDatabase.Refresh(ImportAssetOptions.Default);
				return;
				}
			}	

		if (GUILayout.Button("Reload Scripts", GUILayout.ExpandHeight(true)))
			{
			this.BuildTreeView();
			}



		EditorGUILayout.BeginVertical(CFEditorStyles.Inst.transpSunkenBG, GUILayout.Width(150));

			//this.hideIgnoredScripts = CFGUI.PushButton(new GUIContent(CFEditorStyles.Inst.texIgnore, "Hide scripts marked as IGNORED."), this.hideIgnoredScripts, 
			//	CFEditorStyles.Inst.buttonStyle, GUILayout.Width(24));
	
			this.hideIgnoredScripts = EditorGUILayout.ToggleLeft(new GUIContent("Hide Ignored scripts."), this.hideIgnoredScripts);
			
			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button(new GUIContent("All", "Enable all scripts."), GUILayout.Height(22), GUILayout.Width(50)))
				this.EnableAllScripts(true);					

			if (GUILayout.Button(new GUIContent("None", "Uncheck all scripts."), GUILayout.Height(22), GUILayout.Width(50)))
				this.EnableAllScripts(false);					

			if (GUILayout.Button(new GUIContent("Ignore", "Mark all script as ignored."), GUILayout.Height(22), GUILayout.Width(50)))
				this.MarkAllScriptsAsIgnored();					

			if (GUILayout.Button(new GUIContent("Reset", "Reset Conversion Action for all scripts."), GUILayout.Height(22), GUILayout.Width(50)))
				this.ResetAllScriptConversionAction();
			
			EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndVertical();

			
/*		Color initialBgColor = GUI.backgroundColor;
		

		EditorGUILayout.BeginVertical(CFEditorStyles.Inst.transpSunkenBG, GUILayout.Width(160));
		//GUILayout.Label("Enable:", CFEditorStyles.Inst.centeredTextTranspBg); //, GUILayout.Width(50));

			EditorGUILayout.BeginHorizontal();

			GUI.backgroundColor = Color.Lerp(Color.green, Color.white, 0.8f);			
	
			if (GUILayout.Button(new GUIContent("ALL", "Enable all scripts."), GUILayout.Height(22), GUILayout.Width(32)))
				this.EnableAllScripts(true);					
			if (GUILayout.Button(new GUIContent(CFEditorStyles.Inst.texOk, "Enable all OK scripts."), GUILayout.Width(26)))
				this.EnableScriptsByConvState(ConvertedScript.ConvState.OK, true);					
			if (GUILayout.Button(new GUIContent(CFEditorStyles.Inst.texWarning, "Enable all scripts with warnings."), GUILayout.Width(26)))
				this.EnableScriptsByConvState(ConvertedScript.ConvState.OK_WITH_WARNINGS, true);					
			if (GUILayout.Button(new GUIContent(CFEditorStyles.Inst.texError, "Enable all scripts with errors."), GUILayout.Width(26)))
				this.EnableScriptsByConvState(ConvertedScript.ConvState.PROBLEMATIC, true);					
			if (GUILayout.Button(new GUIContent(CFEditorStyles.Inst.texIgnore, "Enable all IGNORED scripts."), GUILayout.Width(26)))
				this.EnableIgnoredScripts(true);
					
			EditorGUILayout.EndHorizontal();
		//EditorGUILayout.EndVertical();
		

		GUI.backgroundColor = initialBgColor;

		//EditorGUILayout.BeginVertical(CFEditorStyles.Inst.transpSunkenBG);
		//GUILayout.Label("Disable:", CFEditorStyles.Inst.centeredTextTranspBg); //, GUILayout.Width(50));

			EditorGUILayout.BeginHorizontal();
	
			GUI.backgroundColor = Color.Lerp(Color.red, Color.white, 0.8f);			
	
			if (GUILayout.Button(new GUIContent("ALL", "Disable all scripts."), GUILayout.Height(22), GUILayout.Width(32)))
				this.EnableAllScripts(false);					
			if (GUILayout.Button(new GUIContent(CFEditorStyles.Inst.texOk, "Disable all OK scripts."), GUILayout.Width(26)))
				this.EnableScriptsByConvState(ConvertedScript.ConvState.OK, false);					
			if (GUILayout.Button(new GUIContent(CFEditorStyles.Inst.texWarning, "Disable all scripts with warnings."), GUILayout.Width(26)))
				this.EnableScriptsByConvState(ConvertedScript.ConvState.OK_WITH_WARNINGS, false);					
			if (GUILayout.Button(new GUIContent(CFEditorStyles.Inst.texError, "Disable all scripts with errors."), GUILayout.Width(26)))
				this.EnableScriptsByConvState(ConvertedScript.ConvState.PROBLEMATIC, false);					
			if (GUILayout.Button(new GUIContent(CFEditorStyles.Inst.texIgnore, "Disable all IGNORED scripts."), GUILayout.Width(26)))
				this.EnableIgnoredScripts(false);
					
			EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

		GUI.backgroundColor = initialBgColor;			


*/

		EditorGUILayout.EndHorizontal();
	


		// Draw the tree view..


		Color c = GUI.backgroundColor;
		GUI.backgroundColor *= 0.5f;
		EditorGUILayout.BeginVertical(CFEditorStyles.Inst.whiteSunkenBG, //CFEditorStyles.Inst.treeViewBG, 
			GUILayout.ExpandWidth(true),
			GUILayout.Height(Mathf.Max(100, ((this.position.height - (SHOW_SCRIPT_HEIGHT + TOOLBAR_HEIGHT)) * (this.scriptInfoVisible ? 0.5f : 1)))));

		GUI.backgroundColor = c;

		//this.treeElemDrawCount = 0;


		if (this.treeView.IsEmpty())
			{
			//if (GUILayout.Button("Collect scripts..."))
			//	{
			//	this.BuildTreeView();
			//	}
				

			GUILayout.Box("No relevant scripts found in this project!\nUse Script Backup Restore tool if you want to step back...", 
				CFEditorStyles.Inst.centeredTextTranspBG, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			}
		else
			{
			this.treeView.DrawTreeGUI();
				
			}

		EditorGUILayout.EndVertical();


		// Draw script info...
			
		EditorGUILayout.BeginVertical(CFEditorStyles.Inst.transpSunkenBG, GUILayout.ExpandHeight(true)); //.Height(Mathf.Max(100, (this.position.height - 100))));
			

		if (GUILayout.Button((this.scriptInfoVisible ? new GUIContent("Hide Script Info", CFEditorStyles.Inst.texMoveDown) : 
			new GUIContent("Show script Info.", CFEditorStyles.Inst.texMoveUp)) )) //, CFEditorStyles.Inst.whiteBevelBG))
			this.scriptInfoVisible = !this.scriptInfoVisible;


		if (this.scriptInfoVisible)
			{
			
			ScriptElem selScript = this.treeView.selectedElem as ScriptElem;
	
			if (selScript != null)
				{
				selScript.DrawScriptInfoGUI();
				}			
			else
				{
				GUILayout.Box("No script selected.", CFEditorStyles.Inst.centeredTextTranspBG, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				}
				
			}
		EditorGUILayout.EndVertical();
		}
		

		

	// --------------------
	private void SaveCode(string path, string code)
		{
		string dir = Path.GetDirectoryName(path);
		if (!Directory.Exists(dir))
			Directory.CreateDirectory(dir);

		File.WriteAllText(path, code, Encoding.UTF8);
		}
		


	// --------------------
	private bool ConvertScripts(List<ScriptElem> scriptElems)
		{	
		int scriptsToConvertCount = 0;
		int scriptsToIgnoreCount = 0;

		//int enabledScriptCount = 0;
		foreach (ScriptElem s in scriptElems) 
			{	
			if (s.actionType == ActionType.Convert)
				++scriptsToConvertCount;

			if ((s.actionType == ActionType.TagAsIgnored) && !s.script.IsTaggedAsIgnored())
				++scriptsToIgnoreCount;

			//if (s.enabled)
			//	++enabledScriptCount;
			}
			
		if ((scriptsToConvertCount == 0) && (scriptsToIgnoreCount == 0))
			{
			EditorUtility.DisplayDialog(DIALOG_TITLE, "No scripts to convert or to tag as ignred!", "OK");
			return false;
			}

		string baseDir = string.Empty;
			
		StringBuilder backupLog = new StringBuilder(4096);

		try {
		
			baseDir = ScriptBackup.CreateBackupFolder();

			string originalFilesDir = Path.Combine(baseDir, ScriptBackup.ORIGINAL_FILES_FOLDER);
			string modifiedFilesDir = Path.Combine(baseDir, ScriptBackup.MODIFIED_FILES_FOLDER);
				
			List<string> modifiedFileList = new List<string>(scriptsToConvertCount + scriptsToIgnoreCount);


			
//Debug.Log("base dir: [" + baseDir + "]\n orig[" + originalFilesDir + "] \n modifed[" + modifiedFilesDir + "]");
				
			int convertedScriptCount = 0;
			int ignoredScriptCount = 0;

			foreach (ScriptElem s in scriptElems)
				{
				//if (!s.enabled)
				if ((s.actionType == ActionType.Skip) || ((s.actionType == ActionType.TagAsIgnored) && s.script.IsTaggedAsIgnored()))	
					continue;
				
				string originalDstFile	= (originalFilesDir + s.sourceRelativeFilePath);
				string outputDstFile	= (modifiedFilesDir + s.sourceRelativeFilePath);
					
				string originalCode		= s.script.GetOriginalCode();
			 	this.SaveCode(originalDstFile,	originalCode);
				
				modifiedFileList.Add(s.sourceRelativeFilePath);


				if (s.actionType == ActionType.Convert)
					{
			
	
					string convertedCode	= s.script.GetConvertedCode();
		
					backupLog.Append("Converting script [" + (convertedScriptCount + 1) + " / " + scriptsToConvertCount + 
						"]\t[" + s.sourceRelativeFilePath + "]\n"); 
						//"\tInput  : " + originalCode.Length + " bytes \t[" + originalDstFile + "]\n" +
						//"\tOutput : " + convertedCode.Length + " bytes \t[" + outputDstFile + "]\n");
						
					backupLog.Append("\tFragments : " + s.script.fragments.Count + "\n");
			
					for (int fi = 0; fi < s.script.fragments.Count; ++fi)
						{
						backupLog.Append("\t\t[" + fi.ToString().PadLeft(2) + "] : " + s.script.fragments[fi].GetLogLine() + "\n");
						}
	
					backupLog.Append("\n");
	
	
				 	this.SaveCode(outputDstFile,	convertedCode);

					++convertedScriptCount;
					}

				// Tag as ignored...

				else if (s.actionType == ActionType.TagAsIgnored)
					{
					string convertedCode	= s.script.GetOriginalCodeTaggedAsIgnored();
		
					backupLog.Append("Tagging script as IGNORED [" + (ignoredScriptCount + 1) + " / " + scriptsToIgnoreCount + "]\t[" + s.sourceRelativeFilePath + "]\n"); 
						//"\tInput  : " + originalCode.Length + " bytes \t[" + originalDstFile + "]\n" +
						//"\tOutput : " + convertedCode.Length + " bytes \t[" + outputDstFile + "]\n");
						
					backupLog.Append("\n");
	
				 	this.SaveCode(outputDstFile,	convertedCode);
						
					++ignoredScriptCount;
					}

				}

	
			backupLog.Append("\n" + (ignoredScriptCount + convertedScriptCount)  + " scripts modified!\n");
			

			// Copy modified scripts to /Assets/
	

			backupLog.Append("Overwriting actual scripts...\n");
				
			string assetsPath = CFEditorUtils.GetAssetsPath();

			for (int i = 0; i < modifiedFileList.Count; ++i)
				{
				string outputDstFile	= (modifiedFilesDir + modifiedFileList[i]);
				string fileToReplace	= (assetsPath + modifiedFileList[i]);
	
				backupLog.Append("\t[" + i + " / " + modifiedFileList.Count + "] from  ["  + outputDstFile + "] to [" +  fileToReplace + "]\n");

				File.Copy(outputDstFile, fileToReplace, true);
				}
			backupLog.Append("\nOverwriting COMPLETED!\n");
				

			// Save log..
				
			File.WriteAllText(Path.Combine(baseDir, ScriptBackup.CONVERSION_LOG_FILENAME), backupLog.ToString());
				
			
			EditorUtility.DisplayDialog(DIALOG_TITLE, "" + 
				((convertedScriptCount == 0) ? "" : (convertedScriptCount.ToString() + " scripts converted!\n")) +
				((ignoredScriptCount == 0) ? "" : (ignoredScriptCount.ToString() + " scripts tagged!\n")) , "OK");
			
			} 
		catch (System.Exception error)
			{
			string errorMsg = "Error while performing backup :\n\n" + error.Message;
			EditorUtility.DisplayDialog(DIALOG_TITLE, errorMsg, "OK");
			Debug.LogError(errorMsg);

			if ((baseDir != null) && (baseDir.Length > 0) && Directory.Exists(baseDir))
				{ 
//Debug.Log("Deleting temp dir : [" + baseDir + "]");
				Directory.Delete(baseDir, true);
				}
				
			Debug.Log("LOG up to that point:\n\n" + backupLog.ToString());

			return false;  
			}
		
		return true;
		}



	// ---------------------
	private void BuildTreeView()
		{
		EditorUtility.DisplayProgressBar(DIALOG_TITLE, "Collectiong files...", 0);

		List<string> scriptFiles = GetAllScripts();

		this.scriptElems.Clear();
		this.treeView = new TreeView();
			
		for (int i = 0; i < scriptFiles.Count; ++i)
			{	
			string scriptPath = scriptFiles[i];
			EditorUtility.DisplayProgressBar(DIALOG_TITLE, "Parsing [" + scriptPath.Replace(Application.dataPath, "") + "].", 
				Mathf.Lerp(0.1f, 1.0f, ((float)i / (float)(scriptFiles.Count - 1)))); 

			this.CreateScriptElem(scriptPath);
			}
			
		EditorUtility.ClearProgressBar();

		this.treeView.InvalidateUpwards(true);
		}

		

	// ----------------
	private void EnableAllScripts(bool state)
		{
		foreach (ScriptElem script in this.scriptElems)
			script.SetState((int)ElemStateId.Action, (int)(state ? ScriptConverter.ActionType.Convert : ScriptConverter.ActionType.Skip));
	
		this.treeView.InvalidateUpwards(true);
		}
		

	// ---------------------
	private void MarkAllScriptsAsIgnored()
		{
		foreach (ScriptElem script in this.scriptElems)
			script.SetState((int)ElemStateId.Action, (int)ScriptConverter.ActionType.TagAsIgnored);
	
		this.treeView.InvalidateUpwards(true);
		}

	// --------------------
	private void ResetAllScriptConversionAction()
		{
		foreach (ScriptElem script in this.scriptElems)
			{	
			if (script.script.IsTaggedAsIgnored())
				script.SetState((int)ElemStateId.Action, (int)ScriptConverter.ActionType.TagAsIgnored);

			else if (script.script.convState == ConvertedScript.ConvState.Ok)
				script.SetState((int)ElemStateId.Action, (int)ScriptConverter.ActionType.Convert);

			else
				script.SetState((int)ElemStateId.Action, (int)ScriptConverter.ActionType.Skip);
			}

		this.treeView.InvalidateUpwards(true);
		}
	


			


		/*
	// ----------------
	private void EnableIgnoredScripts(bool state)
		{
		foreach (ScriptElem script in this.scriptElems)
			{
			if (script.tagAsIgnored)
				script.SetState((int)ElemStateId.SELECTED, (state ? 1 : 0));
			}

		this.treeView.InvalidateUpwards(true);
		}


	// ----------------
	private void EnableScriptsByConvState(ConvertedScript.ConvState convState, bool state)
		{
		foreach (ScriptElem script in this.scriptElems)
			{
			if (script.script.convState == convState)
				script.SetState((int)ElemStateId.SELECTED, (state ? 1 : 0));
			}

		this.treeView.InvalidateUpwards(true);
		}
*/



	// ---------------
	private TreeViewElem CreateFolderElems(string path)
		{
		int startPos = 0;
		int sepPos = 0;
			
		TreeViewElem parent = this.treeView;

		for (; (startPos < path.Length); startPos = (sepPos + 1))
			{
			sepPos = path.IndexOf("/", startPos);
			if (sepPos == startPos)
				continue;
				
			if (sepPos < 0) 
				sepPos = path.Length;

			string folderName = path.Substring(startPos, (sepPos - startPos));

//Debug.Log("\tCreate sub folder [" + folderName + "] of [" + path + "]");

			TreeViewElem folderElem = parent.Find(folderName) as FolderElem;
			if (folderElem == null)
				{
//Debug.Log("\tNOT found...");
				folderElem = new FolderElem(this); //.treeView);
			
				folderElem.name = folderName;
				folderElem.parent = parent;

				parent.AddChild(folderElem);
				}	
			else
				{
//Debug.Log("\tFOUND!");

				}
				

			parent = folderElem;
			}

		return parent;		
		}

		
	// -------------
	private ScriptElem CreateScriptElem(string path)
		{

		// Create script elem...

		ScriptElem scriptElem = ScriptElem.Create(this, path);
	
		if (scriptElem == null)
			return null;	


	
// Cut off project root path...
			
		string rootPath = Application.dataPath.Replace("\\", "/");
	
		string relPath = path.Replace("\\", "/");
			
		if (relPath.IndexOf(rootPath) == 0)
			relPath = relPath.Substring(rootPath.Length);			// Creaet sub folders...
	

//Debug.Log("Rel[" + relPath + "] dir[" + Path.GetDirectoryName(relPath) + " full[" + path + "]");

		TreeViewElem folderElem = this.CreateFolderElems(Path.GetDirectoryName(relPath));




		if (scriptElem != null)
			{
			this.scriptElems.Add(scriptElem);	
	
			if (folderElem != null)
				folderElem.AddChild(scriptElem);
			}

		return scriptElem;	
		}



	// -------------------
	private enum ElemStateId
		{
		Action,
		//SELECTED,
		//IGNORED,
		ConvState
		}

	private const int UNDEFINED_STATE	= -2;
	private const int MIXED_STATE		= -1;
		

		static private Color BG_COLOR_SEL	= new Color(1, 1, 1, 0.3f);
		static private Color BG_COLOR_UNSEL = new Color(1, 1, 1, 0);
		static private Color
			BG_COLOR_OK			= new Color(0, 1, 0, 0.2f),
			BG_COLOR_ERROR		= new Color(1, 0, 0, 0.2f),
			BG_COLOR_OK_WARNING = new Color(1, 1, 0, 0.2f),	
			BG_COLOR_IGNORED	= new Color(0, 0, 0, 0.2f);

			//TEXT_COLOR_SEL		= Color.white,
			//TEXT_COLOR_UNSEL	= Color.black;



			
		// ----------------------
		static public void StartTreeViewElemHorizontal(TreeView view, TreeViewElem elem)
			{
			Color initialBgColor = GUI.backgroundColor;

			bool isSelected = (view.selectedElem == elem);

			Color bgColor = (isSelected ? BG_COLOR_SEL : BG_COLOR_UNSEL); //this.bgColor);

			if ((view.treeElemDrawCount & 1) == 1) 
				bgColor = Color.Lerp(bgColor, Color.white, 0.1f);

			GUI.backgroundColor = bgColor;
	
			EditorGUILayout.BeginHorizontal(isSelected ? CFEditorStyles.Inst.treeViewElemSelBG : CFEditorStyles.Inst.treeViewElemBG, 
				GUILayout.ExpandWidth(true));

			GUI.backgroundColor = initialBgColor;
			}

		

	// -----------------
	public enum ActionType
		{
		Skip,
		Convert,
		TagAsIgnored
		}

	// ---------------------
	private class FolderOrScriptElem : TreeViewElem
		{
		protected ScriptConverter cc;
		protected Color			bgColor;

		//protected int cachedIgnoredState = 0;
		//protected int cachedEnabledState = 0;
		protected int cachedActionState = 0;
		protected int cachedConvState = 0;
			

		// -------------------------				
		protected virtual void OnDrawElemLabel(float indent) {}


		// --------------
		public FolderOrScriptElem(ScriptConverter cc) : base(cc.treeView)
			{
			this.cc = cc;
			this.bgColor = new Color(1,1,1, 0); 
			}


		// -------------------
		override protected void OnInvalidate()
			{
			//this.cachedEnabledState = this.GetState((int)ElemStateId.SELECTED,	UNDEFINED_STATE, 0);
			//this.cachedIgnoredState = this.GetState((int)ElemStateId.IGNORED,	UNDEFINED_STATE, 0);
			this.cachedActionState	= this.GetState((int)ElemStateId.Action,	UNDEFINED_STATE, 0);
			this.cachedConvState	= this.GetState((int)ElemStateId.ConvState,UNDEFINED_STATE, (int)ConvertedScript.ConvState.NothingChanged);
				
//Debug.Log("Invalidate[" + this.name + "]\tEN: " + this.cachedEnabledState + " \tCNV: " + this.cachedConvState);

	
			this.bgColor = new Color(1, 1, 1, 0);				

			//if (this.cachedIgnoredState == 1)
			if (this.cachedActionState == (int)ActionType.TagAsIgnored)
				{
				this.bgColor = BG_COLOR_IGNORED;
				}
			else 
				{
				switch (this.cachedConvState)
					{
					case (int)ConvertedScript.ConvState.Ok					: this.bgColor = BG_COLOR_OK; break; 
					case (int)ConvertedScript.ConvState.OkWithWarnings	: this.bgColor = BG_COLOR_OK_WARNING; break; 
					case (int)ConvertedScript.ConvState.Problematic			: this.bgColor = BG_COLOR_ERROR; break; 
					}
				}
			}
			

		

		// -------------------
		override protected void OnDrawGUI(float indent)
			{
/*
			Color initialBgColor = GUI.backgroundColor;
			Color initialContentColor = GUI.color;

			bool isSelected = (this.view.selectedElem == this);

			Color bgColor = (isSelected ? BG_COLOR_SEL : BG_COLOR_UNSEL); //this.bgColor);

			if ((this.view.treeElemDrawCount & 1) == 1) 
				bgColor = Color.Lerp(bgColor, Color.white, 0.1f);

			GUI.backgroundColor = bgColor;
			GUI.contentColor = (isSelected ? TEXT_COLOR_SEL : TEXT_COLOR_UNSEL);
	
			EditorGUILayout.BeginHorizontal(isSelected ? CFEditorStyles.Inst.treeViewElemSelBG : CFEditorStyles.Inst.treeViewElemBG, 
				GUILayout.ExpandWidth(true));

			GUI.backgroundColor = initialBgColor;
	*/

			StartTreeViewElemHorizontal(this.view, this);			

	
			this.OnDrawElemLabel(indent);

			//GUI.color = initialContentColor;
				

			//++this.cc.treeElemDrawCount;

			EditorGUILayout.EndHorizontal();
			}

		}


	// ----------------------
	private class FolderElem : FolderOrScriptElem		
		{
		private IntPopupEx 
			actionTypePopup;			

		// -------------------
		public FolderElem(ScriptConverter cc) : base(cc)
			{
			this.actionTypePopup = new IntPopupEx();
			}

			
		// -----------------
		override protected bool IsVisible()
			{
			if (this.cachedActionState == (int)ActionType.Convert)
				return (!this.cc.hideEnabledScripts);
	
			if (this.cachedActionState == (int)ActionType.TagAsIgnored)
				return (!this.cc.hideIgnoredScripts);

			return true;
			}


		// -----------------
//		override protected void OnDrawGUI(float indent)
		override protected void OnDrawElemLabel(float indent)

			{
			//EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

			GUILayout.Space(indent);
	
	
		
/*
			int enabledTriState = CFGUI.TriState("", "Enable all scripts in this folder?", 
				this.cachedEnabledState, EditorStyles.toggle, EditorStyles.toggle, GUILayout.Width(16)); //CFEditorStyles.Inst.checkbox, CFEditorStyles.Inst.checkbox);

			if (enabledTriState != this.cachedEnabledState)
				{
				this.SetState((int)ElemStateId.SELECTED, enabledTriState);
				}
*/

			int actionType = this.actionTypePopup.Draw(this.cachedActionState);
			if (actionType != this.cachedActionState)
				{
				this.SetState((int)ElemStateId.Action, actionType);
				}
			
			//GUILayout.Space(10);

			//DrawConvStateIcon(this.cachedConvState);

			//GUILayout.Space(indent + 10); 

			//this.isFoldedOut = GUILayout.Toggle(this.isFoldedOut, "", CFEditorStyles.Inst.foldout);
				

			GUILayout.Box(GUIContent.none, CFEditorStyles.Inst.iconFolder);

			//DrawConvStateIcon(this.cachedConvState);

			Color initialBGColor = GUI.backgroundColor;
			//GUI.backgroundColor = new Color(1,1,1,0); //this.bgColor;
			GUI.backgroundColor = this.bgColor;
 
			if (GUILayout.Button(this.name, CFEditorStyles.Inst.treeViewElemLabel, GUILayout.ExpandWidth(true)))
				this.view.Select(this);	

			GUI.backgroundColor = initialBGColor;

			//EditorGUILayout.EndHorizontal();
			}
		}


		

	// --------------------
	public class IntPopupEx
		{	
		//private static GUIContent[]
		//	mContentArray;
		//private static int[] 
		//	mValueArray;

		private bool
			menuSelected;
		private int
			menuSelectedValue;


		// -------------------
		public int Draw(int actionType)
			{							
				
			GUIContent buttonContent = (
				(actionType == (int)ActionType.Convert) 		? new GUIContent(CFEditorStyles.Inst.checkboxCheckedTex, "Convert this element.") :
				(actionType == (int)ActionType.Skip) 			? new GUIContent(CFEditorStyles.Inst.checkboxUncheckedTex, "Skip this element.") :
				(actionType == (int)ActionType.TagAsIgnored)	? new GUIContent(CFEditorStyles.Inst.texIgnore, "Tag this element as IGNORED.") :
																  new GUIContent(CFEditorStyles.Inst.checkboxMixedTex, "This folder contains mixed elements."));

			if (GUILayout.Button(buttonContent, CFEditorStyles.Inst.emptyStyle, GUILayout.Width(16)))
				{
				// Simple toggle...

				if (((actionType == (int)ActionType.Convert) || (actionType == (int)ActionType.Skip)) && (Event.current.button != 1))
					{
					actionType = ((actionType == (int)ActionType.Convert) ? (int)ActionType.Skip : (int)ActionType.Convert); 
					}

				// Show context menu in complicated situation or on RMB click...

				else
					{
	
					GenericMenu menu = new GenericMenu();
						
					menu.AddItem(new GUIContent("Convert (Check)"), (actionType == (int)ActionType.Convert), this.OnMenuSelect, (object)ActionType.Convert);
					menu.AddItem(new GUIContent("Skip (Uncheck)"), (actionType == (int)ActionType.Skip), this.OnMenuSelect, (object)ActionType.Skip);
					menu.AddSeparator("");
					menu.AddItem(new GUIContent("Tag as Ignored"), (actionType == (int)ActionType.TagAsIgnored), this.OnMenuSelect, (object)ActionType.TagAsIgnored);
	
					menu.ShowAsContext();
					}
				}
			
			if (this.menuSelected)
				{
				this.menuSelected = false;
				return this.menuSelectedValue;
				}

			return actionType;
			}

		// -----------------
		private void OnMenuSelect(object sel)
			{
			this.menuSelected = true;
			this.menuSelectedValue = (int)sel; 
			}

		}




	// --------------------
	private class ScriptElem : FolderOrScriptElem //TreeViewElem
		{
//		public ScriptConverter	cc;
		public ConvertedScript	script;			
			
		public ActionType
			actionType;

		public IntPopupEx
			actionTypePopup;

		//public bool	
		//	enabled,
		//	tagAsIgnored;

		public string			
			sourceFilePath,
			sourceRelativeFilePath,
			outputFilePath;
		




		// -------------------
		public ScriptElem(ScriptConverter cc) : base(cc)
			{
			this.actionTypePopup = new IntPopupEx();
			}


		// -----------------
		static public ScriptElem Create(ScriptConverter cc, string filePath)
			{
//Debug.Log("Processing [" + filePath + "]...");
			ConvertedScript script = new ConvertedScript();
			script.ProcessTextFile(filePath);
				
			if (!script.IsRelevant())
				{
//Debug.Log("\tSCRIPT IS NOT RELEVANT!!!");
				return null;
				}
	
//Debug.Log("\tdone.");

			ScriptElem scriptElem = new ScriptElem(cc); //.treeView);
				
			scriptElem.name = Path.GetFileName(filePath);
			scriptElem.script = script;		
			scriptElem.cc = cc;
				

			scriptElem.actionType =
				(script.IsTaggedAsIgnored() ? ActionType.TagAsIgnored :
				(script.convState != ConvertedScript.ConvState.Problematic) ? ActionType.Convert : ActionType.Skip);

			//scriptElem.enabled = (script.convState != ConvertedScript.ConvState.PROBLEMATIC);
				
			scriptElem.sourceFilePath = filePath;	
			scriptElem.sourceRelativeFilePath = filePath.Replace(Application.dataPath, "");
			scriptElem.outputFilePath = ""; 			// TODO

			return scriptElem;	
			}
			

		// -----------------
		override protected bool IsVisible()
			{
			if (this.actionType == ActionType.Convert)
				return (!this.cc.hideEnabledScripts);
	
			if (this.actionType == ActionType.TagAsIgnored)
				return (!this.cc.hideIgnoredScripts);

			return true;
			}
			

		// ---------------
		override protected void OnSetState(int stateId, int state)
			{
			/*

			if ((state < 0) || (state > 1)) 
				return;

			switch (stateId)
				{
				case (int)ElemStateId.SELECTED : 					
					this.enabled = (state != 0);
					break;
			
				case (int)ElemStateId.IGNORED :
					this.tagAsIgnored = (state != 0);
					break;
			
				default:
					return;
				}
			*/

			switch (stateId)		
				{
				case (int)ElemStateId.Action :
					this.actionType = (ActionType)state;
					break;

				default:
					return;
				}

			this.SetDirtyFlag();
			}

		// ---------------
		override protected int OnGetState(int stateId, int prevState) //, int mixedStateVal)
			{
			switch (stateId)
				{
//				case (int)ElemStateId.SELECTED :	
//					{
//					int curState = (this.enabled ? 1 : 0);
//					return (((prevState == UNDEFINED_STATE) || (curState == prevState)) ? curState : MIXED_STATE);
//					}
//					
//				case (int)ElemStateId.IGNORED :	
//					{
//					int curState = (this.tagAsIgnored ? 1 : 0);
//					return (((prevState == UNDEFINED_STATE) || (curState == prevState)) ? curState : MIXED_STATE);
//					}

				case (int)ElemStateId.Action :	
					{
					int curState = (int)this.actionType;
					return (((prevState == UNDEFINED_STATE) || (curState == prevState)) ? curState : MIXED_STATE);
					}


				case (int)ElemStateId.ConvState :	
					//if (!this.enabled)
					//	return prevState;
					//else
						return ((prevState == UNDEFINED_STATE) ? (int)(this.script.convState) : Mathf.Max(prevState, (int)this.script.convState));
				}

			return prevState;
			}
		

			
		// -------------------
		
	
		// -----------------
		//override protected void OnDrawGUI(float indent)
		override protected void OnDrawElemLabel(float indent)
			{
			//EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

			GUILayout.Space(indent);
	
			int actionType = this.actionTypePopup.Draw((int)this.actionType);
				
			if (actionType != (int)this.actionType)
				this.SetState((int)ElemStateId.Action, actionType);

			//GUILayout.Space(10);


			//int enabledState = CFGUI.TriState("", "Convert this script?", 
			//	(this.enabled ? 1 : 0), EditorStyles.toggle, EditorStyles.toggle, GUILayout.Width(16)); //CFEditorStyles.Inst.checkbox, CFEditorStyles.Inst.checkbox );

			//if (enabledState != (this.enabled ? 1 : 0))
			//	this.SetState((int)ElemStateId.SELECTED, enabledState);

			//this.enabled = GUILayout.Toggle(this.enabled, GUIContent.none, CFEditorStyles.Inst.checkbox);
				
			//DrawConvStateIcon((int)this.script.convState);


			//GUILayout.Space(indent);
				
			GUILayout.Box(GUIContent.none, ((this.script.lang == ConvertedScript.Lang.CS) ? CFEditorStyles.Inst.iconScriptCS : CFEditorStyles.Inst.iconScriptJS));
				
			//DrawConvStateIcon((int)this.script.convState);


			Color initialBGColor = GUI.backgroundColor;
			GUI.backgroundColor = this.bgColor;
				
			if (GUILayout.Button(new GUIContent(this.name, (this.script.IsTaggedAsIgnored() ? CFEditorStyles.Inst.texIgnore : null)), 
				CFEditorStyles.Inst.treeViewElemLabel, GUILayout.ExpandWidth(true)))
				{
				//Event c = Event.current;
//if (Event.current != null) Debug.Log("Event : key:" + c.keyCode + " button: " + c.button + " clicks: " + c.clickCount);
				this.view.Select(this);	
					
				}
				
			GUI.backgroundColor = initialBGColor;
				

			//EditorGUILayout.EndHorizontal();
			}
			
		private Vector2 fragListScrollPos;

		// ---------------------
		public void DrawScriptInfoGUI()
			{
			GUILayout.Label(this.sourceRelativeFilePath);
				

			this.fragListScrollPos = EditorGUILayout.BeginScrollView(this.fragListScrollPos);
				
			if (this.script.errorFragments.Count > 0)
				{
				GUILayout.Label("Errors: " + this.script.errorFragments.Count, CFEditorStyles.Inst.transpBevelBG);
				this.DrawFragmentInfos(this.script.errorFragments, new Color(1, 0, 0, 0.2f));
				GUILayout.Space(10);
				}


			if (this.script.warningFragments.Count > 0)
				{
				GUILayout.Label("Warnings: " + this.script.warningFragments.Count, CFEditorStyles.Inst.transpBevelBG);
				this.DrawFragmentInfos(this.script.warningFragments, new Color(1, 1, 0, 0.2f));
				GUILayout.Space(10);
				}
				
			if (this.script.okFragments.Count > 0)
				{
				GUILayout.Label("Perfect conversions: " + this.script.okFragments.Count, CFEditorStyles.Inst.transpBevelBG);
				this.DrawFragmentInfos(this.script.okFragments, new Color(0, 1, 0, 0.2f));
				}
		


			EditorGUILayout.EndScrollView();
		
			}




	/*
	static public int DrawScriptActionPopup(int actionType)
			{
			if ((mScriptActionPopupContentArray == null) || (mScriptActionPopupContentArray.Length != 3) ||
				(mScriptActionPopupValueArray == null) || (mScriptActionPopupValueArray.Length != 3))
				{
				mScriptActionPopupValueArray = new int[] = { 
				}

			EditorGUILayout.IntPopup(
			}
*/
		
		// ---------------------
		private void DrawFragmentInfos(List<ConvertedScript.Fragment> fragList, Color bgColor)
			{
			if ((fragList == null) || (fragList.Count == 0))
				return;

				

			EditorGUILayout.BeginVertical(CFEditorStyles.Inst.transpSunkenBG);

			foreach (ConvertedScript.Fragment frag in fragList)
				{	
				EditorGUILayout.BeginHorizontal();
					
				if (GUILayout.Button(new GUIContent(CFEditorStyles.Inst.magnifiyingGlassTex, "Show this line in IDE."), GUILayout.Width(20)))
					{
					CFEditorUtils.OpenScriptInIDE(this.sourceFilePath, frag.line);

/*					string assetPath = ("Assets" +  this.sourceRelativeFilePath);
					//int extPos = assetPath.LastIndexOf('.');
					//if (extPos > 0)
					//	assetPath = assetPath.Substring(0, extPos);
	
					Object assetRef = AssetDatabase.LoadMainAssetAtPath(assetPath); //, typeof(MonoScript));
				//Debug.Log("Openmng [" + assetPath + "] : " + (assetRef != null ? "OK" : "ERROR!!"));	
					AssetDatabase.OpenAsset(assetRef , frag.line);	
*/
					}

				Color initialBgColor = GUI.backgroundColor;
				GUI.backgroundColor = bgColor;

				GUILayout.Label(new GUIContent("Line " + frag.line.ToString().PadLeft(4) + ": " + frag.originalFrag + 
					((frag.problem != ConvertedScript.ProblemType.None) ? (" <i>("  + UnderscoresToSpaces(frag.problem.ToString()) + ")</i>") : 
					(frag.warning != ConvertedScript.WarningType.None) ? (" <i>(" + UnderscoresToSpaces(frag.warning.ToString()) + ")</i>") : "")), 	
					CFEditorStyles.Inst.scriptFragInfoBG);

				GUI.backgroundColor = initialBgColor;
	
				EditorGUILayout.EndHorizontal();
				}

			EditorGUILayout.EndVertical();

			}
			
		// ----------------------
		static private string UnderscoresToSpaces(string s)
			{
			return s.Replace('_', ' ');
			}

		}


		
	
	// ---------------
	static private void DrawConvStateIcon(int convState)
		{
		GUILayout.Box(GUIContent.none, 
				(convState == (int)ConvertedScript.ConvState.Ok) ? CFEditorStyles.Inst.iconOk : 
				(convState == (int)ConvertedScript.ConvState.Problematic) ? CFEditorStyles.Inst.iconError : 
				(convState == (int)ConvertedScript.ConvState.OkWithWarnings) ? CFEditorStyles.Inst.iconOkWarning : CFEditorStyles.Inst.iconQuestionMark);
		}



		

	// ---------------------
	static private List<string> GetFirstPassJavaScripts()
		{	
		List<string> fileList = new List<string>(128);

		string baseDir = CFEditorUtils.GetAssetsPath();
			
		if (Directory.Exists(baseDir + "Plugins/"))
			fileList.AddRange(Directory.GetFiles(baseDir + "Plugins/", 			"*.js", SearchOption.AllDirectories)); 

		if (Directory.Exists(baseDir + "Standard Assets/"))
			fileList.AddRange(Directory.GetFiles(baseDir + "Standard Assets/", 	"*.js", SearchOption.AllDirectories));
			
		// Exclude files based on folder black list...
			
		for (int i = (fileList.Count - 1); i >= 0; --i)
			{
//Debug.Log("found [" + fileList[i] + "]");

			if (IsFileBlacklisted(fileList[i]))
				fileList.RemoveAt(i);
			}

//Debug.Log("Found " + fileList.Count +  " files...");	foreach (string s in fileList) Debug.Log("Dir[" + s + "]");

		return fileList; 
		}

	// ---------------------
	static private List<string> GetAllScripts()
		{	
		List<string> fileList = new List<string>(128);

		string baseDir = Application.dataPath;

		fileList.AddRange(Directory.GetFiles(baseDir, "*.cs", SearchOption.AllDirectories)); 
		fileList.AddRange(Directory.GetFiles(baseDir, "*.js", SearchOption.AllDirectories));
			
		// Exclude files based on folder black list...
			
		for (int i = (fileList.Count - 1); i >= 0; --i)
			{
			if (IsFileBlacklisted(fileList[i]) || IsFileFirstPassJavaScript(fileList[i]))
				fileList.RemoveAt(i);
			}

//Debug.Log("Found " + fileList.Count +  " files...");	foreach (string s in fileList) Debug.Log("Dir[" + s + "]");

		return fileList; 
		}


	// --------------------
	static private bool IsFileBlacklisted(string path)
		{
		path = path.Replace("\\", "/");
			
		for (int i = 0; i < mDirectoryBlackList.Length; ++i)
			{
			if (path.IndexOf(mDirectoryBlackList[i], System.StringComparison.OrdinalIgnoreCase) >= 0)
				return true;			
			}

		//if (path.EndsWith(".js") && (path.Contains("/Standard Assets/") || path.Contains("/Plugins/")))
		//	return true;
 	
		return false;
		}

	// --------------------
	static private bool IsFileFirstPassJavaScript(string path)
		{
		path = path.Replace("\\", "/");

		if (path.EndsWith(".js") && (path.Contains("/Standard Assets/") || path.Contains("/Plugins/")))
			return true;
 	
		return false;
		}



		
	// -----------------
	[MenuItem("Control Freak 2/Prepare First-Pass JavaScripts for conversion.")]
	static public void CheckForRelevantFirstPassJavaScripts()
		{
		//if (EditorUtility.DisplayCancelableProgressBar(DIALOG_TITLE, "Looking for First-Pass JavaScripts...", 0.2f))
		//	return;

		List<string> jsList = GetFirstPassJavaScripts();

		if (jsList.Count == 0)
			{
			EditorUtility.ClearProgressBar();
			EditorUtility.DisplayDialog(DIALOG_TITLE, "No First-Pass JavaScripts found!", "OK");
			return;
			}
			
		List<string> relevantList = new List<string>(8);
			
		string listSummary = "";

		for (int i = 0; i < jsList.Count; ++i)
			{
			//if (EditorUtility.DisplayCancelableProgressBar(DIALOG_TITLE, "Parsing JS [" + (i + 1) + "/" + jsList.Count + "] : " + jsList[i], 
			//	((float)(i+1) / (float)jsList.Count)))
			//	{
			//	EditorUtility.ClearProgressBar();
			//	return;
			//	}

			ConvertedScript s = new ConvertedScript();
			s.ProcessTextFile(jsList[i]);
			if (s.IsRelevant())
				{
				relevantList.Add(jsList[i]);
				listSummary += CFEditorUtils.GetAssetsRelativePath(jsList[i]) + "\n";
				}						
			}
		
		//EditorUtility.ClearProgressBar();

		if (relevantList.Count == 0)
			{
			EditorUtility.DisplayDialog(DIALOG_TITLE, "No RELEVANT First-Pass JavaScripts found!", "OK");
			return;
			}

		
		if (!EditorUtility.DisplayDialog(DIALOG_TITLE, 
			"Found "  + relevantList.Count + " relevant JavaScripts in First-Pass folders!\n\n" + 
				listSummary + "\n\nThey must be moved outside these special folders to be able to use Control Freak 2.\n\n" +
				"I can do this for you, but be warned - this operation may result in compilation errors, when other first-pass javascript class is referencing any of the moved scripts!\n\n" +
				"Scripts from /Plugins/ and /Standard Assets/ folders are moved to /Plugins-JS/ and /Standard Assets-JS/ folders, respectively.\n\n" +  
				"So, are you sure you want to do this?", "Yes, I want to take the risk!", "No, thanks..."))
			return;
			

		string projPath = CFEditorUtils.GetProjectPath();

		try 
			{
			for (int i = 0; i < relevantList.Count; ++i)
				{
				string srcFile = CFEditorUtils.GetProjectRelativePath(relevantList[i]);
				
				string dstFile = null; 
					
				if (srcFile.IndexOf("/Plugins/") >= 0)
					dstFile = srcFile.Replace("/Plugins/", "/Plugins-JS/");

				else if (srcFile.IndexOf("/Standard Assets/") >= 0)
					dstFile = srcFile.Replace("/Standard Assets/", "/Standard Assets-JS/");

				if (dstFile == null)
					{
					Debug.LogError("Something's wrong with JS path [" + srcFile + "]!");
					continue;
					}


				EditorUtility.DisplayProgressBar(DIALOG_TITLE, "Moving [" + srcFile + "] to [" + dstFile + "]!", ((float)(i + 1) / (float)relevantList.Count));
					
									

				//Debug.Log("Moving [" + srcFile + "] to [" + dstFile + "]! : to dir[" + Path.GetDirectoryName((projPath + dstFile)) + "] : " );

				CFEditorUtils.EnsureDirectoryExists(projPath + dstFile); //Path.GetDirectoryName(dstFile));
				
				System.IO.File.Move(projPath + srcFile, projPath + dstFile);
	
				//string msg = AssetDatabase.MoveAsset(srcFile, dstFile);
		//Debug.Log("\tmsg: " + msg);
				}

			EditorUtility.ClearProgressBar();


			EditorUtility.DisplayDialog(DIALOG_TITLE, "Moved " + relevantList.Count + " JS Scripts!\n" +
				"Scripts will now be recompiled and when it's finished, please run Control Freak 2 Script Converter to convert them.", "Recompile moved scripts.");
			
			AssetDatabase.Refresh();
			return;		

			}
		catch (System.Exception err)
			{
			EditorUtility.ClearProgressBar();
			EditorUtility.DisplayDialog(DIALOG_TITLE, "Something went wrong when moving JS scripts! : " + err.Message, "OK");		
			}
		}


	}
}

#endif
