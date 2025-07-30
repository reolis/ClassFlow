using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ClassFlow.UmlData;

namespace ClassFlow
{
    public partial class ClassFlowFormMain : Form
    {
        Bitmap image;

        public ClassFlowFormMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            UmlDiagram diagram = UmlParser.Parse(richTextBox1.Text);
            UmlRenderer renderer = new UmlRenderer(diagram);
            image = renderer.Render(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = image;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ImageSaver.Save(image);
        }
    }
}
