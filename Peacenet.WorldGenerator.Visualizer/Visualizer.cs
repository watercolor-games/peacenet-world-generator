using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Peacenet.WorldGenerator.Visualizer
{
    public partial class Visualizer : Form
    {
        private const int _worldSize = 512;
        private Heightmap _heightmap = null;
        private NPCMap _npc = null;


        public Visualizer()
        {
            InitializeComponent();
        }

        private void reportProgress(int value, string message)
        {
            Invoke(new Action(() =>
            {
                worker.ReportProgress(value, message);
            }));
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                worker.ReportProgress(0, "Allocating memory...");
                Invoke(new Action(() =>
                {
                    if (pbHeightmap.Image != null)
                    {
                        pbHeightmap.Image.Dispose();
                        pbHeightmap.Image = null;
                    }
                }));
                _heightmap = new Heightmap();
                _heightmap.Width = _worldSize;
                _heightmap.Height = _worldSize;
                worker.ReportProgress(0, "Generating heightmap...");
                var map = _heightmap.Generate();
                worker.ReportProgress(0, "Rendering preview...");
                var bmp = new Bitmap(_worldSize, _worldSize);
                var lck = bmp.LockBits(new Rectangle(0, 0, _worldSize, _worldSize), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                var data = new byte[Math.Abs(lck.Stride) * lck.Height];
                for (int i = 0; i < data.Length; i += 4)
                {
                    double v = map[i / 4];
                    byte gray = (byte)(v * 255);
                    data[i] = gray;
                    data[i + 1] = gray;
                    data[i + 2] = gray;
                    data[i + 3] = 255;
                    float p = (float)i / data.Length;
                    worker.ReportProgress((int)(p * 100), "Rendering preview...");
                }
                Marshal.Copy(data, 0, lck.Scan0, data.Length);
                data = null;
                bmp.UnlockBits(lck);
                lck = null;
                pbHeightmap.Image = bmp;

                _npc = new NPCMap(_heightmap.Seed, map);

                map = null;

                worker.ReportProgress(0, "Generating NPC density map...");

                var npcMap = _npc.GenerateDensityMap();

                bmp = new Bitmap(_worldSize, _worldSize);
                lck = bmp.LockBits(new Rectangle(0, 0, _worldSize, _worldSize), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                data = new byte[Math.Abs(lck.Stride) * lck.Height];
                for (int i = 0; i < data.Length; i += 4)
                {
                    bool v = npcMap[i / 4];
                    if(v)
                    {
                        data[i] = 255;
                        data[i + 1] = 255;
                        data[i + 2] = 255;
                        data[i + 3] = 255;
                    }
                    else
                    {
                        data[i] = 0;
                        data[i + 1] = 0;
                        data[i + 2] = 0;
                        data[i + 3] = 255;
                    }
                    float p = (float)i / data.Length;
                    worker.ReportProgress((int)(p * 100), "Rendering preview...");
                }
                Marshal.Copy(data, 0, lck.Scan0, data.Length);
                data = null;
                bmp.UnlockBits(lck);
                lck = null;
                pbDensity.Image = bmp;

                worker.ReportProgress(0, "Generating NPC type map...");

                var types = _npc.GetTypeMap(npcMap);

                bmp = new Bitmap(_worldSize, _worldSize);
                lck = bmp.LockBits(new Rectangle(0, 0, _worldSize, _worldSize), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                data = new byte[Math.Abs(lck.Stride) * lck.Height];
                for (int i = 0; i < data.Length; i += 4)
                {
                    int t = types[i / 4];
                    Color px = Color.Black;
                    switch(t)
                    {
                        case 1:
                            px = Color.Yellow;
                            break;
                        case 2:
                            px = Color.Red;
                            break;
                        case 3:
                            px = Color.Orange;
                            break;
                        case 4:
                            px = Color.Gray;
                            break;
                        case 5:
                            px = Color.Pink;
                            break;
                        case 6:
                            px = Color.Green;
                            break;
                        case 7:
                            px = Color.Blue;
                            break;
                        case 8:
                            px = Color.White;
                            break;
                    }
                    data[i] = px.R;
                    data[i + 1] = px.G;
                    data[i + 2] = px.B;
                    data[i + 3] = 255;
                    float p = (float)i / data.Length;
                    worker.ReportProgress((int)(p * 100), "Rendering preview...");
                }
                Marshal.Copy(data, 0, lck.Scan0, data.Length);
                data = null;
                bmp.UnlockBits(lck);
                lck = null;
                pbTypes.Image = bmp;


            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show(caption: "Heightmap generation error", text: ex.Message, buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                }));
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            worker.RunWorkerAsync();
        }

        private void Visualizer_Load(object sender, EventArgs e)
        {
            worker.ProgressChanged += Worker_ProgressChanged;
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pgGeneration.Value = e.ProgressPercentage;
            lbStatus.Text = e.UserState.ToString();
        }
    }
}
