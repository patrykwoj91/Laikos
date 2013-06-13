using Microsoft.Xna.Framework;

namespace UI
{
    class ScreenTest : Screen
    {
        public ScreenTest() : base( "Test" )
	{
        // create a graphic in the centre of the screen that is the size of the safe area
        WidgetGraphic g = new WidgetGraphic();
        g.Position = new Vector3(_UI.SXM, _UI.SYM, 0.0f);
        g.Size = new Vector3(_UI.SSX, _UI.SSY, 0.0f);
        g.ColorBase = Color.White;
        g.Align = E_Align.MiddleCentre;
        g.AddTexture("null", 0.0f, 0.0f, 1.0f, 1.0f);

        // add the graphic to the screen
        Add(g);
    }
    }
}
