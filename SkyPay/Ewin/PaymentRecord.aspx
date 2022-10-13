<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Common.cs" Inherits="Common" %>

<%
    string PostBody;
    
    using (System.IO.StreamReader reader = new System.IO.StreamReader(Request.InputStream))
    {
        PostBody = reader.ReadToEnd();
    };

    System.Data.DataTable PaymentOrderDT;
    APIResult R = new APIResult() { ResultState = APIResult.enumResultCode.ERR };

    if (!string.IsNullOrEmpty(PostBody))
    {

    }
    else
    {
        //R.ResultState = APIResult.enumResultCode.ERR;
        //R.Message = "No Data";
    }

    //Response.Write(R.Message);
    //Response.Flush();
    //Response.End();
%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta charset="UTF-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <meta http-equiv="cache-control" content="no-cache" />
    <meta http-equiv="pragma" content="no-cache" />
    <title></title>


<link rel="stylesheet" href="/VPay/assets/plugins/bootstrap/css/bootstrap.min.css">
    <!-- Bootstrap Select Css -->
    <link href="/VPay/assets/plugins/bootstrap-select/css/bootstrap-select.css" rel="stylesheet" />

    <!-- Custom Css -->
    <link rel="stylesheet" href="/VPay/assets/css/main.css">
    <!-- Jquery Core Js -->
    <script src="/VPay/assets/js/jquery-3.3.1.min.js"></script>
    <script src="/VPay/assets/bundles/libscripts.bundle.js"></script> <!--Lib Scripts Plugin Js ( jquery.v3.2.1, Bootstrap4 js)-->



    <script src="/VPay/assets/js/BackendJS/BackendAPI.js?20200508"></script>
    <script src="/VPay/assets/js/AutoNumeric.js"></script>
    <style>
        * {
            font-family: 微軟正黑體, 微軟雅黑體, Arial;
        }
    </style>
</head>
<script>
    $(function () {
        $('#WithdrawalModal').modal('show');
    });
</script>
<body>
    <div class="modal fade" style="background-color: rgba(0, 0, 0, 0.4);" data-backdrop="false" id="WithdrawalModal" tabindex="-1" role="dialog" aria-hidden="true">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="title" id="WithdrawalModalModalLabel">充值单</h4>
                </div>
                <div class="modal-body">
                    <span style="display: block">平台订单号:<span id="modal_CompanyName" style="margin-left: 5px"></span></span>
                    <span style="display: block">商户订单号:<span id="modal_WithdrawSerial" style="margin-left: 5px"></span></span>
                    <span style="display: block">渠道:<span id="modal_BankName" style="margin-left: 5px"></span></span>
                    <span style="display: block">支付方式:<span id="modal_BankBranchName" style="margin-left: 5px"></span></span>
                    <span style="display: block">状态:<span id="modal_BankCard" style="margin-left: 5px"></span></span>
                    <span style="display: block">申请金额:<span id="modal_BankCardName" style="margin-left: 5px"></span></span>
                    <span style="display: block">所属城市:<span id="modal_OwnCity" style="margin-left: 5px"></span></span>
                    <span style="display: block">费率(%):<span id="modal_OwnProvince" style="margin-left: 5px"></span></span>
                    <span style="display: block">手续费:<span id="modal_CurrencyType" style="margin-left: 5px"></span></span>
                    <span style="display: block">提交时间:<span id="modal_CreateDate" style="margin-left: 5px"></span></span>
                    <span style="display: block">完成时间:<span id="modal_FinishDate" style="margin-left: 5px"></span></span>
                    <span style="display: block">IP:<span id="modal_IP" style="margin-left: 5px"></span></span>
                </div>
                <div class="modal-footer">
      
                </div>
            </div>
        </div>
    </div>
</body>
</html>


