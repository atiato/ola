using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace TryApp.Models
{
    public class StoreContext
    {
        public string ConnectionString { get; set; }

        public StoreContext(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

       

        private SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public List<Store> GetAllAlbums()
        {
            List<Store> list = new List<Store>();

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from Items where id < 10", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Store()
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Item = reader["Item"].ToString(),
                            NoOfDays= reader["NoOfDays"].ToString()
                            //     Price = Convert.ToInt32(reader["Price"]),
                            //   Genre = reader["genre"].ToString()
                        });
                    }
                }
            }
            return list;
        }

    }

    
}