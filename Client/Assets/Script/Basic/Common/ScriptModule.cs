//// **********************************************************************
//// Copyright (c) 2009 All Rights Reserved
//// File     : Script.cs
//// Author   : cjq
//// Created  : 2010-04-21
//// Porpuse  : 脚本模块
//// **********************************************************************
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.IO;
//using System.Reflection;
//using System.CodeDom.Compiler;

//using Microsoft.CSharp;
//using OBJECT_DIC = System.Collections.Generic.Dictionary<string, object>;
///***********************************************************************/

//namespace Script
//{
//        public class Driver
//        {
//                /// <summary>
//                /// 脚本扩散名
//                /// </summary>
//                private static string scriptExt = ".cs";
//                /// <summary>
//                /// 需要加载的引用
//                /// </summary>
//                private static HashSet<string> m_setReference = new HashSet<string>();
//                /// <summary>
//                /// 命名空间序列
//                /// </summary>
//                private static int m_nSpaceSN = 0;
//                /// <summary>
//                /// 脚本函数分析结果集
//                /// </summary>
//                private static Dictionary<string, SplitInfo> m_dicSplitInfo = new Dictionary<string, SplitInfo>();

//                //////////////////////////////////////////////////////////////////////////
//                // Script下的所有文件都将编译成类，类名 = ScriptNum.文件名
//                /// <summary>
//                /// 脚本对象字典
//                /// </summary>
//                private static OBJECT_DIC m_dicScript = new OBJECT_DIC();
//                /// <summary>
//                /// 命名空间
//                /// </summary>
//                private static string m_namespace { get { return "Script" + m_nSpaceSN.ToString(); } }
//                /// <summary>
//                /// 任意模块对象
//                /// </summary>
//                //public static CCliModuleBase mod { get { return ScriptModule.Instance(); } }
//                /// <summary>
//                /// 环境对象 触发者
//                /// </summary>
//                public static object actor { get; set; }
//                /// <summary>
//                /// 环境对象 目标
//                /// </summary>
//                public static object target { get; set; }
//                /// <summary>
//                /// 环境对象 物品
//                /// </summary>
//                public static object item { get; set; }
//                /// <summary>
//                /// 环境对象 批量
//                /// </summary>
//                public static object quantity { get; set; }


//                static Assembly currentAssembly = null;
//                //////////////////////////////////////////////////////////////////////////
//                public static string datapath = "";
//                /// <summary>
//                /// 脚本函数分析结果
//                /// </summary>
//                struct SplitInfo
//                {
//                        /// <summary>
//                        /// 脚本实例
//                        /// </summary>
//                        public object instance { get; set; }
//                        /// <summary>
//                        /// 函数
//                        /// </summary>
//                        public string strFunc { get; set; }
//                        /// <summary>
//                        /// 参数
//                        /// </summary>
//                        public object[] arrParams { get; set; }
//                }

//                // 		private class ScriptModule : CCliModuleBase
//                // 		{
//                // 			private static ScriptModule m_mod = new ScriptModule();
//                // 			public static ScriptModule Instance() { return m_mod; }
//                // 			public override object OnFuncCalled(string strFunc, string strPram) { return null; }
//                // 		}

//                /// 构造函数
//                /// </summary>
//                private Driver()
//                {
//                }

//                /// <summary>
//                /// 添加引用
//                /// </summary>
//                /// <param name="strRef">引用名</param>
//                public static void AddReference(string strRef)
//                {
//                        string str = strRef;

//                        if (!m_setReference.Contains(str))
//                        {
//                                m_setReference.Add(str);
//                        }
//                }

//                /// <summary>
//                /// 载入脚本资源
//                /// </summary>
//                /// <param name="filePath">脚本文件路径数组</param>
//                /// <returns>成功返回true，否则返回false</returns>
//                public static bool Load()
//                {
//                        //mod.DebugLog("Prepare script file.");
//                        // 脚本默认加入以下引用
//                        AddReference("Script.dll");
//                        AddReference("System.dll");
//                        AddReference("System.Core.dll");
//                        AddReference("UnityEngine.dll");
//                        //AddReference("ModDotNetWrapper.dll");
//                        //AddReference("Helper.dll");

