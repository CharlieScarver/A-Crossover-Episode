#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Engine;
using Emotion.Graphics;
using Emotion.Graphics.Text;
using Emotion.Primitives;
using EmotionPlayground.GameObjects;

#endregion

namespace ACrossoverEpisode.GameObjects
{
    public class DialogBox : Unit
    {
        private const string PixelatedFont = "Fonts/pixelated_princess/pixelated_princess.ttf";
        private const int CharactersPerRow = 70;
        private const int RowHeight = 35;
        private const int RowMargin = 5;

        public DialogBox(string text) : base("dialog-box", Vector3.Zero, Vector2.Zero)
        {
            Text = text;

            Context.AssetLoader.Get<Font>(PixelatedFont);

            string loopText = Text;
            TextRows = new List<string>();
            while (loopText.Length > CharactersPerRow)
            {
                // Get row text
                string row = loopText.Substring(0, CharactersPerRow);
                bool willCutOnLastSpace = loopText[CharactersPerRow] != ' ';
                int lastSpaceIndex = row.LastIndexOf(' ');

                if (willCutOnLastSpace) row = row.Substring(0, lastSpaceIndex);

                // Save the row text
                TextRows.Add(row);

                // Remove row text from the loop text
                loopText = willCutOnLastSpace ? loopText.Substring(lastSpaceIndex) : loopText.Substring(CharactersPerRow);
            }

            if (loopText.Length > 0) TextRows.Add(loopText);
        }

        public string Text { get; set; }
        public List<string> TextRows { get; set; }

        public override void Draw(Renderer renderer)
        {
            float boxHeight = 40 + RowHeight * TextRows.Count;

            Position = new Vector3(Context.Renderer.Camera.Position.X, Context.Host.Size.Y - boxHeight, 10);
            Size = new Vector2(Context.Host.Size.X, boxHeight);

            renderer.Render(Position, Size, Color.Black);
            renderer.RenderOutline(Position, Size, Color.White);

            renderer.RenderString(Context.AssetLoader.Get<Font>(PixelatedFont), 22, string.Join("\n", TextRows), Position + new Vector3(30, 20, 10), Color.White);
        }
    }
}