using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OneNoteMindMap.Core.Model;

namespace OneNoteMindMap.Editor.ViewModels
{
    public class MindMapEditorViewModel : INotifyPropertyChanged
    {
        private MindMapDocument _document;
        private string _statusText;
        private string _selectedLayout = "RightTree";
        private string _selectedTheme = "Default";

        public MindMapDocument Document
        {
            get => _document;
            set { _document = value; OnPropertyChanged(); }
        }

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public string SelectedLayout
        {
            get => _selectedLayout;
            set { _selectedLayout = value; OnPropertyChanged(); }
        }

        public string SelectedTheme
        {
            get => _selectedTheme;
            set { _selectedTheme = value; OnPropertyChanged(); }
        }

        public int NodeCount => CountNodes(Document?.Root);

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private static int CountNodes(MindMapNode node)
        {
            if (node == null) return 0;
            int count = 1;
            foreach (var child in node.Children)
                count += CountNodes(child);
            return count;
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(NodeCount));
        }
    }

    public class NodeViewModel : INotifyPropertyChanged
    {
        private MindMapNode _node;
        private bool _isSelected;
        private bool _isEditing;

        public MindMapNode Node
        {
            get => _node;
            set { _node = value; OnPropertyChanged(); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); }
        }

        public string Text
        {
            get => _node?.Text ?? "";
            set
            {
                if (_node != null)
                {
                    _node.Text = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