////                         string[] strFiles = Directory.GetFiles("./", "MMod*.dll");
////                         foreach (string strFile in strFiles)
////                         {
////                                 Assembly MyAssembly = Assembly.LoadFrom(strFile);
////                                 string modName = MyAssembly.GetName().Name.Replace(".dll", "");
////                                 AddReference(modName + ".dll");
////                         }

//                        if (null == datapath || 0 == datapath.Length)
//                        {
//                                return false;
//                        }
//                        string[] arrFiles = ScanScript(datapath);

//                        string strTmpFile, strClass;
//                        StringBuilder buiFunc = new StringBuilder();
//                        m_nSpaceSN++;
//                        string[] szCode = new string[arrFiles.Length + 1];
//                        HashSet<string> setClass = new HashSet<string>();

//                        string strTempPath = Path.GetTempPath() + "df123456789zzz223hhttyyuikliopddrthmyhy" + "\\";
//                        if (!Directory.Exists(strTempPath))
//                        {
//                                Directory.CreateDirectory(strTempPath);
//                        }
//                        else
//                        {
//                                string[] vecstr = ScanScript(strTempPath);
//                                foreach (string str in vecstr)
//                                {
//                                        File.Delete(str);
//                                }
//                        }

//                        // 组织代码&编译
//                        int nIndex = 0;
//                        if (!CreateBaseScript(strTempPath, out strTmpFile))
//                        {
//                                return false;
//                        }
//                        szCode[nIndex++] = strTmpFile;
//                        foreach (string filePath in arrFiles)
//                        {
//                                if (!Encode(filePath, strTempPath, out strClass, out strTmpFile))
//                                {
//                                        return false;
//                                }
//                                szCode[nIndex++] = strTmpFile;
//                                if (strTmpFile.Length != 0)
//                                {
//                                        if (setClass.Contains(strClass))
//                                        { // 重名
//                                                Log("[Script Error] too many files are same name (" + strClass + ".cs)");
//                                                return false;
//                                        }
//                                        setClass.Add(strClass);
//                                }
//                        }
//                        bool bRes = Complile(szCode, setClass);
//#if !DEBUG
//                        for (int n = 0; n < szCode.Length; n++)
//                        {
//                                if (szCode[n].Length != 0)
//                                {
//                                        File.Delete(szCode[n]);
//                                }
//                        }
//                        Directory.Delete(strTempPath);
//#endif
//                        return bRes;
//                }

//                /// <summary>
//                /// 扫描目录，获取所有脚本文件
//                /// </summary>
//                /// <param name="strDir">目录</param>
//                /// <returns>文件数组</returns>
//                protected static string[] ScanScript(string strDir)
//                {
//                        if (!Directory.Exists(strDir))
//                        {
//                                return new string[0];
//                        }

//                        string[] arrFiles = Directory.GetFiles(strDir, "*" + scriptExt);
//                        string[] arrDirs = Directory.GetDirectories(strDir);

//                        if (0 != arrDirs.Length)
//                        {
//                                foreach (string dir in arrDirs)
//                                {
//                                        string[] arrSub = ScanScript(dir);
//                                        string[] newFiles = new string[arrSub.Length + arrFiles.Length];
//                                        arrFiles.CopyTo(newFiles, 0);
//                                        arrSub.CopyTo(newFiles, arrFiles.Length);
//                                        arrFiles = newFiles;
//                                }
//                        }

//                        return arrFiles;
//                }

