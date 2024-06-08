using MaterialSkin;
using MaterialSkin.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using System.Runtime.InteropServices;
using System.Text;

namespace SmartHome
{
    public partial class Form1 : MaterialForm
    {

        private readonly MaterialSkinManager materialSkinManager;
        //用来记录按钮状态，从而实现status更新，只写有相应后台逻辑的即可
        int flag_livingroom_light = 0;

        private const string API_KEY = "8TYtBwK5XqMzXDQT1h4jrkta";
        private const string SECRET_KEY = "4yjTGWBU4CF5s09Bsd9MQSyL63AhqQhL";

        private static string currentAlias = "temp_alias";
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);

        public Form1()
        {
            InitializeComponent();

            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.EnforceBackcolorOnAllComponents = true;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.ColorScheme = new ColorScheme(
                       Primary.Cyan700,
                       Primary.Cyan900,
                       Primary.Cyan500,
                       Accent.DeepOrange200,
                       TextShade.WHITE);

            //materialSkinManager.Theme = materialSkinManager.Theme == MaterialSkinManager.Themes.DARK ? MaterialSkinManager.Themes.LIGHT : MaterialSkinManager.Themes.DARK;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            //不知道为啥运行时控件的不透明度跟设计界面的不一样，所以只能在这里再设一次
            //panel1.BackColor = Color.FromArgb(255, 255, 255);
            //panel2.BackColor = Color.FromArgb(255, 255, 255);
            //panel3.BackColor = Color.FromArgb(255, 255, 255);
            //panel4.BackColor = Color.FromArgb(255, 255, 255);
            pictureBox1.BackColor = Color.Transparent;
            pictureBox2.BackColor = Color.Transparent;
            pictureBox3.BackColor = Color.Transparent;
            pictureBox4.BackColor = Color.Transparent;
            tabPage1.BackgroundImageLayout = ImageLayout.Stretch;
            lounge_status.Font = new Font("等线", 25.8000011F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lounge_status.ForeColor = SystemColors.GrayText;
            bedroom_status.Font = new Font("等线", 25.8000011F, FontStyle.Regular, GraphicsUnit.Point, 134);
            bedroom_status.ForeColor = SystemColors.GrayText;
            kitchen_status.Font = new Font("等线", 25.8000011F, FontStyle.Regular, GraphicsUnit.Point, 134);
            kitchen_status.ForeColor = SystemColors.GrayText;
            washroom_status.Font = new Font("等线", 25.8000011F, FontStyle.Regular, GraphicsUnit.Point, 134);
            washroom_status.ForeColor = SystemColors.GrayText;
            panel1.AutoSize = true;

            //panelMenu.Controls.Add(leftBorderBtn);
        }


        private void materialExpansionPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void materialSwitch4_CheckedChanged(object sender, EventArgs e)
        {


        }

        private void materialSwitch1_CheckedChanged(object sender, EventArgs e)
        {
            if (flag_livingroom_light == 0)
            {
                lounge_status.Text = "灯已打开";
                flag_livingroom_light = 1;
            }
            else
            {
                lounge_status.Text = "灯已关闭";
                flag_livingroom_light = 0;
            }
        }

        /*
         * 语音合成
         * 在调用该接口后进行页面的修改
         */
        private bool text2audio(string text)
        {
            var client = new Baidu.Aip.Speech.Tts(API_KEY, SECRET_KEY);
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
         * pos：0-客厅；1-卧室；2-厨房；3-厕所
         */
        private void changeStatus(int pos, string status, string furniture)
        {
            var buttonMap = new Dictionary<int, Button>
            {
                { 0, lounge_status },
                { 1, bedroom_status },
                { 2, kitchen_status },
                { 3, washroom_status }
            };

            var switchMap = new Dictionary<(int, string), MaterialSwitch>
            {
                { (0, "灯"), materialSwitch1 },
                { (0, "电视"), materialSwitch2 },
                { (0, "电视"), materialSwitch3 },
                { (0, "电视"), materialSwitch4 },

                { (1, "灯"), materialSwitch5 },
                { (1, "空调"), materialSwitch6 },
                { (1, "电视"), materialSwitch7 },
                { (1, "电视"), materialSwitch8 },

                { (2, "灯"), materialSwitch9 },
                { (2, "空调"), materialSwitch10 },
                { (2, "电视"), materialSwitch11 },
                { (2, "电视"), materialSwitch12 },

                { (3, "灯"), materialSwitch13 },
                { (3, "空调"), materialSwitch14 },
                { (3, "电视"), materialSwitch15 },
                { (3, "电视"), materialSwitch16 }
            };

            if (!buttonMap.ContainsKey(pos) || !switchMap.ContainsKey((pos, furniture)))
            {
                return;
            }

            var targetButton = buttonMap[pos];
            var targetSwitch = switchMap[(pos, furniture)];

            UpdateStatus(targetButton, targetSwitch, furniture, status);
        }

        private void UpdateStatus(Button targetButton, MaterialSwitch targetSwitch, string furniture, string status)
        {
            if ("打开".Equals(status))
            {
                targetButton.Text = $"{furniture}已打开";
                targetSwitch.Checked = true;
            }
            else
            {
                targetButton.Text = $"{furniture}已关闭";
                targetSwitch.Checked = false;
            }
        }

    }
}
