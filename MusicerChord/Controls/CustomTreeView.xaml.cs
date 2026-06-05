using System.Windows;
using System.Windows.Controls;

namespace MusicerChord.Controls
{
    public partial class CustomTreeView : UserControl
    {
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(CustomTreeView),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedItemChangedByProperty));

        public CustomTreeView()
        {
            InitializeComponent();
            DirectoryTree.SelectedItemChanged += OnSelectedItemChanged;
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        private static void OnSelectedItemChangedByProperty(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CustomTreeView control && e.NewValue != control.DirectoryTree.SelectedItem)
            {
                if (e.NewValue == null)
                {
                    // 選択解除は直接的には難しいが、一応
                    return;
                }

                // TreeViewItem を探して選択状態にするロジックが必要だが、
                // ViewModel からの変更を反映させるには、TreeViewItem.IsSelected を各アイテムの ViewModel にバインドするのが一般的。
                // 今回は SelectedItem の「取得」を外部から可能にすることが主目的と思われる。
                // 逆方向（VM -> View）の同期が必要な場合は、TreeViewItem の Container を辿るか、
                // ItemContainerStyle で IsSelected を ViewModel にバインドする必要がある。
            }
        }

        private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!Equals(SelectedItem, e.NewValue))
            {
                SelectedItem = e.NewValue;
            }
        }
    }
}