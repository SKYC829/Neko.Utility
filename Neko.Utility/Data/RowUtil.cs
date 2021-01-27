using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Text;

namespace Neko.Utility.Data
{
    /// <summary>
    /// DataTable、DataRow帮助类
    /// </summary>
    public sealed partial class RowUtil
    {
        /// <summary>
        /// 给DataTable添加列
        /// </summary>
        /// <param name="dataTable"><see cref="DataTable"/></param>
        /// <param name="columns">列</param>
        public static void AddColumn(DataTable dataTable,params DataColumn[] columns)
        {
            if (dataTable == null)
            {
                throw new ArgumentNullException(nameof(dataTable), "参数dataTable不允许为空!");
            }
            if (columns == null)
            {
                return;
            }
            foreach (DataColumn column in columns)
            {
                if (dataTable.Columns.Contains(column.ColumnName))
                {
                    continue;
                }
                dataTable.Columns.Add(column);
            }
        }

        /// <summary>
        /// <inheritdoc cref="AddColumn(DataTable, DataColumn[])"/>
        /// </summary>
        /// <param name="dataTable"><see cref="DataTable"/></param>
        /// <param name="columnNames">列名</param>
        public static void AddColumn(DataTable dataTable, params string[] columnNames)
        {
            if(dataTable == null)
            {
                throw new ArgumentNullException(nameof(dataTable), "参数dataTable不允许为空!");
            }
            if(columnNames == null)
            {
                return;
            }
            List<DataColumn> columns = new List<DataColumn>();
            foreach (string columnName in columnNames)
            {
                DataColumn column = new DataColumn(columnName);
                columns.Add(column);
            }
            AddColumn(dataTable, columns.ToArray());
        }

        /// <summary>
        /// 从DataTable中获取指定位置的DataRow
        /// </summary>
        /// <param name="dataTable"><see cref="DataTable"/></param>
        /// <param name="index">位置索引</param>
        /// <returns></returns>
        public static DataRow GetRow(DataTable dataTable,int index)
        {
            if(dataTable == null)
            {
                throw new ArgumentNullException(nameof(dataTable), "参数dataTable不允许为空!");
            }
            index = Math.Max(0, index);
            if(index > dataTable.Rows.Count)
            {
                throw new IndexOutOfRangeException("位置索引已超出DataTable最大行数!");
            }
            return dataTable.Rows[index];
        }

        /// <summary>
        /// 获取DataTable的第一行
        /// </summary>
        /// <param name="dataTable"><see cref="DataTable"/></param>
        /// <returns></returns>
        public static DataRow GetFirstRow(DataTable dataTable)
        {
            if (dataTable == null)
            {
                throw new ArgumentNullException(nameof(dataTable), "参数dataTable不允许为空!");
            }
            return GetRow(dataTable, 0);
        }

