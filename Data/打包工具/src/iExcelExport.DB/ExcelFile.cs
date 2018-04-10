using System;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using System.Text;

namespace iExcelExport.DB
{
	public class ExcelFile
	{
		public static string lastError = "";

		public static string MakeConnectionString(string file)
		{
			return string.Format("Provider=Microsoft.ACE.OleDb.12.0;Data Source={0};Extended Properties='Excel 12.0;HDR=Yes'", file);
		}

		public static string MakeSelectString(ExcelXMLLayout layout)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("SELECT ");
			int num = 1;
			foreach (ExcelXMLLayout.KeyPair item in layout.EachKeyPair())
			{
				stringBuilder.Append("[");
				stringBuilder.Append(item.name);
				stringBuilder.Append("]");
				if (num != layout.count)
				{
					stringBuilder.Append(",");
				}
				num++;
			}
			stringBuilder.Append(" FROM [");
			stringBuilder.Append(layout.sheet);
			stringBuilder.Append("$]");
			return stringBuilder.ToString();
		}

		public static bool IsHeadRow(OleDbDataReader r, ExcelXMLLayout layout)
		{
			for (int i = 0; i < layout.count; i++)
			{
				if (layout[i].name.ToLower() == "id")
				{
					string text = ((DbDataReader)r)[i].ToString();
					if (!(text == "") && (text.Length < 2 || !(text.Substring(0, 2) == "__")) && (text.Length < 2 || !(text.Substring(0, 2) == "表头")) && (text.Length < 2 || !(text.Substring(0, 2) == "编号")))
					{
						return false;
					}
					return true;
				}
			}
			return false;
		}

		public static bool DoExport(ExcelXMLLayout layout)
		{
			try
			{
				OleDbConnection oleDbConnection = new OleDbConnection(ExcelFile.MakeConnectionString(layout.solution.path));
				StringBuilder stringBuilder = new StringBuilder(1048576);
				try
				{
					oleDbConnection.Open();
					OleDbCommand oleDbCommand = new OleDbCommand(ExcelFile.MakeSelectString(layout), oleDbConnection);
					try
					{
						OleDbDataReader oleDbDataReader = oleDbCommand.ExecuteReader();
						stringBuilder.Append("<?xml version='1.0' encoding='utf-8'?>");
						stringBuilder.Append("<dataroot>");
						while (oleDbDataReader.Read())
						{
							if (!ExcelFile.IsHeadRow(oleDbDataReader, layout))
							{
								stringBuilder.AppendFormat("<{0}>", layout.name);
								for (int i = 0; i < oleDbDataReader.FieldCount; i++)
								{
									if (layout.typed)
									{
										stringBuilder.AppendFormat("<{0} type='{1}'>{2}</{0}>", layout[i].name, ExcelXMLLayout.Type2Int(layout[i].type), ((DbDataReader)oleDbDataReader)[i].ToString());
									}
									else
									{
										stringBuilder.AppendFormat("<{0}>{1}</{0}>", layout[i].name, ((DbDataReader)oleDbDataReader)[i].ToString());
									}
								}
								stringBuilder.AppendFormat("</{0}>", layout.name);
							}
						}
						stringBuilder.Append("</dataroot>");
						oleDbDataReader.Dispose();
					}
					catch (Exception ex)
					{
						ExcelFile.lastError = string.Format("表格查询命令\r\n{0}", ex.ToString());
						oleDbCommand.Dispose();
						oleDbConnection.Close();
						oleDbConnection.Dispose();
						return false;
					}
					oleDbCommand.Dispose();
					oleDbConnection.Close();
					oleDbConnection.Dispose();
				}
				catch (Exception ex2)
				{
					ExcelFile.lastError = string.Format("表格打开失败\r\n{0}", ex2.ToString());
					oleDbConnection.Dispose();
					return false;
				}
				try
				{
					TextWriter textWriter = File.CreateText(layout.path);
					textWriter.Write(stringBuilder.ToString());
					textWriter.Close();
				}
				catch (Exception ex3)
				{
					ExcelFile.lastError = string.Format("目标XML文件无法写入\r\n{0}", ex3.ToString());
					return false;
				}
				return true;
			}
			catch (Exception ex4)
			{
				ExcelFile.lastError = string.Format("Excel 无法打开\r\n{0}", ex4.ToString());
			}
			return false;
		}

		public static bool AnalyzeExcel(ExcelXMLLayout layout)
		{
			OleDbConnection oleDbConnection = null;
			try
			{
				oleDbConnection = new OleDbConnection(ExcelFile.MakeConnectionString(layout.solution.path));
				oleDbConnection.Open();
				DataTable oleDbSchemaTable = oleDbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[4]
				{
					null,
					null,
					layout.sheet + "$",
					null
				});
				layout.Clear();
				foreach (DataRow row in oleDbSchemaTable.Rows)
				{
					string text = row["Column_Name"].ToString();
					ExcelXMLLayout.KeyType keyType = ExcelXMLLayout.KeyType.Unknown;
					OleDbCommand oleDbCommand = new OleDbCommand(string.Format("select [{0}] from [{1}$]", text, layout.sheet), oleDbConnection);
					OleDbDataReader oleDbDataReader = oleDbCommand.ExecuteReader();
					while (oleDbDataReader.Read())
					{
						if (!(((DbDataReader)oleDbDataReader)[0].GetType() == typeof(double)))
						{
							if (keyType == ExcelXMLLayout.KeyType.String)
							{
								break;
							}
							keyType = ExcelXMLLayout.KeyType.String;
							continue;
						}
						keyType = ExcelXMLLayout.KeyType.Integer;
						break;
					}
					oleDbDataReader.Close();
					oleDbCommand.Dispose();
					layout.Add(text, keyType);
				}
				oleDbSchemaTable.Dispose();
				oleDbConnection.Close();
				return true;
			}
			catch (Exception ex)
			{
				ExcelFile.lastError = string.Format("无法分析，Excel 无法打开\r\n{0}", ex.Message);
			}
			return false;
		}
	}
}
