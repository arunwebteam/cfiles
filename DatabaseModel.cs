using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace Tourism_CRM.Models
{
    public class DatabaseModel
    {
        SqlConnection con = null;
        SqlCommand cmd = null;
        SqlDataAdapter da = null;
        DataSet ds = null;
        DataTable dt = null;
        SqlTransaction sqlTran = null;
        string conString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString.ToString();
        int rowsAffected = 0;

        public void OpenConnection()
        {
            con = new SqlConnection(conString);
            if (con.State == ConnectionState.Open)
            {
                con.Close();
                con.Dispose();
            }
            con.Open();
        }

        public void CloseConnection()
        {
            if (con.State == ConnectionState.Open)
            {
                con.Close();
                con.Dispose();
            }
        }

        public DataTable ExecuteDataTable(string Proc, string TableName, string[,] Param)
        {
            try
            {
                da = new SqlDataAdapter();
                ds = new DataSet();
                OpenConnection();
                cmd = new SqlCommand(Proc, con);
                cmd.CommandType = CommandType.StoredProcedure;
                if (Param.Length != 0)
                {
                    for (int i = 0; i < (Param.Length / 2); i++)
                    {
                        cmd.Parameters.AddWithValue(Param[i, 0].ToString(), Param[i, 1].ToString());
                    }
                }
                da.SelectCommand = cmd;
                da.Fill(ds, "Table");
                if (ds.Tables != null && ds.Tables.Count > 0)
                {
                    ds.Tables[0].TableName = TableName;
                    dt = ds.Tables[0];
                    return dt;
                }
                else
                {
                    return dt = null;
                }
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
                da.Dispose();
                cmd.Dispose();
                CloseConnection();
            }
        }

        public int ExecuteCommand(string Proc, string[,] Param)
        {
            try
            {
                OpenConnection();
                sqlTran = con.BeginTransaction();
                cmd = new SqlCommand(Proc, con);
                cmd.Transaction = sqlTran;
                cmd.CommandType = CommandType.StoredProcedure;
                if (Param.Length != 0)
                {
                    for (int i = 0; i < (Param.Length / 2); i++)
                    {
                        cmd.Parameters.AddWithValue(Param[i, 0].ToString(), Param[i, 1].ToString());
                    }
                }
                rowsAffected = cmd.ExecuteNonQuery();
                sqlTran.Commit();
                return rowsAffected;
            }
            catch (SqlException ex)
            {
                sqlTran.Rollback();
                throw ex;
            }
            finally
            {
                cmd.Dispose();
                CloseConnection();
            }
        }

        public string ExecuteCommandReturn(string Proc, string[,] Param)
        {
            string returnValue = string.Empty;
            try
            {
                OpenConnection();
                sqlTran = con.BeginTransaction();
                cmd = new SqlCommand(Proc, con);
                cmd.Transaction = sqlTran;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@CID", SqlDbType.VarChar, 30);
                cmd.Parameters["@CID"].Direction = ParameterDirection.Output;
                if (Param.Length != 0)
                {
                    for (int i = 0; i < (Param.Length / 2); i++)
                    {
                        cmd.Parameters.AddWithValue(Param[i, 0].ToString(), Param[i, 1].ToString());
                    }
                }
                cmd.ExecuteNonQuery();
                string value = cmd.Parameters["@CID"].Value.ToString();
                sqlTran.Commit();
                return value;
            }
            catch (SqlException ex)
            {
                return "0";
            }
            finally
            {
                cmd.Dispose();
                CloseConnection();
            }
        }

        public string Encrypt(string encryptString)
        {
            string EncryptionKey = "CholanIT!23";
            byte[] clearBytes = Encoding.Unicode.GetBytes(encryptString);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    encryptString = Convert.ToBase64String(ms.ToArray());
                }
            }
            return encryptString;
        }

        public string Decrypt(string cipherText)
        {
            string EncryptionKey = "CholanIT!23";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }
}