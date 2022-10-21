﻿using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;


/// <summary>
/// Common 的摘要描述
/// </summary>
public partial class Common : System.Web.UI.Page
{
    public static string DBConnStr = System.Configuration.ConfigurationManager.ConnectionStrings["DBConnStr"].ConnectionString;
    public static string WebRedisConnStr = System.Configuration.ConfigurationManager.AppSettings["WebRedisConnStr"];
    public static dynamic EPaySetting;
 
    public Common()
    {
        //
        // TODO: 在這裡新增建構函式邏輯
        //
    }

    public static dynamic ParseData(string PostBody)
    {
        dynamic BodyObj = null;
        try { BodyObj = Newtonsoft.Json.JsonConvert.DeserializeObject(PostBody); } catch (Exception ex) { }
        return BodyObj;
    }

    public static bool CheckProviderListSign(string CompanyCode,string Sign,string Timestamp)
    {
        bool checkbool = false;
        string CompanyKey;
        CompanyKey = GetCompanyKeyByCompanyCode(CompanyCode);
        string signStr = "CompanyCode=" + CompanyCode;
        signStr += "&CompanyKey=" + CompanyKey;
        signStr += "&Timestamp=" + Timestamp;
        string _Sign = GetMD5(signStr, false);

        if (_Sign.ToUpper() == Sign.ToUpper())
        {
            checkbool = true;
        }
        return checkbool;
    }

    public static bool CheckPaymentSign(string CompanyCode,string OrderID,string PaymentType, string Sign,string Timestamp)
    {
        bool checkbool = false;
        string CompanyKey;

        CompanyKey = GetCompanyKeyByCompanyCode(CompanyCode);
        string signStr = "CompanyCode=" + CompanyCode;
        signStr += "&OrderID=" +OrderID;
        signStr += "&PaymentType=" + PaymentType;
        signStr += "&Timestamp=" + Timestamp;
        signStr += "&CompanyKey=" + CompanyKey;

        string _Sign = GetMD5(signStr, false);

        if (_Sign.ToUpper() == Sign.ToUpper())
        {
            checkbool = true;
        }
        return checkbool;
    }

    public static bool CheckWithdrawReviewSign(string CompanyCode, string OrderID, string Sign, string Timestamp)
    {
        bool checkbool = false;
        string CompanyKey;

        CompanyKey = GetCompanyKeyByCompanyCode(CompanyCode);
        string signStr = "CompanyCode=" + CompanyCode;
        signStr += "&OrderID=" + OrderID;
        signStr += "&Timestamp=" + Timestamp;
        signStr += "&CompanyKey=" + CompanyKey;

        string _Sign = GetMD5(signStr, false);

        if (_Sign.ToUpper() == Sign.ToUpper())
        {
            checkbool = true;
        }
        return checkbool;
    }

    public static bool CheckTimestamp(long Timestamp)
    {
        bool checkbool = false;
        DateTime NowTime = DateTime.UtcNow;
        DateTime CheckTime= (new DateTime(1970, 1, 1)).AddSeconds(Convert.ToInt32(Timestamp));
        double DiffMinutes = ExecDateDiff(NowTime,CheckTime);
        if (DiffMinutes<=5)
        {
            checkbool = true;
        }
        return checkbool;
    }

    public static double ExecDateDiff(DateTime dateBegin, DateTime dateEnd)
    {
        TimeSpan ts1 = new TimeSpan(dateBegin.Ticks);
        TimeSpan ts2 = new TimeSpan(dateEnd.Ticks);
        TimeSpan ts3 = ts1.Subtract(ts2).Duration();
        return ts3.TotalMinutes;
    }

    private static string GetCompanyKeyByCompanyCode(string CompanyCode)
    {
        string ret = null;
        string SS;
        System.Data.SqlClient.SqlCommand DBCmd;
        PaymentReport returnValue = null;
        DataTable DT;
        DBCmd = new System.Data.SqlClient.SqlCommand();
        object DBreturn;
        SS = " SELECT CompanyKey From CompanyTable Where CompanyCode=@CompanyCode ";

        DBCmd = new SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = System.Data.CommandType.Text;
        DBCmd.Parameters.Add("@CompanyCode", SqlDbType.VarChar).Value = CompanyCode;
        DBreturn = DBAccess.GetDBValue(DBConnStr, DBCmd);
        if (DBreturn != null)
        {
            ret = Convert.ToString(DBreturn);
        }

        return ret;
    }

    public static int GetCompanyKeyByCompanyID(string CompanyCode)
    {
        int ret = -1;
        string SS;
        System.Data.SqlClient.SqlCommand DBCmd;
        PaymentReport returnValue = null;
        DataTable DT;
        DBCmd = new System.Data.SqlClient.SqlCommand();
        object DBreturn;
        SS = " SELECT CompanyID From CompanyTable Where CompanyCode=@CompanyCode ";

        DBCmd = new SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = System.Data.CommandType.Text;
        DBCmd.Parameters.Add("@CompanyCode", SqlDbType.VarChar).Value = CompanyCode;
        DBreturn = DBAccess.GetDBValue(DBConnStr, DBCmd);
        if (DBreturn != null)
        {
            ret = Convert.ToInt32(DBreturn);
        }

        return ret;
    }

