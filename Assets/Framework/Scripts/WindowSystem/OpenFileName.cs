using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;


/*
#define OFN_READONLY                        0x00000001
#define OFN_OVERWRITEPROMPT          0x00000002
#define OFN_HIDEREADONLY                0x00000004
#define OFN_NOCHANGEDIR                 0x00000008
#define OFN_SHOWHELP                      0x00000010
#define OFN_ENABLEHOOK                   0x00000020
#define OFN_ENABLETEMPLATE           0x00000040
#define OFN_ENABLETEMPLATEHANDLE     0x00000080
#define OFN_NOVALIDATE                     0x00000100
#define OFN_ALLOWMULTISELECT         0x00000200
#define OFN_EXTENSIONDIFFERENT       0x00000400
#define OFN_PATHMUSTEXIST              0x00000800
#define OFN_FILEMUSTEXIST                0x00001000
#define OFN_CREATEPROMPT               0x00002000
#define OFN_SHAREAWARE                  0x00004000
#define OFN_NOREADONLYRETURN        0x00008000
#define OFN_NOTESTFILECREATE                   0x00010000
#define OFN_NONETWORKBUTTON        0x00020000
#define OFN_NOLONGNAMES                        0x00040000   
*/

namespace Framework
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class OpenFileName
    {
        public int structSize = 0;
        public IntPtr dlgOwner = IntPtr.Zero;
        public IntPtr instance = IntPtr.Zero;
        public String filter = null;
        public String customFilter = null;
        public int maxCustFilter = 0;
        public int filterIndex = 0;
        public String file = null;
        public int maxFile = 0;
        public String fileTitle = null;
        public int maxFileTitle = 0;
        public String initialDir = null;
        public String title = null;
        public int flags = 0;
        public short fileOffset = 0;
        public short fileExtension = 0;
        public String defExt = null;
        public IntPtr custData = IntPtr.Zero;
        public IntPtr hook = IntPtr.Zero;
        public String templateName = null;
        public IntPtr reservedPtr = IntPtr.Zero;
        public int reservedInt = 0;
        public int flagsEx = 0;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class OpenDialogDir
    {
        public IntPtr hwndOwner = IntPtr.Zero;
        public IntPtr pidlRoot = IntPtr.Zero;
        public String pszDisplayName = null;
        public String lpszTitle = null;
        public UInt32 ulFlags = 0;
        public IntPtr lpfn = IntPtr.Zero;
        public IntPtr lParam = IntPtr.Zero;
        public int iImage = 0;
    }

    public class WindowDll
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        //链接指定系统函数       打开文件对话框
        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

        //链接指定系统函数        另存为对话框
        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        static extern bool GetSaveFileName([In, Out] OpenFileName ofn);

        [DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SHBrowseForFolder([In, Out] OpenDialogDir ofn);

        [DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool SHGetPathFromIDList([In] IntPtr pidl, [In, Out] char[] fileName);

#endif

        public static bool GetOpenFileNameWindows(OpenFileName ofn)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return GetOpenFileName(ofn);
#else
        return false;
#endif
        }
        public static bool GetSaveFileNameWindows(OpenFileName ofn)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return GetSaveFileName(ofn);
#else
        return false;
#endif
        }
        public static IntPtr GetSaveDialogWindows(OpenDialogDir ofn)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return SHBrowseForFolder(ofn);
#else
        return IntPtr.Zero;
#endif
        }
        public static bool GetDialogPathList(IntPtr ptr, char[] files)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return SHGetPathFromIDList(ptr, files);
#else
        return false;
#endif
        }
    }

}