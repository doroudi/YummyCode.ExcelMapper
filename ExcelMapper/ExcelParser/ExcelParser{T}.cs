﻿using ExcelMapper.ExcelMapper;
using ExcelMapper.Exceptions;
using ExcelMapper.Models;
using ExcelMapper.Util;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExcelMapper.ExcelParser
{
    public class ExcelParser<TSource> where TSource : new()
    {
        #region Fields
        private readonly FileInfo _file;
        private readonly ExcelImportMapper<TSource> _mapper;
        private readonly int _sheetIndex;
        #endregion

        public Dictionary<int, Dictionary<string, CellErrorLevel>> InvalidRows;
        public bool IgnoreHeader { get; private set; } = true;

        public FileInfo ActiveFile
        {
            get
            {
                return _file;
            }
        }
        public ExcelParser(FileInfo file, ExcelImportMapper<TSource> mapper, int sheetIndex = 0)
        {
            _file = file;
            _mapper = mapper;
            InvalidRows = new Dictionary<int, Dictionary<string, CellErrorLevel>>();
            _sheetIndex = sheetIndex;
        }

        /// <summary>
        /// Get items list from excel file based on mapping profile
        /// </summary>
        /// <returns>List of employees in exel file</returns>
        public List<RowModel<TSource>> GetItems(int? take = null, int skip = 0)
        {
            var workSheet = InitializeExcelFile();
            var result = new List<RowModel<TSource>>();
            var parallelOptions = new ParallelOptions
            {
                // it is recommended to use 75% of available CPU cores for parallelism
                // MaxDegreeOfParallelism = 1
                MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 1.0))
            };

            var collection = workSheet.GetAllRows().Skip(skip);
            if (take != null)
            {
                collection = collection.Take((int)take);
            }

            Parallel.ForEach(
                collection,
                parallelOptions,
                row =>
                {
                    if (IgnoreHeader && row.RowNum == 0) { return; }
                    try
                    {
                        var item = _mapper.Map(workSheet, row);
                        result.Add(new RowModel<TSource> (row.RowNum,item));
                    }
                    catch (ExcelMappingException ex)
                    {
                        InvalidRows.Add(row.RowNum, ex.Cols);
                    }
                    catch (Exception ex)
                    {
                        WriteLine.Error($"error in converting data at row  {row.RowNum} - {ex.Message}");
                        return;
                    }
                });

            return result;
        }


        #region Utilities
        private ISheet InitializeExcelFile()
        {
            try
            {
                return new ExcelReader(_file.FullName)[_sheetIndex];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion
    }
}
