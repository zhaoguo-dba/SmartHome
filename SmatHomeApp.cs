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
        //������¼��ť״̬���Ӷ�ʵ��status���£�ֻд����Ӧ��̨�߼��ļ���
        int flag_livingroom_light = 0;
        private SpeechRecognitionEngine recognizer;
        private WaveIn waveIn;
        private WaveFileWriter writer;
        private bool recording = false;
        private const int maxSilenceCount = 3; // ���Ĭ����
        private int currentSilenceCount = 0; // ��ǰ��Ĭ����
        private string outputFilePath = "test.wav";
        private const string wakeUpPhrase = "С��С��";
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
                    lounge_status.Text = "���Ѵ�";
                    flag_livingroom_light = 1;
                    materialSwitch1.Checked = true;
                }
                else
                {
                    lounge_status.Text = "���ѹر�";
                    flag_livingroom_light = 0;
                    materialSwitch1.Checked = false;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            //��֪��Ϊɶ����ʱ�ؼ��Ĳ�͸���ȸ���ƽ���Ĳ�һ��������ֻ������������һ��
            //panel1.BackColor = Color.FromArgb(255, 255, 255);
            //panel2.BackColor = Color.FromArgb(255, 255, 255);
            //panel3.BackColor = Color.FromArgb(255, 255, 255);
            //panel4.BackColor = Color.FromArgb(255, 255, 255);
            pictureBox1.BackColor = Color.Transparent;
            pictureBox2.BackColor = Color.Transparent;
            pictureBox3.BackColor = Color.Transparent;
            pictureBox4.BackColor = Color.Transparent;
            tabPage1.BackgroundImageLayout = ImageLayout.Stretch;
            lounge_status.Font = new Font("����", 25.8000011F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lounge_status.ForeColor = SystemColors.GrayText;
            bedroom_status.Font = new Font("����", 25.8000011F, FontStyle.Regular, GraphicsUnit.Point, 134);
            bedroom_status.ForeColor = SystemColors.GrayText;
            kitchen_status.Font = new Font("����", 25.8000011F, FontStyle.Regular, GraphicsUnit.Point, 134);
            kitchen_status.ForeColor = SystemColors.GrayText;
            washroom_status.Font = new Font("����", 25.8000011F, FontStyle.Regular, GraphicsUnit.Point, 134);
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
                lounge_status.Text = "���Ѵ�";
                flag_livingroom_light = 1;
            }
            else
            {
                lounge_status.Text = "���ѹر�";
                flag_livingroom_light = 0;
            }
        }

    

        private void SpeechForm_Load(object sender, EventArgs e)
        {
            recognizer = new SpeechRecognitionEngine();
            waveIn = new WaveIn();

            // ����﷨����ʶ����ָ��
            Choices wakeUpChoices = new Choices(wakeUpPhrase);
            GrammarBuilder wakeUpGrammar = new GrammarBuilder(wakeUpChoices);
            Grammar grammar = new Grammar(wakeUpGrammar);
            recognizer.LoadGrammar(grammar);

            // �����¼�
            recognizer.SpeechRecognized += Recognizer_SpeechRecognized;

            // ����¼���豸����
            waveIn.DeviceNumber = 0;
            waveIn.WaveFormat = new WaveFormat(16000, 1); // 16kHz �����ʣ�������
            waveIn.DataAvailable += WaveIn_DataAvailable;

            // ����ʶ������
            recognizer.SetInputToDefaultAudioDevice();
            recognizer.RecognizeAsync(RecognizeMode.Multiple);

            Console.WriteLine("Listening for wake-up command...");

            Console.ReadLine(); // ���س����˳�����

            if (recording)
            {
                // ֹͣ¼��
                StopRecording();
            }
        }

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // ��ʶ�𵽻���ָ��ʱ����ʼ¼��
            if (e.Result.Text == wakeUpPhrase)
            {
                // �洢Ԥ¼��������
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
                // �������¼�����ݵĴ���
                writer.Write(e.Buffer, 0, e.BytesRecorded);

                // ����¼�����ݵ�����
                double energy = GetEnergyLevel(e.Buffer);

                // �������������ֵ�����Ӿ�Ĭ������
                if (energy < 10) // 
                {
                    currentSilenceCount++;
                }
                else
                {
                    currentSilenceCount = 0;
                }

                // �����Ĭ����ʱ�䳬����ֵ��ֹͣ¼��
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
            // ����Ƶ����ת��Ϊ˫���ȸ���������
            double[] samples = new double[buffer.Length / 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short sample = BitConverter.ToInt16(buffer, i * 2);
                samples[i] = sample / (double)short.MaxValue;
            }

            // �����������RMS��
            double sumOfSquares = 0;
            foreach (var sample in samples)
            {
                sumOfSquares += sample * sample;
            }
            double rms = Math.Sqrt(sumOfSquares / samples.Length);

            // ��������ֵ
            double energy = rms * rms * samples.Length;

            return energy;
        }


    }
}