//                /// <summary>
//                /// 编译
//                /// </summary>
//                /// <param name="szCode">脚本代码</param>
//                /// <param name="setClass">脚本类</param>
//                /// <returns>成功返回true，否则返回false</returns>
//                protected static bool Complile(string[] szCode, HashSet<string> setClass)
//                {
//                        //mod.DebugLog("Start complite script.");
//                        CompilerResults results = null;
//                        using (CSharpCodeProvider provider = new CSharpCodeProvider())
//                        {
//                                CompilerParameters options = new CompilerParameters();
//                                options.GenerateInMemory = true;
//#if DEBUG
//                                options.IncludeDebugInformation = true;
//#endif
//                                options.CompilerOptions = "-codepage:65001 -nowarn:0219,0414";//force source file codepage to utf8, nowarn: value or member created,but no reference
//                                foreach (string r in m_setReference)
//                                {
//                                        options.ReferencedAssemblies.Add(r);
//                                }

//                                results = provider.CompileAssemblyFromFile(options, szCode);
//                                if (results.Errors.HasErrors || results.Errors.HasWarnings)
//                                {
//                                        StringBuilder errorMessage = new StringBuilder();
//                                        foreach (CompilerError error in results.Errors)
//                                        {
//                                                errorMessage.AppendFormat("[{0}:{5}]File {1}.cs Line {2} Column {3} : {4}\n", error.IsWarning ? "Warning" : "Error", GetFileName(error.FileName), error.Line, error.Column, error.ErrorText, error.ErrorNumber);
//                                        }
//                                        string returnData = errorMessage.ToString();

//                                        Log("[Script Complile Result]");
//                                        Log(returnData);

//                                        //mod.Log("debug", "[Script Complile Result]");
//                                        //mod.Log("debug", returnData);

//                                        if (results.Errors.HasErrors)
//                                        {
//                                                return false;
//                                        }
//                                }

//                                if (results.CompiledAssembly == null)
//                                {
//                                        return false;
//                                }

//                                currentAssembly = results.CompiledAssembly;
//                                m_dicScript.Clear();
//                                m_dicSplitInfo.Clear();
//                                foreach (string strClass in setClass)
//                                {
//                                        m_dicScript.Add(strClass, null);
//                                }
//                        }

//                        return true;
//                }

//                public static bool Init()
//                {
//                        //mod.DebugLog("Init script.");
//                        OBJECT_DIC dicScript = new OBJECT_DIC();

//                        foreach (var scrpt in m_dicScript.Keys)
//                        {
//                                Type _type = currentAssembly.GetType(m_namespace + "." + scrpt);
//                                object obj = _type.InvokeMember("m_instance", BindingFlags.GetField, null, null, null);
//                                if (null == obj)
//                                {
//                                        Log("[Script Complile Error] " + scrpt + ".cs");
//                                        return false;
//                                }
//                                dicScript[scrpt] = obj;
//                        }

//                        m_dicScript = dicScript;

//                        //mod.ProcessModMsg(0, "loadscriptcfg");
//                        return true;
//                }

//                /// <summary>
//                /// 获取文件名
//                /// </summary>
//                /// <param name="filePath">文件路径</param>
//                /// <returns>成功返回文件名，否则返回空串</returns>
//                protected static string GetFileName(string filePath)
//                {
//                        if (null == filePath || "" == filePath || !File.Exists(filePath))
//                        {
//                                return "";
//                        }

//                        FileInfo info = new FileInfo(filePath);
//                        return info.Name.Replace(".cs", "");
//                }

//                /// <summary>
//                /// 创建基础脚本类
//                /// </summary>
//                /// <param name="strTempPath">临时路径</param>
//                /// <param name="strTmpFile">返回的临时代码文件路径</param>
//                /// <returns>成功返回true,否则返回false</returns>
//                protected static bool CreateBaseScript(string strTempPath, out string strTmpFile)
//                {
//                        strTmpFile = strTempPath + "base_script.cs";
//                        if (File.Exists(strTmpFile))
//                        {
//                                File.Delete(strTmpFile);
//                        }
//                        using (StreamWriter sw = File.CreateText(strTmpFile))
//                        {
//                                // 以下是以文件绝对行数进行组织代码,所以如果报错,对应行就是文件错误行.
//                                sw.Write("using System;");
//                                sw.Write("public class base_script");
//                                sw.Write("{");
////                                 sw.Write("  public static object m_actor { get { return Script.Driver.actor; } }");
////                                 sw.Write("  public static object m_target { get { return Script.Driver.target; } }");
////                                 sw.Write("  public static object m_item { get { return Script.Driver.item; } }");
//                                //sw.Write("  public static CCliModuleBase m_mod { get { return Script.Driver.mod; } }");
//                                sw.Write("}");

