
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;


namespace ControlFreak2Editor.ScriptConverter
{
// ------------------
public class ConvertedScript
	{
		
	// --------------
	public enum Lang
		{ 
		CS, 
		JS
		}

	public enum LineEndingStyle 
		{
		Win,	
		Unix
		}
		
	// -------------
	public enum ConvState
		{
		NothingChanged,	
		Ok,
		OkWithWarnings,
		Problematic
		}

	// ------------
	public enum ProblemType
		{
		None,
		Control_Freak_1_Feature,
		Unsupported_Method,
		Unsupported_Property	
		}

	// -----------------
	public enum WarningType
		{
		None,
		Non_Constant_Parameter,
		Unkown_Property,
		Unkown_Method
		}


	private string			srcFile;
	private string			outFile;

	public Lang					lang;
	public LineEndingStyle	lineEndingStyle;

	public ConvState		convState;

	private bool
		convUsedControlFreak1,
		convUsedUnsupportedMethods,
		convUsedUnsupportedProps,
		convUsedNonConstParams;
	

	private string			
		originalCode,
		sanitizedCode;
	private StringBuilder
		modifiedCode;	

	private bool			isTaggedWithDontConvert;
	
	public List<Fragment> 		
		fragments,
		okFragments,
		warningFragments,
		errorFragments;

	private List<CommentBlock>	commentBlocks;


	static string 
		WHITESPACE_CHARS				= @" \t\n\r",
		OPERATOR_CHARS					= @"\:\?\-\+\%\!\~\>\<\*\/\&\|\=",
		SPECIAL_CHARS					= @"\;\,\(\)\{\}\[\]",

		REGEX_WS_CLASS 					= "[" + WHITESPACE_CHARS + "]",
		//REGEX_METHOD_ANCHOR_CLASS 		= @"[ \t\r\n\+\-\*\;\<\>\/]",
		REGEX_PROPERTY_ANCHOR_CLASS 	= "[" + OPERATOR_CHARS + @"\[\.\)\}\;\," + "]",
		REGEX_WS_OP_SPEC_CLASS			= "[" + WHITESPACE_CHARS  + OPERATOR_CHARS + SPECIAL_CHARS + "]",
		REGEX_WS_UNDERSCORE_CLASS		= "[" + WHITESPACE_CHARS + "_" + "]",
			
		REGEX_OPTIONAL_WS				= REGEX_WS_CLASS + "*",
		REGEX_OPTIONAL_WS_UNDERSCORE	= REGEX_WS_UNDERSCORE_CLASS + "*",
			
		REGEX_TYPE_SPECIFIER_START_ANCHOR_CLASS 	= "[" + WHITESPACE_CHARS + @"\(\:\;\,\{" + "]",
		REGEX_TYPE_SPECIFIER_END_ANCHOR_CLASS 		= "[" + WHITESPACE_CHARS + @"\)\;" + "]",

		//REGEX_BLOCK_COMMENT				= @"(?:\/\*(?:.|\n)*?(?:\*\/))",
		//REGEX_LINE_COMMENT				= @"(?:\/\/.+?\n)",

		REGEX_TOKEN						= @"[a-zA-Z_]+[a-zA-Z0-9_]?",
		REGEX_DOT_OPERATOR				= REGEX_OPTIONAL_WS + "\\." + REGEX_OPTIONAL_WS,

		REGEX_STRING_PARAM				= @"^(?:" + REGEX_OPTIONAL_WS + "(\\\".+(?=\\\")\\\")" + REGEX_OPTIONAL_WS + @")\z",
		REGEX_INT_PARAM					= @"^(?:" + REGEX_OPTIONAL_WS + "([0-9]+)" + REGEX_OPTIONAL_WS  + @")\z",
		REGEX_KEYCODE_ENUM_PARAM		= @"^(?:" + REGEX_OPTIONAL_WS + "KeyCode" + REGEX_OPTIONAL_WS + "\\." + REGEX_OPTIONAL_WS + "([a-zA-Z0-9]+)" + REGEX_OPTIONAL_WS + @")\z",


		REGEX_INPUT_CLASSNAMES 			= @"(?:Input|CrossPlatformInputManager|CFInput|(?:UnityEngine" + REGEX_DOT_OPERATOR + "Input))",
		REGEX_SCREEN_CLASSNAMES 		= @"(?:Screen|UnityEngine" + REGEX_DOT_OPERATOR + "Screen)",
		REGEX_CURSOR_CLASSNAMES 		= @"(?:Cursor|UnityEngine" + REGEX_DOT_OPERATOR + "Cursor)",
			
		STR_DONT_CONVERT_TO_CF2_TAG_FULL_LINE	= "// DONT_CONVERT_TO_CF2 \n",

		REGEX_DONT_CONVERT_TO_CF2_TAG_ONLY	= @"Don\'?t" + REGEX_OPTIONAL_WS_UNDERSCORE + "Convert" + 
			REGEX_OPTIONAL_WS_UNDERSCORE + "To" + REGEX_OPTIONAL_WS_UNDERSCORE + 
			"(?:(?:CF)|(?:Control" + REGEX_OPTIONAL_WS_UNDERSCORE + "Freak))" + REGEX_OPTIONAL_WS_UNDERSCORE + "2" + @"[ \t\.\!]*",

		REGEX_DONT_CONVERT_TO_CF2_TAG_WITH_OPTIONAL_COMMENT =
			"(?:" + @"(?:\/\/" + REGEX_OPTIONAL_WS + ")" + REGEX_DONT_CONVERT_TO_CF2_TAG_ONLY + @"(?:" + REGEX_WS_CLASS + @"*(?=\n)\n)" + ")|(?:" +

			REGEX_DONT_CONVERT_TO_CF2_TAG_ONLY + ")",
				 

		// (0 = dummy)(1 = class)(2 = dot)(3 = method)(4 = dummy)(5=params)(6=dummy)
		REGEX_INPUT_METHOD = 
			CreateStaticMethodRegex(REGEX_INPUT_CLASSNAMES),
			//"(?<=(?:" + REGEX_WS_OP_SPEC_CLASS + "))(?<class>" + REGEX_INPUT_CLASSNAMES + ")(?<dot>" + REGEX_DOT_OPERATOR + ")(?<method>" + REGEX_TOKEN + ")" +
			//"(?<opBr>" + REGEX_OPTIONAL_WS + @"\(" + REGEX_OPTIONAL_WS +  @")(?<params>.+?(?=\)))(?<clBr>\))" ,

		REGEX_SCREEN_METHOD	= 
			CreateStaticMethodRegex(REGEX_SCREEN_CLASSNAMES),

		//REGEX_CURSOR_METHOD	= 
		//	CreateStaticMethodRegex(REGEX_CURSOR_CLASSNAMES),


		// (0 = dummy)(1 = class)(2 = dummy)(3 = prop)(4 = dummy) 
		//REGEX_INPUT_PROPERTY	= "(?<pad>" + REGEX_WS_OP_SPEC_CLASS + ")(?<class>" + REGEX_INPUT_CLASSNAMES + ")(?<dot>" + REGEX_DOT_OPERATOR + ")(?<prop>" + REGEX_TOKEN + ")" +
		//	"(?<end>" + REGEX_OPTIONAL_WS + REGEX_PROPERTY_ANCHOR_CLASS + "{1})",

		REGEX_INPUT_PROPERTY	= 
			CreateStaticPropertyRegex(REGEX_INPUT_CLASSNAMES),

