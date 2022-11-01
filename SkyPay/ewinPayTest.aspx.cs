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


public partial class ewinPayTest : System.Web.UI.Page
{
    public static string CompanyCode = "OCW_PHP";

    public static string CompanyKey = "a8513205c0044cb480d5c3c903271966";
    //測試Url
    //public static string EwinPayUrl = "http://epaybackend.dev4.mts.idv.tw/Ewin";
    public static string EwinPayUrl = "http://localhost:9458/Ewin";
    //正式Url
    public static string OfficialEwinPayUrl = "https://backend.ewin-pay.com/Ewin";

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public static string HeartBeat(string test)
    {
        return test;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public static string GetProviderList()
    {
        string Timestamp = ConvertUtcTimestamp(DateTime.UtcNow);
        string Sign = GetProviderListSign(Timestamp);
        string Url = EwinPayUrl + "/ProviderList.aspx?CompanyCode=" + CompanyCode + "&Timestamp=" + Timestamp + "&Sign=" + Sign;
        return Url;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public static string SetEPayCompanyService()
    {
        string Timestamp = ConvertUtcTimestamp(DateTime.UtcNow);
        string Sign = GetProviderListSign(Timestamp);
        string Url = EwinPayUrl + "/SetEPayCompanyService.aspx?CompanyCode=" + CompanyCode + "&Timestamp=" + Timestamp + "&Sign=" + Sign;
        return Url;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public static string WithdrawReview()
    {
        string Timestamp = ConvertUtcTimestamp(DateTime.UtcNow);
        //Ewin訂單號
        string OrderID = "PW2022101811420000001872";
        string Sign = GetWithdrawReviewSign(Timestamp, OrderID);
        string Url = EwinPayUrl + "/WithdrawReview.aspx?CompanyCode=" + CompanyCode + "&Timestamp=" + Timestamp + "&Sign=" + Sign+ "&OrderID="+ OrderID;
        return Url;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public static string WithdrawalRecord()
    {
        // 0=充值單 / 1=代付單
        int PaymentType = 1;
        string Timestamp = ConvertUtcTimestamp(DateTime.UtcNow);

        //Ewin訂單號
        string OrderID = "PW2022101811420000001872";
        string Sign = GetPaymentSign(Timestamp, OrderID, PaymentType);
        string Url = EwinPayUrl + "/PaymentRecord.aspx?CompanyCode=" + CompanyCode + "&Timestamp=" + Timestamp + "&Sign=" + Sign+ "&OrderID="+ OrderID+ "&PaymentType="+ PaymentType;
        return Url;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public static string PaymentRecord()
    {
        // 0=充值單 / 1=代付單
        int PaymentType = 0;
        string Timestamp = ConvertUtcTimestamp(DateTime.UtcNow);
        //Ewin訂單號
        string OrderID = "PD101N6651586486565813236320221014130432";
        string Sign = GetPaymentSign(Timestamp, OrderID, PaymentType);
        string Url = EwinPayUrl + "/PaymentRecord.aspx?CompanyCode=" + CompanyCode + "&Timestamp=" + Timestamp + "&Sign=" + Sign + "&OrderID=" + OrderID + "&PaymentType=" + PaymentType;
        return Url;
    }

    private static string GetProviderListSign(string Timestamp)
    {
        string signStr = "CompanyCode=" + CompanyCode;
        signStr += "&CompanyKey=" + CompanyKey;
        signStr += "&Timestamp=" + Timestamp;

        string _Sign = GetMD5(signStr, false);

        return _Sign;
    }

    private static string GetPaymentSign(string Timestamp,string OrderID,int PaymentType)
    {
        string signStr = "CompanyCode=" + CompanyCode;
        signStr += "&OrderID=" + OrderID;
        signStr += "&PaymentType=" + PaymentType;
        signStr += "&Timestamp=" + Timestamp;
        signStr += "&CompanyKey=" + CompanyKey;

        string _Sign = GetMD5(signStr, false);

        return _Sign;
    }

    private static string GetWithdrawReviewSign(string Timestamp,string OrderID)
    {
        string signStr = "CompanyCode=" + CompanyCode;
        signStr += "&OrderID=" + OrderID;
        signStr += "&Timestamp=" + Timestamp;
        signStr += "&CompanyKey=" + CompanyKey;

        string _Sign = GetMD5(signStr, false);

        return _Sign;
    }

    private static string ConvertUtcTimestamp(DateTime dateTime)
    {
        return ((long)(dateTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
    }

    private static string GetMD5(string DataString, bool Base64Encoding = true)
    {
        return GetMD5(System.Text.Encoding.UTF8.GetBytes(DataString), Base64Encoding);
    }

    private static string GetMD5(byte[] Data, bool Base64Encoding = true)
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
}
