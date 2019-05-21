using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class ExcelHelperInspector : EditorWindow
    {
        private ExcelHelperInspector _currentWindow = null;
		private string _importPath = string.Empty;
        private string _exportPath = string.Empty;


        [MenuItem("shaco/Tools/ExcelHelperInspector", false, (int)ToolsGlobalDefine.MenuPriority.Tools.EXCEL_HELPER_INSPECTOR)]
        static void OpenExcelHelperInspector()
        {
            shacoEditor.EditorHelper.GetWindow<ExcelHelperInspector>(null, true, "ExcelHelperInspector");
        }

		void OnEnable()
		{
            _currentWindow = shacoEditor.EditorHelper.GetWindow<ExcelHelperInspector>(this, true, "ExcelHelperInspector");
            _currentWindow.LoadSettings();
        }

		void OnDestroy()
		{
            SaveSettings();
		}

		void OnGUI()
		{
			if (null == _currentWindow)
				return;

			this.Repaint();

            _importPath = GUILayoutHelper.PathField("Import Excel Path", _importPath, string.Empty);
            _exportPath = GUILayoutHelper.PathField("Export Script Path", _exportPath, string.Empty);

			EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_importPath) || string.IsNullOrEmpty(_importPath));
			{
                if (GUILayout.Button("Generate Code"))
                {
					//遍历导入目录中所有的excel相关文件
					var findFiles = new List<string>();
					shaco.Base.FileHelper.GetSeekPath(_importPath, ref findFiles, false, ".xlsx", ".xls", ".csv");

                    //删除目标目录原有文件
                    if (findFiles.Count > 0)
					{
						var deleteFiles = new List<string>();
                        shaco.Base.FileHelper.GetSeekPath(_exportPath, ref deleteFiles, false, "cs");
						for (int i = 0; i < deleteFiles.Count; ++i)
						{
							shaco.Base.FileHelper.DeleteByUserPath(deleteFiles[i]);
						}
                    }

					//自动生成脚本文件到导出目录
					for (int i = 0; i < findFiles.Count; ++i)
					{
						var fileNameTmp = shaco.Base.FileHelper.GetLastFileName(findFiles[i]);
                        fileNameTmp = shaco.Base.FileHelper.RemoveExtension(fileNameTmp);
						var exportPathTmp = shaco.Base.FileHelper.ContactPath(_exportPath, fileNameTmp);
						var excelDataTmp = shaco.Base.ExcelHelper.OpenWithFile(findFiles[i]);
						excelDataTmp.SerializableAsCSharpScript(exportPathTmp);
					}

					SaveSettings();

					if (findFiles.Count > 0)
					{
						AssetDatabase.Refresh();
					}
					else 
					{
						Debug.LogWarning("ExcelHelperInspector Generate Code warning: no thing need export");
					}
                }

				if (GUILayout.Button("Generate Txt File"))
				{
                    //遍历导入目录中所有的excel相关文件
                    var findFiles = new List<string>();
                    shaco.Base.FileHelper.GetSeekPath(_importPath, ref findFiles, false, "xlsx", "xls", "csv");

                    //删除目标目录原有文件
                    if (findFiles.Count > 0)
                    {
                        var deleteFiles = new List<string>();
                        shaco.Base.FileHelper.GetSeekPath(_exportPath, ref deleteFiles, false, "txt");
                        for (int i = 0; i < deleteFiles.Count; ++i)
                        {
                            shaco.Base.FileHelper.DeleteByUserPath(deleteFiles[i]);
                        }
                    }

                    //自动转换excel为txt到导出目录
                    for (int i = 0; i < findFiles.Count; ++i)
                    {
                        var fileNameTmp = shaco.Base.FileHelper.GetLastFileName(findFiles[i]);
                        fileNameTmp = shaco.Base.FileHelper.RemoveExtension(fileNameTmp);
                        var exportPathTmp = shaco.Base.FileHelper.ContactPath(_exportPath, fileNameTmp);
                        var excelDataTmp = shaco.Base.ExcelHelper.OpenWithFile(findFiles[i]);
                        excelDataTmp.SaveAsTxt(exportPathTmp);
                    }

                    SaveSettings();

                    if (findFiles.Count > 0)
                    {
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        Debug.LogWarning("ExcelHelperInspector Generate Txt File warning: no thing need export");
                    }
				}
			}
            EditorGUI.EndDisabledGroup();
		}

		private void SaveSettings()
		{
            shaco.DataSave.Instance.Write("ExcelHelperInspector+_importPath", _importPath);
            shaco.DataSave.Instance.Write("ExcelHelperInspector+_exportPath", _exportPath);
		}

		private void LoadSettings()
		{
			_importPath = shaco.DataSave.Instance.ReadString("ExcelHelperInspector+_importPath");
            _exportPath = shaco.DataSave.Instance.ReadString("ExcelHelperInspector+_exportPath");
		}
    }
}