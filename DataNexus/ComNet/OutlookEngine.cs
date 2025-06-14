using Microsoft.Office.Interop.Outlook;
using OfficeOpenXml.Table.PivotTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataNexus.ComNet
{
    public class OutlookEngine
    {
        public string recipient { get; set; }
        public string subject { get; set; }
        public string shipper { get; set; }
        public OutlookEngine(string _shipper, string _recipient, string _subject)
        {
            if (string.IsNullOrWhiteSpace(shipper))
            {
                throw new System.Exception("Shipper cannot be null or empty for SendEmail2.");
            }
            shipper = _shipper;
            recipient = _recipient;
            subject = _subject;
        }

        public OutlookEngine(string recipient, string subject): this(null, recipient, subject)
        {
        }

        public void SendEmail1(string content)
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

        public void SendEmail2(string content)
        {
            // 替換為實際的 SMTP 伺服器和端口
            SmtpClient client = new SmtpClient("mail.deep-wise.com.tw", 25); 

            // 設置 SMTP 用戶名和密碼
            client.Credentials = new NetworkCredential("chimingkuei@deep-wise.com.tw", "7@2wbxD6");
            client.EnableSsl = false;

            // 創建郵件對象
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(shipper);
            mail.To.Add(recipient);
            mail.Subject = subject;
            mail.Body = content;
            client.Send(mail);
        }
    }
}
