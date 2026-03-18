using Core.Models;
using Core.Preprocessing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Indexing
{
    /// <summary>
    /// Single-Pass In-Memory Indexing (SPIMI)
    /// 
    /// Алгоритм:
    /// 1. Читаем документы потоком (пара term, docID)
    /// 2. Для каждого терма — если терм новый, создаём новый постинг-лист
    ///    (НЕ используем глобальный словарь term→termID как в BSBI)
    /// 3. Добавляем docID в постинг-лист терма
    /// 4. Когда блок заполнился (лимит памяти) — сортируем термы, сбрасываем блок
    /// 5. После обработки всех документов — мержим все блоки
    /// </summary>
    public class SPIMIIndexer
    {
        private readonly TextPreprocessor _preprocessor;
        private readonly int _blockSizeLimit; // макс. кол-во термов в блоке
        private readonly List<SPIMIBlock> _blocks;
        private int _blockCounter;

        // События для отслеживания прогресса
        public event Action<string>? OnLogMessage;
        public event Action<int, int>? OnProgressChanged; // (current, total)
        public event Action<int>? OnBlockFlushed; // blockId

        public SPIMIIndexer(int blockSizeLimit = 10000)
        {
            _preprocessor = new TextPreprocessor();
            _blockSizeLimit = blockSizeLimit;
            _blocks = new List<SPIMIBlock>();
            _blockCounter = 0;
        }

        /// <summary>
        /// Основной метод: построить инвертированный индекс из коллекции вакансий
        /// </summary>
        public InvertedIndex BuildIndex(List<Vacancy> vacancies)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _blocks.Clear();
            _blockCounter = 0;

            Log($"Начало индексации {vacancies.Count} документов...");
            Log($"Лимит блока: {_blockSizeLimit} уникальных термов");

            // Фаза 1: SPIMI-Invert — разбиваем на блоки
            var currentBlock = CreateNewBlock();

            for (int i = 0; i < vacancies.Count; i++)
            {
                var vacancy = vacancies[i];
                var termsWithPositions = _preprocessor.ProcessWithPositions(vacancy.GetFullText());

                foreach (var (term, position) in termsWithPositions)
                {
                    // Ключевое отличие SPIMI от BSBI:
                    // Мы НЕ маппим term → termID
                    // Мы используем сам term как ключ словаря

                    if (!currentBlock.Index.ContainsKey(term))
                    {
                        // Создаём новый постинг-лист прямо в словаре
                        currentBlock.Index[term] = new PostingList { Term = term };
                    }

                    // Добавляем posting (docId + позиция)
                    currentBlock.Index[term].AddPosting(vacancy.DocId, position);

                    // Проверяем лимит блока (по количеству уникальных термов)
                    if (currentBlock.Index.Count >= _blockSizeLimit)
                    {
                        FlushBlock(currentBlock);
                        currentBlock = CreateNewBlock();
                    }
                }

                OnProgressChanged?.Invoke(i + 1, vacancies.Count);
            }

            // Сбрасываем последний блок
            if (currentBlock.Index.Count > 0)
            {
                FlushBlock(currentBlock);
            }

            Log($"Создано блоков: {_blocks.Count}");

            // Фаза 2: Merge — слияние всех блоков
            Log("Начало слияния блоков...");
            var merger = new IndexMerger();
            var mergedIndex = merger.MergeBlocks(_blocks);

            stopwatch.Stop();

            // Собираем финальный индекс
            var invertedIndex = new InvertedIndex
            {
                Index = mergedIndex,
                Documents = vacancies.ToDictionary(v => v.DocId),
                BuildDate = DateTime.Now,
                BuildDuration = stopwatch.Elapsed
            };

            Log($"Индексация завершена за {stopwatch.Elapsed.TotalSeconds:F2} сек.");
            Log($"Уникальных термов: {invertedIndex.TotalTerms}");
            Log($"Документов: {invertedIndex.TotalDocuments}");

            return invertedIndex;
        }

        private SPIMIBlock CreateNewBlock()
        {
            return new SPIMIBlock
            {
                BlockId = _blockCounter++,
                Index = new Dictionary<string, PostingList>()
            };
        }

        private void FlushBlock(SPIMIBlock block)
        {
            // В реальной системе здесь бы записывали на диск
            // Сортируем термы в блоке (SPIMI сортирует при сбросе)
            // SortedDictionary создаётся при мерже

            Log($"  Блок #{block.BlockId} сброшен: {block.Index.Count} термов");
            _blocks.Add(block);
            OnBlockFlushed?.Invoke(block.BlockId);
        }

        private void Log(string message)
        {
            OnLogMessage?.Invoke($"[SPIMI] {message}");
        }
    }
}
