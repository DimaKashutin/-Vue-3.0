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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;  
using System.IO;        


namespace VxodK
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            // Создаем диалог выбора файла
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                // Фильтрация для PNG, JPG и JPEG файлов
                Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Multiselect = false  // Разрешаем выбрать только один файл
            };

            // Проверка, если файл выбран
            if (openFileDialog.ShowDialog() == true)
            {
                // Загружаем выбранное изображение в элемент Image
                string selectedFilePath = openFileDialog.FileName;
                BitmapImage bitmap = new BitmapImage(new Uri(selectedFilePath));
                SelectedImage.Source = bitmap; // Устанавливаем выбранное изображение в элемент Image
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Очистка всех текстовых полей
            NameTextBox.Clear();
            DatePicker.SelectedDate = null;
            PlaceTextBox.Clear();
            DescriptionTextBox.Clear();

            // Очистка изображения
            SelectedImage.Source = null;

            // Удаление всех элементов из ProgramStackPanelContainer
            ProgramStackPanelContainer.Children.Clear();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Сбор основной информации
            string name = NameTextBox.Text;
            string date = DatePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? "Не указана";
            string place = PlaceTextBox.Text;
            string description = DescriptionTextBox.Text;

            // Формирование основной части сообщения
            StringBuilder message = new StringBuilder();
            message.AppendLine($"Название: {name}");
            message.AppendLine($"Дата: {date}");
            message.AppendLine($"Место: {place}");
            message.AppendLine($"Описание: {description}");

            // Проверка на наличие изображения
            if (SelectedImage.Source != null)
            {
                message.AppendLine("Изображение выбрано.");
            }
            else
            {
                message.AppendLine("Изображение не выбрано.");
            }

            // Сбор информации из newProgramStackPanel
            if (ProgramStackPanelContainer.Children.Count > 0)
            {
                message.AppendLine("\nПрограмма:");
                int programCounter = 1;

                foreach (var child in ProgramStackPanelContainer.Children)
                {
                    if (child is Border border && border.Child is Grid grid)
                    {
                        // Извлечение TextBox для начала и окончания времени
                        TextBox startTextBox = grid.Children.OfType<TextBox>().ElementAtOrDefault(0); // Первый TextBox
                        TextBox endTextBox = grid.Children.OfType<TextBox>().ElementAtOrDefault(1); // Второй TextBox

                        // Получение значений
                        string startTime = startTextBox?.Text ?? "Не указано";
                        string endTime = endTextBox?.Text ?? "Не указано";

                        // Формирование строки для текущей программы
                        message.AppendLine($"Программа {programCounter}");
                        message.AppendLine($"Начало: {startTime}");
                        message.AppendLine($"Конец: {endTime}");

                        programCounter++;
                    }
                }
            }

            // Вывод сообщения с данными
            MessageBox.Show(message.ToString(), "Данные сохранены", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void AddProgramStage_Click(object sender, RoutedEventArgs e)
        {
            // Создаем новый Border для нового этапа программы
            var newProgramStackPanel = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.5),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(2),
                Margin = new Thickness(10),
                Child = new Grid
                {
                    Children =
            {
                new Label
                {
                    Content = "Начало",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(10, 10, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top
                },
                new Label
                {
                    Content = "Окончание",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(162, 10, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top
                },
                new Button
                {
                    Content = "Удалить",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(335, 78, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    Height = 20,
                    Width = 72
                },
                new TextBox
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(10, 41, 0, 0),
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 120,
                    MaxLength = 5 // Ограничиваем ввод до 5 символов (например, 00:00)
                },
                new TextBox
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(162, 41, 0, 0),
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 120,
                    MaxLength = 5 // Ограничиваем ввод до 5 символов (например, 00:00)
                }
            }
                }
            };

            // Находим кнопку внутри нового Border
            var deleteButton = (Button)((Grid)newProgramStackPanel.Child).Children[2];

            // Добавляем обработчик события для кнопки "Удалить"
            deleteButton.Click += (s, args) =>
            {
                // Удаляем текущий newProgramStackPanel из контейнера
                ProgramStackPanelContainer.Children.Remove(newProgramStackPanel);
            };

            // Получаем текстовые поля для времени
            var startTextBox = (TextBox)((Grid)newProgramStackPanel.Child).Children[3];
            var endTextBox = (TextBox)((Grid)newProgramStackPanel.Child).Children[4];

            // Обработчик события для проверки ввода в формате 00:00 и ограничение значений
            startTextBox.PreviewTextInput += (s, args) =>
            {
                // Проверка, чтобы вводимые символы были цифрами или ":"
                if (!char.IsDigit(args.Text, 0) && args.Text != ":")
                {
                    args.Handled = true; // Отменяем ввод, если это не цифры или ":"
                }
                else if (startTextBox.Text.Length == 2 && args.Text != ":")
                {
                    // Если в поле уже есть 2 символа (часы), то ":"
                    args.Handled = true; // Ожидаем символ ":"
                }
                else if (startTextBox.Text.Length == 5)
                {
                    // Если в поле уже 5 символов (00:00), не даем вводить больше
                    args.Handled = true;
                }
            };

            endTextBox.PreviewTextInput += (s, args) =>
            {
                // Проверка, чтобы вводимые символы были цифрами или ":"
                if (!char.IsDigit(args.Text, 0) && args.Text != ":")
                {
                    args.Handled = true; // Отменяем ввод, если это не цифры или ":"
                }
                else if (endTextBox.Text.Length == 2 && args.Text != ":")
                {
                    // Если в поле уже есть 2 символа (часы), то ":"
                    args.Handled = true; // Ожидаем символ ":"
                }
                else if (endTextBox.Text.Length == 5)
                {
                    // Если в поле уже 5 символов (00:00), не даем вводить больше
                    args.Handled = true;
                }
            };

            // Обработчик события для проверки значения часов и минут
            startTextBox.LostFocus += (s, args) =>
            {
                // Проверяем, что текст имеет формат 00:00
                if (startTextBox.Text.Length == 5 && startTextBox.Text[2] == ':')
                {
                    string[] parts = startTextBox.Text.Split(':');
                    int hours, minutes;
                    if (int.TryParse(parts[0], out hours) && int.TryParse(parts[1], out minutes))
                    {
                        // Проверка на максимальные значения часов и минут
                        if (hours < 0 || hours > 23 || minutes < 0 || minutes > 59)
                        {
                            MessageBox.Show("Время должно быть в пределах 00:00 - 23:59.");
                            startTextBox.Text = "00:00"; // Сброс значения
                        }
                    }
                }
            };

            endTextBox.LostFocus += (s, args) =>
            {
                // Проверяем, что текст имеет формат 00:00
                if (endTextBox.Text.Length == 5 && endTextBox.Text[2] == ':')
                {
                    string[] parts = endTextBox.Text.Split(':');
                    int hours, minutes;
                    if (int.TryParse(parts[0], out hours) && int.TryParse(parts[1], out minutes))
                    {
                        // Проверка на максимальные значения часов и минут
                        if (hours < 0 || hours > 23 || minutes < 0 || minutes > 59)
                        {
                            MessageBox.Show("Время должно быть в пределах 00:00 - 23:59.");
                            endTextBox.Text = "00:00"; // Сброс значения
                        }
                    }
                }
            };

            // Добавляем новый элемент в контейнер StackPanel
            ProgramStackPanelContainer.Children.Add(newProgramStackPanel);

            endTextBox.LostFocus += (s, args) =>
            {
                if (!ValidateTimeInput(startTextBox.Text, endTextBox.Text))
                {
                    MessageBox.Show("Время окончания не может совпадать или быть меньше времени начала.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                    endTextBox.Text = ""; // Сброс значения
                }
            };

            // Проверка времени начала следующей программы
            startTextBox.LostFocus += (s, args) =>
            {
                int currentIndex = ProgramStackPanelContainer.Children.IndexOf(newProgramStackPanel);
                if (currentIndex > 0)
                {
                    var previousPanel = ProgramStackPanelContainer.Children[currentIndex - 1] as Border;
                    var previousEndTextBox = ((Grid)previousPanel.Child).Children.OfType<TextBox>().ElementAt(1);
                    if (!ValidateTimeInput(previousEndTextBox.Text, startTextBox.Text))
                    {
                        MessageBox.Show("Начало текущего этапа не может быть раньше окончания предыдущего.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                        startTextBox.Text = ""; // Сброс значения
                    }
                }
            };
        }

        private bool ValidateTimeInput(string startTime, string endTime)
        {
            if (string.IsNullOrWhiteSpace(startTime) || string.IsNullOrWhiteSpace(endTime)) return true;

            // Парсинг времени
            if (TimeSpan.TryParse(startTime, out TimeSpan start) && TimeSpan.TryParse(endTime, out TimeSpan end))
            {
                return end > start;
            }

            return false;
        }
    }
}
