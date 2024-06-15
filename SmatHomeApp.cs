using MaterialSkin;
using MaterialSkin.Controls;
using NAudio.Wave;
using System.Speech.Recognition;

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
            
            if (message.Equals("00000"))
            {
                service.Text2Audio("��û�������������˵һ�Σ�");
            }
            else
            {
                string status = message.Substring(0,1);
                string roomCode = message.Substring(1, 1);
                string equipCode = message.Substring(2);
                string equipName = DataTools.equipment.FirstOrDefault(x => x.Value == equipCode).Key;
                ChangeStatus(int.Parse(roomCode), status, equipName);
            }
            
        }

        /**
        * pos��0-������1-���ң�2-������3-����
        */
        private void ChangeStatus(int pos, string status, string furniture)
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
                { (0, "��"), materialSwitch1 },
                { (0, "����"), materialSwitch2 },
                { (0, "�յ�"), materialSwitch3 },
                { (0, "����"), materialSwitch4 },

                { (1, "̨��"), materialSwitch5 },
                { (1, "����"), materialSwitch6 },
                { (1, "�յ�"), materialSwitch7 },
                { (1, "��"), materialSwitch8 },

                { (2, "��"), materialSwitch9 },
                { (2, "΢��¯"), materialSwitch10 },
                { (2, "�緹��"), materialSwitch11 },
                { (2, "ˮ��"), materialSwitch12 },

                { (3, "��"), materialSwitch13 },
                { (3, "�ŷ���"), materialSwitch14 },
                { (3, "ϴ�»�"), materialSwitch15 },
                { (3, "ԡ��"), materialSwitch16 }
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
            if (targetSwitch.Checked) {
                if ("0".Equals(status))
                {
                   
                    targetButton.Text = $"{furniture}�ѹر�";
                    targetSwitch.Checked = false;
                    service.Text2Audio(targetButton.Text);
                }
                else
                {
                    service.Text2Audio($"{furniture}�Ѵ�,�����ٿ�");
                }
            }
            else
            {
                if ("0".Equals(status))
                {
                   service.Text2Audio($"{furniture}�Ѵ�,�����ٿ�");
                }
                else
                {
                    targetButton.Text = $"{furniture}�Ѵ�";
                    targetSwitch.Checked = true;
                    service.Text2Audio(targetButton.Text);
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


        private void MaterialSwitch1_CheckedChanged(object sender, EventArgs e)
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
                StopRecording();
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
                    currentSilenceCount = 0; // ���þ�Ĭ����
                }
                // �����Ĭ����ʱ�䳬����ֵ��ֹͣ¼��
                if (currentSilenceCount >= maxSilenceCount)
                {
                    StopRecording();
                    try
                    {
                        // ʹ��FileStream���ļ�
                        using (FileStream fileStream = new FileStream(outputFilePath, FileMode.Open))
                        {
                            // ���ļ����ж�ȡ�����ֽڵ�һ���ֽ�������
                            byte[] fileData = new byte[fileStream.Length];
                            fileStream.Read(fileData, 0, fileData.Length);
                            service.EnterServic(fileData, "wav");
                            currentSilenceCount = 0;
                          

                        }
                        if (File.Exists(outputFilePath))
                        {
                            File.Delete(outputFilePath);
                        }
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine("�޷������ļ��� " + ex.Message);
                    }
                   
                    

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
            if (recording)
            {
                recording = false;
                waveIn.StopRecording();
                if (writer != null)
                {
                    writer.Dispose(); // �ͷ�֮ǰ�� WaveFileWriter
                    writer = null;
                }
            }
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
