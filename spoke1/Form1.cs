using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ConsoleApplication1;
using Timer = System.Threading.Timer;

namespace spoke1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();


            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

            this.Paint += Form1_Paint;
            this.Load += Form1_Load;
            this.MouseDown += Form1_MouseDown;
            this.MouseUp += Form1_MouseUp;
            this.MouseMove += Form1_MouseMove;

        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            isMouseDown = true;
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
        }

        private Point MousePoint;
        private bool isMouseDown;

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {

            MousePoint = e.Location;

        }



        private void Form1_Load(object sender, EventArgs e)
        {





            var l = new liqud(MouseStatus);
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += l.run;
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerAsync();

        }

        public Tuple<Point,bool> MouseStatus()
        {
            return new Tuple<Point, bool>(MousePoint,isMouseDown);
        }
        

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lines = ((List<Tuple<PointF, PointF>>) e.UserState);
            ticks++;
            Invalidate();


        }
         

        private List<Tuple<PointF, PointF>> lines;
        private int ticks = 0;
        

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.Black, e.ClipRectangle);
            e.Graphics.DrawString("frame "+ticks, this.Font, Brushes.White, 0, 0);
            if (lines!=null)
            {
           
                for (int index = lines.Count-1; index >= 0; index--)
                {
                    var line = lines[index];
                    if (line.Item1 == line.Item2)
                    {  e.Graphics.DrawLine(new Pen(Color.Red), line.Item1, new PointF(line.Item2.X+0.01f, line.Item2.Y));

                    }
                    else
                     //   e.Graphics.FillRectangle((new SolidBrush(Color.Blue)), new RectangleF(line.Item1, new SizeF(line.Item2.X - line.Item1.X, line.Item2.Y - line.Item1.Y)));
                    e.Graphics.DrawLine(new Pen(Color.Blue), line.Item1, line.Item2);
                }
                lines.Clear();

            }
        }
    }
}
