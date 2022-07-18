using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BiancasBikeShop.Models;
using BiancasBikeShop.Utils;

namespace BiancasBikeShop.Repositories
{
    public class BikeRepository : IBikeRepository
    {
        private SqlConnection Connection
        {
            get
            {
                return new SqlConnection("server=localhost\\SQLExpress;database=BiancasBikeShop;integrated security=true;TrustServerCertificate=true");
            }
        }

        public List<Bike> GetAllBikes()
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT b.Id AS BikeId, b.Brand, b.Color, b.OwnerId, b.BikeTypeId,
                    o.Id AS OwnerId, o.Name AS OwnerName, o.Address, o.Email, o.Telephone,
                    bt.Id AS BikeTypeId, bt.Name AS BikeTypeName
                    FROM Bike b
                    JOIN Owner o ON b.OwnerId = o.Id
                    JOIN BikeType bt ON b.BikeTypeId = bt.Id
                    ";

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var bikes = new List<Bike>();
                        while (reader.Read())
                        {
                            bikes.Add(new Bike()
                            {
                                Id = DbUtils.GetInt(reader, "BikeId"),
                                Brand = DbUtils.GetString(reader, "Brand"),
                                Color = DbUtils.GetString(reader, "Color"),
                                Owner = new Owner()
                                {
                                    Id = DbUtils.GetInt(reader, "OwnerId"),
                                    Name = DbUtils.GetString(reader, "OwnerName"),
                                    Address = DbUtils.GetString(reader, "Address"),
                                    Email = DbUtils.GetString(reader, "Email"),
                                    Telephone = DbUtils.GetString(reader, "Telephone")
                                },
                                BikeType = new BikeType()
                                {
                                    Id = DbUtils.GetInt(reader, "BikeTypeId"),
                                    Name = DbUtils.GetString(reader, "BikeTypeName")
                                }
                            });
                        }
                        return bikes;
                    }
                }

            }
        }

        public Bike GetBikeById(int id)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT b.Id AS BikeId, b.Brand, b.Color, b.OwnerId, b.BikeTypeId,
                    o.Id AS OwnerId, o.Name AS OwnerName, o.Address, o.Email, o.Telephone,
                    bt.Id AS BikeTypeId, bt.Name AS BikeTypeName,
                    wo.Id AS WorkOrderId, wo.Description, wo.DateInitiated, wo.DateCompleted
                    FROM Bike b
                    JOIN Owner o ON b.OwnerId = o.Id
                    JOIN BikeType bt ON b.BikeTypeId = bt.Id
                    LEFT JOIN WorkOrder wo ON b.Id = wo.BikeId
                    WHERE b.Id = @Id
                    ";

                    DbUtils.AddParameter(cmd, "@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        Bike bike = null;
                        if (reader.Read())
                        {
                            bike = new Bike()
                            {
                                Id = DbUtils.GetInt(reader, "BikeId"),
                                Brand = DbUtils.GetString(reader, "Brand"),
                                Color = DbUtils.GetString(reader, "Color"),
                                Owner = new Owner()
                                {
                                    Id = DbUtils.GetInt(reader, "OwnerId"),
                                    Name = DbUtils.GetString(reader, "OwnerName"),
                                    Address = DbUtils.GetString(reader, "Address"),
                                    Email = DbUtils.GetString(reader, "Email"),
                                    Telephone = DbUtils.GetString(reader, "Telephone")
                                },
                                BikeType = new BikeType()
                                {
                                    Id = DbUtils.GetInt(reader, "BikeTypeId"),
                                    Name = DbUtils.GetString(reader, "BikeTypeName")
                                },
                                WorkOrders = new List<WorkOrder>()
                            };

                            if (DbUtils.IsNotDbNull(reader, "WorkOrderId"))
                            {
                                bike.WorkOrders.Add(new WorkOrder()
                                {
                                    Id = DbUtils.GetInt(reader, "WorkOrderId"),
                                    Description = DbUtils.GetString(reader, "Description"),
                                    DateInitiated = DbUtils.GetDateTime(reader, "DateInitiated"),
                                    DateCompleted = DbUtils.GetNullableDateTime(reader, "DateCompleted"),
                                });
                            }

                        }
                        return bike;
                    }
                }
            }
        }

        public int GetBikesInShopCount()
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT COUNT (b.Id) AS BikeCount
                    FROM Bike b
                    LEFT JOIN WorkOrder wo ON b.Id = wo.BikeId
                    WHERE wo.DateCompleted IS NULL
                    ";

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        int count = 0;

                        if (reader.Read())
                        {
                            count = DbUtils.GetInt(reader, "BikeCount");
                        }

                        return count;
                    }
                }

            }
        }
    }
}
