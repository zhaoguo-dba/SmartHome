using Baidu.Aip.Speech;
using MaterialSkin.Controls;
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
                text2audio(msg);
            }
        }

      
    }
}