//                                sw.Close();
//                        }
//                        return true;
//                }

//                /// <summary>
//                /// 文件代码分析
//                /// </summary>
//                /// <param name="filePath">文件</param>
//                /// <param name="strTempPath">临时文件侠</param>
//                /// <param name="strClass">返回的类名</param>
//                /// <param name="strTmpFile">返回的临时代码文件路径</param>
//                /// <returns>成功返回true,否则返回false</returns>
//                protected static bool Encode(string filePath, string strTempPath, out string strClass, out string strTmpFile)
//                {
//                        strClass = "";
//                        strTmpFile = "";

//                        strClass = GetFileName(filePath);
//                        if ("" == strClass)
//                        {
//                                return false;
//                        }

//                        using (FileStream file = File.OpenRead(filePath))
//                        {
//                                byte[] arrFlag = new byte[3];
//                                int nRead = file.Read(arrFlag, 0, 3);
//                                if (nRead == 3 && (arrFlag[0] != 0xefU || arrFlag[1] != 0xbbU || arrFlag[2] != 0xbfU))
//                                {
//                                        Log(string.Format("[Script Warning] The encoding of {0}.cs is no utf-8 !", strClass));
//                                }
//                                strTmpFile = strTempPath + strClass + ".cs";
//                                if (File.Exists(strTmpFile))
//                                {
//                                        File.Delete(strTmpFile);
//                                }
//                                using (StreamWriter sw = File.CreateText(strTmpFile))
//                                {
//                                        string strData;
//                                        file.Seek(0, SeekOrigin.Begin);
//                                        StreamReader stream = new StreamReader(file, Encoding.UTF8);
//                                        // 以下是以文件绝对行数进行组织代码,所以如果报错,对应行就是文件错误行.
//                                        bool bUsing = true;
//                                        sw.Write("using System;");
//                                        bool bEndSkip = true;
//                                        while (null != (strData = stream.ReadLine()))
//                                        {
//                                                if (bUsing)
//                                                {
//                                                        string strTmp = strData;
//                                                        strTmp.Replace(" ", "");
//                                                        strTmp.Replace("\t", "");
//                                                        if (0 != strTmp.Length)
//                                                        { // 一直找到最后一个using
//                                                                if (!strTmp.StartsWith("using") && !strTmp.StartsWith("//") && !strTmp.StartsWith("/*") && bEndSkip)
//                                                                {
//                                                                        bUsing = false;
//                                                                        sw.Write("namespace " + m_namespace + "{" + "public class " + strClass + " : base_script{");
//                                                                }
//                                                                else if (strTmp.StartsWith("/*"))
//                                                                {
//                                                                        bEndSkip = strTmp.EndsWith("*/");
//                                                                }
//                                                                else if (strTmp.EndsWith("*/"))
//                                                                {
//                                                                        bEndSkip = true;
//                                                                }
//                                                        }
//                                                }
//                                                if (strData.Contains("m_mod.DebugLog"))
//                                                {
//                                                        Log(string.Format("[Script Warning] File {0}.cs call the function \"m_mod.DebugLog !\"", strClass));
//                                                        strData = strData.Replace("m_mod.DebugLog", "sys.debuglog");
//                                                }
//                                                sw.Write(strData + "\n");
//                                        }

//                                        if (bUsing)
//                                        {
//                                                sw.Write(" namespace " + m_namespace + "{" + "public class " + strClass + " : base_script{");
//                                        }
//                                        // 以下是一些类固定代码
//                                        sw.Write("public static " + strClass + " m_instance" + " = new " + strClass + "();\n");
//                                        sw.Write("}\n}");

