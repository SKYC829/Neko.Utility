using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;
using Neko.Utility.IO;
using System.Linq;

namespace Neko.Utility.Net
{
    /// <summary>
    /// 发送邮件帮助类
    /// </summary>
    public sealed class EmailUtil
    {
        /// <summary>
        /// 邮件对象
        /// </summary>
        private MailMessage _mail;

        /// <summary>
        /// 发信人
        /// </summary>
        private string _sender;

        /// <summary>
        /// 收信人列表
        /// </summary>
        private List<string> _receiver;

        /// <summary>
        /// 附件列表
        /// </summary>
        private List<FileInfo> _attachments;

        /// <summary>
        /// 发信人昵称
        /// </summary>
        public string SenderNickName { get; set; }

        /// <summary>
        /// 邮件标题
        /// </summary>
        public string MailTitle { get; set; }

        /// <summary>
        /// <inheritdoc cref="_receiver"/>
        /// </summary>
        public IEnumerable<string> Receivers { get => _receiver; }

        /// <summary>
        /// <inheritdoc cref="_attachments"/>
        /// </summary>
        public IEnumerable<FileInfo> Attachments { get => _attachments; }

        /// <summary>
        /// 发送邮件帮助类
        /// </summary>
        /// <param name="senderAddress">发信人邮箱</param>
        public EmailUtil(string senderAddress):this(senderAddress,string.Format("来自{0}的邮件", senderAddress))
        {

        }

        /// <summary>
        /// <inheritdoc cref="EmailUtil"/>
        /// </summary>
        /// <param name="senderAddress">发信人邮箱</param>
        /// <param name="mailTitle">邮件标题</param>
        public EmailUtil(string senderAddress,string mailTitle)
        {
            if (string.IsNullOrEmpty(senderAddress))
            {
                throw new ArgumentNullException(nameof(senderAddress), "发信人邮箱不允许为空!");
            }
            if (!RegularUtil.VerifyEmail(senderAddress))
            {
                throw new ArgumentException(nameof(senderAddress), "请输入正确的发信人邮箱!");
            }
            MailTitle = mailTitle;
            _sender = senderAddress;
            _mail = new MailMessage();
            _mail.IsBodyHtml = true;
            _mail.BodyEncoding = Encoding.UTF8;
            _mail.Priority = MailPriority.Normal;
            _receiver = new List<string>();
            _attachments = new List<FileInfo>();
        }

        /// <summary>
        /// 添加收信人
        /// </summary>
        /// <param name="receivers">收信人邮箱</param>
        public void AddReceiver(params string[] receivers)
        {
            if(receivers == null)
            {
                throw new ArgumentNullException(nameof(receivers), "收信人邮箱不允许为空!");
            }
            foreach (string receiver in receivers)
            {
                if (string.IsNullOrEmpty(receiver))
                {
                    throw new ArgumentNullException(nameof(receiver), "收信人邮箱不允许为空!");
                }
                if (!RegularUtil.VerifyEmail(receiver))
                {
                    throw new ArgumentException(nameof(receiver), "请输入正确的收信人邮箱!");
                }
                _mail.To.Add(receiver);
                _receiver.Add(receiver);
            }
        }

        /// <summary>
        /// 添加邮件内容
        /// </summary>
        /// <param name="message">邮件内容</param>
        /// <param name="args">邮件内容参数</param>
        public void AppendMessage(string message,params object[] args)
        {
            StringBuilder sb = new StringBuilder(_mail.Body);
            sb.AppendFormat(message, args);
            _mail.Body = sb.ToString();
        }

        /// <summary>
        /// 添加附件
        /// </summary>
        /// <param name="file">附件文件信息</param>
        public void AddAttachment(FileInfo file)
        {
            if(file == null)
            {
                throw new ArgumentNullException(nameof(file), "参数file不允许为空!");
            }

            if (!file.Exists)
            {
                throw new FileNotFoundException("文件不存在!无法添加至附件!", file.FullName);
            }
            Attachment attachment = new Attachment(file.FullName);
            ContentDisposition disposition = attachment.ContentDisposition;
            disposition.CreationDate = file.CreationTime;
            disposition.ModificationDate = file.LastWriteTime;
            disposition.ReadDate = file.LastAccessTime;
            _mail.Attachments.Add(attachment);
            _attachments.Add(file);
        }

        /// <summary>
        /// <inheritdoc cref="AddAttachment(FileInfo)"/>
        /// </summary>
        /// <param name="files">文件列表</param>
        public void AddAttachments(params string[] files)
        {
            if(files == null)
            {
                throw new ArgumentNullException(nameof(files), "参数files不允许为空!");
            }
            foreach (string file in files)
            {
                if (string.IsNullOrEmpty(file))
                {
                    throw new ArgumentNullException(nameof(file), "文件名称不允许为空!");
                }
                AddAttachment(new FileInfo(file));
            }
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="senderPassword">发信人密码</param>
        /// <param name="proxy">邮件服务器使用的协议(默认为smtp)</param>
        /// <param name="useSsl">是否启用Ssl安全连接(默认为false)</param>
        public void Send(string senderPassword,string proxy="smtp",bool useSsl = false)
        {
            if (string.IsNullOrEmpty(senderPassword))
            {
                throw new ArgumentNullException(nameof(senderPassword), "发信人密码不允许为空!");
            }
            if(_mail == null)
            {
                throw new InvalidOperationException("发送邮件之前请先初始化邮件信息!");
            }
            if(_receiver.Count <= 0)
            {
                throw new InvalidOperationException("请先添加收信人信息!");
            }
            try
            {
                _mail.From = new MailAddress(_sender, SenderNickName);
                _mail.Subject = MailTitle;
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Host = DetectProxyHost(proxy);
                smtpClient.EnableSsl = useSsl;
                smtpClient.UseDefaultCredentials = true;
                smtpClient.Credentials = new NetworkCredential(_sender, senderPassword);
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Send(_mail);
            }
            catch (Exception ex)
            {
                if(ex.Message.Contains("need EHLO and AUTH first") && !useSsl)
                {
                    Send(senderPassword, proxy, true);
                }
                else if(ex.Message.Contains("mail from address must be same as authorization user"))
                {
                    throw new Exception("发送邮件失败!请检查邮箱是否启用了SMTP/POP服务,并且确认发信人密码是否启用了客户端安全密码!");
                }
                else
                {
                    throw ex;
                }
            }
        }

        private string DetectProxyHost(string proxy)
        {
            if (string.IsNullOrEmpty(_sender))
            {
                throw new ArgumentNullException("senderAddress", "发信人邮箱不允许为空!");
            }
            int quotCount = proxy.ToCharArray().Count((c) => { return c.Equals('.'); });
            if(quotCount != 2)
            {
                int hostStart = _sender.LastIndexOf('@');
                string host = _sender.Substring(hostStart, _sender.Length - hostStart).Replace('@', '.');
                proxy = string.Format("{0}{1}", proxy, host);
            }
            return proxy;
        }
    }
}