        /// <summary>
        /// 设置一行中某一列的值
        /// </summary>
        /// <param name="dataRow"><see cref="DataRow"/></param>
        /// <param name="columnName">列名</param>
        /// <param name="fieldValue">列的值</param>
        public static void Set(DataRow dataRow,string columnName,object fieldValue)
        {
            if(dataRow == null)
            {
                throw new ArgumentNullException(nameof(dataRow), "参数dataRow不允许为空!");
            }
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentNullException(nameof(columnName), "参数columnName不允许为空!");
            }
            DataTable parent = dataRow.Table;
            if (!parent.Columns.Contains(columnName))
            {
                AddColumn(parent, columnName);
            }
            dataRow[columnName] = fieldValue;
        }
    }

    /// <summary>
    /// 取值部分
    /// </summary>
    public sealed partial class RowUtil
    {
        /// <summary>
        /// 获取一行中某一列的值
        /// </summary>
        /// <param name="dataRow"><see cref="DataRow"/></param>
        /// <param name="columnName">列名</param>
        /// <returns></returns>
        public static object Get(DataRow dataRow,string columnName)
        {
            if(dataRow == null)
            {
                throw new ArgumentNullException(nameof(dataRow), "参数dataRow不允许为空!");
            }
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentNullException(nameof(columnName), "参数columnName不允许为空!");
            }
            DataTable dataTable = dataRow.Table;
            if (dataTable.Columns.Contains(columnName))
            {
                if(dataRow.RowState != DataRowState.Detached)
                {
                    return dataRow[columnName];
                }
            }
            return null;
        }

        /// <summary>
        /// <inheritdoc cref="Get(DataRow, string)"/>的指定类型
        /// </summary>
        /// <param name="dataRow"><see cref="DataRow"/></param>
        /// <param name="columnName">列名</param>
        /// <param name="toType">要转换的类型</param>
        /// <returns></returns>
        public static object Get(DataRow dataRow,string columnName,Type toType)
        {
            if (dataRow == null)
            {
                throw new ArgumentNullException(nameof(dataRow), "参数dataRow不允许为空!");
            }
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentNullException(nameof(columnName), "参数columnName不允许为空!");
            }
            object res = Get(dataRow, columnName);
            if(res != null)
            {
                res = StringUtil.Get(toType, res);
            }
            return res;
        }

        /// <summary>
        /// <inheritdoc cref="Get(DataRow, string, Type)"/>
        /// </summary>
        /// <typeparam name="TResult">值类型</typeparam>
        /// <param name="dataRow"><see cref="DataRow"/></param>
        /// <param name="columnName">列名</param>
        /// <returns></returns>
        public static TResult Get<TResult>(DataRow dataRow,string columnName)
        {
            object res = Get(dataRow, columnName);
            if(res != null)
            {
                res = StringUtil.Get<TResult>(res);
            }
            return (TResult)res;
        }

        /// <summary>
        /// <inheritdoc cref="Get(DataRow, string)"/>
        /// </summary>
        /// <param name="dataRow"><see cref="DataRow"/></param>
        /// <param name="columnName">列名</param>
        /// <returns></returns>
        public static bool GetBoolean(DataRow dataRow,string columnName)
        {
            return Get<bool>(dataRow, columnName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(DataRow, string)"/>
        /// </summary>
        /// <param name="dataRow"><see cref="DataRow"/></param>
        /// <param name="columnName">列名</param>
        /// <returns></returns>
        public static string GetString(DataRow dataRow,string columnName)
        {
            return Get<string>(dataRow, columnName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(DataRow, string)"/>
        /// </summary>
        /// <param name="dataRow"><see cref="DataRow"/></param>
        /// <param name="columnName">列名</param>
        /// <returns></returns>
        public static int GetInt(DataRow dataRow,string columnName)
        {
            return Get<int>(dataRow, columnName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(DataRow, string)"/>
        /// </summary>
        /// <param name="dataRow"><see cref="DataRow"/></param>
        /// <param name="columnName">列名</param>
        /// <returns></returns>
        public static double GetDouble(DataRow dataRow,string columnName)
        {
            return Get<double>(dataRow, columnName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(DataRow, string)"/>
        /// </summary>
        /// <param name="dataRow"><see cref="DataRow"/></param>
        /// <param name="columnName">列名</param>
        /// <returns></returns>
        public static float GetFloat(DataRow dataRow,string columnName)
        {
            return Get<float>(dataRow, columnName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(DataRow, string)"/>
        /// </summary>
        /// <param name="dataRow"><see cref="DataRow"/></param>
        /// <param name="columnName">列名</param>
        /// <returns></returns>
        public static short GetShort(DataRow dataRow,string columnName)
        {
            return Get<short>(dataRow, columnName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(DataRow, string)"/>
        /// </summary>
        /// <param name="dataRow"><see cref="DataRow"/></param>
        /// <param name="columnName">列名</param>
        /// <returns></returns>
        public static decimal GetDecimal(DataRow dataRow,string columnName)
        {
            return Get<decimal>(dataRow, columnName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(DataRow, string)"/>
        /// </summary>
        /// <param name="dataRow"><see cref="DataRow"/></param>
        /// <param name="columnName">列名</param>
        /// <returns></returns>
        public static DateTime? GetDateTime(DataRow dataRow,string columnName)
        {
            return Get<DateTime>(dataRow, columnName);
        }

        /// <summary>
        /// <inheritdoc cref="Get(DataRow, string)"/>
        /// </summary>
        /// <param name="dataRow"><see cref="DataRow"/></param>
        /// <param name="columnName">列名</param>
        /// <returns></returns>
        public static BigInteger GetBigInteger(DataRow dataRow,string columnName)
        {
            return Get<BigInteger>(dataRow, columnName);
        }
    }
}
