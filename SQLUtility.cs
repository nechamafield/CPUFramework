﻿using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace CPUFramework
{
    public class SQLUtility
    {
        public static string ConnectionString = "";

        public static SqlCommand GetSQLCommand(string sprocname)
        {
            SqlCommand cmd;
            using (SqlConnection conn = new SqlConnection(SQLUtility.ConnectionString))
            {
                cmd = new SqlCommand(sprocname, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                SqlCommandBuilder.DeriveParameters(cmd);
            }
            return cmd;
        }

        public static DataTable GetDataTable(SqlCommand cmd)
        {
            return DoExecuteSql(cmd, true);
        }

        private static DataTable DoExecuteSql (SqlCommand cmd, bool loadtable)
        {

            DataTable dt = new();
            using (SqlConnection conn = new SqlConnection(SQLUtility.ConnectionString))
            {
                conn.Open();
                cmd.Connection = conn;
                Debug.Print(GetSQL(cmd));
                try
                {
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (loadtable == true)
                    {
                        dt.Load(dr);
                    }
                }
                catch (SqlException ex)
                {
                    string msg = ParseConstraintMsg(ex.Message);
                    throw new Exception(msg);
                }
                catch (InvalidCastException ex)
                {
                    throw new Exception(cmd.CommandText + ": " + ex.Message, ex);
                }
            }
            SetAllColumnsAllowNull(dt);
            return dt;
        }

        public static DataTable GetDataTable(string sqlstatement) //take a sql statement and return a datatable
        {

            return DoExecuteSql(new SqlCommand(sqlstatement), true);
        }

        public static void ExecuteSQL(SqlCommand cmd)
        {
            DoExecuteSql(cmd, false);
        }

        public static void ExecuteSQL(string sqlstatement)
        {
            GetDataTable(sqlstatement);
        }

        public static void SetParameterValue(SqlCommand cmd, string paramname, object value)
        {
            try
            {
                cmd.Parameters[paramname].Value = value;
            }
            catch(Exception ex)
            {
                throw new Exception(cmd.CommandText + ": " + ex.Message, ex);
            }
        }

        private static string ParseConstraintMsg(string msg)
        {
            string origmsg = msg;
            string prefix = "ck_";
            string msgend = "";
            if (msg.Contains(prefix) == false)
            {
                if (msg.Contains("u_"))
                {
                    prefix = "u_";
                    msgend = "must be unique";
                }
                else if (msg.Contains("f_"))
                {
                    prefix = "f_";
                }
            }
                if (msg.Contains(prefix))
                {
                msg = msg.Replace("\"", "'");
                    int position = msg.IndexOf(prefix) + prefix.Length;
                    msg = msg.Substring(position);
                    position = msg.IndexOf("'");
                    if (position == -1)
                    {
                        msg = origmsg;
                    }
                    else
                    {
                        msg = msg.Substring(0, position);
                        msg = msg.Replace("_", " ");
                        msg = msg + msgend;

                        if(prefix == "f_")
                    {
                        var words = msg.Split(" ");
                        if(words.Length > 1)
                        {
                            msg = $"Cannot delete {words[0]} because it has a related {words[1]} record.";
                        }
                    }
                    }
                }
                return msg;
            }

            public static int GetFirstColumnFirstRowValue(string sql)
            {
                int n = 0;

                DataTable dt = GetDataTable(sql);
                if (dt.Rows.Count > 0 && dt.Columns.Count > 0)
                {
                    if (dt.Rows[0][0] != DBNull.Value)
                    {
                        int.TryParse(dt.Rows[0][0].ToString(), out n);
                    }
                }
                return n;
            }

            private static void SetAllColumnsAllowNull(DataTable dt)
            {
                foreach (DataColumn c in dt.Columns)
                {
                    c.AllowDBNull = true;
                }
            }

            public static string GetSQL(SqlCommand cmd)
            {
                string val = "";
#if DEBUG
                StringBuilder sb = new StringBuilder();
                if (cmd.Connection != null)
                {
                    sb.AppendLine($"--{cmd.Connection.DataSource}");
                    sb.AppendLine($"use {cmd.Connection.Database}");
                    sb.AppendLine("go");
                }
                if (cmd.CommandType == CommandType.StoredProcedure)
                {
                    sb.AppendLine($"exec {cmd.CommandText}");
                    int paramcount = cmd.Parameters.Count - 1;
                    int paramnum = 0;
                    string comma = ",";
                    foreach (SqlParameter p in cmd.Parameters)
                    {
                        if (p.Direction != ParameterDirection.ReturnValue)
                        {
                            if (paramnum == paramcount)
                            {
                                comma = "";
                            }
                            sb.AppendLine($"{p.ParameterName} = {(p.Value == null ? "null" : p.Value.ToString())} {comma}");
                        }
                        paramnum++;
                    }
                }
                else
                {
                    sb.AppendLine(cmd.CommandText);
                }
                val = sb.ToString();
#endif
                return val;
            }

            public static void DebugPrintDataTable(DataTable dt)
            {
                foreach (DataRow r in dt.Rows)
                {
                    foreach (DataColumn c in dt.Columns)
                    {
                        Debug.Print(c.ColumnName + " = " + r[c.ColumnName].ToString());
                    }
                }
            }
        }
    }
