using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmiumScripter.Controls
{

    public class TextControl : TextBox
    {
        private System.Threading.Timer updateTimer;
        private readonly Func<string> sourceRead;
        private readonly Action<string> sourceWrite;

        public TextControl(string name, int x, int y, int height, int width, Func<string> sourceRead, Action<string> sourceWrite, int updateInterval = 1000)
        {
            this.sourceRead = sourceRead ?? throw new ArgumentNullException(nameof(sourceRead));
            this.sourceWrite = sourceWrite ?? throw new ArgumentNullException(nameof(sourceWrite));

            Location = new Point(x, y);
            Name = name;
            Size = new Size(width, height); // Breite = width, Höhe = height

            updateTimer = new System.Threading.Timer(_ =>
            {
                try
                {
                    var value = sourceRead.Invoke();
                    if (value != null && Text != value)
                    {
                        if (InvokeRequired)
                        {
                            Invoke(new Action(() => Text = value));
                        }
                        else
                        {
                            Text = value;
                        }
                    }
                }
                catch
                {
                    // Logging oder Fehlerbehandlung optional
                }
            }, null, 1000, updateInterval);

            TextChanged += (s, e) =>
            {
                sourceWrite.Invoke(Text);
            };
        }
    }


    public class AmiumControls
    {



    }
}