    public static Withdrawal GetWithdrawalByOrderID(string OrderID,int CompanyID)
    {
        string SS;
        System.Data.SqlClient.SqlCommand DBCmd;
        Withdrawal returnValue = null;
        DataTable DT;
        DBCmd = new System.Data.SqlClient.SqlCommand();

        SS = "";
        SS += " SELECT ServiceTypeName,ProviderCode.DecimalPlaces,ProviderCode.WithdrawRate,ProviderName,convert(varchar,Withdrawal.CreateDate,120) as CreateDate2,convert(varchar,Withdrawal.FinishDate,120) as FinishDate2,";
        SS += " Withdrawal.Status,Withdrawal.WithdrawSerial,Withdrawal.DownOrderID,";
        SS += " Withdrawal.WithdrawType,Withdrawal.BankCard,Withdrawal.BankCardName,Withdrawal.CurrencyType,";
        SS += " Withdrawal.BankName,Withdrawal.Amount,Withdrawal.OwnProvince,Withdrawal.BankBranchName,";
        SS += " Withdrawal.CollectCharge,Withdrawal.FloatType,Withdrawal.forCompanyID,";
        SS += " Withdrawal.OwnCity,Withdrawal.FinishAmount,Withdrawal.DownStatus";
        SS += " ,CompanyName FROM Withdrawal WITH (NOLOCK) ";
        SS += " LEFT JOIN ProviderCode WITH (NOLOCK) ON ProviderCode.ProviderCode=Withdrawal.ProviderCode";
        SS += " LEFT JOIN CompanyTable WITH (NOLOCK) ON CompanyTable.CompanyID=Withdrawal.forCompanyID";
        SS += " LEFT JOIN ServiceType WITH (NOLOCK) ON Withdrawal.ServiceType=ServiceType.ServiceType";
        SS += " WHERE  Withdrawal.DownOrderID= @DownOrderID And Withdrawal.forCompanyID=@CompanyID ";
        DBCmd = new SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = System.Data.CommandType.Text;
        DBCmd.Parameters.Add("@DownOrderID", SqlDbType.VarChar).Value = OrderID;
        DBCmd.Parameters.Add("@CompanyID", SqlDbType.VarChar).Value = CompanyID;
        DT = DBAccess.GetDB(DBConnStr, DBCmd);

        if (DT != null)
        {
            if (DT.Rows.Count > 0)
            {
                returnValue = new Withdrawal();
                returnValue.WithdrawSerial = (string)DT.Rows[0]["WithdrawSerial"];
                returnValue.DownOrderID = DT.Rows[0]["DownOrderID"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["DownOrderID"];
                returnValue.Status = (int)DT.Rows[0]["Status"];
                returnValue.WithdrawRate = DT.Rows[0]["WithdrawRate"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[0]["WithdrawRate"];
                returnValue.DecimalPlaces = DT.Rows[0]["DecimalPlaces"] == System.DBNull.Value ? 0 : (int)DT.Rows[0]["DecimalPlaces"];
                returnValue.ProviderName = DT.Rows[0]["ProviderName"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["ProviderName"];
                returnValue.CreateDate2 = DT.Rows[0]["CreateDate2"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["CreateDate2"];
                returnValue.FinishDate2 = DT.Rows[0]["FinishDate2"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["FinishDate2"];
                returnValue.WithdrawType = DT.Rows[0]["WithdrawType"] == System.DBNull.Value ? 0 : (int)DT.Rows[0]["WithdrawType"];
                returnValue.BankCard = DT.Rows[0]["BankCard"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["BankCard"];
                returnValue.forCompanyID= DT.Rows[0]["forCompanyID"] == System.DBNull.Value ? 0 : (int)DT.Rows[0]["forCompanyID"];
                returnValue.BankCardName = DT.Rows[0]["BankCardName"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["BankCardName"];
                returnValue.CurrencyType = DT.Rows[0]["CurrencyType"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["CurrencyType"];
                returnValue.BankName = DT.Rows[0]["BankName"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["BankName"];
                returnValue.Amount = DT.Rows[0]["Amount"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[0]["Amount"];
                returnValue.OwnProvince = DT.Rows[0]["OwnProvince"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["OwnProvince"];
                returnValue.BankBranchName = DT.Rows[0]["BankBranchName"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["BankBranchName"];
                returnValue.CollectCharge = DT.Rows[0]["CollectCharge"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[0]["CollectCharge"];
                returnValue.FloatType = DT.Rows[0]["FloatType"] == System.DBNull.Value ? 0 : (int)DT.Rows[0]["FloatType"];
                returnValue.OwnCity = DT.Rows[0]["OwnCity"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["OwnCity"];
                returnValue.FinishAmount = DT.Rows[0]["FinishAmount"] == System.DBNull.Value ? 0: (decimal)DT.Rows[0]["FinishAmount"];
                returnValue.DownStatus = DT.Rows[0]["DownStatus"] == System.DBNull.Value ? 0 : (int)DT.Rows[0]["DownStatus"]; 
                returnValue.CompanyName = DT.Rows[0]["CompanyName"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["CompanyName"];
                returnValue.ServiceTypeName = DT.Rows[0]["ServiceTypeName"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["ServiceTypeName"];
            }
        }

        return returnValue;
    }

    public static Withdrawal GetWithdrawalByWithdrawSerial(string WithdrawSerial)
    {
        string SS;
        System.Data.SqlClient.SqlCommand DBCmd;
        Withdrawal returnValue = null;
        DataTable DT;
        DBCmd = new System.Data.SqlClient.SqlCommand();

        SS = "";
        SS += " SELECT ProviderCode.DecimalPlaces,ProviderCode.WithdrawRate,ProviderName,convert(varchar,Withdrawal.CreateDate,120) as CreateDate2,convert(varchar,Withdrawal.FinishDate,120) as FinishDate2,";
        SS += " Withdrawal.Status,Withdrawal.WithdrawSerial,Withdrawal.DownOrderID,";
        SS += " Withdrawal.WithdrawType,Withdrawal.BankCard,Withdrawal.BankCardName,Withdrawal.CurrencyType,";
        SS += " Withdrawal.BankName,Withdrawal.Amount,Withdrawal.OwnProvince,Withdrawal.BankBranchName,";
        SS += " Withdrawal.CollectCharge,Withdrawal.FloatType,Withdrawal.forCompanyID,";
        SS += " Withdrawal.OwnCity,Withdrawal.FinishAmount,Withdrawal.DownStatus";
        SS += " ,CompanyName FROM Withdrawal WITH (NOLOCK) ";
        SS += " LEFT JOIN ProviderCode WITH (NOLOCK) ON ProviderCode.ProviderCode=Withdrawal.ProviderCode";
        SS += " LEFT JOIN CompanyTable WITH (NOLOCK) ON CompanyTable.CompanyID=Withdrawal.forCompanyID";
        SS += " WHERE  Withdrawal.WithdrawSerial= @WithdrawSerial ";
        DBCmd = new SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = System.Data.CommandType.Text;
        DBCmd.Parameters.Add("@WithdrawSerial", SqlDbType.VarChar).Value = WithdrawSerial;
  
        DT = DBAccess.GetDB(DBConnStr, DBCmd);

        if (DT != null)
        {
            if (DT.Rows.Count > 0)
            {
                returnValue = new Withdrawal();
                returnValue.WithdrawSerial = (string)DT.Rows[0]["WithdrawSerial"];
                returnValue.DownOrderID = DT.Rows[0]["DownOrderID"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["DownOrderID"];
                returnValue.Status = (int)DT.Rows[0]["Status"];
                returnValue.WithdrawRate = DT.Rows[0]["WithdrawRate"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[0]["WithdrawRate"];
                returnValue.DecimalPlaces = DT.Rows[0]["DecimalPlaces"] == System.DBNull.Value ? 0 : (int)DT.Rows[0]["DecimalPlaces"];
                returnValue.ProviderName = DT.Rows[0]["ProviderName"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["ProviderName"];
                returnValue.CreateDate2 = DT.Rows[0]["CreateDate2"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["CreateDate2"];
                returnValue.FinishDate2 = DT.Rows[0]["FinishDate2"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["FinishDate2"];
                returnValue.WithdrawType = DT.Rows[0]["WithdrawType"] == System.DBNull.Value ? 0 : (int)DT.Rows[0]["WithdrawType"];
                returnValue.BankCard = DT.Rows[0]["BankCard"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["BankCard"];
                returnValue.forCompanyID = DT.Rows[0]["forCompanyID"] == System.DBNull.Value ? 0 : (int)DT.Rows[0]["forCompanyID"];
                returnValue.BankCardName = DT.Rows[0]["BankCardName"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["BankCardName"];
                returnValue.CurrencyType = DT.Rows[0]["CurrencyType"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["CurrencyType"];
                returnValue.BankName = DT.Rows[0]["BankName"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["BankName"];
                returnValue.Amount = DT.Rows[0]["Amount"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[0]["Amount"];
                returnValue.OwnProvince = DT.Rows[0]["OwnProvince"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["OwnProvince"];
                returnValue.BankBranchName = DT.Rows[0]["BankBranchName"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["BankBranchName"];
                returnValue.CollectCharge = DT.Rows[0]["CollectCharge"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[0]["CollectCharge"];
                returnValue.FloatType = DT.Rows[0]["FloatType"] == System.DBNull.Value ? 0 : (int)DT.Rows[0]["FloatType"];
                returnValue.OwnCity = DT.Rows[0]["OwnCity"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["OwnCity"];
                returnValue.FinishAmount = DT.Rows[0]["FinishAmount"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[0]["FinishAmount"];
                returnValue.DownStatus = DT.Rows[0]["DownStatus"] == System.DBNull.Value ? 0 : (int)DT.Rows[0]["DownStatus"];
                returnValue.CompanyName = DT.Rows[0]["CompanyName"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["CompanyName"];

            }
        }

        return returnValue;
    }

    public static List<ProviderListResult> GetProviderListResult(int CompanyID)
    {
        List<ProviderListResult> returnValue = null;
        string SS;
        SqlCommand DBCmd = null;
        DataTable DT;

        SS = " SELECT PC.ProviderCode, " +
                 "        PC.ProviderName, " +
                 "        PC.ProviderAPIType," +
                 "        PC.ProviderState," +
                 "        WL.MaxLimit, " +
                 "        WL.MinLimit, " +
                 "        WL.Charge, " +
                 "        WL.CurrencyType" +
                 " FROM   ProviderCode PC " +
                 "        LEFT JOIN WithdrawLimit WL " +
                 "               ON WL.ProviderCode = PC.ProviderCode " +
                 "                  AND WL.WithdrawLimitType = 0 " +
                 " WHERE PC.forCompanyID=@CompanyID "+
                 " ORDER BY PC.ProviderState";
        DBCmd = new SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = System.Data.CommandType.Text;
        DBCmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = CompanyID;
        DT = DBAccess.GetDB(DBConnStr, DBCmd);

        if (DT != null)
        {
            if (DT.Rows.Count > 0)
            {
                returnValue = new List<ProviderListResult>();
             
                for (int i = 0; i < DT.Rows.Count; i++)
                {
                    var _ProviderListResult = new ProviderListResult();
                    _ProviderListResult.ProviderCode = DT.Rows[i]["ProviderCode"] == System.DBNull.Value ? "" : (string)DT.Rows[i]["ProviderCode"];
                    _ProviderListResult.ProviderName = DT.Rows[i]["ProviderName"] == System.DBNull.Value ? "" : (string)DT.Rows[i]["ProviderName"];
                    _ProviderListResult.ProviderAPIType = DT.Rows[i]["ProviderAPIType"] == System.DBNull.Value ? 0 : (int)DT.Rows[i]["ProviderAPIType"];
                    _ProviderListResult.ProviderState = DT.Rows[i]["ProviderState"] == System.DBNull.Value ? 0 : (int)DT.Rows[i]["ProviderState"];
                    _ProviderListResult.MaxLimit = DT.Rows[i]["MaxLimit"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["MaxLimit"];
                    _ProviderListResult.MinLimit = DT.Rows[i]["MinLimit"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["MinLimit"];
                    _ProviderListResult.Charge = DT.Rows[i]["Charge"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["Charge"];
                    _ProviderListResult.CurrencyType = DT.Rows[i]["CurrencyType"] == System.DBNull.Value ? "" : (string)DT.Rows[i]["CurrencyType"];
                    returnValue.Add(_ProviderListResult);
                }
            }
        }

        return returnValue;
    }

    public static List<ServiceData> GetProviderListServiceData(int CompanyID)
    {
        List<ServiceData> returnValue = null;
        string SS;
        SqlCommand DBCmd = null;
        DataTable DT;

        SS = " SELECT ST.ServiceTypeName, " +
                 "        PS.MaxDaliyAmount, " +
                 "        PS.MaxOnceAmount, " +
                 "        PS.MinOnceAmount, " +
                 "        PS.CostCharge, " +
                 "        PS.CostRate, " +
                 "        PS.ServiceType, " +
                 "        PS.ProviderCode, " +
                 "        PS.CurrencyType, " +
                 "        PS.State," +
                 "        PS.CheckoutType" +
                 " FROM   ProviderService PS" +
                 " 	JOIN ProviderCode PC ON PC.ProviderCode=PS.ProviderCode "+
                 "        LEFT JOIN ServiceType ST" +
                 "               ON ST.ServiceType = PS.ServiceType " +
                 " WHERE  PC.forCompanyID=@CompanyID ";
   
        DBCmd = new SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = System.Data.CommandType.Text;
        DBCmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = CompanyID;
        DT = DBAccess.GetDB(DBConnStr, DBCmd);

        if (DT != null)
        {
            if (DT.Rows.Count > 0)
            {
                returnValue = new List<ServiceData>();
      
                for (int i = 0; i < DT.Rows.Count; i++)
                {
                    ServiceData _ServiceData = new ServiceData();
                    _ServiceData.ServiceTypeName = DT.Rows[i]["ServiceTypeName"] == System.DBNull.Value ? "" : (string)DT.Rows[i]["ServiceTypeName"];
                    _ServiceData.MaxDaliyAmount = DT.Rows[i]["MaxDaliyAmount"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["MaxDaliyAmount"];
                    _ServiceData.MaxOnceAmount = DT.Rows[i]["MaxOnceAmount"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["MaxOnceAmount"];
                    _ServiceData.MinOnceAmount = DT.Rows[i]["MinOnceAmount"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["MinOnceAmount"];
                    _ServiceData.CostCharge = DT.Rows[i]["CostCharge"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["CostCharge"];
                    _ServiceData.CostRate = DT.Rows[i]["CostRate"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["CostRate"];
                    _ServiceData.ServiceType = DT.Rows[i]["ServiceType"] == System.DBNull.Value ? "" : (string)DT.Rows[i]["ServiceType"];
                    _ServiceData.ProviderCode = DT.Rows[i]["ProviderCode"] == System.DBNull.Value ? "" : (string)DT.Rows[i]["ProviderCode"];
                    _ServiceData.CurrencyType = DT.Rows[i]["CurrencyType"] == System.DBNull.Value ? "" : (string)DT.Rows[i]["CurrencyType"];
                    _ServiceData.State = DT.Rows[i]["State"] == System.DBNull.Value ? 0 : (int)DT.Rows[i]["State"];
                    _ServiceData.CheckoutType = DT.Rows[i]["CheckoutType"] == System.DBNull.Value ? 0 : (int)DT.Rows[i]["CheckoutType"];
                    returnValue.Add(_ServiceData);
                }
            }
        }

        return returnValue;
    }

    public static List<ProviderPointVM> GetAllProviderPoint(int CompanyID)
    {
        List<ProviderPointVM> returnValue = null;
        string SS;
        SqlCommand DBCmd = null;
        DataTable DT;


        SS = " SELECT PC.ProviderName,PC.ProviderAPIType,PC.ProviderCode, ISNULL(PP.TotalDepositePointValue,0) TotalDepositePointValue,ISNULL(PP.TotalProfitPointValue,0) TotalProfitPointValue, " +
             " (ISNULL(PP.SystemPointValue, 0) - ISNULL(FP.ProviderFrozenAmount, 0)) AS SystemPointValue, ISNULL(FP.ProviderFrozenAmount, 0) ProviderFrozenAmount, ISNULL(FMH.WProfit,0) WithdrawProfit, " +
             " (SELECT  ISNULL(SUM(W.Amount + W.CostCharge), 0) FROM Withdrawal W WITH (NOLOCK) " +
             " WHERE W.Status <> 2 AND W.Status <> 3 AND W.Status <> 8  AND W.Status <> 90 AND W.Status <> 91 " +
             " AND W.ProviderCode = PP.ProviderCode AND W.CurrencyType = PP.CurrencyType) AS WithdrawPoint " +
             " FROM ProviderCode PC " +
             " LEFT JOIN ProviderPoint PP ON PC.ProviderCode = PP.ProviderCode" +
             " LEFT JOIN(SELECT forProviderCode, SUM(ISNULL(ProviderFrozenAmount, 0)) ProviderFrozenAmount FROM FrozenPoint FP " +
             " WHERE  FP.Status = 0 " +
             " GROUP BY forProviderCode) FP ON FP.forProviderCode = PC.ProviderCode " +
             " LEFT JOIN (SELECT ProviderCode,SUM(ISNULL(Amount, 0)) WProfit FROM ProviderManualHistory FMH " +
             " WHERE FMH.Type = 2 " +
             " GROUP BY ProviderCode) FMH ON FMH.ProviderCode = PC.ProviderCode " +
             " WHERE PC.forCompanyID=@CompanyID ";

        DBCmd = new SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = System.Data.CommandType.Text;
        DBCmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = CompanyID;
        DT = DBAccess.GetDB(DBConnStr, DBCmd);
        if (DT != null)
        {
            if (DT.Rows.Count > 0)
            {
                returnValue = new List<ProviderPointVM>();
                for (int i = 0; i < DT.Rows.Count; i++)
                {
                    ProviderPointVM _ProviderPointVM = new ProviderPointVM();
                    _ProviderPointVM.ProviderName = DT.Rows[i]["ProviderName"] == System.DBNull.Value ? "" : (string)DT.Rows[i]["ProviderName"];
                    _ProviderPointVM.ProviderAPIType = DT.Rows[i]["ProviderAPIType"] == System.DBNull.Value ? 0 : (int)DT.Rows[i]["ProviderAPIType"];
                    _ProviderPointVM.ProviderCode = DT.Rows[i]["ProviderCode"] == System.DBNull.Value ? "" : (string)DT.Rows[i]["ProviderCode"];
                    _ProviderPointVM.TotalDepositePointValue = DT.Rows[i]["TotalDepositePointValue"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["TotalDepositePointValue"];
                    _ProviderPointVM.TotalProfitPointValue = DT.Rows[i]["TotalProfitPointValue"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["TotalProfitPointValue"];
                    _ProviderPointVM.SystemPointValue = DT.Rows[i]["SystemPointValue"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["SystemPointValue"];
                    _ProviderPointVM.ProviderFrozenAmount = DT.Rows[i]["ProviderFrozenAmount"] == System.DBNull.Value ?0 : (decimal)DT.Rows[i]["ProviderFrozenAmount"];
                
                    _ProviderPointVM.WithdrawPoint = DT.Rows[i]["WithdrawPoint"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["WithdrawPoint"];

                    _ProviderPointVM.WithdrawProfit = DT.Rows[i]["WithdrawProfit"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["WithdrawProfit"];
                    returnValue.Add(_ProviderPointVM);
                }
            }
        }

        return returnValue;
    }

    public static PaymentReport GetPaymentByOrderID(string OrderID)
    {
        string SS;
        System.Data.SqlClient.SqlCommand DBCmd;
        PaymentReport returnValue = null;
        DataTable DT;
        DBCmd = new System.Data.SqlClient.SqlCommand();

        SS = "";
        SS += " SELECT P.* ,  ";
        SS += "       S.ServiceTypeName, ";
        SS += "       PC.ProviderName, ";
        SS += "       C.CompanyName, ";
        SS += "       C.CompanyCode, ";
        SS += "       C.MerchantCode, ";
        SS += "       convert(varchar, P.CreateDate, 120) as CreateDate2, ";
        SS += "       convert(varchar, P.OrderDate, 120) as OrderDate2, ";
        SS += "       convert(varchar, P.FinishDate, 120) as FinishDate2 ";
        SS += " FROM PaymentTable AS P ";
        SS += " LEFT JOIN ServiceType AS S ON P.ServiceType = S.ServiceType ";
        SS += " LEFT JOIN ProviderCode AS PC ON PC.ProviderCode = P.ProviderCode ";
        SS += " LEFT JOIN CompanyTable AS C  ON C.CompanyID = P.forCompanyID ";
        SS += " WHERE P.OrderID =@OrderID ";

        DBCmd = new SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = System.Data.CommandType.Text;
        DBCmd.Parameters.Add("@OrderID", SqlDbType.VarChar).Value = OrderID;
        DT = DBAccess.GetDB(DBConnStr, DBCmd);

        if (DT != null)
        {
            if (DT.Rows.Count > 0)
            {
                returnValue = new PaymentReport();
                returnValue.PaymentSerial = DT.Rows[0]["PaymentSerial"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["PaymentSerial"];
                returnValue.OrderID = DT.Rows[0]["OrderID"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["OrderID"];
                returnValue.ProviderName = DT.Rows[0]["ProviderName"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["ProviderName"];
                returnValue.ServiceTypeName = DT.Rows[0]["ServiceTypeName"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["ServiceTypeName"];
                returnValue.ProcessStatus = DT.Rows[0]["ProcessStatus"] == System.DBNull.Value ? 0 : (int)DT.Rows[0]["ProcessStatus"];
                returnValue.OrderAmount = DT.Rows[0]["OrderAmount"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[0]["OrderAmount"];
                returnValue.PaymentAmount = DT.Rows[0]["PaymentAmount"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[0]["PaymentAmount"];
                returnValue.CostRate = DT.Rows[0]["CostRate"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[0]["CostRate"];
                returnValue.CostCharge = DT.Rows[0]["CostCharge"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[0]["CostCharge"];
                returnValue.CollectRate = DT.Rows[0]["CollectRate"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[0]["CollectRate"];
                returnValue.CollectCharge = DT.Rows[0]["CollectCharge"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[0]["CollectCharge"];
                returnValue.CreateDate2 = DT.Rows[0]["CreateDate2"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["CreateDate2"];
                returnValue.FinishDate2 = DT.Rows[0]["FinishDate2"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["FinishDate2"]; 
                returnValue.PartialOrderAmount = DT.Rows[0]["PartialOrderAmount"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[0]["PartialOrderAmount"]; 
                returnValue.UserIP = DT.Rows[0]["UserIP"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["UserIP"];
 
            }
        }

        return returnValue;
    }

    public static int GetCompanyIDByCompanyCode(string CompanyCode)
    {
        int ret = -1;
        string SS;
        System.Data.SqlClient.SqlCommand DBCmd;
        PaymentReport returnValue = null;
        DataTable DT;
        DBCmd = new System.Data.SqlClient.SqlCommand();
        object DBreturn;
        SS = " SELECT CompanyID From CompanyTable Where CompanyCode=@CompanyCode ";

        DBCmd = new SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = System.Data.CommandType.Text;
        DBCmd.Parameters.Add("@CompanyCode", SqlDbType.VarChar).Value = CompanyCode;
        DBreturn = DBAccess.GetDBValue(DBConnStr, DBCmd);
        if (DBreturn != null)
        {
            ret = Convert.ToInt32(DBreturn);
        }

        return ret;
    }

    public static int GetProviderServiceState(string ServiceType, string CurrencyType, string ProviderCode)
    {

        int returnValue = -1;
        string SS;
        System.Data.SqlClient.SqlCommand DBCmd = null;

        SS = "SELECT State FROM ProviderService WITH (NOLOCK) WHERE ProviderCode =@ProviderCode AND ServiceType = @ServiceType AND CurrencyType =@CurrencyType";
        DBCmd = new SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = System.Data.CommandType.Text;
        DBCmd.Parameters.Add("@ProviderCode", SqlDbType.VarChar).Value = ProviderCode;
        DBCmd.Parameters.Add("@ServiceType", SqlDbType.VarChar).Value = ServiceType;
        DBCmd.Parameters.Add("@CurrencyType", SqlDbType.VarChar).Value = CurrencyType;
        returnValue = int.Parse(DBAccess.GetDBValue(DBConnStr, DBCmd).ToString());
        return returnValue;
    }

    public static int ChangeProviderServiceState(string ProviderCode,string ServiceType,string CurrencyType)
    {
        int returnValue;
        string SS;
        System.Data.SqlClient.SqlCommand DBCmd = null;

        SS = "UPDATE ProviderService SET State=(IIF(State=0,1,0)) WHERE ProviderCode=@ProviderCode AND ServiceType=@ServiceType AND CurrencyType=@CurrencyType";

        DBCmd = new System.Data.SqlClient.SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = CommandType.Text;
        DBCmd.Parameters.Add("@ProviderCode", SqlDbType.VarChar).Value = ProviderCode;
        DBCmd.Parameters.Add("@ServiceType", SqlDbType.VarChar).Value = ServiceType;
        DBCmd.Parameters.Add("@CurrencyType", SqlDbType.VarChar).Value = CurrencyType;
        returnValue = DBAccess.ExecuteDB(DBConnStr, DBCmd);
        RedisCache.ProviderService.UpdateProviderService(ProviderCode, ServiceType, CurrencyType);

        return returnValue;
    }

    public static int ChangeProviderCodeState(string ProviderCode)
    {
        int returnValue;
        string SS;
        System.Data.SqlClient.SqlCommand DBCmd = null;

        SS = "UPDATE ProviderCode SET ProviderState=(IIF(ProviderState=0,1,0)) WHERE ProviderCode=@ProviderCode";

        DBCmd = new System.Data.SqlClient.SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = CommandType.Text;
        DBCmd.Parameters.Add("@ProviderCode", SqlDbType.VarChar).Value = ProviderCode;
        returnValue = DBAccess.ExecuteDB(DBConnStr, DBCmd);
        RedisCache.ProviderCode.UpdateProviderCode(ProviderCode);

        return returnValue;
    }

    public static int ChangeProviderAPIType(string ProviderCode, int APIType)
    {
        int returnValue;
        string SS;
        System.Data.SqlClient.SqlCommand DBCmd = null;

        SS = "UPDATE ProviderCode SET ProviderAPIType= ProviderAPIType + @ProviderAPIType WHERE ProviderCode=@ProviderCode";

        DBCmd = new System.Data.SqlClient.SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = CommandType.Text;
        DBCmd.Parameters.Add("@ProviderCode", SqlDbType.VarChar).Value = ProviderCode;
        DBCmd.Parameters.Add("@ProviderAPIType", SqlDbType.Int).Value = APIType;
        returnValue = DBAccess.ExecuteDB(DBConnStr, DBCmd);
        RedisCache.ProviderCode.UpdateProviderCode(ProviderCode);

        return returnValue;
    }

    public static List<CompanyServicePointVM> GetCompanyServicePointDetail(int CompanyID, string CurrencyType)
    {
        List<CompanyServicePointVM> returnValue = null;
        string SS;
        SqlCommand DBCmd = null;
        DataTable DT;
        SS = " SELECT CompanyServicePoint.*,CompanyService.State,ServiceTypeName,WithdrawLimit.MaxLimit,WithdrawLimit.MinLimit,WithdrawLimit.Charge" +
             " FROM  CompanyServicePoint" +
             " LEFT JOIN ServiceType ON CompanyServicePoint.ServiceType=ServiceType.ServiceType" +
             " LEFT JOIN CompanyService ON CompanyService.ServiceType=CompanyServicePoint.ServiceType And CompanyService.CurrencyType=CompanyServicePoint.CurrencyType And CompanyService.forCompanyID=CompanyServicePoint.CompanyID " +
             " LEFT JOIN WithdrawLimit ON WithdrawLimit.ServiceType=CompanyServicePoint.ServiceType And WithdrawLimit.CurrencyType=@CurrencyType And WithdrawLimit.WithdrawLimitType=1 And WithdrawLimit.forCompanyID=@CompanyID" +
             " WHERE CompanyServicePoint.CompanyID = @CompanyID" +
             " AND CompanyServicePoint.CurrencyType = @CurrencyType";

        DBCmd = new SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = System.Data.CommandType.Text;
        DBCmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = CompanyID;
        DBCmd.Parameters.Add("@CurrencyType", SqlDbType.VarChar).Value = CurrencyType;
        DT = DBAccess.GetDB(DBConnStr, DBCmd);


        if (DT != null)
        {
            if (DT.Rows.Count > 0)
            {
                returnValue = new List<CompanyServicePointVM>();
                for (int i = 0; i < DT.Rows.Count; i++)
                {
                    CompanyServicePointVM _CompanyServicePointVM = new CompanyServicePointVM();
        
                    _CompanyServicePointVM.Charge = DT.Rows[i]["Charge"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["Charge"];
                    _CompanyServicePointVM.CompanyID = DT.Rows[i]["CompanyID"] == System.DBNull.Value ? 0 : (int)DT.Rows[i]["CompanyID"];
                    _CompanyServicePointVM.CurrencyType = DT.Rows[i]["CurrencyType"] == System.DBNull.Value ? "" : (string)DT.Rows[i]["CurrencyType"];
     
                    _CompanyServicePointVM.MaxLimit = DT.Rows[i]["MaxLimit"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["MaxLimit"];

                    _CompanyServicePointVM.MinLimit = DT.Rows[i]["MinLimit"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["MinLimit"];
                    _CompanyServicePointVM.ServiceType = DT.Rows[i]["ServiceType"] == System.DBNull.Value ? "" : (string)DT.Rows[i]["ServiceType"];
                    _CompanyServicePointVM.ServiceTypeName = DT.Rows[i]["ServiceTypeName"] == System.DBNull.Value ? "" : (string)DT.Rows[i]["ServiceTypeName"];
                    _CompanyServicePointVM.State = DT.Rows[i]["State"] == System.DBNull.Value ? 0 : (int)DT.Rows[i]["State"];
                    _CompanyServicePointVM.SystemPointValue = DT.Rows[i]["SystemPointValue"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["SystemPointValue"];

                    returnValue.Add(_CompanyServicePointVM);
                }
            }
        }

        return returnValue;
    }

    public static List<ProviderPointVM> GetAllProviderPointByCompanyID(int CompanyID,string CurrencyType)
    {
        List<ProviderPointVM> returnValue = null;
        string SS;
        SqlCommand DBCmd = null;
        DataTable DT;


        SS = " SELECT PC.ProviderName,PC.ProviderAPIType,PC.ProviderCode, ISNULL(PP.TotalDepositePointValue,0) TotalDepositePointValue,ISNULL(PP.TotalProfitPointValue,0) TotalProfitPointValue, " +
             " (ISNULL(PP.SystemPointValue, 0) - ISNULL(FP.ProviderFrozenAmount, 0)) AS SystemPointValue, ISNULL(FP.ProviderFrozenAmount, 0) ProviderFrozenAmount, ISNULL(FMH.WProfit,0) WithdrawProfit, " +
             " (SELECT  ISNULL(SUM(W.Amount + W.CostCharge), 0) FROM Withdrawal W WITH (NOLOCK) " +
             " WHERE W.Status <> 2 AND W.Status <> 3 AND W.Status <> 8  AND W.Status <> 90 AND W.Status <> 91 " +
             " AND W.ProviderCode = PP.ProviderCode AND W.CurrencyType = PP.CurrencyType) AS WithdrawPoint " +
             " FROM ProviderCode PC " +
             " LEFT JOIN ProviderPoint PP ON PC.ProviderCode = PP.ProviderCode" +
             " LEFT JOIN(SELECT forProviderCode, SUM(ISNULL(ProviderFrozenAmount, 0)) ProviderFrozenAmount FROM FrozenPoint FP " +
             " WHERE  FP.Status = 0 " +
             " GROUP BY forProviderCode) FP ON FP.forProviderCode = PC.ProviderCode " +
             " LEFT JOIN (SELECT ProviderCode,SUM(ISNULL(Amount, 0)) WProfit FROM ProviderManualHistory FMH " +
             " WHERE FMH.Type = 2 " +
             " GROUP BY ProviderCode) FMH ON FMH.ProviderCode = PC.ProviderCode " +
             " WHERE PC.ProviderState = 0 And PC.forCompanyID=@CompanyID And PP.CurrencyType=@CurrencyType ";

        DBCmd = new SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = System.Data.CommandType.Text;
        DBCmd.Parameters.Add("@CompanyID", System.Data.SqlDbType.Int).Value = CompanyID;
        DBCmd.Parameters.Add("@CurrencyType", SqlDbType.VarChar).Value = CurrencyType;
        DT = DBAccess.GetDB(DBConnStr, DBCmd);
        if (DT != null)
        {
            if (DT.Rows.Count > 0)
            {
                returnValue = new List<ProviderPointVM>();
                for (int i = 0; i < DT.Rows.Count; i++)
                {
                    ProviderPointVM _ProviderPointVM = new ProviderPointVM();
               
                
                    _ProviderPointVM.ProviderAPIType = DT.Rows[i]["ProviderAPIType"] == System.DBNull.Value ? 0 : (int)DT.Rows[i]["ProviderAPIType"];
                    _ProviderPointVM.ProviderCode = DT.Rows[i]["ProviderCode"] == System.DBNull.Value ? "" : (string)DT.Rows[i]["ProviderCode"];
                    _ProviderPointVM.ProviderFrozenAmount = DT.Rows[i]["ProviderFrozenAmount"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["ProviderFrozenAmount"];
                    _ProviderPointVM.ProviderName = DT.Rows[i]["ProviderName"] == System.DBNull.Value ? "" : (string)DT.Rows[i]["ProviderName"];
                    _ProviderPointVM.SystemPointValue = DT.Rows[i]["SystemPointValue"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["SystemPointValue"]; 
                    _ProviderPointVM.TotalDepositePointValue = DT.Rows[i]["TotalDepositePointValue"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["TotalDepositePointValue"];

                    _ProviderPointVM.TotalProfitPointValue = DT.Rows[i]["TotalProfitPointValue"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["TotalProfitPointValue"];
                    _ProviderPointVM.WithdrawPoint = DT.Rows[i]["WithdrawPoint"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["WithdrawPoint"];
                    _ProviderPointVM.WithdrawProfit = DT.Rows[i]["WithdrawProfit"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["WithdrawProfit"];
                    returnValue.Add(_ProviderPointVM);
                }
            }
        }

        return returnValue;
    }

    public static APIResult UpdateWithdrawalResultByWithdrawSerial(int Status, string WithdrawSerial, string ProviderCode, int WithdrawType, string ServiceType)
    {
        APIResult returnValue = new APIResult() {ResultState= APIResult.enumResultCode.ERR};
        String SS = String.Empty;
        SqlCommand DBCmd;
        int DBreturn = -6;//其他錯誤
        Withdrawal WithdrawData = GetWithdrawalByWithdrawSerial(WithdrawSerial);
        decimal Charge;
   
        if (WithdrawData == null)
        {
            returnValue.Message = "订单不存在";
            return returnValue;
        }

        if (WithdrawData.Status == 1)
        {
            returnValue.Message = "订单审核中";
            return returnValue;
        }

        if (WithdrawData.Status == 2 || WithdrawData.Status == 3)
        {
            returnValue.Message = "订单已完成";
            return returnValue;
        }

        if (Status != 3)
        {
            //取得供應商代付手續費
            WithdrawLimit _WithdrawLimit = new WithdrawLimit()
            {
                CompanyID = WithdrawData.forCompanyID,
                WithdrawLimitType = 0,
                ProviderCode = ProviderCode
            };

            //0=上游手續費，API/1=提現手續費/2=代付手續費(下游)
            var withdrawLimitResult = GetWithdrawLimitResultByCurrencyType(WithdrawData.CurrencyType, 0, ProviderCode, WithdrawData.forCompanyID);
            if (withdrawLimitResult == null)
            {
                returnValue.Message = "尚未定供应商手续费";
                return returnValue;
            }
            Charge = withdrawLimitResult.Charge;

            //检查供应商通道额度
            var ProviderPointModel = GetProviderPointByProviderCode(ProviderCode, WithdrawData.CurrencyType);
            if (ProviderPointModel == null)
            {
                returnValue.Message = "供应商通道额度错误";
                return returnValue;
            }

            if (WithdrawData.Amount + Charge > ProviderPointModel.SystemPointValue)
            {
                returnValue.Message = "供应商通道额度不足";
                return returnValue;
            }
            //检查商户支付通道额度
            var ServicePointModel = GetCompanyServicePointByServiceType(WithdrawData.forCompanyID, ServiceType, WithdrawData.CurrencyType);
            if (ServicePointModel == null)
            {
                returnValue.Message = "商户支付通道额度错误";
                return returnValue;
            }

            if (ServicePointModel.First().MaxLimit == 0 || ServicePointModel.First().MinLimit == 0)
            {
                returnValue.Message = "尚未设定商户支付通道限额";
                return returnValue;
            }

            WithdrawData.CollectCharge = ServicePointModel.First().Charge;

            if (WithdrawData.Amount + ServicePointModel.First().Charge > ServicePointModel.First().SystemPointValue)
            {
                if (WithdrawData.FloatType == 1)
                {
                    int intModifyCompanyServicePointResult = ModifyCompanyServicePointByWithdrawal(WithdrawData.WithdrawID, ServiceType, WithdrawData.forCompanyID, WithdrawData.CurrencyType, WithdrawData.Amount + ServicePointModel.First().Charge);
                    string tmpReturnStr = "";
                    if (intModifyCompanyServicePointResult != 0)
                    {
                        switch (intModifyCompanyServicePointResult)
                        {
                            case -1:
                                tmpReturnStr = "支付通道扣点失败";
                                break;
                            case -2:
                                tmpReturnStr = "商户支付通道额度不足";
                                break;
                            case -3:
                                tmpReturnStr = "商户钱包额度不足";
                                break;
                            case -4:
                                tmpReturnStr = "商户钱包不存在";
                                break;
                            case -5:
                                tmpReturnStr = "锁定失败";
                                break;
                            case -6:
                                tmpReturnStr = "支付通道加点失败";
                                break;
                            default:
                                break;
                        }
              
                        returnValue.Message = "支付通道调整额度失败,原因:" + tmpReturnStr;
                        return returnValue;
                    }

                }
                else
                {
                 
                    returnValue.Message = "商户支付通道额度不足";
                    return returnValue;
                }

            }

            //修改訂單狀態為審核中
            SS = " UPDATE Withdrawal  Set ConfirmByAdminID=@AdminID,ProviderCode=@ProviderCode,WithdrawType=@WithdrawType,Status=@Status,CollectCharge=@CollectCharge,CostCharge=@CostCharge,ServiceType=@ServiceType " +
                 " WHERE WithdrawSerial=@WithdrawSerial";

            DBCmd = new System.Data.SqlClient.SqlCommand();
            DBCmd.CommandText = SS;
            DBCmd.CommandType = CommandType.Text;
            DBCmd.Parameters.Add("@AdminID", SqlDbType.Int).Value = 0;
            DBCmd.Parameters.Add("@ProviderCode", SqlDbType.VarChar).Value = ProviderCode;
            DBCmd.Parameters.Add("@ServiceType", SqlDbType.VarChar).Value = ServiceType;
            DBCmd.Parameters.Add("@WithdrawType", SqlDbType.Int).Value = WithdrawType;
            DBCmd.Parameters.Add("@CostCharge", SqlDbType.Int).Value = Charge;
            DBCmd.Parameters.Add("@CollectCharge", SqlDbType.Int).Value = WithdrawData.CollectCharge;
            DBCmd.Parameters.Add("@Status", SqlDbType.Int).Value = 1;
            DBCmd.Parameters.Add("@WithdrawSerial", SqlDbType.VarChar).Value = WithdrawSerial;
            DBreturn = DBAccess.ExecuteDB(DBConnStr, DBCmd);

            if (DBreturn == 0)
            {
                returnValue.Message = "修改订单状态错误";
                return returnValue;
            }

            WithdrawData.ProviderCode = ProviderCode;
        }

        //更改為失敗單
        if (Status == 3)
        {
            SS = " UPDATE Withdrawal  SET Status=@Status,ConfirmByAdminID=@AdminID,FinishDate=@FinishDate" +
                " WHERE WithdrawSerial=@WithdrawSerial";

            DBCmd = new System.Data.SqlClient.SqlCommand();
            DBCmd.CommandText = SS;
            DBCmd.CommandType = CommandType.Text;
            DBCmd.Parameters.Add("@Status", SqlDbType.Int).Value = Status;
            DBCmd.Parameters.Add("@AdminID", SqlDbType.Int).Value = 0;
            DBCmd.Parameters.Add("@WithdrawSerial", SqlDbType.VarChar).Value = WithdrawSerial;
            DBCmd.Parameters.Add("@FinishDate", SqlDbType.DateTime).Value = DateTime.Now;
            returnValue.Message = "审核完成";
            DBAccess.ExecuteDB(DBConnStr, DBCmd);
            returnValue.ResultState = APIResult.enumResultCode.OK;
            return returnValue;
        }
        else
        {
            bool autoPay = false;
            var ProviderData = GetProviderCodeResult(WithdrawData.ProviderCode);
            if (ProviderData != null)
            {
                var ProviderAPIType = ProviderData.ProviderAPIType;

                if ((ProviderAPIType & 2) == 2)
                {
                    autoPay = true;
                }
            }
            if (autoPay)
            {
                //api自動代付
                string GPayApiUrl = System.Configuration.ConfigurationManager.AppSettings["GPayApiUrl"];
                string GPayBackendKey = System.Configuration.ConfigurationManager.AppSettings["GPayBackendKey"];
                #region SignCheck
                string strSign;
                string sign;
                WithdrawAPIResult returnRequireWithdrawal = null;

                strSign = string.Format("WithdrawSerial={0}&GPayBackendKey={1}"
                , WithdrawData.WithdrawSerial
                , GPayBackendKey
                );

                sign = GetSHA256(strSign);

                #endregion
                var _RequireWithdrawalSet = new RequireWithdrawalSet();
                _RequireWithdrawalSet.WithdrawSerial = WithdrawData.WithdrawSerial;
                _RequireWithdrawalSet.Sign = sign;

                var strRequireWithdrawal = RequestJsonAPI(GPayApiUrl + "SendWithdraw", Newtonsoft.Json.JsonConvert.SerializeObject(_RequireWithdrawalSet));

                if (!string.IsNullOrEmpty(strRequireWithdrawal))
                {
                    returnRequireWithdrawal = Newtonsoft.Json.JsonConvert.DeserializeObject<WithdrawAPIResult>(strRequireWithdrawal);
                    //OK = 0,ERR = 1,SignErr = 2,Invalidate = 3(查無此單) //Success=4 (交易已完成)
                    returnValue.Message = returnRequireWithdrawal.Message;

                    if ((int)returnRequireWithdrawal.Status == 1 || (int)returnRequireWithdrawal.Status == 2 || (int)returnRequireWithdrawal.Status == 3)
                    {
                        UpdateWithdrawalStatus(WithdrawSerial, 0);
                        returnValue.Message = returnRequireWithdrawal.Message;
                        return returnValue;
                    }
                    else
                    {
                        returnValue.ResultState = APIResult.enumResultCode.OK;
                        returnValue.Message = "审核完成";
                        return returnValue;
                    }
                }
                else
                {
                    UpdateWithdrawalStatus(WithdrawSerial, 0);
                    returnValue.Message = "API代付失败";
                    return returnValue;
                }
            }
            else
            {
                //更改訂單狀態回建立狀態
                UpdateWithdrawalStatus(WithdrawSerial, 0);
                returnValue.Message = "供应商尚未开启API代付";
                return returnValue;
            }
        }

    }


    public static int UpdateWithdrawalStatus(string WithdrawSerial, int Status)
    {
        int DBreturn = -1;
        String SS = String.Empty;
        SqlCommand DBCmd;

        SS = " UPDATE Withdrawal  SET Status=@Status" +
             " WHERE WithdrawSerial=@WithdrawSerial";

        DBCmd = new System.Data.SqlClient.SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = CommandType.Text;

        DBCmd.Parameters.Add("@WithdrawSerial", SqlDbType.VarChar).Value = WithdrawSerial;
        DBCmd.Parameters.Add("@Status", SqlDbType.Int).Value = Status;
        DBreturn = DBAccess.ExecuteDB(DBConnStr, DBCmd);
        return DBreturn;
    }

    public static Provider GetProviderCodeResult(string ProviderCode)
    {
        Provider returnValue = null;
        string SS;
        SqlCommand DBCmd = null;
        DataTable DT;
    
        SS = "SELECT * FROM ProviderCode WITH (NOLOCK) WHERE ProviderCode=@ProviderCode";

        DBCmd = new SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = System.Data.CommandType.Text;
        DBCmd.Parameters.Add("@ProviderCode", SqlDbType.VarChar).Value = ProviderCode;
        DT = DBAccess.GetDB(DBConnStr, DBCmd);
     
        if (DT != null)
        {
            if (DT.Rows.Count > 0)
            {
                returnValue = new Provider();
                returnValue.ProviderAPIType = DT.Rows[0]["ProviderAPIType"] == System.DBNull.Value ? 0 : (int)DT.Rows[0]["ProviderAPIType"];
                returnValue.ProviderCode = DT.Rows[0]["ProviderCode"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["ProviderCode"];
            }
        }

        return returnValue;
    }

    public static ProviderPointVM GetProviderPointByProviderCode(string ProviderCode,string CurrencyType)
    {
        ProviderPointVM returnValue = null;
        string SS;
        SqlCommand DBCmd = null;
        DataTable DT;

        SS = " SELECT SystemPointValue " +
             " FROM ProviderPoint " +
             " WHERE ProviderCode=@ProviderCode ";

        DBCmd = new SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = System.Data.CommandType.Text;
        DBCmd.Parameters.Add("@ProviderCode", SqlDbType.VarChar).Value = ProviderCode;
        DBCmd.Parameters.Add("@CurrencyType", SqlDbType.VarChar).Value = CurrencyType;
        DT = DBAccess.GetDB(DBConnStr, DBCmd);
        if (DT != null)
        {
            if (DT.Rows.Count > 0)
            {
                try
                {
                    returnValue = new ProviderPointVM();
                    returnValue.SystemPointValue = DT.Rows[0]["SystemPointValue"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[0]["SystemPointValue"];
                }
                catch (Exception ex) 
                {
                    var a = ex.Message;
                    throw;
                }
            
            }
        }

        return returnValue;
    }

    public static WithdrawLimit GetWithdrawLimitResultByCurrencyType(string CurrencyType, int WithdrawLimitType, string ProviderCode, int CompanyID)
    {
        WithdrawLimit returnValue = null;
        string SS = "";
        SqlCommand DBCmd = null;
        DataTable DT = null;
        if (WithdrawLimitType == 0)
        { //供應商資料
            SS = "SELECT * FROM WithdrawLimit WHERE ProviderCode =@ProviderCode And WithdrawLimitType=@WithdrawLimitType And CurrencyType=@CurrencyType ";
        }
        else if (WithdrawLimitType == 1)
        {   //營運商資料
            SS = "SELECT * FROM WithdrawLimit WHERE forCompanyID =@forCompanyID And WithdrawLimitType=@WithdrawLimitType And CurrencyType=@CurrencyType ";
        }

        if (!String.IsNullOrEmpty(SS))
        {
            DBCmd = new SqlCommand();
            DBCmd.CommandText = SS;
            DBCmd.CommandType = System.Data.CommandType.Text;
            DBCmd.Parameters.Add("@WithdrawLimitType", SqlDbType.Int).Value = WithdrawLimitType;
            DBCmd.Parameters.Add("@ProviderCode", SqlDbType.VarChar).Value = ProviderCode;
            DBCmd.Parameters.Add("@forCompanyID", SqlDbType.Int).Value = CompanyID;
            DBCmd.Parameters.Add("@CurrencyType", SqlDbType.VarChar).Value = CurrencyType;
            DT = DBAccess.GetDB(DBConnStr, DBCmd);
        }


        if (DT != null)
        {
            if (DT.Rows.Count > 0)
            {
                returnValue = new WithdrawLimit();
                returnValue.WithdrawLimitType = DT.Rows[0]["WithdrawLimitType"] == System.DBNull.Value ? 0 : (int)DT.Rows[0]["WithdrawLimitType"];
                returnValue.CurrencyType = DT.Rows[0]["CurrencyType"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["CurrencyType"];
                returnValue.ProviderCode = DT.Rows[0]["ProviderCode"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["ProviderCode"];
                returnValue.ServiceType = DT.Rows[0]["ServiceType"] == System.DBNull.Value ? "" : (string)DT.Rows[0]["ServiceType"];
                returnValue.MaxLimit = DT.Rows[0]["MaxLimit"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[0]["MaxLimit"];
                returnValue.MinLimit = DT.Rows[0]["MinLimit"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[0]["MinLimit"];
                returnValue.Charge = DT.Rows[0]["Charge"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[0]["Charge"];
            }
        }

        return returnValue;
    }

    public static List<CompanyServicePointVM> GetCompanyServicePointByServiceType(int CompanyID, string ServiceType, string CurrencyType)
    {
        List<CompanyServicePointVM> returnValue = null;
        string SS;
        SqlCommand DBCmd = null;
        DataTable DT;

        SS = " SELECT *,ServiceTypeName" +
             " FROM  CompanyServicePoint" +
             " LEFT JOIN ServiceType ON CompanyServicePoint.ServiceType=ServiceType.ServiceType" +
              " LEFT JOIN WithdrawLimit ON WithdrawLimit.ServiceType=CompanyServicePoint.ServiceType And WithdrawLimit.CurrencyType= CompanyServicePoint.CurrencyType And WithdrawLimit.WithdrawLimitType=1 And CompanyServicePoint.CompanyID=WithdrawLimit.forCompanyID" +
             " WHERE CompanyServicePoint.CompanyID = @CompanyID" +
             " AND CompanyServicePoint.CurrencyType = @CurrencyType" +
             " AND CompanyServicePoint.ServiceType = @ServiceType";
        DBCmd = new SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = System.Data.CommandType.Text;
        DBCmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = CompanyID;
        DBCmd.Parameters.Add("@CurrencyType", SqlDbType.VarChar).Value = CurrencyType;
        DBCmd.Parameters.Add("@ServiceType", SqlDbType.VarChar).Value = ServiceType;
        DT = DBAccess.GetDB(DBConnStr, DBCmd);


        if (DT != null)
        {
            if (DT.Rows.Count > 0)
            {
                returnValue = new List<CompanyServicePointVM>();
                for (int i = 0; i < DT.Rows.Count; i++)
                {
                    CompanyServicePointVM _CompanyServicePointVM = new CompanyServicePointVM();

                    _CompanyServicePointVM.Charge = DT.Rows[i]["Charge"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["Charge"];
                    _CompanyServicePointVM.CompanyID = DT.Rows[i]["CompanyID"] == System.DBNull.Value ? 0 : (int)DT.Rows[i]["CompanyID"];
                    _CompanyServicePointVM.CurrencyType = DT.Rows[i]["CurrencyType"] == System.DBNull.Value ? "" : (string)DT.Rows[i]["CurrencyType"];

                    _CompanyServicePointVM.MaxLimit = DT.Rows[i]["MaxLimit"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["MaxLimit"];

                    _CompanyServicePointVM.MinLimit = DT.Rows[i]["MinLimit"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["MinLimit"];
                    _CompanyServicePointVM.ServiceType = DT.Rows[i]["ServiceType"] == System.DBNull.Value ? "" : (string)DT.Rows[i]["ServiceType"];
                    _CompanyServicePointVM.ServiceTypeName = DT.Rows[i]["ServiceTypeName"] == System.DBNull.Value ? "" : (string)DT.Rows[i]["ServiceTypeName"];
                    _CompanyServicePointVM.SystemPointValue = DT.Rows[i]["SystemPointValue"] == System.DBNull.Value ? 0 : (decimal)DT.Rows[i]["SystemPointValue"];

                    returnValue.Add(_CompanyServicePointVM);
                }
            }
        }

        return returnValue;
    }

    public static int ModifyCompanyServicePointByWithdrawal(int WithdrawID, string ServiceType, int CompanyID, string CurrencyType, decimal FinishtAmount)
    {
        int DBreturn = -9;
        String SS = String.Empty;
        SqlCommand DBCmd;
        SS = "spModifyCompanyServicePointByWithdrawal";
        DBCmd = new System.Data.SqlClient.SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = CommandType.StoredProcedure;
        DBCmd.Parameters.Add("@WithdrawID", SqlDbType.Int).Value = WithdrawID;
        DBCmd.Parameters.Add("@ServiceType", SqlDbType.VarChar).Value = ServiceType;
        DBCmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = CompanyID;
        DBCmd.Parameters.Add("@CurrencyType", SqlDbType.VarChar).Value = CurrencyType;
        DBCmd.Parameters.Add("@FinishtAmount", SqlDbType.Decimal).Value = FinishtAmount;
        DBCmd.Parameters.Add("@Return", SqlDbType.VarChar).Direction = System.Data.ParameterDirection.ReturnValue;
        DBAccess.ExecuteDB(DBConnStr, DBCmd);
        DBreturn = (int)DBCmd.Parameters["@Return"].Value;

        return DBreturn;
    }

    public static string GetMD5(string DataString, bool Base64Encoding = true)
    {
        return GetMD5(System.Text.Encoding.UTF8.GetBytes(DataString), Base64Encoding);
    }

    public static string GetMD5(byte[] Data, bool Base64Encoding = true)
    {
        System.Security.Cryptography.MD5CryptoServiceProvider MD5Provider = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hash;
        System.Text.StringBuilder RetValue = new System.Text.StringBuilder();

        hash = MD5Provider.ComputeHash(Data);
        MD5Provider = null;

        if (Base64Encoding)
        {
            RetValue.Append(System.Convert.ToBase64String(hash));
        }
        else
        {
            foreach (byte EachByte in hash)
            {
                // => .ToString("x2")
                string ByteStr = EachByte.ToString("x");

                ByteStr = new string('0', 2 - ByteStr.Length) + ByteStr;
                RetValue.Append(ByteStr);
            }
        }


        return RetValue.ToString();
    }


    public static string GetSHA256(string DataString, bool Base64Encoding = true)
    {
        return GetSHA256(System.Text.Encoding.UTF8.GetBytes(DataString), Base64Encoding);
    }

    public static string GetSHA256(byte[] Data, bool Base64Encoding = true)
    {
        System.Security.Cryptography.SHA256 SHA256Provider = new System.Security.Cryptography.SHA256CryptoServiceProvider();
        byte[] hash;
        System.Text.StringBuilder RetValue = new System.Text.StringBuilder();

        hash = SHA256Provider.ComputeHash(Data);
        SHA256Provider = null;

        if (Base64Encoding)
        {
            RetValue.Append(System.Convert.ToBase64String(hash));
        }
        else
        {
            foreach (byte EachByte in hash)
            {
                // => .ToString("x2")
                string ByteStr = EachByte.ToString("x");

                ByteStr = new string('0', 2 - ByteStr.Length) + ByteStr;
                RetValue.Append(ByteStr);
            }
        }


        return RetValue.ToString();
    }

    public class UpdateWithdrawalResult
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public decimal PaymentAmount { get; set; }
    }

    public class RequireWithdrawalSet
    {
        public string WithdrawSerial { get; set; }
        public string Sign { get; set; }
    }

    public class Provider
    {
        public string ProviderCode { get; set; }
        public string ProviderName { get; set; }
        public string Introducer { get; set; }
        public string ProviderUrl { get; set; }
        public string MerchantCode { get; set; }
        public string MerchantKey { get; set; }
        public string NotifyAsyncUrl { get; set; }
        public string NotifySyncUrl { get; set; }
        public int ProviderAPIType { get; set; }
        public int ProviderState { get; set; }
        public int CollectType { get; set; }
        public int forCompanyID { get; set; }
        public decimal WithdrawRate { get; set; }
    }

    public class WithdrawLimit
    {
        public string CurrencyType { get; set; }
        //0=Provider/1=Company/2=代付
        public string ServiceType { get; set; }
        public string ServiceTypeName { get; set; }
        public int WithdrawLimitType { get; set; }
        public string ProviderCode { get; set; }
        public int CompanyID { get; set; }
        public decimal MaxLimit { get; set; }
        public decimal MinLimit { get; set; }
        public decimal Charge { get; set; }
    }

    public static string RequestJsonAPI(string Url, string JsonString)
    {
        string result = string.Empty;
        using (System.Net.Http.HttpClientHandler handler = new System.Net.Http.HttpClientHandler())
        {
            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient(handler))
            {
                try
                {
                    #region 呼叫遠端 Web API

                    System.Net.Http.HttpRequestMessage request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post,Url);
                    System.Net.Http.HttpResponseMessage response = null;

                    #region  設定相關網址內容

                    // Accept 用於宣告客戶端要求服務端回應的文件型態 (底下兩種方法皆可任選其一來使用)
                    //client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    // Content-Type 用於宣告遞送給對方的文件型態
                    //client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");


                    // 將 data 轉為 json
                    string json = JsonString;
                    request.Content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    response = client.SendAsync(request).GetAwaiter().GetResult();

                    #endregion
                    #endregion

                    #region 處理呼叫完成 Web API 之後的回報結果
                    if (response != null)
                    {
                        if (response.IsSuccessStatusCode == true)
                        {
                            // 取得呼叫完成 API 後的回報內容
                            result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        }

                    }

                    #endregion

                }
                catch (Exception ex)
                {
                    //return ex.Message;
                }
            }
        }

        return result;
    }

    public class Withdrawal
    {
        public int WithdrawID { get; set; }
        public int WithdrawType { get; set; }
        public string WithdrawSerial { get; set; }
        public string CreateDate2 { get; set; }
        public string SummaryDate { get; set; }
        public int forCompanyID { get; set; }
        public string CurrencyType { get; set; }
        public decimal Amount { get; set; }
        public decimal FinishAmount { get; set; }
        public string FinishDate2 { get; set; }
        public int Status { get; set; } //流程狀態，0=建立/1=進行中/2=成功/3=失敗
        public string BankCard { get; set; }
        public string CompanyName { get; set; }
        public string CompanyCode { get; set; }
        public string BankCardName { get; set; }
        public string BankName { get; set; }
        public int BankType { get; set; }
        public string OwnProvince { get; set; }
        public string OwnCity { get; set; }
        public string BankBranchName { get; set; }
        public string RealName1 { get; set; }
        public string RealName2 { get; set; }
        public string ProviderCode { get; set; }
        public string ProviderName { get; set; }
        public string BankDescription { get; set; }
        public string ServiceType { get; set; }
        public string ServiceTypeName { get; set; }
        public decimal CollectCharge { get; set; }//手續費(商户)
        public decimal CostCharge { get; set; }//手續費(供應商)
        public decimal WithdrawalCharge { get; set; }//手續費(供應商)
        public string Description { get; set; }
        public string RejectDescription { get; set; }
        public string ManagerRejectDescription { get; set; }
        public string CashierRejectDescription { get; set; }
        public int FloatType { get; set; } // 0=後台申請提現單=>後台審核 /1=API申請代付=>後台審核 /2=API申請代付=>不經後台審核
        public int DownStatus { get; set; }
        public string DownUrl { get; set; }
        public string DownOrderID { get; set; }
        public DateTime DownOrderDate { get; set; }
        public string DownMobile { get; set; }
        public string DownClientIP { get; set; }
        public int UpStatus { get; set; }
        public string UpResult { get; set; }
        public string UpOrderID { get; set; }
        public decimal UpDidAmount { get; set; }
        public int UpAccounting { get; set; }
        public string forProviderCode { get; set; }
        public int HandleByAdminID { get; set; }
        public string GroupName { get; set; }
        public int GroupID { get; set; }
        public string CompanyDescription { get; set; }
        public decimal WithdrawRate { get; set; }
        public int DecimalPlaces { get; set; }

    }

    public class PaymentReport
    {
        public string CreateDate2 { get; set; }
        public string OrderDate2 { get; set; }
        public int PaymentID { get; set; }
        public int forCompanyID { get; set; }
        public string PaymentSerial { get; set; }
        public string forPaymentSerial { get; set; }
        public string CurrencyType { get; set; }
        public string ServiceType { get; set; }
        public string ProviderCode { get; set; }
        public int ProcessStatus { get; set; }
        public string ReturnURL { get; set; }
        public object PaymentResult { get; set; }
        public string ClientIP { get; set; }
        public string UserIP { get; set; }
        public string OrderID { get; set; }
        public decimal OrderAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal CostRate { get; set; }
        public decimal CostCharge { get; set; }
        public decimal CollectRate { get; set; }
        public decimal CollectCharge { get; set; }
        public decimal PartialOrderAmount { get; set; }
        public decimal PaymentRate { get; set; }
        public int Accounting { get; set; }
        public string ServiceTypeName { get; set; }
        public string ProviderName { get; set; }
        public string BankName { get; set; }
        public int BankType { get; set; }
        public string CompanyName { get; set; }
        public string CompanyCode { get; set; }
        public string ProviderOrderID { get; set; }
        public string FinishDate2 { get; set; }
        public int SubmitType { get; set; }
        public string PatchDescription { get; set; }
        public string Description { get; set; }
        public string MerchantCode { get; set; }
        public string RealName { get; set; }
        public string GroupName { get; set; }
        public string UserName { get; set; }
    }

    public class ProviderListResult
    {
        public string ProviderCode { get; set; }
        public string ProviderName { get; set; }
        public int ProviderAPIType { get; set; }
        public decimal MaxLimit { get; set; }
        public decimal MinLimit { get; set; }
        public decimal Charge { get; set; }
        public string CurrencyType { get; set; }
        public int ProviderState { get; set; }
        public List<ServiceData> ServiceDatas { get; set; }
        public List<ProviderPointVM> ProviderListPoints { get; set; }
        public List<ProviderListPoint> ProviderListFrozenPoints { get; set; }
    }

    public class ProviderPointVM : ProviderPoint
    {
        public string ProviderName { get; set; }
        public int ProviderAPIType { get; set; }
        public decimal ProviderFrozenAmount { get; set; }
        public decimal WithdrawPoint { get; set; }
        public decimal WithdrawProfit { get; set; }
    }

    public class ProviderList
    {
        public List<ProviderListResult> ProviderListResults;
    }

    public class ProviderPoint
    {
        public string ProviderCode { get; set; }
        public string CurrencyType { get; set; }
        public string exMessage { get; set; }
        public decimal SystemPointValue { get; set; }
        public decimal ProviderPointValue { get; set; }
        public decimal TotalDepositePointValue { get; set; }
        public decimal TotalProfitPointValue { get; set; }
    }

    public class ServiceData
    {
        public string ServiceTypeName { get; set; }
        public decimal MaxDaliyAmount { get; set; }
        public decimal MaxOnceAmount { get; set; }
        public decimal MinOnceAmount { get; set; }
        public decimal CostCharge { get; set; }
        public decimal CostRate { get; set; }
        public int CheckoutType { get; set; }
        public string ServiceType { get; set; }
        public string ProviderCode { get; set; }
        public string CurrencyType { get; set; }
        public int State { get; set; }
    }

    public class ProviderListPoint
    {
        public string CurrencyType { get; set; }
        public decimal SystemPointValue { get; set; }
    }

    public class CompanyServicePointVM : CompanyServicePoint
    {
        public string CompanyName { get; set; }
        public string ServiceTypeName { get; set; }
        public decimal MaxLimit { get; set; }
        public decimal MinLimit { get; set; }
        public decimal FrozenPoint { get; set; }
        public decimal CanUsePoint { get; set; }
        public decimal Charge { get; set; }
        public decimal WithdrawalPoint { get; set; }
        public int FrozenServiceCount { get; set; }
        public int State { get; set; }
        public decimal FrozenServicePoint { get; set; }

    }

    public class CompanyServicePoint
    {
        public int CompanyID { get; set; }
        public string CurrencyType { get; set; }
        public string ServiceType { get; set; }
        public decimal SystemPointValue { get; set; }
    }

    public class APIResult
    {
        public enum enumResultCode
        {
            OK = 0,
            ERR = 1
        }

        public enumResultCode ResultState { get; set; }
        public string GUID { get; set; }
        public string Message { get; set; }
    }

    public class WithdrawAPIResult
    {
        public ResultStatus Status;
        public string Message;
    }

    public enum ResultStatus
    {
        OK = 0,
        ERR = 1,
        SignErr = 2,
        Invalidate = 3,
        Success = 4
    }

    public static class DBAccess
    {
        private static EnumDBType iDBType = EnumDBType.SqlClient;

        public enum EnumDBType
        {
            OleDB = 0,
            SqlClient = 1
        }

        public static EnumDBType DBAccessSetType
        {
            set
            {
                iDBType = value;
            }
        }

        public static System.Data.DataTable GetDB(string DBConnStr, string SS)
        {
            System.Data.Common.DbCommand DBCmd;

            DBCmd = GetDBObjCommand();
            DBCmd.CommandType = System.Data.CommandType.Text;
            DBCmd.CommandText = SS;

            return GetDB(DBConnStr, DBCmd);
        }

        public static System.Data.DataTable GetDB(string DBConnStr, System.Data.Common.DbCommand DBCmd)
        {
            System.Data.DataTable DT;
            System.Data.Common.DbDataAdapter DA;
            System.Data.Common.DbConnection DBConn;
            string SS;

            DBConn = GetDBObjConnection();
            DBConn.ConnectionString = DBConnStr;
            DBCmd.Connection = DBConn;

            SS = DBCmd.CommandText;

            DT = new System.Data.DataTable();
            DA = GetDBObjDataAdapter();
            DA.SelectCommand = DBCmd;

            try
            {
                DBConn.Open();
                // DA.FillSchema(DT, System.Data.SchemaType.Source)
                DA.Fill(DT);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                try
                {
                    DBConn.Close();
                }
                catch (Exception ex)
                {
                }
            }

            DT.ExtendedProperties.Add("CreateInstance", "DBAccess");
            DT.ExtendedProperties.Add("DBAccess_OleDBString", SS);
            DT.ExtendedProperties.Add("DBAccess_DBCommand", DBCmd);
            DT.ExtendedProperties.Add("DBAccess_AutoNumber", string.Empty);

            foreach (System.Data.DataColumn DC in DT.Columns)
            {
                if (DC.AutoIncrement)
                {
                    DT.ExtendedProperties["DBAccess_AutoNumber"] = DC.ColumnName;
                    DC.AutoIncrementSeed = -1;
                    DC.AutoIncrementStep = -1;
                    break;
                }
            }

            return DT;
        }

        public static object GetDBValue(string DBConnStr, string SS)
        {
            System.Data.Common.DbCommand DBCmd;

            DBCmd = GetDBObjCommand();
            DBCmd.CommandType = System.Data.CommandType.Text;
            DBCmd.CommandText = SS;

            return GetDBValue(DBConnStr, DBCmd);
        }

        public static object GetDBValue(string DBConnStr, System.Data.Common.DbCommand DBCmd)
        {
            System.Data.Common.DbConnection DBConn;
            object RetValue = null;
            TransactionDB T = null;
            bool DoTrans = false;
            System.LocalDataStoreSlot LDS;

            LDS = System.Threading.Thread.GetNamedDataSlot("DBAccess_Transaction");
            if (LDS != null)
            {
                T = (TransactionDB)System.Threading.Thread.GetData(LDS);
                if (T != null)
                {
                    if (T.ConnectionString == DBConnStr)
                        DoTrans = true;
                }
            }

            if (DoTrans)
            {
                RetValue = T.GetDBValue(DBCmd);
            }
            else
            {
                DBConn = GetDBObjConnection();
                DBConn.ConnectionString = DBConnStr;
                DBCmd.Connection = DBConn;

                try
                {
                    DBConn.Open();
                    RetValue = DBCmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    try
                    {
                        DBConn.Close();
                    }
                    catch (Exception ex)
                    {
                    }
                }

                DBConn.Dispose();
                DBConn = null;
            }

            return RetValue;
        }

        public static int ExecuteDB(string DBConnStr, string SS)
        {
            System.Data.Common.DbCommand DBCmd;
            int RetValue;

            DBCmd = GetDBObjCommand();

            DBCmd.CommandText = SS;
            DBCmd.CommandType = System.Data.CommandType.Text;

            RetValue = ExecuteDB(DBConnStr, DBCmd);

            return RetValue;
        }

        public static int ExecuteDB(string DBConnStr, System.Data.Common.DbCommand Cmd)
        {
            System.Data.Common.DbConnection DBConn;
            int RetValue;
            TransactionDB T = null;
            bool DoTrans = false;
            System.LocalDataStoreSlot LDS;

            LDS = System.Threading.Thread.GetNamedDataSlot("DBAccess_Transaction");
            if (LDS != null)
            {
                T = (TransactionDB)System.Threading.Thread.GetData(LDS);
                if (T != null)
                {
                    if (T.ConnectionString == DBConnStr)
                        DoTrans = true;
                }
            }

            if (DoTrans)
            {
                RetValue = T.ExecuteDB(Cmd);
            }
            else
            {
                DBConn = GetDBObjConnection();
                DBConn.ConnectionString = DBConnStr;
                Cmd.Connection = DBConn;

                try
                {
                    DBConn.Open();
                    RetValue = Cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    try
                    {
                        DBConn.Close();
                    }
                    catch (Exception ex)
                    {
                    }
                }

                DBConn.Dispose();
                DBConn = null;
            }

            return RetValue;
        }

        public static void ExecuteTransDB(string DBConnStr, Action<TransactionDB> Func)
        {
            TransactionDB TransDB = null;
            bool ExecuteSuccess = false;
            System.LocalDataStoreSlot LDS;

            LDS = System.Threading.Thread.GetNamedDataSlot("DBAccess_Transaction");
            if (LDS == null)
                LDS = System.Threading.Thread.AllocateNamedDataSlot("DBAccess_Transaction");

            TransDB = new TransactionDB(DBConnStr);
            System.Threading.Thread.SetData(LDS, TransDB);
            try
            {
                Func.Invoke(TransDB);
                ExecuteSuccess = true;
            }
            catch (Exception ex)
            {
                if (TransDB != null)
                {
                    try
                    {
                        TransDB.Rollback();
                    }
                    catch (Exception ex2)
                    {
                    }
                }

                System.Threading.Thread.FreeNamedDataSlot("DBAccess_Transaction");

                throw ex;
            }

            if (ExecuteSuccess)
            {
                if (TransDB != null)
                {
                    try
                    {
                        TransDB.Commit();
                    }
                    catch (Exception ex)
                    {
                    }
                }

                System.Threading.Thread.FreeNamedDataSlot("DBAccess_Transaction");
            }
        }

        public static System.Data.DataTable SubmitDB(string DBConnStr, System.Data.DataTable DT)
        {
            System.Data.Common.DbConnection DBConn;
            System.Data.Common.DbDataAdapter DA;
            System.Data.Common.DbCommand DBCmd;
            System.Data.Common.DbCommandBuilder DBCmdBuilder;
            System.Data.DataView DV;
            bool NeedUpdate;
            string QueryString;

            NeedUpdate = false;
            if ((string)DT.ExtendedProperties["CreateInstance"] == "DBAccess")
            {
                QueryString = (string)DT.ExtendedProperties["DBAccess_OleDBString"];
                DBCmd = (System.Data.Common.DbCommand)DT.ExtendedProperties["DBAccess_DBCommand"];

                DBConn = GetDBObjConnection();
                DBConn.ConnectionString = DBConnStr;

                // DBCmd = GetDBObjCommand()
                // DBCmd.CommandText = QueryString
                DBCmd.Connection = DBConn;

                DA = GetDBObjDataAdapter();
                DA.SelectCommand = DBCmd;

                // DA = DT.ExtendedProperties.Item("DBAccess_DataAdapter")

                DBCmdBuilder = GetDBObjCommandBuilder(DA);

                DV = new System.Data.DataView(DT);

                // 檢查是否有新增資料
                DV.RowStateFilter = System.Data.DataViewRowState.Added;
                if (DV.Count > 0)
                {
                    DA.InsertCommand = DBCmdBuilder.GetInsertCommand();
                    NeedUpdate = true;
                }

                // 檢查是否有刪除資料
                DV.RowStateFilter = System.Data.DataViewRowState.Deleted;
                if (DV.Count > 0)
                {
                    DA.DeleteCommand = DBCmdBuilder.GetDeleteCommand();
                    NeedUpdate = true;
                }

                // 檢查是否有更新資料
                DV.RowStateFilter = System.Data.DataViewRowState.ModifiedCurrent;
                if (DV.Count > 0)
                {
                    DA.UpdateCommand = DBCmdBuilder.GetUpdateCommand();
                    NeedUpdate = true;
                }

                if (NeedUpdate)
                {
                    DBConn.Open();
                    DBHandleUpdate(DA, DT);
                    DBConn.Close();

                    DT.AcceptChanges();
                }

                DV.Dispose();
                DBCmdBuilder.Dispose();
                DA.Dispose();
                DBCmd.Dispose();
                DBConn.Dispose();

                DA = null;
                DV = null;
                DBCmdBuilder = null;
                DBCmd = null;
                DBConn = null;
            }
            else
            {
                throw new Exception("DataTable 無法識別");
            }

            return DT;
        }

        private static void DBHandleUpdate(System.Data.Common.DbDataAdapter DA, System.Data.DataTable DT)
        {
            System.Data.SqlClient.SqlDataAdapter SqlDA;
            System.Data.OleDb.OleDbDataAdapter OleDA;

            switch (iDBType)
            {
                case EnumDBType.OleDB:
                    OleDA = (System.Data.OleDb.OleDbDataAdapter)DA;

                    OleDA.RowUpdated += DBAccess_onRowUpdate_OleDB;
                    OleDA.Update(DT);
                    OleDA.RowUpdated -= DBAccess_onRowUpdate_OleDB;
                    break;
                case EnumDBType.SqlClient:
                    SqlDA = (System.Data.SqlClient.SqlDataAdapter)DA;

                    SqlDA.RowUpdated += DBAccess_onRowUpdate_SqlClient;
                    SqlDA.Update(DT);
                    SqlDA.RowUpdated -= DBAccess_onRowUpdate_SqlClient;
                    break;
                default:
                    DA.Update(DT);
                    break;
            }
        }

        private static void DBAccess_onRowUpdate_OleDB(object sender, System.Data.OleDb.OleDbRowUpdatedEventArgs args)
        {
            object newID;
            string IDFieldName;
            System.Data.OleDb.OleDbCommand DBCmd;
            bool ColumnReadOnlyValue;

            if (args.StatementType == System.Data.StatementType.Insert)
            {
                IDFieldName = (string)args.Row.Table.ExtendedProperties["DBAccess_AutoNumber"];
                if (IDFieldName != string.Empty)
                {
                    DBCmd = new System.Data.OleDb.OleDbCommand("SELECT @@IDENTITY");
                    DBCmd.Connection = args.Command.Connection;
                    newID = DBCmd.ExecuteScalar();

                    if (newID != System.DBNull.Value)
                    {
                        ColumnReadOnlyValue = args.Row.Table.Columns[IDFieldName].ReadOnly;

                        args.Row.Table.Columns[IDFieldName].ReadOnly = false;
                        args.Row[IDFieldName] = (int)newID;
                        args.Row.Table.Columns[IDFieldName].ReadOnly = ColumnReadOnlyValue;
                    }
                }
            }
        }

        private static void DBAccess_onRowUpdate_SqlClient(object sender, System.Data.SqlClient.SqlRowUpdatedEventArgs args)
        {
            object newID;
            string IDFieldName;
            System.Data.SqlClient.SqlCommand DBCmd;
            bool ColumnReadOnlyValue;

            if (args.StatementType == System.Data.StatementType.Insert)
            {
                IDFieldName = (string)args.Row.Table.ExtendedProperties["DBAccess_AutoNumber"];
                if (IDFieldName != string.Empty)
                {
                    DBCmd = new System.Data.SqlClient.SqlCommand("SELECT @@IDENTITY");
                    DBCmd.Connection = args.Command.Connection;
                    newID = DBCmd.ExecuteScalar();

                    if (newID != System.DBNull.Value)
                    {
                        ColumnReadOnlyValue = args.Row.Table.Columns[IDFieldName].ReadOnly;

                        args.Row.Table.Columns[IDFieldName].ReadOnly = false;
                        args.Row[IDFieldName] = (int)newID;
                        args.Row.Table.Columns[IDFieldName].ReadOnly = ColumnReadOnlyValue;
                    }
                }
            }
        }

        private static System.Data.Common.DbConnection GetDBObjConnection()
        {
            System.Data.Common.DbConnection RetValue = null;

            switch (iDBType)
            {
                case EnumDBType.OleDB:
                    RetValue = new System.Data.OleDb.OleDbConnection();
                    break;
                case EnumDBType.SqlClient:
                    RetValue = new System.Data.SqlClient.SqlConnection();
                    break;
            }

            return RetValue;
        }

        private static System.Data.Common.DbCommand GetDBObjCommand()
        {
            System.Data.Common.DbCommand RetValue = null;

            switch (iDBType)
            {
                case EnumDBType.OleDB:
                    RetValue = new System.Data.OleDb.OleDbCommand();
                    break;
                case EnumDBType.SqlClient:
                    RetValue = new System.Data.SqlClient.SqlCommand();
                    break;
            }

            return RetValue;
        }

        private static System.Data.Common.DbCommandBuilder GetDBObjCommandBuilder(System.Data.Common.DataAdapter DA)
        {
            System.Data.Common.DbCommandBuilder RetValue = null;

            switch (iDBType)
            {
                case EnumDBType.OleDB:
                    RetValue = new System.Data.OleDb.OleDbCommandBuilder((System.Data.OleDb.OleDbDataAdapter)DA);
                    break;
                case EnumDBType.SqlClient:
                    RetValue = new System.Data.SqlClient.SqlCommandBuilder((System.Data.SqlClient.SqlDataAdapter)DA);
                    break;
            }

            return RetValue;
        }

        private static System.Data.Common.DbDataAdapter GetDBObjDataAdapter()
        {
            System.Data.Common.DbDataAdapter RetValue = null;

            switch (iDBType)
            {
                case EnumDBType.OleDB:
                    RetValue = new System.Data.OleDb.OleDbDataAdapter();
                    break;
                case EnumDBType.SqlClient:
                    RetValue = new System.Data.SqlClient.SqlDataAdapter();
                    break;
            }

            return RetValue;
        }

        public class TransactionDB : IDisposable
        {
            private string iConnectionString = string.Empty;
            private System.Data.Common.DbConnection iDBConn = null;
            private System.Data.Common.DbTransaction iDBTrans = null;

            public string ConnectionString
            {
                get
                {
                    return iConnectionString;
                }
            }

            public TransactionDB(string DBConnStr)
            {
                bool Success = false;

                iConnectionString = DBConnStr;
                iDBConn = GetDBObjConnection();
                iDBConn.ConnectionString = DBConnStr;

                try
                {
                    iDBConn.Open();
                    iDBTrans = iDBConn.BeginTransaction();
                }
                catch (Exception ex)
                {
                    if (iDBTrans != null)
                    {
                        try
                        {
                            iDBTrans.Dispose();
                        }
                        catch (Exception ex2)
                        {
                        }
                    }

                    if (iDBConn != null)
                    {
                        try
                        {
                            iDBConn.Close();
                        }
                        catch (Exception ex2)
                        {
                        }

                        try
                        {
                            iDBConn.Dispose();
                        }
                        catch (Exception ex2)
                        {
                        }
                    }

                    iDBConn = null;
                    iDBTrans = null;

                    throw ex;
                }
            }

            public int ExecuteDB(string SS)
            {
                System.Data.Common.DbCommand DBCmd;

                DBCmd = GetDBObjCommand();

                DBCmd.CommandText = SS;
                DBCmd.CommandType = System.Data.CommandType.Text;

                return ExecuteDB(DBCmd);
            }

            public int ExecuteDB(System.Data.Common.DbCommand Cmd)
            {
                int RetValue;

                Cmd.Connection = iDBConn;
                Cmd.Transaction = iDBTrans;

                try
                {
                    RetValue = Cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                return RetValue;
            }

            public object GetDBValue(string SS)
            {
                System.Data.Common.DbCommand DBCmd;

                DBCmd = GetDBObjCommand();
                DBCmd.CommandType = System.Data.CommandType.Text;
                DBCmd.CommandText = SS;

                return GetDBValue(DBCmd);
            }

            public object GetDBValue(System.Data.Common.DbCommand DBCmd)
            {
                object RetValue;

                DBCmd.Connection = iDBConn;
                DBCmd.Transaction = iDBTrans;

                try
                {
                    RetValue = DBCmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                return RetValue;
            }

            public void Commit()
            {
                if (iDBTrans != null)
                    iDBTrans.Commit();

                this.Dispose();
            }

            public void Rollback()
            {
                if (iDBTrans != null)
                    iDBTrans.Rollback();

                this.Dispose();
            }

            private bool disposedValue; // 偵測多餘的呼叫

            // IDisposable
            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: 處置 Managed 狀態 (Managed 物件)。
                        if (iDBTrans != null)
                        {
                            try
                            {
                                iDBTrans.Dispose();
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        if (iDBConn != null)
                        {
                            try
                            {
                                iDBConn.Close();
                            }
                            catch (Exception ex)
                            {
                            }

                            try
                            {
                                iDBConn.Dispose();
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }
                disposedValue = true;
            }

            // TODO: 只有當上方的 Dispose(disposing As Boolean) 具有要釋放 Unmanaged 資源的程式碼時，才覆寫 Finalize()。
            // Protected Overrides Sub Finalize()
            // ' 請勿變更這個程式碼。請將清除程式碼放在上方的 Dispose(disposing As Boolean) 中。
            // Dispose(False)
            // MyBase.Finalize()
            // End Sub

            // Visual Basic 加入這個程式碼的目的，在於能正確地實作可處置的模式。
            public void Dispose()
            {
                // 請勿變更這個程式碼。請將清除程式碼放在上方的 Dispose(disposing As Boolean) 中。
                Dispose(true);
            }
        }
    }

    #region Redis
    public static class RedisCache
    {
        public static class ProviderService
        {
            private static string XMLPath = "ProviderService";
            private static int DBIndex = 0;

            public static System.Data.DataTable GetProviderService(string ProviderCode, string ServiceType, string CurrencyType)
            {
                string Key1;
                System.Data.DataTable DT;

                Key1 = XMLPath + ":" + ProviderCode + "." + ServiceType + "." + CurrencyType;
                if (KeyExists(DBIndex, Key1))
                {
                    DT = DTReadFromRedis(DBIndex, Key1);
                }
                else
                {
                    DT = UpdateProviderService(ProviderCode, ServiceType, CurrencyType);
                }

                return DT;
            }

            public static System.Data.DataTable UpdateProviderService(string ProviderCode, string ServiceType, string CurrencyType)
            {
                string SS;
                System.Data.SqlClient.SqlCommand DBCmd;
                System.Data.DataTable DT;
                string Key;

                Key = XMLPath + ":" + ProviderCode + "." + ServiceType + "." + CurrencyType;

                SS = "SELECT * FROM ProviderService WITH (NOLOCK) WHERE ProviderCode=@ProviderCode AND ServiceType=@ServiceType AND CurrencyType=@CurrencyType";
                DBCmd = new System.Data.SqlClient.SqlCommand();
                DBCmd.CommandText = SS;
                DBCmd.CommandType = System.Data.CommandType.Text;
                DBCmd.Parameters.Add("@ProviderCode", System.Data.SqlDbType.VarChar).Value = ProviderCode;
                DBCmd.Parameters.Add("@ServiceType", System.Data.SqlDbType.VarChar).Value = ServiceType;
                DBCmd.Parameters.Add("@CurrencyType", System.Data.SqlDbType.VarChar).Value = CurrencyType;
                DT = DBAccess.GetDB(DBConnStr, DBCmd);

                if (DT.Rows.Count > 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            DTWriteToRedis(DBIndex, DT, Key);
                            break;
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }

                return DT;
            }

            public static void DeleteProviderService(string ProviderCode, string ServiceType, string CurrencyType)
            {
                string Key1;

                Key1 = XMLPath + ":" + ProviderCode + "." + ServiceType + "." + CurrencyType;
                if (KeyExists(DBIndex, Key1))
                {
                    KeyDelete(DBIndex, Key1);
                }
            }
        }

        public static class ProviderCode
        {
            private static string XMLPath = "ProviderCode";
            private static int DBIndex = 0;

            public static System.Data.DataTable GetProviderCode(string ProviderCode)
            {
                string Key1;
                System.Data.DataTable DT;

                Key1 = XMLPath + ":" + ProviderCode;
                if (KeyExists(DBIndex, Key1))
                {
                    DT = DTReadFromRedis(DBIndex, Key1);
                }
                else
                {
                    DT = UpdateProviderCode(ProviderCode);
                }

                return DT;
            }

            public static System.Data.DataTable UpdateProviderCode(string ProviderCode)
            {
                string SS;
                System.Data.SqlClient.SqlCommand DBCmd;
                System.Data.DataTable DT;
                string Key;

                Key = XMLPath + ":" + ProviderCode.ToString();

                SS = "SELECT * FROM ProviderCode WITH (NOLOCK) WHERE ProviderCode=@ProviderCode";
                DBCmd = new System.Data.SqlClient.SqlCommand();
                DBCmd.CommandText = SS;
                DBCmd.CommandType = System.Data.CommandType.Text;
                DBCmd.Parameters.Add("@ProviderCode", System.Data.SqlDbType.VarChar).Value = ProviderCode;
                DT = DBAccess.GetDB(DBConnStr, DBCmd);

                if (DT.Rows.Count > 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            DTWriteToRedis(DBIndex, DT, Key);
                            break;
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }

                return DT;
            }
        }

        [System.Web.Services.WebMethod]
        [System.Web.Script.Services.ScriptMethod(ResponseFormat = System.Web.Script.Services.ResponseFormat.Json)]
        public static string HeartBeat(string test)
        {
            return test;
        }

        public static void DTWriteToRedis(int DBIndex, System.Data.DataTable DT, string Key, int ExpireTimeoutSeconds = 0)
        {
            string XMLContent;

            XMLContent = DTSerialize(DT);
            RedisWrite(DBIndex, Key, XMLContent, ExpireTimeoutSeconds);
        }

        public static void DSWriteToRedis(int DBIndex, System.Data.DataSet DS, string Key, int ExpireTimeoutSeconds = 0)
        {
            string XMLContent;

            XMLContent = DSSerialize(DS);
            RedisWrite(DBIndex, Key, XMLContent, ExpireTimeoutSeconds);
        }

        public static System.Data.DataTable DTReadFromRedis(int DBIndex, string Key)
        {
            string XMLContent;

            XMLContent = RedisRead(DBIndex, Key);

            return DTDeserialize(XMLContent);
        }

        public static System.Data.DataSet DSReadFromRedis(int DBIndex, string Key)
        {
            string XMLContent;

            XMLContent = RedisRead(DBIndex, Key);

            return DSDeserialize(XMLContent);
        }

        public static string DTSerialize(System.Data.DataTable _dt)
        {
            string result = string.Empty;

            if (_dt != null)
            {
                System.IO.StringWriter writer = new System.IO.StringWriter();

                if (string.IsNullOrEmpty(_dt.TableName))
                {
                    _dt.TableName = "Datatable";
                }

                _dt.WriteXml(writer, System.Data.XmlWriteMode.WriteSchema);
                result = writer.ToString();
            }

            return result;
        }

        public static string DSSerialize(System.Data.DataSet _ds)
        {
            string result = string.Empty;

            if (_ds != null)
            {
                System.IO.StringWriter writer = new System.IO.StringWriter();
                int I = 0;

                foreach (System.Data.DataTable EachTable in _ds.Tables)
                {
                    I += 1;

                    if (string.IsNullOrEmpty(EachTable.TableName))
                    {
                        EachTable.TableName = "Datatable." + I.ToString();
                    }
                }

                _ds.WriteXml(writer, System.Data.XmlWriteMode.WriteSchema);
                result = writer.ToString();
            }

            return result;
        }

        public static System.Data.DataTable DTDeserialize(string _strData)
        {
            if (string.IsNullOrEmpty(_strData) == false)
            {
                System.Data.DataTable DT = new System.Data.DataTable();
                System.IO.StringReader StringStream = new System.IO.StringReader(_strData);

                DT.ReadXml(StringStream);

                return DT;
            }
            else
            {
                return null;
            }
        }

        public static System.Data.DataSet DSDeserialize(string _strData)
        {
            if (string.IsNullOrEmpty(_strData) == false)
            {
                System.Data.DataSet DS = new System.Data.DataSet();
                System.IO.StringReader StringStream = new System.IO.StringReader(_strData);

                DS.ReadXml(StringStream);

                return DS;
            }
            else
            {
                return null;
            }
        }

        public static void KeyDelete(int DBIndex, string Key)
        {
            StackExchange.Redis.IDatabase Client = GetRedisClient(DBIndex);

            Client.KeyDelete(Key.ToUpper());
        }

        public static bool KeyExists(int DBIndex, string Key)
        {
            StackExchange.Redis.IDatabase Client = GetRedisClient(DBIndex);

            return Client.KeyExists(Key.ToUpper());
        }

        public static void RedisSetExpire(int DBIndex, string Key, int ExpireTimeoutSecond)
        {
            StackExchange.Redis.IDatabase Client = GetRedisClient(DBIndex);

            Client.KeyExpire(Key.ToUpper(), new TimeSpan(0, 0, ExpireTimeoutSecond));
        }

        public static void RedisWrite(int DBIndex, string Key, string Content, int ExpireTimeoutSecond = 0)
        {
            StackExchange.Redis.IDatabase Client = GetRedisClient(DBIndex);

            if (ExpireTimeoutSecond == 0)
            {
                Client.StringSet(Key.ToUpper(), Content);
            }
            else
            {
                StackExchange.Redis.ITransaction T = Client.CreateTransaction();

                T.StringSetAsync(Key.ToUpper(), Content);
                T.KeyExpireAsync(Key.ToUpper(), new TimeSpan(0, 0, ExpireTimeoutSecond));
                T.Execute();

                T = null;
            }
        }

        public static string RedisRead(int DBIndex, string Key)
        {
            StackExchange.Redis.IDatabase Client = GetRedisClient(DBIndex);
            string RetValue = string.Empty;


            if (Client.KeyExists(Key.ToUpper()))
            {
                RetValue = Client.StringGet(Key.ToUpper()).ToString();
            }

            return RetValue;
        }

        public static void RedisPushToList(int DBIndex, string Key, string Value)
        {
            StackExchange.Redis.IDatabase Client = GetRedisClient(DBIndex);
            string RetValue = string.Empty;

            Client.ListLeftPush(Key, Value);

        }

        public static void RedisRemoveFromList(int DBIndex, string Key, string Value)
        {
            StackExchange.Redis.IDatabase Client = GetRedisClient(DBIndex);
            string RetValue = string.Empty;

            Client.ListRemove(Key, Value);

        }

        public static bool RedisCheckValueInList(int DBIndex, string Key, string Value)
        {
            StackExchange.Redis.IDatabase Client = GetRedisClient(DBIndex);

            //StackExchange.Redis.RedisValue[] RetValue = null;
            bool RetValue = false;
            int count = 0;
            if (Client.ListLength(Key) > 0)
                count = Client.ListRange(Key).ToList().Where(x => x.ToString().IndexOf(Value) > -1).Count();
            if (count > 0)
                RetValue = true;

            return RetValue;
        }

        public static dynamic RedisGetList(int DBIndex, string Key)
        {
            StackExchange.Redis.IDatabase Client = GetRedisClient(DBIndex);

            StackExchange.Redis.RedisValue[] RetValue = null;

            if (Client.ListLength(Key) > 0)
                RetValue = Client.ListRange(Key);

            return RetValue;
        }


        public static bool RedisHashExists(int DBIndex, string Key, string HashName)
        {
            StackExchange.Redis.IDatabase Client = GetRedisClient(DBIndex);

            return Client.HashExists(Key.ToUpper(), HashName.ToUpper());
        }

        public static void RedisHashDelete(int DBIndex, string Key, string HashName)
        {
            StackExchange.Redis.IDatabase Client = GetRedisClient(DBIndex);

            Client.HashDelete(Key.ToUpper(), HashName.ToUpper());
        }

        public static void RedisHashWrite(int DBIndex, string Key, string HashName, string Content, int ExpireTimeoutSecond = 0)
        {
            StackExchange.Redis.IDatabase Client = GetRedisClient(DBIndex);

            if (ExpireTimeoutSecond == 0)
            {
                Client.HashSet(Key.ToUpper(), HashName.ToUpper(), Content);
            }
            else
            {
                StackExchange.Redis.ITransaction T = Client.CreateTransaction();

                T.HashSetAsync(Key.ToUpper(), HashName.ToUpper(), Content);
                T.KeyExpireAsync(Key.ToUpper(), new TimeSpan(0, 0, ExpireTimeoutSecond));
                T.Execute();

                T = null;
            }
        }

        public static string RedisHashRead(int DBIndex, string Key, string HashName)
        {
            StackExchange.Redis.IDatabase Client = GetRedisClient(DBIndex);
            string RetValue = string.Empty;

            if (Client.KeyExists(Key.ToUpper()))
            {
                RetValue = Client.HashGet(Key.ToUpper(), HashName.ToUpper());
            }

            return RetValue;
        }

        public static StackExchange.Redis.HashEntry[] RedisHashReadAll(int DBIndex, string Key)
        {
            StackExchange.Redis.IDatabase Client = GetRedisClient(DBIndex);
            StackExchange.Redis.HashEntry[] RetValue = null;

            if (Client.KeyExists(Key.ToUpper()))
            {
                RetValue = Client.HashGetAll(Key.ToUpper());
            }

            return RetValue;
        }

        public static void RedisAddSortedRange(int DBIndex, string Key, string Value, long score)
        {

            StackExchange.Redis.IDatabase Client = GetRedisClient(DBIndex);

            Client.SortedSetAdd(Key.ToUpper(), Value, score, StackExchange.Redis.When.NotExists);

        }

        public static StackExchange.Redis.RedisValue[] RedisGetSortedRange(int DBIndex, string Key, long score)
        {

            StackExchange.Redis.IDatabase Client = GetRedisClient(DBIndex);
            StackExchange.Redis.RedisValue[] RetValue = null;

            if (Client.KeyExists(Key.ToUpper()))
            {
                RetValue = Client.SortedSetRangeByScore(Key.ToUpper(), score, Double.MaxValue, StackExchange.Redis.Exclude.None, StackExchange.Redis.Order.Ascending, 0, 1, StackExchange.Redis.CommandFlags.None);
            }

            return RetValue;
        }

        public static void RedisEnqueue(int DBIndex, string Key, string Content)
        {
            StackExchange.Redis.IDatabase Client = GetRedisClient(DBIndex);

            Client.ListRightPush(Key.ToUpper(), Content);
        }

        public static string RedisDequeue(int DBIndex, string Key)
        {
            StackExchange.Redis.IDatabase Client = GetRedisClient(DBIndex);
            string RetValue = null;

            if (Client.KeyExists(Key.ToUpper()))
            {
                RetValue = Client.ListLeftPop(Key.ToUpper()).ToString();
            }

            return RetValue;
        }
    }

    private static StackExchange.Redis.ConnectionMultiplexer RedisClient = null;

    public static StackExchange.Redis.IDatabase GetRedisClient(int db = -1)
    {
        StackExchange.Redis.IDatabase RetValue;

        RedisPrepare();

        if (db == -1)
        {
            RetValue = RedisClient.GetDatabase();
        }
        else
        {
            RetValue = RedisClient.GetDatabase(db);
        }

        return RetValue;
    }

    private static void RedisPrepare()
    {
        if (RedisClient == null)
        {
            RedisClient = StackExchange.Redis.ConnectionMultiplexer.Connect(WebRedisConnStr);
        }
    } 
    #endregion
}