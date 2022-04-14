using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SG.Checkouts_Overview
{
    /// <summary>
    /// Interaction logic for SaveConfirmDialogWindow.xaml
    /// </summary>
    public partial class SaveConfirmDialogWindow : Window
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.Cancel;

        public SaveConfirmDialogWindow()
        {
            InitializeComponent();
        }

        private void ButtonSaveNow_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }

        private void ButtonContinueWithoutSave_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }
    }
}