		REGEX_CURSOR_PROPERTY	= 
			CreateStaticPropertyRegex(REGEX_CURSOR_CLASSNAMES),

		REGEX_SCREEN_PROPERTY	= 
			CreateStaticPropertyRegex(REGEX_SCREEN_CLASSNAMES),
				
		// (0=dummy) (1=namespace) (2=type) 
		REGEX_UNITY_TOUCH_TYPE = 
			CreateTypeSpecifierRegex("UnityEngine", "Touch");

	

		//REGEX_CF1_CLASSNAMES 			= @"(?:TouchController|TouchStick|TochZone|TouchControl)",
		//REGEX_CF1_CLASSES		= REGEX_WS_OP_SPEC_CLASS + "(" + REGEX_CF1_CLASSNAMES + ")";	// TODO

	static private Regex 
		regexBlockComment,
		regexLineComment,

		regexStringParam,
		regexIntParam,
		regexKeyCodeEnumParam,

		//regexInputClass,

		//regexInputMethod,
		//regexInputProperty,

		//regexScreenMethod,
		//regexScreenProperty,

		//regexCursorMethod,
		//regexCursorProperty,

		//regexTouchControllerCode,

		regexDontConvertTagOnly,
		regexDontConvertTagWithOptionalComment;
		
		

	// ------------------------
	// (0 = dummy)(1 = class)(2 = dot)(3 = method)(4 = dummy)(5=params)(6=dummy)	
	// ------------------------
	static private string CreateStaticMethodRegex(string classNameRegex)
		{
		return (
			"(?<=(?:" + REGEX_WS_OP_SPEC_CLASS + "))(?<class>" + classNameRegex + ")(?<dot>" + REGEX_DOT_OPERATOR + ")(?<method>" + REGEX_TOKEN + ")" +
			"(?<opBr>" + REGEX_OPTIONAL_WS + @"\(" + REGEX_OPTIONAL_WS +  @")(?<params>.+?(?=\)))(?<clBr>\))" );
		}

	// ----------------------
	// (0 = dummy)(1 = class)(2 = dummy)(3 = prop)(4 = dummy) 
	// ----------------------
	static private string CreateStaticPropertyRegex(string classNameRegex)
		{
		return (
			"(?<=(?:" + REGEX_WS_OP_SPEC_CLASS + "))(?<class>" + classNameRegex + ")(?<dot>" + REGEX_DOT_OPERATOR + ")" + 
			"(?<prop>" + REGEX_TOKEN + "(?=(?:" + REGEX_OPTIONAL_WS + REGEX_PROPERTY_ANCHOR_CLASS + "{1})))");
		}
		

	// ------------------------
	// (0 = namespace) (1 = type)
	// ------------------------
	static private string CreateTypeSpecifierRegex(string namespaceRegex, string typeRegex)
		{
		return (
			"(?<=(?:" + REGEX_TYPE_SPECIFIER_START_ANCHOR_CLASS /*REGEX_WS_OP_SPEC_CLASS*/ + "))(?<namespace>(?:" + namespaceRegex + ")(?:" + REGEX_DOT_OPERATOR + "))?" + 
			"(?<type>" + typeRegex + "(?=(?:" + REGEX_OPTIONAL_WS + REGEX_TYPE_SPECIFIER_END_ANCHOR_CLASS + "{1})))" );
		}



	// -------------------------
	public string GetOriginalCode()		{ return this.originalCode; }
	public string GetConvertedCode()	{ return this.modifiedCode.ToString(); }
	public string GetOriginalCodeTaggedAsIgnored() { return (this.SetDontConvertTag(this.originalCode, true)); }

	// ------------------------	
	public ConvertedScript()
		{
		this.originalCode		= "";
		this.sanitizedCode	= "";
		this.modifiedCode		= new StringBuilder();
		this.commentBlocks		= new List<CommentBlock>();
		this.fragments			= new List<Fragment>();
		this.okFragments		= new List<Fragment>();
		this.warningFragments	= new List<Fragment>();
		this.errorFragments		= new List<Fragment>();
		}

			
	static ConvertedScript()
		{
		// Init regex..
			
//		ConvertedScript.regexInputMethod			= new Regex(REGEX_INPUT_METHOD);
//		ConvertedScript.regexInputProperty			= new Regex(REGEX_INPUT_PROPERTY);
//
//		ConvertedScript.regexCursorMethod			= new Regex(REGEX_CURSOR_METHOD);
//		ConvertedScript.regexCursorProperty			= new Regex(REGEX_CURSOR_PROPERTY);
//
//		ConvertedScript.regexScreenMethod			= new Regex(REGEX_SCREEN_METHOD);
//		ConvertedScript.regexScreenProperty			= new Regex(REGEX_SCREEN_PROPERTY);

		//ConvertedScript.regexTouchControllerCode	= new Regex(REGEX_CF1_CLASSES);
		//ConvertedScript.regexBlockComment			= new Regex(REGEX_BLOCK_COMMENT);
		//ConvertedScript.regexLineComment			= new Regex(REGEX_LINE_COMMENT);
		//ConvertedScript.regexInputClass				= new Regex(REGEX_INPUT_CLASSNAMES);
		
		ConvertedScript.regexStringParam			= new Regex(REGEX_STRING_PARAM);
		ConvertedScript.regexIntParam				= new Regex(REGEX_INT_PARAM);
		ConvertedScript.regexKeyCodeEnumParam		= new Regex(REGEX_KEYCODE_ENUM_PARAM);

		ConvertedScript.regexDontConvertTagOnly	= new Regex(REGEX_DONT_CONVERT_TO_CF2_TAG_ONLY, RegexOptions.IgnoreCase);		
		ConvertedScript.regexDontConvertTagWithOptionalComment	= new Regex(REGEX_DONT_CONVERT_TO_CF2_TAG_WITH_OPTIONAL_COMMENT, RegexOptions.IgnoreCase);		
		}
		
/*
	[MenuItem("Control Freak 2/Test Type Search ")]
	static void TESTCode()
		{
		string code = System.IO.File.ReadAllText(
			"Assets/CF2-Car/Unity-3.x-Car-Demo/Scripts/Javascripts/car.js");			
			//"Test/Code/In/CFAxisTest.cs");			
			
		
Debug.Log("TYPES IN TESTAXIS...");

		InputTypeConverter c = new InputTypeConverter();
			

		ConvertedScript s = new ConvertedScript();
		s.Start(code);
		s.CollectCommentBlocks();
		s.CollectFragments();


Debug.Log("TYPES IN TESTAXIS END------------- fr: " + s.fragments.Count);
for (int i = 0; i < s.fragments.Count; ++i) Debug.Log("Fr["+i+"] [" + s.fragments[i].line + "] [" + s.fragments[i].usedFunction + "]");
			
for (int i = 0; i < s.commentBlocks.Count; ++i) Debug.Log("Com[" + i + "] from : " + s.commentBlocks[i].index + " len:" + s.commentBlocks[i].len + " line:" + ControlFreak2.CFUtils.GetLineNumber(s.GetOriginalCode(), s.commentBlocks[i].index));

		}
*/

/*
	[MenuItem("Control Freak 2/Test Code Converter")]
	static void TESTCode()
		{
		ConvertedScript s = new ConvertedScript();
		s.ProcessTextFile("Test/Code/In/MouseLook.cs");	
		s.SaveToFile("Test/Code/Out/MouseLook-mod.cs");	
		}



	[MenuItem("Control Freak 2/Test param parsing")]
	static void TESTCode22()
		{

		Regex cf2bootstrap = new Regex(REGEX_DONT_CONVERT_TO_CF2_TAG_WITH_OPTIONAL_COMMENT, RegexOptions.IgnoreCase);

		string[] testStraps = new string[] {
			//@" // DONT convert to control freak 2   ",
			//@" // DON'T convert to control freak2   ",
			" // DON'T convert to control freak2   \n",
			" // DON'T convert to control  freak2 !! \n ",
			" // DON'T convert to controlfreak 2 !  \n",
			" // DON'T convert to control  freak  2.!.  \n ",
			" // DON'T convert to control freak  2.   \n",
			" // DON'T convert to control freak 2 !!  \n",
			" // DON'T convert to control freak2 !!! \n // xxx ",
			" // DON'T convert to control  freak2   \n //xxx  ",
			" // DON'T convert to controlfreak 2  \n //xxx   ",
			" // DON'T convert to control  freak  2  \n //xxx    ",
			" // DON'T convert to control freak  2   \n // xxx  ",
			" // DON'T convert to control freak 2   \n // xxx  ",
			" // DON'T convert to control freak 2 yy  \n // xxx  ",
			//@" // DONT_CONVERT to CF2 2   ",
			//@" // DON'T_CONVERT_TO_CONTROLFREAK2  ",
			//@" // CONVERT to CF2 2   ",
			
	};
	
		foreach (string strap in testStraps)
			Debug.Log("[" + cf2bootstrap.IsMatch(strap) + " ]  rep[" + cf2bootstrap.Replace(strap, "{CUT}").Replace("\n", "\\n") + "] full[" + strap + "]");




		ConvertedScript code = new ConvertedScript();
string[] keycodeParams = new string[] 
	{
	"  KeyCode.X",
	"  KeyCode.Return  ",
	"KeyCode.Escape  ",
	"\"x\"",
	" \"Y\" ",
	" \"escape\" ",
	"\"X\"",
	"  \"lshift\" , xxx"
	};


	foreach (string s in keycodeParams)
		{
		string s1 = s;
		bool isOk = code.IsKeyCodeParamConst(ref s1);
		Debug.Log("[" + s + "] -> " + isOk + " \t[" + s1 + "]");

		
		}
	}
*/		
		
		
	// ----------------
	public bool IsRelevant()
		{
		return ((this.fragments != null) && (this.fragments.Count > 0));
		}
		
