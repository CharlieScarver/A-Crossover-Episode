namespace ACrossoverEpisode.GameObjects
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using Emotion.Engine;
    using Emotion.Graphics;
    using Emotion.Graphics.Text;
    using Emotion.Primitives;
    using EmotionPlayground.GameObjects;

    public class DialogBox : Unit
    {
        private const string PixelatedFont = "Fonts/pixelated_princess/pixelated_princess.ttf";
        private const int CharactersPerRow = 70;
        private const int RowHeight = 35;
        private const int RowMargin = 5;

        public DialogBox(string text) : base(Vector3.Zero, Vector2.Zero)
        {
            this.Text = text;

            Context.AssetLoader.Get<Font>(PixelatedFont);

            string loopText = this.Text;
            this.TextRows = new List<string>();
            while (loopText.Length > CharactersPerRow)
            {
                // Get row text
                string row = loopText.Substring(0, CharactersPerRow);
                bool willCutOnLastSpace = loopText[CharactersPerRow] != ' ';
                int lastSpaceIndex = row.LastIndexOf(' ');

                if (willCutOnLastSpace)
                {
                    // Cut on the last space character
                    row = row.Substring(0, lastSpaceIndex);
                }

                // Save the row text
                this.TextRows.Add(row);

                // Remove row text from the loop text
                loopText = willCutOnLastSpace ? loopText.Substring(lastSpaceIndex) : loopText.Substring(CharactersPerRow);
            }

            if (loopText.Length > 0)
            {
                // Save the last row
                this.TextRows.Add(loopText);
            }
        }

        public string Text { get; set; }
        public List<string> TextRows { get; set; }

        public override void Draw(Renderer renderer)
        {
            float boxHeight = 40 + (RowHeight * this.TextRows.Count);

            this.Position = new Vector3(Context.Renderer.Camera.Position.X, Context.Host.Size.Y - boxHeight, 10);
            this.Size = new Vector2(Context.Host.Size.X, boxHeight);
            
            renderer.Render(this.Position, this.Size, Color.Black);
            renderer.RenderOutline(this.Position, this.Size, Color.White);
            

            for (int i = 0; i < this.TextRows.Count; i++)
            {
                renderer.RenderString(Context.AssetLoader.Get<Font>(PixelatedFont), 22, this.TextRows[i], this.Position + new Vector3(30, 20 + (RowHeight * i), 10), Color.White);
            }
        }
    }
}
