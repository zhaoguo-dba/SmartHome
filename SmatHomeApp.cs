using MaterialSkin;
using MaterialSkin.Controls;
using NAudio.Wave;
using System.Speech.Recognition;
using MathNet.Numerics.IntegralTransforms;

namespace SmartHome
{
    public partial class SmatHomeApp : MaterialForm
    {

        private readonly MaterialSkinManager materialSkinManager;
        //用来记录按钮状态，从而实现status更新，只写有相应后台逻辑的即可
        int flag_livingroom_light = 0;
        private SpeechRecognitionEngine recognizer;
        private WaveIn waveIn;
        private WaveFileWriter writer;
        private bool recording = false;
        private const int maxSilenceCount = 3; // 最大静默次数
        private int currentSilenceCount = 0; // 当前静默次数
        private string outputFilePath = "test.wav";
        private const string wakeUpPhrase = "小度小度";
        private SpeechService service;
       


        public SmatHomeApp()
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
            service = new SpeechService();
            service.SpeechServiceCompleted += Service_SpeechServiceCompleted;

            //materialSkinManager.Theme = materialSkinManager.Theme == MaterialSkinManager.Themes.DARK ? MaterialSkinManager.Themes.LIGHT : MaterialSkinManager.Themes.DARK;
        }

        private void Service_SpeechServiceCompleted(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateControls(message)));
            }
            else
            {
                UpdateControls(message);
            }

        }
        private void UpdateControls(string message)
        {
            if (message.Equals("010"))
            {
                if (flag_livingroom_light == 0)
                {
                    lounge_status.Text = "灯已打开";
                    flag_livingroom_light = 1;
                    materialSwitch1.Checked = true;
                }
                else
                {
                    lounge_status.Text = "灯已关闭";
                    flag_livingroom_light = 0;
                    materialSwitch1.Checked = false;
                }
            }
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
            SpeechForm_Load(sender,e);
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

    

        private void SpeechForm_Load(object sender, EventArgs e)
        {
            recognizer = new SpeechRecognitionEngine();
            waveIn = new WaveIn();

            // 添加语法规则，识别唤醒指令
            Choices wakeUpChoices = new Choices(wakeUpPhrase);
            GrammarBuilder wakeUpGrammar = new GrammarBuilder(wakeUpChoices);
            Grammar grammar = new Grammar(wakeUpGrammar);
            recognizer.LoadGrammar(grammar);

            // 订阅事件
            recognizer.SpeechRecognized += Recognizer_SpeechRecognized;

            // 设置录音设备参数
            waveIn.DeviceNumber = 0;
            waveIn.WaveFormat = new WaveFormat(16000, 1); // 16kHz 采样率，单声道
            waveIn.DataAvailable += WaveIn_DataAvailable;

            // 启动识别引擎
            recognizer.SetInputToDefaultAudioDevice();
            recognizer.RecognizeAsync(RecognizeMode.Multiple);

            Console.WriteLine("Listening for wake-up command...");

            Console.ReadLine(); // 按回车键退出程序

            if (recording)
            {
                // 停止录音
                StopRecording();
            }
        }

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // 当识别到唤醒指令时，开始录音
            if (e.Result.Text == wakeUpPhrase)
            {
                // 存储预录音缓冲区
                if (File.Exists(outputFilePath))
                {
                    File.Delete(outputFilePath);
                }
                writer = new WaveFileWriter(outputFilePath, waveIn.WaveFormat);
                StartRecording();
            }
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (recording)
            {
                // 这里进行录音数据的处理
                writer.Write(e.Buffer, 0, e.BytesRecorded);

                // 计算录音数据的能量
                double energy = GetEnergyLevel(e.Buffer);

                // 如果能量低于阈值，增加静默计数器
                if (energy < 10) // 
                {
                    currentSilenceCount++;
                }
                else
                {
                    currentSilenceCount = 0;
                }

                // 如果静默持续时间超过阈值，停止录音
                if (currentSilenceCount >= maxSilenceCount)
                {
                    StopRecording();
                   
                    service.EnterServic(File.ReadAllBytes(outputFilePath), "wav");

                }
            }
        }

        private void StartRecording()
        {
           
            if (!recording)
            {
                waveIn.BufferMilliseconds = 1000;
                waveIn.StartRecording();
                recording = true;
            }
          
        }

        private void StopRecording()
        {
            recording = false;
            waveIn.StopRecording();
            writer.Dispose();
        }

        private static double GetEnergyLevel(byte[] buffer)
        {
            // 将音频数据转换为双精度浮点数数组
            double[] samples = new double[buffer.Length / 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short sample = BitConverter.ToInt16(buffer, i * 2);
                samples[i] = sample / (double)short.MaxValue;
            }

            // 计算均方根（RMS）
            double sumOfSquares = 0;
            foreach (var sample in samples)
            {
                sumOfSquares += sample * sample;
            }
            double rms = Math.Sqrt(sumOfSquares / samples.Length);

            // 计算能量值
            double energy = rms * rms * samples.Length;

            return energy;
        }


    }
}
