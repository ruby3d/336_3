using System;
using System.Data;
using System.Windows.Forms;

namespace Lab2_ThinClient_DB
{
    public partial class Form1 : Form
    {
        private readonly DbDemosService service;
        private DataTable currentTable;

        public Form1()
        {
            InitializeComponent();

            service = new DbDemosService();

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AllowUserToAddRows = true;
            dataGridView1.AllowUserToDeleteRows = true;

            btnVenues.Click += (s, e) => LoadTable("venues.xml", "Venues");
            btnEvents.Click += (s, e) => LoadTable("events.xml", "Events");
            btnCustomer.Click += (s, e) => LoadTable("customer.xml", "Customer");
            btnReservat.Click += (s, e) => LoadTable("reservat.xml", "Reservat");

            btnSearch.Click += BtnSearch_Click;
            btnRefresh.Click += BtnRefresh_Click;
            btnSave.Click += BtnSave_Click;

            ConnectMenuEvents();
        }

        private void LoadTable(string fileName, string tableName)
        {
            try
            {
                currentTable = service.LoadTable(fileName, tableName);
                dataGridView1.DataSource = currentTable;

                txtSearch.Clear();

                Text = "Лабораторна робота №2 - Тонкий клієнт DBDEMOS | Таблиця: " + tableName;

                if (currentTable.Rows.Count == 0)
                {
                    MessageBox.Show("Таблиця завантажена, але дані не знайдено.",
                        "Інформація", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка завантаження таблиці:\n" + ex.Message,
                    "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentTable == null)
                {
                    MessageBox.Show("Спочатку оберіть таблицю.",
                        "Інформація", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                dataGridView1.DataSource = service.Search(currentTable, txtSearch.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка пошуку:\n" + ex.Message,
                    "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(service.CurrentFilePath))
                {
                    MessageBox.Show("Спочатку оберіть таблицю.",
                        "Інформація", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string tableName = service.CurrentTableName;
                string fileName = System.IO.Path.GetFileName(service.CurrentFilePath);

                LoadTable(fileName, tableName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка оновлення:\n" + ex.Message,
                    "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentTable == null)
                {
                    MessageBox.Show("Немає відкритої таблиці для збереження.",
                        "Інформація", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                dataGridView1.EndEdit();
                service.SaveTable(currentTable);

                MessageBox.Show("Зміни збережено у файл:\n" + service.CurrentFilePath,
                    "Збереження", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка збереження:\n" + ex.Message,
                    "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConnectMenuEvents()
        {
            foreach (ToolStripMenuItem item in menuStrip1.Items)
            {
                foreach (ToolStripItem subItem in item.DropDownItems)
                {
                    if (subItem.Text == "Exit")
                    {
                        subItem.Click += (s, e) => Close();
                    }

                    if (subItem.Text == "About")
                    {
                        subItem.Click += (s, e) =>
                        {
                            MessageBox.Show(
                                "Лабораторна робота №2\n" +
                                "Створення багатоланкових додатків БД\n\n" +
                                "Варіант 3: Events - Venues - Reservat - Customer\n" +
                                "Розробник: Костюченко Маркіян Євгенович\n\n" +
                                "Програма реалізує тонкий клієнт для відображення, пошуку та модифікації даних з таблиць DBDEMOS.",
                                "About",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        };
                    }
                }
            }
        }
    }
}