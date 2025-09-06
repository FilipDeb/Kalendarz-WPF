using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace YourNamespace
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Media;

    public class TaskItem : INotifyPropertyChanged
    {
        private bool isPriority;
        private string name;
        private string firstName;
        private string lastName;
        private string address;
        private string phoneNumber;
        private DateTime endDate;
        private string folderPath;
        private Brush backgroundColor;

        public bool IsNearDeadline { get; set; }

        public bool IsPriority
        {
            get => isPriority;
            set
            {
                isPriority = value;
                OnPropertyChanged();
                BackgroundColor = isPriority ? Brushes.Red : Brushes.Transparent;
                SavePriorityState();
            }
        }
        private void SavePriorityState()
        {
            string priorityFilePath = Path.Combine(FolderPath, "priority.txt");
            File.WriteAllText(priorityFilePath, isPriority.ToString());
        }

        public string Name
        {
            get => name;
            set { name = value; OnPropertyChanged(); }
        }

        public string FirstName
        {
            get => firstName;
            set { firstName = value; OnPropertyChanged(); }
        }

        public string LastName
        {
            get => lastName;
            set { lastName = value; OnPropertyChanged(); }
        }

        public string Address
        {
            get => address;
            set { address = value; OnPropertyChanged(); }
        }

        public string PhoneNumber
        {
            get => phoneNumber;
            set { phoneNumber = value; OnPropertyChanged(); }
        }

        public DateTime EndDate
        {
            get => endDate;
            set { endDate = value; OnPropertyChanged(); }
        }

        public string FolderPath
        {
            get => folderPath;
            set { folderPath = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> TaskList { get; set; } = new ObservableCollection<string>();

        public Brush BackgroundColor
        {
            get => backgroundColor;
            set { backgroundColor = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public partial class MainWindow : Window
    {
        private ObservableCollection<TaskItem> tasks = new ObservableCollection<TaskItem>();
        private bool isEditing = false; // Flaga określająca, czy edytujemy istniejące zadanie
        private TaskItem currentEditingTask; // Referencja do aktualnie edytowanego zadania

        public MainWindow()
        {
            InitializeComponent();
            LoadActiveTasks();
            LoadCompletedTasks();
            UpdatePlaceholderVisibility();
            LoadTasksFromFile();

            TaskListTextBox.TextChanged += TaskListTextBox_TextChanged;
            TaskListTextBox.GotFocus += TaskListTextBox_GotFocus;
            TaskListTextBox.LostFocus += TaskListTextBox_LostFocus;
        }
        private void TaskListTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            UpdatePlaceholderVisibility();
        }

        private void TaskListTextBox_GotFocus(object sender, RoutedEventArgs e)
        {

            PlaceholderTextBlock.Visibility = Visibility.Collapsed;
        }

        private void TaskListTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

            UpdatePlaceholderVisibility();
        }

        private void UpdatePlaceholderVisibility()
        {
            PlaceholderTextBlock.Visibility = string.IsNullOrWhiteSpace(TaskListTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private List<string> taskList = new List<string>();

        private void SendTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string input = TaskListTextBox.Text.Trim();


            if (!string.IsNullOrEmpty(input))
            {
                string[] tasks = input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);


                foreach (var task in tasks)
                {
                    string formattedTask = "- " + task.Trim();
                    taskList.Add(formattedTask);
                }


                UpdateTaskList();


                SaveTasksToFile();


                TaskListTextBox.Clear();
            }
            else
            {
                MessageBox.Show("Wprowadź co najmniej jedno zadanie.");
            }
        }
        private void SaveTasksToFile()
        {
            if (ActiveTasksListView.SelectedItem is TaskItem selectedTask)
            {
                try
                {
                    string filePath = Path.Combine(selectedTask.FolderPath, "tasks.txt");

                    using (StreamWriter writer = new StreamWriter(filePath, false)) // false = nadpisanie pliku
                    {
                        foreach (var task in taskList)
                        {
                            writer.WriteLine(task); // Zadanie już zawiera "-"
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Błąd podczas zapisywania pliku: " + ex.Message);
                }
            }
        }
        private void LoadTasksFromFile()
        {
            if (ActiveTasksListView.SelectedItem is TaskItem selectedTask)
            {
                try
                {
                    string filePath = Path.Combine(selectedTask.FolderPath, "tasks.txt"); // Ścieżka folderu z wybranego zadania

                    if (File.Exists(filePath))
                    {
                        var lines = File.ReadAllLines(filePath);

                        taskList.Clear();

                        foreach (var line in lines)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                taskList.Add(line.Trim());
                            }
                        }

                        UpdateTaskList();
                    }
                    else
                    {
                        taskList.Clear(); // Czyścimy listę, żeby była pusta
                        UpdateTaskList(); // Odświeżamy widok
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Błąd podczas wczytywania zadań: " + ex.Message);
                }
            }
            else
            {
                taskList.Clear(); // Czyścimy listę zadań
                UpdateTaskList(); // Odświeżamy widok
            }
        }
        private void UpdateTaskList()
        {
            TaskListBox.Items.Clear();

            foreach (var task in taskList)
            {
                TaskListBox.Items.Add(task);
            }
        }
        private void DeleteElementButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var task = button.DataContext as string;

                if (task != null)
                {
                    taskList.Remove(task);

                    UpdateTaskList();

                    SaveTasksToFile();
                }
            }
        }
        private void ActiveTasksListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActiveTasksListView.SelectedItem is TaskItem selectedTask)
            {
                InfoPanel.Visibility = Visibility.Visible;
                OptionalDataTextBlock.Text = $"    Imię: {selectedTask.FirstName}\n" +
                                              $"    Nazwisko: {selectedTask.LastName}\n" +
                                              $"    Adres: {selectedTask.Address}\n" +
                                              $"    Numer telefonu: {selectedTask.PhoneNumber}\n" +
                                              $"    Data zakończenia: {selectedTask.EndDate.ToShortDateString()}";

                PriorityCheckBox.IsChecked = selectedTask.IsPriority;

                UpdateBackgroundColor(selectedTask);

                LoadTasksFromFile();
            }
            else
            {
                InfoPanel.Visibility = Visibility.Collapsed;
            }
        }
        private void UpdateBackgroundColor(TaskItem task)
        {
            if (task.IsPriority)
            {
                task.BackgroundColor = Brushes.Red; // Kolor dla priorytetowych
            }
            else if (task.IsNearDeadline)
            {
                task.BackgroundColor = Brushes.Yellow; // Kolor dla bliskich zakończeń
            }
            else
            {
                task.BackgroundColor = Brushes.Transparent; // Kolor domyślny
            }
        }
        private void SortTasks()
        {
            var sortedTasks = tasks.OrderByDescending(t => t.IsPriority) // Najpierw priorytetowe
                                    .ThenBy(t => t.IsNearDeadline ? 0 : 1) // Następnie bliskie zakończenia
                                    .ThenBy(t => GetTaskIndex(t)) // Na końcu sortowanie według indeksu zadania
                                    .ToList();

            tasks.Clear();

            foreach (var task in sortedTasks)
            {
                tasks.Add(task);
            }

            ActiveTasksListView.SelectedItem = ActiveTasksListView.SelectedItem;
        }
        private void PriorityCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (ActiveTasksListView.SelectedItem is TaskItem selectedTask)
            {
                selectedTask.IsPriority = true; // Ustaw priorytet
                UpdateBackgroundColor(selectedTask); // Zaktualizuj kolor tła
                SortTasks(); // Sortuj po zmianie
            }
        }
        private void PriorityCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ActiveTasksListView.SelectedItem is TaskItem selectedTask)
            {
                selectedTask.IsPriority = false; // Ustaw priorytet na fałsz
                UpdateBackgroundColor(selectedTask); // Zaktualizuj kolor tła
                SortTasks(); // Sortuj po zmianie
            }
        }
        private void ActiveTasksListView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ActiveTasksListView.SelectedItem is TaskItem selectedTask)
            {
                var checkBox = e.OriginalSource as CheckBox;
                if (checkBox != null)
                {
                    checkBox.IsChecked = !checkBox.IsChecked;

                    if (checkBox.IsChecked == true)
                        PriorityCheckBox_Checked(sender, e);
                    else
                        PriorityCheckBox_Unchecked(sender, e);
                }
            }
        }

        private int GetTaskIndex(TaskItem task)
        {

            var parts = task.Name.Split('_');
            if (parts.Length >= 3 && int.TryParse(parts[2], out int index))
            {
                return index;
            }
            return -1;
        }

        private void LoadActiveTasks()
        {
            string basePath = @"C:\Users\3tp\Desktop\ProjektFD\Zlecenia";

            if (!Directory.Exists(basePath))
            {
                MessageBox.Show("Folder Zlecenia nie istnieje.");
                return;
            }

            tasks.Clear();

            var directories = Directory.GetDirectories(basePath);
            foreach (var directory in directories)
            {
                string name = Path.GetFileName(directory);
                var optionalData = GetOptionalData(directory);
                bool isPriority = LoadPriorityState(directory);
                DateTime endDate = optionalData.Item5;

                var taskItem = new TaskItem
                {
                    Name = name,
                    FirstName = optionalData.Item1,
                    LastName = optionalData.Item2,
                    Address = optionalData.Item3,
                    PhoneNumber = optionalData.Item4,
                    EndDate = endDate,
                    FolderPath = directory,
                    IsPriority = isPriority,
                    IsNearDeadline = (endDate - DateTime.Now).TotalDays <= 7
                };

                UpdateBackgroundColor(taskItem); // Dodaj to wywołanie
                tasks.Add(taskItem);
            }

            SortTasks();
            ActiveTasksListView.ItemsSource = tasks;
        }

        private bool LoadPriorityState(string folderPath)
        {
            string priorityFilePath = Path.Combine(folderPath, "priority.txt");
            if (File.Exists(priorityFilePath))
            {
                string priorityValue = File.ReadAllText(priorityFilePath);
                return bool.TryParse(priorityValue, out bool isPriority) && isPriority;
            }
            return false; // Domyślnie, jeśli plik nie istnieje
        }
        private DateTime GetEndDate(string directory)
        {
            string endDatePath = Path.Combine(directory, "endDate.txt"); // Przykład, dostosuj do swoich potrzeb
            if (File.Exists(endDatePath))
            {
                if (DateTime.TryParse(File.ReadAllText(endDatePath), out DateTime endDate))
                {
                    return endDate;
                }
            }
            return DateTime.Now.AddDays(7); // Domyślna data
        }
        private (string, string, string, string, DateTime) GetOptionalData(string directory)
        {
            string firstName = "[Nie podano]";
            string lastName = "[Nie podano]";
            string address = "[Nie podano]";
            string phoneNumber = "[Nie podano]";
            DateTime endDate = DateTime.Now.AddDays(7); // Domyślna data

            string optionalDataPath = Path.Combine(directory, "optionalData.txt");

            if (File.Exists(optionalDataPath))
            {
                var lines = File.ReadAllLines(optionalDataPath);
                if (lines.Length > 0) firstName = lines[0];
                if (lines.Length > 1) lastName = lines[1];
                if (lines.Length > 2) address = lines[2];
                if (lines.Length > 3) phoneNumber = lines[3];
                if (lines.Length > 4 && DateTime.TryParse(lines[4], out DateTime parsedDate))
                {
                    endDate = parsedDate; // Odczytaj datę zakończenia
                }
            }

            return (firstName, lastName, address, phoneNumber, endDate);
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveTasksListView.SelectedItem is TaskItem selectedTask)
            {
                System.Diagnostics.Process.Start("explorer.exe", selectedTask.FolderPath);
            }
        }
        private void UpdateTaskData(TaskItem task)
        {
            SaveOptionalData(task.FolderPath);

            File.WriteAllText(Path.Combine(task.FolderPath, "endDate.txt"), task.EndDate.ToString());
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string nazwa = NameTextBox.Text;
            DateTime dataZakoncenia = DatePicker.SelectedDate ?? DateTime.MinValue;

            if (string.IsNullOrWhiteSpace(nazwa))
            {
                MessageBox.Show("Nazwa nie może być pusta.");
                return;
            }

            if (dataZakoncenia == DateTime.MinValue)
            {
                MessageBox.Show("Data zakończenia musi być ustawiona.");
                return;
            }

            if (isEditing && currentEditingTask != null)
            {
                string oldFolderName = currentEditingTask.Name;
                string oldFolderPath = currentEditingTask.FolderPath;

                int index = GetIndexFromFolderName(oldFolderName);

                currentEditingTask.Name = $"US_{dataZakoncenia.Year}_{index}_{nazwa}";  // Zmieniona nazwa z zachowaniem indeksu
                currentEditingTask.FirstName = FirstNameTextBox.Text;
                currentEditingTask.LastName = LastNameTextBox.Text;
                currentEditingTask.Address = AddressTextBox.Text;
                currentEditingTask.PhoneNumber = PhoneNumberTextBox.Text;
                currentEditingTask.EndDate = dataZakoncenia;

                if (oldFolderName != currentEditingTask.Name)
                {
                    string newFolderPath = Path.Combine(Path.GetDirectoryName(oldFolderPath), $"US_{dataZakoncenia.Year}_{index}_{nazwa}");
                    Directory.Move(oldFolderPath, newFolderPath); // Zmień nazwę folderu
                    currentEditingTask.FolderPath = newFolderPath; // Zaktualizuj ścieżkę
                }

                SaveOptionalData(currentEditingTask.FolderPath);

                MessageBox.Show("Dane zostały zaktualizowane.");

                ClearFormFields();
            }
            else
            {
                int index = GetNextIndex();  // Przy nowym zadaniu generujemy nowy indeks
                string folderName = $"US_{dataZakoncenia.Year}_{index}_{nazwa}";
                string folderPath = Path.Combine(@"C:\Users\3tp\Desktop\ProjektFD\Zlecenia", folderName);

                try
                {
                    Directory.CreateDirectory(folderPath);
                    SaveOptionalData(folderPath);

                    var newTask = new TaskItem
                    {
                        Name = folderName,
                        FirstName = FirstNameTextBox.Text,
                        LastName = LastNameTextBox.Text,
                        Address = AddressTextBox.Text,
                        PhoneNumber = PhoneNumberTextBox.Text,
                        EndDate = dataZakoncenia,
                        FolderPath = folderPath
                    };

                    tasks.Add(newTask);
                    MessageBox.Show("Folder został utworzony: " + folderPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Wystąpił błąd: " + ex.Message);
                }
            }
            FormPanel.Visibility = Visibility.Collapsed;

            isEditing = false;
            currentEditingTask = null;

            ClearFormFields();
        }
        private int GetIndexFromFolderName(string folderName)
        {
            var parts = folderName.Split('_');
            if (parts.Length >= 3 && int.TryParse(parts[2], out int index))
            {
                return index;
            }
            return 0;
        }
        private void ClearFormFields()
        {
            NameTextBox.Clear();
            FirstNameTextBox.Clear();
            LastNameTextBox.Clear();
            AddressTextBox.Clear();
            PhoneNumberTextBox.Clear();
            DatePicker.SelectedDate = null; // Resetuj datę
        }
        private int GetNextIndex()
        {
            string basePath = @"C:\Users\3tp\Desktop\ProjektFD\Zlecenia";
            string[] directories = Directory.GetDirectories(basePath, "US_*");
            return directories.Length + 1;
        }
        private void OpenForm_Click(object sender, RoutedEventArgs e)
        {
            FormPanel.Visibility = FormPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ToggleTab_Click(object sender, RoutedEventArgs e)
        {
            var tabItem = (TabItem)((Button)sender).DataContext;
            tabItem.IsSelected = !tabItem.IsSelected;
        }
        private void SaveOptionalData(string folderPath)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(folderPath, "optionalData.txt")))
            {
                writer.WriteLine(FirstNameTextBox.Text);
                writer.WriteLine(LastNameTextBox.Text);
                writer.WriteLine(AddressTextBox.Text);
                writer.WriteLine(PhoneNumberTextBox.Text);
                writer.WriteLine(DatePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd")); // Zapisz datę zakończenia
            }
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveTasksListView.SelectedItem is TaskItem selectedTask)
            {
                string[] parts = selectedTask.Name.Split('_');


                string actualName = parts.Length > 3 ? string.Join("_", parts.Skip(3)) : selectedTask.Name; // Połącz pozostałe części

                NameTextBox.Text = actualName; // Przypisz aktualną nazwę
                FirstNameTextBox.Text = selectedTask.FirstName;
                LastNameTextBox.Text = selectedTask.LastName;
                AddressTextBox.Text = selectedTask.Address;
                PhoneNumberTextBox.Text = selectedTask.PhoneNumber;
                DatePicker.SelectedDate = selectedTask.EndDate;

                isEditing = true;
                currentEditingTask = selectedTask;

                FormPanel.Visibility = Visibility.Visible;
            }
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveTasksListView.SelectedItem is TaskItem selectedTask)
            {
                MessageBoxResult result = MessageBox.Show($"Czy na pewno chcesz usunąć zadanie: {selectedTask.Name}?",
                                                           "Potwierdzenie usunięcia",
                                                           MessageBoxButton.YesNo,
                                                           MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (Directory.Exists(selectedTask.FolderPath))
                        {
                            Directory.Delete(selectedTask.FolderPath, true);
                        }

                        tasks.Remove(selectedTask);

                        ActiveTasksListView.ItemsSource = tasks;
                        ActiveTasksListView.SelectedItem = null; // Odznacz wybrany element
                        InfoPanel.Visibility = Visibility.Collapsed; // Ukryj panel informacji
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Wystąpił błąd podczas usuwania: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Proszę zaznaczyć zadanie do usunięcia.");
            }
        }
        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveTasksListView.SelectedItem is TaskItem selectedTask)
            {
                var result = MessageBox.Show(
                    $"Czy na pewno chcesz zakończyć zadanie '{selectedTask.Name}'?",
                    "Potwierdzenie zakończenia",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    string completedFolderPath = Path.Combine(@"C:\Users\3tp\Desktop\ProjektFD\Zakończone", selectedTask.Name);

                    try
                    {
                        Directory.Move(selectedTask.FolderPath, completedFolderPath);

                        tasks.Remove(selectedTask);

                        ActiveTasksListView.ItemsSource = tasks;
                        ActiveTasksListView.SelectedItem = null;

                        LoadCompletedTasks(); // Ta metoda załaduje foldery zakończone do widoku
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Wystąpił błąd podczas przenoszenia: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("Zadanie nie zostało zakończone.");
                }
            }
            else
            {
                MessageBox.Show("Proszę zaznaczyć zadanie do zakończenia.");
            }
        }
        private void LoadCompletedTasks()
        {
            string completedPath = @"C:\Users\3tp\Desktop\ProjektFD\Zlecenia";
            if (!Directory.Exists(completedPath))
            {
                Directory.CreateDirectory(completedPath);
            }

            var completedDirectories = Directory.GetDirectories(completedPath);
            var completedTasks = new ObservableCollection<TaskItem>();

            foreach (var directory in completedDirectories)
            {
                string name = Path.GetFileName(directory);
                completedTasks.Add(new TaskItem { Name = name, FolderPath = directory });
            }

            CompletedTasksListView.ItemsSource = completedTasks;
        }
        private void ListView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;

            if (e.Delta > 0)
            {
                Console.WriteLine("Przewijasz w górę");
            }
            else if (e.Delta < 0)
            {
                Console.WriteLine("Przewijasz w dół");
            }

            foreach (var task in ActiveTasksListView.Items)
            {
                UpdateBackgroundColor((TaskItem)task);
            }
        }
    }
}