	// ----------------
	public bool IsTaggedAsIgnored()
		{	
		return this.isTaggedWithDontConvert;
		}	


	// ---------------------
	private string SetDontConvertTag(string code, bool setTag)
		{
		if (this.ContainsDontConvertTag(code) == setTag)
			return code;

		if (setTag) 
			return (this.FixUnixLineEndings(STR_DONT_CONVERT_TO_CF2_TAG_FULL_LINE) + code);
		else
			return ConvertedScript.regexDontConvertTagWithOptionalComment.Replace(code, "");
		}


/*
	// ---------------------
	public bool SaveToFile(string filename)
		{
		if ((this.modifiedCode == null) || (this.modifiedCode.Length == 0))
			return false;

		try { 	System.IO.File.WriteAllText(filename, this.modifiedCode.ToString()); }
			catch (System.Exception ) { return false; }

		return true;
		}
*/

//static void DBBox(string msg) { EditorUtility.DisplayDialog("DB", msg, "OK"); }

	// ----------------------
	public bool ProcessText(string originalCode, Lang lang)
		{
		this.lang = lang;


//DBBox("CHeck tag");
		if (this.ContainsDontConvertTag(originalCode))
			{
			this.isTaggedWithDontConvert = true;
			//return false;
			}

//DBBox("start");
		this.Start(originalCode);	
//DBBox("Comments");
			
		this.CollectCommentBlocks();
//DBBox("frags");

		this.CollectFragments();
//DBBox("frags : " + this.fragments.Count);
			
		this.CalcConversionState();

		if (this.fragments.Count == 0)
			return false;
//DBBox("build output");

		this.BuildOutput();
//DBBox("done");

		return true;			
		}
		


	// -------------------------	
	public void ProcessTextFile(string filename)
		{
		string originalCode = System.IO.File.ReadAllText(filename); 
		ProcessText(originalCode, GetScriptLang(filename));
		}
		

	// ----------------------
	static public Lang GetScriptLang(string filename)
		{
		return (filename.EndsWith(".js", System.StringComparison.OrdinalIgnoreCase) ? Lang.JS : Lang.CS);
		}

	// ----------------------
	static public LineEndingStyle GetLineEndingStyle(string code)
		{
		return ((code.IndexOf("\r\n") >= 0) ? LineEndingStyle.Win : LineEndingStyle.Unix);
		}


	// --------------------
	private bool ContainsDontConvertTag(string code)
		{
		return ConvertedScript.regexDontConvertTagOnly.IsMatch(code);
		}



	// --------------------
	protected string GetOriginalCaptureValue(Capture capture)
		{
		return this.originalCode.Substring(capture.Index, capture.Length);
		}


	// --------------------
	public void Start(string originalCode)
		{
		this.originalCode = originalCode;
		this.sanitizedCode = originalCode;


		this.lineEndingStyle = GetLineEndingStyle(originalCode);

		// Init string builder...

		int outputCapacity = ((originalCode.Length * 12) / 10);
		if (this.modifiedCode == null)
			this.modifiedCode = new StringBuilder(outputCapacity);
		else 
			{
			this.modifiedCode.EnsureCapacity(outputCapacity);
			this.modifiedCode.Length = 0;
			}
			

		// Init comment blocks...

		if (this.commentBlocks == null)
			this.commentBlocks = new List<CommentBlock>(64);
		else
			this.commentBlocks.Clear();


		// Init fragments...

		if (this.fragments == null)
			this.fragments = new List<Fragment>(32);
		else
			this.fragments.Clear();	


		}



	// ---------------
	public void AddFragment(Fragment frag)
		{
		if (frag != null)
			this.fragments.Add(frag);
		}
		
		
		
	// ---------------
	public enum MethodParamType
		{
		NONE,
		KEYCODE,
		STRING,
		INT
		}


	// -----------------
	private class MethodDesc
		{
		public string			name;
		public MethodParamType	param;

		// -----------------
		public MethodDesc(string name)
			{
			this.name = name;
			this.param = MethodParamType.NONE;
			}

		// --------------------
		public MethodDesc(string name, MethodParamType param)
			{
			this.name = name;
			this.param = param;
			}
		}	




	// ---------------------
	// Fragment converter base class.
	// ---------------------
	private abstract class FragmentConverterBase
		{
		abstract public void CollectFragments(ConvertedScript script);
		}
		


	// ----------------------
	// Type converter class.
	// ----------------------
	private class InputTypeConverter : FragmentConverterBase
		{
		private Regex
			regexTypeDecl;
			
		const string CONVERTED_TOUCH_TYPE = "ControlFreak2.InputRig.Touch";

			
		// ------------------
		public InputTypeConverter() : base()
			{
			this.regexTypeDecl = new Regex(ConvertedScript.REGEX_UNITY_TOUCH_TYPE);
			}	

	

		// ---------------------
		private bool IsTypeSupported(string name)
			{
			return (name.Equals("Touch"));
			}

		// --------------------
		private string GetConvertedTypeName(string typeName)
			{
			if (typeName.Equals("Touch"))
				return CONVERTED_TOUCH_TYPE;
			
			return typeName;
			}
	


