using System.Collections.Generic;

namespace iExcelExport.DB
{
	public class ExcelSolution
	{
		private string m_name;

		private string m_path;

		private List<ExcelXMLLayout> m_layouts;

		private ExcelManager m_mgr;

		public string text
		{
			get
			{
				return this.m_name;
			}
		}

		public string path
		{
			get
			{
				return this.m_path;
			}
			set
			{
				this.m_path = value;
			}
		}

		public string name
		{
			get
			{
				return this.m_name;
			}
			set
			{
				this.m_name = value;
			}
		}

		public ExcelSolution(ExcelManager mgr)
		{
			this.m_name = "新Excel文件";
			this.m_layouts = new List<ExcelXMLLayout>(10);
			this.m_mgr = mgr;
		}

		public void AddLayout(ExcelXMLLayout layout)
		{
			this.m_layouts.Add(layout);
		}

		public IEnumerable<ExcelXMLLayout> EachLayout()
		{
			return this.m_layouts;
		}

		public void RemoveLayout(ExcelXMLLayout layout)
		{
			this.m_layouts.Remove(layout);
		}
	}
}
