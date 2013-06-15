using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace UI
{
    class ScreenTest : Screen
    {

        WidgetGraphic tlo_jednostka = new WidgetGraphic();

        public ScreenTest(GraphicsDevice device, Laikos.Player player) : base( "Test" )
	    {
            // create a graphic in the centre of the screen that is the size of the safe area
            PresentationParameters pp = device.PresentationParameters;
            int bundleIndex = _UI.Texture.CreateBundle();
            _UI.Texture.Add(bundleIndex, "GUI/tlo_minimapa", "minimapback");
            _UI.Texture.Add(bundleIndex, "GUI/desen", "desen");
            _UI.Texture.Add(bundleIndex, "GUI/button", "zasoby");
            _UI.Texture.Add(bundleIndex, "GUI/chain", "chain");
            _UI.Texture.Add(bundleIndex, "GUI/tlo", "tlo_jednostka");
            WidgetGraphic g = new WidgetGraphic();
            g.Position = new Vector3(0, 0, 0.0f);
            g.Size = new Vector3(230, 230, 0.0f);
            g.ColorBase = Color.White;
            g.AddTexture("minimapback", 0.0f, 0.0f, 1.0f, 1.0f);

            WidgetGraphic tlo = new WidgetGraphic();
            tlo.Position = new Vector3(0, 0, 0);
            tlo.Size = new Vector3(pp.BackBufferWidth, 50, 0);
            tlo.ColorBase = Color.White;
            tlo.AddTexture("desen", 0, 0, 1, 0.5f);

            WidgetGraphic zasoby = new WidgetGraphic();
            zasoby.Position = new Vector3(pp.BackBufferWidth - 150, 0, 0);
            zasoby.Size = new Vector3(100, 50, 0);
            zasoby.ColorBase = Color.White;
            zasoby.AddTexture("zasoby", 0, 0, 1, 1);

            
            tlo_jednostka.Position = new Vector3(0, pp.BackBufferHeight, 0);
            tlo_jednostka.Size = new Vector3(150, 150, 0);
            tlo_jednostka.ColorBase = Color.White;
            tlo_jednostka.AddTexture("tlo_jednostka", 0, 0, 1, 1);
            Timeline wysunTlo = new Timeline("start", false, 0.25f, 0.25f, E_TimerType.Stop, E_RestType.None);
            wysunTlo.AddEffect(new TimelineEffect_PositionY(0.0f, -150, E_LerpType.SmootherStep));
            tlo_jednostka.AddTimeline(wysunTlo);

            WidgetGraphic tlo_bot = new WidgetGraphic();
            tlo_bot.Position = new Vector3(0, pp.BackBufferHeight - 110, 0);
            tlo_bot.Size = new Vector3(pp.BackBufferWidth, 110, 0);
            tlo_bot.ColorBase = Color.White;
            tlo_bot.AddTexture("desen", 0, 0, 1, 1);

            Add(tlo); 
            Add(g);
            Add(zasoby);
            Add(tlo_bot);
            Add(tlo_jednostka);
        }

        protected override void OnProcessInput(Input input)
        {
            if (input.ButtonJustPressed((int)E_UiButton.MouseLeft))
            {
                tlo_jednostka.Render();
            }
        }
    }
}
