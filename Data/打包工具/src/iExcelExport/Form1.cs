using iExcelExport.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace iExcelExport
{
	public class Form1 : Form
	{
		private ExcelManager mgr;

		private ExcelSolution m_currentSolution;

		private ExcelXMLLayout m_currentLayout;

		private IContainer components;

		private TreeView tvExcel;

		private ContextMenuStrip ctxExport;

		private ToolStripMenuItem 导出XMLToolStripMenuItem;

		private Button btnSave;

		private TextBox txtItemName;

		private TextBox txtSummary;

		private Label label1;

		private Label label2;

		private GroupBox groupBox1;

		private GroupBox groupBox2;

		private TextBox txtExportPath;

		private Label label3;

		private TextBox txtExcelPath;

		private Label label5;

		private TextBox txtExcelName;

		private Label label4;

		private ToolStripSeparator toolStripMenuItem1;

		private ToolStripMenuItem 猜测格式ToolStripMenuItem;

		private ContextMenuStrip ctxKeyPair;

		private ToolStripMenuItem 整数ToolStripMenuItem;

		private ToolStripMenuItem 字符串ToolStripMenuItem;

		private ContextMenuStrip ctxBatchExport;

		private ToolStripMenuItem 全部导出XToolStripMenuItem;

		private ToolStripSeparator toolStripMenuItem2;

		private ToolStripMenuItem 添加LayoutToolStripMenuItem;

		private TextBox txtSheetName;

		private Label label6;

		private ToolStripMenuItem 删除ToolStripMenuItem;

		private ToolStripSeparator toolStripMenuItem3;

		private ToolStripMenuItem 添加新数据ToolStripMenuItem;

		private ToolStripSeparator toolStripMenuItem4;

		private ToolStripMenuItem 删除此数据ToolStripMenuItem;

		private GroupBox groupBox3;

		private TextBox txtColumnName;

		private Label label7;

		private CheckBox chkTypedLayout;

		private ContextMenuStrip ctxAllExcel;

		private ToolStripMenuItem 添加ExcelToolStripMenuItem;

		private ToolStripSeparator toolStripMenuItem5;

		private ToolStripMenuItem 导出所有ExcelToolStripMenuItem;

		private ImageList ilExcelTree;

		private ComboBox cmbAllConfig;

		private Label label8;

		private ToolStripMenuItem 删除ExcelToolStripMenuItem;

		private SplitContainer splitContainer1;

		public ExcelSolution CurrentSolution
		{
			get
			{
				return this.m_currentSolution;
			}
			set
			{
				if (this.m_currentSolution != value)
				{
					this.m_currentSolution = value;
				}
			}
		}

		public ExcelXMLLayout CurrentLayout
		{
			get
			{
				return this.m_currentLayout;
			}
			set
			{
				if (this.m_currentLayout != value)
				{
					this.m_currentLayout = value;
				}
			}
		}

		public Form1()
		{
			this.InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			this.Util_BuildConfigList();
			if (this.cmbAllConfig.Items.Count > 0)
			{
				int num = this.cmbAllConfig.Items.IndexOf(Environment.UserName + ".xml");
				if (num != -1)
				{
					this.cmbAllConfig.SelectedIndex = num;
				}
				else
				{
					this.cmbAllConfig.SelectedIndex = 0;
					this.cmbAllConfig.Items.Add(Environment.UserName + ".xml");
				}
			}
		}

		private void cmbAllConfig_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.mgr = new ExcelManager();
			if (!this.mgr.Load("user\\" + this.cmbAllConfig.Text))
			{
				MessageBox.Show(this, "配置加载失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			else
			{
				this.Util_BuildExcelTV();
				if (this.tvExcel.Nodes.Count != 0)
				{
					this.tvExcel.Nodes[0].Expand();
				}
				this.Util_ShowLayout();
				this.Util_ShowSolution();
			}
		}

		private void Util_BuildConfigList()
		{
			string[] files = Directory.GetFiles("user", "*.xml");
			string[] array = files;
			foreach (string fileName in array)
			{
				this.cmbAllConfig.Items.Add(new FileInfo(fileName).Name);
			}
		}

		private void Util_BuildExcelTV()
		{
			this.tvExcel.Nodes.Clear();
			TreeNode treeNode = this.tvExcel.Nodes.Add("所有表格");
			TreeNode treeNode2 = treeNode;
			TreeNode treeNode3 = treeNode;
			string text3 = treeNode2.SelectedImageKey = (treeNode3.ImageKey = "ROOT");
			foreach (ExcelSolution item in this.mgr.EachSolution())
			{
				this.Util_AddExcelSolution(treeNode, item);
			}
		}

		private void Util_AddExcelSolution(TreeNode root, ExcelSolution solu)
		{
			TreeNode treeNode = new TreeNode(solu.text);
			treeNode.Name = solu.name;
			treeNode.Tag = solu;
			TreeNode treeNode2 = treeNode;
			TreeNode treeNode3 = treeNode;
			string text3 = treeNode2.SelectedImageKey = (treeNode3.ImageKey = "EXCEL");
			foreach (ExcelXMLLayout item in solu.EachLayout())
			{
				this.Util_AddLayout(treeNode, item);
			}
			root.Nodes.Add(treeNode);
		}

		private void Util_AddLayout(TreeNode soluNode, ExcelXMLLayout layout)
		{
			TreeNode treeNode = new TreeNode(layout.text);
			treeNode.Name = layout.path;
			treeNode.Tag = layout;
			TreeNode treeNode2 = treeNode;
			TreeNode treeNode3 = treeNode;
			string text3 = treeNode2.SelectedImageKey = (treeNode3.ImageKey = "LAYOUT");
			foreach (ExcelXMLLayout.KeyPair item in layout.EachKeyPair())
			{
				this.Util_AddKeyPair(treeNode, item);
			}
			soluNode.Nodes.Add(treeNode);
		}

		private void Util_AddKeyPair(TreeNode layoutNode, ExcelXMLLayout.KeyPair kp)
		{
			TreeNode treeNode = new TreeNode(kp.text);
			treeNode.Name = kp.name;
			treeNode.Tag = kp;
			TreeNode treeNode2 = treeNode;
			TreeNode treeNode3 = treeNode;
			string text3 = treeNode2.SelectedImageKey = (treeNode3.ImageKey = "KEYPAIR");
			layoutNode.Nodes.Add(treeNode);
		}

		private void tvExcel_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				this.tvExcel.SelectedNode = e.Node;
				if (e.Node.Level == 0)
				{
					this.ctxAllExcel.Show(Control.MousePosition);
				}
				else if (this.tvExcel.SelectedNode.Tag is ExcelXMLLayout)
				{
					this.ctxExport.Show(Control.MousePosition);
				}
				else if (this.tvExcel.SelectedNode.Tag is ExcelXMLLayout.KeyPair)
				{
					this.ctxKeyPair.Show(Control.MousePosition);
				}
				else if (this.tvExcel.SelectedNode.Tag is ExcelSolution)
				{
					this.ctxBatchExport.Show(Control.MousePosition);
				}
			}
		}

		private void Util_ShowSolution()
		{
			if (this.CurrentSolution == null)
			{
				this.txtExcelName.Enabled = false;
				this.txtExcelPath.Enabled = false;
			}
			else
			{
				this.txtExcelName.Text = this.CurrentSolution.name;
				this.txtExcelPath.Text = this.CurrentSolution.path;
				this.txtExcelName.Enabled = true;
				this.txtExcelPath.Enabled = true;
			}
		}

		private void Util_ShowLayout()
		{
			if (this.CurrentLayout == null)
			{
				this.txtSummary.Enabled = false;
				this.txtItemName.Enabled = false;
				this.txtExportPath.Enabled = false;
				this.txtSheetName.Enabled = false;
				this.chkTypedLayout.Enabled = false;
			}
			else
			{
				this.txtSummary.Text = this.CurrentLayout.summary;
				this.txtItemName.Text = this.CurrentLayout.name;
				this.txtExportPath.Text = this.CurrentLayout.path;
				this.txtSheetName.Text = this.CurrentLayout.sheet;
				this.chkTypedLayout.Checked = !this.CurrentLayout.typed;
				this.txtSummary.Enabled = true;
				this.txtItemName.Enabled = true;
				this.txtExportPath.Enabled = true;
				this.txtSheetName.Enabled = true;
				this.chkTypedLayout.Enabled = true;
			}
		}

		private bool Util_DoExport(ExcelXMLLayout layout)
		{
			return ExcelFile.DoExport(layout);
		}

		private void 导出XMLToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.tvExcel.SelectedNode.Tag is ExcelXMLLayout)
			{
				if (this.Util_DoExport(this.tvExcel.SelectedNode.Tag as ExcelXMLLayout))
				{
					MessageBox.Show(this, "成功", "信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
				else
				{
					MessageBox.Show(this, ExcelFile.lastError, "错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}

		private void 猜测格式ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.tvExcel.SelectedNode.Tag is ExcelXMLLayout)
			{
				ExcelXMLLayout excelXMLLayout = this.tvExcel.SelectedNode.Tag as ExcelXMLLayout;
				ExcelFile.AnalyzeExcel(excelXMLLayout);
				this.tvExcel.SelectedNode.Nodes.Clear();
				foreach (ExcelXMLLayout.KeyPair item in excelXMLLayout.EachKeyPair())
				{
					this.Util_AddKeyPair(this.tvExcel.SelectedNode, item);
				}
			}
		}

		private void 整数ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExcelXMLLayout.KeyPair keyPair = this.tvExcel.SelectedNode.Tag as ExcelXMLLayout.KeyPair;
			keyPair.type = ExcelXMLLayout.KeyType.Integer;
			this.tvExcel.SelectedNode.Text = keyPair.text;
		}

		private void 字符串ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExcelXMLLayout.KeyPair keyPair = this.tvExcel.SelectedNode.Tag as ExcelXMLLayout.KeyPair;
			keyPair.type = ExcelXMLLayout.KeyType.String;
			this.tvExcel.SelectedNode.Text = keyPair.text;
		}

		private void 全部导出XToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.tvExcel.SelectedNode.Tag is ExcelSolution)
			{
				List<string> list = new List<string>(5);
				ExcelSolution excelSolution = this.tvExcel.SelectedNode.Tag as ExcelSolution;
				foreach (ExcelXMLLayout item in excelSolution.EachLayout())
				{
					if (!this.Util_DoExport(item))
					{
						list.Add(item.text);
					}
				}
				if (list.Count == 0)
				{
					MessageBox.Show(this, "全部导出成功", "信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
				else
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (string item2 in list)
					{
						stringBuilder.AppendLine(item2);
					}
					MessageBox.Show(this, "以下项目导出失败：\r\n" + stringBuilder.ToString(), "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
		}

		private void 导出所有ExcelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			List<string> list = new List<string>(15);
			foreach (ExcelSolution item in this.mgr.EachSolution())
			{
				foreach (ExcelXMLLayout item2 in item.EachLayout())
				{
					if (!this.Util_DoExport(item2))
					{
						list.Add(item2.text);
					}
				}
			}
			if (list.Count == 0)
			{
				MessageBox.Show(this, "全部导出成功", "信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (string item3 in list)
				{
					stringBuilder.AppendLine(item3);
				}
				MessageBox.Show(this, "以下项目导出失败：\r\n" + stringBuilder.ToString(), "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		private void 添加LayoutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExcelSolution excelSolution = this.tvExcel.SelectedNode.Tag as ExcelSolution;
			ExcelXMLLayout layout = new ExcelXMLLayout(excelSolution);
			excelSolution.AddLayout(layout);
			this.Util_AddLayout(this.tvExcel.SelectedNode, layout);
		}

		private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.CurrentLayout.RemoveSelf();
			this.tvExcel.SelectedNode.Remove();
		}

		private void 添加新数据ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExcelXMLLayout.KeyPair kp = this.CurrentLayout.Add("NewColumn", ExcelXMLLayout.KeyType.Unknown);
			this.Util_AddKeyPair(this.tvExcel.SelectedNode, kp);
		}

		private void 删除此数据ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExcelXMLLayout.KeyPair kp = this.tvExcel.SelectedNode.Tag as ExcelXMLLayout.KeyPair;
			ExcelXMLLayout excelXMLLayout = this.tvExcel.SelectedNode.Parent.Tag as ExcelXMLLayout;
			excelXMLLayout.Remove(kp);
			this.tvExcel.SelectedNode.Remove();
		}

		private void 添加ExcelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExcelSolution solu = new ExcelSolution(this.mgr);
			this.mgr.AddSolution(solu);
			this.Util_AddExcelSolution(this.tvExcel.SelectedNode, solu);
		}

		private void 删除ExcelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.mgr.RemoveSolution(this.CurrentSolution);
			this.tvExcel.SelectedNode.Remove();
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			if (this.mgr.Save("user\\" + this.cmbAllConfig.Text))
			{
				MessageBox.Show(this, "保存成功", "信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
			else
			{
				MessageBox.Show(this, "无法保存", "错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		private void tvExcel_AfterSelect(object sender, TreeViewEventArgs e)
		{
			this.txtColumnName.Enabled = false;
			if (e.Node.Tag is ExcelXMLLayout)
			{
				this.CurrentLayout = (e.Node.Tag as ExcelXMLLayout);
				this.CurrentSolution = this.CurrentLayout.solution;
			}
			else if (e.Node.Tag is ExcelSolution)
			{
				this.CurrentLayout = null;
				this.CurrentSolution = (e.Node.Tag as ExcelSolution);
			}
			else
			{
				this.CurrentLayout = null;
				this.CurrentSolution = null;
				if (e.Node.Tag is ExcelXMLLayout.KeyPair)
				{
					this.txtColumnName.Text = (e.Node.Tag as ExcelXMLLayout.KeyPair).name;
					this.txtColumnName.Enabled = true;
				}
			}
			this.Util_ShowLayout();
			this.Util_ShowSolution();
		}

		private void txtExcelName_TextChanged(object sender, EventArgs e)
		{
			if (this.txtExcelName.Enabled)
			{
				this.CurrentSolution.name = this.txtExcelName.Text;
				if (this.tvExcel.SelectedNode.Tag is ExcelSolution)
				{
					this.tvExcel.SelectedNode.Text = this.CurrentSolution.text;
				}
				else if (this.tvExcel.SelectedNode.Tag is ExcelXMLLayout)
				{
					this.tvExcel.SelectedNode.Parent.Text = this.CurrentSolution.text;
				}
			}
		}

		private void txtExcelPath_TextChanged(object sender, EventArgs e)
		{
			if (this.txtExcelPath.Enabled)
			{
				this.CurrentSolution.path = this.txtExcelPath.Text;
				if (this.tvExcel.SelectedNode.Tag is ExcelSolution)
				{
					this.tvExcel.SelectedNode.Text = this.CurrentSolution.text;
				}
				else if (this.tvExcel.SelectedNode.Tag is ExcelXMLLayout)
				{
					this.tvExcel.SelectedNode.Parent.Text = this.CurrentSolution.text;
				}
			}
		}

		private void txtItemName_TextChanged(object sender, EventArgs e)
		{
			if (this.txtItemName.Enabled)
			{
				this.CurrentLayout.name = this.txtItemName.Text;
				this.tvExcel.SelectedNode.Text = this.CurrentLayout.text;
			}
		}

		private void txtSummary_TextChanged(object sender, EventArgs e)
		{
			if (this.txtSummary.Enabled)
			{
				this.CurrentLayout.summary = this.txtSummary.Text;
				this.tvExcel.SelectedNode.Text = this.CurrentLayout.text;
			}
		}

		private void txtExportPath_TextChanged(object sender, EventArgs e)
		{
			if (this.txtExportPath.Enabled)
			{
				this.CurrentLayout.path = this.txtExportPath.Text;
				this.tvExcel.SelectedNode.Text = this.CurrentLayout.text;
			}
		}

		private void txtSheetName_TextChanged(object sender, EventArgs e)
		{
			if (this.txtSheetName.Enabled)
			{
				this.CurrentLayout.sheet = this.txtSheetName.Text;
				this.tvExcel.SelectedNode.Text = this.CurrentLayout.text;
			}
		}

		private void txtColumnName_TextChanged(object sender, EventArgs e)
		{
			if (this.txtColumnName.Enabled)
			{
				ExcelXMLLayout.KeyPair keyPair = this.tvExcel.SelectedNode.Tag as ExcelXMLLayout.KeyPair;
				keyPair.name = this.txtColumnName.Text;
				this.tvExcel.SelectedNode.Text = keyPair.text;
			}
		}

		private void chkTypedLayout_CheckedChanged(object sender, EventArgs e)
		{
			if (this.chkTypedLayout.Enabled)
			{
				this.CurrentLayout.typed = !this.chkTypedLayout.Checked;
			}
		}

		private void Form1_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effect = DragDropEffects.Link;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}

		private void Form1_DragDrop(object sender, DragEventArgs e)
		{
			string text = (e.Data.GetData(DataFormats.FileDrop) as Array).GetValue(0).ToString();
			string text2 = text.ToLower();
			if (text2.Contains(".xlsx") || text2.Contains(".xls"))
			{
				this.txtExcelPath.Text = text;
			}
			else if (text2.Contains(".xml"))
			{
				this.txtExportPath.Text = text;
			}
		}

		private void cmbAllConfig_DrawItem(object sender, DrawItemEventArgs e)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.components = new Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(Form1));
			this.tvExcel = new TreeView();
			this.ilExcelTree = new ImageList(this.components);
			this.ctxExport = new ContextMenuStrip(this.components);
			this.导出XMLToolStripMenuItem = new ToolStripMenuItem();
			this.toolStripMenuItem1 = new ToolStripSeparator();
			this.猜测格式ToolStripMenuItem = new ToolStripMenuItem();
			this.删除ToolStripMenuItem = new ToolStripMenuItem();
			this.toolStripMenuItem3 = new ToolStripSeparator();
			this.添加新数据ToolStripMenuItem = new ToolStripMenuItem();
			this.btnSave = new Button();
			this.txtItemName = new TextBox();
			this.txtSummary = new TextBox();
			this.label1 = new Label();
			this.label2 = new Label();
			this.groupBox1 = new GroupBox();
			this.txtExcelPath = new TextBox();
			this.label5 = new Label();
			this.txtExcelName = new TextBox();
			this.label4 = new Label();
			this.groupBox2 = new GroupBox();
			this.txtExportPath = new TextBox();
			this.txtSheetName = new TextBox();
			this.label3 = new Label();
			this.chkTypedLayout = new CheckBox();
			this.label6 = new Label();
			this.ctxKeyPair = new ContextMenuStrip(this.components);
			this.整数ToolStripMenuItem = new ToolStripMenuItem();
			this.字符串ToolStripMenuItem = new ToolStripMenuItem();
			this.toolStripMenuItem4 = new ToolStripSeparator();
			this.删除此数据ToolStripMenuItem = new ToolStripMenuItem();
			this.ctxBatchExport = new ContextMenuStrip(this.components);
			this.全部导出XToolStripMenuItem = new ToolStripMenuItem();
			this.toolStripMenuItem2 = new ToolStripSeparator();
			this.添加LayoutToolStripMenuItem = new ToolStripMenuItem();
			this.删除ExcelToolStripMenuItem = new ToolStripMenuItem();
			this.groupBox3 = new GroupBox();
			this.txtColumnName = new TextBox();
			this.label7 = new Label();
			this.ctxAllExcel = new ContextMenuStrip(this.components);
			this.导出所有ExcelToolStripMenuItem = new ToolStripMenuItem();
			this.toolStripMenuItem5 = new ToolStripSeparator();
			this.添加ExcelToolStripMenuItem = new ToolStripMenuItem();
			this.cmbAllConfig = new ComboBox();
			this.label8 = new Label();
			this.splitContainer1 = new SplitContainer();
			this.ctxExport.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.ctxKeyPair.SuspendLayout();
			this.ctxBatchExport.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.ctxAllExcel.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			base.SuspendLayout();
			this.tvExcel.Dock = DockStyle.Fill;
			this.tvExcel.HideSelection = false;
			this.tvExcel.ImageIndex = 0;
			this.tvExcel.ImageList = this.ilExcelTree;
			this.tvExcel.Location = new Point(0, 0);
			this.tvExcel.Name = "tvExcel";
			this.tvExcel.SelectedImageIndex = 0;
			this.tvExcel.Size = new Size(255, 346);
			this.tvExcel.TabIndex = 0;
			this.tvExcel.AfterSelect += this.tvExcel_AfterSelect;
			this.tvExcel.NodeMouseClick += this.tvExcel_NodeMouseClick;
			this.ilExcelTree.ImageStream = (ImageListStreamer)componentResourceManager.GetObject("ilExcelTree.ImageStream");
			this.ilExcelTree.TransparentColor = Color.Transparent;
			this.ilExcelTree.Images.SetKeyName(0, "ROOT");
			this.ilExcelTree.Images.SetKeyName(1, "LAYOUT");
			this.ilExcelTree.Images.SetKeyName(2, "KEYPAIR");
			this.ilExcelTree.Images.SetKeyName(3, "EXCEL");
			this.ctxExport.Items.AddRange(new ToolStripItem[6]
			{
				this.导出XMLToolStripMenuItem,
				this.toolStripMenuItem1,
				this.猜测格式ToolStripMenuItem,
				this.删除ToolStripMenuItem,
				this.toolStripMenuItem3,
				this.添加新数据ToolStripMenuItem
			});
			this.ctxExport.Name = "ctxExport";
			this.ctxExport.Size = new Size(149, 104);
			this.导出XMLToolStripMenuItem.Name = "导出XMLToolStripMenuItem";
			this.导出XMLToolStripMenuItem.Size = new Size(148, 22);
			this.导出XMLToolStripMenuItem.Text = "导出XML(&X)";
			this.导出XMLToolStripMenuItem.Click += this.导出XMLToolStripMenuItem_Click;
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new Size(145, 6);
			this.猜测格式ToolStripMenuItem.Name = "猜测格式ToolStripMenuItem";
			this.猜测格式ToolStripMenuItem.Size = new Size(148, 22);
			this.猜测格式ToolStripMenuItem.Text = "猜测格式(&G)";
			this.猜测格式ToolStripMenuItem.Click += this.猜测格式ToolStripMenuItem_Click;
			this.删除ToolStripMenuItem.Name = "删除ToolStripMenuItem";
			this.删除ToolStripMenuItem.Size = new Size(148, 22);
			this.删除ToolStripMenuItem.Text = "删除(&D)";
			this.删除ToolStripMenuItem.Click += this.删除ToolStripMenuItem_Click;
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Size = new Size(145, 6);
			this.添加新数据ToolStripMenuItem.Name = "添加新数据ToolStripMenuItem";
			this.添加新数据ToolStripMenuItem.Size = new Size(148, 22);
			this.添加新数据ToolStripMenuItem.Text = "添加新数据(&A)";
			this.添加新数据ToolStripMenuItem.Click += this.添加新数据ToolStripMenuItem_Click;
			this.btnSave.Location = new Point(5, 315);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new Size(75, 23);
			this.btnSave.TabIndex = 2;
			this.btnSave.Text = "保存(&S)";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += this.btnSave_Click;
			this.txtItemName.Enabled = false;
			this.txtItemName.Location = new Point(65, 23);
			this.txtItemName.Name = "txtItemName";
			this.txtItemName.Size = new Size(100, 21);
			this.txtItemName.TabIndex = 3;
			this.txtItemName.TextChanged += this.txtItemName_TextChanged;
			this.txtSummary.Enabled = false;
			this.txtSummary.Location = new Point(65, 50);
			this.txtSummary.Name = "txtSummary";
			this.txtSummary.Size = new Size(301, 21);
			this.txtSummary.TabIndex = 4;
			this.txtSummary.TextChanged += this.txtSummary_TextChanged;
			this.label1.AutoSize = true;
			this.label1.Location = new Point(18, 53);
			this.label1.Name = "label1";
			this.label1.Size = new Size(29, 12);
			this.label1.TabIndex = 5;
			this.label1.Text = "备注";
			this.label2.AutoSize = true;
			this.label2.Location = new Point(18, 26);
			this.label2.Name = "label2";
			this.label2.Size = new Size(41, 12);
			this.label2.TabIndex = 5;
			this.label2.Text = "元素名";
			this.groupBox1.Controls.Add(this.txtExcelPath);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.txtExcelName);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Location = new Point(3, 3);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new Size(372, 103);
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Excel";
			this.txtExcelPath.Enabled = false;
			this.txtExcelPath.Location = new Point(20, 65);
			this.txtExcelPath.Name = "txtExcelPath";
			this.txtExcelPath.Size = new Size(348, 21);
			this.txtExcelPath.TabIndex = 3;
			this.txtExcelPath.TextChanged += this.txtExcelPath_TextChanged;
			this.label5.AutoSize = true;
			this.label5.Location = new Point(18, 50);
			this.label5.Name = "label5";
			this.label5.Size = new Size(59, 12);
			this.label5.TabIndex = 5;
			this.label5.Text = "Excel文件";
			this.txtExcelName.Enabled = false;
			this.txtExcelName.Location = new Point(65, 20);
			this.txtExcelName.Name = "txtExcelName";
			this.txtExcelName.Size = new Size(301, 21);
			this.txtExcelName.TabIndex = 3;
			this.txtExcelName.TextChanged += this.txtExcelName_TextChanged;
			this.label4.AutoSize = true;
			this.label4.Location = new Point(18, 23);
			this.label4.Name = "label4";
			this.label4.Size = new Size(29, 12);
			this.label4.TabIndex = 5;
			this.label4.Text = "备注";
			this.groupBox2.Controls.Add(this.txtExportPath);
			this.groupBox2.Controls.Add(this.txtSheetName);
			this.groupBox2.Controls.Add(this.txtItemName);
			this.groupBox2.Controls.Add(this.txtSummary);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.chkTypedLayout);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Location = new Point(3, 112);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new Size(372, 135);
			this.groupBox2.TabIndex = 6;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "XML";
			this.txtExportPath.Enabled = false;
			this.txtExportPath.Location = new Point(20, 95);
			this.txtExportPath.Name = "txtExportPath";
			this.txtExportPath.Size = new Size(346, 21);
			this.txtExportPath.TabIndex = 3;
			this.txtExportPath.TextChanged += this.txtExportPath_TextChanged;
			this.txtSheetName.Enabled = false;
			this.txtSheetName.Location = new Point(266, 23);
			this.txtSheetName.Name = "txtSheetName";
			this.txtSheetName.Size = new Size(100, 21);
			this.txtSheetName.TabIndex = 3;
			this.txtSheetName.TextChanged += this.txtSheetName_TextChanged;
			this.label3.AutoSize = true;
			this.label3.Location = new Point(18, 80);
			this.label3.Name = "label3";
			this.label3.Size = new Size(71, 12);
			this.label3.TabIndex = 5;
			this.label3.Text = "导出XML文件";
			this.chkTypedLayout.AutoSize = true;
			this.chkTypedLayout.Enabled = false;
			this.chkTypedLayout.Location = new Point(288, 79);
			this.chkTypedLayout.Name = "chkTypedLayout";
			this.chkTypedLayout.Size = new Size(78, 16);
			this.chkTypedLayout.TabIndex = 8;
			this.chkTypedLayout.Text = "客户端XML";
			this.chkTypedLayout.UseVisualStyleBackColor = true;
			this.chkTypedLayout.CheckedChanged += this.chkTypedLayout_CheckedChanged;
			this.label6.AutoSize = true;
			this.label6.Location = new Point(213, 26);
			this.label6.Name = "label6";
			this.label6.Size = new Size(47, 12);
			this.label6.TabIndex = 5;
			this.label6.Text = "Sheet名";
			this.ctxKeyPair.Items.AddRange(new ToolStripItem[4]
			{
				this.整数ToolStripMenuItem,
				this.字符串ToolStripMenuItem,
				this.toolStripMenuItem4,
				this.删除此数据ToolStripMenuItem
			});
			this.ctxKeyPair.Name = "ctxKeyPair";
			this.ctxKeyPair.Size = new Size(149, 76);
			this.整数ToolStripMenuItem.Name = "整数ToolStripMenuItem";
			this.整数ToolStripMenuItem.Size = new Size(148, 22);
			this.整数ToolStripMenuItem.Text = "整数(&1)";
			this.整数ToolStripMenuItem.Click += this.整数ToolStripMenuItem_Click;
			this.字符串ToolStripMenuItem.Name = "字符串ToolStripMenuItem";
			this.字符串ToolStripMenuItem.Size = new Size(148, 22);
			this.字符串ToolStripMenuItem.Text = "文本(&3)";
			this.字符串ToolStripMenuItem.Click += this.字符串ToolStripMenuItem_Click;
			this.toolStripMenuItem4.Name = "toolStripMenuItem4";
			this.toolStripMenuItem4.Size = new Size(145, 6);
			this.删除此数据ToolStripMenuItem.Name = "删除此数据ToolStripMenuItem";
			this.删除此数据ToolStripMenuItem.Size = new Size(148, 22);
			this.删除此数据ToolStripMenuItem.Text = "删除此数据(&D)";
			this.删除此数据ToolStripMenuItem.Click += this.删除此数据ToolStripMenuItem_Click;
			this.ctxBatchExport.Items.AddRange(new ToolStripItem[4]
			{
				this.全部导出XToolStripMenuItem,
				this.toolStripMenuItem2,
				this.添加LayoutToolStripMenuItem,
				this.删除ExcelToolStripMenuItem
			});
			this.ctxBatchExport.Name = "ctxBatchExport";
			this.ctxBatchExport.Size = new Size(153, 98);
			this.全部导出XToolStripMenuItem.Name = "全部导出XToolStripMenuItem";
			this.全部导出XToolStripMenuItem.Size = new Size(152, 22);
			this.全部导出XToolStripMenuItem.Text = "全部导出(&X)";
			this.全部导出XToolStripMenuItem.Click += this.全部导出XToolStripMenuItem_Click;
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new Size(149, 6);
			this.添加LayoutToolStripMenuItem.Name = "添加LayoutToolStripMenuItem";
			this.添加LayoutToolStripMenuItem.Size = new Size(152, 22);
			this.添加LayoutToolStripMenuItem.Text = "添加Layout(&A)";
			this.添加LayoutToolStripMenuItem.Click += this.添加LayoutToolStripMenuItem_Click;
			this.删除ExcelToolStripMenuItem.Name = "删除ExcelToolStripMenuItem";
			this.删除ExcelToolStripMenuItem.Size = new Size(152, 22);
			this.删除ExcelToolStripMenuItem.Text = "删除Excel(&D)";
			this.删除ExcelToolStripMenuItem.Click += this.删除ExcelToolStripMenuItem_Click;
			this.groupBox3.Controls.Add(this.txtColumnName);
			this.groupBox3.Controls.Add(this.label7);
			this.groupBox3.Location = new Point(4, 253);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new Size(371, 58);
			this.groupBox3.TabIndex = 7;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "数据";
			this.txtColumnName.Enabled = false;
			this.txtColumnName.Location = new Point(64, 24);
			this.txtColumnName.Name = "txtColumnName";
			this.txtColumnName.Size = new Size(100, 21);
			this.txtColumnName.TabIndex = 6;
			this.txtColumnName.TextChanged += this.txtColumnName_TextChanged;
			this.label7.AutoSize = true;
			this.label7.Location = new Point(17, 27);
			this.label7.Name = "label7";
			this.label7.Size = new Size(29, 12);
			this.label7.TabIndex = 5;
			this.label7.Text = "名字";
			this.ctxAllExcel.Items.AddRange(new ToolStripItem[3]
			{
				this.导出所有ExcelToolStripMenuItem,
				this.toolStripMenuItem5,
				this.添加ExcelToolStripMenuItem
			});
			this.ctxAllExcel.Name = "ctxAllExcel";
			this.ctxAllExcel.Size = new Size(167, 54);
			this.导出所有ExcelToolStripMenuItem.Name = "导出所有ExcelToolStripMenuItem";
			this.导出所有ExcelToolStripMenuItem.Size = new Size(166, 22);
			this.导出所有ExcelToolStripMenuItem.Text = "导出所有Excel(&X)";
			this.导出所有ExcelToolStripMenuItem.Click += this.导出所有ExcelToolStripMenuItem_Click;
			this.toolStripMenuItem5.Name = "toolStripMenuItem5";
			this.toolStripMenuItem5.Size = new Size(163, 6);
			this.添加ExcelToolStripMenuItem.Name = "添加ExcelToolStripMenuItem";
			this.添加ExcelToolStripMenuItem.Size = new Size(166, 22);
			this.添加ExcelToolStripMenuItem.Text = "添加Excel(&A)";
			this.添加ExcelToolStripMenuItem.Click += this.添加ExcelToolStripMenuItem_Click;
			this.cmbAllConfig.FormattingEnabled = true;
			this.cmbAllConfig.Location = new Point(187, 317);
			this.cmbAllConfig.Name = "cmbAllConfig";
			this.cmbAllConfig.Size = new Size(182, 20);
			this.cmbAllConfig.TabIndex = 8;
			this.cmbAllConfig.DrawItem += this.cmbAllConfig_DrawItem;
			this.cmbAllConfig.SelectedIndexChanged += this.cmbAllConfig_SelectedIndexChanged;
			this.label8.AutoSize = true;
			this.label8.Location = new Point(128, 320);
			this.label8.Name = "label8";
			this.label8.Size = new Size(53, 12);
			this.label8.TabIndex = 5;
			this.label8.Text = "其他配置";
			this.splitContainer1.Dock = DockStyle.Fill;
			this.splitContainer1.Location = new Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Panel1.Controls.Add(this.tvExcel);
			this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
			this.splitContainer1.Panel2.Controls.Add(this.btnSave);
			this.splitContainer1.Panel2.Controls.Add(this.label8);
			this.splitContainer1.Panel2.Controls.Add(this.groupBox3);
			this.splitContainer1.Panel2.Controls.Add(this.groupBox2);
			this.splitContainer1.Panel2.Controls.Add(this.cmbAllConfig);
			this.splitContainer1.Size = new Size(646, 346);
			this.splitContainer1.SplitterDistance = 255;
			this.splitContainer1.TabIndex = 9;
			this.AllowDrop = true;
			base.AutoScaleDimensions = new SizeF(6f, 12f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(646, 346);
			base.Controls.Add(this.splitContainer1);
			base.Name = "Form1";
			this.Text = "Form1";
			base.Load += this.Form1_Load;
			base.DragDrop += this.Form1_DragDrop;
			base.DragEnter += this.Form1_DragEnter;
			this.ctxExport.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ctxKeyPair.ResumeLayout(false);
			this.ctxBatchExport.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.ctxAllExcel.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			this.splitContainer1.ResumeLayout(false);
			base.ResumeLayout(false);
		}
	}
}
