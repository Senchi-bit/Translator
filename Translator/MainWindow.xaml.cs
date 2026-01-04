using System.Net.Http;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Translator
{
    public partial class MainWindow : Window
    {
        private readonly Dictionary<string, string> _languages = new Dictionary<string, string>
        {
            {"Автоопределение", "auto"},
            {"Английский", "en"},
            {"Русский", "ru"},
            {"Испанский", "es"},
            {"Французский", "fr"},
            {"Немецкий", "de"},
            {"Итальянский", "it"},
            {"Китайский", "zh"},
            {"Японский", "ja"},
            {"Корейский", "ko"},
            {"Арабский", "ar"},
            {"Португальский", "pt"}
        };
        
        private TranslationWindow _translationWindow;
        private HttpClient _httpClient;
        
        public MainWindow()
        {
            InitializeComponent();
            InitializeLanguages();
            InitializeHttpClient();
            InitializeEvents();
        }
        
        private void InitializeLanguages()
        {
            foreach (var language in _languages.Keys)
            {
                SourceLanguageComboBox.Items.Add(language);
                TargetLanguageComboBox.Items.Add(language);
            }
            
            SourceLanguageComboBox.SelectedItem = "Автоопределение";
            TargetLanguageComboBox.SelectedItem = "Английский";
        }
        
        private void InitializeHttpClient()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "TranslateApp/1.0");
        }
        
        private void InitializeEvents()
        {
            TranslateButton.Click += async (s, e) => await TranslateText();
            SwapLanguagesButton.Click += SwapLanguages;
        }
        
        private async Task TranslateText()
        {
            if (string.IsNullOrWhiteSpace(SourceTextBox.Text))
            {
                MessageBox.Show("Введите текст для перевода", "Внимание", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            try
            {
                string sourceLang = _languages[SourceLanguageComboBox.SelectedItem.ToString()];
                string targetLang = _languages[TargetLanguageComboBox.SelectedItem.ToString()];
                
                string translatedText = await TranslateWithGoogle(SourceTextBox.Text, sourceLang, targetLang);
                
                if (_translationWindow != null && _translationWindow.IsLoaded)
                {
                    _translationWindow.UpdateTranslation(translatedText);
                }
                else
                {
                    ShowTranslationWindow(this, null);
                    _translationWindow.UpdateTranslation(translatedText);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка перевода: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async Task<string> TranslateWithGoogle(string text, string sourceLang, string targetLang)
        {
            string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sourceLang}&tl={targetLang}&dt=t&q={Uri.EscapeDataString(text)}";
            
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            string responseBody = await response.Content.ReadAsStringAsync();
            
            var jsonArray = JsonConvert.DeserializeObject<JArray>(responseBody);
            var resultArray = jsonArray[0] as JArray;
            
            StringBuilder translatedText = new StringBuilder();
            foreach (var item in resultArray)
            {
                translatedText.Append(item[0].ToString());
            }
            
            return translatedText.ToString();
        }
        
        private void ShowTranslationWindow(object sender, RoutedEventArgs e)
        {
            if (_translationWindow == null || !_translationWindow.IsLoaded)
            {
                _translationWindow = new TranslationWindow();
                _translationWindow.Closed += (s, args) => _translationWindow = null;
                _translationWindow.Show();
            }
            else
            {
                _translationWindow.Activate();
            }
        }
        
        private void SwapLanguages(object sender, RoutedEventArgs e)
        {
            var sourceItem = SourceLanguageComboBox.SelectedItem;
            var targetItem = TargetLanguageComboBox.SelectedItem;
            
            SourceLanguageComboBox.SelectedItem = targetItem;
            TargetLanguageComboBox.SelectedItem = sourceItem;
        }
        
        protected override void OnClosed(EventArgs e)
        {
            _httpClient?.Dispose();
            base.OnClosed(e);
        }
    }
}