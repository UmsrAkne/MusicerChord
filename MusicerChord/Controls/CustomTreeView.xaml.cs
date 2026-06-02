using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicerChord.Controls
{
    public partial class CustomTreeView : UserControl
    {
        public readonly static DependencyProperty ExpandedCommandProperty =
            DependencyProperty.Register(
                nameof(ExpandedCommand),
                typeof(ICommand),
                typeof(CustomTreeView),
                new PropertyMetadata(null));

        public CustomTreeView()
        {
            InitializeComponent();
        }

        public ICommand ExpandedCommand
        {
            get => (ICommand)GetValue(ExpandedCommandProperty);
            set => SetValue(ExpandedCommandProperty, value);
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is not TreeViewItem treeViewItem)
            {
                return;
            }

            var commandParameter = treeViewItem.DataContext;

            if (ExpandedCommand?.CanExecute(commandParameter) == true)
            {
                ExpandedCommand.Execute(commandParameter);
            }
        }
    }
}