using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;

namespace MyConexaoOracle
{
    public static class ConexaoOracle
    {
        private static OracleConnection myOracleConnection;
        private static OracleTransaction myOracleTransaction;
        private static OracleCommand myOracleCommand;
        private static string sBancoDados = "MEUBANCODADOS", 
                              sUsuario    = "MEUUSUARIO",
                              sSenha      = "MINHASENHA",
                              sParametrosConexao = String.Format("Data Source={0};User Id={1};Password={2};Integrated Security=no;",
                                                                 sBancoDados,
                                                                 sUsuario,
                                                                 sSenha);

        #region Funções da classe ConexaoOracle
        public static OracleConnection criarConexaoOracle
        {
            get
            {
                if (myOracleConnection == null)
                    myOracleConnection = new OracleConnection(sParametrosConexao);
                return myOracleConnection;
            }
        }
        public static bool testarConexaoOracle()
        {
            if (sParametrosConexao.Trim().Length == 0)
                return false;
            else
            {
                try
                {
                    criarConexaoOracle.Close();
                    criarConexaoOracle.Open();

                    return myOracleConnection.State == ConnectionState.Open;
                }
                catch
                {                    
                    return false;
                }
            }
        }
        public static bool verificarStatusConexao()
        {
            if (myOracleConnection == null)
                return false;

            return myOracleConnection.State == ConnectionState.Open;
        }
        public static void abrirConexaoOracle()
        {
            try
            {
                if (!ConexaoOracle.verificarStatusConexao())
                {
                    criarConexaoOracle.Close();
                    criarConexaoOracle.Open();
                }
            }
            catch
            {
                myOracleConnection = null;                
            }
        }
        public static OracleDataReader leitor(string sSQL, List<OracleParameter> parametros)
        {
            if (!verificarStatusConexao())
                abrirConexaoOracle();

            using (myOracleCommand = new OracleCommand
            {
                CommandType = CommandType.Text,
                Connection = myOracleConnection,
                CommandText = sSQL,
                Transaction = myOracleTransaction
            })
            {
                if (parametros != null)
                    foreach (OracleParameter parametro in parametros)
                        myOracleCommand.Parameters.AddWithValue(parametro.ParameterName, parametro.Value);
                return myOracleCommand.ExecuteReader();
            }
        }
        public static OracleDataReader leitor(string sSQL)
        {
            return ConexaoOracle.leitor(sSQL, null);
        }
        public static void executar(string sSQL, List<OracleParameter> parametros)
        {
            if (!verificarStatusConexao())
                abrirConexaoOracle();

            using (myOracleCommand = new OracleCommand
            {
                CommandType = CommandType.Text,
                Connection = myOracleConnection,
                CommandText = sSQL,
                Transaction = myOracleTransaction
            })
            {
                if (parametros != null)
                    foreach (OracleParameter parametro in parametros)
                        myOracleCommand.Parameters.AddWithValue(parametro.ParameterName, parametro.Value);

                myOracleCommand.ExecuteNonQuery();
            }
        }
        public static void fecharConexaoOracle()
        {
            try
            {
                if (verificarStatusConexao())
                    myOracleConnection.Close();
            }
            catch 
            {
                throw;
            }
            finally
            {
                myOracleConnection = null;
            }
        }
        public static void iniciarTransacao()
        {
            if (!verificarStatusConexao())
                abrirConexaoOracle();

            if (myOracleConnection == null)
                return;

            myOracleTransaction = myOracleConnection.BeginTransaction(IsolationLevel.ReadCommitted);
        }
        public static void efetivarTransacao()
        {
            if (myOracleTransaction != null)
                myOracleTransaction.Commit();
        }
        public static void desfazerTranscao()
        {
            if (myOracleTransaction != null)
                myOracleTransaction.Rollback();
            myOracleTransaction = null;
        }        
        public static void executar(string sSQL)
        {
            ConexaoOracle.executar(sSQL, null);
        }
        #endregion
    }
}
