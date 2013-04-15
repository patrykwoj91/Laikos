using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

namespace AnimationPipeline
{
    //przechowuje definicje animacji z pliku XML
    class AnimationDefinition
    {
        public String OriginalClipName //oryginalna nazwa klipu w fbx , najczesciej Take 001
        {
            get;
            set;
        }

        public int OriginalFrameCount //liczba klatek w calym timeline
        {
            get;
            set;
        }

        public class ClipPart //klasa przechowujaca informacje o danej czesci animacji "walk" itp.
        {

            public string ClipName // nazwa czesci
            {
                get;
                set;
            }

            public int StartFrame //klatka startowa
            {
                get;
                set;
            }

            public int EndFrame // klatka koncowa
            {
                get;
                set;
            }

            public class Event // event , dzieki tej klasie wiadomo bedzie kiedy jaka czesc animacji jest aktualnie odtwarzana
            {

                public string Name //nazwa eventu
                {
                    get;
                    set;
                }

                public int Keyframe //klatka na ktorej ma sie wywolac event
                {
                    get;
                    set;
                }
            };


            [Microsoft.Xna.Framework.Content.ContentSerializer(Optional = true)] //lista eventow, opcjonalna
            public List<Event> Events
            {
                get;
                set;
            }
        };

        public List<ClipPart> ClipParts //lista animacji po podziale animacji glownej
        {
            get;
            set;
        }
    }
}
