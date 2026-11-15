using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace Delivery
{
    public class Conexao
    {

        private static readonly string stringConexao = "Server=localhost;Port=3306;Database=DB_Delivery;Uid=root;Pwd=CLSena@2302;";

        public static MySqlConnection ObterConexao()
        {
            return new MySqlConnection(stringConexao);
        }
    }

}