		// ----------------------
		private Fragment ProcessMatch(Match m, Regex regex, ConvertedScript script)
			{
			string typeName = m.Groups["type"].Value;
			if (!this.IsTypeSupported(typeName))
				return null;
	
			Fragment frag = new Fragment(ModType.InputType, m);
		
			frag.originalFrag	= script.GetOriginalCaptureValue(m);
			frag.modifiedFrag	= script.GetOriginalCaptureValue(m);
			frag.usedClass		= m.Groups["namespace"].Value;
			frag.usedFunction	= typeName;
	
			//if ((frag.problem = this.IsPropertyUnsupported(frag.usedFunction)) != ConvertedScript.ProblemType.None)
			//	{
			//	//frag.problem = ProblemType.Unsupported_Property;
			//	}
			//else if (!this.IsPropertySupported(frag.usedFunction))
			//	{
			//	frag.warning = ConvertedScript.WarningType.Unkown_Property;
			//	}
				
	
			// Build modified code...
				
			if ((frag.problem == ProblemType.None) && (frag.warning == ConvertedScript.WarningType.None))
				{
				frag.modifiedFrag = "";
		
				for (int i = 1; i < m.Groups.Count; ++i)
					{
					string groupName = regex.GroupNameFromNumber(i);
//Debug.Log("\tGroup[" + i + "][" + groupName +"] = ["+ m.Groups[i].Value+"]");
					if (groupName == "type")
						frag.modifiedFrag += this.GetConvertedTypeName(typeName);
					else if (groupName == "namespace")
						{} // skip namespace
					else 
						frag.modifiedFrag += script.GetOriginalCaptureValue(m.Groups[i]); //.Value;			
					}
				}		
//return null;
			return frag;
			}
				
			
		// -------------------
		override public void CollectFragments(ConvertedScript script)
			{
			// Collect methods...
	
			if (this.regexTypeDecl != null)
				{
				MatchCollection typeMatches = this.regexTypeDecl.Matches(script.sanitizedCode);
				
				if (typeMatches != null)
					{
//int matchNum = 0;
					foreach (Match m in typeMatches)
						{
						if (script.IsMatchCommentedOut(m))
							continue;

//Debug.Log("TYPE match[" + (matchNum++) + "] ["+ m.Value+"] line[" + ControlFreak2.CFUtils.GetLineNumber(script.originalCode, m.Index) + "]");

							
						Fragment typeFrag = this.ProcessMatch(m, this.regexTypeDecl, script);
						if (typeFrag != null)
							script.AddFragment(typeFrag);
						}
					}
				}		
			}
		
		}
	

	// ---------------------
	// Base class for all static classes to convert.
	// ---------------------
	private abstract class ClassConverterBase : FragmentConverterBase
		{
		abstract protected Regex GetMethodRegex();
		abstract protected Regex GetPropertyRegex();

		// -----------------
		abstract protected string GetConvertedClassName();
		abstract protected MethodDesc[] GetMethodDescriptions();
		abstract protected ModType GetMethodModType();
		abstract protected ModType GetPropModType();
		//abstract protected string[] GetIgnoredMethods		();
		//abstract protected string[] GetSupportedProperties	();
		//abstract protected string[] GetIgnoredProperties	();
			
		
		// ---------------------
		protected MethodDesc GetMethodDesc(string name)
			{
			MethodDesc[] descArray = this.GetMethodDescriptions();
			return (System.Array.Find<MethodDesc>(descArray, x => (x.name.Equals(name))));
			}
				
		// -------------------
		abstract public bool IsMethodSupported		(string name);	//	{ return (this.GetMethodDesc(name) != null); }
		abstract public bool IsMethodIgnored		(string name);	//	{ return FindStringInArray(this.GetIgnoredMethods(), name); }
		abstract public ProblemType IsMethodUnsupported	(string name);	//	{ return
 
		abstract public bool IsPropertySupported	(string name);	//	{ return FindStringInArray(this.GetSupportedProperties(), name); }
		abstract public bool IsPropertyIgnored		(string name);	//	{ return FindStringInArray(this.GetIgnoredProperties(), name); }
		abstract public ProblemType IsPropertyUnsupported	(string name);	//	{ return
	


		// --------------------	
		static protected bool FindStringInArray(string[] strArray, string strToFind)
			{
			return (System.Array.Find<string>(strArray, x => x.Equals(strToFind)) != null);
			}

	
		// --------------------
		private Fragment ProcessMethodMatch(Match m, Regex regex, ConvertedScript script)
			{
			string methodName = script.GetOriginalCaptureValue(m.Groups["method"]); //.Value;
			if (this.IsMethodIgnored(methodName))
				return null;	
	
			Fragment frag = new Fragment(this.GetMethodModType(), m);
		
			frag.originalFrag	= script.GetOriginalCaptureValue(m); //.Value;
			frag.modifiedFrag	= script.GetOriginalCaptureValue(m); //.Value;
			frag.usedClass		= script.GetOriginalCaptureValue(m.Groups["class"]); //.Value;
			frag.usedFunction	= methodName;
			frag.usedParameter	= script.GetOriginalCaptureValue(m.Groups["params"]); //.Value;
	
			MethodDesc methodDesc = this.GetMethodDesc(frag.usedFunction);
			if ((methodDesc != null))
				{
				if ((methodDesc.param != MethodParamType.NONE))
					{
					string paramStr = frag.usedParameter;
					if (!ConvertedScript.IsMethodParamConst(methodDesc.param, ref paramStr))
						{ } //frag.warning = WarningType.NON_CONST_PARAM;
					else
						frag.usedParameter = paramStr;
					}
				}
			else if ((frag.problem = this.IsMethodUnsupported(frag.usedFunction)) != ConvertedScript.ProblemType.None)
				{
				}
			else 	
				{
				frag.warning = ConvertedScript.WarningType.Unkown_Method;
				}

				
			// Build modified code...
	
			if ((frag.problem == ProblemType.None) && (frag.warning == ConvertedScript.WarningType.None))
				{
				frag.modifiedFrag = "";
				
				for (int i = 1; i < m.Groups.Count; ++i)
					{
					string groupName = regex.GroupNameFromNumber(i);
					if (groupName == "class")
						frag.modifiedFrag += this.GetConvertedClassName();
		
					else if (groupName == "method")
						frag.modifiedFrag += frag.usedFunction;
		
					else if (groupName == "params")
						frag.modifiedFrag += frag.usedParameter;
		
					else 
						frag.modifiedFrag += script.GetOriginalCaptureValue(m.Groups[i]); //.Value;			
		
		//Debug.Log("\t\tgr["+i+"]["+groupName+"] \t["  + m.Groups[i].Value + "] \t->[" + frag.modifiedFrag + "]");
					}
				}

			return frag;
			}
	
	
	
