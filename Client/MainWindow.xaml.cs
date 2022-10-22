using System;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Http.Json;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Linq;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        bool isComma { get; set; } = false;

        private string _inputValue = "0";
        public string InputValue
        {
            get { return _inputValue; }
            set
            {
                _inputValue = value;
                OnPropertyChanged();
            }
        }


        private string _result = "zero";
        public string Result
        {
            get { return _result; }
            set
            {
                _result = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var task = new Task(async () =>
                {
                    using (var client = new HttpClient())
                    {
                        //Post Data
                        client.BaseAddress = new Uri("https://localhost:7009/");
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = await client.PostAsJsonAsync("api/convertor/", InputValue);
                        response.EnsureSuccessStatusCode();

                        //Fetch Data
                        Result = await client.GetStringAsync("api/convertor");
                        Result = Result.ToString();
                    }
                });

                task.Start();
                await task;
                task.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            ConvertButton.IsEnabled = true;

            Regex regex = new Regex("[^0-9.-]+");
            var value = e.Text;
            e.Handled = regex.IsMatch(value);

            if (Regex.IsMatch(InputValue, @"\.\d\d"))
            {
                e.Handled = true;
                isComma = true;
            }
        }

        private void TextValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isComma && InputValue.Length > 12 || InputValue.Length > 9)
            {
                MessageBox.Show("Input to long");
                ConvertButton.IsEnabled = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
