using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace TryApp.Models
{
    public class MusicStoreContext
    {
        public string ConnectionString { get; set; }

        public MusicStoreContext(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

       

        private SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public List<Album> GetAllAlbums()
        {
            List<Album> list = new List<Album>();

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from Items where id < 10", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Album()
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Item = reader["Item"].ToString()
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