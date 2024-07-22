using Microsoft.Office.Interop.Outlook;
using OfficeOpenXml.Table.PivotTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataSphereX.ComNet
{
    public class OutlookEngine
    {
        public void SendEmail1(string recipient, string subject, string content)
        {
            // 建立Outlook Application物件
            Application outlookApp = new Application();
            // 建立信件物件
            MailItem mail = (MailItem)outlookApp.CreateItem(OlItemType.olMailItem);
            // 設定信件的收件人、主旨、內容等資訊
            mail.To = recipient;// "chimingkuei@deep-wise.com.tw"
            mail.Subject = subject;// "測試郵件"
            mail.Body = content;// "Dear MK:\n這是一封測試郵件。"
            // 寄出信件
            mail.Send();
        }

        public void SendEmail2()
        {
            SmtpClient client = new SmtpClient("mail.deep-wise.com.tw", 25); // 替換為實際的 SMTP 伺服器和端口

            // 設置 SMTP 用戶名和密碼
            client.Credentials = new NetworkCredential("chimingkuei@deep-wise.com.tw", "7@2wbxD6"); // 替換為實際的用戶名和密碼
            client.EnableSsl = false;

            // 創建郵件對象
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("chimingkuei@deep-wise.com.tw"); // 替換為發件人地址
            mail.To.Add("chimingkuei@deep-wise.com.tw"); // 替換為收件人地址
            mail.Subject = "測試郵件";
            mail.Body = "這是一封測試郵件。";
            client.Send(mail);
        }
    }
}
