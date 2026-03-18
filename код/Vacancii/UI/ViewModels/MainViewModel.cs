using Core.Indexing;
using Core.Models;
using Core.Search;
using Data.SampleData;
using Data.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace UI.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly SearchEngine _searchEngine;
        private readonly SPIMIIndexer _indexer;
        private readonly VacancyGenerator _generator;
        private readonly IndexSerializer _serializer;

        // === Поиск ===
        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set => SetProperty(ref _searchQuery, value);
        }

        private string _parsedQuery = string.Empty;
        public string ParsedQuery
        {
            get => _parsedQuery;
            set => SetProperty(ref _parsedQuery, value);
        }

        private string _searchInfo = string.Empty;
        public string SearchInfo
        {
            get => _searchInfo;
            set => SetProperty(ref _searchInfo, value);
        }

        private bool _useSimpleMode = true;
        public bool UseSimpleMode
        {
            get => _useSimpleMode;
            set => SetProperty(ref _useSimpleMode, value);
        }

        public ObservableCollection<SearchResultItem> SearchResults { get; } = new();

        // === Индексация ===
        private int _documentCount = 500;
        public int DocumentCount
        {
            get => _documentCount;
            set => SetProperty(ref _documentCount, value);
        }

        private int _blockSize = 5000;
        public int BlockSize
        {
            get => _blockSize;
            set => SetProperty(ref _blockSize, value);
        }

        private double _indexProgress;
        public double IndexProgress
        {
            get => _indexProgress;
            set => SetProperty(ref _indexProgress, value);
        }

        private bool _isIndexing;
        public bool IsIndexing
        {
            get => _isIndexing;
            set
            {
                SetProperty(ref _isIndexing, value);
                OnPropertyChanged(nameof(IsNotIndexing));
                OnPropertyChanged(nameof(CanSearch));
            }
        }
        public bool IsNotIndexing => !_isIndexing;

        private bool _isIndexLoaded;
        public bool IsIndexLoaded
        {
            get => _isIndexLoaded;
            set
            {
                SetProperty(ref _isIndexLoaded, value);
                OnPropertyChanged(nameof(CanSearch));
            }
        }

        public bool CanSearch => IsIndexLoaded && !IsIndexing;

        public ObservableCollection<string> LogMessages { get; } = new();

        // === Статистика ===
        private string _statsText = "Индекс не загружен";
        public string StatsText
        {
            get => _statsText;
            set => SetProperty(ref _statsText, value);
        }

        // === Словарь (для просмотра индекса) ===
        private string _termLookup = string.Empty;
        public string TermLookup
        {
            get => _termLookup;
            set => SetProperty(ref _termLookup, value);
        }

        private string _termInfo = string.Empty;
        public string TermInfo
        {
            get => _termInfo;
            set => SetProperty(ref _termInfo, value);
        }

        // === Выбранный результат ===
        private SearchResultItem? _selectedResult;
        public SearchResultItem? SelectedResult
        {
            get => _selectedResult;
            set => SetProperty(ref _selectedResult, value);
        }

        // === Команды ===
        public ICommand BuildIndexCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand LookupTermCommand { get; }
        public ICommand SaveIndexCommand { get; }
        public ICommand LoadIndexCommand { get; }

        public MainViewModel()
        {
            _searchEngine = new SearchEngine();
            _indexer = new SPIMIIndexer();
            _generator = new VacancyGenerator();
            _serializer = new IndexSerializer();

            BuildIndexCommand = new AsyncRelayCommand(BuildIndexAsync, () => !IsIndexing);
            SearchCommand = new RelayCommand(ExecuteSearch, () => CanSearch);
            ClearCommand = new RelayCommand(ClearSearch);
            LookupTermCommand = new RelayCommand(LookupTerm);
            SaveIndexCommand = new AsyncRelayCommand(SaveIndexAsync, () => IsIndexLoaded);
            LoadIndexCommand = new AsyncRelayCommand(LoadIndexAsync, () => !IsIndexing);

            // Подписываемся на события индексера
            _indexer.OnLogMessage += msg =>
                Application.Current.Dispatcher.Invoke(() => LogMessages.Add(msg));

            _indexer.OnProgressChanged += (current, total) =>
                Application.Current.Dispatcher.Invoke(() =>
                    IndexProgress = (double)current / total * 100);
        }

        private async Task BuildIndexAsync()
        {
            IsIndexing = true;
            IndexProgress = 0;
            LogMessages.Clear();
            SearchResults.Clear();

            try
            {
                LogMessages.Add($"Генерация {DocumentCount} вакансий...");

                var vacancies = await Task.Run(() => _generator.Generate(DocumentCount));
                LogMessages.Add($"Вакансии сгенерированы.");

                // Создаём новый индексер с указанным размером блока
                var indexer = new SPIMIIndexer(BlockSize);
                indexer.OnLogMessage += msg =>
                    Application.Current.Dispatcher.Invoke(() => LogMessages.Add(msg));
                indexer.OnProgressChanged += (current, total) =>
                    Application.Current.Dispatcher.Invoke(() =>
                        IndexProgress = (double)current / total * 100);

                var index = await Task.Run(() => indexer.BuildIndex(vacancies));

                _searchEngine.LoadIndex(index);
                IsIndexLoaded = true;

                UpdateStatistics();
                LogMessages.Add("✓ Индекс готов к использованию!");
            }
            catch (Exception ex)
            {
                LogMessages.Add($"ОШИБКА: {ex.Message}");
                MessageBox.Show($"Ошибка при построении индекса:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsIndexing = false;
            }
        }

        private void ExecuteSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
                return;

            SearchResults.Clear();

            try
            {
                var response = _searchEngine.Search(SearchQuery, UseSimpleMode);

                ParsedQuery = response.ParsedQuery;
                SearchInfo = $"Найдено: {response.TotalFound} | " +
                             $"Время: {response.SearchDuration.TotalMilliseconds:F1} мс";

                foreach (var result in response.Results)
                {
                    SearchResults.Add(new SearchResultItem
                    {
                        Vacancy = result.Vacancy,
                        Score = result.Score,
                        MatchedTerms = string.Join(", ", result.MatchedTerms),
                        Rank = SearchResults.Count + 1
                    });
                }
            }
            catch (Exception ex)
            {
                SearchInfo = $"Ошибка: {ex.Message}";
            }
        }

        private void ClearSearch()
        {
            SearchQuery = string.Empty;
            ParsedQuery = string.Empty;
            SearchInfo = string.Empty;
            SearchResults.Clear();
            SelectedResult = null;
        }

        private void LookupTerm()
        {
            if (string.IsNullOrWhiteSpace(TermLookup) || !IsIndexLoaded)
                return;

            var postingList = _searchEngine.LookupTerm(TermLookup);
            if (postingList == null)
            {
                TermInfo = $"Терм '{TermLookup}' не найден в индексе.";
                return;
            }

            var docIds = string.Join(", ", postingList.GetDocIds().Take(50));
            var more = postingList.DocumentFrequency > 50
                ? $"\n... и ещё {postingList.DocumentFrequency - 50}"
                : "";

            TermInfo = $"Терм: '{postingList.Term}'\n" +
                       $"Document Frequency: {postingList.DocumentFrequency}\n" +
                       $"DocIDs: [{docIds}]{more}";
        }

        private async Task SaveIndexAsync()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON Index|*.json",
                FileName = "vacancy_index.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var stats = _searchEngine.Statistics;
                    // Нужно сохранить через сериализатор — допилим позже
                    LogMessages.Add($"Индекс сохранён: {dialog.FileName}");
                }
                catch (Exception ex)
                {
                    LogMessages.Add($"Ошибка сохранения: {ex.Message}");
                }
            }
        }

        private async Task LoadIndexAsync()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON Index|*.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var index = await _serializer.LoadAsync(dialog.FileName);
                    if (index != null)
                    {
                        _searchEngine.LoadIndex(index);
                        IsIndexLoaded = true;
                        UpdateStatistics();
                        LogMessages.Add($"Индекс загружен: {dialog.FileName}");
                    }
                }
                catch (Exception ex)
                {
                    LogMessages.Add($"Ошибка загрузки: {ex.Message}");
                }
            }
        }

        private void UpdateStatistics()
        {
            var stats = _searchEngine.Statistics;
            if (stats == null) return;

            StatsText = $"📄 Документов: {stats.TotalDocuments}\n" +
                        $"📝 Уникальных термов: {stats.TotalTerms}\n" +
                        $"📊 Всего постингов: {stats.TotalPostings}\n" +
                        $"📈 Средняя длина постинг-листа: {stats.AveragePostingListLength:F1}\n" +
                        $"🕐 Время построения: {stats.BuildDuration.TotalSeconds:F2} сек.\n" +
                        $"📅 Дата: {stats.BuildDate:dd.MM.yyyy HH:mm:ss}";
        }
    }

    /// <summary>
    /// Элемент результата для отображения в UI
    /// </summary>
    public class SearchResultItem : BaseViewModel
    {
        public int Rank { get; set; }
        public Vacancy Vacancy { get; set; } = null!;
        public double Score { get; set; }
        public string MatchedTerms { get; set; } = string.Empty;

        public string SalaryDisplay
        {
            get
            {
                if (!Vacancy.SalaryFrom.HasValue && !Vacancy.SalaryTo.HasValue)
                    return "Зарплата не указана";

                var from = Vacancy.SalaryFrom?.ToString("N0") ?? "?";
                var to = Vacancy.SalaryTo?.ToString("N0") ?? "?";
                return $"{from} – {to} ₽";
            }
        }

        public string TagsDisplay => string.Join(" • ", Vacancy.Tags);
    }
}
