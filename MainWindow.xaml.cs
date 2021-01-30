using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;

namespace MailReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// 
        /// </summary>
        private static IList<string> MailFilePathList = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
        }
        public void ImportFiles()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "请选择要导入的邮件消息（可多选）",
                Filter = "邮件文件(*.eml;*.msg)|*.eml;*msg"
            };
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                FileNameList.Items.Clear();
                MailFilePathList.Clear();
                foreach (var filename in dialog.FileNames)
                {
                    MailFilePathList.Add(filename);
                    FileNameList.Items.Add(Path.GetFileNameWithoutExtension(filename));
                }
                HeadLabel.Content = $"已导入 {MailFilePathList.Count} 项";
            }
            else
            {
                MessageBox.Show("取消了导入", "MailReader", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }

        }

        public void OutputFiles()
        {
            List<TextMailDTO> res = new List<TextMailDTO>();
            foreach (var filename in MailFilePathList)
            {
                try
                {
                    switch (Path.GetExtension(filename))
                    {

                        case ".msg":
                            {
                                var msg = new MsgReader.Outlook.Storage.Message(filename);
                                var rec = new TextMailDTO()
                                {
                                    From = msg.Sender.Email,
                                    To = msg.GetEmailRecipients(MsgReader.Outlook.RecipientType.To, false, false),
                                    Subject = msg.Subject,
                                    Body = msg.BodyText

                                };
                                res.Add(rec);
                                break;
                            }
                        case ".eml":
                            {
                                var fileInfo = new FileInfo(filename);
                                var eml = MsgReader.Mime.Message.Load(fileInfo);
                                var rec = new TextMailDTO();
                                if (eml.Headers != null)
                                {
                                    StringBuilder toList = new StringBuilder();
                                    if (eml.Headers.To != null)
                                    {
                                        
                                        foreach(var recipient in eml.Headers.To)
                                        {
                                            toList.Append(recipient.Address);
                                        }
                                    }
                                    rec.To = toList.ToString();
                                    rec.From = eml.Headers.From.Address;
                                }
                                rec.Subject = eml.Headers.Subject;
                                if (eml.TextBody != null)
                                {
                                    rec.Body = Encoding.UTF8.GetString(eml.TextBody.Body);
                                }
                                res.Add(rec);
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
                catch
                {

                }
            }
            ResultLabel.Content = $"已解析 {res.Count} 项";
            ResultList.Items.Clear();
            foreach (var item in res)
            {
                ResultList.Items.Add(item.ToString());
            }
            
            try
            {
                var text = JsonConvert.SerializeObject(res);
                File.WriteAllText("result.json", text);
            }
            catch
            {
                MessageBox.Show("写入文件失败", "MailReader", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }



        private void FileNameListScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = sender
            };
            FileNameListScrollViewer.RaiseEvent(eventArg);
        }
        private void ResultScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = sender
            };
            ResultScrollViewer.RaiseEvent(eventArg);
        }

        private void Button_Click_Import(object sender, RoutedEventArgs e)
        {
            ImportFiles();
        }

        private void Button_Click_Output(object sender, RoutedEventArgs e)
        {
            OutputFiles();
        }
    }
}