		// ----------------------
		private Fragment ProcessPropertyMatch(Match m, Regex regex, ConvertedScript script)
			{
			string propertyName = script.GetOriginalCaptureValue(m.Groups["prop"]); //.Value;
			if (this.IsPropertyIgnored(propertyName))
				return null;
	
			Fragment frag = new Fragment(this.GetPropModType(), m);
		
			frag.originalFrag	= script.GetOriginalCaptureValue(m); //.Value;
			frag.modifiedFrag	= script.GetOriginalCaptureValue(m); //.Value;
			frag.usedClass		= m.Groups["class"].Value;
			frag.usedFunction	= propertyName;
	
			if ((frag.problem = this.IsPropertyUnsupported(frag.usedFunction)) != ConvertedScript.ProblemType.None)
				{
				//frag.problem = ProblemType.Unsupported_Property;
				}
			else if (!this.IsPropertySupported(frag.usedFunction))
				{
				frag.warning = ConvertedScript.WarningType.Unkown_Property;
				}
				
	
			// Build modified code...
				
			if ((frag.problem == ProblemType.None) && (frag.warning == ConvertedScript.WarningType.None))
				{
				frag.modifiedFrag = "";
		
				for (int i = 1; i < m.Groups.Count; ++i)
					{
					string groupName = regex.GroupNameFromNumber(i);
					if (groupName == "class")
						frag.modifiedFrag += this.GetConvertedClassName();
					else 
						frag.modifiedFrag += script.GetOriginalCaptureValue(m.Groups[i]); //.Value;			
					}
				}		
			
			return frag;
			}
				
			
		// -------------------
		override public void CollectFragments(ConvertedScript script)
			{
			// Collect methods...
	
			if (this.GetMethodRegex() != null)
				{
				MatchCollection methodMatches = this.GetMethodRegex().Matches(script.sanitizedCode);
				
				if (methodMatches != null)
					{
					foreach (Match m in methodMatches)
						{
						if (script.IsMatchCommentedOut(m))
							continue;
							
						Fragment methodFrag = this.ProcessMethodMatch(m, this.GetMethodRegex(), script);
						if (methodFrag != null)
							script.AddFragment(methodFrag);
						}
					}
				}			
	
			// Collect properties...
	
			if (this.GetPropertyRegex() != null)
				{				
	
				MatchCollection inputPropertyMatches = this.GetPropertyRegex().Matches(script.sanitizedCode);
				
				if (inputPropertyMatches != null)
					{
					foreach (Match m in inputPropertyMatches)
						{
						if (script.IsMatchCommentedOut(m))
							continue;
					
						Fragment propFrag = this.ProcessPropertyMatch(m, this.GetPropertyRegex(), script);
						if (propFrag != null)
							script.AddFragment(propFrag);
		
		
						}
					}
				}	
			}
		}
		


	// ---------------------
	// Input class converter.
	// ------------------
	private class InputClassConverter : ClassConverterBase
		{	
		private Regex
			methodRegex,
			propRegex;

		// ----------------------
		public InputClassConverter() : base()
			{
			this.methodRegex	= new Regex(ConvertedScript.REGEX_INPUT_METHOD);
			this.propRegex		= new Regex(ConvertedScript.REGEX_INPUT_PROPERTY);
			}


		override protected string GetConvertedClassName()		{ return "ControlFreak2.CF2Input"; }
		override protected ModType GetMethodModType()			{ return ModType.InputMethod; }
		override protected ModType GetPropModType()				{ return ModType.InputProperty; }
		override protected Regex GetMethodRegex() 				{ return this.methodRegex; }
		override protected Regex GetPropertyRegex()				{ return this.propRegex; }

		override protected MethodDesc[] GetMethodDescriptions() { return mMethodDescriptions; }
		//override protected string[] GetIgnoredMethods()			{ return mIgnoredMethods; }
		//override protected string[] GetSupportedProperties()	{ return mSupportedProps; }
		//override protected string[] GetIgnoredProperties()		{ return mIgnoredProps; }
		
		override public bool IsMethodSupported		(string name)	{ return (this.GetMethodDesc(name) != null); }
		override public bool IsMethodIgnored		(string name)	{ return FindStringInArray(mIgnoredMethods, name); }

		override public bool IsPropertySupported	(string name)	{ return FindStringInArray(mSupportedProps, name); }
		override public bool IsPropertyIgnored		(string name)	{ return FindStringInArray(mIgnoredProps, name); }
		
		// -----------------
		override public ProblemType IsMethodUnsupported		(string name)	
			{
			if (FindStringInArray(mControlFreak1Methods, name))
				return ProblemType.Control_Freak_1_Feature;

			return ProblemType.None;
			} 


		// -----------------
 		override public ProblemType IsPropertyUnsupported	(string name)
			{
			if (FindStringInArray(mControlFreak1Props, name))
				return ProblemType.Control_Freak_1_Feature;

			return ProblemType.None;
			}


	

		// ---------------------
		static private MethodDesc[] mMethodDescriptions = new MethodDesc[] {
			new MethodDesc("ResetInputAxes"),
	
			new MethodDesc("GetAxis",				MethodParamType.STRING),
			new MethodDesc("GetAxisRaw",			MethodParamType.STRING),
	
			new MethodDesc("GetButton",				MethodParamType.STRING),
			new MethodDesc("GetButtonDown",			MethodParamType.STRING),
			new MethodDesc("GetButtonUp",			MethodParamType.STRING),
	
			new MethodDesc("GetKey",				MethodParamType.KEYCODE),
			new MethodDesc("GetKeyDown",			MethodParamType.KEYCODE),
			new MethodDesc("GetKeyUp",				MethodParamType.KEYCODE),
	
			new MethodDesc("GetMouseButton",		MethodParamType.INT),
			new MethodDesc("GetMouseButtonDown",	MethodParamType.INT),
			new MethodDesc("GetMouseButtonUp",		MethodParamType.INT),
	
	
			new MethodDesc("GetTouch",				MethodParamType.INT),
		
			new MethodDesc("ControllerActive")
	
			};
	
		// ---------------
		static private string[] mIgnoredMethods = new string[] 
			{
			"GetAccelerationEvent",
			"GetJoystickNames",
			//"GetTouch",
			"IsJoystickPreconfigured"
			};
	
	
			
		// ---------------
		static private string[] mSupportedProps = new string[] {
			"mousePosition",
			"mouseScrollDelta",
			"anyKey",	
			"anyKeyDown",
			"touchCount",
			"touches",	
			"simulateMouseWithTouches",
			};
	
		// ---------------
		static private string[] mIgnoredProps = new string[] 
			{
			"acceleration",
			"accelerationEventCount",
			"accelerationEvents",
			"gyro",
			"compass",
			"compensateSensors",
			"compositionCursorPos",
			"compositionString",
			"deviceOrientation",
			"imeCompositionMode",
			"imeIsSelected",
			"inputString",
			"location",
			"multiTouchEnabled",
			"touchSupported",
			"mousePresent"
			};
			
		// ----------------
		static private string[] mControlFreak1Methods = new string[]
			{
			
			};

		// ----------------
		static private string[] mControlFreak1Props = new string[]
			{
			"ctrl"
			};


		}
		

		
	// ---------------------
	// Screen class converter.
	// ------------------
	private class ScreenClassConverter : ClassConverterBase
		{	
		private Regex
			methodRegex,
			propRegex;

		// ----------------------
		public ScreenClassConverter() : base()
			{
			this.methodRegex	= new Regex(ConvertedScript.REGEX_SCREEN_METHOD);
			this.propRegex		= new Regex(ConvertedScript.REGEX_SCREEN_PROPERTY);
			}


		override protected string GetConvertedClassName()		{ return "ControlFreak2.CFScreen"; }
		override protected ModType GetMethodModType()			{ return ModType.ScreenMethod; }
		override protected ModType GetPropModType()				{ return ModType.ScreenProperty; }
		override protected Regex GetMethodRegex() 				{ return this.methodRegex; }
		override protected Regex GetPropertyRegex()				{ return this.propRegex; }

		override protected MethodDesc[] GetMethodDescriptions() { return mMethodDescriptions; }
		//override protected string[] GetIgnoredMethods()			{ return mIgnoredMethods; }
		//override protected string[] GetSupportedProperties()	{ return mSupportedProps; }
		//override protected string[] GetIgnoredProperties()		{ return mIgnoredProps; }
		
