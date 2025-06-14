using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataNexus
{
    public class MySQLEngine
    {
        public string port { get; set; }
        public string user { get; set; }
        public string password { get; set; }

        /// <summary>
        /// _port:3306<para/>
        /// _user:root<para/>
        /// _password:Asher19910930
        /// </summary>
        public MySQLEngine(string _port, string _user, string _password)
        {
            port = _port;
            user = _user;
            password = _password;
        }

        private string StrPack()
        {
            return "server=127.0.0.1;" + nameof(port) + "=" + port + ";" + nameof(user) + "=" + user + ";" + nameof(password) + "=" + password + ";";
        }

        #region Show Database Information
        public void ShowDatabase()
        {
            string connetStr = StrPack();
            MySqlConnection conn = new MySqlConnection(connetStr);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("show databases;", conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("Databases:");
                    while (reader.Read())
                    {
                        string databaseName = reader.GetString(0);
                        Console.WriteLine(databaseName);
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        public void ShowTable(string database_name)
        {
            string connetStr = StrPack();
            MySqlConnection conn = new MySqlConnection(connetStr);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("use " + database_name + ";" + "show tables;", conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("Table:");
                    while (reader.Read())
                    {
                        string databaseName = reader.GetString(0);
                        Console.WriteLine(databaseName);
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }
        #endregion

        #region Database Operation
        public string CreateDatabase(string database_name)
        {
            return "create database " + database_name + ";";
        }

        public string DropDatabase(string database_name)
        {
            return "drop database " + database_name + ";";
        }

        public void MySQLDatabase(string database_name, Func<string, string> fun)
        {
            string connetStr = StrPack();
            MySqlConnection conn = new MySqlConnection(connetStr);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(fun(database_name), conn);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }

        }
        #endregion

        #region Table Operation
        public string CreateTable(string database_name, string table_name, string content)
        {
            return "use " + database_name + ";" +
                   "create table " + table_name + "(" + content + ");";
        }

        public string AddTableColumn(string database_name, string table_name, string content)
        {
            return "use " + database_name + ";" +
                   "alter table " + table_name + "add" + "(" + content + ");";
        }

        public string DeleteTableColumn(string database_name, string table_name, string column)
        {
            return "use " + database_name + ";" +
                   "alter table " + table_name + "drop" + column + ";";
        }

        public string AddTableData(string database_name, string table_name, string data)
        {
            return "use " + database_name + ";" +
                   "insert into " + table_name + "values" + "(" + data + ");";
        }

        public string DeleteTableData(string database_name, string table_name, string condition)
        {
            return "use " + database_name + ";" +
                   "delete from " + table_name + "where" + condition + ";";
        }

        public void MySQLTable(string database_name, string table_name, string content, Func<string, string, string, string> fun)
        {
            string connetStr = StrPack();
            MySqlConnection conn = new MySqlConnection(connetStr);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(fun(database_name, table_name, content), conn);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }

        }

        public string DropTable(string database_name, string table_name)
        {
            return "use " + database_name + ";" +
                   "drop table " + table_name + ";";
        }

        public void MySQLTable(string database_name, string table_name, Func<string, string, string> fun)
        {
            string connetStr = StrPack();
            MySqlConnection conn = new MySqlConnection(connetStr);
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(fun(database_name, table_name), conn);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }

        }
        #endregion



    }
}
