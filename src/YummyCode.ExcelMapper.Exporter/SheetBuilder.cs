﻿using System;
using System.Collections.Generic;
using NPOI.SS.UserModel;
using YummyCode.ExcelMapper.Shared.Models;

namespace YummyCode.ExcelMapper.Exporter
{
    public class SheetBuilder<TSource> where TSource : new()
    {
        private ISheet _sheet;
        private readonly IWorkbook _workBook;
        private readonly IExportMapper<TSource> _mapper;
        private readonly SheetOptions<TSource> _options;
        private ICollection<RowModel<TSource>> _data;
        public SheetBuilder(IWorkbook workBook, IExportMapper<TSource> mapper, Action<SheetOptions<TSource>> options = null)
        {
            _workBook = workBook;
            _mapper = mapper;
            _data = new List<RowModel<TSource>>();
            _options = new SheetOptions<TSource>(mapper);
            options?.Invoke(_options);
        }
        public SheetBuilder<TSource> UseData(ICollection<RowModel<TSource>> data)
        {
            this._data = data;
            return this;
        }

        public SheetBuilder<TSource> WithName(string sheetName)
        {
            if (sheetName == null)
                throw new ArgumentNullException(nameof(sheetName));

            if (_options != null)
                _options.Name = sheetName;
            return this;
        }

        public SheetBuilder<TSource> SetRtl()
        {
            _options.Rtl = true;
            return this;
        }

        public SheetBuilder<TSource> UseDefaultHeaderStyle()
        {
            var headerStyle = _workBook.CreateCellStyle();
            headerStyle.Alignment = HorizontalAlignment.Center;
            headerStyle.BorderTop = headerStyle.BorderBottom = headerStyle.BorderLeft = headerStyle.BorderRight = BorderStyle.Thin;
            headerStyle.VerticalAlignment = VerticalAlignment.Center;
            headerStyle.FillForegroundColor = IndexedColors.Grey25Percent.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            _options.HeaderStyle = headerStyle;
            return this;
        }

        public SheetBuilder<TSource> UseHeaderStyle(CellStyleOptions cellStyle)
        {
            var headerStyle = cellStyle.ConvertToExcelCellStyle(_workBook);
            _options.HeaderStyle = headerStyle;
            return this;
        }

        public ISheet Build()
        {
            _sheet = _workBook.CreateSheet(_options?.Name ?? "Sheet 1");

            if (_options.Rtl)
            {
                _sheet.IsRightToLeft = true;
            }
            BuildHeader();
            SetData();
            SetAutoWidthColumns();
            return _sheet;
        }

        private void SetAutoWidthColumns()
        {
            for (var i = 0; i < _sheet.GetRow(0).Cells.Count; i++)
            {
                _sheet.AutoSizeColumn(i);
            }
        }

        private void SetData()
        {
            foreach (var item in _data)
            {
                var sheetRow = _sheet.CreateRow(item.Row);
                _mapper.Map(item.Source, sheetRow);
            }
        }

        private void BuildHeader()
        {
            var headerRow = _sheet.CreateRow(0);
            _mapper.MapHeader(ref headerRow);
            foreach (var cell in _sheet.GetRow(0))
            {
                cell.CellStyle = _options.HeaderStyle;
            }
        }

    }
}