		override public bool IsMethodSupported		(string name)	{ return (this.GetMethodDesc(name) != null); }
		override public bool IsMethodIgnored		(string name)	{ return FindStringInArray(mIgnoredMethods, name); }
 
		override public bool IsPropertySupported	(string name)	{ return FindStringInArray(mSupportedProps, name); }
		override public bool IsPropertyIgnored		(string name)	{ return FindStringInArray(mIgnoredProps, name); }

		override public ProblemType IsMethodUnsupported		(string name)	{ return ProblemType.None; } 
		override public ProblemType IsPropertyUnsupported	(string name)	{ return ProblemType.None; }
	

		// ---------------------
		static private MethodDesc[] mMethodDescriptions = new MethodDesc[] {
			new MethodDesc("SetResolution"),
			};
	
		// ---------------
		static private string[] mIgnoredMethods = new string[] 
			{
			//"GetAccelerationEvent",
			//"GetJoystickNames",
			//"GetTouch",
			//"IsJoystickPreconfigured"
			};
	
	
			
		// ---------------
		static private string[] mSupportedProps = new string[] {
			"dpi",
			"lockCursor",
			"showCursor",
			"fullScreen"		
			};
	
		// ---------------
		static private string[] mIgnoredProps = new string[] 
			{
			"width",
			"height",	
			"autorotateToLandscapeLeft",
			"autorotateToLandscapeRight",
			"autorotateToPortrait",
			"autorotateToPortraitUpsideDown",
			"currentResolution",
			//"fullScreen",
			"orientation",
			"resolutions",
			"sleepTimeout"				
			};


		}



	// ---------------------
	// Cursor class converter.
	// ------------------
	private class CursorClassConverter : ClassConverterBase
		{	
		private Regex
			//methodRegex,
			propRegex;

		// ----------------------
		public CursorClassConverter() : base()
			{
			//this.methodRegex	= new Regex(ConvertedScript.REGEX_CURSOR_METHOD);
			this.propRegex		= new Regex(ConvertedScript.REGEX_CURSOR_PROPERTY);
			}


		override protected string GetConvertedClassName()		{ return "ControlFreak2.CFCursor"; }
		override protected ModType GetMethodModType()			{ return ModType.CursorMethod; }
		override protected ModType GetPropModType()				{ return ModType.CursorProperty; }
		override protected Regex GetMethodRegex() 				{ return null; } //this.methodRegex; }
		override protected Regex GetPropertyRegex()				{ return this.propRegex; }

		override protected MethodDesc[] GetMethodDescriptions() { return mMethodDescriptions; }
		//override protected string[] GetIgnoredMethods()			{ return mIgnoredMethods; }
		//override protected string[] GetSupportedProperties()	{ return mSupportedProps; }
		//override protected string[] GetIgnoredProperties()		{ return mIgnoredProps; }
		
		override public bool IsMethodSupported		(string name)	{ return (this.GetMethodDesc(name) != null); }
		override public bool IsMethodIgnored		(string name)	{ return FindStringInArray(mIgnoredMethods, name); }
 
		override public bool IsPropertySupported	(string name)	{ return FindStringInArray(mSupportedProps, name); }
		override public bool IsPropertyIgnored		(string name)	{ return FindStringInArray(mIgnoredProps, name); }

		override public ProblemType IsMethodUnsupported		(string name)	{ return ProblemType.None; } 
		override public ProblemType IsPropertyUnsupported	(string name)	{ return ProblemType.None; }
	

		// ---------------------
		static private MethodDesc[] mMethodDescriptions = new MethodDesc[] {
			//new MethodDesc("ResetInputAxes"),
	
			//new MethodDesc("GetAxis",				MethodParamType.STRING),
			};
	
		// ---------------
		static private string[] mIgnoredMethods = new string[] 
			{
			//"GetAccelerationEvent",
			//"GetJoystickNames",
			//"GetTouch",
			//"IsJoystickPreconfigured"
			};
	
	
			
		// ---------------
		static private string[] mSupportedProps = new string[] {
			"lockState",
			"visible"		
			};
	
		// ---------------
		static private string[] mIgnoredProps = new string[] 
			{
				
			};


		}




	

	// ------------------
	static public bool IsMethodParamConst(MethodParamType paramType, ref string paramStr)
		{
		switch (paramType)
			{
			case MethodParamType.INT		: return IsIntParamConst(ref paramStr);
			case MethodParamType.KEYCODE	: return IsKeyCodeParamConst(ref paramStr);
			case MethodParamType.STRING		: return IsStringParamConst(ref paramStr);
			}

		return true; 
		}
		

	// -----------------
	static public bool IsStringParamConst(ref string paramStr)
		{
		return ConvertedScript.regexStringParam.IsMatch(paramStr);
		}

	// -----------------
	static public bool IsIntParamConst(ref string paramStr)
		{
		return ConvertedScript.regexIntParam.IsMatch(paramStr);
		}

	

/*
	// -------------------
	private struct NameToKeyCode 
		{
		public string name;
		public KeyCode key;

		
		// --------------
		public NameToKeyCode(string name, KeyCode key)
			{
			this.name = name;
			this.key = key;
			}
		}
		

	// ----------------------
	static private NameToKeyCode[] mNameToKeyCodeLookup = new NameToKeyCode[] {
		new NameToKeyCode("enter",			KeyCode.Return),
		new NameToKeyCode("return", 		KeyCode.Return),
		new NameToKeyCode("escape", 		KeyCode.Escape),
		new NameToKeyCode("esc",			KeyCode.Escape),
		new NameToKeyCode("space", 			KeyCode.Space),
		new NameToKeyCode("spacebar", 		KeyCode.Space),
		new NameToKeyCode("shift", 			KeyCode.LeftShift),
		new NameToKeyCode("lshift", 		KeyCode.LeftShift),
		new NameToKeyCode("l shift", 		KeyCode.LeftShift),
		new NameToKeyCode("left shift",		KeyCode.LeftShift),
		new NameToKeyCode("leftshift",		KeyCode.LeftShift),
		new NameToKeyCode("rshift", 		KeyCode.RightShift),
		new NameToKeyCode("r shift", 		KeyCode.RightShift),
		new NameToKeyCode("right shift",	KeyCode.RightShift),
		new NameToKeyCode("rightshift",		KeyCode.RightShift),

		};
	*/	