//                                        stream.Close();
//                                }
//                                file.Close();
//                        }
//                        return true;
//                }

//                /// <summary>
//                /// 执行脚本命令
//                /// </summary>
//                /// <param name="objScript">脚本类</param>
//                /// <param name="strFunc">脚本函数名</param>
//                /// <param name="pszObj">参数集</param>
//                /// <returns>由脚本返回的对象</returns>
//                protected static object Execute(object objScript, string strFunc, object[] pszObj)
//                {
//                        if (null == objScript)
//                        {
//                                UnityEngine.Debug.Log("here 2");
//                                return null;
//                        }

//                        try
//                        {

//                                object loResult = objScript.GetType().InvokeMember(
//                                                    strFunc, BindingFlags.InvokeMethod,
//                                                    null, objScript, pszObj);

//                                UnityEngine.Debug.Log("here 3");
//                                return loResult;
//                        }
//                        catch (Exception loError)
//                        {
//                                Log("[Script Error : " + objScript.GetType().Name + "." + strFunc + "] " + loError);
//                                if (null != pszObj)
//                                {
//                                        string strParam = "Parameters: ";
//                                        foreach (object obj in pszObj)
//                                        {
//                                                strParam += obj.ToString() + ",";
//                                        }
//                                        Log(strParam);
//                                }
//                                return null;
//                        }
//                }

//                /// <summary>
//                /// 执行脚本命令
//                /// </summary>
//                protected static object Execute(string strFunc, object[] szObj)
//                {
//                        UnityEngine.Debug.Log(strFunc);

//                        int nDot = strFunc.IndexOf('.');
//                        if (nDot == -1)
//                        {
//                                UnityEngine.Debug.LogError("here 0");
//                                return null;
//                        }

//                        string strFileName = strFunc.Substring(0, nDot);
//                        string strFunction = strFunc.Substring(nDot + 1, strFunc.Length - nDot - 1);
//                        UnityEngine.Debug.Log(strFileName + " " + strFunc);
//                        object objScript;
//                        if (!m_dicScript.TryGetValue(strFileName, out objScript) || null == objScript)
//                        {
//                                UnityEngine.Debug.Log("here 1");
//                                return null;
//                        }
//                        return Execute(objScript, strFunction, szObj);
//                }

//                /// <summary>
//                /// 解析参数
//                /// </summary>
//                /// <param name="strCode">函数式的语句，如FileName.Fuction(a,b)</param>
//                /// <param name="objScript">脚本对象</param>
//                /// <param name="strFunc">函数名</param>
//                /// <param name="arrObj">参数集</param>
//                /// <returns>成功返回true,否则返回false</returns>
//                protected static bool SliptParam(string strCode, out object objScript, out string strFunc, out object[] arrObj)
//                {
//                        objScript = null;
//                        strFunc = null;
//                        arrObj = null;

//                        int nDot = strCode.IndexOf('.');
//                        int nPre = strCode.IndexOf("(");
//                        int nLast = strCode.LastIndexOf(")");

//                        if (nDot == -1 || nLast == -1 || nPre == -1)
//                        {
//                                return false;
//                        }

//                        string strClass = strCode.Substring(0, nDot);
//                        strFunc = strCode.Substring(nDot + 1, nPre - nDot - 1);
//                        string strParam = strCode.Substring(nPre + 1, nLast - nPre - 1);
//                        string[] arrStr = null;

//                        if (0 != strParam.Length)
//                                arrStr = strParam.Split(',');

//                        if (!m_dicScript.TryGetValue(strClass, out objScript) || null == objScript)
//                                return false;

//                        MethodInfo methodInfo = objScript.GetType().GetMethod(strFunc);
//                        if (null == methodInfo)
//                                return false;

//                        ParameterInfo[] types = methodInfo.GetParameters();
//                        if (null == types)
//                                return (null == arrStr || arrStr.Length == 0);

//                        if ((null == arrStr && 0 != types.Length) || (null != arrStr && types.Length != arrStr.Length))
//                                return false;

