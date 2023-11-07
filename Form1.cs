using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Expedientes_Dentales
{
    public partial class Form1 : Form
    {
        Bitmap bm;
        Graphics g;
        bool paint = false;
        Point px, py;
        Pen p = new Pen(Color.Black, 1);
        Pen erase = new Pen(Color.White, 10);
        int index;

        ColorDialog cd = new ColorDialog();
        Color new_color;

        public Form1()
        {
            InitializeComponent();
            bm = new Bitmap(pictureBox6.Width, pictureBox6.Height);
            g = Graphics.FromImage(bm);
            g.Clear(Color.White);
            pictureBox6.Image = bm;

            g.DrawImage(new Bitmap(Properties.Resources.odontograma_2), new Point(1,1));
            p.Width = 8.0F;

            Variables();

            index = 1;
        }

        private void pictureBox6_MouseDown(object sender, MouseEventArgs e)
        {
            paint = true;
            py = e.Location;
        }

        private void pictureBox6_MouseUp(object sender, MouseEventArgs e)
        {
            paint = false;
        }

        private void pictureBox6_MouseMove(object sender, MouseEventArgs e)
        {
            if (paint)
            {
                if (index == 1)
                {
                    px = e.Location;
                    g.DrawLine(p, px, py);
                    py = px;
                }
                //index 2 eraser
                if (index == 2)
                {
                    px = e.Location;
                    g.DrawLine(erase, px, py);
                    py = px;
                }

            }
            pictureBox6.Refresh();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (richTextBox1.Text.Equals(""))
            {
                MessageBox.Show("No se encuentra la ruta de la imagen");
            }
            else
            {
                String urlImg = richTextBox1.Text;
                g.Clear(Color.White);
                pictureBox6.Image = bm;

                g.DrawImage(new Bitmap(urlImg), new Point(1, 1));
                index = 1;
            }
            
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            cd.ShowDialog();
            new_color = cd.Color;
            panel4.BackColor = cd.Color;
            p.Color = cd.Color;
        }


        private void pane_Click(object sender, EventArgs e)
        {
            
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {}

        public static class FileSizeFormatter
        {
            // Load all suffixes in an array  
            static readonly string[] suffixes =
            { "Bytes", "KB", "MB", "GB", "TB", "PB" };
            public static string FormatSize(Int64 bytes)
            {
                int counter = 0;
                decimal number = (decimal)bytes;
                while (Math.Round(number / 1024) >= 1)
                {
                    number = number / 1024;
                    counter++;
                }
                return string.Format("{0:n1}{1}", number, suffixes[counter]);
            }
        }

        String fullPath = "", folderPath = "", dirRegistros = "", dirImagenes = "", dirPerfiles = "";

        AutoCompleteStringCollection lista_identificaciones = new AutoCompleteStringCollection();


        void Variables()
        {

            //Revisar si existen carpetas importantes
            fullPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            folderPath = System.IO.Path.GetDirectoryName(fullPath);
            
            dirRegistros = folderPath + "\\Registros\\";
            dirImagenes = folderPath + "\\Imagenes\\";
            dirPerfiles = folderPath + "\\Perfiles\\";

            if (!Directory.Exists(dirRegistros))
            {
                Directory.CreateDirectory(dirRegistros);
                MessageBox.Show("No se encontro la carpeta de Registros, se creará despues de este mensaje", "Carpeta No existe");
            }

            if (!Directory.Exists(dirImagenes))
            {
                Directory.CreateDirectory(dirImagenes);
                MessageBox.Show("No se encontro la carpeta de Imagenes, se creará despues de este mensaje", "Carpeta No existe");
            }

            if (!Directory.Exists(dirPerfiles))
            {
                Directory.CreateDirectory(dirPerfiles);
                MessageBox.Show("No se encontro la carpeta de Fotos de Perfil, se creará despues de este mensaje", "Carpeta No existe");
            }

            //Calular el espacio en disco

            long registros = new DirectoryInfo(Directory.GetCurrentDirectory().ToString() + "\\Registros\\").
                EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);

            long imagenes = new DirectoryInfo(Directory.GetCurrentDirectory().ToString() + "\\Imagenes\\").
                EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);

            long perfiles = new DirectoryInfo(Directory.GetCurrentDirectory().ToString() + "\\Perfiles\\").
                EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);


            label29.Text = "Usado por Registros: "+ FileSizeFormatter.FormatSize(registros) + "    " +
                "Usado por Imagenes: "+ FileSizeFormatter.FormatSize(imagenes) + "    " +
                "Usado por Fotos de Perfil: " + FileSizeFormatter.FormatSize(perfiles);

            //LeerInfoUsuario();
            Recarga_Lista();
        }

        void Recarga_Lista()
        {
            String mainDir = folderPath + "\\Registros\\";

            string[] files = Directory.GetFiles(mainDir, "*.xml");

            foreach (var file in files)
            {
                lista_identificaciones.Add(Path.GetFileNameWithoutExtension(file));
            }

            txtIdentificacion.AutoCompleteCustomSource = lista_identificaciones;
            txtIdentificacion.AutoCompleteMode = AutoCompleteMode.Suggest;
            txtIdentificacion.AutoCompleteSource = AutoCompleteSource.CustomSource;

        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            LeerInfoUsuario(txtIdentificacion.Text);
        }

        XmlDocument doc = new XmlDocument();

        void LeerInfoUsuario(String identificacion)
        {
            String dir = Directory.GetCurrentDirectory().ToString() + "\\Registros\\" +identificacion+".xml";

            if (File.Exists(dir))
            {
                try
                {
                    doc.Load(dir);

                    XmlNodeList infoPaciente = doc.SelectNodes("Paciente/InformacionBasica");
                    XmlNode paciente;

                    for (int j = 0; j < infoPaciente.Count; j++)
                    {

                        paciente = infoPaciente.Item(j);

                        txtNombre.Text = paciente.SelectSingleNode("Nombre").InnerText;
                        txtIdentificacion.Text = paciente.SelectSingleNode("Identificacion").InnerText;
                        comboTipoIdentificacion.Text = paciente.SelectSingleNode("TipoIdentificacion").InnerText;
                        fechaIngreso.Value = Convert.ToDateTime(paciente.SelectSingleNode("FechaIngreso").InnerText);
                        Edad.Value = Int32.Parse(paciente.SelectSingleNode("Edad").InnerText);
                        fechaNacimiento.Value = Convert.ToDateTime(paciente.SelectSingleNode("FechaNacimiento").InnerText);
                        txtLugarNacimiento.Text = paciente.SelectSingleNode("LugarNacimiento").InnerText;
                        comboEstadoCivil.Text = paciente.SelectSingleNode("EstadoCivil").InnerText;
                        comboSexo.Text = paciente.SelectSingleNode("Sexo").InnerText;
                        txtDireccion.Text = paciente.SelectSingleNode("DireccionExacta").InnerText;
                        txtTelefono.Text = paciente.SelectSingleNode("Telefono").InnerText;
                        txtCelular.Text = paciente.SelectSingleNode("Celular").InnerText;
                        txtFax.Text = paciente.SelectSingleNode("Fax").InnerText;
                        txtEmail.Text = paciente.SelectSingleNode("Email").InnerText;

                        txtOcupacion.Text = paciente.SelectSingleNode("Ocupacion").InnerText;
                        txtLugarTrabajo.Text = paciente.SelectSingleNode("LugarTrabajo").InnerText;
                        txtTelefono2.Text = paciente.SelectSingleNode("Telefono2").InnerText;
                        txtPrecauciones.Text = paciente.SelectSingleNode("Precauciones").InnerText;

                    }


                    //Procedemos a leer la parte de Anamnesis
                    infoPaciente = doc.SelectNodes("Paciente/Anamnesis");

                    for (int j = 0; j < infoPaciente.Count; j++)
                    {

                        paciente = infoPaciente.Item(j);

                        txtMedicoCabecera.Text = paciente.SelectSingleNode("MedicoCabecera").InnerText;
                        txtCasoEmergencia.Text = paciente.SelectSingleNode("CasoEmergencia").InnerText;
                        txtDireccion2.Text = paciente.SelectSingleNode("DireccionAnamnesis").InnerText;
                        txtSangre.Text = paciente.SelectSingleNode("TipoSangre").InnerText;
                        txttelefonoAnamnesis.Text = paciente.SelectSingleNode("TelefonoAnamnesis").InnerText;
                        txtParentesco.Text = paciente.SelectSingleNode("Parentesco").InnerText;

                        txtQuejaPrincipal.Text = paciente.SelectSingleNode("QuejaPrincipal").InnerText;
                        txtQueja.Text = paciente.SelectSingleNode("HistorialQueja").InnerText;

                    }

                    //Procedemos a leer la parte de Historia Medica
                    infoPaciente = doc.SelectNodes("Paciente/HistoriaMedica");

                    for (int j = 0; j < infoPaciente.Count; j++)
                    {

                        paciente = infoPaciente.Item(j);

                        checkBox1.Checked = bool.Parse(paciente.SelectSingleNode("EnfermedadProcedimiento").InnerText);
                        textBox18.Text = paciente.SelectSingleNode("EnfermedadProcedimientoTxt").InnerText;

                        checkBox2.Checked = bool.Parse(paciente.SelectSingleNode("ProblemasPresion").InnerText);
                        textBox19.Text = paciente.SelectSingleNode("ProblemasPresionTxt").InnerText;

                        checkBox3.Checked = bool.Parse(paciente.SelectSingleNode("FiebreReumatica").InnerText);
                        checkBox4.Checked = bool.Parse(paciente.SelectSingleNode("VIHoSIDA").InnerText);
                        checkBox5.Checked = bool.Parse(paciente.SelectSingleNode("AccidenteCerebrovascular").InnerText);
                        checkBox6.Checked = bool.Parse(paciente.SelectSingleNode("Anemia").InnerText);
                        checkBox7.Checked = bool.Parse(paciente.SelectSingleNode("TransfusionSangre").InnerText);
                        checkBox9.Checked = bool.Parse(paciente.SelectSingleNode("Equimosis").InnerText);
                        checkBox10.Checked = bool.Parse(paciente.SelectSingleNode("ProblemasRinon").InnerText);
                        textBox22.Text = paciente.SelectSingleNode("ProblemasRinonTxt").InnerText;
                        checkBox11.Checked = bool.Parse(paciente.SelectSingleNode("ProblemasGastrointestinales").InnerText);
                        textBox23.Text = paciente.SelectSingleNode("ProblemasGastrointestinalesTxt").InnerText;
                        checkBox12.Checked = bool.Parse(paciente.SelectSingleNode("ProblemasVision").InnerText);
                        checkBox13.Checked = bool.Parse(paciente.SelectSingleNode("TratamientoCorticoesteroides").InnerText);
                        checkBox14.Checked = bool.Parse(paciente.SelectSingleNode("Diabetes12").InnerText);
                        checkBox15.Checked = bool.Parse(paciente.SelectSingleNode("EpilepsiaConvulsionDesmayo").InnerText);
                        checkBox16.Checked = bool.Parse(paciente.SelectSingleNode("EnfermedadesRespiratorias").InnerText);
                        checkBox17.Checked = bool.Parse(paciente.SelectSingleNode("RadioterapiaQuimioterapia").InnerText);
                        checkBox18.Checked = bool.Parse(paciente.SelectSingleNode("Reumatismo").InnerText);

                        checkBox19.Checked = bool.Parse(paciente.SelectSingleNode("ProblemasHepaticos").InnerText);
                        textBox24.Text = paciente.SelectSingleNode("ProblemasHepaticosTxt").InnerText;
                        checkBox20.Checked = bool.Parse(paciente.SelectSingleNode("Herpes").InnerText);
                        checkBox21.Checked = bool.Parse(paciente.SelectSingleNode("PerdidaAumentoPeso").InnerText);
                        textBox25.Text = paciente.SelectSingleNode("PerdidaAumentoPesoTxt").InnerText;
                        checkBox22.Checked = bool.Parse(paciente.SelectSingleNode("Artritis").InnerText);
                        checkBox23.Checked = bool.Parse(paciente.SelectSingleNode("TratamientoPsiquiatrico").InnerText);
                        checkBox24.Checked = bool.Parse(paciente.SelectSingleNode("Tiroides").InnerText);
                        textBox26.Text = paciente.SelectSingleNode("TiroidesTxt").InnerText;

                        checkBox25.Checked = bool.Parse(paciente.SelectSingleNode("Ets").InnerText);
                        checkBox26.Checked = bool.Parse(paciente.SelectSingleNode("Osteoporosis").InnerText);
                        checkBox27.Checked = bool.Parse(paciente.SelectSingleNode("Migrana").InnerText);

                        checkBox28.Checked = bool.Parse(paciente.SelectSingleNode("TratamientoBifosfonados").InnerText);
                        textBox27.Text = paciente.SelectSingleNode("TratamientoBifosfonadosTxt").InnerText;

                        checkBox29.Checked = bool.Parse(paciente.SelectSingleNode("ConsumoDroga").InnerText);
                        textBox28.Text = paciente.SelectSingleNode("ConsumoDrogaTxt").InnerText;
                        checkBox30.Checked = bool.Parse(paciente.SelectSingleNode("Fuma").InnerText);

                        checkBox31.Checked = bool.Parse(paciente.SelectSingleNode("Fumo").InnerText);
                        textBox29.Text = paciente.SelectSingleNode("CigarrosTxt").InnerText;
                        textBox30.Text = paciente.SelectSingleNode("TiempoFumandoTxt").InnerText;
                        checkBox32.Checked = bool.Parse(paciente.SelectSingleNode("BebidasAlcoholicas").InnerText);
                        textBox31.Text = paciente.SelectSingleNode("FrecuenciaAlcoholicaTxt").InnerText;
                        checkBox34.Checked = bool.Parse(paciente.SelectSingleNode("ShockAnafilactico").InnerText);

                        checkBox35.Checked = bool.Parse(paciente.SelectSingleNode("Embarazo").InnerText);
                        textBox33.Text = paciente.SelectSingleNode("SemanasEmbarazoTxt").InnerText;
                        checkBox36.Checked = bool.Parse(paciente.SelectSingleNode("Anticonceptivos").InnerText);
                        checkBox37.Checked = bool.Parse(paciente.SelectSingleNode("Ginecoobstetricos").InnerText);
                        textBox34.Text = paciente.SelectSingleNode("PartosTxt").InnerText;
                        textBox35.Text = paciente.SelectSingleNode("AbortosTxt").InnerText;
                        textBox36.Text = paciente.SelectSingleNode("CesareasTxt").InnerText;

                    }

                    //Procedemos a leer la parte de Historial
                    infoPaciente = doc.SelectNodes("Paciente/Historial");

                    for (int j = 0; j < infoPaciente.Count; j++)
                    {

                        paciente = infoPaciente.Item(j);

                        richTextBox4.Text = paciente.SelectSingleNode("Observaciones").InnerText;
                        richTextBox5.Text = paciente.SelectSingleNode("AntecedentesAlergicos").InnerText;
                        richTextBox6.Text = paciente.SelectSingleNode("AntecedentesQuirurgicos").InnerText;
                        richTextBox7.Text = paciente.SelectSingleNode("Medicamentos").InnerText;
                        richTextBox8.Text = paciente.SelectSingleNode("HistoriaDental").InnerText;
                        txtPresionArterial.Text = paciente.SelectSingleNode("PresionArterial").InnerText;
                        textBox38.Text = paciente.SelectSingleNode("Pulso").InnerText;
                        textBox39.Text = paciente.SelectSingleNode("FrecuenciaRespiratoria").InnerText;
                        richTextBox9.Text = paciente.SelectSingleNode("TejidosBlandos").InnerText;

                    }

                    //Procedemos a leer la parte de TratamientoEfectuado
                    infoPaciente = doc.SelectNodes("Paciente/TratamientoEfectuado/Fila");

                    dataGridView1.Rows.Clear();

                    for (int j = 0; j < infoPaciente.Count; j++)
                    {

                        paciente = infoPaciente.Item(j);

                        String Fecha = paciente.SelectSingleNode("Fecha").InnerText;
                        String Dientes = paciente.SelectSingleNode("Dientes").InnerText;
                        String Descripcion = paciente.SelectSingleNode("Descripcion").InnerText;
                        String MontoTotal = paciente.SelectSingleNode("MontoTotal").InnerText;
                        String Abono = paciente.SelectSingleNode("Abono").InnerText;
                        String Saldo = paciente.SelectSingleNode("Saldo").InnerText;

                        dataGridView1.Rows.Add(Fecha, Dientes, Descripcion, MontoTotal, Abono, Saldo);

                    }

                    //Procedemos a leer la parte de Odontograma
                    infoPaciente = doc.SelectNodes("Paciente/Odontograma");

                    for (int j = 0; j < infoPaciente.Count; j++)
                    {

                        paciente = infoPaciente.Item(j);

                        textBox45.Text = paciente.SelectSingleNode("HuesoAlveolar").InnerText;
                        textBox46.Text = paciente.SelectSingleNode("Calculo").InnerText;
                        textBox47.Text = paciente.SelectSingleNode("Caries").InnerText;
                        textBox48.Text = paciente.SelectSingleNode("DientesAusentes").InnerText;
                        textBox49.Text = paciente.SelectSingleNode("MorfologiaRadicular").InnerText;

                        textBox50.Text = paciente.SelectSingleNode("Patologia").InnerText;
                        textBox51.Text = paciente.SelectSingleNode("TercerasMolares").InnerText;
                        textBox52.Text = paciente.SelectSingleNode("AusenciaCongenita").InnerText;
                        textBox53.Text = paciente.SelectSingleNode("Supernumerarios").InnerText;
                        textBox54.Text = paciente.SelectSingleNode("TorusExostosis").InnerText;
                        richTextBox10.Text = paciente.SelectSingleNode("Observaciones").InnerText;

                        //MessageBox.Show(paciente.SelectSingleNode("ImagenOdontograma").InnerText);

                        richTextBox1.Text = paciente.SelectSingleNode("ImagenOdontograma").InnerText;
                        
                        if (!richTextBox1.Text.Equals(""))
                        {
                            g.Clear(Color.White);
                            pictureBox6.Image = bm;

                            g.DrawImage(new Bitmap(richTextBox1.Text), new Point(1, 1));
                            index = 1;
                        }


                    }

                    //Procedemos a leer la parte de Imagenologia
                    infoPaciente = doc.SelectNodes("Paciente/Imagenologia");
                    
                    for (int j = 0; j < infoPaciente.Count; j++)
                    {
                        paciente = infoPaciente.Item(j);

                        dateTimePicker4.Text = paciente.SelectSingleNode("Fecha1").InnerText;
                        richTextBox12.Text = paciente.SelectSingleNode("Observaciones").InnerText;

                        dateTimePicker7.Text = paciente.SelectSingleNode("Fecha2").InnerText;
                        richTextBox13.Text = paciente.SelectSingleNode("Observaciones2").InnerText;

                        dateTimePicker9.Text = paciente.SelectSingleNode("Fecha3").InnerText;
                        richTextBox15.Text = paciente.SelectSingleNode("Observaciones3").InnerText;

                        dateTimePicker11.Text = paciente.SelectSingleNode("Fecha4").InnerText;
                        richTextBox17.Text = paciente.SelectSingleNode("Observaciones4").InnerText;

                        richTextBox19.Text = paciente.SelectSingleNode("PlanTratamiento").InnerText;

                        richTextBox20.Text = paciente.SelectSingleNode("ExamenesSolicitados").InnerText;
                        richTextBox21.Text = paciente.SelectSingleNode("Resultados").InnerText;
                        dateTimePicker12.Text = paciente.SelectSingleNode("Fecha").InnerText;
                        richTextBox22.Text = paciente.SelectSingleNode("ExamenExtraoral").InnerText;

                        textBox1.Text = paciente.SelectSingleNode("ImagenFecha1").InnerText;
                        textBox2.Text = paciente.SelectSingleNode("ImagenFecha2").InnerText;
                        textBox3.Text = paciente.SelectSingleNode("ImagenFecha3").InnerText;
                        textBox4.Text = paciente.SelectSingleNode("ImagenFecha4").InnerText;

                        if (!textBox1.Text.Equals(""))
                        {
                            pictureBox1.Image = Image.FromFile(paciente.SelectSingleNode("ImagenFecha1").InnerText);
                        }

                        if (!textBox2.Text.Equals(""))
                        {
                            pictureBox3.Image = Image.FromFile(paciente.SelectSingleNode("ImagenFecha2").InnerText);
                        }

                        if (!textBox3.Text.Equals(""))
                        {
                            pictureBox4.Image = Image.FromFile(paciente.SelectSingleNode("ImagenFecha3").InnerText);
                        }

                        if (!textBox4.Text.Equals(""))
                        {
                            pictureBox5.Image = Image.FromFile(paciente.SelectSingleNode("ImagenFecha4").InnerText);
                        }

                    }

                }
                catch (Exception e)
                {
                    //MessageBox.Show("Ha ocurrido un error al tratar de abrir el archivo, posiblemente el registro esta en blanco.", "Error");
                }

            }
            else 
            {
                MessageBox.Show("El siguiente registro no existe", "Error");
            }

            /**/
        }

        private void Limpiar_Campos()
        {
            //Informacion Basica
            txtNombre.Text = "";
            txtIdentificacion.Text = "";
            comboTipoIdentificacion.Text = "";
            fechaIngreso.Text = "";
            Edad.Text = "";
            fechaNacimiento.Text = "";
            txtLugarNacimiento.Text = "";
            comboEstadoCivil.Text = "";
            comboSexo.Text = "";
            txtDireccion.Text = "";
            txtTelefono.Text = "";
            txtCelular.Text = "";
            txtFax.Text = "";
            txtEmail.Text = "";

            txtOcupacion.Text = "";
            txtLugarTrabajo.Text = "";
            txtTelefono2.Text = "";
            txtPrecauciones.Text = "";

            txtMedicoCabecera.Text = "";
            txtCasoEmergencia.Text = "";
            txtDireccion2.Text = "";

            txtSangre.Text = "";
            txttelefonoAnamnesis.Text = "";
            txtParentesco.Text = "";

            txtQuejaPrincipal.Text = "";
            txtQueja.Text = "";

            checkBox1.Checked = false;
            textBox18.Text = "";

            checkBox2.Checked = false;
            textBox19.Text = "";

            checkBox3.Checked = false;
            checkBox4.Checked = false;
            checkBox5.Checked = false;
            checkBox6.Checked = false;
            checkBox7.Checked = false;

            checkBox9.Checked = false;

            checkBox10.Checked = false;
            textBox22.Text = "";

            checkBox11.Checked = false;
            textBox23.Text = "";

            checkBox12.Checked = false;
            checkBox13.Checked = false;
            checkBox14.Checked = false;
            checkBox15.Checked = false;
            checkBox16.Checked = false;
            checkBox17.Checked = false;
            checkBox18.Checked = false;
            checkBox19.Checked = false;
            textBox24.Text = "";
            checkBox20.Checked = false;
            checkBox21.Checked = false;
            textBox25.Text = "";
            checkBox22.Checked = false;
            checkBox23.Checked = false;

            checkBox24.Checked = false;
            textBox26.Text = "";

            checkBox25.Checked = false;
            checkBox26.Checked = false;
            checkBox27.Checked = false;

            checkBox28.Checked = false;
            textBox27.Text = "";

            checkBox29.Checked = false;
            textBox28.Text = "";

            checkBox30.Checked = false;
            checkBox31.Checked = false;
            textBox29.Text = "";
            textBox30.Text = "";

            checkBox32.Checked = false;
            textBox31.Text = "";
            checkBox34.Checked = false;

            checkBox35.Checked = false;
            textBox33.Text = "";
            checkBox36.Checked = false;
            checkBox37.Checked = false;
            textBox34.Text = "";
            textBox35.Text = "";
            textBox36.Text = "";

            //Historial
            richTextBox4.Text = "";
            richTextBox5.Text = "";
            richTextBox6.Text = "";
            richTextBox7.Text = "";
            richTextBox8.Text = "";
            txtPresionArterial.Text = "";
            textBox38.Text = "";
            textBox39.Text = "";
            richTextBox9.Text = "";

            //"ImagenOdontograma", dirImagenes + txtIdentificacion.Text + "-odontograma.jpg");
            textBox45.Text = "";
            textBox46.Text = "";
            textBox47.Text = "";
            textBox48.Text = "";
            textBox49.Text = "";
            textBox50.Text = "";
            textBox51.Text = "";
            textBox52.Text = "";
            textBox53.Text = "";
            textBox54.Text = "";
            richTextBox10.Text = "";

            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";

            richTextBox12.Text = "";
            richTextBox13.Text = "";
            richTextBox15.Text = "";
            richTextBox17.Text = "";
            richTextBox19.Text = "";
            richTextBox20.Text = "";
            richTextBox21.Text = "";
            richTextBox22.Text = "";

            dateTimePicker4.CustomFormat = " ";
            dateTimePicker4.Format = DateTimePickerFormat.Custom;

            dateTimePicker7.CustomFormat = " ";
            dateTimePicker7.Format = DateTimePickerFormat.Custom;

            dateTimePicker9.CustomFormat = " ";
            dateTimePicker9.Format = DateTimePickerFormat.Custom;

            dateTimePicker11.CustomFormat = " ";
            dateTimePicker11.Format = DateTimePickerFormat.Custom;

            dateTimePicker12.CustomFormat = " ";
            dateTimePicker12.Format = DateTimePickerFormat.Custom;

            dataGridView1.Rows.Clear();

            g.Clear(Color.White);
            pictureBox6.Image = bm;

            g.DrawImage(new Bitmap(Properties.Resources.odontograma_2), new Point(1, 1));
            index = 1;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (txtIdentificacion.Text.Equals(""))
            {
                MessageBox.Show("Necesita proveer una identificacion para guardar un registro.", "Error");
            }
            else
            {
                GuardarInfoUsuario(txtIdentificacion.Text);
                Limpiar_Campos();
            }
            
        }

        public String BuscarImagen(PictureBox pictureBox)
        {
            // open file dialog   
            OpenFileDialog open = new OpenFileDialog();
            String path = "";
            // image filters  
            //open.Filter = "Image Files(*.jpg; *.jpeg; *.png; *.gif; *.bmp)|*.jpg; *.jpeg; *.png; *.gif; *.bmp";
            if (open.ShowDialog() == DialogResult.OK)
            {
                // display image in picture box  
                pictureBox.Image = new Bitmap(open.FileName);
                // image file path  
                path = open.FileName;
                //MessageBox.Show("Se ha agregado la imagen " + path, "Información");
                return path;
            }
            else
            {
                return null;
            }

        }

        void GuardarOdontograma(String name)
        {
            var sfd = new SaveFileDialog();
            sfd.FileName = "odonto-"+txtIdentificacion.Text; // Default file name
            sfd.DefaultExt = ".jpg"; // Default file extension
            sfd.Filter = "Image(*.jpg)|*.jpg|(*.*|*.*";

            if (sfd.ShowDialog()==DialogResult.OK)
            {
                Bitmap btm = bm.Clone(new Rectangle(0, 0, pictureBox6.Width, pictureBox6.Height), bm.PixelFormat);
                btm.Save(sfd.FileName, ImageFormat.Jpeg);
                MessageBox.Show("Se ha guardado la imagen en el sistema, para enlazarla al paciente tiene que guardar todo.");
                richTextBox1.Text = sfd.FileName;
            }
        }


        void GuardarImagen(TextBox path, String name)
        {
            /*try
            {
                
                Bitmap bmp1 = new Bitmap(path.Text);

                if (File.Exists(dirImagenes + name + ".jpg"))
                {
                    //picture.Image.Dispose();
                    File.Delete(dirImagenes + name + ".jpg");
                }

                bmp1.Save(dirImagenes + name + ".jpg", ImageFormat.Jpeg);
                bmp1.Dispose();
                MessageBox.Show("Se ha guardado la imagen", "Información");

                //Abrir la imagen recien guardada
                new Previsualizar(path.Text).Show();
            }
            catch (Exception)
            {
                MessageBox.Show("La imagen esta en uso, no se puede reemplazar. Reinicie el programa, abra el registro de este paciente y cambie la imagen nuevamente. \n\nNo le de 'Ver' antes de cambiarla ya que esto hace que se bloquee mientras se visualiza.", "Error");
            }*/

        }

        private void button2_Click(object sender, EventArgs e)
        {
            GuardarImagen1();
        }

        void GuardarImagen1()
        {
            //if (!textBox1.Text.Equals(""))
            //{
            //    GuardarImagen(textBox1, dir1);
            //}
            //else
            //{
            //    MessageBox.Show("No hay imagen por guardar.", "Información");
            //}
        }

        private void button7_Click(object sender, EventArgs e)
        {
            GuardarImagen2();
        }

        void GuardarImagen2()
        {
            //String dir2 = txtIdentificacion.Text + "-oclusal";

            //if (!textBox2.Text.Equals(""))
            //{
            //    GuardarImagen(textBox2, dir2);
            //}
            //else
            //{
            //    MessageBox.Show("No hay imagen por guardar.", "Información");
            //}
        }

        private void button9_Click(object sender, EventArgs e)
        {
            GuardarImagen3();
        }

        void GuardarImagen3()
        {
            //String dir3 = txtIdentificacion.Text + "-periapicales";

            //if (!textBox3.Text.Equals(""))
            //{
            //    GuardarImagen(textBox3, dir3);
            //}
            //else
            //{
            //    MessageBox.Show("No hay imagen por guardar.", "Información");
            //}
        }

        private void button11_Click(object sender, EventArgs e)
        {
            GuardarImagen4();
        }

        void GuardarImagen4()
        {
            //String dir4 = txtIdentificacion.Text + "-aletas";

            //if (!textBox4.Text.Equals(""))
            //{
            //    GuardarImagen(textBox4, dir4);
            //}
            //else
            //{
            //    MessageBox.Show("No hay imagen por guardar.", "Información");
            //}
        }

        private void button12_Click(object sender, EventArgs e)
        {
 
        }

        private void button3_Click(object sender, EventArgs e)
        {
            GuardarOdontograma(txtIdentificacion.Text);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            new Previsualizar(textBox3.Text).Show();
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            new Previsualizar(textBox4.Text).Show();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            new Previsualizar(textBox1.Text).Show();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            new Previsualizar(textBox3.Text).Show();
        }

        private void button16_Click(object sender, EventArgs e)
        {
            new Previsualizar(textBox4.Text).Show();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            new Previsualizar(textBox2.Text).Show();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            new Previsualizar(textBox1.Text).Show();
        }

        private void pictureBox3_Click_1(object sender, EventArgs e)
        {
            new Previsualizar(textBox2.Text).Show();
        }

        private void pictureBox4_Click_1(object sender, EventArgs e)
        {
            new Previsualizar(textBox3.Text).Show();
        }

        private void pictureBox5_Click_1(object sender, EventArgs e)
        {
            new Previsualizar(textBox4.Text).Show();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 0;
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 2;
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 3;
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 4;
        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 5;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Limpiar_Campos();
            Recarga_Lista();
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            pictureBox6.Image = bm;

            g.DrawImage(new Bitmap(Properties.Resources.odontograma_2), new Point(1, 1));
            index = 1;
        }

        private void comboTipoIdentificacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboTipoIdentificacion.SelectedItem.ToString().Equals("Celular"))
            {
                MessageBox.Show("Digite el numero de telefono o celular despues de la palabra CEL.");
                txtIdentificacion.Text = "CEL";
                txtIdentificacion.Focus();
            }else if (comboTipoIdentificacion.SelectedItem.ToString().Equals("Menor Edad"))
            {
                MessageBox.Show("Digite la cedula del padre o madre despues de la palabra MENOR.");
                txtIdentificacion.Text = "MENOR";
                txtIdentificacion.Focus();
            }
        }

        private void txtIdentificacion_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                LeerInfoUsuario(txtIdentificacion.Text);
            }
        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("Cedula: 9, 11, 12 o más digitos" +
                "\n\nExtranjero: Incluir números y letras" +
                "\n\nCelular: Incluir la palabra CEL seguido del número del paciente" +
                "\n\nMenor Edad: Incluir la palabra MENOR seguido de la cédula del padre o madre.\n" +
                "Nota: Para mas de un menor, incluir la palabra MENOR seguido de la cedula del padre, un guion (-) y el numero de hijo." +
                "\n\nEjemplo: MENOR1050806-3, siendo 3 el tercer hijo de esta persona.", "Digitación de Información");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBox2.Text = BuscarImagen(pictureBox3);
            //GuardarImagen2();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            textBox3.Text = BuscarImagen(pictureBox4);
            //GuardarImagen3();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            textBox4.Text = BuscarImagen(pictureBox5);
            //GuardarImagen4();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox1.Text = BuscarImagen(pictureBox1);
            //GuardarImagen1();
        }

        void GuardarInfoUsuario(String identificacion)
        {
            try
            {
                String dir = Directory.GetCurrentDirectory().ToString() + "\\Registros\\" + identificacion + ".xml";

                XmlWriter writer = XmlWriter.Create(dir);
                writer.WriteStartDocument();
                writer.WriteStartElement("Paciente");

                //Informacion Basica
                writer.WriteStartElement("InformacionBasica");
                writer.WriteElementString("Nombre", txtNombre.Text.ToString());
                writer.WriteElementString("Identificacion", txtIdentificacion.Text.ToString());
                writer.WriteElementString("TipoIdentificacion", comboTipoIdentificacion.Text.ToString());
                writer.WriteElementString("FechaIngreso", fechaIngreso.Text.ToString());
                writer.WriteElementString("Edad", Edad.Text.ToString());
                writer.WriteElementString("FechaNacimiento", fechaNacimiento.Text.ToString());
                writer.WriteElementString("LugarNacimiento", txtLugarNacimiento.Text.ToString());
                writer.WriteElementString("EstadoCivil", comboEstadoCivil.Text.ToString());
                writer.WriteElementString("Sexo", comboSexo.Text.ToString());
                writer.WriteElementString("DireccionExacta", txtDireccion.Text.ToString());
                writer.WriteElementString("Telefono", txtTelefono.Text.ToString());
                writer.WriteElementString("Celular", txtCelular.Text.ToString());
                writer.WriteElementString("Fax", txtFax.Text.ToString());
                writer.WriteElementString("Email", txtEmail.Text.ToString());

                writer.WriteElementString("Ocupacion", txtOcupacion.Text.ToString());
                writer.WriteElementString("LugarTrabajo", txtLugarTrabajo.Text.ToString());
                writer.WriteElementString("Telefono2", txtTelefono2.Text.ToString());
                writer.WriteElementString("Precauciones", txtPrecauciones.Text.ToString());

                writer.WriteEndElement(); //Finaliza Informacion Basica

                //Anamnesis
                writer.WriteStartElement("Anamnesis");
                writer.WriteElementString("MedicoCabecera", txtMedicoCabecera.Text.ToString());
                writer.WriteElementString("CasoEmergencia", txtCasoEmergencia.Text.ToString());
                writer.WriteElementString("DireccionAnamnesis", txtDireccion2.Text.ToString());

                writer.WriteElementString("TipoSangre", txtSangre.Text.ToString());
                writer.WriteElementString("TelefonoAnamnesis", txttelefonoAnamnesis.Text.ToString());
                writer.WriteElementString("Parentesco", txtParentesco.Text.ToString());

                writer.WriteElementString("QuejaPrincipal", txtQuejaPrincipal.Text.ToString());
                writer.WriteElementString("HistorialQueja", txtQueja.Text.ToString());


                writer.WriteEndElement(); //Finaliza Anamnesis

                //Historia Medica
                writer.WriteStartElement("HistoriaMedica");

                writer.WriteElementString("EnfermedadProcedimiento", checkBox1.Checked.ToString());
                writer.WriteElementString("EnfermedadProcedimientoTxt", textBox18.Text.ToString());

                writer.WriteElementString("ProblemasPresion", checkBox2.Checked.ToString());
                writer.WriteElementString("ProblemasPresionTxt", textBox19.Text.ToString());

                writer.WriteElementString("FiebreReumatica", checkBox3.Checked.ToString());
                writer.WriteElementString("VIHoSIDA", checkBox4.Checked.ToString());
                writer.WriteElementString("AccidenteCerebrovascular", checkBox5.Checked.ToString());
                writer.WriteElementString("Anemia", checkBox6.Checked.ToString());
                writer.WriteElementString("TransfusionSangre", checkBox7.Checked.ToString());

                writer.WriteElementString("Equimosis", checkBox9.Checked.ToString());

                writer.WriteElementString("ProblemasRinon", checkBox10.Checked.ToString());
                writer.WriteElementString("ProblemasRinonTxt", textBox22.Text.ToString());

                writer.WriteElementString("ProblemasGastrointestinales", checkBox11.Checked.ToString());
                writer.WriteElementString("ProblemasGastrointestinalesTxt", textBox23.Text.ToString());

                writer.WriteElementString("ProblemasVision", checkBox12.Checked.ToString());
                writer.WriteElementString("TratamientoCorticoesteroides", checkBox13.Checked.ToString());
                writer.WriteElementString("Diabetes12", checkBox14.Checked.ToString());
                writer.WriteElementString("EpilepsiaConvulsionDesmayo", checkBox15.Checked.ToString());
                writer.WriteElementString("EnfermedadesRespiratorias", checkBox16.Checked.ToString());
                writer.WriteElementString("RadioterapiaQuimioterapia", checkBox17.Checked.ToString());
                writer.WriteElementString("Reumatismo", checkBox18.Checked.ToString());

                writer.WriteElementString("ProblemasHepaticos", checkBox19.Checked.ToString());
                writer.WriteElementString("ProblemasHepaticosTxt", textBox24.Text.ToString());

                writer.WriteElementString("Herpes", checkBox20.Checked.ToString());
                writer.WriteElementString("PerdidaAumentoPeso", checkBox21.Checked.ToString());
                writer.WriteElementString("PerdidaAumentoPesoTxt", textBox25.Text.ToString());


                writer.WriteElementString("Artritis", checkBox22.Checked.ToString());
                writer.WriteElementString("TratamientoPsiquiatrico", checkBox23.Checked.ToString());

                writer.WriteElementString("Tiroides", checkBox24.Checked.ToString());
                writer.WriteElementString("TiroidesTxt", textBox26.Text.ToString());

                writer.WriteElementString("Ets", checkBox25.Checked.ToString());
                writer.WriteElementString("Osteoporosis", checkBox26.Checked.ToString());
                writer.WriteElementString("Migrana", checkBox27.Checked.ToString());

                writer.WriteElementString("TratamientoBifosfonados", checkBox28.Checked.ToString());
                writer.WriteElementString("TratamientoBifosfonadosTxt", textBox27.Text.ToString());

                writer.WriteElementString("ConsumoDroga", checkBox29.Checked.ToString());
                writer.WriteElementString("ConsumoDrogaTxt", textBox28.Text.ToString());

                writer.WriteElementString("Fuma", checkBox30.Checked.ToString());
                writer.WriteElementString("Fumo", checkBox31.Checked.ToString());
                writer.WriteElementString("CigarrosTxt", textBox29.Text.ToString());
                writer.WriteElementString("TiempoFumandoTxt", textBox30.Text.ToString());

                writer.WriteElementString("BebidasAlcoholicas", checkBox32.Checked.ToString());
                writer.WriteElementString("FrecuenciaAlcoholicaTxt", textBox31.Text.ToString());
                writer.WriteElementString("ShockAnafilactico", checkBox34.Checked.ToString());

                writer.WriteElementString("Embarazo", checkBox35.Checked.ToString());
                writer.WriteElementString("SemanasEmbarazoTxt", textBox33.Text.ToString());
                writer.WriteElementString("Anticonceptivos", checkBox36.Checked.ToString());
                writer.WriteElementString("Ginecoobstetricos", checkBox37.Checked.ToString());
                writer.WriteElementString("PartosTxt", textBox34.Text.ToString());
                writer.WriteElementString("AbortosTxt", textBox35.Text.ToString());
                writer.WriteElementString("CesareasTxt", textBox36.Text.ToString());
                writer.WriteEndElement(); //Finaliza Historia Medica

                //Historial
                writer.WriteStartElement("Historial");
                writer.WriteElementString("Observaciones", richTextBox4.Text.ToString());
                writer.WriteElementString("AntecedentesAlergicos", richTextBox5.Text.ToString());
                writer.WriteElementString("AntecedentesQuirurgicos", richTextBox6.Text.ToString());
                writer.WriteElementString("Medicamentos", richTextBox7.Text.ToString());
                writer.WriteElementString("HistoriaDental", richTextBox8.Text.ToString());

                writer.WriteElementString("PresionArterial", txtPresionArterial.Text.ToString());
                writer.WriteElementString("Pulso", textBox38.Text.ToString());
                writer.WriteElementString("FrecuenciaRespiratoria", textBox39.Text.ToString());
                writer.WriteElementString("TejidosBlandos", richTextBox9.Text.ToString());
                writer.WriteEndElement(); //Finaliza Historial

                //Odontograma
                writer.WriteStartElement("Odontograma");

                writer.WriteElementString("ImagenOdontograma", richTextBox1.Text); ////////////////////

                writer.WriteElementString("HuesoAlveolar", textBox45.Text.ToString());
                writer.WriteElementString("Calculo", textBox46.Text.ToString());
                writer.WriteElementString("Caries", textBox47.Text.ToString());
                writer.WriteElementString("DientesAusentes", textBox48.Text.ToString());
                writer.WriteElementString("MorfologiaRadicular", textBox49.Text.ToString());
                writer.WriteElementString("Patologia", textBox50.Text.ToString());
                writer.WriteElementString("TercerasMolares", textBox51.Text.ToString());
                writer.WriteElementString("AusenciaCongenita", textBox52.Text.ToString());
                writer.WriteElementString("Supernumerarios", textBox53.Text.ToString());
                writer.WriteElementString("TorusExostosis", textBox54.Text.ToString());
                writer.WriteElementString("Observaciones", richTextBox10.Text.ToString());

                writer.WriteEndElement(); //Finaliza Odontograma

                //Imagenologia
                writer.WriteStartElement("Imagenologia");
                writer.WriteElementString("ImagenFecha1", textBox1.Text.ToString());
                writer.WriteElementString("ImagenFecha2", textBox2.Text.ToString());
                writer.WriteElementString("ImagenFecha3", textBox3.Text.ToString());
                writer.WriteElementString("ImagenFecha4", textBox4.Text.ToString());

                writer.WriteElementString("Fecha1", dateTimePicker4.Text.ToString());
                writer.WriteElementString("Observaciones", richTextBox12.Text.ToString());

                writer.WriteElementString("Fecha2", dateTimePicker7.Text.ToString());
                writer.WriteElementString("Observaciones2", richTextBox13.Text.ToString());

                writer.WriteElementString("Fecha3", dateTimePicker9.Text.ToString());
                writer.WriteElementString("Observaciones3", richTextBox15.Text.ToString());

                writer.WriteElementString("Fecha4", dateTimePicker11.Text.ToString());
                writer.WriteElementString("Observaciones4", richTextBox17.Text.ToString());

                writer.WriteElementString("PlanTratamiento", richTextBox19.Text.ToString());

                writer.WriteElementString("ExamenesSolicitados", richTextBox20.Text.ToString());
                writer.WriteElementString("Resultados", richTextBox21.Text.ToString());
                writer.WriteElementString("Fecha", dateTimePicker12.Text.ToString());
                writer.WriteElementString("ExamenExtraoral", richTextBox22.Text.ToString());

                writer.WriteEndElement(); //Finaliza Imagenologia

                //Tratamiento Efectuado
                writer.WriteStartElement("TratamientoEfectuado");

                for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                {
                    writer.WriteStartElement("Fila");
                    writer.WriteElementString("Fecha", dataGridView1.Rows[i].Cells[0].Value.ToString());
                    writer.WriteElementString("Dientes", dataGridView1.Rows[i].Cells[1].Value.ToString());
                    writer.WriteElementString("Descripcion", dataGridView1.Rows[i].Cells[2].Value.ToString());

                    if (dataGridView1.Rows[i].Cells[3].Value.ToString().Equals(""))
                    {
                        writer.WriteElementString("MontoTotal", "0");
                    }
                    else
                    {
                        writer.WriteElementString("MontoTotal", dataGridView1.Rows[i].Cells[3].Value.ToString());
                    }

                    if (dataGridView1.Rows[i].Cells[4].Value.ToString().Equals(""))
                    {
                        writer.WriteElementString("Abono", "0");
                    }
                    else
                    {
                        writer.WriteElementString("Abono", dataGridView1.Rows[i].Cells[4].Value.ToString());
                    }

                    if (dataGridView1.Rows[i].Cells[5].Value.ToString().Equals(""))
                    {
                        writer.WriteElementString("Saldo", "0");
                    }
                    else
                    {
                        writer.WriteElementString("Saldo", dataGridView1.Rows[i].Cells[5].Value.ToString());
                    }

                    writer.WriteEndElement();

                }

                writer.WriteEndElement(); //Finaliza
                                          //Finaliza Tratamiento Efectuado

                writer.WriteEndElement(); //Fin de Paciente
                writer.WriteEndDocument(); //Fin del documento
                writer.Flush();

                writer.Close();

                MessageBox.Show("Se ha guardado la información del usuario.", "Guardado");

                //xml.SelectSingleNode("//reminder/Title").InnerText = "NewValue";
            }
            catch (Exception)
            {
                MessageBox.Show("Ha ocurrido un error al tratar de guardar la información del usuario.", "Error");
            }
        }
    }
}
