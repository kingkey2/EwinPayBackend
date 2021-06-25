<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="wFrmRefresh.aspx.cs" Inherits="SkyPay.Backend.wFrmRefresh" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
<meta http-equiv="refresh" content="30"/>
    <script src="assets/js/jquery-3.3.1.min.js"></script>
   
    <script src="assets/js/BackendJS/BackendAPI.js?20200326"></script>


</head>    
 <script>
    $(document).ready(function () {
        pageLoad();
    });

     function pageLoad() {
         api = parent.api;
          api.updateBID(function (success, obj) {
            if (obj.ResultCode == 7) {
                errorAlert("(BID)您已斷線請重新登入", "Login.cshtml");
            }
        })
     }
 </script>
<body>
    <form id="form1" runat="server">
        <div>
        </div>
    </form>

</body>
  
</html>