//                        arrObj = new object[types.Length];
//                        int n = 0, m = 0;

//                        foreach (ParameterInfo paramType in types)
//                        {
//                                if (paramType.ParameterType.Name == "String")
//                                {
//                                        int nBegin = arrStr[m].IndexOf('"');
//                                        if (-1 == nBegin)
//                                        {
//                                                if (arrStr[m].Contains("null"))
//                                                        arrObj[n] = null;
//                                                else
//                                                        return false;
//                                        }
//                                        else
//                                        {
//                                                int nEnd = arrStr[m].LastIndexOf('"');
//                                                if (-1 == nEnd || nEnd == nBegin)
//                                                        return false;

//                                                if (nBegin > 0)
//                                                        arrObj[n] = arrStr[m].Substring(nBegin + 1, nEnd - nBegin - 1);
//                                                else
//                                                        arrObj[n] = arrStr[m];
//                                        }
//                                }
//                                else
//                                {
//                                        if (arrStr[m].Contains("null"))
//                                                arrObj[n] = null;
//                                        else if (arrStr[m].Contains("true"))
//                                                arrObj[n] = true;
//                                        else if (arrStr[m].Contains("false"))
//                                                arrObj[n] = false;
//                                        else
//                                                arrObj[n] = paramType.ParameterType.InvokeMember("Parse", BindingFlags.InvokeMethod, null, null, new object[] { arrStr[m] });
//                                }

//                                ++m; ++n;
//                        }

//                        return true;
//                }

//                /// <summary>
//                /// 是否为脚本
//                /// </summary>
//                /// <param name="strCode">字串</param>
//                /// <returns>是返回true, 否则返回false</returns>
//                public static bool IsScript(string strCode)
//                {
//                        if (null == strCode || strCode.Length <= 3)
//                        {
//                                return false;
//                        }

//                        return true;
//                }

//                /// <summary>
//                /// 执行脚本命令
//                /// </summary>
//                /// <param name="strCode">函数式的语句，如FileName.Fuction(a,b)</param>
//                /// <returns>由脚本返回的对象</returns>
//                public static object ExecuteScript(string strCode)
//                {
//                        string strFunc;
//                        object objScript;
//                        object[] arrParam;
//                        SplitInfo split;
//                        if (!m_dicSplitInfo.TryGetValue(strCode, out split))
//                        {
//                                if (!SliptParam(strCode, out objScript, out strFunc, out arrParam))
//                                {
//                                        Log("[Script Error]" + strCode);
//                                        return null;
//                                }
//                                split = new SplitInfo();
//                                split.instance = objScript;
//                                split.strFunc = strFunc;
//                                split.arrParams = arrParam;
//                                m_dicSplitInfo.Add(strCode, split);
//                        }

//                        return Execute(split.instance, split.strFunc, split.arrParams);
//                }

//                /// <summary>
//                /// 执行脚本命令
//                /// </summary>
//                public static object Execute(string strFunc)
//                {
//                        return Execute(strFunc, null);
//                }

//                /// <summary>
//                /// 执行脚本命令
//                /// </summary>
//                public static object Execute(string strFunc, object p0)
//                {
//                        object[] szObj = new object[] { p0 };
//                        return Execute(strFunc, szObj);
//                }

//                /// <summary>
//                /// 执行脚本命令
//                /// </summary>
//                public static object Execute(string strFunc, object p0, object p1)
//                {
//                        object[] szObj = new object[] { p0, p1 };
//                        return Execute(strFunc, szObj);
//                }

//                /// <summary>
//                /// 执行脚本命令
//                /// </summary>
//                public static object Execute(string strFunc, object p0, object p1, object p2)
//                {
//                        object[] szObj = new object[] { p0, p1, p2 };
//                        return Execute(strFunc, szObj);
//                }

//                /// <summary>
//                /// 执行脚本命令
//                /// </summary>
//                public static object Execute(string strFunc, object p0, object p1, object p2, object p3)
//                {
//                        object[] szObj = new object[] { p0, p1, p2, p3 };
//                        return Execute(strFunc, szObj);
//                }

