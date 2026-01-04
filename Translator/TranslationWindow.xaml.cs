using System.Windows;

namespace Translator
{
    public partial class TranslationWindow : Window
    {
        public TranslationWindow()
        {
            InitializeComponent();
        }
        
        public void UpdateTranslation(string text)
        {
            TranslationTextBox.Text = text;
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
    }
}