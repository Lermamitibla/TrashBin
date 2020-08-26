using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Data.SqlClient;

namespace AutoProt
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "Inquiry" в коде и файле конфигурации.
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class Inquiry : IInquiry
    {
        public SimleItem CheckInquiry(string Partnumber, string Quantity, string Priemka, int Stringnumber, int ItemsQTY, string userName, string domain)
        {

            try
            {
                Const.CONN.Open();
            }
            catch (SqlException)
            {
                Console.WriteLine("Ошибка подключения к базе данных, запрос пользователя не обработан");
                return null;
            }
            SqlCommand cmd = Const.CONN.CreateCommand();
            cmd.CommandText = "SELECT * FROM Users WHERE UserName = '" + userName + "' AND Domain = '" + domain +
                "' AND AccessDate >= '" + DateTime.Now + "'";
            SqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                reader.Close();
                Const.CONN.Close();
                Console.WriteLine("Неавторизованный запрос от пользвателя {0}, домен {1}", userName, domain);
                return null;
            }
            reader.Close();
            Const.CONN.Close();

            Search check = new Search();
            SimleItem item = check.CheckManufacturer(Partnumber, Quantity, Priemka, Stringnumber);
            Console.WriteLine("Имеем " + item.realPartnumber + " по цене " + item.basePrice);
            return item;
            //Может ли айтем оказаться null?
        }

        public bool CheckRegStatus(string userName, string domain)
        {

            SqlConnection CONN = Const.CONN;
            CONN.Open();
            SqlCommand cmd = CONN.CreateCommand();
            cmd.CommandText = "SELECT * FROM Users WHERE UserName = '" + userName + "' AND Domain = '" + domain +
                "'";
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Close();
                CONN.Close();
                return true;
            }
            else
            {
                reader.Close();
                Console.WriteLine("Записываю в базу нового пользователя");
                cmd.CommandText = "INSERT INTO Users VALUES ('" + userName + "', '" + domain + "', NULL)";
                cmd.ExecuteNonQuery();
                reader.Close();
                CONN.Close();
                return false;
            }
            
        }
    }
}
