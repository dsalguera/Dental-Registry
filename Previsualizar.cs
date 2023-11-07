using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Expedientes_Dentales
{
    public partial class Previsualizar : Form
    {
        public Previsualizar(String path)
        {
            InitializeComponent();
            if (!path.Equals(""))
            {
                try
                {
                    pictureBox1.Image = Image.FromFile(path);
                }
                catch (Exception)
                {
                    MessageBox.Show("No se puede mostrar la imagen ya que la ruta no es válida, agregue la imagen nuevamente y guardela.");
                }
                
            }
            
        }
    }
}
