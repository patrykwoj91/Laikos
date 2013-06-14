using Microsoft.Xna.Framework;

namespace UI
{
    class ScreenTest : Screen
    {
        public ScreenTest() : base( "Test" )
	    {
            // create a graphic in the centre of the screen that is the size of the safe area
            int bundleIndex = _UI.Texture.CreateBundle();
            _UI.Texture.Add(bundleIndex, "GUI/tlo_minimapa", "minimapback");
            _UI.Texture.Add(bundleIndex, "GUI/desen", "desen");
            WidgetGraphic g = new WidgetGraphic();
            g.Position = new Vector3(0, 0, 0.0f);
            g.Size = new Vector3(230, 230, 0.0f);
            g.ColorBase = Color.White;
            //g.Align = E_Align.MiddleCentre;
            g.AddTexture("minimapback", 0.0f, 0.0f, 1.0f, 1.0f);

            WidgetGraphic tlo = new WidgetGraphic();
            tlo.Position = new Vector3(0, 0, 0);
            tlo.Size = new Vector3(1000, 200, 0);
            tlo.ColorBase = Color.White;
            tlo.Align = E_Align.TopRight;
            tlo.AddTexture("desen", 0, 0, 1, 1);
            // add the graphic to the screen
            Add(tlo); 
            Add(g);
            
        }
    }
}
