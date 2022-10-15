using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Collections;
using System.Data.SqlClient;
using System.Data;


public partial class ProviderList: System.Web.UI.Page
{
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public static string HeartBeat(string test)
    {
        return test;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public static APIResult ChangeProviderServiceState(string ServiceType, string CurrencyType,string ProviderCode)
    {
        APIResult retValue = new APIResult();

        if (Common.ChangeProviderServiceState(ProviderCode, ServiceType, CurrencyType) > 0)
        {
            retValue.ResultCode = APIResult.enumResult.OK;
        }
        else
        {
            retValue.ResultCode = APIResult.enumResult.Error;
        }

        return retValue;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public static APIResult ChangeProviderCodeState(string ProviderCode)
    {
        APIResult retValue = new APIResult();


        if (Common.ChangeProviderCodeState(ProviderCode) > 0)
        {
            retValue.ResultCode = APIResult.enumResult.OK;
        }
        else
        {
            retValue.ResultCode = APIResult.enumResult.Error;
        }

        return retValue;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public static APIResult ChangeProviderAPIType(string ProviderCode,int setAPIType)
    {
        APIResult retValue = new APIResult();
     
        System.Data.DataTable DT =Common.RedisCache.ProviderCode.GetProviderCode(ProviderCode);

        if (((int)DT.Rows[0]["ProviderAPIType"] & setAPIType) == setAPIType)
        {
            setAPIType = 0 - setAPIType;
        }
  
        if (Common.ChangeProviderAPIType(ProviderCode, setAPIType) > 0)
        {
            retValue.ResultCode = APIResult.enumResult.OK;
        }
        else
        {
            retValue.ResultCode = APIResult.enumResult.Error;
        }

        return retValue;
    }

    public class APIResult
    {
        public enum enumResult
        {
            OK = 0,
            LoginAccountEmpty,
            PasswordEmpty,
            CompanyCodeNotExist,
            VerificationError,
            NoData,
            Error,
            SessionError,
            DataExist,
            DataDuplicate, //資料重複
            UplineMaxDaliyAmountByUseError,
            MaxDaliyAmountByUseError,
            OfflineMaxDaliyAmountByUseError,
            GoogleKeyEmpty,
            GoogleKeyError,
            CompanyPointError,
            Other = 99
        }
        public enumResult ResultCode;
        public string Message;
    }
}
