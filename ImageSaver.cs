using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassFlow
{
    internal class ImageSaver
    {
        public static void Save(Bitmap image)
        {
            if (image != null)
            {
                using (SaveFileDialog ofd = new SaveFileDialog())
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        image.Save(ofd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
            }
        }
    }
}
