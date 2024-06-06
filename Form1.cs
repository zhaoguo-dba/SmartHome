using MaterialSkin;
using MaterialSkin.Controls;

namespace SmartHome
{
    public partial class Form1 : MaterialForm
    {

        private readonly MaterialSkinManager materialSkinManager;
        //������¼��ť״̬���Ӷ�ʵ��status���£�ֻд����Ӧ��̨�߼��ļ���
        int flag_livingroom_light = 0;

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
                lounge_status.Text = "���Ѵ�";
                flag_livingroom_light = 1;
            }
            else
            {
                lounge_status.Text = "���ѹر�";
                flag_livingroom_light = 0;
            }
        }
    }
}