	// -----------------
 	static public bool IsKeyCodeParamConst(ref string paramStr)
		{
		if (ConvertedScript.regexKeyCodeEnumParam.IsMatch(paramStr))
			return true;

		Match strMatch = ConvertedScript.regexStringParam.Match(paramStr);		
		if ((strMatch == null) || !strMatch.Success || (strMatch.Groups.Count != 2))
			{
//Debug.Log("Keycode stirng not a match!!");
			return false;
			}
			
//foreach (Capture cc in strMatch.Groups[1].Captures) { Debug.Log("\t\tCap [" + cc.Value + "]"); }
 
		Capture c = strMatch.Groups[1].Captures[0];
		if (c == null)
			{
//Debug.Log("Keycode capture null!");

			return false;		
			}

		string cval = c.Value.Substring(1, c.Value.Length - 2);
	
			
//Debug.Log("\tcval: [" + cval + "]");


		KeyCode targetKey = ControlFreak2.InputRig.NameToKeyCode(cval);

/*
		KeyCode.None;

		// Single character keycode...

		if (cval.Length == 1)
			{
			char keyChar = cval[0];

			if ((keyChar >= 'a') && (keyChar <= 'z'))
				targetKey = KeyCode.A + (keyChar - 'a');
 
			else if ((keyChar >= 'A') && (keyChar <= 'Z'))
				targetKey = KeyCode.A + (keyChar - 'A'); 

			else if ((keyChar >= '0') && (keyChar <= '9'))
				targetKey = KeyCode.Alpha0 + (keyChar - '0');
			} 


		if (targetKey == KeyCode.None)
			{
			for (int i = 0; i < mNameToKeyCodeLookup.Length; ++i)
				{
				if (cval.Equals(mNameToKeyCodeLookup[i].name, System.StringComparison.InvariantCultureIgnoreCase))
					{
					targetKey = mNameToKeyCodeLookup[i].key;
					break;
					}			
				}
			}
*/


		if (targetKey == KeyCode.None)
			{
//Debug.Log("\t Keycode not found");
			return false;
			}

		// Substitue strign for a KeyCode enum...
			
		paramStr = (
			((c.Index > 0) ? paramStr.Substring(0, c.Index) : "") +
			"KeyCode." + targetKey.ToString() + 
			(((c.Index + c.Length) < paramStr.Length) ? paramStr.Substring((c.Index + c.Length)) : "") );

		return true;
		}
		
	/*

	// -------------------
	private void ProcessInputMethodMatch(Match m, Regex regex)
		{
		string methodName = m.Groups["method"].Value;
		if (IsInputMethodIgnored(methodName))
			return;


		Fragment frag = new Fragment(ModType.InputMethod, m);
	
		frag.originalFrag	= m.Value;
		frag.usedClass		= m.Groups["class"].Value;
		frag.usedFunction	= methodName;
		frag.usedParameter	= m.Groups["params"].Value;

		MethodDesc methodDesc = GetInputMethodDesc(frag.usedFunction);
		if (methodDesc == null)
			{
			frag.problem = ProblemType.UNSUPPORTED_METHOD;
			}
		else
			{
			string paramStr = frag.usedParameter;
			if (!this.IsInputMethodParamConst(methodDesc.param, ref paramStr))
				frag.warning = WarningType.NON_CONST_PARAM;
			else
				frag.usedParameter = paramStr;
			}
			
		// Build modified code...

		if (frag.problem != ProblemType.NONE)
			{
			frag.modifiedFrag = frag.originalFrag;
			}
		else
			{
			frag.modifiedFrag = "";
			
			for (int i = 1; i < m.Groups.Count; ++i)
				{
				string groupName = regex.GroupNameFromNumber(i);
				if (groupName == "class")
					frag.modifiedFrag += "CF2Input";
	
				else if (groupName == "method")
					frag.modifiedFrag += frag.usedFunction;
	
				else if (groupName == "params")
					frag.modifiedFrag += frag.usedParameter;
	
				else 
					frag.modifiedFrag += m.Groups[i].Value;			
	
	//Debug.Log("\t\tgr["+i+"]["+groupName+"] \t["  + m.Groups[i].Value + "] \t->[" + frag.modifiedFrag + "]");
				}
			}
		
		this.fragments.Add(frag);
		}



	// ----------------------
	private void ProcessInputPropertyMatch(Match m, Regex regex)
		{
		string propertyName = m.Groups["prop"].Value;
		if (IsInputPropertyIgnored(propertyName))
			return;

		Fragment frag = new Fragment(ModType.InputProperty, m);
	
		frag.originalFrag	= m.Value;
		frag.usedClass		= m.Groups["class"].Value;
		frag.usedFunction	= propertyName;

		if (!this.IsInputPropertySupported(frag.usedFunction))
			{
			frag.problem = ProblemType.UNSUPPORTED_PROP;
			}
			

		// Build modified code...
			
		if (frag.problem != ProblemType.NONE)
			{
			frag.modifiedFrag = frag.originalFrag;
			}
		else
			{
			frag.modifiedFrag = "";
	
			for (int i = 1; i < m.Groups.Count; ++i)
				{
				string groupName = regex.GroupNameFromNumber(i);
				if (groupName == "class")
					frag.modifiedFrag += "CF2Input";
				else 
					frag.modifiedFrag += m.Groups[i].Value;			
				}
			}		
		
		this.fragments.Add(frag);
		}
	
		
*/


	// -------------------
	public bool IsModified()
		{	
		return (this.fragments.Count > 0);
		}
		


	// ------------------
	private enum CodeBlockType
		{
		None,
		Comment,
		LineComment,
		String,
		AltString
		}

	// -------------------
	private void CollectCommentBlocks()
		{
		this.commentBlocks.Clear();

		StringBuilder 
			sanitizedBuilder = new StringBuilder(this.originalCode.Length);

		char 
			curC = '\0', 
			prevC = '\0';
		
		bool 
			blockOpen = false;
		int 
			blockStart = 0;

		CodeBlockType
			blockType = CodeBlockType.None; 

		for (int i = 0; i < this.originalCode.Length; ++i, prevC = curC)
			{
			curC = this.originalCode[i];

			// Try to open a new block...

			if (!blockOpen)
				{
				if (curC == '\"')
					{
					blockOpen = true;
					blockType = CodeBlockType.String;
					blockStart = i;
					}

				else if (curC == '\'')
					{
					blockOpen = true;
					blockType = CodeBlockType.AltString;
					blockStart = i;
					}

				else if ((curC == '/') && (prevC == '/'))
					{
					blockOpen = true;
					blockType = CodeBlockType.LineComment;
					blockStart = i - 1;
					}

				else if ((prevC == '/') && (curC == '*') )
					{
					blockOpen = true;
					blockType = CodeBlockType.Comment;
					blockStart = i - 1;
					}
				

				if (!blockOpen || ((blockType != CodeBlockType.Comment) && (blockType != CodeBlockType.LineComment)))
					sanitizedBuilder.Append(curC);
				else
					{
					// Remove prev char and add double space placeholder...

					sanitizedBuilder.Remove(sanitizedBuilder.Length - 1, 1);
					sanitizedBuilder.Append("  ");
					}

				}

			// Close an open block...
			else
				{
				if ((blockType == CodeBlockType.LineComment) || (blockType == CodeBlockType.Comment))
					sanitizedBuilder.Append((curC == '\n') ? '\n' : ' ');
				else
					sanitizedBuilder.Append(curC);

				if (
					((blockType == CodeBlockType.String)		&& (curC == '\"') && (prevC != '\\')) ||
					((blockType == CodeBlockType.AltString)	&& (curC == '\'') && (prevC != '\\')) ||
					((blockType == CodeBlockType.Comment)		&& (prevC == '*') && (curC == '/')) ||
					((blockType == CodeBlockType.LineComment)	&& (curC == '\n')) )
					{
					blockOpen = false;
					this.commentBlocks.Add(new CommentBlock(blockStart, (i - blockStart)));
					}					 
				}

			
			} 


		// Add unclosed block...

		if (blockOpen)
			this.commentBlocks.Add(new CommentBlock(blockStart, (this.originalCode.Length - blockStart)));
		

	
		// Apply sanitized string...

		this.sanitizedCode = sanitizedBuilder.ToString();


/*
		Match m = ConvertedScript.regexBlockComment.Match(this.originalCode);
		for (; m.Success; m = m.NextMatch())
			{
			this.commentBlocks.Add(new CommentBlock(m));
			}

		m = ConvertedScript.regexLineComment.Match(this.originalCode);
		for (; m.Success; m = m.NextMatch())
			{
			if (this.IsMatchCommentedOut(m))
				continue;

			this.commentBlocks.Add(new CommentBlock(m));
			}			
*/
		}
		