//                /// <summary>
//                /// 执行脚本命令
//                /// </summary>
//                public static object Execute(string strFunc, object p0, object p1, object p2, object p3, object p4)
//                {
//                        object[] szObj = new object[] { p0, p1, p2, p3, p4 };
//                        return Execute(strFunc, szObj);
//                }

//                /// <summary>
//                /// 执行脚本命令
//                /// </summary>
//                public static object Execute(string strFunc, object p0, object p1, object p2, object p3, object p4, object p5)
//                {
//                        object[] szObj = new object[] { p0, p1, p2, p3, p4, p5 };
//                        return Execute(strFunc, szObj);
//                }

//                /// <summary>
//                /// 执行脚本命令
//                /// </summary>
//                public static object Execute(string strFunc, object p0, object p1, object p2, object p3, object p4, object p5, object p6)
//                {
//                        object[] szObj = new object[] { p0, p1, p2, p3, p4, p5, p6 };
//                        return Execute(strFunc, szObj);
//                }

//                /// <summary>
//                /// 执行脚本命令
//                /// </summary>
//                public static object Execute(string strFunc, object p0, object p1, object p2, object p3, object p4, object p5, object p6, object p7)
//                {
//                        object[] szObj = new object[] { p0, p1, p2, p3, p4, p5, p6, p7 };
//                        return Execute(strFunc, szObj);
//                }

//                /// <summary>
//                /// 设置脚本变量
//                /// </summary>
//                /// <param name="strField">变量名</param>
//                /// <param name="obj">变量值</param>
//                /// <returns>成功返回true,否则返回false</returns>
//                public static bool SetField(string strField, object obj)
//                {
//                        int nDot = strField.IndexOf('.');
//                        if (nDot == -1)
//                        {
//                                return false;
//                        }

//                        string strFileName = strField.Substring(0, nDot);
//                        string field = strField.Substring(nDot + 1, strField.Length - nDot - 1);

//                        object objScript;
//                        if (!m_dicScript.TryGetValue(strFileName, out objScript) || null == objScript)
//                        {
//                                return false;
//                        }

//                        try
//                        {
//                                objScript.GetType().InvokeMember(field, BindingFlags.SetField, null, objScript, new object[] { obj });

//                                return true;
//                        }
//                        catch (Exception loError)
//                        {
//                                System.Diagnostics.Debug.WriteLine("[Script Error : SetField(" + strField + ")] " + loError);
//                                return false;
//                        }
//                }

//                /// <summary>
//                /// 获取脚本变量
//                /// </summary>
//                /// <param name="strField">变量名</param>
//                /// <returns>变量</returns>
//                public static object GetField(string strField)
//                {
//                        int nDot = strField.IndexOf('.');
//                        if (nDot == -1)
//                        {
//                                return null;
//                        }

//                        string strFileName = strField.Substring(0, nDot);
//                        string field = strField.Substring(nDot + 1, strField.Length - nDot - 1);

//                        object objScript;
//                        if (!m_dicScript.TryGetValue(strFileName, out objScript) || null == objScript)
//                        {
//                                return null;
//                        }

//                        try
//                        {
//                                return objScript.GetType().InvokeMember(field, BindingFlags.GetField, null, objScript, null);
//                        }
//                        catch (Exception loError)
//                        {
//                                Log("[Script Error : GetField(" + strField + ")] " + loError);
//                                return null;
//                        }
//                }

//                /// <summary>
//                /// 输出日志
//                /// </summary>
//                /// <param name="msg"></param>
//                protected static void Log(string msg)
//                {
//                        Console.WriteLine(msg);
//                        UnityEngine.Debug.LogError(msg);
//                        //             if (mod != null)
//                        //             {
//                        //                 mod.Log("script", msg);
//                        //             }
//                        System.Diagnostics.Debug.WriteLine(msg);
//                }
//        }
//}
