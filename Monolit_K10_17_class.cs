using System;
using System.Data.SQLite;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace AutoProt.Plants
{
    sealed class Monolit_K10_17_class : Monolit_CommonCapacitor
    {
        string[] acceptedVoltageStrings = { "50В", "100В" };
        string[] TCEVariants = { "Н50", "Н20", "Н90", "МП0", "МПО", "М47", "М1500" };
        int size = 0;
        string nameOfTable = String.Empty;
        public Monolit_K10_17_class(string partnumber, int quantity, string priemka, int stringnumber) : base(partnumber, quantity, priemka, stringnumber)
        {
            //----
            Recognize();
            //----
        }

        private void Recognize()
        {
            if (_priemka == Const.PRIEMKA_OTK) nameOfTable = "Monolit_K10_17OTK";
            else nameOfTable = "Monolit_K10_17";

            if (partnumber.Contains("К10-17А")) type = "К10-17А";
            else if (partnumber.Contains("К10-17Б")) type = "К10-17Б";
            else if (partnumber.Contains("К10-17В")) type = "К10-17В";
            else if (partnumber.Contains("К10-17-4В")) type = "К10-17-4В";
            else
            {
                _comments.Insert(0, "Не указан тип конденсатора (К10-17а, К10-17б, К10-17в или К10-17-4в); ");
                _recognizeSucceed = false;
                return;
            }

            if (partnumber.Contains("К10-17-4В"))
            {
                partnumber.Remove(0, partnumber.IndexOf("К10-17-4В") + 10);
            }
            else partnumber.Remove(0, partnumber.IndexOf("К10-17") + 7);

            if (CheckVoltage(acceptedVoltageStrings) == 0) voltage = 0;
            if (!_recognizeSucceed) return;
            Console.WriteLine("Voltage : " + voltage);

            CheckCapacity();
            if (!_recognizeSucceed) return;
            Console.WriteLine("Capacity : " + capacity);

            CheckTCE(TCEVariants);
            if (!_recognizeSucceed) return;
            Console.WriteLine("TCE : " + TCE);

            if (TCE != "Н90") CheckTolerance();
            else tolerance = 80d;

            if (!_recognizeSucceed) return;
            Console.WriteLine("Tolerance : " + tolerance);

            if (type.Contains("К10-17В") || type.Contains("К10-17-4В")) CheckNikelPads();

            CheckSize();
            Console.WriteLine("Size : " + size);


            CheckPriceInDB();
            if (!_recognizeSucceed) return;

            CheckPriceInDB2();
            if (!_recognizeSucceed) return;

            CheckPriceInDB3();
            if (!_recognizeSucceed) return;

            GetRealPNandPrice();
        }
        
        private void CheckPriceInDB()
        {
            try
            {
                Const.CONN.Open();
            }
            catch (SqlException)
            {
                MessageBox.Show("Ошибка подключения к базе данных");
                return;
            }

            //SQLiteCommand CMD = Const.CONN.CreateCommand();
            SqlCommand CMD = Const.CONN.CreateCommand();
            CMD.CommandText = "SELECT * FROM " + nameOfTable + " WHERE Type = '" + type + "' AND ContPads = '" + contPads + "' AND TCE LIKE '%" + TCE + "%'";
            //SQLiteDataReader reader = CMD.ExecuteReader();
            SqlDataReader reader = CMD.ExecuteReader();
            if (!reader.HasRows)
            {
                //пробуем рассмотреть вариант с никель-барьером
                if (contPads != "N")
                {
                    reader.Close();
                    _comments.Insert(0, "Попробую поискать вариант с никель-барьером; ");
                    contPads = "N";
                    //SQLiteCommand CMD2 = Const.CONN.CreateCommand();
                    SqlCommand CMD2 = Const.CONN.CreateCommand();
                    CMD2.CommandText = "SELECT * FROM " + nameOfTable + " WHERE Type = '" + type + "' AND ContPads = '" + contPads + "' AND TCE LIKE '%" + TCE + "%'";
                    //SQLiteDataReader reader = CMD.ExecuteReader();
                    SqlDataReader Reader2 = CMD.ExecuteReader();

                    if (!Reader2.HasRows)
                    {
                        _comments.Insert(0, "с никель-барьером тоже нет; ");
                        _recognizeSucceed = false;
                        Reader2.Close();
                        reader.Close();
                        Const.CONN.Close();
                        return;
                    }
                    Reader2.Close();
                }
            }
            reader.Close();
        }

        private void CheckPriceInDB2()
        {

            //SQLiteCommand CMD = Const.CONN.CreateCommand();
            SqlCommand CMD = Const.CONN.CreateCommand();
            CMD.CommandText = "SELECT * FROM " + nameOfTable + " WHERE Type = '" + type + "' AND ContPads = '" + contPads + "' AND TCE LIKE '%" + TCE + "%' AND Voltage = " + voltage
                + " AND Size = " + size + " AND CapacityFrom <= " + capacityString + " AND CapacityTo >= " + capacityString;
            //SQLiteDataReader reader = CMD.ExecuteReader();
            SqlDataReader reader = CMD.ExecuteReader();

            if (!reader.HasRows)
            {
                //пробуем рассмотреть вариант с неопред вольтажом
                reader.Close();

                voltage = 0;
                //SQLiteCommand CMD2 = Const.CONN.CreateCommand();
                SqlCommand CMD2 = Const.CONN.CreateCommand();
                CMD2.CommandText = "SELECT * FROM " + nameOfTable + " WHERE Type = '" + type + "' AND ContPads = '" + contPads + "' AND TCE LIKE '%" + TCE + "%' AND Voltage = " + voltage
                 + " AND Size = " + size + " AND CapacityFrom <= " + capacityString + " AND CapacityTo >= " + capacityString;
                //SQLiteDataReader reader = CMD.ExecuteReader();
                SqlDataReader reader2 = CMD.ExecuteReader();

                if (!reader2.HasRows)
                {
                    _comments.Insert(0, "указана не существующая комбинация параметров конденсатора; ");
                    _recognizeSucceed = false;
                    reader2.Close();
                    reader.Close();
                    Const.CONN.Close();
                    return;
                }
                Console.WriteLine("Запрос 2 доп пройден");
                reader2.Close();
            }
            Console.WriteLine("Запрос 2 пройден");
            reader.Close();
        }

        private void CheckPriceInDB3()
        {

            //SQLiteCommand CMD = Const.CONN.CreateCommand();
            SqlCommand CMD = Const.CONN.CreateCommand();
            CMD.CommandText = "SELECT * FROM " + nameOfTable + " WHERE Type = '" + type + "' AND ContPads = '" + contPads + "' AND TCE LIKE '%" + TCE + "%' AND Voltage = " + voltage
                + " AND Size = " + size + " AND CapacityFrom <= " + capacityString + " AND CapacityTo >= " + capacityString;
            //SQLiteDataReader reader = CMD.ExecuteReader();
            SqlDataReader reader = CMD.ExecuteReader();

            if (tolerance == 0.25d)
            {
                while (reader.Read())
                {
                    _basePrice = (double)reader["Tolerance_025"];
                }
            }

            if (tolerance == 0.5d)
            {
                while (reader.Read())
                {
                    _basePrice = (double)reader["Tolerance_05"];
                }
            }

            if (tolerance == 1d)
            {
                while (reader.Read())
                {
                    _basePrice = (double)reader["Tolerance_1"];
                }
            }

            if (tolerance == 5d)
            {
                while (reader.Read())
                {
                    _basePrice = (double)reader["Tolerance_5"];
                }
            }

            if (tolerance == 10d)
            {
                while (reader.Read())
                {
                    _basePrice = (double)reader["Tolerance_10"];
                }
            }

            if (tolerance == 20d)
            {
                while (reader.Read())
                {
                    _basePrice = (double)reader["Tolerance_20"];
                }
            }

            if (tolerance == 50d)
            {
                while (reader.Read())
                {
                    _basePrice = (double)reader["Tolerance_50"];
                }
            }

            if (tolerance == 80d)
            {
                while (reader.Read())
                {
                    _basePrice = (double)reader["Tolerance_80"];
                }
            }

            if (_basePrice == 0)
            {
                _comments.Insert(0, "Конденсаторы с указанными параметрами не существуют; ");
                _recognizeSucceed = false;
            }

            reader.Close();
            Const.CONN.Close();
        }

        private void GetRealPNandPrice()
        {
            Console.WriteLine("Final Partnumber: " + partnumber);

            _formula.Insert(0, "RC[-1]");
            if (_priemka == Const.PRIEMKA_OCM)
            {
                _realPartnumber.Insert(0, "ОС " + type);
                _formula.Insert(_formula.Length, "*1.3");
            }
            if (TCE == "МП0")
            {
                _realPartnumber.Append(type + "-" + voltage + "В-" + TCE + "-" + capacity + capacityGrade + "-" + toleranceForPN);
            } 
            else _realPartnumber.Append(type + "-" + TCE + "-" + capacity + capacityGrade + "-" + toleranceForPN);

            if (size != 0) _realPartnumber.Append("-" + size);

            if (type == "К10-17В")
            {
                if (contPads == "N") _realPartnumber.Append("N");
                else _realPartnumber.Append("нелуженые");
            }

            if (_quantity < 50)
            {
                _formula.Insert(_formula.Length, "*2");
                _comments.Insert(0, "Коэфф. на кол-во менее 50шт - х2 ");
            }

            if (partnumber.Contains("-A") || partnumber.Contains("-А"))
            {
                _formula.Insert(_formula.Length, "*1.1");
                _realPartnumber.Append("-A");
            }

            _formula.Insert(_formula.Length, "*1.18");

            if (type == "К10-17А" || type == "К10-17Б")
            {
                _realPartnumber.Append("-В");
            }

            char[] partnumberRemnants = partnumber.ToString().ToCharArray();
            foreach (char symbol in partnumberRemnants)
            {
                if (Char.IsDigit(symbol))
                {
                    _comments.Insert(0, "Если партномер содержит дополнительные данные о размерах, то программа их НЕ проверяет, т.к. они не влияют на стоимость конденсатора; ");
                    break;
                }
            }
        }

        private void CheckSize()
        {
            string[] acceptedSize = { "-1", "-2", "-3", "-4", "-5", "-6", "-11", "-12", "-13", "-14", "-15", "-16", };

            foreach (string accepted in acceptedSize)
            {
                if (partnumber.Contains(accepted))
                {
                    size = Int32.Parse(accepted.Substring(1));
                }
            }
        }
    }
}
