using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using TimeTrackerApp.Models;
using TimeTrackerApp.Services;

namespace TimeTrackerApp
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<Client> Clients { get; set; } = new();
        public ObservableCollection<WorkSession> Sessions { get; set; } = new();
        public ObservableCollection<WorkSession> FilteredSessions { get; set; } = new();

        private Client _selectedClient;
        public Client SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                OnPropertyChanged(nameof(SelectedClient));
                UpdateClientEditFieldsFromSelection();
                UpdateTimerRateFromClient();
            }
        }

        private WorkSession _currentSession;
        private DateTime? _currentBreakStart;
        private bool _isRunning;
        private bool _isOnBreak;

        private string _timerStatus = "Not started.";
        public string TimerStatus
        {
            get => _timerStatus;
            set { _timerStatus = value; OnPropertyChanged(nameof(TimerStatus)); }
        }

        private string _timerDetails = "";
        public string TimerDetails
        {
            get => _timerDetails;
            set { _timerDetails = value; OnPropertyChanged(nameof(TimerDetails)); }
        }

        private string _timerCurrency = "£";
        public string TimerCurrency
        {
            get => _timerCurrency;
            set { _timerCurrency = value; OnPropertyChanged(nameof(TimerCurrency)); }
        }

        private string _timerHourlyRate = "10.00";
        public string TimerHourlyRate
        {
            get => _timerHourlyRate;
            set { _timerHourlyRate = value; OnPropertyChanged(nameof(TimerHourlyRate)); }
        }

        public string ClientNameEdit { get; set; } = "";
        public bool ClientIsCompanyEdit { get; set; } = true;
        public bool ClientIsIndividualEdit
        {
            get => !ClientIsCompanyEdit;
            set
            {
                ClientIsCompanyEdit = !value;
                OnPropertyChanged(nameof(ClientIsCompanyEdit));
                OnPropertyChanged(nameof(ClientIsIndividualEdit));
            }
        }
        public string ClientCurrencyEdit { get; set; } = "£";
        public string ClientHourlyRateEdit { get; set; } = "10.00";

        public Client FilterClient { get; set; }
        public bool FilterCompaniesOnly { get; set; }
        public bool FilterIndividualsOnly { get; set; }
        public DateTime? FilterFromDate { get; set; }
        public DateTime? FilterToDate { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            LoadData();
            ApplyFilter(); 
        }

        private void LoadData()
        {
            var clients = DataStore.LoadClients();
            Clients.Clear();
            foreach (var c in clients)
                Clients.Add(c);

            var sessions = DataStore.LoadSessions();
            Sessions.Clear();
            foreach (var s in sessions.OrderByDescending(s => s.StartTime))
                Sessions.Add(s);
        }

        private void SaveData()
        {
            DataStore.SaveClients(Clients.ToList());
            DataStore.SaveSessions(Sessions.ToList());
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedClient == null)
            {
                MessageBox.Show("Please select a client first.");
                return;
            }

            if (!decimal.TryParse(TimerHourlyRate, out var rate))
            {
                MessageBox.Show("Invalid hourly rate.");
                return;
            }

            _currentSession = new WorkSession
            {
                ClientId = SelectedClient.ClientId,
                StartTime = DateTime.Now,
                HourlyRate = rate,
                CurrencySymbol = TimerCurrency
            };

            _currentBreakStart = null;
            _isRunning = true;
            _isOnBreak = false;

            TimerStatus = $"Session started at {_currentSession.StartTime:HH:mm}";
            UpdateTimerDetails();
        }

        private void BtnBreak_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRunning || _currentSession == null)
            {
                MessageBox.Show("No active session.");
                return;
            }

            if (_isOnBreak)
            {
                MessageBox.Show("Already on break.");
                return;
            }

            _isOnBreak = true;
            _currentBreakStart = DateTime.Now;
            TimerStatus = $"On break since {_currentBreakStart:HH:mm}";
            UpdateTimerDetails();
        }

        private void BtnResume_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRunning || _currentSession == null)
            {
                MessageBox.Show("No active session.");
                return;
            }

            if (!_isOnBreak || _currentBreakStart == null)
            {
                MessageBox.Show("Not currently on a break.");
                return;
            }

            _currentSession.Breaks.Add(new BreakPeriod
            {
                Start = _currentBreakStart.Value,
                End = DateTime.Now
            });

            _currentBreakStart = null;
            _isOnBreak = false;

            TimerStatus = "Working (resumed).";
            UpdateTimerDetails();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRunning || _currentSession == null)
            {
                MessageBox.Show("No active session.");
                return;
            }

            if (_isOnBreak && _currentBreakStart != null)
            {
                _currentSession.Breaks.Add(new BreakPeriod
                {
                    Start = _currentBreakStart.Value,
                    End = DateTime.Now
                });
            }

            _currentSession.EndTime = DateTime.Now;

            Sessions.Insert(0, _currentSession); 
            SaveData();
            ApplyFilter();

            TimerStatus = $"Session finished at {_currentSession.EndTime:HH:mm}";
            TimerDetails = $"Worked: {_currentSession.WorkedTimeFormatted}, " +
                           $"Break: {_currentSession.BreakTimeFormatted}, " +
                           $"Pay: {_currentSession.PayFormatted}";

            _currentSession = null;
            _currentBreakStart = null;
            _isRunning = false;
            _isOnBreak = false;
        }

        private void UpdateTimerDetails()
        {
            if (_currentSession == null)
            {
                TimerDetails = "";
                return;
            }

            var now = DateTime.Now;
            DateTime effectiveEnd = now;

            var tempSession = new WorkSession
            {
                StartTime = _currentSession.StartTime,
                HourlyRate = _currentSession.HourlyRate,
                CurrencySymbol = _currentSession.CurrencySymbol,
                Breaks = new List<BreakPeriod>(_currentSession.Breaks)
            };

            if (_isOnBreak && _currentBreakStart != null)
            {
                tempSession.Breaks.Add(new BreakPeriod
                {
                    Start = _currentBreakStart.Value,
                    End = now
                });
            }

            tempSession.EndTime = effectiveEnd;

            TimerDetails = $"Started: {_currentSession.StartTime:HH:mm} | " +
                           $"Now: {now:HH:mm}\n" +
                           $"Worked: {tempSession.WorkedTimeFormatted}, " +
                           $"Break: {tempSession.BreakTimeFormatted}, " +
                           $"Est. Pay: {tempSession.PayFormatted}";
        }

        private void UpdateTimerRateFromClient()
        {
            if (SelectedClient == null) return;
            TimerCurrency = SelectedClient.CurrencySymbol;
            TimerHourlyRate = SelectedClient.HourlyRate.ToString("0.00");
        }

        private void BtnAddClientFromTimer_Click(object sender, RoutedEventArgs e)
        {
            if (this.Content is System.Windows.Controls.Grid grid &&
                grid.Children.OfType<System.Windows.Controls.TabControl>().FirstOrDefault() is System.Windows.Controls.TabControl tab)
            {
                tab.SelectedIndex = 1; 
            }
        }

        private void UpdateClientEditFieldsFromSelection()
        {
            if (SelectedClient == null)
            {
                ClientNameEdit = "";
                ClientIsCompanyEdit = true;
                ClientCurrencyEdit = "£";
                ClientHourlyRateEdit = "10.00";
            }
            else
            {
                ClientNameEdit = SelectedClient.Name;
                ClientIsCompanyEdit = SelectedClient.IsCompany;
                ClientCurrencyEdit = SelectedClient.CurrencySymbol;
                ClientHourlyRateEdit = SelectedClient.HourlyRate.ToString("0.00");
            }

            OnPropertyChanged(nameof(ClientNameEdit));
            OnPropertyChanged(nameof(ClientIsCompanyEdit));
            OnPropertyChanged(nameof(ClientIsIndividualEdit));
            OnPropertyChanged(nameof(ClientCurrencyEdit));
            OnPropertyChanged(nameof(ClientHourlyRateEdit));
        }

        private void BtnClientNew_Click(object sender, RoutedEventArgs e)
        {
            SelectedClient = null;
            ClientNameEdit = "";
            ClientIsCompanyEdit = true;
            ClientCurrencyEdit = "£";
            ClientHourlyRateEdit = "10.00";

            OnPropertyChanged(nameof(ClientNameEdit));
            OnPropertyChanged(nameof(ClientIsCompanyEdit));
            OnPropertyChanged(nameof(ClientIsIndividualEdit));
            OnPropertyChanged(nameof(ClientCurrencyEdit));
            OnPropertyChanged(nameof(ClientHourlyRateEdit));
        }

        private void BtnClientSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ClientNameEdit))
            {
                MessageBox.Show("Client name is required.");
                return;
            }
            if (!decimal.TryParse(ClientHourlyRateEdit, out var rate))
            {
                MessageBox.Show("Invalid hourly rate.");
                return;
            }

            if (SelectedClient == null)
            {
                var newClient = new Client
                {
                    Name = ClientNameEdit,
                    IsCompany = ClientIsCompanyEdit,
                    CurrencySymbol = ClientCurrencyEdit,
                    HourlyRate = rate
                };
                Clients.Add(newClient);
                SelectedClient = newClient;
            }
            else
            {
                SelectedClient.Name = ClientNameEdit;
                SelectedClient.IsCompany = ClientIsCompanyEdit;
                SelectedClient.CurrencySymbol = ClientCurrencyEdit;
                SelectedClient.HourlyRate = rate;

                OnPropertyChanged(nameof(Clients));
            }

            SaveData();
        }

        private void BtnClientDelete_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedClient == null)
            {
                MessageBox.Show("No client selected.");
                return;
            }

            if (MessageBox.Show("Delete this client? (sessions remain but will be orphaned)",
                                "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var toRemove = SelectedClient;
                SelectedClient = null;
                Clients.Remove(toRemove);
                SaveData();
            }
        }

        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            FilteredSessions.Clear();
            var query = Sessions.AsEnumerable();

            if (FilterClient != null)
            {
                query = query.Where(s => s.ClientId == FilterClient.ClientId);
            }

            if (FilterCompaniesOnly && !FilterIndividualsOnly)
            {
                var companyIds = Clients.Where(c => c.IsCompany).Select(c => c.ClientId).ToHashSet();
                query = query.Where(s => companyIds.Contains(s.ClientId));
            }
            else if (FilterIndividualsOnly && !FilterCompaniesOnly)
            {
                var indivIds = Clients.Where(c => !c.IsCompany).Select(c => c.ClientId).ToHashSet();
                query = query.Where(s => indivIds.Contains(s.ClientId));
            }

            if (FilterFromDate.HasValue)
            {
                var from = FilterFromDate.Value.Date;
                query = query.Where(s => s.StartTime.Date >= from);
            }

            if (FilterToDate.HasValue)
            {
                var to = FilterToDate.Value.Date;
                query = query.Where(s => s.StartTime.Date <= to);
            }

            foreach (var s in query.OrderByDescending(s => s.StartTime))
                FilteredSessions.Add(s);
        }

        private void BtnExportFiltered_Click(object sender, RoutedEventArgs e)
        {
            ExportToCsv(FilteredSessions.ToList());
        }

        private void BtnExportAll_Click(object sender, RoutedEventArgs e)
        {
            ExportToCsv(Sessions.ToList());
        }

        private void ExportToCsv(List<WorkSession> sessions)
        {
            if (sessions == null || sessions.Count == 0)
            {
                MessageBox.Show("No sessions to export.");
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = "work_sessions.csv"
            };

            if (dialog.ShowDialog() != true) return;

            var clientLookup = Clients.ToDictionary(c => c.ClientId, c => c.Name);

            var sb = new StringBuilder();
            sb.AppendLine("Date,Start,End,BreakMinutes,WorkedHours,Client,Pay,Currency");

            foreach (var s in sessions)
            {
                var clientName = clientLookup.TryGetValue(s.ClientId, out var n) ? n : "Unknown";
                var breakMinutes = s.GetTotalBreakTime().TotalMinutes;
                var workedHours = s.GetWorkedTime().TotalHours;
                var pay = s.GetPay();
                var currency = s.CurrencySymbol;

                sb.AppendLine(
                    $"{s.StartTime:yyyy-MM-dd}," +
                    $"{s.StartTime:HH:mm}," +
                    $"{s.EndTime:HH:mm}," +
                    $"{breakMinutes:0}," +
                    $"{workedHours:0.00}," +
                    $"{EscapeCsv(clientName)}," +
                    $"{pay:0.00}," +
                    $"{currency}"
                );
            }

            System.IO.File.WriteAllText(dialog.FileName, sb.ToString());
            MessageBox.Show("Export complete.");
        }

        private string EscapeCsv(string value)
        {
            if (value.Contains(",") || value.Contains("\""))
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }
            return value;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