	// -------------------
	private bool IsMatchCommentedOut(Match m)
		{
		int count = this.commentBlocks.Count;
		for (int i = 0; i < count; ++i)
			{
			if (this.commentBlocks[i].IsMatchCommentedOut(m))
				return true;
			}

		return false;
		}
		
		
	// -------------------
	//private string ConvertFragmentToCF2Input(string originalFrag)
	//	{
	//	return this.regexInputClass.Replace(originalFrag, "CF2Input");
	//	}
		

	
	static private FragmentConverterBase[] mClassConverters = new FragmentConverterBase[]
		{
		new InputClassConverter(),
		new ScreenClassConverter(),
		new CursorClassConverter(),
		new InputTypeConverter()
		};


	// ------------------
	private bool CollectFragments()
		{
		for (int i = 0; i < mClassConverters.Length; ++i)
			{
			mClassConverters[i].CollectFragments(this);
			}


/*
		// Collect Input methods...

		MatchCollection inputMethodMatches = this.regexInputMethod.Matches(this.originalCode);
		
		if (inputMethodMatches != null)
			{
			foreach (Match m in inputMethodMatches)
				{
				if (this.IsMatchCommentedOut(m))
					continue;
					
				this.ProcessInputMethodMatch(m, this.regexInputMethod);

				}
			}
		

		// Collect Input properties...

		MatchCollection inputPropertyMatches = this.regexInputProperty.Matches(this.originalCode);
		
		if (inputPropertyMatches != null)
			{
			foreach (Match m in inputPropertyMatches)
				{
				if (this.IsMatchCommentedOut(m))
					continue;
			
				this.ProcessInputPropertyMatch(m, this.regexInputProperty);


				}
			}
*/

		

			
		// Calculate line numbers for fragments...

		this.SortFragments();
			

		this.CalculateFragmentLines();
			

		// Categorize fragments...

		foreach (Fragment frag in this.fragments)
			{
			if (frag.problem != ProblemType.None)
				this.errorFragments.Add(frag);

			else if (frag.warning != WarningType.None)
				this.warningFragments.Add(frag);

			else
				this.okFragments.Add(frag);
			}
		

		return true;
		}
		
	// -------------------	
	private void SortFragments()
		{
		if (this.fragments != null)
			this.fragments.Sort(Fragment.CompareByFilePos);
		}


	// -------------------
	private void CalculateFragmentLines()
		{
		int curLine = 1;
		int lastLineStart = 0;
		
		foreach (Fragment frag in this.fragments)
			{
			int nextLineStart = -1;
			while (((nextLineStart = this.originalCode.IndexOf('\n', lastLineStart)) >= 0) && (nextLineStart < frag.filePos))
				{ 
				curLine++;	
				lastLineStart = nextLineStart + 1;
				}

			frag.line = curLine;
			}
		}
		

	// ----------------------	
	private void CalcConversionState()
		{
		if (this.fragments.Count == 0)
			{
			this.convState = ConvState.NothingChanged;
			return;		
			}

		this.convState = ConvState.Ok;

		foreach (Fragment frag in this.fragments)
			{	
			if (frag.problem != ProblemType.None)
				{
				this.convState = ConvState.Problematic;
				break;
				}

			if (frag.warning != WarningType.None)
				{
				if (this.convState == ConvState.Ok)
					this.convState = ConvState.OkWithWarnings;
				}
			}
		}



	// -----------------
	private string FixUnixLineEndings(string s)
		{
		if (this.lineEndingStyle == LineEndingStyle.Unix)
			return s;	
		else
			return s.Replace("\n", "\r\n");
		}

	// ---------------

	private bool BuildOutput()
		{
		int originalPos = 0;
		int originalLen = this.originalCode.Length;
			
		int curFragId = 0;

		this.modifiedCode.Length = 0;

		// Add DGT header...

		//this.modifiedCode.Append(
		//	this.FixUnixLineEndings("// Code auto-converted by Control Freak 2 on " + System.DateTime.Now.ToLongDateString() + "!\n") +
		//	//((this.lang == Lang.CS) ? 
		//	//	"using ControlFreak2;\n\n" :
		//	//	"import ControlFreak2;\n\n") +
		//	"");


		// Convert code..

		while (originalPos < originalLen)
			{
			Fragment frag = ((curFragId < this.fragments.Count) ? this.fragments[curFragId] : null);
			if (frag != null)
				{
				if (originalPos < frag.filePos)
					{
					this.modifiedCode.Append(this.originalCode, originalPos, (frag.filePos - originalPos));
					originalPos = frag.filePos;	
					}
					
				this.modifiedCode.Append(frag.modifiedFrag);			
				originalPos = frag.filePos + frag.originalFrag.Length;
					 
				++curFragId;
				}
			else
				{
				this.modifiedCode.Append(this.originalCode, originalPos, (originalLen - originalPos));

				originalPos = originalLen;
				break;
				}
			}
			
		// Clear DONT_CONVERT Tag!
			
		string finalCode = this.modifiedCode.ToString();
		this.modifiedCode.Length = 0;	
		this.modifiedCode.Append( this.SetDontConvertTag(finalCode, false));
	

		return true;
		}
	

	
	
	// ----------------------
	private struct CommentBlock
		{
		public int index, len;
	
		// -----------------------
		public CommentBlock(Match m)
			{
			this.index	= m.Index;
			this.len	= m.Length;
			}

		// -----------------
		public CommentBlock(int index, int len)
			{
			this.index = index;
			this.len = len;
			}
			
		// ------------------------
		public bool IsMatchCommentedOut(Match m)
			{
			return ((m == null) || ((m.Index >= this.index) && ((m.Index + m.Length) < (this.index + this.len))));
			}
		}
		

	
	// ----------------------
	public enum ModType
		{
		InputMethod,
		InputProperty,

		ScreenMethod,
		ScreenProperty,
		
		CursorMethod,
		CursorProperty,

		InputType,

		ControlFreak1
		}
	
	// --------------------
	public class Fragment 
		{
		public ProblemType	problem;
		public WarningType	warning;
		public ModType		modType;
		public string		originalFrag;
		public string		modifiedFrag;
		public string		usedClass;
		public string		usedFunction;
		public string		usedParameter;
		public bool			paramIsConst;
		public int			line;
		public int			filePos;
	
		
	
		// --------------------
		public Fragment(ModType modType, Match match)
			{
			this.modType = modType;	
			this.filePos = match.Index;
			this.originalFrag = match.Value;	
	
	//			this.filePos = match
			}
	
		// --------------------
		static public int CompareByFilePos(Fragment a, Fragment b)
			{
			return ((a.filePos < b.filePos) ? -1 : (a.filePos == b.filePos) ? 0 : 1); 
			}


		// ----------------------
		public string GetLogLine()
			{
			string s = "Line: " + this.line.ToString().PadLeft(4) +  
				((this.problem != ConvertedScript.ProblemType.None) ? ("\t" + this.problem.ToString()) : "") + "\t";
 
			switch (this.modType)
				{
				case ModType.InputMethod :
					s += "InputMethod   " + this.usedFunction.PadRight(20) + "\t(" + this.usedParameter + ")";
					break;
	
				case ModType.InputProperty :
					s += "InputProperty " + this.usedFunction.PadRight(20);
					break;

				default :
					s += "Other mod.\t[" + this.originalFrag.Replace("\n", " ") + "]";
					break;
				}

			return s;
			}
	
		}
		
		
	}

}

#endif
