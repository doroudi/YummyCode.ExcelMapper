﻿using System;
using System.Globalization;
using NPOI.SS.UserModel;
using YummyCode.ExcelMapper.Shared.Models;

namespace YummyCode.ExcelMapper.Shared.Extensions
{
    public static class CellExtensions
    {
        public static ICell Colorize(this ICell @this, ResultState errorLevel)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            @this.CellStyle.FillForegroundColor =
                (errorLevel == ResultState.Danger) ?
                    IndexedColors.Red.Index :
                    IndexedColors.Yellow.Index;
            @this.CellStyle.FillPattern = FillPattern.SolidForeground;

            return @this;
        }

        public static ICell ApplyStyle(this ICell @this, ICellStyle cellStyle)
        {
            @this.CellStyle = cellStyle;
            return @this;
        }

        public static ICell SetAlignment(this ICell @this, HorizontalAlignment alignment)
        {
            @this.CellStyle.Alignment = alignment;
            return @this;
        }

        public static ICell SetCentered(this ICell @this)
        {
            if (@this == null) 
                throw new ArgumentNullException(nameof(@this));
            @this.CellStyle.Alignment = HorizontalAlignment.Center;
            @this.CellStyle.VerticalAlignment = VerticalAlignment.Center;
            return @this;
        }

        public static string GetValue(this ICell @this)
        {
            try
            {
                return @this.CellType switch
                {
                    CellType.Numeric => DateUtil.IsCellDateFormatted(@this) ? 
                        @this.DateCellValue.ToString(CultureInfo.InvariantCulture) 
                        : @this.NumericCellValue.ToString(CultureInfo.InvariantCulture),
                    CellType.String => @this.StringCellValue,
                    CellType.Blank => string.Empty,
                    CellType.Formula =>
                                @this.CachedFormulaResultType == CellType.Numeric ?
                                    @this.NumericCellValue.ToString(CultureInfo.InvariantCulture) :
                                    @this.StringCellValue,
                    _ => @this.StringCellValue ?? @this.NumericCellValue.ToString(CultureInfo.InvariantCulture)
                };
            }
            catch
            {
                return @this.StringCellValue;
            }
        }
    }
}
