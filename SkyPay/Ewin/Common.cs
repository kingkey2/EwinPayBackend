using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;

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

    public static bool CheckProviderListSign(dynamic Data)
    {
        bool checkbool = false;
        string CompanyKey;
        string CompanyCode = (string)Data.CompanyCode;
        CompanyKey = GetCompanyKeyByCompanyCode(CompanyCode);
        string signStr = "CompanyCode=" + CompanyCode;
        signStr += "&CompanyKey=" + CompanyCode;

        string Sign = GetMD5(signStr, false);

        if (Sign.ToUpper() == ((string)Data.Sign).ToUpper())
        {
            checkbool = true;
        }
        return checkbool;
    }

    public static bool CheckSign(string CompanyCode,string OrderID,string Sign)
    {
        bool checkbool = false;
        string CompanyKey;

        CompanyKey = GetCompanyKeyByCompanyCode(CompanyCode);
        string signStr = "CompanyCode=" + CompanyCode;
        signStr += "&OrderID=" +OrderID;
        signStr += "&CompanyKey=" + CompanyCode;

        string _Sign = GetMD5(signStr, false);

        if (_Sign.ToUpper() == Sign.ToUpper())
        {
            checkbool = true;
        }
        return checkbool;
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

    public static Withdrawal GetWithdrawalByOrderID(string OrderID)
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
        SS += " WHERE  Withdrawal.DownOrderID= @DownOrderID  ";
        DBCmd = new SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = System.Data.CommandType.Text;
        DBCmd.Parameters.Add("@DownOrderID", SqlDbType.VarChar).Value = OrderID;
        DT = DBAccess.GetDB(DBConnStr, DBCmd);

        if (DT != null)
        {
            if (DT.Rows.Count > 0)
            {
                returnValue = new Withdrawal();
                returnValue.WithdrawSerial = (string)DT.Rows[0]["WithdrawSerial"];
                returnValue.DownOrderID = (string)DT.Rows[0]["DownOrderID"];
                returnValue.ProviderName = (string)DT.Rows[0]["ProviderName"];
                returnValue.Status = (int)DT.Rows[0]["Status"];
                returnValue.WithdrawRate = (decimal)DT.Rows[0]["WithdrawRate"];
                returnValue.DecimalPlaces = (int)DT.Rows[0]["DecimalPlaces"];
                returnValue.ProviderName = (string)DT.Rows[0]["ProviderName"];
                returnValue.CreateDate2 = (string)DT.Rows[0]["CreateDate2"];
                returnValue.FinishDate2 = (string)DT.Rows[0]["FinishDate2"];
                returnValue.WithdrawType = (int)DT.Rows[0]["WithdrawType"];
                returnValue.BankCard = (string)DT.Rows[0]["BankCard"];
                returnValue.forCompanyID= (int)DT.Rows[0]["forCompanyID"];
                returnValue.BankCardName = (string)DT.Rows[0]["BankCardName"];
                returnValue.CurrencyType = (string)DT.Rows[0]["CurrencyType"];
                returnValue.BankName = (string)DT.Rows[0]["BankName"];
                returnValue.Amount = (decimal)DT.Rows[0]["Amount"];
                returnValue.OwnProvince = (string)DT.Rows[0]["OwnProvince"];
                returnValue.BankBranchName = (string)DT.Rows[0]["BankBranchName"];
                returnValue.CollectCharge = (decimal)DT.Rows[0]["CollectCharge"];
                returnValue.FloatType = (int)DT.Rows[0]["FloatType"];
                returnValue.OwnCity = (string)DT.Rows[0]["OwnCity"];
                returnValue.FinishAmount = (decimal)DT.Rows[0]["FinishAmount"];
                returnValue.DownStatus = (int)DT.Rows[0]["DownStatus"];
                returnValue.CompanyName = (string)DT.Rows[0]["CompanyName"];

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
                    _ProviderListResult.ProviderCode = (string)DT.Rows[i]["ProviderCode"];
                    _ProviderListResult.ProviderName = (string)DT.Rows[i]["ProviderName"];
                    _ProviderListResult.ProviderAPIType = (int)DT.Rows[i]["ProviderAPIType"];
                    _ProviderListResult.ProviderState = (int)DT.Rows[i]["ProviderState"];
                    _ProviderListResult.MaxLimit = (decimal)DT.Rows[i]["MaxLimit"];
                    _ProviderListResult.MinLimit = (decimal)DT.Rows[i]["MinLimit"];
                    _ProviderListResult.Charge = (decimal)DT.Rows[i]["Charge"];
                    _ProviderListResult.CurrencyType = (string)DT.Rows[i]["CurrencyType"];
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
                    _ServiceData.ServiceTypeName = (string)DT.Rows[i]["ServiceTypeName"];
                    _ServiceData.MaxDaliyAmount = (decimal)DT.Rows[i]["MaxDaliyAmount"];
                    _ServiceData.MaxOnceAmount = (decimal)DT.Rows[i]["MaxOnceAmount"];
                    _ServiceData.MinOnceAmount = (decimal)DT.Rows[i]["MinOnceAmount"];
                    _ServiceData.CostCharge = (decimal)DT.Rows[i]["CostCharge"];
                    _ServiceData.CostRate = (decimal)DT.Rows[i]["CostRate"];
                    _ServiceData.ServiceType = (string)DT.Rows[i]["ServiceType"];
                    _ServiceData.ProviderCode = (string)DT.Rows[i]["ProviderCode"];
                    _ServiceData.CurrencyType = (string)DT.Rows[i]["CurrencyType"];
                    _ServiceData.State = (int)DT.Rows[i]["State"];
                    _ServiceData.CheckoutType = (int)DT.Rows[i]["CheckoutType"];
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
                    _ProviderPointVM.ProviderName = (string)DT.Rows[i]["ProviderName"];
                    _ProviderPointVM.ProviderAPIType = (int)DT.Rows[i]["ProviderAPIType"];
                    _ProviderPointVM.ProviderCode = (string)DT.Rows[i]["ProviderCode"];
                    _ProviderPointVM.TotalDepositePointValue = (decimal)DT.Rows[i]["TotalDepositePointValue"];
                    _ProviderPointVM.TotalProfitPointValue = (decimal)DT.Rows[i]["TotalProfitPointValue"];
                    _ProviderPointVM.SystemPointValue = (decimal)DT.Rows[i]["SystemPointValue"];
                    _ProviderPointVM.ProviderFrozenAmount = (decimal)DT.Rows[i]["ProviderFrozenAmount"];
                    _ProviderPointVM.WithdrawProfit = (decimal)DT.Rows[i]["WithdrawProfit"];
                    _ProviderPointVM.WithdrawPoint = (decimal)DT.Rows[i]["WithdrawPoint"];
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
                returnValue.PaymentSerial = (string)DT.Rows[0]["PaymentSerial"];
                returnValue.OrderID = (string)DT.Rows[0]["OrderID"];
                returnValue.ProviderName = (string)DT.Rows[0]["ProviderName"];
                returnValue.ServiceTypeName = (string)DT.Rows[0]["ServiceTypeName"];
                returnValue.ProcessStatus = (int)DT.Rows[0]["ProcessStatus"];
                returnValue.OrderAmount = (decimal)DT.Rows[0]["OrderAmount"];
                returnValue.PaymentAmount = (decimal)DT.Rows[0]["PaymentAmount"];
                returnValue.CostRate = (decimal)DT.Rows[0]["CostRate"];
                returnValue.CostCharge = (decimal)DT.Rows[0]["CostCharge"];
                returnValue.CollectRate = (decimal)DT.Rows[0]["CollectRate"];
                returnValue.CollectCharge = (decimal)DT.Rows[0]["CollectCharge"];
                returnValue.CreateDate2 = (string)DT.Rows[0]["CreateDate2"];
                returnValue.FinishDate2 = (string)DT.Rows[0]["FinishDate2"];
                returnValue.PartialOrderAmount = (decimal)DT.Rows[0]["PartialOrderAmount"];
                returnValue.UserIP = (string)DT.Rows[0]["UserIP"];
 
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
                    _CompanyServicePointVM.CanUsePoint = (decimal)DT.Rows[i]["CanUsePoint"];
                    _CompanyServicePointVM.Charge = (decimal)DT.Rows[i]["Charge"];
                    _CompanyServicePointVM.CompanyID = (int)DT.Rows[i]["CompanyID"];
                    _CompanyServicePointVM.CompanyName = (string)DT.Rows[i]["CompanyName"];
                    _CompanyServicePointVM.CurrencyType = (string)DT.Rows[i]["CurrencyType"];
                    _CompanyServicePointVM.FrozenPoint = (decimal)DT.Rows[i]["FrozenPoint"];
                    _CompanyServicePointVM.FrozenServiceCount = (int)DT.Rows[i]["FrozenServiceCount"];
                    _CompanyServicePointVM.FrozenServicePoint = (decimal)DT.Rows[i]["FrozenServicePoint"];
                    _CompanyServicePointVM.MaxLimit = (decimal)DT.Rows[i]["MaxLimit"];

                    _CompanyServicePointVM.MinLimit = (decimal)DT.Rows[i]["MinLimit"];
                    _CompanyServicePointVM.ServiceType = (string)DT.Rows[i]["ServiceType"];
                    _CompanyServicePointVM.ServiceTypeName = (string)DT.Rows[i]["ServiceTypeName"];
                    _CompanyServicePointVM.State = (int)DT.Rows[i]["State"];
                    _CompanyServicePointVM.SystemPointValue = (decimal)DT.Rows[i]["SystemPointValue"];
                    _CompanyServicePointVM.WithdrawalPoint = (decimal)DT.Rows[i]["WithdrawalPoint"];
                    returnValue.Add(_CompanyServicePointVM);
                }
            }
        }

        return returnValue;
    }

    public static List<ProviderPointVM> GetAllProviderPointByCompanyID(int CompanyID)
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
             " WHERE PC.ProviderState = 0 And PC.forCompanyID=@CompanyID ";

        DBCmd = new SqlCommand();
        DBCmd.CommandText = SS;
        DBCmd.CommandType = System.Data.CommandType.Text;
        DBCmd.Parameters.Add("@CompanyID", System.Data.SqlDbType.Int).Value = CompanyID;
        DT = DBAccess.GetDB(DBConnStr, DBCmd);
        if (DT != null)
        {
            if (DT.Rows.Count > 0)
            {
                returnValue = new List<ProviderPointVM>();
                for (int i = 0; i < DT.Rows.Count; i++)
                {
                    ProviderPointVM _ProviderPointVM = new ProviderPointVM();
                    _ProviderPointVM.CurrencyType = (string)DT.Rows[i]["CurrencyType"];
                    _ProviderPointVM.exMessage = (string)DT.Rows[i]["exMessage"];
                    _ProviderPointVM.ProviderAPIType = (int)DT.Rows[i]["ProviderAPIType"];
                    _ProviderPointVM.ProviderCode = (string)DT.Rows[i]["ProviderCode"];
                    _ProviderPointVM.ProviderFrozenAmount = (decimal)DT.Rows[i]["ProviderFrozenAmount"];
                    _ProviderPointVM.ProviderName = (string)DT.Rows[i]["ProviderName"];
                    _ProviderPointVM.ProviderPointValue = (decimal)DT.Rows[i]["ProviderPointValue"];
                    _ProviderPointVM.SystemPointValue = (decimal)DT.Rows[i]["SystemPointValue"];
                    _ProviderPointVM.TotalDepositePointValue = (decimal)DT.Rows[i]["TotalDepositePointValue"];

                    _ProviderPointVM.TotalProfitPointValue = (decimal)DT.Rows[i]["TotalProfitPointValue"];
                    _ProviderPointVM.WithdrawPoint = (decimal)DT.Rows[i]["WithdrawPoint"];
                    _ProviderPointVM.WithdrawProfit = (decimal)DT.Rows[i]["WithdrawProfit"];
                    returnValue.Add(_ProviderPointVM);
                }
            }
        }

        return returnValue;
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