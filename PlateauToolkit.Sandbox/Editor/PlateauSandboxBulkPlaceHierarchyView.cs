using PlateauToolkit.Sandbox.Runtime;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Events;

namespace PlateauToolkit.Sandbox.Editor
{
    class PlateauSandboxBulkPlaceHierarchyContext
    {
        public PlateauSandboxBulkPlaceHierarchy Hierarchy { set; get; }
    }

    class PlateauSandboxBulkPlaceHierarchy
    {
        public PlateauSandboxBulkPlaceHierarchy(PlateauSandboxBulkPlaceHierarchyHeader header)
        {
            Header = header;
        }

        public PlateauSandboxBulkPlaceHierarchyHeader Header { get; }
        public PlateauSandboxBulkPlaceHierarchyItem[] Items { get; set; }
    }

    class PlateauSandboxBulkPlaceHierarchyHeader
    {
        readonly MultiColumnHeaderState m_HeaderState;

        public PlateauSandboxBulkPlaceHierarchyHeader(MultiColumnHeaderState.Column[] columns)
        {
            Columns = columns;
        }

        public MultiColumnHeaderState.Column[] Columns { get; }
    }

    class PlateauSandboxBulkPlaceHierarchyViewItem : TreeViewItem
    {
        public PlateauSandboxBulkPlaceHierarchyItem Item { get; set; }
    }

    class PlateauSandboxBulkPlaceHierarchyView : TreeView
    {
        readonly PlateauSandboxBulkPlaceHierarchyContext m_Context;

        public PlateauSandboxBulkPlaceHierarchyView(
            TreeViewState state,
            MultiColumnHeader multiColumnHeader,
            PlateauSandboxBulkPlaceHierarchyContext context)
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
            IList<TreeViewItem> rows = GetRows() ?? new List<TreeViewItem>();
            rows.Clear();

            if (m_Context.Hierarchy.Items != null)
            {
                foreach (PlateauSandboxBulkPlaceHierarchyItem item in m_Context.Hierarchy.Items)
                {
                    var treeItem = new PlateauSandboxBulkPlaceHierarchyViewItem
                    {
                        id = item.ID,
                        displayName = item.CategoryName,
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
            var viewItem = (PlateauSandboxBulkPlaceHierarchyViewItem)args.item;

            Event evt = Event.current;
            if (evt.type == EventType.MouseDown && evt.button == 0 && args.rowRect.Contains(evt.mousePosition))
            {
                OnItemClicked(viewItem.Item);
            }

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                Rect cellRect = args.GetCellRect(i);
                int columnIndex = args.GetColumn(i);

                CenterRectUsingSingleLineHeight(ref cellRect);

                if (columnIndex == 0)
                {
                    base.RowGUI(args);
                }
                else if (columnIndex == 1)
                {
                    var style = new GUIStyle
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal =
                        {
                            textColor = Color.white
                        }
                    };
                    GUI.Label(cellRect, viewItem.Item.Count.ToString(), style);
                }
                else if (columnIndex == 2)
                {
                    GUI.Label(cellRect, viewItem.Item.PrefabName);
                }
            }
        }

        private void OnItemClicked(PlateauSandboxBulkPlaceHierarchyItem item)
        {
            item.OnClicked?.Invoke();
        }
    }
}