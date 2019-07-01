﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AsteroidsApp1
{
    public partial class Form1 : Form
    {
        const int SHIP_SIZE = 20;
        const int SHIP_TURN_SPD = 270;
        const int SHIP_THRUST = 5;
        const float FRICTION = 0.5f;
        const bool SHOW_BOUNDING = false; // show or hide collision bounding
        const int ROID_SIZE = 100; // starting size of asteroids in pixels
        const int FPS = 30;
        const int ROID_SPD = 50; // max starting speed of asteroids in pixels per second
        const int ROID_VERT = 10; // average number of vertices on each asteroid
        const int ROID_NUM = 3; // starting number of asteroids
        const int GAME_LIVES = 3; // starting number of lives

        int roidsTotal = 0;
        int roidsLeft = 0;
        int level = 0;
        int lives = 0;

        PointF[] ShipFigure = new PointF[4];
        PointF[] ThrustingFigure = new PointF[3];

        Bitmap finalImage = new Bitmap(640, 480);
        Graphics gfx;
        

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
        bool ThrustShowFlag = false;

        struct AsteroidStruct
        {
            public float x;
            public float y;
            public float xv;
            public float yv;
            public float a;
            public float r;
        }

        List<AsteroidStruct> roids = new List<AsteroidStruct>();
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
            gfx = Graphics.FromImage(finalImage); // pictureBox1.CreateGraphics() - Галімий метод

            newGame(); // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

            tmrMain.Interval = 1000 / FPS;
            tmrMain.Enabled = true;
            //System.Media.SystemSounds.Beep.Play();
        }

        private void newGame()
        {
            level = 0;
            lives = GAME_LIVES;
            ship = NewShip();

            NewLevel();
        }

        private void NewLevel()
        {
            createAsteroidBelt();
        }

        private void PlaySoundFromRes()
        {
            System.Media.SoundPlayer snd = new System.Media.SoundPlayer(res.Drill);
            snd.Play();
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

        private void RefreshScene()
        {
            // gfx.DrawImage(Bitmap, new Rectangle(x, y, w, h));
            gfx.Clear(Color.Transparent);
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
                if (Math.Abs(ship.thrustX) < 0.01f) ship.thrustX = 0;
                ship.thrustY -= FRICTION * ship.thrustY / FPS;
                if (Math.Abs(ship.thrustY) < 0.01f) ship.thrustY = 0;
            }

            // rotate the ship
            ship.a += ship.rot;

            // move the ship
            ship.x += ship.thrustX;
            ship.y += ship.thrustY;

            
            PaintString(ShowSPD(), 10, 30);

            // handle edge of screen
            HandleEdgeOfScreen();

            // paint the ship
            PaintShip();

            // Move asteroids 
            for (var i = 0; i < roids.Count; i++)
            {
                AsteroidStruct tmp = roids[i];
                tmp.x += roids[i].xv;
                tmp.y += roids[i].yv;
                
                // handle asteroid edge of screen
                if (roids[i].x < 0 - roids[i].r) tmp.x= 640 + roids[i].r; else if (roids[i].x > 640 + roids[i].r) tmp.x = 0 - roids[i].r;
                if (roids[i].y < 0 - roids[i].r) tmp.y = 480 + roids[i].r; else if (roids[i].y > 480 + roids[i].r) tmp.y = 0 - roids[i].r;

                roids[i] = tmp;

            }
            //PaintString(roids[0].xv.ToString(), 10, 70);
            // Paint Asteroids
            DrawTheAsteroids();
           
            // paint thrusting
            if (ship.thrusting) PaintShipThrust();

            // show ship collision
            if (SHOW_BOUNDING) gfx.DrawArc(new Pen(Color.Red, 1), ship.x - (SHIP_SIZE / 2), ship.y - (SHIP_SIZE / 2), SHIP_SIZE, SHIP_SIZE, 0, 360);

            PaintString(DateTime.Now.ToString("HH:mm:ss"), 10, 10);

            //if (DateTime.Now.Second == 0 && SoundFlag) { PlaySoundFromRes(); SoundFlag = false; }
            //if (DateTime.Now.Second == 1) SoundFlag = true;



            pic.Image = finalImage;
            if (DateTime.Now.Second % 2 == 0) GC.Collect();
        }

        private void DrawTheAsteroids()
        {
            // draw the asteroids
            float a, r, x, y, offs, vert;
            for (var i = 0; i < roids.Count; i++)
            {
                
                // get the asteroid properties
                r = roids[i].r;
                x = roids[i].x;
                y = roids[i].y;

                gfx.DrawArc(new Pen(Color.Red, 3), x - r, y - r, r, r, 0, 360);
            }
        }

        private AsteroidStruct NewAsteroid(float x, float y, float r)
        {
            float lvlMult = 1f + 0.1f * level;
            AsteroidStruct roid = new AsteroidStruct();
            Random rnd = new Random();
            roid.x = x;
            roid.y = y;
            roid.xv = (rnd.Next(0, 10) / 10f) * ROID_SPD * lvlMult / FPS * (rnd.Next(10) < 5 ? 1f : -1f);
            Thread.Sleep(50);
            roid.yv = (rnd.Next(0, 10) / 10f) * ROID_SPD * lvlMult / FPS * (rnd.Next(10) < 5 ? 1f : -1f);
            Thread.Sleep(50);
            roid.a = (rnd.Next(0, 10) / 10f) * (float)Math.PI * 2f;
            Thread.Sleep(50);
            roid.r = r;
            return roid;
        }

        private string ShowSPD()
        {
            return "SPD: " + (Math.Round(Math.Sqrt((ship.thrustX * ship.thrustX) + (ship.thrustY * ship.thrustY)), 2)*100).ToString();
        }

        private void createAsteroidBelt()
        {
            roids.Clear();
            roidsTotal = (ROID_NUM + level) * 7;
            roidsLeft = roidsTotal;
            Random rnd=new Random();
            float x=0f, y=0f;
            for (var i = 0; i < ROID_NUM + level; i++)
            {
                // random asteroid location (not touching spaceship)
                do
                {
                    x = rnd.Next(640);
                    y = rnd.Next(480);
                } while (distBetweenPoints(ship.x, ship.y, x, y) < ROID_SIZE * 2 + ship.r);
                roids.Add(NewAsteroid(x, y, (float)Math.Ceiling((double)ROID_SIZE / 2)));
            }
        }

        private float distBetweenPoints(float x1, float y1, float x2, float y2)
        {
            return (float)Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        private void PaintString(string msg, int x, int y)
        {
            gfx.DrawString(msg, new Font("Arial", 16), new SolidBrush(Color.FromArgb(128, Color.White)), x, y, new StringFormat());
        }

        private void HandleEdgeOfScreen()
        {

            if (ship.x < 0 - ship.r) { ship.x = pic.Width + ship.r; }
            else if (ship.x > pic.Width + ship.r) { ship.x = 0 - ship.r; }
            if (ship.y < 0 - ship.r) { ship.y = pic.Height + ship.r; }
            else if (ship.y > pic.Height + ship.r) { ship.y = 0 - ship.r; }
        }

        private void PaintShipThrust()
        {
            ThrustShowFlag = !ThrustShowFlag;
            if (ThrustShowFlag)
            {
                float sinA = (float)Math.Sin(ship.a);
                float cosA = (float)Math.Cos(ship.a);
                ThrustingFigure[0] = new PointF(ship.x - ship.r * (2f / 3f * cosA + 0.2f * sinA), ship.y + ship.r * (2f / 3f * sinA - 0.2f * cosA));
                ThrustingFigure[1] = new PointF(ship.x - ship.r * 3.3f / 3f * cosA, ship.y + ship.r * 3.3f / 3f * sinA);
                ThrustingFigure[2] = new PointF(ship.x - ship.r * (2f / 3f * cosA - 0.2f * sinA), ship.y + ship.r * (2f / 3f * sinA + 0.2f * cosA));
                gfx.DrawLines(new Pen(Color.Orange, 5), ThrustingFigure);
            }
        }

        private void PaintShip()
        {
            float sinA = (float)Math.Sin(ship.a);
            float cosA = (float)Math.Cos(ship.a);
            ShipFigure[0] = new PointF(ship.x + 4f / 3f * ship.r * cosA, ship.y - 4f / 3f * ship.r * sinA);
            ShipFigure[1] = new PointF(ship.x - ship.r * 2f / 3f * (cosA + sinA), ship.y + ship.r * 2f / 3f * (sinA - cosA));
            ShipFigure[2] = new PointF(ship.x - ship.r * 2f / 3f * (cosA - sinA), ship.y + ship.r * 2f / 3f * (sinA + cosA));
            ShipFigure[3] = ShipFigure[0];
            gfx.DrawLines(new Pen(Color.Yellow, 2), ShipFigure);
        }

        private void tmrMain_Tick(object sender, EventArgs e)
        {
            RefreshScene();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    break;
                case Keys.Left:
                    ship.rot = SHIP_TURN_SPD / 180f * (float)Math.PI / FPS;
                    break;
                case Keys.Right:
                    ship.rot = -SHIP_TURN_SPD / 180f * (float)Math.PI / FPS;
                    break;
                case Keys.Up:
                    ship.thrusting = true;
                    break;
                case Keys.Escape:

                    Application.Exit();
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
