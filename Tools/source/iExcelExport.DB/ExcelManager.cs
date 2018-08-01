using System;
using System.Collections.Generic;
using System.Xml;

namespace iExcelExport.DB
{
	public class ExcelManager
	{
		private List<ExcelSolution> m_solutions;

		public ExcelManager()
		{
			this.m_solutions = new List<ExcelSolution>(10);
		}
        public List<ExcelSolution> GetSortList()
        {
            m_solutions.Sort(
                (x, y) =>
                {
                    return x.name.CompareTo(y.name);
                }
                );
            return m_solutions;
        }

		public void AddSolution(ExcelSolution solu)
		{
			this.m_solutions.Add(solu);
		}

		public bool Save(string file = "config.xml")
		{
			XmlWriter xmlWriter = XmlWriter.Create(file);
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("root");
			foreach (ExcelSolution solution in this.m_solutions)
			{
				xmlWriter.WriteStartElement("excel");
				xmlWriter.WriteAttributeString("name", solution.name);
				xmlWriter.WriteAttributeString("path", solution.path);
				foreach (ExcelXMLLayout item in solution.EachLayout())
				{
					xmlWriter.WriteStartElement("layout");
					xmlWriter.WriteAttributeString("path", item.path);
					xmlWriter.WriteAttributeString("sheet", item.sheet);
					xmlWriter.WriteAttributeString("name", item.name);
					xmlWriter.WriteAttributeString("summary", item.summary);
					xmlWriter.WriteAttributeString("typed", item.typed.ToString());
					foreach (ExcelXMLLayout.KeyPair item2 in item.EachKeyPair())
					{
						xmlWriter.WriteStartElement("column");
						xmlWriter.WriteAttributeString("type", item2.type.ToString());
						xmlWriter.WriteValue(item2.name);
						xmlWriter.WriteEndElement();
					}
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
			xmlWriter.Close();
			return true;
		}

		public bool Load(string file = "config.xml")
		{
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(file);
				XmlNode xmlNode = xmlDocument.SelectSingleNode("root");
				this.m_solutions.Clear();
				foreach (XmlNode item in xmlNode.SelectNodes("excel"))
				{
					ExcelSolution excelSolution = new ExcelSolution(this);
					excelSolution.name = item.Attributes["name"].Value;
					excelSolution.path = item.Attributes["path"].Value;
					foreach (XmlNode item2 in item.SelectNodes("layout"))
					{
						ExcelXMLLayout excelXMLLayout = new ExcelXMLLayout(excelSolution);
						excelXMLLayout.path = item2.Attributes["path"].Value;
						excelXMLLayout.sheet = item2.Attributes["sheet"].Value;
						excelXMLLayout.name = item2.Attributes["name"].Value;
						excelXMLLayout.summary = item2.Attributes["summary"].Value;
						excelXMLLayout.typed = bool.Parse(item2.Attributes["typed"].Value);
						foreach (XmlNode item3 in item2.SelectNodes("column"))
						{
							excelXMLLayout.Add(item3.InnerText, (ExcelXMLLayout.KeyType)Enum.Parse(typeof(ExcelXMLLayout.KeyType), item3.Attributes["type"].Value));
						}
						excelSolution.AddLayout(excelXMLLayout);
					}
					this.AddSolution(excelSolution);
				}
				return true;
			}
			catch (Exception)
			{
			}
			return false;
		}

		public IEnumerable<ExcelSolution> EachSolution()
		{
			return this.GetSortList();
		}

		public void RemoveSolution(ExcelSolution solu)
		{
			this.m_solutions.Remove(solu);
		}
	}
}
