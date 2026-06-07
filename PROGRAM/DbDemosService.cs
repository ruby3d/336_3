using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Lab2_ThinClient_DB
{
    public class DbDemosService
    {
        public string CurrentFilePath { get; private set; } = "";
        public string CurrentTableName { get; private set; } = "";

        public DataTable LoadTable(string fileName, string tableName)
        {
            string dataFolder = FindDataFolder();
            CurrentFilePath = Path.Combine(dataFolder, fileName);
            CurrentTableName = tableName;

            if (!File.Exists(CurrentFilePath))
            {
                throw new FileNotFoundException("Файл не знайдено: " + CurrentFilePath);
            }

            return LoadDbDemosXml(CurrentFilePath, tableName);
        }

        public DataView Search(DataTable table, string searchText)
        {
            DataView view = table.DefaultView;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                view.RowFilter = "";
                return view;
            }

            string safeText = searchText.Replace("'", "''");

            string filter = string.Join(" OR ",
                table.Columns.Cast<DataColumn>()
                .Select(col => $"CONVERT([{col.ColumnName}], System.String) LIKE '%{safeText}%'"));

            view.RowFilter = filter;
            return view;
        }

        public void SaveTable(DataTable table)
        {
            if (table == null)
            {
                throw new ArgumentException("Таблиця не вибрана.");
            }

            if (string.IsNullOrEmpty(CurrentFilePath))
            {
                throw new ArgumentException("Файл для збереження не вибрано.");
            }

            SaveTableToSimpleXml(table, CurrentFilePath);
        }

        private DataTable LoadDbDemosXml(string filePath, string tableName)
        {
            XDocument document = XDocument.Load(filePath);
            DataTable table = new DataTable(tableName);

            var rows = document.Descendants("ROW").ToList();

            if (rows.Count > 0)
            {
                foreach (var attribute in rows.First().Attributes())
                {
                    table.Columns.Add(attribute.Name.LocalName);
                }

                foreach (var row in rows)
                {
                    DataRow dataRow = table.NewRow();

                    foreach (var attribute in row.Attributes())
                    {
                        if (!table.Columns.Contains(attribute.Name.LocalName))
                        {
                            table.Columns.Add(attribute.Name.LocalName);
                        }

                        dataRow[attribute.Name.LocalName] = attribute.Value;
                    }

                    table.Rows.Add(dataRow);
                }

                return table;
            }

            DataSet dataSet = new DataSet();
            dataSet.ReadXml(filePath);

            if (dataSet.Tables.Count > 0)
            {
                return dataSet.Tables[0];
            }

            return table;
        }

        private void SaveTableToSimpleXml(DataTable table, string filePath)
        {
            XDocument document = new XDocument(
                new XElement(table.TableName,
                    table.Rows.Cast<DataRow>().Select(row =>
                        new XElement("ROW",
                            table.Columns.Cast<DataColumn>().Select(column =>
                                new XAttribute(column.ColumnName, row[column]?.ToString() ?? "")
                            )
                        )
                    )
                )
            );

            document.Save(filePath);
        }

        private string FindDataFolder()
        {
            DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            for (int i = 0; i < 8; i++)
            {
                string possiblePath = Path.Combine(dir.FullName, "Data");

                if (Directory.Exists(possiblePath))
                    return possiblePath;

                if (dir.Parent == null)
                    break;

                dir = dir.Parent;
            }

            throw new DirectoryNotFoundException("Папку Data не знайдено. Перевірте, що вона знаходиться в папці проекту.");
        }
    }
}