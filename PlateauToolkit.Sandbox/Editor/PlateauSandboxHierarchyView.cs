using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    class PlateauSandboxHierarchyContext
    {
        public PlateauSandboxHierarchy Hierarchy { set; get; }
    }

    class PlateauSandboxHierarchy
    {
        public PlateauSandboxHierarchy(PlateauSandboxHierarchyHeader header)
        {
            Header = header;
        }

        public PlateauSandboxHierarchyHeader Header { get; }
        public PlateauSandboxHierarchyItem[] Items { get; set; }
    }

    class PlateauSandboxHierarchyHeader
    {
        readonly MultiColumnHeaderState m_HeaderState;

        public PlateauSandboxHierarchyHeader(MultiColumnHeaderState.Column[] columns)
        {
            Columns = columns;
        }

        public MultiColumnHeaderState.Column[] Columns { get; }
    }

    class PlateauSandboxHierarchyItem
    {
        public int Id { get; set; }
        public string GameObjectName { get; set; }
        public int KnotsCount { get; set; }
    }

    class PlateauSandboxHierarchyViewItem : TreeViewItem
    {
        public PlateauSandboxHierarchyItem Item { get; set; }
    }

    class PlateauSandboxHierarchyView : TreeView
    {
        readonly PlateauSandboxHierarchyContext m_Context;

        public PlateauSandboxHierarchyView(
            TreeViewState state,
            MultiColumnHeader multiColumnHeader,
            PlateauSandboxHierarchyContext context)
            : base(state, multiColumnHeader)
        {
            m_Context = context;
        }

        protected override TreeViewItem BuildRoot()
        {
            return new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = GetRows() ?? new List<TreeViewItem>();
            rows.Clear();

            if (m_Context.Hierarchy.Items != null)
            {
                foreach (var item in m_Context.Hierarchy.Items)
                {
                    var treeItem = new PlateauSandboxHierarchyViewItem
                    {
                        id = item.Id,
                        displayName = item.GameObjectName,
                        Item = item,
                    };
                    root.AddChild(treeItem);
                    rows.Add(treeItem);
                }

                SetupDepthsFromParentsAndChildren(root);
            }

            return rows;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var viewItem = (PlateauSandboxHierarchyViewItem)args.item;

            for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                var cellRect = args.GetCellRect(i);
                var columnIndex = args.GetColumn(i);

                CenterRectUsingSingleLineHeight(ref cellRect);

                if (columnIndex == 0)
                {
                    base.RowGUI(args);
                }
                else if (columnIndex == 1)
                {
                    GUI.Label(cellRect, viewItem.Item.KnotsCount.ToString());
                }
            }
        }
    }
}