using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicerChord.Behaviors
{
    public static class TreeViewItemBehavior
    {
        // ExpandedCommand 添付プロパティの定義
        public readonly static DependencyProperty ExpandedCommandProperty =
            DependencyProperty.RegisterAttached(
                "ExpandedCommand",
                typeof(ICommand),
                typeof(TreeViewItemBehavior),
                new PropertyMetadata(null, OnExpandedCommandChanged));

        public static void SetExpandedCommand(DependencyObject element, ICommand value)
            => element.SetValue(ExpandedCommandProperty, value);

        public static ICommand GetExpandedCommand(DependencyObject element)
            => (ICommand)element.GetValue(ExpandedCommandProperty);

        // プロパティが設定された時の処理（ここでイベントをフックする）
        private static void OnExpandedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeViewItem item)
            {
                // 重複登録を防ぐため、一度外してから登録
                item.Expanded -= OnItemExpanded;
                if (e.NewValue != null)
                {
                    item.Expanded += OnItemExpanded;
                }
            }
        }

        // イベントが発火した時にコマンドを実行
        private static void OnItemExpanded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem item && Equals(e.OriginalSource, item))
            {
                var command = GetExpandedCommand(item);

                // DataContext（TreeItem など）をパラメータとして渡す
                if (command != null && command.CanExecute(item.DataContext))
                {
                    command.Execute(item.DataContext);

                    // 必要に応じてバブリングを止める
                    // e.Handled = true;
                }
            }
        }
    }
}