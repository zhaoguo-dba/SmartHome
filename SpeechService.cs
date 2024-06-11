using Baidu.Aip.Speech;
using MaterialSkin.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome
{
    public  class SpeechService
    {
        public string APP_ID = ConfigHelper.GetAppSetting("APP_ID");
        public string APP_KEY = ConfigHelper.GetAppSetting("APP_KEY");
        public string SECRET_KEY = ConfigHelper.GetAppSetting("SECRET_KEY");
        // 定义委托
        public delegate void SpeechServiceCompletedHandler(string message);

        // 定义事件
        public event SpeechServiceCompletedHandler SpeechServiceCompleted;

        private static string currentAlias = "temp_alias";
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);


        /*
         * 语音识别接口
         */

        public String speechRecognition(byte[] data, string format)
        {
            var client = new Asr(APP_ID, APP_KEY, SECRET_KEY);
            client.Timeout = 60000;
            var options = new Dictionary<string, object> { { "dev_pid", 1537 } };
            client.Timeout = 120000; // 若语音较长，建议设置更大的超时时间. ms
            var json = client.Recognize(data, format, 16000, options);
            int err_no = (int)json["err_no"];
            if (err_no > 0)
            {
                return "error";
            }
            string result = json["result"].ToString();
            return result;

        }

        /*
        * 语音合成
        * 在调用该接口后进行页面的修改
        */
        private bool text2audio(string text)
        {
            var client = new Baidu.Aip.Speech.Tts(APP_KEY, SECRET_KEY);
            client.Timeout = 60000;  // 修改超时时间

            var option = new Dictionary<string, object>()
            {
                {"spd", 5}, // 语速
                            // {"vol", 7}, // 音量
                {"per", 4}  // 发音人，4：情感度丫丫童声
            };
            var result = client.Synthesis(text, option);

            if (!result.Success)
            {
                // 合成失败
                return false;
            }

            // 停止并关闭当前播放的音频
            StopAudio();

            File.WriteAllBytes("temp.mp3", result.Data);

            mciSendString("open temp.mp3 alias temp_alias", null, 0, IntPtr.Zero);
            mciSendString("play temp_alias", null, 0, IntPtr.Zero);
            StringBuilder strReturn = new StringBuilder(64);
            do
            {
                mciSendString($"status {currentAlias} mode", strReturn, 64, IntPtr.Zero);
            } while (!strReturn.ToString().Contains("stopped"));

            mciSendString($"close {currentAlias}", null, 0, IntPtr.Zero);
            return true;
        }


        private void StopAudio()
        {
            // 停止当前播放的音频并关闭资源
            mciSendString($"stop {currentAlias}", null, 0, IntPtr.Zero);
            mciSendString($"close {currentAlias}", null, 0, IntPtr.Zero);
        }
        /**
         * 后台提供前台入口
         * 
         */
        public void EnterServic(byte[] data, string format)
        {
            string msg = this.speechRecognition(data, format);
            if (msg != null)
            {
                changeStatus(msg);
                text2audio(msg);
                //用不同编码对应不同状态和房间
                SpeechServiceCompleted?.Invoke("010");
            }
        }
        private string getGBKStr(string text)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            byte[] bytes = Encoding.GetEncoding("GBK").GetBytes(text);
            string gbkText = Encoding.GetEncoding("GBK").GetString(bytes);
            return gbkText;

        }


        //文本信息提取，获取文本中的房间名和电器名，并更改状态
        private void changeStatus(String text)
        {
            var client = new Baidu.Aip.Nlp.Nlp(APP_KEY, SECRET_KEY);
            client.Timeout = 60000;  // 修改超时时间
            string gbkText = this.getGBKStr(text);
            var result = client.Lexer(gbkText);
            if (result != null)
            {
                JObject obj = (JObject)result;
                List<string> words = analysisContent(obj);
                int status = CheckStatus(text);//1表示需要打开，0表示需要关闭
                Console.WriteLine("状态为：" + words[0]); //表示状态
                Console.WriteLine("房间名为：" + words[1]); //表示房间
                Console.WriteLine("电器名为：" + words[2]); //表示电器
            }

        }
        //提取文本信息后，更改对应目标状态
        private List<string> analysisContent(JObject obj)
        {

            List<string> words = new List<string>();
            var items = obj["items"];
            int i = 1;
            foreach (var item in items)
            {
                var baseWords = item["basic_words"];
                foreach (var word in baseWords)
                {
                   words.Add((string)word);
                }
                i++;
            }
            return words;
        }


        //检测文本中包含开还是关，返回期望的目标状态
        private int CheckStatus(string input)
        {
            if (input.Contains("开"))
            {
                return 1;
            }
            else if (input.Contains("关"))
            {
                return 0;
            }
            else
            {
                return -1; // 如果既没有“开”也没有“关”，可以根据需要返回其他值
            }
        }


    }
}
