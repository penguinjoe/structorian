using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Structorian.Engine;

namespace Structorian
{
    public partial class DataView : UserControl
    {
        private string _dataFileName;
        private StructDef _rootStructDef;
        private InstanceTree _instanceTree;
        private InstanceTreeNode _activeInstance;
        private Dictionary<InstanceTreeNode, TreeNode> _nodeMap = new Dictionary<InstanceTreeNode, TreeNode>();
        private HexDump _hexDump;
        private bool _showLocalOffsets;

        public event CellSelectedEventHandler CellSelected;

        public DataView()
        {
            InitializeComponent();
            _hexDump = new HexDump();
            _hexDump.Font = new Font("Lucida Console", 9);
            _hexDump.BackColor = SystemColors.Window;
            _hexDump.Dock = DockStyle.Fill;
            splitContainer2.Panel2.Controls.Add(_hexDump);
        }

        public TreeView StructTreeView
        {
            get { return _structTreeView; }
        }

        public DataGridView StructGridView
        {
            get { return _structGridView; }
        }

        public bool ShowLocalOffsets
        {
            get { return _showLocalOffsets; }
            set
            {
                _showLocalOffsets = value;
                _structGridView.Invalidate();
            }
        }

        public void LoadData(string fileName, StructDef def)
        {
            _dataFileName = fileName;
            ReloadData(def, false);
        }

        internal void ReloadData(StructDef def, bool keepState)
        {
            if (_dataFileName == null)
                return;
            
            DataViewState viewState = null;
            if (keepState)
                viewState = DataViewState.Save(this);
            
            _rootStructDef = def;
            Stream stream = new BufferedStream(new FileStream(_dataFileName, FileMode.Open, FileAccess.Read, FileShare.Read), 16384);
            if (_instanceTree != null)
            {
                _instanceTree.InstanceAdded -= new InstanceAddedEventHandler(HandleInstanceAdded);
                _instanceTree.NodeNameChanged -= new NodeNameChangedEventHandler(HandleNodeNameChanged);
                _nodeMap.Clear();
            }
            _instanceTree = _rootStructDef.LoadData(stream);
            _instanceTree.InstanceAdded += new InstanceAddedEventHandler(HandleInstanceAdded);
            _instanceTree.NodeNameChanged += new NodeNameChangedEventHandler(HandleNodeNameChanged);
            FillStructureTree();
            _hexDump.Stream = stream;
            
            if (viewState != null)
                viewState.Restore(this);
        }

        private void HandleInstanceAdded(object sender, InstanceAddedEventArgs e)
        {
            if (e.Parent is InstanceTree)
                AddInstanceNode(null, e.Child);
            else
            {
                TreeNode parent;
                if (_nodeMap.TryGetValue(e.Parent, out parent))
                {
                    AddInstanceNode(parent, e.Child);
                }
            }
        }

        private void HandleNodeNameChanged(object sender, NodeNameChangedEventArgs e)
        {
            TreeNode node = _nodeMap[e.Node];
            node.Text = e.Node.NodeName;
        }

        private void FillStructureTree()
        {
            _structTreeView.Nodes.Clear();
            foreach (InstanceTreeNode instance in _instanceTree.Children)
            {
                AddInstanceNode(null, instance);
            }
        }

        private void AddInstanceNode(TreeNode parent, InstanceTreeNode instance)
        {
            TreeNode node;
            if (parent == null)
                node = _structTreeView.Nodes.Add(instance.NodeName);
            else
                node = parent.Nodes.Add(instance.NodeName);

            _nodeMap.Add(instance, node);
            node.Tag = instance;
            if (instance.HasChildren)
            {
                WindowsAPI.SetHasChildren(node, true);
            }
        }

        private void _structTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _activeInstance = (InstanceTreeNode)e.Node.Tag;
            _structGridView.DataSource = _activeInstance.Cells;
        }

        private void _structTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes.Count == 0)
            {
                InstanceTreeNode instance = (InstanceTreeNode)e.Node.Tag;
                instance.NeedChildren();
                if (instance.Children.Count == 0)
                    WindowsAPI.SetHasChildren(e.Node, false);
            }
        }

        private void _structGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (_structGridView.SelectedRows.Count > 0)
            {
                StructCell cell = (StructCell)_structGridView.SelectedRows[0].DataBoundItem;
                int offset = cell.Offset;
                if (offset >= 0)
                {
                    int dataSize = cell.GetDataSize((StructInstance) _activeInstance);
                    if (dataSize <= 0)
                        dataSize = 1;
                    _hexDump.SelectBytes(offset, dataSize);
                }
                if (CellSelected != null)
                    CellSelected(this, new CellSelectedEventArgs(cell));
            }
        }

        private void _structGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 0 && _showLocalOffsets && _activeInstance is StructInstance)
            {
                StructInstance instance = (StructInstance) _activeInstance;
                e.Value = ((int) e.Value - instance.Offset).ToString();
                e.FormattingApplied = true;
            }
        }
    }

    public class CellSelectedEventArgs: EventArgs
    {
        private StructCell _cell;

        public CellSelectedEventArgs(StructCell cell)
        {
            _cell = cell;
        }

        public StructCell Cell
        {
            get { return _cell; }
        }
    }
    
    public delegate void CellSelectedEventHandler(object sender, CellSelectedEventArgs e);
}
