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
public class ScriptBackup : EditorWindow
	{
	public const string 
		DIALOG_TITLE				= "Control Freak 2 Script Backup Restore",
		BACKUP_FOLDER				= "CF2-Data/Script-Backups/",
		ORIGINAL_FILES_FOLDER		= "Assets-Before/",
		MODIFIED_FILES_FOLDER		= "Assets-After/",
		CONVERSION_LOG_FILENAME		= "Conversion-Log.txt";
		
	
	public enum ToolMode
		{
		SelectBackup,
		BackupConfig
		}	

		
	public ToolMode 			toolMode;
	private BackupSelectionMode	backupSelection;
	private BackupConfigMode	backupConfig;
		
	
	
	// ---------------------
	public ScriptBackup()
		{	
		this.backupConfig		= new BackupConfigMode(this);
		this.backupSelection	= new BackupSelectionMode(this);
			
		this.minSize = new Vector2(400, 200);
		}
	

	// --------------------
	[MenuItem("Control Freak 2/CF2 Script Backup Restore")]
	static public void ShowDialog()
		{
		ScriptBackup w = GetWindow<ScriptBackup>(true, "CF2 Script Backup", true);
		if (w != null) w.Repaint();
		}


	// -------------------
	void OnEnable()
		{
		this.EnterBackupSelectionMode();
		this.Repaint();
		}

		
	// ------------------
	void OnGUI()
		{
		if (this.toolMode == ToolMode.SelectBackup)
			this.backupSelection.DrawGUI();
		else
			this.backupConfig.DrawGUI();
		}
	
		

	// -------------------
	static public string GetProjectPath()
		{
		string s = Application.dataPath;
		return s.Substring(0, s.LastIndexOf("Assets"));
		}

	// ---------------------
	static public string GetBackupFolder()
		{
		return Path.Combine(GetProjectPath(), BACKUP_FOLDER);
		}


	// ---------------------
	static public string CreateBackupFolder()
		{
		string baseFolder = GetBackupFolder() + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
		
		for (int i = 0; i < 99; ++i)
			{		
			string folderDup = ((i == 0) ? baseFolder : (baseFolder + "_" + i.ToString("00")));
			if (Directory.Exists(folderDup))
				continue;
	
			if (Directory.CreateDirectory(folderDup) == null)
				continue;

			return folderDup;
			}

		throw new System.Exception("Can't create backup folder!");

		//return string.Empty;		
		}
		
		

	// ----------------------
	public void EnterBackupSelectionMode()
		{
		this.toolMode = ToolMode.SelectBackup;
		this.backupSelection.CollectBackupFolders();	
		}
		

	// -------------------
	public bool EnterBackupConfigMode(string path, string name)
		{
		if (this.backupConfig.Start(path, name))
			{
			this.toolMode = ToolMode.BackupConfig;
			return true;
			}

		return false;
		}
		


		



	// ----------------------
	private class BackupConfigMode
		{

		private const float TOOLBAR_HEIGHT = 70;


		private ScriptBackup				codeBackup;
		private List<BackupFileElem>	fileElems;
		private TreeView				treeView;
			
		private string					basePath;
		private string 					backupName;


		// -------------------
		public BackupConfigMode(ScriptBackup cb)
			{
			this.codeBackup	= cb;
			this.treeView	= new TreeView();
			this.fileElems	= new List<BackupFileElem>(32);
			}
	
		// ------------------
		public bool Start(string path, string name)
			{
			this.basePath = path + "/" + ORIGINAL_FILES_FOLDER;
			this.backupName = name; 

			this.treeView = new TreeView();
			this.fileElems.Clear();

			this.CollectFiles();

			return true;
			}
			

		// ------------------
		private bool RestoreSelected()
			{
			//EditorUtility.DisplayDialog(ScriptBackup.DIALOG_TITLE, "Restoring...", "OK");

			int selectedFileCount = 0;
			for (int i = 0; i < this.fileElems.Count; ++i)
				{
				if (this.fileElems[i].enabled)	
					++selectedFileCount;
				} 
				
			if (selectedFileCount == 0)
				{
				return false;
				}


			try 	
				{
				//string assetsPath = CFEditorUtils.GetAssetsPath();

				for (int i = 0; i < this.fileElems.Count; ++i)
					{
					if (!this.fileElems[i].enabled)
						continue;
						
//Debug.Log("Rstoring from " + this.fileElems[i].pathIn + " to " + this.fileElems[i].pathOut );
					File.Copy(this.fileElems[i].pathIn, this.fileElems[i].pathOut, true);
					
					}
				}
			catch (System.Exception err)
				{
				EditorUtility.DisplayDialog(ScriptBackup.DIALOG_TITLE, "Error while restoring backed up files!\n" + err.Message, "OK");
				}

			return true;
			}



		// ----------------
		private void CollectFiles()
			{

			EditorUtility.DisplayProgressBar(DIALOG_TITLE, "Collecting files...", 0);


			List<string> fileList = new List<string>(64);
			fileList.AddRange(Directory.GetFiles(this.basePath, "*.cs", SearchOption.AllDirectories));
			fileList.AddRange(Directory.GetFiles(this.basePath, "*.js", SearchOption.AllDirectories));

//Debug.Log("Collected " + fileList.Count + " files in [" + this.basePath + "]");

			//foreach (string filePath in fileList)	
			for (int i = 0; i < fileList.Count; ++i)
				{
				string filePath = fileList[i];

				EditorUtility.DisplayProgressBar(DIALOG_TITLE, "Checking [" + filePath.Replace(this.basePath, "") + "].", 
					Mathf.Lerp(0.1f, 1.0f, ((float)i / (float)(fileList.Count - 1)))); 

//Debug.Log("Reading file[" + i + "/" + fileList.Count + "] ["  + filePath.Replace(this.basePath, "") + "]");
					
				this.CreateFile(filePath);
				}
				
			EditorUtility.ClearProgressBar();

			this.treeView.InvalidateUpwards(true);

			}

		// -------------------
		private BackupFileElem CreateFile(string path)
			{
			string relativePath = path.Replace(this.basePath, "");

			BackupFileElem file = BackupFileElem.Create(path, relativePath, this);
			if (file == null)
				return null;
				

			TreeViewElem parent = TreeViewElem.CreateDirectoryStructure(this.treeView, Path.GetDirectoryName(relativePath), this.CreateFolder);

			if (parent != null)
				{
				parent.AddChild(file);
				this.fileElems.Add(file);
				}
	
			return file;			
			}

		// -------------------
		private TreeViewElem CreateFolder(TreeViewElem root, string path, string name)
			{
			BackupFolderElem folder = new BackupFolderElem(this.treeView);
			folder.name = name;
			return folder;
			}
			
		private Vector2 treeScrollPos;

		// ------------------
		public void DrawGUI()
			{
			GUILayout.Box(GUIContent.none, CFEditorStyles.Inst.headerScriptBackup, GUILayout.ExpandWidth(true));

			// Tool bar...

			EditorGUILayout.BeginHorizontal(CFEditorStyles.Inst.toolbarBG, GUILayout.Height(TOOLBAR_HEIGHT));
				
			if (GUILayout.Button("Restore Scripts", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
				{
				if (this.RestoreSelected())
					{
					if (EditorUtility.DisplayDialog(ScriptBackup.DIALOG_TITLE, "Close this window and refresh restored scripts?", "Yes", "No, I'm not done yet."))
						{
						this.codeBackup.Close();
						AssetDatabase.Refresh(ImportAssetOptions.Default);
						return;
						}
					}
				}


			if (GUILayout.Button("Back", GUILayout.Width(50), GUILayout.ExpandHeight(true)))
				{
				this.codeBackup.EnterBackupSelectionMode();	
				return;
				}
				

	
			EditorGUILayout.EndHorizontal();


			GUILayout.Box("Backup State: " + this.backupName, CFEditorStyles.Inst.centeredTextSunkenBG, GUILayout.ExpandWidth(true), GUILayout.Height(20));
				
			if (this.fileElems.Count == 0)
				{
				GUILayout.Box("All scripts of this state are are the same as currently used scripts!", CFEditorStyles.Inst.centeredTextSunkenBG, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				}
			else
				{
				// Tree view...
		
				this.treeScrollPos = EditorGUILayout.BeginScrollView(this.treeScrollPos, CFEditorStyles.Inst.treeViewBG);
	
				this.treeView.DrawTreeGUI();
	
				EditorGUILayout.EndScrollView();
				}

			}
	

	// ------------------
	private enum ElemStateId 
		{
		Enabled
		}
			

	const int 
		UNDEFINED_STATE = -2,	
		MIXED_STATE		= -1;


	// ----------------------
	private class BackupFolderElem : TreeViewElem
		{
		private int enableState;

		// -----------------
		public BackupFolderElem(TreeView view) : base(view)
			{
			}

				
		// ------------------
		override protected void OnInvalidate()
			{
			this.enableState = this.GetState((int)ElemStateId.Enabled, UNDEFINED_STATE, 0);
			}

		// ----------------
		override protected void OnDrawGUI(float indent)
			{
			ScriptConverter.StartTreeViewElemHorizontal(this.view, this);

			//EditorGUILayout.BeginHorizontal((this.view.selectedElem == this) ? CFEditorStyles.Inst.treeViewElemSelBG : CFEditorStyles.Inst.treeViewElemBG,
			//	GUILayout.ExpandWidth(true));

			GUILayout.Space(indent);

			int enabledState = CFGUI.TriState("", "Restore entire folder.", 
				(this.enableState), EditorStyles.toggle, EditorStyles.toggle, GUILayout.Width(16)); //, CFEditorStyles.Inst.checkbox, CFEditorStyles.Inst.checkbox );

			if (enabledState != this.enableState)
				this.SetState((int)ElemStateId.Enabled, enabledState);


			//GUILayout.Space(indent);


			//this.isFoldedOut = EditorGUILayout.Toggle(this.isFoldedOut, CFEditorStyles.Inst.foldout, GUILayout.Width(16));

			GUILayout.Box(GUIContent.none, CFEditorStyles.Inst.iconFolder);

			Color initialBGColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(1,1,1,0); //this.bgColor;
 
			if (GUILayout.Button(this.name, CFEditorStyles.Inst.treeViewElemTranspLabel, GUILayout.ExpandWidth(true)))
				this.view.Select(this);	

			GUI.backgroundColor = initialBGColor;


			EditorGUILayout.EndHorizontal();
			}
		}
			






	// --------------------
	private class BackupFileElem : TreeViewElem
		{
		public bool enabled;
		public string	pathIn,
						pathOut;

		private string	backupCode;
	
		private ConvertedScript.Lang	lang;
		//private BackupConfigMode		config;

		//public bool		isDifferent;
				
	
		// ----------------------
		public BackupFileElem(TreeView tree) : base(tree)
			{
			}
				

		// --------------------
		public void ViewBackupFile()
			{
			System.Diagnostics.Process.Start(this.pathIn);
			}


		// -------------------
		public void ViewCurrentFile()
			{
			System.Diagnostics.Process.Start(this.pathOut);
			}


		// ---------------------
		static public BackupFileElem Create(string filename, string relFilename, BackupConfigMode config)
			{
			string targetFilename = Path.Combine(Application.dataPath, relFilename);
			
			if (!File.Exists(targetFilename))
				{
//Debug.Log("Target file [" + targetFilename + "] doesn't exist!!!");
				return null;
				}

			string backupCode = File.ReadAllText(filename);
			string targetCode = File.ReadAllText(targetFilename);

			if ((backupCode.Length == targetCode.Length) && backupCode.Equals(targetCode))	
				{
//Debug.Log("Target file is the same as backed up one [" + targetFilename + "]!");
				return null;
				}


			BackupFileElem file = new BackupFileElem(config.treeView);
			file.name 		= Path.GetFileName(filename);	
			//file.config		= config;
			file.lang		= ConvertedScript.GetScriptLang(filename);
			file.pathIn		= filename;
			file.pathOut	= targetFilename;
			file.enabled 	= true;
	
			return file;
			}


		// ---------------
		override protected void OnSetState(int stateId, int state)
			{
			if ((state < 0) || (state > 1)) 
				return;

			switch (stateId)
				{
				case (int)ElemStateId.Enabled : 					
					this.enabled = (state != 0);
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
				case (int)ElemStateId.Enabled :	
					{
					int curState = (this.enabled ? 1 : 0);
					return (((prevState == UNDEFINED_STATE) || (curState == prevState)) ? curState : MIXED_STATE);
					}
				}

			return prevState;
			}
		

				
	
		// -----------------
		override protected void OnDrawGUI(float indent)
			{
			ScriptConverter.StartTreeViewElemHorizontal(this.view, this);

			//EditorGUILayout.BeginHorizontal((this.view.selectedElem == this) ? CFEditorStyles.Inst.treeViewElemSelBG : CFEditorStyles.Inst.treeViewElemBG, 
			//	GUILayout.ExpandWidth(true));

			GUILayout.Space(indent);
	
			int enabledState = CFGUI.TriState("", "Restore this script.", 
				(this.enabled ? 1 : 0), EditorStyles.toggle, EditorStyles.toggle, GUILayout.Width(16)); //CFEditorStyles.Inst.checkbox, CFEditorStyles.Inst.checkbox );

			if (enabledState != (this.enabled ? 1 : 0))
				this.SetState((int)ElemStateId.Enabled, enabledState);

			GUILayout.Space(indent);
	

			
			GUILayout.Box(GUIContent.none, ((this.lang == ConvertedScript.Lang.CS) ? CFEditorStyles.Inst.iconScriptCS : CFEditorStyles.Inst.iconScriptJS));

			//Color initialBGColor = GUI.backgroundColor;
			//GUI.backgroundColor = this.bgColor;
				
			if (GUILayout.Button(this.name, CFEditorStyles.Inst.treeViewElemTranspLabel, GUILayout.ExpandWidth(true)))
				{
				//Event c = Event.current;
//if (Event.current != null) Debug.Log("Event : key:" + c.keyCode + " button: " + c.button + " clicks: " + c.clickCount);
				this.view.Select(this);	
					
				}
				
			//GUI.backgroundColor = initialBGColor;
				
			if (GUILayout.Button(new GUIContent("B", CFEditorStyles.Inst.magnifiyingGlassTex, "Open backup file."), GUILayout.Width(30), GUILayout.Height(16)))
				this.ViewBackupFile(); 

			if (GUILayout.Button(new GUIContent("C", CFEditorStyles.Inst.magnifiyingGlassTex, "Open current file."), GUILayout.Width(30), GUILayout.Height(16)))
				this.ViewCurrentFile(); 



			EditorGUILayout.EndHorizontal();
			}

				/*
		// ----------------
		override protected void OnDrawGUI(float indent)
			{
			EditorGUILayout.BeginHorizontal();

			GUILayout.Space(indent);
			GUILayout.Label(this.name, CFEditorStyles.Inst.transpBevelBG);

			EditorGUILayout.EndHorizontal();
			}
				 
*/
		}
	}






		
	// --------------------
	private class BackupSelectionMode
		{
		private ScriptBackup			codeBackup;
		private List<BackupFolder>	backupFolders;
			
		// -------------------
		public BackupSelectionMode(ScriptBackup codeBackup)
			{
			this.codeBackup		= codeBackup;
			this.backupFolders	= new List<BackupFolder>(32);
			}
			
		// --------------------
		public void CollectBackupFolders()	
			{
			this.backupFolders.Clear();
	
			DirectoryInfo backupDir = new DirectoryInfo(GetBackupFolder());
			if (!backupDir.Exists)
				{
				return; 
				}
	
			DirectoryInfo[] subDirs = backupDir.GetDirectories();
			foreach (DirectoryInfo subDir in subDirs)	
				{	
				if (IsBackupDir(subDir))
					this.backupFolders.Add(new BackupFolder(subDir));
				}
	
			this.backupFolders.Sort(BackupFolder.Cmp);
			}
		
		
		// ----------------------
		static private bool IsBackupDir(DirectoryInfo dir)
			{
			if (!dir.Exists) 
				return false;
				
			if (!Directory.Exists(Path.Combine(dir.FullName, MODIFIED_FILES_FOLDER)))
				return false;
	
			return true;
			}

			
		private Vector2 scrollPos;
	
		// -----------------
		public void DrawGUI()
			{	

			GUILayout.Box(GUIContent.none, CFEditorStyles.Inst.headerScriptBackup, GUILayout.ExpandWidth(true));

			if (this.backupFolders.Count == 0)
				{
				GUILayout.Label("No backup folders found!");
				}
			else
				{	
			
				GUILayout.Box("List of available backup states:", CFEditorStyles.Inst.transpSunkenBG);
				GUILayout.Space(10);

				this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, CFEditorStyles.Inst.transpSunkenBG);

				foreach (BackupFolder folder in this.backupFolders)
					{
					EditorGUILayout.BeginHorizontal();
						
					if (GUILayout.Button(new GUIContent(CFEditorStyles.Inst.magnifiyingGlassTex, "Open this backup's log file."), GUILayout.Width(30)))
						{
						folder.ViewLog();
						}
						
					if (GUILayout.Button(new GUIContent(CFEditorStyles.Inst.folderTex, "Open this backup's folder."), GUILayout.Width(30)))
						{
						folder.ViewFolder();
						}

					if (GUILayout.Button(new GUIContent(CFEditorStyles.Inst.trashCanTex, "Delete this backup's folder."), GUILayout.Width(30)))
						{
						if (EditorUtility.DisplayDialog(ScriptBackup.DIALOG_TITLE, "Are you sure you want to delete [" + folder.name + "]?", "Yes, I'm sure!", "No."))
							{
							folder.DeleteFolder();
							this.CollectBackupFolders();
							return;
							}
						}


					if (GUILayout.Button(folder.name))
						{
						if (this.codeBackup.EnterBackupConfigMode(folder.path, folder.name))
							return;
						}
				

					
					EditorGUILayout.EndHorizontal();
					}

				EditorGUILayout.EndScrollView();
				}

			}
	


		// --------------------
		private class BackupFolder
			{
			public string path;
			public string name;
		
			// ----------------
			public BackupFolder(DirectoryInfo dir)
				{
				this.path = dir.FullName;
				this.name = dir.Name;
				}
				
			// ----------------
			static public int Cmp(BackupFolder a, BackupFolder b)
				{
				return string.Compare(b.name, a.name);
				}
		
		
			// -------------------
			public void ViewLog()
				{
				string logPath = Path.Combine(this.path, CONVERSION_LOG_FILENAME);
				System.Diagnostics.Process.Start(logPath);
				}

			// -----------------
			public void ViewFolder()
				{
				System.Diagnostics.Process.Start(this.path);				
				}

			// ------------------
			public void DeleteFolder()	
				{
				try 
					{ Directory.Delete(this.path, true); }
				catch (System.Exception e)
					{ EditorUtility.DisplayDialog(ScriptBackup.DIALOG_TITLE, "Could not delete backup folder ["  + this.name + "]!\n" + e.Message, "OK"); }
					
				}
			}
		}		

	


	

/*
	 
Collect folders indise Backups/ , but the only ones with Assets-Before/ Assets-After/ subfolders
Compare existing files by size, contents and modified date
build tree view and allow selective restore...

*/

	}
}

#endif


