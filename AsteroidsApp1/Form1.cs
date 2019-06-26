using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AsteroidsApp1
{
    public partial class Form1 : Form
    {
        const int SHIP_SIZE= 30;
        const int SHIP_TURN_SPD = 270;
        const int SHIP_THRUST = 5;
        const float FRICTION = 0.6f;
        PointF[] ShipFigure = new PointF[4];
        PointF[] ThrustingFigure = new PointF[3];

        int testAngle = 90;

        Bitmap finalImage;
        int FPS = 30;
        Point ShipPos;

        struct ShipStruct
        {
            public float x;
            public float y;
            public float a;
            public int r;
            public int blinkNum;
            public int blinkTime;
            public bool canShoot;
            public bool dead;
            public int explodeTime;
            public List<int> lasers;
            public float rot;
            public bool thrusting;
            public float thrustX;
            public float thrustY;
        }
        ShipStruct ship;

        bool rotateLeftFlag = false;
        bool rotateRightFlag = false;

        struct MyPoint
        {
            public float x;
            public float y;
        }

        //#####################################################################################################################
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pic.SetBounds(0, 0, 640, 480);
            pic.BackgroundImage = res.kosmos;
            this.ClientSize = new Size(640, 480);
            this.Text = "Asteroids by Yozheg";
            
            ship = NewShip();
            

            tmrMain.Interval = 1000 / FPS;
            tmrMain.Enabled = true;
        }

        private ShipStruct NewShip()
        {
            return new ShipStruct
            {
                x = 320,
                y = 240,
                a = 90f / 180f * (float)Math.PI,
                r = SHIP_SIZE / 2,
                blinkNum = 0,
                blinkTime = 0,
                canShoot = false,
                dead = false,
                explodeTime = 0,
                rot = 0,
                thrusting = false,
                thrustX = 0,
                thrustY = 0
            };
        }

        private void PaintLevel()
        {
            float x = ship.x, y = ship.y;
            finalImage = new Bitmap(640, 480);
            using (Graphics g = Graphics.FromImage(finalImage))//pictureBox1.CreateGraphics()
            {
                //g.RotateTransform(-28,System.Drawing.Drawing2D.MatrixOrder.Prepend);
                g.Clear(Color.Transparent);
                //g.DrawImage(Bitmap, new Rectangle(x, y, w, h));

                
                //g.DrawEllipse(new Pen(Color.Yellow, 1), x - (SHIPSIZE / 2), y - (SHIPSIZE / 2), (SHIPSIZE / 2) + (SHIPSIZE / 2), (SHIPSIZE / 2) + (SHIPSIZE / 2));


                // thrust the ship
                if (ship.thrusting && !ship.dead)
                {
                    ship.thrustX += SHIP_THRUST * (float)Math.Cos(ship.a) / FPS;
                    ship.thrustY -= SHIP_THRUST * (float)Math.Sin(ship.a) / FPS;
                }
                else
                {
                    // apply friction (slow the ship down when not thrusting)
                    ship.thrustX -= FRICTION * ship.thrustX / FPS;
                    ship.thrustY -= FRICTION * ship.thrustY / FPS;
                }

                // rotate the ship
                ship.a += ship.rot;
                // move the ship
                ship.x += ship.thrustX;
                ship.y += ship.thrustY;

                // handle edge of screen
                if (ship.x < 0 - ship.r) { ship.x = pic.Width + ship.r; }
                else if (ship.x > pic.Width + ship.r) { ship.x = 0 - ship.r; }
                if (ship.y < 0 - ship.r) { ship.y = pic.Height + ship.r; }
                else if (ship.y > pic.Height + ship.r) { ship.y = 0 - ship.r; }

                // paint the ship
                float sinA = (float)Math.Sin(ship.a);
                float cosA = (float)Math.Cos(ship.a);
                ShipFigure[0] = new PointF(x + 4f / 3f * ship.r * cosA, y - 4f / 3f * ship.r * sinA);
                ShipFigure[1] = new PointF(x - ship.r * 2f / 3f * (cosA + sinA), y + ship.r * 2f / 3f * (sinA - cosA));
                ShipFigure[2] = new PointF(x - ship.r * 2f / 3f * (cosA - sinA), y + ship.r * 2f / 3f * (sinA + cosA));
                ShipFigure[3] = ShipFigure[0];
                g.DrawLines(new Pen(Color.Yellow,2), ShipFigure);

                // paint thrusting
                if (ship.thrusting)
                {
                    ThrustingFigure[0] = new PointF(x - ship.r * (2f / 3f * cosA + 0.2f * sinA), y + ship.r * (2f / 3f * sinA - 0.2f * cosA));
                    ThrustingFigure[1] = new PointF(x - ship.r * 3.3f / 3f * cosA, y + ship.r * 3.3f / 3f * sinA);
                    ThrustingFigure[2] = new PointF(x - ship.r * (2f / 3f * cosA - 0.2f * sinA), y + ship.r * (2f / 3f * sinA + 0.2f * cosA));
                    g.DrawLines(new Pen(Color.Orange,5), ThrustingFigure);
                }

                g.DrawString(""+DateTime.Now.ToString("HH:mm:ss"), new Font("Arial", 16), new SolidBrush(Color.FromArgb(128,Color.White)), 10, 10, new StringFormat());
            }
            pic.Image = finalImage;
            GC.Collect();
        }

        private void tmrMain_Tick(object sender, EventArgs e)
        {
            PaintLevel();
            
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    break;
                case Keys.Left:
                    ship.rot = (float)SHIP_TURN_SPD / 180f * (float)Math.PI / (float)FPS;
                    break;
                case Keys.Right:
                    ship.rot = -(float)SHIP_TURN_SPD / 180f * (float)Math.PI / (float)FPS;
                    break;
                case Keys.Up:
                    ship.thrusting = true;
                    break;

            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    break;
                case Keys.Left:
                    ship.rot = 0f;
                    break;
                case Keys.Right:
                    ship.rot = 0f;
                    break;
                case Keys.Up:
                    ship.thrusting = false;
                    break;
            }
        }
    }
}
