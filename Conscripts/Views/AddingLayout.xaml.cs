using System;
using System.IO;
using System.Linq;
using Conscripts.Helpers;
using Conscripts.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Conscripts.Views
{
    public sealed partial class AddingLayout : UserControl
    {
        private MainViewModel _viewModel = null;

        private string _desireFileName = string.Empty;

        private StorageFile _chosenFile = null;

        private Action _closeAddingAction = null;

        public AddingLayout(MainViewModel viewModel, Action closeAddingAction)
        {
            this.InitializeComponent();

            _viewModel = viewModel;

            viewModel.LoadSegoeFluentIcons();

            this.Loaded += (_, _) => ResetLayout();

            _closeAddingAction += closeAddingAction;
        }

        /// <summary>
        /// ���ѡ���ļ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnClickChooseFile(object sender, RoutedEventArgs e)
        {
            try
            {
                var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
                WinRT.Interop.InitializeWithWindow.Initialize(openPicker, App.MainWindow.GetWindowHandle());
                openPicker.ViewMode = PickerViewMode.Thumbnail;
                openPicker.FileTypeFilter.Add(".ps1");
                openPicker.FileTypeFilter.Add(".bat");

                _chosenFile = await openPicker.PickSingleFileAsync();

                UpdateLayoutByChosenFile();
            }
            catch (Exception ex) { System.Diagnostics.Trace.WriteLine(ex); }
        }

        /// <summary>
        /// ���ȷ�ϴ����������ļ�������б�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnClickCreate(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_chosenFile is not null)
                {
                    var ext = Path.GetExtension(_chosenFile.Name);
                    if (ext == ".ps1" || ext == ".bat")
                    {
                        var dataFolder = await StorageFilesService.GetDataFolder();
                        var copiedFile = await _chosenFile.CopyAsync(dataFolder, $"{_desireFileName}{ext}", NameCollisionOption.ReplaceExisting);
                        if (copiedFile is not null)
                        {
                            string name = string.IsNullOrWhiteSpace(AddingShortcutNameTextBox.Text) ? _chosenFile.DisplayName : AddingShortcutNameTextBox.Text;
                            string category = string.IsNullOrWhiteSpace(AddingShortcutCategoryTextBox.Text) ? "δ����" : AddingShortcutCategoryTextBox.Text;
                            int colorIndex = AddingShortcutColorComboBox.SelectedIndex + 1;
                            bool runas = AddingShortcutRunasCheckBox.IsChecked == true;
                            bool noWindow = AddingShortcutNoWindowCheckBox.IsChecked == true;
                            int iconIndex = AddingShortcutIconGridView.SelectedIndex;

                            _viewModel.AddShortcut(name, category, colorIndex, runas, noWindow, iconIndex, ext, copiedFile.Path);

                            _closeAddingAction?.Invoke();
                        }
                    }
                }
            }
            catch (Exception ex) { System.Diagnostics.Trace.WriteLine(ex); }
        }

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickReset(object sender, RoutedEventArgs e)
        {
            ResetLayout();
        }

        /// <summary>
        /// ���ݵ�ǰѡ����ļ�������UI
        /// </summary>
        private void UpdateLayoutByChosenFile()
        {
            AddingShortcutNameTextBox.PlaceholderText = "Ĭ��ʹ�ýű��ļ���";

            if (string.IsNullOrWhiteSpace(_chosenFile?.Name))
            {
                NoFileSelectedStackPanel.Visibility = Visibility.Visible;
                FileSelectedStackPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                var fileExt = Path.GetExtension(_chosenFile.Name);
                if (fileExt == ".bat")
                {
                    NoFileSelectedStackPanel.Visibility = Visibility.Collapsed;
                    FileSelectedStackPanel.Visibility = Visibility.Visible;
                    Ps1FileIconImage.Visibility = Visibility.Collapsed;
                    BatFileIconImage.Visibility = Visibility.Visible;
                    CopyTipTextBlock.Text = $"����Ϊ {_desireFileName}.bat ���Ƶ� \"�ĵ�\\NoMewing\\Conscript\\\"";
                    AddingShortcutNameTextBox.PlaceholderText = _chosenFile.DisplayName;
                }
                else if (fileExt == ".ps1")
                {
                    NoFileSelectedStackPanel.Visibility = Visibility.Collapsed;
                    FileSelectedStackPanel.Visibility = Visibility.Visible;
                    Ps1FileIconImage.Visibility = Visibility.Visible;
                    BatFileIconImage.Visibility = Visibility.Collapsed;
                    CopyTipTextBlock.Text = $"����Ϊ {_desireFileName}.ps1 ���Ƶ� \"�ĵ�\\NoMewing\\Conscript\\\"";
                    AddingShortcutNameTextBox.PlaceholderText = _chosenFile.DisplayName;
                }
                else
                {
                    NoFileSelectedStackPanel.Visibility = Visibility.Visible;
                    FileSelectedStackPanel.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// ����UI
        /// </summary>
        public void ResetLayout()
        {
            try
            {
                _desireFileName = DateTime.Now.Ticks.ToString();
                _chosenFile = null;

                AddingShortcutNameTextBox.Text = "";
                AddingShortcutNameTextBox.PlaceholderText = "Ĭ��ʹ�ýű��ļ���";
                AddingShortcutCategoryTextBox.Text = "";
                AddingShortcutColorComboBox.SelectedIndex = 4;
                AddingShortcutIconGridView.SelectedIndex = 0;
                AddingShortcutRunasCheckBox.IsChecked = false;
                AddingShortcutNoWindowCheckBox.IsEnabled = true;
                AddingShortcutNoWindowCheckBox.IsChecked = false;
                UpdateLayoutByChosenFile();

                AddingShortcutIconGridView.ScrollIntoView(AddingShortcutIconGridView.Items.First());
                AddingShortcutScrollViewer.ChangeView(0, 0, null, true);
            }
            catch (Exception ex) { System.Diagnostics.Trace.WriteLine(ex); }
        }

        /// <summary>
        /// ��ѡ����ԱȨ��ʱ�����ɹ�ѡ�޴���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddingShortcutRunasCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            AddingShortcutNoWindowCheckBox.IsChecked = false;
            AddingShortcutNoWindowCheckBox.IsEnabled = false;
        }

        /// <summary>
        /// ȡ����ѡ����ԱȨ��ʱ������ѡ�޴���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddingShortcutRunasCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            AddingShortcutNoWindowCheckBox.IsEnabled = true;
        }

    }
}
