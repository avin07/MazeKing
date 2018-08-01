using System.Collections.Generic;

namespace iExcelExport.DB
{
	public class ExcelXMLLayout
	{
		public enum KeyType
		{
			Integer,
			String,
			Unknown
		}

		public class KeyPair
		{
			public string name;

			public KeyType type;

			public int index;

			public string text
			{
				get
				{
					return string.Format("{0}:{1}", this.name, ExcelXMLLayout.Type2Text(this.type));
				}
			}
		}

		private string m_path;

		private string m_sheet;

		private string m_name;

		private string m_summary;

		private List<KeyPair> m_allkey;

		private ExcelSolution m_solution;

		private bool m_typed;

		public bool typed
		{
			get
			{
				return this.m_typed;
			}
			set
			{
				this.m_typed = value;
			}
		}

		public ExcelSolution solution
		{
			get
			{
				return this.m_solution;
			}
			set
			{
				this.m_solution = value;
			}
		}

		public string summary
		{
			get
			{
				return this.m_summary;
			}
			set
			{
				this.m_summary = value;
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

		public string sheet
		{
			get
			{
				return this.m_sheet;
			}
			set
			{
				this.m_sheet = value;
			}
		}

		public int count
		{
			get
			{
				return this.m_allkey.Count;
			}
		}

		public KeyPair this[int index]
		{
			get
			{
				return this.m_allkey[index];
			}
		}

		public string text
		{
			get
			{
				return this.m_sheet + ((this.m_summary == "") ? "" : ":") + this.m_summary;
			}
		}

		public ExcelXMLLayout(ExcelSolution solution)
		{
			this.m_typed = true;
			this.m_path = "invalid path";
			this.m_allkey = new List<KeyPair>(10);
			this.m_solution = solution;
			this.m_name = "item";
			this.m_sheet = "Sheet1";
		}

		public KeyPair Add(string name, KeyType type)
		{
			KeyPair keyPair = new KeyPair();
			keyPair.name = name;
			keyPair.type = type;
			keyPair.index = this.m_allkey.Count;
			this.m_allkey.Add(keyPair);
			return keyPair;
		}

		public IEnumerable<KeyPair> EachKeyPair()
		{
//             m_allkey.Sort((x, y) =>
//             {
//                 return x.name.CompareTo(y.name);
//             });

            return this.m_allkey;
		}

		public static string Type2Text(KeyType t)
		{
			switch (t)
			{
			case KeyType.Integer:
				return "整数";
			case KeyType.String:
				return "文本";
			default:
				return "未知类型";
			}
		}

		public static int Type2Int(KeyType t)
		{
			switch (t)
			{
			case KeyType.Integer:
				return 1;
			case KeyType.String:
				return 3;
			default:
				return 3;
			}
		}

		public void Clear()
		{
			this.m_allkey.Clear();
		}

		public void RemoveSelf()
		{
			this.solution.RemoveLayout(this);
		}

		public void Remove(KeyPair kp)
		{
			this.m_allkey.Remove(kp);
		}
	}
}
