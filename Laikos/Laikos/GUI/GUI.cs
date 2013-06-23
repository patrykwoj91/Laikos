using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Laikos
{
    static class GUI
    {
        public enum Events
        {
            GuiCLICK,
            GuiUP,
            GuiDOWN
        };


        public static int screenWidth;
        public static int screenHeight;
        private static SpriteBatch spriteBatch;
        private static List<Message> messages;
        private static bool block;

        public static void Initialize(GraphicsDevice Device, SpriteBatch SpriteBatch, ContentManager content, Player player)
        {
            block = false;
            screenWidth = Device.PresentationParameters.BackBufferWidth;
            screenHeight = Device.PresentationParameters.BackBufferHeight;
            spriteBatch = SpriteBatch;
            messages = new List<Message>();
            MinimapBackground.Initialize(content);
            UpperBackground.Initialize(content);
            UnitBackground.Initialize(content);
            LowerBackground.Initialize(content);
            SourcesButton.Initialize(content, player);
            LowerOptionPanel.Initialize(content);
        }

        public static void Draw()
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            //Minimap
            MinimapBackground.Create(spriteBatch);
            UpperBackground.Create(spriteBatch);
            LowerBackground.Create(spriteBatch);
            UnitBackground.Create(spriteBatch);
            SourcesButton.Create(spriteBatch);
            LowerOptionPanel.Create(spriteBatch);
            SelectingGUI.DrawUnitInfo(spriteBatch);
            spriteBatch.End();
        }

        public static void Update(GameTime gameTime)
        {
            UnitBackground.UpdateAnimation(gameTime);
            HandleEvent();
            if (UnitBackground.upTime <= 1.0f)
            {
                UnitBackground.MoveUp();
                LowerBackground.MoveUp();
                LowerOptionPanel.MoveUp();
            }
            else if (UnitBackground.downTime <= 1.0f)
            {
                UnitBackground.MoveDown();
                LowerBackground.MoveDown();
                LowerOptionPanel.MoveDown();
            }
            CleanMessages();
        }

        public static void ProcessInput(Game1 game)
        {
            if (Input.currentMouseState.LeftButton == ButtonState.Pressed)
            {
                if (Input.currentMouseState.X < Minimap.width + Minimap.diff && Input.currentMouseState.Y < Minimap.height + Minimap.diff)
                {
                    Camera.cameraPosition.X = Input.currentMouseState.X * 5;
                    Camera.cameraPosition.Z = Input.currentMouseState.Y * 5 + 75;
                }
            }

            if (Input.oldMouseState.LeftButton == ButtonState.Pressed && Input.currentMouseState.LeftButton == ButtonState.Released)
            {
               if (insideRectangle(LowerOptionPanel.firstTabPosition) && UnitBackground.whichUnit == 0 && LowerOptionPanel.isUnit)
                {
                    CreateMessage(new Message((int)GUI.Events.GuiCLICK, 1, 0, game)); //1 to nadawca czyli 1 button , 0 to odbiorca czyli kto ma wykonac : 0 to dron
                }
                else if (insideRectangle(LowerOptionPanel.secondTabPosition) && UnitBackground.whichUnit == 0 && LowerOptionPanel.isUnit)
                {
                    CreateMessage(new Message((int)GUI.Events.GuiCLICK, 2, 0, game)); //1 to nadawca czyli 1 button , 0 to odbiorca czyli kto ma wykonac : 0 to dron
                }
               else if (insideRectangle(LowerOptionPanel.thirdTabPosition) && UnitBackground.whichUnit == 0 && LowerOptionPanel.isUnit)
                {
                    CreateMessage(new Message((int)GUI.Events.GuiCLICK, 3, 0, game)); //1 to nadawca czyli 1 button , 0 to odbiorca czyli kto ma wykonac : 0 to dron
                }
               else if (insideRectangle(LowerOptionPanel.fourthTabPosition) && UnitBackground.whichUnit == 0 && LowerOptionPanel.isUnit)
                {
                    CreateMessage(new Message((int)GUI.Events.GuiCLICK, 4, 0, game)); //1 to nadawca czyli 1 button , 0 to odbiorca czyli kto ma wykonac : 0 to dron
                }

               else if (insideRectangle(LowerOptionPanel.firstTabPosition) && UnitBackground.whichUnit == 1 && !LowerOptionPanel.isUnit)
                {
                    CreateMessage(new Message((int)GUI.Events.GuiCLICK, 1, 1, game)); //1 to nadawca czyli 1 button , 0 to odbiorca czyli kto ma wykonac : 0 to dron
                }
               else if (insideRectangle(LowerOptionPanel.secondTabPosition) && UnitBackground.whichUnit == 1 && !LowerOptionPanel.isUnit)
                {
                    CreateMessage(new Message((int)GUI.Events.GuiCLICK, 2, 1, game)); //1 to nadawca czyli 1 button , 0 to odbiorca czyli kto ma wykonac : 0 to dron
                }
               else if (insideRectangle(LowerOptionPanel.thirdTabPosition) && UnitBackground.whichUnit == 1 && !LowerOptionPanel.isUnit)
                {
                    CreateMessage(new Message((int)GUI.Events.GuiCLICK, 3, 1, game)); //1 to nadawca czyli 1 button , 0 to odbiorca czyli kto ma wykonac : 0 to dron
                }
               else if (insideRectangle(LowerOptionPanel.firstTabPosition) && UnitBackground.whichUnit == 2 && !LowerOptionPanel.isUnit)
               {
                   CreateMessage(new Message((int)GUI.Events.GuiCLICK, 1, 2, game)); //1 to nadawca czyli 1 button , 0 to odbiorca czyli kto ma wykonac : 0 to dron
               }
               else if (insideRectangle(LowerOptionPanel.secondTabPosition) && UnitBackground.whichUnit == 2 && !LowerOptionPanel.isUnit)
               {
                   CreateMessage(new Message((int)GUI.Events.GuiCLICK, 2, 2, game)); //1 to nadawca czyli 1 button , 0 to odbiorca czyli kto ma wykonac : 0 to dron
               }
         
            }
        }

        public static void HandleEvent()
        {
            EventManager.FindMessage(delegate(Message e) { return ((EventManager.Events)e.Type).ToString().Contains("Gui") ; }, messages);
            FindDoubledMessages();

            if (messages.Count > 0)
            {
                int i = 0;
                if (messages[i].Done == false)
                    switch (messages[i].Type)
                    {
                        case (int)EventManager.Events.GuiUP:
                            if (messages[i].Sender is Unit)
                            {
                                UnitBackground.animated = true;
                                LowerOptionPanel.isUnit = true;
                                Unit unit = (Unit)messages[i].Sender;
                                    switch (unit.type.name)
                                    {
                                        case "Droid Worker":
                                            UnitBackground.whichUnit = 0;
                                            break;
                                        case "Reconnaissance Eye":
                                            UnitBackground.whichUnit = 1;
                                            break;
                                    }
                                if (CheckIfMultipleSelected())
                                {
                                    UnitBackground.animated = false;
                                    UnitBackground.whichUnit = 2;
                                    messages.RemoveAll(DeleteMessages);
                                }
                            }
                            else if (messages[i].Sender is Building)
                            {
                                UnitBackground.animated = false;
                                LowerOptionPanel.isUnit = false;
                                Building building = (Building)messages[i].Sender;
                                Console.WriteLine(building.type.Name);
                                switch (building.type.Name)
                                {
                                    case "Cementary":
                                        UnitBackground.whichUnit = 0;
                                        LowerOptionPanel.soulNumbers = building.Souls;
                                        break;
                                    case "Pałac rady2":
                                        UnitBackground.whichUnit = 1;
                                        break;
                                    case "BJ Niebian2":
                                        
                                        UnitBackground.whichUnit = 2;
                                        break;
                                }
                            }
                            if (UnitBackground.upTime > 1.0f && LowerBackground.upTime > 1.0f)
                            {
                                if (UnitBackground.isUp)
                                {
                                    messages[i].Done = true;
                                    break;
                                }
                                UnitBackground.upTime = 0;
                                LowerBackground.upTime = 0;
                                LowerOptionPanel.upTime = 0;
                            }
                            break;
                        case (int)EventManager.Events.GuiDOWN:
                            UnitBackground.whichUnit = int.MaxValue;
                            if (UnitBackground.downTime > 1.0f && LowerBackground.downTime > 1.0f)
                            {
                                if (!UnitBackground.isUp)
                                {
                                    messages[i].Done = true;
                                    break;
                                }
                                UnitBackground.downTime = 0;
                                LowerBackground.downTime = 0;
                                LowerOptionPanel.downTime = 0;
                            }
                            break;

                        case (int)GUI.Events.GuiCLICK:
                            if ((int)messages[i].Destination == 0)//to dron worker
                            {
                                if ((int)messages[i].Sender == 1) //to 1 przycisk na workerze
                                {
                                    Console.WriteLine("GUI CLICKED");
                                    if (((Game1)messages[i].Payload).player.Souls < ((Game1)messages[i].Payload).player.BuildingTypes["Cementary"].Souls)
                                    {
                                        messages[i].Done = true;
                                        break;
                                    }
                                    //dodac renderowanie za myszka
                                    Input.next_build = new WhereToBuild(new Building((Game)messages[i].Payload, ((Game1)messages[i].Payload).player, ((Game1)messages[i].Payload).player.BuildingTypes["Cementary"], new Vector3(720, 0, 650), ((Game1)messages[i].Payload).player.BuildingTypes["Cementary"].Scale, false));
                                    Input.building_mode = true;
                                    messages[i].Done = true;
                                }
                                if ((int)messages[i].Sender == 2) //to 1 przycisk na workerze
                                {
                                    Console.WriteLine("GUI CLICKED");
                                    if (((Game1)messages[i].Payload).player.Souls < ((Game1)messages[i].Payload).player.BuildingTypes["BJ Niebian2"].Souls)
                                    {
                                        messages[i].Done = true;
                                        break;
                                    }
                                    //dodac renderowanie za myszka
                                    Input.next_build = new WhereToBuild(new Building((Game)messages[i].Payload, ((Game1)messages[i].Payload).player, ((Game1)messages[i].Payload).player.BuildingTypes["BJ Niebian2"], new Vector3(720, 0, 650), ((Game1)messages[i].Payload).player.BuildingTypes["BJ Niebian2"].Scale, false));
                                    Input.building_mode = true;
                                    messages[i].Done = true;
                                }
                                if ((int)messages[i].Sender == 3) //to 1 przycisk na workerze
                                {
                                    Console.WriteLine("GUI CLICKED");
                                    if (((Game1)messages[i].Payload).player.Souls < ((Game1)messages[i].Payload).player.BuildingTypes["Pałac rady1"].Souls)
                                    {
                                        messages[i].Done = true;
                                        break;
                                    }
                                    //dodac renderowanie za myszka
                                    Input.next_build = new WhereToBuild(new Building((Game)messages[i].Payload, ((Game1)messages[i].Payload).player, ((Game1)messages[i].Payload).player.BuildingTypes["Pałac rady1"], new Vector3(720, 0, 650), ((Game1)messages[i].Payload).player.BuildingTypes["Pałac rady1"].Scale, false));
                                    Input.building_mode = true;
                                    messages[i].Done = true;
                                }
                                if ((int)messages[i].Sender == 4) //to 1 przycisk na workerze
                                {
                                    Console.WriteLine("GUI CLICKED");
                                    if (((Game1)messages[i].Payload).player.Souls < ((Game1)messages[i].Payload).player.BuildingTypes["Strażnica"].Souls)
                                    {
                                        messages[i].Done = true;
                                        break;
                                    }
                                    //dodac renderowanie za myszka
                                    Input.next_build = new WhereToBuild(new Building((Game)messages[i].Payload, ((Game1)messages[i].Payload).player, ((Game1)messages[i].Payload).player.BuildingTypes["Strażnica"], new Vector3(720, 0, 650), ((Game1)messages[i].Payload).player.BuildingTypes["Strażnica"].Scale, false));
                                    Input.building_mode = true;
                                    messages[i].Done = true;
                                }
                            }
                                else if ((int)messages[i].Destination == 2)//budynek
                                {
                                    if ((int)messages[i].Sender == 1) //to 1 przycisk na workerze
                                    {
                                        Console.WriteLine("GUI CLICKED");
                                        if (((Game1)messages[i].Payload).player.Souls < ((Game1)messages[i].Payload).player.UnitTypes["Antigravity Tank"].Souls)
                                        {
                                            messages[i].Done = true;
                                            break;
                                        }
                                        //dodac renderowanie za myszka
                                        foreach (Building build in ((Game1)messages[i].Payload).player.BuildingList)
                                            if (build.type.Name.Contains("BJ Niebian2"))
                                            {
                                                EventManager.CreateMessage(new Message((int)EventManager.Events.BuildUnit, 1, build, null));
                                                messages[i].Done = true;
                                            }
                                    }
                                }
                            break;
                    }
            }
        }

        public static void CleanMessages()
        {
            for (int i = 0; i < messages.Count; i++)
            {
                if (messages[i].Done == true)
                {
                    Console.WriteLine("Usuwam " + (EventManager.Events)messages[i].Type);
                    messages.RemoveAt(i);
                }
            }
        }

        private static bool DeleteMessages(Message message)
        {
            if (message.Type == (int)EventManager.Events.GuiUP)
                return true;
            else
                return false;
        }

        public static bool CheckIfMultipleSelected()
        {
            int counter = 0;
            for (int i = 0; i < messages.Count; i++)
            {
                if (messages[i].Type == (int)EventManager.Events.GuiUP)
                    counter++;
            }
            if (counter > 1)
                return true;
            else
                return false;
        }

        public static void FindDoubledMessages()
        {
            for (int i = 0; i < messages.Count - 1; i++)
                for (int j = i + 1; j < messages.Count; j++)
                {
                    if (messages[i].CompareTo(messages[j]) == 0)
                    {
                        if (messages[i].time.CompareTo(messages[j].time) > 0)
                            messages[j].Done = true;
                        else
                            messages[i].Done = true;
                    }
                }
        }

        public static bool insideRectangle(Rectangle button)
        {
            bool isIn = false;
            Rectangle mouseRectangle = new Rectangle(Input.currentMouseState.X, Input.currentMouseState.Y, 5, 5);
            if (button.Intersects(mouseRectangle))
                return true;
            else
                return false;
        }

        public static void CreateMessage(Message message)
        {
            if (!messages.Contains(message))
                messages.Add(message);
        }
    }
}
